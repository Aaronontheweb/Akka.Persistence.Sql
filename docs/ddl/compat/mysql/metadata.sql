-- Journal Metadata Table DDL
-- Generated for MySQL (table-mapping = mysql)
-- This table is used for delete-compatibility-mode
-- Use this DDL when migrating from Akka.Persistence.MySql

CREATE TABLE IF NOT EXISTS `metadata` (
    `persistence_id` varchar(255) NOT NULL,
    `sequence_nr` bigint NOT NULL,
    PRIMARY KEY (`persistence_id`, `sequence_nr`)
) ENGINE=InnoDB;

