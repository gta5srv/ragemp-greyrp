let bonesWait = false;
const MIN = 0;
const MAX = 100;

keyBinder.bind('7', true, () => {
  if(bonesWait){
    return gameNotifyOld('~y~Еще рано бросать кости');
  }

  bonesWait = true;

  setTimeout(() => {
    bonesWait = false;
  }, 5000);

  const randNum = randInt(MIN, MAX);

  gameNotifyOld('Вам выпало: ~b~' + randNum);

  rpc.callServer(
    'local-chat:action',
    `бросил(a) кости, выпало: {#76bfff}${randNum} {white}(${MIN}-${MAX})`,
    true
  );
});