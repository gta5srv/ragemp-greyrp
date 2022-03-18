var kpz = new Vue({
    el: ".kpz",
    data: {
        active: false,
		title: "Гражданин Ivan_Ivanov",
		input: "",
        time: "",
		
    },
    methods: {
		close: function (){
			this.active = false;
			mp.trigger('closekpz');
		},
		yes: function() {
			this.close();
			mp.trigger('sendkpz', this.input, this.time);
		},
		no: function() {
			this.close();
		},
		open: function(text) {
			this.active = true;
			this.title = text;
		},
    }
});
    