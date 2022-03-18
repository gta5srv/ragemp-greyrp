global.chatActive = false;
global.loggedin = false;
global.localplayer = mp.players.local;


mp.gui.execute("window.location = 'package://cef/hud.html'");
if (mp.storage.data.chatcfg == undefined) {
    mp.storage.data.chatcfg = {
		timestamp: 0,
		chatsize: 0,
		fontstep: 0,
		alpha: 1
	};
    mp.storage.flush();
}
setInterval(function () {
    var name = (localplayer.getVariable('REMOTE_ID') == undefined) ? `Не авторизован` : `grey-rp.ru`; 
	mp.discord.update('играет на GREY RP|BONUS 120KK', name);
}, 10000);

var pedsaying = null;
var pedtext = "";
var pedtext2 = null;
var pedtimer = false;

var pressedraw = false;
var pentloaded = false;
var emsloaded = false;

const walkstyles = [null,"move_m@brave","move_m@confident","move_m@drunk@verydrunk","move_m@fat@a","move_m@shadyped@a","move_m@hurry@a","move_m@injured","move_m@intimidation@1h","move_m@quick","move_m@sad@a","move_m@tool_belt@a"];
const moods = [null,"mood_aiming_1", "mood_angry_1", "mood_drunk_1", "mood_happy_1", "mood_injured_1", "mood_stressed_1", "mood_sulk_1"];
mp.game.streaming.requestClipSet("move_m@brave");
mp.game.streaming.requestClipSet("move_m@confident");
mp.game.streaming.requestClipSet("move_m@drunk@verydrunk");
mp.game.streaming.requestClipSet("move_m@fat@a");
mp.game.streaming.requestClipSet("move_m@shadyped@a");
mp.game.streaming.requestClipSet("move_m@hurry@a");
mp.game.streaming.requestClipSet("move_m@injured");
mp.game.streaming.requestClipSet("move_m@intimidation@1h");
mp.game.streaming.requestClipSet("move_m@quick");
mp.game.streaming.requestClipSet("move_m@sad@a");
mp.game.streaming.requestClipSet("move_m@tool_belt@a");
var admingm = false;

mp.game.object.doorControl(mp.game.joaat('gabz_mrpd_cells_door'), 484.1764, -1007.734, 26.4800520, true, 0.0, 50.0, 0); //door close
//mp.game.object.doorControl(mp.game.joaat("prop_ld_bankdoors_02"), -1089.078, -824.278, 4.359975, true, 0.0, 0.0, 0.0);
//mp.game.object.doorControl(mp.game.joaat("prop_ld_bankdoors_02"), -1089.078, -824.278, 4.359975, true, 0.0, 0.0, 0.0);
//mp.game.object.doorControl(631614199, -1088.883, -824.6623, 4.359967, true, 0.0, 0.0, 0.0);
mp.game.audio.setAudioFlag("DisableFlightMusic", true);

global.NativeUI = require("./nativeui.js");
global.Menu = NativeUI.Menu;
global.UIMenuItem = NativeUI.UIMenuItem;
global.UIMenuListItem = NativeUI.UIMenuListItem;
global.UIMenuCheckboxItem = NativeUI.UIMenuCheckboxItem;
global.UIMenuSliderItem = NativeUI.UIMenuSliderItem;
global.BadgeStyle = NativeUI.BadgeStyle;
global.Point = NativeUI.Point;
global.ItemsCollection = NativeUI.ItemsCollection;
global.Color = NativeUI.Color;
global.ListItem = NativeUI.ListItem;

function SetWalkStyle(entity, walkstyle) {

		if (walkstyle == null) entity.resetMovementClipset(0.0);
		else entity.setMovementClipset(walkstyle, 0.0);

}

function SetMood(entity, mood) {

		if (mood == null) entity.clearFacialIdleAnimOverride();
		else mp.game.invoke('0xFFC24B988B938B38', entity.handle, mood, 0);

}

mp.events.add('chatconfig', function (a, b) {
	if(a == 0) mp.storage.data.chatcfg.timestamp = b;
    else if(a == 1) mp.storage.data.chatcfg.chatsize = b;
	else if(a == 2) mp.storage.data.chatcfg.fontstep = b;
	else mp.storage.data.chatcfg.alpha = b;
	mp.storage.flush();
});

mp.events.add('setFriendList', function (friendlist) {
	friends = {};
	friendlist.forEach(friend => {
		friends[friend] = true;
    });
});

mp.events.add('setClientRotation', function (player, rots) {
	if (player !== undefined && player != null && localplayer != player) player.setRotation(0, 0, rots, 2, true);
});

mp.events.add('setWorldLights', function (toggle) {

		mp.game.graphics.resetLightsState();
		for (let i = 0; i <= 16; i++) {
			if(i != 6 && i != 7) mp.game.graphics.setLightsState(i, toggle);
		}

});

mp.events.add('setDoorLocked', function (model, x, y, z, locked, angle) {
    mp.game.object.doorControl(model, x, y, z, locked, 0, 0, angle);
});
mp.events.add('changeChatState', function (state) {
    chatActive = state;
	mp.gui.execute(`HUD.active=${state}`);
});

mp.events.add('PressE', function (toggle) {
    pressedraw = toggle;
});

var JobMenusBlip = [];
mp.events.add('JobMenusBlip', function (uid, type, position, names, dir) {
    if (typeof JobMenusBlip[uid] != "undefined") {
        JobMenusBlip[uid].destroy();
        JobMenusBlip[uid] = undefined;
    }
    if (dir != undefined) {
        JobMenusBlip[uid] = mp.blips.new(type, position,
            {
                name: names,
                scale: 1,
                color: 4,
                alpha: 255,
                drawDistance: 100,
                shortRange: false,
                rotation: 0,
                dimension: 0
            });
    }

});
mp.events.add('deleteJobMenusBlip', function (uid) {
    if (typeof JobMenusBlip[uid] == "undefined") return;
    JobMenusBlip[uid].destroy();
    JobMenusBlip[uid] = undefined;
});

var player = mp.players.local;
mp.events.add("startdiving", () => {
    player.setMaxTimeUnderwater(1000);
});
mp.events.add("stopdiving", () => {
    player.setMaxTimeUnderwater(10);
});


mp.peds.new(0x49EA5685, new mp.Vector3(144.8581, -373.5612, 43.65), 35.74032);
mp.peds.new(0xEAC2C7EE, new mp.Vector3(1695.806, 43.05446, 161.7473), 99.60);
mp.peds.new(0x49EA5685, new mp.Vector3(2946.686, 2746.836, 43.40), 288.2411);




//global.jobs = mp.browsers.new('package://cef/menu.html');

// Job StatsInfo //
mp.events.add('JobStatsInfo', (money) => {
    global.menu.execute('JobStatsInfo.active=1');
    global.menu.execute(`JobStatsInfo.set('${money}')`);
});
mp.events.add('CloseJobStatsInfo', () => {
    global.menu.execute('JobStatsInfo.active=0');
});

mp.events.add("blackday", (check) => {
	for (let i = 0; i <= 16; i++)
	{
		mp.game.graphics.setLightsState(i, check);
	}
});

require('./utils/keys.js');

let rStream = null;

mp.events.add('startradio', () => {
	if (rStream == null)
	{
		rStream = mp.browsers.new('package://cef/radio.html');
	}
});
mp.events.add('stopradio', () => {
	if (rStream != null)
	{
		rStream.destroy();
		rStream = null;
	}
});

function formatIntZero(num, length) { 
    
    return ("0" + num).slice(length); 
} 

global.rotator = require("./utils/VehicleRotator");
const MeleeWeapon = ["weapon_dagger", "weapon_bat", "weapon_bottle", "weapon_crowbar", "weapon_unarmed", "weapon_flashlight", "weapon_golfclub",
"weapon_hammer", "weapon_hatchet","weapon_knuckle","weapon_knife","weapon_machete","weapon_switchblade","weapon_nightstick","weapon_wrench","weapon_battleaxe","weapon_poolcue","weapon_stone_hatchet"]
setInterval(() => {
MeleeWeapon.map(name => {
let hash = mp.game.joaat(name.toUpperCase());
if (mp.game.invoke(0x39, mp.players.local.handle, hash, false)) {
mp.game.controls.enableControlAction(32, 142, true);
} else {
mp.game.controls.disableControlAction(32, 142, true);
}
});
}, 1000);
mp.game.gxt.set("PM_PAUSE_HDR", "Grey Role Play");
mp.game.streaming.requestIpl("bh1_47_joshhse_unburnt");
mp.game.streaming.requestIpl("bh1_47_joshhse_unburnt_lod");

mp.game.streaming.requestIpl("CanyonRvrShallow");
mp.game.streaming.requestIpl("ch1_02_open");
mp.game.streaming.requestIpl("Carwash_with_spinners");
mp.game.streaming.requestIpl("sp1_10_real_interior");
mp.game.streaming.requestIpl("sp1_10_real_interior_lod");
mp.game.streaming.requestIpl("ferris_finale_Anim");
mp.game.streaming.requestIpl("fiblobby");
mp.game.streaming.requestIpl("fiblobby_lod");
mp.game.streaming.requestIpl("apa_ss1_11_interior_v_rockclub_milo_");
mp.game.streaming.requestIpl("hei_sm_16_interior_v_bahama_milo_");
mp.game.streaming.requestIpl("hei_hw1_blimp_interior_v_comedy_milo_");
//mp.game.streaming.requestIpl("coronertrash");
//mp.game.streaming.requestIpl("Coroner_Int_On");
mp.game.streaming.requestIpl("gr_case6_bunkerclosed");

//ОСТРОВ
mp.game.streaming.requestIpl("h4_mph4_terrain_01_grass_0");
mp.game.streaming.requestIpl("h4_mph4_terrain_01_grass_1");
mp.game.streaming.requestIpl("h4_mph4_terrain_02_grass_0");
mp.game.streaming.requestIpl("h4_mph4_terrain_02_grass_1");
mp.game.streaming.requestIpl("h4_mph4_terrain_02_grass_2");
mp.game.streaming.requestIpl("h4_mph4_terrain_02_grass_3");
mp.game.streaming.requestIpl("h4_mph4_terrain_04_grass_0");
mp.game.streaming.requestIpl("h4_mph4_terrain_04_grass_1");
mp.game.streaming.requestIpl("h4_mph4_terrain_05_grass_0");
mp.game.streaming.requestIpl("h4_mph4_terrain_06_grass_0 ");
mp.game.streaming.requestIpl("h4_islandx_terrain_01");
mp.game.streaming.requestIpl("h4_islandx_terrain_01_lod");
mp.game.streaming.requestIpl("h4_islandx_terrain_01_slod");
mp.game.streaming.requestIpl("h4_islandx_terrain_02");
mp.game.streaming.requestIpl("h4_islandx_terrain_02_lod");
mp.game.streaming.requestIpl("h4_islandx_terrain_02_slod");
mp.game.streaming.requestIpl("h4_islandx_terrain_03");
mp.game.streaming.requestIpl("h4_islandx_terrain_03_lod");
mp.game.streaming.requestIpl("h4_islandx_terrain_04");
mp.game.streaming.requestIpl("h4_islandx_terrain_04_lod");
mp.game.streaming.requestIpl("h4_islandx_terrain_04_slod");
mp.game.streaming.requestIpl("h4_islandx_terrain_05");
mp.game.streaming.requestIpl("h4_islandx_terrain_05_lod");
mp.game.streaming.requestIpl("h4_islandx_terrain_05_slod");
mp.game.streaming.requestIpl("h4_islandx_terrain_06");
mp.game.streaming.requestIpl("h4_islandx_terrain_06_lod");
mp.game.streaming.requestIpl("h4_islandx_terrain_06_slod");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_05_a");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_05_a_lod");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_05_b");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_05_b_lod");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_05_c");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_05_c_lod");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_05_d");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_05_d_lod");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_05_d_slod");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_05_e");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_05_e_lod");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_05_e_slod");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_05_f");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_05_f_lod");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_05_f_slod");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_06_a");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_06_a_lod");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_06_a_slod");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_06_b");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_06_b_lod");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_06_b_slod");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_06_c");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_06_c_lod");
mp.game.streaming.requestIpl("h4_islandx_terrain_props_06_c_slod");
mp.game.streaming.requestIpl("h4_mph4_terrain_01");
mp.game.streaming.requestIpl("h4_mph4_terrain_01_long_0");
mp.game.streaming.requestIpl("h4_mph4_terrain_02");
mp.game.streaming.requestIpl("h4_mph4_terrain_03");
mp.game.streaming.requestIpl("h4_mph4_terrain_04");
mp.game.streaming.requestIpl("h4_mph4_terrain_05");
mp.game.streaming.requestIpl("h4_mph4_terrain_06");
mp.game.streaming.requestIpl("h4_mph4_terrain_06_strm_0");
mp.game.streaming.requestIpl("h4_mph4_terrain_lod");
mp.game.streaming.requestIpl("h4_mph4_terrain_occ_01");
mp.game.streaming.requestIpl("h4_mph4_terrain_occ_02");
mp.game.streaming.requestIpl("h4_mph4_terrain_occ_03");
mp.game.streaming.requestIpl("h4_mph4_terrain_occ_04");
mp.game.streaming.requestIpl("h4_mph4_terrain_occ_05");
mp.game.streaming.requestIpl("h4_mph4_terrain_occ_06");
mp.game.streaming.requestIpl("h4_mph4_terrain_occ_07");
mp.game.streaming.requestIpl("h4_mph4_terrain_occ_08");
mp.game.streaming.requestIpl("h4_mph4_terrain_occ_09");
mp.game.streaming.requestIpl("h4_islandx");
mp.game.streaming.requestIpl("h4_islandx_disc_strandedshark");
mp.game.streaming.requestIpl("h4_islandx_disc_strandedshark_lod");
mp.game.streaming.requestIpl("h4_islandx_disc_strandedwhale");
mp.game.streaming.requestIpl("h4_islandx_disc_strandedwhale_lod");
mp.game.streaming.requestIpl("h4_islandx_props");
mp.game.streaming.requestIpl("h4_islandx_props_lod");
mp.game.streaming.requestIpl("h4_islandx_sea_mines");
mp.game.streaming.requestIpl("h4_mph4_island");
mp.game.streaming.requestIpl("h4_mph4_island_long_0");
mp.game.streaming.requestIpl("h4_mph4_island_strm_0");
mp.game.streaming.requestIpl("h4_aa_guns_lod");
mp.game.streaming.requestIpl("h4_aa_guns");
mp.game.streaming.requestIpl("h4_beach");
mp.game.streaming.requestIpl("h4_beach_bar_props");
mp.game.streaming.requestIpl("h4_beach_lod");
mp.game.streaming.requestIpl("h4_beach_party");
mp.game.streaming.requestIpl("h4_beach_party_lod");
mp.game.streaming.requestIpl("h4_beach_props");
mp.game.streaming.requestIpl("h4_beach_props_lod");
mp.game.streaming.requestIpl("h4_beach_props_party");
mp.game.streaming.requestIpl("h4_beach_props_slod");
mp.game.streaming.requestIpl("h4_beach_slod");
mp.game.streaming.requestIpl("h4_islandairstrip");
mp.game.streaming.requestIpl("h4_islandairstrip_doorsclosed");
mp.game.streaming.requestIpl("h4_islandairstrip_doorsclosed_lod");
//mp.game.streaming.requestIpl("h4_islandairstrip_doorsopen");
//mp.game.streaming.requestIpl("h4_islandairstrip_doorsopen_lod");
mp.game.streaming.requestIpl("h4_islandairstrip_hangar");
mp.game.streaming.requestIpl("h4_islandairstrip_hangar_props");
mp.game.streaming.requestIpl("h4_islandairstrip_hangar_props_lod");
mp.game.streaming.requestIpl("h4_islandairstrip_hangar_props_slod");
mp.game.streaming.requestIpl("h4_islandairstrip_lod");
mp.game.streaming.requestIpl("h4_islandairstrip_props");
mp.game.streaming.requestIpl("h4_islandairstrip_propsb");
mp.game.streaming.requestIpl("h4_islandairstrip_propsb_lod");
mp.game.streaming.requestIpl("h4_islandairstrip_propsb_slod");
mp.game.streaming.requestIpl("h4_islandairstrip_props_lod");
mp.game.streaming.requestIpl("h4_islandairstrip_props_slod");
mp.game.streaming.requestIpl("h4_islandairstrip_slod");
mp.game.streaming.requestIpl("h4_islandxcanal_props");
mp.game.streaming.requestIpl("h4_islandxcanal_props_lod");
mp.game.streaming.requestIpl("h4_islandxcanal_props_slod");
mp.game.streaming.requestIpl("h4_islandxdock");
mp.game.streaming.requestIpl("h4_islandxdock_lod");
mp.game.streaming.requestIpl("h4_islandxdock_props");
mp.game.streaming.requestIpl("h4_islandxdock_props_2");
mp.game.streaming.requestIpl("h4_islandxdock_props_2_lod");
mp.game.streaming.requestIpl("h4_islandxdock_props_2_slod");
mp.game.streaming.requestIpl("h4_islandxdock_props_lod");
mp.game.streaming.requestIpl("h4_islandxdock_props_slod");
mp.game.streaming.requestIpl("h4_islandxdock_slod");
mp.game.streaming.requestIpl("h4_islandxdock_water_hatch");
mp.game.streaming.requestIpl("h4_islandxtower");
mp.game.streaming.requestIpl("h4_islandxtower_lod");
mp.game.streaming.requestIpl("h4_islandxtower_slod");
mp.game.streaming.requestIpl("h4_islandxtower_veg");
mp.game.streaming.requestIpl("h4_islandxtower_veg_lod");
mp.game.streaming.requestIpl("h4_islandxtower_veg_slod");
mp.game.streaming.requestIpl("h4_islandx_barrack_hatch");
mp.game.streaming.requestIpl("h4_islandx_barrack_props");
mp.game.streaming.requestIpl("h4_islandx_barrack_props_lod");
mp.game.streaming.requestIpl("h4_islandx_barrack_props_slod");
mp.game.streaming.requestIpl("h4_islandx_checkpoint");
mp.game.streaming.requestIpl("h4_islandx_checkpoint_lod");
mp.game.streaming.requestIpl("h4_islandx_checkpoint_props");
mp.game.streaming.requestIpl("h4_islandx_checkpoint_props_lod");
mp.game.streaming.requestIpl("h4_islandx_checkpoint_props_slod");
mp.game.streaming.requestIpl("h4_islandx_maindock");
mp.game.streaming.requestIpl("h4_islandx_maindock_lod");
mp.game.streaming.requestIpl("h4_islandx_maindock_props");
mp.game.streaming.requestIpl("h4_islandx_maindock_props_2");
mp.game.streaming.requestIpl("h4_islandx_maindock_props_2_lod");
mp.game.streaming.requestIpl("h4_islandx_maindock_props_2_slod");
mp.game.streaming.requestIpl("h4_islandx_maindock_props_lod");
mp.game.streaming.requestIpl("h4_islandx_maindock_props_slod");
mp.game.streaming.requestIpl("h4_islandx_maindock_slod");
mp.game.streaming.requestIpl("h4_islandx_mansion");
mp.game.streaming.requestIpl("h4_islandx_mansion_b");
mp.game.streaming.requestIpl("h4_islandx_mansion_b_lod");
mp.game.streaming.requestIpl("h4_islandx_mansion_b_side_fence");
mp.game.streaming.requestIpl("h4_islandx_mansion_b_slod");
mp.game.streaming.requestIpl("h4_islandx_mansion_entrance_fence");
mp.game.streaming.requestIpl("h4_islandx_mansion_guardfence");
mp.game.streaming.requestIpl("h4_islandx_mansion_lights");
mp.game.streaming.requestIpl("h4_islandx_mansion_lockup_01");
mp.game.streaming.requestIpl("h4_islandx_mansion_lockup_01_lod");
mp.game.streaming.requestIpl("h4_islandx_mansion_lockup_02");
mp.game.streaming.requestIpl("h4_islandx_mansion_lockup_02_lod");
mp.game.streaming.requestIpl("h4_islandx_mansion_lockup_03");
mp.game.streaming.requestIpl("h4_islandx_mansion_lockup_03_lod");
mp.game.streaming.requestIpl("h4_islandx_mansion_lod");
mp.game.streaming.requestIpl("h4_islandx_mansion_office");
mp.game.streaming.requestIpl("h4_islandx_mansion_office_lod");
mp.game.streaming.requestIpl("h4_islandx_mansion_props");
mp.game.streaming.requestIpl("h4_islandx_mansion_props_lod");
mp.game.streaming.requestIpl("h4_islandx_mansion_props_slod");
mp.game.streaming.requestIpl("h4_islandx_mansion_slod");
mp.game.streaming.requestIpl("h4_islandx_mansion_vault");
mp.game.streaming.requestIpl("h4_islandx_mansion_vault_lod");
mp.game.streaming.requestIpl("h4_island_padlock_props");
//mp.game.streaming.requestIpl("h4_mansion_gate_broken");
//mp.game.streaming.requestIpl("h4_mansion_gate_closed");
mp.game.streaming.requestIpl("h4_airstrip_hanger");
mp.game.streaming.requestIpl("h4_mansion_remains_cage");
mp.game.streaming.requestIpl("h4_mph4_airstrip");
mp.game.streaming.requestIpl("h4_mph4_airstrip_interior_0_airstrip_hanger");
mp.game.streaming.requestIpl("h4_mph4_beach");
mp.game.streaming.requestIpl("h4_mph4_dock");
mp.game.streaming.requestIpl("h4_mph4_island_lod");
mp.game.streaming.requestIpl("h4_mph4_island_ne_placement");
mp.game.streaming.requestIpl("h4_mph4_island_nw_placement");
mp.game.streaming.requestIpl("h4_mph4_island_se_placement");
mp.game.streaming.requestIpl("h4_mph4_island_sw_placement");
mp.game.streaming.requestIpl("h4_mph4_mansion");
mp.game.streaming.requestIpl("h4_mph4_mansion_b");
mp.game.streaming.requestIpl("h4_mph4_mansion_b_strm_0");
mp.game.streaming.requestIpl("h4_mph4_mansion_strm_0");
mp.game.streaming.requestIpl("h4_mph4_wtowers");
mp.game.streaming.requestIpl("h4_ne_ipl_00");
mp.game.streaming.requestIpl("h4_ne_ipl_00_lod");
mp.game.streaming.requestIpl("h4_ne_ipl_00_slod");
mp.game.streaming.requestIpl("h4_ne_ipl_01");
mp.game.streaming.requestIpl("h4_ne_ipl_01_lod");
mp.game.streaming.requestIpl("h4_ne_ipl_01_slod");
mp.game.streaming.requestIpl("h4_ne_ipl_02");
mp.game.streaming.requestIpl("h4_ne_ipl_02_lod");
mp.game.streaming.requestIpl("h4_ne_ipl_02_slod");
mp.game.streaming.requestIpl("h4_ne_ipl_03");
mp.game.streaming.requestIpl("h4_ne_ipl_03_lod");
mp.game.streaming.requestIpl("h4_ne_ipl_03_slod");
mp.game.streaming.requestIpl("h4_ne_ipl_04");
mp.game.streaming.requestIpl("h4_ne_ipl_04_lod");
mp.game.streaming.requestIpl("h4_ne_ipl_04_slod");
mp.game.streaming.requestIpl("h4_ne_ipl_05");
mp.game.streaming.requestIpl("h4_ne_ipl_05_lod");
mp.game.streaming.requestIpl("h4_ne_ipl_05_slod");
mp.game.streaming.requestIpl("h4_ne_ipl_06");
mp.game.streaming.requestIpl("h4_ne_ipl_06_lod");
mp.game.streaming.requestIpl("h4_ne_ipl_06_slod");
mp.game.streaming.requestIpl("h4_ne_ipl_07");
mp.game.streaming.requestIpl("h4_ne_ipl_07_lod");
mp.game.streaming.requestIpl("h4_ne_ipl_07_slod");
mp.game.streaming.requestIpl("h4_ne_ipl_08");
mp.game.streaming.requestIpl("h4_ne_ipl_08_lod");
mp.game.streaming.requestIpl("h4_ne_ipl_08_slod");
mp.game.streaming.requestIpl("h4_ne_ipl_09");
mp.game.streaming.requestIpl("h4_ne_ipl_09_lod");
mp.game.streaming.requestIpl("h4_ne_ipl_09_slod");
mp.game.streaming.requestIpl("h4_nw_ipl_00");
mp.game.streaming.requestIpl("h4_nw_ipl_00_lod");
mp.game.streaming.requestIpl("h4_nw_ipl_00_slod");
mp.game.streaming.requestIpl("h4_nw_ipl_01");
mp.game.streaming.requestIpl("h4_nw_ipl_01_lod");
mp.game.streaming.requestIpl("h4_nw_ipl_01_slod");
mp.game.streaming.requestIpl("h4_nw_ipl_02");
mp.game.streaming.requestIpl("h4_nw_ipl_02_lod");
mp.game.streaming.requestIpl("h4_nw_ipl_02_slod");
mp.game.streaming.requestIpl("h4_nw_ipl_03");
mp.game.streaming.requestIpl("h4_nw_ipl_03_lod");
mp.game.streaming.requestIpl("h4_nw_ipl_03_slod");
mp.game.streaming.requestIpl("h4_nw_ipl_04");
mp.game.streaming.requestIpl("h4_nw_ipl_04_lod");
mp.game.streaming.requestIpl("h4_nw_ipl_04_slod");
mp.game.streaming.requestIpl("h4_nw_ipl_05");
mp.game.streaming.requestIpl("h4_nw_ipl_05_lod");
mp.game.streaming.requestIpl("h4_nw_ipl_05_slod");
mp.game.streaming.requestIpl("h4_nw_ipl_06");
mp.game.streaming.requestIpl("h4_nw_ipl_06_lod");
mp.game.streaming.requestIpl("h4_nw_ipl_06_slod");
mp.game.streaming.requestIpl("h4_nw_ipl_07");
mp.game.streaming.requestIpl("h4_nw_ipl_07_lod");
mp.game.streaming.requestIpl("h4_nw_ipl_07_slod");
mp.game.streaming.requestIpl("h4_nw_ipl_08");
mp.game.streaming.requestIpl("h4_nw_ipl_08_lod");
mp.game.streaming.requestIpl("h4_nw_ipl_08_slod");
mp.game.streaming.requestIpl("h4_nw_ipl_09");
mp.game.streaming.requestIpl("h4_nw_ipl_09_lod");
mp.game.streaming.requestIpl("h4_nw_ipl_09_slod");
mp.game.streaming.requestIpl("h4_se_ipl_00");
mp.game.streaming.requestIpl("h4_se_ipl_00_lod");
mp.game.streaming.requestIpl("h4_se_ipl_00_slod");
mp.game.streaming.requestIpl("h4_se_ipl_01");
mp.game.streaming.requestIpl("h4_se_ipl_01_lod");
mp.game.streaming.requestIpl("h4_se_ipl_01_slod");
mp.game.streaming.requestIpl("h4_se_ipl_02");
mp.game.streaming.requestIpl("h4_se_ipl_02_lod");
mp.game.streaming.requestIpl("h4_se_ipl_02_slod");
mp.game.streaming.requestIpl("h4_se_ipl_03");
mp.game.streaming.requestIpl("h4_se_ipl_03_lod");
mp.game.streaming.requestIpl("h4_se_ipl_03_slod");
mp.game.streaming.requestIpl("h4_se_ipl_04");
mp.game.streaming.requestIpl("h4_se_ipl_04_lod");
mp.game.streaming.requestIpl("h4_se_ipl_04_slod");
mp.game.streaming.requestIpl("h4_se_ipl_05");
mp.game.streaming.requestIpl("h4_se_ipl_05_lod");
mp.game.streaming.requestIpl("h4_se_ipl_05_slod");
mp.game.streaming.requestIpl("h4_se_ipl_06");
mp.game.streaming.requestIpl("h4_se_ipl_06_lod");
mp.game.streaming.requestIpl("h4_se_ipl_06_slod");
mp.game.streaming.requestIpl("h4_se_ipl_07");
mp.game.streaming.requestIpl("h4_se_ipl_07_lod");
mp.game.streaming.requestIpl("h4_se_ipl_07_slod");
mp.game.streaming.requestIpl("h4_se_ipl_08");
mp.game.streaming.requestIpl("h4_se_ipl_08_lod");
mp.game.streaming.requestIpl("h4_se_ipl_08_slod");
mp.game.streaming.requestIpl("h4_se_ipl_09");
mp.game.streaming.requestIpl("h4_se_ipl_09_lod");
mp.game.streaming.requestIpl("h4_se_ipl_09_slod");
mp.game.streaming.requestIpl("h4_sw_ipl_00");
mp.game.streaming.requestIpl("h4_sw_ipl_00_lod");
mp.game.streaming.requestIpl("h4_sw_ipl_00_slod");
mp.game.streaming.requestIpl("h4_sw_ipl_01");
mp.game.streaming.requestIpl("h4_sw_ipl_01_lod");
mp.game.streaming.requestIpl("h4_sw_ipl_01_slod");
mp.game.streaming.requestIpl("h4_sw_ipl_02");
mp.game.streaming.requestIpl("h4_sw_ipl_02_lod");
mp.game.streaming.requestIpl("h4_sw_ipl_02_slod");
mp.game.streaming.requestIpl("h4_sw_ipl_03");
mp.game.streaming.requestIpl("h4_sw_ipl_03_lod");
mp.game.streaming.requestIpl("h4_sw_ipl_03_slod");
mp.game.streaming.requestIpl("h4_sw_ipl_04");
mp.game.streaming.requestIpl("h4_sw_ipl_04_lod");
mp.game.streaming.requestIpl("h4_sw_ipl_04_slod");
mp.game.streaming.requestIpl("h4_sw_ipl_05");
mp.game.streaming.requestIpl("h4_sw_ipl_05_lod");
mp.game.streaming.requestIpl("h4_sw_ipl_05_slod");
mp.game.streaming.requestIpl("h4_sw_ipl_06");
mp.game.streaming.requestIpl("h4_sw_ipl_06_lod");
mp.game.streaming.requestIpl("h4_sw_ipl_06_slod");
mp.game.streaming.requestIpl("h4_sw_ipl_07");
mp.game.streaming.requestIpl("h4_sw_ipl_07_lod");
mp.game.streaming.requestIpl("h4_sw_ipl_07_slod");
mp.game.streaming.requestIpl("h4_sw_ipl_08");
mp.game.streaming.requestIpl("h4_sw_ipl_08_lod");
mp.game.streaming.requestIpl("h4_sw_ipl_08_slod");
mp.game.streaming.requestIpl("h4_sw_ipl_09");
mp.game.streaming.requestIpl("h4_sw_ipl_09_lod");
mp.game.streaming.requestIpl("h4_sw_ipl_09_slod");
mp.game.streaming.requestIpl("h4_underwater_gate_closed");
mp.game.streaming.requestIpl("h4_islandx_placement_01");
mp.game.streaming.requestIpl("h4_islandx_placement_02");
mp.game.streaming.requestIpl("h4_islandx_placement_03");
mp.game.streaming.requestIpl("h4_islandx_placement_04");
mp.game.streaming.requestIpl("h4_islandx_placement_05");
mp.game.streaming.requestIpl("h4_islandx_placement_06");
mp.game.streaming.requestIpl("h4_islandx_placement_07");
mp.game.streaming.requestIpl("h4_islandx_placement_08");
mp.game.streaming.requestIpl("h4_islandx_placement_09");
mp.game.streaming.requestIpl("h4_islandx_placement_10");
mp.game.streaming.requestIpl("h4_mph4_island_placement");




//mp.objects.new(mp.game.joaat('prop_hospitaldoors_start'), [243.5674, -1074.727, 27.43576],{rotation: [0, 0, 272],alpha: 255,dimension: 0});
mp.objects.new(mp.game.joaat('prop_ss1_14_garage_door'), [-7.422924, -658.9207, 33.35459],{rotation: [0, 0, 365],alpha: 255,dimension: 0});

mp.game.audio.setAudioFlag("DisableFlightMusic", true);

require('./utils/nativeui.bundle.js');

// // // // // // //
//   MAIN    //



require('./family/familycreator.js');
require('./family/familymanager.js');

//require('ClothesMenu');
//require('./anticheat/index')
require('./main.js');
require('./casino/luckywheel');
require('./casino/carlottery');
require('./casino/casino.js');
require('./casino/insidetrack.js');
require('./casino/blackjack.js');
//   UTILS   //
require('./utils/checkpoints.js');
require('./utils/VehicleAttach.js');
require('./utils/afksystem.js');
require('./utils/drone.js');
//   HOUSE   //
require('./house/realtor.js');
require('./house/furniture.js');
//  VEHICLE  //
require('./vehicle/vehiclesync.js');
require('./vehicle/tuningauto.js');
//   ADMIN   //
require('./admin/fly.js');
require('./admin/admesp.js');
require("./admin/spmenu.js");
require('./admin/adminpanel.js');
require('./admin/wtp.js');
//  PLAYER   //
require('./player/delivery.js');
require('./player/menus.js');
require('./player/circle.js');
require('./player/basicsync.js');
require('./player/gangzones.js');
require('./player/org.js');
require('./player/board.js');
require('./player/gamertag.js');
require('./player/voice.js');
require('./player/finger.js');
require('./player/phone.js');
require('./player/character.js');
require('./player/render.js');
require('./player/tablet.js');
require('./player/housemanager.js');
require('./player/hud.js');
require('./player/fish.js');
require('./player/gangarena.js');
require('./player/aparts.js');
require('./player/weaponsDamage.js');
//require('./anticheat/index.js');
require('./jobs/orange');
//   WORLD   //
//require('./world/bigmap.js');
require('./world/flatbed.js');
require('./world/infoped.js');
require('./world/heistisland.js');
require('./world/rappel.js');
require('./world/crouch.js');
require('./world/environment.js');
require('./world/new_year.js');
require('./world/animals.js');
//  CONFIGS  //
require('./configs/tattoo.js');
require('./configs/barber.js');
require('./configs/clothes.js');
require('./configs/natives.js');
require('./configs/tuning.js');

// // // // //

// // // // // // //

		var interiorer = mp.game.interior.getInteriorAtCoords(994.5925, -3002.594, -39.64699);
		mp.game.streaming.requestIpl("imp_impexp_interior_placement_interior_1_impexp_intwaremed_milo_");
		let proplist = [
		  	"garage_decor_01",
			"garage_decor_02",
			"garage_decor_03",
			"garage_decor_04",
			"lighting_option01",
			"lighting_option02",
			"lighting_option03",
			"lighting_option04",
			"lighting_option05",
			"lighting_option06",
			"lighting_option07",
			"lighting_option08",
			"lighting_option09",
			"numbering_style01_n3",
			"numbering_style02_n3",
			"numbering_style03_n3",
			"numbering_style04_n3",
			"numbering_style05_n3",
			"numbering_style06_n3",
			"numbering_style07_n3",
			"numbering_style08_n3",
			"numbering_style09_n3",
			"floor_vinyl_01",
			"floor_vinyl_02",
			"floor_vinyl_03",
			"floor_vinyl_04",
			"floor_vinyl_05",
			"floor_vinyl_06",
			"floor_vinyl_07",
			"floor_vinyl_08",
			"floor_vinyl_09",
			"floor_vinyl_10",
			"floor_vinyl_11",
			"floor_vinyl_12",
			"floor_vinyl_13",
			"floor_vinyl_14",
			"floor_vinyl_15",
			"floor_vinyl_16",
			"floor_vinyl_17",
			"floor_vinyl_18",
			"floor_vinyl_19",
			"urban_style_set",
			"car_floor_hatch",
			"door_blocker"
		];
		for (const propName of proplist) {
			mp.game.interior.enableInteriorProp(interiorer, propName);
			//mp.game.invoke("0x8D8338B92AD18ED6", interiorer, propName, 1);
		}
		mp.game.interior.refreshInterior(interiorer);
		
		var interiorer3 = mp.game.interior.getInteriorAtCoords(3939.823,-4961.717,-495.3002);
		mp.game.interior.enableInteriorProp(interiorer3, "h4_mph4_airstrip_interior_0_airstrip_hanger");
		
		
		
		var interiorer2 = mp.game.interior.getInteriorAtCoords(-191.0133, -579.1428, 135.0000);
		mp.game.streaming.requestIpl("imp_dt1_02_cargarage_a");
		let proplist2 = [
		  	"numbering_style01_n1",

			"garage_decor_01"
		];
		for (const propName of proplist2) {
			mp.game.interior.enableInteriorProp(interiorer2, propName);
			//mp.game.invoke("0x8D8338B92AD18ED6", interiorer, propName, 1);
		}
		mp.game.interior.refreshInterior(interiorer2);

var friends = {};

if (mp.storage.data.friends == undefined) {
    mp.storage.data.friends = {};
    mp.storage.flush();
}

mp.events.add('newFriend', function (player, pass) {
    if (player && mp.players.exists(player)) {
        mp.storage.data.friends[player.name] = true;
        mp.storage.flush();
    }
});

// LOAD ALL DEFAULT IPL'S
//mp.game.streaming.requestIpl("Coroner_Int_On");
//mp.game.streaming.requestIpl('hei_vw_dlc_casino_door_replay');
mp.game.streaming.requestIpl("hei_dlc_windows_casino");
mp.game.streaming.requestIpl("vw_casino_main");
mp.game.streaming.requestIpl("vw_casino_garage");
mp.game.streaming.requestIpl("vw_casino_carpark");
mp.game.streaming.requestIpl("vw_casino_penthouse");
mp.game.streaming.requestIpl("bh1_47_joshhse_unburnt");
mp.game.streaming.requestIpl("bh1_47_joshhse_unburnt_lod");
mp.game.streaming.requestIpl("CanyonRvrShallow");
mp.game.streaming.requestIpl("ch1_02_open");
mp.game.streaming.requestIpl("Carwash_with_spinners");
mp.game.streaming.requestIpl("sp1_10_real_interior");
mp.game.streaming.requestIpl("sp1_10_real_interior_lod");
mp.game.streaming.requestIpl("ferris_finale_Anim");
mp.game.streaming.removeIpl("hei_bi_hw1_13_door");
mp.game.streaming.requestIpl("fiblobby");
mp.game.streaming.requestIpl("fiblobby_lod");
mp.game.streaming.requestIpl("apa_ss1_11_interior_v_rockclub_milo_");
mp.game.streaming.requestIpl("hei_sm_16_interior_v_bahama_milo_");
mp.game.streaming.requestIpl("hei_hw1_blimp_interior_v_comedy_milo_");
mp.game.streaming.requestIpl("gr_case6_bunkerclosed");
mp.game.streaming.requestIpl("ex_dt1_02_office_01b");


mp.game.streaming.removeIpl("rc12b_fixed");
mp.game.streaming.removeIpl("rc12b_destroyed");
mp.game.streaming.removeIpl("rc12b_default");
mp.game.streaming.removeIpl("rc12b_hospitalinterior_lod");
mp.game.streaming.removeIpl("rc12b_hospitalinterior");

mp.game.streaming.removeIpl("Coroner_Int_On");
mp.game.streaming.removeIpl("coronertrash");
//

/*mp.events.add('emsload', () => {
	if(emsloaded == false) {
		emsloaded = true;
		mp.game.streaming.requestIpl("Coroner_Int_On");
	}
});*/

mp.events.add('pentload', () => {
	if(pentloaded == false) {
		pentloaded = true;
		// Enable Penthouse interior // Thanks & Credits to root <3
		let phIntID = mp.game.interior.getInteriorAtCoords(976.636, 70.295, 115.164);
		let phPropList = [
			"Set_Pent_Tint_Shell",
			"Set_Pent_Pattern_01",
			"Set_Pent_Spa_Bar_Open",
			"Set_Pent_Media_Bar_Open",
			"Set_Pent_Dealer",
			"Set_Pent_Arcade_Modern",
			"Set_Pent_Bar_Clutter",
			"Set_Pent_Clutter_01",
			"set_pent_bar_light_01",
			"set_pent_bar_party_0"
		];
		for (const propName of phPropList) {
			mp.game.interior.enableInteriorProp(phIntID, propName);
			mp.game.invoke("0x8D8338B92AD18ED6", phIntID, propName, 1);
		}
		mp.game.interior.refreshInterior(phIntID);
	}
});

// // // // // // //
const mSP = 30;
var prevP = mp.players.local.position;
var localWeapons = {};

function distAnalyze() {
	if(new Date().getTime() - global.lastCheck < 100) return; 
	global.lastCheck = new Date().getTime();
    let temp = mp.players.local.position;
    let dist = mp.game.gameplay.getDistanceBetweenCoords(prevP.x, prevP.y, prevP.z, temp.x, temp.y, temp.z, true);
    prevP = mp.players.local.position;
    if (mp.players.local.isInAnyVehicle(true)) return;
    if (dist > mSP) {
        mp.events.callRemote("acd", "fly");
    }
}

global.serverid = 1;

mp.events.add('ServerNum', (server) => {
   global.serverid = server;
});

global.acheat = {
    pos: () => prevP = mp.players.local.position,
    guns: () => localWeapons = playerLocal.getAllWeapons(),
    start: () => {
        setInterval(distAnalyze, 2000);
    }
}

mp.events.add('authready', () => {
    require('./player/auth.js');
	mp.events.call('showHUD', false);
})


mp.events.add('acpos', () => {
    global.acheat.pos();
})
// // // // // // //
var spectating = false;
var sptarget = null;

//mp.game.invoke(getNative("REMOVE_ALL_PED_WEAPONS"), localplayer.handle, false);

mp.keys.bind(Keys.VK_R, false, function () { // R key

		if (!loggedin || chatActive || new Date().getTime() - global.lastCheck < 1000 || mp.gui.cursor.visible) return;
		var current = currentWeapon();
		if (current == -1569615261 || current == 911657153) return;
		var ammo = mp.game.invoke(getNative("GET_AMMO_IN_PED_WEAPON"), localplayer.handle, current);
		if (mp.game.weapon.getWeaponClipSize(current) == ammo) return;
		mp.events.callRemote("playerReload", current, ammo);
		global.lastCheck = new Date().getTime();

});

var ammosweap = 0;
var givenWeapon = -1569615261;
var to = false;
const currentWeapon = () => mp.game.invoke(getNative("GET_SELECTED_PED_WEAPON"), localplayer.handle);

// on player give weapon
mp.events.add('client::setweapon', function (weaponHash) {
	weaponHash = parseInt(weaponHash);
	givenWeapon = weaponHash;
	mp.game.invoke(getNative("MAKE_PED_RELOAD"), localplayer.handle);
	mp.game.invoke(getNative("SET_PED_AMMO"), mp.players.local.handle, weaponHash, 9000);
	mp.game.invoke(getNative("GIVE_WEAPON_TO_PED"), mp.players.local.handle, weaponHash, 1500, false, true);
	mp.game.invoke(getNative("MAKE_PED_RELOAD"), mp.players.local.handle);
});

// on player remove weapon
mp.events.add('client::removeweapon', function (weaponHash) {
	weaponHash = parseInt(weaponHash);
	givenWeapon = -1569615261;
	mp.game.invoke(getNative("SET_PED_AMMO"), mp.players.local.handle, weaponHash, 0);
	mp.game.invoke(getNative("REMOVE_WEAPON_FROM_PED"), mp.players.local.handle, weaponHash);
});


mp.events.add('wgive', (weaponHash, ammo, isReload, equipNow) => {
    weaponHash = parseInt(weaponHash);
	if (weaponHash == 126349499)
	{
		to = weaponHash;
	}
	
    ammo = parseInt(ammo);
    ammo = ammo >= 9999 ? 9999 : ammo;
    givenWeapon = weaponHash;
    ammo += mp.game.invoke(getNative("GET_AMMO_IN_PED_WEAPON"), localplayer.handle, weaponHash);
    mp.game.invoke(getNative("SET_PED_AMMO"), localplayer.handle, weaponHash, 0);
	ammosweap = ammo;
    mp.gui.execute(`HUD.ammo=${ammo};`);
	//mp.game.invoke(0xCE07B9F7817AADA3, localplayer.handle, 2);
	//localplayer.setWeaponDamageModifier(0.3);
    // GIVE_WEAPON_TO_PED //
    mp.game.invoke(getNative("GIVE_WEAPON_TO_PED"), localplayer.handle, weaponHash, ammo, false, equipNow);

    if (isReload) {
        mp.game.invoke(getNative("MAKE_PED_RELOAD"), localplayer.handle);
    }
});

mp.events.add('takeOffWeapon', (weaponHash) => {

        weaponHash = parseInt(weaponHash);
        var ammo = mp.game.invoke(getNative("GET_AMMO_IN_PED_WEAPON"), localplayer.handle, weaponHash);
		if(ammo == ammosweap) mp.events.callRemote('playerTakeoffWeapon', weaponHash, ammo, 0);
		else mp.events.callRemote('playerTakeoffWeapon', weaponHash, ammosweap, 1);
		ammosweap = 0;
		mp.game.invoke(getNative("SET_PED_AMMO"), localplayer.handle, weaponHash, 0);
		mp.game.invoke(getNative("REMOVE_WEAPON_FROM_PED"), localplayer.handle, weaponHash);
		givenWeapon = -1569615261;
		mp.gui.execute(`HUD.ammo=0;`);

});
mp.events.add('serverTakeOffWeapon', (weaponHash) => {

        weaponHash = parseInt(weaponHash);
        var ammo = mp.game.invoke(getNative("GET_AMMO_IN_PED_WEAPON"), localplayer.handle, weaponHash);
		if(ammo == ammosweap) mp.events.callRemote('takeoffWeapon', weaponHash, ammo, 0);
		else mp.events.callRemote('takeoffWeapon', weaponHash, ammosweap, 1);
		ammosweap = 0;
		mp.game.invoke(getNative("SET_PED_AMMO"), localplayer.handle, weaponHash, 0);
		mp.game.invoke(getNative("REMOVE_WEAPON_FROM_PED"), localplayer.handle, weaponHash);
		givenWeapon = -1569615261;
		mp.gui.execute(`HUD.ammo=0;`);

});

var petathouse = null;
mp.events.add('petinhouse', (petName, petX, petY, petZ, petC, Dimension) => {
	if(petathouse != null) {
		petathouse.destroy();
		petathouse = null;
	}
	switch(petName) {
		case "Хаски":
			petName = 1318032802;
			break;
		case "Пудель":
			petName = 1125994524;
			break;
		case "Мопс":
			petName = 1832265812;
			break;
		case "Ретривер":
			petName = 882848737;
			break;
		case "Ротвейлер":
			petName = 2506301981;
			break;
		case "Шеперд":
			petName = 1126154828;
			break;
		case "Вест-терьер":
			petName = 2910340283;
			break;
		case "Кошка":
			petName = 1462895032;
			break;
		case "Кролик":
			petName = 3753204865;
			break;
			
	}
	petathouse = mp.peds.new(petName, new mp.Vector3(petX, petY, petZ), petC, Dimension);
});

var checkTimer = setInterval(function () {
    var current = currentWeapon();
    if (localplayer.isInAnyVehicle(true)) {
        var vehicle = localplayer.vehicle;
        if (vehicle == null) return;

        if (vehicle.getClass() == 15) {
            if (vehicle.getPedInSeat(-1) == localplayer.handle || vehicle.getPedInSeat(0) == localplayer.handle) return;
        }
        else {
            if (canUseInCar.indexOf(current) == -1) return;
        }
    }

    if (currentWeapon() != givenWeapon) {
		ammosweap = 0;
        mp.game.invoke(getNative("GIVE_WEAPON_TO_PED"), localplayer.handle, givenWeapon, 1, false, true);
        mp.game.invoke(getNative("SET_PED_AMMO"), localplayer.handle, givenWeapon, 0);
        localplayer.taskReloadWeapon(false);
        localplayer.taskSwapWeapon(false);
        mp.gui.execute(`HUD.ammo=0;`);
    }
}, 100);
var canUseInCar = [
    453432689,
    1593441988,
    -1716589765,
    -1076751822,
    -771403250,
    137902532,
    -598887786,
    -1045183535,
    584646201,
    911657153,
    1198879012,
    324215364,
    -619010992,
    -1121678507,
];
mp.events.add('playerWeaponShot', (targetPosition, targetEntity) => {
	if (match) return;
    var current = currentWeapon();
    var ammo = mp.game.invoke(getNative("GET_AMMO_IN_PED_WEAPON"), localplayer.handle, current);
	
	
	
    mp.gui.execute(`HUD.ammo=${ammo};`);
	
	if (current != -1569615261 && current != 911657153) {
		if(ammosweap > 0) ammosweap--;
		if(ammosweap == 0 && ammo != 0) {
			mp.events.callRemote('takeoffWeapon', current, 0, 1);
			ammosweap = 0;
			mp.game.invoke(getNative("SET_PED_AMMO"), localplayer.handle, current, 0);
			mp.game.invoke(getNative("REMOVE_WEAPON_FROM_PED"), localplayer.handle, current);
			givenWeapon = -1569615261;
			mp.gui.execute(`HUD.ammo=0;`);
		}
	}
	
	if (ammo <= 0) {
		ammosweap = 0;
        localplayer.taskSwapWeapon(false);
        mp.gui.execute(`HUD.ammo=0;`);
    }
	
	if (to)
	{
        var ammo = mp.game.invoke(getNative("GET_AMMO_IN_PED_WEAPON"), localplayer.handle, current);
		if(ammo == ammosweap) mp.events.callRemote('playerTakeoffWeapon', current, ammo, 0);
		else mp.events.callRemote('playerTakeoffWeapon', current, ammosweap, 1);
		ammosweap = 0;
		mp.game.invoke(getNative("SET_PED_AMMO"), localplayer.handle, current, 0);
		mp.game.invoke(getNative("REMOVE_WEAPON_FROM_PED"), localplayer.handle, current);
		givenWeapon = -1569615261;
		mp.gui.execute(`HUD.ammo=0;`);
		to = false;
		mp.events.callRemote('takeoffWeapon', current, 0, 1);
	}
	
	
	
});

mp.events.add('render', () => {

        mp.game.controls.disableControlAction(2, 45, true); // reload control
        //localplayer.setCanSwitchWeapon(false);

        //     weapon switch controls       //
		mp.game.controls.disableControlAction(1, 243, true); // CCPanelDisable
		
        mp.game.controls.disableControlAction(2, 12, true);
        mp.game.controls.disableControlAction(2, 13, true);
        mp.game.controls.disableControlAction(2, 14, true);
        mp.game.controls.disableControlAction(2, 15, true);
        mp.game.controls.disableControlAction(2, 16, true);
        mp.game.controls.disableControlAction(2, 17, true);

        mp.game.controls.disableControlAction(2, 37, true);
        mp.game.controls.disableControlAction(2, 99, true);
        mp.game.controls.disableControlAction(2, 100, true);

        mp.game.controls.disableControlAction(2, 157, true);
        mp.game.controls.disableControlAction(2, 158, true);
        mp.game.controls.disableControlAction(2, 159, true);
        mp.game.controls.disableControlAction(2, 160, true);
        mp.game.controls.disableControlAction(2, 161, true);
        mp.game.controls.disableControlAction(2, 162, true);
        mp.game.controls.disableControlAction(2, 163, true);
        mp.game.controls.disableControlAction(2, 164, true);
        mp.game.controls.disableControlAction(2, 165, true);

        mp.game.controls.disableControlAction(2, 261, true);
        mp.game.controls.disableControlAction(2, 262, true);
        //      weapon switch controls       //

        if (currentWeapon() != -1569615261) { // heavy attack controls
            mp.game.controls.disableControlAction(2, 140, true);
            mp.game.controls.disableControlAction(2, 141, true);
            mp.game.controls.disableControlAction(2, 143, true);
            mp.game.controls.disableControlAction(2, 263, true);
        }

});

mp.events.add("Player_SetMood", (player, index) => {

        if (player !== undefined) {
            if (index == 0) player.clearFacialIdleAnimOverride();
			else mp.game.invoke('0xFFC24B988B938B38', player.handle, moods[index], 0);
        }

});

mp.events.add("Player_SetWalkStyle", (player, index) => {
	try{
        if (player !== undefined) {
            if (index == 0) player.resetMovementClipset(0.0);
			else player.setMovementClipset(walkstyles[index], 0.0);
			//mp.game.graphics.notify(`index ${index}`);
        }
	}
	catch (e) {}
});

mp.events.add("playerDeath", function (player, reason, killer) {
    givenWeapon = -1569615261;
});

mp.events.add("removeAllWeapons", function () {
    givenWeapon = -1569615261;
});


mp.events.add('svem', (pm, tm) => {
	var vehc = localplayer.vehicle;
	vehc.setEnginePowerMultiplier(pm);
	vehc.setEngineTorqueMultiplier(tm);
});

var f10rep = new Date().getTime();

mp.events.add('f10report', (report) => {
	if (!loggedin || new Date().getTime() - f10rep < 3000) return;
    f10rep = new Date().getTime();
	mp.events.callRemote('f10helpreport', report);
});

mp.events.add('dmgmodif', (multi) => {
	mp.game.ped.setAiWeaponDamageModifier(multi);
});

mp.game.ped.setAiWeaponDamageModifier(0.5);
mp.game.ped.setAiMeleeWeaponDamageModifier(0.4);

mp.game.player.setMeleeWeaponDefenseModifier(0.25);
mp.game.player.setWeaponDefenseModifier(1.3);

var resistStages = {
    0: 0.0,
    1: 0.05,
    2: 0.07,
    3: 0.1,
};
mp.events.add("setResistStage", function (stage) {
    mp.game.player.setMeleeWeaponDefenseModifier(0.25 + resistStages[stage]);
    mp.game.player.setWeaponDefenseModifier(1.3 + resistStages[stage]);
});

mp.events.add('render', () =>
{
	const controls = mp.game.controls;
	
	controls.enableControlAction(0, 23, true);
	controls.disableControlAction(0, 197, true);
	
	/*if(controls.isDisabledControlJustPressed(0, 197))
	{
		let position = mp.players.local.position;		
		let vehHandle = mp.game.vehicle.getClosestVehicle(position.x, position.y, position.z, 5, 0, 70);
		
		let vehicle = mp.vehicles.atHandle(vehHandle);
		
		if(vehicle
			&& vehicle.isAnySeatEmpty()
			&& vehicle.getSpeed() < 5)
		{
			mp.players.local.taskEnterVehicle(vehicle.handle, 5000, 0, 2, 1, 0);
		}
	}*/
});







