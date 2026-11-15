CREATE TABLE kiosk_acceptance_grants (
    visit_id TEXT PRIMARY KEY NOT NULL REFERENCES visits(id),
    kiosk_id TEXT NOT NULL REFERENCES kiosk_registry(id),
    created_at INTEGER NOT NULL,
    expires_at INTEGER NOT NULL CHECK (expires_at - created_at BETWEEN 1 AND 900),
    used_assignment_id TEXT UNIQUE REFERENCES assignments(id),
    revoked_at INTEGER
);

CREATE TRIGGER audit_kiosk_acceptance_grant AFTER INSERT ON kiosk_acceptance_grants
BEGIN
    INSERT INTO admin_audit(recorded_at, actor, action, target, reason)
    VALUES (NEW.created_at, 'operator', 'kiosk-acceptance-granted', NEW.visit_id, NEW.kiosk_id);
END;

CREATE TRIGGER audit_kiosk_acceptance_use AFTER UPDATE OF used_assignment_id ON kiosk_acceptance_grants
WHEN OLD.used_assignment_id IS NULL AND NEW.used_assignment_id IS NOT NULL
BEGIN
    INSERT INTO admin_audit(recorded_at, actor, action, target, reason)
    VALUES (unixepoch(), 'worker', 'kiosk-acceptance-used', NEW.visit_id, NEW.used_assignment_id);
END;

CREATE TRIGGER audit_kiosk_acceptance_revoke AFTER UPDATE OF revoked_at ON kiosk_acceptance_grants
WHEN OLD.revoked_at IS NULL AND NEW.revoked_at IS NOT NULL
BEGIN
    INSERT INTO admin_audit(recorded_at, actor, action, target, reason)
    VALUES (NEW.revoked_at, 'operator', 'kiosk-acceptance-revoked', NEW.visit_id, NEW.kiosk_id);
END;
