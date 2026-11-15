import * as store from "./store";
import { derive, digest, isToken, matches, nonce, randomIndex } from "./security";
import { createChallenge, exchangeChallenge, sessionKiosk } from "./kiosk";
import { claimPage } from "./claim-page";

export interface Env {
  DB: D1Database;
  ASSETS: Fetcher;
  SERVICE_MODE?: string;
  ISSUANCE_MODE?: string;
  CONTROLLED_CLAIM_IDS?: string;
  PUBLIC_ORIGIN?: string;
  VISITS_PER_KIOSK_PER_DAY?: string;
  LOCAL_TEST_AUTH?: string;
  LOCAL_KIOSK_KEY?: string;
  TOKEN_KEY?: string;
  QR_TTL_SECONDS?: string;
  ISSUANCE_PER_DAY?: string;
  REQUESTS_PER_MINUTE?: string;
  REQUESTS_PER_DAY?: string;
}

type Settings = { key: string; kioskKey?: string; production: boolean; issuanceEnabled: boolean; acceptancePaused: boolean; controlledClaims: string; visitsPerKiosk: number;
  ttl: number; issuance: number; minuteRequests: number; dailyRequests: number };
const kiosk = "local-kiosk";
const identifier = /^[A-Za-z0-9_-]{1,80}$/;
const visitIdPattern = /^[a-f0-9-]{36}$/;
const privateHeaders = { "Cache-Control": "no-store", "Referrer-Policy": "no-referrer", "X-Content-Type-Options": "nosniff" };

class ApiError extends Error {
  constructor(readonly status: number, readonly code: string, message: string, readonly retryAfter?: number) {
    super(message);
  }
}

function json(value: unknown, status = 200, retryAfter?: number): Response {
  return Response.json(value, {
    status, headers: { ...privateHeaders, ...(retryAfter === undefined ? {} : { "Retry-After": String(retryAfter) }) },
  });
}

function invalid(): never {
  throw new ApiError(400, "invalid_request", "The request fields are invalid.");
}

function notFound(): never {
  throw new ApiError(404, "not_found", "The requested resource was not found.");
}

function unavailable(): never {
  throw new ApiError(503, "service_unavailable", "Souvenirs are temporarily unavailable.");
}

function integerSetting(value: string | undefined, fallback: number, max: number): number {
  const parsed = value === undefined ? fallback : /^\d+$/.test(value) ? Number(value) : NaN;
  if (!Number.isInteger(parsed) || parsed < 1 || parsed > max) unavailable();
  return parsed;
}

function settings(env: Env, url: URL): Settings {
  const production = env.SERVICE_MODE === "production";
  const key = env.TOKEN_KEY;
  const kioskKey = env.LOCAL_KIOSK_KEY;
  if (production) {
    if (url.protocol !== "https:" || !env.PUBLIC_ORIGIN || url.origin !== env.PUBLIC_ORIGIN) notFound();
    if (env.LOCAL_TEST_AUTH !== "disabled" || kioskKey || !key || !/^[a-fA-F0-9]{64}$/.test(key)) unavailable();
  } else {
    if ((env.SERVICE_MODE !== undefined && env.SERVICE_MODE !== "local") || env.LOCAL_TEST_AUTH !== "enabled" ||
        !["localhost", "127.0.0.1", "[::1]"].includes(url.hostname)) notFound();
    if (!key || !kioskKey || key.length < 32 || kioskKey.length < 32 || key === kioskKey) unavailable();
  }
  const controlled = env.CONTROLLED_CLAIM_IDS?.split(",") ?? [];
  if (controlled.length && (!production || env.ISSUANCE_MODE !== "paused" || controlled.length > 2 ||
      controlled.some(id => !visitIdPattern.test(id)))) unavailable();
  return {
    key, kioskKey, production,
    issuanceEnabled: !production || env.ISSUANCE_MODE === "enabled",
    acceptancePaused: production && env.ISSUANCE_MODE === "paused",
    controlledClaims: JSON.stringify(controlled),
    visitsPerKiosk: integerSetting(env.VISITS_PER_KIOSK_PER_DAY, 100, 500),
    ttl: integerSetting(env.QR_TTL_SECONDS, 1800, 3600),
    issuance: integerSetting(env.ISSUANCE_PER_DAY, 100, 500),
    minuteRequests: integerSetting(env.REQUESTS_PER_MINUTE, 60, 600),
    dailyRequests: integerSetting(env.REQUESTS_PER_DAY, 5000, 10000),
  };
}

async function body(request: Request): Promise<Record<string, unknown>> {
  if (request.headers.get("Content-Type")?.split(";")[0].trim().toLowerCase() !== "application/json") invalid();
  const reader = request.body?.getReader();
  if (!reader) invalid();
  let text = "";
  let bytes = 0;
  const decoder = new TextDecoder();
  try {
    for (;;) {
      const { value, done } = await reader.read();
      if (done) break;
      bytes += value.byteLength;
      if (bytes > 2048) {
        await reader.cancel();
        throw new ApiError(413, "request_too_large", "The request body exceeds 2 KiB.");
      }
      text += decoder.decode(value, { stream: true });
    }
  } finally {
    reader.releaseLock();
  }
  let parsed: unknown;
  try { parsed = JSON.parse(text + decoder.decode()); } catch { invalid(); }
  if (!parsed || typeof parsed !== "object" || Array.isArray(parsed)) invalid();
  return parsed as Record<string, unknown>;
}

function fields(value: Record<string, unknown>, expected: string[]): void {
  if (Object.keys(value).length !== expected.length || expected.some(key => !Object.hasOwn(value, key))) invalid();
}

async function requireKiosk(request: Request, db: D1Database, config: Settings): Promise<string> {
  const auth = request.headers.get("Authorization") ?? "";
  if (!config.production && matches(auth, `Bearer ${config.kioskKey}`)) return kiosk;
  if (config.production && auth.startsWith("Bearer ") && isToken(auth.slice(7))) {
    const id = await sessionKiosk(db, auth.slice(7));
    if (id) return id;
  }
  throw new ApiError(401, "kiosk_auth_required", "A valid kiosk credential is required.");
}

async function authorizedVisit(request: Request, db: D1Database, config: Settings, id: string, kioskId: string): Promise<store.Visit> {
  const token = request.headers.get("X-Visit-Token") ?? "";
  if (!isToken(token) || !matches(token, await derive(config.key, "visit", id))) {
    throw new ApiError(401, "visit_auth_required", "A valid visit credential is required.");
  }
  const visit = await store.findVisit(db, kioskId, id);
  return visit ?? notFound();
}

function publicEntry(entry: { id: string; title_en: string; title_zh_tw: string; asset_path: string | null }) {
  return { id: entry.id, title: { en: entry.title_en, "zh-TW": entry.title_zh_tw }, hasImage: entry.asset_path !== null };
}

function iso(seconds: number): string {
  return new Date(seconds * 1000).toISOString();
}

function liveClaim(claim: store.Claim | null): store.Claim {
  if (!claim) notFound();
  if (claim.revoked_at !== null) throw new ApiError(410, "claim_revoked", "This souvenir link has been revoked.");
  if (!claim.available) throw new ApiError(410, "claim_expired", "This souvenir link has expired.");
  return claim;
}

async function assignment(db: D1Database, config: Settings, visit: store.Visit): Promise<Response> {
  if (visit.completed_at === null) throw new ApiError(409, "visit_incomplete", "Complete the visit before requesting a souvenir.");
  let claim = await store.claimForVisit(db, visit.id);
  let created = false;
  if (!claim && config.acceptancePaused) {
    const seed = nonce();
    const hash = await digest(await derive(config.key, "qr", seed));
    ({ claim, created } = await store.assignAcceptance(db, visit.id, seed, hash));
  }
  if (!claim && !config.issuanceEnabled) {
    throw new ApiError(503, "issuance_paused", "Souvenir issuance is paused.");
  }
  for (let attempt = 0; claim === null && attempt < 3; attempt++) {
    const entries = await store.catalog(db, config.production);
    if (!entries.length) {
      // Another request may have issued the QR before the last image was retired.
      claim = await store.claimForVisit(db, visit.id);
      break;
    }
    const ids = entries.map(entry => entry.id);
    const seed = nonce();
    const hash = await digest(await derive(config.key, "qr", seed));
    if (config.production && visit.closed_at !== null) {
      throw new ApiError(409, "visit_closed", "This visit is closed.");
    }
    const result = await store.assign(db, visit.id, ids[randomIndex(ids.length)], ids, seed, hash,
      config.ttl, config.issuance, config.production);
    claim = result.claim;
    created = result.created;
    if (!claim && result.paused) throw new ApiError(503, "issuance_paused", "Souvenir issuance is paused.");
    if (!claim && result.limitReached) throw new ApiError(429, "issuance_limit", "Today's souvenir issuance limit has been reached.");
  }
  const issued = liveClaim(claim ?? unavailable());
  const claimToken = await derive(config.key, "qr", issued.nonce);
  return json({
    assignmentId: issued.assignment_id, souvenir: publicEntry(issued),
    claimUrl: config.production ? `/claim#${claimToken}` : `/api/claims/${claimToken}`,
    pageUrl: `/claim#${claimToken}`,
    issuedAt: iso(issued.issued_at), expiresAt: iso(issued.expires_at),
  }, created ? 201 : 200);
}

async function visits(request: Request, db: D1Database, config: Settings, kioskId: string, id?: string, action?: string): Promise<Response> {
  if (!id) {
    const key = request.headers.get("Idempotency-Key") ?? "";
    const value = await body(request);
    fields(value, ["performer"]);
    if (!identifier.test(key) || !store.performers.some(performer => performer === value.performer)) invalid();
    const { visit, capped } = await store.startVisit(db, kioskId, key, value.performer as string, config.visitsPerKiosk);
    if (!visit && capped) throw new ApiError(429, "kiosk_visit_limit", "This kiosk has reached its daily visit limit.");
    if (!visit || visit.performer !== value.performer) {
      throw new ApiError(409, "visit_conflict", "Resume the existing visit or use a new key for a new visit.");
    }
    return json({
      visitId: visit.id, visitToken: await derive(config.key, "visit", visit.id),
      performer: visit.performer, nextMilestone: store.milestones[visit.progress] ?? null,
    });
  }

  if (!visitIdPattern.test(id)) notFound();
  const visit = await authorizedVisit(request, db, config, id, kioskId);
  const value = await body(request);
  if (action === "progress") {
    fields(value, ["eventId", "sequence", "milestone"]);
    const sequence = value.sequence;
    if (typeof value.eventId !== "string" || !identifier.test(value.eventId) || typeof sequence !== "number" ||
        !Number.isInteger(sequence) || sequence < 1 || sequence > store.milestones.length ||
        value.milestone !== store.milestones[sequence - 1]) invalid();
    if (!await store.recordProgress(db, id, value.eventId, sequence)) {
      throw new ApiError(409, "invalid_progress", "The visit is closed or the milestone is out of order.");
    }
    return json({ accepted: true, sequence });
  }

  fields(value, []);
  if (action === "reset") {
    await store.resetVisit(db, id);
    return json({ closed: true });
  }
  if (action === "complete") {
    const completed = await store.completeVisit(db, id);
    if (completed === null) throw new ApiError(409, "visit_incomplete", "Required milestones are missing or the visit is closed.");
    return json({ completedAt: iso(completed) });
  }
  return assignment(db, config, visit);
}

type Delivery = "preview" | "download";

async function claims(
  request: Request, env: Env, config: Settings, token: string, delivery?: Delivery, legacy = false,
): Promise<Response> {
  if (!isToken(token)) notFound();
  const hash = await digest(token);
  const claim = liveClaim(await store.claimForHash(env.DB, hash, config.production, config.controlledClaims, config.acceptancePaused));
  const now = Math.floor(Date.now() / 1000);
  if (claim.expires_at <= now) throw new ApiError(410, "claim_expired", "This souvenir link has expired.");
  if (!delivery) {
    if (config.production) await admit(env.DB, `claim:${claim.assignment_id}:resolve`, 12, 60);
    const expires = Math.min(now + 300, claim.expires_at);
    const grant = async (kind: Delivery) => ({
      url: `/api/claims/${kind}`,
      authorization: `${expires}.${await derive(config.key, "image", `${claim.assignment_id}:${expires}:${kind}`)}`,
      expiresAt: iso(expires),
    });
    const image = claim.asset_path ? await grant("preview") : null;
    return json({
      souvenir: publicEntry(claim), expiresAt: iso(claim.expires_at),
      image: image && legacy ? { ...image, url: `/api/claims/${token}/image` } : image,
      ...(legacy ? {} : { download: claim.asset_path ? await grant("download") : null }),
    });
  }

  const grant = /^Bearer (\d{1,12})\.([A-Za-z0-9_-]{43})$/.exec(request.headers.get("Authorization") ?? "");
  const expiry = Number(grant?.[1]);
  if (!grant || expiry <= now || expiry > claim.expires_at || expiry > now + 300 ||
      !matches(grant[2], await derive(config.key, "image", `${claim.assignment_id}:${expiry}:${delivery}`))) {
    throw new ApiError(401, "image_auth_required", "A valid image authorization is required.");
  }
  const path = claim.asset_path;
  if (!path) notFound();
  if (!/^\/media\/[a-z0-9][a-z0-9_-]{0,79}\.webp$/.test(path)) unavailable();
  if (config.production) await admit(env.DB, `claim:${claim.assignment_id}:${delivery}`, 12, 60);
  const assetRequest = new Request(new URL(path, request.url), { redirect: "manual" });
  const asset = await env.ASSETS.fetch(assetRequest);
  if (asset.status !== 200 || !asset.body || asset.headers.get("Content-Type")?.split(";")[0] !== "image/webp") {
    await asset.body?.cancel();
    unavailable();
  }
  try {
    // Authorization may have changed while waiting for the asset response.
    const current = liveClaim(await store.claimForHash(env.DB, hash, config.production, config.controlledClaims, config.acceptancePaused));
    if (current.asset_path !== path) unavailable();
    if (expiry <= Math.floor(Date.now() / 1000)) {
      throw new ApiError(401, "image_auth_required", "A valid image authorization is required.");
    }
  } catch (error) {
    await asset.body.cancel().catch(() => undefined);
    throw error;
  }
  return new Response(asset.body, {
    headers: { ...privateHeaders, "Content-Type": "image/webp",
      "Content-Disposition": `${delivery === "download" ? "attachment" : "inline"}; filename="souvenir.webp"` },
  });
}

async function admit(db: D1Database, scope: string, minute: number, day: number): Promise<void> {
  if (!await store.admitScoped(db, scope, minute, day)) {
    throw new ApiError(429, "request_limit", "Too many souvenir requests. Try again shortly.", 60);
  }
}

async function authenticate(request: Request, db: D1Database, url: URL) {
  await admit(db, "kiosk-auth", 60, 500);
  const value = await body(request);
  if (url.pathname.endsWith("/challenge")) {
    fields(value, ["kioskId"]);
    if (typeof value.kioskId !== "string" || !identifier.test(value.kioskId)) invalid();
    const challenge = await createChallenge(db, value.kioskId, url.origin);
    if (challenge) return json(challenge);
  } else {
    fields(value, ["challengeId", "signature"]);
    if (typeof value.challengeId !== "string" || !isToken(value.challengeId) ||
        typeof value.signature !== "string" || !/^[A-Za-z0-9_-]{86}$/.test(value.signature)) invalid();
    const session = await exchangeChallenge(db, value.challengeId, value.signature, url.origin);
    if (session) return json(session);
  }
  throw new ApiError(401, "kiosk_auth_failed", "Kiosk authentication failed.");
}

export default {
  async fetch(request: Request, env: Env): Promise<Response> {
    const url = new URL(request.url);
    try {
      const config = settings(env, url);
      if (request.method === "GET" && url.pathname === "/health") {
        const ready = await env.DB.prepare("SELECT 1 AS ready FROM souvenir_catalog LIMIT 1").first();
        if (!ready) unavailable();
        return json({ status: "ok" });
      }

      if (config.production && request.method === "POST" &&
          (url.pathname === "/api/kiosks/challenge" || url.pathname === "/api/kiosks/session")) {
        return await authenticate(request, env.DB, url);
      }
      if (request.method === "GET" && url.pathname === "/claim") return claimPage();

      const visit = request.method === "POST" && /^\/api\/visits(?:\/([^/]+)\/(progress|complete|assignment|reset))?$/.exec(url.pathname);
      if (visit) {
        const id = await requireKiosk(request, env.DB, config);
        if (config.production) {
          await admit(env.DB, `kiosk:${id}`, config.minuteRequests, config.dailyRequests);
          if (visit[2] === "assignment") await admit(env.DB, `assign:${id}`, 24, 500);
        } else if (!await store.admitRequest(env.DB, config.minuteRequests, config.dailyRequests)) {
          throw new ApiError(429, "request_limit", "Too many souvenir requests. Try again shortly.", 60);
        }
        return await visits(request, env.DB, config, id, visit[1], visit[2]);
      }

      if (request.method === "POST" && url.pathname === "/api/claims/resolve") {
        await admit(env.DB, "claim", 60, 1200);
        const value = await body(request);
        fields(value, ["token"]);
        if (typeof value.token !== "string") invalid();
        return await claims(request, env, config, value.token);
      }
      if (request.method === "GET" && (url.pathname === "/api/claims/preview" || url.pathname === "/api/claims/download")) {
        const kind = url.pathname.endsWith("/download") ? "download" : "preview";
        await admit(env.DB, kind, kind === "download" ? 40 : 60, kind === "download" ? 500 : 1000);
        return await claims(request, env, config, request.headers.get("X-Claim-Token") ?? "", kind);
      }

      const isCatalog = request.method === "GET" && url.pathname === "/api/souvenirs/catalog";
      const legacy = !config.production && request.method === "GET" && /^\/api\/claims\/([^/]+)(\/image)?$/.exec(url.pathname);
      if (isCatalog || legacy) {
        if (config.production) await admit(env.DB, "catalog", 30, 500);
        else if (!await store.admitRequest(env.DB, config.minuteRequests, config.dailyRequests)) {
          throw new ApiError(429, "request_limit", "Too many souvenir requests. Try again shortly.", 60);
        }
        if (isCatalog) return json({ souvenirs: (await store.catalog(env.DB, config.production)).map(publicEntry) });
        if (legacy) return await claims(request, env, config, legacy[1], legacy[2] ? "preview" : undefined, true);
      }
      return notFound();
    } catch (error) {
      if (error instanceof ApiError) {
        return json({ error: { code: error.code, message: error.message } }, error.status, error.retryAfter);
      }
      return json({ error: { code: "service_unavailable", message: "Souvenirs are temporarily unavailable." } }, 503);
    }
  },
} satisfies ExportedHandler<Env>;
