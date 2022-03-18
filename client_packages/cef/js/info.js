var infosss = new Vue({
    el: ".infos",
    data: {
        active: false,
		answermy: "text"
    },
    methods: {
		hide: function () {
			mp.trigger('client::closeinfo');
        },
		hides: function () {
			this.active = false;
        },
        show: function (data) {
			this.active = true;
			this.answermy = data;
        },
		answer: function() {
			mp.trigger('client::acceptPressed');
		}
    }
})