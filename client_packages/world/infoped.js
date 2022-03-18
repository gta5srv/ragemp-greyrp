global.infoped = mp.browsers.new('package://cef/infoped.html');

let ped = mp.peds.new(mp.game.joaat('u_m_y_juggernaut_01'), new mp.Vector3(-1042.4424, -2738.2603, 20.3), 270.0,);

mp.events.add('openInfoMenu', () => {
	if (global.menuCheck()) return;
    menuOpen();
	infoped.execute('infoped.active=1');
	mp.events.call('toBlur', 200)
});

mp.events.add('CloseInfoMenu', () => {
	infoped.execute('infoped.active=0');
    mp.gui.cursor.visible = false;
	global.menuClose();
	mp.events.call('fromBlur', 200)
});
