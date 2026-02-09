"use strict";

// Variablen für den Seatbelt-Status und die vorherige Fahrzeuggeschwindigkeit
let seatbeltActive = false;
let lastSpeed = 0;

// Dictionary, um Fahrzeuge zu speichern, die als stark beschädigt markiert wurden
let brokenVehicles = {};

// Empfang von Seatbelt-Status-Updates (z.B. über den /seatbelt-Befehl)
mp.events.add("updateSeatbelt", (status) => {
    seatbeltActive = status;
});

// Beim Betreten eines Fahrzeugs: Sicherheitsgurt zurücksetzen und, falls das Fahrzeug bereits beschädigt wurde, den Motor ausschalten
mp.events.add("playerEnterVehicle", (vehicle, seat) => {
    seatbeltActive = false;
    if (brokenVehicles[vehicle.remoteId]) {
        vehicle.setEngineOn(false, true, true);
    }
});

// Beim Verlassen des Fahrzeugs wird der Seatbelt-Status zurückgesetzt
mp.events.add("playerExitVehicle", () => {
    seatbeltActive = false;
});

// Funktion zur Überwachung der Fahrzeugdaten in kurzen Intervallen (z.B. 100ms)
function monitorVehicle() {
    let player = mp.players.local;
    if (!player.vehicle) {
        lastSpeed = 0;
        return;
    }
    
    let vehicle = player.vehicle;
    let currentSpeed = vehicle.getSpeed(); // Geschwindigkeit in m/s

    // Verwende die im Wiki dokumentierte Methode, um den BodyHealth-Wert zu erhalten
    let bodyHealth = vehicle.getBodyHealth();
    
    // Debug-Ausgabe zur Überprüfung (optional)
    

    // Festgelegte Schwellenwerte:
    const minCrashSpeed = 35.0;          // Mindestgeschwindigkeit, ab der ein Crash sinnvoll bewertet wird
    const decelerationThreshold = 35.0;  // Ein plötzlicher Geschwindigkeitsabfall (in m/s)
    const highSpeedThreshold = 42.0;     // Hohe Ausgangsgeschwindigkeit für schwerwiegende Crashs

    // Prüfe, ob ein starker Bremsvorgang stattgefunden hat, wenn der Spieler ausreichend schnell unterwegs war
    if (lastSpeed >= minCrashSpeed && (lastSpeed - currentSpeed) >= decelerationThreshold) {
        if (!seatbeltActive) {
            // Sende den von GTA berechneten BodyHealth-Wert an den Server
            mp.events.callRemote("handlePlayerCrash", bodyHealth);
        }
        // Zusätzlich: Bei hoher Geschwindigkeit Fahrzeug als stark beschädigt markieren und den Motor ausschalten
        if (lastSpeed >= highSpeedThreshold && !brokenVehicles[vehicle.remoteId]) {
            brokenVehicles[vehicle.remoteId] = true;
            vehicle.setEngineOn(false, true, true);
            mp.gui.chat.push("Fahrzeug stark beschädigt! Motor wurde abgeschaltet.");
        }
    }
    
    // Aktualisiere die letzte Geschwindigkeit für die nächste Überprüfung
    lastSpeed = currentSpeed;
}

// Starte die Überwachung jedes 100ms
setInterval(monitorVehicle, 100);
