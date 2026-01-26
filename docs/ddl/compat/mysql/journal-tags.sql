-- Journal Tags Table DDL
-- Generated for MySQL (table-mapping = mysql)
-- This table stores tags in normalized form (TagMode.TagTable)
-- Use this DDL when migrating from Akka.Persistence.MySql

CREATE TABLE IF NOT EXISTS `tags` (
    `ordering_id` bigint NOT NULL,
    `tag` varchar(64) NOT NULL,
    `sequence_nr` bigint NOT NULL,
    `persistence_id` varchar(255) NOT NULL,
    PRIMARY KEY (`ordering_id`, `tag`),
    INDEX `tags_persistence_id_sequence_nr_idx` (`persistence_id`, `sequence_nr`),
    INDEX `tags_tag_idx` (`tag`)
) ENGINE=InnoDB;

