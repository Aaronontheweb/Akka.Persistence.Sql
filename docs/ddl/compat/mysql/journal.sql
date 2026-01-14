-- Journal Table DDL
-- Generated for MySQL (table-mapping = mysql)
-- This table stores all persisted events
-- Use this DDL when migrating from Akka.Persistence.MySql

CREATE TABLE IF NOT EXISTS `event_journal` (
    `ordering` bigint NOT NULL AUTO_INCREMENT,
    `is_deleted` tinyint(1) NOT NULL DEFAULT 0,
    `persistence_id` varchar(255) NOT NULL,
    `sequence_nr` bigint NOT NULL,
    `created_at` bigint NOT NULL,
    `tags` text,
    `payload` longblob NOT NULL,
    `serializer_id` int,
    `manifest` varchar(500),
    PRIMARY KEY (`ordering`),
    UNIQUE KEY `event_journal_uq` (`persistence_id`, `sequence_nr`),
    INDEX `event_journal_ordering_idx` (`ordering`),
    INDEX `event_journal_created_at_idx` (`created_at`),
    INDEX `event_journal_persistence_id_idx` (`persistence_id`)
) ENGINE=InnoDB;

