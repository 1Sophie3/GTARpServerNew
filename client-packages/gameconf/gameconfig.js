// Nametag deaktivieren
mp.nametags.enabled = false;

// Diesen Block komplett neu am Ende der Datei hinzufügen oder den alten ersetzen.

// Workaround, um die Idle-Kamera zu deaktivieren, indem der Idle-Timer regelmäßig zurückgesetzt wird.
setInterval(() => {
    // Diese native Funktion (_UPDATE_PLAYER_TELEPORT) wird normalerweise für den Rockstar Editor
    // verwendet, hat aber den nützlichen Nebeneffekt, den AFK-/Idle-Timer des Spiels zurückzusetzen.
    // So denkt das Spiel, du wärst nie lange genug inaktiv, um die Kamera zu starten.
   
	mp.game.invoke('0x9E4CFFF989258472');
	mp.game.invoke('0xF4F2C0D4EE209E20');
    
}, 20000); // Führt den Befehl alle 20.000 Millisekunden (20 Sekunden) aus.


// Wir geben weiterhin eine Bestätigung aus, dass der Workaround geladen wurde.
mp.events.add('playerReady', () => {
  //  mp.gui.chat.push("Info: Idle-Kamera-Workaround ist aktiv.");
});