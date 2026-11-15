CREATE TABLE kiosk_registry (
    id TEXT PRIMARY KEY NOT NULL,
    active INTEGER NOT NULL DEFAULT 1 CHECK (active IN (0, 1))
);

CREATE TABLE visits (
    id TEXT PRIMARY KEY NOT NULL,
    kiosk_id TEXT NOT NULL REFERENCES kiosk_registry(id),
    creation_key TEXT NOT NULL,
    performer TEXT NOT NULL CHECK (performer IN (
        'GAIA', 'PANDEMIC', 'RE-BIRTH', 'SANCTUARY', 'WHITE-GUARDIAN', 'CONSCIENCE'
    )),
    started_at INTEGER NOT NULL,
    updated_at INTEGER NOT NULL,
    completed_at INTEGER,
    closed_at INTEGER,
    UNIQUE (kiosk_id, creation_key)
);
CREATE UNIQUE INDEX one_open_visit_per_kiosk ON visits(kiosk_id) WHERE closed_at IS NULL;

CREATE TABLE visit_progress (
    visit_id TEXT NOT NULL REFERENCES visits(id),
    sequence INTEGER NOT NULL,
    event_id TEXT NOT NULL,
    milestone TEXT NOT NULL,
    recorded_at INTEGER NOT NULL,
    PRIMARY KEY (visit_id, sequence),
    UNIQUE (visit_id, event_id),
    CHECK ((sequence = 1 AND milestone = 'entered') OR
           (sequence = 2 AND milestone = 'journey-started') OR
           (sequence = 3 AND milestone = 'journey-finished') OR
           (sequence = 4 AND milestone = 'closing-reached'))
);

CREATE TABLE souvenir_catalog (
    id TEXT PRIMARY KEY NOT NULL,
    title_en TEXT NOT NULL,
    title_zh_tw TEXT NOT NULL,
    active INTEGER NOT NULL DEFAULT 0 CHECK (active IN (0, 1)),
    asset_path TEXT UNIQUE,
    retired_at INTEGER
);
CREATE INDEX catalog_eligible ON souvenir_catalog(id) WHERE active = 1 AND retired_at IS NULL;

CREATE TABLE assignments (
    id TEXT PRIMARY KEY NOT NULL,
    visit_id TEXT NOT NULL UNIQUE REFERENCES visits(id),
    souvenir_id TEXT NOT NULL REFERENCES souvenir_catalog(id),
    assigned_at INTEGER NOT NULL
);
CREATE INDEX assignments_by_souvenir ON assignments(souvenir_id);
CREATE TRIGGER assignment_requires_completion BEFORE INSERT ON assignments
WHEN NOT EXISTS (SELECT 1 FROM visits WHERE id = NEW.visit_id AND completed_at IS NOT NULL)
BEGIN
    SELECT RAISE(ABORT, 'visit completion required');
END;

CREATE TABLE qr_tokens (
    assignment_id TEXT PRIMARY KEY NOT NULL REFERENCES assignments(id),
    token_hash TEXT NOT NULL UNIQUE CHECK (length(token_hash) = 64),
    nonce TEXT NOT NULL,
    issued_at INTEGER NOT NULL,
    expires_at INTEGER NOT NULL,
    revoked_at INTEGER,
    revoked_by TEXT,
    revocation_reason TEXT,
    CHECK (expires_at - issued_at BETWEEN 1 AND 3600),
    CHECK ((revoked_at IS NULL AND revoked_by IS NULL AND revocation_reason IS NULL) OR
           (revoked_at IS NOT NULL AND length(trim(coalesce(revoked_by, ''))) > 0 AND
            length(trim(coalesce(revocation_reason, ''))) > 0))
);

CREATE TABLE issuance_limits (
    kind TEXT NOT NULL CHECK (kind IN ('request', 'request-day', 'issuance')),
    window_start INTEGER NOT NULL,
    count INTEGER NOT NULL CHECK (count >= 0),
    PRIMARY KEY (kind, window_start)
);

CREATE TABLE admin_audit (
    id INTEGER PRIMARY KEY,
    recorded_at INTEGER NOT NULL,
    actor TEXT NOT NULL,
    action TEXT NOT NULL,
    target TEXT NOT NULL,
    reason TEXT NOT NULL
);
CREATE INDEX admin_audit_by_target ON admin_audit(target, recorded_at);
CREATE TRIGGER protect_qr_issuance BEFORE UPDATE ON qr_tokens
WHEN OLD.assignment_id <> NEW.assignment_id OR OLD.token_hash <> NEW.token_hash OR OLD.nonce <> NEW.nonce
     OR OLD.issued_at <> NEW.issued_at OR OLD.expires_at <> NEW.expires_at
     OR (OLD.revoked_at IS NOT NULL AND (
         OLD.revoked_at IS NOT NEW.revoked_at OR OLD.revoked_by IS NOT NEW.revoked_by
         OR OLD.revocation_reason IS NOT NEW.revocation_reason
     ))
BEGIN
    SELECT RAISE(ABORT, 'QR issuance and revocation are fixed');
END;
CREATE TRIGGER audit_qr_revocation AFTER UPDATE OF revoked_at ON qr_tokens
WHEN OLD.revoked_at IS NULL AND NEW.revoked_at IS NOT NULL
BEGIN
    INSERT INTO admin_audit(recorded_at, actor, action, target, reason)
    VALUES (NEW.revoked_at, NEW.revoked_by, 'qr-revoked', NEW.assignment_id, NEW.revocation_reason);
END;
