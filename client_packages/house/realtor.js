const localPlayer = mp.players.local;
global.realtorMenu = mp.browsers.new('package://cef/HouseAgenti.html'); //1AL
var Cam = null;
var Checkpoint = null;
var vec = new mp.Vector3(0,0,0);

mp.events.add("openRealtorMenu", (houseclass) => {
	if(!global.loggedin) return;
	global.menuOpen();
	global.realtorMenu.active = true;
	
	player.freezePosition(true);
	
	player.position = new mp.Vector3(-1065.4387, -1411.3657, 5.284571)
	
	
	Cam = mp.cameras.new('default', new mp.Vector3(-1093.543, -1430.218, 28.22892), new mp.Vector3(0, 0, 0), 70);
    Cam.pointAtCoord(-1040.87, -1393.737, 4.219687);
    Cam.setActive(true);
	mp.game.cam.renderScriptCams(true, false, 500, true, false);
	
	
	
	global.realtorMenu.execute(`realtorMenu.selectClass(${houseclass})`);
	
	setTimeout(function () { 
		global.realtorMenu.execute(`realtorMenu.active=true`);
	}, 250);
});

mp.events.add("closeRealtorMenu", () => {
		global.realtorMenu.active = false;
		global.realtorMenu.execute(`realtorMenu.active=false`);
		mp.events.callRemote("closeRealtorMenu");
		if (Checkpoint != null)
			Checkpoint.destroy();
		Checkpoint = null;
		//player.freezePosition(false);
		Cam.destroy();
		global.menuClose();
		mp.game.cam.renderScriptCams(false, false, 0, true, true);
		mp.players.local.position = vec;
});

mp.events.add("LoadHouse", (houses, price, vector) => {
	global.realtorMenu.execute(`realtorMenu.houses=${houses}`);
	global.realtorMenu.execute(`realtorMenu.priceInfo=${price}`);
	vec = new mp.Vector3(vector[0],vector[1],vector[2]);
});

mp.events.add("SelectHouseClass", (hclass) => {
	mp.events.callRemote("LoadHouseToMenu", hclass);
});

mp.events.add("buyInfoHome", (hclass, x, y) => {
	mp.events.callRemote("buyRealtorInfoHome", hclass, x, y);
});

mp.events.add("getStreetAndAreaHouse", (x, y, z) => {
	var street = mp.game.pathfind.getStreetNameAtCoord(x, y, z, 0, 0);
    let areahouse  = mp.game.zone.getNameOfZone(x, y, z);
	//var Old = Cam;
	
	//Cam = mp.cameras.new('default', new mp.Vector3(x, y, z + 100), new mp.Vector3(0, 0, 0), 70);
	Cam.setCoord(x, y, z + 100);
    Cam.pointAtCoord(x, y, 0);
    Cam.setActive(true);
	//Cam.setActiveWithInterp(Old.handle, 500, 1, 1);
	
	if (Checkpoint != null)
		Checkpoint.destroy();
	
	Checkpoint = mp.checkpoints.new(27, new mp.Vector3(x,y,z + 20), 5,
    {
        direction: 0,
        color: [0, 0, 255, 255],
        visible: true,
        dimension: player.dimension
    });
	
	player.position = new mp.Vector3(x, y, z + 110)
	
	global.realtorMenu.execute(`realtorMenu.street='${mp.game.ui.getStreetNameFromHashKey(street.streetName)}'`);
    global.realtorMenu.execute(`realtorMenu.crossingRoad='${mp.game.ui.getLabelText(areahouse)}'`);
});