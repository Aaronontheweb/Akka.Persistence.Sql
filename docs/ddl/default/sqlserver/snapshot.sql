-- Snapshot Table DDL
-- Generated for SqlServer (table-mapping = default)
-- This table stores actor state snapshots

IF NOT EXISTS (
    SELECT 1
    FROM sys.tables
    WHERE
        SCHEMA_NAME(schema_id) = 'dbo' AND
        name = 'snapshot'
)
BEGIN
    CREATE TABLE [dbo].[snapshot] (
    [persistence_id] nvarchar(255) NOT NULL,
    [sequence_number] bigint NOT NULL,
    [created] datetime2(7) NOT NULL,
    [snapshot] varbinary(max),
    [manifest] nvarchar(500),
    [serializer_id] int,
    CONSTRAINT [PK_snapshot] PRIMARY KEY ([persistence_id], [sequence_number])
    );
END



-- Additional constraints and indexes:
;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE
        object_id = OBJECT_ID('dbo.snapshot') AND
        name = 'IX_snapshot_sequence_number'
)
BEGIN TRY
    CREATE INDEX IX_snapshot_sequence_number ON dbo.snapshot(sequence_number);
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 1913 -- Error code for 'index already exists'
    BEGIN
        PRINT 'Index IX_snapshot_sequence_number already exists, skipping.';
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
        object_id = OBJECT_ID('dbo.snapshot') AND
        name = 'IX_snapshot_created'
)
BEGIN TRY
    CREATE INDEX IX_snapshot_created ON dbo.snapshot(created);
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() = 1913 -- Error code for 'index already exists'
    BEGIN
        PRINT 'Index IX_snapshot_created already exists, skipping.';
    END
    ELSE
    BEGIN
        THROW;
    END
END CATCH;

