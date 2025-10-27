-- Journal Tags Table DDL
-- Generated for PostgreSQL.15
-- This table stores tags in normalized form (TagMode.TagTable)


-- Additional constraints and indexes:
;
do $BLOCK$
begin
	begin
		create index tags_persistence_id_sequence_nr_idx on "public"."tags" (persistence_id, sequence_nr);
	exception
		when duplicate_table
		then raise notice 'index "tags_persistence_id_sequence_nr_idx" on "public"."tags" already exists, skipping';
	end;

	begin
		create index tags_tag_idx on "public"."tags" (tag);
	exception
		when duplicate_table
		then raise notice 'index "tags_tag_idx" on "public"."tags" already exists, skipping';
	end;
end;
$BLOCK$

