let pc = {
    active : false,
    self : null,
    head : null,
    el : null,
    openCar : function(model, owner){
        this.reset()
        this.head.html('База номеров')
        this.el.append('<input type="text" maxlength="8" placeholder="Номер">')
        this.el.append('<div class="button">Пробить</div>')
        this.el.append('<p>Марка автомобиля: <span></span></p><p>Владелец автомобиля: <span></span></p>')
        this.el.children('p:first').children().html(model)
        this.el.children('p:last').children().html(owner)
        this.set()
    },
    openWanted : function(data){
        this.reset()
        this.head.html('Сейчас в розыске')
        this.el.append('<ol></ol>')
        var json = JSON.parse(data);
        json.forEach(function(item, i, arr) {
            pc.el.children('ol').append('<li>'+item+'</li>');
        });
    },
    openPerson : function(fname,lname,pass,gender,lvl,lic,number,states){
        this.reset()
        this.head.html("База данных")
        this.el.append('<input type="text" maxlength="30" placeholder="Паспорт/Имя_Фамилия">')
        this.el.append('<div class="button">Пробить</div>')
        this.el.append('<p>Имя Фамилия (пол): <span>'+fname+' ' + lname + '</span> <span>('+gender+')</span></p>')
		this.el.append('<p style="color: #fff;">Розыск: <span>'+lvl+'</span>, <span>'+states+'</span></p>')
		this.el.append('<p style="display: inline-block;float: left;">Номер телефона: <span>' + number + '</span></p>')
        this.el.append('<p style="display: inline-block;">Паспорт: <span>' + pass + '</span></p>')
        
        this.el.append('<p>Список лицензий: <span>'+lic+'</span></p>')
        this.set()
    },
    clearWanted : function(){
        this.reset()
        this.head.html("Снять розыск")
        this.el.append('<input type="text" maxlength="30" placeholder="Паспорт/Имя_Фамилия">')
        this.el.append('<div class="button">Снять</div>')
        this.set()
    },
    set : function(){
        $('.button').on('click', function(){
            var t = $(this);
            //console.log(t);
            var data = $('input')[0].value;
            //console.log('pcMenuInput:'+data);
            mp.trigger('pcMenuInput', data);
        });
    },
    reset : function(){
        $('.button').off('click');
        $(".elements").empty();
    },
    show : function(){
        this.active = true
        this.self.css('display','block')
    },
    hide : function(){
        this.active = false
        this.self.css('display','none')
    }
}
$('.pc menu li').on('click', function(){
    var t = $(this)
    //console.log("pcMenu:"+t[0].id);
    mp.trigger('pcMenu', Number(t[0].id));
})
$(document).ready(function(){
    pc.head = $('.pc .right h1')
    pc.el = $('.pc .right .elements')
    pc.self = $('.pc');
})