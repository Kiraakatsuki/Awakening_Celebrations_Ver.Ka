import { env, exports } from "cloudflare:workers";
import { applyD1Migrations, type D1Migration } from "cloudflare:test";
import { beforeEach, describe, expect, it, vi } from "vitest";
import worker, { type Env } from "../src/index";
import { randomIndex } from "../src/security";
import { milestones } from "../src/store";

declare global {
  namespace Cloudflare {
    interface Env {
      DB: D1Database;
      ASSETS: Fetcher;
      TEST_MIGRATIONS: D1Migration[];
      LOCAL_TEST_AUTH: string;
      LOCAL_KIOSK_KEY: string;
      TOKEN_KEY: string;
      QR_TTL_SECONDS: string;
      ISSUANCE_PER_DAY: string;
      REQUESTS_PER_MINUTE: string;
      REQUESTS_PER_DAY: string;
    }
    interface GlobalProps {
      mainModule: typeof import("../src/index");
    }
  }
}

type Visit = { visitId: string; visitToken: string; nextMilestone: string | null };
type Assignment = { assignmentId: string; claimUrl: string; issuedAt: string; expiresAt: string; souvenir: { id: string } };

function request(path: string, init?: RequestInit, bindings: Env = env): Promise<Response> {
  return worker.fetch(new Request(new URL(path, "http://localhost"), init), bindings);
}

function post(path: string, value: unknown, visit?: Visit, key?: string, bindings?: Env): Promise<Response> {
  return request(path, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${env.LOCAL_KIOSK_KEY}`,
      ...(visit ? { "X-Visit-Token": visit.visitToken } : {}),
      ...(key ? { "Idempotency-Key": key } : {}),
    },
    body: JSON.stringify(value),
  }, bindings);
}

async function start(key = "visit-one"): Promise<Visit> {
  const response = await post("/api/visits", { performer: "GAIA" }, undefined, key);
  expect(response.status).toBe(200);
  return response.json<Visit>();
}

async function complete(visit: Visit): Promise<void> {
  const base = `/api/visits/${visit.visitId}`;
  for (const [index, milestone] of milestones.entries()) {
    const payload = { eventId: `event-${index}`, sequence: index + 1, milestone };
    expect((await post(`${base}/progress`, payload, visit)).status).toBe(200);
  }
  expect((await post(`${base}/complete`, {}, visit)).status).toBe(200);
}

function assign(visit: Visit, bindings?: Env): Promise<Response> {
  return post(`/api/visits/${visit.visitId}/assignment`, {}, visit, undefined, bindings);
}

async function issued(): Promise<Assignment> {
  const visit = await start();
  await complete(visit);
  const response = await assign(visit);
  expect(response.status).toBe(201);
  return response.json<Assignment>();
}

beforeEach(async () => {
  await applyD1Migrations(env.DB, env.TEST_MIGRATIONS);
  await env.DB.batch([
    "DELETE FROM qr_tokens", "DELETE FROM assignments", "DELETE FROM visit_progress", "DELETE FROM visits",
    "DELETE FROM issuance_limits", "DELETE FROM admin_audit",
    "DELETE FROM kiosk_sessions", "DELETE FROM kiosk_challenges", "DELETE FROM request_limits",
    "DELETE FROM kiosk_registry WHERE id <> 'local-kiosk'",
    "DELETE FROM souvenir_catalog WHERE test_only = 0",
    "UPDATE souvenir_controls SET issuance_paused = 0",
    "UPDATE souvenir_catalog SET asset_path = NULL, active = 1, retired_at = NULL WHERE id GLOB 'test-*'",
    "UPDATE souvenir_catalog SET active = 0 WHERE id = 'placeholder-test'",
  ].map(sql => env.DB.prepare(sql)));
});

describe("local souvenir foundation", () => {
  it("applies all migrations and supports six isolated metadata fixtures", async () => {
    const rows = await env.DB.prepare("SELECT name FROM d1_migrations ORDER BY id").all<{ name: string }>();
    expect(rows.results.map(row => row.name)).toEqual([
      "0001_foundation.sql", "0002_local_catalog.sql", "0003_production_preparation.sql",
      "0004_controlled_kiosk_acceptance.sql",
    ]);
    expect((await request("/health")).status).toBe(200);
    const response = await exports.default.fetch("http://localhost/api/souvenirs/catalog");
    expect(response.status).toBe(200);
    const result = await response.json<{ souvenirs: { id: string; hasImage: boolean }[] }>();
    expect(result.souvenirs.map(x => x.id)).toEqual(["test-01", "test-02", "test-03", "test-04", "test-05", "test-06"]);
    expect(result.souvenirs.every(x => x.hasImage === false)).toBe(true);
  });

  it("rejects unauthenticated and incomplete visits and out-of-order progress", async () => {
    expect((await request("/api/visits", { method: "POST" })).status).toBe(401);
    const visit = await start();
    const base = `/api/visits/${visit.visitId}`;
    expect((await post(`${base}/complete`, {}, { ...visit, visitToken: "a".repeat(43) })).status).toBe(401);
    expect((await post(`${base}/progress`, { eventId: "late", sequence: 4, milestone: "closing-reached" }, visit)).status).toBe(409);
    expect((await post(`${base}/complete`, {}, visit)).status).toBe(409);
    expect((await assign(visit)).status).toBe(409);
    await expect(env.DB.prepare("INSERT INTO assignments VALUES ('broken', ?, 'test-01', unixepoch())")
      .bind(visit.visitId).run()).rejects.toThrow(/visit completion required/);
    const event = { eventId: "first", sequence: 1, milestone: "entered" };
    expect((await post(`${base}/progress`, event, visit)).status).toBe(200);
    expect((await post(`${base}/progress`, event, visit)).status).toBe(200);
    expect((await post(`${base}/progress`, { ...event, eventId: "different" }, visit)).status).toBe(409);
    const resumed = await start();
    expect(resumed.visitId).toBe(visit.visitId);
    expect(resumed.visitToken === visit.visitToken).toBe(true);
    expect(resumed.nextMilestone).toBe("journey-started");
    expect((await post("/api/visits", { performer: "GAIA" }, undefined, "other-key")).status).toBe(409);
  });

  it("returns one durable assignment and QR to concurrent retries", async () => {
    const visit = await start();
    await complete(visit);
    const responses = await Promise.all(Array.from({ length: 12 }, () => assign(visit)));
    expect(responses.map(r => r.status).sort()).toEqual([...Array<number>(11).fill(200), 201]);
    const replies = await Promise.all(responses.map(response => response.json<Assignment>()));
    for (const reply of replies) expect(reply).toEqual(replies[0]);
    expect(await env.DB.prepare("SELECT COUNT(*) AS n FROM assignments").first("n")).toBe(1);
    expect(await env.DB.prepare("SELECT COUNT(*) AS n FROM qr_tokens").first("n")).toBe(1);
    expect(await env.DB.prepare("SELECT count FROM issuance_limits WHERE kind = 'issuance'").first("count")).toBe(1);
    const hash = await env.DB.prepare("SELECT token_hash FROM qr_tokens").first<string>("token_hash");
    expect(hash).toMatch(/^[a-f0-9]{64}$/);
    expect(replies[0].claimUrl.endsWith(hash!)).toBe(false);
    expect(await start("explicit-next-visit")).not.toEqual(visit);
  });

  it("enforces fixed QR expiration and the 60-minute maximum without reassigning", async () => {
    const visit = await start();
    await complete(visit);
    expect((await assign(visit, { ...env, QR_TTL_SECONDS: "3601" })).status).toBe(503);
    const assigned = await (await assign(visit)).json<Assignment>();
    expect(Date.parse(assigned.expiresAt) - Date.parse(assigned.issuedAt)).toBe(1800_000);
    expect(await (await request(assigned.claimUrl)).json()).toMatchObject({ expiresAt: assigned.expiresAt, image: null });
    await expect(env.DB.prepare("UPDATE qr_tokens SET expires_at = expires_at + 1").run())
      .rejects.toThrow(/QR issuance and revocation are fixed/);
    const shortVisit = await start("short-window");
    await complete(shortVisit);
    const short = await (await assign(shortVisit, { ...env, QR_TTL_SECONDS: "2" })).json<Assignment>();
    await new Promise(resolve => setTimeout(resolve, Math.max(0, Date.parse(short.expiresAt) - Date.now()) + 50));
    expect((await request(short.claimUrl)).status).toBe(410);
    expect((await request(short.claimUrl + "/image")).status).toBe(410);
    expect((await assign(shortVisit)).status).toBe(410);
    expect(await env.DB.prepare("SELECT COUNT(*) AS n FROM assignments").first("n")).toBe(2);
  });

  it("revokes a single QR and records the administrator action", async () => {
    const assigned = await issued();
    await env.DB.prepare(`UPDATE qr_tokens SET revoked_at = unixepoch(), revoked_by = 'test-operator',
      revocation_reason = 'synthetic test' WHERE assignment_id = ?`).bind(assigned.assignmentId).run();
    const response = await request(assigned.claimUrl);
    expect(response.status).toBe(410);
    expect(await response.json()).toMatchObject({ error: { code: "claim_revoked" } });
    expect(await env.DB.prepare("SELECT target FROM admin_audit WHERE action = 'qr-revoked'").first("target"))
      .toBe(assigned.assignmentId);
    await expect(env.DB.prepare(`UPDATE qr_tokens SET revoked_at = NULL, revoked_by = NULL, revocation_reason = NULL
      WHERE assignment_id = ?`).bind(assigned.assignmentId).run()).rejects.toThrow(/revocation are fixed/);
  });

  it("excludes retired entries from new draws and preserves existing claims", async () => {
    const assigned = await issued();
    await env.DB.prepare("UPDATE souvenir_catalog SET retired_at = unixepoch() WHERE id = ?")
      .bind(assigned.souvenir.id).run();
    expect((await request(assigned.claimUrl)).status).toBe(200);
    const next = await start("after-retirement");
    await complete(next);
    const response = await assign(next);
    expect(response.status).toBe(201);
    expect((await response.json<Assignment>()).souvenir.id).not.toBe(assigned.souvenir.id);
  });

  it("blocks direct assets and requires a scoped image grant before fetching bytes", async () => {
    const assigned = await issued();
    await env.DB.prepare("UPDATE souvenir_catalog SET asset_path = '/media/test-image.webp' WHERE id = ?")
      .bind(assigned.souvenir.id).run();
    const loadAsset = vi.fn(async (_request: Request) => new Response("synthetic stream", {
      headers: { "Content-Type": "image/webp" },
    }));
    const bindings = { ...env, ASSETS: { fetch: loadAsset } as unknown as Fetcher };
    const media = assigned.claimUrl + "/image";
    expect((await exports.default.fetch("http://localhost/media/test-image.webp")).status).toBe(404);
    expect((await request("/media/test-image.webp", undefined, bindings)).status).toBe(404);
    expect((await request(media, undefined, bindings)).status).toBe(401);
    expect((await request(media, { headers: { Authorization: "Bearer 12345." + "x".repeat(43) } }, bindings)).status).toBe(401);
    expect(loadAsset).not.toHaveBeenCalled();
    const { image } = await (await request(assigned.claimUrl)).json<{ image: { authorization: string } }>();
    const auth = { headers: { Authorization: `Bearer ${image.authorization}` } };
    const response = await request(media, auth, bindings);
    expect(response.status).toBe(200);
    expect(response.headers.get("Cache-Control")).toBe("no-store");
    expect(new TextDecoder().decode(await response.arrayBuffer())).toBe("synthetic stream");
    expect(new URL(loadAsset.mock.calls[0][0].url).pathname).toBe("/media/test-image.webp");
    await env.DB.prepare(`UPDATE qr_tokens SET revoked_at = unixepoch(), revoked_by = 'test',
      revocation_reason = 'stop delivery'`).run();
    expect((await request(media, auth, bindings)).status).toBe(410);
    expect(loadAsset).toHaveBeenCalledTimes(1);
  });

  it("fails closed when DB access fails or the request is outside the local boundary", async () => {
    const DB = { prepare: () => { throw new Error("private failure"); } } as unknown as D1Database;
    const result = await request("/api/souvenirs/catalog", undefined, { ...env, DB });
    expect(result.status).toBe(503);
    expect(await result.json()).toEqual({ error: { code: "service_unavailable", message: "Souvenirs are temporarily unavailable." } });
    expect((await request("/health", undefined, { ...env, DB })).status).toBe(503);
    expect((await worker.fetch(new Request("https://example.test/health"), env)).status).toBe(404);
  });

  it("honors configurable request and daily issuance limits and preserves the first assignment", async () => {
    const first = await start();
    await complete(first);
    const config = { ...env, ISSUANCE_PER_DAY: "1" };
    expect((await assign(first, config)).status).toBe(201);
    const second = await start("new-visit");
    await complete(second);
    expect((await assign(second, config)).status).toBe(429);
    expect((await assign(first, config)).status).toBe(200);
    await env.DB.prepare("DELETE FROM issuance_limits WHERE kind = 'request'").run();
    const requests = { ...env, REQUESTS_PER_MINUTE: "1" };
    expect((await request("/api/souvenirs/catalog", undefined, requests)).status).toBe(200);
    const limited = await request("/api/souvenirs/catalog", undefined, requests);
    expect(limited.status).toBe(429);
    expect(limited.headers.get("Retry-After")).toBe("60");
    const todayCount = await env.DB.prepare("SELECT count FROM issuance_limits WHERE kind = 'request-day'").first("count");
    expect(typeof todayCount).toBe("number");
    expect((await request("/api/souvenirs/catalog", undefined, { ...env, REQUESTS_PER_DAY: String(todayCount) })).status).toBe(429);
  });


  it("completes once under concurrency and returns the same timestamp on every retry", async () => {
    const visit = await start();
    const base = `/api/visits/${visit.visitId}`;
    for (const [index, milestone] of milestones.entries()) {
      expect((await post(`${base}/progress`, { eventId: `event-${index}`, sequence: index + 1, milestone }, visit)).status).toBe(200);
    }
    const requests = Array.from({ length: 12 }, () => post(`${base}/complete`, {}, visit));
    const responses = await Promise.all(requests);
    expect(responses.every(response => response.status === 200)).toBe(true);
    const timestamps = await Promise.all(responses.map(response => response.json<{ completedAt: string }>()));
    expect(new Set(timestamps.map(x => x.completedAt)).size).toBe(1);
    expect((await assign(visit)).status).toBe(201);
    expect(await (await post(`${base}/complete`, {}, visit)).json()).toEqual(timestamps[0]);
  });

  it("rolls back issuance when a later statement in the D1 batch fails", async () => {
    const visit = await start();
    await complete(visit);
    await env.DB.prepare(`CREATE TRIGGER reject_qr_for_test BEFORE INSERT ON qr_tokens
      BEGIN SELECT RAISE(ABORT, 'synthetic failure'); END`).run();
    try {
      expect((await assign(visit)).status).toBe(503);
      expect(await env.DB.prepare("SELECT COUNT(*) AS n FROM assignments").first("n")).toBe(0);
      expect(await env.DB.prepare("SELECT COUNT(*) AS n FROM qr_tokens").first("n")).toBe(0);
      expect(await env.DB.prepare("SELECT count FROM issuance_limits WHERE kind = 'issuance'").first("count")).toBeNull();
      expect(await env.DB.prepare("SELECT closed_at FROM visits WHERE id = ?").bind(visit.visitId).first("closed_at")).toBeNull();
    } finally {
      await env.DB.prepare("DROP TRIGGER reject_qr_for_test").run();
    }
    expect((await assign(visit)).status).toBe(201);
  });

  it("keeps repeated claims stable and rejects guessed paths, bad IDs and cross-origin preflight", async () => {
    const assignment = await issued();
    const scans = await Promise.all(Array.from({ length: 6 }, () => request(assignment.claimUrl)));
    for (const response of scans) {
      expect(response.status).toBe(200);
      expect(response.headers.get("Cache-Control")).toBe("no-store");
      expect(response.headers.get("Referrer-Policy")).toBe("no-referrer");
      expect(response.headers.get("X-Content-Type-Options")).toBe("nosniff");
      expect(response.headers.get("Access-Control-Allow-Origin")).toBeNull();
      expect(await response.json()).toMatchObject({ souvenir: assignment.souvenir, expiresAt: assignment.expiresAt });
    }
    expect(await env.DB.prepare("SELECT COUNT(*) AS n FROM qr_tokens").first("n")).toBe(1);
    expect((await request("/api/claims/" + "a".repeat(43) + "/image")).status).toBe(404);
    expect((await request("/media/test-01.webp")).status).toBe(404);
    expect((await post("/api/visits/not-a-visit/complete", {})).status).toBe(404);
    expect((await post("/api/visits/00000000-0000-4000-8000-000000000000/complete", {})).status).toBe(401);
    const preflight = await request("/api/visits", { method: "OPTIONS", headers: {
      Origin: "https://untrusted.example", "Access-Control-Request-Method": "POST",
      "Access-Control-Request-Headers": "authorization",
    } });
    expect(preflight.status).toBe(404);
    expect(preflight.headers.get("Access-Control-Allow-Origin")).toBeNull();
  });

  it("rejects kiosk requests without spending the shared request quota", async () => {
    const config = { ...env, REQUESTS_PER_MINUTE: "1" };
    const denied = await request("/api/visits", { method: "POST" }, config);
    const next = await request("/api/souvenirs/catalog", undefined, config);
    expect(denied.status).toBe(401);
    expect(next.status).toBe(200);
  });

  it("admits only the available daily request slots under concurrency", async () => {
    const config = { ...env, REQUESTS_PER_MINUTE: "60", REQUESTS_PER_DAY: "3" };
    const requests = await Promise.all(Array.from({ length: 12 }, () =>
      request("/api/souvenirs/catalog", undefined, config)));
    expect(requests.filter(response => response.status === 200)).toHaveLength(3);
    expect(requests.filter(response => response.status === 429)).toHaveLength(9);
    expect(await env.DB.prepare("SELECT count FROM issuance_limits WHERE kind = 'request-day'").first("count")).toBe(3);
  });

  it("disables local authentication in production mode even if test secrets are present", async () => {
    const production = { ...env, LOCAL_TEST_AUTH: "disabled" };
    const result = await post("/api/visits", { performer: "GAIA" }, undefined, "remote-test", production);
    expect(result.status).toBe(404);
    expect(await env.DB.prepare("SELECT COUNT(*) AS n FROM visits").first("n")).toBe(0);
  });

  it("denies an image when the QR is revoked while the asset is being fetched", async () => {
    const assignment = await issued();
    await env.DB.prepare("UPDATE souvenir_catalog SET asset_path = '/media/test.webp' WHERE id = ?")
      .bind(assignment.souvenir.id).run();
    const { image } = await (await request(assignment.claimUrl)).json<{ image: { url: string; authorization: string } }>();
    const loadAsset = vi.fn(async () => {
      await env.DB.prepare(`UPDATE qr_tokens SET revoked_at = unixepoch(), revoked_by = 'test',
        revocation_reason = 'synthetic race' WHERE assignment_id = ?`).bind(assignment.assignmentId).run();
      return new Response("private synthetic bytes", { headers: { "Content-Type": "image/webp" } });
    });
    const bindings = { ...env, ASSETS: { fetch: loadAsset } as unknown as Fetcher };
    const response = await request(image.url, { headers: { Authorization: `Bearer ${image.authorization}` } }, bindings);
    expect(loadAsset).toHaveBeenCalledTimes(1);
    expect(response.status).toBe(410);
  });

  it("samples six outcomes without obvious bias", () => {
    const counts = [0, 0, 0, 0, 0, 0];
    for (let i = 0; i < 12_000; i++) counts[randomIndex(counts.length)]++;
    for (const count of counts) {
      expect(count).toBeGreaterThan(1700);
      expect(count).toBeLessThan(2300);
    }
  });
});

describe("production preparation", () => {
  const origin = "https://souvenir.example";
  const production = (): Env => ({
    ...env, SERVICE_MODE: "production", PUBLIC_ORIGIN: origin,
    LOCAL_TEST_AUTH: "disabled", LOCAL_KIOSK_KEY: undefined, TOKEN_KEY: "03".repeat(32),
  });
  const base64url = (bytes: Uint8Array) => btoa(String.fromCharCode(...bytes))
    .replaceAll("+", "-").replaceAll("/", "_").replace(/=+$/, "");

  function call(path: string, value?: unknown, headers: Record<string, string> = {}, config = production()) {
    return request(origin + path, value === undefined ? { headers } : {
      method: "POST", headers: { "Content-Type": "application/json", ...headers }, body: JSON.stringify(value),
    }, config);
  }

  async function enrollment(id: string) {
    const keys = await crypto.subtle.generateKey({ name: "ECDSA", namedCurve: "P-256" }, false, ["sign", "verify"]);
    if (!("publicKey" in keys)) throw new Error("Expected an ECDSA key pair.");
    const publicKey = await crypto.subtle.exportKey("spki", keys.publicKey);
    if (!(publicKey instanceof ArrayBuffer)) throw new Error("Expected an SPKI buffer.");
    const spki = base64url(new Uint8Array(publicKey));
    await env.DB.prepare("INSERT INTO kiosk_registry (id, public_key) VALUES (?, ?)").bind(id, spki).run();
    return async () => {
      const response = await call("/api/kiosks/challenge", { kioskId: id });
      expect(response.status).toBe(200);
      const { message, challengeId } = await response.json<{ message: string; challengeId: string }>();
      expect(message.split("\n").slice(0, 4)).toEqual(["awakening-souvenir/kiosk/v1", origin, id, challengeId]);
      const signed = await crypto.subtle.sign({ name: "ECDSA", hash: "SHA-256" }, keys.privateKey, new TextEncoder().encode(message));
      return { challengeId, signature: base64url(new Uint8Array(signed)) };
    };
  }

  async function login(id: string) {
    const proof = await (await enrollment(id))();
    const response = await call("/api/kiosks/session", proof);
    expect(response.status).toBe(200);
    return (await response.json<{ session: string }>()).session;
  }

  it("requires production configuration, consumes challenges once and revokes sessions", async () => {
    expect((await call("/api/visits", { performer: "GAIA" },
      { Authorization: `Bearer ${env.LOCAL_KIOSK_KEY}`, "Idempotency-Key": "fake" })).status).toBe(401);
    expect((await call("/health", undefined, {}, { ...production(), LOCAL_TEST_AUTH: "enabled" })).status).toBe(503);
    expect((await call("/health", undefined, {}, { ...production(), LOCAL_KIOSK_KEY: env.LOCAL_KIOSK_KEY })).status).toBe(503);
    const challenge = await enrollment("kiosk-a");
    const expired = await challenge();
    await env.DB.prepare("UPDATE kiosk_challenges SET expires_at = unixepoch() WHERE id = ?").bind(expired.challengeId).run();
    expect((await call("/api/kiosks/session", expired)).status).toBe(401);
    const proof = await challenge();
    expect((await call("/api/kiosks/session", { ...proof, signature: "a".repeat(86) })).status).toBe(401);
    const attempts = await Promise.all(Array.from({ length: 5 }, () => call("/api/kiosks/session", proof)));
    expect(attempts.map(r => r.status).sort()).toEqual([200, 401, 401, 401, 401]);
    const { session, expiresAt } = await attempts.find(r => r.status === 200)!.json<{ session: string; expiresAt: number }>();
    expect(expiresAt - Math.floor(Date.now() / 1000)).toBeLessThanOrEqual(300);
    expect(await env.DB.prepare("SELECT token_hash FROM kiosk_sessions").first("token_hash")).toMatch(/^[a-f0-9]{64}$/);
    const headers = { Authorization: `Bearer ${session}`, "Idempotency-Key": "visit-a" };
    expect((await call("/api/visits", { performer: "GAIA" }, headers)).status).toBe(200);
    await env.DB.prepare("UPDATE kiosk_sessions SET revoked_at = unixepoch()").run();
    expect((await call("/api/visits", { performer: "GAIA" }, headers)).status).toBe(401);
    const next = await call("/api/kiosks/session", await challenge());
    const nextSession = (await next.json<{ session: string }>()).session;
    await env.DB.prepare("UPDATE kiosk_registry SET active = 0 WHERE id = 'kiosk-a'").run();
    expect((await call("/api/visits", { performer: "GAIA" }, { ...headers, Authorization: `Bearer ${nextSession}` })).status).toBe(401);
  });

  it("keeps one active visit per kiosk, enforces ownership and preserves reset retries", async () => {
    const authA = { Authorization: `Bearer ${await login("kiosk-a")}` };
    const authB = { Authorization: `Bearer ${await login("kiosk-b")}` };
    const start = (key: string, auth = authA, config = production()) =>
      call("/api/visits", { performer: "GAIA" }, { ...auth, "Idempotency-Key": key }, config);
    const visitResponse = await start("a");
    expect(visitResponse.status).toBe(200);
    const visit = await visitResponse.json<Visit>();
    expect((await start("b")).status).toBe(409);
    expect((await start("b", authB)).status).toBe(200);
    const reset = `/api/visits/${visit.visitId}/reset`;
    expect((await call(reset, {}, { ...authB, "X-Visit-Token": visit.visitToken })).status).toBe(404);
    for (let i = 0; i < 2; i++) {
      expect((await call(reset, {}, { ...authA, "X-Visit-Token": visit.visitToken })).status).toBe(200);
    }
    const capped = { ...production(), VISITS_PER_KIOSK_PER_DAY: "1" };
    expect((await start("fresh", authA, capped)).status).toBe(429);
    expect((await start("a", authA, capped)).status).toBe(200);
    expect((await start("fresh")).status).toBe(200);
  });

  it("preserves production issuance when public quota is exhausted and honors pause and atomic caps", async () => {
    await env.DB.prepare("UPDATE souvenir_catalog SET active = 1 WHERE id = 'placeholder-test'").run();
    const hidden = await call("/api/souvenirs/catalog");
    expect(await hidden.json()).toEqual({ souvenirs: [] });
    await env.DB.prepare(`INSERT INTO souvenir_catalog(id, title_en, title_zh_tw, active, asset_path)
      VALUES ('synthetic-approved', 'Test fixture', '測試', 1, '/media/approved-test.webp')`).run();
    await env.DB.prepare(`INSERT INTO request_limits(scope, window_start, count)
      VALUES ('claim:day', unixepoch() - unixepoch() % 86400, 1200)`).run();
    expect((await call("/api/claims/resolve", { token: "a".repeat(43) })).status).toBe(429);
    const issueReady = async (id: string) => {
      const auth = { Authorization: `Bearer ${await login(id)}` };
      const started = await call("/api/visits", { performer: "GAIA" }, { ...auth, "Idempotency-Key": id });
      expect(started.status).toBe(200);
      const visit = await started.json<Visit>();
      const headers = { ...auth, "X-Visit-Token": visit.visitToken };
      const path = `/api/visits/${visit.visitId}`;
      for (const [index, milestone] of milestones.entries()) {
        expect((await call(path + "/progress", { eventId: milestone, sequence: index + 1, milestone }, headers)).status).toBe(200);
      }
      expect((await call(path + "/complete", {}, headers)).status).toBe(200);
      return (issuanceMode?: string) => call(path + "/assignment", {}, headers,
        { ...production(), ISSUANCE_MODE: issuanceMode, ISSUANCE_PER_DAY: "1" });
    };
    const first = await issueReady("kiosk-a");
    const second = await issueReady("kiosk-b");
    expect(await (await first()).json()).toMatchObject({ error: { code: "issuance_paused" } });
    await env.DB.prepare("UPDATE souvenir_controls SET issuance_paused = 1").run();
    expect((await first("enabled")).status).toBe(503);
    await env.DB.prepare("UPDATE souvenir_controls SET issuance_paused = 0").run();
    const [one, two] = await Promise.all([first("enabled"), second("enabled")]);
    expect([one.status, two.status].sort()).toEqual([201, 429]);
    const winner = one.status === 201 ? one : two;
    const data = await winner.json<Assignment>();
    expect(data.claimUrl).toMatch(/^\/claim#[A-Za-z0-9_-]{43}$/);
    expect(data.souvenir.id).toBe("synthetic-approved");
    await env.DB.prepare("UPDATE souvenir_controls SET issuance_paused = 1").run();
    expect((await (one.status === 201 ? first() : second())).status).toBe(200);
    expect(await env.DB.prepare("SELECT count FROM issuance_limits WHERE kind = 'issuance'").first("count")).toBe(1);
    expect((await call("/api/claims/" + data.claimUrl.split("#")[1])).status).toBe(404);
    await env.DB.prepare("DELETE FROM request_limits WHERE scope = 'claim:day'").run();
    const token = data.claimUrl.split("#")[1];
    for (let index = 0; index < 12; index++) {
      expect((await call("/api/claims/resolve", { token })).status).toBe(200);
    }
    expect((await call("/api/claims/resolve", { token })).status).toBe(429);
  });

  it("streams the real placeholder only with the matching preview or download grant", async () => {
    await env.DB.prepare("UPDATE souvenir_catalog SET active = (id = 'placeholder-test')").run();
    const result = await issued();
    expect(result.souvenir.id).toBe("placeholder-test");
    const token = result.claimUrl.split("/").at(-1)!;
    const controlled = { ...production(), ISSUANCE_MODE: "paused", CONTROLLED_CLAIM_IDS: result.assignmentId };
    expect((await call("/api/claims/resolve", { token })).status).toBe(404);
    expect((await call("/api/claims/resolve", { token }, {}, controlled)).status).toBe(404);
    await env.DB.prepare("UPDATE souvenir_controls SET issuance_paused = 1").run();
    expect((await call("/api/claims/resolve", { token }, {}, controlled)).status).toBe(200);
    expect((await call("/api/claims/resolve", { token }, {}, { ...controlled, ISSUANCE_MODE: "enabled" })).status).toBe(503);
    const resolve = await request("/api/claims/resolve", {
      method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ token }),
    });
    const { image, download } = await resolve.json<{
      image: { authorization: string }; download: { authorization: string };
    }>();
    const headers = (grant: string) => ({ "X-Claim-Token": token, Authorization: "Bearer " + grant });
    expect((await request("/api/claims/download", { headers: headers(image.authorization) })).status).toBe(401);
    const preview = await request("/api/claims/preview", { headers: headers(image.authorization) });
    const file = await request("/api/claims/download", { headers: headers(download.authorization) });
    expect(preview.status).toBe(200);
    expect(file.status).toBe(200);
    expect(preview.headers.get("Content-Disposition")).toMatch(/^inline/);
    expect(file.headers.get("Content-Disposition")).toMatch(/^attachment/);
    expect(file.headers.get("Cache-Control")).toBe("no-store");
    const previewBytes = await preview.arrayBuffer();
    expect(new TextDecoder().decode(previewBytes.slice(8, 12))).toBe("WEBP");
    const checksum = new Uint8Array(await crypto.subtle.digest("SHA-256", previewBytes));
    expect(Array.from(checksum, byte => byte.toString(16).padStart(2, "0")).join(""))
      .toBe("9a6783283e73c669fab1dd3ab11f1a305919b4218674536dd2744f57a651e07d");
    expect(await file.arrayBuffer()).toEqual(previewBytes);
    const direct = await exports.default.fetch("http://localhost/media/placeholder-test.webp");
    expect(direct.status).toBe(404);
    const page = await request("/claim");
    expect(page.headers.get("Content-Security-Policy")).toContain("frame-ancestors 'none'");
    await env.DB.prepare(`UPDATE qr_tokens SET revoked_at = unixepoch(), revoked_by = 'test', revocation_reason = 'test'`).run();
    expect((await request("/api/claims/download", { headers: headers(download.authorization) })).status).toBe(410);
  });

  it("uses a paused operator grant once for only the named kiosk visit", async () => {
    await env.DB.prepare("UPDATE souvenir_controls SET issuance_paused = 1").run();
    await env.DB.prepare("UPDATE souvenir_catalog SET active = 1 WHERE id = 'placeholder-test'").run();
    const config = { ...production(), ISSUANCE_MODE: "paused" };
    const session = await login("acceptance-kiosk");
    const auth = { Authorization: `Bearer ${session}`, "Idempotency-Key": "acceptance" };
    const started = await call("/api/visits", { performer: "GAIA" }, auth, config);
    const visit = await started.json<Visit>();
    const headers = { ...auth, "X-Visit-Token": visit.visitToken };
    const path = `/api/visits/${visit.visitId}`;
    for (const [index, milestone] of milestones.entries()) {
      expect((await call(path + "/progress", { eventId: milestone, sequence: index + 1, milestone }, headers, config)).status).toBe(200);
    }
    expect((await call(path + "/complete", {}, headers, config)).status).toBe(200);
    const assignment = () => call(path + "/assignment", {}, headers, config);
    expect((await assignment()).status).toBe(503);
    await env.DB.prepare(`INSERT INTO kiosk_acceptance_grants(visit_id,kiosk_id,created_at,expires_at)
      VALUES (?, 'acceptance-kiosk',unixepoch(),unixepoch()+120)`).bind(visit.visitId).run();
    const pair = await Promise.all([assignment(), assignment()]);
    expect(pair.map(response => response.status).sort()).toEqual([200, 201]);
    const claims = await Promise.all(pair.map(response => response.json<Assignment>()));
    expect(claims[0]).toEqual(claims[1]);
    expect(claims[0].souvenir.id).toBe("placeholder-test");
    expect(Date.parse(claims[0].expiresAt) - Date.parse(claims[0].issuedAt)).toBe(1800000);
    expect(await env.DB.prepare("SELECT used_assignment_id FROM kiosk_acceptance_grants").first("used_assignment_id"))
      .toBe(claims[0].assignmentId);
    const token = claims[0].claimUrl.split("#")[1];
    expect((await call("/api/claims/resolve", { token }, {}, config)).status).toBe(200);
    await env.DB.prepare("UPDATE kiosk_acceptance_grants SET revoked_at = unixepoch()").run();
    expect((await assignment()).status).toBe(200);
    expect(await env.DB.prepare("SELECT COUNT(*) AS count FROM admin_audit WHERE action LIKE 'kiosk-acceptance-%'").first("count"))
      .toBe(3);
  });
});
