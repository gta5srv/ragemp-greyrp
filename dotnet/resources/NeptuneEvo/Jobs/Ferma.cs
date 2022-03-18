using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.GUI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Jobs
{
    class FermaWork : Script
    {
        public static int id = 19; // число может меняться в зависимости от кол-ва фракций в вашем моде 

        #region Values
        private static int ColsCount = 0;
        public static int FermaLeft = 235000;
        public static int CowTimer = 175; // Время (в секундах), за которое будет производиться молоко
        public static int ProdTimer = 300; // Время-ограничение, которое делает бессмысленным фарм с помощью читов (в секундах)

        public static int FermaKultLeft = 7500;
        public static List<FermaWork> FermaKult = new List<FermaWork>();
        public static bool Kult = false;
        #endregion Values

        #region Other
        private static nLog Log = new nLog("Ferma");

        public static Dictionary<int, ColShape> Ferma = new Dictionary<int, ColShape>();
        public static List<GTANetworkAPI.Object> FermaObj = new List<GTANetworkAPI.Object>();

        private static List<int> dbIDs = new List<int>();
        public Vector3 position { get; set; }
        #endregion Other

        public static void OnSpawnCar(Player player)
        {
            try
            {
                if (FermaLeft != 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Поле не нуждается в культивации", 5000);
                    return;
                }
                if (player.HasData("TRACTOR"))
                {
                    NAPI.Task.Run(() => { try { player.GetData<Vehicle>("TRACTOR").Delete(); player.ResetData("TRACTOR"); } catch { } });
                    return;
                }
                Vehicle veh = NAPI.Vehicle.CreateVehicle(VehicleHash.Tractor3, new Vector3(2020.6418, 4961.6523, 42.06095), new Vector3(0, 0, -46.37), 1, 1);
                NAPI.Task.Run(() => { try { } catch { VehicleStreaming.SetEngineState(veh, true); veh.EngineStatus = true; } }, 500);
                VehicleStreaming.SetLockStatus(veh, false);
                veh.SetData("TRACTOR", true);
                veh.SetData("DRIVER", player);
                veh.SetData("ACCESS", "RENT");
                veh.SetData("TYPE", "TRACTOR2");
                player.SetData("TRACTOR", veh);
                player.SetIntoVehicle(veh, 0);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Начинайте культивировать поле", 5000);
            }
            catch { }
        }


        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {

           #region StartTimers
            Timers.StartTask("checktractor", 180000, () => CheckTractor()); // Проверка тракторов
            Timers.StartTask("checkprodcar", 480000, () => CheckProdCar()); // Проверка машин для перевозки продукции
            Timers.StartTask("resetdataplants", 60000, () => DestroyAllTree()); // Очистка фермы раз в час
          //  Timers.StartTask("resetdataplants_pred", 5400000, () => DestroyAllTree_pred()); // Очистка фермы раз в 2 час - Предупреждение
            #endregion StartTimers

            #region GPrice
            // Генерация цены для продукции
            var rnd = new Random();
            int intrnd1 = rnd.Next(15, 55);
            int intrnd2 = rnd.Next(55, 95);
            int intrnd3 = rnd.Next(95, 120);
            int intrnd4 = rnd.Next(120, 170);



            Commands.Kartofel = intrnd1;
            Commands.Psenica = intrnd2;
            Commands.Morkov = intrnd3;
            Commands.Milk = intrnd4;
            #endregion GPrice

            #region cols
            #endregion cols

            var col = NAPI.ColShape.CreateCylinderColShape(FermaCheckpoints[0] + new Vector3(0, 0, 1.8f), 1, 2, 0);
            col.SetData("INTERACT", 7825);
            col.OnEntityEnterColShape += FermaShape_onEntityEnterColShape;
            col.OnEntityExitColShape += FermaShape_onEntityExitColShape;
            NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Продажа продукции"), new Vector3(FermaCheckpoints[0].X, FermaCheckpoints[0].Y, FermaCheckpoints[0].Z + 2.35), 5F, 0.3F, 0, new Color(255, 255, 255));

            /*col = NAPI.ColShape.CreateCylinderColShape(FermaCheckpoints[1], 2, 3, 0);
            col.SetData("INTERACT", 7826);
            col.OnEntityEnterColShape += FermaShape_onEntityEnterColShape;
            col.OnEntityExitColShape += FermaShape_onEntityExitColShape;
            NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Загрузить продукцию"), new Vector3(FermaCheckpoints[1].X, FermaCheckpoints[1].Y, FermaCheckpoints[1].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

            col = NAPI.ColShape.CreateCylinderColShape(FermaCheckpoints[2], 3, 3, 0);
            col.SetData("INTERACT", 7827);
            col.OnEntityEnterColShape += FermaShape_onEntityEnterColShape;
            col.OnEntityExitColShape += FermaShape_onEntityExitColShape;
            NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Разгрузка продукции"), new Vector3(FermaCheckpoints[2].X, FermaCheckpoints[2].Y, FermaCheckpoints[2].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));
            
            col = NAPI.ColShape.CreateCylinderColShape(FermaCheckpoints[3], 1, 2, 0);
            col.SetData("INTERACT", 7828);
            col.OnEntityEnterColShape += FermaShape_onEntityEnterColShape;
            col.OnEntityExitColShape += FermaShape_onEntityExitColShape;
            NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Взять комплексный обед"), new Vector3(FermaCheckpoints[3].X, FermaCheckpoints[3].Y, FermaCheckpoints[3].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

            col = NAPI.ColShape.CreateCylinderColShape(FermaCheckpoints[4], 1, 2, 0);
            col.SetData("INTERACT", 7829);
            col.OnEntityEnterColShape += FermaShape_onEntityEnterColShape;
            col.OnEntityExitColShape += FermaShape_onEntityExitColShape;
            NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Сбыт продовольствия"), new Vector3(FermaCheckpoints[4].X, FermaCheckpoints[4].Y, FermaCheckpoints[4].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));
            */
            #region CowShape

            col = NAPI.ColShape.CreateCylinderColShape(CowCheckpoints[0], 1, 2, 0);
            col.SetData("INTERACT", 70001);
            col.OnEntityEnterColShape += FermaShape_onEntityEnterColShape;
            col.OnEntityExitColShape += FermaShape_onEntityExitColShape;
            NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Коровник ~n~ ~o~#1"), new Vector3(CowCheckpoints[0].X, CowCheckpoints[0].Y, CowCheckpoints[0].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

            col = NAPI.ColShape.CreateCylinderColShape(CowCheckpoints[1], 1, 2, 0);
            col.SetData("INTERACT", 70002);
            col.OnEntityEnterColShape += FermaShape_onEntityEnterColShape;
            col.OnEntityExitColShape += FermaShape_onEntityExitColShape;
            NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Коровник ~n~ ~o~#2"), new Vector3(CowCheckpoints[1].X, CowCheckpoints[1].Y, CowCheckpoints[1].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

            col = NAPI.ColShape.CreateCylinderColShape(CowCheckpoints[2], 1, 2, 0);
            col.SetData("INTERACT", 70003);
            col.OnEntityEnterColShape += FermaShape_onEntityEnterColShape;
            col.OnEntityExitColShape += FermaShape_onEntityExitColShape;
            NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Коровник ~n~ ~o~#3"), new Vector3(CowCheckpoints[2].X, CowCheckpoints[2].Y, CowCheckpoints[2].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

            col = NAPI.ColShape.CreateCylinderColShape(CowCheckpoints[3], 1, 2, 0);
            col.SetData("INTERACT", 70004);
            col.OnEntityEnterColShape += FermaShape_onEntityEnterColShape;
            col.OnEntityExitColShape += FermaShape_onEntityExitColShape;
            NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Коровник ~n~ ~o~#4"), new Vector3(CowCheckpoints[3].X, CowCheckpoints[3].Y, CowCheckpoints[3].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

            col = NAPI.ColShape.CreateCylinderColShape(CowCheckpoints[4], 1, 2, 0);
            col.SetData("INTERACT", 70005);
            col.OnEntityEnterColShape += FermaShape_onEntityEnterColShape;
            col.OnEntityExitColShape += FermaShape_onEntityExitColShape;
            NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Коровник ~n~ ~o~#5"), new Vector3(CowCheckpoints[4].X, CowCheckpoints[4].Y, CowCheckpoints[4].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

            col = NAPI.ColShape.CreateCylinderColShape(CowCheckpoints[5], 1, 2, 0);
            col.SetData("INTERACT", 70006);
            col.OnEntityEnterColShape += FermaShape_onEntityEnterColShape;
            col.OnEntityExitColShape += FermaShape_onEntityExitColShape;
            NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Коровник ~n~ ~o~#6"), new Vector3(CowCheckpoints[5].X, CowCheckpoints[5].Y, CowCheckpoints[5].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

            col = NAPI.ColShape.CreateCylinderColShape(CowCheckpoints[6], 1, 2, 0);
            col.SetData("INTERACT", 70007);
            col.OnEntityEnterColShape += FermaShape_onEntityEnterColShape;
            col.OnEntityExitColShape += FermaShape_onEntityExitColShape;
            NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Коровник ~n~ ~o~#7"), new Vector3(CowCheckpoints[6].X, CowCheckpoints[6].Y, CowCheckpoints[6].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

            col = NAPI.ColShape.CreateCylinderColShape(CowCheckpoints[7], 1, 2, 0);
            col.SetData("INTERACT", 70008);
            col.OnEntityEnterColShape += FermaShape_onEntityEnterColShape;
            col.OnEntityExitColShape += FermaShape_onEntityExitColShape;
            NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Коровник ~n~ ~o~#8"), new Vector3(CowCheckpoints[7].X, CowCheckpoints[7].Y, CowCheckpoints[7].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));

            col = NAPI.ColShape.CreateCylinderColShape(CowCheckpoints[8], 1, 2, 0);
            col.SetData("INTERACT", 70009);
            col.OnEntityEnterColShape += FermaShape_onEntityEnterColShape;
            col.OnEntityExitColShape += FermaShape_onEntityExitColShape;
            NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Коровник ~n~ ~o~#9"), new Vector3(CowCheckpoints[8].X, CowCheckpoints[8].Y, CowCheckpoints[8].Z + 1), 5F, 0.3F, 0, new Color(255, 255, 255));


            #endregion CowShape

            #region Shapes
            var plantshape = NAPI.ColShape.CreateCylinderColShape(new Vector3(2038.6017, 4908.0933, 39.536266), 54, 10, 0);

            plantshape.OnEntityEnterColShape += (s, e) =>
            {
                if (!Main.Players.ContainsKey(e)) return;
                e.SetData("CANPLANT", plantshape);
            };
            plantshape.OnEntityExitColShape += (s, e) =>
            {
                if (!Main.Players.ContainsKey(e)) return;
                e.ResetData("CANPLANT");
            };

            /* var fermazone = NAPI.ColShape.CreateCylinderColShape(new Vector3(2038.6017, 4908.0933, 39.536266), 170, id, 0);
             fermazone.OnEntityExitColShape += (s, e) =>
             {
                 try
                 {
                     if (!Main.Players.ContainsKey(e)) return;
                     if (e.Vehicle.HasData("TRACTOR"))
                     {
                         e.ResetData("TRACTOR");
                         NAPI.Task.Run(() => { try { e.Vehicle.Delete(); } catch { } });
                         Notify.Send(e, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы попытались покинуть территорию фермы.", 3000);

                     }
                 }
                 catch { }
             };*/
            #endregion Shapes

            #region markers
            NAPI.Marker.CreateMarker(27, FermaCheckpoints[0] + new Vector3(0, 0, 0.15), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220));
            NAPI.Marker.CreateMarker(1, FermaCheckpoints[1] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 3f, new Color(255, 215, 0));
            NAPI.Marker.CreateMarker(1, FermaCheckpoints[2] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 3f, new Color(255, 215, 0));
            NAPI.Marker.CreateMarker(1, FermaCheckpoints[3] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 215, 0));
            NAPI.Marker.CreateMarker(1, FermaCheckpoints[4] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 215, 0));

            NAPI.Marker.CreateMarker(1, CowCheckpoints[0], new Vector3(), new Vector3(), 1f, new Color(255, 215, 0));
            NAPI.Marker.CreateMarker(1, CowCheckpoints[1], new Vector3(), new Vector3(), 1f, new Color(255, 215, 0));
            NAPI.Marker.CreateMarker(1, CowCheckpoints[2], new Vector3(), new Vector3(), 1f, new Color(255, 215, 0));

            NAPI.Marker.CreateMarker(1, CowCheckpoints[3], new Vector3(), new Vector3(), 1f, new Color(255, 215, 0));
            NAPI.Marker.CreateMarker(1, CowCheckpoints[4], new Vector3(), new Vector3(), 1f, new Color(255, 215, 0));
            NAPI.Marker.CreateMarker(1, CowCheckpoints[5], new Vector3(), new Vector3(), 1f, new Color(255, 215, 0));

            NAPI.Marker.CreateMarker(1, CowCheckpoints[6], new Vector3(), new Vector3(), 1f, new Color(255, 215, 0));
            NAPI.Marker.CreateMarker(1, CowCheckpoints[7], new Vector3(), new Vector3(), 1f, new Color(255, 215, 0));
            NAPI.Marker.CreateMarker(1, CowCheckpoints[8], new Vector3(), new Vector3(), 1f, new Color(255, 215, 0));
            #endregion markers

        }
        public static void CheckProdCar()
        {
            NAPI.Task.Run(() =>
            {

            });
        } // Проверка машин, которые перевозят продукцию (респавн, заправа)

        #region Checkpoints
        public static List<Vector3> FermaCheckpoints = new List<Vector3>()
        {
             new Vector3(2016.344, 4987.024, 41.05), // 
             new Vector3(1906.2898, 4926.8765, 47.793625), // 
              new Vector3(-2221.1367, 3484.8938, 29.049551), // 
               new Vector3(-2354.8643, 3258.3398, 91.78368), // 
                new Vector3(1539.9316, 6336.0063, 22.9541), // 
        }; // Описание для взаимодействия с фермерскими чекпоинтами

        public static List<Vector3> CowCheckpoints = new List<Vector3>()
        {
             new Vector3(2251.348, 4875.394, 39.77567), // Коровник #1
             new Vector3(2250.1626, 4876.494, 39.754757), // Коровник #2
              new Vector3(2248.754, 4877.7817, 39.76184), // Коровник #3
               new Vector3(2247.4863, 4903.017, 39.590847), // Коровник #4
                new Vector3(2246.2622, 4904.0186, 39.579166), // Коровник #5
                 new Vector3(2245.0728, 4905.466, 39.57094), // Коровник #6
                 new Vector3(2223.3584, 4903.6455, 39.557095), // Коровник #7
                 new Vector3(2222.2715, 4904.371, 39.596916), // Коровник #8
                 new Vector3(2221.0547, 4905.4, 39.60111), //Коровник #9
        }; // Взаимодействие с коровником
        #endregion Checkpoints

        #region ServerEvents
        [ServerEvent(Event.PlayerDisconnected)]
        public void Event_OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (Main.Players[player].product == 10)
                {
                    var data = Fractions.Stocks.fracStocks[id];

                    Main.Players[player].product = 0;
                    data.Product += 10;
                    data.UpdateLabel();
                }
            }
        }  // Человек откобчается от сервера и...

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            if (player.Vehicle.HasData("CANPRODUCT") && !player.Vehicle.HasData("WORKINGNOW"))
            {
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Загрузите машину продукцией", 3000);
                Trigger.PlayerEvent(player, "createWaypoint", 1906.2898, 4926.8765);

            }
            if (player.Vehicle.HasData("CANPRODUCT") && player.Vehicle.HasData("WORKINGNOW"))
            {
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Отвезите продукцию на склад армии", 3000);
                Trigger.PlayerEvent(player, "createWaypoint", -2220.969, 3484.6597);
            }
        } // Если человек садиться в Vetir и....

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeathHandler(Player player, Player killer, uint reason)
        {
            try
            {
                if (Main.Players.ContainsKey(player))
                {
                    if (Main.Players[player].product == 10)
                    {
                        var data = Fractions.Stocks.fracStocks[id];

                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы умерли и потеряли 10 продуктов", 3000);
                        Main.Players[player].product = 0;
                        if (player == killer || killer == null)
                        {

                            data.Product += 10;
                            data.UpdateLabel();
                            Main.Players[player].product = 0;
                            return;
                        }
                        killer.SetData("KILLER", true);
                        Main.Players[killer].product = 10;
                        Notify.Send(killer, NotifyType.Success, NotifyPosition.BottomCenter, "Вы перехватили машину с продуктами, продайте их на нелегальной точке сбыта продукции", 3000);

                    }
                }

            }
            catch { }



        } // Человек умирает и....

        public static void interactPressed(Player player, int interact)
        {

            var data = Fractions.Stocks.fracStocks[id];
            switch (interact)
            {


                case 7825:

                    var mItemFerma = nInventory.Find(Main.Players[player].UUID, ItemType.Potata); // Картофель
                    var countFerma = (mItemFerma == null) ? 0 : mItemFerma.Count;

                    var mItemFerma2 = nInventory.Find(Main.Players[player].UUID, ItemType.Pshenica); // Пшеница
                    var countFerma2 = (mItemFerma2 == null) ? 0 : mItemFerma2.Count;

                    var mItemFerma3 = nInventory.Find(Main.Players[player].UUID, ItemType.Morkov); // Морковь
                    var countFerma3 = (mItemFerma3 == null) ? 0 : mItemFerma3.Count;

                    var mItemFerma4 = nInventory.Find(Main.Players[player].UUID, ItemType.Milk); // Клевер
                    var countFerma4 = (mItemFerma4 == null) ? 0 : mItemFerma4.Count;

                    var amountFerma = (countFerma * Commands.Kartofel) + (countFerma2 * Commands.Psenica) + (countFerma3 * Commands.Morkov) + (countFerma4 * Commands.Milk);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали продукты и получили {amountFerma}$", 3000);

                    data.Product += (countFerma / 5);
                    data.UpdateLabel();
                    nInventory.Remove(player, new nItem(ItemType.Potata, countFerma));
                    nInventory.Remove(player, new nItem(ItemType.Pshenica, countFerma2));
                    nInventory.Remove(player, new nItem(ItemType.Morkov, countFerma3));
                    nInventory.Remove(player, new nItem(ItemType.Milk, countFerma4));
                    MoneySystem.Wallet.Change(player, amountFerma);
                    return;
                case 7826:
                    var vehicle = player.Vehicle;
                    if (vehicle.HasData("CANPRODUCT"))
                    {


                        if (Main.Players[player].product == 10)
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Машина уже загружена", 3000);
                            return;
                        }

                        if (data.Product < 10)
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "На складе нет продуктов", 3000);
                            return;
                        }

                        Trigger.PlayerEvent(player, "createWaypoint", -2220.969, 3484.6597);

                        NAPI.Data.SetEntityData(player, "PRODTIMER_COUNT", 0);
                        NAPI.Data.SetEntityData(player, "PRODTIMER", Timers.Start(1000, () => timer_prod(player)));
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы загрузили 10 продукции, отвезите продовольствие на склад армии, он отмечен на карте", 9000);
                        Main.Players[player].product = 10;
                        data.Product -= 10;
                        data.UpdateLabel();
                    }

                    return;
                case 7827:
                    var vehicless = player.Vehicle;
                    if (!player.HasData("ARMYPROD"))
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Пожалуйста, подождите. (Осталось: {ProdTimer - NAPI.Data.GetEntityData(player, "PRODTIMER_COUNT")})", 3000);
                        return;
                    }
                    if (vehicless.HasData("CANPRODUCT") && player.HasData("ARMYPROD"))
                    {
                        data = Fractions.Stocks.fracStocks[14];
                        if (Main.Players[player].product == 0)
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Машина пуста", 3000);
                            return;
                        }


                        vehicless.ResetData("ARMYPROD");
                        NAPI.Data.SetEntityData(player, "PRODTIMER_COUNT", 0);
                        MoneySystem.Wallet.Change(player, 60000);
                        NAPI.Data.SetEntityData(player, "PRODTIMER", Timers.Start(1000, () => timer_prod(player)));
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы разгрузили 10 продукции", 3000);
                        Main.Players[player].product = 0;
                        data.Product += 10;
                        data.UpdateLabel();
                    }
                    return;
                case 7828:
                    data = Fractions.Stocks.fracStocks[14];
                    if (data.Product < 4)
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "На складе нет продуктов", 3000);
                        return;
                    }
                    data.Product -= 4;

                    data.UpdateLabel();
                    if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы съели комплексный обед", 3000);
                    EatManager.AddEat(player, 75);
                    EatManager.AddWater(player, 65);

                    return;
                case 7829:
                    var vehiclesss = player.Vehicle;
                    if (vehiclesss.HasData("CANPRODUCT") && Main.Players[player].product == 10)
                    {

                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы продали 10 продукции", 3000);
                        Main.Players[player].product = 0;
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Командный центр USNG заметил, что продукты не были доставлены, начато расследование", 3000);
                        var oldStars = (Main.Players[player].WantedLVL == null) ? 0 : Main.Players[player].WantedLVL.Level;
                        var wantedLevel = new WantedLevel(oldStars + 2, player.Name, DateTime.Now, "Кража продовольствия");
                        MoneySystem.Wallet.Change(player, 90000);
                        Fractions.Police.setPlayerWantedLevel(player, wantedLevel);
                        //  Fractions.IAA.setPlayerWantedLevel(player, wantedLevel);
                        // Fractions.Fbi.setPlayerWantedLevel(player, wantedLevel);
                        Fractions.Manager.sendFractionMessage(7, "Командный центр USNG заметил, что продукты не были доставлены, просим посетить вас точку нелегального сбыта продуктов");
                        Fractions.Manager.sendFractionMessage(9, "Командный центр USNG заметил, что продукты не были доставлены, просим посетить вас точку нелегального сбыта продуктов");
                        Fractions.Manager.sendFractionMessage(18, "Командный центр USNG заметил, что продукты не были доставлены, просим посетить вас точку нелегального сбыта продуктов");
                        Fractions.Manager.sendFractionMessage(17, "Командный центр USNG заметил, что продукты не были доставлены, просим посетить вас точку нелегального сбыта продуктов");
                    }

                    return;

                case 70001:
                    if (player.HasData("COW_9") || player.HasData("COW_2") || player.HasData("COW_3") || player.HasData("COW_4") || player.HasData("COW_5") || player.HasData("COW_6") || player.HasData("COW_7") || player.HasData("COW_8")) return;
                    var Clever = nInventory.Find(Main.Players[player].UUID, ItemType.Clever);
                    var Ccount = (Clever == null) ? 0 : Clever.Count;
                    if (player.HasData("COW_1_DONE"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы успешно взяли молоко, продайте его скупщику", 3000);
                        nInventory.Add(player, new nItem(ItemType.Milk, 1));
                        player.ResetData("COW_1_DONE");
                        return;
                    }
                    if (Ccount >= 3 && !player.HasData("COW_1"))
                    {
                        player.SendChatMessage($"~y~Забрать молоко вы сможете в Коровнике #1");
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы накормили корову, ожидайте", 3000);
                        nInventory.Remove(player, new nItem(ItemType.Clever, 3));
                        player.SetData("COW_1", true);
                        NAPI.Data.SetEntityData(player, "COW_1_COUNT", 0);
                        NAPI.Data.SetEntityData(player, "COW_1_TIMER", Timers.StartTask("checkcow1", 1000, () => timer_COW1(player)));
                        return;
                    }
                    if (Ccount < 3 && !player.HasData("COW_1"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Для получения молока необходимо 3 клевера", 3000);
                        return;
                    }
                    if (player.HasData("COW_1"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко подготавливается, ожидайте", 3000);
                        return;
                    }
                    return;
                case 70002:
                    if (player.HasData("COW_1") || player.HasData("COW_9") || player.HasData("COW_3") || player.HasData("COW_4") || player.HasData("COW_5") || player.HasData("COW_6") || player.HasData("COW_7") || player.HasData("COW_8")) return;
                    Clever = nInventory.Find(Main.Players[player].UUID, ItemType.Clever);
                    Ccount = (Clever == null) ? 0 : Clever.Count;
                    if (player.HasData("COW_2_DONE"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы успешно взяли молоко, продайте его скупщику", 3000);
                        nInventory.Add(player, new nItem(ItemType.Milk, 1));
                        player.ResetData("COW_2_DONE");
                        return;
                    }
                    if (Ccount >= 3 && !player.HasData("COW_2"))
                    {
                        player.SendChatMessage($"~y~Забрать молоко вы сможете в Коровнике #2");
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы накормили корову, ожидайте", 3000);
                        nInventory.Remove(player, new nItem(ItemType.Clever, 3));
                        player.SetData("COW_2", true);
                        NAPI.Data.SetEntityData(player, "COW_2_COUNT", 0);
                        NAPI.Data.SetEntityData(player, "COW_2_TIMER", Timers.StartTask("checkcow2", 1000, () => timer_COW2(player)));
                        return;
                    }
                    if (Ccount < 3 && !player.HasData("COW_2"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Для получения молока необходимо 3 клевера", 3000);
                        return;
                    }
                    if (player.HasData("COW_2"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко подготавливается, ожидайте", 3000);
                        return;
                    }
                    return;
                case 70003:
                    if (player.HasData("COW_1") || player.HasData("COW_2") || player.HasData("COW_9") || player.HasData("COW_4") || player.HasData("COW_5") || player.HasData("COW_6") || player.HasData("COW_7") || player.HasData("COW_8")) return;
                    Clever = nInventory.Find(Main.Players[player].UUID, ItemType.Clever);
                    Ccount = (Clever == null) ? 0 : Clever.Count;
                    if (player.HasData("COW_3_DONE"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы успешно взяли молоко, продайте его скупщику", 3000);
                        nInventory.Add(player, new nItem(ItemType.Milk, 1));
                        player.ResetData("COW_3_DONE");
                        return;
                    }
                    if (Ccount >= 3 && !player.HasData("COW_3"))
                    {
                        player.SendChatMessage($"~y~Забрать молоко вы сможете в Коровнике #3");
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы накормили корову, ожидайте", 3000);
                        nInventory.Remove(player, new nItem(ItemType.Clever, 3));
                        player.SetData("COW_3", true);
                        NAPI.Data.SetEntityData(player, "COW_3_COUNT", 0);
                        NAPI.Data.SetEntityData(player, "COW_3_TIMER", Timers.StartTask("checkcow3", 1000, () => timer_COW3(player)));
                        return;

                    }
                    if (Ccount < 3 && !player.HasData("COW_3"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Для получения молока необходимо 3 клевера", 3000);
                        return;
                    }
                    if (player.HasData("COW_3"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко подготавливается, ожидайте", 3000);
                        return;
                    }

                    return;
                case 70004:
                    if (player.HasData("COW_1") || player.HasData("COW_2") || player.HasData("COW_3") || player.HasData("COW_9") || player.HasData("COW_5") || player.HasData("COW_6") || player.HasData("COW_7") || player.HasData("COW_8")) return;
                    Clever = nInventory.Find(Main.Players[player].UUID, ItemType.Clever);
                    Ccount = (Clever == null) ? 0 : Clever.Count;
                    if (player.HasData("COW_4_DONE"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы успешно взяли молоко, продайте его скупщику", 3000);
                        nInventory.Add(player, new nItem(ItemType.Milk, 1));
                        player.ResetData("COW_4_DONE");
                        return;
                    }
                    if (Ccount >= 3 && !player.HasData("COW_4"))
                    {
                        player.SendChatMessage($"~y~Забрать молоко вы сможете в Коровнике #4");
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы накормили корову, ожидайте", 3000);
                        nInventory.Remove(player, new nItem(ItemType.Clever, 3));
                        player.SetData("COW_4", true);
                        NAPI.Data.SetEntityData(player, "COW_4_COUNT", 0);
                        NAPI.Data.SetEntityData(player, "COW_4_TIMER", Timers.StartTask("checkcow4", 1000, () => timer_COW4(player)));
                        return;

                    }
                    if (Ccount < 3 && !player.HasData("COW_4"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Для получения молока необходимо 3 клевера", 3000);
                        return;
                    }
                    if (player.HasData("COW_4"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко подготавливается, ожидайте", 3000);
                        return;
                    }

                    return;
                case 70005:
                    if (player.HasData("COW_1") || player.HasData("COW_2") || player.HasData("COW_3") || player.HasData("COW_4") || player.HasData("COW_9") || player.HasData("COW_6") || player.HasData("COW_7") || player.HasData("COW_8")) return;
                    Clever = nInventory.Find(Main.Players[player].UUID, ItemType.Clever);
                    Ccount = (Clever == null) ? 0 : Clever.Count;
                    if (player.HasData("COW_5_DONE"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы успешно взяли молоко, продайте его скупщику", 3000);
                        nInventory.Add(player, new nItem(ItemType.Milk, 1));
                        player.ResetData("COW_5_DONE");
                        return;
                    }
                    if (Ccount >= 3 && !player.HasData("COW_5"))
                    {
                        player.SendChatMessage($"~y~Забрать молоко вы сможете в Коровнике #5");
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы накормили корову, ожидайте", 3000);
                        nInventory.Remove(player, new nItem(ItemType.Clever, 3));
                        player.SetData("COW_5", true);
                        NAPI.Data.SetEntityData(player, "COW_5_COUNT", 0);
                        NAPI.Data.SetEntityData(player, "COW_5_TIMER", Timers.StartTask("checkcow5", 1000, () => timer_COW5(player)));
                        return;

                    }
                    if (Ccount < 3 && !player.HasData("COW_5"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Для получения молока необходимо 3 клевера", 3000);
                        return;
                    }
                    if (player.HasData("COW_5"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко подготавливается, ожидайте", 3000);
                        return;
                    }

                    return;
                case 70006:
                    if (player.HasData("COW_1") || player.HasData("COW_2") || player.HasData("COW_3") || player.HasData("COW_4") || player.HasData("COW_5") || player.HasData("COW_9") || player.HasData("COW_7") || player.HasData("COW_8")) return;
                    Clever = nInventory.Find(Main.Players[player].UUID, ItemType.Clever);
                    Ccount = (Clever == null) ? 0 : Clever.Count;
                    if (player.HasData("COW_6_DONE"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы успешно взяли молоко, продайте его скупщику", 3000);
                        nInventory.Add(player, new nItem(ItemType.Milk, 1));
                        player.ResetData("COW_6_DONE");
                        return;
                    }
                    if (Ccount >= 3 && !player.HasData("COW_6"))
                    {
                        player.SendChatMessage($"~y~Забрать молоко вы сможете в Коровнике #6");
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы накормили корову, ожидайте", 3000);
                        nInventory.Remove(player, new nItem(ItemType.Clever, 3));
                        player.SetData("COW_6", true);
                        NAPI.Data.SetEntityData(player, "COW_6_COUNT", 0);
                        NAPI.Data.SetEntityData(player, "COW_6_TIMER", Timers.StartTask("checkcow6", 1000, () => timer_COW6(player)));
                        return;

                    }
                    if (Ccount < 3 && !player.HasData("COW_6"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Для получения молока необходимо 3 клевера", 3000);
                        return;
                    }
                    if (player.HasData("COW_6"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко подготавливается, ожидайте", 3000);
                        return;
                    }

                    return;
                case 70007:
                    if (player.HasData("COW_1") || player.HasData("COW_2") || player.HasData("COW_3") || player.HasData("COW_4") || player.HasData("COW_5") || player.HasData("COW_6") || player.HasData("COW_9") || player.HasData("COW_8")) return;
                    Clever = nInventory.Find(Main.Players[player].UUID, ItemType.Clever);
                    Ccount = (Clever == null) ? 0 : Clever.Count;
                    if (player.HasData("COW_7_DONE"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы успешно взяли молоко, продайте его скупщику", 3000);
                        nInventory.Add(player, new nItem(ItemType.Milk, 1));
                        player.ResetData("COW_7_DONE");
                        return;
                    }
                    if (Ccount >= 3 && !player.HasData("COW_7"))
                    {
                        player.SendChatMessage($"~y~Забрать молоко вы сможете в Коровнике #7");
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы накормили корову, ожидайте", 3000);
                        nInventory.Remove(player, new nItem(ItemType.Clever, 3));
                        player.SetData("COW_7", true);
                        NAPI.Data.SetEntityData(player, "COW_7_COUNT", 0);
                        NAPI.Data.SetEntityData(player, "COW_7_TIMER", Timers.StartTask("checkcow7", 1000, () => timer_COW7(player)));
                        return;

                    }
                    if (Ccount < 3 && !player.HasData("COW_7"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Для получения молока необходимо 3 клевера", 3000);
                        return;
                    }
                    if (player.HasData("COW_7"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко подготавливается, ожидайте", 3000);
                        return;
                    }

                    return;

                case 70008:
                    if (player.HasData("COW_1") || player.HasData("COW_2") || player.HasData("COW_3") || player.HasData("COW_4") || player.HasData("COW_5") || player.HasData("COW_6") || player.HasData("COW_7") || player.HasData("COW_9")) return;
                    Clever = nInventory.Find(Main.Players[player].UUID, ItemType.Clever);
                    Ccount = (Clever == null) ? 0 : Clever.Count;
                    if (player.HasData("COW_8_DONE"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы успешно взяли молоко, продайте его скупщику", 3000);
                        nInventory.Add(player, new nItem(ItemType.Milk, 1));
                        player.ResetData("COW_8_DONE");
                        return;
                    }
                    if (Ccount >= 3 && !player.HasData("COW_8"))
                    {
                        player.SendChatMessage($"~y~Забрать молоко вы сможете в Коровнике #8");
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы накормили корову, ожидайте", 3000);
                        nInventory.Remove(player, new nItem(ItemType.Clever, 3));
                        player.SetData("COW_8", true);
                        NAPI.Data.SetEntityData(player, "COW_8_COUNT", 0);
                        NAPI.Data.SetEntityData(player, "COW_8_TIMER", Timers.StartTask("checkcow8", 1000, () => timer_COW8(player)));
                        return;

                    }
                    if (Ccount < 3 && !player.HasData("COW_8"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Для получения молока необходимо 3 клевера", 3000);
                        return;
                    }
                    if (player.HasData("COW_8"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко подготавливается, ожидайте", 3000);
                        return;
                    }

                    return;

                case 70009:
                    if (player.HasData("COW_1") || player.HasData("COW_2") || player.HasData("COW_3") || player.HasData("COW_4") || player.HasData("COW_5") || player.HasData("COW_6") || player.HasData("COW_7") || player.HasData("COW_8")) return;
                    Clever = nInventory.Find(Main.Players[player].UUID, ItemType.Clever);
                    Ccount = (Clever == null) ? 0 : Clever.Count;
                    if (player.HasData("COW_9_DONE"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы успешно взяли молоко, продайте его скупщику", 3000);
                        nInventory.Add(player, new nItem(ItemType.Milk, 1));
                        player.ResetData("COW_9_DONE");
                        return;
                    }

                    if (Ccount >= 10 && !player.HasData("COW_9"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы накормили корову, ожидайте", 3000);
                        player.SendChatMessage($"~y~Забрать молоко вы сможете в Коровнике #9");
                        nInventory.Remove(player, new nItem(ItemType.Clever, 3));
                        player.SetData("COW_9", true);
                        NAPI.Data.SetEntityData(player, "COW_9_COUNT", 0);
                        NAPI.Data.SetEntityData(player, "COW_9_TIMER", Timers.StartTask("checkcow9", 1000, () => timer_COW9(player)));
                        return;

                    }
                    if (Ccount < 3 && !player.HasData("COW_9"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Для получения молока необходимо 3 клевера", 3000);
                        return;
                    }
                    if (player.HasData("COW_9"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко подготавливается, ожидайте", 3000);
                        return;
                    }

                    return;

            }

        }

        // Человек взаимодействует с колшейпом и...
        #endregion ServerEvents

        #region timer
        public static void timer_prod(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("PRODTIMER")) return;

                    if (NAPI.Data.GetEntityData(player, "PRODTIMER_COUNT") > ProdTimer)
                    {
                        Timers.Stop(NAPI.Data.GetEntityData(player, "PRODTIMER"));
                        player.SetData("ARMYPROD", true);
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "PRODTIMER_COUNT", NAPI.Data.GetEntityData(player, "PRODTIMER_COUNT") + 1);

                }
                catch (Exception e)
                {
                    Log.Write("Timer_Ferma: \n" + e.ToString(), nLog.Type.Error);
                }
            });
        } // Таймер-задержка
        public static void timer_COW1(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("COW_1")) return;
                    if (player.HasData("COW_9") || player.HasData("COW_2") || player.HasData("COW_3") || player.HasData("COW_4") || player.HasData("COW_5") || player.HasData("COW_6") || player.HasData("COW_7") || player.HasData("COW_8")) return;

                    if (NAPI.Data.GetEntityData(player, "COW_1_COUNT") > CowTimer)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко готово, можете забирать", 3000);


                        player.ResetData("COW_1");
                        player.SetData("COW_1_DONE", true);

                        Timers.Stop(NAPI.Data.GetEntityData(player, "COW_1_TIMER"));
                        player.ResetData("COW_1_TIMER");
                        return;
                    }

                    NAPI.Data.SetEntityData(player, "COW_1_COUNT", NAPI.Data.GetEntityData(player, "COW_1_COUNT") + 1);
                    var lefttime = (NAPI.Data.GetEntityData(player, "COW_1_COUNT"));
                    if (lefttime % 10 == 0)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко будет готово через: {CowTimer - lefttime} секунд", 3000);
                    }
                }
                catch (Exception e) { Log.Write("timerExitRentVehicle: " + e.ToString(), nLog.Type.Error); }
            });
        } // Таймер на коровник #1
        public static void timer_COW2(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("COW_2")) return;
                    if (player.HasData("COW_1") || player.HasData("COW_9") || player.HasData("COW_3") || player.HasData("COW_4") || player.HasData("COW_5") || player.HasData("COW_6") || player.HasData("COW_7") || player.HasData("COW_8")) return;

                    if (NAPI.Data.GetEntityData(player, "COW_2_COUNT") > CowTimer)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко готово, можете забирать", 3000);

                        player.ResetData("COW_2");
                        player.SetData("COW_2_DONE", true);

                        Timers.Stop(NAPI.Data.GetEntityData(player, "COW_2_TIMER"));
                        player.ResetData("COW_2_TIMER");
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "COW_2_COUNT", NAPI.Data.GetEntityData(player, "COW_2_COUNT") + 1);
                    var lefttime = (NAPI.Data.GetEntityData(player, "COW_2_COUNT"));
                    if (lefttime % 10 == 0)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко будет готово через: {CowTimer - lefttime} секунд", 3000);
                    }
                }
                catch (Exception e) { Log.Write("timerExitRentVehicle: " + e.ToString(), nLog.Type.Error); }
            });
        } //Таймер на коровник #2
        public static void timer_COW3(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("COW_3")) return;
                    if (player.HasData("COW_1") || player.HasData("COW_2") || player.HasData("COW_9") || player.HasData("COW_4") || player.HasData("COW_5") || player.HasData("COW_6") || player.HasData("COW_7") || player.HasData("COW_8")) return;

                    if (NAPI.Data.GetEntityData(player, "COW_3_COUNT") > CowTimer)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко готово, можете забирать", 3000);


                        player.ResetData("COW_3");
                        player.SetData("COW_3_DONE", true);

                        Timers.Stop(NAPI.Data.GetEntityData(player, "COW_3_TIMER"));
                        player.ResetData("COW_3_TIMER");
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "COW_3_COUNT", NAPI.Data.GetEntityData(player, "COW_3_COUNT") + 1);
                    var lefttime = (NAPI.Data.GetEntityData(player, "COW_3_COUNT"));
                    if (lefttime % 10 == 0)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко будет готово через: {CowTimer - lefttime} секунд", 3000);
                    }
                }
                catch (Exception e) { Log.Write("timerExitRentVehicle: " + e.ToString(), nLog.Type.Error); }
            });
        } //Таймер на коровник #3
        public static void timer_COW4(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("COW_4")) return;
                    if (player.HasData("COW_1") || player.HasData("COW_2") || player.HasData("COW_3") || player.HasData("COW_9") || player.HasData("COW_5") || player.HasData("COW_6") || player.HasData("COW_7") || player.HasData("COW_8")) return;

                    if (NAPI.Data.GetEntityData(player, "COW_4_COUNT") > CowTimer)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко готово, можете забирать", 3000);


                        player.ResetData("COW_4");
                        player.SetData("COW_4_DONE", true);

                        Timers.Stop(NAPI.Data.GetEntityData(player, "COW_4_TIMER"));
                        player.ResetData("COW_4_TIMER");
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "COW_4_COUNT", NAPI.Data.GetEntityData(player, "COW_4_COUNT") + 1);
                    var lefttime = (NAPI.Data.GetEntityData(player, "COW_4_COUNT"));
                    if (lefttime % 10 == 0)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко будет готово через: {CowTimer - lefttime} секунд", 3000);
                    }
                }
                catch (Exception e) { Log.Write("timerExitRentVehicle: " + e.ToString(), nLog.Type.Error); }
            });
        } //Таймер на коровник #4
        public static void timer_COW5(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("COW_5")) return;
                    if (player.HasData("COW_1") || player.HasData("COW_2") || player.HasData("COW_3") || player.HasData("COW_4") || player.HasData("COW_9") || player.HasData("COW_6") || player.HasData("COW_7") || player.HasData("COW_8")) return;

                    if (NAPI.Data.GetEntityData(player, "COW_5_COUNT") > CowTimer)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко готово, можете забирать", 3000);


                        player.ResetData("COW_5");
                        player.SetData("COW_5_DONE", true);

                        Timers.Stop(NAPI.Data.GetEntityData(player, "COW_5_TIMER"));
                        player.ResetData("COW_5_TIMER");
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "COW_5_COUNT", NAPI.Data.GetEntityData(player, "COW_5_COUNT") + 1);
                    var lefttime = (NAPI.Data.GetEntityData(player, "COW_5_COUNT"));
                    if (lefttime % 10 == 0)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко будет готово через: {CowTimer - lefttime} секунд", 3000);
                    }
                }
                catch (Exception e) { Log.Write("timerExitRentVehicle: " + e.ToString(), nLog.Type.Error); }
            });
        } //Таймер на коровник #5
        public static void timer_COW6(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("COW_6")) return;
                    if (player.HasData("COW_1") || player.HasData("COW_2") || player.HasData("COW_3") || player.HasData("COW_4") || player.HasData("COW_5") || player.HasData("COW_9") || player.HasData("COW_7") || player.HasData("COW_8")) return;

                    if (NAPI.Data.GetEntityData(player, "COW_6_COUNT") > CowTimer)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко готово, можете забирать", 3000);


                        player.ResetData("COW_6");
                        player.SetData("COW_6_DONE", true);

                        Timers.Stop(NAPI.Data.GetEntityData(player, "COW_6_TIMER"));
                        player.ResetData("COW_6_TIMER");
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "COW_6_COUNT", NAPI.Data.GetEntityData(player, "COW_6_COUNT") + 1);
                    var lefttime = (NAPI.Data.GetEntityData(player, "COW_6_COUNT"));
                    if (lefttime % 10 == 0)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко будет готово через: {CowTimer - lefttime} секунд", 3000);
                    }
                }
                catch (Exception e) { Log.Write("timerExitRentVehicle: " + e.ToString(), nLog.Type.Error); }
            });
        } //Таймер на коровник #6

        public static void timer_COW7(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("COW_7")) return;
                    if (player.HasData("COW_1") || player.HasData("COW_2") || player.HasData("COW_3") || player.HasData("COW_4") || player.HasData("COW_5") || player.HasData("COW_6") || player.HasData("COW_9") || player.HasData("COW_8")) return;

                    if (NAPI.Data.GetEntityData(player, "COW_7_COUNT") > CowTimer)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко готово, можете забирать", 3000);


                        player.ResetData("COW_7");
                        player.SetData("COW_7_DONE", true);

                        Timers.Stop(NAPI.Data.GetEntityData(player, "COW_7_TIMER"));
                        player.ResetData("COW_7_TIMER");
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "COW_7_COUNT", NAPI.Data.GetEntityData(player, "COW_7_COUNT") + 1);
                    var lefttime = (NAPI.Data.GetEntityData(player, "COW_7_COUNT"));
                    if (lefttime % 10 == 0)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко будет готово через: {CowTimer - lefttime} секунд", 3000);
                    }
                }
                catch (Exception e) { Log.Write("timerExitRentVehicle: " + e.ToString(), nLog.Type.Error); }
            });
        } //Таймер на коровник #7
        public static void timer_COW8(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("COW_8")) return;
                    if (player.HasData("COW_1") || player.HasData("COW_2") || player.HasData("COW_3") || player.HasData("COW_4") || player.HasData("COW_5") || player.HasData("COW_6") || player.HasData("COW_7") || player.HasData("COW_9")) return;

                    if (NAPI.Data.GetEntityData(player, "COW_8_COUNT") > CowTimer)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко готово, можете забирать", 3000);

                        player.ResetData("COW_8");
                        player.SetData("COW_8_DONE", true);

                        Timers.Stop(NAPI.Data.GetEntityData(player, "COW_8_TIMER"));
                        player.ResetData("COW_8_TIMER");

                        return;
                    }
                    NAPI.Data.SetEntityData(player, "COW_8_COUNT", NAPI.Data.GetEntityData(player, "COW_8_COUNT") + 1);
                    var lefttime = (NAPI.Data.GetEntityData(player, "COW_8_COUNT"));
                    if (lefttime % 10 == 0)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко будет готово через: {CowTimer - lefttime} секунд", 3000);
                    }
                }
                catch (Exception e) { Log.Write("timerExitRentVehicle: " + e.ToString(), nLog.Type.Error); }
            });
        } //Таймер на коровник #8
        public static void timer_COW9(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("COW_9")) return;
                    if (player.HasData("COW_1") || player.HasData("COW_2") || player.HasData("COW_3") || player.HasData("COW_4") || player.HasData("COW_5") || player.HasData("COW_6") || player.HasData("COW_7") || player.HasData("COW_8")) return;

                    if (NAPI.Data.GetEntityData(player, "COW_9_COUNT") > CowTimer)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко готово, можете забирать", 3000);

                        player.ResetData("COW_9");
                        player.SetData("COW_9_DONE", true);

                        Timers.Stop(NAPI.Data.GetEntityData(player, "COW_9_TIMER"));
                        player.ResetData("COW_9_TIMER");
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "COW_9_COUNT", NAPI.Data.GetEntityData(player, "COW_9_COUNT") + 1);
                    var lefttime = (NAPI.Data.GetEntityData(player, "COW_9_COUNT"));
                    if (lefttime % 10 == 0)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Молоко будет готово через: {CowTimer - lefttime} секунд", 3000);
                    }
                }
                catch (Exception e) { Log.Write("timerExitRentVehicle: " + e.ToString(), nLog.Type.Error); }
            });
        } //Таймер на коровник #9
        #endregion timer

        #region planting
        private static void CreateTree(int UUID, Vector3 pos, int amount, int type, int id, bool save = true)
        {
            try
            {
                if (type == 0)
                {
                    var obj = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_p_spider_01c"), pos + new Vector3(0, 0, -0.1), new Vector3(0, 0, 0), 255, 0);

                    if (amount <= 0) amount = Main.rnd.Next(2, 3);

                    //  var Checkpoint = NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 0.4), new Vector3(), new Vector3(), 0.7f, new Color(204, 204, 33));
                    var Msg = API.Shared.CreateTextLabel("~h~~g~-~y~Картофель~g~-~w~~n~~n~Нажмите ~y~Е~w~, чтобы собрать", pos + new Vector3(0, 0, 0.9), 11.0f, 0.3f, 4, new Color(255, 255, 255, 255), false, 0);
                    //var Msg = NAPI.TextLabel.CreateTextLabel("/take", pos + new Vector3(0, 0, 1.7), 5f, 0.3f, 0, new Color(0, 255, 0), true, 0);

                    ColsCount++;
                    Ferma.Add(ColsCount, NAPI.ColShape.CreateCylinderColShape(pos, 0.7f, 2, 0));
                    FermaObj.Add(obj);
                    Ferma[ColsCount].Position = pos;
                    NAPI.Entity.SetEntityPosition(Ferma[ColsCount], pos);
                    Ferma[ColsCount].SetData("INTERACT", 10567890);
                    Ferma[ColsCount].SetData("ColsCount", ColsCount);
                    Ferma[ColsCount].SetData("ColsCount", ColsCount);
                    Ferma[ColsCount].SetData("FERMA_ID", obj);
                    Ferma[ColsCount].OnEntityEnterColShape += Shape_onEntityEnterColShape;
                    Ferma[ColsCount].OnEntityExitColShape += Shape_onEntityExitColShape;


                    obj.SetData("FERMA_OWNER", UUID);
                    obj.SetData("FERMA_AMOUNT", amount);
                    obj.SetData("FERMA_TYPE", type);
                    obj.SetData("FERMA_PROCESSRISE", DateTime.Now.AddMinutes(0.1));
                    //  obj.SetData("FERMA_MARKER", Checkpoint);
                    obj.SetData("FERMA_MSG", Msg);
                    obj.SetSharedData("FERMA_ACTIVE", true);

                    if (id <= 0)
                    {
                        int rnd;
                        do
                        {
                            rnd = Main.rnd.Next(1111, 9999999);
                        }
                        while (dbIDs.Contains(rnd));
                        dbIDs.Add(rnd);
                        obj.SetData("FERMA_DB", rnd);
                    }
                    else obj.SetData("FERMA_DB", id);

                    if (save)
                    {
                        if (SaveTree(obj))
                        {

                        }
                        else
                        {
                        }
                    }
                }
                if (type == 1)
                {
                    var obj = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_plant_paradise"), pos + new Vector3(0, 0, -0.1), new Vector3(0, 0, 0), 255, 0);

                    if (amount <= 0) amount = Main.rnd.Next(1, 5);

                    //  var Checkpoint = NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 0.4), new Vector3(), new Vector3(), 0.7f, new Color(204, 204, 33));
                    var Msg = API.Shared.CreateTextLabel("~h~~g~-~y~Пшеница~g~-~w~~n~~n~Нажмите ~y~Е~w~, чтобы собрать", pos + new Vector3(0, 0, 0.9), 11.0f, 0.3f, 4, new Color(255, 255, 255, 255), false, 0);
                    //var Msg = NAPI.TextLabel.CreateTextLabel("/take", pos + new Vector3(0, 0, 1.7), 5f, 0.3f, 0, new Color(0, 255, 0), true, 0);

                    ColsCount++;
                    Ferma.Add(ColsCount, NAPI.ColShape.CreateCylinderColShape(pos, 1.2f, 2, 0));
                    FermaObj.Add(obj);
                    Ferma[ColsCount].Position = pos;
                    NAPI.Entity.SetEntityPosition(Ferma[ColsCount], pos);
                    Ferma[ColsCount].SetData("INTERACT", 10567890);
                    Ferma[ColsCount].SetData("ColsCount", ColsCount);
                    Ferma[ColsCount].SetData("FERMA_ID", obj);
                    Ferma[ColsCount].OnEntityEnterColShape += Shape_onEntityEnterColShape;
                    Ferma[ColsCount].OnEntityExitColShape += Shape_onEntityExitColShape;


                    obj.SetData("FERMA_OWNER", UUID);
                    obj.SetData("FERMA_AMOUNT", amount);
                    obj.SetData("FERMA_TYPE", type);
                    obj.SetData("FERMA_PROCESSRISE", DateTime.Now.AddMinutes(5));
                    //  obj.SetData("FERMA_MARKER", Checkpoint);
                    obj.SetData("FERMA_MSG", Msg);
                    obj.SetSharedData("FERMA_ACTIVE", true);

                    if (id <= 0)
                    {
                        int rnd;
                        do
                        {
                            rnd = Main.rnd.Next(1111, 9999999);
                        }
                        while (dbIDs.Contains(rnd));
                        dbIDs.Add(rnd);
                        obj.SetData("FERMA_DB", rnd);
                    }
                    else obj.SetData("FERMA_DB", id);

                    if (save)
                    {
                        if (SaveTree(obj))
                        {

                        }
                        else
                        {

                        }
                    }
                }
                if (type == 2)
                {
                    var obj = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_grass_dry_02"), pos + new Vector3(0, 0, -0.1), new Vector3(0, 0, 0), 255, 0);

                    if (amount <= 0) amount = Main.rnd.Next(2, 6);

                    //  var Checkpoint = NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 0.4), new Vector3(), new Vector3(), 0.7f, new Color(204, 204, 33));
                    var Msg = API.Shared.CreateTextLabel("~h~~g~-~y~Морковь~g~-~w~~n~~n~Нажмите ~y~Е~w~, чтобы собрать", pos + new Vector3(0, 0, 0.9), 11.0f, 0.3f, 4, new Color(255, 255, 255, 255), false, 0);
                    //var Msg = NAPI.TextLabel.CreateTextLabel("/take", pos + new Vector3(0, 0, 1.7), 5f, 0.3f, 0, new Color(0, 255, 0), true, 0);

                    ColsCount++;
                    Ferma.Add(ColsCount, NAPI.ColShape.CreateCylinderColShape(pos, 1.2f, 2, 0));
                    FermaObj.Add(obj);
                    Ferma[ColsCount].Position = pos;
                    NAPI.Entity.SetEntityPosition(Ferma[ColsCount], pos);
                    Ferma[ColsCount].SetData("INTERACT", 10567890);
                    Ferma[ColsCount].SetData("ColsCount", ColsCount);
                    Ferma[ColsCount].SetData("FERMA_ID", obj);
                    Ferma[ColsCount].OnEntityEnterColShape += Shape_onEntityEnterColShape;
                    Ferma[ColsCount].OnEntityExitColShape += Shape_onEntityExitColShape;


                    obj.SetData("FERMA_OWNER", UUID);
                    obj.SetData("FERMA_AMOUNT", amount);
                    obj.SetData("FERMA_TYPE", type);
                    obj.SetData("FERMA_PROCESSRISE", DateTime.Now.AddMinutes(6));

                    obj.SetData("FERMA_MSG", Msg);
                    obj.SetSharedData("FERMA_ACTIVE", true);

                    if (id <= 0)
                    {
                        int rnd;
                        do
                        {
                            rnd = Main.rnd.Next(1111, 9999999);
                        }
                        while (dbIDs.Contains(rnd));
                        dbIDs.Add(rnd);
                        obj.SetData("FERMA_DB", rnd);
                    }
                    else obj.SetData("FERMA_DB", id);

                    if (save)
                    {
                        if (SaveTree(obj))
                        {

                        }
                        else
                        {

                        }
                    }
                }
                if (type == 3)
                {
                    var obj = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_grass_dry_02"), pos + new Vector3(0, 0, -0.1), new Vector3(0, 0, 0), 255, 0);

                    if (amount <= 0) amount = Main.rnd.Next(1, 3);

                    //  var Checkpoint = NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 0.4), new Vector3(), new Vector3(), 0.7f, new Color(204, 204, 33));
                    var Msg = API.Shared.CreateTextLabel("~h~~g~-~y~Клевер~g~-~w~~n~~n~Нажмите ~y~Е~w~, чтобы собрать", pos + new Vector3(0, 0, 0.9), 11.0f, 0.3f, 4, new Color(255, 255, 255, 255), false, 0);
                    //var Msg = NAPI.TextLabel.CreateTextLabel("/take", pos + new Vector3(0, 0, 1.7), 5f, 0.3f, 0, new Color(0, 255, 0), true, 0);

                    ColsCount++;
                    Ferma.Add(ColsCount, NAPI.ColShape.CreateCylinderColShape(pos, 1.2f, 2, 0));
                    FermaObj.Add(obj);
                    Ferma[ColsCount].Position = pos;
                    NAPI.Entity.SetEntityPosition(Ferma[ColsCount], pos);
                    Ferma[ColsCount].SetData("INTERACT", 10567890);
                    Ferma[ColsCount].SetData("ColsCount", ColsCount);
                    Ferma[ColsCount].SetData("FERMA_ID", obj);
                    Ferma[ColsCount].OnEntityEnterColShape += Shape_onEntityEnterColShape;
                    Ferma[ColsCount].OnEntityExitColShape += Shape_onEntityExitColShape;


                    obj.SetData("FERMA_OWNER", UUID);
                    obj.SetData("FERMA_AMOUNT", amount);
                    obj.SetData("FERMA_TYPE", type);
                    obj.SetData("FERMA_PROCESSRISE", DateTime.Now.AddMinutes(12));

                    obj.SetData("FERMA_MSG", Msg);
                    obj.SetSharedData("FERMA_ACTIVE", true);

                    if (id <= 0)
                    {
                        int rnd;
                        do
                        {
                            rnd = Main.rnd.Next(1111, 9999999);
                        }
                        while (dbIDs.Contains(rnd));
                        dbIDs.Add(rnd);
                        obj.SetData("FERMA_DB", rnd);
                    }
                    else obj.SetData("FERMA_DB", id);

                    if (save)
                    {
                        if (SaveTree(obj))
                        {

                        }
                        else
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write($"Exception at CreateTree with {e.Message}");
            }
        }  // Создание растений

        public static void DestroyTree(GTANetworkAPI.Object obj)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (obj == null) return;

                    if (obj.HasData("FERMA_MARKER"))
                    {
                        Marker mark = obj.GetData<Marker>("FERMA_MARKER");
                        if (mark != null)
                        {
                            mark.Delete();
                        }
                    }

                    if (obj.HasData("FERMA_SHAPEID"))
                    {
                        ColShape col = obj.GetData<ColShape>("FERMA_SHAPEID");
                        if (col != null)
                        {
                            col.Delete();
                        }
                    }

                    if (obj.HasData("FERMA_MSG"))
                    {
                        TextLabel Msg = obj.GetData<TextLabel>("FERMA_MSG");
                        if (Msg != null)
                        {
                            Msg.Delete();
                        }
                    }
                    Ferma.Remove(obj.GetData<int>("ColsCount"));
                    FermaObj.Remove(obj);
                    DeleteTreeFromDB(obj);

                    obj.ResetData("FERMA_OWNER");
                    obj.ResetData("FERMA_TYPE");
                    obj.ResetData("FERMA_AMOUNT");
                    obj.ResetData("FERMA_ID");
                    obj.ResetData("FERMA_SHAPEID");
                    obj.ResetData("FERMA_PROCESSRISE");
                    obj.ResetData("FERMA_MARKER");
                    obj.ResetData("FERMA_DB");
                    obj.ResetData("FERMA_MSG");

                    obj.Delete();
                }
                catch (Exception e)
                {
                    Log.Write($"Exception at DestroyTree with {e.Message}");
                }
            }, 0);
        } // Удаление растения
       /* public static void DestroyAllTree_pred()
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    NAPI.Chat.SendChatMessageToAll("!{#DF5353}[ФЕРМА] Дорогие игроки, через 30 минут ферма будет очищена, просим Вас собрать свой урожай!.");
                }
                catch (Exception e)
                {
                    Log.Write($"Exception at DestroyTree with {e.Message}");
                }
            });
        } */
        public static void DestroyAllTree()
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (DateTime.Now.Minute == 30 && DateTime.Now.Hour % 2 != 0)
                    {
                        NAPI.Chat.SendChatMessageToAll("!{#DF5353}[ФЕРМА] Дорогие игроки, через 30 минут ферма будет очищена, просим Вас собрать свой урожай!.");
                    }

                        if (DateTime.Now.Minute == 0 && DateTime.Now.Hour % 2 == 0)
                      {
                        foreach (GTANetworkAPI.Object obj in FermaObj.ToList())
                        {
                            if (obj == null) return;

                            if (obj.HasData("FERMA_MARKER"))
                            {
                                Marker mark = obj.GetData<Marker>("FERMA_MARKER");
                                if (mark != null)
                                {
                                    mark.Delete();
                                }
                            }

                            if (obj.HasData("FERMA_SHAPEID"))
                            {
                                ColShape col = obj.GetData<ColShape>("FERMA_SHAPEID");
                                if (col != null)
                                {
                                    col.Delete();
                                }
                            }

                            if (obj.HasData("FERMA_MSG"))
                            {
                                TextLabel Msg = obj.GetData<TextLabel>("FERMA_MSG");
                                if (Msg != null)
                                {
                                    Msg.Delete();
                                }
                            }
                            obj.ResetData("FERMA_OWNER");
                            obj.ResetData("FERMA_TYPE");
                            obj.ResetData("FERMA_AMOUNT");
                            obj.ResetData("FERMA_ID");
                            obj.ResetData("FERMA_SHAPEID");
                            obj.ResetData("FERMA_PROCESSRISE");
                            obj.ResetData("FERMA_MARKER");
                            obj.ResetData("FERMA_DB");
                            obj.ResetData("FERMA_MSG");
                            obj.Delete();
                            Ferma.Remove(obj.GetData<int>("ColsCount"));
                            FermaObj.Remove(obj);
                            DeleteAllTreeFromDB(obj);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Write($"Exception at DestroyTree with {e.Message}");
                }
            }, 0);
        } // Удаление растения

        public static bool PlantTree(Player player, int type)
        {
            if (player == null) return false;
            if (player.Dimension != 0)
            {
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Сейчас вы не можете делать это!", 3000);
                return false;
            }

            CreateTree(Main.Players[player].UUID, player.Position - new Vector3(0, 0, 1.0), 0, type, 0);
            Commands.RPChat("me", player, $"посадил(а) растение");
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы посадили росток, через некоторое время вернитесь, чтобы забрать плод!", 5000);
            return true;
        } // Посадка растения

        public static void PlantDrag(Player player)
        {
            try
            {
                if (!player.HasData("FERMA_ID")) return;

                GTANetworkAPI.Object obj = player.GetData<GTANetworkAPI.Object>("FERMA_ID");
                if (obj == null) return;
                if (!obj.HasData("FERMA_OWNER"))
                {
                    return;
                }
                if (obj.GetData<int>("FERMA_OWNER") != Main.Players[player].UUID)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Это не ваше растение!", 3000);
                    return;
                }

                obj.SetData("FERMA_SHAPEID", player.GetData<ColShape>("FERMA_SHAPEID"));

                /*  switch(interact) {
                      case 10567890: */
                if (player == null) return;

                if (player.HasData("FERMA_ID"))
                {
                    if (player.GetData<object>("FERMA_ID") == null) return;

                    DateTime ReadyToDrag = ((GTANetworkAPI.Object)player.GetData<object>("FERMA_ID")).GetData<DateTime>("FERMA_PROCESSRISE");
                    int Result = DateTime.Compare(DateTime.Now, ReadyToDrag);


                    if (Result <= 0)
                    {
                        DateTime g = new DateTime((ReadyToDrag - DateTime.Now).Ticks);
                        var min = g.Minute;
                        var sec = g.Second;
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Росток еще не готов! ( Осталось: {min}:{sec} )", 3000);
                        return;
                    }
                    int type = ((GTANetworkAPI.Object)player.GetData<object>("FERMA_ID")).GetData<int>("FERMA_TYPE");
                    int amount = ((GTANetworkAPI.Object)player.GetData<object>("FERMA_ID")).GetData<int>("FERMA_AMOUNT");

                    if (type == 0)
                    {
                        var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Potata, amount));

                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре!", 3000);
                            return;
                        }


                        if (amount == 0)
                        {
                            return;
                        }
                        nInventory.Add(player, new nItem(ItemType.Potata, amount));
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы успешно вырастили и собрали {amount} картофеля!", 3000);

                        DestroyTree((GTANetworkAPI.Object)player.GetData<object>("FERMA_ID"));

                    }
                    if (type == 1)
                    {

                        var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Pshenica, amount));

                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре!", 3000);
                            return;
                        }

                        if (amount == 0)
                        {
                            return;
                        }
                        nInventory.Add(player, new nItem(ItemType.Pshenica, amount));
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы успешно вырастили и собрали {amount} пшеницы!", 3000);

                        DestroyTree((GTANetworkAPI.Object)player.GetData<object>("FERMA_ID"));

                    }
                    if (type == 2)
                    {

                        var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Morkov, amount));

                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре!", 3000);
                            return;
                        }

                        if (amount == 0)
                        {
                            return;
                        }
                        nInventory.Add(player, new nItem(ItemType.Morkov, amount));
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы успешно вырастили и собрали {amount} моркови!", 3000);

                        DestroyTree((GTANetworkAPI.Object)player.GetData<object>("FERMA_ID"));

                    }
                    if (type == 3)
                    {

                        var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Clever, amount));

                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре!", 3000);
                            return;
                        }

                        if (amount == 0)
                        {
                            return;
                        }
                        nInventory.Add(player, new nItem(ItemType.Clever, amount));
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы успешно вырастили и собрали {amount} клевера!", 3000);

                        DestroyTree((GTANetworkAPI.Object)player.GetData<object>("FERMA_ID"));

                    }

                    NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
                    player.ResetData("FERMA_ID");
                    player.ResetData("FERMA_SHAPEID");

                }
                else
                {
                    NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
                    player.ResetData("FERMA_ID");
                    player.ResetData("FERMA_SHAPEID");
                }

                return;
                //  }
            }
            catch 
            {

            }
        } // Сбор растений

        private static bool SaveTree(GTANetworkAPI.Object obj)
        {
            try
            {
                MySqlCommand queryCommand = new MySqlCommand(@"
                    INSERT INTO `ferma`
                    (
                        `object_id`,
                        `object_pos`,
                        `object_owner`,
                        `object_type`,
                        `object_amount`
                    ) VALUES (
                        @ID,
                        @POS,
                        @OWNER,
                        @TYPE,
                        @AMOUNT
                    )
                ");

                queryCommand.Parameters.AddWithValue("@ID", obj.GetData<int>("FERMA_DB"));
                queryCommand.Parameters.AddWithValue("@POS", JsonConvert.SerializeObject(obj.Position));
                queryCommand.Parameters.AddWithValue("@OWNER", obj.GetData<int>("FERMA_OWNER"));
                queryCommand.Parameters.AddWithValue("@TYPE", obj.GetData<int>("FERMA_TYPE"));
                queryCommand.Parameters.AddWithValue("@AMOUNT", obj.GetData<int>("FERMA_AMOUNT"));

                MySQL.Query(queryCommand);

                return true;
            }
            catch (Exception ex)
            {
                Log.Write($"Exception at SaveTree with {ex.ToString()}");
                return false;
            }
        } // Сохранение растений в базу данных

        private static void DeleteTreeFromDB(GTANetworkAPI.Object obj)
        {
            try
            {
                MySqlCommand queryCommand = new MySqlCommand(@"
                    DELETE FROM `ferma`
                    WHERE `object_id` = @OBJ
                ");

                queryCommand.Parameters.AddWithValue("@OBJ", obj.GetData<int>("FERMA_DB"));

                MySQL.Query(queryCommand);
            }
            catch (Exception ex)
            {
                Log.Write($"Exception at DeleteTreeFromDB with {ex.ToString()}");
            }
        } // Удаление растений из базы данных
        public static void DeleteAllTreeFromDB(GTANetworkAPI.Object obj)
        {
            try
            {
                MySqlCommand queryCommand = new MySqlCommand(@"
                    DELETE FROM `ferma`
                    WHERE `object_id` > 0
                ");

                MySQL.Query(queryCommand);
            }
            catch (Exception ex)
            {
                Log.Write($"Exception at DeleteTreeFromDB with {ex.ToString()}");
            }
        } // Удаление растений из базы данных

        [ServerEvent(Event.ResourceStart)]
        public void LoadTreeFromDB()
        {
            try
            {
                MySqlCommand queryCommand = new MySqlCommand(@"SELECT * FROM `ferma`");

                DataTable Result = MySQL.QueryRead(queryCommand);

                if (Result == null)
                {
                    return;
                }

                int id, owner, amount, type;
                Vector3 pos;

                foreach (DataRow Row in Result.Rows)
                {
                    id = (int)Row["object_id"];
                    owner = (int)Row["object_owner"];
                    amount = (int)Row["object_amount"];
                    type = (int)Row["object_type"];
                    pos = JsonConvert.DeserializeObject<Vector3>(Row["object_pos"].ToString());
                    CreateTree(owner, pos, amount, type, id, false);
                }
            }
            catch (Exception ex)
            {
                Log.Write($"Exception at LoadTree with {ex.ToString()}");
            }
        } //
        #endregion planting

        #region Tractor
        public static void CheckTractor()
        {
            NAPI.Task.Run(() =>
            {
                if (FermaLeft != 0)
                {

                }
            });
        } // Провека тракторов (респавн, заправа)
        public static void SpawnСols(Player player)
        {


            foreach (var check in FermaKult)
            {


                var kultcol = NAPI.ColShape.CreateCylinderColShape(new Vector3(check.position.X, check.position.Y, check.position.Z), 1.5f, 6, 0);
                var marker = NAPI.Marker.CreateMarker(1, new Vector3(check.position.X, check.position.Y, check.position.Z), new Vector3(), new Vector3(), 1.5f, new Color(255, 215, 0));




                kultcol.OnEntityEnterColShape += (s, e) =>
                {

                    if (!Main.Players.ContainsKey(e)) return;
                    if (!e.IsInVehicle || !e.HasData("TRACTOR") || e.Vehicle != e.GetData<Vehicle>("TRACTOR")) return;
                    var vehicle = e.Vehicle;

                    NAPI.Entity.DeleteEntity(marker);
                    NAPI.Entity.DeleteEntity(kultcol);
                    if (FermaKultLeft == 0)
                    {
                        Notify.Send(e, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы успешно культивировали поле, продолжайте садить!", 3000);
                        Kult = false;
                        FermaKultLeft = 7500;
                        FermaLeft = 23500;

                        NAPI.Task.Run(() => {
                            try
                            {
                                foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                                    if (veh.HasData("TRACTOR"))
                                    {
                                        veh.GetData<Player>("DRIVER").ResetData("TRACTOR");
                                        veh.SetData("ACCESS", "NONE");
                                        veh.GetData<Player>("DRIVER").SetData("IN_WORK_CAR", false);
                                        veh.Delete();
                                    }

                            }
                            catch { }
                        });

                        return;
                    }
                    FermaKultLeft -= 1;
                    if (FermaKultLeft % 5 == 0)
                    {
                        Notify.Send(e, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы обработали участок поля, осталось {FermaKultLeft}", 3000);
                    }
                    // e.SetData("KULTSHAPE", kultcol);
                };
                kultcol.OnEntityExitColShape += (s, e) =>
                {
                    if (!Main.Players.ContainsKey(e)) return;

                    //  e.ResetData("KULTSHAPE");
                    //    e.ResetData("VEHSPAWNTEXTimpala64");
                };


            }

        } // Спавн колшейпов для культивации
        #endregion Tractor

        #region OnPlayerEnter-Exit shapes
        private void FermaShape_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
            }
            catch  { }
        }
        private void FermaShape_onEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch{ }
        }
        private static void Shape_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
                NAPI.Data.SetEntityData(entity, "FERMA_ID", shape.GetData<object>("FERMA_ID"));
                NAPI.Data.SetEntityData(entity, "FERMA_SHAPEID", shape);
            }
            catch (Exception ex)
            {
                Log.Write("Shape_onEntityEnterColShape: " + ex.Message, nLog.Type.Error);
            }
        }

        private static void Shape_onEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
                entity.ResetData("FERMA_ID");
                entity.ResetData("FERMA_SHAPEID");
            }
            catch (Exception ex)
            {
                Log.Write("Shape_onEntityExitColShape: " + ex.Message, nLog.Type.Error);
            }
        }
        #endregion

        [RemoteEvent("takeferma")] // Взаимодействие с клиентской частью
        public static void ClientEvent_takeferma(Player player)
        {
            try
            {
                if (player.GetData<object>("FERMA_ID") != null)
                {

                    if (FermaLeft == 0 && Kult == false)
                    {
                        Kult = true;
                        SpawnСols(player);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Поле нуждается в культивации", 3000);
                        return;
                    }
                    if (FermaLeft == 0 && Kult == true)
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Процесс культивации уже запущен, возьмите трактор", 3000);
                        player.ResetData("FERMA_ID");
                        return;
                    }

                    PlantDrag(player);
                }
            }
            catch (Exception e) { Log.Write($"takeferma: " + e.Message); }
        }
    }

}


