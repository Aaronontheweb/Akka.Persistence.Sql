-- Journal Tags Table DDL
-- Generated for SqlServer.2022
-- This table stores tags in normalized form (TagMode.TagTable)

IF NOT EXISTS (
    SELECT 1
    FROM sys.tables
    WHERE
        SCHEMA_NAME(schema_id) = 'dbo' AND
        name = 'tags'
)
BEGIN
    CREATE TABLE [dbo].[tags] (
    [ordering_id] bigint NOT NULL,
    [tag] nvarchar(64) NOT NULL,
    [sequence_nr] bigint NOT NULL,
    [persistence_id] nvarchar(255) NOT NULL,
    CONSTRAINT [PK_tags] PRIMARY KEY ([ordering_id], [tag])
    );
END



-- Additional constraints and indexes:
;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE 
        object_id = OBJECT_ID('dbo.tags') AND
        name = 'IX_tags_persistence_id_sequence_nr'
)
BEGIN TRY
    CREATE INDEX IX_tags_persistence_id_sequence_nr ON dbo.tags (persistence_id, sequence_nr);
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 1913 -- Error code for 'index already exists'
    BEGIN
        PRINT 'Index IX_tags_persistence_id_sequence_nr already exists, skipping.';
    END
    ELSE
    BEGIN
        THROW;
    END
END CATCH;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE 
        object_id = OBJECT_ID('dbo.tags') AND
        name = 'IX_tags_tag'
)
BEGIN TRY
    CREATE INDEX IX_tags_tag ON dbo.tags (tag);
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 1913 -- Error code for 'index already exists'
    BEGIN
        PRINT 'Index IX_tags_tag already exists, skipping.';
    END
    ELSE
    BEGIN
        THROW;
    END
END CATCH;

