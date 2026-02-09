-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Erstellungszeit: 05. Jul 2025 um 13:21
-- Server-Version: 10.4.32-MariaDB
-- PHP-Version: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Datenbank: `data`
--

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `accounts`
--

CREATE TABLE `accounts` (
  `id` int(11) NOT NULL,
  `password` varchar(100) NOT NULL,
  `name` varchar(25) NOT NULL,
  `adminlevel` int(2) NOT NULL DEFAULT 0,
  `geld` int(11) NOT NULL,
  `clothingStyle` enum('elegant','normal','gangster') NOT NULL DEFAULT 'normal',
  `hardware_id` varchar(255) NOT NULL,
  `is_banned` tinyint(1) DEFAULT 0,
  `ban_reason` varchar(255) DEFAULT NULL,
  `characterdata` longtext NOT NULL,
  `licenses` text DEFAULT NULL COMMENT 'JSON array of acquired licenses',
  `fraktion` int(11) DEFAULT 0,
  `duty_start` datetime DEFAULT NULL,
  `duty_offset` int(11) DEFAULT 0,
  `last_logout_duty` datetime DEFAULT NULL COMMENT 'Zeitpunkt des letzten Logouts während des Dienstes',
  `socialClubId` varchar(255) DEFAULT NULL,
  `creation_date` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Daten für Tabelle `accounts`
--

INSERT INTO `accounts` (`id`, `password`, `name`, `adminlevel`, `geld`, `clothingStyle`, `hardware_id`, `is_banned`, `ban_reason`, `characterdata`, `licenses`, `fraktion`, `duty_start`, `duty_offset`, `last_logout_duty`, `socialClubId`, `creation_date`) VALUES
(4, '$2a$11$Lxqu/dh3mPp2rllGzAOYYOuwQzhyDZYSPogNY/NNaxcBgr2MAbRE6', 'Michael Babinski', 5, 12142, 'normal', 'D8903A045B585100FBEC42648F543BB075F41E8444A481C889F018C8DD229CE00026BAD81FA2EA48E3120E38E958EA50A31208A056B6E9E06F0051886E4E82C0', 0, '', '{\"gender\":\"Männlich\",\"firstname\":\"Michael\",\"lastname\":\"Babinski\",\"birth\":\"23.11.1998\",\"size\":\"1.80\",\"origin\":\"Los-Santos\",\"hair\":[\"61\",61,51],\"beard\":[\"20\",0],\"blendData\":[0,0,0,0,0,0],\"faceFeatures\":[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],\"clothingChoice\":\"Casual\",\"headOverlays\":[255,255,255,255,255,255,255,255,255,255,255,255],\"headOverlaysColors\":[0,0,0,0,0,0,0,0,0,0,0,0],\"eyeColor\":[0]}', '{\"Car\":\"2025-06-26T16:34:59.0871593+02:00\",\"Truck\":\"2025-06-26T16:46:23.8574275+02:00\"}', 1, NULL, 0, NULL, '93207126', '2025-06-25 15:31:16'),
(11, 'Dummy', 'Dummy', 0, 1, 'normal', '', 1, NULL, '', NULL, 0, NULL, 0, NULL, NULL, '2025-06-25 15:31:16'),
(12, '$2a$11$21mP8sFcH9LmozhDp6gT/e/Y93Xke/RHK0KqgGtp2Gf67zS5z//.O', 'WeirdNewbie', 5, 1500, 'normal', 'D8903A045BACF9B0CAF27CF81AEA2B10B9F48620058EC610BBF018C8DD224EE0D6344F50D24C55C8D9BA0ED808CEBBE0367608A056B6E9F0C7FC9EAC246E5300', 0, NULL, '{\"gender\":\"Weiblich\",\"firstname\":\"Sophie\",\"lastname\":\"Babinski\",\"birth\":\"01.01.2000\",\"size\":\"170\",\"origin\":\"Los-Santos\",\"hair\":[\"48\",14,0],\"beard\":[-1,0],\"blendData\":[0,0,0,0,0,0],\"faceFeatures\":[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],\"clothingChoice\":\"Casual\",\"headOverlays\":[-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],\"headOverlaysColors\":[0,0,0,0,0,0,0,0,0,0,0,0],\"eyeColor\":[0]}', '[]', 2, NULL, 0, NULL, '156514680', '2025-06-25 15:31:16');

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `bankautomaten`
--

CREATE TABLE `bankautomaten` (
  `id` int(11) NOT NULL,
  `name` varchar(50) NOT NULL,
  `posX` float NOT NULL,
  `posY` float NOT NULL,
  `posZ` float NOT NULL,
  `dimension` int(11) DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Daten für Tabelle `bankautomaten`
--

INSERT INTO `bankautomaten` (`id`, `name`, `posX`, `posY`, `posZ`, `dimension`) VALUES
(1, 'ATM Pillbox', 315.427, -593.737, 43.283, 0),
(2, 'ATM ROT Würfelpark', 119.165, -883.83, 31.123, 0),
(3, 'FLEECA Würfelpark', 149.841, -1040.7, 29.374, 0),
(4, 'ATM FLEECA Würfelpark', 147.745, -1035.59, 29.343, 0),
(5, 'ATM FLEECA Würfelpark', 146.01, -1034.83, 29.344, 0),
(6, 'ATM BLAU würfelpark', 296.4, -894.116, 29.229, 0),
(7, 'ATM BLAU würfelpark', 295.666, -896.167, 23.215, 0),
(8, 'ATM BLAU LOMBANK', 5.212, -919.741, 29.56, 0),
(9, 'ATM ROT GRUPPE 6', -203.913, -861.419, 30.267, 0),
(10, 'ATM ROT Quik House', -303.135, -829.778, 32.417, 0),
(11, 'ATM ROT Quik House', -301.655, -830.16, 32.417, 0),
(12, 'ATM ROT Look - See', -537.804, -845.34, 29.29, 0),
(13, 'ATM BLAU China copy', -713.011, -819.564, 23.628, 0),
(14, 'ATM BLAU China copy', -709.828, -818.982, 23.729, 0),
(15, 'ATM Little Soul (vagos tanke)', -717.614, -915.662, 19.215, 0),
(16, 'ATM BLAU Vespucci Binco', -821.75, -1082, 11.132, 0),
(17, 'ATM Bike Rental Vespucci', -1109.83, -1690.74, 4.437, 0),
(18, 'ATM ROT MAZE BANK Del perro', -1315.73, -834.741, 16.961, 0),
(19, 'ATM ROT MAZE BANK Del perro', -1314.67, -836.187, 16.959, 0),
(20, 'ATM ROT Pillbox Hill', 111.433, -775.351, 31.437, 0),
(21, 'ATM ROT Pillbox Hill', 114.423, -776.538, 31.417, 0),
(22, 'ATM ROT Pillbox Hill', 112.458, -819.251, 31.339, 0),
(23, 'FLEECA ALTA', 314.109, -279.051, 54.17, 0),
(24, 'ATM STAATSBANK', 236.546, 219.571, 106.286, 0),
(25, 'ATM STAATSBANK', 218.788, 218.788, 106.286, 0),
(26, 'ATM STAATSBANK', 237.247, 217.62, 106.286, 0),
(27, 'ATM STAATSBANK', 237.704, 216.623, 106.286, 0),
(28, 'ATM STAATSBANK', 238.07, 215.619, 106.286, 0),
(29, 'ATM Vinewood Mid', 158.666, 234.009, 106.624, 0);

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `bankkonten`
--

CREATE TABLE `bankkonten` (
  `id` int(11) NOT NULL,
  `player_id` int(11) DEFAULT NULL,
  `kontonummer` char(9) NOT NULL,
  `kontostand` decimal(10,2) DEFAULT 0.00,
  `creation_date` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Daten für Tabelle `bankkonten`
--

INSERT INTO `bankkonten` (`id`, `player_id`, `kontonummer`, `kontostand`, `creation_date`) VALUES
(1, 4, '204323296', 903700.00, '2025-06-05 17:54:55'),
(2, 12, '734126779', 18694.00, '2025-06-08 18:05:04');

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `bank_transactions`
--

CREATE TABLE `bank_transactions` (
  `id` int(11) NOT NULL,
  `player_id` int(11) NOT NULL,
  `kontonummer` char(9) NOT NULL,
  `type` varchar(20) NOT NULL,
  `amount` decimal(10,2) NOT NULL,
  `target_kontonummer` char(9) DEFAULT NULL,
  `description` varchar(100) DEFAULT NULL,
  `transaction_date` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Daten für Tabelle `bank_transactions`
--

INSERT INTO `bank_transactions` (`id`, `player_id`, `kontonummer`, `type`, `amount`, `target_kontonummer`, `description`, `transaction_date`) VALUES
(1, 12, '734126779', 'deposit', 60000.00, NULL, 'Finanzamt-Gutschrift', '2025-06-23 14:33:17'),
(2, 12, '734126779', 'deposit', 30000.00, NULL, 'Finanzamt-Gutschrift', '2025-06-23 14:33:39');

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `dealerships`
--

CREATE TABLE `dealerships` (
  `id` int(11) NOT NULL,
  `name` varchar(100) NOT NULL,
  `category` varchar(50) NOT NULL COMMENT 'z.B. Sportwagen, SUV, etc.',
  `vehicleModel` varchar(50) NOT NULL,
  `posX` float NOT NULL,
  `posY` float NOT NULL,
  `posZ` float NOT NULL,
  `rotZ` float NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='Definiert die physischen Ausstellungsorte der Fahrzeuge.';

--
-- Daten für Tabelle `dealerships`
--

INSERT INTO `dealerships` (`id`, `name`, `category`, `vehicleModel`, `posX`, `posY`, `posZ`, `rotZ`) VALUES
(1, 'Sport-Händler', 'Sport', '', -47.761, -1092.83, 25.8155, 162.639),
(2, 'Sport-Händler', 'Sport', '', -44.7765, -1094.6, 26.0047, 167.655),
(3, 'Sport-Händler', 'Sport', '', -47.8304, -1102.15, 26.042, 70.3654),
(4, 'Sport-Händler', 'Sport', '', -41.6752, -1095.56, 25.9864, 159.78),
(5, 'Classic-Händler', 'Classic', '', -53.0324, -1694.12, 29.154, 6.03927),
(6, 'Classic-Händler', 'Classic', '', -49.5222, -1692.69, 28.7972, 9.30408),
(7, 'Classic-Händler', 'Classic', '', -46.4288, -1690.99, 28.8722, 3.15744),
(8, 'Classic-Händler', 'Classic', '', -43.1649, -1689.75, 29.0425, 8.90382),
(9, 'Classic-Händler', 'Classic', '', -59.3479, -1686.03, 28.6781, -105.11),
(10, 'Classic-Händler', 'Classic', '', -56.6328, -1683.15, 28.8536, -100.982),
(11, 'Classic-Händler', 'Classic', '', -54.2312, -1680.06, 29.0229, -99.184),
(17, 'boat-Händler', 'boat', '', -745.937, -1348.55, 0.0837145, 49.8333),
(18, 'boat-Händler', 'boat', '', -751.401, -1356.32, 0.442063, 53.2242),
(19, 'heli-Händler', 'heli', '', -721.599, -1473.14, 4.89973, 49.9225),
(20, 'heli-Händler', 'heli', '', -700.676, -1446.83, 4.90293, 58.1856),
(21, 'Sveh-Händler', 'Sveh', '', -31.2778, -1678.31, 29.2842, 126.432),
(22, 'Sveh-Händler', 'Sveh', '', -28.651, -1681.91, 29.2416, 125.869),
(23, 'Sveh-Händler', 'Sveh', '', -34.6282, -1733.63, 29.1214, -157.368),
(24, 'Sveh-Händler', 'Sveh', '', -31.8112, -1732.31, 29.1062, -157.493),
(25, 'Sveh-Händler', 'Sveh', '', -25.9536, -1730.13, 29.5041, -157.544),
(26, 'Sveh-Händler', 'Sveh', '', -23.1536, -1728.7, 29.5052, -156.506),
(27, 'Bikes-Händler', 'Bikes', '', -4.70678, -1721.11, 28.6465, -156.483),
(28, 'Bikes-Händler', 'Bikes', '', -6.40895, -1721.87, 28.6509, -155.437),
(29, 'Bikes-Händler', 'Bikes', '', -3.53955, -1720.37, 28.8094, -151.675),
(30, 'Bikes-Händler', 'Bikes', '', -2.19555, -1719.84, 28.8152, -152.757),
(33, 'Bikes-Händler', 'Bikes', '', 3.83824, -1718.16, 28.7751, -155.152),
(34, 'Bikes-Händler', 'Bikes', '', 2.12387, -1718.39, 28.7666, -155.476),
(35, 'Bikes-Händler', 'Bikes', '', 5.1311, -1717.47, 28.7692, -152.338),
(36, 'Bikes-Händler', 'Bikes', '', 6.79279, -1717.09, 28.7621, -155.789),
(37, 'SuperSport-Händler', 'SuperSport', '', -46.0804, -1108.72, 25.7761, 69.9211),
(38, 'SuperSport-Händler', 'SuperSport', '', -52.0437, -1106.87, 25.9424, 73.1802),
(39, 'Sport-Händler', 'Sport', '', -57.8798, -1104.44, 26.0009, 70.3803),
(40, 'Classic-Händler', 'lowb', '', 14.275, -1713.07, 28.9, -147.055),
(41, 'Classic-Händler', 'lowb', '', 17.159, -1712.01, 28.9, -147.055),
(42, 'Classic-Händler', 'lowb', '', 19.911, -1710.4, 29.9, -147.055),
(43, 'Classic-Händler', 'lowb', '', 22.654, -1709.26, 29.9, -147.055);

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `fishing_loottable`
--

CREATE TABLE `fishing_loottable` (
  `id` int(11) NOT NULL,
  `itemName` varchar(255) NOT NULL,
  `itemDbName` varchar(255) NOT NULL COMMENT 'Der interne Name des Items, z.B. für dein Inventarsystem',
  `chance` float NOT NULL COMMENT 'Wahrscheinlichkeit in Prozent (z.B. 40.5 für 40.5%)'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Daten für Tabelle `fishing_loottable`
--

INSERT INTO `fishing_loottable` (`id`, `itemName`, `itemDbName`, `chance`) VALUES
(1, 'Alter Schuh', 'junk_shoe', 40),
(2, 'Kleine Forelle', 'fish_trout_small', 30),
(3, 'Rostiger Anker', 'junk_anchor', 15),
(4, 'Großer Lachs', 'fish_salmon_large', 10),
(5, 'Schatzkiste', 'treasure_chest', 5);

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `fraktion`
--

CREATE TABLE `fraktion` (
  `id` int(11) NOT NULL,
  `name` varchar(50) NOT NULL,
  `type` enum('good','bad') NOT NULL,
  `bankkonto` bigint(20) DEFAULT 0,
  `leader_id` int(11) NOT NULL,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Daten für Tabelle `fraktion`
--

INSERT INTO `fraktion` (`id`, `name`, `type`, `bankkonto`, `leader_id`, `created_at`) VALUES
(1, 'LSPD', 'good', 0, 4, '2025-05-16 15:32:15'),
(2, 'LSMD', 'good', 0, 12, '2025-05-19 15:42:55'),
(3, 'LSCS', 'good', 0, 66, '2025-05-19 15:45:00'),
(4, 'DrivingSchool', 'good', 0, 11, '2025-06-26 13:16:00');

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `fraktionsmitglieder`
--

CREATE TABLE `fraktionsmitglieder` (
  `id` int(11) NOT NULL,
  `account_id` int(11) NOT NULL,
  `fraktion_id` int(11) NOT NULL,
  `rank` int(11) NOT NULL DEFAULT 0,
  `gehalt` int(11) NOT NULL DEFAULT 0,
  `joined_at` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Daten für Tabelle `fraktionsmitglieder`
--

INSERT INTO `fraktionsmitglieder` (`id`, `account_id`, `fraktion_id`, `rank`, `gehalt`, `joined_at`) VALUES
(1, 4, 1, 12, 8000, '2025-05-16 15:32:15'),
(2, 12, 2, 12, 8000, '2025-05-19 15:42:55'),
(11, 11, 4, 12, 1, '2025-06-26 13:17:30');

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `fraktion_bankkonten`
--

CREATE TABLE `fraktion_bankkonten` (
  `id` int(11) NOT NULL,
  `fraktion_id` int(11) NOT NULL,
  `kontonummer` char(9) NOT NULL,
  `kontostand` decimal(15,2) NOT NULL DEFAULT 25000.00,
  `creation_date` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `fraktion_bank_transactions`
--

CREATE TABLE `fraktion_bank_transactions` (
  `id` int(11) NOT NULL,
  `fraktion_id` int(11) NOT NULL,
  `kontonummer` char(9) NOT NULL,
  `type` varchar(20) NOT NULL,
  `amount` decimal(10,2) NOT NULL,
  `player_id` int(11) DEFAULT NULL,
  `player_name` varchar(50) DEFAULT NULL,
  `description` varchar(100) DEFAULT NULL,
  `transaction_date` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `garages`
--

CREATE TABLE `garages` (
  `id` int(11) NOT NULL,
  `name` varchar(255) NOT NULL,
  `position_x` float NOT NULL,
  `position_y` float NOT NULL,
  `position_z` float NOT NULL,
  `npc_heading` float NOT NULL DEFAULT 0,
  `max_vehicles` int(11) NOT NULL DEFAULT 10,
  `blip_id` int(11) NOT NULL DEFAULT 402,
  `blip_color` int(11) NOT NULL DEFAULT 4,
  `fraktionId` int(11) DEFAULT 0,
  `ped_model` varchar(50) NOT NULL DEFAULT 'a_m_m_business_01'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Daten für Tabelle `garages`
--

INSERT INTO `garages` (`id`, `name`, `position_x`, `position_y`, `position_z`, `npc_heading`, `max_vehicles`, `blip_id`, `blip_color`, `fraktionId`, `ped_model`) VALUES
(1, 'Alta Garage', 276.28, -345.049, 45.173, -114.35, 60, 357, 4, 0, 'a_m_m_business_01'),
(2, 'Würfelpark Garage', 213.949, -809.331, 31.014, -116.012, 60, 402, 4, 0, 'a_m_m_business_01'),
(3, 'LSGC Garage', 100.397, -1073.34, 29.374, 159.133, 60, 402, 4, 0, 'a_m_m_business_01'),
(4, '1. Boot Garage', -720.234, -1325.1, 1.595, -46.254, 4, 402, 4, 0, 'a_m_m_business_01'),
(5, '2. Boot Garage', -734.93, -1337.51, 1.595, -117.885, 4, 402, 4, 0, 'a_m_m_business_01'),
(6, 'Hubschrauber Garage', -735.856, -1455.79, 5, 61.318, 10, 402, 4, 0, 'a_m_m_business_01'),
(7, 'Vespucci Garage', -1184.45, -1509.33, 4.649, 38.188, 10, 402, 4, 0, 'a_m_m_business_01'),
(8, 'Del Perro Garage', -1579.72, -909.08, 9.57, 48.776, 120, 402, 4, 0, 'a_m_m_business_01'),
(9, 'Richman Garage', -1684.76, 58.059, 64.035, 136.685, 60, 402, 4, 0, 'a_m_m_business_01'),
(10, 'VineWood Zug Garage', -1684.76, 58.059, 64.035, 136.685, 60, 402, 4, 0, 'a_m_m_business_01'),
(11, 'VineWood Mitte  Garage', 362.509, 298.381, 103.883, -98.783, 30, 402, 4, 0, 'a_m_m_business_01'),
(12, 'VineWood Mitte 2 Garage', 362.509, 298.381, 103.883, -98.783, 30, 402, 4, 0, 'a_m_m_business_01'),
(13, 'Diamond Casino Garage', 940.641, -38.209, 78.764, 99.781, 120, 402, 4, 0, 'a_m_m_business_01'),
(14, 'Mirror Park Garage', 1036.15, -762.806, 57.992, 66.222, 30, 402, 4, 0, 'a_m_m_business_01'),
(15, 'Mission Row Garage', 472.555, -1111.96, 29.199, 13.877, 10, 402, 4, 0, 'a_m_m_business_01'),
(16, 'Rancho Garage', 356.24, -1701.84, 32.53, -95.689, 20, 402, 4, 0, 'a_m_m_business_01'),
(17, 'Rancho 2', 344.578, -1688.07, 27.298, -130.914, 10, 402, 4, 0, 'a_m_m_business_01'),
(18, 'Arena Garage', -168.823, -1987.43, 27.75, 91.329, 120, 402, 4, 0, 'a_m_m_business_01'),
(19, 'Rockford Hills Garage', -945.791, -182.227, 41.876, -53.307, 20, 402, 4, 0, 'a_m_m_business_01'),
(20, 'Grand Senora Hubschrauber Garage', 1764.54, 3230.19, 42.362, -28.773, 10, 402, 4, 0, 'a_m_m_business_01'),
(21, 'Grand Senora Flugzeug Garage', 1408.37, 2993.26, 40.553, 31.599, 10, 402, 4, 0, 'a_m_m_business_01'),
(22, 'Sandy Shores Garage', 1542.34, 3785.32, 34.213, -153.185, 20, 402, 4, 0, 'a_m_m_business_01'),
(23, 'Paleto Garage', -274.9, 6073.15, 31.444, -153.792, 20, 402, 4, 0, 'a_m_m_business_01'),
(24, 'Paleto Fallschirm Garage', -748.718, 5551.23, 33.605, 105.206, 20, 402, 4, 0, 'a_m_m_business_01'),
(666, 'LSPD Garage', 454.915, -1011.67, 28.458, 117.938, 10, 0, 0, 1, 'a_m_m_business_01'),
(667, 'LSMD Garage', 340.762, -563.548, 28.742, -39.419, 10, 0, 0, 2, 'a_m_m_business_01'),
(668, 'LSCS Garage', -176.855, -1282.93, 31.295, 90.149, 10, 0, 0, 3, 'a_m_m_business_01'),
(669, 'Heli Garage LSCS', -766.229, -1451.32, 5, -113.8, 10, 0, 0, 3, 'a_m_m_business_01');

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `garage_spawnpoints`
--

CREATE TABLE `garage_spawnpoints` (
  `id` int(11) NOT NULL,
  `GarageId` int(11) NOT NULL,
  `SpawnPositionX` float NOT NULL,
  `SpawnPositionY` float NOT NULL,
  `SpawnPositionZ` float NOT NULL,
  `SpawnHeading` float NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Daten für Tabelle `garage_spawnpoints`
--

INSERT INTO `garage_spawnpoints` (`id`, `GarageId`, `SpawnPositionX`, `SpawnPositionY`, `SpawnPositionZ`, `SpawnHeading`) VALUES
(1, 1, 286, -342, 44.9, -110);

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `houses`
--

CREATE TABLE `houses` (
  `id` int(11) NOT NULL,
  `name` varchar(255) DEFAULT 'Apartment',
  `ipl` varchar(255) NOT NULL,
  `posX` float NOT NULL,
  `posY` float NOT NULL,
  `posZ` float NOT NULL,
  `interiorPosX` float NOT NULL,
  `interiorPosY` float NOT NULL,
  `interiorPosZ` float NOT NULL,
  `interiorRotZ` float NOT NULL DEFAULT 0,
  `preis` int(11) NOT NULL DEFAULT 0,
  `besitzer_acc_id` int(11) DEFAULT NULL,
  `status` tinyint(1) NOT NULL DEFAULT 0 COMMENT '0: Zum Verkauf, 1: In Besitz',
  `abgeschlossen` tinyint(1) NOT NULL DEFAULT 1,
  `dimension` int(11) NOT NULL DEFAULT 0,
  `is_rentable` tinyint(1) NOT NULL DEFAULT 0 COMMENT '0=Nein, 1=Ja',
  `mietpreis` int(11) NOT NULL DEFAULT 0,
  `mieter_acc_id` int(11) DEFAULT NULL,
  `miete_bezahlt_bis` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Daten für Tabelle `houses`
--

INSERT INTO `houses` (`id`, `name`, `ipl`, `posX`, `posY`, `posZ`, `interiorPosX`, `interiorPosY`, `interiorPosZ`, `interiorRotZ`, `preis`, `besitzer_acc_id`, `status`, `abgeschlossen`, `dimension`, `is_rentable`, `mietpreis`, `mieter_acc_id`, `miete_bezahlt_bis`) VALUES
(1000, '', 'apa_v_mp_h_08_b', -2587.78, 1911.23, 167.498, -773.811, 342.151, 196.686, 83.6523, 1200000, 4, 1, 0, 1000, 0, 0, NULL, NULL),
(1001, 'VineWood 1', 'v_motel_mp', 519.705, 228.007, 104.744, 151.587, -1007.01, -99, -24.3388, 0, NULL, 0, 1, 1001, 1, 200, NULL, NULL),
(1002, 'VineWood 2', 'v_motel_mp', 527.831, 224.904, 104.745, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1002, 1, 200, NULL, NULL),
(1003, 'VineWood 3', 'v_motel_mp', 527.442, 214.569, 104.745, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1003, 1, 200, NULL, NULL),
(1004, 'VineWood 4', 'v_motel_mp', 525.051, 208.067, 104.744, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1004, 1, 200, NULL, NULL),
(1005, 'VineWood 5', 'v_motel_mp', 521.697, 198.659, 104.744, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1005, 1, 200, NULL, NULL),
(1006, 'VineWood 6', 'v_motel_mp', 519.864, 193.371, 104.745, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1006, 1, 200, NULL, NULL),
(1007, 'VineWood 7', 'v_motel_mp', 514.18, 192.643, 104.745, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1007, 1, 200, NULL, NULL),
(1008, 'VineWood 8', 'v_motel_mp', 507.87, 194.704, 104.745, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1008, 1, 200, NULL, NULL),
(1009, 'VineWood 9', 'v_motel_mp', 487.028, 202.806, 104.745, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1009, 1, 200, NULL, NULL),
(1010, 'VineWood 10', 'v_motel_mp', 482.941, 204.962, 104.745, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1010, 1, 200, NULL, NULL),
(1011, 'VineWood 11', 'v_motel_mp', 485.471, 212.071, 104.744, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1011, 1, 200, NULL, NULL),
(1012, 'VineWood 12', 'v_motel_mp', 488.937, 221.301, 104.744, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1012, 1, 200, NULL, NULL),
(1013, 'VineWood 13', 'v_motel_mp', 491.151, 227.474, 104.744, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1013, 1, 200, NULL, NULL),
(1014, 'VineWood 14', 'v_motel_mp', 497.265, 236.629, 104.745, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1014, 1, 200, NULL, NULL),
(1015, 'VineWood 15', 'v_motel_mp', 503.981, 233.736, 104.744, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1015, 1, 200, NULL, NULL),
(1016, 'VineWood 16', 'v_motel_mp', 510.195, 231.011, 104.744, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1016, 1, 200, NULL, NULL),
(1017, 'VineWood 17', 'v_motel_mp', 486.7, 201.683, 108.31, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1017, 1, 200, NULL, NULL),
(1018, 'VineWood 18', 'v_motel_mp', 483.028, 206.783, 108.31, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1018, 1, 200, NULL, NULL),
(1019, 'VineWood 19', 'v_motel_mp', 485.093, 212.378, 108.31, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1019, 1, 200, NULL, NULL),
(1020, 'VineWood 20', 'v_motel_mp', 508.081, 194.393, 108.31, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1020, 1, 200, NULL, NULL),
(1021, 'VineWood 21', 'v_motel_mp', 514.279, 192.204, 108.31, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1021, 1, 200, NULL, NULL),
(1022, 'VineWood 22', 'v_motel_mp', 520.248, 193.09, 108.309, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1022, 1, 200, NULL, NULL),
(1024, 'VineWood 23', 'v_motel_mp', 522.598, 200.019, 108.309, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1024, 1, 200, NULL, NULL),
(1025, 'MagellanAve 1', 'v_motel_mp', -1038.47, -1609.69, 5.00338, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1025, 1, 200, NULL, NULL),
(1026, 'MagellanAve 2', 'v_motel_mp', -1029.65, -1603.84, 4.95495, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1026, 1, 200, NULL, NULL),
(1027, 'MagellanAve 3', 'v_motel_mp', -1032.61, -1582.5, 5.12841, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1027, 1, 200, NULL, NULL),
(1028, 'MagellanAve 4', 'v_motel_mp', -1043.95, -1579.72, 5.03858, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1028, 1, 200, NULL, NULL),
(1029, 'MagellanAve 5', 'v_motel_mp', -1048.88, -1580.81, 4.92065, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1029, 1, 200, NULL, NULL),
(1030, 'MagellanAve 6', 'v_motel_mp', -1056.58, -1588.11, 4.61302, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1030, 1, 200, NULL, NULL),
(1031, 'MagellanAve 7', 'v_motel_mp', -1057.93, -1540.39, 5.04833, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1031, 1, 200, NULL, NULL),
(1032, 'MagellanAve 8', 'v_motel_mp', -1066.43, -1545.05, 4.90243, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1032, 1, 200, NULL, NULL),
(1033, 'MagellanAve 9', 'v_motel_mp', -1077.57, -1553.1, 4.6289, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1033, 1, 200, NULL, NULL),
(1034, 'MagellanAve 10', 'v_motel_mp', -1085.08, -1558.59, 4.49418, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1034, 1, 200, NULL, NULL),
(1035, 'MirrorPark 1', 'apa_v_mp_h_01_b', 1060.95, -378.444, 68.2312, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1035, 0, 0, NULL, NULL),
(1036, 'MirrorPark 2', 'apa_v_mp_h_01_b', 1029.58, -409.378, 65.9493, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1036, 0, 0, NULL, NULL),
(1037, 'MirrorPark 3', 'apa_v_mp_h_01_b', 1011.77, -422.806, 64.9527, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1037, 0, 0, NULL, NULL),
(1038, 'MirrorPark 4', 'apa_v_mp_h_01_b', 987.887, -433.374, 63.8907, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1038, 0, 0, NULL, NULL),
(1039, 'MirrorPark 5', 'apa_v_mp_h_01_b', 967.843, -452.347, 62.4028, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1039, 0, 0, NULL, NULL),
(1040, 'VinewoodWest 1', 'v_motel_mp', -456.633, 92.8968, 63.5468, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1040, 1, 200, NULL, NULL),
(1041, 'MirrorPark 6', 'apa_v_mp_h_01_b', 943.813, -463.718, 61.3957, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1041, 0, 0, NULL, NULL),
(1042, 'MirrorPark 7', 'apa_v_mp_h_01_b', 921.877, -478.596, 61.0836, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1042, 0, 0, NULL, NULL),
(1043, 'MirrorPark 8', 'apa_v_mp_h_01_b', 906.728, -490.119, 59.4371, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1043, 0, 0, NULL, NULL),
(1044, 'VinewoodWest 2', 'v_motel_mp', -452.561, 78.6579, 67.3424, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1044, 1, 200, NULL, NULL),
(1045, 'VinewoodWest 3', 'v_motel_mp', -452.66, 78.7603, 71.3438, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1045, 1, 200, NULL, NULL),
(1046, 'VinewoodWest 4', 'v_motel_mp', -431.735, 83.9009, 68.5114, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1046, 1, 200, NULL, NULL),
(1047, 'MirrorPark 9', 'apa_v_mp_h_01_b', 878.896, -498.314, 58.0824, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1047, 0, 0, NULL, NULL),
(1048, 'VinewoodWest 5', 'v_motel_mp', -432.134, 83.6111, 72.5154, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1048, 1, 200, NULL, NULL),
(1049, 'MirrorPark 10', 'apa_v_mp_h_01_b', 862.886, -510.164, 57.329, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1049, 0, 0, NULL, NULL),
(1050, 'MirrorPark 11', 'apa_v_mp_h_01_b', 850.692, -532.433, 57.9253, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1050, 0, 0, NULL, NULL),
(1051, 'MirrorPark 12', 'apa_v_mp_h_01_b', 844.39, -563.571, 57.8339, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1051, 0, 0, NULL, NULL),
(1052, 'MirrorPark 13', 'apa_v_mp_h_01_b', 861.919, -582.845, 58.1565, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1052, 0, 0, NULL, NULL),
(1053, 'VinewoodWest 6', 'v_motel_mp', -422.147, 71.8661, 64.2632, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1053, 1, 200, NULL, NULL),
(1054, 'MirrorPark 14', 'apa_v_mp_h_01_b', 887.413, -607.561, 58.2178, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1054, 0, 0, NULL, NULL),
(1055, 'MirrorPark 15', 'apa_v_mp_h_01_b', 903.527, -615.588, 58.4534, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1055, 0, 0, NULL, NULL),
(1056, 'MirrorPark 16', 'apa_v_mp_h_01_b', 929.396, -639.189, 58.2423, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1056, 0, 0, NULL, NULL),
(1057, 'MirrorPark 17', 'apa_v_mp_h_01_b', 943.785, -653.643, 58.4292, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1057, 0, 0, NULL, NULL),
(1058, 'MirrorPark 18', 'apa_v_mp_h_01_b', 971.148, -700.989, 58.482, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1058, 0, 0, NULL, NULL),
(1059, 'MirrorPark 19', 'apa_v_mp_h_01_b', 979.812, -715.766, 58.0237, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1059, 0, 0, NULL, NULL),
(1060, 'MirrorPark 20', 'apa_v_mp_h_01_b', 997.531, -728.905, 57.8157, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1060, 0, 0, NULL, NULL),
(1061, 'MirrorPark 21', 'apa_v_mp_h_01_b', 980.117, -627.324, 59.2359, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1061, 0, 0, NULL, NULL),
(1062, 'MirrorPark 22', 'apa_v_mp_h_01_b', 963.241, -596.13, 59.9027, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1062, 0, 0, NULL, NULL),
(1063, 'MirrorPark 23', 'apa_v_mp_h_01_b', 976.086, -579.522, 59.6356, -774.013, 342.043, 196.686, 0, 170000, NULL, 0, 1, 1063, 0, 0, NULL, NULL),
(1064, 'MirrorPark 24', 'apa_v_mp_h_01_b', 1000.67, -594.065, 59.2321, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1064, 0, 0, NULL, NULL),
(1065, 'MirrorPark 25', 'apa_v_mp_h_01_b', 988.597, -527.055, 60.4761, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1065, 0, 0, NULL, NULL),
(1066, 'MirrorPark 26', 'apa_v_mp_h_01_b', 965.694, -542.511, 59.3591, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1066, 0, 0, NULL, NULL),
(1067, 'MirrorPark 27', 'apa_v_mp_h_01_b', 919.931, -570.005, 58.3664, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1067, 0, 0, NULL, NULL),
(1068, 'MirrorPark 28', 'apa_v_mp_h_01_b', 892.736, -540.772, 58.5066, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1068, 0, 0, NULL, NULL),
(1069, 'MirrorPark 29', 'apa_v_mp_h_01_b', 923.784, -525.126, 59.5745, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1069, 0, 0, NULL, NULL),
(1070, 'MirrorPark 30', 'apa_v_mp_h_01_b', 945.818, -518.604, 60.6275, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1070, 0, 0, NULL, NULL),
(1071, 'MirrorPark 31', 'apa_v_mp_h_01_b', 969.744, -502.128, 62.1398, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1071, 0, 0, NULL, NULL),
(1072, 'MirrorPark 32', 'apa_v_mp_h_01_b', 1014.26, -468.161, 64.2885, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1072, 0, 0, NULL, NULL),
(1073, 'MirrorPark 33', 'apa_v_mp_h_01_b', 1056.56, -447.604, 66.2574, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1073, 0, 0, NULL, NULL),
(1074, 'MirrorPark 34', 'apa_v_mp_h_01_b', 1052.81, -470.964, 63.8989, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1074, 0, 0, NULL, NULL),
(1075, 'MirrorPark 35', 'apa_v_mp_h_01_b', 1046.66, -497.305, 64.0793, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1075, 0, 0, NULL, NULL),
(1076, 'MirrorPark 36', 'apa_v_mp_h_01_b', 1089.25, -484.342, 65.6552, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1076, 0, 0, NULL, NULL),
(1077, 'MirrorPark 37', 'apa_v_mp_h_01_b', 1098.59, -465.161, 67.3194, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1077, 0, 0, NULL, NULL),
(1078, 'MirrorPark 38', 'apa_v_mp_h_01_b', 1098.29, -450.47, 67.4303, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1078, 0, 0, NULL, NULL),
(1079, 'MirrorPark 39', 'apa_v_mp_h_01_b', 1099.49, -437.618, 67.5883, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1079, 0, 0, NULL, NULL),
(1080, 'MirrorPark 40', 'apa_v_mp_h_01_b', 1100.33, -411.529, 67.5551, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1080, 0, 0, NULL, NULL),
(1081, 'MirrorPark 41', 'apa_v_mp_h_01_b', 1113.77, -390.912, 68.7414, -774.013, 342.043, 196.686, 0, 200000, NULL, 0, 1, 1081, 0, 0, NULL, NULL),
(1082, 'SanAndreas 1', 'v_motel_mp', -111.513, 6322.53, 31.5762, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1082, 1, 200, NULL, NULL),
(1083, 'SanAndreas 2', 'v_motel_mp', -114.344, 6325.96, 31.5761, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1083, 1, 200, NULL, NULL),
(1084, 'SanAndreas 3', 'v_motel_mp', -120.316, 6327.19, 31.5759, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1084, 1, 200, NULL, NULL),
(1085, 'SanAndreas 6', 'v_motel_mp', -120.149, 6327.21, 35.501, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1085, 1, 200, NULL, NULL),
(1086, 'SanAndreas 5', 'v_motel_mp', -114.351, 6325.97, 35.501, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1086, 1, 200, NULL, NULL),
(1087, 'SanAndreas 4', 'v_motel_mp', -111.143, 6322.76, 35.501, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1087, 1, 200, NULL, NULL),
(1088, 'SanAndreas 7', 'v_motel_mp', -103.497, 6330.82, 31.5762, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1088, 1, 200, NULL, NULL),
(1089, 'SanAndreas 8', 'v_motel_mp', -106.815, 6334.04, 31.5762, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1089, 1, 200, NULL, NULL),
(1090, 'SanAndreas 9', 'v_motel_mp', -107.546, 6339.86, 31.5759, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1090, 1, 200, NULL, NULL),
(1091, 'SanAndreas 10', 'v_motel_mp', -102.19, 6345.28, 31.5759, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1091, 1, 200, NULL, NULL),
(1092, 'SanAndreas 11', 'v_motel_mp', -98.8076, 6348.58, 31.5759, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1092, 1, 200, NULL, NULL),
(1093, 'SanAndreas 12', 'v_motel_mp', -93.4823, 6353.86, 31.5759, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1093, 1, 200, NULL, NULL),
(1094, 'SanAndreas 13', 'v_motel_mp', -90.1748, 6357.19, 31.5759, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1094, 1, 200, NULL, NULL),
(1095, 'SanAndreas 14', 'v_motel_mp', -84.8828, 6362.47, 31.5759, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1095, 1, 200, NULL, NULL),
(1096, 'SanAndreas 22', 'v_motel_mp', -84.7756, 6362.39, 35.5007, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1096, 1, 200, NULL, NULL),
(1097, 'SanAndreas 21', 'v_motel_mp', -90.1453, 6357.11, 35.5007, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1097, 1, 200, NULL, NULL),
(1098, 'SanAndreas 20', 'v_motel_mp', -93.311, 6353.85, 35.5007, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1098, 1, 200, NULL, NULL),
(1099, 'SanAndreas 19', 'v_motel_mp', -98.9135, 6348.48, 35.5007, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1099, 1, 200, NULL, NULL),
(1100, 'SanAndreas 18', 'v_motel_mp', -102.306, 6345.05, 35.5007, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1100, 1, 200, NULL, NULL),
(1101, 'SanAndreas 17', 'v_motel_mp', -107.452, 6339.96, 35.5007, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1101, 1, 200, NULL, NULL),
(1102, 'SanAndreas 16', 'v_motel_mp', -106.703, 6334.16, 35.5007, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1102, 1, 200, NULL, NULL),
(1103, 'SanAndreas 15', 'v_motel_mp', -103.474, 6330.7, 35.5007, 151.587, -1007.01, -99, 0, 0, NULL, 0, 1, 1103, 1, 200, NULL, NULL),
(1104, 'ProcopinoDrive 1', 'apa_v_mp_h_01_a', 35.4521, 6662.81, 32.1904, 346.348, -1013.05, -99.196, 0, 175000, NULL, 0, 1, 1104, 0, 0, NULL, NULL),
(1105, 'ProcopinoDrive 2', 'apa_v_mp_h_01_a', -9.51127, 6654.27, 31.6987, 346.348, -1013.05, -99.196, 0, 180000, NULL, 0, 1, 1105, 0, 0, NULL, NULL),
(1106, 'ProcopinoDrive 3', 'apa_v_mp_h_01_a', -41.5362, 6637.47, 31.0875, 346.348, -1013.05, -99.196, 0, 180000, NULL, 0, 1, 1106, 0, 0, NULL, NULL),
(1107, 'ProcopinoDrive 5', 'apa_v_mp_h_01_a', -229.538, 6445.12, 31.1975, 346.348, -1013.05, -99.196, 0, 180000, NULL, 0, 1, 1107, 0, 0, NULL, NULL),
(1108, 'ProcopinoDrive 6', 'apa_v_mp_h_01_a', -238.31, 6423.68, 31.4586, 346.348, -1013.05, -99.196, 0, 175000, NULL, 0, 1, 1108, 0, 0, NULL, NULL),
(1109, 'ProcopinoDrive 7', 'apa_v_mp_h_01_a', -245.896, 6414.39, 31.4606, 346.348, -1013.05, -99.196, 0, 175000, NULL, 0, 1, 1109, 0, 0, NULL, NULL),
(1110, 'ProcopinoDrive 4', 'apa_v_mp_h_02_a', -130.708, 6551.93, 29.8732, -787.075, 315.82, 217.639, 0, 200000, NULL, 0, 1, 1110, 0, 0, NULL, NULL),
(1111, 'ProcopinoDrive 8', 'apa_v_mp_h_01_a', -272.497, 6400.98, 31.505, 346.348, -1013.05, -99.196, 0, 175000, NULL, 0, 1, 1111, 0, 0, NULL, NULL),
(1112, 'ProcopinoDrive 10', 'apa_v_mp_h_02_a', -407.215, 6314.21, 28.9414, -787.075, 315.82, 217.639, 0, 200000, NULL, 0, 1, 1112, 0, 0, NULL, NULL),
(1113, 'ProcopinoDrive 12', 'apa_v_mp_h_01_a', -447.936, 6259.98, 30.048, 346.348, -1013.05, -99.196, 0, 0, NULL, 0, 1, 1113, 1, 500, NULL, NULL),
(1114, 'ProcopinoDrive 11', 'apa_v_mp_h_01_a', -438.057, 6272.34, 30.0683, 346.348, -1013.05, -99.196, 0, 0, NULL, 0, 1, 1114, 1, 500, NULL, NULL),
(1115, 'ProcopinoDrive 13', 'apa_v_mp_h_01_a', -467.912, 6206.27, 29.5528, 346.348, -1013.05, -99.196, 0, 175000, NULL, 0, 1, 1115, 0, 0, NULL, NULL),
(1116, 'ProcopinoDrive 14', 'apa_v_mp_h_01_a', -379.85, 6252.74, 31.8512, 346.348, -1013.05, -99.196, 0, 175000, NULL, 0, 1, 1116, 0, 0, NULL, NULL),
(1117, 'ProcopinoDrive 15', 'apa_v_mp_h_01_a', -360.258, 6260.64, 31.9, 346.348, -1013.05, -99.196, 0, 150000, NULL, 0, 1, 1117, 0, 0, NULL, NULL),
(1118, 'ProcopinoDrive 17', 'apa_v_mp_h_01_a', -302.219, 6327.02, 32.8868, 346.348, -1013.05, -99.196, 0, 150000, NULL, 0, 1, 1118, 0, 0, NULL, NULL),
(1119, 'ProcopinoDrive 19', 'apa_v_mp_h_01_a', -247.758, 6370.09, 31.8457, 346.348, -1013.05, -99.196, 0, 130000, NULL, 0, 1, 1119, 0, 0, NULL, NULL),
(1120, 'ProcopinoDrive 20', 'apa_v_mp_h_01_a', -227.22, 6377.41, 31.7592, 346.348, -1013.05, -99.196, 0, 130000, NULL, 0, 1, 1120, 0, 0, NULL, NULL),
(1121, 'ProcopinoDrive 21', 'apa_v_mp_h_01_a', -213.722, 6396.2, 33.0851, 346.348, -1013.05, -99.196, 0, 150000, NULL, 0, 1, 1121, 0, 0, NULL, NULL),
(1122, 'ProcopinoDrive 22', 'apa_v_mp_h_01_a', -188.891, 6409.61, 32.2968, 346.348, -1013.05, -99.196, 0, 150000, NULL, 0, 1, 1122, 0, 0, NULL, NULL),
(1123, 'ProcopinoDrive 23', 'apa_v_mp_h_01_a', -105.385, 6529.09, 30.1669, 346.348, -1013.05, -99.196, 0, 155000, NULL, 0, 1, 1123, 0, 0, NULL, NULL),
(1124, 'ProcopinoDrive 24', 'apa_v_mp_h_01_a', -44.7114, 6582.16, 32.1755, 346.348, -1013.05, -99.196, 0, 150000, NULL, 0, 1, 1124, 0, 0, NULL, NULL),
(1125, 'ProcopinoDrive 25', 'apa_v_mp_h_01_a', -26.6158, 6597.13, 31.8608, 346.348, -1013.05, -99.196, 0, 155000, NULL, 0, 1, 1125, 0, 0, NULL, NULL),
(1126, 'ProcopinoDrive 26', 'apa_v_mp_h_01_a', 1.5536, 6612.41, 32.0796, 346.348, -1013.05, -99.196, 0, 155000, NULL, 0, 1, 1126, 0, 0, NULL, NULL),
(1127, 'PaletoBulevard 1', 'apa_v_mp_h_01_a', 30.9925, 6596.63, 32.8222, 346.348, -1013.05, -99.196, 0, 170000, NULL, 0, 1, 1127, 0, 0, NULL, NULL),
(1128, 'PaletoBoulevard 3', 'apa_v_mp_h_01_a', -15.5327, 6557.55, 33.2404, 346.348, -1013.05, -99.196, 0, 170000, NULL, 0, 1, 1128, 0, 0, NULL, NULL),
(1129, 'PaletoBoulevard 4', 'apa_v_mp_h_01_a', -347.567, 6225.31, 31.8841, 346.348, -1013.05, -99.196, 0, 150000, NULL, 0, 1, 1129, 0, 0, NULL, NULL),
(1130, 'PaletoBoulevard 5', 'apa_v_mp_h_01_a', -356.887, 6207.57, 31.8423, 346.348, -1013.05, -99.196, 0, 160000, NULL, 0, 1, 1130, 0, 0, NULL, NULL),
(1131, 'PaletoBoulevard 6', 'apa_v_mp_h_01_a', -374.554, 6190.99, 31.7295, 346.348, -1013.05, -99.196, 0, 160000, NULL, 0, 1, 1131, 0, 0, NULL, NULL);

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `house_keys`
--

CREATE TABLE `house_keys` (
  `id` int(11) NOT NULL,
  `house_id` int(11) NOT NULL,
  `target_acc_id` int(11) NOT NULL,
  `target_player_name` varchar(255) NOT NULL,
  `key_type` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `player_tracking`
--

CREATE TABLE `player_tracking` (
  `account_id` int(11) NOT NULL,
  `position_x` float NOT NULL,
  `position_y` float NOT NULL,
  `position_z` float NOT NULL,
  `played_time` int(11) NOT NULL,
  `last_health` int(11) NOT NULL,
  `last_armor` int(11) NOT NULL,
  `is_dead` tinyint(1) NOT NULL,
  `remaining_time` int(11) NOT NULL,
  `last_updated` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `last_payday` int(11) DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Daten für Tabelle `player_tracking`
--

INSERT INTO `player_tracking` (`account_id`, `position_x`, `position_y`, `position_z`, `played_time`, `last_health`, `last_armor`, `is_dead`, `remaining_time`, `last_updated`, `last_payday`) VALUES
(4, 1097.5, -1169.69, 55.5517, 127724, 100, 100, 0, 0, '2025-07-02 13:21:45', 126000),
(12, 1757.41, 3704.59, 34.4415, 16208, 100, 100, 0, 0, '2025-06-30 18:11:37', 14400);

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `player_vehicles`
--

CREATE TABLE `player_vehicles` (
  `id` int(11) NOT NULL,
  `ownerAccountId` int(11) NOT NULL,
  `model` varchar(50) NOT NULL,
  `plate` varchar(8) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `player_warnings`
--

CREATE TABLE `player_warnings` (
  `id` int(11) NOT NULL,
  `admin_id` int(11) NOT NULL,
  `target_id` int(11) NOT NULL,
  `admin_name` varchar(50) NOT NULL,
  `reason` text NOT NULL,
  `warning_date` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `support_tickets`
--

CREATE TABLE `support_tickets` (
  `id` int(11) NOT NULL,
  `reporter_id` int(11) NOT NULL,
  `reporter_name` varchar(50) NOT NULL,
  `message` text NOT NULL,
  `pos_x` float NOT NULL,
  `pos_y` float NOT NULL,
  `pos_z` float NOT NULL,
  `report_date` datetime NOT NULL DEFAULT current_timestamp(),
  `status` enum('Offen','In Bearbeitung','Geschlossen') NOT NULL DEFAULT 'Offen',
  `is_priority` tinyint(1) NOT NULL DEFAULT 0,
  `admin_comment` text DEFAULT NULL,
  `handled_by_admin_id` int(11) DEFAULT NULL,
  `handled_date` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `vehicle_definitions`
--

CREATE TABLE `vehicle_definitions` (
  `model` varchar(50) NOT NULL,
  `displayName` varchar(50) NOT NULL,
  `category` varchar(50) NOT NULL,
  `price` int(10) UNSIGNED NOT NULL,
  `isFactionBuyable` tinyint(1) NOT NULL DEFAULT 0,
  `description` text DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='Master-Katalog aller kaufbaren Fahrzeuge.';

--
-- Daten für Tabelle `vehicle_definitions`
--

INSERT INTO `vehicle_definitions` (`model`, `displayName`, `category`, `price`, `isFactionBuyable`, `description`) VALUES
('AKUMA', 'AKUMA', 'Bikes', 50000, 1, NULL),
('BANSHEE', 'BANSHEE', 'Sport', 85000, 0, NULL),
('BATI', 'BATI', 'Bikes', 80000, 1, NULL),
('BRIOSO', 'BRIOSO', 'lowb', 25000, 0, NULL),
('BUCCANEE2', 'BUCCANEE2', 'Classic', 75000, 1, NULL),
('BUFFALO', 'BUFFALO', 'Sport', 95000, 0, NULL),
('BURRITO', 'BURRITO', 'Sveh', 75000, 1, NULL),
('BUZZARD2', 'BUZZARD2', 'heli', 1200000, 1, NULL),
('CHINO2', 'CHINO2', 'Classic', 75000, 1, NULL),
('COMET2', 'COMET2', 'Sport', 95000, 0, NULL),
('DAEMON', 'DAEMON', 'Bikes', 50000, 1, NULL),
('DEATHBIKE', 'DEATHBIKE', 'Bikes', 50000, 1, NULL),
('DUKES', 'DUKES', 'Classic', 75000, 1, NULL),
('GBURRITO2', 'GBURRITO2', 'Sveh', 80000, 1, NULL),
('INFERNUS', 'INFERNUS', 'SuperSport', 250000, 0, NULL),
('PEYOTE', 'PEYOTE', 'Classic', 75000, 1, NULL),
('RUMPO3', 'RUMPO3', 'Sveh', 80000, 1, NULL),
('SCHAFTER3', 'SCHAFTER3', 'Sport', 100000, 1, NULL),
('SPEEDER', 'SPEEDER', 'boat', 140000, 0, NULL),
('STANIER', 'STANIER', 'lowb', 35000, 1, NULL),
('SULTAN', 'SULTAN', 'Sport', 75000, 0, NULL),
('SUNTRAP', 'SUNTRAP', 'boat', 100000, 0, NULL),
('SWIFT', 'SWIFT', 'heli', 2000000, 1, NULL),
('TEMPESTA', 'TEMPESTA', 'SuperSport', 250000, 0, NULL),
('TORNADO5', 'TORNADO5', 'Classic', 75000, 1, NULL),
('VIGERO', 'VIGERO', 'Classic', 75000, 1, NULL);

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `vehsafe`
--

CREATE TABLE `vehsafe` (
  `id` int(11) NOT NULL,
  `owner` int(11) NOT NULL,
  `is_faction_vehicle` tinyint(1) DEFAULT 0,
  `faction_id` int(11) DEFAULT 0,
  `model_name` varchar(50) NOT NULL,
  `number_plate` varchar(20) NOT NULL,
  `modifications` text NOT NULL,
  `pos_x` float DEFAULT 0,
  `pos_y` float DEFAULT 0,
  `pos_z` float DEFAULT 0,
  `heading` float DEFAULT 0,
  `color_primary` int(11) DEFAULT 0,
  `color_secondary` int(11) DEFAULT 0,
  `health` float DEFAULT 1000,
  `status` int(11) NOT NULL DEFAULT 0,
  `garage_id` int(11) DEFAULT NULL,
  `is_in_garage` tinyint(1) DEFAULT 0,
  `fuel` float NOT NULL DEFAULT 100,
  `mileage` float NOT NULL DEFAULT 0,
  `fuel_type` enum('benzin','diesel','kerosin') NOT NULL DEFAULT 'benzin'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Daten für Tabelle `vehsafe`
--

INSERT INTO `vehsafe` (`id`, `owner`, `is_faction_vehicle`, `faction_id`, `model_name`, `number_plate`, `modifications`, `pos_x`, `pos_y`, `pos_z`, `heading`, `color_primary`, `color_secondary`, `health`, `status`, `garage_id`, `is_in_garage`, `fuel`, `mileage`, `fuel_type`) VALUES
(1, 4, 0, 0, 'Tempesta', 'OG', '{\"0\":3,\"1\":3,\"2\":3,\"3\":3,\"4\":3,\"5\":3,\"6\":3,\"7\":3,\"8\":3,\"9\":3,\"10\":3,\"11\":3,\"12\":2,\"13\":1,\"14\":3,\"15\":4,\"16\":4,\"17\":3,\"18\":3,\"19\":3,\"20\":3,\"21\":3,\"22\":3,\"23\":3,\"24\":3,\"25\":3,\"26\":3,\"27\":3,\"28\":3,\"29\":3,\"30\":3,\"31\":3,\"32\":3,\"33\":3,\"34\":3,\"35\":3,\"36\":3,\"37\":3,\"38\":3,\"39\":3,\"40\":3,\"41\":3,\"42\":3,\"43\":3,\"44\":3,\"45\":3,\"46\":3,\"47\":3,\"48\":3}', 300.22, -329.99, 44.92, 63.47, 55, 55, 1000, 0, 1, 1, 100, 0, 'benzin'),
(2, 4, 0, 0, 'polndom', 'CHIEF_02', '{\"0\":3,\"1\":3,\"2\":3,\"3\":3,\"4\":3,\"5\":3,\"6\":3,\"7\":3,\"8\":3,\"9\":3,\"10\":3,\"11\":3,\"12\":2,\"13\":1,\"14\":3,\"15\":4,\"16\":4,\"17\":3,\"18\":3,\"19\":3,\"20\":3,\"21\":3,\"22\":3,\"23\":3,\"24\":3,\"25\":3,\"26\":3,\"27\":3,\"28\":3,\"29\":3,\"30\":3,\"31\":3,\"32\":3,\"33\":3,\"34\":3,\"35\":3,\"36\":3,\"37\":3,\"38\":3,\"39\":3,\"40\":3,\"41\":3,\"42\":3,\"43\":3,\"44\":3,\"45\":3,\"46\":3,\"47\":3,\"48\":3}', 407.983, -988.029, 29.2289, -115.313, 0, 0, 1000, 0, 0, 0, 100, 0, 'benzin'),
(3, 4, 0, 0, 'polcara6x6', 'CHIEF_01', '{\"0\":3,\"1\":3,\"2\":3,\"3\":3,\"4\":3,\"5\":3,\"6\":3,\"7\":3,\"8\":3,\"9\":3,\"10\":3,\"11\":3,\"12\":2,\"13\":1,\"14\":3,\"15\":4,\"16\":4,\"17\":3,\"18\":3,\"19\":3,\"20\":3,\"21\":3,\"22\":3,\"23\":3,\"24\":3,\"25\":3,\"26\":3,\"27\":3,\"28\":3,\"29\":3,\"30\":3,\"31\":3,\"32\":3,\"33\":3,\"34\":3,\"35\":3,\"36\":3,\"37\":3,\"38\":3,\"39\":3,\"40\":3,\"41\":3,\"42\":3,\"43\":3,\"44\":3,\"45\":3,\"46\":3,\"47\":3,\"48\":3}', 403.889, -1019.3, 30.4491, -31.2891, 0, 0, 1000, 0, 0, 0, 100, 0, 'benzin'),
(4, 0, 1, 2, 'fbi', 'MD1', '', 282.568, -610.642, 42.9314, 86.8397, 39, 39, 1000, 0, 0, 0, 100, 0, 'benzin'),
(5, 0, 1, 2, 'ambulance', 'MD2', '', 294.572, -604.01, 43.0773, 72.0712, 111, 111, 1000, 0, 0, 0, 100, 0, 'benzin'),
(6, 0, 1, 2, 'ambulance', 'MD3', '', 292.956, -607.775, 43.1057, 85.0937, 111, 111, 1000, 0, 0, 0, 100, 0, 'benzin'),
(7, 0, 1, 2, 'ambulance', 'MD4', '', 274.122, -608.795, 42.6617, 92.8181, 111, 111, 1000, 0, 0, 0, 100, 0, 'benzin'),
(8, 0, 1, 2, 'lguard', 'MD4', '', 321.165, -542.193, 28.3755, 3.51559, 28, 28, 1000, 0, 0, 0, 100, 0, 'benzin'),
(9, 0, 1, 2, 'lguard', 'MD5', '', 326.69, -542.374, 28.3755, 4.92184, 28, 28, 1000, 0, 0, 0, 100, 0, 'benzin'),
(10, 0, 1, 2, 'ambulance', 'MD6', '', 332.291, -542.299, 28.5084, 13.3591, 111, 111, 1000, 0, 0, 0, 100, 0, 'benzin'),
(11, 0, 1, 1, 'police3', 'LSPD1', '', 455.225, -1021.07, 28.1091, -34.8122, 134, 134, 1000, 0, 0, 0, 100, 0, 'benzin'),
(12, 0, 1, 1, 'police3', 'LSPD2', '', 453.276, -1011.08, 28.0374, 2.5412, 134, 134, 1000, 0, 666, 1, 100, 0, 'benzin'),
(13, 0, 1, 1, 'police3', 'LSPD3', '', 450.98, -1020.87, 28.1734, -58.0506, 134, 134, 1000, 0, 0, 0, 100, 0, 'benzin'),
(14, 0, 1, 1, 'police3', 'LSPD4', '', 446.85, -1021.29, 28.2563, -58.0415, 134, 134, 1000, 0, 0, 0, 100, 0, 'benzin'),
(15, 0, 1, 1, 'police3', 'LSPD5', '', 20.7163, -2532.58, 5.8043, 67.7537, 134, 134, 1000, 0, 0, 0, 100, 0, 'benzin'),
(16, 0, 1, 1, 'riot', 'LSPDSF', '', 429.847, -1028.17, 28.5993, 105.132, 1, 1, 1000, 0, 0, 0, 100, 0, 'benzin'),
(17, 0, 1, 1, 'riot2', 'LSPDSF', '', 421.28, -1029.49, 28.9326, 104.089, 1, 1, 1000, 0, 0, 0, 100, 0, 'benzin'),
(18, 0, 1, 1, 'polmav', 'LSPDMAV', '', 452.41, -985.256, 44.0792, -120.887, 134, 0, 1000, 0, 0, 0, 100, 0, 'benzin'),
(19, 0, 1, 1, 'fbi', 'LSPD6', '', 434.886, -1027.32, 28.5018, 1.40096, 0, 0, 1000, 0, 0, 0, 100, 0, 'benzin'),
(20, 0, 1, 1, 'fbi', 'LSPD7', '', 438.44, -1022.96, 28.3429, -23.5532, 0, 0, 1000, 0, 0, 0, 100, 0, 'benzin'),
(21, 0, 1, 3, 'towtruck', 'LSCS1', '', -159.059, -1305.97, 31.2534, 33.3904, 41, 41, 1000, 0, 0, 0, 100, 0, 'benzin'),
(22, 0, 1, 3, 'towtruck', 'LSCS2', '', -164.873, -1305.95, 31.2515, 32.6761, 41, 41, 1000, 0, 0, 0, 100, 0, 'benzin'),
(23, 0, 1, 3, 'towtruck', 'LSCS3', '', -171.773, -1306.15, 31.2514, 36.5792, 41, 41, 1000, 0, 0, 0, 100, 0, 'benzin'),
(24, 0, 1, 3, 'cargobob', 'LSCS4', '', -762.344, -1453.88, 5.46271, -29.9332, 41, 41, 1000, 0, 1, 6, 100, 0, 'benzin'),
(25, 0, 1, 3, 'bison', 'LSCS5', '', -222.046, -1309.3, 34.0888, -137.461, 41, 41, 1000, 0, 0, 0, 100, 0, 'benzin'),
(26, 4, 0, 0, 'virtue', 'Babinski', '{\"0\":3,\"1\":3,\"2\":3,\"3\":3,\"4\":3,\"5\":3,\"6\":3,\"7\":3,\"8\":3,\"9\":3,\"10\":3,\"11\":3,\"12\":2,\"13\":1,\"14\":3,\"15\":4,\"16\":4,\"17\":3,\"18\":3,\"19\":3,\"20\":3,\"21\":3,\"22\":3,\"23\":3,\"24\":3,\"25\":3,\"26\":3,\"27\":3,\"28\":3,\"29\":3,\"30\":3,\"31\":3,\"32\":3,\"33\":3,\"34\":3,\"35\":3,\"36\":3,\"37\":3,\"38\":3,\"39\":3,\"40\":3,\"41\":3,\"42\":3,\"43\":3,\"44\":3,\"45\":3,\"46\":3,\"47\":3,\"48\":3}', 1096.81, -1168.26, 54.8648, 87.9147, 0, 0, 1000, 0, NULL, 0, 100, 0, 'benzin'),
(27, 4, 0, 0, 'SULTAN', 'H7NN399C', '{}', -1257.91, -337.047, 36.2447, 128.372, 0, 0, 1000, 0, NULL, 0, 100, 0, 'benzin'),
(28, 12, 0, 0, 'RUMPO3', 'CQ67LRBP', '{\"0\":3,\"1\":3,\"2\":3,\"3\":3,\"4\":3,\"5\":3,\"6\":3,\"7\":3,\"8\":3,\"9\":3,\"10\":3,\"11\":3,\"12\":2,\"13\":1,\"14\":3,\"15\":4,\"16\":4,\"17\":3,\"18\":3,\"19\":3,\"20\":3,\"21\":3,\"22\":3,\"23\":3,\"24\":3,\"25\":3,\"26\":3,\"27\":3,\"28\":3,\"29\":3,\"30\":3,\"31\":3,\"32\":3,\"33\":3,\"34\":3,\"35\":3,\"36\":3,\"37\":3,\"38\":3,\"39\":3,\"40\":3,\"41\":3,\"42\":3,\"43\":3,\"44\":3,\"45\":3,\"46\":3,\"47\":3,\"48\":3}', 1768.28, 3702.29, 34.3715, 28.2402, 0, 0, 1000, 0, NULL, 0, 100, 0, 'benzin'),
(29, 4, 0, 0, 'stafford', 'Babinski_01', '{\"0\":3,\"1\":3,\"2\":3,\"3\":3,\"4\":3,\"5\":3,\"6\":3,\"7\":3,\"8\":3,\"9\":3,\"10\":3,\"11\":3,\"12\":2,\"13\":1,\"14\":3,\"15\":4,\"16\":4,\"17\":3,\"18\":3,\"19\":3,\"20\":3,\"21\":3,\"22\":3,\"23\":3,\"24\":3,\"25\":3,\"26\":3,\"27\":3,\"28\":3,\"29\":3,\"30\":3,\"31\":3,\"32\":3,\"33\":3,\"34\":3,\"35\":3,\"36\":3,\"37\":3,\"38\":3,\"39\":3,\"40\":3,\"41\":3,\"42\":3,\"43\":3,\"44\":3,\"45\":3,\"46\":3,\"47\":3,\"48\":3}', 408.141, -998.06, 29.8129, -132.188, 1, 1, 1000, 0, 0, 0, 100, 0, 'benzin'),
(30, 0, 1, 4, 'squalo', 'LSDS01', '{}', -723.129, -1318.66, 0.350788, -130.175, 0, 0, 1000, 0, 1, 1, 100, 0, 'benzin'),
(31, 0, 1, 4, 'felon', 'LSDS02', '{}', -705.143, -1278.3, 4.6897, 141.504, 0, 0, 1000, 0, NULL, 0, 100, 0, 'benzin'),
(32, 0, 1, 4, 'felon', 'LSDS03', '{}', -710.581, -1274.48, 4.68971, 140.257, 0, 0, 1000, 0, NULL, 0, 100, 0, 'benzin'),
(33, 0, 1, 4, 'felon', 'LSDS04', '{}', -708.09, -1276.68, 4.68971, 140.688, 0, 0, 1000, 0, NULL, 0, 100, 0, 'benzin'),
(34, 0, 1, 4, 'felon', 'LSDS05', '{}', -712.914, -1272.52, 4.6897, 140.172, 0, 0, 1000, 0, NULL, 0, 100, 0, 'benzin'),
(35, 0, 1, 4, 'mule3', 'LSDS6', '{}', -718.971, -1290.86, 5.23183, 49.9222, 0, 0, 1000, 0, NULL, 0, 100, 0, 'benzin'),
(36, 0, 1, 4, 'mule3', 'LSDS7', '{}', -720.937, -1293.49, 5.23184, 49.9219, 0, 0, 1000, 0, NULL, 0, 100, 0, 'benzin'),
(37, 0, 1, 4, 'seasparrow2', 'LSDS8', '{}', -745.471, -1470.18, 5.61871, -164.713, 0, 0, 961, 0, 1, 6, 100, 0, 'benzin');

--
-- Indizes der exportierten Tabellen
--

--
-- Indizes für die Tabelle `accounts`
--
ALTER TABLE `accounts`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `socialClubId` (`socialClubId`);

--
-- Indizes für die Tabelle `bankautomaten`
--
ALTER TABLE `bankautomaten`
  ADD PRIMARY KEY (`id`);

--
-- Indizes für die Tabelle `bankkonten`
--
ALTER TABLE `bankkonten`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `kontonummer` (`kontonummer`);

--
-- Indizes für die Tabelle `bank_transactions`
--
ALTER TABLE `bank_transactions`
  ADD PRIMARY KEY (`id`),
  ADD KEY `kontonummer_idx` (`kontonummer`),
  ADD KEY `player_id_idx` (`player_id`);

--
-- Indizes für die Tabelle `dealerships`
--
ALTER TABLE `dealerships`
  ADD PRIMARY KEY (`id`);

--
-- Indizes für die Tabelle `fishing_loottable`
--
ALTER TABLE `fishing_loottable`
  ADD PRIMARY KEY (`id`);

--
-- Indizes für die Tabelle `fraktion`
--
ALTER TABLE `fraktion`
  ADD PRIMARY KEY (`id`);

--
-- Indizes für die Tabelle `fraktionsmitglieder`
--
ALTER TABLE `fraktionsmitglieder`
  ADD PRIMARY KEY (`id`),
  ADD KEY `account_id` (`account_id`),
  ADD KEY `fraktion_id` (`fraktion_id`);

--
-- Indizes für die Tabelle `fraktion_bankkonten`
--
ALTER TABLE `fraktion_bankkonten`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `kontonummer` (`kontonummer`),
  ADD UNIQUE KEY `fraktion_id` (`fraktion_id`);

--
-- Indizes für die Tabelle `fraktion_bank_transactions`
--
ALTER TABLE `fraktion_bank_transactions`
  ADD PRIMARY KEY (`id`),
  ADD KEY `kontonummer_idx` (`kontonummer`),
  ADD KEY `fraktion_id_idx` (`fraktion_id`);

--
-- Indizes für die Tabelle `garages`
--
ALTER TABLE `garages`
  ADD PRIMARY KEY (`id`);

--
-- Indizes für die Tabelle `garage_spawnpoints`
--
ALTER TABLE `garage_spawnpoints`
  ADD PRIMARY KEY (`id`),
  ADD KEY `GarageId` (`GarageId`);

--
-- Indizes für die Tabelle `houses`
--
ALTER TABLE `houses`
  ADD PRIMARY KEY (`id`);

--
-- Indizes für die Tabelle `house_keys`
--
ALTER TABLE `house_keys`
  ADD PRIMARY KEY (`id`),
  ADD KEY `house_id` (`house_id`);

--
-- Indizes für die Tabelle `player_tracking`
--
ALTER TABLE `player_tracking`
  ADD PRIMARY KEY (`account_id`),
  ADD KEY `idx_account_id` (`account_id`);

--
-- Indizes für die Tabelle `player_vehicles`
--
ALTER TABLE `player_vehicles`
  ADD PRIMARY KEY (`id`),
  ADD KEY `ownerAccountId` (`ownerAccountId`);

--
-- Indizes für die Tabelle `player_warnings`
--
ALTER TABLE `player_warnings`
  ADD PRIMARY KEY (`id`);

--
-- Indizes für die Tabelle `support_tickets`
--
ALTER TABLE `support_tickets`
  ADD PRIMARY KEY (`id`);

--
-- Indizes für die Tabelle `vehicle_definitions`
--
ALTER TABLE `vehicle_definitions`
  ADD PRIMARY KEY (`model`);

--
-- Indizes für die Tabelle `vehsafe`
--
ALTER TABLE `vehsafe`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT für exportierte Tabellen
--

--
-- AUTO_INCREMENT für Tabelle `accounts`
--
ALTER TABLE `accounts`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=13;

--
-- AUTO_INCREMENT für Tabelle `bankautomaten`
--
ALTER TABLE `bankautomaten`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=30;

--
-- AUTO_INCREMENT für Tabelle `bankkonten`
--
ALTER TABLE `bankkonten`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT für Tabelle `bank_transactions`
--
ALTER TABLE `bank_transactions`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT für Tabelle `dealerships`
--
ALTER TABLE `dealerships`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=44;

--
-- AUTO_INCREMENT für Tabelle `fishing_loottable`
--
ALTER TABLE `fishing_loottable`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT für Tabelle `fraktion`
--
ALTER TABLE `fraktion`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT für Tabelle `fraktionsmitglieder`
--
ALTER TABLE `fraktionsmitglieder`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=12;

--
-- AUTO_INCREMENT für Tabelle `fraktion_bankkonten`
--
ALTER TABLE `fraktion_bankkonten`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT für Tabelle `fraktion_bank_transactions`
--
ALTER TABLE `fraktion_bank_transactions`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT für Tabelle `garages`
--
ALTER TABLE `garages`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=670;

--
-- AUTO_INCREMENT für Tabelle `garage_spawnpoints`
--
ALTER TABLE `garage_spawnpoints`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT für Tabelle `houses`
--
ALTER TABLE `houses`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=1132;

--
-- AUTO_INCREMENT für Tabelle `house_keys`
--
ALTER TABLE `house_keys`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT für Tabelle `player_vehicles`
--
ALTER TABLE `player_vehicles`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT für Tabelle `player_warnings`
--
ALTER TABLE `player_warnings`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT für Tabelle `support_tickets`
--
ALTER TABLE `support_tickets`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT für Tabelle `vehsafe`
--
ALTER TABLE `vehsafe`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=38;

--
-- Constraints der exportierten Tabellen
--

--
-- Constraints der Tabelle `fraktionsmitglieder`
--
ALTER TABLE `fraktionsmitglieder`
  ADD CONSTRAINT `fraktionsmitglieder_ibfk_1` FOREIGN KEY (`account_id`) REFERENCES `accounts` (`id`),
  ADD CONSTRAINT `fraktionsmitglieder_ibfk_2` FOREIGN KEY (`fraktion_id`) REFERENCES `fraktion` (`id`);

--
-- Constraints der Tabelle `garage_spawnpoints`
--
ALTER TABLE `garage_spawnpoints`
  ADD CONSTRAINT `garage_spawnpoints_ibfk_1` FOREIGN KEY (`GarageId`) REFERENCES `garages` (`id`) ON DELETE CASCADE;

--
-- Constraints der Tabelle `player_tracking`
--
ALTER TABLE `player_tracking`
  ADD CONSTRAINT `player_tracking_ibfk_1` FOREIGN KEY (`account_id`) REFERENCES `accounts` (`id`);

--
-- Constraints der Tabelle `player_vehicles`
--
ALTER TABLE `player_vehicles`
  ADD CONSTRAINT `player_vehicles_ibfk_1` FOREIGN KEY (`ownerAccountId`) REFERENCES `accounts` (`id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
