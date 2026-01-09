-- Journal Metadata Table DDL
-- Generated for PostgreSQL (table-mapping = default)
-- This table is used for delete-compatibility-mode

CREATE TABLE IF NOT EXISTS "public"."journal_metadata" (
    "persistence_id" text NOT NULL,
    "sequence_number" bigint NOT NULL,
    CONSTRAINT "PK_journal_metadata" PRIMARY KEY ("persistence_id", "sequence_number")
);

