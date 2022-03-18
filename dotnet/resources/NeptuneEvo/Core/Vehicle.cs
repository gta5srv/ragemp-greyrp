﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using NeptuneEVO.GUI;
using Newtonsoft.Json;
using System.Linq;
using NeptuneEVO.SDK;
using MySql.Data.MySqlClient;
using NeptuneEVO.Fractions;
using System.IO;
using System.Text;

namespace NeptuneEVO.Core
{
    class VehicleManager : Script
    {
        private static nLog Log = new nLog("Vehicle");
        private static Random Rnd = new Random();
        //private static Timer fuelTimer;

        private Vehicle SpawnVeh = NAPI.Vehicle.CreateVehicle(VehicleHash.Dinghy, new Vector3(3370.183, 5186.575, 0.6195515), new Vector3(0.3827768, 2.631065, 261.6981), 1, 1);

        public static SortedDictionary<string, VehicleData> Vehicles = new SortedDictionary<string, VehicleData>();
        public static SortedDictionary<int, int> VehicleTank = new SortedDictionary<int, int>()
        {
            { -1, 100 },
            { 0, 120 }, // compacts
            { 1, 150 }, // Sedans
            { 2, 200 }, // SUVs
            { 3, 100 }, // Coupes
            { 4, 130 }, // Muscle
            { 5, 150 }, // Sports
            { 6, 100 }, // Sports (classic?)
            { 7, 150 }, // Super
            { 8, 100 }, // Motorcycles
            { 9, 200 }, // Off-Road
            { 10, 150 }, // Industrial
            { 11, 150 }, // Utility
            { 12, 150 }, // Vans
            { 13, 1   }, // cycles
            { 14, 300 }, // Boats
            { 15, 400 }, // Helicopters
            { 16, 500 }, // Planes
            { 17, 130 }, // Service
            { 18, 200 }, // Emergency
            { 19, 150 }, // Military
            { 20, 150 }, // Commercial
            // 21 trains
        };

        [ServerEvent(Event.VehicleDamage)]
        public void OnVehicleDamageHandlers(Vehicle vehicle, float body, float engine)
        {
            // Log.Write($"ПРОШЁЛ УРОН корпус: {body} ; движок: {engine}", nLog.Type.Info);
            if (body > 150 || engine > 150)
            {
                vehicle.SetData("TIMEOUT", DateTime.Now.AddSeconds(10));
                Core.VehicleStreaming.SetEngineState(vehicle, false);
            }
        }


        public static SortedDictionary<int, int> VehicleRepairPrice = new SortedDictionary<int, int>()
        {
            { -1, 100 }, // compacts
            { 0, 100 }, // compacts
            { 1, 100 }, // Sedans
            { 2, 100 }, // SUVs
            { 3, 100 }, // Coupes
            { 4, 100 }, // Muscle
            { 5, 100 }, // Sports
            { 6, 100 }, // Sports (classic?)
            { 7, 100 }, // Super
            { 8, 100 }, // Motorcycles
            { 9, 100 }, // Off-Road
            { 10, 100 }, // Industrial
            { 11, 100 }, // Utility
            { 12, 100 }, // Vans
            { 13, 100 }, // 13 cycles
            { 14, 100 }, // Boats
            { 15, 100 }, // Helicopters
            { 16, 100 }, // Planes
            { 17, 100 }, // Service
            { 18, 100 }, // Emergency
            { 19, 100 }, // Military
            { 20, 100 }, // Commercial
            // 21 trains
        };
        private static SortedDictionary<int, int> PetrolRate = new SortedDictionary<int, int>()
        {
            { -1, 0 },
            { 0, 1 }, // compacts
            { 1, 1 }, // Sedans
            { 2, 1 }, // SUVs
            { 3, 1 }, // Coupes
            { 4, 1 }, // Muscle
            { 5, 1 }, // Sports
            { 6, 1 }, // Sports (classic?)
            { 7, 1 }, // Super
            { 8, 1 }, // Motorcycles
            { 9, 1 }, // Off-Road
            { 10, 1 }, // Industrial
            { 11, 1 }, // Utility
            { 12, 1 }, // Vans
            { 13, 0 }, // Cycles
            { 14, 1 }, // Boats
            { 15, 1 }, // Helicopters
            { 16, 1 }, // Planes
            { 17, 1 }, // Service
            { 18, 1 }, // Emergency
            { 19, 1 }, // Military
            { 20, 1 }, // Commercial
            // 21 trains
        };

        public VehicleManager()
        {
            try
            {
                //fuelTimer = Main.StartT(30000, 30000, (o) => FuelControl(), "FUELCONTROL_TIMER");
                Timers.StartTask("fuel", 30000, () => FuelControl());
                Timers.StartTask("mile", 5000, () => Mile());
                Log.Write("Loading Vehicles...");
                DataTable result = MySQL.QueryRead("SELECT * FROM `vehicles`");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result.", nLog.Type.Warn);
                    return;
                }
                int count = 0;
                foreach (DataRow Row in result.Rows)
                {
                    count++;
                    VehicleData data = new VehicleData();
                    data.Holder = Convert.ToString(Row["holder"]);
                    data.Model = Convert.ToString(Row["model"]);
                    data.Health = Convert.ToInt32(Row["health"]);
                    data.Fuel = Convert.ToInt32(Row["fuel"]);
                    data.Price = Convert.ToInt32(Row["price"]);
                    data.Components = JsonConvert.DeserializeObject<VehicleCustomization>(Row["components"].ToString());
                    //if (Row["components"].ToString() == "null") data.Components = new VehicleCustomization();
                    data.Items = JsonConvert.DeserializeObject<List<nItem>>(Row["items"].ToString());
                    if (data.Items == null)
                        data.Items = new List<nItem>();
                    data.Position = Convert.ToString(Row["position"]);
                    data.Rotation = Convert.ToString(Row["rotation"]);
                    data.KeyNum = Convert.ToInt32(Row["keynum"]);
                    data.Sell = Convert.ToInt32(Row["sell"]);
                    data.Dirt = (float)Row["dirt"];
                    data.OnPlast = Convert.ToInt32(Row["onplast"]) == 1;
                    if (string.IsNullOrEmpty(Row["plastic"].ToString()))
                        data.Plastic = DateTime.Now.AddYears(-1000);
                    else
                        data.Plastic = (DateTime)Row["plastic"];



                    Vehicles.Add(Convert.ToString(Row["number"]), data);
                }
                Log.Write($"Vehicles are loaded ({count})", nLog.Type.Success);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.ToString(), nLog.Type.Error); }
        }

        static DateTime Now = DateTime.Now;
        static void Mile()
        {
            NAPI.Task.Run(() => {
                Vehicle localveh = null;
                try
                {
                    
                    foreach(Vehicle veh in NAPI.Pools.GetAllVehicles())
                    {
                        localveh = veh;
                        if (!veh.HasSharedData("MILE") || !veh.EngineStatus) continue;
                        Vector3 velocity = NAPI.Entity.GetEntityVelocity(veh.Handle);
                        double speeds = Math.Sqrt((velocity.X * velocity.X) + (velocity.Y * velocity.Y) + (velocity.Z * velocity.Z)) * 3.6;

                        float trip = (float)((float)speeds * ((DateTime.Now - Now).TotalSeconds / 1000) * 100 / 100);

                        if (Vehicles.ContainsKey(veh.NumberPlate))
                        {
                            Vehicles[veh.NumberPlate].Sell += trip / 5;
                            float distance = Vehicles[veh.NumberPlate].Sell;
                            veh.SetSharedData("MILE", distance);
                            if (distance > 10000)
                            {
                                NAPI.Vehicle.SetVehicleEnginePowerMultiplier(veh, -10);
                                NAPI.Vehicle.SetVehicleEngineTorqueMultiplier(veh, -10);
                            }
                            Save(veh.NumberPlate);
                        }
                        else
                        {
                            float distance = veh.GetSharedData<float>("MILE") + trip / 5;
                        }
                        
                    }
                    Now = DateTime.Now;
                }
                catch (Exception e) { Log.Write($"MILE ({localveh.NumberPlate}): " + e.ToString(), nLog.Type.Error);  }
            });
        }

        public static bool HavePlactic(string number)
        {
            try
            {
                if (!VehicleManager.Vehicles.ContainsKey(number)) return false;
                return !(VehicleManager.Vehicles[number].Plastic < DateTime.Now.AddYears(-100));
            }
            catch { return false; }
        }

        private static void FuelControl()
        {
            NAPI.Task.Run(() =>
            {
                List<Vehicle> allVehicles = NAPI.Pools.GetAllVehicles();
                if (allVehicles.Count == 0) return;
                foreach (Vehicle veh in allVehicles)
                {
                    object f = null;
                    try
                    {
                        if (!veh.HasSharedData("PETROL")) continue;
                        if (!Core.VehicleStreaming.GetEngineState(veh)) continue;

                        /*if(!Int32.TryParse(veh.GetSharedData("PETROL"), out int fuel))
                        {
                            Log.Write($"Bad fuel data detected from car [{veh.NumberPlate}]", nLog.Type.Warn);
                            return;
                        }*/

                        f = veh.GetSharedData<int>("PETROL");
                        int fuel = (int)f;

                        if (fuel == 0) continue;
                        if (fuel - PetrolRate[veh.Class] <= 0)
                        {
                            fuel = 0;
                            Core.VehicleStreaming.SetEngineState(veh, false);
                        }
                        else fuel -= PetrolRate[veh.Class];
                        if (VehicleManager.Vehicles.ContainsKey(veh.NumberPlate))
                        {
                            VehicleManager.Vehicles[veh.NumberPlate].Fuel = fuel;
                            VehicleManager.Save(veh.NumberPlate);
                        }
                        veh.SetSharedData("PETROL", fuel);
                    }
                    catch (Exception e)
                    {
                        Log.Write($"FUELCONTROL_TIMER: {veh.NumberPlate} {f.ToString()}\n{e.Message}", nLog.Type.Error);
                    }
                }
            });
        }

        [RemoteEvent("fpress")]
        public static void Fpress(Player player)
        {
            try {

                if (player.HasSharedData("attachToVehicleTrunk") && player.HasData("VEH"))
                {
                    WarpPlayerOutOfVehicle(player);
                    Main.OnAntiAnim(player);
                    player.PlayAnimation("amb@world_human_bum_slumped@male@laying_on_right_side@base", "base", 35);
                    Trigger.PlayerEventInRange(player.Position, 500, "vehicleattach", player, player.GetData<Vehicle>("VEH"));
                }
            }
            catch { }
        }

        [ServerEvent(Event.PlayerEnterVehicleAttempt)]
        public void onPlayerEnterVehicleAttemptHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (SpawnVeh == vehicle) player.StopAnimation();
            }
            catch { }
        }


        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (SpawnVeh == vehicle) {
                    player.WarpOutOfVehicle();
                    return;
                }
                if (!vehicle.HasData("OCCUPANTS"))
                {
                    List<Player> occupantsList = new List<Player>();
                    occupantsList.Add(player);
                    vehicle.SetData("OCCUPANTS", occupantsList);
                }
                else
                {
                    if (!vehicle.GetData<List<Player>>("OCCUPANTS").Contains(player)) vehicle.GetData<List<Player>>("OCCUPANTS").Add(player);
                }

                if (player.VehicleSeat == 0)
                {
                    if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "FRACTION")
                    {
                        if (NAPI.Data.GetEntityData(vehicle, "FRACTION") == 14 && vehicle.DisplayName == "BARRACKS")
                        {
                            int fracid = Main.Players[player].FractionID;
                            if ((fracid >= 1 && fracid <= 5) || (fracid >= 10 && fracid <= 13) || fracid == 17)
                            {
                                if (DateTime.Now.Hour < 10)
                                {
                                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Невозможно сесть в машину с 00:00 до 10:00", 3000);
                                    return;
                                }
                                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Чтобы завести двигатель, нажмите B", 3000);
                                return;
                            }
                            else if (fracid == 14)
                            {
                                if (Main.Players[player].FractionLVL < NAPI.Data.GetEntityData(vehicle, "MINRANK"))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому транспорту", 3000);
                                    VehicleManager.WarpPlayerOutOfVehicle(player);
                                    return;
                                }
                                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Чтобы завести двигатель, нажмите B", 3000);
                                return;
                            }
                            else
                                VehicleManager.WarpPlayerOutOfVehicle(player);
                        }
                        if (NAPI.Data.GetEntityData(vehicle, "FRACTION") == Main.Players[player].FractionID)
                        {
                            if (Main.Players[player].FractionLVL < NAPI.Data.GetEntityData(vehicle, "MINRANK"))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому транспорту", 3000);
                                VehicleManager.WarpPlayerOutOfVehicle(player);
                                return;
                            }
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Чтобы завести двигатель, нажмите B", 3000);
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к этому транспорту", 3000);
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        }
                    }
                    else if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "WORK" && player.GetData<Vehicle>("WORK") == vehicle)
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Чтобы завести двигатель, нажмите B", 3000);
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicleAttempt)]
        public void onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (VehicleStreaming.GetEngineState(vehicle))
                    VehicleStreaming.SetEngineState(vehicle, true);
                if (!vehicle.HasData("OCCUPANTS"))
                {
                    List<Player> occupantsList = new List<Player>();
                    vehicle.SetData("OCCUPANTS", occupantsList);
                }
                else
                {
                    if (vehicle.GetData<List<Player>>("OCCUPANTS").Contains(player)) vehicle.GetData<List<Player>>("OCCUPANTS").Remove(player);
                }
            }
            catch (Exception e) { Log.Write("PlayerExitVehicleAttempt: " + e.Message, nLog.Type.Error); }
        }

        public static void API_onPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (player.IsInVehicle)
                {
                    Vehicle vehicle = player.Vehicle;
                    if (!vehicle.HasData("OCCUPANTS"))
                    {
                        List<Player> occupantsList = new List<Player>();
                        vehicle.SetData("OCCUPANTS", occupantsList);
                    }
                    else
                    {
                        if (vehicle.GetData<List<Player>>("OCCUPANTS").Contains(player)) vehicle.GetData<List<Player>>("OCCUPANTS").Remove(player);
                    }
                }

                if (player.HasData("WORK_CAR_EXIT_TIMER"))
                    //Main.StopT(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"), "WORK_CAR_EXIT_TIMER_vehicle");
                    Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        public static void WarpPlayerOutOfVehicle(Player player)
        {
            Vehicle vehicle = player.Vehicle;
            if (vehicle == null) return;

            if (!vehicle.HasData("OCCUPANTS"))
            {
                List<Player> occupantsList = new List<Player>();
                vehicle.SetData("OCCUPANTS", occupantsList);
            }
            else
            {
                if (vehicle.GetData<List<Player>>("OCCUPANTS").Contains(player)) vehicle.GetData<List<Player>>("OCCUPANTS").Remove(player);
            }
            Trigger.PlayerEvent(player, "outVeh", 0);
        }

        public static List<Player> GetVehicleOccupants(Vehicle vehicle)
        {
            if (!vehicle.HasData("OCCUPANTS"))
                return new List<Player>();
            else
                return vehicle.GetData<List<Player>>("OCCUPANTS");
        }

        public static void RepairCar(Vehicle vehicle)
        {
            vehicle.Repair();
            VehicleStreaming.UpdateVehicleSyncData(vehicle, new VehicleStreaming.VehicleSyncData());
        }

        public static string Create(string Holder, string Model, Color Color1, Color Color2, Color Color3, int Health = 1000, int Fuel = 100, int Price = 0)
        {
            VehicleData data = new VehicleData();
            data.Holder = Holder;
            data.Model = Model;
            data.Health = Health;
            data.Fuel = Fuel;
            data.Price = Price;
            Color3.Alpha = 0;
            data.Components = new VehicleCustomization();
            data.Components.PrimColor = Color1;
            data.Components.SecColor = Color2;
            data.Components.NeonColor = Color3;
            data.Items = new List<nItem>();
            data.Dirt = 0.0F;
            data.Plastic = DateTime.Now.AddYears(-1000);
            data.OnPlast = false;

            string Number = GenerateNumber();
            Vehicles.Add(Number, data);
            MySQL.Query("INSERT INTO `vehicles`(`number`, `holder`, `model`, `health`, `fuel`, `price`, `components`, `items`,`sell`,`plastic`,`onplast`)" +
                $" VALUES ('{Number}','{Holder}','{Model}',{Health},{Fuel},{Price},'{JsonConvert.SerializeObject(data.Components)}','{JsonConvert.SerializeObject(data.Items)}','0','{MySQL.ConvertTime(data.Plastic)}','0')");
            Log.Write("Created new vehicle with number: " + Number);
            return Number;
        }

        public static void Remove(string Number, Player player = null)
        {
            if (!Vehicles.ContainsKey(Number)) return;
            try
            {
                Houses.House house = Houses.HouseManager.GetHouse(Vehicles[Number].Holder, true);
                if (house != null)
                {
                    Houses.Garage garage = Houses.GarageManager.Garages[house.GarageID];
                    garage.DeleteCar(Number);
                }
            }
            catch { }
            Vehicles.Remove(Number);
            MySQL.Query($"DELETE FROM `vehicles` WHERE number='{Number}'");
        }

        

        public static void Spawn(string Number, Vector3 Pos, float Rot)
        {
            if (!Vehicles.ContainsKey(Number)) return;
            VehicleData data = Vehicles[Number];
            VehicleHash model = NAPI.Util.VehicleNameToModel(data.Model);
            Vehicle veh = NAPI.Vehicle.CreateVehicle(model, Pos, Rot, 0, 0);
            veh.Health = data.Health;
            veh.NumberPlate = Number;
            ApplyCustomization(veh);
        }
        public static bool Save(string Number)
        {
            if (!Vehicles.ContainsKey(Number)) return false;
            VehicleData data = Vehicles[Number];
            string items = JsonConvert.SerializeObject(data.Items);
            if (string.IsNullOrEmpty(items) || items == null) items = "[]";

            string pos = data.Position;
            if (pos == null || string.IsNullOrEmpty(pos))
                pos = "{}";

            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "UPDATE `vehicles` SET holder=@hold, model=@model, health=@hp, fuel=@fuel, components=@comp, items=@it,position=@pos,rotation=@rot,keynum=@keyn,dirt=@dirt,sell=@sell,plastic=@plastic,onplast=@onplast WHERE number=@numb";
            cmd.Parameters.AddWithValue("@hold", data.Holder);
            cmd.Parameters.AddWithValue("@model", data.Model);
            cmd.Parameters.AddWithValue("@hp", data.Health);
            cmd.Parameters.AddWithValue("@fuel", data.Fuel);
            cmd.Parameters.AddWithValue("@comp", JsonConvert.SerializeObject(data.Components));
            cmd.Parameters.AddWithValue("@it", items);
            cmd.Parameters.AddWithValue("@pos", data.Position);
            cmd.Parameters.AddWithValue("@rot", data.Rotation);
            cmd.Parameters.AddWithValue("@keyn", data.KeyNum);
            cmd.Parameters.AddWithValue("@sell", data.Sell);
            cmd.Parameters.AddWithValue("@plastic", data.Plastic);
            cmd.Parameters.AddWithValue("@onplast", Convert.ToInt32( data.OnPlast));
            cmd.Parameters.AddWithValue("@dirt", (byte)data.Dirt);
            cmd.Parameters.AddWithValue("@numb", Number);

            MySQL.Query(cmd);

            return true;
        }
        public static bool isHaveAccess(Player Player, Vehicle Vehicle)
        {
            if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "WORK")
            {
                if (NAPI.Data.GetEntityData(Player, "WORK") != Vehicle)
                    return false;
                else
                    return true;
            }
            else if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "FRACTION")
            {
                if (Main.Players[Player].FractionID != NAPI.Data.GetEntityData(Vehicle, "FRACTION"))
                    return false;
                else
                    return true;
            }
            else if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "PERSONAL")
            {
                bool access = canAccessByNumber(Player, Vehicle.NumberPlate);
                if (access)
                    return true;
                else
                    return false;
            }
            else if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "GARAGE")
            {
                bool access = canAccessByNumber(Player, Vehicle.NumberPlate);
                if (access)
                    return true;
                else
                    return false;
            }
            else if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "HOTEL")
            {
                if (Player.HasData("HOTELCAR") && NAPI.Data.GetEntityData(Player, "HOTELCAR") == Vehicle)
                {
                    return true;
                }
                else
                    return false;
            }
            else if (NAPI.Data.GetEntityData(Vehicle, "ACCESS") == "RENT")
            {
                if (NAPI.Data.GetEntityData(Vehicle, "DRIVER") == Player)
                {
                    return true;
                }
                else
                    return false;
            }
            return true;
        }
        public static Vehicle getNearestVehicle(Player player, int radius)
        {
            List<Vehicle> all_vehicles = NAPI.Pools.GetAllVehicles();
            Vehicle nearest_vehicle = null;
            foreach (Vehicle v in all_vehicles)
            {
                if (v.Dimension != player.Dimension) continue;
                if (nearest_vehicle == null && player.Position.DistanceTo(v.Position) < radius)
                {
                    nearest_vehicle = v;
                    continue;
                }
                else if(nearest_vehicle != null) {
                    if (player.Position.DistanceTo(v.Position) < player.Position.DistanceTo(nearest_vehicle.Position))
                    {
                        nearest_vehicle = v;
                        continue;
                    }
                }
            }
            return nearest_vehicle;
        }
        public static List<string> getAllPlayerVehicles(string playername)
        {
            List<string> all_number = new List<string>();
            foreach (KeyValuePair<string, VehicleData> accVehicle in Vehicles)
                if (accVehicle.Value.Holder == playername)
                {
                    all_number.Add(accVehicle.Key);
                }
            return all_number;
        }

        public static void sellCar(Player player, Player target)
        {
            player.SetData("SELLCARFOR", target);
            OpenSellCarMenu(player);
        }



        [RemoteEvent("savemytun")]

        public static void SaveMyTun(Player player, string SpoilersT, string FrontBumpersT, string RearsBumpersT, string RoofsT, string WingsT, string LatticesT, string HoodsT, string SideSkirtsT, string MufflersT)
        {
            try
            {



                StreamWriter config = new StreamWriter("tuning.txt", true, Encoding.UTF8);
                StreamWriter js = new StreamWriter("tuningjs.txt", true, Encoding.UTF8);
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                config.Write("{\"" + player.GetData<string>("VEHNAME") + "\", new Dictionary<int, List<Tuple<int, string, int>>>() {\r\n");
                js.Write("\"" + player.GetData<string>("VEHNAME") + "\":{\r\n");

                List<int> Spoilers = JsonConvert.DeserializeObject<List<int>>(SpoilersT);
                List<int> FrontBumpers = JsonConvert.DeserializeObject<List<int>>(FrontBumpersT);
                List<int> RearsBumpers = JsonConvert.DeserializeObject<List<int>>(RearsBumpersT);
                List<int> Roofs = JsonConvert.DeserializeObject<List<int>>(RoofsT);
                List<int> Wings = JsonConvert.DeserializeObject<List<int>>(WingsT);
                List<int> Lattices = JsonConvert.DeserializeObject<List<int>>(LatticesT);
                List<int> Hoods = JsonConvert.DeserializeObject<List<int>>(HoodsT);
                List<int> SideSkirts = JsonConvert.DeserializeObject<List<int>>(SideSkirtsT);
                List<int> Mufflers = JsonConvert.DeserializeObject<List<int>>(MufflersT);

                /*
                veh.SetMod(4, data.Muffler);
                    veh.SetMod(3, data.SideSkirt);
                    veh.SetMod(7, data.Hood);
                    veh.SetMod(0, data.Spoiler);
                    veh.SetMod(6, data.Lattice);
                    veh.SetMod(8, data.Wings);
                    veh.SetMod(10, data.Roof);
                    veh.SetMod(48, data.Vinyls);
                    veh.SetMod(1, data.FrontBumper);
                    veh.SetMod(2, data.RearBumper);*/

                List<List<int>> Lists = new List<List<int>> { Spoilers, FrontBumpers, RearsBumpers, Roofs, Wings, Lattices, Hoods, SideSkirts, Mufflers };

                int last = -1;

                List<int> Numbers = new List<int> { 3, 8, 9, 6, 5, 4, 2, 1, 0 };

                List<string> Names = new List<string> { "Стандартный спойлер", "Стандартный пер. бампер", "Стандартный зад. бампер", "Стандартная крыша", "Стандартное крыло", "Стандартная решётка", "Стандартный капот", "Стандартные пороги", "Стандартный глушитель" };
                List<string> SNames = new List<string> { "Улучшенный спойлер", "Улучшенный пер. бампер", "Улучшенный зад. бампер", "Улучшенная крыша", "Улучшенное крыло", "Улучшенная решётка", "Улучшенный капот", "Улучшенные пороги", "Улучшенный глушитель" };

                for (int i = 0; i < 9; i++)
                {
                    Log.Write(i.ToString());
                    if (Lists[i].Count == 0)
                        continue;
                    else
                        last = i;
                }

                for (int i = 0; i < 9; i++)
                {
                    if (Lists[i].Count > 0)
                    {
                        config.Write("      {" + Numbers[i] + ",new List<Tuple<int, string, int>>() {\r\n");
                        js.Write("      \"" + Numbers[i] + "\":[\r\n");

                        for (int f = -1; f < Lists[i].Count; f++)
                        {

                            if (f == -1)
                            {
                                config.Write($"             new Tuple<int, string, int>(-1, \"{Names[i]}\", 600000),\r\n");
                                js.Write("             {\"Item1\":-1,\"Item2\":\"" + Names[i] + "\",\"Item3\":600000},\r\n");
                            }
                            else
                            {
                                string vavada = ",";
                                if (f + 1 == Lists[i].Count)
                                    vavada = "";

                                

                                config.Write($"             new Tuple<int, string, int>({Lists[i][f]}, \"{SNames[i]} {f}\", 600000){vavada}\r\n");
                                js.Write("             {\"Item1\":" + Lists[i][f] + ",\"Item2\":\"" + SNames[i] + " " + f.ToString() + "\",\"Item3\":600000}" + vavada + "\r\n");
                            }

                            
                        }

                        if (last == i)
                        {
                            config.Write("      }}   \r\n");
                            js.Write("      ]   \r\n");
                        }
                        else
                        {
                            config.Write("      }},   \r\n");
                            js.Write("      ],   \r\n");
                        }

                    }
                }

                config.Write("}},\r\n");
                js.Write("},\r\n");

                config.Close();
                js.Close();
            }
            catch (Exception e) { Log.Write("SAVE TUN: " + e.ToString(), nLog.Type.Error); }

        }

        #region Selling Menu
        public static void OpenSellCarMenu(Player player)
        {
            Menu menu = new Menu("sellcar", false, true);
            menu.Callback = callback_sellcar;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Продажа машины";
            menu.Add(menuItem);

            foreach (string number in getAllPlayerVehicles(player.Name))
            {
                menuItem = new Menu.Item(number, Menu.MenuItem.Button);
                menuItem.Text = ParkManager.GetNormalName(Vehicles[number].Model) + " - " + number;
                menu.Add(menuItem);
            }

            foreach(string number in AirVehicles.getAllAirVehicles(player.Name).Keys)
            {
                menuItem = new Menu.Item(number, Menu.MenuItem.Button);
                menuItem.Text = ParkManager.GetNormalName(AirVehicles.Airs[number].Model) + " - " + number;
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }

        private static void callback_sellcar(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            if (item.ID == "close") return;
            if (AirVehicles.Airs.ContainsKey(item.ID) && AirVehicles.Airs[item.ID].Holder == player.Name)
            {
                player.SetData("SELLAIRNUMBER", item.ID);
                Trigger.PlayerEvent(player, "openInput", "Продать воздушный транспорт", "Введите цену", 10, "sellair");
                return;
            }
            player.SetData("SELLCARNUMBER", item.ID);
            Trigger.PlayerEvent(player, "openInput", "Продать машину", "Введите цену", 10, "sellcar");
        }
        #endregion

        public static void FracApplyCustomization(Vehicle veh, int fraction)
        {
            try
            {
                if(veh != null) {
                    if (!Configs.FractionVehicles[fraction].ContainsKey(veh.NumberPlate)) return;

                    VehicleCustomization data = Configs.FractionVehicles[fraction][veh.NumberPlate].Item7;

                    if (data.NeonColor.Alpha != 0)
                    {
                        NAPI.Vehicle.SetVehicleNeonState(veh, true);
                        NAPI.Vehicle.SetVehicleNeonColor(veh, data.NeonColor.Red, data.NeonColor.Green, data.NeonColor.Blue);
                    }

                    veh.SetMod(4, data.Muffler);
                    veh.SetMod(3, data.SideSkirt);
                    veh.SetMod(7, data.Hood);
                    veh.SetMod(0, data.Spoiler);
                    veh.SetMod(6, data.Lattice);
                    veh.SetMod(8, data.Wings);
                    veh.SetMod(10, data.Roof);
                    veh.SetMod(48, data.Vinyls);
                    veh.SetMod(1, data.FrontBumper);
                    veh.SetMod(2, data.RearBumper);

                    veh.SetMod(11, data.Engine);
                    veh.SetMod(18, data.Turbo);
                    veh.SetMod(13, data.Transmission);
                    veh.SetMod(15, data.Suspension);
                    veh.SetMod(12, data.Brakes);
                    veh.SetMod(14, data.Horn);
					veh.SetMod(16, data.Armor);

                    veh.WindowTint = data.WindowTint;
                    veh.NumberPlateStyle = data.NumberPlate;

                    if(data.Headlights >= 0) {
                        veh.SetMod(22, 0);
                        veh.SetSharedData("hlcolor", data.Headlights);
                        Trigger.PlayerEventInRange(veh.Position, 250f, "VehStream_SetVehicleHeadLightColor", veh.Handle, data.Headlights);
                    } else {
                        veh.SetMod(22, -1);
                        veh.SetSharedData("hlcolor", 0);
                    }

                    veh.WheelType = data.WheelsType;
                    veh.SetMod(23, data.Wheels);
                }
            }
            catch (Exception e) { Log.Write("ApplyCustomization: " + e.Message, nLog.Type.Error); }
        }

        public static void ResetColors(Vehicle veh)
        {
            NAPI.Vehicle.SetVehiclePrimaryColor(veh, 0);
            NAPI.Vehicle.SetVehicleSecondaryColor(veh, 0);
            NAPI.Vehicle.SetVehiclePrimaryPaint(veh, 0, 0);
            NAPI.Vehicle.SetVehicleSecondaryPaint(veh, 0, 0);
            NAPI.Vehicle.SetVehicleCustomPrimaryColor(veh, 0, 0, 0);
            NAPI.Vehicle.SetVehicleCustomSecondaryColor(veh, 0, 0, 0);
        }

        public static void ApplyCustomization(Vehicle veh)
        {
            try
            {
                if(veh != null) {
                    if (!Vehicles.ContainsKey(veh.NumberPlate)) return;

                    VehicleCustomization data = Vehicles[veh.NumberPlate].Components;

                    if (data.NeonColor.Alpha != 0)
                    {
                        NAPI.Vehicle.SetVehicleNeonState(veh, true);
                        NAPI.Vehicle.SetVehicleNeonColor(veh, data.NeonColor.Red, data.NeonColor.Green, data.NeonColor.Blue);
                    }

                    //ResetColors(veh);

                    veh.SetMod(4, data.Muffler);
                    veh.SetMod(3, data.SideSkirt);
                    veh.SetMod(7, data.Hood);
                    veh.SetMod(0, data.Spoiler);
                    veh.SetMod(6, data.Lattice);
                    veh.SetMod(8, data.Wings);
                    veh.SetMod(10, data.Roof);
                    veh.SetMod(48, data.Vinyls);
                    veh.SetMod(1, data.FrontBumper);
                    veh.SetMod(2, data.RearBumper);

                    veh.SetMod(11, data.Engine);
                    veh.SetMod(18, data.Turbo);
                    veh.SetMod(13, data.Transmission);
                    veh.SetMod(15, data.Suspension);
                    veh.SetMod(12, data.Brakes);
                    veh.SetMod(14, data.Horn);
					veh.SetMod(16, data.Armor);
					

                    veh.WindowTint = data.WindowTint;
                    veh.NumberPlateStyle = data.NumberPlate;

                    if(data.Headlights >= 0) {
                        veh.SetMod(22, 0);
                        veh.SetSharedData("hlcolor", data.Headlights);
                        Trigger.PlayerEventInRange(veh.Position, 250f, "VehStream_SetVehicleHeadLightColor", veh.Handle, data.Headlights);
                    } else {
                        veh.SetMod(22, -1);
                        veh.SetSharedData("hlcolor", 0);
                    }


                    if (data.PrimModColor != -1)
                    {
                        NAPI.Vehicle.SetVehiclePrimaryColor(veh, data.PrimModColor);
                        NAPI.Vehicle.SetVehicleSecondaryColor(veh, data.SecModColor);

                        VehicleStreaming.SetVehicleColors(veh, data.PrimModColor, data.SecModColor);
                    }



                    //NAPI.Vehicle.SetVehicleCustomPrimaryColor(veh, data.PrimColor.Red, data.PrimColor.Green, data.PrimColor.Blue);



                    NAPI.Vehicle.SetVehicleCustomPrimaryColor(veh, data.PrimColor.Red, data.PrimColor.Green, data.PrimColor.Blue);
                    NAPI.Vehicle.SetVehicleCustomSecondaryColor(veh, data.SecColor.Red, data.SecColor.Green, data.SecColor.Blue);

                    


                    veh.WheelType = data.WheelsType;
                    veh.SetMod(23, data.Wheels);

                    VehicleStreaming.SetVehicleDirt(veh, Vehicles[veh.NumberPlate].Dirt);
                }
            }
            catch (Exception e) { Log.Write("ApplyCustomization: " + e.Message, nLog.Type.Error); }
        }
        
        public static void ChangeVehicleDoors(Player player, Vehicle vehicle)
        {
            switch (NAPI.Data.GetEntityData(vehicle, "ACCESS"))
            {
                case "HOTEL":
                    if (NAPI.Data.GetEntityData(vehicle, "OWNER") != player && Main.Players[player].AdminLVL < 3)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого транспорта", 3000);
                        return;
                    }
                    if (Core.VehicleStreaming.GetLockState(vehicle))
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, false);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы открыли двери машины", 3000);
                    }
                    else
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, true);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закрыли двери машины", 3000);
                    }
                    break;
                case "RENT":
                    if (NAPI.Data.GetEntityData(vehicle, "DRIVER") != player && Main.Players[player].AdminLVL < 3)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого транспорта", 3000);
                        return;
                    }
                    if (Core.VehicleStreaming.GetLockState(vehicle))
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, false);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы открыли двери машины", 3000);
                    }
                    else
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, true);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закрыли двери машины", 3000);
                    }
                    break;
                case "WORK":
                    if (NAPI.Data.GetEntityData(player, "WORK") != vehicle && Main.Players[player].AdminLVL < 3)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого транспорта", 3000);
                        return;
                    }
                    if (Core.VehicleStreaming.GetLockState(vehicle))
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, false);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы открыли двери машины", 3000);
                    }
                    else
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, true);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закрыли двери машины", 3000);
                    }
                    break;
                case "AIR":

                    bool access = canAccessByNumberAIR(player, vehicle.NumberPlate);
                    if (!access)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого транспорта", 3000);
                        return;
                    }

                    if (Core.VehicleStreaming.GetLockState(vehicle))
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, false);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы открыли двери воздушного транспорта", 3000);
                        return;
                    }
                    else
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, true);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закрыли двери воздушного транспорта", 3000);
                        return;
                    }
                case "PERSONAL":

                    access = canAccessByNumber(player, vehicle.NumberPlate);
                    if (!access)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого транспорта", 3000);
                        return;
                    }

                    if (Core.VehicleStreaming.GetLockState(vehicle))
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, false);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы открыли двери машины", 3000);
                        return;
                    }
                    else
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, true);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закрыли двери машины", 3000);
                        return;
                    }
                case "GARAGE":

                    access = canAccessByNumber(player, vehicle.NumberPlate);
                    if (!access && Main.Players[player].AdminLVL < 3)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого транспорта", 3000);
                        return;
                    }

                    if (Core.VehicleStreaming.GetLockState(vehicle))
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, false);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы открыли двери машины", 3000);
                        return;
                    }
                    else
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, true);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закрыли двери машины", 3000);
                        return;
                    }
                case "GARAGEORG":
                    access = canAccessByNumber(player, vehicle.NumberPlate);
                    if (!access && Main.Players[player].AdminLVL < 3)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого транспорта", 3000);
                        return;
                    }

                    if (Core.VehicleStreaming.GetLockState(vehicle))
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, false);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы открыли двери машины", 3000);
                        return;
                    }
                    else
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, true);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закрыли двери машины", 3000);
                        return;
                    }
                case "ADMIN":
                    if (Main.Players[player].AdminLVL == 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого транспорта", 3000);
                        return;
                    }

                    if (Core.VehicleStreaming.GetLockState(vehicle))
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, false);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы открыли двери машины", 3000);
                        return;
                    }
                    else
                    {
                        Core.VehicleStreaming.SetLockStatus(vehicle, true);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закрыли двери машины", 3000);
                        return;
                    }
                default:
                    return;
            }
            Trigger.PlayerEvent(player, "updlock");
            return;
        }

        public static bool canAccessByNumberAIR(Player player, string number)
        {
            List<nItem> items = nInventory.Items[Main.Players[player].UUID];
            string needData = $"{number}_{number}";
            bool access = (items.FindIndex(i => i.Type == ItemType.CarKey && i.Data == needData) != -1);

            if (!access)
            {
                int index = items.FindIndex(i => i.Type == ItemType.KeyRing && new List<string>(Convert.ToString(i.Data).Split('/')).Contains(needData));
                if (index != -1) access = true;
            }

            return access;
        }

        public static bool canAccessByNumber(Player player, string number)
        {
            if (Main.Players[player].FamilyCID != null)
            {
                if (Main.PlayerUUIDs.ContainsKey(VehicleManager.Vehicles[number].Holder))
                {
                    Golemo.Families.Family fam = Golemo.Families.Manager.Families.Find(x => x.Leader == Main.PlayerUUIDs[VehicleManager.Vehicles[number].Holder]);
                    if (fam != null && fam.FamilyCID == Main.Players[player].FamilyCID)
                        return true;
                }
            }

            List<nItem> items = nInventory.Items[Main.Players[player].UUID];
            string needData = $"{number}_{Vehicles[number].KeyNum}";
            bool access = (items.FindIndex(i => i.Type == ItemType.CarKey && i.Data == needData) != -1);

            if (!access)
            {
                int index = items.FindIndex(i => i.Type == ItemType.KeyRing && new List<string>(Convert.ToString(i.Data).Split('/')).Contains(needData));
                if (index != -1) access = true;
            }

            return access;
        }

        // ///// need refactoring //// //
        public static void onPlayerEvent(Player sender, string eventName, params object[] args)
        {
            switch (eventName)
            {

                case "engineCarPressed":
                    #region Engine button
                    if (!NAPI.Player.IsPlayerInAnyVehicle(sender)) return;
                    if (sender.VehicleSeat != 0)
                    {
                        Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны быть в водительском месте", 3000);
                        return;
                    }
                    Vehicle vehicle = sender.Vehicle;
                    if (vehicle.Class == 13 && Main.Players[sender].InsideGarageID == -1) return;
                    if (!vehicle.HasSharedData("PETROL"))
                        VehicleStreaming.SetEngineState(vehicle, true);
                    
                    if (vehicle.HasSharedData("PETROL") && vehicle.GetSharedData<int>("PETROL") <= 0)
                    {
                        Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Топливный бак пуст, невозможно завести машину", 3000);
                        return;
                    }
					if (DateTime.Now < vehicle.GetData<DateTime>("TIMEOUT"))
						{
							 Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Ваша машина заглохла", 3000);
							 return;
						}
						vehicle.ResetData("TIMEOUT");

                    switch (NAPI.Data.GetEntityData(vehicle, "ACCESS"))
                    {
                        case "HOTEL":
                            if (NAPI.Data.GetEntityData(vehicle, "OWNER") != sender && Main.Players[sender].AdminLVL < 3)
                            {
                                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого транспорта", 3000);
                                return;
                            }
                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы заглушили двигатель машины", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы завели машину", 3000);
                            }
                            break;
                        case "SCHOOL":
                            if (NAPI.Data.GetEntityData(vehicle, "DRIVER") != sender && Main.Players[sender].AdminLVL < 3)
                            {
                                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого транспорта", 3000);
                                return;
                            }
                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы заглушили двигатель машины", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы завели машину", 3000);
                            }
                            break;
                        case "RENT":
                            if (NAPI.Data.GetEntityData(vehicle, "DRIVER") != sender && Main.Players[sender].AdminLVL < 3)
                            {
                                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого транспорта", 3000);
                                return;
                            }
                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы заглушили двигатель машины", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы завели машину", 3000);
                            }
                            break;
                        case "WORK":
                            if (NAPI.Data.GetEntityData(sender, "WORK") != vehicle && Main.Players[sender].AdminLVL < 3)
                            {
                                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого транспорта", 3000);
                                return;
                            }
                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы заглушили двигатель машины", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы завели машину", 3000);
                            }
                            break;
                        case "FRACTION":
                            if (Main.Players[sender].FractionID != NAPI.Data.GetEntityData(vehicle, "FRACTION"))
                            {
                                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого транспорта", 3000);
                                return;
                            }
                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы заглушили двигатель машины", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы завели машину", 3000);
                            }
                            break;
                        case "AIR":

                            bool access = canAccessByNumberAIR(sender, vehicle.NumberPlate);
                            if (!access && Main.Players[sender].AdminLVL < 3)
                            {
                                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого воздушного транспорта", 3000);
                                return;
                            }

                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы заглушили двигатель воздушного транспорта", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы завели двигатели воздушного транспорта", 3000);
                            }
                            break;
                        case "PERSONAL":

                             access = canAccessByNumber(sender, vehicle.NumberPlate);
                            if (!access && Main.Players[sender].AdminLVL < 3)
                            {
                                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого транспорта", 3000);
                                return;
                            }

                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы заглушили двигатель машины", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы завели машину", 3000);
                            }
                            break;
                        case "GARAGE":
                            if (Main.Players[sender].InsideGarageID == -1) return;
                            string number = NAPI.Vehicle.GetVehicleNumberPlate(vehicle);

                            Houses.Garage garage = Houses.GarageManager.Garages[Main.Players[sender].InsideGarageID];
                            if (garage == null) return;
                            garage.RemovePlayer(sender);

                            garage.GetVehicleFromGarage(sender, number);
                            break;
                        case "GARAGEORG":
                            if (!vehicle.HasData("PARKCL")) return;
                            var cals = vehicle.GetData<Organization.MaterialsI.MaterialsOrg>("PARKCL");
                            cals.OutGarage(sender);
                            break;
                        case "QUEST":
                        case "MAFIADELIVERY":
                        case "GANGDELIVERY":
                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы заглушили двигатель машины", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы завели машину", 3000);
                            }
                            break;
                        case "ADMIN":
                            if (Main.Players[sender].AdminLVL == 0)
                            {
                                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого транспорта", 3000);
                                return;
                            }
                            if (Core.VehicleStreaming.GetEngineState(vehicle))
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, false);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы заглушили двигатель машины", 3000);
                            }
                            else
                            {
                                Core.VehicleStreaming.SetEngineState(vehicle, true);
                                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы завели машину", 3000);
                            }
                            break;
                    }
                    Trigger.PlayerEvent(sender, "updengine", VehicleStreaming.GetEngineState(vehicle));
                    if (Core.VehicleStreaming.GetEngineState(vehicle)) Commands.RPChat("me", sender, "завел(а) транспортное средство");
                    else Commands.RPChat("me", sender, "заглушил(а) транспортное средство");
                    
                    return;
                #endregion Engine button
                case "lockCarPressed":
                    #region inVehicle
                    if (NAPI.Player.IsPlayerInAnyVehicle(sender) && sender.VehicleSeat == 0)
                    {
                        vehicle = sender.Vehicle;
                        ChangeVehicleDoors(sender, vehicle);
                        Trigger.PlayerEvent(sender, "updlock");
                        return;
                    }
                    #endregion
                    #region outVehicle
                    vehicle = getNearestVehicle(sender, 10);
                    if (vehicle != null)
                        ChangeVehicleDoors(sender, vehicle);
                    #endregion
                    break;
            }
        }
        // ////////////////////////// //

        [ServerEvent(Event.VehicleDeath)]
        public void Event_vehicleDeath(Vehicle vehicle)
        {
            try
            {
                if (!vehicle.HasData("ACCESS") || vehicle.GetData<string>("ACCESS") == "ADMIN") return;
                string access = vehicle.GetData<string>("ACCESS");
                if (vehicle.HasData("TRUNK"))
                {
                    Player player = vehicle.GetData<Player>("TRUNK");
                    vehicle.ResetData("TRUNK");
                    player.ResetSharedData("attachToVehicleTrunk");
                    Trigger.PlayerEventInRange(player.Position, 500, "vehicledeattach", player);
                    Main.OffAntiAnim(player);
                    player.StopAnimation();
                    player.ResetData("VEH");
                    return;
                }
                switch (access)
                {
                    case "PERSONAL":
                        {
                            Player owner = vehicle.GetData<Player>("OWNER");
                            string number = vehicle.NumberPlate;
                            
                            Notify.Send(owner, NotifyType.Alert, NotifyPosition.BottomCenter, "Ваша машина уничтожена", 3000);

                            VehicleData vData = Vehicles[number];
                            vData.Items = new List<nItem>();
                            vData.Health = 0;

                            vehicle.Delete();
                        }
                        return;
                    case "AIR":
                        {
                            Player owner = vehicle.GetData<Player>("OWNER");
                            string number = vehicle.NumberPlate;

                            Notify.Send(owner, NotifyType.Alert, NotifyPosition.BottomCenter, "Ваш воздушный транспорт уничтожен", 3000);

                            owner.ResetData("SPAWNAIR");

                            vehicle.Delete();
                        }
                        return;
                    case "WORK":
                        Player player = vehicle.GetData<Player>("DRIVER");
                        if (player != null)
                        {
                            string paymentMsg = (player.GetData<int>("PAYMENT") == 0) ? "" : $"Вы получили зарплату в {player.GetData<int>("PAYMENT")}$";
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Ваш рабочий транспорт был уничтожен. " + paymentMsg, 3000);
                            player.SetData("ON_WORK", false);
                            Customization.ApplyCharacter(player);
                        }
                        string work = vehicle.GetData<string>("TYPE");
                        switch (work)
                        {
                            case "TRUCKER":
                                if (player != null) Jobs.Truckers.cancelOrder(player);
                                return;
                        }
                        return;
                }
            }
            catch (Exception e) { Log.Write("VehicleDeath: " + e.Message, nLog.Type.Error); }
        }

        private static List<string> whitelist = new List<string> 
        {
          "A","B","E","K","M","H","O","P","C","T","Y","X"
        };

        public static string GenerateNumber()
        {
            string number;
            Random rnd = new Random(); 
            do
            {
                number = "";

                number += whitelist[rnd.Next(0, whitelist.Count)];
                number += whitelist[rnd.Next(0, whitelist.Count)];
                number += whitelist[rnd.Next(0, whitelist.Count)];
                number += rnd.Next(0,9);
				number += rnd.Next(0,9);
				number += rnd.Next(0,9);
				number += whitelist[rnd.Next(0, whitelist.Count)];
				number += whitelist[rnd.Next(0, whitelist.Count)];
                // AAA111AA

            } while (Vehicles.ContainsKey(number) && AirVehicles.Airs.ContainsKey(number));
            return number;
        }
		

        internal class VehicleData
        {
            public string Holder { get; set; }
            public string Model { get; set; }
            public int Health { get; set; }
            public int Fuel { get; set; }
            public int Price { get; set; }
            public VehicleCustomization Components { get; set; }
            public List<nItem> Items { get; set; }
            public string Position { get; set; }
            public string Rotation { get; set; }
            public int KeyNum { get; set; }
            public float Dirt { get; set; }
			public float Sell { get; set; }
            public DateTime Plastic { get; set; }
            public bool OnPlast { get; set; }
        }

        internal class VehicleCustomization
        {
            public Color PrimColor = new Color(0, 0, 0);
            public Color SecColor = new Color(0, 0, 0);
            public Color NeonColor = new Color(0, 0, 0, 0);

            public int PrimModColor = -1;
            public int SecModColor = -1;

            public int Muffler = -1;
            public int SideSkirt = -1;
            public int Hood = -1;
            public int Spoiler = -1;
            public int Lattice = -1;
            public int Wings = -1;
            public int Roof = -1;
            public int Vinyls = -1;
            public int FrontBumper = -1;
            public int RearBumper = -1;

            public int Engine = -1;
            public int Turbo = -1;
            public int Horn = -1;
            public int Transmission = -1;
            public int WindowTint = 0;
            public int Suspension = -1;
            public int Brakes = -1;
            public int Headlights = -1;
            //public int HeadlightColor = 0;
            public int NumberPlate = 0;

            public int Wheels = -1;
            public int WheelsType = 0;
            public int WheelsColor = 0;


            public int Armor = -1;
        }

        public static void changeOwner(string oldName, string newName)
        {
            List<string> toChange = new List<string>();

            lock (AirVehicles.Airs)
            {
                foreach (KeyValuePair<string, AirVehicle> vd in AirVehicles.Airs)
                {
                    if (vd.Value.Holder != oldName) continue;
                    Log.Write($"The car was found! [{vd.Key}]");
                    toChange.Add(vd.Key);
                }
                foreach (string num in toChange)
                {
                    if (AirVehicles.Airs.ContainsKey(num)) AirVehicles.Airs[num].Holder = newName;
                }
                // // //
                MySQL.Query($"UPDATE `airvehicles` SET `holder`='{newName}' WHERE `holder`='{oldName}'");
            }

            toChange = new List<string>();

            lock (Vehicles)
            {
                foreach(KeyValuePair<string, VehicleData> vd in Vehicles)
                {
                    if (vd.Value.Holder != oldName) continue;
                    Log.Write($"The car was found! [{vd.Key}]");
                    toChange.Add(vd.Key);
                }
                foreach (string num in toChange) {
                    if(Vehicles.ContainsKey(num)) Vehicles[num].Holder = newName;
                }
                // // //
                MySQL.Query($"UPDATE `vehicles` SET `holder`='{newName}' WHERE `holder`='{oldName}'");
            }
        }
    }

    class VehicleInventory : Script
    {
        public static void Add(Vehicle vehicle, nItem item)
        {
            if (!vehicle.HasData("ITEMS")) return;
            List<nItem> items = vehicle.GetData<List<nItem>>("ITEMS");

            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type) 
                || nInventory.MeleeWeaponsItems.Contains(item.Type) || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing)
            {
                items.Add(item);
            }
            else
            {
                int count = item.Count;
                for (int i = 0; i < items.Count; i++)
                {
                    if (i >= items.Count) break;
                    if (items[i].Type == item.Type && items[i].Count < nInventory.ItemsStacks[item.Type])
                    {
                        int temp = nInventory.ItemsStacks[item.Type] - items[i].Count;
                        if (count < temp) temp = count;
                        items[i].Count += temp;
                        count -= temp;
                    }
                }

                while (count > 0)
                {
                    if (count >= nInventory.ItemsStacks[item.Type])
                    {
                        items.Add(new nItem(item.Type, nInventory.ItemsStacks[item.Type], item.Data));
                        count -= nInventory.ItemsStacks[item.Type];
                    }
                    else
                    {
                        items.Add(new nItem(item.Type, count, item.Data));
                        count = 0;
                    }
                }
            }

            vehicle.SetData("ITEMS", items);

            if (vehicle.GetData<string>("ACCESS") == "PERSONAL" || vehicle.GetData<string>("ACCESS") == "GARAGE")
                VehicleManager.Vehicles[vehicle.NumberPlate].Items = items;

            foreach (Player p in NAPI.Pools.GetAllPlayers())
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                if (p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 2 && p.HasData("SELECTEDVEH") && p.GetData<Vehicle>("SELECTEDVEH") == vehicle) GUI.Dashboard.OpenOut(p, vehicle.GetData<List<nItem>>("ITEMS"), "Багажник", 2);
            }
        }

        public static int TryAdd(Vehicle vehicle, nItem item)
        {
            if (!vehicle.HasData("ITEMS")) return -1;
            List<nItem> items = vehicle.GetData<List<nItem>>("ITEMS");

            int tail = 0;
            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type) || 
                item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing)
            {
                if (items.Count >= 25) return -1;
            }
            else
            {
                int count = 0;
                foreach (nItem i in items)
                    if (i.Type == item.Type) count += nInventory.ItemsStacks[i.Type] - i.Count;

                int slots = 25;
                int maxCapacity = (slots - items.Count) * nInventory.ItemsStacks[item.Type] + count;
                if (item.Count > maxCapacity) tail = item.Count - maxCapacity;
            }
            return tail;
        }

        public static int GetCountOfType(Vehicle vehicle, ItemType type)
        {
            if (!vehicle.HasData("ITEMS")) return 0;
            List<nItem> items = vehicle.GetData<List<nItem>>("ITEMS");
            int count = 0;

            for (int i = 0; i < items.Count; i++)
            {
                if (i >= items.Count) break;
                if (items[i].Type == type) count += items[i].Count;
            }

            return count;
        }

        public static void Remove(Vehicle vehicle, ItemType type, int amount)
        {
            if (!vehicle.HasData("ITEMS")) return;
            List<nItem> items = vehicle.GetData<List<nItem>>("ITEMS");
            
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (i >= items.Count) continue;
                if (items[i].Type != type) continue;
                if (items[i].Count <= amount)
                {
                    amount -= items[i].Count;
                    items.RemoveAt(i);
                }
                else
                {
                    items[i].Count -= amount;
                    amount = 0;
                    break;
                }
            }

            if (vehicle.GetData<string>("ACCESS") == "PERSONAL" || vehicle.GetData<string>("ACCESS") == "GARAGE")
                VehicleManager.Vehicles[vehicle.NumberPlate].Items = items;

            foreach (Player p in NAPI.Pools.GetAllPlayers())
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                if (p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 2 && p.HasData("SELECTEDVEH") && p.GetData<Vehicle>("SELECTEDVEH") == vehicle) GUI.Dashboard.OpenOut(p, vehicle.GetData<List<nItem>>("ITEMS"), "Багажник", 2);
            }
        }

        public static void Remove(Vehicle vehicle, nItem item)
        {
            if (!vehicle.HasData("ITEMS")) return;
            List<nItem> items = vehicle.GetData<List<nItem>>("ITEMS");

            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type) || 
                item.Type == ItemType.BagWithDrill || item.Type == ItemType.BagWithMoney || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing)
            {
                items.Remove(item);
            }
            else
            {
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    if (i >= items.Count) continue;
                    if (items[i].Type != item.Type) continue;
                    if (items[i].Count <= item.Count)
                    {
                        item.Count -= items[i].Count;
                        items.RemoveAt(i);
                    }
                    else
                    {
                        items[i].Count -= item.Count;
                        item.Count = 0;
                        break;
                    }
                }
            }

            if (vehicle.GetData<string>("ACCESS") == "PERSONAL" || vehicle.GetData<string>("ACCESS") == "GARAGE")
                VehicleManager.Vehicles[vehicle.NumberPlate].Items = items;

            foreach (Player p in NAPI.Pools.GetAllPlayers())
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                if (p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 2 && p.HasData("SELECTEDVEH") && p.GetData<Vehicle>("SELECTEDVEH") == vehicle) GUI.Dashboard.OpenOut(p, vehicle.GetData<List<nItem>>("ITEMS"), "Багажник", 2);
            }
        }
    }
}
