using GTANetworkAPI;
using System.Collections.Generic;

namespace RPCore.Configs
{
    /// <summary>
    /// Diese Klasse ist verantwortlich für das Erstellen aller statischen Blips auf der Karte.
    /// Sie wird einmal beim Serverstart aufgerufen, um die Welt mit Markierungen zu füllen.
    /// </summary>
    public static class BlipManager
    {
        // Private, schreibgeschützte Liste für die Positionen der 24/7 Shops.
        private static readonly List<Vector3> Shops247 = new List<Vector3>
        {
            new Vector3(268.437f, -983.643f, 29.369f),
            new Vector3(-582.279f, -1015.186f, 22.323f),
            new Vector3(-695.949f, -862.612f, 23.719f),
            new Vector3(-2541.662f, 2313.737f, 33.410f)
        };

        // Private, schreibgeschützte Liste für die Positionen der Tankstellen.
        private static readonly List<Vector3> gasStations = new List<Vector3>
        {
            new Vector3(49.4187, 2778.793, 58.043),
            new Vector3(263.894, 2606.463, 44.983),
            new Vector3(1039.958, 2671.134, 39.550),
            new Vector3(1207.260, 2657.308, 37.899),
            new Vector3(2539.685, 2594.192, 37.944),
            new Vector3(2679.858, 3263.946, 55.240),
            new Vector3(2005.055, 3773.887, 32.403),
            new Vector3(1687.156, 4929.392, 42.078),
            new Vector3(1701.314, 6416.028, 32.763),
            new Vector3(179.857, 6602.839, 31.868),
            new Vector3(-94.4619, 6419.594, 31.489),
            new Vector3(-2554.996, 2334.40, 33.078),
            new Vector3(-1800.375, 803.661, 138.651),
            new Vector3(-1437.622, -276.747, 46.207),
            new Vector3(-2096.243, -320.286, 13.168),
            new Vector3(-724.619, -935.163, 19.213),
            new Vector3(-526.019, -1211.003, 18.184),
            new Vector3(-70.2148, -1761.792, 29.534),
            new Vector3(265.648, -1261.309, 29.292),
            new Vector3(819.653, -1028.846, 26.404),
            new Vector3(1208.951, -1402.567, 35.224),
            new Vector3(1181.381, -330.847, 69.316),
            new Vector3(620.843, 269.100, 103.089),
            new Vector3(2581.321, 362.039, 108.468)
        };

        /// <summary>
        /// Erstellt alle Blips, die auf der Weltkarte angezeigt werden sollen.
        /// </summary>
        public static void CreateAllStaticBlips()
        {
            NAPI.Util.ConsoleOutput("[Blip Manager] Erstelle alle statischen Blips...");

            // --- Öffentliche Dienste & wichtige Orte ---
            Vector3 pillboxPosition = new Vector3(311.254, -592.420, 43.284);
            NAPI.Blip.CreateBlip(61, pillboxPosition, 1.0f, 1, "Pillbox Hospital", 255, 0, true, 0, NAPI.GlobalDimension);

            Vector3 missionRowPDPosition = new Vector3(428.611, -982.350, 30.711);
            NAPI.Blip.CreateBlip(60, missionRowPDPosition, 1.0f, 3, "Mission Row PD", 255, 0, true, 0, NAPI.GlobalDimension);

            Vector3 paletopd = new Vector3(-441.685, 6018.802, 31.557);
            NAPI.Blip.CreateBlip(60, paletopd, 1.0f, 3, "Paleto PD", 255, 0, true, 0, NAPI.GlobalDimension);

            Vector3 paletoHospitalPosition = new Vector3(-247.553, 6331.574, 32.426);
            NAPI.Blip.CreateBlip(61, paletoHospitalPosition, 1.0f, 1, "Paleto Bay Hospital", 255, 0, true, 0, NAPI.GlobalDimension);

            // --- Zivile Einrichtungen ---
            Vector3 stateBankPosition = new Vector3(249.175, 217.843, 106.286);
            NAPI.Blip.CreateBlip(108, stateBankPosition, 1.0f, 4, "Bank", 255, 0, true, 0, NAPI.GlobalDimension);

            Vector3 drivingschool = new Vector3(-706.690, -1302.299, 5.112);
            NAPI.Blip.CreateBlip(280, drivingschool, 1.0f, 4, "Fahrschule", 255, 0, true, 0, NAPI.GlobalDimension);

            Vector3 lcs = new Vector3(-214.360, -1317.489, 30.890);
            NAPI.Blip.CreateBlip(643, lcs, 1.0f, 4, "LCS", 255, 0, true, 0, NAPI.GlobalDimension);

            // --- Jobs ---
            Vector3 busjob = new Vector3(472.168, -585.925, 28.5);
            NAPI.Blip.CreateBlip(513, busjob, 1.0f, 4, "Bus Job", 255, 0, true, 0, NAPI.GlobalDimension);

            // --- Shops (aus der Liste) ---
            foreach (var shop in Shops247)
            {
                NAPI.Blip.CreateBlip(52, shop, 1.0f, 4, "24/7", 255, 0, true, 0, NAPI.GlobalDimension);
            }

            // --- Tankstellen (aus der Liste) ---
            foreach (var gasStation in gasStations)
            {
                NAPI.Blip.CreateBlip(361, gasStation, 1.0f, 4, "Tankstelle", 255, 0, true, 0, NAPI.GlobalDimension);
            }

            NAPI.Util.ConsoleOutput("[Blip Manager] Alle Blips erfolgreich erstellt.");
        }
    }
}