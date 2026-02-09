(function() {
    if (typeof mp.Vector2 === "undefined") {
        mp.Vector2 = function(X, Y) { this.X = X; this.Y = Y; };
    }
    const NativeUI = require("./nativeui/index.js");
    const Menu = NativeUI.Menu;
    const UIMenuItem = NativeUI.UIMenuItem;

    let dutyEventTriggered = false;
    const npcPosition = new mp.Vector3(447.213, -980.714, 30.689);

    function createDutyMenu(title, description, actionEvent) {
        let dutyMenu = new Menu(title, description, new mp.Vector2((1920/2)-250, (1080/2)-200));
        dutyMenu.MenuHeader = false;
        dutyMenu.SetMenuWidthOffset(50);
        if (dutyMenu._logo && typeof dutyMenu._logo.Draw === "function")
            dutyMenu._logo.Draw = function() {};

        let startDutyOption = new UIMenuItem("Dienst Starten", "Melde dich im LSPD-Dienst an.");
        let endDutyOption = new UIMenuItem("Dienst Beenden", "Melde dich vom LSPD-Dienst ab.");
        dutyMenu.AddItem(startDutyOption);
        dutyMenu.AddItem(endDutyOption);

        dutyMenu.onItemSelect.on((sender, item, index) => {
            if (dutyEventTriggered) return;
            dutyEventTriggered = true;
            if (actionEvent && actionEvent.length > 0)
                mp.events.callRemote(actionEvent);
            else {
                if (index === 0)
                    mp.events.callRemote("LSPD_StartDuty");
                else if (index === 1)
                    mp.events.callRemote("LSPD_EndDuty");
            }
            dutyMenu.Close();
        });
        dutyMenu.Open();
    }
  
    mp.events.add("ShowDutyMenu_LSPD", (title, description, actionEvent) => {
        dutyEventTriggered = false;
        createDutyMenu(title, description, actionEvent);
    });
  
    let lastInteractionTime = 0;
    mp.events.add("render", () => {
        let localPlayer = mp.players.local;
        let distance = mp.game.system.vdist(
            localPlayer.position.x, localPlayer.position.y, localPlayer.position.z,
            npcPosition.x, npcPosition.y, npcPosition.z
        );
        if (mp.keys.isDown(0x45) && Date.now()-lastInteractionTime > 1000 && distance <= 2.5) {
            lastInteractionTime = Date.now();
            mp.events.callRemote("LSPD_DutyNpc_Interact");
        }
    });
})();
