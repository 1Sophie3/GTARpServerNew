const E_KEY = 0x45; // Taste "E"

mp.keys.bind(E_KEY, true, function() {
    // Diese Nachricht ist der erste Test.
    
    // Sende das Event an den Server.
    mp.events.callRemote('OnPlayerPressE_Parachute');
});

// Wir entfernen zur Sicherheit alle anderen Events, um Konflikte auszuschließen.
let currentOpenMenu = null;