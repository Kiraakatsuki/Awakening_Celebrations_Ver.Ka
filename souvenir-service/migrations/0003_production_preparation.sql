ALTER TABLE kiosk_registry ADD COLUMN public_key TEXT;
ALTER TABLE kiosk_registry ADD COLUMN key_version INTEGER NOT NULL DEFAULT 1;
CREATE TRIGGER kiosk_key_rotation BEFORE UPDATE OF public_key ON kiosk_registry
WHEN OLD.public_key IS NOT NEW.public_key AND NEW.key_version <= OLD.key_version
BEGIN
    SELECT RAISE(ABORT, 'increment kiosk key version');
END;

CREATE TABLE kiosk_challenges (
    id TEXT PRIMARY KEY NOT NULL,
    kiosk_id TEXT NOT NULL REFERENCES kiosk_registry(id),
    key_version INTEGER NOT NULL,
    expires_at INTEGER NOT NULL,
    consumed_at INTEGER
);
CREATE INDEX kiosk_challenges_expiry ON kiosk_challenges(expires_at);

CREATE TABLE kiosk_sessions (
    token_hash TEXT PRIMARY KEY NOT NULL,
    challenge_id TEXT NOT NULL UNIQUE REFERENCES kiosk_challenges(id),
    kiosk_id TEXT NOT NULL REFERENCES kiosk_registry(id),
    key_version INTEGER NOT NULL,
    expires_at INTEGER NOT NULL,
    revoked_at INTEGER
);
CREATE INDEX kiosk_sessions_expiry ON kiosk_sessions(expires_at);

CREATE TABLE request_limits (
    scope TEXT NOT NULL,
    window_start INTEGER NOT NULL,
    count INTEGER NOT NULL CHECK (count > 0),
    PRIMARY KEY(scope, window_start)
);
CREATE INDEX visits_by_kiosk_and_start ON visits(kiosk_id, started_at);

CREATE TABLE souvenir_controls (
    id INTEGER PRIMARY KEY CHECK (id = 1),
    issuance_paused INTEGER NOT NULL CHECK (issuance_paused IN (0, 1))
);
INSERT INTO souvenir_controls VALUES (1, 0);

ALTER TABLE souvenir_catalog ADD COLUMN test_only INTEGER NOT NULL DEFAULT 0 CHECK (test_only IN (0, 1));
UPDATE souvenir_catalog SET active = 0, test_only = 1 WHERE id IN
    ('test-01', 'test-02', 'test-03', 'test-04', 'test-05', 'test-06') AND asset_path IS NULL;
INSERT INTO souvenir_catalog(id, title_en, title_zh_tw, active, asset_path, test_only) VALUES
    ('placeholder-test', 'Digital souvenir — test', '數位紀念品 — 測試', 1, '/media/placeholder-test.webp', 1);
