using GTANetworkAPI;
using System;
using System.Linq;
using System.Collections.Generic;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using Newtonsoft.Json;
using NeptuneEVO.GUI;

namespace NeptuneEVO.Jobs
{
    class Truckers : Script
    {
        //private static int JobMultiper = 6;
        private static nLog Log = new nLog("Truckers");
        private static int lastcar = -1;

        private static List<Vector3> carspawns = new List<Vector3>
        {
            new Vector3(574.7359, -3035.08, 4.949285),
            new Vector3(568.2927, -3035.08, 4.949285),
            new Vector3(561.8536, -3035.08, 4.949286),
        };

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try
            {
                for (int i = 0; i < getProduct.Count; i++)
                {
                    cols.Add(NAPI.ColShape.CreateCylinderColShape(getProduct[i], 5f, 6f, 0));
                    cols[i].OnEntityEnterColShape += onEntityEnterGetProduct;
                    cols[i].OnEntityExitColShape += onEntityExitGetProduct;
                    cols[i].SetData("PROD", i);
                    NAPI.Marker.CreateMarker(1, getProduct[i] - new Vector3(0, 0, 1.2), new Vector3(), new Vector3(), 5f, new Color(255, 255, 255, 220));
                    NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Доставка"), new Vector3(getProduct[i].X, getProduct[i].Y, getProduct[i].Z + 1), 30f, 0.5f, 0, new Color(255, 255, 255));
                }

                ColShape Shape = NAPI.ColShape.CreateCylinderColShape(new Vector3(596.5147, -3046.633, -15.049733), 1F, 2F); //596.5147, -3046.633, 5.049733
                NAPI.Marker.CreateMarker(27, new Vector3(596.5147, -3046.633, -15.049733) + new Vector3(0, 0, 0.2f), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220), false, 0); //596.5147, -3046.633, 5.049733
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Взять грузовик"), new Vector3(596.5147, -3046.633, -15.049733 + 0), 30f, 0.5f, 0, new Color(255, 255, 255)); //596.5147, -3046.633, 5.049733 + 1.2
                Shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 223);
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

            } catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }   
        }

        public static Vector3 getTruckPos()
        {
            if (lastcar + 1 >= carspawns.Count)
                lastcar = 0;
            else
                lastcar += 1;
            return carspawns[lastcar];
        }

        public static void truckSpawn(Player player)
        {
            var veh = NAPI.Vehicle.CreateVehicle(VehicleHash.Pounder2, getTruckPos(), 3, 0, 0, "TRUCKER");
            Core.VehicleStreaming.SetEngineState(veh, false);
            NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
            NAPI.Data.SetEntityData(veh, "WORK", 6);
            NAPI.Data.SetEntityData(veh, "TYPE", "TRUCKER");
            NAPI.Data.SetEntityData(veh, "NUMBER", "TRUCKER");
            NAPI.Data.SetEntityData(veh, "ON_WORK", true);
            NAPI.Data.SetEntityData(veh, "DRIVER", player);
            player.SetData("PAYMENT", 0);
            Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));
            veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
            player.ResetData("WayPointBiz");
            player.SetData("WORK", veh);
            player.SetData("ON_WORK", true);
            NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
            Trigger.PlayerEvent(player, "createWaypoint", getProduct[0].X, getProduct[0].Y);
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Ваш грузовик ожидает на парковке", 3000);
        }

        private static List<ColShape> cols = new List<ColShape>();
        public static List<Vector3> getProduct = new List<Vector3>()
        {
            new Vector3(269.2824, -3015.241, -14.599093) //269.2824, -3015.241, 4.599093
        };

        public static List<Vector3> UnloadPoints = new List<Vector3>()
        {
            new Vector3(2912.352, 4382.515, 49.16564),
            new Vector3(2682.344, 3446.881, 54.68411), 
            new Vector3(2538.11, 2584.708, 36.82092), 
            new Vector3(2753.11, 1443.242, 23.36453),       
            new Vector3(2551.9, 418.1792, 107.339),
            new Vector3(1711.65, -1470.693, 111.845),
            new Vector3(1200.187, -1493.241, 33.56733), 
            new Vector3(1013.324, -1845.742, 30.19025),      
            new Vector3(837.0986, -1935.611, 27.84735),
            new Vector3(728.9784, -2099.882, 28.16317),    
            new Vector3(499.3904, -2167.005, 4.793656),       
            new Vector3(592.0522, -2771.608, 4.931742),    
            new Vector3(-347.8799, -2615.808, 4.874506),       
            new Vector3(-842.402, -2668.795, 12.68766),  
            new Vector3(-1032.224, -2218.355, 7.85603),    
        };

        public static List<List<Vector3>> SpawnTrailers = new List<List<Vector3>>()
        {
            new List<Vector3>()
            {
                new Vector3(262.4323, -3017.513, 4.633697),
                new Vector3(260.4323, -3017.513, 4.633697),
                new Vector3(264.4323, -3017.513, 4.633697),
            }, 
        };
        public static List<List<Vector3>> SpawnTrailersRot = new List<List<Vector3>>()
        {
            new List<Vector3>()
            {
                new Vector3(0, 0, 262.673),
                new Vector3(0, 0, 262.673),
                new Vector3(0, 0, 262.673),
            }, 
        };
        public static List<int> LastTrailerSpawn = new List<int>()
        {
            0, 0, 0, 0
        };

        public static void cancelOrder(Player player)
        {
            try
            {
                if (player.HasData("ORDER"))
                {
                    player.ResetData("ORDER");
                }
                
                player.ResetData("ORDERDATE");
            }
            catch (Exception e) { Log.Write("CancelOrder: " + e.Message, nLog.Type.Error); }
        }
        
        public static void getOrderTrailer(Player player)
        {
            if (Main.Players[player].WorkID != 6)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете дальнобойщиком", 3000);
                return;
            }
            if (!player.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в транспорте", 3000);
                return;
            }
            if (player.HasData("ORDER"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже взяли заказ", 3000);
                return;
            }
            if (player.HasData("ON_WORK") && !player.GetData<bool>("ON_WORK") || !player.HasData("ON_WORK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете", 3000);
                return;
            }
            Trigger.PlayerEvent(player, "openDialog", "TRUCKER_TRAILER", $"Вы хотите взять заказ?");
            return;
        }
        public static void onPlayerDissconnectedHandler(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID == 6 &&
                    NAPI.Data.GetEntityData(player, "WORK") != null)
                {
                    var vehicle = NAPI.Data.GetEntityData(player, "WORK");
                    vehicle.Delete();

                    MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                    Golemo.Families.Family.GiveMoneyOnJob(player, player.GetData<int>("PAYMENT"));

                    Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                    player.SetData("PAYMENT", 0);

                    cancelOrder(player);
                    return;
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        private void onEntityEnterGetProduct(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 31);
                NAPI.Data.SetEntityData(entity, "PROD", shape.GetData<int>("PROD"));
            }
            catch (Exception ex) { Log.Write("onEntityEnterGetProduct: " + ex.Message, nLog.Type.Error); }
        }
        private void onEntityExitGetProduct(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception ex) { Log.Write("onEntityExitGetProduct: " + ex.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "TRUCKER") return;
                if (player.VehicleSeat == 0)
                {
                    if (Main.Players[player].WorkID == 6)
                    {
                        if (!NAPI.Data.GetEntityData(vehicle, "ON_WORK"))
                        {
                            if (NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                            {
                                NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                            }

                        }
                        else
                        {
                            if (NAPI.Data.GetEntityData(player, "WORK") != vehicle)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У машины есть водитель", 3000);
                                VehicleManager.WarpPlayerOutOfVehicle(player);
                                vehicle.Delete();
                            }
                            else
                            {
                                NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                                if (player.HasData("ORDER") && !player.HasData("GOTPRODUCT"))
                                {
                                    playerGotProducts(player);
                                }
                            }
                        }
                    }
                    else
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Устроиться дальнобойщиком можно у начальника", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                        vehicle.Delete();
                    }
                }
            } catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }   
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") == "TRUCKER" &&
                Main.Players[player].WorkID == 6 &&
                NAPI.Data.GetEntityData(player, "ON_WORK") &&
                NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Если Вы не сядете в транспорт через 5 минут, то рабочий день закончится", 3000);
                    NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "WORK_CAR_EXIT_TIMER_truckers_1");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", 0);
                    //NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Main.StartT(1000, 1000, (o) => timer_playerExitWorkVehicle(player, vehicle), "TRUCK_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Timers.Start(1000, () => timer_playerExitWorkVehicle(player, vehicle)));
                }
            } catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }   
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
                        if (player.HasData("TRAILER"))
                        {
                            int uid = player.GetData<int>("ORDER");
                            if (player.HasData("WayPointBiz"))
                            {
                                    Trigger.PlayerEvent(player, "createWaypoint", UnloadPoints[uid].X, UnloadPoints[uid].Y);
                            }
                            return;
                        }
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                    }
                    if (NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") > 300)
                    {
                        vehicle.Delete();
                        Trigger.PlayerEvent(player, "SetOrderTruck", null);
                        player.ResetData("WayPointBiz");

                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили зарплату в размере: {player.GetData<int>("PAYMENT")}$", 3000);

                        MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                        Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                        Golemo.Families.Family.GiveMoneyOnJob(player, player.GetData<int>("PAYMENT"));

                        player.SetData("PAYMENT", 0);

                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);
                        NAPI.Data.SetEntityData(player, "ON_WORK", false);
                        NAPI.Data.SetEntityData(player, "WORK", null);
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        cancelOrder(player);
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") + 1);

                } catch(Exception e)
                {
                    Log.Write("Timer_PlayerExitWorkVehicle_Truckers: \n" + e.ToString(), nLog.Type.Error);
                }
            });
        }

        public static void playerGotProducts(Player player)
        {
            try
            {
                if (!player.HasData("ORDER") || !player.IsInVehicle) return;

                Vehicle veh = NAPI.Entity.GetEntityFromHandle<Vehicle>(player.Vehicle);
                if (!veh.HasData("ACCESS") || veh.GetData<string>("ACCESS") != "WORK" || !player.HasData("WORK") || player.GetData<Vehicle>("WORK") != veh) return;

                int uid = player.GetData<int>("ORDER");

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Довезите товар до выгрузки отмеченной в GPS [{uid}]", 3000);

                player.SetData("GOTPRODUCT", true);
                var shape = NAPI.ColShape.CreateCylinderColShape(UnloadPoints[uid], 5f, 3, 0);
                shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        onEntityEnterDropTrailer(shape, player);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                shape.SetData("ORDERID", uid);
                //player.SetData("SHAPE", shape);
                Trigger.PlayerEvent(player, "createRoute", UnloadPoints[uid].X, UnloadPoints[uid].Y);
                Trigger.PlayerEvent(player, "createCheckpoint", 10, 1, UnloadPoints[uid], 7, 0, 255, 0, 0);
            }
            catch (Exception ex)
            {
                Log.Write($"Error: {ex.Message} ", nLog.Type.Error);
            }
        }

        public static void onEntityEnterDropTrailer(ColShape shape, Player player)
        {
            try
            {
                if (!player.HasData("ORDER") || player.GetData<int>("ORDER") != shape.GetData<int>("ORDERID") || !player.HasData("GOTPRODUCT") || !player.IsInVehicle || player.Vehicle != player.GetData<Vehicle>("WORK")) return;

                int uid = player.GetData<int>("ORDER");

                var goshape = Convert.ToInt32(player.Position.DistanceTo2D(getProduct[0]) / 100) * 25;

                int level = Main.Players[player].LVL > 5 ? 150 : 25 * Main.Players[player].LVL;

                player.SetData("PAYMENT", player.GetData<int>("PAYMENT") + (level + goshape + Main.AddJobPoint(player) * 50));
                Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));

                player.ResetData("WayPointBiz");
                
                Trigger.PlayerEvent(player, "deleteCheckpoint", 10);
				Trigger.PlayerEvent(player, "removeRoute");
                player.ResetData("GOTPRODUCT");
                player.ResetData("ORDER");
                shape.Delete();

                player.SendNotification($"Доставка: ~h~~g~+{(level + goshape + Main.AddJobPoint(player) * 50) * Main.Multipy}$", true);
                return;
            }
            catch (Exception e) { Log.Write("onEntityDropTrailer_ATTENTION: " + e.Message, nLog.Type.Error); }
        }
    }
}
