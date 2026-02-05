using System;

namespace RPCore.Models.Account
{
    /// <summary>
    /// Account Model - Repräsentiert die Login-Daten eines Spielers
    /// Ein Account kann mehrere Charaktere haben
    /// </summary>
    public class Account
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsBanned { get; set; }
        public string BanReason { get; set; }
        public DateTime? BanExpiry { get; set; }
        public string HardwareId { get; set; }
        public string SocialClubName { get; set; }

        public Account()
        {
            CreatedAt = DateTime.Now;
            LastLogin = DateTime.Now;
            IsBanned = false;
        }
    }
}
