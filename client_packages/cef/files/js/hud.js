var HUD = new Vue({
    el: ".hud_wrapper",
    data: {
        show: true,
		showhelp: true,
		active: false,
		mic: false,
		id: 228,
        helpActive: true,
        bank: 2500,
        money: 231312,
        date: "22.10.2020",
        time: "22:10",
        online: 100,
        maxOnline: 1000,
		crossingRoad: "Вайнвуд",
		street: "Бульвар кольтров",
		vehs: true,
		poss: true,
		eats: true,
		biz: 'f',
        fps: 160,
		static: 1488,
        ammo: 0,
        green: false,
		// speedInfo
			isVeh: true,
            speed: 200,
            fuel: 10,
            maxfuel: 120,
            hp: 0,
            maxhp: 1000,
            isEngine: false,
            isDoors: false,
            isBelt: false,
            water: 40,
            hunger: 29
        
		
    },
    methods: {
        getSpeed(){
            let num = 600 + (this.speed * 100 / 300) * 7;
            return num > 1155 ? 1155 : num;
        },
        showNotify(title, status2, text2) {
            
            $('.notify_list').append(`
            <div class="notify ${status2} animate__animated animate__fadeInUp">
                <div class="line"></div>
                <img src="./images/player_hud/noty_${status2}.png" alt="" class="icon">
                <div class="content">
                    <div class="title"></div>
                    <div class="text">${text2}</div>
                </div>
            </div>`);
				var notify = $(' .notify_list .notify:last');
				setInterval(function () {
					notify.removeClass('animate__fadeInUp');

					notify.addClass('animate__fadeOutUp');
					setInterval(function () {
						notify.remove();
					}, 600)

				}, 6000);
        },
        
    }
});


var lastW = 0;
var lastR = 0;

function updatehud(width, ratio,safezone,offset=0) {

    lastW = width; lastR = ratio;

   let y1 = 316, y2 = 436;

    if(width > 2100) {
        offset += 98;
        
        document.querySelector(".mappings_block").style['bottom'] = '1px';
    }

    if(width < 1440) {
        offset -= 92;
        y2 = 404;

        document.querySelector(".mappings_block").style.bottom = '13px';
        document.querySelector(".mappings_block").style.transform = 'scale(0.75)';
    }

    if(width < 1320) {
        offset -= 38;
        document.querySelector(".mappings_block").style.bottom = '13px';
        document.querySelector(".mappings_block").style.transform = 'scale(0.75)';
    }

    const m = (y2 - y1) / (5/4 - 16/9);
    const b = y1 - (m * 16 / 9);

    document.querySelector(".mappings_block").style.left = `${m * ratio + b + offset}px`;
    
}

$(document).ready(() => {
    window.mapAPI = {
        on: () => {
            updatehud(lastW,lastR,false,lastW * 0.08);
        },
        off: () => {
            updatehud(lastW,lastR,false,0);
        },
    };
});
