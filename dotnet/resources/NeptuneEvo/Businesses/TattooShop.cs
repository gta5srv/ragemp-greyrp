using GTANetworkAPI;
using System.Collections.Generic;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;
using System;
using Newtonsoft.Json;


namespace NeptuneEVO.Businesses
{
    class TattooShopI : Script
    {

        private static nLog Log = new nLog("TATTOO");

        public class TattooShop : BCore.Bizness
        {
            public static int CostForTatto = 100;

            public TattooShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 9;
                Name = "Тату-Салон";
                BlipColor = 4;
                BlipType = 75;
                Range = 2f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;

                // if ((player.GetData("ON_DUTY") && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2) || player.GetData("ON_WORK"))
                // {
                    // Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны закончить рабочий день", 300);
                    // return;
                // }
                Main.Players[player].ExteriorPos = player.Position;
                var dim = Dimensions.RequestPrivateDimension(player);
                NAPI.Entity.SetEntityDimension(player, dim);
                NAPI.Entity.SetEntityPosition(player, new Vector3(324.9798, 180.6418, 103.6665));
                player.Rotation = new Vector3(0, 0, 101.0228);
                player.PlayAnimation("amb@world_human_guard_patrol@male@base", "base", 1);
                Customization.ClearClothes(player, Main.Players[player].Gender);

                Trigger.PlayerEvent(player, "openBody", false, CostForTatto);
            }

            public static void Buy(Player player, params object[] arguments)
            {
                try
                {
                    var zone = Convert.ToInt32(arguments[0].ToString());
                    var tattooID = Convert.ToInt32(arguments[1].ToString());
                    var tattoo = BusinessTattoos[zone][tattooID];

                    Log.Debug($"buyTattoo zone: {zone} | id: {tattooID}");


                    double price = tattoo.Price / 100.0 * CostForTatto;
                    if (Main.Players[player].Money < Convert.ToInt32(price))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 300);
                        return;
                    }

                    var amount = Convert.ToInt32(price * 0.75 / 10);
                    if (amount <= 0) amount = 1;
                    //GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", Convert.ToInt32(price), "buyTattoo");
                    MoneySystem.Wallet.Change(player, -Convert.ToInt32(price));

                    var tattooHash = (Main.Players[player].Gender) ? tattoo.MaleHash : tattoo.FemaleHash;
                    List<Tattoo> validTattoos = new List<Tattoo>();
                    foreach (var t in Customization.CustomPlayerData[Main.Players[player].UUID].Tattoos[zone])
                    {
                        var isValid = true;
                        foreach (var slot in tattoo.Slots)
                        {
                            if (t.Slots.Contains(slot))
                            {
                                isValid = false;
                                break;
                            }
                        }
                        if (isValid) validTattoos.Add(t);
                    }

                    validTattoos.Add(new Tattoo(tattoo.Dictionary, tattooHash, tattoo.Slots));
                    Customization.CustomPlayerData[Main.Players[player].UUID].Tattoos[zone] = validTattoos;

                    player.SetSharedData("TATTOOS", JsonConvert.SerializeObject(Customization.CustomPlayerData[Main.Players[player].UUID].Tattoos));

                    Utils.QuestsManager.AddQuestProcess(player, 8);

                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вам набили татуировку {tattoo.Name} за {Convert.ToInt32(price)}$", 300);
                }
                catch (Exception e) { Log.Write("BuyTattoo: " + e.Message, nLog.Type.Error); }
            }

            public static void Cancel(Player player)
            {
                try
                {
                    NAPI.Entity.SetEntityDimension(player, 0);
                    NAPI.Entity.SetEntityPosition(player, Main.Players[player].ExteriorPos + new Vector3(0, 0, 1.12));
                    Main.Players[player].ExteriorPos = new Vector3();
                    Customization.ApplyCharacter(player);
                }
                catch (Exception e) { Log.Write("CancelBody: " + e.Message, nLog.Type.Error); }
            }

        }

        // REMOTE EVENTS //

        [RemoteEvent("cancelBody")]
        public static void Cancel(Player player)
        {
            try
            {
                TattooShopI.TattooShop.Cancel(player);
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }

        [RemoteEvent("buyTattoo")]
        public static void Buy(Player player, params object[] arguments)
        {
            try
            {
                TattooShopI.TattooShop.Buy(player, arguments);
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }

        // TATTOOS //

        

        public static List<List<BusinessTattoo>> BusinessTattoos = new List<List<BusinessTattoo>>()
        {
            // Torso
            new List<BusinessTattoo>()
            {
	            // Левый сосок  -   0
                // Правый сосок -   1
                // Живот        -   2
                // Левый низ спины    -   3
	            // Правый низ спины    -   4
                // Левый верх спины   -   5
                // Правый верх спины   -   6
                // Левый бок    -   7
                // Правый бок   -   8
                new BusinessTattoo(new List<int>(){ 2 }, "Refined Hustler", "mpbusiness_overlays", "MP_Buis_M_Stomach_000", String.Empty, 4500),
                new BusinessTattoo(new List<int>(){ 1 }, "Rich", "mpbusiness_overlays", "MP_Buis_M_Chest_000", String.Empty, 2475),
                new BusinessTattoo(new List<int>(){ 0 }, "$$$", "mpbusiness_overlays", "MP_Buis_M_Chest_001", String.Empty, 2475),
                new BusinessTattoo(new List<int>(){ 3, 4 }, "Makin' Paper", "mpbusiness_overlays", "MP_Buis_M_Back_000", String.Empty, 3000),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "High Roller", "mpbusiness_overlays", String.Empty, "MP_Buis_F_Chest_000", 2475),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Makin' Money", "mpbusiness_overlays", String.Empty, "MP_Buis_F_Chest_001", 3650),
                new BusinessTattoo(new List<int>(){ 1 }, "Love Money", "mpbusiness_overlays", String.Empty, "MP_Buis_F_Chest_002", 2475),
                new BusinessTattoo(new List<int>(){ 2 }, "Diamond Back", "mpbusiness_overlays", String.Empty, "MP_Buis_F_Stom_000", 4500),
                new BusinessTattoo(new List<int>(){ 8 }, "Santo Capra Logo", "mpbusiness_overlays", String.Empty, "MP_Buis_F_Stom_001", 3000),
                new BusinessTattoo(new List<int>(){ 8 }, "Money Bag", "mpbusiness_overlays", String.Empty, "MP_Buis_F_Stom_002", 3000),
                new BusinessTattoo(new List<int>(){ 3, 4 }, "Respect", "mpbusiness_overlays", String.Empty, "MP_Buis_F_Back_000", 3000),
                new BusinessTattoo(new List<int>(){ 3, 4 }, "Gold Digger", "mpbusiness_overlays", String.Empty, "MP_Buis_F_Back_001", 1250),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Carp Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_005", "MP_Xmas2_F_Tat_005", 9325),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Carp Shaded", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_006", "MP_Xmas2_F_Tat_006", 9325),
                new BusinessTattoo(new List<int>(){ 1 }, "Time To Die", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_009", "MP_Xmas2_F_Tat_009", 1825),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Roaring Tiger", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_011", "MP_Xmas2_F_Tat_011", 3325),
                new BusinessTattoo(new List<int>(){ 7 }, "Lizard", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_013", "MP_Xmas2_F_Tat_013", 3000),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Japanese Warrior", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_015", "MP_Xmas2_F_Tat_015", 3650),
                new BusinessTattoo(new List<int>(){ 0 }, "Loose Lips Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_016", "MP_Xmas2_F_Tat_016", 2475),
                new BusinessTattoo(new List<int>(){ 0 }, "Loose Lips Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_017", "MP_Xmas2_F_Tat_017", 2475),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Royal Dagger Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_018", "MP_Xmas2_F_Tat_018", 3650),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Royal Dagger Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_019", "MP_Xmas2_F_Tat_019", 3650),
                new BusinessTattoo(new List<int>(){ 2, 8 }, "Executioner", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_028", "MP_Xmas2_F_Tat_028", 3000),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Bullet Proof", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_000_M", "MP_Gunrunning_Tattoo_000_F", 3000),
                new BusinessTattoo(new List<int>(){ 3, 4 }, "Crossed Weapons", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_001_M", "MP_Gunrunning_Tattoo_001_F", 3000),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Butterfly Knife", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_009_M", "MP_Gunrunning_Tattoo_009_F", 3325),
                new BusinessTattoo(new List<int>(){ 2 }, "Cash Money", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_010_M", "MP_Gunrunning_Tattoo_010_F", 4500),
                new BusinessTattoo(new List<int>(){ 1 }, "Dollar Daggers", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_012_M", "MP_Gunrunning_Tattoo_012_F", 2475),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Wolf Insignia", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_013_M", "MP_Gunrunning_Tattoo_013_F", 3325),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Backstabber", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_014_M", "MP_Gunrunning_Tattoo_014_F", 3325),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Dog Tags", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_017_M", "MP_Gunrunning_Tattoo_017_F", 3650),
                new BusinessTattoo(new List<int>(){ 3, 4 }, "Dual Wield Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_018_M", "MP_Gunrunning_Tattoo_018_F", 3325),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Pistol Wings", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_019_M", "MP_Gunrunning_Tattoo_019_F", 3325),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Crowned Weapons", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_020_M", "MP_Gunrunning_Tattoo_020_F", 3650),
                new BusinessTattoo(new List<int>(){ 5 }, "Explosive Heart", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_022_M", "MP_Gunrunning_Tattoo_022_F", 2475),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Micro SMG Chain", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_028_M", "MP_Gunrunning_Tattoo_028_F", 3650),
                new BusinessTattoo(new List<int>(){ 2 }, "Win Some Lose Some", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_029_M", "MP_Gunrunning_Tattoo_029_F", 4500),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Crossed Arrows", "mphipster_overlays", "FM_Hip_M_Tat_000", "FM_Hip_F_Tat_000", 3325),
                new BusinessTattoo(new List<int>(){ 1 }, "Chemistry", "mphipster_overlays", "FM_Hip_M_Tat_002", "FM_Hip_F_Tat_002", 2475),
                new BusinessTattoo(new List<int>(){ 7 }, "Feather Birds", "mphipster_overlays", "FM_Hip_M_Tat_006", "FM_Hip_F_Tat_006", 300),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Infinity", "mphipster_overlays", "FM_Hip_M_Tat_011", "FM_Hip_F_Tat_011", 3325),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Antlers", "mphipster_overlays", "FM_Hip_M_Tat_012", "FM_Hip_F_Tat_012", 3325),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Boombox", "mphipster_overlays", "FM_Hip_M_Tat_013", "FM_Hip_F_Tat_013", 3650),
                new BusinessTattoo(new List<int>(){ 6 }, "Pyramid", "mphipster_overlays", "FM_Hip_M_Tat_024", "FM_Hip_F_Tat_024", 2475),
                new BusinessTattoo(new List<int>(){ 5 }, "Watch Your Step", "mphipster_overlays", "FM_Hip_M_Tat_025", "FM_Hip_F_Tat_025", 2475),
                new BusinessTattoo(new List<int>(){ 2, 8 }, "Sad", "mphipster_overlays", "FM_Hip_M_Tat_029", "FM_Hip_F_Tat_029", 5475),
                new BusinessTattoo(new List<int>(){ 3, 4 }, "Shark Fin", "mphipster_overlays", "FM_Hip_M_Tat_030", "FM_Hip_F_Tat_030", 3325),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Skateboard", "mphipster_overlays", "FM_Hip_M_Tat_031", "FM_Hip_F_Tat_031", 3325),
                new BusinessTattoo(new List<int>(){ 6 }, "Paper Plane", "mphipster_overlays", "FM_Hip_M_Tat_032", "FM_Hip_F_Tat_032", 2475),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Stag", "mphipster_overlays", "FM_Hip_M_Tat_033", "FM_Hip_F_Tat_033", 3650),
                new BusinessTattoo(new List<int>(){ 2, 8 }, "Sewn Heart", "mphipster_overlays", "FM_Hip_M_Tat_035", "FM_Hip_F_Tat_035", 5475),
                new BusinessTattoo(new List<int>(){ 3 }, "Tooth", "mphipster_overlays", "FM_Hip_M_Tat_041", "FM_Hip_F_Tat_041", 3000),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Triangles", "mphipster_overlays", "FM_Hip_M_Tat_046", "FM_Hip_F_Tat_046", 3325),
                new BusinessTattoo(new List<int>(){ 1 }, "Cassette", "mphipster_overlays", "FM_Hip_M_Tat_047", "FM_Hip_F_Tat_047", 2475),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Block Back", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_000_M", "MP_MP_ImportExport_Tat_000_F", 3325),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Power Plant", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_001_M", "MP_MP_ImportExport_Tat_001_F", 3325),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Tuned to Death", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_002_M", "MP_MP_ImportExport_Tat_002_F", 3325),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Serpents of Destruction", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_009_M", "MP_MP_ImportExport_Tat_009_F", 1125),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Take the Wheel", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_010_M", "MP_MP_ImportExport_Tat_010_F", 3325),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Talk Shit Get Hit", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_011_M", "MP_MP_ImportExport_Tat_011_F", 3325),
                new BusinessTattoo(new List<int>(){ 0 }, "King Fight", "mplowrider_overlays", "MP_LR_Tat_001_M", "MP_LR_Tat_001_F", 2475),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Holy Mary", "mplowrider_overlays", "MP_LR_Tat_002_M", "MP_LR_Tat_002_F", 3650),
                new BusinessTattoo(new List<int>(){ 7 }, "Gun Mic", "mplowrider_overlays", "MP_LR_Tat_004_M", "MP_LR_Tat_004_F", 3000),
                new BusinessTattoo(new List<int>(){ 6 }, "Amazon", "mplowrider_overlays", "MP_LR_Tat_009_M", "MP_LR_Tat_009_F", 875),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Bad Angel", "mplowrider_overlays", "MP_LR_Tat_010_M", "MP_LR_Tat_010_F", 1800),
                new BusinessTattoo(new List<int>(){ 1 }, "Love Gamble", "mplowrider_overlays", "MP_LR_Tat_013_M", "MP_LR_Tat_013_F", 2475),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Love is Blind", "mplowrider_overlays", "MP_LR_Tat_014_M", "MP_LR_Tat_014_F", 1825),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Sad Angel", "mplowrider_overlays", "MP_LR_Tat_021_M", "MP_LR_Tat_021_F", 1550),
                new BusinessTattoo(new List<int>(){ 1 }, "Royal Takeover", "mplowrider_overlays", "MP_LR_Tat_026_M", "MP_LR_Tat_026_F", 2475),
                new BusinessTattoo(new List<int>(){ 1 }, "Turbulence", "mpairraces_overlays", "MP_Airraces_Tattoo_000_M", "MP_Airraces_Tattoo_000_F", 2475),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Pilot Skull", "mpairraces_overlays", "MP_Airraces_Tattoo_001_M", "MP_Airraces_Tattoo_001_F", 3325),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Winged Bombshell", "mpairraces_overlays", "MP_Airraces_Tattoo_002_M", "MP_Airraces_Tattoo_002_F", 3325),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Balloon Pioneer", "mpairraces_overlays", "MP_Airraces_Tattoo_004_M", "MP_Airraces_Tattoo_004_F", 1500),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Parachute Belle", "mpairraces_overlays", "MP_Airraces_Tattoo_005_M", "MP_Airraces_Tattoo_005_F", 3325),
                new BusinessTattoo(new List<int>(){ 2 }, "Bombs Away", "mpairraces_overlays", "MP_Airraces_Tattoo_006_M", "MP_Airraces_Tattoo_006_F", 4500),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Eagle Eyes", "mpairraces_overlays", "MP_Airraces_Tattoo_007_M", "MP_Airraces_Tattoo_007_F", 3325),
                new BusinessTattoo(new List<int>(){ 0 }, "Demon Rider", "mpbiker_overlays", "MP_MP_Biker_Tat_000_M", "MP_MP_Biker_Tat_000_F", 2475),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Both Barrels", "mpbiker_overlays", "MP_MP_Biker_Tat_001_M", "MP_MP_Biker_Tat_001_F", 3650),
                new BusinessTattoo(new List<int>(){ 2 }, "Web Rider", "mpbiker_overlays", "MP_MP_Biker_Tat_003_M", "MP_MP_Biker_Tat_003_F", 4500),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Made In America", "mpbiker_overlays", "MP_MP_Biker_Tat_005_M", "MP_MP_Biker_Tat_005_F", 3650),
                new BusinessTattoo(new List<int>(){ 3, 4 }, "Chopper Freedom", "mpbiker_overlays", "MP_MP_Biker_Tat_006_M", "MP_MP_Biker_Tat_006_F", 3000),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Freedom Wheels", "mpbiker_overlays", "MP_MP_Biker_Tat_008_M", "MP_MP_Biker_Tat_008_F", 3325),
                new BusinessTattoo(new List<int>(){ 2 }, "Skull Of Taurus", "mpbiker_overlays", "MP_MP_Biker_Tat_010_M", "MP_MP_Biker_Tat_010_F", 925),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "R.I.P. My Brothers", "mpbiker_overlays", "MP_MP_Biker_Tat_011_M", "MP_MP_Biker_Tat_011_F", 3325),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Demon Crossbones", "mpbiker_overlays", "MP_MP_Biker_Tat_013_M", "MP_MP_Biker_Tat_013_F", 4500),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Clawed Beast", "mpbiker_overlays", "MP_MP_Biker_Tat_017_M", "MP_MP_Biker_Tat_017_F", 3325),
                new BusinessTattoo(new List<int>(){ 1 }, "Skeletal Chopper", "mpbiker_overlays", "MP_MP_Biker_Tat_018_M", "MP_MP_Biker_Tat_018_F", 2700),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Gruesome Talons", "mpbiker_overlays", "MP_MP_Biker_Tat_019_M", "MP_MP_Biker_Tat_019_F", 3975),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Flaming Reaper", "mpbiker_overlays", "MP_MP_Biker_Tat_021_M", "MP_MP_Biker_Tat_021_F", 3325),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Western MC", "mpbiker_overlays", "MP_MP_Biker_Tat_023_M", "MP_MP_Biker_Tat_023_F", 3975),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "American Dream", "mpbiker_overlays", "MP_MP_Biker_Tat_026_M", "MP_MP_Biker_Tat_026_F", 3925),
                new BusinessTattoo(new List<int>(){ 0 }, "Bone Wrench", "mpbiker_overlays", "MP_MP_Biker_Tat_029_M", "MP_MP_Biker_Tat_029_F", 2725),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Brothers For Life", "mpbiker_overlays", "MP_MP_Biker_Tat_030_M", "MP_MP_Biker_Tat_030_F", 3350),
                new BusinessTattoo(new List<int>(){ 2 }, "Gear Head", "mpbiker_overlays", "MP_MP_Biker_Tat_031_M", "MP_MP_Biker_Tat_031_F", 4500),
                new BusinessTattoo(new List<int>(){ 0 }, "Western Eagle", "mpbiker_overlays", "MP_MP_Biker_Tat_032_M", "MP_MP_Biker_Tat_032_F", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Brotherhood of Bikes", "mpbiker_overlays", "MP_MP_Biker_Tat_034_M", "MP_MP_Biker_Tat_034_F", 2725),
                new BusinessTattoo(new List<int>(){ 2 }, "Gas Guzzler", "mpbiker_overlays", "MP_MP_Biker_Tat_039_M", "MP_MP_Biker_Tat_039_F", 4225),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "No Regrets", "mpbiker_overlays", "MP_MP_Biker_Tat_041_M", "MP_MP_Biker_Tat_041_F", 3650),
                new BusinessTattoo(new List<int>(){ 3, 4 }, "Ride Forever", "mpbiker_overlays", "MP_MP_Biker_Tat_043_M", "MP_MP_Biker_Tat_043_F", 3050),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Unforgiven", "mpbiker_overlays", "MP_MP_Biker_Tat_050_M", "MP_MP_Biker_Tat_050_F", 4500),
                new BusinessTattoo(new List<int>(){ 2 }, "Biker Mount", "mpbiker_overlays", "MP_MP_Biker_Tat_052_M", "MP_MP_Biker_Tat_052_F", 3650),
                new BusinessTattoo(new List<int>(){ 1 }, "Reaper Vulture", "mpbiker_overlays", "MP_MP_Biker_Tat_058_M", "MP_MP_Biker_Tat_058_F", 2475),
                new BusinessTattoo(new List<int>(){ 1 }, "Faggio", "mpbiker_overlays", "MP_MP_Biker_Tat_059_M", "MP_MP_Biker_Tat_059_F", 2475),
                new BusinessTattoo(new List<int>(){ 0 }, "We Are The Mods!", "mpbiker_overlays", "MP_MP_Biker_Tat_060_M", "MP_MP_Biker_Tat_060_F", 2725),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "SA Assault", "mplowrider2_overlays", "MP_LR_Tat_000_M", "MP_LR_Tat_000_F", 1550),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Love the Game", "mplowrider2_overlays", "MP_LR_Tat_008_M", "MP_LR_Tat_008_F", 1525),
                new BusinessTattoo(new List<int>(){ 7 }, "Lady Liberty", "mplowrider2_overlays", "MP_LR_Tat_011_M", "MP_LR_Tat_011_F", 3050),
                new BusinessTattoo(new List<int>(){ 0 }, "Royal Kiss", "mplowrider2_overlays", "MP_LR_Tat_012_M", "MP_LR_Tat_012_F", 2475),
                new BusinessTattoo(new List<int>(){ 2 }, "Two Face", "mplowrider2_overlays", "MP_LR_Tat_016_M", "MP_LR_Tat_016_F", 910),
                new BusinessTattoo(new List<int>(){ 1 }, "Death Behind", "mplowrider2_overlays", "MP_LR_Tat_019_M", "MP_LR_Tat_019_F", 2475),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Dead Pretty", "mplowrider2_overlays", "MP_LR_Tat_031_M", "MP_LR_Tat_031_F", 1525),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Reign Over", "mplowrider2_overlays", "MP_LR_Tat_032_M", "MP_LR_Tat_032_F", 1560),
                new BusinessTattoo(new List<int>(){ 2 }, "Abstract Skull", "mpluxe_overlays", "MP_LUXE_TAT_003_M", "MP_LUXE_TAT_003_F", 3975),
                new BusinessTattoo(new List<int>(){ 1 }, "Eye of the Griffin", "mpluxe_overlays", "MP_LUXE_TAT_007_M", "MP_LUXE_TAT_007_F", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Flying Eye", "mpluxe_overlays", "MP_LUXE_TAT_008_M", "MP_LUXE_TAT_008_F", 2700),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Ancient Queen", "mpluxe_overlays", "MP_LUXE_TAT_014_M", "MP_LUXE_TAT_014_F", 660),
                new BusinessTattoo(new List<int>(){ 0 }, "Smoking Sisters", "mpluxe_overlays", "MP_LUXE_TAT_015_M", "MP_LUXE_TAT_015_F", 2475),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Feather Mural", "mpluxe_overlays", "MP_LUXE_TAT_024_M", "MP_LUXE_TAT_024_F", 9325),
                new BusinessTattoo(new List<int>(){ 0 }, "The Howler", "mpluxe2_overlays", "MP_LUXE_TAT_002_M", "MP_LUXE_TAT_002_F", 2475),
                new BusinessTattoo(new List<int>(){ 0, 1, 2, 8 }, "Geometric Galaxy", "mpluxe2_overlays", "MP_LUXE_TAT_012_M", "MP_LUXE_TAT_012_F", 2100),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Cloaked Angel", "mpluxe2_overlays", "MP_LUXE_TAT_022_M", "MP_LUXE_TAT_022_F", 1800),
                new BusinessTattoo(new List<int>(){ 0 }, "Reaper Sway", "mpluxe2_overlays", "MP_LUXE_TAT_025_M", "MP_LUXE_TAT_025_F", 2475),
                new BusinessTattoo(new List<int>(){ 1 }, "Cobra Dawn", "mpluxe2_overlays", "MP_LUXE_TAT_027_M", "MP_LUXE_TAT_027_F", 2700),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Geometric Design T", "mpluxe2_overlays", "MP_LUXE_TAT_029_M", "MP_LUXE_TAT_029_F", 1550),
                new BusinessTattoo(new List<int>(){ 1 }, "Bless The Dead", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_000_M", "MP_Smuggler_Tattoo_000_F", 300),
                new BusinessTattoo(new List<int>(){ 2 }, "Dead Lies", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_002_M", "MP_Smuggler_Tattoo_002_F", 4500),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Give Nothing Back", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_003_M", "MP_Smuggler_Tattoo_003_F", 3000),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Never Surrender", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_006_M", "MP_Smuggler_Tattoo_006_F", 3050),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "No Honor", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_007_M", "MP_Smuggler_Tattoo_007_F", 3650),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Tall Ship Conflict", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_009_M", "MP_Smuggler_Tattoo_009_F", 3000),
                new BusinessTattoo(new List<int>(){ 2 }, "See You In Hell", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_010_M", "MP_Smuggler_Tattoo_010_F", 4500),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Torn Wings", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_013_M", "MP_Smuggler_Tattoo_013_F", 3050),
                new BusinessTattoo(new List<int>(){ 2 }, "Jolly Roger", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_015_M", "MP_Smuggler_Tattoo_015_F", 3500),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Skull Compass", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_016_M", "MP_Smuggler_Tattoo_016_F", 3000),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Framed Tall Ship", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_017_M", "MP_Smuggler_Tattoo_017_F", 1550),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Finders Keepers", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_018_M", "MP_Smuggler_Tattoo_018_F", 1800),
                new BusinessTattoo(new List<int>(){ 0 }, "Lost At Sea", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_019_M", "MP_Smuggler_Tattoo_019_F", 2475),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Dead Tales", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_021_M", "MP_Smuggler_Tattoo_021_F", 3000),
                new BusinessTattoo(new List<int>(){ 5 }, "X Marks The Spot", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_022_M", "MP_Smuggler_Tattoo_022_F", 2475),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Pirate Captain", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_024_M", "MP_Smuggler_Tattoo_024_F", 1550),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Claimed By The Beast", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_025_M", "MP_Smuggler_Tattoo_025_F", 1550),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Wheels of Death", "mpstunt_overlays", "MP_MP_Stunt_Tat_011_M", "MP_MP_Stunt_Tat_011_F", 3000),
                new BusinessTattoo(new List<int>(){ 7 }, "Punk Biker", "mpstunt_overlays", "MP_MP_Stunt_Tat_012_M", "MP_MP_Stunt_Tat_012_F", 3000),
                new BusinessTattoo(new List<int>(){ 2 }, "Bat Cat of Spades", "mpstunt_overlays", "MP_MP_Stunt_Tat_014_M", "MP_MP_Stunt_Tat_014_F", 910),
                new BusinessTattoo(new List<int>(){ 0 }, "Vintage Bully", "mpstunt_overlays", "MP_MP_Stunt_Tat_018_M", "MP_MP_Stunt_Tat_018_F", 2475),
                new BusinessTattoo(new List<int>(){ 1 }, "Engine Heart", "mpstunt_overlays", "MP_MP_Stunt_Tat_019_M", "MP_MP_Stunt_Tat_019_F", 2475),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Road Kill", "mpstunt_overlays", "MP_MP_Stunt_Tat_024_M", "MP_MP_Stunt_Tat_024_F", 1500),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Winged Wheel", "mpstunt_overlays", "MP_MP_Stunt_Tat_026_M", "MP_MP_Stunt_Tat_026_F", 3000),
                new BusinessTattoo(new List<int>(){ 0 }, "Punk Road Hog", "mpstunt_overlays", "MP_MP_Stunt_Tat_027_M", "MP_MP_Stunt_Tat_027_F", 2475),
                new BusinessTattoo(new List<int>(){ 3, 4 }, "Majestic Finish", "mpstunt_overlays", "MP_MP_Stunt_Tat_029_M", "MP_MP_Stunt_Tat_029_F", 3000),
                new BusinessTattoo(new List<int>(){ 6 }, "Man's Ruin", "mpstunt_overlays", "MP_MP_Stunt_Tat_030_M", "MP_MP_Stunt_Tat_030_F", 3050),
                new BusinessTattoo(new List<int>(){ 1 }, "Sugar Skull Trucker", "mpstunt_overlays", "MP_MP_Stunt_Tat_033_M", "MP_MP_Stunt_Tat_033_F", 2475),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Feather Road Kill", "mpstunt_overlays", "MP_MP_Stunt_Tat_034_M", "MP_MP_Stunt_Tat_034_F", 1825),
                new BusinessTattoo(new List<int>(){ 5 }, "Big Grills", "mpstunt_overlays", "MP_MP_Stunt_Tat_037_M", "MP_MP_Stunt_Tat_037_F", 2475),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Monkey Chopper", "mpstunt_overlays", "MP_MP_Stunt_Tat_040_M", "MP_MP_Stunt_Tat_040_F", 3000),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Brapp", "mpstunt_overlays", "MP_MP_Stunt_Tat_041_M", "MP_MP_Stunt_Tat_041_F", 3000),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Ram Skull", "mpstunt_overlays", "MP_MP_Stunt_Tat_044_M", "MP_MP_Stunt_Tat_044_F", 3000),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Full Throttle", "mpstunt_overlays", "MP_MP_Stunt_Tat_046_M", "MP_MP_Stunt_Tat_046_F", 3050),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Racing Doll", "mpstunt_overlays", "MP_MP_Stunt_Tat_048_M", "MP_MP_Stunt_Tat_048_F", 3050),
                new BusinessTattoo(new List<int>(){ 0 }, "Blackjack", "multiplayer_overlays", "FM_Tat_Award_M_003", "FM_Tat_Award_F_003", 2700),
                new BusinessTattoo(new List<int>(){ 2 }, "Hustler", "multiplayer_overlays", "FM_Tat_Award_M_004", "FM_Tat_Award_F_004", 925),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Angel", "multiplayer_overlays", "FM_Tat_Award_M_005", "FM_Tat_Award_F_005", 3050),
                new BusinessTattoo(new List<int>(){ 3, 4 }, "Los Santos Customs", "multiplayer_overlays", "FM_Tat_Award_M_008", "FM_Tat_Award_F_008", 2440),
                new BusinessTattoo(new List<int>(){ 1 }, "Blank Scroll", "multiplayer_overlays", "FM_Tat_Award_M_011", "FM_Tat_Award_F_011", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Embellished Scroll", "multiplayer_overlays", "FM_Tat_Award_M_012", "FM_Tat_Award_F_012", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Seven Deadly Sins", "multiplayer_overlays", "FM_Tat_Award_M_013", "FM_Tat_Award_F_013", 2700),
                new BusinessTattoo(new List<int>(){ 3, 4 }, "Trust No One", "multiplayer_overlays", "FM_Tat_Award_M_014", "FM_Tat_Award_F_014", 4050),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Clown", "multiplayer_overlays", "FM_Tat_Award_M_016", "FM_Tat_Award_F_016", 3000),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Clown and Gun", "multiplayer_overlays", "FM_Tat_Award_M_017", "FM_Tat_Award_F_017", 3050),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Clown Dual Wield", "multiplayer_overlays", "FM_Tat_Award_M_018", "FM_Tat_Award_F_018", 3000),
                new BusinessTattoo(new List<int>(){ 6, 6 }, "Clown Dual Wield Dollars", "multiplayer_overlays", "FM_Tat_Award_M_019", "FM_Tat_Award_F_019", 3050),
                new BusinessTattoo(new List<int>(){ 2 }, "Faith T", "multiplayer_overlays", "FM_Tat_M_004", "FM_Tat_F_004", 910),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Skull on the Cross", "multiplayer_overlays", "FM_Tat_M_009", "FM_Tat_F_009", 1800),
                new BusinessTattoo(new List<int>(){ 1 }, "LS Flames", "multiplayer_overlays", "FM_Tat_M_010", "FM_Tat_F_010", 2700),
                new BusinessTattoo(new List<int>(){ 5 }, "LS Script", "multiplayer_overlays", "FM_Tat_M_011", "FM_Tat_F_011", 3050),
                new BusinessTattoo(new List<int>(){ 2 }, "Los Santos Bills", "multiplayer_overlays", "FM_Tat_M_012", "FM_Tat_F_012", 4500),
                new BusinessTattoo(new List<int>(){ 6 }, "Eagle and Serpent", "multiplayer_overlays", "FM_Tat_M_013", "FM_Tat_F_013", 3050),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Evil Clown", "multiplayer_overlays", "FM_Tat_M_016", "FM_Tat_F_016", 1575),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "The Wages of Sin", "multiplayer_overlays", "FM_Tat_M_019", "FM_Tat_F_019", 1550),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Dragon T", "multiplayer_overlays", "FM_Tat_M_020", "FM_Tat_F_020", 1500),
                new BusinessTattoo(new List<int>(){ 0, 1, 2, 8 }, "Flaming Cross", "multiplayer_overlays", "FM_Tat_M_024", "FM_Tat_F_024", 2475),
                new BusinessTattoo(new List<int>(){ 0 }, "LS Bold", "multiplayer_overlays", "FM_Tat_M_025", "FM_Tat_F_025", 2700),
                new BusinessTattoo(new List<int>(){ 2, 8 }, "Trinity Knot", "multiplayer_overlays", "FM_Tat_M_029", "FM_Tat_F_029", 1210),
                new BusinessTattoo(new List<int>(){ 5, 6 }, "Lucky Celtic Dogs", "multiplayer_overlays", "FM_Tat_M_030", "FM_Tat_F_030", 3050),
                new BusinessTattoo(new List<int>(){ 1 }, "Flaming Shamrock", "multiplayer_overlays", "FM_Tat_M_034", "FM_Tat_F_034", 370),
                new BusinessTattoo(new List<int>(){ 2 }, "Way of the Gun", "multiplayer_overlays", "FM_Tat_M_036", "FM_Tat_F_036", 4500),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Stone Cross", "multiplayer_overlays", "FM_Tat_M_044", "FM_Tat_F_044", 4050),
                new BusinessTattoo(new List<int>(){ 3, 4, 5, 6 }, "Skulls and Rose", "multiplayer_overlays", "FM_Tat_M_045", "FM_Tat_F_045", 1550),
            },

            // Head
            new List<BusinessTattoo>(){
	            // Передняя шея -   0
                // Левая шея    -   1
                // Правая шея   -   2
                // Задняя шея   -   3
	            // Левая щека - 4
                // Правая щека - 5

                new BusinessTattoo(new List<int>(){ 0 }, "Cash is King", "mpbusiness_overlays", "MP_Buis_M_Neck_000", String.Empty, 2475),
                new BusinessTattoo(new List<int>(){ 1 }, "Bold Dollar Sign", "mpbusiness_overlays", "MP_Buis_M_Neck_001", String.Empty, 2475),
                new BusinessTattoo(new List<int>(){ 2 }, "Script Dollar Sign", "mpbusiness_overlays", "MP_Buis_M_Neck_002", String.Empty, 2475),
                new BusinessTattoo(new List<int>(){ 3 }, "$100", "mpbusiness_overlays", "MP_Buis_M_Neck_003", String.Empty, 2475),
                new BusinessTattoo(new List<int>(){ 1 }, "Val-de-Grace Logo", "mpbusiness_overlays", String.Empty, "MP_Buis_F_Neck_000", 2475),
                new BusinessTattoo(new List<int>(){ 2 }, "Money Rose", "mpbusiness_overlays", String.Empty, "MP_Buis_F_Neck_001", 2475),
                new BusinessTattoo(new List<int>(){ 2 }, "Los Muertos", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_007", "MP_Xmas2_F_Tat_007", 2475),
                new BusinessTattoo(new List<int>(){ 1 }, "Snake Head Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_025", "MP_Xmas2_F_Tat_025", 2475),
                new BusinessTattoo(new List<int>(){ 2 }, "Beautiful Death", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_029", "MP_Xmas2_F_Tat_029", 2475),
                new BusinessTattoo(new List<int>(){ 1 }, "Lock & Load", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_003_M", "MP_Gunrunning_Tattoo_003_F", 2475),
                new BusinessTattoo(new List<int>(){ 2 }, "Beautiful Eye", "mphipster_overlays", "FM_Hip_M_Tat_005", "FM_Hip_F_Tat_005", 2475),
                new BusinessTattoo(new List<int>(){ 1 }, "Geo Fox", "mphipster_overlays", "FM_Hip_M_Tat_021", "FM_Hip_F_Tat_021", 2475),
                new BusinessTattoo(new List<int>(){ 5 }, "Morbid Arachnid", "mpbiker_overlays", "MP_MP_Biker_Tat_009_M", "MP_MP_Biker_Tat_009_F", 2475),
                new BusinessTattoo(new List<int>(){ 2 }, "FTW", "mpbiker_overlays", "MP_MP_Biker_Tat_038_M", "MP_MP_Biker_Tat_038_F", 2475),
                new BusinessTattoo(new List<int>(){ 1 }, "Western Stylized", "mpbiker_overlays", "MP_MP_Biker_Tat_051_M", "MP_MP_Biker_Tat_051_F", 2475),
                new BusinessTattoo(new List<int>(){ 1 }, "Sinner", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_011_M", "MP_Smuggler_Tattoo_011_F", 2475),
                new BusinessTattoo(new List<int>(){ 2 }, "Thief", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_012_M", "MP_Smuggler_Tattoo_012_F", 2475),
                new BusinessTattoo(new List<int>(){ 1 }, "Stunt Skull", "mpstunt_overlays", "MP_MP_Stunt_Tat_000_M", "MP_MP_Stunt_Tat_000_F", 2475),
                new BusinessTattoo(new List<int>(){ 5 }, "Scorpion", "mpstunt_overlays", "MP_MP_Stunt_Tat_004_M", "MP_MP_Stunt_Tat_004_F", 300),
                new BusinessTattoo(new List<int>(){ 2 }, "Toxic Spider", "mpstunt_overlays", "MP_MP_Stunt_Tat_006_M", "MP_MP_Stunt_Tat_006_F", 300),
                new BusinessTattoo(new List<int>(){ 2 }, "Bat Wheel", "mpstunt_overlays", "MP_MP_Stunt_Tat_017_M", "MP_MP_Stunt_Tat_017_F", 300),
                new BusinessTattoo(new List<int>(){ 2 }, "Flaming Quad", "mpstunt_overlays", "MP_MP_Stunt_Tat_042_M", "MP_MP_Stunt_Tat_042_F", 2475),
            },

            // Left Arm
            new List<BusinessTattoo>()
            {
                // Кисть        -   0
                // До локтя     -   1
                // Выше локтя   -   2

                new BusinessTattoo(new List<int>(){ 1 }, "$100 Bill", "mpbusiness_overlays", "MP_Buis_M_LeftArm_000", String.Empty, 2725),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "All-Seeing Eye", "mpbusiness_overlays", "MP_Buis_M_LeftArm_001", String.Empty, 990),
                new BusinessTattoo(new List<int>(){ 1 }, "Greed is Good", "mpbusiness_overlays", String.Empty, "MP_Buis_F_LArm_000", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Skull Rider", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_000", "MP_Xmas2_F_Tat_000", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Electric Snake", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_010", "MP_Xmas2_F_Tat_010", 2700),
                new BusinessTattoo(new List<int>(){ 2 }, "8 Ball Skull", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_012", "MP_Xmas2_F_Tat_012", 2750),
                new BusinessTattoo(new List<int>(){ 0 }, "Time's Up Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_020", "MP_Xmas2_F_Tat_020", 1850),
                new BusinessTattoo(new List<int>(){ 0 }, "Time's Up Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_021", "MP_Xmas2_F_Tat_021", 1850),
                new BusinessTattoo(new List<int>(){ 0 }, "Sidearm", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_004_M", "MP_Gunrunning_Tattoo_004_F", 335),
                new BusinessTattoo(new List<int>(){ 2 }, "Bandolier", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_008_M", "MP_Gunrunning_Tattoo_008_F", 2490),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Spiked Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_015_M", "MP_Gunrunning_Tattoo_015_F", 980),
                new BusinessTattoo(new List<int>(){ 2 }, "Blood Money", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_016_M", "MP_Gunrunning_Tattoo_016_F", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Praying Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_025_M", "MP_Gunrunning_Tattoo_025_F", 2700),
                new BusinessTattoo(new List<int>(){ 2 }, "Serpent Revolver", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_027_M", "MP_Gunrunning_Tattoo_027_F", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Diamond Sparkle", "mphipster_overlays", "FM_Hip_M_Tat_003", "FM_Hip_F_Tat_003", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "Bricks", "mphipster_overlays", "FM_Hip_M_Tat_007", "FM_Hip_F_Tat_007", 1850),
                new BusinessTattoo(new List<int>(){ 2 }, "Mustache", "mphipster_overlays", "FM_Hip_M_Tat_015", "FM_Hip_F_Tat_015", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Lightning Bolt", "mphipster_overlays", "FM_Hip_M_Tat_016", "FM_Hip_F_Tat_016", 2700),
                new BusinessTattoo(new List<int>(){ 2 }, "Pizza", "mphipster_overlays", "FM_Hip_M_Tat_026", "FM_Hip_F_Tat_026", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Padlock", "mphipster_overlays", "FM_Hip_M_Tat_027", "FM_Hip_F_Tat_027", 3000),
                new BusinessTattoo(new List<int>(){ 1 }, "Thorny Rose", "mphipster_overlays", "FM_Hip_M_Tat_028", "FM_Hip_F_Tat_028", 3000),
                new BusinessTattoo(new List<int>(){ 0 }, "Stop", "mphipster_overlays", "FM_Hip_M_Tat_034", "FM_Hip_F_Tat_034", 1825),
                new BusinessTattoo(new List<int>(){ 2 }, "Sunrise", "mphipster_overlays", "FM_Hip_M_Tat_037", "FM_Hip_F_Tat_037", 2725),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Sleeve", "mphipster_overlays", "FM_Hip_M_Tat_039", "FM_Hip_F_Tat_039", 1250),
                new BusinessTattoo(new List<int>(){ 2 }, "Triangle White", "mphipster_overlays", "FM_Hip_M_Tat_043", "FM_Hip_F_Tat_043", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Peace", "mphipster_overlays", "FM_Hip_M_Tat_048", "FM_Hip_F_Tat_048", 1850),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Piston Sleeve", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_004_M", "MP_MP_ImportExport_Tat_004_F", 980),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Scarlett", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_008_M", "MP_MP_ImportExport_Tat_008_F", 5475),
                new BusinessTattoo(new List<int>(){ 1 }, "No Evil", "mplowrider_overlays", "MP_LR_Tat_005_M", "MP_LR_Tat_005_F", 2490),
                new BusinessTattoo(new List<int>(){ 2 }, "Los Santos Life", "mplowrider_overlays", "MP_LR_Tat_027_M", "MP_LR_Tat_027_F", 2700),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "City Sorrow", "mplowrider_overlays", "MP_LR_Tat_033_M", "MP_LR_Tat_033_F", 980),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Toxic Trails", "mpairraces_overlays", "MP_Airraces_Tattoo_003_M", "MP_Airraces_Tattoo_003_F", 4570),
                new BusinessTattoo(new List<int>(){ 1 }, "Urban Stunter", "mpbiker_overlays", "MP_MP_Biker_Tat_012_M", "MP_MP_Biker_Tat_012_F", 2725),
                new BusinessTattoo(new List<int>(){ 2 }, "Macabre Tree", "mpbiker_overlays", "MP_MP_Biker_Tat_016_M", "MP_MP_Biker_Tat_016_F", 3000),
                new BusinessTattoo(new List<int>(){ 2 }, "Cranial Rose", "mpbiker_overlays", "MP_MP_Biker_Tat_020_M", "MP_MP_Biker_Tat_020_F", 2700),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Live to Ride", "mpbiker_overlays", "MP_MP_Biker_Tat_024_M", "MP_MP_Biker_Tat_024_F", 980),
                new BusinessTattoo(new List<int>(){ 2 }, "Good Luck", "mpbiker_overlays", "MP_MP_Biker_Tat_025_M", "MP_MP_Biker_Tat_025_F", 1550),
                new BusinessTattoo(new List<int>(){ 2 }, "Chain Fist", "mpbiker_overlays", "MP_MP_Biker_Tat_035_M", "MP_MP_Biker_Tat_035_F", 360),
                new BusinessTattoo(new List<int>(){ 2 }, "Ride Hard Die Fast", "mpbiker_overlays", "MP_MP_Biker_Tat_045_M", "MP_MP_Biker_Tat_045_F", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Muffler Helmet", "mpbiker_overlays", "MP_MP_Biker_Tat_053_M", "MP_MP_Biker_Tat_053_F", 2725),
                new BusinessTattoo(new List<int>(){ 2 }, "Poison Scorpion", "mpbiker_overlays", "MP_MP_Biker_Tat_055_M", "MP_MP_Biker_Tat_055_F", 2700),
                new BusinessTattoo(new List<int>(){ 2 }, "Love Hustle", "mplowrider2_overlays", "MP_LR_Tat_006_M", "MP_LR_Tat_006_F", 2700),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Skeleton Party", "mplowrider2_overlays", "MP_LR_Tat_018_M", "MP_LR_Tat_018_F", 2770),
                new BusinessTattoo(new List<int>(){ 1 }, "My Crazy Life", "mplowrider2_overlays", "MP_LR_Tat_022_M", "MP_LR_Tat_022_F", 2725),
                new BusinessTattoo(new List<int>(){ 2 }, "Archangel & Mary", "mpluxe_overlays", "MP_LUXE_TAT_020_M", "MP_LUXE_TAT_020_F", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Gabriel", "mpluxe_overlays", "MP_LUXE_TAT_021_M", "MP_LUXE_TAT_021_F", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Fatal Dagger", "mpluxe2_overlays", "MP_LUXE_TAT_005_M", "MP_LUXE_TAT_005_F", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Egyptian Mural", "mpluxe2_overlays", "MP_LUXE_TAT_016_M", "MP_LUXE_TAT_016_F", 2490),
                new BusinessTattoo(new List<int>(){ 2 }, "Divine Goddess", "mpluxe2_overlays", "MP_LUXE_TAT_018_M", "MP_LUXE_TAT_018_F", 2490),
                new BusinessTattoo(new List<int>(){ 1 }, "Python Skull", "mpluxe2_overlays", "MP_LUXE_TAT_028_M", "MP_LUXE_TAT_028_F", 925),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Geometric Design LA", "mpluxe2_overlays", "MP_LUXE_TAT_031_M", "MP_LUXE_TAT_031_F", 980),
                new BusinessTattoo(new List<int>(){ 1 }, "Honor", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_004_M", "MP_Smuggler_Tattoo_004_F", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Horrors Of The Deep", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_008_M", "MP_Smuggler_Tattoo_008_F", 2725),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Mermaid's Curse", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_014_M", "MP_Smuggler_Tattoo_014_F", 980),
                new BusinessTattoo(new List<int>(){ 2 }, "8 Eyed Skull", "mpstunt_overlays", "MP_MP_Stunt_Tat_001_M", "MP_MP_Stunt_Tat_001_F", 2475),
                new BusinessTattoo(new List<int>(){ 0 }, "Big Cat", "mpstunt_overlays", "MP_MP_Stunt_Tat_002_M", "MP_MP_Stunt_Tat_002_F", 1825),
                new BusinessTattoo(new List<int>(){ 2 }, "Moonlight Ride", "mpstunt_overlays", "MP_MP_Stunt_Tat_008_M", "MP_MP_Stunt_Tat_008_F", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Piston Head", "mpstunt_overlays", "MP_MP_Stunt_Tat_022_M", "MP_MP_Stunt_Tat_022_F", 2700),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Tanked", "mpstunt_overlays", "MP_MP_Stunt_Tat_023_M", "MP_MP_Stunt_Tat_023_F", 5475),
                new BusinessTattoo(new List<int>(){ 1 }, "Stuntman's End", "mpstunt_overlays", "MP_MP_Stunt_Tat_035_M", "MP_MP_Stunt_Tat_035_F", 2700),
                new BusinessTattoo(new List<int>(){ 2 }, "Kaboom", "mpstunt_overlays", "MP_MP_Stunt_Tat_039_M", "MP_MP_Stunt_Tat_039_F", 2725),
                new BusinessTattoo(new List<int>(){ 2 }, "Engine Arm", "mpstunt_overlays", "MP_MP_Stunt_Tat_043_M", "MP_MP_Stunt_Tat_043_F", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Burning Heart", "multiplayer_overlays", "FM_Tat_Award_M_001", "FM_Tat_Award_F_001", 2725),
                new BusinessTattoo(new List<int>(){ 2 }, "Racing Blonde", "multiplayer_overlays", "FM_Tat_Award_M_007", "FM_Tat_Award_F_007", 2725),
                new BusinessTattoo(new List<int>(){ 2 }, "Racing Brunette", "multiplayer_overlays", "FM_Tat_Award_M_015", "FM_Tat_Award_F_015", 2725),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Serpents", "multiplayer_overlays", "FM_Tat_M_005", "FM_Tat_F_005", 2490),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Oriental Mural", "multiplayer_overlays", "FM_Tat_M_006", "FM_Tat_F_006", 980),
                new BusinessTattoo(new List<int>(){ 2 }, "Zodiac Skull", "multiplayer_overlays", "FM_Tat_M_015", "FM_Tat_F_015", 2700),
                new BusinessTattoo(new List<int>(){ 2 }, "Lady M", "multiplayer_overlays", "FM_Tat_M_031", "FM_Tat_F_031", 2725),
                new BusinessTattoo(new List<int>(){ 2 }, "Dope Skull", "multiplayer_overlays", "FM_Tat_M_041", "FM_Tat_F_041", 2700),
            },
            
            // RightArm
            new List<BusinessTattoo>()
            {
                // Кисть        -   0
                // До локтя     -   1
                // Выше локтя   -   2

                new BusinessTattoo(new List<int>(){ 2 }, "Dollar Skull", "mpbusiness_overlays", "MP_Buis_M_RightArm_000", String.Empty, 2490),
                new BusinessTattoo(new List<int>(){ 1 }, "Green", "mpbusiness_overlays", "MP_Buis_M_RightArm_001", String.Empty, 2490),
                new BusinessTattoo(new List<int>(){ 1 }, "Dollar Sign", "mpbusiness_overlays", String.Empty, "MP_Buis_F_RArm_000", 2700),
                new BusinessTattoo(new List<int>(){ 2 }, "Snake Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_003", "MP_Xmas2_F_Tat_003", 2490),
                new BusinessTattoo(new List<int>(){ 2 }, "Snake Shaded", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_004", "MP_Xmas2_F_Tat_004", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Death Before Dishonor", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_008", "MP_Xmas2_F_Tat_008", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "You're Next Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_022", "MP_Xmas2_F_Tat_022", 300),
                new BusinessTattoo(new List<int>(){ 1 }, "You're Next Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_023", "MP_Xmas2_F_Tat_023", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "Fuck Luck Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_026", "MP_Xmas2_F_Tat_026", 1825),
                new BusinessTattoo(new List<int>(){ 0 }, "Fuck Luck Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_027", "MP_Xmas2_F_Tat_027", 1825),
                new BusinessTattoo(new List<int>(){ 0 }, "Grenade", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_002_M", "MP_Gunrunning_Tattoo_002_F", 1825),
                new BusinessTattoo(new List<int>(){ 2 }, "Have a Nice Day", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_021_M", "MP_Gunrunning_Tattoo_021_F", 2490),
                new BusinessTattoo(new List<int>(){ 1 }, "Combat Reaper", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_024_M", "MP_Gunrunning_Tattoo_024_F", 2725),
                new BusinessTattoo(new List<int>(){ 2 }, "Single Arrow", "mphipster_overlays", "FM_Hip_M_Tat_001", "FM_Hip_F_Tat_001", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Bone", "mphipster_overlays", "FM_Hip_M_Tat_004", "FM_Hip_F_Tat_004", 2700),
                new BusinessTattoo(new List<int>(){ 2 }, "Cube", "mphipster_overlays", "FM_Hip_M_Tat_008", "FM_Hip_F_Tat_008", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "Horseshoe", "mphipster_overlays", "FM_Hip_M_Tat_010", "FM_Hip_F_Tat_010", 1825),
                new BusinessTattoo(new List<int>(){ 1 }, "Spray Can", "mphipster_overlays", "FM_Hip_M_Tat_014", "FM_Hip_F_Tat_014", 1800),
                new BusinessTattoo(new List<int>(){ 1 }, "Eye Triangle", "mphipster_overlays", "FM_Hip_M_Tat_017", "FM_Hip_F_Tat_017", 1825),
                new BusinessTattoo(new List<int>(){ 1 }, "Origami", "mphipster_overlays", "FM_Hip_M_Tat_018", "FM_Hip_F_Tat_018", 2700),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Geo Pattern", "mphipster_overlays", "FM_Hip_M_Tat_020", "FM_Hip_F_Tat_020", 980),
                new BusinessTattoo(new List<int>(){ 1 }, "Pencil", "mphipster_overlays", "FM_Hip_M_Tat_022", "FM_Hip_F_Tat_022", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "Smiley", "mphipster_overlays", "FM_Hip_M_Tat_023", "FM_Hip_F_Tat_023", 1850),
                new BusinessTattoo(new List<int>(){ 2 }, "Shapes", "mphipster_overlays", "FM_Hip_M_Tat_036", "FM_Hip_F_Tat_036",2700),
                new BusinessTattoo(new List<int>(){ 2 }, "Triangle Black", "mphipster_overlays", "FM_Hip_M_Tat_044", "FM_Hip_F_Tat_044",2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Mesh Band", "mphipster_overlays", "FM_Hip_M_Tat_045", "FM_Hip_F_Tat_045", 2725),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Mechanical Sleeve", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_003_M", "MP_MP_ImportExport_Tat_003_F", 980),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Dialed In", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_005_M", "MP_MP_ImportExport_Tat_005_F", 985),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Engulfed Block", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_006_M", "MP_MP_ImportExport_Tat_006_F", 980),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Drive Forever", "mpimportexport_overlays", "MP_MP_ImportExport_Tat_007_M", "MP_MP_ImportExport_Tat_007_F", 980),
                new BusinessTattoo(new List<int>(){ 1 }, "Seductress", "mplowrider_overlays", "MP_LR_Tat_015_M", "MP_LR_Tat_015_F", 398),
                new BusinessTattoo(new List<int>(){ 2 }, "Swooping Eagle", "mpbiker_overlays", "MP_MP_Biker_Tat_007_M", "MP_MP_Biker_Tat_007_F", 2700),
                new BusinessTattoo(new List<int>(){ 2 }, "Lady Mortality", "mpbiker_overlays", "MP_MP_Biker_Tat_014_M", "MP_MP_Biker_Tat_014_F", 2725),
                new BusinessTattoo(new List<int>(){ 2 }, "Eagle Emblem", "mpbiker_overlays", "MP_MP_Biker_Tat_033_M", "MP_MP_Biker_Tat_033_F", 398),
                new BusinessTattoo(new List<int>(){ 1 }, "Grim Rider", "mpbiker_overlays", "MP_MP_Biker_Tat_042_M", "MP_MP_Biker_Tat_042_F", 2725),
                new BusinessTattoo(new List<int>(){ 2 }, "Skull Chain", "mpbiker_overlays", "MP_MP_Biker_Tat_046_M", "MP_MP_Biker_Tat_046_F", 2700),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Snake Bike", "mpbiker_overlays", "MP_MP_Biker_Tat_047_M", "MP_MP_Biker_Tat_047_F", 980),
                new BusinessTattoo(new List<int>(){ 2 }, "These Colors Don't Run", "mpbiker_overlays", "MP_MP_Biker_Tat_049_M", "MP_MP_Biker_Tat_049_F", 2700),
                new BusinessTattoo(new List<int>(){ 2 }, "Mum", "mpbiker_overlays", "MP_MP_Biker_Tat_054_M", "MP_MP_Biker_Tat_054_F", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Lady Vamp", "mplowrider2_overlays", "MP_LR_Tat_003_M", "MP_LR_Tat_003_F", 2490),
                new BusinessTattoo(new List<int>(){ 2 }, "Loving Los Muertos", "mplowrider2_overlays", "MP_LR_Tat_028_M", "MP_LR_Tat_028_F", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Black Tears", "mplowrider2_overlays", "MP_LR_Tat_035_M", "MP_LR_Tat_035_F", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Floral Raven", "mpluxe_overlays", "MP_LUXE_TAT_004_M", "MP_LUXE_TAT_004_F", 2700),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Mermaid Harpist", "mpluxe_overlays", "MP_LUXE_TAT_013_M", "MP_LUXE_TAT_013_F", 980),
                new BusinessTattoo(new List<int>(){ 2 }, "Geisha Bloom", "mpluxe_overlays", "MP_LUXE_TAT_019_M", "MP_LUXE_TAT_019_F", 2490),
                new BusinessTattoo(new List<int>(){ 1 }, "Intrometric", "mpluxe2_overlays", "MP_LUXE_TAT_010_M", "MP_LUXE_TAT_010_F", 2490),
                new BusinessTattoo(new List<int>(){ 2 }, "Heavenly Deity", "mpluxe2_overlays", "MP_LUXE_TAT_017_M", "MP_LUXE_TAT_017_F", 2475),
                new BusinessTattoo(new List<int>(){ 2 }, "Floral Print", "mpluxe2_overlays", "MP_LUXE_TAT_026_M", "MP_LUXE_TAT_026_F", 2700),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Geometric Design RA", "mpluxe2_overlays", "MP_LUXE_TAT_030_M", "MP_LUXE_TAT_030_F", 980),
                new BusinessTattoo(new List<int>(){ 1 }, "Crackshot", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_001_M", "MP_Smuggler_Tattoo_001_F", 2700),
                new BusinessTattoo(new List<int>(){ 2 }, "Mutiny", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_005_M", "MP_Smuggler_Tattoo_005_F", 398),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Stylized Kraken", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_023_M", "MP_Smuggler_Tattoo_023_F", 980),
                new BusinessTattoo(new List<int>(){ 1 }, "Poison Wrench", "mpstunt_overlays", "MP_MP_Stunt_Tat_003_M", "MP_MP_Stunt_Tat_003_F", 2475),
                new BusinessTattoo(new List<int>(){ 2 }, "Arachnid of Death", "mpstunt_overlays", "MP_MP_Stunt_Tat_009_M", "MP_MP_Stunt_Tat_009_F", 2725),
                new BusinessTattoo(new List<int>(){ 2 }, "Grave Vulture", "mpstunt_overlays", "MP_MP_Stunt_Tat_010_M", "MP_MP_Stunt_Tat_010_F", 2490),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Coffin Racer", "mpstunt_overlays", "MP_MP_Stunt_Tat_016_M", "MP_MP_Stunt_Tat_016_F", 980),
                new BusinessTattoo(new List<int>(){ 0 }, "Biker Stallion", "mpstunt_overlays", "MP_MP_Stunt_Tat_036_M", "MP_MP_Stunt_Tat_036_F", 1825),
                new BusinessTattoo(new List<int>(){ 1 }, "One Down Five Up", "mpstunt_overlays", "MP_MP_Stunt_Tat_038_M", "MP_MP_Stunt_Tat_038_F", 2725),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Seductive Mechanic", "mpstunt_overlays", "MP_MP_Stunt_Tat_049_M", "MP_MP_Stunt_Tat_049_F", 980),
                new BusinessTattoo(new List<int>(){ 2 }, "Grim Reaper Smoking Gun", "multiplayer_overlays", "FM_Tat_Award_M_002", "FM_Tat_Award_F_002", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Ride or Die RA", "multiplayer_overlays", "FM_Tat_Award_M_010", "FM_Tat_Award_F_010", 2700),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Brotherhood", "multiplayer_overlays", "FM_Tat_M_000", "FM_Tat_F_000", 980),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Dragons", "multiplayer_overlays", "FM_Tat_M_001", "FM_Tat_F_001", 980),
                new BusinessTattoo(new List<int>(){ 2 }, "Dragons and Skull", "multiplayer_overlays", "FM_Tat_M_003", "FM_Tat_F_003", 2725),
                new BusinessTattoo(new List<int>(){ 1, 2 }, "Flower Mural", "multiplayer_overlays", "FM_Tat_M_014", "FM_Tat_F_014", 980),
                new BusinessTattoo(new List<int>(){ 1, 2, 0 }, "Serpent Skull RA", "multiplayer_overlays", "FM_Tat_M_018", "FM_Tat_F_018", 1250),
                new BusinessTattoo(new List<int>(){ 2 }, "Virgin Mary", "multiplayer_overlays", "FM_Tat_M_027", "FM_Tat_F_027", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Mermaid", "multiplayer_overlays", "FM_Tat_M_028", "FM_Tat_F_028", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Dagger", "multiplayer_overlays", "FM_Tat_M_038", "FM_Tat_F_038", 2700),
                new BusinessTattoo(new List<int>(){ 2 }, "Lion", "multiplayer_overlays", "FM_Tat_M_047", "FM_Tat_F_047", 2700),
            },

            // LeftLeg
            new List<BusinessTattoo>()
            {
	            // До колена    -   0
                // Выше колена  -   1

                new BusinessTattoo(new List<int>(){ 0 }, "Single", "mpbusiness_overlays", String.Empty, "MP_Buis_F_LLeg_000", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Spider Outline", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_001", "MP_Xmas2_F_Tat_001", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Spider Color", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_002", "MP_Xmas2_F_Tat_002", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Patriot Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_005_M", "MP_Gunrunning_Tattoo_005_F", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Stylized Tiger", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_007_M", "MP_Gunrunning_Tattoo_007_F", 2700),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Death Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_011_M", "MP_Gunrunning_Tattoo_011_F", 950),
                new BusinessTattoo(new List<int>(){ 1 }, "Rose Revolver", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_023_M", "MP_Gunrunning_Tattoo_023_F", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Squares", "mphipster_overlays", "FM_Hip_M_Tat_009", "FM_Hip_F_Tat_009", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "Charm", "mphipster_overlays", "FM_Hip_M_Tat_019", "FM_Hip_F_Tat_019", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Black Anchor", "mphipster_overlays", "FM_Hip_M_Tat_040", "FM_Hip_F_Tat_040", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "LS Serpent", "mplowrider_overlays", "MP_LR_Tat_007_M", "MP_LR_Tat_007_F", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Presidents", "mplowrider_overlays", "MP_LR_Tat_020_M", "MP_LR_Tat_020_F", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "Rose Tribute", "mpbiker_overlays", "MP_MP_Biker_Tat_002_M", "MP_MP_Biker_Tat_002_F", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Ride or Die LL", "mpbiker_overlays", "MP_MP_Biker_Tat_015_M", "MP_MP_Biker_Tat_015_F", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "Bad Luck", "mpbiker_overlays", "MP_MP_Biker_Tat_027_M", "MP_MP_Biker_Tat_027_F", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Engulfed Skull", "mpbiker_overlays", "MP_MP_Biker_Tat_036_M", "MP_MP_Biker_Tat_036_F", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Scorched Soul", "mpbiker_overlays", "MP_MP_Biker_Tat_037_M", "MP_MP_Biker_Tat_037_F", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Ride Free", "mpbiker_overlays", "MP_MP_Biker_Tat_044_M", "MP_MP_Biker_Tat_044_F", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Bone Cruiser", "mpbiker_overlays", "MP_MP_Biker_Tat_056_M", "MP_MP_Biker_Tat_056_F", 2725),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Laughing Skull", "mpbiker_overlays", "MP_MP_Biker_Tat_057_M", "MP_MP_Biker_Tat_057_F", 950),
                new BusinessTattoo(new List<int>(){ 0 }, "Death Us Do Part", "mplowrider2_overlays", "MP_LR_Tat_029_M", "MP_LR_Tat_029_F", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Serpent of Death", "mpluxe_overlays", "MP_LUXE_TAT_000_M", "MP_LUXE_TAT_000_F", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Cross of Roses", "mpluxe2_overlays", "MP_LUXE_TAT_011_M", "MP_LUXE_TAT_011_F", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Dagger Devil", "mpstunt_overlays", "MP_MP_Stunt_Tat_007_M", "MP_MP_Stunt_Tat_007_F", 2490),
                new BusinessTattoo(new List<int>(){ 1 }, "Dirt Track Hero", "mpstunt_overlays", "MP_MP_Stunt_Tat_013_M", "MP_MP_Stunt_Tat_013_F", 2700),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Golden Cobra", "mpstunt_overlays", "MP_MP_Stunt_Tat_021_M", "MP_MP_Stunt_Tat_021_F", 950),
                new BusinessTattoo(new List<int>(){ 0 }, "Quad Goblin", "mpstunt_overlays", "MP_MP_Stunt_Tat_028_M", "MP_MP_Stunt_Tat_028_F", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "Stunt Jesus", "mpstunt_overlays", "MP_MP_Stunt_Tat_031_M", "MP_MP_Stunt_Tat_031_F", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Dragon and Dagger", "multiplayer_overlays", "FM_Tat_Award_M_009", "FM_Tat_Award_F_009", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Melting Skull", "multiplayer_overlays", "FM_Tat_M_002", "FM_Tat_F_002", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Dragon Mural", "multiplayer_overlays", "FM_Tat_M_008", "FM_Tat_F_008", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Serpent Skull LL", "multiplayer_overlays", "FM_Tat_M_021", "FM_Tat_F_021", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Hottie", "multiplayer_overlays", "FM_Tat_M_023", "FM_Tat_F_023", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Smoking Dagger", "multiplayer_overlays", "FM_Tat_M_026", "FM_Tat_F_026", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Faith LL", "multiplayer_overlays", "FM_Tat_M_032", "FM_Tat_F_032", 2725),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Chinese Dragon", "multiplayer_overlays", "FM_Tat_M_033", "FM_Tat_F_033", 950),
                new BusinessTattoo(new List<int>(){ 0 }, "Dragon LL", "multiplayer_overlays", "FM_Tat_M_035", "FM_Tat_F_035", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "Grim Reaper", "multiplayer_overlays", "FM_Tat_M_037", "FM_Tat_F_037", 2725),
            },
            
            // RightLeg
            new List<BusinessTattoo>()
            {
	            // До колена    -   0
                // Выше колена  -   1

                new BusinessTattoo(new List<int>(){ 0 }, "Diamond Crown", "mpbusiness_overlays", String.Empty, "MP_Buis_F_RLeg_000", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "Floral Dagger", "mpchristmas2_overlays", "MP_Xmas2_M_Tat_014", "MP_Xmas2_F_Tat_014", 2475),
                new BusinessTattoo(new List<int>(){ 0 }, "Combat Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_006_M", "MP_Gunrunning_Tattoo_006_F", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "Restless Skull", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_026_M", "MP_Gunrunning_Tattoo_026_F", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Pistol Ace", "mpgunrunning_overlays", "MP_Gunrunning_Tattoo_030_M", "MP_Gunrunning_Tattoo_030_F", 5285),
                new BusinessTattoo(new List<int>(){ 0 }, "Grub", "mphipster_overlays", "FM_Hip_M_Tat_038", "FM_Hip_F_Tat_038", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "Sparkplug", "mphipster_overlays", "FM_Hip_M_Tat_042", "FM_Hip_F_Tat_042", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "Ink Me", "mplowrider_overlays", "MP_LR_Tat_017_M", "MP_LR_Tat_017_F", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "Dance of Hearts", "mplowrider_overlays", "MP_LR_Tat_023_M", "MP_LR_Tat_023_F", 2725),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Dragon's Fury", "mpbiker_overlays", "MP_MP_Biker_Tat_004_M", "MP_MP_Biker_Tat_004_F", 950),
                new BusinessTattoo(new List<int>(){ 0 }, "Western Insignia", "mpbiker_overlays", "MP_MP_Biker_Tat_022_M", "MP_MP_Biker_Tat_022_F", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "Dusk Rider", "mpbiker_overlays", "MP_MP_Biker_Tat_028_M", "MP_MP_Biker_Tat_028_F", 2700),
                new BusinessTattoo(new List<int>(){ 1 }, "American Made", "mpbiker_overlays", "MP_MP_Biker_Tat_040_M", "MP_MP_Biker_Tat_040_F", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "STFU", "mpbiker_overlays", "MP_MP_Biker_Tat_048_M", "MP_MP_Biker_Tat_048_F", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "San Andreas Prayer", "mplowrider2_overlays", "MP_LR_Tat_030_M", "MP_LR_Tat_030_F", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Elaborate Los Muertos", "mpluxe_overlays", "MP_LUXE_TAT_001_M", "MP_LUXE_TAT_001_F", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Starmetric", "mpluxe2_overlays", "MP_LUXE_TAT_023_M", "MP_LUXE_TAT_023_F", 2475),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Homeward Bound", "mpsmuggler_overlays", "MP_Smuggler_Tattoo_020_M", "MP_Smuggler_Tattoo_020_F", 950),
                new BusinessTattoo(new List<int>(){ 0 }, "Demon Spark Plug", "mpstunt_overlays", "MP_MP_Stunt_Tat_005_M", "MP_MP_Stunt_Tat_005_F", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Praying Gloves", "mpstunt_overlays", "MP_MP_Stunt_Tat_015_M", "MP_MP_Stunt_Tat_015_F", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Piston Angel", "mpstunt_overlays", "MP_MP_Stunt_Tat_020_M", "MP_MP_Stunt_Tat_020_F", 2725),
                new BusinessTattoo(new List<int>(){ 1 }, "Speed Freak", "mpstunt_overlays", "MP_MP_Stunt_Tat_025_M", "MP_MP_Stunt_Tat_025_F", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "Wheelie Mouse", "mpstunt_overlays", "MP_MP_Stunt_Tat_032_M", "MP_MP_Stunt_Tat_032_F", 2475),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Severed Hand", "mpstunt_overlays", "MP_MP_Stunt_Tat_045_M", "MP_MP_Stunt_Tat_045_F", 950),
                new BusinessTattoo(new List<int>(){ 0 }, "Brake Knife", "mpstunt_overlays", "MP_MP_Stunt_Tat_047_M", "MP_MP_Stunt_Tat_047_F", 2475),
                new BusinessTattoo(new List<int>(){ 0 }, "Skull and Sword", "multiplayer_overlays", "FM_Tat_Award_M_006", "FM_Tat_Award_F_006", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "The Warrior", "multiplayer_overlays", "FM_Tat_M_007", "FM_Tat_F_007", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Tribal", "multiplayer_overlays", "FM_Tat_M_017", "FM_Tat_F_017", 2700),
                new BusinessTattoo(new List<int>(){ 0 }, "Fiery Dragon", "multiplayer_overlays", "FM_Tat_M_022", "FM_Tat_F_022", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Broken Skull", "multiplayer_overlays", "FM_Tat_M_039", "FM_Tat_F_039", 2725),
                new BusinessTattoo(new List<int>(){ 0, 1 }, "Flaming Skull", "multiplayer_overlays", "FM_Tat_M_040", "FM_Tat_F_040", 940),
                new BusinessTattoo(new List<int>(){ 0 }, "Flaming Scorpion", "multiplayer_overlays", "FM_Tat_M_042", "FM_Tat_F_042", 2725),
                new BusinessTattoo(new List<int>(){ 0 }, "Indian Ram", "multiplayer_overlays", "FM_Tat_M_043", "FM_Tat_F_043", 2725)
            }

        };

        public class BusinessTattoo
        {
            public List<int> Slots { get; set; }
            public string Name { get; set; }
            public string Dictionary { get; set; }
            public string MaleHash { get; set; }
            public string FemaleHash { get; set; }
            public int Price { get; set; }

            public BusinessTattoo(List<int> slots, string name, string dictionary, string malehash, string femalehash, int price)
            {
                Slots = slots;
                Name = name;
                Dictionary = dictionary;
                MaleHash = malehash;
                FemaleHash = femalehash;
                Price = price;
            }
        }

    }
}
