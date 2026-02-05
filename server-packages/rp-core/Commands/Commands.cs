using GTANetworkAPI;
using RPCore.Player;

namespace RPCore.Commands
{
    public class AdminCommands : Script
    {
        [Command("heal")]
        public void CMD_Heal(GTANetworkAPI.Player player)
        {
            player.Health = 100;
            player.SendChatMessage("~g~Du wurdest geheilt!");
        }

        [Command("tp")]
        public void CMD_Teleport(GTANetworkAPI.Player player, float x, float y, float z)
        {
            player.Position = new Vector3(x, y, z);
            player.SendChatMessage($"~b~Teleportiert zu: {x}, {y}, {z}");
        }

        [Command("car")]
        public void CMD_SpawnCar(GTANetworkAPI.Player player, string vehicleName)
        {
            VehicleHash hash;
            if (!System.Enum.TryParse(vehicleName, true, out hash))
            {
                player.SendChatMessage("~r~Ungültiger Fahrzeugname!");
                return;
            }

            Vector3 pos = player.Position;
            Vector3 rot = player.Rotation;
            
            var vehicle = NAPI.Vehicle.CreateVehicle(hash, pos, rot.Z, 0, 0);
            NAPI.Player.SetPlayerIntoVehicle(player, vehicle, -1);
            
            player.SendChatMessage($"~g~Fahrzeug {vehicleName} gespawnt!");
        }
    }

    public class PlayerCommands : Script
    {
        [Command("stats")]
        public void CMD_Stats(GTANetworkAPI.Player player)
        {
            var data = player.GetPlayerData();
            if (data != null)
            {
                player.SendChatMessage("~b~=== Deine Stats ===");
                player.SendChatMessage($"~w~Level: ~g~{data.Level}");
                player.SendChatMessage($"~w~Geld: ~g~${data.Money}");
                player.SendChatMessage($"~w~Bank: ~g~${data.BankMoney}");
                player.SendChatMessage($"~w~Job: ~y~{data.Job}");
            }
        }

        [Command("help")]
        public void CMD_Help(GTANetworkAPI.Player player)
        {
            player.SendChatMessage("~b~=== Verfügbare Befehle ===");
            player.SendChatMessage("~w~/stats - Zeigt deine Statistiken");
            player.SendChatMessage("~w~/help - Zeigt diese Hilfe");
        }
    }
}
