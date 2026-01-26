-- Journal Metadata Table DDL
-- Generated for MySQL (table-mapping = default)
-- This table is used for delete-compatibility-mode

CREATE TABLE IF NOT EXISTS `journal_metadata` (
    `persistence_id` varchar(255) NOT NULL,
    `sequence_number` bigint NOT NULL,
    PRIMARY KEY (`persistence_id`, `sequence_number`)
) ENGINE=InnoDB;

