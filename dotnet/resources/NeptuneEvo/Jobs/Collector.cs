using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeptuneEVO.Jobs
{
    class Collector : Script
    {
        private static nLog Log = new nLog("Collector");
        private static int checkpointPayment = 7;
        //private static int JobMultiper = 4;

        private static Vector3 TakeMoneyPos = new Vector3(915.9069, -1265.255, 24.50912);

        private static List<Vector3> inkpoints = new List<Vector3>
        { 
            new Vector3(902.4651, -1261.294, 24.69521),
            new Vector3(-1484.01, -518.9309, 31.68689),
            new Vector3(-142.7316, 6367.828, 30.37061)
        };


        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                var col = NAPI.ColShape.CreateCylinderColShape(TakeMoneyPos, 1, 3, 0);
                col.OnEntityEnterColShape += (s, e) => {
                    try
                    {
                        e.SetData("INTERACTIONCHECK", 45);
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
                NAPI.Marker.CreateMarker(1, TakeMoneyPos - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220), false, 0);
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Взять мешки"), TakeMoneyPos + new Vector3(0, 0, 0.3), 30f, 0.4f, 0, new Color(255, 255, 255), true, 0);

                for (int i = 0; i < 3; i++)
                {
                    ColShape Shape = NAPI.ColShape.CreateCylinderColShape(inkpoints[i], 1F, 2F);
                    NAPI.Marker.CreateMarker(27, inkpoints[i] + new Vector3(0, 0, 0.2f), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220), false, 0);
                    NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Взять инкассатор"), inkpoints[i] + new Vector3(0,0, 1.2f), 30f, 0.5f, 0, new Color(255, 255, 255));
                    Shape.SetData("INK", i);
                    Shape.OnEntityEnterColShape += (s, entity) =>
                    {
                        try
                        {
                            NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 226);
                            NAPI.Data.SetEntityData(entity, "INK", s.GetData<int>("INK"));
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

                

            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "COLLECTOR" || player.VehicleSeat != 0) return;
                if (Main.Players[player].WorkID == 7)
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
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете инкассатором. Устроиться можно у начальника", 3000);
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
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") == "COLLECTOR" &&
                Main.Players[player].WorkID == 7 &&
                NAPI.Data.GetEntityData(player, "ON_WORK") &&
                NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                {
                    if (!player.HasData("WORKOBJECT") && player.GetData<int>("COLLECTOR_BAGS") > 0)
                    {
                        BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_money_bag_01"), 18905, new Vector3(0.55, 0.02, 0), new Vector3(0, -90, 0));
                        player.SetData("WORKOBJECT", true);
                    }

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
                if (Main.Players[player].WorkID == 7 && player.GetData<bool>("ON_WORK"))
                {
                    var vehicle = player.GetData<Vehicle>("WORK");

                    vehicle.Delete();

                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);

                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили зарплату в размере: {player.GetData<int>("PAYMENT")}$", 3000);

                    MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                    Golemo.Families.Family.GiveMoneyOnJob(player, player.GetData<int>("PAYMENT"));
                    Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                    player.SetData("PAYMENT", 0);

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
                if (Main.Players[player].WorkID == 7 && player.GetData<bool>("ON_WORK"))
                {
                    var vehicle = player.GetData<Vehicle>("WORK");

                    NAPI.Task.Run(() => { try { vehicle.Delete(); } catch { } });
                    

                    MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                    Golemo.Families.Family.GiveMoneyOnJob(player, player.GetData<int>("PAYMENT"));
                    Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                    player.SetData("PAYMENT", 0);
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
                        NAPI.Task.Run(() => { vehicle.Delete(); });
                        

                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили зарплату в размере: {player.GetData<int>("PAYMENT")}$", 3000);

                        MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                        Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                        player.SetData("PAYMENT", 0);
                        Golemo.Families.Family.GiveMoneyOnJob(player, player.GetData<int>("PAYMENT"));
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);
                        NAPI.Data.SetEntityData(player, "PAYMENT", 0);

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

        private static List<int> lastcar = new List<int> {0,0,0};
        private static List<int> rotate = new List<int> { 36, 33, 45 };

        private static List<List<Vector3>> carspawns = new List<List<Vector3>>
        {
            new List<Vector3>
            {
                new Vector3(911.9891, -1265.722, 24.4611),
                new Vector3(926.6917, -1253.327, 24.3698),
                new Vector3(938.7714, -1245.45, 24.52321),
            },
            new List<Vector3>
            {
                new Vector3(-1498.998, -512.3082, 31.68688),
                new Vector3(-1494.113, -509.0401, 31.68688),
                new Vector3(-1488.421, -506.2317, 31.68688),
            },
            new List<Vector3>
            {
                new Vector3(-154.2873, 6356.403, 30.37062),
                new Vector3(-151.5853, 6358.701, 30.37061),
                new Vector3(-148.9587, 6361.461, 30.37061),
            }
        };

        public static Vector3 getCollePos(int ink)
        {
            if (lastcar[ink] + 1 >= 3)
                lastcar[ink] = 0;
            else
                lastcar[ink] += 1;
            return carspawns[ink][lastcar[ink]];
        }

        public static void collectspawn(Player player)
        {
            try
            {
                int point = player.GetData<int>("INK");
                var veh = NAPI.Vehicle.CreateVehicle(VehicleHash.Stockade, getCollePos(point), rotate[point], 111, 111, "COLLECTOR");
                NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
                NAPI.Data.SetEntityData(player, "WORK", veh);
                NAPI.Data.SetEntityData(veh, "WORK", 7);
                NAPI.Data.SetEntityData(veh, "TYPE", "COLLECTOR");
                NAPI.Data.SetEntityData(veh, "NUMBER", "COLLECTOR");
                NAPI.Data.SetEntityData(veh, "ON_WORK", false);
                NAPI.Data.SetEntityData(veh, "DRIVER", null);
                player.SetData("PAYMENT", 0);
                Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));
                veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
                Core.VehicleStreaming.SetEngineState(veh, false);
                Core.VehicleStreaming.SetLockStatus(veh, false);
                player.SetIntoVehicle(veh, 0);
                rentCar(player);
            }
            catch (Exception e) { Log.Write("SPAWN: " + e.ToString()); }
        }

        public static void rentCar(Player player)
        {

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы начали работу инкассатором. Развезите деньги по банкоматам.", 3000);
            MoneySystem.Wallet.Change(player, -100);
            //GameLog.Money($"player({Main.Players[player].UUID})", $"server", 100, $"collectorRent");
            player.SetData("ON_WORK", true);
            Vehicle vehicle = player.GetData<Vehicle>("WORK");
            //Core.VehicleStreaming.SetEngineState(vehicle, false);
            NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
            NAPI.Data.SetEntityData(vehicle, "DRIVER", player);
            player.SetData("COLLECTOR_BAGS", 15);
            player.SetData("W_LASTPOS", player.Position);
            player.SetData("W_LASTTIME", DateTime.Now);

            var x = WorkManager.rnd.Next(0, MoneySystem.ATM.ATMs.Count - 1); ;
            while (x == 36 || MoneySystem.ATM.ATMs[x].DistanceTo2D(player.Position) < 200)
                x = WorkManager.rnd.Next(0, MoneySystem.ATM.ATMs.Count - 1);
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

            BasicSync.DetachObject(player);
            player.ResetData("WORKOBJECT");

            Trigger.PlayerEvent(player, "createCheckpoint", 16, 29, MoneySystem.ATM.ATMs[x] + new Vector3(0, 0, 1.12), 1, 0, 220, 220, 0);
            Trigger.PlayerEvent(player, "createWaypoint", MoneySystem.ATM.ATMs[x].X, MoneySystem.ATM.ATMs[x].Y);
            Trigger.PlayerEvent(player, "createWorkBlip", MoneySystem.ATM.ATMs[x]);
        }

        public static void CollectorTakeMoney(Player player)
        {
            if (player.IsInVehicle || Main.Players[player].WorkID != 7 || !player.GetData<bool>("ON_WORK")) return;
            if (player.GetData<int>("COLLECTOR_BAGS") != 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас ещё остались мешки с деньгами ({player.GetData<int>("COLLECTOR_BAGS")}шт)", 3000);
                return;
            }
            else
            {
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы взяли новые мешки с деньгами.", 3000);
                player.SetData("COLLECTOR_BAGS", 15);

                var x = WorkManager.rnd.Next(0, MoneySystem.ATM.ATMs.Count - 1);
                while (x == 36 || MoneySystem.ATM.ATMs[x].DistanceTo2D(player.Position) < 200)
                    x = WorkManager.rnd.Next(0, MoneySystem.ATM.ATMs.Count - 1);

                player.SetData("W_LASTPOS", player.Position);
                player.SetData("W_LASTTIME", DateTime.Now);
                player.SetData("WORKCHECK", x);
                Trigger.PlayerEvent(player, "createCheckpoint", 16, 29, MoneySystem.ATM.ATMs[x] + new Vector3(0, 0, 1.12), 1, 0, 220, 220, 0);
                Trigger.PlayerEvent(player, "createWaypoint", MoneySystem.ATM.ATMs[x].X, MoneySystem.ATM.ATMs[x].Y);
                Trigger.PlayerEvent(player, "createWorkBlip", MoneySystem.ATM.ATMs[x]);
            }
        }
        public static void CollectorEnterATM(Player player, ColShape shape)
        {
            try
            {
                if (player.IsInVehicle || Main.Players[player].WorkID != 7 || !player.GetData<bool>("ON_WORK")
                    || player.GetData<int>("COLLECTOR_BAGS") == 0 || player.GetData<int>("WORKCHECK") != shape.GetData<int>("NUMBER")) return;
                player.SetData("COLLECTOR_BAGS", player.GetData<int>("COLLECTOR_BAGS") - 1);

                var coef = Convert.ToInt32(player.Position.DistanceTo2D(player.GetData<Vector3>("W_LASTPOS")) / (100 - Main.AddJobPoint(player) * 2));
                var payment = Convert.ToInt32(coef * checkpointPayment * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);

                DateTime lastTime = player.GetData<DateTime>("W_LASTTIME");
                if (DateTime.Now < lastTime.AddSeconds(coef * 2))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Банкомат ещё полон. Попробуйте позже", 3000);
                    return;
                }

                //player.SetData("PAYMENT", player.GetData("PAYMENT") + payment);
                player.SetData("W_LASTPOS", player.Position);
                player.SetData("W_LASTTIME", DateTime.Now);

                var level = Main.Players[player].LVL > 5 ? 250 : 40;

                player.SetData("PAYMENT", player.GetData<int>("PAYMENT") + ( payment + level ) * Main.Multipy);

                player.SendNotification($"Инкассатор: ~h~~g~+{(payment + level) * Main.Multipy}$", true);

                Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));
                //MoneySystem.Wallet.Change(player, payment);
                //GameLog.Money($"server", $"player({Main.Players[player].UUID})", payment, $"collectorCheck");

                if (player.HasData("WORKOBJECT"))
                {
                    BasicSync.DetachObject(player);
                    player.ResetData("WORKOBJECT");
                }

                if (player.GetData<int>("COLLECTOR_BAGS") == 0)
                {
                    Notify.Send(player, NotifyType.Alert, NotifyPosition.BottomCenter, "Возвращайтесь на базу, чтобы взять новые мешки с деньгами", 3000);
                    Trigger.PlayerEvent(player, "deleteWorkBlip");
                    Trigger.PlayerEvent(player, "createWaypoint", TakeMoneyPos.X, TakeMoneyPos.Y);
                    Trigger.PlayerEvent(player, "deleteCheckpoint", 16);
                    return;
                }
                else
                {
                    var x = WorkManager.rnd.Next(0, MoneySystem.ATM.ATMs.Count - 1); ;
                    while (x == 36 || x == player.GetData<int>("WORKCHECK") || MoneySystem.ATM.ATMs[x].DistanceTo2D(player.Position) < 200)
                        x = WorkManager.rnd.Next(0, MoneySystem.ATM.ATMs.Count - 1);
                    player.SetData("WORKCHECK", x);
                    Trigger.PlayerEvent(player, "createCheckpoint", 16, 29, MoneySystem.ATM.ATMs[x] + new Vector3(0, 0, 1.12), 1, 0, 220, 220, 0);
                    Trigger.PlayerEvent(player, "createWaypoint", MoneySystem.ATM.ATMs[x].X, MoneySystem.ATM.ATMs[x].Y);
                    Trigger.PlayerEvent(player, "createWorkBlip", MoneySystem.ATM.ATMs[x]);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Направляйтесь к следующему банкомату.", 3000);
                }
            }
            catch { }
        }
    }
}
