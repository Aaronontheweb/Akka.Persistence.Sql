-- Journal Metadata Table DDL
-- Generated for SQLite (table-mapping = sqlite)
-- This table is used for delete-compatibility-mode
-- Use this DDL when migrating from Akka.Persistence.Sqlite

CREATE TABLE IF NOT EXISTS metadata (
    persistence_id TEXT NOT NULL,
    sequence_nr INTEGER NOT NULL,
    PRIMARY KEY (persistence_id, sequence_nr)
);

