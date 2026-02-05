using System;
using GTANetworkAPI;

namespace RPCore.Models.Account
{
    /// <summary>
    /// Account Model - Repräsentiert die Login-Daten UND den Charakter eines Spielers
    /// HINWEIS: 1 Charakter pro Account - Charakter-Daten sind hier integriert
    /// </summary>
    public class Account
    {
        // === Account/Login Daten ===
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string HardwareId { get; set; }
        public string SocialClubName { get; set; }

        // === Charakter Daten (1 Charakter pro Account) ===
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        // === Finanzen ===
        public int Cash { get; set; }           // Bargeld im Inventar
        public int BankMoney { get; set; }      // Bankguthaben

        // === Level & Erfahrung ===
        public int Level { get; set; }
        public int Experience { get; set; }
        public int PlayTimeMinutes { get; set; }

        // === Job & Fraktion ===
        public string Job { get; set; }
        public int? FactionId { get; set; }
        public int FactionRank { get; set; }

        // === Position ===
        public Vector3 LastPosition { get; set; }
        public float LastRotation { get; set; }
        public int Dimension { get; set; }

        // === Gesundheit & Status ===
        public int Health { get; set; }
        public int Armor { get; set; }
        public bool IsAlive { get; set; }
        public bool IsInjured { get; set; }

        // === Aussehen ===
        public string Appearance { get; set; }  // JSON für Gesichts-Customization

        // === Admin & Ban Status ===
        public int AdminLevel { get; set; }
        public bool IsBanned { get; set; }
        public string BanReason { get; set; }
        public DateTime? BanExpiry { get; set; }

        // === Zeitstempel ===
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }

        public Account()
        {
            // Account Defaults
            Username = string.Empty;
            // Email wurde entfernt (Charakter/Account speichert keine E-Mail mehr)
            PasswordHash = string.Empty;
            HardwareId = string.Empty;
            SocialClubName = string.Empty;

            // Charakter Defaults
            FirstName = string.Empty;
            LastName = string.Empty;
            Cash = 500;
            BankMoney = 5000;
            Level = 1;
            Experience = 0;
            PlayTimeMinutes = 0;

            // Job & Fraktion
            Job = "Arbeitslos";
            FactionId = null;
            FactionRank = 0;

            // Position (LSIA Spawn)
            LastPosition = new Vector3(-1037.7f, -2738.5f, 13.8f);
            LastRotation = 0f;
            Dimension = 0;

            // Status
            Health = 100;
            Armor = 0;
            IsAlive = true;
            IsInjured = false;
            Appearance = string.Empty;

            // Admin
            AdminLevel = 0;
            IsBanned = false;
            BanReason = string.Empty;
            BanExpiry = null;

            // Zeitstempel
            CreatedAt = DateTime.Now;
            LastLogin = DateTime.Now;
        }

        // === Hilfsmethoden ===

        /// <summary>
        /// Prüft ob der Spieler in einer Fraktion ist
        /// </summary>
        public bool IsInFaction()
        {
            return FactionId.HasValue && FactionId.Value > 0;
        }

        /// <summary>
        /// Prüft ob der Spieler ein Zivilist ist (kein Fraktionsmitglied)
        /// </summary>
        public bool IsCivilian()
        {
            return !IsInFaction();
        }

        /// <summary>
        /// Prüft ob der Spieler Admin-Rechte hat
        /// </summary>
        public bool IsAdmin()
        {
            return AdminLevel > 0;
        }

        /// <summary>
        /// Prüft ob der Spieler ein bestimmtes Admin-Level hat
        /// </summary>
        public bool HasAdminLevel(int requiredLevel)
        {
            return AdminLevel >= requiredLevel;
        }

        /// <summary>
        /// Prüft ob der Account aktuell gebannt ist
        /// </summary>
        public bool IsCurrentlyBanned()
        {
            if (!IsBanned) return false;
            if (BanExpiry.HasValue && BanExpiry.Value < DateTime.Now)
            {
                // Ban ist abgelaufen
                IsBanned = false;
                BanReason = string.Empty;
                BanExpiry = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gibt Geld zum Bargeld hinzu
        /// </summary>
        public bool AddCash(int amount)
        {
            if (amount < 0) return false;
            Cash += amount;
            return true;
        }

        /// <summary>
        /// Zieht Bargeld ab
        /// </summary>
        public bool RemoveCash(int amount)
        {
            if (amount < 0 || Cash < amount) return false;
            Cash -= amount;
            return true;
        }

        /// <summary>
        /// Gibt Geld zur Bank hinzu
        /// </summary>
        public bool AddBankMoney(int amount)
        {
            if (amount < 0) return false;
            BankMoney += amount;
            return true;
        }

        /// <summary>
        /// Zieht Geld von der Bank ab
        /// </summary>
        public bool RemoveBankMoney(int amount)
        {
            if (amount < 0 || BankMoney < amount) return false;
            BankMoney -= amount;
            return true;
        }
    }
}
