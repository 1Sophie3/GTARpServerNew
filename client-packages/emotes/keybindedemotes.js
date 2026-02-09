let isHandsUp = false;

// Definiere eine Hilfsfunktion, die prüft, ob der Chat aktiv oder der Cursor sichtbar ist.
const shouldIgnoreKeyPress = () => {
    return mp.gui.chat.active || mp.gui.cursor.visible;
};

// H-Taste (Keycode 72): Hände hoch/runter
mp.keys.bind(72, false, () => {  // 72 = H (dezimal)
    if (shouldIgnoreKeyPress()) return; // Ignorieren, wenn Chat aktiv oder Cursor sichtbar

    // Verhindere das Abspielen des Emotes, wenn der Spieler in einem Fahrzeug sitzt
    if (mp.players.local.vehicle) return;

    if (!isHandsUp) {
        isHandsUp = true;
        mp.game.streaming.requestAnimDict("random@mugging3");
        let interval = setInterval(() => {
            if (mp.game.streaming.hasAnimDictLoaded("random@mugging3")) {
                clearInterval(interval);
                mp.players.local.taskPlayAnim("random@mugging3", "handsup_standing_base", 8.0, -8.0, -1, 49, 0, false, false, false);
            }
        }, 100);
    } else {
        isHandsUp = false;
        mp.players.local.clearTasks();
    }
});
