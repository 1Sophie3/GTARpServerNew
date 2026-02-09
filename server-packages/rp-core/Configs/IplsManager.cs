using GTANetworkAPI;
using System.Collections.Generic;

namespace RPCore.Configs
{
    public static class IplManager
    {
        // Private Liste für die Flugzeugträger-IPLs, um alles an einem Ort zu haben.
        private static readonly List<string> _carrierIpls = new List<string>
        {
            "hei_carrier", "hei_carrier_DistantLights", "hei_carrier_int1", "hei_carrier_int2",
            "hei_carrier_int3", "hei_carrier_int4", "hei_carrier_int5", "hei_carrier_int6",
            "hei_carrier_lod", "hei_carrier_slod"
        };

        // Eine zentrale Methode, um alle custom IPLs zu laden.
        public static void LoadAllCustomIpls()
        {
            NAPI.Util.ConsoleOutput("[IPL Manager] Lade alle benutzerdefinierten IPLs...");

            // --- Standard-IPLs entfernen ---
            NAPI.World.RemoveIpl("rc12b_destroyed");
            NAPI.World.RemoveIpl("rc12b_default");
            NAPI.World.RemoveIpl("rc12b_hospitalinterior_lod");
            NAPI.World.RemoveIpl("rc12b_hospitalinterior");
            NAPI.World.RemoveIpl("rc12b_fixed");

            // --- Flugzeugträger laden ---
            foreach (var ipl in _carrierIpls)
            {
                NAPI.World.RequestIpl(ipl);
            }
            NAPI.Util.ConsoleOutput("[IPL Manager] Flugzeugträger geladen.");

            // --- Cayo Perico laden ---
            LoadCayoPerico();

            NAPI.Util.ConsoleOutput("[IPL Manager] Alle IPLs erfolgreich geladen.");
        }

        // Eine dedizierte Methode nur für Cayo Perico, für mehr Übersicht.
        // In der Datei: IplManager.cs

        // Ersetze die komplette Methode "LoadCayoPerico" mit dieser hier.
        private static void LoadCayoPerico()
        {
            NAPI.Util.ConsoleOutput("[IPL Manager] Lade Cayo Perico (Vollständige Version)...");

            // Entfernt die unsichtbare Kollision um die Insel herum
            NAPI.World.RemoveIpl("h4_fake_islandx");

            // Entfernt ein Objekt, das die Hauptkarte (Los Santos) ausblendet, wenn man auf der Insel ist.
            // DIES IST EIN SEHR WICHTIGER SCHRITT!
            NAPI.World.RemoveIpl("h4_islandx_terrain_03_lod");

            // --- Insel-Terrain laden ---
            // Diese laden die eigentliche Landmasse der Insel.
            NAPI.World.RequestIpl("h4_islandx_terrain_01");
            NAPI.World.RequestIpl("h4_islandx_terrain_02");
            NAPI.World.RequestIpl("h4_islandx_terrain_04");
            NAPI.World.RequestIpl("h4_islandx_terrain_05");
            NAPI.World.RequestIpl("h4_islandx_terrain_06");
            NAPI.World.RequestIpl("h4_islandx_terrain_props_01_lod");
            NAPI.World.RequestIpl("h4_islandx_terrain_props_02_lod");
            NAPI.World.RequestIpl("h4_islandx_terrain_props_03_lod");

            // --- Allgemeine Insel-Objekte ---
            NAPI.World.RequestIpl("h4_islandx");
            NAPI.World.RequestIpl("h4_islandx_props");
            NAPI.World.RequestIpl("h4_islandx_props_lod");
            NAPI.World.RequestIpl("h4_islandy");
            NAPI.World.RequestIpl("h4_islandy_props");
            NAPI.World.RequestIpl("h4_islandy_props_lod");
            NAPI.World.RequestIpl("h4_islandylocs");
            NAPI.World.RequestIpl("h4_islandylocs_lod");
            NAPI.World.RequestIpl("h4_islandx_barrack_hatch");

            // --- Strandbereiche ---
            NAPI.World.RequestIpl("h4_beach");
            NAPI.World.RequestIpl("h4_beach_lod");
            NAPI.World.RequestIpl("h4_beach_props");
            NAPI.World.RequestIpl("h4_beach_props_lod");
            NAPI.World.RequestIpl("h4_beach_party");
            NAPI.World.RequestIpl("h4_beach_party_lod");
            NAPI.World.RequestIpl("h4_islandx_beach_props");

            // --- Anwesen & Tor ---
            NAPI.World.RequestIpl("h4_islandx_mansion");
            NAPI.World.RequestIpl("h4_islandx_mansion_props");
            NAPI.World.RequestIpl("h4_islandx_mansion_props_lod");
            NAPI.World.RequestIpl("h4_islandx_mansion_entrance_gate");

            // --- Hauptdock ---
            NAPI.World.RequestIpl("h4_islandx_maindock");
            NAPI.World.RequestIpl("h4_islandx_maindock_props");
            NAPI.World.RequestIpl("h4_islandx_maindock_props_2");

            // --- Flugplatz ---
            NAPI.World.RequestIpl("h4_islandairstrip");
            NAPI.World.RequestIpl("h4_islandairstrip_props");
            NAPI.World.RequestIpl("h4_islandairstrip_props_lod");

            NAPI.Util.ConsoleOutput("[IPL Manager] Cayo Perico wurde geladen.");
        }
    }
    }