var tablet = new Vue({
    el: '.bgg',
    data: {
        active: false,
		text: ["Наличные","Банк", "Уровень", "EXP", "Телефон", "Статус", "Баны", "Варны", "Лицензии", "Номер паспорта", "Номер счета", "Фракция", "Ранг", "Сытость", "Жажда", "Супруг(а)", "Работа", "Уровень работы","Передать/Переложить","Отыграно"],
		stats: [1, 2, "88005553535", "Admin", 0, 0, "Мото транспорт,Легковой транспорт,Грузовой транспорт,Водный транспорт,Лицензия на вертолёт,Лицензия на самолёт,Лицензия на оружие,Медицинская карта,Лицензия парамедика,Военный билет,Лицензия Адвоката", "987654321$", "", 9999999, 9999999, 100, 100],
		orders: [ ["Pavel_Derentino", "Online", 533522, 27, 3000], ["Max_Tifan", "Offline", 864343, 123, 5000, 1]],
		forbes: [ ["Pavel_Derentino", 1000000], ["Max_Tifan", 55333]],
		quests: [ ["Отыграть 3 часа", "1/3"], ["Купить машину", "ВЫПОЛНЕНО"], ["Собрать 200 листьев коки", "66/200"], ["Купить ECOLA", "0/1"]],
		access: 2,
		donate: 100,
		money: 1000,
		bank: 10000,
		balance: 0,
        menu: 0,
		bizid: "",
		bizmats: "",
        totrans: null,
        aftertrans: null,
		bizlist: [ ],
        fname: null,
        lname: null,
		nick: "Ivan_Ivanov",
		chat: true,
		hud: true,
		players: true,
		hudhelp: true,
		pos: true,
		veh: true,
		eat: true,
		muteplayers: "",
		myselect: 0,
		promocode: "",
		cars: [ "Sultan (R423FF)", "Nero (Y534TS)"]
		
	},
	methods: {	
		sel: function (index) {
			this.myselect = index;
		},
		setError: function(error){
			 document.querySelector('.market__error-field').textContent = error;
		},
		getNumber: function(){
			const numbersInput = document.querySelectorAll('.market__num-place .market__num-in');
			  const arrayOfResult = [];
			  for (let i = 0; i < numbersInput.length; i++) {
				arrayOfResult.push(numbersInput[i].value);
			  }
			  return arrayOfResult;
		},
		checkOnValidate: function (array) {
			const arrayOfValidateQuery = [
			  /^[a-zA-Z]$/, //буква
			  /^[a-zA-Z]$/,
			  /^[a-zA-Z]$/,
			  /^[a-zA-Z]$/,
			  /^[a-zA-Z]$/,
			  /^[a-zA-Z]$/,
			  /^[a-zA-Z]$/
			];
			for (let i = 0; i < array.length; i++) {
			if (!array[i].match(arrayOfValidateQuery[i])) {
			  this.setError('Формат номера только буквы в количестве 7 символов, AAAAAAA');
			  return false;
			}
		  }
		  return true;
		},
		change: function () {
			const arrayOfData = this.getNumber();
			  if (this.checkOnValidate(arrayOfData)) {
				const resultString = arrayOfData.join('');
				mp.trigger('buyLicensePlate', this.myselect, resultString);
			  }
		},
        close: function(){
            //this.active = false
            this.balance = 0;
            this.menu = 0;
            this.totrans = null;
            this.aftertrans = null;
			this.fname = null;
			this.lname = null;
        },
        onInputTrans: function(){
            if(!this.check(this.totrans)){
                this.totrans = null;
                this.aftertrans = null;
            } else {
				if(Number(this.totrans) < 0) this.totrans = 0;
                this.aftertrans = Number(this.totrans) * 100000;
            }
        },
        onInputName: function(){
            if(this.check(this.fname) || this.check(this.lname)){
                this.fname = null;
                this.lname = null;
            }
        },
		onInputPromo: function(){
            if(this.check(this.promocode)){
				this.promocode = null
            }
        },	
        check: function(str) {
            return (/[^a-zA-Z]/g.test(str));
        },
		select: function(owner) {
			this.active = false;
			mp.trigger("sendowner", owner);
		},
		cancel: function() {
			this.active = false;
			if (this.access == 3)
			{
				mp.trigger("sendorder");
			}
			else
			{
				mp.trigger("sendcancel");
			}
			
		},
		inputfor: function() {
			mp.trigger("sendinputs", this.bizid, this.bizmats);
			this.active = false;
			
		},
		setorders: function(orde) {
			this.orders = orde;
		},

		setacceess: function(acc) {
			this.access = acc;
		},
		
		setquests: function(acc) {
			this.quests = acc;
		},
		
		setforbes: function(forb, carg, bizs) {
			this.forbes = forb;
			this.cars = carg;
			this.bizlist = bizs;
		},
		sellbiz: function(index) {
			mp.trigger('sendbiz', index);
		},
        back: function(){
            this.menu = 4;
        },
        open: function(id){
            this.menu = id;
        },
		
		showhud: function(){
			this.hud = !this.hud;
			mp.trigger('showHUDTOGGLE');
		},
		showchat: function(){
			this.chat = !this.chat;
			mp.trigger('showCHAT');
		},
		showplayers: function(){
			this.players = !this.players;
			mp.trigger('showPLAYERS');
		},
		showhudhelp: function(){
			this.hudhelp = !this.hudhelp;
			mp.trigger('showHUDHELP');
		},
		showveh: function(){
			this.veh = !this.veh;
			mp.trigger('showVEH');
		},
		showpos: function(){
			this.pos = !this.pos;
			mp.trigger('showPOS');
		},
		showeat: function(){
			this.eat = !this.eat;
			mp.trigger('showEAT');
		},
		updatevoice: function(){
			mp.trigger('updateVOICE');
		},
		updatemute: function(){
			mp.trigger('updateMUTE', this.muteplayers);
		},
        buy: function(id){
            let data = null;
            switch(id){
                case 1:
                data = this.fname+"_"+this.lname;
                break;
                case 2:
                data = this.totrans;
                break;
                default:
                break;
            }
            mp.trigger("donbuy", id, data);
        },
		promo: function(id){
			mp.trigger('donbuy', id, this.promocode)
		},
		show: function(stars){
			this.balance = stars,
			//this.active = true;
			mp.trigger("updatedonate");
		}
		
		
    }
});

const inputs = document.querySelectorAll(".market__num-in");
let valIn = "";

for (let i = 0; i < inputs.length; i++) {
    valIn = valIn + inputs[i].value;
}