-- Journal Table DDL
-- Generated for SqlServer (table-mapping = default)
-- This table stores all persisted events

IF NOT EXISTS (
    SELECT 1
    FROM sys.tables
    WHERE
        SCHEMA_NAME(schema_id) = 'dbo' AND
        name = 'journal'
)
BEGIN
    CREATE TABLE [dbo].[journal] (
    [ordering] bigint NOT NULL IDENTITY,
    [created] bigint NOT NULL,
    [deleted] bit NOT NULL,
    [persistence_id] nvarchar(255) NOT NULL,
    [sequence_number] bigint NOT NULL,
    [message] varbinary(max) NOT NULL,
    [manifest] nvarchar(500),
    [serializer_id] int,
    CONSTRAINT [PK_journal] PRIMARY KEY ([ordering])
    );
END



-- Additional constraints and indexes:
;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE
        object_id = OBJECT_ID('dbo.journal') AND
        name = 'UQ_journal'
)
BEGIN TRY
    ALTER TABLE dbo.journal ADD CONSTRAINT UQ_journal UNIQUE (persistence_id, sequence_number);
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 2714 -- Error code for 'constraint already exists'
    BEGIN
        PRINT 'Constraint UQ_journal already exists, skipping.';
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
        object_id = OBJECT_ID('dbo.journal') AND
        name = 'IX_journal_sequence_number'
)
BEGIN TRY
    CREATE INDEX IX_journal_sequence_number ON dbo.journal(sequence_number);
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 1913 -- Error code for 'index already exists'
    BEGIN
        PRINT 'Index IX_journal_sequence_number already exists, skipping.';
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
        object_id = OBJECT_ID('dbo.journal') AND
        name = 'IX_journal_created'
)
BEGIN TRY
    CREATE INDEX IX_journal_created ON dbo.journal(created);
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 1913 -- Error code for 'index already exists'
    BEGIN
        PRINT 'Index IX_journal_created already exists, skipping.';
    END
    ELSE
    BEGIN
        THROW;
    END
END CATCH;

