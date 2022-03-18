var familyCreator = new Vue({
  el: '#app',
  data: {
    active: false,
    header: "Создание своей семьи",
    page: 1,
    price: 0,
    needlvl: 0,
    playerlvl: 0,
    familycount: 0,
    createname: null,
    createcount: 15,
    allprices: 0,
    multiplier: 0,
    families: [],
    familyurl: "images/avatar.png",
    imgupload: false,
  },
  methods: {
    setinfo(json) {
      this.price = json[0]
      this.needlvl = json[1]
      this.playerlvl = json[3]
      this.multiplier = json[4]
      this.familycount = json[2]
      this.allprices = json[0]
    },
    loadFamilies() {
      mp.trigger("loadlistfamilies", "client");
    },
    changeRad(index) {
      switch (index) {
        case 0:
          this.allprices = this.price
          break;
        case 1:
          this.allprices = this.price + this.multiplier
          break;
        case 2:
          this.allprices = this.price + this.multiplier * 2
          break;
        case 3:
          this.allprices = this.price + this.multiplier * 3
          break;
      }
    },
    replaceImg() {
      if(this.imgupload) this.familyurl = "images/avatar.png"
    },
    changePage(value) {
      if(value == 2) this.loadFamilies();
      this.page = value
    },
    createFamily() {
      mp.trigger("createFamily", this.createname, this.createcount, this.familyurl);
    },
    closeMenu() {
      this.active = false
      this.page = 0
      mp.trigger("closeFamilyCreatorMenu")
    }
  }
})
