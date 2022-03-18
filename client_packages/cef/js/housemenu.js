var housemenu = new Vue({
    el: ".HouseMenu",
    data: {
        active: false,
		text: ["101", "Комфорт", "5", "3", "100000", "Государство", "Открыты"],
		style: -1,
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
})

$('.HouseMenu #check').on('click', function(){
    mp.trigger('housemanage', 0);
});
$('.HouseMenu #buy').on('click', function(){
    mp.trigger('housemanage', 1);
});
$('.HouseMenu #enter').on('click', function(){
    mp.trigger('housemanage', 2);
});
$('.HouseMenu #enter').on('click', function(){
    mp.trigger('garageHouse', 3);
});
$('.HouseMenu #exit').on('click', function(){
	mp.trigger('closehouse');
    housemenu.hide();
})