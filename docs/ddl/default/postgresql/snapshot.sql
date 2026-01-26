-- Snapshot Table DDL
-- Generated for PostgreSQL (table-mapping = default)
-- This table stores actor state snapshots

CREATE TABLE IF NOT EXISTS "public"."snapshot" (
    "persistence_id" text NOT NULL,
    "sequence_number" bigint NOT NULL,
    "created" bigint NOT NULL,
    "snapshot" bytea,
    "manifest" text,
    "serializer_id" integer,
    CONSTRAINT "PK_snapshot" PRIMARY KEY ("persistence_id", "sequence_number")
);



-- Additional constraints and indexes:
;
do $BLOCK$
begin
	begin
		create index snapshot_sequence_number_idx on "public"."snapshot" (sequence_number);
	exception
		when duplicate_table
		then raise notice 'index "snapshot_sequence_number_idx" on "public"."snapshot" already exists, skipping';
	end;

	begin
		create index snapshot_created_idx on "public"."snapshot" (created);
	exception
		when duplicate_table
		then raise notice 'index "snapshot_created_idx" on "public"."snapshot" already exists, skipping';
	end;
end;
$BLOCK$

