-- Snapshot Table DDL
-- Generated for MySQL (table-mapping = mysql)
-- This table stores actor state snapshots
-- Use this DDL when migrating from Akka.Persistence.MySql

CREATE TABLE IF NOT EXISTS `snapshot_store` (
    `persistence_id` varchar(255) NOT NULL,
    `sequence_nr` bigint NOT NULL,
    `created_at` bigint NOT NULL,
    `snapshot` longblob,
    `manifest` varchar(500),
    `serializer_id` int,
    PRIMARY KEY (`persistence_id`, `sequence_nr`),
    INDEX `snapshot_store_sequence_nr_idx` (`sequence_nr`),
    INDEX `snapshot_store_created_at_idx` (`created_at`)
) ENGINE=InnoDB;

