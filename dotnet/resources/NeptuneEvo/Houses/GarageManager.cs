using GTANetworkAPI;
using Newtonsoft.Json;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace NeptuneEVO.Houses
{
    #region GarageType Class
    class GarageType
    {
        public int MaxCars { get; }

        public GarageType(int maxCars)
        {
            MaxCars = maxCars;
        }
    }
    #endregion

    #region Garage Class
    class Garage
    {
        public int ID { get; }
        public int Type { get; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        [JsonIgnore] public int Dimension { get; set; }

        [JsonIgnore]
        private ColShape shape;

        [JsonIgnore]
        private ColShape intShape;
        [JsonIgnore]
        private Marker intMarker;

        [JsonIgnore]
        public Dictionary<string, Tuple<int, Entity>> entityVehicles = new Dictionary<string, Tuple<int, Entity>>();
        [JsonIgnore]
        public Dictionary<string, Entity> vehiclesOut = new Dictionary<string, Entity>();
        private nLog Log = new nLog("Garage");

        public Garage(int id, int type, Vector3 position, Vector3 rotation)
        {
            ID = id;
            Type = type;
            Position = position;
            Rotation = rotation;

            shape = NAPI.ColShape.CreateCylinderColShape(position - new Vector3(0, 0, 1), 5, 10, 0);
            shape.OnEntityEnterColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "GARAGEID", id);
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 40);
                }
                catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
            };
            shape.OnEntityExitColShape += (s, ent) =>
            {
                try
                {
                    if (NAPI.Entity.GetEntityType(ent) != EntityType.Player) return;
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                    NAPI.Data.ResetEntityData(ent, "GARAGEID");
                }
                catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
            };
        }
        public bool CheckCar(bool checkin, string number)
        {
            if (checkin)
            {
                if (entityVehicles.ContainsKey(number)) return true;
                else return false;
            }
            else
            {
                if (vehiclesOut.ContainsKey(number)) return true;
                else return false;
            }
        }

        public void SetPos(Vector3 pos, Vector3 angle)
        {
            try
            {
                Position = pos;
                Rotation = angle;
                shape.Delete();
            }
            catch { }
        }

        public void SetOutVehicle(string number, Entity veh)
        {
            vehiclesOut.Add(number, veh);
        }

        public void RemoveOutVehicle(string number)
        {
            vehiclesOut.Remove(number);
        }
        public Vehicle GetOutsideCar(string number)
        {
            if (!vehiclesOut.ContainsKey(number)) return null;
            return NAPI.Entity.GetEntityFromHandle<Vehicle>(vehiclesOut[number]);
        }
        public void DeleteCar(string number, bool resetPosition = true)
        {
            if (entityVehicles.ContainsKey(number))
            {
                NAPI.Task.Run(() => {
                    try
                    {
                        if (VehicleManager.Vehicles.ContainsKey(number))
                        {
                            VehicleManager.Vehicles[number].Items = NAPI.Data.GetEntityData(entityVehicles[number].Item2, "ITEMS");
                            var vclass = NAPI.Vehicle.GetVehicleClass(NAPI.Util.VehicleNameToModel(VehicleManager.Vehicles[number].Model));
                            VehicleManager.Vehicles[number].Fuel = (!entityVehicles[number].Item2.HasSharedData("PETROL")) ? VehicleManager.VehicleTank[vclass] : entityVehicles[number].Item2.GetSharedData<int>("PETROL");
                        }
                        NAPI.Entity.DeleteEntity(entityVehicles[number].Item2);
                        entityVehicles.Remove(number);
                    }
                    catch { }
                });
            }

            if (vehiclesOut.ContainsKey(number))
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (VehicleManager.Vehicles.ContainsKey(number))
                        {
                            VehicleManager.Vehicles[number].Items = NAPI.Data.GetEntityData(vehiclesOut[number], "ITEMS");
                            var vclass = NAPI.Vehicle.GetVehicleClass(NAPI.Util.VehicleNameToModel(VehicleManager.Vehicles[number].Model));
                            VehicleManager.Vehicles[number].Fuel = (!vehiclesOut[number].HasSharedData("PETROL")) ? VehicleManager.VehicleTank[vclass] : vehiclesOut[number].GetSharedData<int>("PETROL");
                        }
                        NAPI.Entity.DeleteEntity(vehiclesOut[number]);
                        vehiclesOut.Remove(number);
                    }
                    catch { }
                });
                if (resetPosition) VehicleManager.Vehicles[number].Position = null;
            }
        }
        public void DeleteOffCar(string number)
        {
            if (number == null) return;
            if (vehiclesOut.ContainsKey(number))
            {
                vehiclesOut.Remove(number);
            }
        }
        public void Create()
        {
            MySQL.Query($"INSERT INTO `garages`(`id`,`type`,`position`,`rotation`) VALUES ({ID},{Type},'{JsonConvert.SerializeObject(Position)}','{JsonConvert.SerializeObject(Rotation)}')");
        }
        public void Save()
        {
            //MySQL.Query($"UPDATE `garages` SET `data`='{JsonConvert.SerializeObject(this)}' WHERE `id`='{ID}'");
        }
        public void Destroy()
        {
            shape.Delete();
            intShape.Delete();
            intMarker.Delete();
        }
        public void SpawnCar(string number)
        {
            if (entityVehicles.ContainsKey(number))
            {
                return;
            }
            if (vehiclesOut.ContainsKey(number))
            {
                return;
            }

            int i = 0;

            for (i = 0; i < 10; i++)
            {
                if (entityVehicles.Values.FirstOrDefault(t => t.Item1 == i) == null)
                {
                    break;
                }
            }

            if (i >= GarageManager.vehPositions.Count)
            {
                return;
            }

            var vehData = VehicleManager.Vehicles[number];
            if (vehData.Health < 1)
            {
                return;
            }

            var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vehData.Model), GarageManager.vehPositions[i] + new Vector3(0, 0, 0.25), GarageManager.vehRotations[i], 0, 0);
            veh.Rotation = GarageManager.vehRotations[i];
            veh.NumberPlate = number;
            NAPI.Entity.SetEntityDimension(veh, (uint)Dimension);
            VehicleStreaming.SetEngineState(veh, false);
            VehicleStreaming.SetLockStatus(veh, true);
            veh.SetData("ACCESS", "GARAGE");
            veh.SetData("ITEMS", vehData.Items);
            veh.SetSharedData("PETROL", vehData.Fuel);
            NAPI.Task.Run(() =>
            {
                if (veh != null)
                    VehicleManager.ApplyCustomization(veh);
            }, 500);
            entityVehicles.Add(number, new Tuple<int, Entity>(i, veh));
        }

        public void DestroyCars()
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    foreach (var veh in entityVehicles)
                    {
                        VehicleManager.Vehicles[veh.Key].Items = NAPI.Data.GetEntityData(veh.Value.Item2, "ITEMS");
                        NAPI.Entity.DeleteEntity(veh.Value.Item2);
                    }
                    entityVehicles = new Dictionary<string, Tuple<int, Entity>>();

                    foreach (var veh in vehiclesOut)
                    {
                        VehicleManager.Vehicles[veh.Key].Items = NAPI.Data.GetEntityData(veh.Value, "ITEMS");
                        NAPI.Entity.DeleteEntity(veh.Value);
                        VehicleManager.Vehicles[veh.Key].Position = null;
                    }
                    vehiclesOut = new Dictionary<string, Entity>();
                }
                catch { }
            });
        }
        public void RespawnCars()
        {
            try
            {
                List<string> vehicles = entityVehicles.Keys.ToList();

                foreach (var v in NAPI.Pools.GetAllVehicles())
                {
                    if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "GARAGE" && vehicles.Contains(v.NumberPlate))
                    {
                        if (VehicleManager.Vehicles.ContainsKey(v.NumberPlate) && v.HasData("ITEMS"))
                            VehicleManager.Vehicles[v.NumberPlate].Items = v.GetData<List<nItem>>("ITEMS");
                        v.Delete();
                    }
                }
                entityVehicles.Clear();

                //SpawnCars(vehicles);
            }
            catch { }
        }
        public void SpawnCarAtPosition(Player player, string number, Vector3 position, Vector3 rotation)
        {
            if (vehiclesOut.ContainsKey(number))
            {
                Main.Players[player].LastVeh = "";
                return;
            }

            var vData = VehicleManager.Vehicles[number];
            var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vData.Model), position, rotation, 0, 0, number);
            veh.Rotation = rotation;
            vehiclesOut.Add(number, veh);

            veh.SetSharedData("PETROL", vData.Fuel);
            veh.SetData("ACCESS", "PERSONAL");
            veh.SetData("OWNER", player);
            veh.SetData("ITEMS", vData.Items);

            NAPI.Vehicle.SetVehicleNumberPlate(veh, number);

            VehicleStreaming.SetEngineState(veh, false);
            VehicleStreaming.SetLockStatus(veh, true);



            NAPI.Task.Run(() =>
            {
                try
                {
                    VehicleManager.ApplyCustomization(veh);
                }
                catch { }
            }, 3000);

        }
        public void GetVehicleFromGarage(Player player, string number)
        {
            var vData = VehicleManager.Vehicles[number];
            var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vData.Model), player.Position + new Vector3(0, 0, 0.3), Rotation, 0, 0, number);
            veh.NumberPlate = number;
            veh.Rotation = Rotation;
			veh.Position = Position;
            vehiclesOut.Add(number, veh);
            veh.SetSharedData("PETROL", vData.Fuel);
            veh.SetData("ACCESS", "PERSONAL");
            veh.SetData("OWNER", player);
            veh.SetData("ITEMS", vData.Items);

            NAPI.Vehicle.SetVehicleNumberPlate(veh, number);

            if (Type == -1)
            {
                VehicleStreaming.SetEngineState(veh, false);
                VehicleStreaming.SetLockStatus(veh, true);
            }
            else
            {
                player.SetIntoVehicle(veh, 0);
                NAPI.Task.Run(() => { try { if (player != null && veh != null) player.SetIntoVehicle(veh, 0); veh.Rotation = Rotation; } catch { } }, 1000);
                if (vData.Fuel > 0)
                    VehicleStreaming.SetEngineState(veh, true);
                else
                    VehicleStreaming.SetEngineState(veh, false);
            }

            if (Type != -1)
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        NAPI.Entity.DeleteEntity(entityVehicles[number].Item2);
                        entityVehicles.Remove(number);
                    }
                    catch { }
                });
            }

            VehicleManager.ApplyCustomization(veh);

            NAPI.Task.Run(() =>
            {
                try
                {
                    VehicleManager.ApplyCustomization(veh);
                }
                catch { }
            }, 300);
        }
        public void SendVehicleIntoGarage(string number)
        {
            vehiclesOut.Remove(number);
            VehicleManager.Vehicles[number].Position = null;
            if (Type != -1) SpawnCar(number);
        }

        public static Vector3 enterPoint = new Vector3(240.411, -1004.753, -100);

        public void SendPlayer(Player player)
        {
            NAPI.Entity.SetEntityDimension(player, Convert.ToUInt32(Dimension));
            NAPI.Entity.SetEntityPosition(player, enterPoint);
            Main.Players[player].InsideGarageID = ID;
            //Оптимизация
            RespawnCars();
        }
        public void RemovePlayer(Player player)
        {
            NAPI.Entity.SetEntityDimension(player, 0);
            NAPI.Entity.SetEntityPosition(player, Position + new Vector3(0,0,2f));
            Main.Players[player].InsideGarageID = -1;
        }
        public void SendAllVehiclesToGarage()
        {
            try
            {
                var toSend = new List<string>();
                foreach (var v in vehiclesOut)
                {
                    toSend.Add(v.Key);
                    VehicleManager.Vehicles[v.Key].Items = NAPI.Data.GetEntityData(v.Value, "ITEMS");
                    var vclass = NAPI.Vehicle.GetVehicleClass(NAPI.Util.VehicleNameToModel(VehicleManager.Vehicles[v.Key].Model));
                    VehicleManager.Vehicles[v.Key].Fuel = (!v.Value.HasSharedData("PETROL")) ? VehicleManager.VehicleTank[vclass] : v.Value.GetSharedData<int>("PETROL");
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            NAPI.Entity.DeleteEntity(v.Value);
                        }
                        catch { }
                    });
                }
                foreach (var v in toSend)
                {
                    SendVehicleIntoGarage(v);
                }
            }
            catch { }
        }
        public string SendVehiclesInsteadNearest(List<Player> Roommates, Player player)
        {
            var number = "";
            var nearPlayerVehicles = new List<Vehicle>();
            var toSend = new List<string>();
            foreach (var v in vehiclesOut)
            {
                if (v.Value == null) continue;
                var veh = NAPI.Entity.GetEntityFromHandle<Vehicle>(v.Value);
                var someNear = false;
                foreach (var p in Roommates)
                {
                    if (p.Position.DistanceTo(veh.Position) < 100 && p != null)
                    {
                        if (veh.HasData("PARKCLASS")) SendVehicleIntoGarage(v.Key);
                        someNear = true;
                        break;
                    }
                }

                if (!someNear)
                {
                    if (player.Position.DistanceTo(veh.Position) < 300) nearPlayerVehicles.Add(veh);
                    toSend.Add(v.Key);
                }
            }

            Vehicle nearestVehicle = null;
            foreach (var v in nearPlayerVehicles)
            {
                if (nearestVehicle == null)
                {
                    nearestVehicle = v;
                    continue;
                }
                if (player.Position.DistanceTo(v.Position) < nearestVehicle.Position.DistanceTo(v.Position)) nearestVehicle = v;
            }

            if (nearestVehicle != null)
            {
                if (!nearestVehicle.HasData("PARKCLASS"))
                {
                    toSend.Remove(nearestVehicle.NumberPlate);
                    number = nearestVehicle.NumberPlate;
                    VehicleManager.Vehicles[number].Position = JsonConvert.SerializeObject(nearestVehicle.Position);
                    VehicleManager.Vehicles[number].Rotation = JsonConvert.SerializeObject(nearestVehicle.Rotation);
                    VehicleManager.Save(number);
                    //NAPI.Util.ConsoleOutput("delete " + number);
                    DeleteCar(number, false);
                }
            }

            try
            {
                foreach (var v in toSend)
                {
                    if (vehiclesOut.ContainsKey(v))
                    {
                        VehicleManager.Vehicles[v].Items = NAPI.Data.GetEntityData(vehiclesOut[v], "ITEMS");
                        var vclass = NAPI.Vehicle.GetVehicleClass(NAPI.Util.VehicleNameToModel(VehicleManager.Vehicles[v].Model));
                        VehicleManager.Vehicles[v].Fuel = (!vehiclesOut[v].HasSharedData("PETROL")) ? VehicleManager.VehicleTank[vclass] :vehiclesOut[v].GetSharedData<int>("PETROL");
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                NAPI.Entity.DeleteEntity(vehiclesOut[v]);
                            }
                            catch { };
                            SendVehicleIntoGarage(v);
                        });
                    }
                }
            }
            catch (Exception e) { Log.Write($"SendVehiclesInsteadNearest: " + e.Message, nLog.Type.Error); }

            return number;
        }

        public static void InteractOnType(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;

                List<List<string>> items = new List<List<string>>();

                House house = HouseManager.GetHouse(player, false);
                House apartament = Houses.HouseManager.GetApart(player, false);
                if (house == null)
                {
                    if (apartament != null)
                    {
                        house = apartament;
                    }
                    else
                        return;
                }

                if (Main.Players[player].FamilyCID != null)
                {
                    Golemo.Families.Family family = Golemo.Families.Manager.Families.Find(x => x.FamilyCID == Main.Players[player].FamilyCID);
                    if (family != null)
                    {
                        House houset = HouseManager.Houses.Find(x => x.ID == family.FamilyHouse);
                        if (houset != null && GarageManager.Garages.ContainsKey(houset.GarageID) && GarageManager.Garages[houset.GarageID].Type != -1 && player.Dimension == GarageManager.Garages[houset.GarageID].Dimension)
                        {
                            house = houset;
                        }
                    }
                }


                foreach (var veh in VehicleManager.getAllPlayerVehicles(house.Owner))
                {
                    List<string> item = new List<string>();
                    item.Add(ParkManager.GetNormalName(VehicleManager.Vehicles[veh].Model));
                    item.Add($"{veh}");
                    items.Add(item);
                }

                string json = JsonConvert.SerializeObject(items);
                Trigger.PlayerEvent(player, "garageauto", json);
            }
            catch { }
        }

        public void CreateInterior()
        {
            #region Creating Interior ColShape
            intMarker = NAPI.Marker.CreateMarker(1, enterPoint - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220), false, (uint)Dimension);

            intShape = NAPI.ColShape.CreateCylinderColShape(enterPoint - new Vector3(0, 0, 1.12), 1.2f, 10f, (uint)Dimension);
            intShape.OnEntityEnterColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 41);
                }
                catch (Exception ex) { Console.WriteLine("intShape.OnEntityEnterColShape: " + ex.Message); }
            };
            intShape.OnEntityExitColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                }
                catch (Exception ex) { Console.WriteLine("intShape.OnEntityExitColShape: " + ex.Message); }
            };

            NAPI.TextLabel.CreateTextLabel("Вызов авто", new Vector3(237.218, -1003.24, -99.12) + new Vector3(0, 0, 1f), 3f, 1f, 0, new Color(255, 255, 255), false, (uint)Dimension);
            NAPI.Marker.CreateMarker(27, new Vector3(237.218, -1003.24, -99.12) - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220), false, (uint)Dimension);
            ColShape Shape = NAPI.ColShape.CreateCylinderColShape(new Vector3(237.218, -1003.24, -99.12) + new Vector3(0, 0, 1.12), 1.2f, 10f, (uint)Dimension);

            Shape.OnEntityEnterColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 157);
                }
                catch (Exception ex) { Console.WriteLine("intShape.OnEntityEnterColShape: " + ex.Message); }
            };
            Shape.OnEntityExitColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                }
                catch (Exception ex) { Console.WriteLine("intShape.OnEntityExitColShape: " + ex.Message); }
            };

            #endregion
        }
    }
    #endregion


    class GarageManager : Script
    {
        private static nLog Log = new nLog("Garages");

        public static string GetNumberByKey(Player player, int key, House house)
        {
            try
            {
                int i = 0;
                foreach (string number in VehicleManager.getAllPlayerVehicles(house.Owner))
                    if (i == key)
                        return number;
                    else
                        i++;
                return "";
            }
            catch { return ""; }
        }


        [RemoteEvent("garageauto")]
        public static void GarageAuto(Player player, int key)
        {
            try
            {
                if (player.HasData("ORG"))
                {
                    Organization.MaterialsI.SpawnCar(player, key);
                    return;
                }
                else if (player.HasData("AIR"))
                {
                    AirVehicles.SpawnCar(player, key);
                    return;
                }


                House house = null;

                if (player.GetData<bool>("APART_GARAGE"))
                {
                    house = HouseManager.GetApart(player, false);
                }
                else
                {
                    house = HouseManager.GetHouse(player, false);
                }

                if (Main.Players[player].FamilyCID != null)
                {
                    Golemo.Families.Family family = Golemo.Families.Manager.Families.Find(x => x.FamilyCID == Main.Players[player].FamilyCID);
                    if (family != null)
                    {
                        House houset = HouseManager.Houses.Find(x => x.ID == family.FamilyHouse);
                        if (houset != null && GarageManager.Garages.ContainsKey(houset.GarageID) && GarageManager.Garages[houset.GarageID].Type != -1 && player.Dimension == GarageManager.Garages[houset.GarageID].Dimension)
                        {
                            house = houset;
                        }
                    }
                 }

                string number = GetNumberByKey(player, key, house);
                if (string.IsNullOrEmpty(number)) return;
                if (GarageManager.Garages[house.GarageID].Dimension != player.Dimension) return;

                if (Main.Players[player].FamilyCID != null)
                {
                    Golemo.Families.Family family = Golemo.Families.Manager.Families.Find(x => x.FamilyCID == Main.Players[player].FamilyCID);
                    if (family != null)
                        if (family.Vehicles != null && family.Vehicles.Count > 0 && family.Vehicles.ContainsKey(number) && family.Vehicles[number] > Main.Players[player].FamilyRank)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.CenterRight, $"Этот транспорт доступен с {family.Vehicles[number]} ранга", 2000);
                            return;
                        }
                 }



                

                if (VehicleManager.Vehicles[number].OnPlast)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.CenterRight, "Этот транспорт стоит на штрафт стоянке", 2000);
                    return;
                }


                Garage garage = Garages[house.GarageID];
                if (garage == null) return;
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                        {
                            if (veh.NumberPlate == number)
                            {
                                veh.Delete();
                                break;
                            }
                                
                        }

                        if (!garage.entityVehicles.ContainsKey(number))
                        {
                            if (garage.entityVehicles.Count + 1 > 10)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.CenterRight, "У вас не хватает места в гараже!", 2000);
                                return;
                            }
                            if (FineManager.GetHaveFine(number, player.Name.ToString()) == 10)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.CenterRight, "У вашего транспорта 10 штрафов!", 2000);
                                return;
                            }
                            garage.SpawnCar(number);
                            Notify.Send(player, NotifyType.Alert, NotifyPosition.CenterRight, "Транспорт был вызван!", 2000);
                        }
                        else if (garage.entityVehicles.ContainsKey(number) && !garage.vehiclesOut.ContainsKey(number))
                        {
                            NAPI.Entity.DeleteEntity(garage.entityVehicles[number].Item2);
                            garage.entityVehicles.Remove(number);
                            Notify.Send(player, NotifyType.Alert, NotifyPosition.CenterRight, "Транспорт был убран!", 2000);
                        }
                        else if (!garage.entityVehicles.ContainsKey(number) && garage.vehiclesOut.ContainsKey(number))
                        {
                            foreach (Player ply in NAPI.Pools.GetAllPlayers())
                                if (ply.IsInVehicle && ply.Vehicle == garage.vehiclesOut[number])
                                    return;

                            NAPI.Entity.DeleteEntity(garage.vehiclesOut[number]);
                            garage.vehiclesOut.Remove(number);
                            Notify.Send(player, NotifyType.Alert, NotifyPosition.CenterRight, "Транспорт был убран!", 2000);
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Alert, NotifyPosition.CenterRight, "ERROR!", 2000);
                        }

                    }
                    catch (Exception e) { Log.Write("garageautospawn: " + e.ToString(), nLog.Type.Error); }
                });
            }
            catch (Exception e) { Log.Write("garageauto: " + e.ToString(), nLog.Type.Error); }

        }

        public static Dictionary<int, Garage> Garages = new Dictionary<int, Garage>();

        public static List<Vector3> vehPositions = new List<Vector3>
        {
            new Vector3(223.2661, -978.6877, -99.41358),
            new Vector3(223.1918, -982.4593, -99.41795),
            new Vector3(222.8921, -985.879, -99.41821),
            new Vector3(222.8588, -989.4495, -99.41826),
            new Vector3(223.0551, -993.4521, -99.41066),
            new Vector3(233.6587, -983.3923, -99.41045),
            new Vector3(234.0298, -987.5615, -99.41094),
            new Vector3(234.0298, -991.406, -99.4104),
            new Vector3(234.2386, -995.7032, -99.41273),
            new Vector3(234.3856, -999.8402, -99.41091),
        };

        public static List<Vector3> vehRotations = new List<Vector3>
        {
            new Vector3(-0.03247262, -0.08614436, 251.3986),
            new Vector3(-0.8253403, 0.03646085, 246.0103),
            new Vector3(-0.8608215, 0.004363943, 251.0875),
            new Vector3(-0.8236036, 0.02502611, 248.026),
            new Vector3(-0.1083736, -0.1425103, 240.252),
            new Vector3(-0.1053052, 0.02684846, 130.5622),
            new Vector3(-0.09362753, 0.1056001, 130.4442),
            new Vector3(-0.09778301, 0.03327406, 129.4973),
            new Vector3(-0.05343597, 0.06972831, 129.157),
            new Vector3(-0.08984898, 0.1096697, 128.8663),
        };

        public static Dictionary<int, GarageType> GarageTypes = new Dictionary<int, GarageType>()
        {
            { -1,new GarageType(1) },
            { 0, new GarageType(2) },
            { 1, new GarageType(3) },
            { 2, new GarageType(4) },
            { 3, new GarageType(5) },
            { 4, new GarageType(6) },
            { 5, new GarageType(10)},
            { 6, new GarageType(15)},
            { 7, new GarageType(35)},
            { 8, new GarageType(60)},
            { 9, new GarageType(100)},
        };
        public static int DimensionID = 1000;


        public static void onResourceStart()
        {
            try
            {
                var result = MySQL.QueryRead($"SELECT * FROM `garages`");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {

                    var id = Convert.ToInt32(Row["id"]);
                    var type = Convert.ToInt32(Row["type"]);
                    var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                    var rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());

                    var garage = new Garage(id, type, position, rotation);
                    garage.Dimension = DimensionID;
                    if (garage.Type != -1) garage.CreateInterior();

                    Garages.Add(id, garage);
                    DimensionID++;
                }
                Log.Write($"Loaded {Garages.Count} garages.", nLog.Type.Success);
                HouseManager.onResourceStart();
            }
            catch (Exception e) { Log.Write($"ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static void interactionPressed(Player player, int id, bool apart = false)
        {
            try
            {
                switch (id)
                {
                    case 40:

                        if (!player.HasData("GARAGEID")) return;
                        Garage garage = Garages[player.GetData<int>("GARAGEID")];
                        if (garage == null) return;

                        House house = null;

                        if (!apart)
                        {
                            player.SetData("APART_GARAGE", false);
                            house = Houses.HouseManager.GetHouse(player);
                        }
                        else
                        {
                            player.SetData("APART_GARAGE", true);
                            house = Houses.HouseManager.GetApart(player);
                        }



                        if (house == null || house.GarageID != garage.ID) return;


                        var vehicles = VehicleManager.getAllPlayerVehicles(house.Owner);
                        if (player.IsInVehicle && !vehicles.Contains(player.Vehicle.NumberPlate))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не можете въехать в гараж на этой машине", 3000);
                            return;
                        }
                        else if (player.IsInVehicle && vehicles.Contains(player.Vehicle.NumberPlate))
                        {
                            var vehicle = player.Vehicle;
                            var number = vehicle.NumberPlate;
                            VehicleManager.Vehicles[number].Fuel = (!player.Vehicle.HasSharedData("PETROL")) ? VehicleManager.VehicleTank[player.Vehicle.Class] : player.Vehicle.GetSharedData<int>("PETROL");
                            VehicleManager.Vehicles[number].Items = player.Vehicle.GetData<List<nItem>>("ITEMS");
                            VehicleManager.Vehicles[number].Position = null;
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            NAPI.Task.Run(() => { try { NAPI.Entity.DeleteEntity(vehicle); } catch { } });

                            garage.SendVehicleIntoGarage(number);
                        }

                        if (garage.Type == -1)
                        {
                            if (vehicles.Count == 0) return;
                            if (garage.CheckCar(false, vehicles[0]))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ваша машина сейчас где-то в штате, вы можете эвакуировать её", 3000);
                                return;
                            }
                            if (player.IsInVehicle) return;

                            if (VehicleManager.Vehicles[vehicles[0]].Health < 1)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы должны восстановить машину", 3000);
                                return;
                            }
                            garage.GetVehicleFromGarage(player, vehicles[0]);
                        }
                        else
                        {
                            garage.SendPlayer(player);
                        }
                        return;
                    case 41:
                        if (Main.Players[player].InsideGarageID == -1) return;
                        garage = Garages[Main.Players[player].InsideGarageID];
                        garage.RemovePlayer(player);
                        return;
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"GARAGE_INTERACTION\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static void Event_PlayerDisconnected(Player player)
        {

        }

        #region Commands
        [Command("setgarage")]
        public static void CMD_SetGarage(Player player, int ID)
        {
            if (!Group.CanUseCmd(player, "ban")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны стоять на маркере дома", 3000);
                return;
            }

            House house = HouseManager.Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            if (house == null) return;

            if (!Garages.ContainsKey(ID)) return;
            house.GarageID = ID;
            house.Save();
        }

        [Command("creategarage")]
        public static void CMD_CreateGarage(Player player, int type)
        {
            if (!Group.CanUseCmd(player, "allspawncar")) return;
            if (!GarageTypes.ContainsKey(type)) return;
            int id = 0;
            do
            {
                id++;
            } while (Garages.ContainsKey(id));

            Garage garage = new Garage(id, type, player.Vehicle.Position, player.Vehicle.Rotation);
            garage.Dimension = DimensionID;
            garage.Create();
            if (type != -1) garage.CreateInterior();

            Garages.Add(garage.ID, garage);
            NAPI.Chat.SendChatMessageToPlayer(player, garage.ID.ToString());
        }

        [Command("removegarage")]
        public static void CMD_RemoveGarage(Player player)
        {
            if (!Group.CanUseCmd(player, "allspawncar")) return;
            if (!player.HasData("GARAGEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны стоять на маркере гаража", 3000);
                return;
            }
            if (!GarageManager.Garages.ContainsKey(player.GetData<int>("GARAGEID"))) return;
            Garage garage = GarageManager.Garages[player.GetData<int>("GARAGEID")];

            garage.Destroy();
            Garages.Remove(player.GetData<int>("GARAGEID"));
            MySQL.Query($"DELETE FROM `garages` WHERE `id`='{garage.ID}'");
        }

        #endregion
    }
}
