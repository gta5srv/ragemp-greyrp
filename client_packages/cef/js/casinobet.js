var casino = new Vue({
    el: '.casino-bet',

    data: {
        active: false,
        minus: 100,
        minn: 100,
        maxx: 1000,
        plus: 100,
        val: 0,
        chips: 0,
        timeToStart: 12
    },

    methods: {
        show (data){
            var res = JSON.parse(data);

            this.minn = res[0];
            this.val = res[0];
            this.maxx = res[2];

            this.plus = res[0];
            this.minus = res[0];

            this.chips = res[3];

            this.active = true;
        },
        rest(){
            this.active = true;
        },
        hide (){
            this.active = false;
        },
        minusAct() {
            if (this.val - this.minus < this.minn) return;

            this.val -= this.minus;
        },

        plusAct() {
            if (this.val + this.plus > this.maxx) return;
            this.val += this.plus;
        },

        accept() {
            mp.trigger('casinoBet', this.val);
        },

        setChips(chips) {
            this.chips = chips;
        },

        setTimeToStart(time) {
            this.timeToStart = time;
        },

        close() {
             mp.trigger('exitRoultteUI');
        }
    }
});
