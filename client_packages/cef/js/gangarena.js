
var hudis = new Vue({
	el: ".timers",
    data: {
		hud: false,
		kills: 0,
		time: "0:00",
		deaths: 0
    }
})
var gangarena = new Vue({
    el: ".gangs",
    data: {
        active: false,
		adm: false,
		page: 0,
		inlobby: false,
		weapon: 0,
		lobby: [],
		lobbies: [ ],
		players: [ ],
		winners: [ ]
    },
    methods: {
		hide: function () {

				mp.trigger('client::closemenu');
        },
        hides: function () {
            this.active = false;
			this.page = 0;
			if (this.inlobby)
			{
				this.adm = false;
				mp.trigger('client::disconnectlobby');
			}
        },
		hidesno: function () {
            this.active = false;
			this.page = 0;
        },
		show: function() {
			this.active = true;
			this.page = 0;
			this.inlobby = false;
			this.adm = false;
			this.weapon = 0;
			this.lobby = [];
		},
		gopage: function(index) {
			this.page = index;
			if (index == 2)
			{
				mp.trigger("client::getlobbylist");
			}
		},
		set: function(index) {
			this.weapon = index;
		},
		sendlobby: function() {
			mp.trigger('client::sendlobby', JSON.stringify([this.lobby[0],this.lobby[1],this.lobby[2],this.weapon]));
		},
		kick: function(nick){
			mp.trigger('client::kickplayer', nick);
		},
		connectlobby: function(index) {
			mp.trigger('client::connectlobby', index);
		},
		start: function() {
			mp.trigger('client::startmatch', );
		},
		
		
		// Called server
		sendwinners: function(listwinners) {
			this.inlobby = false;
			this.active = true;
			this.page = 3;
			this.winners = listwinners;
		},
		refreshlobby: function(listplayers) {
			this.players = listplayers;
			this.lobby[0] = this.players.length;
		},
		setlobbylist: function(listlobby){
			this.lobbies = listlobby;
		},
		createlobby: function(listplayers, lobbyinfo){
			this.page = 4;
			this.players = listplayers;
			this.adm = true;
			this.inlobby = true;
			this.lobby = lobbyinfo;
		},
		setlobby: function(listplayers, lobbyinfo) {
			this.page = 4;
			this.players = listplayers;
			this.inlobby = true;
			this.lobby = lobbyinfo;
		},
    }
})