using RPCore.Database;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPCore.Administration
{
    public class SupportTicket
    {
        public int Id { get; set; }
        public int ReporterId { get; set; }
        public string ReporterName { get; set; }
        public string Message { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public DateTime ReportDate { get; set; }
        public string Status { get; set; }
        public bool IsPriority { get; set; }
        public string AdminComment { get; set; }
    }
}

public class AdminCommentEntry
{
    public string Timestamp { get; set; }
    public string AdminName { get; set; }
    public string Text { get; set; }
}



public class SupportCommands : Script
{
    [Command("support", GreedyArg = true)]
    public void CMD_CreateSupportTicket(Player player, string message)
    {
        Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
        if (account == null) return;

        if (string.IsNullOrWhiteSpace(message) || message.Length < 10)
        {
            player.SendNotification("~r~Deine Support-Anfrage ist zu kurz. Bitte beschreibe dein Problem genauer.");
            return;
        }

        // --- NEU: Zeitstempel und Koordinaten erfassen ---
        string timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        Vector3 position = player.Position;
        string contextInfo = $"[{timestamp} | Pos: {position.X:F0}, {position.Y:F0}, {position.Z:F0}]";

        // --- NEU: Die finale Nachricht zusammensetzen ---
        string finalMessage = $"{contextInfo}\n\n{message}";

        // Erstelle das Ticket in der Datenbank mit der erweiterten Nachricht
        Datenbank.CreateSupportTicket(account.ID, player.Name, finalMessage, player.Position);

        player.SendNotification("~g~Dein Support-Ticket wurde erstellt. Ein Admin wird sich bald darum kümmern.");

        // Die Admin-Benachrichtigungen bleiben gleich...
        string adminMessage = $"~p~[SUPPORT]~w~ Neues Ticket";
        var admins = NAPI.Pools.GetAllPlayers().Where(p => p.HasData(Accounts.Account_Key) && p.GetData<Accounts>(Accounts.Account_Key).Adminlevel > 0).ToList();

        foreach (var admin in admins)
        {
            admin.SendChatMessage(adminMessage);
            admin.TriggerEvent("support:newTicket");
        }
    }
}