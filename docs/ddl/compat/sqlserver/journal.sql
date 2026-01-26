-- Journal Table DDL
-- Generated for SqlServer (table-mapping = sql-server)
-- This table stores all persisted events
-- Use this DDL when migrating from Akka.Persistence.SqlServer

IF NOT EXISTS (
    SELECT 1
    FROM sys.tables
    WHERE
        SCHEMA_NAME(schema_id) = 'dbo' AND
        name = 'EventJournal'
)
BEGIN
    CREATE TABLE [dbo].[EventJournal] (
        [Ordering] bigint NOT NULL IDENTITY,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        [PersistenceId] nvarchar(255) NOT NULL,
        [SequenceNr] bigint NOT NULL,
        [Timestamp] bigint NOT NULL,
        [Tags] nvarchar(max),
        [Payload] varbinary(max) NOT NULL,
        [SerializerId] int,
        [Manifest] nvarchar(500),
        CONSTRAINT [PK_EventJournal] PRIMARY KEY ([Ordering])
    );
END

-- Additional constraints and indexes:

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE
        object_id = OBJECT_ID('dbo.EventJournal') AND
        name = 'UQ_EventJournal'
)
BEGIN TRY
    ALTER TABLE dbo.EventJournal ADD CONSTRAINT UQ_EventJournal UNIQUE (PersistenceId, SequenceNr);
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 2714 -- Error code for 'constraint already exists'
    BEGIN
        PRINT 'Constraint UQ_EventJournal already exists, skipping.';
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
        object_id = OBJECT_ID('dbo.EventJournal') AND
        name = 'IX_EventJournal_Ordering'
)
BEGIN TRY
    CREATE INDEX IX_EventJournal_Ordering ON dbo.EventJournal(Ordering);
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 1913 -- Error code for 'index already exists'
    BEGIN
        PRINT 'Index IX_EventJournal_Ordering already exists, skipping.';
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
        object_id = OBJECT_ID('dbo.EventJournal') AND
        name = 'IX_EventJournal_Timestamp'
)
BEGIN TRY
    CREATE INDEX IX_EventJournal_Timestamp ON dbo.EventJournal(Timestamp);
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 1913 -- Error code for 'index already exists'
    BEGIN
        PRINT 'Index IX_EventJournal_Timestamp already exists, skipping.';
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
        object_id = OBJECT_ID('dbo.EventJournal') AND
        name = 'IX_EventJournal_PersistenceId'
)
BEGIN TRY
    CREATE INDEX IX_EventJournal_PersistenceId ON dbo.EventJournal(PersistenceId);
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 1913 -- Error code for 'index already exists'
    BEGIN
        PRINT 'Index IX_EventJournal_PersistenceId already exists, skipping.';
    END
    ELSE
    BEGIN
        THROW;
    END
END CATCH;

