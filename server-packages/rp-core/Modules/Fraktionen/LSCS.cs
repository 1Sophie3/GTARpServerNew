using GTANetworkAPI;
using RPCore.Datenbank;

namespace RPCore.LSCS
{
    public class LSCS : Script
    {
        public LSCS()
        {
            NAPI.Ped.CreatePed(NAPI.Util.GetHashKey("s_m_m_mechanic_01"), new Vector3(460.0f, -570.0f, 28.0f), 90.0f, true, true, true);
        }

        [RemoteEvent("LSCS_DutyNpc_Interact")]
        public void OnNpcInteract(Player player)
        {
            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            if (acc == null || acc.Fraktion != 3) return;
            bool isOnDuty = acc.DutyStart.HasValue;
            NAPI.ClientEvent.TriggerClientEvent(player, "Faction:ShowMenu", "LSCS", isOnDuty);
        }
    }
}