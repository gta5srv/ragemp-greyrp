var gangzone = new Vue({
    el: ".bizWarInfoBlock",
    data: {
        active: false,
        att: 0,
        def: 0,
        min: 0,
        sec: 0,
    },
    methods: {
        show: function () {
            this.active = true;
        },
        hide: function () {
            this.active = false;
        },
    }
})