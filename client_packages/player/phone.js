let phone;
var phoneWindow = null;
var phoneOppened = false;
mp.events.add('initPhone', () => {
    phone = mp.browsers.new('package://cef/phone.html');
});
// // //
const player = mp.players.local;

mp.events.add('phoneShow', () => {
	try{
		phone.execute('show();');
		mp.gui.cursor.visible = true;
	}
	catch(e) {}
	
	/*mp.game.mobile.createMobilePhone(3);
	mp.game.mobile.setMobilePhoneScale(0);
	mp.game.mobile.scriptIsMovingMobilePhoneOffscreen(false);
	mp.game.mobile.setPhoneLean(false);*/

});



mp.events.add('phoneHide', () => {
    phone.execute('hide();');
    mp.gui.cursor.visible = false;
	
	
});

mp.events.add('phoneOpen', (data) => {
	try{
		var json = JSON.parse(data);
		// // //
		var id = json[0];
		var canHome = json[3];
		var canBack = json[2];
		var items = JSON.stringify(json[1]);
		// // //
		var exec = "open('" + id + "'," + canHome + "," + canBack + ",'"  + items + "');";
		
		
		
		phone.execute(exec);
	}
	catch(e) {}
	
});
mp.events.add('phoneChange', (ind, data) => {
    var exec = "change(" + ind + ",'" + data + "');";
    //mp.gui.chat.push(exec);
    phone.execute(exec);
});
mp.events.add('phoneClose', () => {
    if(phone != null) 
	{
		phone.execute('reset();');
		mp.game.invoke ('0x3BC861DF703E5097', mp.players.local.handle, true);

	}
});
mp.events.add('phoneChangeBg', (img) => {
	phone.execute(`changebackground("${img}");`);
});
// // //
mp.events.add('phoneCallback', (itemid, event, data) => {
    mp.events.callRemote('Phone', 'callback', itemid, event, data);
    //mp.gui.chat.push(itemid+":"+event+":"+data);
});
mp.events.add('phoneNavigation', (btn) => {
    mp.events.callRemote('Phone', 'navigation', btn);
});
// // //
/*mp.events.add("playerQuit", (player, exitType, reason) => {
    if (phone !== null) {
        if (player.name === localplayer.name) {
            phone.destroy();
            phone = null;
            phoneOppened = false;
            phoneWindow = null;
        }
    }
});*/