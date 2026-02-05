using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using RPCore.Models.Character;
using RPCore.Database;
using MySql.Data.MySqlClient;

namespace RPCore.Managers
{
    /// <summary>
    /// [VERALTET/DEPRECATED] Character Manager
    /// 
    /// HINWEIS: Mit dem neuen System gibt es nur noch 1 Charakter pro Account.
    /// Alle Charakter-Daten sind jetzt im Account integriert.
    /// 
    /// Bitte nutze stattdessen: AccountManager
    /// - AccountManager.Instance.LoadAccount(username) - lädt Account inkl. Charakter
    /// - AccountManager.Instance.SaveAccount(account) - speichert Account inkl. Charakter
    /// - AccountManager.Instance.GetAccountByPlayer(player) - holt Account für Spieler
    /// 
    /// Diese Klasse bleibt für Abwärtskompatibilität erhalten.
    /// </summary>
    [Obsolete("Nutze AccountManager - 1 Charakter pro Account. Charakter-Daten sind im Account integriert.")]
    public class CharacterManager
    {
        private static CharacterManager? _instance;
        public static CharacterManager Instance => _instance ??= new CharacterManager();

        // Player -> Character Mapping
        private Dictionary<GTANetworkAPI.Player, Character> _playerCharacters;

        // Cache für geladene Characters
        private Dictionary<int, Character> _loadedCharacters;

        private CharacterManager()
        {
            _playerCharacters = new Dictionary<GTANetworkAPI.Player, Character>();
            _loadedCharacters = new Dictionary<int, Character>();
        }

        /// <summary>
        /// [VERALTET] Lädt alle Characters eines Accounts aus der Datenbank
        /// HINWEIS: Mit dem neuen System gibt es nur 1 Charakter pro Account,
        /// der direkt im Account gespeichert ist. Nutze AccountManager.LoadAccount()
        /// </summary>
        [Obsolete("Nutze AccountManager.LoadAccount() - 1 Charakter pro Account")]
        public async Task<List<Character>> LoadCharactersByAccount(int accountId)
        {
            var characters = new List<Character>();

            // Legacy-Unterstützung für alte Datenbank
            string query = "SELECT * FROM characters WHERE account_id = @accountId";
            var reader = await DatabaseManager.Instance.ExecuteReader(query,
                DatabaseManager.CreateParameter("@accountId", accountId));

            if (reader == null) return characters;

            while (await reader.ReadAsync())
            {
                var character = new Character
                {
                    Id = reader.GetInt32("id"),
                    AccountId = reader.GetInt32("account_id"),
                    FirstName = reader.GetString("first_name"),
                    LastName = reader.GetString("last_name"),
                    Cash = reader.GetInt32("money"),
                    BankBalance = reader.GetInt32("bank_money"),
                    Level = reader.GetInt32("level"),
                    Experience = reader.GetInt32("experience"),
                    PlaytimeMinutes = reader.GetInt32("play_time"),
                    Health = reader.GetInt32("health"),
                    Armor = reader.GetInt32("armor"),
                    LastPosition = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z")),
                    LastRotation = new Vector3(0, 0, reader.GetFloat("rotation")),
                    FactionId = reader.IsDBNull(reader.GetOrdinal("faction_id")) ? null : reader.GetInt32("faction_id"),
                    FactionRank = reader.GetInt32("faction_rank"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    LastPlayed = reader.IsDBNull(reader.GetOrdinal("last_played")) ? DateTime.Now : reader.GetDateTime("last_played")
                };

                characters.Add(character);
                _loadedCharacters[character.Id] = character;
            }

            reader.Close();
            NAPI.Util.ConsoleOutput($"[CharacterManager] [LEGACY] {characters.Count} Character(e) für Account {accountId} geladen");
            return characters;
        }

        /// <summary>
        /// [VERALTET] Lädt einen einzelnen Character aus der Datenbank
        /// </summary>
        [Obsolete("Nutze AccountManager.LoadAccount() - 1 Charakter pro Account")]
        public async Task<Character?> LoadCharacter(int characterId)
        {
            // Cache-Check
            if (_loadedCharacters.ContainsKey(characterId))
            {
                return _loadedCharacters[characterId];
            }

            string query = "SELECT * FROM characters WHERE id = @charId LIMIT 1";
            var reader = await DatabaseManager.Instance.ExecuteReader(query,
                DatabaseManager.CreateParameter("@charId", characterId));

            if (reader == null || !await reader.ReadAsync())
            {
                reader?.Close();
                return null;
            }

            var character = new Character
            {
                Id = reader.GetInt32("id"),
                AccountId = reader.GetInt32("account_id"),
                FirstName = reader.GetString("first_name"),
                LastName = reader.GetString("last_name"),
                Cash = reader.GetInt32("money"),
                BankBalance = reader.GetInt32("bank_money"),
                Level = reader.GetInt32("level"),
                Experience = reader.GetInt32("experience"),
                PlaytimeMinutes = reader.GetInt32("play_time"),
                Health = reader.GetInt32("health"),
                Armor = reader.GetInt32("armor"),
                LastPosition = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z")),
                LastRotation = new Vector3(0, 0, reader.GetFloat("rotation")),
                FactionId = reader.IsDBNull(reader.GetOrdinal("faction_id")) ? null : reader.GetInt32("faction_id"),
                FactionRank = reader.GetInt32("faction_rank"),
                CreatedAt = reader.GetDateTime("created_at"),
                LastPlayed = reader.IsDBNull(reader.GetOrdinal("last_played")) ? DateTime.Now : reader.GetDateTime("last_played")
            };

            reader.Close();
            _loadedCharacters[characterId] = character;
            return character;
        }

        /// <summary>
        /// Erstellt einen neuen Character
        /// </summary>
        public async Task<Character?> CreateCharacter(int accountId, string firstName, string lastName)
        {
            // Name-Validation
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                NAPI.Util.ConsoleOutput("[CharacterManager] Ungültiger Name!");
                return null;
            }

            // Prüfe ob Name bereits existiert
            if (await CharacterNameExists(firstName, lastName))
            {
                NAPI.Util.ConsoleOutput($"[CharacterManager] Name {firstName} {lastName} existiert bereits!");
                return null;
            }

            string query = "INSERT INTO characters (account_id, first_name, last_name, money, bank_money, health, pos_x, pos_y, pos_z, created_at) " +
                          "VALUES (@accountId, @firstName, @lastName, 500, 5000, 100, -1037.7, -2738.5, 13.8, @created)";

            long charId = await DatabaseManager.Instance.ExecuteInsert(query,
                DatabaseManager.CreateParameter("@accountId", accountId),
                DatabaseManager.CreateParameter("@firstName", firstName),
                DatabaseManager.CreateParameter("@lastName", lastName),
                DatabaseManager.CreateParameter("@created", DateTime.Now));

            if (charId == 0)
            {
                NAPI.Util.ConsoleOutput($"[CharacterManager] Fehler beim Erstellen von Character '{firstName} {lastName}'");
                return null;
            }

            var character = new Character
            {
                Id = (int)charId,
                AccountId = accountId,
                FirstName = firstName,
                LastName = lastName,
                Cash = 500,
                BankBalance = 5000,
                Level = 1,
                Experience = 0,
                Health = 100,
                Armor = 0,
                LastPosition = new Vector3(-1037.7f, -2738.5f, 13.8f),
                LastRotation = new Vector3(0, 0, 0),
                CreatedAt = DateTime.Now
            };

            _loadedCharacters[character.Id] = character;
            NAPI.Util.ConsoleOutput($"[CharacterManager] Character '{firstName} {lastName}' erstellt (ID: {charId})");
            return character;
        }

        /// <summary>
        /// Überprüft ob ein Charaktername bereits existiert
        /// </summary>
        public async Task<bool> CharacterNameExists(string firstName, string lastName)
        {
            string query = "SELECT COUNT(*) FROM characters WHERE first_name = @firstName AND last_name = @lastName";
            var result = await DatabaseManager.Instance.ExecuteScalar(query,
                DatabaseManager.CreateParameter("@firstName", firstName),
                DatabaseManager.CreateParameter("@lastName", lastName));

            return result != null && Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Speichert einen Character in der Datenbank
        /// </summary>
        public async Task<bool> SaveCharacter(Character character)
        {
            string query = @"UPDATE characters SET 
                money = @money, bank_money = @bankMoney, level = @level, experience = @exp, 
                play_time = @playTime, health = @health, armor = @armor,
                pos_x = @posX, pos_y = @posY, pos_z = @posZ, rotation = @rotation,
                faction_id = @factionId, faction_rank = @factionRank,
                last_played = @lastPlayed
                WHERE id = @charId";

            int result = await DatabaseManager.Instance.ExecuteNonQuery(query,
                DatabaseManager.CreateParameter("@money", character.Cash),
                DatabaseManager.CreateParameter("@bankMoney", character.BankBalance),
                DatabaseManager.CreateParameter("@level", character.Level),
                DatabaseManager.CreateParameter("@exp", character.Experience),
                DatabaseManager.CreateParameter("@playTime", character.PlaytimeMinutes),
                DatabaseManager.CreateParameter("@health", character.Health),
                DatabaseManager.CreateParameter("@armor", character.Armor),
                DatabaseManager.CreateParameter("@posX", character.LastPosition.X),
                DatabaseManager.CreateParameter("@posY", character.LastPosition.Y),
                DatabaseManager.CreateParameter("@posZ", character.LastPosition.Z),
                DatabaseManager.CreateParameter("@rotation", character.LastRotation.Z),
                DatabaseManager.CreateParameter("@factionId", character.FactionId),
                DatabaseManager.CreateParameter("@factionRank", character.FactionRank),
                DatabaseManager.CreateParameter("@lastPlayed", DateTime.Now),
                DatabaseManager.CreateParameter("@charId", character.Id));

            return result > 0;
        }

        /// <summary>
        /// Setzt den Character für einen Spieler
        /// </summary>
        public void SetPlayerCharacter(GTANetworkAPI.Player player, Character character)
        {
            _playerCharacters[player] = character;
            player.Name = character.FullName;
            player.Position = character.LastPosition;
            player.Rotation = character.LastRotation;
            player.Health = character.Health;
            player.Armor = character.Armor;

            NAPI.Util.ConsoleOutput($"[CharacterManager] Character {character.FullName} für Spieler gesetzt");
        }

        /// <summary>
        /// Holt den Character eines Spielers
        /// </summary>
        public Character? GetPlayerCharacter(GTANetworkAPI.Player player)
        {
            return _playerCharacters.ContainsKey(player) ? _playerCharacters[player] : null;
        }

        /// <summary>
        /// Entfernt Character-Zuordnung beim Disconnect
        /// </summary>
        public async Task RemovePlayerCharacter(GTANetworkAPI.Player player)
        {
            if (_playerCharacters.ContainsKey(player))
            {
                var character = _playerCharacters[player];

                // Speichere Character vor dem Entfernen
                character.LastPosition = player.Position;
                character.LastRotation = player.Rotation;
                character.Health = player.Health;
                character.Armor = player.Armor;
                await SaveCharacter(character);

                _playerCharacters.Remove(player);
            }
        }

        /// <summary>
        /// Gibt Geld an einen Character
        /// </summary>
        public async Task<bool> GiveMoney(Character character, int amount)
        {
            if (amount <= 0) return false;

            character.Cash += amount;
            await SaveCharacter(character);
            return true;
        }

        /// <summary>
        /// Nimmt Geld von einem Character
        /// </summary>
        public async Task<bool> TakeMoney(Character character, int amount)
        {
            if (amount <= 0 || character.Cash < amount) return false;

            character.Cash -= amount;
            await SaveCharacter(character);
            return true;
        }

        /// <summary>
        /// Gibt Bank-Geld an einen Character
        /// </summary>
        public async Task<bool> GiveBankMoney(Character character, int amount)
        {
            if (amount <= 0) return false;

            character.BankBalance += amount;
            await SaveCharacter(character);
            return true;
        }

        /// <summary>
        /// Nimmt Bank-Geld von einem Character
        /// </summary>
        public async Task<bool> TakeBankMoney(Character character, int amount)
        {
            if (amount <= 0 || character.BankBalance < amount) return false;

            character.BankBalance -= amount;
            await SaveCharacter(character);
            return true;
        }

        /// <summary>
        /// Gibt Experience und prüft Level-Up
        /// </summary>
        public async Task<bool> GiveExperience(Character character, int amount)
        {
            if (amount <= 0) return false;

            character.Experience += amount;

            // Level-Up Check (1000 EXP pro Level)
            int requiredExp = character.Level * 1000;
            if (character.Experience >= requiredExp)
            {
                character.Level++;
                character.Experience -= requiredExp;
                NAPI.Util.ConsoleOutput($"[CharacterManager] {character.FullName} ist jetzt Level {character.Level}!");
            }

            await SaveCharacter(character);
            return true;
        }

        /// <summary>
        /// Aktualisiert Spielzeit
        /// </summary>
        public async Task UpdatePlayTime(Character character, int minutes)
        {
            character.PlaytimeMinutes += minutes;
            await SaveCharacter(character);
        }
    }
}
