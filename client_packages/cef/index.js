let browser = null;
function drawMenuRibalka(param) {
  if (!browser) {
    mp.events.callRemote('console', 'draw menu payment house');
    browser = mp.browsers.new('package://ribalka/index.html');
    browser.execute(`setType('${param}');`);

  }
  if (!param) {
    localPlayer.freezePosition(true)
  }

}

function undrawMenuRibalka() {
  if (browser) {
    browser.destroy();
    browser = null;
  }
  localPlayer.freezePosition(false)
  mp.gui.chat.show(true);

  mp.gui.cursor.show(false, false);
}
mp.events.add('drawMenuRibalka', (param) => {
  drawMenuRibalka(param);
});

mp.events.add('undrawMenuRibalka', () => {
  undrawMenuRibalka();
});

mp.events.add('catchFishClient:ribalka', () => {
  mp.events.callRemote("console", 'catch FISH')
  mp.events.callRemote('catchFish')
})
mp.events.add('catchFishClient:ugonAuto', () => {
  mp.events.callRemote('startUgonAuto:back')

  undrawMenuRibalka()
  // mp.events.callRemote('completeUgonAuto')
})
keyBinder.bind(32, true, () => {
  if (!browser) return;
  undrawMenuRibalka()
  mp.events.callRemote('stopRibalky')
}, true)