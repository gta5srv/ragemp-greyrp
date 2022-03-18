var biz = new Vue({
    el: "#bizmenu",
    data: {
        active: false,
		data: [  ]
    },
    methods: {
		open: function(json){
			this.active = true;
			this.data = json;
		},
		close: function(){
			this.active = false;
			mp.trigger('client::bizclose');
		},
		buy: function(){
			mp.trigger('client::bizbuy');
		},
    }
})