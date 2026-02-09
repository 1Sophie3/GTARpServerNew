using GTANetworkAPI;
using RPCore.Database;
using System;

namespace RPCore.AdminMenu
{
    public class AdminMenu : Script
    {
        [RemoteEvent("RequestAdminData")]
        public void RequestAdminData(Player player)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            if (account != null)
            {
                player.TriggerEvent("SetAdminRank", account.Adminlevel);
                NAPI.Util.ConsoleOutput($"[DEBUG] (Request) SetAdminRank gesendet: {account.Adminlevel} an {player.Name}");
            }
            else
            {
                player.TriggerEvent("SetAdminRank", 0);
            }
        }

        private bool HasRequiredAdminLevel(Player player, int requiredLevel)
        {
            Accounts account = player.GetData<Accounts>(Accounts.Account_Key);
            return account != null && account.IstSpielerAdmin(requiredLevel);
        }

        [RemoteEvent("admin_heal")]
        public void AdminHeal(Player player)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.TestGamemaster))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            NAPI.Util.ConsoleOutput($"[DEBUG] admin_heal aufgerufen von {player.Name}");
            new gamemode.Commands.Commands().cmd_heal(player);
            gamemode.Datenbank.Datenbank.SavePlayerStatus(player);
        }

        [RemoteEvent("admin_av")]
        public void AdminAV(Player player)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.TestGamemaster))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            new gamemode.Commands.Commands().cmd_aveh(player);
        }

        [RemoteEvent("admin_weapon")]
        public void AdminWeapon(Player player)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.GameMaster))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            new gamemode.Commands.Commands().GivePistol50(player);
        }

        [RemoteEvent("admin_cdim")]
        public void AdminCDim(Player player)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.TestGamemaster))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            new gamemode.Commands.Commands().CheckDimensionCommand(player);
        }

        [RemoteEvent("admin_dim")]
        public void AdminDim(Player player, int dimension)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.TestGamemaster))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            new gamemode.Commands.Commands().ChangeDimension(player, dimension);
        }

        [RemoteEvent("admin_respawn")]
        public void AdminRespawn(Player player)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.TestGamemaster))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            new gamemode.Commands.Commands().RespawnPlayerCommand(player);
            gamemode.Datenbank.Datenbank.SavePlayerStatus(player);
        }

        [RemoteEvent("admin_tune")]
        public void AdminTune(Player player)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.Admin))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            new gamemode.Commands.Commands().cmd_tuneAll(player);
        }

        [RemoteEvent("admin_tp")]
        public void AdminTP(Player player, string locationName)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.TestGamemaster))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            new gamemode.Commands.TeleportCommand().TeleportPlayerCommand(player, locationName);
        }

        [RemoteEvent("admin_vtp")]
        public void AdminVTP(Player player, string locationName)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.TestGamemaster))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            new gamemode.Commands.TeleportCommand().TeleportPlayerAndVehicleCommand(player, locationName);
        }

        [RemoteEvent("admin_goto")]
        public void AdminGoto(Player player, string identifier)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.TestGamemaster))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            new gamemode.Commands.TeleportCommand().TeleportToPlayerCommand(player, identifier);
        }

        [RemoteEvent("admin_createveh")]
        public void AdminCreateVeh(Player player, string modelName)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.Admin))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            new gamemode.Commands.VehSafeCommands().CreateVehicleCommand(player, modelName);
        }

        [RemoteEvent("admin_delveh")]
        public void AdminDelVeh(Player player)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.Admin))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            new gamemode.Commands.VehSafeCommands().DelVehicleCommand(player);
        }

        [RemoteEvent("admin_setcolor")]
        public void AdminSetColor(Player player, int color1, int color2)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.Admin))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            new gamemode.Commands.VehSafeCommands().SetColorCommand(player, color1, color2);
        }

        [RemoteEvent("admin_setnumberplate")]
        public void AdminSetNumberPlate(Player player, string plate)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.Admin))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            new gamemode.Commands.VehSafeCommands().SetNumberPlateCommand(player, plate);
        }

        [RemoteEvent("admin_createhouse")]
        public void AdminCreateHouse(Player player, int preis, int interiorIndex)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.Owner))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            new gamemode.Commands.Commands().CMD_createhouse(player, preis, interiorIndex);
        }

        [RemoteEvent("admin_setped")]
        public void AdminSetPed(Player player, string pedName)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.Owner))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            new gamemode.Commands.Commands().CMD_SetPed(player, pedName);
        }

        [RemoteEvent("admin_spawnveh")]
        public void AdminSpawnVeh(Player player, int vehId)
        {
            if (!HasRequiredAdminLevel(player, (int)Accounts.AdminRanks.Owner))
            {
                player.SendChatMessage("~r~Du hast keine Berechtigung für diesen Befehl.");
                return;
            }
            new gamemode.Commands.VehSafeCommands().SpawnVehicleCommand(player, vehId);
        }
    }
}
