let noclip = false;
let baseSpeed = 1.0; // Basis-Tempo; passe diesen Wert an dein Empfinden an

// Berechnet die Vorwärtsrichtung basierend auf der Kamerarotation
function getCamDirection() {
    let camRot = mp.game.cam.getGameplayCamRot(2);
    let radPitch = camRot.x * (Math.PI / 180.0);
    let radYaw = camRot.z * (Math.PI / 180.0);
    return { 
        x: -Math.sin(radYaw) * Math.cos(radPitch),
        y: Math.cos(radYaw) * Math.cos(radPitch),
        z: Math.sin(radPitch)
    };
}

// Berechnet die Rechtsrichtung (als Kreuzprodukt der Vorwärts- und Aufwärtsrichtung)
function getCamRightVector() {
    let forward = getCamDirection();
    let up = { x: 0, y: 0, z: 1 };
    let right = { 
        x: forward.y * up.z - forward.z * up.y,
        y: forward.z * up.x - forward.x * up.z,
        z: forward.x * up.y - forward.y * up.x
    };
    let length = Math.sqrt(right.x * right.x + right.y * right.y + right.z * right.z);
    return { 
        x: right.x / length, 
        y: right.y / length, 
        z: right.z / length 
    };
}

// Wird vom Server aufgerufen, um den Noclip-Modus umzuschalten
mp.events.add("toggleNoclip", (enabled) => {
    noclip = enabled;
    let localPlayer = mp.players.local;

    if (noclip) {
        // Aktivierung: Position einfrieren, Kollision deaktivieren und Spieler unsichtbar machen
        localPlayer.freezePosition = true;
        localPlayer.setCollision(false, false);
        localPlayer.setVisible(false, false); // Spieler unsichtbar
    } else {
        // Deaktivierung: Eigenschaften zurücksetzen
        localPlayer.freezePosition = false;
        localPlayer.setCollision(true, true);
        localPlayer.setVisible(true, true); // Spieler wieder sichtbar
    }
});

// Jeder Frame wird geprüft, ob Noclip aktiv ist und, falls ja, die Bewegungslogik abgearbeitet.
mp.events.add("render", () => {
    if (noclip) {
        let localPlayer = mp.players.local;
        let pos = localPlayer.position;
        let forward = getCamDirection();
        let right = getCamRightVector();

        // Erhöhe die Bewegungsgeschwindigkeit, wenn Shift (Taste 16) gedrückt wird
        let speedMultiplier = mp.keys.isDown(16) ? 3.0 : 1.0;
        let movementSpeed = baseSpeed * speedMultiplier;

        // Bewegung:
        // W/S: Vorwärts / Rückwärts
        if (mp.keys.isDown(87)) { // W
            pos.x += forward.x * movementSpeed;
            pos.y += forward.y * movementSpeed;
            pos.z += forward.z * movementSpeed;
        }
        if (mp.keys.isDown(83)) { // S
            pos.x -= forward.x * movementSpeed;
            pos.y -= forward.y * movementSpeed;
            pos.z -= forward.z * movementSpeed;
        }
        // A/D: Links/Rechts
        if (mp.keys.isDown(65)) { // A
            pos.x -= right.x * movementSpeed;
            pos.y -= right.y * movementSpeed;
            pos.z -= right.z * movementSpeed;
        }
        if (mp.keys.isDown(68)) { // D
            pos.x += right.x * movementSpeed;
            pos.y += right.y * movementSpeed;
            pos.z += right.z * movementSpeed;
        }
        // Q/E: Abwärts / Aufwärts
        if (mp.keys.isDown(81)) { // Q
            pos.z -= movementSpeed;
        }
        if (mp.keys.isDown(69)) { // E
            pos.z += movementSpeed;
        }

        localPlayer.position = pos;
    }
});
