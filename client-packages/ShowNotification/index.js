// // Initialisiere die globale Variable gui
// global.gui = { notify: null }; // notify wird später von Vue zugewiesen

// // Erstelle den Browser und lade die index.html vom localhost
// const browser = mp.browsers.new("http://localhost:8080");

// // Event-Listener für Benachrichtigungen vom Server
// mp.events.add("showNotification", (message, icon) => {
//     console.log("Triggering showNotification event:", message, icon);
//     console.log("Current gui.notify:", global.gui ? global.gui.notify : "undefined");
//     if (global.gui && global.gui.notify) {
//         global.gui.notify.showNotification(message, icon);
//     } else {
//         mp.gui.chat.push(`[Fallback] Notification: ${message}`);
//     }
// });

let notifyHud = null;

mp.events.add("showNotification", (text, iconpic) => {
    if (notifyHud == null) {
        notifyHud = mp.browsers.new("package://web/vue/index.html");
    notifyHud.execute(`gui.notify.showNotification('${text}', '${iconpic}')`);
    }
else{
    notifyHud.execute(`gui.notify.showNotification('${text}', '${iconpic}')`);
}});