using System;
using GTANetworkAPI;

namespace RPCore.Player
{
    public class PlayerData
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int Money { get; set; }
        public int BankMoney { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public string Job { get; set; }
        public Vector3 LastPosition { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }

        public PlayerData()
        {
            Money = 5000; // Startgeld
            BankMoney = 0;
            Level = 1;
            Experience = 0;
            Job = "Arbeitslos";
            LastPosition = new Vector3(-1037.7f, -2738.5f, 13.8f); // LSIA
            CreatedAt = DateTime.Now;
            LastLogin = DateTime.Now;
        }
    }

    public static class PlayerExtensions
    {
        // Extension für RAGE Player Objekt
        public static void SetPlayerData(this GTANetworkAPI.Player player, PlayerData data)
        {
            player.SetData("PlayerData", data);
        }

        public static PlayerData GetPlayerData(this GTANetworkAPI.Player player)
        {
            return player.GetData<PlayerData>("PlayerData");
        }
    }
}
