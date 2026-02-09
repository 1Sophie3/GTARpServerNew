// =========================================
// COMPLETE CLIENTSIDE SCRIPT – BUSJOB, HALTESTELLEN, FREEZE & NOTIFICATIONS
// =========================================

// Erzeuge den Busfahrer-NPC
let busDriverNPC = mp.peds.new(
    mp.game.joaat("ig_andreas"),
    new mp.Vector3(449.991, -658.367, 28.443),
    -81.97838,
    0
);

// Lokales Busjob-Blip
let localBusJobBlip = null;

// Variablen für Bushaltestellenobjekte ("CoolShapes")
let currentBusStopMarker = null;
let currentBusStopColShape = null;
let currentBusStopPos = null;
let currentBusStopIndex = -1;

// Flag, um Mehrfachtrigger pro Haltestelle zu verhindern
let busStopTriggered = false;

// Definierter horizontaler Trigger-Radius in Metern
const busStopTriggerRange = 5.0;

// ----- KEYBINDING: F -----
mp.keys.bind(0x46, true, () => {
    let playerPos = mp.players.local.position;
    let npcPos = busDriverNPC.position;
    let distance = mp.game.system.vdist(playerPos.x, playerPos.y, playerPos.z, npcPos.x, npcPos.y, npcPos.z);
    if (distance < 3.0) {
        mp.events.callRemote("OnPlayerPressF");
    }
});

// ----- BUSJOB-BLIP -----
mp.events.add("createBusJobBlip", (x, y, z) => {
    if (localBusJobBlip) { localBusJobBlip.destroy(); localBusJobBlip = null; }
    localBusJobBlip = mp.blips.new(513, new mp.Vector3(x, y, z), {
        color: 46,
        shortRange: true,
        name: "Dein Bus"
    });
});
mp.events.add("removeBusJobBlip", () => {
    if (localBusJobBlip) { localBusJobBlip.destroy(); localBusJobBlip = null; }
});

// ----- WAYPOINT -----
mp.events.add("setBusStopWaypoint", (x, y) => {
    mp.game.ui.setNewWaypoint(x, y);
});

// ----- HALTESTELLEN-OBJEKTE ("COOLSHAPES") -----
mp.events.add("createBusStopObjects", (x, y, z, stopIndex) => {
    if (currentBusStopMarker) { currentBusStopMarker.destroy(); currentBusStopMarker = null; }
    if (currentBusStopColShape) { currentBusStopColShape.destroy(); currentBusStopColShape = null; }
    
    currentBusStopPos = new mp.Vector3(x, y, z);
    currentBusStopIndex = stopIndex;
    busStopTriggered = false;
    
    currentBusStopMarker = mp.markers.new(1, new mp.Vector3(x, y, z + 1), 5.0, {
        color: { r:255, g:0, b:0, a:200 }
    });
    currentBusStopColShape = mp.colshapes.newSphere(x, y, z, busStopTriggerRange);
});

// ----- TRIGGER & FREEZE -----
// Diese Events sollen nur feuern, wenn der Spieler im richtigen Bus fährt.
// Sie bleiben unverändert, denn hier wird über ColShape-Events getriggert.
mp.events.add("playerEnterColshape", (colshape) => {
    if (currentBusStopColShape && colshape === currentBusStopColShape && !busStopTriggered) {
        if (!mp.players.local.vehicle) return;
        busStopTriggered = true;
        mp.players.local.freezePosition(true);
        if (mp.players.local.vehicle)
            mp.players.local.vehicle.freezePosition(true);
        mp.events.callRemote("BusStopReached", currentBusStopIndex);
    }
});
mp.events.add("entityEnterColshape", (entity, colshape) => {
    if ((entity === mp.players.local || (mp.players.local.vehicle && entity === mp.players.local.vehicle))
         && currentBusStopColShape && colshape === currentBusStopColShape && !busStopTriggered) {
        if (!mp.players.local.vehicle) return;
        busStopTriggered = true;
        mp.players.local.freezePosition(true);
        if (mp.players.local.vehicle)
            mp.players.local.vehicle.freezePosition(true);
        mp.events.callRemote("BusStopReached", currentBusStopIndex);
    }
});

// ----- FALLBACK: RENDER-LOOP -----
// Hier prüfen wir kontinuierlich den horizontalen Abstand zwischen deinem (Fahrzeug-)Position und der Bushaltestelle.
// Zusätzlich wird der Triggerradius dynamisch vergrößert, wenn du schnell fährst.
mp.events.add("render", () => {
    if (currentBusStopPos && !busStopTriggered) {
        let pos = mp.players.local.position;
        // Wenn im Fahrzeug, nutze Fahrzeugposition und ermittle Geschwindigkeit
        let effectiveRadius = busStopTriggerRange;
        if (mp.players.local.vehicle) {
            pos = mp.players.local.vehicle.position;
            let speed = mp.players.local.vehicle.getSpeed(); // Annahme: getSpeed() gibt m/s zurück
            // Erhöhe den effektiven Radius um beispielhaft 2,5 Meter, wenn schneller als 10 m/s (ca. 36 km/h)
            if (speed > 10) { effectiveRadius += 2.5; }
        }
        let dx = pos.x - currentBusStopPos.x;
        let dy = pos.y - currentBusStopPos.y;
        let horizontalDist = Math.sqrt(dx * dx + dy * dy);
        if (horizontalDist <= effectiveRadius) {
            busStopTriggered = true;
            mp.players.local.freezePosition(true);
            if (mp.players.local.vehicle)
                mp.players.local.vehicle.freezePosition(true);
            mp.events.callRemote("BusStopReached", currentBusStopIndex);
            // Nachdem Trigger erfolgt, leere die aktuell gesetzten Haltestellendaten
            currentBusStopPos = null;
            currentBusStopIndex = -1;
        }
    }
});

// ----- UNFREEZE-Events -----
mp.events.add("ContinueBusJob", () => {
    mp.players.local.freezePosition(false);
    if (mp.players.local.vehicle)
        mp.players.local.vehicle.freezePosition(false);
});
mp.events.add("unfreezePlayer", () => {
    mp.players.local.freezePosition(false);
    if (mp.players.local.vehicle)
        mp.players.local.vehicle.freezePosition(false);
});
