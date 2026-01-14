-- Journal Table DDL
-- Generated for MySQL (table-mapping = default)
-- This table stores all persisted events

CREATE TABLE IF NOT EXISTS `journal` (
    `ordering` bigint NOT NULL AUTO_INCREMENT,
    `deleted` tinyint(1) NOT NULL DEFAULT 0,
    `persistence_id` varchar(255) NOT NULL,
    `sequence_number` bigint NOT NULL,
    `created` bigint NOT NULL,
    `tags` text,
    `message` longblob NOT NULL,
    `identifier` int,
    `manifest` varchar(500),
    `writer_uuid` varchar(128),
    PRIMARY KEY (`ordering`),
    UNIQUE KEY `journal_uq` (`persistence_id`, `sequence_number`),
    INDEX `journal_ordering_idx` (`ordering`),
    INDEX `journal_created_idx` (`created`),
    INDEX `journal_persistence_id_idx` (`persistence_id`)
) ENGINE=InnoDB;

