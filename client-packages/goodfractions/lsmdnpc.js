(function() {
    // Sicherstellen, dass mp.Vector2 existiert
    if (typeof mp.Vector2 === "undefined") {
        mp.Vector2 = function(X, Y) { this.X = X; this.Y = Y; };
    }
    
    // NativeUI laden – genau wie beim LSPD
    const NativeUI = require("./nativeui/index.js");
    const Menu = NativeUI.Menu;
    const UIMenuItem = NativeUI.UIMenuItem;

    let dutyEventTriggered = false;
    // LSMD-NPC-Position – diese muss mit der im Serverscript verwendeten Position übereinstimmen
    const npcPosition = new mp.Vector3(315.043, -592.215, 43.265);

    function createDutyMenu(title, description, actionEvent) {
        // Erstelle das Menü zentriert auf dem Bildschirm, wie beim LSPD
        let dutyMenu = new Menu(title, description, new mp.Vector2((1920 / 2) - 250, (1080 / 2) - 200));
        dutyMenu.MenuHeader = false;
        dutyMenu.SetMenuWidthOffset(50);
        // Falls ein Logo vorhanden ist, das nun nicht gezeichnet werden soll:
        if (dutyMenu._logo && typeof dutyMenu._logo.Draw === "function")
            dutyMenu._logo.Draw = function() {};

        // Erstelle LSMD-spezifische Menüeinträge
        let startDutyOption = new UIMenuItem("Dienst Starten", "Melde dich im LSMD-Dienst an.");
        let endDutyOption = new UIMenuItem("Dienst Beenden", "Melde dich vom LSMD-Dienst ab.");
        dutyMenu.AddItem(startDutyOption);
        dutyMenu.AddItem(endDutyOption);

        dutyMenu.onItemSelect.on((sender, item, index) => {
            if (dutyEventTriggered) return;
            dutyEventTriggered = true;
            if (actionEvent && actionEvent.length > 0)
                mp.events.callRemote(actionEvent);
            else {
                if (index === 0)
                    mp.events.callRemote("LSMD_StartDuty");
                else if (index === 1)
                    mp.events.callRemote("LSMD_EndDuty");
            }
            dutyMenu.Close();
        });
        dutyMenu.Open();
    }

    mp.events.add("ShowDutyMenu_LSMD", (title, description, actionEvent) => {
        dutyEventTriggered = false;
        createDutyMenu(title, description, actionEvent);
    });

    // Kontrolliere, ob der Spieler in der Nähe des LSMD-NPC steht und drücke den Interaktions-Key (E, 0x45)
    let lastInteractionTime = 0;
    mp.events.add("render", () => {
        let localPlayer = mp.players.local;
        let distance = mp.game.system.vdist(
            localPlayer.position.x, localPlayer.position.y, localPlayer.position.z,
            npcPosition.x, npcPosition.y, npcPosition.z
        );
        if (mp.keys.isDown(0x45) && Date.now() - lastInteractionTime > 1000 && distance <= 2.5) {
            lastInteractionTime = Date.now();
            mp.events.callRemote("LSMD_DutyNpc_Interact");
        }
    });
})();
