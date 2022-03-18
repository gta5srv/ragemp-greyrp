var cam = mp.cameras.new('default', new mp.Vector3(0, 0, 0), new mp.Vector3(0, 0, 0), false);
var effect = '';
global.loggedin = false;
global.lastCheck = 0;
global.chatLastCheck = 0;
global.pocketEnabled = false;

//mp.game.invoke("0x5E1460624D194A38", true);

//var emscol = mp.colshapes.newSphere(264.5199, -1352.684, 23.446, 50, 0);

var Peds = [
    { Hash: -39239064, Pos: new mp.Vector3(1394.9376, 3611.9573, 34.9), Angle: -97.92953 }, // Caleb Baker
    { Hash: -1176698112, Pos: new mp.Vector3(166.6278, 2229.249, 90.73845), Angle: 47.0 }, // Matthew Allen
    { Hash: 1161072059, Pos: new mp.Vector3(2887.687, 4387.17, 50.65578), Angle: 174.0 }, // Owen Nelson
    { Hash: -1398552374, Pos: new mp.Vector3(2192.614, 5596.246, 53.75177), Angle: 318.0 }, // Daniel Roberts
    { Hash: -459818001, Pos: new mp.Vector3(-215.4299, 6445.921, 31.30351), Angle: 262.0 }, // Michael Turner
    { Hash: 0x9D0087A8, Pos: new mp.Vector3(480.9385, -1302.576, 29.24353), Angle: 224.0 }, // jimmylishman
    //{ Hash: 1706635382, Pos: new mp.Vector3(-222.5464, -1617.449, 34.86932), Angle: 309.2058 }, // Lamar_Davis
    //{ Hash: 588969535, Pos: new mp.Vector3(85.79006, -1957.156, 20.74745), Angle: 320.4474 }, // Carl_Ballard
    //{ Hash: -812470807, Pos: new mp.Vector3(485.6168, -1529.195, 29.28829), Angle: 56.19691 }, // Chiraq_Bloody
    //{ Hash: 653210662, Pos: new mp.Vector3(1408.224, -1486.415, 60.65733), Angle: 192.2974 }, // Riki_Veronas
    //{ Hash: 663522487, Pos: new mp.Vector3(892.2745, -2172.252, 32.28627), Angle: 172.3141 }, // Santano_Amorales
    //{ Hash: 645279998, Pos: new mp.Vector3(-113.9224, 985.793, 235.754), Angle: 110.9234 }, // Vladimir_Medvedev
    { Hash: 0x2A797197, Pos: new mp.Vector3(-43.831474, 1960.0846, 190.4), Angle: 25.90957 }, // Создать Семью
    //{ Hash: -1427838341, Pos: new mp.Vector3(-1549.287, -89.35114, 54.92917), Angle: 7.874235 }, // Jotaro_Josuke
    //{ Hash: -2034368986, Pos: new mp.Vector3(1392.098, 1155.892, 114.4433), Angle: 82.24557 }, // Solomon_Gambino

    { Hash: 368603149, Pos: new mp.Vector3(443.55908, -981.87085, 30.6896), Angle: 89.16982 }, // Nancy_Spungen
    { Hash: 941695432, Pos: new mp.Vector3(149.1317, -758.3485, 242.152), Angle: 66.82055 }, //  Steve_Hain
    { Hash: 1558115333, Pos: new mp.Vector3(120.0836, -726.7773, 242.152), Angle: 248.3546 }, // Michael Bisping
    { Hash: 1925237458, Pos: new mp.Vector3(-2347.958, 3268.936, 32.81076), Angle: 240.8822 }, // Ronny_Pain
    //{ Hash: 988062523, Pos: new mp.Vector3(253.9357, 228.9332, 101.6832), Angle: 250.3564 }, // Anthony_Young
    { Hash: 0x5D71A46F, Pos: new mp.Vector3(-553.6412, -185.95569, 38.3), Angle: -146.45332 }, // Информатор
	{ Hash: 0xE7565327, Pos: new mp.Vector3(-554.94415, -186.95253, 38.3), Angle: -153.44913 }, // med carta
	{ Hash: 0xA5C787B6, Pos: new mp.Vector3(-556.6857, -187.80418, 38.3), Angle: -164.15198 }, // Оружие
	{ Hash: 0xE497BBEF, Pos: new mp.Vector3(308.74008, 3572.2566, 33.6), Angle: -94.41156 }, // Косяки
    //{ Hash: 826475330, Pos: new mp.Vector3(-32.440727, -1111.5065, 25.302338), Angle: 13.846124 }, // Heady_Hunterw
    { Hash: -1420211530, Pos: new mp.Vector3(251.4247, -1346.499, 24.5378), Angle: 223.6044 }, // Bdesma_Katsuni
    { Hash: 1092080539, Pos: new mp.Vector3(262.3232, -1359.772, 24.53779), Angle: 49.42155 }, // Steve_Hobs
	{ Hash: 0x909D9E7F, Pos: new mp.Vector3(-32.440727, -1111.5065, 26.325338), Angle: 14.9 }, // Avto293
	{ Hash: 0x49EA5685, Pos: new mp.Vector3(-285.38406, -885.6798, 31.160619), Angle: 173.5264 }, // nomera
	{ Hash: 0x49EA5685, Pos: new mp.Vector3(-292.45886, -884.01855, 31.160619), Angle: 167.86559 }, // nomera1
	{ Hash: 0x49EA5685, Pos: new mp.Vector3(-299.83557, -882.3633, 31.160619), Angle: 171.7445 }, // nomera2
	{ Hash: 0xF06B849D, Pos: new mp.Vector3(-479.38858, -797.42786, 30.5), Angle: -84.43331 }, // handling
	{ Hash: 0x62018559, Pos: new mp.Vector3(364.48993, 297.93716, 103.47151), Angle: -109.96255 }, // аренда
	{ Hash: 0xB3B3F5E6, Pos: new mp.Vector3(-795.91626, -219.7338, 37.1), Angle: 119 }, // LuxeAuto
	{ Hash: 0xB3B3F5E6, Pos: new mp.Vector3(-911.5957, -227.2981, 39.858784), Angle: -122 }, // LuxeAutoGalaxy
    { Hash: -1306051250, Pos: new mp.Vector3(257.5671, -1344.612, 24.54937), Angle: 229.3922 }, // Billy_Bob
    { Hash: -907676309, Pos: new mp.Vector3(724.8585, 134.1029, 80.95643), Angle: 245.0083 }, // Ronny_Bolls
	{ Hash: 1097048408, Pos: new mp.Vector3(2485.132, 3445.385, 51.0670), Angle: 41.60225 },
	{ Hash: -1369710022, Pos: new mp.Vector3(1550.667, 3800.679, 34.4111), Angle: 203.1918 },
	
	{ Hash: -50684386, Pos: new mp.Vector3(2220.723, 4900.9136, 40.965938), Angle: -42.2323 }, // Korova1
{ Hash: -50684386, Pos: new mp.Vector3(2219.496, 4902.2153, 40.965938), Angle: -42.2323 }, // Korova2
{ Hash: -50684386, Pos: new mp.Vector3(2218.4912, 4903.2583, 40.965938), Angle: -42.2323 }, // Korova3

{ Hash: -50684386, Pos: new mp.Vector3(2249.899, 4905.576, 40.965938), Angle: 136.42168 }, // Korova1
{ Hash: -50684386, Pos: new mp.Vector3(2248.5317, 4906.8213, 40.965938), Angle: 136.42168 }, // Korova2
{ Hash: -50684386, Pos: new mp.Vector3(2247.2295, 4908.0063, 40.965938), Angle: 136.42168 }, // Korova3

{ Hash: -50684386, Pos: new mp.Vector3(2248.1023, 4872.494, 40.965938), Angle: -41.132217 }, // Korova1
{ Hash: -50684386, Pos: new mp.Vector3(2247.0042, 4873.5312, 40.965938), Angle: -41.132217 }, // Korova2
{ Hash: -50684386, Pos: new mp.Vector3(2245.6777, 4874.731, 40.965938), Angle: -41.132217 }, // Korova3

{ Hash: 0xF0EC56E2, Pos: new mp.Vector3(2866.113, 4729.5645, 48.8), Angle: 17.939724 }, // vinogradnik
{ Hash: -681546704, Pos: new mp.Vector3(806.474, 2174.3894, 52.2), Angle: -30.131432 }, // vinogradnik
];

/*mp.colshapes.forEach( 
	(colshape) => {
		if(colshape == emscol) mp.gui.chat.push("You are near EMS");
	}
);*/

setTimeout(function () {
    Peds.forEach(ped => {
        mp.peds.new(ped.Hash, ped.Pos, ped.Angle, 0);
    });
}, 10000);


mp.events.add('outVeh', (flag) => {
	const player = mp.players.local;
	if (player.vehicle != null)
		player.taskLeaveVehicle(player.vehicle.handle, 0);
});

mp.game.gameplay.disableAutomaticRespawn(true);
mp.game.gameplay.ignoreNextRestart(true);
mp.game.gameplay.setFadeInAfterDeathArrest(false);
mp.game.gameplay.setFadeOutAfterDeath(false);
mp.game.gameplay.setFadeInAfterLoad(false);

mp.events.add('freeze', function (toggle) {
    localplayer.freezePosition(toggle);
});

mp.events.add('destroyCamera', function () {
    if(cam!=null)
    {       
        cam.destroy();
        mp.game.cam.renderScriptCams(false, false, 3000, true, true);
    }
});

mp.events.add('carRoom', function (x, y, z, x2, y2, z2) {
    cam = mp.cameras.new('default', new mp.Vector3(x, y, z), new mp.Vector3(0, 0, 0), 45);
    cam.pointAtCoord(x2, y2, z2);
    cam.setActive(true);
    mp.game.cam.renderScriptCams(true, false, 0, true, false);
});

mp.events.add('screenFadeOut', function (duration) {
    mp.game.cam.doScreenFadeOut(duration);
});

mp.events.add('screenFadeIn', function (duration) {
    mp.game.cam.doScreenFadeIn(duration);
});

var lastScreenEffect = "";
mp.events.add('startScreenEffect', function (effectName, duration, looped) {
	try {
		lastScreenEffect = effectName;
		mp.game.graphics.startScreenEffect(effectName, duration, looped);
	} catch (e) { }
});

mp.events.add('stopScreenEffect', function (effectName) {
	try {
		var effect = (effectName == undefined) ? lastScreenEffect : effectName;
		mp.game.graphics.stopScreenEffect(effect);
	} catch (e) { }
});

mp.events.add('stopAndStartScreenEffect', function (stopEffect, startEffect, duration, looped) {
	try {
		mp.game.graphics.stopScreenEffect(stopEffect);
		mp.game.graphics.startScreenEffect(startEffect, duration, looped);
	} catch (e) { }
});

mp.events.add('setHUDVisible', function (arg) {
    mp.game.ui.displayHud(arg);
    mp.gui.chat.show(arg);
    mp.game.ui.displayRadar(arg);
});

mp.events.add('setPocketEnabled', function (state) {
    pocketEnabled = state;
    if (state) {
        mp.gui.execute("fx.set('inpocket')");
        mp.game.invoke(getNative("SET_FOLLOW_PED_CAM_VIEW_MODE"), 4);
    }
    else {
        mp.gui.execute("fx.reset()");
    }
});
mp.keys.bind(Keys.VK_ALT, false, function () { // ALT key
    if (global.menuCheck() || localplayer.getVariable('InDeath') == true && !localplayer.isInAnyVehicle(false)) return;
    if (circleOpen) {
        CloseCircle();
        return;
    }
    if (!loggedin || chatActive || entity == null || new Date().getTime() - lastCheck < 1000) return;
    switch (entity.type) {
        case "player":
			if (localplayer.getVariable('familycid') != null )
			{
				mp.gui.cursor.visible = true;
				OpenCircle("Семья", 0);
				return;
			}
            mp.gui.cursor.visible = true;
            OpenFracData("Фракция");
            return;
    }
    lastCheck = new Date().getTime();
});



mp.keys.bind(global.Keys.VK_E, false, function() {

    mp.events.callRemote('takeferma');

});


mp.keys.bind(Keys.VK_K, false, function () {
    if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
    mp.events.callRemote('cancelPressed');
    lastCheck = new Date().getTime();
});

mp.events.add('connected', function () {
    mp.game.ui.displayHud(false);
    cam = mp.cameras.new('default', startCamPos, startCamRot, 90.0);
    cam.setActive(true);
    mp.game.graphics.startScreenEffect('SwitchSceneMichael', 5000, false);
    var effect = 'SwitchSceneMichael';
});

mp.events.add('ready', function () {
    mp.game.ui.displayHud(true);
    //cam.setActive(false);
    //mp.game.graphics.stopScreenEffect(effect);
});

mp.events.add('kick', function (notify) {
    mp.events.call('notify', 4, 9, notify, 10000);
    mp.events.callRemote('kickclient');
});

mp.events.add('loggedIn', function () {
    loggedin = true;
});

mp.events.add('setFollow', function (toggle, entity) {
    if (toggle) {
        if (entity && mp.players.exists(entity))
            localplayer.taskFollowToOffsetOf(entity.handle, 0, 0, 0, 1, -1, 1, true)
    }
    else
        localplayer.clearTasks();
});

setInterval(function () {
    if (localplayer.getArmour() <= 0 && localplayer.getVariable('HASARMOR') === true) {
        mp.events.callRemote('deletearmor');
    }
}, 600);

mp.keys.bind(Keys.VK_U, false, function () { // Animations selector
    if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
    if (localplayer.isInAnyVehicle(true)) return;
    OpenCircle("Категории", 0);
});

mp.keys.bind(Keys.VK_E, false, function () { // E key
    if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
    if(global.casinoOpened){
        mp.events.callRemote('interactionPressed');
    }
    if( global.menuOpened) return;
    mp.events.callRemote('interactionPressed');
    lastCheck = new Date().getTime();
    global.acheat.pos();
});

mp.keys.bind(Keys.VK_L, false, function () { // L key
    if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
    mp.events.callRemote('lockCarPressed');
    lastCheck = new Date().getTime();
});

mp.keys.bind(Keys.VK_LEFT, true, () => {
	if(mp.gui.cursor.visible || !loggedin) return;
	if(localplayer.vehicle) {
		if(localplayer.vehicle.getPedInSeat(-1) != localplayer.handle) return;
		if(new Date().getTime() - lastCheck > 500) {
			lastCheck = new Date().getTime();
			if(localplayer.vehicle.getVariable('leftlight') == true) mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 0, 0);
			else mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 1, 0);
		}
	}
});

mp.keys.bind(Keys.VK_RIGHT, true, () => {
	if(mp.gui.cursor.visible || !loggedin) return;
	if(localplayer.vehicle) {
		if(localplayer.vehicle.getPedInSeat(-1) != localplayer.handle) return;
		if(new Date().getTime() - lastCheck > 500) {
			lastCheck = new Date().getTime();
			if(localplayer.vehicle.getVariable('rightlight') == true) mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 0, 0);
			else mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 0, 1);
		}
	}
});

mp.keys.bind(Keys.VK_DOWN, true, () => {
	if(mp.gui.cursor.visible || !loggedin) return;
	if(localplayer.vehicle) {
		if(localplayer.vehicle.getPedInSeat(-1) != localplayer.handle) return;
		if(new Date().getTime() - lastCheck > 500) {
			lastCheck = new Date().getTime();
			if(localplayer.vehicle.getVariable('leftlight') == true && localplayer.vehicle.getVariable('rightlight') == true) mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 0, 0);
			else mp.events.callRemote("VehStream_SetIndicatorLightsData", localplayer.vehicle, 1, 1);
		}
	}
});

mp.keys.bind(Keys.VK_2, false, function () { // B key
    if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 400 || global.menuOpened) return;
    if (localplayer.isInAnyVehicle(false) && localplayer.vehicle.getSpeed() <= 3) {
        lastCheck = new Date().getTime();
        mp.events.callRemote('engineCarPressed');
    }
});

mp.keys.bind(Keys.VK_M, false, function () { // Телефон
    if (!loggedin || chatActive || editing || global.menuCheck() || cuffed || localplayer.getVariable('InDeath') == true) return;
    mp.events.callRemote('openPlayerMenu');
    lastCheck = new Date().getTime();
});

mp.keys.bind(Keys.VK_N, false, function () {
    if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
    mp.events.callRemote('playerPressCuffBut');
    lastCheck = new Date().getTime();
});

// mp.keys.bind(Keys.VK_Z, false, function () { // Z key
    // if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
	
    // if(localplayer.vehicle) {
        // CheckMyWaypoint();
    // } else mp.events.callRemote('playerPressFollowBut');
    // lastCheck = new Date().getTime();
// });



// function CheckMyWaypoint() {
    // try {
        // if(mp.game.invoke('0x1DD1F58F493F1DA5')) {
            // let foundblip = false;
            // let blipIterator = mp.game.invoke('0x186E5D252FA50ES7D');
            // let totalBlipsFound = mp.game.invoke('0x9A3FF3DE163034E8');
            // let FirstInfoId = mp.game.invoke('0x1BEDE233E6CD2A1F', blipIterator);
            // let NextInfoId = mp.game.invoke('0x14F96AA50D6FBEA7', blipIterator);
            // for (let i = FirstInfoId, blipCount = 0; blipCount != totalBlipsFound; blipCount++, i = NextInfoId) {
                // if (mp.game.invoke('0x1FC877464A04FC4F', i) == 8) {
                    // var coord = mp.game.ui.getBlipInfoIdCoord(i);
                    // foundblip = true;
                    // break;
                // }
            // }
            // if(foundblip) mp.events.callRemote('syncWaypoint', coord.x, coord.y, coord.z);
        // }
    // } catch (e) { }
// }

// function CheckWP() {
        // if(mp.game.invoke('0x1DD1F58F493F1DA5')) {
            // let blipIterator = mp.game.invoke('0x186E5D252FA50E7D');
            // let totalBlipsFound = mp.game.invoke('0x9A3FF3DE163034E8');
            // let FirstInfoId = mp.game.invoke('0x1BEDE233E6CD2A1F', blipIterator);
            // let NextInfoId = mp.game.invoke('0x14F96AA50D6FBEA7', blipIterator);
            // for (let i = FirstInfoId, blipCount = 0; blipCount != totalBlipsFound; blipCount++, i = NextInfoId) {
                // if (mp.game.invoke('0x1FC877464A04FC4F', i) == 8) {
                    // var coord = mp.game.ui.getBlipInfoIdCoord(i);
					// mp.game.graphics.notify('~g~Телепорт на метку')
					// mp.events.callRemote('teleportWaypoint', coord.x, coord.y);
                    // break;
                // }
            // }
        // }
// }

// mp.events.add('teleportbyz', function()
// {
	// const position = mp.players.local.position;
	// const getz = mp.game.gameplay.getGroundZFor3dCoord(position.x, position.y, position.z, 0.0, false);
    // mp.players.local.setCoordsNoOffset(position.x, position.y, getz, false, false, false);
// });

// var gproute = null;

// mp.events.add('syncWP', function (bX, bY, type) {
    // if(!mp.game.invoke('0x1DD1F58F493F1DA5')) {
        // //mp.game.ui.setNewWaypoint(bX, bY);
        // gproute = mp.blips.new(38, new mp.Vector3(bX, bY), { alpha: 255, name: "", scale: 1, color: 1 });
        // gproute.setRoute(true);
        // gproute.setRouteColour(5);
        // if(type == 0) mp.events.call('notify', 2, 9, "Пассажир передал Вам информацию о своём маршруте!", 3000);
        // else if(type == 1) mp.events.call('notify', 2, 9, "Человек из списка контактов Вашего телефона передал Вам метку его местоположения!", 3000);
    // } else {
        // if(type == 0) mp.events.call('notify', 4, 9, "Пассажир попытался передать Вам информацию о маршруте, но у Вас уже установлен другой маршрут.", 5000);
        // else if(type == 1) mp.events.call('notify', 4, 9, "Человек из списка контактов Вашего телефона попытался передать Вам метку его местоположения, но у Вас уже установлена другая метка.", 5000);
    // }
// });

// mp.events.add('removeGRoute', function(){
	// try {
		// if (gproute != null) gproute.destroy();
	// }
	// catch (e) {}
// });

//old scripts
mp.keys.bind(Keys.VK_Z, false, function () { // Z key
    if (!loggedin || chatActive || editing || new Date().getTime() - lastCheck < 1000 || global.menuOpened) return;
    
    if(localplayer.vehicle) {
        CheckMyWaypoint();
    } else mp.events.callRemote('playerPressFollowBut');
    lastCheck = new Date().getTime();
});

function CheckMyWaypoint() {
    try {
        if(mp.game.invoke('0x1DD1F58F493F1DA5')) {
            let foundblip = false;
            let blipIterator = mp.game.invoke('0x186E5D252FA50E7D');
            let totalBlipsFound = mp.game.invoke('0x9A3FF3DE163034E8');
            let FirstInfoId = mp.game.invoke('0x1BEDE233E6CD2A1F', blipIterator);
            let NextInfoId = mp.game.invoke('0x14F96AA50D6FBEA7', blipIterator);
            for (let i = FirstInfoId, blipCount = 0; blipCount != totalBlipsFound; blipCount++, i = NextInfoId) {
                if (mp.game.invoke('0x1FC877464A04FC4F', i) == 8) {
                    var coord = mp.game.ui.getBlipInfoIdCoord(i);
                    foundblip = true;
                    break;
                }
            }
            if(foundblip) mp.events.callRemote('syncWaypoint', coord.x, coord.y, coord.z);
        }
    } catch (e) { }
}

var gproute = null;

mp.events.add('syncWP', function (bX, bY, type) {
    if(!mp.game.invoke('0x1DD1F58F493F1DA5')) {
        //mp.game.ui.setNewWaypoint(bX, bY);
        gproute = mp.blips.new(38, new mp.Vector3(bX, bY), { alpha: 255, name: "", scale: 1, color: 1 });
        gproute.setRoute(true);
        gproute.setRouteColour(5);
        if(type == 0) mp.events.call('notify', 2, 9, "Пассажир передал Вам информацию о своём маршруте!", 3000);
        else if(type == 1) mp.events.call('notify', 2, 9, "Человек из списка контактов Вашего телефона передал Вам метку его местоположения!", 3000);
    } else {
        if(type == 0) mp.events.call('notify', 4, 9, "Пассажир попытался передать Вам информацию о маршруте, но у Вас уже установлен другой маршрут.", 5000);
        else if(type == 1) mp.events.call('notify', 4, 9, "Человек из списка контактов Вашего телефона попытался передать Вам метку его местоположения, но у Вас уже установлена другая метка.", 5000);
    }
});

mp.events.add('removeGRoute', function(){
	try 
	{
		if (gproute != null)
		{
			gproute.destroy();
			gproute = null;
		}
	}
	catch (e) {}
});
//old scripts end

mp.keys.bind(Keys.VK_U, false, function () { // U key
    if (!loggedin || chatActive || editing || global.menuOpened || new Date().getTime() - lastCheck < 1000) return;
    mp.events.callRemote('openCopCarMenu');
    lastCheck = new Date().getTime();
});

mp.keys.bind(Keys.VK_OEM_3, false, function () { // ` key
    if (chatActive || (global.menuOpened && mp.gui.cursor.visible)) return;
    mp.gui.cursor.visible = !mp.gui.cursor.visible;
});

mp.keys.bind(Keys.VK_F6, false, function () { // F6 key
    /*if (global.menuCheck()) return;
    if (!mp.game.recorder.isRecording()) {
        mp.game.recorder.start(1);
    } else {
        mp.game.recorder.stop();
    }*/
});

var lastPos = new mp.Vector3(0, 0, 0);

mp.game.gameplay.setFadeInAfterDeathArrest(false);
mp.game.gameplay.setFadeInAfterLoad(false);

var deathTimerOn = false;
var deathTimer = 0;

mp.events.add('DeathTimer', (time) => {
    if (time === false) {
        deathTimerOn = false;
		global.dialog.closeMED();
		
	}
    else {
		global.menu.execute(`death.buttonact=false`);
        deathTimerOn = true;
        deathTimer = new Date().getTime() + time;
    }
});

mp.events.add('render', () => {
    if (localplayer.getVariable('InDeath') == true || intrunk) {
        mp.game.controls.disableAllControlActions(2);
        mp.game.controls.enableControlAction(2, 1, true);
        mp.game.controls.enableControlAction(2, 2, true);
        mp.game.controls.enableControlAction(2, 3, true);
        mp.game.controls.enableControlAction(2, 4, true);
        mp.game.controls.enableControlAction(2, 5, true);
        mp.game.controls.enableControlAction(2, 6, true);
    }
	if (intrunk)
	{
		mp.game.controls.enableControlAction(2, 27, false);
		mp.game.controls.disableAllControlActions(32);
	}

    if (deathTimerOn) {
        var secondsLeft = Math.trunc((deathTimer - new Date().getTime()) / 1000);
        var minutes = Math.trunc(secondsLeft / 60);
        var seconds = secondsLeft % 60;
		var sseconds = seconds;
		if (seconds < 10)
		{
			sseconds =  "0" + seconds;
		}
		else if (seconds < 0)
		{
			sseconds = 0;
		}
		global.menu.execute(`death.time="${minutes}:${sseconds}"`);
    }

    if (mp.game.controls.isControlPressed(0, 32) || 
        mp.game.controls.isControlPressed(0, 33) || 
        mp.game.controls.isControlPressed(0, 321) ||
        mp.game.controls.isControlPressed(0, 34) || 
        mp.game.controls.isControlPressed(0, 35) || 
        mp.game.controls.isControlPressed(0, 24) || 
        localplayer.getVariable('InDeath') == true) 
    {
        afkSecondsCount = 0;
    }
    else if (localplayer.isInAnyVehicle(false) && localplayer.vehicle.getSpeed() != 0) 
    {
        afkSecondsCount = 0;
    } 
    else if(global.spectating) 
    { // Чтобы не кикало администратора в режиме слежки
		afkSecondsCount = 0;
	}
});

mp.events.add("playerRuleTriggered", (rule, counter) => {
    if (rule === 'ping' && counter > 5) {
        mp.events.call('notify', 4, 2, "Ваш ping слишком большой. Зайдите позже", 5000);
        mp.events.callRemote("kickclient");
    }
    /*if (rule === 'packetLoss' && counter => 10) {
        mp.events.call('notify', 4, 2, "У Вас большая потеря пакетов. Зайдите позже", 5000);
        mp.events.callRemote("kickclient");
    }*/
});