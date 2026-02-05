-- Migration: Inventar-System
-- Erstellt: item_definitions, inventories, inventory_items
-- Kategorien: account (player), vehicle, faction, house, wardrobe

CREATE TABLE IF NOT EXISTS `item_definitions` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `key` VARCHAR(100) NOT NULL UNIQUE,
    `name` VARCHAR(150) NOT NULL,
    `description` TEXT NULL,
    `stackable` BOOLEAN NOT NULL DEFAULT TRUE,
    `max_stack` INT DEFAULT 64,
    `weight` FLOAT DEFAULT 0.0,
    `meta_schema` JSON NULL,
    `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `inventories` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `category` ENUM('player','vehicle','faction','house','wardrobe') NOT NULL,
    `owner_type` ENUM('account','vehicle','faction','house') NOT NULL,
    `owner_id` INT NOT NULL,
    `slot_count` INT NOT NULL DEFAULT 20,
    `max_weight` FLOAT DEFAULT NULL,
    `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY `unique_inventory_owner` (`category`, `owner_type`, `owner_id`),
    INDEX `idx_owner` (`owner_type`, `owner_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `inventory_items` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `inventory_id` INT NOT NULL,
    `slot_index` INT NOT NULL,
    `item_def_id` INT NOT NULL,
    `amount` INT NOT NULL DEFAULT 1,
    `meta` JSON NULL,
    `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (`inventory_id`) REFERENCES `inventories`(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`item_def_id`) REFERENCES `item_definitions`(`id`) ON DELETE RESTRICT,
    UNIQUE KEY `unique_slot` (`inventory_id`, `slot_index`),
    INDEX `idx_inventory` (`inventory_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Beispiel: Standard-Inventare anlegen (für Migration/Testing)
-- Spieler-Default: 20 Slots
INSERT INTO `inventories` (`category`, `owner_type`, `owner_id`, `slot_count`, `max_weight`)
VALUES ('player', 'account', 0, 20, 50.0)
ON DUPLICATE KEY UPDATE `slot_count` = VALUES(`slot_count`), `max_weight` = VALUES(`max_weight`);

-- Beispiel-Item-Definitionen
INSERT INTO `item_definitions` (`key`, `name`, `description`, `stackable`, `max_stack`, `weight`)
VALUES
('water_bottle', 'Wasserflasche', 'Erfrischendes Wasser', TRUE, 16, 0.5),
('bread', 'Brot', 'Einfaches Brot', TRUE, 10, 0.4),
('pistol', 'Pistole', 'Schusswaffe', FALSE, 1, 3.0),
('armor', 'Schutzweste', 'Schützt vor Schaden', FALSE, 1, 5.0),
('clothes_shirt', 'Hemd', 'Kleidung: Hemd', TRUE, 5, 0.2)
ON DUPLICATE KEY UPDATE `name`=VALUES(`name`);
