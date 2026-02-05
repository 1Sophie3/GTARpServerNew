namespace RPCore.Models.Faction
{
    /// <summary>
    /// Fraktionstypen - Kategorisierung der verschiedenen Fraktionen
    /// </summary>
    public enum FactionType
    {
        // Staatsfraktionen
        Polizei = 1,
        Medic = 2,
        FIB = 3,
        LSCS = 4,           // Los Santos County Sheriff
        ArmyNG = 5,         // Army/National Guard
        Regierung = 6,
        DoJ = 7,            // Department of Justice

        // Kriminelle Fraktionen
        Gang = 100,
        Mafia = 101,
        MC = 102,           // Motorcycle Club

        // Neutrale/Sonstige
        Zivilfirma = 200,   // Z.B. Taxi-Unternehmen
        NewsAgency = 201,
        Sonstige = 999
    }

    /// <summary>
    /// Hilfklasse zum Kategorisieren von Fraktionen
    /// </summary>
    public static class FactionTypeHelper
    {
        public static bool IsStateFaction(FactionType type)
        {
            return (int)type >= 1 && (int)type < 100;
        }

        public static bool IsCriminalFaction(FactionType type)
        {
            return (int)type >= 100 && (int)type < 200;
        }

        public static bool IsNeutralFaction(FactionType type)
        {
            return (int)type >= 200 && (int)type < 999;
        }

        public static string GetTypeName(FactionType type)
        {
            if (IsStateFaction(type)) return "Staatsfraktion";
            if (IsCriminalFaction(type)) return "Kriminelle Organisation";
            if (IsNeutralFaction(type)) return "Zivile Organisation";
            return "Sonstige";
        }
    }
}
