var licmenu = new Vue({
    el: ".LicMenu",
    data: {
        active: false,
    },
    methods: {
        show: function () {
            this.active = true;
        },
        hide: function () {
            this.active = false;
        },
		enter: function() {
			mp.trigger('licmenus', 0);
			this.exit();
		},
		exit: function() {
			mp.trigger('closelic');
			this.hide();
		},
    }
})