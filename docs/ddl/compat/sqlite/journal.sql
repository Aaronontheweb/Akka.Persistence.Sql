-- Journal Table DDL
-- Generated for SQLite (table-mapping = sqlite)
-- This table stores all persisted events
-- Use this DDL when migrating from Akka.Persistence.Sqlite

CREATE TABLE IF NOT EXISTS event_journal (
    ordering INTEGER PRIMARY KEY AUTOINCREMENT,
    created_at INTEGER NOT NULL,
    is_deleted INTEGER NOT NULL,
    persistence_id TEXT NOT NULL,
    sequence_nr INTEGER NOT NULL,
    payload BLOB NOT NULL,
    manifest TEXT,
    serializer_id INTEGER,
    UNIQUE (persistence_id, sequence_nr)
);

CREATE INDEX IF NOT EXISTS event_journal_created_at_idx ON event_journal (created_at);
CREATE INDEX IF NOT EXISTS event_journal_sequence_nr_idx ON event_journal (sequence_nr);

