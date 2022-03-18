var fmenu = new Vue({
    el: ".fmenu",
    data: {
        active: false,
        submenu: false,
        members: [],
        menu: 0,
        input: "",
        rank: "",
		name: "",
        btntext: ["", "", "Принять", "Выгнать", "Изменить"],
        header: ["", "", "Принять в организацию", "Выгнать из организации", "Изменить ранг", "","","Редактирование рангов"],
		ranks: ["Рядовой", "Прапорщик", "Сержант", "Ефрейтор", "Лейтенант"],
		newranks: ["Рядовой", "Прапорщик", "Сержант", "Ефрейтор", "Лейтенант"],
        btnactive: [false, false, false, false, false, false, false, false, false, false],
		
        oncounter: 0,
        ofcounter: 0,
        counter: 0,
		logs: []
    },
    methods: {
        set: function (json, count, on, off) {
            this.members = JSON.parse(json);
            this.oncounter = on;
            this.ofcounter = off;
            this.counter = count;
        },
		setranks: function(json){
			this.ranks = json;
			this.newranks = json;
		},
		save: function(){
			mp.trigger("fmenu", 5, JSON.stringify(this.newranks), false);
		},
		setlogs: function(logs) {
			this.logs = logs;
		},
        btn: function (id, event) {
            console.log(id)
            var ind = this.btnactive.indexOf(true);
            if (ind > -1) this.btnactive[ind] = false;
            if (id == 0) {
                this.reset();
                this.active = false;
                mp.trigger('closefm');
                return;
            } else {
                this.submenu = true;
                this.menu = id;
                this.btnactive[id] = true;
                console.log(this.menu)
            }
        },
		yvol: function (owner)
		{
			this.reset();
                this.active = false;
                mp.trigger('closefm');
				mp.trigger("fmenu", 3, owner, this.rank);
		},
        submit: function () {
            //console.log('submit:' + this.menu + ':' + this.input + ':' + this.rank);
            mp.trigger("fmenu", this.menu, this.input, this.rank);
            this.active = false;
            this.reset();
        },
        reset: function () {
            this.btnactive = [false, false, false, false, false];
            this.submenu = false;
            this.members = [];
            this.input = "";
            this.rank = "";
            this.menu = 0;
        }
    }
})