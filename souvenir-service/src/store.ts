export const performers = [
  "GAIA", "PANDEMIC", "RE-BIRTH", "SANCTUARY", "WHITE-GUARDIAN", "CONSCIENCE",
] as const;
export const milestones = ["entered", "journey-started", "journey-finished", "closing-reached"] as const;
const idleSeconds = 20 * 60;

export type Visit = {
  id: string;
  performer: string;
  completed_at: number | null;
  closed_at: number | null;
  updated_at: number;
  progress: number;
};

type CatalogEntry = {
  id: string;
  title_en: string;
  title_zh_tw: string;
  asset_path: string | null;
};

export type Claim = CatalogEntry & {
  assignment_id: string;
  nonce: string;
  issued_at: number;
  expires_at: number;
  revoked_at: number | null;
  available: number;
};

const visitSelect = `SELECT v.*, (SELECT COUNT(*) FROM visit_progress WHERE visit_id = v.id) AS progress
  FROM visits v JOIN kiosk_registry k ON k.id = v.kiosk_id AND k.active = 1`;
const claimSelect = `SELECT c.id, c.title_en, c.title_zh_tw, c.asset_path, a.id AS assignment_id,
  q.nonce, q.issued_at, q.expires_at, q.revoked_at,
  (q.revoked_at IS NULL AND q.expires_at > unixepoch()) AS available
  FROM qr_tokens q JOIN assignments a ON a.id = q.assignment_id
  JOIN souvenir_catalog c ON c.id = a.souvenir_id`;
const eligible = "active = 1 AND retired_at IS NULL";
const productionAsset = "test_only = 0 AND asset_path IS NOT NULL AND asset_path <> '/media/placeholder-test.webp'";
const dayWindow = "unixepoch() - unixepoch() % 86400";

export async function admitScoped(db: D1Database, scope: string, minuteLimit: number, dayLimit: number): Promise<boolean> {
  const now = Math.floor(Date.now() / 1000);
  const minute = now - now % 60;
  const day = now - now % 86400;
  const daily = scope + ":day";
  const [admitted] = await db.batch([
    db.prepare(`INSERT INTO request_limits(scope, window_start, count)
      SELECT ?, ?, 1 WHERE COALESCE((SELECT count FROM request_limits
        WHERE scope = ? AND window_start = ?), 0) < ?
      ON CONFLICT(scope, window_start) DO UPDATE SET count = count + 1
      WHERE count < ? RETURNING count`).bind(scope, minute, daily, day, dayLimit, minuteLimit),
    db.prepare(`INSERT INTO request_limits(scope, window_start, count)
      SELECT ?, ?, 1 WHERE changes() = 1
      ON CONFLICT(scope, window_start) DO UPDATE SET count = count + 1`).bind(daily, day),
  ]);
  return admitted.results.length === 1;
}

export async function admitRequest(db: D1Database, minuteLimit: number, dayLimit: number): Promise<boolean> {
  // Use the same windows for both statements even if this batch crosses midnight.
  const now = Math.floor(Date.now() / 1000);
  const minute = now - now % 60;
  const day = now - now % 86400;
  const [admitted] = await db.batch([
    db.prepare(`INSERT INTO issuance_limits(kind, window_start, count)
      SELECT 'request', ?, 1 WHERE COALESCE((SELECT count FROM issuance_limits
        WHERE kind = 'request-day' AND window_start = ?), 0) < ?
      ON CONFLICT(kind, window_start) DO UPDATE SET count = count + 1
      WHERE count < ? RETURNING count`).bind(minute, day, dayLimit, minuteLimit),
    db.prepare(`INSERT INTO issuance_limits(kind, window_start, count)
      SELECT 'request-day', ?, 1 WHERE changes() = 1
      ON CONFLICT(kind, window_start) DO UPDATE SET count = count + 1`).bind(day),
  ]);
  return admitted.results.length === 1;
}

export async function catalog(db: D1Database, production: boolean): Promise<CatalogEntry[]> {
  return (await db.prepare(`SELECT id, title_en, title_zh_tw, asset_path
    FROM souvenir_catalog WHERE ${eligible} AND (? = 0 OR (${productionAsset}))
    ORDER BY id`).bind(Number(production)).all<CatalogEntry>()).results;
}

export async function startVisit(db: D1Database, kiosk: string, key: string, performer: string, limit: number) {
  const dailyVisits = `SELECT COUNT(*) FROM visits WHERE kiosk_id = ? AND started_at >= ${dayWindow}`;
  const results = await db.batch<Visit & { count: number }>([
    db.prepare(`UPDATE visits SET closed_at = unixepoch()
      WHERE kiosk_id = ? AND closed_at IS NULL AND updated_at <= unixepoch() - ?`).bind(kiosk, idleSeconds),
    db.prepare(`INSERT INTO visits(id, kiosk_id, creation_key, performer, started_at, updated_at)
      SELECT ?, id, ?, ?, unixepoch(), unixepoch() FROM kiosk_registry WHERE id = ? AND active = 1
        AND (${dailyVisits}) < ?
      ON CONFLICT DO NOTHING`).bind(crypto.randomUUID(), key, performer, kiosk, kiosk, limit),
    db.prepare(`${visitSelect} WHERE v.kiosk_id = ? AND v.creation_key = ?`).bind(kiosk, key),
    db.prepare(`SELECT (${dailyVisits}) AS count`).bind(kiosk),
  ]);
  return { visit: results[2].results[0] ?? null, capped: (results[3].results[0]?.count ?? 0) >= limit };
}

export async function resetVisit(db: D1Database, id: string): Promise<void> {
  await db.prepare("UPDATE visits SET closed_at = unixepoch() WHERE id = ? AND closed_at IS NULL").bind(id).run();
}
export function findVisit(db: D1Database, kiosk: string, id: string): Promise<Visit | null> {
  return db.prepare(`${visitSelect} WHERE v.kiosk_id = ? AND v.id = ?`).bind(kiosk, id).first<Visit>();
}

export async function recordProgress(db: D1Database, id: string, eventId: string, sequence: number): Promise<boolean> {
  const results = await db.batch([
    db.prepare(`INSERT INTO visit_progress(visit_id, sequence, event_id, milestone, recorded_at)
      SELECT id, ?, ?, ?, unixepoch() FROM visits
      WHERE id = ? AND closed_at IS NULL AND completed_at IS NULL AND updated_at > unixepoch() - ?
        AND ? = 1 + (SELECT COUNT(*) FROM visit_progress WHERE visit_id = ?)
      ON CONFLICT DO NOTHING`).bind(sequence, eventId, milestones[sequence - 1], id, idleSeconds, sequence, id),
    db.prepare("UPDATE visits SET updated_at = unixepoch() WHERE id = ? AND changes() = 1").bind(id),
    db.prepare("SELECT 1 FROM visit_progress WHERE visit_id = ? AND sequence = ? AND event_id = ?").bind(id, sequence, eventId),
  ]);
  return results[2].results.length === 1;
}

export async function completeVisit(db: D1Database, id: string): Promise<number | null> {
  const results = await db.batch<{ completed_at: number | null }>([
    db.prepare(`UPDATE visits SET completed_at = unixepoch(), updated_at = unixepoch()
      WHERE id = ? AND closed_at IS NULL AND completed_at IS NULL AND updated_at > unixepoch() - ?
        AND (SELECT COUNT(*) FROM visit_progress WHERE visit_id = ?) = ?`).bind(id, idleSeconds, id, milestones.length),
    db.prepare("SELECT completed_at FROM visits WHERE id = ?").bind(id),
  ]);
  return results[1].results[0]?.completed_at ?? null;
}

export function claimForVisit(db: D1Database, id: string): Promise<Claim | null> {
  return db.prepare(`${claimSelect} WHERE a.visit_id = ?`).bind(id).first<Claim>();
}

export function claimForHash(
  db: D1Database, hash: string, production: boolean, controlledClaims: string, acceptancePaused: boolean,
): Promise<Claim | null> {
  return db.prepare(`${claimSelect} WHERE q.token_hash = ? AND (? = 0 OR
    (c.test_only = 0 AND c.asset_path <> '/media/placeholder-test.webp') OR
    (c.test_only = 1 AND c.id = 'placeholder-test' AND c.asset_path = '/media/placeholder-test.webp'
      AND (a.id IN (SELECT value FROM json_each(?)) OR (? = 1 AND EXISTS (
        SELECT 1 FROM kiosk_acceptance_grants g WHERE g.used_assignment_id = a.id AND g.visit_id = a.visit_id)))
      AND (SELECT issuance_paused FROM souvenir_controls WHERE id = 1) = 1))`)
    .bind(hash, Number(production), controlledClaims, Number(acceptancePaused)).first<Claim>();
}

export async function assignAcceptance(db: D1Database, visit: string, seed: string, hash: string): Promise<{
  claim: Claim | null; created: boolean;
}> {
  const candidate = crypto.randomUUID();
  const [, , , , found] = await db.batch<Claim>([
    db.prepare(`INSERT INTO assignments(id, visit_id, souvenir_id, assigned_at)
      SELECT ?, v.id, c.id, unixepoch()
      FROM visits v JOIN kiosk_acceptance_grants g ON g.visit_id = v.id AND g.kiosk_id = v.kiosk_id
      JOIN kiosk_registry k ON k.id = g.kiosk_id AND k.active = 1 AND k.public_key IS NOT NULL
      JOIN souvenir_catalog c ON c.id = 'placeholder-test' AND c.test_only = 1 AND c.active = 1 AND c.retired_at IS NULL
        AND c.asset_path = '/media/placeholder-test.webp'
      WHERE v.id = ? AND v.completed_at IS NOT NULL AND v.closed_at IS NULL
        AND g.expires_at > unixepoch() AND g.revoked_at IS NULL AND g.used_assignment_id IS NULL
        AND (SELECT COUNT(*) FROM visit_progress WHERE visit_id = v.id) = ?
        AND (SELECT issuance_paused FROM souvenir_controls WHERE id = 1) = 1
      ON CONFLICT(visit_id) DO NOTHING`).bind(candidate, visit, milestones.length),
    db.prepare(`INSERT INTO qr_tokens(assignment_id, token_hash, nonce, issued_at, expires_at)
      SELECT id, ?, ?, assigned_at, assigned_at + 1800 FROM assignments WHERE id = ?`).bind(hash, seed, candidate),
    db.prepare(`UPDATE kiosk_acceptance_grants SET used_assignment_id = ?
      WHERE visit_id = ? AND EXISTS (SELECT 1 FROM assignments WHERE id = ?)`).bind(candidate, visit, candidate),
    db.prepare(`UPDATE visits SET closed_at = unixepoch() WHERE id = ? AND closed_at IS NULL
      AND EXISTS (SELECT 1 FROM assignments WHERE id = ?)`).bind(visit, candidate),
    db.prepare(`${claimSelect} WHERE a.visit_id = ?`).bind(visit),
  ]);
  const claim = found.results[0] ?? null;
  return { claim, created: claim?.assignment_id === candidate };
}

export async function assign(
  db: D1Database, visit: string, souvenir: string, snapshot: string[], nonce: string, hash: string,
  ttl: number, limit: number, production: boolean,
): Promise<{ claim: Claim | null; created: boolean; limitReached: boolean; paused: boolean }> {
  const candidate = crypto.randomUUID();
  const results = await db.batch<Claim & { count: number; issuance_paused: number }>([
    // Check the exact eligible set inside the write transaction. A changed catalog requires a new draw.
    db.prepare(`INSERT INTO assignments(id, visit_id, souvenir_id, assigned_at)
      SELECT ?, id, ?, unixepoch() FROM visits WHERE id = ? AND completed_at IS NOT NULL
        AND (? = 0 OR closed_at IS NULL)
        AND EXISTS (SELECT 1 FROM kiosk_registry k WHERE k.id = visits.kiosk_id AND k.active = 1)
        AND (SELECT issuance_paused FROM souvenir_controls WHERE id = 1) = 0
        AND ? = (SELECT json_group_array(id) FROM (SELECT id FROM souvenir_catalog WHERE ${eligible}
          AND (? = 0 OR (${productionAsset})) ORDER BY id))
        AND COALESCE((SELECT count FROM issuance_limits WHERE kind = 'issuance' AND window_start = ${dayWindow}), 0) < ?
      ON CONFLICT(visit_id) DO NOTHING`).bind(candidate, souvenir, visit, Number(production), JSON.stringify(snapshot), Number(production), limit),
    db.prepare(`INSERT INTO qr_tokens(assignment_id, token_hash, nonce, issued_at, expires_at)
      SELECT id, ?, ?, assigned_at, assigned_at + ? FROM assignments WHERE id = ?`).bind(hash, nonce, ttl, candidate),
    db.prepare(`INSERT INTO issuance_limits(kind, window_start, count)
      SELECT 'issuance', assigned_at - assigned_at % 86400, 1 FROM assignments WHERE id = ?
      ON CONFLICT(kind, window_start) DO UPDATE SET count = count + 1`).bind(candidate),
    db.prepare(`UPDATE visits SET closed_at = unixepoch()
      WHERE id = ? AND closed_at IS NULL AND EXISTS (SELECT 1 FROM assignments WHERE id = ?)`).bind(visit, candidate),
    db.prepare(`${claimSelect} WHERE a.visit_id = ?`).bind(visit),
    db.prepare(`SELECT count FROM issuance_limits WHERE kind = 'issuance' AND window_start = ${dayWindow}`),
    db.prepare("SELECT issuance_paused FROM souvenir_controls WHERE id = 1"),
  ]);
  const claim = results[4].results[0] ?? null;
  return { claim, created: claim?.assignment_id === candidate, limitReached: (results[5].results[0]?.count ?? 0) >= limit,
    paused: results[6].results[0]?.issuance_paused !== 0 };
}
