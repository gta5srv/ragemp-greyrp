window.addEventListener('DOMContentLoaded', () => {
    const audio = document.querySelectorAll('.audio');

    function music() {
        audio.forEach(e => {
            e.volume = 0.1;
            e.play();
        });
    }
    document.addEventListener('keydown', (e) => {
        if (e.code == 'Enter' || 'ArrowUp' || 'ArrowDown' || 'Tab') {
            music();
        }
    });

});

var casinoKeys = new Vue({
    el: '.main',

    data: {
        active: false,
        me: 0,
        diller: 0,
        currBet: 0,
        minBet: 0,
        min: 0,
        sec: 0,
        start: false
    },

    methods: {
        setChips(me, diller) {
            this.me = me;
            this.diller = diller;
        },
        setBet(curr, min) {
            this.currBet = curr;
            this.minBet = min;
        },
        setTime(min, sec) {
            this.min = min;
            this.sec = sec;
        },
        toggleStart(toggle) {
            this.sec = 0;
            this.start = toggle;
        },
        show() {
            this.me = 0;
            this.diller = 0;
            this.active = true;
        },
        hide() {
            this.me = 0;
            this.diller = 0;
            this.active = false;
        }
    }
});

var calls = new Vue({
    el: '.main-calls',

    data: {
        active: false,
        calls: [[1, '1', '2', '3', '4']]
    },

    methods: {
        init(data) {
            this.active = true;
            this.calls = JSON.parse(data);
        },
        call(id) {
            mp.trigger('acceptCall', id);
        },
        close() {
           this.active = false;
           mp.trigger('closeCalls');
            //this.active = false;
        }
    }
});