var Delivery = new Vue({
    el: ".delivery",
    data: {
    active: false,
	menu: 0,
    },
    methods:{
        open: function(id){
            this.menu = id;
        }
    }
});
var Delivery2 = new Vue({
    el: ".exit_btn",
    data: {
    active: false,
    menu: 0,}
})
function exit1() {
	mp.trigger('StartDelivery');
}

function exit() {
	mp.trigger('CloseDelivery');
}

function exit2() {
	mp.trigger('TakeDelivery');
}