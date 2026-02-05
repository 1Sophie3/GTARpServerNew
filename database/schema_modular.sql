-- ============================================
-- RolePlay Server - Modulares Datenbank Schema
-- HINWEIS: 1 Charakter pro Account (integriert)
-- ============================================

-- Tabelle für Accounts (Login-Daten + Charakter - 1 Charakter pro Account)
CREATE TABLE IF NOT EXISTS `accounts` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    
    -- Account/Login Daten
    `username` VARCHAR(50) NOT NULL UNIQUE,
    `password_hash` VARCHAR(255) NOT NULL,
    `email` VARCHAR(100) NOT NULL UNIQUE,
    `social_club_name` VARCHAR(50) NOT NULL,
    `hardware_id` VARCHAR(255) NOT NULL,
    
    -- Charakter Daten (1 Charakter pro Account)
    `first_name` VARCHAR(50) NOT NULL,
    `last_name` VARCHAR(50) NOT NULL,
    
    -- Finanzen
    `cash` INT DEFAULT 500,
    `bank_balance` INT DEFAULT 5000,
    
    -- Level & Erfahrung
    `level` INT DEFAULT 1,
    `experience` INT DEFAULT 0,
    `playtime_minutes` INT DEFAULT 0,
    
    -- Job & Fraktion
    `job` VARCHAR(50) DEFAULT 'Arbeitslos',
    `faction_id` INT NULL,
    `faction_rank` INT DEFAULT 0,
    
    -- Position
    `last_pos_x` FLOAT DEFAULT -1037.7,
    `last_pos_y` FLOAT DEFAULT -2738.5,
    `last_pos_z` FLOAT DEFAULT 13.8,
    `last_rotation` FLOAT DEFAULT 0,
    `dimension` INT DEFAULT 0,
    
    -- Gesundheit & Status
    `health` INT DEFAULT 100,
    `armor` INT DEFAULT 0,
    `is_alive` BOOLEAN DEFAULT TRUE,
    `is_injured` BOOLEAN DEFAULT FALSE,
    
    -- Aussehen
    `appearance` TEXT NULL COMMENT 'JSON für Gesichts-Customization',
    
    -- Admin & Ban Status
    `permission_level` TINYINT DEFAULT 0 COMMENT '0=Spieler, 1=Supporter, 2=Moderator, 3=Admin, 4=HeadAdmin, 5=Projektleitung, 6=Owner',
    `is_banned` BOOLEAN DEFAULT FALSE,
    `ban_reason` TEXT NULL,
    `ban_expiry` DATETIME NULL,
    
    -- Zeitstempel
    `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `last_login` DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    UNIQUE KEY unique_fullname (`first_name`, `last_name`),
    INDEX idx_username (`username`),
    INDEX idx_hardware_id (`hardware_id`),
    INDEX idx_faction (`faction_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- VERALTET: player_permissions (jetzt in accounts.permission_level integriert)
-- CREATE TABLE IF NOT EXISTS `player_permissions` ... 

-- VERALTET: characters (jetzt in accounts integriert - 1 Charakter pro Account)
-- CREATE TABLE IF NOT EXISTS `characters` ...

-- Tabelle für Fraktionen
CREATE TABLE IF NOT EXISTS `factions` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(100) NOT NULL UNIQUE,
    `short_name` VARCHAR(20) NOT NULL,
    `type` INT NOT NULL COMMENT '1-99=Staat, 100-199=Kriminell, 200-999=Neutral',
    `bank_balance` INT DEFAULT 0,
    `primary_color` VARCHAR(7) DEFAULT '#FFFFFF',
    `secondary_color` VARCHAR(7) DEFAULT '#000000',
    `is_active` BOOLEAN DEFAULT TRUE,
    `max_members` INT DEFAULT 50,
    `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `created_by` VARCHAR(50) NOT NULL,
    INDEX idx_type (`type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabelle für Fraktions-Ränge
CREATE TABLE IF NOT EXISTS `faction_ranks` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `faction_id` INT NOT NULL,
    `level` INT NOT NULL COMMENT 'Rangstufe (0 = niedrigster)',
    `name` VARCHAR(50) NOT NULL,
    `salary` INT DEFAULT 0,
    `can_invite` BOOLEAN DEFAULT FALSE,
    `can_kick` BOOLEAN DEFAULT FALSE,
    `can_promote` BOOLEAN DEFAULT FALSE,
    `can_manage_ranks` BOOLEAN DEFAULT FALSE,
    `can_access_bank` BOOLEAN DEFAULT FALSE,
    `can_withdraw_money` BOOLEAN DEFAULT FALSE,
    `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (`faction_id`) REFERENCES `factions`(`id`) ON DELETE CASCADE,
    UNIQUE KEY unique_faction_level (`faction_id`, `level`),
    INDEX idx_faction (`faction_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabelle für Fraktions-Mitglieder (jetzt mit account_id statt character_id)
CREATE TABLE IF NOT EXISTS `faction_members` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `account_id` INT NOT NULL,
    `faction_id` INT NOT NULL,
    `rank_id` INT NOT NULL,
    `joined_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `is_on_duty` BOOLEAN DEFAULT FALSE,
    `invited_by` VARCHAR(100) NOT NULL,
    `duty_minutes` INT DEFAULT 0,
    FOREIGN KEY (`account_id`) REFERENCES `accounts`(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`faction_id`) REFERENCES `factions`(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`rank_id`) REFERENCES `faction_ranks`(`id`) ON DELETE RESTRICT,
    UNIQUE KEY unique_account (`account_id`),
    INDEX idx_faction (`faction_id`),
    INDEX idx_account (`account_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- Beispiel-Daten für Testing
-- ============================================

-- Beispiel Fraktionen
INSERT INTO `factions` (`id`, `name`, `short_name`, `type`, `primary_color`, `secondary_color`, `max_members`, `created_by`) VALUES
(1, 'Los Santos Police Department', 'LSPD', 1, '#1E40AF', '#FFFFFF', 100, 'System'),
(2, 'Los Santos Medical Department', 'LSMD', 2, '#DC2626', '#FFFFFF', 80, 'System'),
(3, 'Federal Investigation Bureau', 'FIB', 3, '#1F2937', '#FFFFFF', 60, 'System'),
(4, 'Los Santos County Sheriff', 'LSCS', 4, '#78350F', '#FFFFFF', 80, 'System'),
(5, 'Los Santos Vagos', 'Vagos', 100, '#FACC15', '#000000', 50, 'System'),
(6, 'La Cosa Nostra', 'LCN', 101, '#000000', '#FFFFFF', 40, 'System'),
(7, 'Taxi Unternehmen', 'Taxi Co.', 200, '#FACC15', '#000000', 30, 'System')
ON DUPLICATE KEY UPDATE `id`=`id`;

-- Beispiel Ränge für LSPD
INSERT INTO `faction_ranks` (`faction_id`, `level`, `name`, `salary`, `can_invite`, `can_kick`, `can_promote`, `can_manage_ranks`, `can_access_bank`, `can_withdraw_money`) VALUES
(1, 0, 'Recruit', 500, FALSE, FALSE, FALSE, FALSE, FALSE, FALSE),
(1, 1, 'Officer I', 700, FALSE, FALSE, FALSE, FALSE, FALSE, FALSE),
(1, 2, 'Officer II', 900, FALSE, FALSE, FALSE, FALSE, TRUE, FALSE),
(1, 3, 'Sergeant', 1100, TRUE, FALSE, FALSE, FALSE, TRUE, FALSE),
(1, 4, 'Lieutenant', 1300, TRUE, TRUE, TRUE, FALSE, TRUE, TRUE),
(1, 5, 'Captain', 1500, TRUE, TRUE, TRUE, FALSE, TRUE, TRUE),
(1, 6, 'Chief', 2000, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE)
ON DUPLICATE KEY UPDATE `faction_id`=`faction_id`;

-- Beispiel Ränge für LSMD
INSERT INTO `faction_ranks` (`faction_id`, `level`, `name`, `salary`, `can_invite`, `can_kick`, `can_promote`, `can_manage_ranks`, `can_access_bank`, `can_withdraw_money`) VALUES
(2, 0, 'Praktikant', 400, FALSE, FALSE, FALSE, FALSE, FALSE, FALSE),
(2, 1, 'Sanitäter', 600, FALSE, FALSE, FALSE, FALSE, FALSE, FALSE),
(2, 2, 'Rettungsassistent', 800, FALSE, FALSE, FALSE, FALSE, TRUE, FALSE),
(2, 3, 'Notarzt', 1000, TRUE, FALSE, FALSE, FALSE, TRUE, FALSE),
(2, 4, 'Oberarzt', 1200, TRUE, TRUE, TRUE, FALSE, TRUE, TRUE),
(2, 5, 'Chefarzt', 1500, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE)
ON DUPLICATE KEY UPDATE `faction_id`=`faction_id`;

-- Beispiel Ränge für Vagos
INSERT INTO `faction_ranks` (`faction_id`, `level`, `name`, `salary`, `can_invite`, `can_kick`, `can_promote`, `can_manage_ranks`, `can_access_bank`, `can_withdraw_money`) VALUES
(5, 0, 'Affiliate', 200, FALSE, FALSE, FALSE, FALSE, FALSE, FALSE),
(5, 1, 'Soldier', 400, FALSE, FALSE, FALSE, FALSE, FALSE, FALSE),
(5, 2, 'Capo', 600, TRUE, TRUE, FALSE, FALSE, TRUE, FALSE),
(5, 3, 'Underboss', 800, TRUE, TRUE, TRUE, FALSE, TRUE, TRUE),
(5, 4, 'Boss', 1000, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE)
ON DUPLICATE KEY UPDATE `faction_id`=`faction_id`;
