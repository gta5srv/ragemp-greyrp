let localplayer = mp.players.local;

const HANDLING = {

    PARAMS: {

        BREAK: "fHandBrakeForce",
        STEERING: "fSteeringLock",
        WHEELS: "fTractionCurveLateral",

        SKID: "fTractionCurveLateral",
        ENGINE: "fInitialDriveForce",
        CURVEMIN: "fTractionCurveMin",

        HEIGHT: "fSuspensionRaise",
        ROTATE: "fRollCentreHeightFront", 

    },

    SET: function(VEHICLE, PARAM, VALUE)
    {
        if (PARAM == "") return;
        VEHICLE.setHandling(PARAM, VALUE);
    }

}

var DRIFT = {
    open: false,
    browser: mp.browsers.new('package://cef/modules/Drift/index.html'),

    execute: function(str) {
        this.browser.execute(str);
    }
}

DRIFT.Open = function(json, defaultprm)
{
    if (!loggedin || chatActive || editing || cuffed || menuCheck() || 
        !localplayer.isInAnyVehicle(false) || localplayer.vehicle.getPedInSeat(-1) != localplayer.handle) return;

    global.menuOpen();
    DRIFT.open = true;
    DRIFT.execute(`DRIFT.open('${json}', '${defaultprm}')`);
}

DRIFT.Close = function()
{
    if (!DRIFT.open) return;
    global.menuClose();
    DRIFT.open = false;
    DRIFT.execute(`DRIFT.hide()`);
}

DRIFT.Apply = function(vehicle, params)
{   

    if (vehicle == undefined || params == undefined) return;

    HANDLING.SET(vehicle, HANDLING.PARAMS.BREAK, params.Break);

    HANDLING.SET(vehicle, HANDLING.PARAMS.WHEELS, params.Wheels);

    HANDLING.SET(vehicle, HANDLING.PARAMS.STEERING, params.Steering);

    HANDLING.SET(vehicle, HANDLING.PARAMS.SKID, params.Skid);
    HANDLING.SET(vehicle, HANDLING.PARAMS.ENGINE, params.Skid * 2);
    HANDLING.SET(vehicle, HANDLING.PARAMS.CURVEMIN, params.Skid);


    HANDLING.SET(vehicle, HANDLING.PARAMS.HEIGHT, params.Height);

    HANDLING.SET(vehicle, HANDLING.PARAMS.ROTATE, params.Rotate * 0.03);

}

mp.events.add("handling.hide", () => {
    DRIFT.Close();
});

mp.events.add("handling.open", (json, defaultprm) => {
    DRIFT.Open(json, defaultprm)
});

mp.events.add("handling.send", (breakA, wheelsA, streeringA, skidA, heightA, rotateA) => {

    if (!localplayer.isInAnyVehicle(false) || localplayer.vehicle.getPedInSeat(-1) != localplayer.handle) return;

    mp.events.callRemote("handling.send", breakA, wheelsA, streeringA, skidA, heightA, rotateA);
});

mp.events.add("handling.set", (vehicle, json) => {
    DRIFT.Apply(vehicle, JSON.parse(json));
});
mp.events.add("handling.opn", () => {
    if (!localplayer.isInAnyVehicle(false) || localplayer.vehicle.getPedInSeat(-1) != localplayer.handle) return;

    if (DRIFT.open)
        DRIFT.Close();
    else
        mp.events.callRemote("handling.open");
});