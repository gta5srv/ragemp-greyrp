
var stock = new Vue({
    el: '.stock',
    data: {
        active: false,
        count: [23654, 45, 164, 6324, 21, 87 ]
    },
    methods: {
		take : function (index){
			mp.trigger('stockTake', index);
			this.hide();
		},
		put : function (index){
			mp.trigger('stockPut', index);
			this.hide();
		},
        show : function(){
			this.active = true;
		},
		hide : function(){
			this.active = false;
			mp.trigger('client::bizclose');
		}
    }
})

/*let stock = {
    active : true,
    self : null,
    title : null,
    value : null,
    index : 0,
    count : [10,20,30,40,50,60],
    change : function(){
        this.reset()
        switch(this.index){
            case 0:
                this.self.addClass('cash')
                this.title.html('Деньги')
            break;
            case 1:
                this.self.addClass('healkit')
                this.title.html('Аптечка')
            break;
            case 2:
                this.self.addClass('weed')
                this.title.html('Наркотики')
            break;
            case 3:
                this.self.addClass('weapons')
                this.title.html('Оружейные материалы')
            break;
            case 4:
                this.self.addClass('weaponsstock')
                this.title.html('Оружейный склад')
            break;
			case 5:
                this.self.addClass('armor')
                this.title.html('Бронежилеты')
            break;
			case 6:
                this.self.addClass('koks')
                this.title.html('Листья коки')
            break;
        } this.value.html(this.count[this.index]);
    },
    reset : function(){
        this.self.removeClass()
        this.self.addClass('stock')
    },
    show : function(){
        this.active = true; this.change();
        this.self.css('display','block')
    },
    hide : function(){
        this.active = false;
        this.self.css('display','none')
    }
}
$(document).ready(function(){
    stock.self = $('.stock');
    stock.title = $('.stock .title label');
    stock.value = $('.stock .count span');
})
$('.stock #R').on('click',()=>{
    if(!stock.active)return;
    stock.index++;
    if(stock.index > 6) stock.index = 0;
    stock.change()
})
$('.stock #L').on('click',()=>{
    if(!stock.active)return;
    stock.index--;
    if(stock.index < 0) stock.index = 6;
    stock.change()
})
$('.stock #take').on('click',()=>{
    if(!stock.active)return;
    console.log('stock:take:'+ this.class);
    mp.trigger('stockTake', stock.index);
    stock.hide();
})
$('.stock #put').on('click',()=>{
    if(!stock.active)return;
    console.log('stock:put:'+stock.index);
    mp.trigger('stockPut', stock.index);
    stock.hide();
})
$('.stock #exit').on('click',()=>{
    if(!stock.active)return;
    //console.log('stock:exit');
    mp.trigger('stockExit');
    stock.hide();
})*/