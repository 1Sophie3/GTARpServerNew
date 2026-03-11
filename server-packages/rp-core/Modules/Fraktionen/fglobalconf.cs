using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using GTANetworkAPI;
using Newtonsoft.Json;
using RPCore.Datenbank;
using RPCore.Notifications;
using System.Linq;

namespace RPCore.FraktionsSystem
{
    // Die Datenklassen für Fraktion und Fraktionsmitglied bleiben unverändert.
    public class Fraktion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public long Bankkonto { get; set; }
        public int LeaderId { get; set; }
    }

    public class Fraktionsmitglied
    {
        public int AccountId { get; set; }
        public int FraktionId { get; set; }
        public int Rank { get; set; }
        public int Gehalt { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public static class Fglobalconf
    {
        private static readonly object DbLock = new object();

        #region Datenbank-Operationen (unverändert)

        public static int ErstelleFraktion(string name, string type, int leaderId)
        {
            // Diese Methode bleibt unverändert.
            // ... (dein bestehender Code hier)
            return -1; // Platzhalter
        }

        public static bool EinladungInFraktion(int accountId, int fraktionId, int rang)
        {
            // Diese Methode bleibt unverändert.
            // ... (dein bestehender Code hier)
            return false; // Platzhalter
        }

        public static bool EntferneAusFraktion(int accountId, int fraktionId)
        {
            // Diese Methode bleibt unverändert.
            // ... (dein bestehender Code hier)
            return false; // Platzhalter
        }

        public static bool ÄndereRang(int accountId, int fraktionId, int neuerRang, int neuesGehalt = 0)
        {
            // Diese Methode bleibt unverändert.
            // ... (dein bestehender Code hier)
            return false; // Platzhalter
        }

        #endregion

        #region Fraktionsbefehle (ÜBERARBEITET)

        public class FraktionsCommands : Script
        {
            public static Dictionary<int, int> PendingInvitations = new Dictionary<int, int>();

            #region Helfer-Methoden

            private Player FindPlayerByAccountId(int accountId)
            {
                return NAPI.Pools.GetAllPlayers().FirstOrDefault(p =>
                    Accounts.IstSpielerEingeloggt(p) &&
                    p.GetData<Accounts>(Accounts.Account_Key).ID == accountId
                );
            }

            // NEU: Lädt Basis-Accountdaten für einen Spieler, auch wenn dieser offline ist.
            private Accounts GetAccountDataFromDb(int accountId)
            {
                Accounts acc = null;
                try
                {
                    using (var connection = new MySqlConnection(Datenbank.Datenbank.GetConnectionString()))
                    {
                        connection.Open();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT id, name, fraktion FROM accounts WHERE id = @id LIMIT 1";
                            command.Parameters.AddWithValue("@id", accountId);
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    acc = new Accounts
                                    {
                                        ID = reader.GetInt32("id"),
                                        Name = reader.GetString("name"),
                                        Fraktion = reader.GetInt32("fraktion")
                                    };
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) { NAPI.Util.ConsoleOutput($"[ERROR] GetAccountDataFromDb: {ex.Message}"); }
                return acc;
            }

            public static int GetMemberRank(int accountId, int fraktionId)
            {
                int rank = 0;
                if (fraktionId == 0) return 0;
                try
                {
                    using (var connection = new MySqlConnection(Datenbank.Datenbank.GetConnectionString()))
                    {
                        connection.Open();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT rank FROM fraktionsmitglieder WHERE account_id = @account AND fraktion_id = @fraktion LIMIT 1";
                            command.Parameters.AddWithValue("@account", accountId);
                            command.Parameters.AddWithValue("@fraktion", fraktionId);
                            var result = command.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                rank = Convert.ToInt32(result);
                            }
                        }
                    }
                }
                catch (Exception ex) { NAPI.Util.ConsoleOutput($"[ERROR] Beim Abruf des Rangs für Account {accountId}: {ex.Message}"); }
                return rank;
            }

            #endregion

            // --- BEFEHLE ---

            [Command("finvite", "/finvite <Account-ID> - Lädt einen Spieler in deine Fraktion ein.")]
            public void FinviteCommand(Player player, int targetAccountId)
            {
                Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
                if (acc == null || acc.Fraktion == 0)
                {
                    player.SendChatMessage("~r~Du bist in keiner Fraktion!");
                    return;
                }
                int issuerRank = GetMemberRank(acc.ID, acc.Fraktion);
                if (issuerRank < 10)
                {
                    player.SendChatMessage("~r~Du besitzt nicht ausreichend Rechte (erforderlich: Rang 10+).");
                    return;
                }

                Player target = FindPlayerByAccountId(targetAccountId);
                if (target == null)
                {
                    player.SendChatMessage("~r~Spieler mit dieser ID ist nicht online!");
                    return;
                }

                Accounts targetAcc = target.GetData<Accounts>(Accounts.Account_Key);
                if (targetAcc == null) return;

                if (targetAcc.Fraktion != 0)
                {
                    player.SendChatMessage("~r~Dieser Spieler ist bereits in einer Fraktion.");
                    return;
                }

                PendingInvitations[targetAcc.ID] = acc.Fraktion;
                player.SendChatMessage($"~g~Du hast {targetAcc.GetFullName()} (ID: {targetAccountId}) in deine Fraktion eingeladen.");
                target.SendChatMessage("~y~Du wurdest in eine Fraktion eingeladen. Nutze /acceptinvite, um beizutreten.");
            }

            [Command("fkick", "/fkick <Account-ID> - Entfernt einen Spieler aus einer Fraktion.")]
            public async void FkickCommand(Player player, int targetAccountId)
            {
                Accounts issuerAcc = player.GetData<Accounts>(Accounts.Account_Key);
                if (issuerAcc == null) return;

                bool isIssuerAdmin = issuerAcc.IstSpielerAdmin((int)Accounts.AdminRanks.Admin);
                int issuerRank = GetMemberRank(issuerAcc.ID, issuerAcc.Fraktion);

                if (!isIssuerAdmin && issuerRank < 10)
                {
                    player.SendChatMessage("~r~Du benötigst Rang 10 oder Admin-Rechte.");
                    return;
                }

                Accounts targetAcc = GetAccountDataFromDb(targetAccountId);
                if (targetAcc == null)
                {
                    player.SendChatMessage("~r~Account mit dieser ID wurde nicht gefunden.");
                    return;
                }

                if (targetAcc.Fraktion == 0)
                {
                    player.SendChatMessage("~r~Dieser Spieler ist in keiner Fraktion.");
                    return;
                }

                // Ein Admin kann jeden kicken. Ein Fraktionsmitglied nur aus der eigenen Fraktion.
                if (!isIssuerAdmin && targetAcc.Fraktion != issuerAcc.Fraktion)
                {
                    player.SendChatMessage("~r~Du kannst nur Mitglieder deiner eigenen Fraktion kicken.");
                    return;
                }

                if (EntferneAusFraktion(targetAcc.ID, targetAcc.Fraktion))
                {
                    player.SendChatMessage($"~g~{targetAcc.Name} (ID: {targetAcc.ID}) wurde aus der Fraktion entfernt!");

                    Player targetPlayer = FindPlayerByAccountId(targetAccountId);
                    if (targetPlayer != null)
                    {
                        Accounts onlineTargetAcc = targetPlayer.GetData<Accounts>(Accounts.Account_Key);
                        onlineTargetAcc.Fraktion = 0;
                        if (onlineTargetAcc.DutyStart != null)
                        {
                            await DutyManager.EndDuty(targetPlayer);
                            targetPlayer.SendNotification("~y~Dein Dienst wurde beendet, da du aus der Fraktion entfernt wurdest.");
                        }
                        Datenbank.Datenbank.AccountSpeichern(targetPlayer);
                        targetPlayer.SendNotification("~r~Du wurdest aus deiner Fraktion entfernt!");
                    }
                }
            }

            [Command("fsetrank", "/fsetrank <Account-ID> <Neuer Rang>")]
            public void FsetrankCommand(Player admin, int targetId, int neuerRang)
            {
                Accounts issuerAcc = admin.GetData<Accounts>(Accounts.Account_Key);
                if (issuerAcc == null) return;

                bool isIssuerAdmin = issuerAcc.IstSpielerAdmin((int)Accounts.AdminRanks.Admin);
                int issuerRank = GetMemberRank(issuerAcc.ID, issuerAcc.Fraktion);

                if (!isIssuerAdmin && issuerRank < 10)
                {
                    admin.SendChatMessage("~r~Du benötigst Rang 10 oder Admin-Rechte.");
                    return;
                }

                if (neuerRang < 1 || neuerRang > 12)
                {
                    admin.SendChatMessage("~r~Der neue Rang muss zwischen 1 und 12 liegen.");
                    return;
                }

                Accounts targetAcc = GetAccountDataFromDb(targetId);
                if (targetAcc == null)
                {
                    admin.SendChatMessage("~r~Account nicht gefunden.");
                    return;
                }

                if (targetAcc.Fraktion == 0)
                {
                    admin.SendChatMessage("~r~Dieser Spieler ist in keiner Fraktion.");
                    return;
                }

                if (!isIssuerAdmin && targetAcc.Fraktion != issuerAcc.Fraktion)
                {
                    admin.SendChatMessage("~r~Du kannst nur Ränge von Mitgliedern deiner eigenen Fraktion ändern.");
                    return;
                }

                int newSalary = 0;
                if (DutyManager.FactionSalaryTables.TryGetValue(targetAcc.Fraktion, out Dictionary<int, int> salaryTable))
                {
                    salaryTable.TryGetValue(neuerRang, out newSalary);
                }

                if (ÄndereRang(targetAcc.ID, targetAcc.Fraktion, neuerRang, newSalary))
                {
                    admin.SendChatMessage($"~g~Der Rang von Account {targetId} wurde auf {neuerRang} gesetzt.");
                    Player targetPlayer = FindPlayerByAccountId(targetId);
                    if (targetPlayer != null)
                    {
                        targetPlayer.SendChatMessage($"~y~Dein Fraktionsrang wurde auf {neuerRang} gesetzt.");
                    }
                }
                else
                {
                    admin.SendChatMessage($"~r~Fehler beim Ändern des Rangs für Account {targetId}.");
                }
            }

            // Die Befehle acceptinvite, derank und frakinfo bleiben von der Logik her gleich,
            // können aber ebenfalls auf ID-basierte Suchen umgestellt werden, falls gewünscht.
        }
        #endregion
    }
}