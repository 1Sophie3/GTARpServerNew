
using Newtonsoft.Json.Linq;
using System;

namespace RPCore.CharCreator
{
    // Handler: Nimmt Events entgegen und ruft Controller auf (plattformunabhängig)
    // Die Kommunikation mit dem Frontend (z.B. Vue/CEF) erfolgt über Adapter/Service, siehe unten.
    class CharCreatorHandler
    {
        private readonly CharCreatorController _controller = new CharCreatorController();

        // Diese Methoden werden von der jeweiligen Plattform aufgerufen (z.B. Event, API-Call)
        public void HandleSetPlayerGender(object player, string gender)
        {
            _controller.SetPlayerGender(player, gender);
        }

        public void HandleCharacterCreated(object player, string character, bool created)
        {
            _controller.CharacterCreated(player, character, created);
        }

        // Methode: Wird vom Frontend (z.B. Vue) aufgerufen, um finale Charakterdaten zu übergeben
        public void ReceiveCharacterDataFromFrontend(object player, string characterJson)
        {
            // Hier werden die vom Frontend (Vue/CEF) übermittelten Daten an das Spiel weitergegeben
            // z.B. Speicherung, Anwendung auf den Spieler, Validierung etc.
            _controller.ApplyCharacterData(player, characterJson);
        }

        // Methode: Lädt die aktuellen Charakterdaten aus der DB und sendet sie ans Frontend (z.B. für Frisör, Klinik)
        public void LoadCurrentCharacterData(object player)
        {
            // Beispiel: Charakterdaten aus der DB laden
            // string characterJson = CharacterService.LoadCharacterData(player);
            // Dann an das Frontend senden:
            // FrontendService.SendToVue(player, "charcreator-data", characterJson);
        }

        // Methode: Wird aufgerufen, wenn der Spieler Anpassungen abbricht (z.B. Frisör/Klinik)
        public void CancelCharacterEdit(object player)
        {
            // Setzt das Aussehen des Spielers auf den letzten gespeicherten Stand zurück
            // string originalCharacterJson = CharacterService.LoadCharacterData(player);
            _controller.ApplyCharacterPreview(player, /*originalCharacterJson*/ "");
            // FrontendService.TriggerCloseCharCreator(player);
        }

        // Methode: Wird vom Frontend aufgerufen, um eine Live-Vorschau des Charakters zu ermöglichen
        public void UpdateCharacterPreview(object player, string previewDataJson)
        {
            // Diese Methode wird bei jeder Änderung im CharCreator-Frontend aufgerufen
            // Sie sorgt dafür, dass der Charakter im Spiel live aktualisiert wird (z.B. Aussehen, Kleidung, etc.)
            _controller.ApplyCharacterPreview(player, previewDataJson);
        }

        // Beispiel: Datenübergabe an das Vue-Frontend (Pseudo-Code)
        // public void SendCharacterDataToFrontend(object player, string characterJson)
        // {
        //     // Adapter/Service für CEF/Vue/HTML aufrufen, z.B.:
        //     // FrontendService.SendToVue(player, "charcreator-data", characterJson);
        // }
    }

    // Controller: Kapselt Logik, ruft Charakter-Modul auf (plattformunabhängig)
    class CharCreatorController
    {
        private readonly CharCustomizationLogicBase _charLogic;

        public CharCreatorController()
        {
            // Plattform-Resolver (z.B. per DI, Factory, Config)
            _charLogic = new CharCustomizationLogic();
        }

        public void SetPlayerGender(object player, string gender)
        {
            _charLogic.ApplyGender(player, gender);
        }

        public void CharacterCreated(object player, string character, bool created)
        {
            // Die folgenden Methoden müssen plattformunabhängig implementiert werden
            // Accounts-Handling, Name setzen, Speichern etc. erfolgt in Adapter/Service
            _charLogic.ApplyCustomization(player, character);

            // Beispiel: Übergabe der Daten an einen Service/Adapter (Pseudo-Code)
            // CharacterService.UpdateCharacterData(player, character);
            // CharacterService.SetPlayerName(player, ...);
            // CharacterService.SaveAccount(player);

            // Beispiel: Datenübergabe an das Vue-Frontend (Pseudo-Code)
            // FrontendService.SendToVue(player, "charcreator-data", character);

            // Event/Callback für das Frontend, z.B. Hide-Charcreator
            if (created)
            {
                // FrontendService.TriggerHideCharCreator(player);
            }
        }

        // Methode: Übernimmt finale Charakterdaten vom Frontend und verarbeitet sie
        public void ApplyCharacterData(object player, string characterJson)
        {
            // Hier erfolgt die eigentliche Anwendung der Daten auf den Spieler
            // z.B. Validierung, Speicherung, Anwendung auf das Spielobjekt
            _charLogic.ApplyCustomization(player, characterJson);

            // Hier erfolgt die Speicherung der Charakterdaten in der Datenbank
            // Beispiel: CharacterService.SaveCharacterData(player, characterJson);
            // Die konkrete Implementierung hängt von deinem Datenbanksystem ab
        }

        // Methode: Übernimmt Vorschau-Daten vom Frontend und aktualisiert den Charakter live
        public void ApplyCharacterPreview(object player, string previewDataJson)
        {
            // Hier wird das Aussehen des Spielers temporär angepasst, ohne zu speichern
            // z.B. für Live-Vorschau im CharCreator
            _charLogic.ApplyCustomization(player, previewDataJson);
            // Keine Speicherung, nur temporäre Anwendung für Vorschau
        }
    }
}

