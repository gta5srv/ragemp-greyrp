var vehmanage = new Vue({
    el: ".remote",
    data: {
        active: false,
		namecar: "Sultan"
    },
    methods: {
		hide: function () {
			this.active = false;
        },
        show: function (name) {
			this.active = true;
			this.namecar = name;
        },
		interact: function(index) {
			mp.trigger('client::sendvehicle', index);
		}
    }
})