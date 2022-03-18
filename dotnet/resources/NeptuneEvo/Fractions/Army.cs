using System.Collections.Generic;
using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using NeptuneEVO.GUI;
using System;

namespace NeptuneEVO.Fractions
{
    class Army : Script
    {
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                NAPI.TextLabel.CreateTextLabel("~b~", new Vector3(-2347.958, 3268.936, 33.81076), 5f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);

                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(ArmyCheckpoints[0], 1, 2, 0));
                Cols[0].SetData("INTERACT", 34);
                Cols[0].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[0].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Открыть меню оружия"), new Vector3(ArmyCheckpoints[0].X, ArmyCheckpoints[0].Y, ArmyCheckpoints[0].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(ArmyCheckpoints[1], 1, 2, 0));
                Cols[1].SetData("INTERACT", 35);
                Cols[1].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[1].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Переодевалка"), new Vector3(ArmyCheckpoints[1].X, ArmyCheckpoints[1].Y, ArmyCheckpoints[1].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(2, NAPI.ColShape.CreateCylinderColShape(ArmyCheckpoints[2], 5, 6, 0));
                Cols[2].SetData("INTERACT", 36);
                Cols[2].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[2].OnEntityExitColShape += onEntityExitArmyMats;

                Cols.Add(3, NAPI.ColShape.CreateCylinderColShape(ArmyCheckpoints[3], 1, 2, 0));
                Cols[3].SetData("INTERACT", 25);
                Cols[3].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[3].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Лифт"), new Vector3(ArmyCheckpoints[3].X, ArmyCheckpoints[3].Y, ArmyCheckpoints[3].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(4, NAPI.ColShape.CreateCylinderColShape(ArmyCheckpoints[4], 1, 2, 0));
                Cols[4].SetData("INTERACT", 25);
                Cols[4].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[4].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Лифт"), new Vector3(ArmyCheckpoints[4].X, ArmyCheckpoints[4].Y, ArmyCheckpoints[4].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(5, NAPI.ColShape.CreateCylinderColShape(ArmyCheckpoints[5], 1, 2, 0));
                Cols[5].SetData("INTERACT", 60);
                Cols[5].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[5].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Оружейный склад"), new Vector3(ArmyCheckpoints[5].X, ArmyCheckpoints[5].Y, ArmyCheckpoints[5].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(6, NAPI.ColShape.CreateCylinderColShape(ArmyCheckpoints[6], 1, 2, 0));
                Cols[6].SetData("INTERACT", 530);
                Cols[6].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[6].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel("~h~~b~Авианосец", new Vector3(ArmyCheckpoints[6].X, ArmyCheckpoints[6].Y, ArmyCheckpoints[6].Z + 1f), 5F, 0.5F, 0, new Color(255, 255, 255), true, 0);

                Cols.Add(7, NAPI.ColShape.CreateCylinderColShape(ArmyCheckpoints[7], 1, 2, 0));
                Cols[7].SetData("INTERACT", 531);
                Cols[7].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[7].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel("~h~~b~База", new Vector3(ArmyCheckpoints[7].X, ArmyCheckpoints[7].Y, ArmyCheckpoints[7].Z + 1f), 5F, 0.5F, 0, new Color(255, 255, 255), true, 0);

                Cols.Add(8, NAPI.ColShape.CreateCylinderColShape(ArmyCheckpoints[8], 1, 2, 0));
                Cols[8].SetData("INTERACT", 532);
                Cols[8].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[8].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel("~h~~b~Авиа-Транспорт", new Vector3(ArmyCheckpoints[8].X, ArmyCheckpoints[8].Y, ArmyCheckpoints[8].Z + 1f), 5F, 0.5F, 0, new Color(255, 255, 255), true, 0);


                NAPI.Marker.CreateMarker(1, ArmyCheckpoints[0] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, ArmyCheckpoints[1] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, ArmyCheckpoints[2], new Vector3(), new Vector3(), 5f, new Color(155, 0, 0));
                NAPI.Marker.CreateMarker(1, ArmyCheckpoints[3] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, ArmyCheckpoints[4] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, ArmyCheckpoints[5] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));

                NAPI.Marker.CreateMarker(1, ArmyCheckpoints[6] - new Vector3(0, 0, 0.5f), new Vector3(), new Vector3(), 0.965f, new Color(0, 175, 250, 220), false, 0);
                NAPI.Marker.CreateMarker(27, ArmyCheckpoints[6] + new Vector3(0, 0, 0.14f), new Vector3(), new Vector3(), 1f, new Color(0, 175, 250, 220), false, 0);
                NAPI.Marker.CreateMarker(1, ArmyCheckpoints[7] - new Vector3(0, 0, 0.5f), new Vector3(), new Vector3(), 0.965f, new Color(0, 175, 250, 220), false, 0);
                NAPI.Marker.CreateMarker(27, ArmyCheckpoints[7] + new Vector3(0, 0, 0.14f), new Vector3(), new Vector3(), 1f, new Color(0, 175, 250, 220), false, 0);
                NAPI.Marker.CreateMarker(1, ArmyCheckpoints[8] - new Vector3(0, 0, 0.5f), new Vector3(), new Vector3(), 0.965f, new Color(0, 175, 250, 220), false, 0);
                NAPI.Marker.CreateMarker(27, ArmyCheckpoints[8] + new Vector3(0, 0, 0.14f), new Vector3(), new Vector3(), 1f, new Color(0, 175, 250, 220), false, 0);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        private static nLog Log = new nLog("Army");

        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        public static List<Vector3> ArmyCheckpoints = new List<Vector3>()
        {
            new Vector3(-2467.473, 3315.018, 32), // guns     0
            new Vector3(-2449.677, 3287.597, 32), // dressing room    1
            new Vector3(-108.0619, -2414.873, 5.000001), // army docks mats     2
            new Vector3(-2360.946, 3249.595, 31.81073), // army lift 1 floor     3
            new Vector3(-2360.66, 3249.115, 91.90369), // army lift 9 floor     4
            new Vector3(-2463.956, 3316.425, 32), // army stock    5
            new Vector3(-2349.9744, 3266.465, 31.69074), // AVIA TO    6
            new Vector3(3096.3071, -4701.3193, 11.153861), // AVIA FROM    7
            new Vector3(3090.9487, -4721.371, 26.158636), // SPAWN   8
        };
        public static Dictionary<string, int> ArmyRanks = new Dictionary<string, int>
        {
            { "lazer", 1 },
            { "strikeforce", 1 },
            { "hydra", 1 },
            { "besra", 1 },
        };

        public static List<string> ArmyVehicles = new List<string>
        {
            "lazer",
            "lazer",
            "lazer",
            "strikeforce",
            "strikeforce",
            "hydra",
            "hydra",
            "besra"
        };

        [RemoteEvent("garagearmy")]
        static void GarageArmy(Player player, int index)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].FractionID != 14) return;

                string hash = null;

                int i = index;
                int lvl = -1;
                foreach (var veh in ArmyRanks)
                {
                    if (veh.Key == ArmyVehicles[index])
                    {
                        if (veh.Value > Main.Players[player].FractionLVL)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вам нужен {veh.Value} ранг!", 5000);
                            return;
                        }
                        else
                        {
                            hash = veh.Key;
                            lvl = veh.Value;
                            break;
                        }
                    }
                }

                foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                    if (veh.HasData("ACCESS") && veh.GetData<string>("ACCESS") == "FRACTION" && veh.HasData("FRACTION") && veh.GetData<int>("FRACTION") == 14 && veh.NumberPlate == hash + "_" + i)
                    {
                        NAPI.Task.Run(() => { try { veh.Delete(); } catch { } });
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы эвакуировали воздушный транспорт!", 3000);
                        return;
                    }

                Vehicle vehicle = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(hash), new Vector3(3100.289, -4811.8276, 16.270914), 24.87f, 111, 111, hash + "_" + i);
                vehicle.SetData("ACCESS", "FRACTION");
                vehicle.SetData("FRACTION", 14);
                vehicle.SetData("MINRANK", lvl);
                vehicle.SetData("TYPE", Configs.FractionTypes[14]);

                player.SetIntoVehicle(vehicle, 0);

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы вызвали воздушный транспорт!", 3000);


            }
            catch { }
        }

        [RemoteEvent("armygun")]
        public static void callback_armyGuns(Player Player, int index)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.StunGun, "StunGun");
                        return;
                    case 1: //pistol
                        Fractions.Manager.giveGun(Player, Weapons.Hash.Pistol, "Pistol");
                        return;
                    case 2: //smg
                        Fractions.Manager.giveGun(Player, Weapons.Hash.SMG, "SMG");
                        return;
                    case 3: //pumpshotgun
                        Fractions.Manager.giveGun(Player, Weapons.Hash.PumpShotgun, "PumpShotgun");
                        return;
                    case 4:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.CombatPistol, "CombatPistol");
                        return;                   
                    case 5:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.AssaultSMG, "AssaultSMG");
                        return;
                    case 6:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.CombatMG, "CombatMG");
                        return;
                    case 7:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.AdvancedRifle, "AdvancedRifle");
                        return;
                    case 8:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.CompactRifle, "CompactRifle");
                        return;
                    case 9:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.HeavySniper, "HeavySniper");
                        return;
                    case 10:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.SniperRifle, "SniperRifle");
                        return;
                    case 11:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.BullpupShotgun, "BullpupShotgun");
                        return;
                    case 12:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.HeavyShotgun, "HeavyShotgun");
                        return;
                    case 13: //armor
                        if (!Manager.canGetWeapon(Player, "armor")) return;
                        if (Fractions.Stocks.fracStocks[14].Materials > Fractions.Manager.matsForArmor && nInventory.Find(Main.Players[Player].UUID, ItemType.BodyArmor) == null)
                        {
                            nInventory.Add(Player, new nItem(ItemType.BodyArmor, 1, 100.ToString()));
                            Fractions.Stocks.fracStocks[14].Materials -= Fractions.Manager.matsForArmor;
                            Fractions.Stocks.fracStocks[14].UpdateLabel();
                            GameLog.Stock(Main.Players[Player].FractionID, Main.Players[Player].UUID, "armor", 1, false);
                            Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили бронежилет", 3000);
                            Manager.FracLogs[Main.Players[Player].FractionID].Add(new List<object> { DateTime.Now.ToString("dd.MM.yyyy"), $"{DateTime.Now.Hour}:{(DateTime.Now.Minute < 10 ? "0" : "" )}{DateTime.Now.Minute}", Player.Name, "Бронежилет", 1, "скрафтил" });
                        }
                        return;
                    case 14:
                        if (!Manager.canGetWeapon(Player, "Medkits")) return;
                        if (Fractions.Stocks.fracStocks[14].Medkits == 0)
                        {
                            Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "На складе нет аптечек", 3000);
                            return;
                        }
                        var hItem = nInventory.Find(Main.Players[Player].UUID, ItemType.HealthKit);
                        if (hItem != null)
                        {
                            Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас уже есть аптечка", 3000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[14].Medkits--;
                        Fractions.Stocks.fracStocks[14].UpdateLabel();
                        nInventory.Add(Player, new nItem(ItemType.HealthKit, 1));
                        GameLog.Stock(Main.Players[Player].FractionID, Main.Players[Player].UUID, "medkit", 1, false);
                        Manager.FracLogs[Main.Players[Player].FractionID].Add(new List<object> { DateTime.Now.ToString("dd.MM.yyyy"), $"{DateTime.Now.Hour}:{(DateTime.Now.Minute < 10 ? "0" : "" )}{DateTime.Now.Minute}", Player.Name, "Аптечку", 1, "скрафтил" });
                        Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили аптечку", 3000);
                        return;
                    case 15:
                        Fractions.Manager.giveAmmo(Player, ItemType.PistolAmmo, 12);
                        return;
                    case 16:
                        Fractions.Manager.giveAmmo(Player, ItemType.SMGAmmo, 30);
                        return;
                    case 17:
                        Fractions.Manager.giveAmmo(Player, ItemType.RiflesAmmo, 30);
                        return;
                    case 18:
                        Fractions.Manager.giveAmmo(Player, ItemType.SniperAmmo, 5);
                        return;
                    case 19: // shotgun ammo
                        Fractions.Manager.giveAmmo(Player, ItemType.ShotgunsAmmo, 6);
                        return;
                }
            }
            catch (Exception e) { Log.Write("ArmyGun: " + e.Message, nLog.Type.Error); }
        }

        public static void interactPressed(Player player, int interact)
        {
            switch (interact)
            {
                case 34:
                    if (Main.Players[player].FractionID != 14)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не служите в армии", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[14].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    Trigger.PlayerEvent(player, "armyguns");
                    return;
                case 35:
                    if (Main.Players[player].FractionID != 14)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не служите в армии", 3000);
                        return;
                    }
                    OpenArmyClothesMenu(player);
                    return;
                case 36:
                    if (Main.Players[player].FractionID != 14)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не служите в армии", 3000);
                        return;
                    }
                    if (!player.IsInVehicle)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в машине", 3000);
                        return;
                    }
                    if (!player.Vehicle.HasData("CANMATS"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На этой машине нельзя перевозить маты", 3000);
                        return;
                    }
                    if (player.HasData("loadMatsTimer"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже загружаете материалы в машину", 3000);
                        return;
                    }
                    if (!Fractions.Stocks.maxMats.ContainsKey(player.Vehicle.DisplayName)) return;
                    var count = VehicleInventory.GetCountOfType(player.Vehicle, ItemType.Material);
                    if (count >= Fractions.Stocks.maxMats[player.Vehicle.DisplayName])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В машине максимальное кол-во материала", 3000);
                        return;
                    }
                    //player.SetData("loadMatsTimer", Main.StartT(20000, 99999999, (o) => loadMaterialsTimer(player), "ALOADMATS_TIMER"));
                    player.SetData("loadMatsTimer", Timers.StartOnce(20000, () => loadMaterialsTimer(player)));
                    player.Vehicle.SetData("loaderMats", player);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Загрузка материалов началась (20 секунд)", 3000);
                    Trigger.PlayerEvent(player, "showLoader", "Загрузка материалов", 1);
                    player.SetData("vehicleMats", player.Vehicle);
                    player.SetData("whereLoad", "ARMY");
                    return;
                case 25:
                    if (player.IsInVehicle) return;
                    if (player.HasData("FOLLOWING"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                        return;
                    }
                    if (player.Position.Z > 50)
                    {
                        NAPI.Entity.SetEntityPosition(player, new Vector3(ArmyCheckpoints[3].X, ArmyCheckpoints[3].Y, ArmyCheckpoints[3].Z + 1));
                        Main.PlayerEnterInterior(player, new Vector3(ArmyCheckpoints[3].X, ArmyCheckpoints[3].Y, ArmyCheckpoints[3].Z + 1));
                    }
                    else
                    {
                        NAPI.Entity.SetEntityPosition(player, new Vector3(ArmyCheckpoints[4].X, ArmyCheckpoints[4].Y, ArmyCheckpoints[4].Z + 1));
                        Main.PlayerEnterInterior(player, new Vector3(ArmyCheckpoints[4].X, ArmyCheckpoints[4].Y, ArmyCheckpoints[4].Z + 1));
                    }
                    return;
                case 60: // open stock gun
                    if (Main.Players[player].FractionID != 14)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не служите в армии", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[14].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    player.SetData("ONFRACSTOCK", 14);
                    GUI.Dashboard.OpenOut(player, Stocks.fracStocks[14].Weapons, "Склад оружия", 6);
                    return;
            }
        }

        #region shapes

        private static void onEntityEnterColshape(ColShape shape, Player player)
        {
            try
            {
                NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
                if (shape.GetData<int>("INTERACT") == 36) Trigger.PlayerEvent(player, "interactHint", true);
            }
            catch (Exception e) { Log.Write("onEntityEnterColshape: " + e.Message, nLog.Type.Error); }
        }

        private static void onEntityExitColshape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception e) { Log.Write("onEntityExitColshape: " + e.Message, nLog.Type.Error); }
        }

        private static void onEntityExitArmyMats(ColShape shape, Player player)
        {
            NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
            Trigger.PlayerEvent(player, "interactHint", false);
            if (NAPI.Data.HasEntityData(player, "loadMatsTimer"))
            {
                //Main.StopT(player.GetData("loadMatsTimer"), "onEntityExitArmyMats");
                Timers.Stop(player.GetData<string>("loadMatsTimer"));
                player.ResetData("loadMatsTimer");
                Trigger.PlayerEvent(player, "hideLoader");
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Загрузка материалов отменена, так как машина покинула чекпоинт", 3000);
            }
        }

        #endregion

        public static void loadMaterialsTimer(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("vehicleMats")) return;
                    if (!player.IsInVehicle) return;
                    Vehicle vehicle = player.GetData<Vehicle>("vehicleMats");

                    var itemCount = VehicleInventory.GetCountOfType(player.Vehicle, ItemType.Material);
                    if (player.GetData<string>("whereLoad") == "WAR" && !Fractions.MatsWar.isWar)
                    {
                        player.SetData("INTERACTIONCHECK", 0);
                        //Main.StopT(player.GetData("loadMatsTimer"), "loadMaterialsTimer");
                        player.ResetData("loadMatsTimer");
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Корабль уже уехал", 3000);
                        return;
                    }
                    if (itemCount >= Fractions.Stocks.maxMats[vehicle.DisplayName])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В машине максимальное кол-во материала", 3000);
                        //Main.StopT(player.GetData("loadMatsTimer"), "loadMaterialsTimer_1");
                        player.ResetData("loadMatsTimer");
                        return;
                    }
                    var data = new nItem(ItemType.Material);
                    if (player.GetData<string>("whereLoad") == "WAR")
                    {
                        var count = Fractions.Stocks.maxMats[vehicle.DisplayName] - itemCount;
                        if (count >= Fractions.MatsWar.matsLeft)
                        {
                            data.Count = itemCount + Fractions.MatsWar.matsLeft;
                            Fractions.MatsWar.matsLeft = 0;
                            Fractions.MatsWar.endWar();
                        }
                        else
                        {
                            data.Count = count;
                            Fractions.MatsWar.matsLeft -= count;
                        }
                    }
                    else
                        data.Count = Fractions.Stocks.maxMats[vehicle.DisplayName] - itemCount;
                    VehicleInventory.Add(vehicle, data);
                    NAPI.Data.ResetEntityData(vehicle, "loaderMats");
                    //Main.StopT(player.GetData("loadMatsTimer"), "loadMaterialsTimer_2");
                    player.ResetData("loadMatsTimer");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы загрузили материалы в машину", 3000);
                }
                catch (Exception e) { Log.Write("LoadMatsTimer: " + e.Message, nLog.Type.Error); }
            });
        }

        public static void Event_PlayerDeath(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                if (player.HasData("loadMatsTimer"))
                {
                    Trigger.PlayerEvent(player, "hideLoader");
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Загрузка материалов отменена, так как Вы умерли", 3000);
                    //Main.StopT(player.GetData("loadMatsTimer"), "Event_PlayerDeath_army");
                    Timers.Stop(player.GetData<string>("loadMatsTimer"));
                    var vehicle = player.GetData<Vehicle>("vehicleMats");
                    NAPI.Data.ResetEntityData(vehicle, "loaderMats");
                    player.ResetData("loadMatsTimer");
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }

        public static void onPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (player.HasData("loadMatsTimer"))
                {
                    //Main.StopT(player.GetData("loadMatsTimer"), "army_onPlayerDisconnected");
                    Timers.Stop(player.GetData<string>("loadMatsTimer"));
                    var vehicle = player.GetData<Vehicle>("vehicleMats");
                    NAPI.Data.ResetEntityData(vehicle, "loaderMats");
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.VehicleDeath)]
        public void onVehicleDeath(Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.HasEntityData(vehicle, "loaderMats"))
                {
                    Player player = NAPI.Data.GetEntityData(vehicle, "loaderMats");
                    //Main.StopT(player.GetData("loadMatsTimer"), "");
                    Timers.Stop(player.GetData<string>("loadMatsTimer"));
                    NAPI.Data.ResetEntityData(vehicle, "loaderMats");
                    player.ResetData("loadMatsTimer");
                    Trigger.PlayerEvent(player, "hideLoader");
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Загрузка материалов отменена, так как машина покинула чекпоинт", 3000);
                }
            }
            catch (Exception e) { Log.Write("VehicleDeath: " + e.Message, nLog.Type.Error); }
        }

        #region menu
        public static void OpenArmyClothesMenu(Player player)
        {
            Menu menu = new Menu("armyclothes", false, false);
            menu.Callback = callback_armyclothes;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Одежда";
            menu.Add(menuItem);

            menuItem = new Menu.Item("change", Menu.MenuItem.Button);
            menuItem.Text = "Переодеться";
            menu.Add(menuItem);

            menuItem = new Menu.Item("combat", Menu.MenuItem.Button);
            menuItem.Text = "Боевая форма";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_armyclothes(Player Player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "change":
                    if (Main.Players[Player].FractionLVL < 6)
                    {
                        Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете переодеться/раздеться", 3000);
                        return;
                    }
                    if (!Player.GetData<bool>("ON_DUTY"))
                    {
                        Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы переоделись в служебную форму", 3000);
                        Manager.setSkin(Player, 14, Main.Players[Player].FractionLVL);
                        Player.SetData("ON_DUTY", true);
                    }
                    else
                    {
                        Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы переоделись в повседневную одежду", 3000);
                        Customization.ApplyCharacter(Player);
                        if (Player.HasData("HAND_MONEY")) Player.SetClothes(5, 45, 0);
                        else if (Player.HasData("HEIST_DRILL")) Player.SetClothes(5, 41, 0);
                        Player.SetData("ON_DUTY", false);
                    }
                    return;
                case "combat":
                    MenuManager.Close(Player);
                    OpenArmyCombatMenu(Player);
                    return;
                case "close":
                    MenuManager.Close(Player);
                    return;
            }
        }

        public static void OpenArmyCombatMenu(Player player)
        {
            Menu menu = new Menu("armycombat", false, false);
            menu.Callback = callback_armycombat;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Боевая форма";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam1", Menu.MenuItem.Button);
            menuItem.Text = "Альфа";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam2", Menu.MenuItem.Button);
            menuItem.Text = "Рейд";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam6", Menu.MenuItem.Button);
            menuItem.Text = "ГРОМ";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam3", Menu.MenuItem.Button);
            menuItem.Text = "Зеленый камуфляж";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam4", Menu.MenuItem.Button);
            menuItem.Text = "Military Police";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam5", Menu.MenuItem.Button);
            menuItem.Text = "Какой-то камфуляж";
            menu.Add(menuItem);

            

            menuItem = new Menu.Item("takeoff", Menu.MenuItem.Button);
            menuItem.Text = "Снять форму";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button);
            menuItem.Text = "Назад";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_armycombat(Player Player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (item.ID == "back")
            {
                MenuManager.Close(Player);
                OpenArmyClothesMenu(Player);
                return;
            }
            if (Main.Players[Player].FractionID != 14)
            {
                Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не служите в армии", 3000);
                return;
            }
            if (!Player.GetData<bool>("ON_DUTY"))
            {
                Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать службу", 3000);
                return;
            }
            Player.SetData("IN_CP_MODE", true);
            var gender = Main.Players[Player].Gender;
            Customization.ClearClothes(Player, gender);
            switch (item.ID)
            {
                case "cam1":
                    if (gender)
                    {
                        Player.SetClothes(1, 104, 5);
                        Customization.SetHat(Player, 117, 13);
                        Player.SetClothes(11, 221, 5);
                        Player.SetClothes(4, 87, 5);
                        Player.SetClothes(6, 62, 0);
                        Player.SetClothes(9, 16, 0);
                        Player.SetClothes(3, 49, 1);
                    }
                    else
                    {
                        Customization.SetHat(Player, 116, 13);
                        Player.SetClothes(1, 104, 5);
                        Player.SetClothes(4, 90, 5);
                        Player.SetClothes(11, 224, 5);
                        Player.SetClothes(6, 65, 0);
                        Player.SetClothes(3, 46, 1);
                        Player.SetClothes(9, 18, 0);
                    }
                    return;
                case "cam2":
                    if (gender)
                    {
                        Player.SetClothes(1, 104, 10);
                        Customization.SetHat(Player, 117, 18);
                        Player.SetClothes(11, 222, 10);
                        Player.SetClothes(4, 87, 10);
                        Player.SetClothes(6, 62, 2);
                        Player.SetClothes(9, 16, 2);
                        Player.SetClothes(3, 48, 0);
                    }
                    else
                    {
                        Customization.SetHat(Player, 116, 18);
                        Player.SetClothes(1, 104, 10);
                        Player.SetClothes(4, 90, 10);
                        Player.SetClothes(11, 224, 10);
                        Player.SetClothes(6, 65, 2);
                        Player.SetClothes(3, 46, 0);
                        Player.SetClothes(9, 18, 2);
                    }
                    return;
                case "cam3":
                    if (gender)
                    {
                        Player.SetClothes(1, 104, 15);
                        Customization.SetHat(Player, 117, 22);
                        Player.SetClothes(11, 220, 15);
                        Player.SetClothes(4, 87, 15);
                        Player.SetClothes(6, 25, 0);
                        Player.SetClothes(9, 16, 2);
                        Player.SetClothes(3, 49, 0);
                    }
                    else
                    {
                        Customization.SetHat(Player, 116, 22);
                        Player.SetClothes(1, 104, 15);
                        Player.SetClothes(4, 90, 15);
                        Player.SetClothes(11, 224, 15);
                        Player.SetClothes(6, 25, 0);
                        Player.SetClothes(3, 46, 0);
                        Player.SetClothes(9, 18, 2);
                    }
                    return;
                case "cam4":
                    if (gender)
                    {
                        Player.SetClothes(1, 104, 12);
                        Customization.SetHat(Player, 117, 20);
                        Player.SetClothes(11, 221, 12);
                        Player.SetClothes(4, 87, 12);
                        Player.SetClothes(6, 25, 0);
                        Player.SetClothes(9, 16, 2);
                        Player.SetClothes(3, 49, 0);
                    }
                    else
                    {
                        Customization.SetHat(Player, 116, 20);
                        Player.SetClothes(1, 104, 12);
                        Player.SetClothes(4, 90, 12);
                        Player.SetClothes(11, 224, 12);
                        Player.SetClothes(6, 25, 0);
                        Player.SetClothes(3, 46, 0);
                        Player.SetClothes(9, 18, 2);
                    }
                    return;
                case "cam5":
                    if (gender)
                    {
                        Player.SetClothes(1, 104, 16);
                        Customization.SetHat(Player, 117, 23);
                        Player.SetClothes(11, 220, 16);
                        Player.SetClothes(4, 87, 16);
                        Player.SetClothes(6, 62, 7);
                        Player.SetClothes(9, 16, 0);
                        Player.SetClothes(3, 48, 0);
                    }
                    else
                    {
                        Customization.SetHat(Player, 116, 23);
                        Player.SetClothes(1, 104, 16);
                        Player.SetClothes(4, 90, 16);
                        Player.SetClothes(11, 224, 16);
                        Player.SetClothes(6, 65, 7);
                        Player.SetClothes(3, 46, 1);
                        Player.SetClothes(9, 18, 0);
                    }
                    return;
                case "cam6":
                    if (gender)
                    {
                        Player.SetClothes(1, 104, 11);
                        Customization.SetHat(Player, 117, 19);
                        Player.SetClothes(11, 222, 11);
                        Player.SetClothes(4, 87, 11);
                        Player.SetClothes(6, 25, 0);
                        Player.SetClothes(9, 16, 2);
                        Player.SetClothes(3, 48, 0);
                    }
                    else
                    {
                        Customization.SetHat(Player, 116, 19);
                        Player.SetClothes(1, 104, 11);
                        Player.SetClothes(4, 90, 11);
                        Player.SetClothes(11, 224, 10);
                        Player.SetClothes(6, 25, 0);
                        Player.SetClothes(3, 46, 0);
                        Player.SetClothes(9, 18, 2);
                    }
                    return;
                case "takeoff":
                    Manager.setSkin(Player, Main.Players[Player].FractionID, Main.Players[Player].FractionLVL);
                    Player.SetData("IN_CP_MODE", false);
                    return;
            }
        }
        #endregion
    }
}
