using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using RPCore.Models.Faction;
using RPCore.Models.Character;

namespace RPCore.Managers
{
    /// <summary>
    /// Faction Manager - Verwaltet alle Fraktionen, Ränge und Mitglieder
    /// </summary>
    public class FactionManager
    {
        private static FactionManager _instance;
        public static FactionManager Instance => _instance ??= new FactionManager();

        private Dictionary<int, Faction> _factions;
        private Dictionary<int, List<FactionRank>> _factionRanks;
        private Dictionary<int, FactionMember> _factionMembers; // CharacterId -> FactionMember

        private FactionManager()
        {
            _factions = new Dictionary<int, Faction>();
            _factionRanks = new Dictionary<int, List<FactionRank>>();
            _factionMembers = new Dictionary<int, FactionMember>();

            LoadFactions();
        }

        /// <summary>
        /// Lädt alle Fraktionen aus der Datenbank
        /// </summary>
        private void LoadFactions()
        {
            // TODO: Aus Datenbank laden
            NAPI.Util.ConsoleOutput("[FactionManager] Fraktionen werden geladen...");

            // Beispiel: Erstelle Standard-Fraktionen
            CreateDefaultFactions();
        }

        /// <summary>
        /// Erstellt Standard-Fraktionen (für Testing)
        /// </summary>
        private void CreateDefaultFactions()
        {
            // LSPD
            var lspd = new Faction
            {
                Id = 1,
                Name = "Los Santos Police Department",
                ShortName = "LSPD",
                Type = FactionType.Polizei,
                PrimaryColor = "#1E40AF",
                SecondaryColor = "#FFFFFF",
                MaxMembers = 100,
                CreatedBy = "System"
            };
            _factions[lspd.Id] = lspd;
            CreateDefaultRanks(lspd.Id, new[] { "Recruit", "Officer I", "Officer II", "Sergeant", "Lieutenant", "Captain", "Chief" });

            // Medics
            var medics = new Faction
            {
                Id = 2,
                Name = "Los Santos Medical Department",
                ShortName = "LSMD",
                Type = FactionType.Medic,
                PrimaryColor = "#DC2626",
                SecondaryColor = "#FFFFFF",
                MaxMembers = 80,
                CreatedBy = "System"
            };
            _factions[medics.Id] = medics;
            CreateDefaultRanks(medics.Id, new[] { "Praktikant", "Sanitäter", "Rettungsassistent", "Notarzt", "Oberarzt", "Chefarzt" });

            // Vagos (Gang)
            var vagos = new Faction
            {
                Id = 3,
                Name = "Los Santos Vagos",
                ShortName = "Vagos",
                Type = FactionType.Gang,
                PrimaryColor = "#FACC15",
                SecondaryColor = "#000000",
                MaxMembers = 50,
                CreatedBy = "System"
            };
            _factions[vagos.Id] = vagos;
            CreateDefaultRanks(vagos.Id, new[] { "Affiliate", "Soldier", "Capo", "Underboss", "Boss" });

            NAPI.Util.ConsoleOutput($"[FactionManager] {_factions.Count} Fraktionen geladen");
        }

        /// <summary>
        /// Erstellt Standard-Ränge für eine Fraktion
        /// </summary>
        private void CreateDefaultRanks(int factionId, string[] rankNames)
        {
            var ranks = new List<FactionRank>();
            for (int i = 0; i < rankNames.Length; i++)
            {
                var rank = new FactionRank
                {
                    Id = (factionId * 100) + i,
                    FactionId = factionId,
                    Level = i,
                    Name = rankNames[i],
                    Salary = 500 + (i * 200),
                    CanInvite = i >= rankNames.Length - 2,
                    CanKick = i >= rankNames.Length - 2,
                    CanPromote = i >= rankNames.Length - 2,
                    CanManageRanks = i == rankNames.Length - 1,
                    CanAccessBank = i >= rankNames.Length - 3,
                    CanWithdrawMoney = i >= rankNames.Length - 2
                };
                ranks.Add(rank);
            }
            _factionRanks[factionId] = ranks;
        }

        /// <summary>
        /// Holt eine Fraktion nach ID
        /// </summary>
        public Faction GetFaction(int factionId)
        {
            return _factions.ContainsKey(factionId) ? _factions[factionId] : null;
        }

        /// <summary>
        /// Holt alle Fraktionen
        /// </summary>
        public List<Faction> GetAllFactions()
        {
            return _factions.Values.ToList();
        }

        /// <summary>
        /// Holt alle Fraktionen eines bestimmten Typs
        /// </summary>
        public List<Faction> GetFactionsByType(FactionType type)
        {
            return _factions.Values.Where(f => f.Type == type).ToList();
        }

        /// <summary>
        /// Fügt einen Charakter einer Fraktion hinzu
        /// </summary>
        public bool AddCharacterToFaction(int characterId, int factionId, int rankLevel, string invitedBy)
        {
            var faction = GetFaction(factionId);
            if (faction == null)
            {
                NAPI.Util.ConsoleOutput($"[FactionManager] Fraktion {factionId} nicht gefunden");
                return false;
            }

            // Prüfe ob bereits in einer Fraktion
            if (_factionMembers.ContainsKey(characterId))
            {
                NAPI.Util.ConsoleOutput("[FactionManager] Charakter ist bereits in einer Fraktion");
                return false;
            }

            var ranks = _factionRanks[factionId];
            var rank = ranks.FirstOrDefault(r => r.Level == rankLevel);
            if (rank == null)
            {
                NAPI.Util.ConsoleOutput("[FactionManager] Rang nicht gefunden");
                return false;
            }

            var member = new FactionMember
            {
                CharacterId = characterId,
                FactionId = factionId,
                RankId = rank.Id,
                InvitedBy = invitedBy
            };

            _factionMembers[characterId] = member;
            // TODO: In Datenbank speichern

            NAPI.Util.ConsoleOutput($"[FactionManager] Charakter {characterId} zu {faction.Name} hinzugefügt");
            return true;
        }

        /// <summary>
        /// Entfernt einen Charakter aus einer Fraktion
        /// </summary>
        public bool RemoveCharacterFromFaction(int characterId)
        {
            if (!_factionMembers.ContainsKey(characterId))
            {
                return false;
            }

            _factionMembers.Remove(characterId);
            // TODO: Aus Datenbank löschen

            NAPI.Util.ConsoleOutput($"[FactionManager] Charakter {characterId} aus Fraktion entfernt");
            return true;
        }

        /// <summary>
        /// Holt das FactionMember eines Charakters
        /// </summary>
        public FactionMember GetFactionMember(int characterId)
        {
            return _factionMembers.ContainsKey(characterId) ? _factionMembers[characterId] : null;
        }

        /// <summary>
        /// Holt den Rang eines Charakters
        /// </summary>
        public FactionRank GetCharacterRank(int characterId)
        {
            var member = GetFactionMember(characterId);
            if (member == null) return null;

            var ranks = _factionRanks[member.FactionId];
            return ranks.FirstOrDefault(r => r.Id == member.RankId);
        }

        /// <summary>
        /// Befördert einen Charakter
        /// </summary>
        public bool PromoteCharacter(int characterId)
        {
            var member = GetFactionMember(characterId);
            if (member == null) return false;

            var currentRank = GetCharacterRank(characterId);
            var ranks = _factionRanks[member.FactionId];
            var nextRank = ranks.FirstOrDefault(r => r.Level == currentRank.Level + 1);

            if (nextRank == null) return false; // Bereits höchster Rang

            member.RankId = nextRank.Id;
            // TODO: In Datenbank speichern

            NAPI.Util.ConsoleOutput($"[FactionManager] Charakter {characterId} zu {nextRank.Name} befördert");
            return true;
        }

        /// <summary>
        /// Degradiert einen Charakter
        /// </summary>
        public bool DemoteCharacter(int characterId)
        {
            var member = GetFactionMember(characterId);
            if (member == null) return false;

            var currentRank = GetCharacterRank(characterId);
            var ranks = _factionRanks[member.FactionId];
            var previousRank = ranks.FirstOrDefault(r => r.Level == currentRank.Level - 1);

            if (previousRank == null) return false; // Bereits niedrigster Rang

            member.RankId = previousRank.Id;
            // TODO: In Datenbank speichern

            NAPI.Util.ConsoleOutput($"[FactionManager] Charakter {characterId} zu {previousRank.Name} degradiert");
            return true;
        }

        /// <summary>
        /// Setzt den Duty-Status
        /// </summary>
        public void SetDutyStatus(int characterId, bool onDuty)
        {
            var member = GetFactionMember(characterId);
            if (member != null)
            {
                member.IsOnDuty = onDuty;
                // TODO: In Datenbank speichern
            }
        }

        /// <summary>
        /// Holt alle Mitglieder einer Fraktion
        /// </summary>
        public List<FactionMember> GetFactionMembers(int factionId)
        {
            return _factionMembers.Values.Where(m => m.FactionId == factionId).ToList();
        }
    }
}
