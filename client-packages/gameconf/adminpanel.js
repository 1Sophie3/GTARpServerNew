let adminPanel = null;
let panelOpen = false;
const player = mp.players.local;

// Zustand-Variablen für Noclip und Spectate
let noclip = false;
let noclipCamera;
let isSpectating = false;
let spectateTarget = null;
let spectateCamera;

// --- Panel öffnen/schließen ---
mp.keys.bind(0x74, true, () => { // F5
    if (panelOpen) {
        closeAdminPanel();
    } else {
        mp.gui.chat.activate(false);
        mp.gui.cursor.show(true, true);
        mp.events.callRemote('adminPanel:requestOpen');
    }
});

mp.events.add('adminPanel:show', (adminLevel, adminId, playersJson, housesJson, tpLocationsJson, supportTicketsJson) => {
    if (adminPanel) return;
    panelOpen = true;
    adminPanel = mp.browsers.new('package://gameconf/adminpanel.html');
    mp.events.add('browserDomReady', browser => {
        if (browser === adminPanel) {
            browser.execute(`initializePanel(${adminLevel}, ${adminId}, ${playersJson}, ${housesJson}, ${tpLocationsJson}, ${supportTicketsJson});`);
        }
    });
});

function closeAdminPanel() {
    if (panelOpen) {
        if (adminPanel) {
            adminPanel.destroy();
            adminPanel = null;
        }
        mp.gui.cursor.show(false, false);
        mp.gui.chat.activate(true);
        panelOpen = false;
    }
}

// --- Events vom Server an den Client ---
mp.events.add('admin:doFreeze', (shouldBeFrozen) => {
    player.freezePosition(shouldBeFrozen);
});

mp.events.add('spectate:start', (target) => {
    if (target && mp.players.exists(target)) {
        isSpectating = true;
        spectateTarget = target;
        player.setVisible(false, false);
        player.setInvincible(true);
        player.setCollision(false, true);
        spectateCamera = mp.cameras.new('default', player.position, new mp.Vector3(0,0,0), 50);
        spectateCamera.setActive(true);
        mp.game.cam.renderScriptCams(true, false, 0, true, false);
    }
});

mp.events.add('spectate:stop', () => {
    isSpectating = false;
    spectateTarget = null;
    player.setVisible(true, false);
    player.setInvincible(false);
    player.setCollision(true, true);
    if (spectateCamera) {
        spectateCamera.destroy();
        spectateCamera = null;
    }
    mp.game.cam.renderScriptCams(false, false, 0, true, false);
});


// --- Events vom UI (HTML) zum Server ---
mp.events.add('adminPanel:client:close', closeAdminPanel);
mp.events.add('adminPanel:client:kick', (targetId, reason) => mp.events.callRemote('adminPanel:kick', targetId, reason));
mp.events.add('adminPanel:client:ban', (targetId, reason) => mp.events.callRemote('adminPanel:ban', targetId, reason));
mp.events.add('adminPanel:client:unban', (targetId) => mp.events.callRemote('adminPanel:unban', targetId));
mp.events.add('adminPanel:client:gethere', (targetId) => { mp.events.callRemote('adminPanel:gethere', targetId); });
mp.events.add('adminPanel:client:heal', (targetId) => mp.events.callRemote('adminPanel:heal', targetId));
mp.events.add('adminPanel:client:revive', (targetId) => mp.events.callRemote('adminPanel:revivePlayer', targetId));
mp.events.add('adminPanel:client:giveMoney', (targetId, amount, type) => mp.events.callRemote('adminPanel:giveMoney', targetId, amount, type));
mp.events.add('adminPanel:client:giveWeapon', (targetId, weapon) => mp.events.callRemote('adminPanel:giveWeapon', targetId, weapon));
mp.events.add('adminPanel:client:toggleSpectate', (targetId) => mp.events.callRemote('adminPanel:toggleSpectate', targetId));
mp.events.add('adminPanel:client:toggleFreeze', (targetId) => mp.events.callRemote('adminPanel:toggleFreeze', targetId));
mp.events.add('adminPanel:client:setPlayerDimension', (targetId, dim) => mp.events.callRemote('adminPanel:setPlayerDimension', targetId, dim));
mp.events.add('adminPanel:client:toggleAduty', () => { closeAdminPanel(); mp.events.callRemote('adminPanel:toggleAduty'); });
mp.events.add('adminPanel:client:toggleInvisibility', () => { closeAdminPanel(); mp.events.callRemote('adminPanel:toggleInvisibility'); });
mp.events.add('adminPanel:client:spawnAdminVehicle', () => { closeAdminPanel(); mp.events.callRemote('adminPanel:spawnAdminVehicle'); });
mp.events.add('adminPanel:client:goBack', () => { mp.events.callRemote('adminPanel:goBack'); });
mp.events.add('adminPanel:client:toggleGodMode', () => mp.events.callRemote('adminPanel:toggleGodMode'));
mp.events.add('adminPanel:client:toggleNoClip', () => { closeAdminPanel(); mp.events.callRemote('adminPanel:toggleNoClip'); });
mp.events.add('adminPanel:client:createPersVehicle', (model, ownerId, color1, color2, plate) => mp.events.callRemote('adminPanel:createPersVehicle', model, ownerId, color1, color2, plate));
mp.events.add('adminPanel:client:createFactionVehicle', (model, factionId, plate, color1, color2) => mp.events.callRemote('adminPanel:createFactionVehicle', model, factionId, plate, color1, color2));
mp.events.add('adminPanel:client:spawnTempVehicle', (model) => mp.events.callRemote('adminPanel:spawnTempVehicle', model));
mp.events.add('adminPanel:client:forceToggleLock', (isLocked) => mp.events.callRemote('adminPanel:forceToggleLock', isLocked));
mp.events.add('adminPanel:client:forceToggleEngine', (status) => mp.events.callRemote('adminPanel:forceToggleEngine', status));
mp.events.add('adminPanel:client:setHouseOwner', (houseId, ownerId) => mp.events.callRemote('adminPanel:setHouseOwner', houseId, ownerId));
mp.events.add('adminPanel:client:sendAdminChat', (message) => mp.events.callRemote('adminPanel:sendAdminChat', message));
mp.events.add('adminPanel:client:sendAnnouncement', (message) => { closeAdminPanel(); mp.events.callRemote('adminPanel:sendAnnouncement', message); });

// --- Fahrzeug-Events vom Admin-Panel (inklusive der neuen) ---
mp.events.add('adminPanel:client:tptoVehicle', (vehId) => mp.events.callRemote('adminPanel:tptoVehicle', vehId));
mp.events.add('adminPanel:client:parkVehicleInAlta', (vehId) => mp.events.callRemote('adminPanel:parkVehicleInAlta', vehId));
mp.events.add('adminPanel:client:deleteVehicleDB', (vehId) => mp.events.callRemote('adminPanel:deleteVehicleDB', vehId));
mp.events.add('adminPanel:client:fetchVehicle', (vehId) => mp.events.callRemote('adminPanel:fetchVehicle', vehId));
mp.events.add('adminPanel:client:repairVehicle', (vehId) => mp.events.callRemote('adminPanel:repairVehicle', vehId));


// Korrigierte Teleport-Events
mp.events.add('adminPanel:client:goto', (targetId) => {
    mp.events.callRemote('adminPanel:goto', targetId);
    setTimeout(closeAdminPanel, 200);
});
mp.events.add('adminPanel:client:teleportToLocation', (locName, withVeh) => {
    mp.events.callRemote('adminPanel:teleportToLocation', locName, withVeh);
    setTimeout(closeAdminPanel, 200);
});
mp.events.add('adminPanel:client:teleportToCoords', (x, y, z) => {
    closeAdminPanel();
    setTimeout(() => {
        mp.events.callRemote('adminPanel:teleportToCoords', x, y, z);
    }, 50);
});
mp.events.add('adminPanel:client:teleportToHouse', (houseId) => {
    mp.events.callRemote('adminPanel:teleportToHouse', houseId);
    setTimeout(closeAdminPanel, 200);
});


// Events für das Support System
mp.events.add('adminPanel:client:support:updateStatus', (ticketId, status) => mp.events.callRemote('adminPanel:support:updateStatus', ticketId, status));
mp.events.add('adminPanel:client:support:setPriority', (ticketId, isPriority) => mp.events.callRemote('adminPanel:support:setPriority', ticketId, isPriority));
mp.events.add('adminPanel:client:support:addComment', (ticketId, comment) => mp.events.callRemote('adminPanel:support:addComment', ticketId, comment));

// Event vom Server, wenn ein neues Ticket erstellt wird
mp.events.add('support:newTicket', () => {
    if (panelOpen) {
        closeAdminPanel();
        mp.gui.chat.push("~p~[SUPPORT]~w~ Neues Ticket! Öffne das Panel neu (F5), um es zu sehen.");
    } else {
         mp.gui.chat.push("~p~[SUPPORT]~w~ Es ist ein neues Ticket eingegangen.");
    }
});

// --- Noclip Handler ---
mp.events.add('noclip:toggle', () => {
    noclip = !noclip;
    player.freezePosition(noclip);
    player.setInvincible(noclip);
    player.setVisible(!noclip, false);
    player.setCollision(!noclip, !noclip);

    if (noclip) {
        noclipCamera = mp.cameras.new('default', player.position, new mp.Vector3(0, 0, 0), 50);
        noclipCamera.setActive(true);
        mp.game.cam.renderScriptCams(true, false, 0, false, false);
    } else {
        if (noclipCamera) {
            noclipCamera.destroy();
            noclipCamera = null;
        }
        mp.game.cam.renderScriptCams(false, false, 0, true, false);
    }
    mp.gui.chat.push(noclip ? 'Noclip aktiviert.' : 'Noclip deaktiviert.');
});

// --- Render Loop für Noclip & Spectate ---
mp.events.add('render', () => {
    if (isSpectating && spectateTarget && mp.players.exists(spectateTarget)) {
        const targetPos = spectateTarget.position;
        const offset = new mp.Vector3(0, -5.0, 2.0);
        const behindPos = spectateTarget.getOffsetFromInWorldCoords(offset.x, offset.y, offset.z);
        spectateCamera.setCoord(behindPos.x, behindPos.y, behindPos.z);
        spectateCamera.pointAtCoord(targetPos.x, targetPos.y, targetPos.z + 1.0);
    }
    
    if (noclip && !mp.gui.cursor.visible) {
        const controls = mp.game.controls;
        const camPos = noclipCamera.getCoord();
        const camRot = noclipCamera.getRot(2);
        
        let moveFactor = 1.0;
        if (controls.isControlPressed(0, 21)) { moveFactor = 2.5; } 
        else if (controls.isControlPressed(0, 36)) { moveFactor = 0.2; }
        
        const speed = 1.0 * moveFactor;
        const mouseX = controls.getDisabledControlNormal(0, 1) * 4.0;
        const mouseY = controls.getDisabledControlNormal(0, 2) * 4.0;
        
        let newPos = new mp.Vector3(camPos.x, camPos.y, camPos.z);
        const direction = noclipCamera.getDirection();
        
        if (controls.isControlPressed(0, 32)) { newPos.x += direction.x * speed; newPos.y += direction.y * speed; newPos.z += direction.z * speed; }
        if (controls.isControlPressed(0, 33)) { newPos.x -= direction.x * speed; newPos.y -= direction.y * speed; newPos.z -= direction.z * speed; }
        const rightVector = new mp.Vector3(direction.y, -direction.x, 0);
        if (controls.isControlPressed(0, 34)) { newPos.x -= rightVector.x * speed; newPos.y -= rightVector.y * speed; }
        if (controls.isControlPressed(0, 35)) { newPos.x += rightVector.x * speed; newPos.y += rightVector.y * speed; }
        if (controls.isControlPressed(0, 44)) { newPos.z += speed; }
        if (controls.isControlPressed(0, 38)) { newPos.z -= speed; }
        
        player.position = newPos;
        noclipCamera.setCoord(newPos.x, newPos.y, newPos.z);
        
        const newRotX = camRot.x - mouseY;
        noclipCamera.setRot(newRotX < 89.0 && newRotX > -89.0 ? newRotX : camRot.x, camRot.y, camRot.z - mouseX, 2);
    }
});