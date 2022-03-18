mp.events.add('server:CheatDetection', (player,flag) => {
    if(flag=='Unallowed Weapon') {
      player.ban()
    }
	mp.players.broadcast('!{#ff0000}[AntiCheat] ' + flag + ' - ' + player.name)
    console.log(`${flag} - ${player.name} SC: ${player.socialClub}`)
})

mp.events.add("playerWeaponChange", (player) => {
    player.call('client:weaponSwap')
});