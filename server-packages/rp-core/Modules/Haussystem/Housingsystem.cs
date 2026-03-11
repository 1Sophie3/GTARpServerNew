using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RPCore.Haussystem
{
    public class Housingsystem
    {
        // Datenmodell für Haus
        // Die Verwaltung und Datenbankzugriffe erfolgen im HousingManager
        public int id { get; set; }
        public string ipl { get; set; }
        public Vector3 position { get; set; }
        public int preis { get; set; }
        public string besitzer { get; set; }
        public bool abgeschlossen { get; set; }
        public bool status { get; set; }
        public TextLabel hausLabel { get; set; }
        public Marker hausMarker { get; set; }

        // Die Methoden zur Hausverwaltung werden in HousingManager ausgelagert
        // Beispiel: HousingManager.Instance.HoleHausMitID(id)
        // Beispiel: HousingManager.Instance.HoleHausInReichweite(player, distance)
        // Beispiel: HousingManager.Instance.HatSpielerSchluessel(player, haus)
        // ...existing code...
    }
}

