using GTANetworkAPI;
using RPCore.Datenbank;
using System.Linq;

namespace RPCore.LSMD
{
    public class LSMD : Script
    {
        public LSMD()
        {
            NAPI.Ped.CreatePed(NAPI.Util.GetHashKey("s_m_m_doctor_01"), new Vector3(315.043f, -592.215f, 43.265f), 164.404f, true, true, true);
        }

        [RemoteEvent("LSMD_DutyNpc_Interact")]
        public void OnNpcInteract(Player player)
        {
            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            if (acc == null || acc.Fraktion != 2) return;
            bool isOnDuty = acc.DutyStart.HasValue;
            NAPI.ClientEvent.TriggerClientEvent(player, "Faction:ShowMenu", "LSMD", isOnDuty);
        }

        [Command("mdrevive", "/mdrevive [Spieler-ID/Name]", GreedyArg = true)]
        public void MedicReviveCommand(Player medic, string targetIdOrName)
        {
            Accounts medicAccount = medic.GetData<Accounts>(Accounts.Account_Key);
            if (medicAccount == null || medicAccount.Fraktion != 2)
            {
                medic.SendNotification("~r~Du bist kein Mitglied des LSMD!");
                return;
            }
            if (medicAccount.DutyStart == null)
            {
                medic.SendNotification("~r~Du musst zuerst in den Dienst gehen!");
                return;
            }
            Player targetPlayer = FindPlayerByIdOrName(targetIdOrName);
            if (targetPlayer == null)
            {
                medic.SendNotification("~r~Patient nicht gefunden.");
                return;
            }
            if (medic.Position.DistanceTo(targetPlayer.Position) > 5.0f)
            {
                medic.SendNotification("~r~Du bist zu weit vom Patienten entfernt.");
                return;
            }
            if (targetPlayer.Health > 0)
            {
                medic.SendNotification("~r~Der Patient ist bei Bewusstsein.");
                return;
            }

            // Annahme: Du hast eine Revive-Methode in deiner Datenbank-Klasse.
            Datenbank.Datenbank.RevivePlayer(targetPlayer, 65, 0);

            medic.SendNotification($"~g~Du hast {targetPlayer.Name} erfolgreich stabilisiert.");
            targetPlayer.SendNotification("~g~Ein Sanitäter hat dich wiederbelebt.");
        }

        private Player FindPlayerByIdOrName(string identifier)
        {
            if (int.TryParse(identifier, out int id))
            {
                return NAPI.Pools.GetAllPlayers().FirstOrDefault(p => p.HasData(Accounts.Account_Key) && p.GetData<Accounts>(Accounts.Account_Key).ID == id);
            }
            return NAPI.Player.GetPlayerFromName(identifier);
        }
    }
}