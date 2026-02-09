using RPCore.Database;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCore.Commands
{
    class Banmanager : Script
    {
        [Command("ban", "/ban [AccountID] [Grund]")]
        public void cmd_ban(Player admin, int accountId, string grund = "")
        {
            // Admin-Berechtigungsprüfung
            Accounts adminAccount = admin.GetData<Accounts>(Accounts.Account_Key);
            if (adminAccount == null || !adminAccount.IstSpielerAdmin((int)Accounts.AdminRanks.GameMaster))
            {
                admin.SendChatMessage("~r~Du hast keine Berechtigung, diesen Befehl auszuführen.");
                return;
            }

            // Überprüfen, ob ein Grund angegeben wurde
            if (string.IsNullOrWhiteSpace(grund))
            {
                admin.SendChatMessage("~r~Du musst einen Grund für den Bann angeben.");
                return;
            }

            // Spieler bannen
            try
            {
                gamemode.Datenbank.Datenbank.SpielerBannen(accountId, grund);
                admin.SendChatMessage($"~g~Spieler mit Account-ID {accountId} wurde gebannt. Grund: {grund}");
                NAPI.Util.ConsoleOutput($"[INFO] Admin {admin.Name} hat Spieler mit Account-ID {accountId} gebannt. Grund: {grund}");
            }
            catch (Exception ex)
            {
                admin.SendChatMessage("~r~Fehler beim Bannen des Spielers.");
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Bann-Befehl: {ex.Message}");
            }
        }










        [Command("unban", "/unban [AccountID]")]
        public void cmd_unban(Player admin, int accountId)
        {
            // Admin-Berechtigungsprüfung
            Accounts adminAccount = admin.GetData<Accounts>(Accounts.Account_Key);

            if (adminAccount == null || !adminAccount.IstSpielerAdmin((int)Accounts.AdminRanks.Admin)) // Passe die Adminberechtigungsstufe an
            {
                admin.SendChatMessage("~r~Du hast keine Berechtigung, diesen Befehl auszuführen.");
                return;
            }

            // Spieler entbannen
            try
            {
                gamemode.Datenbank.Datenbank.SpielerEntbannen(accountId);
                admin.SendChatMessage($"~g~Spieler mit Account-ID {accountId} wurde entbannt.");
                NAPI.Util.ConsoleOutput($"[INFO] Admin {admin.Name} hat Spieler mit Account-ID {accountId} entbannt.");
            }
            catch (Exception ex)
            {
                admin.SendChatMessage("~r~Fehler beim Entbannen des Spielers.");
                NAPI.Util.ConsoleOutput($"[FEHLER] Fehler beim Entbann-Befehl: {ex.Message}");
            }
        }


    }
}
