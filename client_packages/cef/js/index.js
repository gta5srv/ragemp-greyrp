var petrol = new Vue({
    el: ".petrol",
    data: {
        active: false,
        price: 10,
        input: "",
        inputelectro: 0,
		street: "Вайнвуд Хиллз",
		fuel: 100,
		style: 0,
    },
    methods: {
		gostyle: function(index) {
			this.style = index
		},
        gov: function () {
            //console.log('full')
            mp.trigger('petrol.gov')
        },
		full: function () {
            //console.log('full')
            mp.trigger('petrol.full')
        },
        yes: function () {
            //console.log('yes')
            mp.trigger('petrol', this.input)
        },
        no: function () {
            //console.log('no')
            mp.trigger('closePetrol')
        },
        reset: function () {
            this.active = false
            this.input = ""
        }
    }
});