global.afkSecondsCount = 0;

setInterval(function () {
        afkSecondsCount++;
        if (afkSecondsCount >= 18000) {
			if(localplayer.getVariable('IS_ADMIN') == true) afkSecondsCount = 0;
			else {
				mp.gui.chat.push('Вы были исключены из игры за AFK более 15 минут.');
				mp.events.callRemote('kickclient');
			}
        }
}, 1000);