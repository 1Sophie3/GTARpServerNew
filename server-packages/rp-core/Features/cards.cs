// Speicherort: Features/cards.cs

using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Linq;
using RPCore.Database;

public class IdCardCommands : Script
{
    // NEU: Direkter Befehl, um den eigenen Ausweis zu zeigen.
    [Command("id", "ausweis")]
    public void CMD_ShowSelfIdCard(Player player)
    {
        if (!Accounts.IstSpielerEingeloggt(player)) return;

        Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
        if (account == null || string.IsNullOrEmpty(account.CharacterData)) return;

        try
        {
            CharacterData charData = JsonConvert.DeserializeObject<CharacterData>(account.CharacterData);
            if (charData == null) return;

            var dataForClient = new
            {
                firstname = charData.Firstname,
                lastname = charData.Lastname,
                birth = charData.Birth,
                gender = charData.Gender, // Deine Logik hier
                accountId = account.ID,
                creationDate = account.CreationDate.ToString("dd.MM.yyyy")
            };

            // Der Aufruf ist jetzt ein direkter TriggerEvent, kein RemoteEvent mehr.
            player.TriggerEvent("showPlayerIdCard", JsonConvert.SerializeObject(dataForClient));
        }
        catch (JsonException) { /* Fehler ignorieren */ }
    }

    // NEU: Direkter Befehl, um den Ausweis anderen zu zeigen.
    [Command("showid", "zeigeausweis")]
    public void CMD_ShowIdToOther(Player player, int targetAccountId)
    {
        if (!Accounts.IstSpielerEingeloggt(player)) return;

        Accounts sourceAccount = player.GetData<Accounts>(Accounts.Account_Key);
        if (sourceAccount == null || string.IsNullOrEmpty(sourceAccount.CharacterData)) return;

        Player target = FindPlayerByAccountId(targetAccountId);
        if (target == null)
        {
            player.SendChatMessage("~r~Dieser Spieler ist nicht online.");
            return;
        }
        Accounts targetAccount = target.GetData<Accounts>(Accounts.Account_Key);
        if (targetAccount == null) return;

        if (player.Position.DistanceTo(target.Position) > 5.0f)
        {
            player.SendChatMessage("Du bist zu weit von diesem Spieler entfernt.");
            return;
        }

        try
        {
            CharacterData charData = JsonConvert.DeserializeObject<CharacterData>(sourceAccount.CharacterData);
            if (charData == null) return;

            var dataForClient = new
            {
                firstname = charData.Firstname,
                lastname = charData.Lastname,
                birth = charData.Birth,
                gender = charData.Gender,
                accountId = sourceAccount.ID,
                creationDate = sourceAccount.CreationDate.ToString("dd.MM.yyyy")
            };

            // Sende die Daten direkt an den Client des Ziels.
            target.TriggerEvent("showPlayerIdCard", JsonConvert.SerializeObject(dataForClient));
        }
        catch (JsonException) { /* Fehler ignorieren */ }

        player.SendChatMessage($"Du zeigst {targetAccount.GetFullName()} deinen Ausweis.");
        target.SendChatMessage($"{sourceAccount.GetFullName()} zeigt dir seinen Ausweis.");
    }

    // Die alten [RemoteEvent]-Handler ("OnRequestSelfIdCard" etc.) wurden durch die Commands ersetzt.

    private Player FindPlayerByAccountId(int accountId)
    {
        return NAPI.Pools.GetAllPlayers().FirstOrDefault(p =>
            Accounts.IstSpielerEingeloggt(p) &&
            p.GetData<Accounts>(Accounts.Account_Key).ID == accountId
        );
    }
}