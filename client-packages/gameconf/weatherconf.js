/*
  weatherconf.js – Client-Skript für RageMP
  Dieses Script lädt über einen versteckten CEF-Browser die HTML-Datei aus dem Ordner "client_packages/gameconf/weather.html".
  Diese HTML-Seite ruft aktuell das Wetter von OpenWeatherMap ab und triggert das Event "updateWeather" mit dem ermittelten Wettertyp.
  Anschließend wird im Client der Wettertyp via mp.game.invoke angewendet.
*/

// CEF-Browser laden, der weather.html aus dem Unterordner "gameconf" verwendet
let weatherBrowser = mp.browsers.new("package://gameconf/weather.html");

// Event-Handler: Reagiere auf den vom CEF-Browser getriggerten Wetter-Update
mp.events.add("updateWeather", (weatherType) => {
    // Prüfe, ob mp.game.invoke existiert – das ist notwendig, um natives Wetter zu ändern
    if (!mp.game || typeof mp.game.invoke !== "function") {
        console.error("Fehler: mp.game.invoke ist nicht verfügbar.");
        return;
    }

    // Setze das Ingame-Wetter mithilfe der native Funktion via mp.game.invoke.
    // Die Hash '0xA43D5C6FE51ADBEF' entspricht dem native Aufruf, der das Wetter ändert.
    mp.game.invoke("0xA43D5C6FE51ADBEF", weatherType);
    console.log("Spiel-Wetter aktualisiert auf:", weatherType);
});
