var aparts = new Vue({
    el: ".main",
    data: {
        active: false,
		apartlist: [ [ 1, "Государство", "50000$", 3, "0/3" ] ]
    },
    methods: {
		hide: function () {
			mp.trigger('client::closeapart');
        },
		hides: function () {
			this.active = false;
			this.apartlist = []; // optimize
        },
        show: function (data) {
			this.active = true;
			this.apartlist = data;
        },
		interact: function(index) {
			mp.trigger('client::sendapart', index);
		}
    }
})