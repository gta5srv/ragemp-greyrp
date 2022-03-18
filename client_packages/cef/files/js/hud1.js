var HUD = new Vue({
    el: ".hud_wrapper",
    data: {
        show: false,
        ammo: 0,
        money: 117000000,
        bank: 0,
        mic: false,
        time: "00:00:00",
        date: "00.00.00",
        street: "3oxaan",
        crossingRoad: "Groups",
        playerId : 0,
        eat: 75,
        water: 50,
        online: 0,
        inVeh: false,
        engine: false,
        doors: false,
        speed: 0,
        hp: 0,
        fuel: 10,
        maxfuel: 300,
    },
    methods: {
        getSpeed(){
            let num = 605 + (this.speed * 100 / 300) * 7;
            return num > 1205 ? 1205 : num;
        },
    }
});

setInterval( () => {
    let date = new Date();
    
    HUD.time = `${date.getHours() < 10 ? '0' + date.getHours() : date.getHours()}:${date.getMinutes() < 10 ? '0' + date.getMinutes() : date.getMinutes()}:${date.getSeconds() < 10 ? '0' + date.getSeconds() : date.getSeconds() }`;
    let str = date.getFullYear();
    let mou = date.getMonth() + 1;
    let day = date.getDate();
    HUD.date = `${day < 10 ? '0' + day : day}.${mou < 10 ? '0' + mou : mou}.${str.toString().replace('20', '')}`;
}, 1000);


var lastW = 0;
var lastR = 0;

function updatehud(width, ratio,safezone,offset=0) {

    lastW = width; lastR = ratio;

   let y1 = 316, y2 = 436;

    if(width > 2100) {
        offset += 98;
        document.querySelector(".lefts").style['bottom'] = '1px';
    }

    if(width < 1440) {
        offset -= 92;
        y2 = 404;

        document.querySelector(".lefts").style.bottom = '13px';
        document.querySelector(".lefts").style.transform = 'scale(0.75)';
    }

    if(width < 1320) {
        offset -= 38;
        document.querySelector(".lefts").style.bottom = '13px';
        document.querySelector(".lefts").style.transform = 'scale(0.75)';
    }

    const m = (y2 - y1) / (5/4 - 16/9);
    const b = y1 - (m * 16 / 9);

    document.querySelector(".lefts").style.left = `${m * ratio + b + offset}px`;
}
