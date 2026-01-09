-- Journal Table DDL
-- Generated for PostgreSQL.15
-- This table stores all persisted events

CREATE TABLE IF NOT EXISTS "public"."event_journal" (
    "ordering" bigint NOT NULL,
    "created_at" bigint NOT NULL,
    "is_deleted" boolean NOT NULL,
    "persistence_id" text NOT NULL,
    "sequence_nr" bigint NOT NULL,
    "payload" bytea NOT NULL,
    "manifest" text,
    "serializer_id" integer,
    CONSTRAINT "PK_event_journal" PRIMARY KEY ("ordering")
);



-- Additional constraints and indexes:
;
do $BLOCK$
begin
	begin
		alter table "public"."event_journal" add constraint event_journal_uq unique (persistence_id, sequence_nr);
	exception
		when duplicate_table
		then raise notice 'unique constraint "event_journal_uq" on "public"."event_journal" already exists, skipping';
	end;

	begin
		create index event_journal_created_at_idx on "public"."event_journal" (created_at);
	exception
		when duplicate_table
		then raise notice 'index "event_journal_created_at_idx" on "public"."event_journal" already exists, skipping';
	end;

	begin
		create index event_journal_sequence_nr_idx on "public"."event_journal" (sequence_nr);
	exception
		when duplicate_table
		then raise notice 'index "event_journal_sequence_nr_idx" on "public"."event_journal" already exists, skipping';
	end;
end;
$BLOCK$

