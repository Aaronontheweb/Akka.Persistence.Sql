-- Snapshot Table DDL
-- Generated for MySQL (table-mapping = default)
-- This table stores actor state snapshots

CREATE TABLE IF NOT EXISTS `snapshot` (
    `persistence_id` varchar(255) NOT NULL,
    `sequence_number` bigint NOT NULL,
    `created` bigint NOT NULL,
    `snapshot` longblob,
    `manifest` varchar(500),
    `serializer_id` int,
    PRIMARY KEY (`persistence_id`, `sequence_number`),
    INDEX `snapshot_sequence_number_idx` (`sequence_number`),
    INDEX `snapshot_created_idx` (`created`)
) ENGINE=InnoDB;

