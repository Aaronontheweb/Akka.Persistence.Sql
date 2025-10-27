-- Snapshot Table DDL
-- Generated for SqlServer.2022
-- This table stores actor state snapshots

IF NOT EXISTS (
    SELECT 1
    FROM sys.tables
    WHERE
        SCHEMA_NAME(schema_id) = 'dbo' AND
        name = 'SnapshotStore'
)
BEGIN
    CREATE TABLE [dbo].[SnapshotStore] (
    [PersistenceId] nvarchar(255) NOT NULL,
    [SequenceNr] bigint NOT NULL,
    [Timestamp] datetime2(7) NOT NULL,
    [Snapshot] varbinary(max),
    [Manifest] nvarchar(500),
    [SerializerId] int,
    CONSTRAINT [PK_SnapshotStore] PRIMARY KEY ([PersistenceId], [SequenceNr])
    );
END



-- Additional constraints and indexes:
;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE
        object_id = OBJECT_ID('dbo.SnapshotStore') AND
        name = 'IX_SnapshotStore_SequenceNr'
)
BEGIN TRY
    CREATE INDEX IX_SnapshotStore_SequenceNr ON dbo.SnapshotStore(SequenceNr);
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 1913 -- Error code for 'index already exists'
    BEGIN
        PRINT 'Index IX_SnapshotStore_SequenceNr already exists, skipping.';
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
        object_id = OBJECT_ID('dbo.SnapshotStore') AND
        name = 'IX_SnapshotStore_Timestamp'
)
BEGIN TRY
    CREATE INDEX IX_SnapshotStore_Timestamp ON dbo.SnapshotStore(Timestamp);
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 1913 -- Error code for 'index already exists'
    BEGIN
        PRINT 'Index IX_SnapshotStore_Timestamp already exists, skipping.';
    END
    ELSE
    BEGIN
        THROW;
    END
END CATCH;

