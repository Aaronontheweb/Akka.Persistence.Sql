-- Journal Table DDL
-- Generated for SQLite (table-mapping = default)
-- This table stores all persisted events

CREATE TABLE IF NOT EXISTS journal (
    ordering INTEGER PRIMARY KEY AUTOINCREMENT,
    created INTEGER NOT NULL,
    deleted INTEGER NOT NULL,
    persistence_id TEXT NOT NULL,
    sequence_number INTEGER NOT NULL,
    message BLOB NOT NULL,
    manifest TEXT,
    serializer_id INTEGER,
    UNIQUE (persistence_id, sequence_number)
);

CREATE INDEX IF NOT EXISTS journal_created_idx ON journal (created);
CREATE INDEX IF NOT EXISTS journal_sequence_number_idx ON journal (sequence_number);

