using System;
using GTANetworkAPI;

namespace RPCore.Models.Character
{
    /// <summary>
    /// Character Model - Repräsentiert einen IC (In-Character) Charakter
    /// Ein Account kann mehrere Charaktere haben
    /// </summary>
    public class Character
    {
        // Primäre Daten
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        // Finanzen
        public int Cash { get; set; }           // Bargeld im Inventar
        public int BankBalance { get; set; }     // Bankguthaben

        // Level & Erfahrung
        public int Level { get; set; }
        public int Experience { get; set; }
        public int PlaytimeMinutes { get; set; }

        // Position & Aussehen
        public Vector3 LastPosition { get; set; }
        public Vector3 LastRotation { get; set; }
        public int Dimension { get; set; }
        public string Appearance { get; set; }  // JSON für Gesichts-Customization

        // Gesundheit & Status
        public int Health { get; set; }
        public int Armor { get; set; }
        public bool IsAlive { get; set; }
        public bool IsInjured { get; set; }

        // Fraktion & Job
        public int? FactionId { get; set; }      // Null wenn Zivilist
        public int FactionRank { get; set; }
        public string Job { get; set; }          // Zivilist-Job (Taxi, Müllmann, etc.)

        // Zeitstempel
        public DateTime CreatedAt { get; set; }
        public DateTime LastPlayed { get; set; }

        public Character()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Cash = 500;
            BankBalance = 5000;
            Level = 1;
            Experience = 0;
            PlaytimeMinutes = 0;
            Health = 100;
            Armor = 0;
            IsAlive = true;
            IsInjured = false;
            Job = "Arbeitslos";
            FactionRank = 0;
            Dimension = 0;
            Appearance = string.Empty;
            LastPosition = new Vector3(-1037.7f, -2738.5f, 13.8f); // LSIA Spawn
            LastRotation = new Vector3(0, 0, 0);
            CreatedAt = DateTime.Now;
            LastPlayed = DateTime.Now;
        }

        // Hilfsmethoden
        public bool IsInFaction()
        {
            return FactionId.HasValue && FactionId.Value > 0;
        }

        public bool IsCivilian()
        {
            return !IsInFaction();
        }
    }
}
