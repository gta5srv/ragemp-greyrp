
var presskeyto = new Vue({
    el: '.press-key-to',

    data: {
        active: false,
        items: ['E', 'текст']
    },

    methods: {
        open(items) {
            this.items = JSON.parse(items);
            this.active = true;
        },
        close() {
            this.active = false;
            this.items = ['E', 'текст'];
        }
    }
});