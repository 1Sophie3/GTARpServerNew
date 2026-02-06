using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using gamemode.Haussystem;
using System.Security.Principal;
using gamemode.CharCreator;

namespace gamemode.Datenbank
{
    class Accounts
    {
        public enum AdminRanks { Spieler, TestGamemaster, GameMaster, Eventmanager, Admin, Owner };
        public const string Account_Key = "Account_Data";

        public int ID { get; set; }
        public string Name { get; set; }
        public short Adminlevel { get; set; }
        public int Geld { get; set; }
        public int last_health { get; set; }
        public int last_armor { get; set; }
        public int played_time { get; set; }
        public Vector3 Position { get; set; }
        public DateTime? DutyStart { get; set; }
        public int DutyOffset { get; set; }
        public int Fraktion { get; set; }
        public Player _player;
        public DateTime loginTimestamp { get; set; }
        public int storedPlaytime { get; set; }
        public string CharacterData { get; set; }

        public Accounts()
        {
            Name = "";
            Adminlevel = 0;
            Geld = 5000;
            last_health = 100;
            last_armor = 0;
            played_time = 0;
            Position = new Vector3(0, 0, 0);
            CharacterData = "";
            Fraktion = 0;
        }

        public Accounts(string name, Player player)
        {
            Name = name;
            _player = player;
            Adminlevel = 0;
            Geld = 5000;
            last_health = 100;
            last_armor = 0;
            played_time = 0;
            Position = new Vector3(0, 0, 0);
            CharacterData = "";
            Fraktion = 0;
        }

        public static bool IstSpielerEingeloggt(Player player)
        {
            return player != null && player.HasData(Account_Key);
        }

        public void Register(string name, string password)
        {
            Datenbank.NeuenAccountErstellen(this, password);
            Login(_player, true);

        }

        public void Login(Player player, bool firstlogin)
        {
            Datenbank.AccountLaden(this);


            if (firstlogin)
            {
                player.SendNotification("Willkommen! Dein Account wurde erfolgreich erstellt.");
                player.TriggerEvent("freezePlayer", true);

                if (CharacterData.Length <= 0)
                {
                    player.TriggerEvent("charcreator-show");
                }

            }
            else
            {
                if (CharacterData.Length <= 0)
                {
                    player.TriggerEvent("charcreator-show");
                }
                else 
                { 
                    player.SendNotification($"Willkommen zurück, {Name}!");
                    player.SetData(Account_Key, this);
                    player.SetData("faction", this.Fraktion);

                    //CharCreatorEvents.OnCharacterCreated(player, CharacterData, false);
                    CharCreatorEvents.ApplyCustomization(player, CharacterData);

                    LadeSpielerPositionAusDB(player, this);
                }
            }
        }

        public void LadeSpielerPositionAusDB(Player player, Accounts account)
        {
            try
            {
                using (MySqlCommand command = Datenbank.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT position_x, position_y, position_z
                        FROM player_tracking
                        WHERE account_id = @id
                        LIMIT 1";
                    command.Parameters.AddWithValue("@id", account.ID);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows && reader.Read())
                        {
                            float posX = reader.GetFloat("position_x");
                            float posY = reader.GetFloat("position_y");
                            float posZ = reader.GetFloat("position_z");
                            player.Position = new Vector3(posX, posY, posZ);
                            NAPI.Util.ConsoleOutput($"[INFO] Spieler {account.Name} geladen. Position: ({posX}, {posY}, {posZ})");
                        }
                        else
                        {
                            player.Position = new Vector3(276.828f, -580.471f, 43.113f);
                            NAPI.Util.ConsoleOutput($"[WARNUNG] Keine gespeicherte Position für {account.Name}. Standardposition wird gesetzt.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Laden der Position für {account.Name}: {ex.Message}");
            }
        }

        public bool IstSpielerAdmin(int requiredAdminlevel)
        {
            return Adminlevel >= requiredAdminlevel;
        }

        //public void LadeSpielerCharakterData(Player player, Accounts account)
        //{
        //    try
        //    {
        //        using (MySqlCommand command = Datenbank.CreateCommand())
        //        {
        //            command.CommandText = @"
        //                SELECT characterdata
        //                FROM accounts
        //                WHERE account_id = @id
        //                LIMIT 1";
        //            command.Parameters.AddWithValue("@id", account.ID);
        //            using (MySqlDataReader reader = command.ExecuteReader())
        //            {
        //                if (reader.HasRows && reader.Read())
        //                {
        //                    reader.GetString("CharacterData");

        //                    NAPI.Util.ConsoleOutput($"[INFO] Spieler {account.Name} geladen. )");
        //                }
        //                else
        //                {

        //                    NAPI.Util.ConsoleOutput($"[WARNUNG] Keine gespeicherte Position für {account.Name}. Standardposition wird gesetzt.");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Laden des Characters für {account.Name}: {ex.Message}");
        //    }
        //}
    }
}
