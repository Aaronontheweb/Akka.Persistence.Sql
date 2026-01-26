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
        [deleted] bit NOT NULL DEFAULT 0,
        [persistence_id] nvarchar(255) NOT NULL,
        [sequence_number] bigint NOT NULL,
        [created] bigint NOT NULL,
        [tags] nvarchar(max),
        [message] varbinary(max) NOT NULL,
        [identifier] int,
        [manifest] nvarchar(500),
        [writer_uuid] nvarchar(128),
        CONSTRAINT [PK_journal] PRIMARY KEY ([ordering])
    );
END

-- Additional constraints and indexes:

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
        name = 'IX_journal_ordering'
)
BEGIN TRY
    CREATE INDEX IX_journal_ordering ON dbo.journal(ordering);
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 1913 -- Error code for 'index already exists'
    BEGIN
        PRINT 'Index IX_journal_ordering already exists, skipping.';
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

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE
        object_id = OBJECT_ID('dbo.journal') AND
        name = 'IX_journal_persistence_id'
)
BEGIN TRY
    CREATE INDEX IX_journal_persistence_id ON dbo.journal(persistence_id);
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 1913 -- Error code for 'index already exists'
    BEGIN
        PRINT 'Index IX_journal_persistence_id already exists, skipping.';
    END
    ELSE
    BEGIN
        THROW;
    END
END CATCH;

