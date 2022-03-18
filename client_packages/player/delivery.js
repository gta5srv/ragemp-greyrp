global.Delivery = mp.browsers.new('package://cef/Delivery/index.html');
mp.events.add('OpenDelivery', () => {
	if (global.menuCheck()) return;
    menuOpen();
	Delivery.execute('Delivery.active=1');
});
mp.events.add('CloseDelivery', () => {
	Delivery.execute('Delivery.active=0');
    mp.gui.cursor.visible = false;
	global.menuClose();
});
mp.events.add('StartDelivery', () => {
	Delivery.execute('Delivery.active=0');
	global.menuClose();
	mp.events.callRemote("StartDeliverys");
});
mp.events.add('TakeDelivery', () => {
	Delivery.execute('Delivery.active=0');
	global.menuClose();
	mp.events.callRemote("TakeDeliverys");
});