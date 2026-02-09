const player = mp.players.local;
const fishingRodHash = mp.game.joaat('WEAPON_FISHINGROD'); // Der "Waffen"-Hash für die Angel

let isFishing = false;
let controlsInterval = null;

// --- Helper-Funktionen zum Sperren/Entsperren der Steuerung ---
const controlsToDisable = [24, 25, 37, 69, 92, 114, 140, 141, 142, 257];
function disableControls() {
    if (controlsInterval) return;
    controlsInterval = setInterval(() => {
        controlsToDisable.forEach(control => {
            mp.game.controls.disableControlAction(0, control, true);
        });
    }, 0);
}

function enableControls() {
    if (controlsInterval) {
        clearInterval(controlsInterval);
        controlsInterval = null;
    }
}

// Wird NUR EINMAL pro Angel-Session aufgerufen (/startfish)
mp.events.add('client:fishing:start', () => {
    if (isFishing) return;
    isFishing = true;

    // Gib dem Spieler die Angel als "Waffe"
    player.giveWeapon(fishingRodHash, 1, true);
    
    // Rüste die Angel aus
    mp.game.invoke('0xADF692B254977C0C', player.handle, fishingRodHash, true); // NATIVE: SET_CURRENT_PED_WEAPON

    // Starte das Angel-Szenario
    player.taskStartScenarioInPlace("WORLD_HUMAN_STAND_FISHING", 0, true);

    // Deaktiviere die Steuerung
    disableControls();
});

// Wird NUR aufgerufen, wenn das Angeln wirklich beendet wird
mp.events.add('client:fishing:stop', () => {
    if (!isFishing) return;
    isFishing = false;

    // === FINALE, FORCIERTE STOP-LOGIK ===

    // 1. Zwinge den Spieler, auf "Unbewaffnet" zu wechseln. 
    //    Dies ist der entscheidende Befehl, um das Fallenlassen zu verhindern.
    mp.game.invoke('0xADF692B254977C0C', player.handle, mp.game.joaat('WEAPON_UNARMED'), true);

    // 2. Angel-Waffe aus dem Inventar entfernen.
    player.removeWeapon(fishingRodHash);

    // 3. Alle Tasks (inkl. Angel-Szenario) sofort beenden.
    player.clearTasksImmediately();

    // 4. Steuerung sofort wieder freigeben.
    enableControls();
});


// #############################################################################
// Der UI-Code bleibt unverändert
// #############################################################################

let continuePromptBrowser = null;
let promptTimer = null;

mp.events.add('client:fishing:showContinuePrompt', () => {
    if (continuePromptBrowser) closeContinuePrompt();
    
    continuePromptBrowser = mp.browsers.new('package://fishingking/continuePrompt.html');
    mp.gui.cursor.show(true, true);

    promptTimer = setTimeout(() => {
        if (continuePromptBrowser) {
            mp.events.callRemote('server:fishing:continue', false);
            closeContinuePrompt();
        }
    }, 5000); 
});

mp.events.add('client:fishing:promptResponse', (response) => {
    mp.events.callRemote('server:fishing:continue', response);
    closeContinuePrompt();
});

function closeContinuePrompt() {
    if (continuePromptBrowser) {
        continuePromptBrowser.destroy();
        continuePromptBrowser = null;
        mp.gui.cursor.show(false, false);
    }
    if (promptTimer) {
        clearTimeout(promptTimer);
        promptTimer = null;
    }
}

mp.keys.bind(0x45, true, () => {
    if (continuePromptBrowser !== null) {
        mp.events.callRemote('server:fishing:continue', true);
        closeContinuePrompt();
    }
});