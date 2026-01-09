-- Journal Table DDL
-- Generated for MySQL (table-mapping = mysql)
-- This table stores all persisted events
-- Use this DDL when migrating from Akka.Persistence.MySql

CREATE TABLE IF NOT EXISTS `event_journal` (
    `ordering` bigint NOT NULL AUTO_INCREMENT,
    `created_at` bigint NOT NULL,
    `is_deleted` tinyint(1) NOT NULL,
    `persistence_id` varchar(255) NOT NULL,
    `sequence_nr` bigint NOT NULL,
    `payload` longblob NOT NULL,
    `manifest` varchar(500),
    `serializer_id` int,
    PRIMARY KEY (`ordering`),
    UNIQUE KEY `event_journal_uq` (`persistence_id`, `sequence_nr`),
    INDEX `event_journal_created_at_idx` (`created_at`),
    INDEX `event_journal_sequence_nr_idx` (`sequence_nr`)
) ENGINE=InnoDB;

