using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using RPCore.Haussystem;
using System.Security.Principal;
using RPCore.CharCreator;
using Newtonsoft.Json; // Hinzugefügt für die GetFullName-Methode

namespace RPCore.Database
{
    // Diese Klasse beschreibt die Struktur deiner JSON-Daten in der 'characterdata'-Spalte.
    public class CharacterData
    {
        [JsonProperty("firstname")]
        public string Firstname { get; set; }

        [JsonProperty("lastname")]
        public string Lastname { get; set; }

        [JsonProperty("birth")]
        public string Birth { get; set; }

        [JsonProperty("origin")]
        public string Origin { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }
    }

    public class Accounts
    {
        public enum AdminRanks { Spieler, TestGamemaster, GameMaster, Eventmanager, Admin, Owner };
        public const string Account_Key = "Account_Data";

        // Eigenschaften des Accounts
        public int ID { get; set; }
        public ulong SocialClubId { get; set; }
        public string Name { get; set; }
        public short Adminlevel { get; set; }
        public int Geld { get; set; }
        public string Kontonummer { get; set; }
        public int last_health { get; set; }
        public int last_armor { get; set; }
        public int played_time { get; set; }
        public Vector3 Position { get; set; }
        public int Fraktion { get; set; }

        // Duty-Eigenschaften
        public DateTime? DutyStart { get; set; }
        public int DutyOffset { get; set; }

        // NEU HINZUGEFÜGT, UM DEN FEHLER ZU BEHEBEN
        // Diese Eigenschaft wurde von deinem Code in anderen Dateien benötigt.
        public DateTime? LastLogoutDuty { get; set; }

        // Sonstige Eigenschaften
        public Player _player;
        public DateTime loginTimestamp { get; set; }
        public int storedPlaytime { get; set; }
        public string CharacterData { get; set; }
        public string Licenses { get; set; }
        public bool Seatbelt { get; set; }
        public DateTime CreationDate { get; set; }

        // Konstruktor: Setzt die Standardwerte, wenn ein neues Account-Objekt erstellt wird.
        public Accounts()
        {
            ID = 0;
            SocialClubId = 0;
            Name = "";
            Adminlevel = 0;
            Geld = 5000;
            Kontonummer = "";
            last_health = 100;
            last_armor = 0;
            played_time = 0;
            Position = new Vector3(0, 0, 0);
            CharacterData = "";
            Licenses = "";
            Fraktion = 0;
            Seatbelt = false;
            _player = null;
            CreationDate = DateTime.Now;

            // Die neuen, nullable DateTime-Eigenschaften müssen hier nicht explizit auf null gesetzt werden,
            // da dies ihr Standardwert ist.
        }
      
        public static bool IstSpielerEingeloggt(Player player)
        {
            return player != null && player.HasData(Account_Key);
        }

        public void Register(string name, string password)
        {
            this.Name = name;
            Datenbank.NeuenAccountErstellen(this, password);
            Login(_player, true);
        }

        public void Login(Player player, bool firstlogin)
        {
            _player = player;

            if (string.IsNullOrEmpty(this.Name))
                this.Name = player.Name;

            if (this.SocialClubId == 0)
                this.SocialClubId = player.SocialClubId;

            bool success = Datenbank.AccountLaden(this);
            NAPI.Util.ConsoleOutput($"[DEBUG] Login: Account {this.Name} (SCID: {this.SocialClubId}) geladen, Adminlevel: {this.Adminlevel}");
            if (!success)
            {
                NAPI.Util.ConsoleOutput($"[ERROR] Account konnte nicht geladen werden: {this.Name} (SCID: {this.SocialClubId})");
                return;
            }

            NAPI.Task.Run(() =>
            {
                player.TriggerEvent("SetAdminRank", this.Adminlevel);
                NAPI.Util.ConsoleOutput($"[DEBUG] TriggerEvent 'SetAdminRank' gesendet mit Wert: {this.Adminlevel}");
            }, 3000);

            if (firstlogin)
            {
                player.SendNotification("Willkommen! Dein Account wurde erfolgreich erstellt.");
                player.TriggerEvent("freezePlayer", true);
                if (string.IsNullOrEmpty(CharacterData) || CharacterData.Length <= 0)
                {
                    player.TriggerEvent("charcreator-show");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(CharacterData) || CharacterData.Length <= 0)
                {
                    player.TriggerEvent("charcreator-show");
                }

                player.SetData(Account_Key, this);
                player.SetData("faction", this.Fraktion);
                CharCreatorEvents.ApplyCustomization(player, CharacterData);

                LadeSpielerPositionAusDB(player, this);
            }
        }

        public void LadeSpielerPositionAusDB(Player player, Accounts account)
        {
            try
            {
                string connectionString = Datenbank.GetConnectionString();

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    using (MySqlCommand command = connection.CreateCommand())
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
                                player.Position = new Vector3(276.828f, -580.471f, 43.113f); // Standardposition
                                NAPI.Util.ConsoleOutput($"[WARNUNG] Keine gespeicherte Position für {account.Name}. Standardposition wird gesetzt.");
                            }
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

        /// <summary>
        /// Gibt den vollständigen Charakternamen (Vorname Nachname) zurück.
        /// Liest die Daten aus dem CharacterData JSON.
        /// Falls keine Charakterdaten vorhanden sind, wird der Account-Name als Fallback genutzt.
        /// </summary>
        /// <returns>Den vollständigen Namen als String.</returns>
        public string GetFullName()
        {
            if (string.IsNullOrWhiteSpace(this.CharacterData))
            {
                return this.Name;
            }

            try
            {
                CharacterData data = JsonConvert.DeserializeObject<CharacterData>(this.CharacterData);

                if (data != null && !string.IsNullOrWhiteSpace(data.Firstname) && !string.IsNullOrWhiteSpace(data.Lastname))
                {
                    return $"{data.Firstname} {data.Lastname}";
                }
            }
            catch (JsonException)
            {
                return this.Name;
            }

            return this.Name;
        }
    }
}