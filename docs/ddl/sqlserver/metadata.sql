-- Journal Metadata Table DDL
-- Generated for SqlServer.2022
-- This table is used for delete-compatibility-mode

IF NOT EXISTS (
    SELECT 1
    FROM sys.tables
    WHERE
        SCHEMA_NAME(schema_id) = 'dbo' AND
        name = 'Metadata'
)
BEGIN
    CREATE TABLE [dbo].[Metadata] (
    [PersistenceId] nvarchar(255) NOT NULL,
    [SequenceNr] bigint NOT NULL,
    CONSTRAINT [PK_Metadata] PRIMARY KEY ([PersistenceId], [SequenceNr])
    );
END


