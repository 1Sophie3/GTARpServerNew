-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Erstellungszeit: 23. Mai 2025 um 15:02
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
  `hardware_id` varchar(255) NOT NULL,
  `is_banned` tinyint(1) DEFAULT 0,
  `ban_reason` varchar(255) DEFAULT NULL,
  `characterdata` longtext NOT NULL,
  `fraktion` int(11) DEFAULT 0,
  `duty_start` datetime DEFAULT NULL,
  `duty_offset` int(11) DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Daten für Tabelle `accounts`
--

INSERT INTO `accounts` (`id`, `password`, `name`, `adminlevel`, `geld`, `hardware_id`, `is_banned`, `ban_reason`, `characterdata`, `fraktion`, `duty_start`, `duty_offset`) VALUES
(3, '$2a$11$VcQn4i.cgwU7qSf.gPn5wee8sj0uFUB.uurKky/Ch4d/E/2lQNFoq', 'WeirdNewbie', 0, 5125, 'D7C2706C29FED698EE8C42BC7D3E85B05226997C9B7A378063A2775887C4B2C0C5829B0C376CFD780B361DAC905A1BF0DA769AD83ECE87F893BE86D4572E0800', 0, NULL, '', 2, '2025-05-22 12:56:38', 0),
(4, '$2a$11$Lxqu/dh3mPp2rllGzAOYYOuwQzhyDZYSPogNY/NNaxcBgr2MAbRE6', 'Michael Babinski', 5, 510625, 'D8903A045B585100FBEC42648F543BB075F41E8444A481C889F018C8DD229CE00026BAD81FA2EA48E3120E38E958EA50A31208A056B6E9E06F0051886E4E82C0', 0, '', '', 1, '2025-05-23 14:40:08', 0);

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
(2, 'LSMD', 'good', 0, 3, '2025-05-19 15:42:55'),
(3, 'LSCS', 'good', 0, 66, '2025-05-19 15:45:00');

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
(2, 3, 2, 12, 8000, '2025-05-19 15:42:55');

-- --------------------------------------------------------

--
-- Tabellenstruktur für Tabelle `haus`
--

CREATE TABLE `haus` (
  `id` int(11) NOT NULL,
  `ipl` varchar(100) NOT NULL,
  `posX` float NOT NULL,
  `posY` float NOT NULL,
  `posZ` float NOT NULL,
  `preis` int(64) NOT NULL,
  `besitzer` varchar(50) NOT NULL,
  `status` int(1) NOT NULL DEFAULT 0,
  `abgeschlossen` int(3) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Daten für Tabelle `haus`
--

INSERT INTO `haus` (`id`, `ipl`, `posX`, `posY`, `posZ`, `preis`, `besitzer`, `status`, `abgeschlossen`) VALUES
(4, 'apa_v_mp_h_02_c', 213.529, -569.237, 43.8679, 15000, 'Keiner', 0, 1),
(5, 'apa_v_mp_h_01_c', 212.26, -593.488, 43.8679, 1, 'Michael Babinski', 1, 1),
(6, 'apa_v_mp_h_03_b', 207.67, -591.841, 43.8679, 50000, 'Michael Babinski', 1, 1);

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
(3, 314.985, -593.424, 43.2654, -16896, 100, 90, 0, 346000, '2025-05-22 11:39:20', 0),
(4, 754.914, -1057.47, 21.692, 29590, 19, 0, 0, 538000, '2025-05-23 12:45:26', 28800);

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
  `color_secondary` int(11) DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Daten für Tabelle `vehsafe`
--

INSERT INTO `vehsafe` (`id`, `owner`, `is_faction_vehicle`, `faction_id`, `model_name`, `number_plate`, `modifications`, `pos_x`, `pos_y`, `pos_z`, `heading`, `color_primary`, `color_secondary`) VALUES
(1, 4, 0, 0, 'Tempesta', 'OG', '{\"color1\":55,\"color2\":55}', 458.289, -657.087, 26.99, 50.0737, 55, 55),
(2, 4, 0, 0, 'polndom', 'CHIEF_02', '{}', 407.855, -989.02, 28.5871, -124.716, 0, 0),
(3, 4, 0, 0, 'polcara6x6', 'CHIEF_01', '{}', 251.385, -569.461, 42.4771, 158.174, 0, 0),
(4, 0, 1, 2, 'fbi', 'MD1', '', 287.263, -615.643, 43.0802, 68.8905, 39, 39),
(5, 0, 1, 2, 'ambulance', 'MD2', '', 294.967, -607.583, 43.1149, 69.9583, 111, 111),
(6, 0, 1, 2, 'ambulance', 'MD3', '', 293.753, -611.342, 43.1462, 70.3516, 111, 111),
(7, 0, 1, 2, 'ambulance', 'MD4', '', 285.416, -611.707, 43.1211, 68.2235, 111, 111),
(8, 0, 1, 2, 'lguard', 'MD4', '', 321.157, -542.081, 28.3731, -0.00000353313, 28, 28),
(9, 0, 1, 2, 'lguard', 'MD5', '', 326.682, -542.268, 28.3731, 0.703156, 28, 28),
(10, 0, 1, 2, 'ambulance', 'MD6', '', 332.277, -542.33, 28.5082, 0.000192039, 111, 111),
(11, 0, 1, 1, 'police3', 'LSPD1', '', 454.505, -1023.28, 28.1853, -0.557997, 134, 134),
(12, 0, 1, 1, 'police3', 'LSPD2', '', 450.428, -1024.2, 28.2837, 0.588592, 134, 134),
(13, 0, 1, 1, 'police3', 'LSPD3', '', 446.945, -1024.32, 28.3446, 1.62291, 134, 134),
(14, 0, 1, 1, 'police3', 'LSPD4', '', 443.093, -1024.53, 28.4126, 0.90424, 134, 134),
(15, 0, 1, 1, 'police3', 'LSPD5', '', 408.414, -982.307, 29.021, -124.063, 134, 134),
(16, 0, 1, 1, 'riot', 'LSPDSF', '', 426.801, -1028.36, 28.6253, 91.9207, 1, 1),
(17, 0, 1, 1, 'riot2', 'LSPDSF', '', 418.653, -1029.36, 28.9998, 92.666, 1, 1),
(18, 0, 1, 1, 'polmav', 'LSPDMAV', '', 449.329, -980.059, 44.0831, 3.59979, 134, 0),
(19, 0, 1, 1, 'fbi', 'LSPD6', '', 432.017, -1027.82, 28.5435, -12.8027, 0, 0),
(20, 0, 1, 1, 'fbi', 'LSPD7', '', 435.617, -1025.71, 28.4545, 0.267887, 0, 0),
(21, 0, 1, 3, 'towtruck', 'LSCS1', '', -157.662, -1306.07, 31.2546, 29.2227, 41, 41),
(22, 0, 1, 3, 'towtruck', 'LSCS2', '', -163.684, -1306.14, 31.2546, 29.1401, 41, 41),
(23, 0, 1, 3, 'towtruck', 'LSCS3', '', -170.353, -1306.69, 31.2619, 28.8961, 41, 41),
(24, 0, 1, 3, 'cargobob', 'LSCS4', '', -220.953, -1324.58, 40.4045, 56.9284, 41, 41),
(25, 0, 1, 3, 'bison', 'LSCS5', '', -222.304, -1309.84, 30.8281, -132.145, 41, 41);

--
-- Indizes der exportierten Tabellen
--

--
-- Indizes für die Tabelle `accounts`
--
ALTER TABLE `accounts`
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
-- Indizes für die Tabelle `haus`
--
ALTER TABLE `haus`
  ADD PRIMARY KEY (`id`);

--
-- Indizes für die Tabelle `player_tracking`
--
ALTER TABLE `player_tracking`
  ADD PRIMARY KEY (`account_id`),
  ADD KEY `idx_account_id` (`account_id`);

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
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT für Tabelle `fraktion`
--
ALTER TABLE `fraktion`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT für Tabelle `fraktionsmitglieder`
--
ALTER TABLE `fraktionsmitglieder`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT für Tabelle `haus`
--
ALTER TABLE `haus`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT für Tabelle `vehsafe`
--
ALTER TABLE `vehsafe`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=26;

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
-- Constraints der Tabelle `player_tracking`
--
ALTER TABLE `player_tracking`
  ADD CONSTRAINT `player_tracking_ibfk_1` FOREIGN KEY (`account_id`) REFERENCES `accounts` (`id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
