
global.apartments = mp.browsers.new('package://cef/apartments.html');

mp.events.add('client::sendapart', function (index) {
	menuClose();
	apartments.execute(`aparts.hides()`);
	mp.events.callRemote("server::interact", index);
});

mp.events.add('client::closeapart', function () {
	menuClose();
	apartments.execute(`aparts.hides()`);
});

mp.events.add('client::openapart', function (data) {
	if (global.menuCheck()) return;
    menuOpen();
	apartments.execute(`aparts.show(${data})`);
});