var air = new Vue({
    el: ".air",
    data: {
        active: false,
        indexM: 0,
        indexC: 0,
		cur: "$",
        models: ["Tesla Model S","Tesla Model 3","Tesla Model X"],
        colors: ["Черный", "Белый", "Красный", "Оранжевый", "Желтый", "Зеленый", "Голубой", "Синий", "Фиолетовый"],
        prices: [19,199,1999],
        speeds: [19,199,1999],
        accelerations: [19,199,1999],
        places: [19,199,1999],
        leftMenuHeader: "Автосалон",
        getSpeed: function(modelName){
            return globalModelsDetails[modelName] && globalModelsDetails[modelName].maxSpeed || "";
        },
        getAcceleration: function(modelName){
            return globalModelsDetails[modelName] && globalModelsDetails[modelName].acceleration || "";
        },
        getSeats: function(modelName) {
            return globalModelsDetails[modelName] && globalModelsDetails[modelName].seats || "";
        },
		getMass: function(modelName) {
            return globalModelsDetails[modelName] && globalModelsDetails[modelName].mass || "";
        },
		getMat: function(modelName) {
            return globalModelsDetails[modelName] && globalModelsDetails[modelName].mat || "";
        },
        getName: function (modelName) {
            return globalModelsDetails[modelName] && globalModelsDetails[modelName].rn || modelName;
        }
    },
    methods: {
        model: function(modelIndex){
            this.indexM = modelIndex;
            mp.trigger('auto','model',modelIndex);
        },
        color: function(colorIndex){
            this.indexC = colorIndex;
            mp.trigger('auto','color',colorIndex);
        },
        buy: function(){
            mp.trigger('buyAuto')
        },
		testdrive: function(){
            //console.log('testdrive')
            mp.trigger('testAuto')
        },
        exit: function(){
            this.reset()
            mp.trigger('closeAuto')
        },
        reset: function(){
            this.price=-1
            this.indexM=0
            this.indexC=0
            this.models=[]
            this.colors=[]
            this.prices=[]
        }
    }
})