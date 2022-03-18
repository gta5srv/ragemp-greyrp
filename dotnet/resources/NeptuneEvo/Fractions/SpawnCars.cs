using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NeptuneEVO.GUI;
using NeptuneEVO.MoneySystem;
using NeptuneEVO.SDK;
using System.Threading;
using NeptuneEVO.Core;
using NeptuneEVO.Businesses;

namespace NeptuneEVO.Fractions
{
    class SpawnCars : Script
    {

        public static Dictionary<int, Vector3> ListFracAutoVector = new Dictionary<int, Vector3>
        {
            {1, new Vector3(3.578885, -1406.009, 28.14561) },
            {2, new Vector3(127.8017, -1997.125, 17.22757) },
            {3, new Vector3(859.4219, -2352.438, 29.21177) },
            {4, new Vector3(1389.459, -2067.248, 50.87851) },
            {5, new Vector3(976.4622, -1823.972, 30.03258) },
            {6, new Vector3(-584.3541, -156.535, 36.80988) },
            {7, new Vector3(431.6695, -988.9024, 24.579813) },
            {8,new Vector3(327.05005, -570.1817, 27.676847) },
            {9, new Vector3(122.5162, -700.265, 31.99674) },
            {10, new Vector3(1390.152, 1117.767, 113.6927) },
            {11, new Vector3(-111.3165, 1007.853, 234.6579) },
            {12, new Vector3(-1528.49, 84.59692, 55.51393) },
            {13, new Vector3(-1920.981, 2044.276, 139.615) },
            {14, new Vector3(-2310.761, 3272.444, 31.70684) },
            {15, new Vector3(-1046.083, -226.20459, 37.893284) },
            {16, new Vector3(0,0,0) },
            {17, new Vector3(4966.307, -5705.3315, 19.904757) },
            {18, new Vector3(-160.5019, -587.9333, 31.30446) },
			
        };

        public static Dictionary<int, Vector3> ListFracAutoAngle = new Dictionary<int, Vector3>
        {
            {1, new Vector3(0, 0, 93.69801) },
            {2, new Vector3(0, 0, 181.5985) },
            {3, new Vector3(0, 0, 357.29) },
            {4, new Vector3(0, 0, 46.02231) },
            {5, new Vector3(0, 0, 356.8896) },
            {6, new Vector3(0, 0, 290.9149) },
            {7, new Vector3(0, 0, 176.53444) },
            {8, new Vector3(0, 0, -20.47136) },
            {9, new Vector3(0, 0, 166.5204) },
            {10, new Vector3(0, 0, 85.18863) },
            {11, new Vector3(0, 0, 108.7052) },
            {12, new Vector3(0, 0, 274.4118) },
            {13, new Vector3(0, 0, 257.3031) },
            {14, new Vector3(0, 0, 59.99879) },
            {15, new Vector3(0, 0, 186.0792) },
            {16, new Vector3(0,0,0) },
            {17, new Vector3(1.1388774, -0.07543097, -29.803839) },
            {18, new Vector3(0, 0, 160.0581) },
			

        };

        private static nLog Log = new nLog("SpawnFracCars");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var result = MySQL.QueryRead($"SELECT * FROM vehpoints");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB rod return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    Vector3 pos = JsonConvert.DeserializeObject<Vector3>(Row["pos"].ToString());

                    SpawnCarPoint data = new SpawnCarPoint(pos, Convert.ToInt32(Row["id"]));
                    int id = Convert.ToInt32(Row["id"]);
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"RODINGS\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static void interactionPressed(Player player, int id)
        {
            try
            {
                switch (id)
                {
                    case 222:
                        if (Main.Players[player].FractionID != player.GetData<int>("FRACIDAU"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не состоите в этой фракции", 3000);
                            return;
                        }
                        var idfrac = Main.Players[player].FractionID;
                        //BCore.Bizness biz = BCore.BizList[66];
                        List<List<string>> items = new List<List<string>>();

                        foreach (var veh in Configs.FractionVehicles[idfrac])
                        {
                            if (Main.Players[player].FractionLVL < veh.Value.Item4) continue;
                            List<string> item = new List<string>();
                            item.Add(ParkManager.GetNormalName(veh.Value.Item1.ToString()));
                            item.Add($"{veh.Key}");
                            items.Add(item);
                        }

                        string json = JsonConvert.SerializeObject(items);
                        Trigger.PlayerEvent(player, "fracauto", json);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"PARK_INTERACTION\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static string FindFractionList(Player player,int frac, int num)
        {
            var i = 1;
            foreach (var veh in Configs.FractionVehicles[frac])
            {
                if (Main.Players[player].FractionLVL < veh.Value.Item4) continue;
                if (i == num)
                {
                    return veh.Key;
                }
                i++;
            }
            return "";
        }

        public static bool FindByNumFrac(Player player, int num, string number)
        {
            foreach (var v in NAPI.Pools.GetAllVehicles())
            {
                if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "FRACTION" && NAPI.Vehicle.GetVehicleNumberPlate(v) == number && v.HasData("FRACTION") && v.GetData<int>("FRACTION") == num)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool FindVehInfo(int num, string number)
        {
            foreach (var v in NAPI.Pools.GetAllVehicles())
            {
                if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "FRACTION" && NAPI.Vehicle.GetVehicleNumberPlate(v) == number && v.HasData("FRACTION") && v.GetData<int>("FRACTION") == num)
                {
                    foreach (Player Player in NAPI.Pools.GetAllPlayers())
                    {
                        if (Player.IsInVehicle)
                        {
                            if (NAPI.Player.GetPlayerVehicle(Player) == v)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static Vehicle GetCarByNumber(int num, string number)
        {
            foreach (var v in NAPI.Pools.GetAllVehicles())
            {
                if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "FRACTION" && NAPI.Vehicle.GetVehicleNumberPlate(v) == number)
                {
                    //Log.Write($"CAR FIND, THIS FUCK: {v} ", nLog.Type.Warn);
                    return v;
                }
            }
            return null;
        }

        [RemoteEvent("fracauto")]
        public static void Event_CallbackSpawnCar(Player player, int key)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].FractionID == 0) return;
                key += 1;

                var number = FindFractionList(player, Main.Players[player].FractionID, key);

                var veh = Configs.FractionVehicles[Main.Players[player].FractionID][number];
                if (veh.Item1 == VehicleHash.Barracks)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Данный вид транспорта нельзя заспавнить!", 3000);
                    return;
                }
                if (Main.Players[player].FractionLVL < veh.Item4)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет доступа к {veh.Item1.ToString()}", 3000);
                    return;
                }
                if (FindByNumFrac(player, Main.Players[player].FractionID, number))
                {
                    if (FindVehInfo(Main.Players[player].FractionID, number))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Сейчас в данной машине кто-то сидит!", 3000);
                        return;
                    }
                    else
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Машина была возвращенна на парковку!", 3000);
                        var vehf = GetCarByNumber(key, number);
                        vehf.Delete();
                        return;
                    }

                }


                var model = veh.Item1;
                var canmats = (model == VehicleHash.Barracks || model == VehicleHash.Youga || model == VehicleHash.Burrito3 || model == VehicleHash.Rumpo3); // "CANMATS"
                var candrugs = (model == VehicleHash.Youga || model == VehicleHash.Burrito3); // "CANDRUGS"
                var canmeds = (model == VehicleHash.Ambulance); // "CANMEDKITS"
                var vehi = NAPI.Vehicle.CreateVehicle(model, ListFracAutoVector[Main.Players[player].FractionID], ListFracAutoAngle[Main.Players[player].FractionID], veh.Item5, veh.Item6);

                NAPI.Data.SetEntityData(vehi, "ACCESS", "FRACTION");
                NAPI.Data.SetEntityData(vehi, "FRACTION", Main.Players[player].FractionID);
                NAPI.Data.SetEntityData(vehi, "MINRANK", veh.Item4);
                NAPI.Data.SetEntityData(vehi, "TYPE", Configs.FractionTypes[Main.Players[player].FractionID]);
                if (canmats)
                    NAPI.Data.SetEntityData(vehi, "CANMATS", true);
                if (candrugs)
                    NAPI.Data.SetEntityData(vehi, "CANDRUGS", true);
                if (canmeds)
                    NAPI.Data.SetEntityData(vehi, "CANMEDKITS", true);
                NAPI.Vehicle.SetVehicleNumberPlate(vehi, number);
                Core.VehicleStreaming.SetEngineState(vehi, false);
                VehicleManager.FracApplyCustomization(vehi, Main.Players[player].FractionID);
                if (model == VehicleHash.Submersible || model == VehicleHash.Thruster) vehi.SetSharedData("PETROL", 0);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваша машина ждёт на парковке!", 3000);
            }
            catch (Exception e) { Log.Write("ERROR ON SPAWN CAR : " + e.ToString(), nLog.Type.Error); }


        }

        [Command("sfa")] // команда для установики метки спавна авто фракций 
        public static void CMD_createSpawnCar(Player player, int id)
        {
            try
            {
                if (!Group.CanUseCmd(player, "createbusiness")) return;

                var pos = player.Position;

                SpawnCarPoint biz = new SpawnCarPoint(pos, id);


                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Успешно созданна точка авто для фракции: {id}", 3000);
                MySQL.Query($"INSERT INTO vehpoints (id, pos) " + $"VALUES ({id}, '{JsonConvert.SerializeObject(pos)}')");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        internal class SpawnCarPoint
        {
            public Vector3 Position { get; }
            public int FracId { get; set; }

            [JsonIgnore]
            private ColShape shape = null;
            [JsonIgnore]
            private TextLabel label = null;
            [JsonIgnore]
            private Marker marker = null;

            public SpawnCarPoint(Vector3 pos, int id)
            {
                Position = pos;
                FracId = id;
                //blip = NAPI.Blip.CreateBlip(255, pos, 1, 4, "Транспорт фракции", 225, 0, true);
                shape = NAPI.ColShape.CreateCylinderColShape(pos, 2f, 3, 0);
                shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 222);
                        entity.SetData("FRACIDAU", FracId);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 0);
                        entity.SetData("FRACIDAU", 0);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                label = NAPI.TextLabel.CreateTextLabel("~b~Транспорт ", new Vector3(pos.X, pos.Y, pos.Z), 20F, 0.5F, 0, new Color(255, 255, 255), true, 0);
                marker = NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 1f), new Vector3(), new Vector3(), 1f, new Color(0, 175, 250, 220), false, 0);
            }
        }



    }
}
