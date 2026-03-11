using System;

namespace RPCore.Models.Faction
{
    /// <summary>
    /// Faction Rank Model - Repräsentiert einen Rang innerhalb einer Fraktion
    /// Jede Fraktion hat eigene Ränge mit verschiedenen Berechtigungen
    /// </summary>
    public class FactionRank
    {
        public int Id { get; set; }
        public int FactionId { get; set; }
        public int Level { get; set; }              // 0 = niedrigster Rang, höher = höherer Rang
        public string Name { get; set; }            // z.B. "Rekrut", "Officer", "Boss"
        public int Salary { get; set; }             // Gehalt pro Payday

        // Berechtigungen
        public bool CanInvite { get; set; }         // Kann Mitglieder einladen
        public bool CanKick { get; set; }           // Kann Mitglieder entfernen
        public bool CanPromote { get; set; }        // Kann Beförderungen vergeben
        public bool CanManageRanks { get; set; }    // Kann Ränge erstellen/bearbeiten
        public bool CanAccessBank { get; set; }     // Zugriff auf Fraktionskasse
        public bool CanWithdrawMoney { get; set; }  // Kann Geld abheben

        public DateTime CreatedAt { get; set; }

        public FactionRank()
        {
            Name = string.Empty;
            Level = 0;
            Salary = 0;
            CanInvite = false;
            CanKick = false;
            CanPromote = false;
            CanManageRanks = false;
            CanAccessBank = false;
            CanWithdrawMoney = false;
            CreatedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// Faction Member - Verknüpfung zwischen Character und Faction mit Rang
    /// </summary>
    public class FactionMember
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public int FactionId { get; set; }
        public int RankId { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsOnDuty { get; set; }
        public string InvitedBy { get; set; }
        public int DutyMinutes { get; set; }        // Gearbeitete Minuten

        public FactionMember()
        {
            InvitedBy = string.Empty;
            JoinedAt = DateTime.Now;
            IsOnDuty = false;
            DutyMinutes = 0;
        }
    }
}
