mp.peds.new(0xEF154C47, new mp.Vector3(406.03088, 6526.229, 27.72493), 81.5896, 0); //Meo

mp.events.add("orangeOpenMenu", (json) => {
	if (!loggedin || chatActive || editing || cuffed) return;
	global.menuOpen();
	global.menuOrange = mp.browsers.new('package://cef/modules/Jobs/Orange/index.html');
	global.menuOrange.active = true;
	global.menuOrange.execute(`init()`);
});

mp.events.add("closeOpenMenu", (count) => {
	global.menuClose();
	global.menuOrange.active = false;
	global.menuOrange.destroy();
	mp.events.callRemote("orangeStopWork", count);
});