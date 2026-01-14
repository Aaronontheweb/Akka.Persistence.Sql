-- Journal Table DDL
-- Generated for SQLite (table-mapping = default)
-- This table stores all persisted events

CREATE TABLE IF NOT EXISTS journal (
    ordering INTEGER PRIMARY KEY AUTOINCREMENT,
    deleted INTEGER NOT NULL DEFAULT 0,
    persistence_id TEXT NOT NULL,
    sequence_number INTEGER NOT NULL,
    created INTEGER NOT NULL,
    tags TEXT,
    message BLOB NOT NULL,
    identifier INTEGER,
    manifest TEXT,
    writer_uuid TEXT,
    UNIQUE (persistence_id, sequence_number)
);

CREATE INDEX IF NOT EXISTS journal_ordering_idx ON journal (ordering);
CREATE INDEX IF NOT EXISTS journal_created_idx ON journal (created);
CREATE INDEX IF NOT EXISTS journal_persistence_id_idx ON journal (persistence_id);

