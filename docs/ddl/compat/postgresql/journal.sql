-- Journal Table DDL
-- Generated for PostgreSQL (table-mapping = postgresql)
-- This table stores all persisted events
-- Use this DDL when migrating from Akka.Persistence.PostgreSql

CREATE TABLE IF NOT EXISTS "public"."event_journal" (
    "ordering" bigserial NOT NULL,
    "is_deleted" boolean NOT NULL DEFAULT false,
    "persistence_id" text NOT NULL,
    "sequence_nr" bigint NOT NULL,
    "created_at" bigint NOT NULL,
    "tags" text,
    "payload" bytea NOT NULL,
    "serializer_id" integer,
    "manifest" text,
    CONSTRAINT "PK_event_journal" PRIMARY KEY ("ordering")
);

-- Additional constraints and indexes:
DO $BLOCK$
BEGIN
    BEGIN
        ALTER TABLE "public"."event_journal" ADD CONSTRAINT event_journal_uq UNIQUE (persistence_id, sequence_nr);
    EXCEPTION
        WHEN duplicate_table OR duplicate_object
        THEN RAISE NOTICE 'unique constraint "event_journal_uq" on "public"."event_journal" already exists, skipping';
    END;

    BEGIN
        CREATE INDEX event_journal_ordering_idx ON "public"."event_journal" (ordering);
    EXCEPTION
        WHEN duplicate_table
        THEN RAISE NOTICE 'index "event_journal_ordering_idx" on "public"."event_journal" already exists, skipping';
    END;

    BEGIN
        CREATE INDEX event_journal_created_at_idx ON "public"."event_journal" (created_at);
    EXCEPTION
        WHEN duplicate_table
        THEN RAISE NOTICE 'index "event_journal_created_at_idx" on "public"."event_journal" already exists, skipping';
    END;

    BEGIN
        CREATE INDEX event_journal_persistence_id_idx ON "public"."event_journal" (persistence_id);
    EXCEPTION
        WHEN duplicate_table
        THEN RAISE NOTICE 'index "event_journal_persistence_id_idx" on "public"."event_journal" already exists, skipping';
    END;
END;
$BLOCK$

