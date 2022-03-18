var data = {
    "test":["green1","green12","green13","green14",],
    "father":["Бенджамин", "Даниэль", "Джошуа", "Ной", "Эндрю", "Джон", "Алекс", "Исаак", "Эван", "Винсент", "Ангел", "Диего", "Адриан", "Габриэль"," Майкл"," Сантьяго"," Кевин"," Сэмюэл"," Энтони"," Клод"," Нико"," Джон"],    "mother":["Ханна", "Обри", "Жасмин", "Жизель", "Амелия", "Изабелла", "Зои", "Ава", "Камила", "Вайолет"," София"," Эвелин"," Николь"," Эшли"," Грейси"," Брианна"," Натали"," Оливия"," Элизабет"," Шарлотта"," Эмма", " Мисти"],
    "eyebrowsM":["Нет", "Сбалансированный", "Модный", "Клеопатра", "Насмешливый", "Женщина", "Соблазнительный", "Ущипнутый", "Чола", "Триумф", "Беззаботный", "Пышный", "Грызун", "Двойной трамвай", "Тонкий", "Карандашный", "Мать-Щипач", "Прямой и узкий", "Естественный", "Пушистый", "Неухоженный", "Гусеница", "Обычный", "Средиземноморский", "Ухоженный", "Бушели", "Пернатый", "Колючий", "Однобровый", "Крылатый", "Тройной трамвай", "Арочный трамвай", "Вырезы", "Исчезают", "Сольный трамвай"],"eyebrowsF":["Нет", "Сбалансированный", "Модный", "Клеопатра", "Насмешливый", "Женщина", "Соблазнительный", "Ущипнутый", "Чола", "Триумф", "Беззаботный", "Пышный", "Грызун", "Двойной трамвай", "Тонкий", "Карандашный", "Мать-Щипач", "Прямой и узкий", "Естественный", "Пушистый", "Неухоженный", "Гусеница", "Обычный", "Средиземноморский", "Ухоженный", "Бушели", "Пернатый", "Колючий", "Однобровый", "Крылатый", "Тройной трамвай", "Арочный трамвай", "Вырезы", "Исчезают", "Сольный трамвай"],
    "beard":["None", "Light Stubble", "Balbo", "Circle Beard", "Goatee", "Chin", "Chin Fuzz", "Pencil Chin Strap", "Scruffy", "Musketeer", "Mustache", "Trimmed Beard", "Stubble", "Thin Circle Beard", "Horseshoe", "Pencil and 'Chops", "Chin Strap Beard", "Balbo and Sideburns", "Mutton Chops", "Scruffy Beard", "Curly", "Curly & Deep Stranger", "Handlebar", "Faustic", "Otto & Patch", "Otto & Full Stranger", "Light Franz", "The Hampstead", "The Ambrose", "Lincoln Curtain"],
    "hairM":["None", "Buzzcut", "Faux Hawk", "Hipster", "Side Parting", "Shorter Cut", "Biker", "Ponytail", "Cornrows", "Slicked", "Short Brushed", "Spikey", "Caesar", "Chopped", "Dreads", "Long Hair", "Shaggy Curls", "Surfer Dude", "Short Side Part", "High Slicked Sides", "Long Slicked", "Hipster Youth", "Mullet", "Classic Cornrows", "Palm Cornrows", "Lightning Cornrows", "Whipped Cornrows", "Zig Zag Cornrows", "Snail Cornrows", "Hightop", "Loose Swept Back", "Undercut Swept Back", "Undercut Swept Side", "Spiked Mohawk", "Mod", "Layered Mod", "Flattop", "Rolled Quiff"],
    "hairF":["None", "Short", "Layered Bob", "Pigtails", "Ponytail", "Braided Mohawk", "Braids", "Bob", "Faux Hawk", "French Twist", "Long Bob", "Loose Tied", "Pixie", "Shaved Bangs", "Top Knot", "Wavy Bob", "Messy Bun", "Pin Up Girl", "Tight Bun", "Twisted Bob", "Flapper Bob", "Big Bangs", "Braided Top Knot", "Mullet", "Pinched Cornrows", "Leaf Cornrows", "Zig Zag Cornrows", "Pigtail Bangs", "Wave Braids", "Coil Braids", "Rolled Quiff", "Loose Swept Back", "Undercut Swept Back", "Undercut Swept Side", "Spiked Mohawk", "Bandana and Braid", "Layered Mod"],
    "hairColor":["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20"],
    "eyeColor":["Зеленый", "Изумрудный", "Светло-голубой", "Океанский синий", "Светло-Коричневый", "Темно-Коричневый", "Ореховый", "Темно-Серый", "Светло-серый", "Розовый", "Желтый", "Фиолетовый"]
};
Vue.component('list',{
    template: '<div v-if="type == 1" v-bind:id="id" class="list">\
    <img v-bind:src="path[id]+titl[id]+index+suffix[id]+png">\
    </div><div v-if="type == 2" v-bind:id="id" class="list">\
    <i @click="left" class="left flaticon-left-arrow"></i>\
    {{ values[index] }}\
    <i @click="right" class="right flaticon-arrowhead-pointing-to-the-right"></i>\
    </div>',
    props: ['id','num','type'],
    data: function(){
        return {
            index: 0,
            titl: {"father":"male_","mother":"mother","hairM":"","hairF":""},
            suffix: {"father":"","mother":"","hairM":"","hairF":"(1)"},
            path: {"father":'parents/',"mother":'parents/',"hairM":"hairs/","hairF":"hairs/"},
            png: '.png',
            values: this.num ? [-1,-0.1,-0.2,-0.3,-0.4,-0.5,-0.6,-0.7,-0.8,-0.9,0,0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,1] : data[this.id],
        }
    },
    methods: {
        left: function(event){
            this.index--
            if(this.index < 0) this.index=0
            this.send()
        },
        right: function(event){
            this.index++
            if(this.index == this.values.length) this.index=0
            this.send()
        },
        send: function(){
            var value = this.num ? this.values[this.index] : this.index
            //console.log('editorList:'+this.id+':'+value)
            mp.trigger('editorList', this.id, Number(value))
        }
    }
})
Vue.component('listfill',{
    template: '<div v-bind:id="id" class="list" style="display:flex">\
    <i @click="left" class="icon arrowLeft"></i>\
    <div> {{ values[index] }}</div>\
    <i @click="right" class="icon arrowRight"></i></div>',
    props: ['id','num'],
    data: function(){
        return {
            index: 0,
            titl: {"father":"male_","mother":"mother","hairM":"","hairF":""},
            suffix: {"father":"","mother":"","hairM":"","hairF":"(1)"},
            path: {"father":'parents/',"mother":'parents/',"hairM":"hairs/","hairF":"hairs/"},
            png: '.png',
            values: this.num ? [-1,-0.1,-0.2,-0.3,-0.4,-0.5,-0.6,-0.7,-0.8,-0.9,0,0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,1] : data[this.id],
        }
    },
    methods: {
        left: function(event){
            this.index--
            if(this.index < 0) this.index=0
            this.send()
        },
        right: function(event){
            this.index++
            if(this.index == this.values.length) this.index=0
            this.send()
        },
        send: function(){
            var value = this.num ? this.values[this.index] : this.index
            //console.log('editorList:'+this.id+':'+value)
            this.$emit('callback', { id: this.id, val: value })

            mp.trigger('editorList', this.id, Number(value))
        }
    }
})
var editor = new Vue({
    el: ".editor",
    data: {
        step: 1,
        active: true,
        gender: true,
        fatherActive: 0,
        motherActive: 0,
        isSurgery: false,
        hairActive: 0,
        genderList: ['Мужской', 'Женский'],
        hairs: {
            "hairM":["None", "Buzzcut", "Faux Hawk", "Hipster", "Side Parting", "Shorter Cut", "Biker", "Ponytail", "Cornrows", "Slicked", "Short Brushed", "Spikey", "Caesar", "Chopped", "Dreads", "Long Hair", "Shaggy Curls", "Surfer Dude", "Short Side Part", "High Slicked Sides", "Long Slicked", "Hipster Youth", "Mullet", "Classic Cornrows", "Palm Cornrows", "Lightning Cornrows", "Whipped Cornrows", "Zig Zag Cornrows", "Snail Cornrows", "Hightop", "Loose Swept Back", "Undercut Swept Back", "Undercut Swept Side", "Spiked Mohawk", "Mod", "Layered Mod", "Flattop", "Rolled Quiff"],
            "hairF":["None", "Short", "Layered Bob", "Pigtails", "Ponytail", "Braided Mohawk", "Braids", "Bob", "Faux Hawk", "French Twist", "Long Bob", "Loose Tied", "Pixie", "Shaved Bangs", "Top Knot", "Wavy Bob", "Messy Bun", "Pin Up Girl", "Tight Bun", "Twisted Bob", "Flapper Bob", "Big Bangs", "Braided Top Knot", "Mullet", "Pinched Cornrows", "Leaf Cornrows", "Zig Zag Cornrows", "Pigtail Bangs", "Wave Braids", "Coil Braids", "Rolled Quiff", "Loose Swept Back", "Undercut Swept Back", "Undercut Swept Side", "Spiked Mohawk", "Bandana and Braid", "Layered Mod"],
        }
    },
    mounted() {
        window.addEventListener('resize', this.Resize)
        this.zoomEl = document.createElement('style');
        document.querySelector('head').appendChild(this.zoomEl);
        this.Resize();
    },
    watch: {
        step(val) {

            $(function() {

                $('input[type=range]').rangeslider({
                    polyfill: false,
                });
            });
        },
        gender() {
            if(this.gender){
                mp.trigger('characterGender',"Male")
            } else { 
                mp.trigger('characterGender',"Female")
            }
        }
    },
    methods: {
        callback(json) {

            if (json.id == 'father') {
                this.fatherActive = json.val;
            } else if (json.id == 'mother') {
                this.motherActive = json.val;
            }
        },
        triggerList(id, value) {
            mp.trigger('editorList', id, Number(value))
        },
        
        Resize: (async function(){
            let zoom = await this.zoom();
            window['zoom'] = zoom;
            this.zoomEl.innerHTML = `
            * { --zoom: ${zoom}; }
            .zoom-container{transform: scale(var(--zoom));}
            .zoom-container-center{transform: scale(var(--zoom)) translate(-50%, -50%); left: 50%; top: 50%;}
            .zoom-base{zoom: var(--zoom);}
            `;
        }),
        zoom() {
            let zoom = Math.min(3, +(Math.sqrt(window.outerWidth ** 2 + window.outerHeight ** 2) / 2202.9071700822983).toFixed(3));
            return zoom
        },
        genderSw: function(type){
            //console.log("gender:"+type)
            if(type){
                this.gender=true
                mp.trigger('characterGender',"Male")
            } else {
                this.gender=false
                mp.trigger('characterGender',"Female")
            }
        },
        save: function(){
            //console.log('characterSave')
            mp.trigger('characterSave')
        }
    }
});
$(function() {
    $('input[type=range]').rangeslider({
        polyfill: false,
    });
    $(document).on('input', 'input[type="range"]', function(e) {
        let id = e.target.id;
        let val = e.target.value;
        //console.log('editorList:'+id+':'+val);
        $('output#'+id).html(val);
        mp.trigger('editorList', id, Number(val));
    });

    $('#gendermale').on('click', function(){
        $('#genderfemale').removeClass('on');
        $('#gendermale').addClass('on');
        //console.log(this)
        editor.genderSw(true);
    });
    $('#genderfemale').on('click', function(){
        $('#gendermale').removeClass('on');
        $('#genderfemale').addClass('on');
        //console.log(this)
        editor.genderSw(false);
    });
});



        // window.mp = { 
        //     trigger: function(name = 'Имя триггера не указано') {
        //         let params = Object.values(arguments).map(x => x).slice(1).join('\n- ');
        //         console.log(name + ':\n- ' + params);
        //     }
        // }