var petrol = new Vue({
    el: ".petrol",
    data: {
        active: false,
        bizInfoActive: false,
        bizSellPrice: 1,
		card: false,
		nall: false,
		fuel: 10,
		price: 2,
		select: -1,
        input: "",
		
    },
    methods: {
		buy: function(){
			if (this.card && this.nall && this.select != 2 || !this.nall && !this.card && this.select != 2 || this.select == -1 || this.input <= 0 && this.select == 0 ) return;
			
			if (this.select == 0)
				this.yes();
			else if (this.select == 1)
				this.full();
			else 
				this.gov();
		
		},
		sel: function(index) {
			if (index == this.select)
			{
				this.select = -1;
				return;
			}
			this.select = index;
		},
        gov: function () {
            mp.trigger('petrol.gov')
        },
		full: function () {
            mp.trigger('petrol.full', !this.nall)
        },
        yes: function () {
            mp.trigger('petrol', this.input, !this.nall)
        },
        no: function () {
            mp.trigger('closePetrol')
        },
        reset: function () {
            this.active = false;
            this.input = "";
			this.card = false;
			this.nal = false;
        }
    }
});


/*$(document).ready(function(){
    $("#showHideContent").click(function () {
        if ($("#content").is(":hidden")) {
            $("#content").show("slow");
        } else {
            $("#content").hide("slow");
        }
        return false;
    });
});*/
    