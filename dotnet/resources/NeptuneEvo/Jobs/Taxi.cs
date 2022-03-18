using GTANetworkAPI;
using System.Collections.Generic;
using NeptuneEVO.GUI;
using System;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Jobs
{
    class Taxi : Script
    {
        public static List<CarInfo> CarInfos = new List<CarInfo>();
        private static nLog Log = new nLog("Taxi");

        public static int taxiRentCost = 20;
        private static Dictionary<Player, ColShape> orderCols = new Dictionary<Player, ColShape>();

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try
            {

                ColShape Shape = NAPI.ColShape.CreateCylinderColShape(new Vector3(899.9929, -173.4167, 72.89657), 1F, 2F);
                NAPI.Marker.CreateMarker(27, new Vector3(899.9929, -173.4167, 72.89657) + new Vector3(0, 0, 0.2f), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220), false, 0);
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Взять такси"), new Vector3(899.9929, -173.4167, 72.89657 + 1.2), 30f, 0.5f, 0, new Color(255, 255, 255));
                Shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 225);
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

        private static int lastcar = -1;

        private static List<Vector3> carspawns = new List<Vector3>
        {
            new Vector3(907.6475, -182.396, 73.06146),
            new Vector3(905.9452, -185.4035, 72.90132),
            new Vector3(904.3374, -188.1086, 72.69463),
        };

        public static Vector3 getTaxiPos()
        {
            if (lastcar + 1 >= carspawns.Count)
                lastcar = 0;
            else
                lastcar += 1;
            return carspawns[lastcar];
        }

        public static void taxispawn(Player player)
        {
            var veh = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey("taxi"), getTaxiPos(), 235, 42, 42, "TAXI");
            NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
            NAPI.Data.SetEntityData(veh, "WORK", 3);
            NAPI.Data.SetEntityData(veh, "TYPE", "TAXI");
            NAPI.Data.SetEntityData(veh, "NUMBER", "TAXI");
            NAPI.Data.SetEntityData(veh, "DRIVER", null);
            NAPI.Data.SetEntityData(veh, "ON_WORK", false);
            NAPI.Data.SetEntityData(player, "WORK", veh);
            veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
            Core.VehicleStreaming.SetEngineState(veh, false);
            Core.VehicleStreaming.SetLockStatus(veh, false);
            player.SetIntoVehicle(veh, 0);
            taxiRent(player);
            
        }

        public static void taxiRent(Player player)
        { 

                    var vehicle = player.GetData<Vehicle>("WORK");
                    NAPI.Data.SetEntityData(player, "WORK", vehicle);
                    NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);

                    NAPI.Data.SetEntityData(vehicle, "DRIVER", player);
                    vehicle.SetData("ON_WORK", true);

                    if (!MoneySystem.Wallet.Change(player, -taxiRentCost))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно денег", 3000);
                        return;
                    }
                    //GameLog.Money($"player({Main.Players[player].UUID})", $"server", taxiRentCost, $"taxiRent");
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы арендовали такси. Чтобы предложить Гражданину оплатить проезд, напишите /tprice [ID] [Цена]", 5000);
                    Core.VehicleStreaming.SetEngineState(vehicle, false);


        }

        public static void taxiPay(Player player)
        {
            var seller = player.GetData<Player>("TAXI_SELLER");
            var price = player.GetData<int>("TAXI_PAY");

            if (!Main.Players.ContainsKey(seller)) return;

            if (!MoneySystem.Wallet.Change(player, -price))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно денег", 3000);
                return;
            }
            MoneySystem.Wallet.Change(seller, price);
            //GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[seller].UUID})", taxiRentCost, $"taxiPay");
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы оплатили проезд", 3000);
            Notify.Send(seller, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин " + player.Name.Replace('_', ' ') + " оплатил проезд", 3000);
        }

        private static void order_onEntityExit(ColShape shape, Player player)
        {
            try
            {
                if (shape.GetData<Player>("PASSAGER") != player) return;

                if (player.HasData("TAXI_DRIVER"))
                {
                    Player driver = player.GetData<Player>("TAXI_DRIVER");
                    driver.ResetData("PASSAGER");
                    player.ResetData("TAXI_DRIVER");
                    player.SetData("IS_CALL_TAXI", false);
                    Notify.Send(driver, NotifyType.Warning, NotifyPosition.BottomCenter, $"Пассажир отменил заказ", 3000);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы покинули место вызова такси", 3000);
                    try
                    {
                        NAPI.ColShape.DeleteColShape(orderCols[player]);
                        orderCols.Remove(player);
                    }
                    catch { }
                }
            }
            catch (Exception ex) { Log.Write("order_onEntityExit: " + ex.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "TAXI") return;
                if (seatid == -1)
                {
                    if (Main.Players[player].WorkID == 3)
                    {
                        if (NAPI.Data.GetEntityData(player, "WORK") == null)
                        {
                            if (vehicle.GetData<Player>("DRIVER") != null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Это такси уже занят", 3000);
                                return;
                            }
                        }
                        else if (NAPI.Data.GetEntityData(player, "WORK") == vehicle) NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                        else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже работаете", 3000);
                    }
                    else
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете в такси. Устроиться можно в таксопарке", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                    }
                }
                else
                {
                    if (NAPI.Data.GetEntityData(vehicle, "DRIVER") != null)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Если Вы хотите передать свой маршрут водителю, то поставьте метку на карте и нажмите Z.", 5000);
                        var driver = NAPI.Data.GetEntityData(vehicle, "DRIVER");
                        if (driver.HasData("PASSAGER") && driver.GetData("PASSAGER") == player)
                        {
                            driver.ResetData("PASSAGER");
                            player.SetData("IS_CALL_TAXI", false);
                            player.ResetData("TAXI_DRIVER");
                            try
                            {
                                NAPI.ColShape.DeleteColShape(orderCols[player]);
                                orderCols.Remove(player);
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Водитель отошел", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }

        public static void onPlayerDissconnectedHandler(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (player.HasData("TAXI_DRIVER"))
                {
                    Player driver = player.GetData<Player>("TAXI_DRIVER");
                    driver.ResetData("PASSAGER");
                    Notify.Send(driver, NotifyType.Warning, NotifyPosition.BottomCenter, $"Пассажир отменил заказ", 3000);
                    try
                    {
                        NAPI.ColShape.DeleteColShape(orderCols[player]);
                        orderCols.Remove(player);
                    }
                    catch { }
                }
                if (Main.Players[player].WorkID == 3 && NAPI.Data.GetEntityData(player, "WORK") != null)
                {
                    var vehicle = NAPI.Data.GetEntityData(player, "WORK");
                    NAPI.Task.Run(() => { try { vehicle.Delete(); } catch { } });
                    if (player.HasData("PASSAGER"))
                    {
                        Player passager = player.GetData<Player>("PASSAGER");
                        passager.ResetData("TAXI_DRIVER");
                        passager.SetData("IS_CALL_TAXI", false);
                        Notify.Send(passager, NotifyType.Warning, NotifyPosition.BottomCenter, $"Таксист покинул рабочее место, сделайте новый заказ", 3000);
                        NAPI.Task.Run(() => {
                            try
                            {
                                NAPI.ColShape.DeleteColShape(orderCols[passager]);
                                orderCols.Remove(passager);
                            }
                            catch { }
                        });
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "WORK" &&
                Main.Players[player].WorkID == 3 &&
                NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Если Вы не сядете в транспорт через 5 минут, то рабочий день закончится", 3000);
                    NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "WORK_CAR_EXIT_TIMER_taxi_1");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", 0);
                    //NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Main.StartT(1000, 1000, (o) => timer_playerExitWorkVehicle(player, vehicle), "TAXI_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Timers.StartTask(1000, () => timer_playerExitWorkVehicle(player, vehicle)));
                }
            }
            catch (Exception e) { Log.Write("PlayerExit: " + e.Message, nLog.Type.Error); }
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
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "WORK_CAR_EXIT_TIMER_taxi_2");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        return;
                    }
                    if (NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") > 300)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);
                        vehicle.Delete();
                        player.SetData("ON_WORK", false);
                        player.ResetData("WORK");
                        //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "WORK_CAR_EXIT_TIMER_taxi_3");
                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        if (player.HasData("PASSAGER"))
                        {
                            Player passager = player.GetData<Player>("PASSAGER");
                            passager.ResetData("TAXI_DRIVER");
                            passager.SetData("IS_CALL_TAXI", false);
                            Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Таксист покинул рабочее место, сделайте новый заказ", 3000);
                            player.ResetData("PASSAGER");
                            try
                            {
                                NAPI.ColShape.DeleteColShape(orderCols[passager]);
                                orderCols.Remove(passager);
                            }
                            catch { }
                        }
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") + 1);
                }
                catch (Exception e) { Log.Write("taxi_exitVehicleTimer: " + e.Message); }
            });
        }

        public static void offerTaxiPay(Player player, Player target, int price)
        {
            if (Main.Players[player].WorkID == 3)
            {
                if (NAPI.Data.GetEntityData(player, "WORK") != null)
                {
                    if (!target.IsInVehicle || player.Position.DistanceTo(target.Position) > 2) return;
                    if (!NAPI.Player.IsPlayerInAnyVehicle(player) || player.VehicleSeat != 0 || player.Vehicle != player.GetData<Vehicle>("WORK") || player.Vehicle != target.Vehicle) return;
                    var vehicle = player.Vehicle;
                    if (NAPI.Data.GetEntityData(vehicle, "TYPE") == "TAXI")
                    {
                        if (price > 1200 || price < 1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете установить цену ниже 1$ и выше 1200$", 3000);
                            return;
                        }
                        if (Main.Players[target].Money < price)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У гражданина недостаточно средств", 3000);
                            return;
                        }

                        Trigger.PlayerEvent(target, "openDialog", "TAXI_PAY", $"Оплатить проезд за ${price}?");
                        target.SetData("TAXI_SELLER", player);
                        target.SetData("TAXI_PAY", price);

                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили гражданину ({target.Value}) оплатить поездку за {price}$", 3000);
                    }
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работает в данный момент", 3000);
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете в такси", 3000);
        }

        public static void acceptTaxi(Player player, Player target)
        {
            if (Main.Players[player].WorkID == 3 && NAPI.Data.GetEntityData(player, "WORK") != null)
            {
                if (player.HasData("PASSAGER"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже взяли заказ", 3000);
                    return;
                }
                if (NAPI.Data.GetEntityData(target, "IS_CALL_TAXI") && !target.HasData("TAXI_DRIVER"))
                {
                    Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Таксист ({player.Value}) принял Ваш вызов. Ожидайте", 3000);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы приняли вызов Гражданина ({target.Value})", 3000);
                    Trigger.PlayerEvent(player, "createWaypoint", NAPI.Entity.GetEntityPosition(target).X, NAPI.Entity.GetEntityPosition(target).Y);

                    target.SetData("TAXI_DRIVER", player);
                    player.SetData("PASSAGER", target);

                    orderCols.Add(target, NAPI.ColShape.CreateCylinderColShape(target.Position, 10F, 10F, 0));
                    orderCols[target].SetData("PASSAGER", target);
                    orderCols[target].OnEntityExitColShape += order_onEntityExit;
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин не вызывал такси или его уже приняли", 3000);
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В данный момент Вы не работаете в такси", 3000);
        }

        public static void cancelTaxi(Player player)
        {
            if (player.HasData("PASSAGER"))
            {
                Player passager = player.GetData<Player>("PASSAGER");
                passager.ResetData("TAXI_DRIVER");
                passager.SetData("IS_CALL_TAXI", false);
                player.ResetData("PASSAGER");
                Notify.Send(passager, NotifyType.Warning, NotifyPosition.BottomCenter, $"Таксист покинул рабочее место, сделайте новый заказ", 3000);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы отменили выезд к клиенту", 3000);
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        NAPI.ColShape.DeleteColShape(orderCols[passager]);
                        orderCols.Remove(passager);
                    }
                    catch { }
                });

                return;
            }
            if (NAPI.Data.GetEntityData(player, "IS_CALL_TAXI"))
            {
                NAPI.Data.SetEntityData(player, "IS_CALL_TAXI", false);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы отменили вызов такси", 3000);
                if (player.HasData("TAXI_DRIVER"))
                {
                    Player driver = player.GetData<Player>("TAXI_DRIVER");
                    driver.ResetData("PASSAGER");
                    player.ResetData("TAXI_DRIVER");
                    Notify.Send(driver, NotifyType.Warning, NotifyPosition.BottomCenter, $"Пассажир отменил заказ", 3000);
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            NAPI.ColShape.DeleteColShape(orderCols[player]);
                            orderCols.Remove(player);
                        }
                        catch { }
                    });
                }
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не вызывали такси.", 3000);
        }

        public static void callTaxi(Player player)
        {
            if (!NAPI.Data.GetEntityData(player, "IS_CALL_TAXI"))
            {
                List<Player> players = NAPI.Pools.GetAllPlayers();
                var i = 0;
                foreach (var p in players)
                {
                    if (p == null || !Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].WorkID == 3 && NAPI.Data.GetEntityData(p, "WORK") != null)
                    {
                        i++;
                        NAPI.Chat.SendChatMessageToPlayer(p, $"~b~[ДИСПЕТЧЕР]: ~w~Гражданин ({player.Value}) вызвал такси ~y~({player.Position.DistanceTo(p.Position)}м)~w~. Напишите ~y~/ta ~b~[ID]~w~, чтобы принять вызов");
                    }
                }
                if (i > 0)
                {
                    NAPI.Data.SetEntityData(player, "IS_CALL_TAXI", true);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ожидайте принятия вызова. В Вашем районе сейчас {i} таксистов. Для отмены вызова используйте /ctaxi", 3000);
                }
                else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В Вашем районе сейчас нет таксистов. Попробуйте в другой раз", 3000);
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже Вызвали такси. Для отмены напишите /ctaxi", 3000);
        }
    }
}
