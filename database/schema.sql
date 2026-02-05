-- RAGE:MP Roleplay Server Database Schema
-- MySQL/MariaDB
-- HINWEIS: 1 Charakter pro Account (Charakter-Daten sind in accounts integriert)

-- Datenbank erstellen
CREATE DATABASE IF NOT EXISTS ragemp_rp CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE ragemp_rp;

-- Accounts Tabelle (mit integriertem Charakter - 1 Charakter pro Account)
CREATE TABLE IF NOT EXISTS accounts (
    id INT AUTO_INCREMENT PRIMARY KEY,
    
    -- Account/Login Daten
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    social_club_name VARCHAR(50),
    hardware_id VARCHAR(100),
    
    -- Charakter Daten (1 Charakter pro Account)
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    
    -- Finanzen
    cash INT DEFAULT 500,
    bank_money INT DEFAULT 5000,
    
    -- Level & Erfahrung
    level INT DEFAULT 1,
    experience INT DEFAULT 0,
    play_time INT DEFAULT 0,
    
    -- Job & Fraktion
    job VARCHAR(50) DEFAULT 'Arbeitslos',
    faction_id INT DEFAULT NULL,
    faction_rank INT DEFAULT 0,
    
    -- Position
    last_position_x FLOAT DEFAULT -1037.7,
    last_position_y FLOAT DEFAULT -2738.5,
    last_position_z FLOAT DEFAULT 13.8,
    last_rotation FLOAT DEFAULT 0,
    dimension INT DEFAULT 0,
    
    -- Gesundheit & Status
    health INT DEFAULT 100,
    armor INT DEFAULT 0,
    is_alive BOOLEAN DEFAULT TRUE,
    is_injured BOOLEAN DEFAULT FALSE,
    
    -- Aussehen
    appearance JSON,
    
    -- Admin & Ban Status
    admin_level INT DEFAULT 0,
    is_banned BOOLEAN DEFAULT FALSE,
    ban_reason TEXT,
    ban_expiry DATETIME,
    
    -- Zeitstempel
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_username (username),
    INDEX idx_email (email),
    INDEX idx_social_club (social_club_name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Fahrzeuge Tabelle
CREATE TABLE IF NOT EXISTS vehicles (
    id INT AUTO_INCREMENT PRIMARY KEY,
    account_id INT NOT NULL,
    vehicle_hash VARCHAR(50) NOT NULL,
    number_plate VARCHAR(10) NOT NULL UNIQUE,
    color1 INT DEFAULT 0,
    color2 INT DEFAULT 0,
    position_x FLOAT,
    position_y FLOAT,
    position_z FLOAT,
    rotation FLOAT DEFAULT 0,
    fuel INT DEFAULT 100,
    engine_health INT DEFAULT 1000,
    body_health INT DEFAULT 1000,
    is_locked BOOLEAN DEFAULT TRUE,
    garage_id INT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (account_id) REFERENCES accounts(id) ON DELETE CASCADE,
    INDEX idx_account_id (account_id),
    INDEX idx_number_plate (number_plate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Inventar Tabelle
CREATE TABLE IF NOT EXISTS inventory (
    id INT AUTO_INCREMENT PRIMARY KEY,
    account_id INT NOT NULL,
    item_name VARCHAR(50) NOT NULL,
    item_count INT DEFAULT 1,
    item_data JSON,
    slot INT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (account_id) REFERENCES accounts(id) ON DELETE CASCADE,
    INDEX idx_account_id (account_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Häuser/Immobilien Tabelle
CREATE TABLE IF NOT EXISTS properties (
    id INT AUTO_INCREMENT PRIMARY KEY,
    owner_id INT,
    property_type VARCHAR(50) NOT NULL,
    name VARCHAR(100),
    position_x FLOAT NOT NULL,
    position_y FLOAT NOT NULL,
    position_z FLOAT NOT NULL,
    interior VARCHAR(50),
    price INT NOT NULL,
    is_locked BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (owner_id) REFERENCES accounts(id) ON DELETE SET NULL,
    INDEX idx_owner_id (owner_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Fraktionen Tabelle
CREATE TABLE IF NOT EXISTS factions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    faction_type VARCHAR(50) NOT NULL,
    balance INT DEFAULT 0,
    leader_id INT,
    max_members INT DEFAULT 50,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (leader_id) REFERENCES accounts(id) ON DELETE SET NULL,
    INDEX idx_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Fraktions-Mitglieder Tabelle
CREATE TABLE IF NOT EXISTS faction_members (
    id INT AUTO_INCREMENT PRIMARY KEY,
    faction_id INT NOT NULL,
    account_id INT NOT NULL,
    rank INT DEFAULT 0,
    joined_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (faction_id) REFERENCES factions(id) ON DELETE CASCADE,
    FOREIGN KEY (account_id) REFERENCES accounts(id) ON DELETE CASCADE,
    UNIQUE KEY unique_member (faction_id, account_id),
    INDEX idx_faction_id (faction_id),
    INDEX idx_account_id (account_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Logs Tabelle (für Admin-Aktionen, etc.)
CREATE TABLE IF NOT EXISTS logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    log_type VARCHAR(50) NOT NULL,
    account_id INT,
    admin_id INT,
    description TEXT,
    data JSON,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (account_id) REFERENCES accounts(id) ON DELETE SET NULL,
    FOREIGN KEY (admin_id) REFERENCES accounts(id) ON DELETE SET NULL,
    INDEX idx_log_type (log_type),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Banken-Konten Tabelle
CREATE TABLE IF NOT EXISTS bank_accounts (
    id INT AUTO_INCREMENT PRIMARY KEY,
    account_id INT NOT NULL,
    account_number VARCHAR(20) NOT NULL UNIQUE,
    balance INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (account_id) REFERENCES accounts(id) ON DELETE CASCADE,
    INDEX idx_account_id (account_id),
    INDEX idx_account_number (account_number)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Transaktions-Historie
CREATE TABLE IF NOT EXISTS transactions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    from_account_id INT,
    to_account_id INT,
    amount INT NOT NULL,
    transaction_type VARCHAR(50) NOT NULL,
    description VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (from_account_id) REFERENCES accounts(id) ON DELETE SET NULL,
    FOREIGN KEY (to_account_id) REFERENCES accounts(id) ON DELETE SET NULL,
    INDEX idx_from_account (from_account_id),
    INDEX idx_to_account (to_account_id),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Standard Admin Account (Passwort: admin123 - BITTE ÄNDERN!)
-- Hash ist bcrypt für "admin123"
INSERT INTO accounts (username, email, password_hash, first_name, last_name, cash, bank_money, level, admin_level) 
VALUES ('admin', 'admin@localhost.local', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyE6T.HmqDd6', 'Admin', 'User', 50000, 100000, 99, 5)
ON DUPLICATE KEY UPDATE id=id;

COMMIT;
