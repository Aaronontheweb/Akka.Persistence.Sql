-- Snapshot Table DDL
-- Generated for PostgreSQL.15
-- This table stores actor state snapshots


-- Additional constraints and indexes:
;
do $BLOCK$
begin
	begin
		create index snapshot_store_sequence_nr_idx on "public"."snapshot_store" (sequence_nr);
	exception
		when duplicate_table
		then raise notice 'index "snapshot_store_sequence_nr_idx" on "public"."snapshot_store" already exists, skipping';
	end;

	begin
		create index snapshot_store_created_at_idx on "public"."snapshot_store" (created_at);
	exception
		when duplicate_table
		then raise notice 'index "snapshot_store_created_at_idx" on "public"."snapshot_store" already exists, skipping';
	end;
end;
$BLOCK$

