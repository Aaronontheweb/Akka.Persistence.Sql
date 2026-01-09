-- Journal Metadata Table DDL
-- Generated for SqlServer (table-mapping = default)
-- This table is used for delete-compatibility-mode

IF NOT EXISTS (
    SELECT 1
    FROM sys.tables
    WHERE
        SCHEMA_NAME(schema_id) = 'dbo' AND
        name = 'journal_metadata'
)
BEGIN
    CREATE TABLE [dbo].[journal_metadata] (
    [persistence_id] nvarchar(255) NOT NULL,
    [sequence_number] bigint NOT NULL,
    CONSTRAINT [PK_journal_metadata] PRIMARY KEY ([persistence_id], [sequence_number])
    );
END


