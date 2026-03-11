using GTANetworkAPI;
using RPCore.Datenbank;

namespace RPCore.LSPD
{
    public class LSPD : Script
    {
        public LSPD()
        {
            NAPI.Ped.CreatePed(NAPI.Util.GetHashKey("s_m_y_cop_01"), new Vector3(447.213f, -980.714f, 30.689f), 155.632f, true, true, true);
        }

        [RemoteEvent("LSPD_DutyNpc_Interact")]
        public void OnNpcInteract(Player player)
        {
            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            if (acc == null || acc.Fraktion != 1) return;
            bool isOnDuty = acc.DutyStart.HasValue;
            NAPI.ClientEvent.TriggerClientEvent(player, "Faction:ShowMenu", "LSPD", isOnDuty);
        }
    }
}