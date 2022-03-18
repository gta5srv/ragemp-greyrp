const weaponData = require("./weaponData");

mp.events.add("playerDeath", (player, reason, killer) => {
    let msg = `${player.name.replace('_', ' ')} died`;

    if (killer) {
        if (killer.name == player.name) {
            msg = `${player.name.replace('_', ' ')} suicide`;
        } else {
            msg = `${killer.name.replace('_', ' ')} killed ${player.name.replace('_', ' ')}`;
            if (weaponData[reason]) msg += ` with ${weaponData[reason].Name}`;
        }
    }

    mp.players.call("pushToKillFeed", [msg]);
});