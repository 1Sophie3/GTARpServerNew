using System;
using System.Collections.Generic;

namespace RPCore.Models.Faction
{
    /// <summary>
    /// Faction Model - Repräsentiert eine Fraktion/Organisation
    /// </summary>
    public class Faction
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }       // z.B. "LSPD", "Vagos"
        public FactionType Type { get; set; }

        // Finanzen & Ressourcen
        public int BankBalance { get; set; }

        // Farben (für UI, Blips, etc.)
        public string PrimaryColor { get; set; }    // Hex Color Code
        public string SecondaryColor { get; set; }

        // Status
        public bool IsActive { get; set; }
        public int MaxMembers { get; set; }

        // Ränge & Hierarchie
        public List<FactionRank> Ranks { get; set; }

        // Zeiten
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        public Faction()
        {
            Name = string.Empty;
            ShortName = string.Empty;
            PrimaryColor = "#FFFFFF";
            SecondaryColor = "#000000";
            CreatedBy = string.Empty;
            IsActive = true;
            MaxMembers = 50;
            BankBalance = 0;
            Ranks = new List<FactionRank>();
            CreatedAt = DateTime.Now;
        }

        // Hilfsmethoden
        public bool IsStateFaction()
        {
            return FactionTypeHelper.IsStateFaction(Type);
        }

        public bool IsCriminalFaction()
        {
            return FactionTypeHelper.IsCriminalFaction(Type);
        }

        public string GetTypeDescription()
        {
            return FactionTypeHelper.GetTypeName(Type);
        }
    }
}
