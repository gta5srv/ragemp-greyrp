var license = new Vue({
    el: '.license',
    data : {
        active : false,
        firstname : "Elon",
        lastname : "Musk",
        date : "9999.99.99",
        gender : "Male",
        lics: "",
        timer: null
    },
    methods : {
        set : function(json){
            var data = JSON.parse(json)
            this.firstname = data[0];
            this.lastname = data[1];
            this.date = data[2];
            this.gender = data[3];
            this.lics = data[4];
        },
        hide : function(){
            this.active = false
            clearTimeout(this.timer)
            mp.trigger('dochide')
        },
        show : function(){
            this.active = true
            this.timer = setTimeout(this.hide,10000);
        }
    }
});
var passport = new Vue({
    el: '.passport',
    data : {
        active : false,
        number : "999999",
        firstname : "Elon",
        lastname : "Musk",
        date : "9999.99.99",
        gender : "Male",
        member : "-",
        work: "SpaceX",
        timer: null
    },
    methods : {
        set : function(json){
            var data = JSON.parse(json)
            this.number = data[0];
            this.firstname = data[1];
            this.lastname = data[2];
            this.date = data[3];
            this.gender = data[4];
            this.member = data[5];
            this.work = data[6];
        },
        hide : function(){
            this.active = false
            clearTimeout(this.timer)
            mp.trigger('dochide')
        },
        show : function(){
            this.active = true
            this.timer = setTimeout(this.hide,10000);
        }
    }
});
var plastic = new Vue({
    el: '.plastic',
    data : {
        active : false,
        numberauto : "AAA666AA69",
        firstname : "Babur",
        dateregauto : "9999.99.99",
        nameauto : "Chery Tiggo 7 PRO",
        timer: null
    },
    methods : {
        set : function(json){
            var data = JSON.parse(json)
            this.numberauto = data[0];
            this.firstname = data[2];
            this.nameauto = data[1];
            this.dateregauto = data[3];
        },
        hide : function(){
            this.active = false
            clearTimeout(this.timer)
            mp.trigger('dochide')
        },
        show : function(){
            this.active = true
            this.timer = setTimeout(this.hide,10000);
        }
    }
});
//удостоверения
var ydovgov = new Vue({
    el: '.ydovgov',
    data : {
        active : false,
        number : "999999",
        firstname : "Elon",
        lastname : "Musk",
        date : "9999.99.99",
        gender : "Male",
        member : "-",
        work: "SpaceX",
        timer: null
    },
    methods : {
        set : function(json){
            var data = JSON.parse(json)
            this.number = data[0];
            this.firstname = data[1];
            this.lastname = data[2];
            this.date = data[3];
            this.gender = data[4];
            this.member = data[5];
            this.work = data[6];
        },
        hide : function(){
            this.active = false
            clearTimeout(this.timer)
            mp.trigger('dochide')
        },
        show : function(){
            this.active = true
            this.timer = setTimeout(this.hide,10000);
        }
    }
});

var ydovpolice = new Vue({
    el: '.ydovpolice',
    data : {
        active : false,
        number : "999999",
        firstname : "Elon",
        lastname : "Musk",
        date : "9999.99.99",
        gender : "Male",
        member : "-",
        work: "SpaceX",
        timer: null
    },
    methods : {
        set : function(json){
            var data = JSON.parse(json)
            this.number = data[0];
            this.firstname = data[1];
            this.lastname = data[2];
            this.date = data[3];
            this.gender = data[4];
            this.member = data[5];
            this.work = data[6];
        },
        hide : function(){
            this.active = false
            clearTimeout(this.timer)
            mp.trigger('dochide')
        },
        show : function(){
            this.active = true
            this.timer = setTimeout(this.hide,10000);
        }
    }
});

var ydovems = new Vue({
    el: '.ydovems',
    data : {
        active : false,
        number : "999999",
        firstname : "Elon",
        lastname : "Musk",
        date : "9999.99.99",
        gender : "Male",
        member : "-",
        work: "SpaceX",
        timer: null
    },
    methods : {
        set : function(json){
            var data = JSON.parse(json)
            this.number = data[0];
            this.firstname = data[1];
            this.lastname = data[2];
            this.date = data[3];
            this.gender = data[4];
            this.member = data[5];
            this.work = data[6];
        },
        hide : function(){
            this.active = false
            clearTimeout(this.timer)
            mp.trigger('dochide')
        },
        show : function(){
            this.active = true
            this.timer = setTimeout(this.hide,10000);
        }
    }
});

var ydovfib = new Vue({
    el: '.ydovfib',
    data : {
        active : false,
        number : "999999",
        firstname : "Elon",
        lastname : "Musk",
        date : "9999.99.99",
        gender : "Male",
        member : "-",
        work: "SpaceX",
        timer: null
    },
    methods : {
        set : function(json){
            var data = JSON.parse(json)
            this.number = data[0];
            this.firstname = data[1];
            this.lastname = data[2];
            this.date = data[3];
            this.gender = data[4];
            this.member = data[5];
            this.work = data[6];
        },
        hide : function(){
            this.active = false
            clearTimeout(this.timer)
            mp.trigger('dochide')
        },
        show : function(){
            this.active = true
            this.timer = setTimeout(this.hide,10000);
        }
    }
});

var ydovarmy = new Vue({
    el: '.ydovarmy',
    data : {
        active : false,
        number : "999999",
        firstname : "Elon",
        lastname : "Musk",
        date : "9999.99.99",
        gender : "Male",
        member : "-",
        work: "SpaceX",
        timer: null
    },
    methods : {
        set : function(json){
            var data = JSON.parse(json)
            this.number = data[0];
            this.firstname = data[1];
            this.lastname = data[2];
            this.date = data[3];
            this.gender = data[4];
            this.member = data[5];
            this.work = data[6];
        },
        hide : function(){
            this.active = false
            clearTimeout(this.timer)
            mp.trigger('dochide')
        },
        show : function(){
            this.active = true
            this.timer = setTimeout(this.hide,10000);
        }
    }
});

var ydovnews = new Vue({
    el: '.ydovnews',
    data : {
        active : false,
        number : "999999",
        firstname : "Elon",
        lastname : "Musk",
        date : "9999.99.99",
        gender : "Male",
        member : "-",
        work: "SpaceX",
        timer: null
    },
    methods : {
        set : function(json){
            var data = JSON.parse(json)
            this.number = data[0];
            this.firstname = data[1];
            this.lastname = data[2];
            this.date = data[3];
            this.gender = data[4];
            this.member = data[5];
            this.work = data[6];
        },
        hide : function(){
            this.active = false
            clearTimeout(this.timer)
            mp.trigger('dochide')
        },
        show : function(){
            this.active = true
            this.timer = setTimeout(this.hide,10000);
        }
    }
});

var ydovmws = new Vue({
    el: '.ydovmws',
    data : {
        active : false,
        number : "999999",
        firstname : "Elon",
        lastname : "Musk",
        date : "9999.99.99",
        gender : "Male",
        member : "-",
        work: "SpaceX",
        timer: null
    },
    methods : {
        set : function(json){
            var data = JSON.parse(json)
            this.number = data[0];
            this.firstname = data[1];
            this.lastname = data[2];
            this.date = data[3];
            this.gender = data[4];
            this.member = data[5];
            this.work = data[6];
        },
        hide : function(){
            this.active = false
            clearTimeout(this.timer)
            mp.trigger('dochide')
        },
        show : function(){
            this.active = true
            this.timer = setTimeout(this.hide,10000);
        }
    }
});
var ydovgr6 = new Vue({
    el: '.ydovgr6',
    data : {
        active : false,
        number : "999999",
        firstname : "Elon",
        lastname : "Musk",
        date : "9999.99.99",
        gender : "Male",
        member : "-",
        work: "SpaceX",
        timer: null
    },
    methods : {
        set : function(json){
            var data = JSON.parse(json)
            this.number = data[0];
            this.firstname = data[1];
            this.lastname = data[2];
            this.date = data[3];
            this.gender = data[4];
            this.member = data[5];
            this.work = data[6];
        },
        hide : function(){
            this.active = false
            clearTimeout(this.timer)
            mp.trigger('dochide')
        },
        show : function(){
            this.active = true
            this.timer = setTimeout(this.hide,10000);
        }
    }
});