using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace NeptuneEVO.Jobs
{
    class TrashCar : Script
    {
        private static nLog Log = new nLog("TrashCar");
        //private static int checkpointPayment = 120;
        //private static int JobMultiper = 3;

        private static Vector3 TakeMoneyPos = new Vector3(982.7246, -2547.97, 27.18197);

        private static List<Vector3> GarbageVec = new List<Vector3>
        {
            new Vector3(420.2066, -1882.828, 25.31137),
            new Vector3(512.0422, -1750.284, 27.57225),
            new Vector3(478.9376, -1958.798, 23.53709),
            new Vector3(139.3396, -1363.275, 28.19832),
            new Vector3(-87.23938, -1330.289, 28.17602),
            new Vector3(51.72773, -1045.366, 28.45192),
            new Vector3(-575.5757, -294.2992, 33.86289),
            new Vector3(-259.3021, 292.378, 90.4246),
            new Vector3(-547.1484, 286.3422, 81.90038),
            new Vector3(-1788.369, -491.3763, 37.64785),
            new Vector3(-1497.711, -944.4611, 7.534143),
            new Vector3(236.9404, 360.7889, 104.5476),
            new Vector3(396.9311, 289.1745, 101.832),
            new Vector3(636.8716, 2729.286, 40.76245),
            new Vector3(562.3978, 2670.314, 41.00822),
            new Vector3(405.217, 2625.672, 43.34476),
            new Vector3(282.7295, 2577.167, 44.05407),
            new Vector3(-3.375389, 6398.191, 30.12284),
            new Vector3(-156.1259, 6190.336, 30.16336),
            new Vector3(-429.4126, 6124.543, 30.35434),
            new Vector3(-3193.236, 1075.028, 19.7337),
            new Vector3(-3243.571, 995.0728, 11.34586),
            new Vector3(-1989.583, -488.1186, 10.50843),
            new Vector3(127.8294, -1054.618, 28.07235),
            new Vector3(160.3462, -778.2338, 30.56956),
        };
        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                var col = NAPI.ColShape.CreateCylinderColShape(TakeMoneyPos, 3, 3, 0);
                col.OnEntityEnterColShape += (s, e) => {
                    try
                    {
                        e.SetData("INTERACTIONCHECK", 128);
                    }
                    catch (Exception ex) { Log.Write("col.OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                col.OnEntityExitColShape += (s, e) => {
                    try
                    {
                        e.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception ex) { Log.Write("col.OnEntityExitColShape: " + ex.Message, nLog.Type.Error); }
                };
                NAPI.Marker.CreateMarker(27, TakeMoneyPos + new Vector3(0, 0, 0.2f), new Vector3(), new Vector3(), 3f, new Color(0, 86, 214, 220), false, 0);
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Нажмите E\nчтобы выгрузить мусор"), TakeMoneyPos + new Vector3(0, 0, 1.5), 30f, 0.4f, 0, new Color(255, 255, 255), true, 0);
                int i = 0;
                foreach(Vector3 vec in GarbageVec)
                {
                    var shape = NAPI.ColShape.CreateCylinderColShape(vec, 3, 3, 0);
                    shape.SetData("NUMBER", i);
                    shape.OnEntityEnterColShape += (s, e) => {
                        try
                        {
                            e.SetData("SHAPE", shape);
                            e.SetData("INTERACTIONCHECK", 129);
                        }
                        catch (Exception ex) { Log.Write("col.OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                    };
                    shape.OnEntityExitColShape += (s, e) => {
                        try
                        {
                            e.ResetData("SHAPE");
                            e.SetData("INTERACTIONCHECK", 0);
                        }
                        catch (Exception ex) { Log.Write("col.OnEntityExitColShape: " + ex.Message, nLog.Type.Error); }
                    };
                    i++;
                }

                    ColShape Shape = NAPI.ColShape.CreateCylinderColShape(new Vector3(1091.654, -2224.976, 30.18398), 1F, 2F);
                    NAPI.Marker.CreateMarker(27, new Vector3(1091.654, -2224.976, 30.18398) + new Vector3(0, 0, 0.2f), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220), false, 0);
                    NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Взять мусоровоз"), new Vector3(1091.654, -2224.976, 30.18398 + 1.2), 30f, 0.5f, 0, new Color(255, 255, 255));
                Shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 227);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                Shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };


            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "TRASH" || player.VehicleSeat != 0) return;
                if (Main.Players[player].WorkID == 10)
                {
                    if (player.HasData("WORKOBJECT"))
                    {
                        BasicSync.DetachObject(player);
                        player.ResetData("WORKOBJECT");
                    }
                    if (!NAPI.Data.GetEntityData(vehicle, "ON_WORK"))
                    {
                        if (NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                            NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                    }
                    else
                    {
                        if (NAPI.Data.GetEntityData(player, "WORK") != vehicle)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Эта машина занята", 3000);
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                        }
                        else NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                    }
                }
                else
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете водителем мусоровоза. Устроиться можно у начальника", 3000);
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") == "TRASH" &&
                Main.Players[player].WorkID == 10 &&
                NAPI.Data.GetEntityData(player, "ON_WORK") &&
                NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                {

                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Если Вы не сядете в транспорт через 3 минуты, то рабочий день закончится", 3000);
                    NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "timer_13");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", 0);
                    //NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Main.StartT(1000, 1000, (o) => timer_playerExitWorkVehicle(player, vehicle), "COL_EXIT_CAR_TIMER"));
                    NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Timers.StartTask(1000, () => timer_playerExitWorkVehicle(player, vehicle)));
                }
            }
            catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }

        public static void Event_PlayerDeath(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID == 10 && player.GetData<bool>("ON_WORK"))
                {
                    var vehicle = player.GetData<Vehicle>("WORK");

                    vehicle.Delete();

                   
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили зарплату в размере: {player.GetData<int>("PAYMENT")}$", 3000);
					
					Utils.QuestsManager.AddQuestProcess(player, 14, player.GetData<int>("PAYMENT"));

                    MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                    Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                    Golemo.Families.Family.GiveMoneyOnJob(player, player.GetData<int>("PAYMENT"));

                    player.SetData("PAYMENT", 0);

                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);

                    NAPI.Data.SetEntityData(player, "ON_WORK", false);
                    NAPI.Data.SetEntityData(player, "WORK", null);
                    Trigger.PlayerEvent(player, "deleteCheckpoint", 16, 0);
                    Trigger.PlayerEvent(player, "deleteWorkBlip");
                    Customization.ApplyCharacter(player);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))
                    {
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "timer_14");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                    }
                }
                if (player.HasData("WORKOBJECT"))
                {
                    BasicSync.DetachObject(player);
                    player.ResetData("WORKOBJECT");
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }

        public static void Event_PlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (Main.Players[player].WorkID == 10 && player.GetData<bool>("ON_WORK"))
                {
                    var vehicle = player.GetData<Vehicle>("WORK");

                    vehicle.Delete();
                }
                if (player.HasData("WORKOBJECT"))
                {
                    BasicSync.DetachObject(player);
                    player.ResetData("WORKOBJECT");
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        private void timer_playerExitWorkVehicle(Player player, Vehicle vehicle)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("WORK_CAR_EXIT_TIMER")) return;
                    if (NAPI.Data.GetEntityData(player, "IN_WORK_CAR"))
                    {
                        //                    Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "timer_16");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        Log.Debug("Player exit work vehicle timer was stoped");
                        return;
                    }
                    if (NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") > 180)
                    {
                        vehicle.Delete();

                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили зарплату в размере: {player.GetData<int>("PAYMENT")}$", 3000);

						Utils.QuestsManager.AddQuestProcess(player, 14, player.GetData<int>("PAYMENT"));

                        MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                        Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                        Golemo.Families.Family.GiveMoneyOnJob(player, player.GetData<int>("PAYMENT"));

                        player.SetData("PAYMENT", 0);

                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);

                        NAPI.Data.SetEntityData(player, "ON_WORK", false);
                        NAPI.Data.SetEntityData(player, "WORK", null);
                        NAPI.ClientEvent.TriggerClientEvent(player, "deleteCheckpoint", 16, 0);
                        NAPI.ClientEvent.TriggerClientEvent(player, "deleteWorkBlip");
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "timer_17");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        Customization.ApplyCharacter(player);

                        if (player.HasData("WORKOBJECT"))
                        {
                            BasicSync.DetachObject(player);
                            player.ResetData("WORKOBJECT");
                        }
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") + 1);

                }
                catch (Exception e)
                {
                    Log.Write("Timer_PlayerExitWorkVehicle_Collector:\n" + e.ToString(), nLog.Type.Error);
                }
            });
        }

        private static int lastcar = -1;

        private static List<Vector3> carspawns = new List<Vector3>
        {
            new Vector3(1113.171, -2283.303, 29.20169),
            new Vector3(1113.323, -2291.603, 29.38149),
            new Vector3(1112.78, -2300.789, 29.38751),
        };

        public static Vector3 getTrashPos()
        {
            if (lastcar + 1 >= carspawns.Count)
                lastcar = 0;
            else
                lastcar += 1;
            return carspawns[lastcar];
        }

        public static void trashspawn(Player player)
        {
            var veh = NAPI.Vehicle.CreateVehicle(VehicleHash.Trash2, getTrashPos(), 235, 4, 4, "TRASH");
            NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
            NAPI.Data.SetEntityData(veh, "WORK", 10);
            NAPI.Data.SetEntityData(veh, "TYPE", "TRASH");
            NAPI.Data.SetEntityData(veh, "NUMBER", "TRASH");
            NAPI.Data.SetEntityData(veh, "DRIVER", null);
            NAPI.Data.SetEntityData(veh, "ON_WORK", false);
            NAPI.Data.SetEntityData(player, "WORK", veh);
            player.SetData("PAYMENT", 0);
            Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));
            veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
            Core.VehicleStreaming.SetEngineState(veh, false);
            Core.VehicleStreaming.SetLockStatus(veh, false);
            player.SetIntoVehicle(veh, 0);
            rentCar(player);
        }


        public static void rentCar(Player player)
        {
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы начали работу водителем мусоровоза. Собирайте мусор, и отвозите на переработку.", 3000);
            var vehicle = player.GetData<Vehicle>("WORK");
            player.SetData("ON_WORK", true);
            Core.VehicleStreaming.SetEngineState(vehicle, false);
            NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
            NAPI.Data.SetEntityData(vehicle, "DRIVER", player);
            player.SetData("TRASH", 0);
            player.SetData("W_LASTPOS", player.Position);
            player.SetData("W_LASTTIME", DateTime.Now);

            var x = WorkManager.rnd.Next(0, GarbageVec.Count - 1); ;
            while (x == 36 || GarbageVec[x].DistanceTo2D(player.Position) < 200)
                x = WorkManager.rnd.Next(0, GarbageVec.Count - 1);
            player.SetData("WORKCHECK", x);
            if (Main.Players[player].Gender)
            {
                Customization.SetHat(player, 63, 9);
                player.SetClothes(11, 132, 0);
                player.SetClothes(4, 33, 0);
                player.SetClothes(6, 24, 0);
                player.SetClothes(9, 1, 1);
                player.SetClothes(8, 129, 0);
                player.SetClothes(3, Customization.CorrectTorso[true][132], 0);
            }
            else
            {
                Customization.SetHat(player, 63, 9);
                player.SetClothes(11, 129, 0);
                player.SetClothes(4, 32, 0);
                player.SetClothes(6, 24, 0);
                player.SetClothes(9, 6, 1);
                player.SetClothes(8, 159, 0);
                player.SetClothes(3, Customization.CorrectTorso[false][129], 0);
            }
            Trigger.PlayerEvent(player, "createCheckpoint", 16, 24, GarbageVec[x] + new Vector3(0, 0, 1.12), 1, 0, 86, 214, 0);
            Trigger.PlayerEvent(player, "createWaypoint", GarbageVec[x].X, GarbageVec[x].Y);
            Trigger.PlayerEvent(player, "createWorkBlip", GarbageVec[x]);
        }

        public static void SellTrash(Player player)
        {
            if (!player.IsInVehicle || player.Vehicle.GetData<string>("TYPE") != "TRASH" || Main.Players[player].WorkID != 10 || !player.GetData<bool>("ON_WORK")) return;
            if (player.HasData("TRASH") && player.GetData<int>("TRASH") > 0)
            {
                int count = player.GetData<int>("TRASH");

                int plus = count * 15 + Main.AddJobPoint(player) ;

                var payment = Convert.ToInt32(plus * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);

                player.SetData("PAYMENT", player.GetData<int>("PAYMENT") + payment * Main.Multipy);
                Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));

                // MoneySystem.Wallet.Change(player, payment + plus);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Сброс мусора +{payment}$", 3000);
                player.SetData("TRASH", 0);

                player.SetData("W_LASTPOS", player.Position);
                player.SetData("W_LASTTIME", DateTime.Now);

                var x = WorkManager.rnd.Next(0, GarbageVec.Count - 1); ;
                while (x == 36 || GarbageVec[x].DistanceTo2D(player.Position) < 200)
                    x = WorkManager.rnd.Next(0, GarbageVec.Count - 1);
                player.SetData("WORKCHECK", x);

                Trigger.PlayerEvent(player, "createCheckpoint", 16, 24, GarbageVec[x] + new Vector3(0, 0, 1.12), 1, 0, 86, 214, 0);
                Trigger.PlayerEvent(player, "createWaypoint", GarbageVec[x].X, GarbageVec[x].Y);
                Trigger.PlayerEvent(player, "createWorkBlip", GarbageVec[x]);
            }
            else
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет мусора в мусоровозе", 3000);
            }

        }
        public static void PickupTrash(Player player, ColShape shape)
        {
            try
            {
                if (player.IsInVehicle || Main.Players[player].WorkID != 10 || !player.GetData<bool>("ON_WORK") || player.GetData<int>("WORKCHECK") != shape.GetData<int>("NUMBER")) return;

                player.SetData("TRASH", player.GetData<int>("TRASH") + 1);
                player.SetData("W_LASTPOS", player.Position);
                player.SetData("W_LASTTIME", DateTime.Now);

                BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_rub_binbag_01"), 18905, new Vector3(0.55, 0.02, 0), new Vector3(0, -90, 0));
                player.SetData("WORKOBJECT", true);

                if (player.GetData<int>("TRASH") >= 25)
                {
                    Notify.Send(player, NotifyType.Alert, NotifyPosition.BottomCenter, "Возвращайтесь на переработку, чтобы выгрузить мусор", 3000);
                    Trigger.PlayerEvent(player, "deleteWorkBlip");
                    Trigger.PlayerEvent(player, "createWaypoint", TakeMoneyPos.X, TakeMoneyPos.Y);
                    Trigger.PlayerEvent(player, "deleteCheckpoint", 16);
                    return;
                }
                else
                {
                    var x = WorkManager.rnd.Next(0, GarbageVec.Count - 1); ;
                    while (x == 36 || x == player.GetData<int>("WORKCHECK") || GarbageVec[x].DistanceTo2D(player.Position) < 200)
                        x = WorkManager.rnd.Next(0, GarbageVec.Count - 1);
                    player.SetData("WORKCHECK", x);
                    Trigger.PlayerEvent(player, "createCheckpoint", 16, 24, GarbageVec[x] + new Vector3(0, 0, 1.12), 1, 0, 86, 214, 0);
                    Trigger.PlayerEvent(player, "createWaypoint", GarbageVec[x].X, GarbageVec[x].Y);
                    Trigger.PlayerEvent(player, "createWorkBlip", GarbageVec[x]);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Направляйтесь к следующему мусорному баку.", 3000);
                }
            }
            catch { }
        }
    }
}
