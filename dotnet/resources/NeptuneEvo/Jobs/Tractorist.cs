using GTANetworkAPI;
using System.Collections.Generic;
using NeptuneEVO.GUI;
using System;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Jobs
{
    class Tractorist : Script
    {
        static int checkpointPayment = 5;
        //private static int JobMultiper = 2;
        private static nLog Log = new nLog("Tractorist");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStartHandler()
        {
            try
            {
                /*MowerWaysCols.Add(new Dictionary<int, ColShape>());
                MowerWaysCols.Add(new Dictionary<int, ColShape>());
                MowerWaysCols.Add(new Dictionary<int, ColShape>());

                for (int i = 0; i < MowerWays.Count; i++)
                {
                    for (int d = 0; d < MowerWays[i].Count; d++)
                    {
                        MowerWaysCols[i].Add(d, NAPI.ColShape.CreateCylinderColShape(MowerWays[i][d], 5F, 2, 0));
                        MowerWaysCols[i][d].OnEntityEnterColShape += mowerCheckpointEnterWay;
                        MowerWaysCols[i][d].SetData("WAY", i);
                        MowerWaysCols[i][d].SetData("NUMBER", d);
                    }
                }


                ColShape Shape = NAPI.ColShape.CreateCylinderColShape(new Vector3(2928.441, 4621.515, 47.60086), 1F, 2F);
                NAPI.Marker.CreateMarker(27, new Vector3(2928.441, 4621.515, 47.60086) + new Vector3(0, 0, 0.2f), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220), false, 0);
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Взять транспорт"), new Vector3(2928.441, 4621.515, 47.60086 + 1.2), 30f, 0.5f, 0, new Color(255, 255, 255));
                Shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 229);
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
                };*/

            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        private static List<Dictionary<int, ColShape>> MowerWaysCols = new List<Dictionary<int, ColShape>>();
        private static List<List<Vector3>> MowerWays = new List<List<Vector3>>()
        {
            new List<Vector3>()
            {
                new Vector3(2944.227, 4677.972, 50.37289),
				new Vector3(2942.201, 4687.977, 51.5766),
				new Vector3(2932.86, 4691.401, 51.32101),
				new Vector3(2932.385, 4678.635, 50.55306),
				new Vector3(2927.685, 4672.192, 49.7314),
				new Vector3(2925.291, 4681.519, 50.68723),
				new Vector3(2916.172, 4689.665, 50.31316),
				new Vector3(2920.123, 4674.473, 49.93061),
				new Vector3(2913.156, 4662.905, 49.76812),
				new Vector3(2910.266, 4676.666, 49.87474),
				new Vector3(2878.675, 4669.81, 48.9025),
				new Vector3(2891.982, 4617.631, 48.90578),
				new Vector3(2858.3, 4584.655, 48.00511),
				new Vector3(2863.468, 4624.046, 49.47181),
				new Vector3(2870.264, 4653.981, 49.0085),
				new Vector3(2843.325, 4647.426, 48.83412),
				new Vector3(2828.575, 4611.024, 47.1997),
				new Vector3(2849.81, 4606.643, 48.60263),
				new Vector3(2868.343, 4648.012, 49.10095),
				new Vector3(2913.065, 4667.597, 49.70699),
				new Vector3(2930.873, 4686.692, 51.17681),
				new Vector3(2874.417, 4619.524, 49.22645),
				new Vector3(2839.164, 4609.603, 48.19302),
				new Vector3(2831.078, 4639.683, 47.39918),
				new Vector3(2928.557, 4659.35, 49.19936),
            },
        };

        public static void onPlayerDissconnectedHandler(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                try { if (!player.GetData<bool>("ON_WORK")) return; }
                catch { return; }
                if (Main.Players[player].WorkID == 9 &&
                    NAPI.Data.GetEntityData(player, "WORK") != null)
                {
                    var vehicle = NAPI.Data.GetEntityData(player, "WORK");
                    vehicle.Delete();
                    MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
					Utils.QuestsManager.AddQuestProcess(player, 13, player.GetData<int>("PAYMENT"));
                    Golemo.Families.Family.GiveMoneyOnJob(player, player.GetData<int>("PAYMENT"));

                    Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                    player.SetData("PAYMENT", 0);
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        private static int lastcar = -1;

        private static List<Vector3> carspawns = new List<Vector3>
        {
            new Vector3(2940.301, 4651.722, 47.42487),
        };

        public static Vector3 getMowerPos()
        {
            if (lastcar + 1 >= carspawns.Count)
                lastcar = 0;
            else
                lastcar += 1;
            return carspawns[lastcar];
        }

        public static void mowerspawn(Player player)
        {
            var veh = NAPI.Vehicle.CreateVehicle(VehicleHash.Tractor2, getMowerPos(), 135, 4, 4, "MOWER");
            //var veh2 = NAPI.Vehicle.CreateVehicle(VehicleHash.RakeTrailer, new Vector3(2942.581, 4653.906, 47.42487), 135, 4, 4, "MOWER");
            NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
            NAPI.Data.SetEntityData(veh, "WORK", 9);
            NAPI.Data.SetEntityData(veh, "TYPE", "TRACTOR");
            NAPI.Data.SetEntityData(veh, "NUMBER", "MOWER");
            NAPI.Data.SetEntityData(veh, "DRIVER", null);
            //NAPI.Data.SetEntityData(veh, "TRAILER", veh2);
            NAPI.Data.SetEntityData(veh, "ON_WORK", false);
            player.SetData("PAYMENT", 0);
            Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));
            veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
            Core.VehicleStreaming.SetEngineState(veh, false);
            Core.VehicleStreaming.SetLockStatus(veh, false);
            player.SetIntoVehicle(veh, 0);
            mowerRent(player);
        }


        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") == "TRACTOR" &&
                Main.Players[player].WorkID == 9 &&
                NAPI.Data.GetEntityData(player, "ON_WORK") &&
                NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Если Вы не сядете в трактор через 60 секунд, то рабочий день закончится", 3000);
                    NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))

                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", 0);

                    NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Timers.StartTask(1000, () => timer_playerExitWorkVehicle(player, vehicle)));
                }
            }
            catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
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

                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        Log.Debug("Player exit work vehicle timer was stoped");
                        return;
                    }
                    if (NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") > 60)
                    {
                        vehicle.Delete();
                        NAPI.Data.SetEntityData(player, "ON_WORK", false);
                        NAPI.Data.SetEntityData(player, "WORK", null);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);
                        Trigger.PlayerEvent(player, "deleteCheckpoint", 4, 0);

                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили зарплату в размере: {player.GetData<int>("PAYMENT")}$", 3000);

                        MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                        Golemo.Families.Family.GiveMoneyOnJob(player, player.GetData<int>("PAYMENT"));

                        Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                        player.SetData("PAYMENT", 0);

                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        Customization.ApplyCharacter(player);
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") + 1);
                }
                catch (Exception e) { Log.Write("Timer_PlayerExitWorkVehicle_Lawnmower: " + e.Message, nLog.Type.Error); }
            });
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "TRACTOR" || player.VehicleSeat != 0) return;

                if (Main.Players[player].WorkID == 9)
                {
                   if (NAPI.Data.GetEntityData(player, "WORK") != vehicle)
                   {
                       Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У трактора есть тракторист", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                   }
                   else NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                }
                else
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете трактористом. Устроиться можно у начальника", 3000);
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }

        public static void mowerRent(Player player)
        {
            if (NAPI.Player.IsPlayerInAnyVehicle(player) || player.VehicleSeat != 0 || player.Vehicle.GetData<string>("TYPE") != "TRACTOR")
            {
                var way = 0;
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы начали работу тракториста, следуйте по чекпоинтам", 3000);
                var vehicle = player.Vehicle;
                NAPI.Data.SetEntityData(player, "WORK", vehicle);
                Core.VehicleStreaming.SetEngineState(vehicle, true);
                NAPI.Data.SetEntityData(player, "ON_WORK", true);
                NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                NAPI.Data.SetEntityData(vehicle, "ON_WORK", true);
                NAPI.Data.SetEntityData(player, "WORKWAY", way);
                NAPI.Data.SetEntityData(player, "WORKCHECK", 0);
                NAPI.Data.SetEntityData(vehicle, "DRIVER", player);

                var gender = Main.Players[player].Gender;
                Core.Customization.ClearClothes(player, gender);
                if (gender)
                {
                    Customization.SetHat(player, 94, 9);
                    player.SetClothes(11, 82, 4);
                    player.SetClothes(4, 27, 10);
                    player.SetClothes(6, 1, 11);
                    player.SetClothes(11, Core.Customization.CorrectTorso[gender][82], 0);
                }
                else
                {
                    Customization.SetHat(player, 93, 9);
                    player.SetClothes(11, 14, 9);
                    player.SetClothes(4, 16, 2);
                    player.SetClothes(6, 1, 3);
                    player.SetClothes(11, Core.Customization.CorrectTorso[gender][14], 0);
                }

                Trigger.PlayerEvent(player, "createCheckpoint", 4, 1, MowerWays[way][0] - new Vector3(0, 0, 1.12), 2, 0, 0, 0, 255, MowerWays[way][1] - new Vector3(0, 0, 1.12));
                Trigger.PlayerEvent(player, "createWaypoint", MowerWays[way][0].X, MowerWays[way][0].Y);
            }
            else
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в транспорте", 3000);
            }
        }

        private static void mowerCheckpointEnterWay(ColShape shape, Player player)
        {
            try
            {
                if (!NAPI.Player.IsPlayerInAnyVehicle(player)) return;
                var vehicle = player.Vehicle;
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "TRACTOR" || Main.Players[player].WorkID != 9 || !player.GetData<bool>("ON_WORK") || player.GetData<int>("WORKWAY") != shape.GetData<int>("WAY") || player.GetData<int>("WORKCHECK") != shape.GetData<int>("NUMBER"))
                    return;

                int way = player.GetData<int>("WORKWAY");
                int check = NAPI.Data.GetEntityData(player, "WORKCHECK");

                var payment = Convert.ToInt32((checkpointPayment + Main.AddJobPoint(player) * 5) * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);

                int level = Main.Players[player].LVL > 5 ? 12 : 2 * Main.Players[player].LVL;

                player.SetData("PAYMENT", player.GetData<int>("PAYMENT") + payment + level);

                player.SendNotification($"Тракторист: ~h~~g~+{payment + level}$", true);

                Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));

                //GameLog.Money($"server", $"player({Main.Players[player].UUID})", payment, $"lawnCheck");

                if (check + 1 != MowerWays[way].Count)
                {
                    var direction = (check + 2 != MowerWays[way].Count) ? MowerWays[way][check + 2] : MowerWays[way][0] - new Vector3(0, 0, 1.12);
                    Trigger.PlayerEvent(player, "createCheckpoint", 4, 1, MowerWays[way][check + 1] - new Vector3(0, 0, 1.12), 2, 0, 0, 0, 255, direction);
                    Trigger.PlayerEvent(player, "createWaypoint", MowerWays[way][check + 1].X, MowerWays[way][check + 1].Y);
                    NAPI.Data.SetEntityData(player, "WORKCHECK", check + 1);
                }
                else
                {
                    var next_way = 0;
                    Trigger.PlayerEvent(player, "createCheckpoint", 4, 1, MowerWays[next_way][0] - new Vector3(0, 0, 1.12), 2, 0, 255, 0, 0, MowerWays[next_way][1] - new Vector3(0, 0, 1.12));
                    Trigger.PlayerEvent(player, "createWaypoint", MowerWays[next_way][0].X, MowerWays[next_way][0].Y);
                    NAPI.Data.SetEntityData(player, "WORKCHECK", 0);
                    NAPI.Data.SetEntityData(player, "WORKWAY", next_way);
                }
            }
            catch (Exception ex) { Log.Write("mowerCheckpointEnterWay: " + ex.Message, nLog.Type.Error); }
        }
    }
}
