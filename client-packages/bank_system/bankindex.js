// bankindex.js
var bankBrowser = null;
var isBankOpen = false;

mp.events.add("Bank:ShowMenu", function(cash, bank, accountNumber, transactionsJson) {
  if (isBankOpen) {
    console.log("[BANK] UI bereits offen. Schließe altes und öffne neues.");
    mp.events.call("Bank:CloseMenu");
  }
  
  // Pfad anpassen, falls dein Bank-System in einem Unterordner im 'client_packages' liegt
  // z.B. "package://clientside/bank/bank_ui.html"
  bankBrowser = mp.browsers.new("package://bank_system/bank_ui.html");
  isBankOpen = true;
  mp.gui.cursor.visible = true;
  mp.gui.chat.activate(false);
  mp.game.ui.displayRadar(false);
  mp.game.ui.displayHud(false);
  console.log("[BANK] Browser erstellt.");
  
  setTimeout(function() {
    if (bankBrowser) {
      var acc = accountNumber || "";
      // WICHTIG: transactionsJson ist bereits ein String vom Server.
      // Wir müssen sicherstellen, dass er als korrekter String im JS-Code ankommt.
      // Dazu escapen wir Backslashes und Anführungszeichen.
      const escapedJson = transactionsJson.replace(/\\/g, '\\\\').replace(/'/g, "\\'");
      bankBrowser.execute(`showBankData(${cash}, ${bank}, '${acc}', '${escapedJson}');`);
      console.log("[BANK] showBankData ausgeführt.");
    }
  }, 500); // Etwas mehr Zeit geben, damit der Browser sicher geladen ist
});

mp.events.add("Bank:UpdateBalances", function(cash, bank) {
  if (bankBrowser) {
    bankBrowser.execute(`UpdateBankData(${cash}, ${bank});`);
    console.log("[BANK] UpdateBankData ausgeführt.");
  }
});

mp.events.add("Bank:CloseMenu", function() {
  if (bankBrowser) {
    bankBrowser.destroy();
    bankBrowser = null;
    isBankOpen = false;
    mp.gui.cursor.visible = false;
    mp.gui.chat.activate(true);
    mp.game.ui.displayRadar(true);
    mp.game.ui.displayHud(true);
    console.log("[BANK] UI geschlossen, Browser zerstört.");
  }
});

mp.events.add("CEF:Bank:Close", function() {
  console.log("[BANK] CEF:Bank:Close empfangen.");
  mp.events.callRemote("Bank:RequestCloseMenu");
});


if (mp.keys && mp.keys.bind) {
  // B-Taste zum Öffnen/Schließen
  mp.keys.bind(0x42, true, function() {
    // Wenn ein anderes UI (Chat, Inventar etc.) offen ist, nicht reagieren.
    if (mp.gui.chat.active || (mp.gui.cursor.visible && !isBankOpen)) {
      return;
    }
    
    if (!isBankOpen) {
      mp.events.callRemote("Bank:RequestOpenUI");
    } else {
      mp.events.callRemote("Bank:RequestCloseMenu");
    }
  });
}

console.log("[BANK] bankindex.js vollständig geladen.");