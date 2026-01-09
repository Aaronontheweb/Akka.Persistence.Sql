-- Snapshot Table DDL
-- Generated for SQLite (table-mapping = sqlite)
-- This table stores actor state snapshots
-- Use this DDL when migrating from Akka.Persistence.Sqlite

CREATE TABLE IF NOT EXISTS snapshot (
    persistence_id TEXT NOT NULL,
    sequence_nr INTEGER NOT NULL,
    created_at INTEGER NOT NULL,
    payload BLOB,
    manifest TEXT,
    serializer_id INTEGER,
    PRIMARY KEY (persistence_id, sequence_nr)
);

CREATE INDEX IF NOT EXISTS snapshot_sequence_nr_idx ON snapshot (sequence_nr);
CREATE INDEX IF NOT EXISTS snapshot_created_at_idx ON snapshot (created_at);

