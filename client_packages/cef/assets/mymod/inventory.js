var itemsData = {
    "-1": "Маска",
    "-3": "Перчатки",
    "-4": "Штаны",
    "-5": "Рюкзак",
    "-6": "Обувь",
    "-7": "Аксессуар",
    "-8": "Нижняя одежда",
    "-9": "Бронежилет",
    "-11": "Верхняя одежда",
    "-12": "Головной убор",
    "-13": "Очки",
    "-14": "Аксессуар",
    0: "Test Item",
    1: "Аптечка",
    2: "Канистра",
    3: "Чипсы",
    4: "Пиво",
    5: "Пицца",
    6: "Бургер",
    7: "Хот-Дог",
    8: "Сэндвич",
    9: "eCola",
    10: "Sprunk",
    11: "Отмычка",
    12: "Сумка с деньгами",
    13: "Материалы",
    14: "Наркотики",
    15: "Сумка с дрелью",
    16: "Военная отмычка",
    17: "Мешок",
    18: "Стяжки",
    19: "Ключи от машины",
    40: "Подарок",
    41: "Связка ключей",

    20: `"На корке лимона"`,
    21: `"На бруснике"`,
    22: `"Русский стандарт"`,
    23: `"Asahi"`,
    24: `"Midori"`,
    25: `"Yamazaki"`,
    26: `"Martini Asti"`,
    27: `"Sambuca"`,
    28: `"Campari"`,
    29: `"Дживан"`,
    30: `"Арарат"`,
    31: `"Noyan Tapan"`,

    100: "Pistol",
    101: "Combat Pistol",
    102: "Pistol .50",
    103: "SNS Pistol",
    104: "Heavy Pistol",
    105: "Vintage Pistol",
    106: "Marksman Pistol",
    107: "Revolver",
    108: "AP Pistol",
    109: "Stun Gun",
    110: "Flare Gun",
    111: "Double Action",
    112: "Pistol Mk2",
    113: "SNSPistol Mk2",
    114: "Revolver Mk2",

    115: "Micro SMG",
    116: "Machine Pistol",
    117: "SMG",
    118: "Assault SMG",
    119: "Combat PDW",
    120: "MG",
    121: "Combat MG",
    122: "Gusenberg",
    123: "Mini SMG",
    124: "SMG Mk2",
    125: "Combat MG Mk2",

    126: "Assault Rifle",
    127: "Carbine Rifle",
    128: "Advanced Rifle",
    129: "Special Carbine",
    130: "Bullpup Rifle",
    131: "Compact Rifle",
    132: "Assault Rifle Mk2",
    133: "Carbine Rifle Mk2",
    134: "Special Carbine Mk2",
    135: "Bullpup Rifle Mk2",

    136: "Sniper Rifle",
    137: "Heavy Sniper",
    138: "Marksman Rifle",
    139: "Heavy Sniper Mk2",
    140: "Marksman Rifle Mk2",

    141: "Pump Shotgun",
    142: "SawnOff Shotgun",
    143: "Bullpup Shotgun",
    144: "Assault Shotgun",
    145: "Musket",
    146: "Heavy Shotgun",
    147: "Double Barrel Shotgun",
    148: "Sweeper Shotgun",
    149: "Pump Shotgun Mk2",

    180: "Нож",
    181: "Дубинка",
    182: "Молоток",
    183: "Бита",
    184: "Лом",
    185: "Гольф клюшка",
    186: "Бутылка",
    187: "Кинжал",
    188: "Топор",
    189: "Кастет",
    190: "Мачете",
    191: "Фонарик",
    192: "Швейцарский нож",
    193: "Кий",
    194: "Ключ",
    195: "Боевой топор",

    200: "Пистолетный калибр",
    201: "Малый калибр",
    202: "Автоматный калибр",
    203: "Снайперский калибр",
    204: "Дробь",
}


var infos = {
	1: "Информация какая-то типа",
	114: "Ревальвер крутой кста"


};

Vue.component('item', {
    template: '<div class="item draggable" :class="items" :id="ids" :data-type="typ" :data-active="ativ" @click.right.prevent="select">\
    <img :src="src"><span>{{count}} Шт</span></div>',
    props: ['id', 'index', 'count', 'isactive', 'type', 'subdata'],
    data: function () {
        return {
            src: './assets/images/icons/items/' + this.id + '.png',
            name: itemsData[this.id],
            ids: this.index,
            typ: this.type,
            ativ: (this.isactive == 1) ? `active` : 0,
            items: `i${this.id}a${this.isactive}`,
        }
    },
    methods: {
        select: function (event) {
            if(board.sType == (this.type == 'inv') ? 1 : 0){
                if(board.sIndex == this.index){
                    context.hide()
                }else{
                    board.sType = (this.type == 'inv') ? 1 : 0;
                    board.sID = this.id;
                    board.sIndex = this.index;
                    context.type = (this.type == 'inv') ? 1 : 0;
					
					let text = ""
					text += infos[this.id] == undefined ? "Нет информации" : infos[this.id];
					if (this.subdata)
						text += " [" + this.subdata + "]"
					
                    board.context(event, itemsData[this.id], text)
                }
            }else{
                board.sType = (this.type == 'inv') ? 1 : 0;
                board.sID = this.id;
                board.sIndex = this.index;
                context.type = (this.type == 'inv') ? 1 : 0;
				
				let text = ""
				text += infos[this.id] == undefined ? "Нет информации" : infos[this.id];
				if (this.subdata)
					text += " [" + this.subdata + "]"
				
                board.context(event, itemsData[this.id], text)
            }
            // console.log(`====================`)
            // console.log(`index ${this.index}`)
            // console.log(`contype ${context.type}`)
            // console.log(`type ${this.type}`)
            // console.log(`====================`)
        }
    }
})
var board = new Vue({
    el: ".inv",
    data: {
        active: true,
        outside: false,
        outType: 0,
        outHead: "Багажник",
        stats: [1, 2, "88005553535", "Admin", 0, 0, "123456789$", "987654321$", "", 9999999, 9999999],
        items: [[-1, 5, 1],[-3, 5, 1],[-4, 5, 1],[-5, 5, 1],[-6, 5, 1],[-7, 5, 1],[-8, 5, 1],[-9, 5, 1],[-11, 5, 1],[-12, 5, 1],[-13, 5, 1],[-14, 5, 1],[114, 5, 1, "ssat"], [5, 10, 0], [10, 500, 0], [11, 100, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0], [5, 10, 0], [10, 500, 0], [11, 100, 0]],
        outitems: [[1, 5], [5, 10], [10, 500]],
        sIndex: 0,
        sType: 0,
        sID: 0,
        key: 0,
    },
    methods: {
        context: function (event, name, sub) {
            if (clickInsideElement(event, 'item')) {
                context.show(event.pageX, event.pageY, name, sub)
            } else {
                context.hide()
            }
        },
        hide: function (event) {
            context.hide()
        },
        outSet: function (json) {
            this.key++
            this.outType = json[0]
            this.outHead = json[1]
            this.outitems = json[2]
        },
        itemsSet: function (json) {
            this.key++
            this.items = json
        },
        itemUpd: function (index, data) {
            this.key++
            this.items[index] = data
        },
        send: function (id) {
            // console.log(`Действие ${id}`)
            let type = (this.sType) ? 0 : this.outType
            mp.trigger('boardCB', id, type, this.sIndex)
        },
        shieldact: function (id, type, index) {
            // console.log(`Действие ${id} Вещь ${index}`)
            // console.log(`====================`)
            mp.trigger('boardCB', id, type, index)
        }
    }
})
var context = new Vue({
    el: ".info",
    data: {
        active: false,
        style: '',
        type: true,
        name: "Ключ",
        subtext: "Серийник",
    },
    methods: {
        show: function (x, y, name, sub) {
            this.style = `left:${x+25}px;top:${y}px;`
            this.name = name;
            this.subtext = sub;
            this.active = true
        },
        hide: function () {
            this.active = false;
            this.name = "";
            this.subtext = "";
            board.sType = null;
            board.sID = null;
            board.sIndex = null
        },
        btn: function (id) {
            board.send(id)
            this.hide()
        }
    }
})
function clickInsideElement(e, className) {
    var el = e.srcElement || e.target;
    if (el.classList.contains(className)) {
        return el;
    } else {
        while (el = el.parentNode) {
            if (el.classList && el.classList.contains(className)) {
                return el;
            }
        }
    }
    return false;
}

DragManager.onDragCancel = function(dragObject) {
    dragObject.avatar.rollback();
    //console.log(`sss`)
};

DragManager.onDragEnd = function(dragObject, dropElem, action) {
        // console.log(`Заканчиваю действие`)
        var sindex = $(dragObject.elem).attr('id')
        var temptype = $(dragObject.elem).data('type')
        var type;
        if(temptype == 'inv' || temptype == 'eq'){
            type = 0
        }else{
            switch(board.outHead) {
                case "Багажник":
                    type = 2;
                break;
                case "Шкаф с предметами":
                    type = 3;
                break;
                case "Шкаф с одеждой":
                    type = 4;
                break;
                case "Склад оружия":
                    type = 6;
                break;
                case "Связка ключей":
                    type = 7;
                break;
                case "Оружейный сейф":
                    type = 8;
                break;
                case "Обмен":
                    type = 8;
                break;
            }
        }
    //   console.log(`=========`)
    //   console.log(`====================`)
    //   console.log(`index ${sindex}`)
    //   console.log(`temptype ${temptype}`)
    //   console.log(`type ${type}`)
    //   console.log(`Действие ${action}`)
    //   console.log(`====================`)
  //dragObject.avatar.rollback();  
        board.shieldact(action, type, sindex)    
        dragObject.avatar.rollback();  
//   console.log($(dragObject.elem).data("name"))
//   console.log($(dropElem).data("action"))
//   dragObject.elem.style.display = 'none';
//   console.log(action)
//   console.log(`Выкинул`)
//  var act = $(dropElem).data("action")
//   if(!act) return;
};