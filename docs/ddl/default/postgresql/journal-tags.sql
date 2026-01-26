-- Journal Tags Table DDL
-- Generated for PostgreSQL (table-mapping = default)
-- This table stores tags in normalized form (TagMode.TagTable)

CREATE TABLE IF NOT EXISTS "public"."tags" (
    "ordering_id" bigint NOT NULL,
    "tag" text NOT NULL,
    "sequence_nr" bigint NOT NULL,
    "persistence_id" text NOT NULL,
    CONSTRAINT "PK_tags" PRIMARY KEY ("ordering_id", "tag")
);



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

