using GTANetworkAPI;
using RPCore.Datenbank;

namespace RPCore.DrivingSchool
{
    public class DrivingSchool : Script
    {
        // Die ID, die wir für die Fahrschule verwenden.
        private const int FACTION_ID = 4;

        public DrivingSchool()
        {
            // Erstellt den Fahrlehrer-NPC an der von dir gewünschten Position.
            // Ich habe ein passendes Ped-Modell ausgewählt (ein Mann im Anzug).
            NAPI.Ped.CreatePed(
                NAPI.Util.GetHashKey("a_m_y_business_02"),
                new Vector3(-711.95, -1307.68, 5.11),
                31.14f,
                true, // isStatic
                true, // isInvincible
                true  // isFrozen
            );
        }

        // Dieses Event wird vom Client ausgelöst, wenn ein Spieler mit dem NPC interagiert.
        [RemoteEvent("DrivingSchool_DutyNpc_Interact")]
        public void OnNpcInteract(Player player)
        {
            Accounts acc = player.GetData<Accounts>(Accounts.Account_Key);
            // Prüft, ob der Spieler Mitglied der Fahrschul-Fraktion ist.
            if (acc == null || acc.Fraktion != FACTION_ID) return;

            bool isOnDuty = acc.DutyStart.HasValue;

            // Löst das Event aus, um das Fraktionsmenü im Browser des Spielers zu öffnen.
            NAPI.ClientEvent.TriggerClientEvent(player, "Faction:ShowMenu", "Fahrschule", isOnDuty);
        }
    }
}