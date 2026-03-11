using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using RPCore.Database;
namespace RPCore.Haussystem
{
    public class HousingManager
    {
        private static HousingManager? _instance;
        public static HousingManager Instance => _instance ??= new HousingManager();
        public List<Housingsystem> hausListe = new List<Housingsystem>();

        public Housingsystem HoleHausMitID(int id)
        {
            return hausListe.FirstOrDefault(h => h.id == id);
        }

        public Housingsystem HoleHausInReichweite(Player player, float distance = 1.5f)
        {
            return hausListe.FirstOrDefault(h => h != null && player.Position.DistanceTo(h.position) < distance);
        }

        public bool HatSpielerSchluessel(Player player, Housingsystem house)
        {
            return player.Name == house.besitzer;
        }

        // Hier können Datenbankoperationen ergänzt werden
        // Beispiel: Laden/Speichern von Häusern
    }
}