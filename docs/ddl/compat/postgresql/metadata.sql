-- Journal Metadata Table DDL
-- Generated for PostgreSQL.15
-- This table is used for delete-compatibility-mode

CREATE TABLE IF NOT EXISTS "public"."metadata" (
    "persistence_id" text NOT NULL,
    "sequence_nr" bigint NOT NULL,
    CONSTRAINT "PK_metadata" PRIMARY KEY ("persistence_id", "sequence_nr")
);


