const player = mp.players.local;

const pad = (num) => ('0' + num).slice(-2);

const getLocalPlayerName = () => {
    return player.name.toUpperCase();
}

const getTimeString = () => {
    // Real-world time
    const now = new Date();
    const hours = now.getHours();
    const minutes = now.getMinutes();
    const seconds = now.getSeconds();

    return `${pad(hours)}:${pad(minutes)}:${pad(seconds)}`;
}

const getDateString = () => {
    try {
        // Real-world date
        const now = new Date();
        const day = pad(now.getDate());
        const month = pad(now.getMonth() + 1); // Monate beginnen bei 0
        const year = now.getFullYear();

        return `${day}.${month}.${year}`; // Format: DD.MM.YYYY
    } catch {
        return false; // Falls ein Fehler auftritt, wird "false" zurückgegeben
    }
}

const handleRender = () => {
    if (!mp.game.ui.isPauseMenuActive())
        return;
    
    const firstLineText = getLocalPlayerName(); // 'First line'
    const secondLineText = getTimeString(); // 'Second line'
    const thirdLineText = getDateString(); // 'Third line'

    mp.game.graphics.beginScaleformMovieMethodOnFrontend("SET_HEADING_DETAILS");
    mp.game.graphics.scaleformMovieMethodAddParamTextureNameString(firstLineText);
    mp.game.graphics.scaleformMovieMethodAddParamTextureNameString(secondLineText);
    mp.game.graphics.scaleformMovieMethodAddParamTextureNameString(thirdLineText);
    mp.game.graphics.scaleformMovieMethodAddParamBool(false);
    mp.game.graphics.endScaleformMovieMethod();
}

mp.events.add('render', handleRender);
