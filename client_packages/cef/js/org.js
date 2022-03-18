var tabletorg = new Vue({
    el: ".bgg",
    data: {
        active: false,
		access: -1,
		myuser: "",
		data: ["Ивушки", 10600, 30],
		members: ["Alan_Walker","Genifer_Jer"],
		cars: [["VSK2352RT", "mule4"],["TFS572GG", "pounder2"]],
    },
    methods: {
        show: function (datamembers, datacars, dataa, accessf) {
			this.members = datamembers;
			this.cars = datacars;
			this.data = dataa;
			this.access = accessf;
			this.myuser = "";
            this.active = true;
        },
        hide: function () {
            this.active = false;
        },
		fak: function() {
			mp.trigger("tabletinputfff", this.myuser);
		},
		callmembers: function(nick) {
			mp.trigger("tabletmembers", nick);
			this.exit();
		},
		callcars: function(number, inf) {
			mp.trigger("tabletcars", number, inf);
			this.exit();
		},
		exit: function() {
			mp.trigger("closetabletorg");
		},
		sell: function() {
			mp.trigger("sellorg");
		},
		
    }
})


