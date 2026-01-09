-- Journal Table DDL
-- Generated for PostgreSQL (table-mapping = default)
-- This table stores all persisted events

CREATE TABLE IF NOT EXISTS "public"."journal" (
    "ordering" bigint NOT NULL,
    "created" bigint NOT NULL,
    "deleted" boolean NOT NULL,
    "persistence_id" text NOT NULL,
    "sequence_number" bigint NOT NULL,
    "message" bytea NOT NULL,
    "manifest" text,
    "serializer_id" integer,
    CONSTRAINT "PK_journal" PRIMARY KEY ("ordering")
);



-- Additional constraints and indexes:
;
do $BLOCK$
begin
	begin
		alter table "public"."journal" add constraint journal_uq unique (persistence_id, sequence_number);
	exception
		when duplicate_table
		then raise notice 'unique constraint "journal_uq" on "public"."journal" already exists, skipping';
	end;

	begin
		create index journal_created_idx on "public"."journal" (created);
	exception
		when duplicate_table
		then raise notice 'index "journal_created_idx" on "public"."journal" already exists, skipping';
	end;

	begin
		create index journal_sequence_number_idx on "public"."journal" (sequence_number);
	exception
		when duplicate_table
		then raise notice 'index "journal_sequence_number_idx" on "public"."journal" already exists, skipping';
	end;
end;
$BLOCK$

