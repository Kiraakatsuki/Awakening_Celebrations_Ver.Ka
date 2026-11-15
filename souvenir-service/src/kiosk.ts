import { digest, nonce } from "./security";

type Challenge = { kiosk_id: string; key_version: number; public_key: string; expires_at: number };
const encoder = new TextEncoder();

export async function createChallenge(db: D1Database, kioskId: string, origin: string) {
  const id = nonce();
  const row = await db.prepare(`INSERT INTO kiosk_challenges(id, kiosk_id, key_version, expires_at)
    SELECT ?, id, key_version, unixepoch() + 60 FROM kiosk_registry
    WHERE id = ? AND active = 1 AND public_key IS NOT NULL RETURNING expires_at`)
    .bind(id, kioskId).first<{ expires_at: number }>();
  if (!row) return null;
  return {
    challengeId: id,
    expiresAt: row.expires_at,
    message: challengeMessage(origin, kioskId, id, row.expires_at),
  };
}

function challengeMessage(origin: string, kioskId: string, challengeId: string, expiry: number): string {
  return ["awakening-souvenir/kiosk/v1", origin, kioskId, challengeId, String(expiry)].join("\n");
}

function decode(value: string): Uint8Array<ArrayBuffer> {
  return Uint8Array.from(atob(value.replaceAll("-", "+").replaceAll("_", "/")), character => character.charCodeAt(0));
}

export async function exchangeChallenge(db: D1Database, id: string, signature: string, origin: string) {
  const challenge = await db.prepare(`SELECT c.kiosk_id, c.key_version, c.expires_at, k.public_key
    FROM kiosk_challenges c JOIN kiosk_registry k ON k.id = c.kiosk_id
    WHERE c.id = ? AND c.consumed_at IS NULL AND c.expires_at > unixepoch()
      AND k.active = 1 AND k.key_version = c.key_version AND k.public_key IS NOT NULL`).bind(id).first<Challenge>();
  if (!challenge) return null;
  try {
    const key = await crypto.subtle.importKey(
      "spki", decode(challenge.public_key), { name: "ECDSA", namedCurve: "P-256" }, false, ["verify"],
    );
    const message = challengeMessage(origin, challenge.kiosk_id, id, challenge.expires_at);
    if (!await crypto.subtle.verify({ name: "ECDSA", hash: "SHA-256" }, key, decode(signature), encoder.encode(message))) {
      return null;
    }
  } catch {
    return null;
  }

  const session = nonce();
  const hash = await digest(session);
  const [inserted] = await db.batch<{ expires_at: number }>([
    db.prepare(`INSERT INTO kiosk_sessions(token_hash, challenge_id, kiosk_id, key_version, expires_at)
      SELECT ?, c.id, c.kiosk_id, c.key_version, unixepoch() + 300 FROM kiosk_challenges c
      JOIN kiosk_registry k ON k.id = c.kiosk_id WHERE c.id = ? AND c.consumed_at IS NULL
        AND c.expires_at > unixepoch() AND k.active = 1 AND c.key_version = k.key_version
        AND k.public_key = ? ON CONFLICT(challenge_id) DO NOTHING RETURNING expires_at`)
      .bind(hash, id, challenge.public_key),
    db.prepare(`UPDATE kiosk_challenges SET consumed_at = unixepoch() WHERE id = ?
      AND EXISTS (SELECT 1 FROM kiosk_sessions WHERE token_hash = ?)`).bind(id, hash),
  ]);
  const row = inserted.results[0];
  return row ? { session, expiresAt: row.expires_at } : null;
}

export async function sessionKiosk(db: D1Database, token: string): Promise<string | null> {
  return db.prepare(`SELECT s.kiosk_id FROM kiosk_sessions s JOIN kiosk_registry k ON k.id = s.kiosk_id
    WHERE s.token_hash = ? AND s.revoked_at IS NULL AND s.expires_at > unixepoch()
      AND k.active = 1 AND k.key_version = s.key_version AND k.public_key IS NOT NULL`)
    .bind(await digest(token)).first<string>("kiosk_id");
}
