using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using RPCore.Database;
using RPCore.FraktionsSystem;

public class LicenseSystem : Script
{
    // --- Konfiguration ---
    private static readonly Dictionary<LicenseType, int> LicensePrices = new Dictionary<LicenseType, int>
    {
        { LicenseType.Car, 750 },
        { LicenseType.Truck, 1200 },
        { LicenseType.Boat, 500 },
        { LicenseType.Aircraft, 2500 }
    };

    private const int DRIVING_SCHOOL_FACTION_ID = 4;
    private const int LSPD_FACTION_ID = 1;

    public enum LicenseType { Car, Truck, Boat, Aircraft }

    [Command("givelicense", "/givelicense [Spieler-ID] [Lizenztyp: Car, Truck, Boat, Aircraft]")]
    public void CMD_GiveLicense(Player instructor, int targetAccountId, string licenseName)
    {
        Accounts instructorAccount = instructor.GetData<Accounts>(Accounts.Account_Key);
        if (instructorAccount == null) return;

        bool hasAdminRights = instructorAccount.IstSpielerAdmin((int)Accounts.AdminRanks.Admin);
        bool hasFactionRights = (instructorAccount.Fraktion == DRIVING_SCHOOL_FACTION_ID);

        if (!hasAdminRights && !hasFactionRights)
        {
            instructor.SendChatMessage("~r~Du bist weder Fahrlehrer noch ein berechtigter Admin.");
            return;
        }

        Player targetPlayer = FindPlayerByAccountId(targetAccountId);
        if (targetPlayer == null)
        {
            instructor.SendChatMessage("~r~Dieser Spieler ist nicht online.");
            return;
        }
        Accounts targetAccount = targetPlayer.GetData<Accounts>(Accounts.Account_Key);
        if (targetAccount == null) return;

        if (!Enum.TryParse<LicenseType>(licenseName, true, out LicenseType licenseType))
        {
            instructor.SendChatMessage("~r~Ungültiger Lizenztyp. Verfügbar: Car, Truck, Boat, Aircraft");
            return;
        }

        var licenses = new Dictionary<LicenseType, DateTime>();
        try
        {
            if (!string.IsNullOrWhiteSpace(targetAccount.Licenses))
                licenses = JsonConvert.DeserializeObject<Dictionary<LicenseType, DateTime>>(targetAccount.Licenses);
        }
        catch (JsonSerializationException) { /* Fängt alte Datenformate ab */ }

        if (licenses.ContainsKey(licenseType))
        {
            instructor.SendChatMessage($"~r~Der Spieler {targetAccount.GetFullName()} besitzt diese Lizenz bereits.");
            return;
        }

        int price = hasAdminRights ? 0 : LicensePrices[licenseType];
        if (targetAccount.Geld < price)
        {
            instructor.SendChatMessage($"~r~Der Spieler hat nicht genug Geld. Benötigt: {price}$.");
            return;
        }

        targetAccount.Geld -= price;
        licenses[licenseType] = DateTime.Now;
        targetAccount.Licenses = JsonConvert.SerializeObject(licenses);

        Datenbank.AccountSpeichern(targetPlayer);

        string priceText = hasAdminRights ? "kostenlos" : $"für {price}$";
        instructor.SendChatMessage($"~g~Du hast {targetAccount.GetFullName()} die {licenseType}-Lizenz {priceText} ausgestellt.");
        targetPlayer.SendChatMessage($"~g~Dir wurde die {licenseType}-Lizenz ausgestellt.");
    }

    [Command("revokelicense", "/revokelicense [Spieler-ID] [Lizenztyp]")]
    public void CMD_RevokeLicense(Player officer, int targetAccountId, string licenseName)
    {
        Accounts officerAccount = officer.GetData<Accounts>(Accounts.Account_Key);
        if (officerAccount == null) return;

        bool hasAdminRights = officerAccount.IstSpielerAdmin((int)Accounts.AdminRanks.Admin);
        bool hasFactionRights = false;
        if (officerAccount.Fraktion == LSPD_FACTION_ID)
        {
            int officerRank = Fglobalconf.FraktionsCommands.GetMemberRank(officerAccount.ID, officerAccount.Fraktion);
            if (officerRank >= 3) hasFactionRights = true;
        }

        if (!hasAdminRights && !hasFactionRights)
        {
            officer.SendChatMessage("~r~Du hast keine Berechtigung, Lizenzen zu entziehen.");
            return;
        }

        Player targetPlayer = FindPlayerByAccountId(targetAccountId);
        if (targetPlayer == null)
        {
            officer.SendChatMessage("~r~Dieser Spieler ist nicht online.");
            return;
        }
        Accounts targetAccount = targetPlayer.GetData<Accounts>(Accounts.Account_Key);
        if (targetAccount == null) return;

        if (!Enum.TryParse<LicenseType>(licenseName, true, out LicenseType licenseType))
        {
            officer.SendChatMessage("~r~Ungültiger Lizenztyp. Verfügbar: Car, Truck, Boat, Aircraft");
            return;
        }

        var licenses = new Dictionary<LicenseType, DateTime>();
        try
        {
            if (!string.IsNullOrWhiteSpace(targetAccount.Licenses))
                licenses = JsonConvert.DeserializeObject<Dictionary<LicenseType, DateTime>>(targetAccount.Licenses);
        }
        catch (JsonSerializationException) { /* Fängt alte Datenformate ab */ }

        if (!licenses.ContainsKey(licenseType))
        {
            officer.SendChatMessage($"~r~Der Spieler {targetAccount.GetFullName()} besitzt diese Lizenz nicht.");
            return;
        }

        licenses.Remove(licenseType);
        targetAccount.Licenses = JsonConvert.SerializeObject(licenses);
        Datenbank.AccountSpeichern(targetPlayer);

        officer.SendChatMessage($"~g~Du hast {targetAccount.GetFullName()} die {licenseType}-Lizenz entzogen.");
        targetPlayer.SendChatMessage($"~r~Ein Officer hat dir die {licenseType}-Lizenz entzogen!");
    }

    [Command("license", "führerschein")]
    public void CMD_ShowSelfLicense(Player player)
    {
        Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
        if (account == null) return;

        var licensesDict = new Dictionary<LicenseType, DateTime>();
        try { if (!string.IsNullOrWhiteSpace(account.Licenses)) licensesDict = JsonConvert.DeserializeObject<Dictionary<LicenseType, DateTime>>(account.Licenses); }
        catch (JsonSerializationException) { /* stillschweigend abfangen */ }

        if (licensesDict.Count == 0)
        {
            player.SendChatMessage("~r~Du besitzt noch keinen Führerschein.");
            return;
        }

        var dataForClient = CreateLicenseDataPacket(account, licensesDict);
        player.TriggerEvent("showPlayerLicense", JsonConvert.SerializeObject(dataForClient));
    }

    [Command("showlicense", "zeigeführerschein")]
    public void CMD_ShowLicenseToOther(Player player, int targetAccountId)
    {
        Accounts sourceAccount = player.GetData<Accounts>(Accounts.Account_Key);
        if (sourceAccount == null) return;

        var sourceLicenses = new Dictionary<LicenseType, DateTime>();
        try { if (!string.IsNullOrWhiteSpace(sourceAccount.Licenses)) sourceLicenses = JsonConvert.DeserializeObject<Dictionary<LicenseType, DateTime>>(sourceAccount.Licenses); }
        catch (JsonSerializationException) { /* stillschweigend abfangen */ }

        if (sourceLicenses.Count == 0)
        {
            player.SendChatMessage("~r~Du besitzt noch keinen Führerschein, den du zeigen könntest.");
            return;
        }

        Player targetPlayer = FindPlayerByAccountId(targetAccountId);
        if (targetPlayer == null) { player.SendChatMessage("~r~Dieser Spieler ist nicht online."); return; }

        Accounts targetAccount = targetPlayer.GetData<Accounts>(Accounts.Account_Key);
        if (targetAccount == null) return;

        if (player.Position.DistanceTo(targetPlayer.Position) > 5.0f)
        {
            player.SendChatMessage("Du bist zu weit von diesem Spieler entfernt.");
            return;
        }

        var dataForClient = CreateLicenseDataPacket(sourceAccount, sourceLicenses);
        targetPlayer.TriggerEvent("showPlayerLicense", JsonConvert.SerializeObject(dataForClient));

        player.SendChatMessage($"Du zeigst {targetAccount.GetFullName()} deinen Führerschein.");
        targetPlayer.SendChatMessage($"{sourceAccount.GetFullName()} zeigt dir seinen Führerschein.");
    }

    private object CreateLicenseDataPacket(Accounts account, Dictionary<LicenseType, DateTime> licenses)
    {
        var dataPacket = new
        {
            ownerName = account.GetFullName(),
            ownerId = account.ID,
            carData = licenses.ContainsKey(LicenseType.Car) ? licenses[LicenseType.Car].ToString("dd.MM.yyyy") : null,
            truckData = licenses.ContainsKey(LicenseType.Truck) ? licenses[LicenseType.Truck].ToString("dd.MM.yyyy") : null,
            boatData = licenses.ContainsKey(LicenseType.Boat) ? licenses[LicenseType.Boat].ToString("dd.MM.yyyy") : null,
            aircraftData = licenses.ContainsKey(LicenseType.Aircraft) ? licenses[LicenseType.Aircraft].ToString("dd.MM.yyyy") : null
        };
        return dataPacket;
    }

    private Player FindPlayerByAccountId(int accountId)
    {
        return NAPI.Pools.GetAllPlayers().FirstOrDefault(p =>
            Accounts.IstSpielerEingeloggt(p) &&
            p.GetData<Accounts>(Accounts.Account_Key).ID == accountId
        );
    }
}