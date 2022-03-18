let rot = 0;
let carcasin;
const podiumHash = 2733879850;
const pedHash = 0xDBB17082;

mp.game.entity.createModelHideExcludingScriptObjects(1100.077,219.9723,-50.04865, 10.0, podiumHash, true);
let podium = mp.objects.new(podiumHash, new mp.Vector3(1100.077,219.9723,-50.084865));

let ped = mp.peds.new(
    pedHash, 
    new mp.Vector3(1104.9641, 220.13695, -48.995),
    270.0,
    0
);

async function rotate()
{
    rot+=0.05;
    if(rot >= 360) rot = 0;
	await podium.setRotation(0, 0, rot, 2, true);
    await carcasin.setHeading(rot);
}

mp.events.add("CAR_LOTTERY::PODIUM_LOAD_CAR_MODEL", (vModel) => {
	if(carcasin) {
		carcasin.destroy();
		carcasin = null;
	}
	carcasin = mp.vehicles.new(mp.game.joaat(vModel), new mp.Vector3(1100.077,219.9723,-50.07865),
	{
		color: [[189, 0, 132],[189,0,132]],
		locked: true
	});
	carcasin.doNotChangeAlpha = true;
	carcasin.setAllowNoPassengersLockon(true);    //no passangers
    carcasin.setCanBeVisiblyDamaged(false);       //no damages
    carcasin.setCanBreak(false);                  //can break
    carcasin.setDeformationFixed();               //fixed deformation
	carcasin.setDirtLevel(0);                     //clear
	carcasin.setDisablePetrolTankDamage(true);    //disable fueltank damage
	carcasin.setDisablePetrolTankFires(true);     //disable fire fuel
	carcasin.setDoorsLockedForAllPlayers(true);   //locked door
	carcasin.freezePosition(true);                //freeze
	carcasin.setInvincible(true);                 //godmode
	carcasin.setDoorsLocked(2);			
	mp.events.add("render", rotate);
});