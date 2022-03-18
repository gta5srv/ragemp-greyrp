var death = new Vue({
    el: '.death_window',
    data: {
        active: false,
		buttonact: true,
		time: "",
        title: "Вы действительно хотите получить по бороде?",
    },
    methods: {
        yes: function () {
            mp.trigger('dialogCallbackMED',true)
			buttonact = false;
        },
        no: function () {
            mp.trigger('dialogCallbackMED',false)
			buttonact = false;
        }
    }
})