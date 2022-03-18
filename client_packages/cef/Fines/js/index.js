var FINE = new Vue({
    el: ".warning",
    data: {
        active: false,
        opacity: 0.0,
        fade: 0,

        interval: null,

        speed: 0,
        money: 0,
    },
    methods: {

        show: function(speed, money)
        {
            this.speed = speed;
            this.money = money;

            this.opacity = 0.0;
            this.active = true;
            this.reverse();

            setTimeout( () => {
                this.reverse();
            }, 4000);
        },

        reverse: function()
        {
            this.fade = this.fade == 0 ? 1 : 0;

            if (this.interval == null)
            this.interval = setInterval( () => {
                if ( (this.fade == 1 && this.opacity >= 1 ) || (this.fade == 0 && this.opacity <= 0))
                {
                    clearInterval(this.interval);
                    this.interval = null;
                    if (this.fade == 0)
                        this.active = false;
                    return;
                }
                this.opacity -= this.fade ? -0.01 : 0.01;
            }, 10);
        }

    }
});