-- Journal Table DDL
-- Generated for SQLite (table-mapping = sqlite)
-- This table stores all persisted events
-- Use this DDL when migrating from Akka.Persistence.Sqlite

CREATE TABLE IF NOT EXISTS event_journal (
    ordering INTEGER PRIMARY KEY AUTOINCREMENT,
    is_deleted INTEGER NOT NULL DEFAULT 0,
    persistence_id TEXT NOT NULL,
    sequence_nr INTEGER NOT NULL,
    timestamp INTEGER NOT NULL,
    tags TEXT,
    payload BLOB NOT NULL,
    serializer_id INTEGER,
    manifest TEXT,
    UNIQUE (persistence_id, sequence_nr)
);

CREATE INDEX IF NOT EXISTS event_journal_ordering_idx ON event_journal (ordering);
CREATE INDEX IF NOT EXISTS event_journal_timestamp_idx ON event_journal (timestamp);
CREATE INDEX IF NOT EXISTS event_journal_persistence_id_idx ON event_journal (persistence_id);

