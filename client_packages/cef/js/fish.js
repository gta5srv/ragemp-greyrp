let progress = 0;
const minProgress = 0;
const maxProgress = 100;
const progressElement = document.querySelector('.bar');
let type = 'ribalka'

const pressButtonContainer = document.querySelector('.press-button-container');

let intervalTime = 100;
let valuePerTick = -0.5;
let keyToPress = "e";
let keyCode = 69
let addProgressDisabled = false;

const parametersData = {
    intervalTime: 100, //in miliseconds
    valuePerTick: -0.5,
    keyToPress: null
}


function addToProgress(value, duration) {
    if (addProgressDisabled) return;
    progress += value;
    if (progress < minProgress) {
        progress = minProgress;
    }
    if (progress >= maxProgress) {
        progress = maxProgress;
        endFunction()
        return;
    }

    drawCurrentProgress(progress, duration)
}

function dropToMin(duration) {
    progress = minProgress;
    console.log(progress)
    drawCurrentProgress(progress, duration)
}

function drawCurrentProgress(value, duration) {
    progressElement.style.width = value + "%";
    progressElement.style.transitionDuration = duration + 'ms';
}


function pressButton(pressed) {
    let className = 'pressed-container';
    if (pressed) pressButtonContainer.classList.add(className)
    else pressButtonContainer.classList.remove(className)
}


function swapElements(showDoggo) {
    addProgressDisabled = showDoggo;

}

function startAgain() {
    swapElements(false)
}

function endFunction() {


    swapElements(true)
    sendToServer()

    dropToMin(1)
    setTimeout(() => {
        startAgain()
    }, 2000)
}

function setType(value) {
    type = value;
}

//


function setIntervalTime(value) {
    intervalTime = value
}

function setValuePerTick(value) {
    valuePerTick = value
}

function setKeyToPress(value) {
    if (value) {
        keyToPress = value;
        document.querySelector('.press-button-value').innerText = value.toUpperCase();
    }
}

function setProgressParameters(params) {
    setIntervalTime(params.intervalTime);
    setValuePerTick(params.valuePerTick);
    setKeyToPress(params.keyToPress);
}


setInterval(() => {
    addToProgress(valuePerTick, intervalTime);
}, intervalTime)

document.addEventListener('keydown', function (event) {
    console.log(event.keyCode)
    //mp.trigger('consoleServer',event)
    if (event.keyCode === keyCode) pressButton(true);
})

document.addEventListener('keyup', function (event) {

    if (event.keyCode === keyCode) {
        pressButton(false)
        addToProgress(10, intervalTime);
    }
})

function sendToServer() {

    mp.trigger('client::fishtoserver')
}

setProgressParameters(parametersData)