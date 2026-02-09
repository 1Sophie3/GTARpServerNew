const factionNpcs = [
    { name: "LSPD", position: new mp.Vector3(447.213, -980.714, 30.689), event: "LSPD_DutyNpc_Interact" },
    { name: "LSMD", position: new mp.Vector3(315.043, -592.215, 43.265), event: "LSMD_DutyNpc_Interact" },
    { name: "LSCS", position: new mp.Vector3(460.0, -570.0, 28.0), event: "LSCS_DutyNpc_Interact" },
	{ name: "DrivingSchool", position: new mp.Vector3(-711.95, -1307.68, 5.11), event: "DrivingSchool_DutyNpc_Interact" }

];
let factionBrowser = null;
mp.events.add('Faction:ShowMenu', (factionName, isOnDuty) => {
    if (factionBrowser === null) {
        factionBrowser = mp.browsers.new('package://goodfractions/fraktions_ui.html');
    }
    factionBrowser.execute(`window.updateMenu('${factionName}', ${isOnDuty});`);
    factionBrowser.active = true;
    mp.gui.cursor.show(true, true);
});
function closeMenu() {
    if (factionBrowser !== null) {
        factionBrowser.destroy();
        factionBrowser = null;
        mp.gui.cursor.show(false, false);
    }
}
mp.events.add('Faction:CEF:StartDuty', () => { mp.events.callRemote('Faction:StartDuty'); closeMenu(); });
mp.events.add('Faction:CEF:EndDuty', () => { mp.events.callRemote('Faction:EndDuty'); closeMenu(); });
mp.events.add('Faction:CEF:CloseMenu', () => { closeMenu(); });
let lastInteractionTime = 0;
mp.events.add('render', () => {
    if (factionBrowser !== null) return;
    let localPlayer = mp.players.local;
    for (const npc of factionNpcs) {
        if (mp.game.system.vdist(localPlayer.position.x, localPlayer.position.y, localPlayer.position.z, npc.position.x, npc.position.y, npc.position.z) <= 2.5) {
            mp.game.graphics.drawText("Drücke E, um zu interagieren", [npc.position.x, npc.position.y, npc.position.z + 1.0], { font: 4, color: [255, 255, 255, 185], scale: [0.4, 0.4], outline: true });
            if (mp.keys.isDown(0x45) && Date.now() - lastInteractionTime > 1000) {
                lastInteractionTime = Date.now();
                mp.events.callRemote(npc.event);
            }
            break;
        }
    }
});