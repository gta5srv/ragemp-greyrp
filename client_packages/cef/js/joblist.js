var jobselector = new Vue({
    el: ".joblist",
    data: {
        active: false,
        jobid: -1,
        level: 1,
		array: -1,
        list: 
        [
            {class: "electro", name: "Электрик", level: 0, jobid: 1},
			{class: "pochta", name: "Почтальон", level: 1, jobid: 2},
			{class: "taxi", name: "Таксист", level: 2, jobid: 3},
			{class: "bus", name: "Водитель автобуса", level: 2, jobid: 4},
            {class: "gazon", name: "Газонокосильщик", level: 0, jobid: 5},
            {class: "truck", name: "Дальнобойщик", level: 5, jobid: 6},
            {class: "inkos", name: "Инкассатор", level: 4, jobid: 7},
            {class: "traktorist", name: "Тракторист", level: 1, jobid: 9},
			{class: "trashcar", name: "Мусоровозчик", level: 3, jobid: 10},
			{class: "snow", name: "Уборка штата", level: 1, jobid: 14},
        ],
    },
    methods: {
        closeJobMenu: function() {
            mp.trigger("closeJobMenu");
        },
        show: function (level, currentjob, id) {
			this.array = id;
            this.level = level;
            this.jobid = currentjob;
            this.active = true;
        },
        hide: function () {
            this.active = false;
        },
        selectJob: function(jobid) {
            mp.trigger("selectJob", jobid);
        }
    }
})