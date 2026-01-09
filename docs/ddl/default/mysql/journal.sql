-- Journal Table DDL
-- Generated for MySQL (table-mapping = default)
-- This table stores all persisted events

CREATE TABLE IF NOT EXISTS `journal` (
    `ordering` bigint NOT NULL AUTO_INCREMENT,
    `created` bigint NOT NULL,
    `deleted` tinyint(1) NOT NULL,
    `persistence_id` varchar(255) NOT NULL,
    `sequence_number` bigint NOT NULL,
    `message` longblob NOT NULL,
    `manifest` varchar(500),
    `serializer_id` int,
    PRIMARY KEY (`ordering`),
    UNIQUE KEY `journal_uq` (`persistence_id`, `sequence_number`),
    INDEX `journal_created_idx` (`created`),
    INDEX `journal_sequence_number_idx` (`sequence_number`)
) ENGINE=InnoDB;

