var JobStatsInfo = new Vue({
    el: ".JobStatsInfo",
    data: {
        active: false,
        money: "1",
    },
    methods: {
        set: function (money) {
            this.money = money;
        }
    }
});

var JobsEinfo = new Vue({
    el: ".JobsEinfo",
    data: {
        active: false,
    }
});




var Construction = new Vue({
    el: ".Construction",
    data: {
        active: false,
        header: "Строитель",
        money: "1",
        jobid: 11,
        work: 0,
    },
    methods: {
        set: function (money, level, currentjob, work) {
            this.money = money;
            this.level = level;
            this.jobid = currentjob;
            this.work = work;
        },
        exit: function () {
            this.active = false;
            mp.trigger('CloseConstruction');
        },
        setnewjob: function (jobsid) {
            this.jobid = jobsid;
        },
        enterJob: function (work) {
            mp.trigger('CloseConstruction');
            mp.trigger("enterJobConstruction", work);
        },
        selectJob: function (jobid) {
            mp.trigger("selectJobConstruction", jobid);
        }
    }
});

var Miner = new Vue({
    el: ".Miner",
    data: {
        active: false,
        header: "Каменщик",
        money: "1",
        money2: "2",
        money3: "3",
        jobid: 12,
        work: 0,
    },
    methods: {
        set: function (money, level, currentjob, work, money2, money3) {
            this.money = money;
            this.level = level;
            this.jobid = currentjob;
            this.work = work;
            this.money2 = money2;
            this.money3 = money3;
        },
        exit: function () {
            this.active = false;
            mp.trigger('CloseMiner');
        },
        setnewjob: function (jobsid) {
            this.jobid = jobsid;
        },
        enterJob: function (work) {
            mp.trigger("enterJobMiner", work);
        },
        selectJob: function (jobid) {
            //mp.trigger('CloseMiner');
            mp.trigger("selectJobMiner", jobid);
        }
    }
});

var Diver = new Vue({
    el: ".Diver",
    data: {
        active: false,
        header: "Дайвер",
        money: "1",
        jobid: 13,
        work: 0,
    },
    methods: {
        set: function (money, level, currentjob, work) {
            this.money = money;
            this.level = level;
            this.jobid = currentjob;
            this.work = work;
        },
        exit: function () {
            this.active = false;
            mp.trigger('CloseDiver');
        },
        setnewjob: function (jobsid) {
            this.jobid = jobsid;
        },
        enterJob: function (work) {
            mp.trigger('CloseDiver');
            mp.trigger("enterJobDiver", work);
        },
        selectJob: function (jobid) {
            mp.trigger("selectJobDiver", jobid);
        }
    }
});
var JobStatsInfoDiver = new Vue({
    el: ".JobStatsInfoDiver",
    data: {
        active: false,
        money: "1",
        obj: "0",
        obji: "5",
    },
    methods: {
        set: function (money, obj, obji) {
            this.money = money;
            this.obj = obj;
            this.obji = obji;
        }
    }
});


