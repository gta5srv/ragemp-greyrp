var wrapper = new Vue({
    el: ".wrapper",
    data:{
        active: false,
        kills:[]
    },
    methods:{
        addKills: function(object){
            this.kills = JSON.parse(object)
        }
    }
});
function cheker(){
    if(wrapper.kills.length > 5){
        wrapper.kills.shift()
    }
}
setInterval(() => {
    cheker()
}, 1);
setInterval(() => {
    wrapper.kills.pop()
}, 10000);