-- Journal Metadata Table DDL
-- Generated for SQLite (table-mapping = default)
-- This table is used for delete-compatibility-mode

CREATE TABLE IF NOT EXISTS journal_metadata (
    persistence_id TEXT NOT NULL,
    sequence_number INTEGER NOT NULL,
    PRIMARY KEY (persistence_id, sequence_number)
);

