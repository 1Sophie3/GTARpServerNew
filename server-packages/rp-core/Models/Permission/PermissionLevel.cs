namespace RPCore.Models.Permission
{
    /// <summary>
    /// OOC Permission Level - Administrationsrechte außerhalb des RP
    /// </summary>
    public enum PermissionLevel
    {
        Spieler = 0,        // Normaler Spieler ohne Rechte
        Supporter = 1,      // Kann Support-Tickets beantworten, kleine Befehle
        Moderator = 2,      // Kann kicken, temporäre Bans, Verwarnungen
        Administrator = 3,  // Kann permanent bannen, größere Rechte
        HeadAdmin = 4,      // Leitung des Admin-Teams
        Projektleitung = 5, // Management
        Owner = 6           // Vollzugriff auf alles
    }

    /// <summary>
    /// Player Permission Model - Verknüpft einen Account mit seinem Permission Level
    /// </summary>
    public class PlayerPermission
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public PermissionLevel Level { get; set; }
        public string GrantedBy { get; set; }  // Wer hat die Rechte gegeben
        public System.DateTime GrantedAt { get; set; }

        public PlayerPermission()
        {
            Level = PermissionLevel.Spieler;
            GrantedAt = System.DateTime.Now;
        }

        // Hilfsmethoden zum Überprüfen von Rechten
        public bool HasPermission(PermissionLevel requiredLevel)
        {
            return Level >= requiredLevel;
        }

        public bool IsAdmin()
        {
            return Level >= PermissionLevel.Administrator;
        }

        public bool IsStaff()
        {
            return Level >= PermissionLevel.Supporter;
        }
    }
}
