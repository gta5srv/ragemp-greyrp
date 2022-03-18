// my english level good (koltr <3)

global.gangarena = mp.browsers.new('package://cef/gangarena.html');
global.match = false;

// < HOOK ON CLIENT >

// on player press hide button and this man in lobby 
mp.events.add('client::disconnectlobby', function () {
	mp.events.callRemote("server::disconnectlobby");
});
// on player press connect button
mp.events.add('client::getlobbylist', function () {
	mp.events.callRemote("server::getlobbylist");
});
// on player press connect lobby button
mp.events.add('client::connectlobby', function (index) {
	mp.events.callRemote('server::connectlobby', index);
});
// on player press connect lobby button
mp.events.add('client::kickplayer', function (nick) {
	mp.events.callRemote('server::kickplayer', nick);
});
// on player press create lobby button
mp.events.add('client::sendlobby', function (lobby) {
	mp.events.callRemote('server::sendlobby', lobby);
});
// on player press start button
mp.events.add('client::startmatch', function () {
	mp.events.callRemote('server::startmatch');
});

// < HOOK ON CLIENT >

// < HOOK ON SERVER >
// on player create lobby
mp.events.add('client::createlobby', function (listplayers, lobbyinfo) {
	gangarena.execute(`gangarena.createlobby(${listplayers},${lobbyinfo})`);
});
// on player press e on shape lobby
mp.events.add('client::openmenu', function () {
	if (global.menuCheck()) return;
    menuOpen();
	gangarena.execute(`gangarena.show()`);
});
// for hooks (kick)
mp.events.add('client::closemenu', function () {
	menuClose();
	gangarena.execute(`gangarena.hides()`);
});
// for hooks (start)
mp.events.add('client::closemenuno', function () {
	menuClose();
	gangarena.execute(`gangarena.hidesno()`);
});
// on set hud
mp.events.add('client::sethud', function (active) {
	match = active;
	mp.players.local.freezePosition(true);
	setTimeout( () => { mp.players.local.freezePosition(false); }, 10000);
	gangarena.execute(`hudis.hud=${active}`);
});
mp.events.add('client::setkills', function (kills) {
	gangarena.execute(`hudis.kills='${kills}'`);
});
mp.events.add('client::setdeaths', function (deaths) {
	gangarena.execute(`hudis.deaths='${deaths}'`);
});
mp.events.add('client::settime', function (time) {
	gangarena.execute(`hudis.time='${time}'`);
});
// on player connect to lobby
mp.events.add('client::refreshlobby', function (listplayers) {
	gangarena.execute(`gangarena.refreshlobby(${listplayers})`);
});
// on player geting lobby
mp.events.add('client::setlobbylist', function (listlobby) {
	gangarena.execute(`gangarena.setlobbylist(${listlobby})`);
});
// on player end round
mp.events.add('client::sendwinners', function (listwinners) {
	gangarena.execute(`gangarena.sendwinners(${listwinners})`);
});
// on player connect lobby
mp.events.add('client::setlobby', function (listplayers, lobbyinfo) {
	gangarena.execute(`gangarena.setlobby(${listplayers},${lobbyinfo})`);
});

