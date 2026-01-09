-- Journal Tags Table DDL
-- Generated for SQLite (table-mapping = default)
-- This table stores tags in normalized form (TagMode.TagTable)

CREATE TABLE IF NOT EXISTS tags (
    ordering_id INTEGER NOT NULL,
    tag TEXT NOT NULL,
    sequence_nr INTEGER NOT NULL,
    persistence_id TEXT NOT NULL,
    PRIMARY KEY (ordering_id, tag)
);

CREATE INDEX IF NOT EXISTS tags_persistence_id_sequence_nr_idx ON tags (persistence_id, sequence_nr);
CREATE INDEX IF NOT EXISTS tags_tag_idx ON tags (tag);

