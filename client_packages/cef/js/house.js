var HOUSE = new Vue({
    el: ".house-wrapper",
    data: {
        active: false,
		text: ["101", "Комфорт", "5", "3", "100000", "Государство", "Открыты"],
		style: -1,
		styles: {
			"Трейлер": 1,
			"Эконом": 2,
			"Эконом+": 2,
			"Комфорт": 3,
			"Комфорт+": 3,
			"Премиум": 4,
			"Премиум+": 4,
			"Элитный": 5
		}
    },
    methods: {
        show: function (arrays, st) {
			this.text = arrays;
			this.style = st;
            this.active = true;
        },
        hide: function () {
            this.active = false;
        },
		check: function() {
			mp.trigger('housemanage', 0);
			this.exit();
		},
		buy: function() {
			mp.trigger('housemanage', 1);
			this.exit();
		},
		enter: function() {
			mp.trigger('housemanage', 2);
			this.exit();
		},
		garageEnter: function(){
            mp.trigger('housemanage', 3);
			this.exit();
        },
		exit: function() {
			mp.trigger('closehouse');
			this.hide();
		},
    }
});