using GTANetworkAPI;
using NeptuneEVO.SDK;
using System;
using System.Collections.Generic;
using System.IO;

namespace NeptuneEVO.Houses
{
    class Realtor : Script
    {
        private static nLog RLog = new nLog("RealtorManager");
        private static List<object> HouseList = new List<object>();
        private static ColShape shape;
        private static Marker intmarker;
        private static Blip blip;
        private static Vector3 PositionRealtor = new Vector3(234.0478, -407.7426, 48);
        private static int[] PriceToInfo = { 15, 25, 50, 85, 100, 120, 175};
        private static int[] Interact = { 510, 511, 512, 513, 514, 515, 516 };
        private static List<Vector3> Vectors = new List<Vector3>
        {
            new Vector3(-1040.167, -1380.4, 4.466526),
            new Vector3(-1044.932, -1378.534, 4.466526),
            new Vector3(-1050.178, -1378.824, 4.466526),
            new Vector3(-1043.232, -1386.745, 4.466526),
            new Vector3(-1047.673, -1384.036, 4.466526),
            new Vector3(-1053.029, -1384.266, 4.466526),
            new Vector3(-1021.849, -1378.973, 4.466526),
        };


        [ServerEvent(Event.ResourceStart)]
        public static void EnterShapeRealtor()
        {
            try
            {
                #region #1AL Creating Marker & Colshape & Blip
                for(int i = 0; i < 7; i++)
                {
                    intmarker = NAPI.Marker.CreateMarker(27, Vectors[i] + new Vector3(0, 0, 0.1), new Vector3(), new Vector3(), 1f, new Color(0, 0, 240), false, 0);
                    NAPI.TextLabel.CreateTextLabel($"~b~{Houses.HouseManager.HouseTypeList[i].Name}", new Vector3(Vectors[i].X, Vectors[i].Y, Vectors[i].Z + 1.9), 1f + 2f, 0.5F, 0, new Color(255, 255, 255), true, 0);
                    shape = NAPI.ColShape.CreateCylinderColShape(Vectors[i], 2, 2, 0);
                    shape.SetData("CHECK", Interact[i]);
                    shape.OnEntityEnterColShape += (s, ent) =>
                    {
                        try
                        {
                            NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", s.GetData<int>("CHECK"));
                        }
                        catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                    };
                    shape.OnEntityExitColShape += (s, ent) =>
                    {
                        try
                        {
                            NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                        }
                        catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
                    };

                }
                blip = NAPI.Blip.CreateBlip(418, new Vector3(-1043.232, -1386.745, 4.466526), 1, 50, "Агентство недвижимости", shortRange: true, dimension: 0);

                shape = NAPI.ColShape.CreateCylinderColShape(new Vector3(-1029.413, -1402.391, 4.437828), 2, 2, 0);
                shape.OnEntityEnterColShape += (s, ent) =>
                {
                    try
                    {
                        Notify.Send(ent, NotifyType.Info, NotifyPosition.BottomCenter, "Пройдите в офис для просмотра недвижимостей!", 5000);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                };


                #endregion

                RLog.Write("Loaded", nLog.Type.Info);
            }
            catch (Exception e) { RLog.Write(e.ToString(), nLog.Type.Error); }
        }

        public static void OpenRealtorMenu(Player player, int houseclass)
        {
            try
            {
                Main.Players[player].ExteriorPos = player.Position;
                NAPI.Entity.SetEntityDimension(player, 1000);


                Trigger.PlayerEvent(player, "openRealtorMenu", houseclass);
            }
            catch (Exception e) { RLog.Write(e.ToString(), nLog.Type.Error); }

        }

        [RemoteEvent("closeRealtorMenu")]
        public static void CloseRealtorMenu(Player player)
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    Trigger.PlayerEvent(player, "unfr");
                }, 3000);

                player.Position = Main.Players[player].ExteriorPos;
                Main.Players[player].ExteriorPos = new Vector3();
                NAPI.Entity.SetEntityDimension(player, 0);
            }
            catch (Exception e) { RLog.Write(e.ToString(), nLog.Type.Error); }
        }

        [RemoteEvent("buyRealtorInfoHome")]
        public static void BuyInfoHome(Player player, int hclass, float x, float y)
        {
            try
            {
                if (PriceToInfo[hclass] > Main.Players[player].Money)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас не хватает средств для покупки информации", 3000);
                }
                else
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Маршрут установлен", 3000);
                    NAPI.Task.Run(() => { if (player != null) Trigger.PlayerEvent(player, "createWaypoint", x, y); }, 3000);

                    MoneySystem.Wallet.Change(player, -PriceToInfo[hclass]);
                }
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        Trigger.PlayerEvent(player, "closeRealtorMenu");
                    }
                    catch { }
                }, 200);
            }
            catch (Exception e) { RLog.Write(e.ToString(), nLog.Type.Error); }
        }

        [RemoteEvent("LoadHouseToMenu")]
        public static void LoadHouseToMenu(Player player, int houseclass)
        {
            try
            {
                foreach (House house in HouseManager.Houses.FindAll(x => x.Type == houseclass))
                {
                    if (house.Owner == "" && house.Apart == -1)
                    {
                        int garagePlace = GarageManager.Garages.ContainsKey(house.GarageID) ? GarageManager.GarageTypes[GarageManager.Garages[house.GarageID].Type].MaxCars : -1;

                        List<object> data = new List<object>
                        {
                            house.ID,
                            house.Type,
                            house.Price,
                            house.Position,
                            garagePlace
                        };
                        HouseList.Add(data);
                    }
                }
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(HouseList);
                string prices = Newtonsoft.Json.JsonConvert.SerializeObject(PriceToInfo);
                Vector3 vec = Main.Players[player].ExteriorPos;
                List<float> vector = new List<float> { vec.X, vec.Y, vec.Z };
                Trigger.PlayerEvent(player, "LoadHouse", json, prices, Newtonsoft.Json.JsonConvert.SerializeObject(vector));

                HouseList.Clear();
            }
            catch (Exception e)
            {
                RLog.Write(e.ToString(), nLog.Type.Error);
            }
        }
    }
}
