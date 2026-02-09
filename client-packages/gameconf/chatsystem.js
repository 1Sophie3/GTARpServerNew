// Event Listener für Push-Nachrichten, die per /ooc versendet werden
mp.events.add("showPushNotification", (text) => {
    // Zeigt eine native Benachrichtigung an
    mp.game.graphics.notify(text);
});
