setInterval(() => {
    let now = new Date();
    let hours = now.getHours();
    let minutes = now.getMinutes();

    mp.world.time.hour = hours;
    mp.world.time.minute = minutes;

    // Sende die Zeit an alle verbundenen Clients
    mp.players.forEach(player => {
        player.call("syncTime", [hours, minutes]);
    });

    console.log(`Serverzeit synchronisiert: ${hours}:${minutes}`);
}, 60000); // Aktualisierung jede Minute
