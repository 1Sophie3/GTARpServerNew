using GTANetworkAPI;
using RPCore.FraktionsSystem;
using System.Threading.Tasks;

namespace RPCore.Events
{
    public class FactionEventHandler : Script
    {
        public FactionEventHandler()
        {
            // Initialisiert den Duty Timer, sobald das Skript geladen wird
            DutyManager.StartGlobalDutyTimer();
            NAPI.Util.ConsoleOutput("[CORE] Duty-Timer und Faction-Event-Handler wurden gestartet.");
        }

        

        // Hier werden die Events vom Client empfangen
        [RemoteEvent("Faction:StartDuty")]
        public void OnFactionStartDuty(Player player)
        {
            DutyManager.BeginDuty(player);
        }

        [RemoteEvent("Faction:EndDuty")]
        public async Task OnFactionEndDuty(Player player)
        {
            await DutyManager.EndDuty(player);
        }
    }
}