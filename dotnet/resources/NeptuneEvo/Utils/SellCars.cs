using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Core
{
    class SellCars : Script
    {
        [ServerEvent(Event.PlayerDisconnected)]
        private static void Event_PlayerDisconnect(Player player, DisconnectionType type, string reason)
        {
            try { 
                RemoveAllVehPly(player);
            }
            catch (Exception e) { Log.Write("SET: " + e.ToString(), nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public static void Event_playerenterveh(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (vehicle.GetData<string>("ACCESS") == "INPARK")
                {
                    VehicleForSell classv = vehicle.GetData<VehicleForSell>("PARKCLASS");
                    if (classv.Owner != player)
                        Trigger.PlayerEvent(player, "openDialog", "PARK_BUY", $"Вы хотите купить данный автомобиль за {classv.Cost}$?");
                    else
                        Trigger.PlayerEvent(player, "openDialog", "PARK_DESTORY", $"Вы хотите убрать машину с продажи?");
                }
            }
            catch (Exception e) { Log.Write("SET: " + e.ToString(), nLog.Type.Error); }
        }

        [ServerEvent(Event.VehicleDeath)]
        public void Event_vehicleDeath(Vehicle vehicle)
        {
            try
            {
                if (vehicle.HasData("PARKCLASS"))
                    vehicle.GetData<VehicleForSell>("PARKCLASS").Destroy(false);
            }
            catch (Exception e) { Log.Write("VehicleDeath: " + e.Message, nLog.Type.Error); }
        }


        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {

                ColShape shape = NAPI.ColShape.CreateCylinderColShape(mainpos, 5f, 3, 0);
                Blip blip = NAPI.Blip.CreateBlip(269, new Vector3(-1634.565, -890.6557, 7.853827), 1, 3, Main.StringToU16("Авто рынок Б/У"), 255, 0, true, 0, 0);
                Marker marker = NAPI.Marker.CreateMarker(27, mainpos + new Vector3(0, 0, 0.2f), new Vector3(), new Vector3(), 5f, new Color(0, 86, 214, 220), false, 0);
                TextLabel label = NAPI.TextLabel.CreateTextLabel($"~b~Выставить авто на продажу", new Vector3(mainpos.X, mainpos.Y, mainpos.Z + 1.5), 20F, 0.5F, 0, new Color(255, 255, 255), true, 0);


                shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 67);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                Log.Write("Loaded all park places!", nLog.Type.Success);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"BUSINESSES\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private static nLog Log = new nLog("SellCars");
        public static Vector3 mainpos = new Vector3(-1642.254, -819.3533, 9.008316);
        public static List<bool> ParkPlaces = new List<bool> { };
        public static List<VehicleForSell> vehlist = new List<VehicleForSell>
        { 		
			new VehicleForSell(new Vector3(-1603.371, -868.4901, 10), 318,0),
			new VehicleForSell(new Vector3(-1605.635, -866.1621, 10), 318,1),
			new VehicleForSell(new Vector3(-1608.074, -864.277, 10), 318,2),
			new VehicleForSell(new Vector3(-1610.264, -862.1171, 10), 318,3),
			new VehicleForSell(new Vector3(-1612.652, -860.0998, 10), 318,4),
			new VehicleForSell(new Vector3(-1615.102, -858.2977, 10), 318,5),
			new VehicleForSell(new Vector3(-1617.568, -856.302, 10), 318,6),
			new VehicleForSell(new Vector3(-1619.794, -854.2312, 10), 318,7),
			new VehicleForSell(new Vector3(-1622.26, -852.3439, 10), 318,8),
			new VehicleForSell(new Vector3(-1624.527, -850.2991, 10), 318,9),
			new VehicleForSell(new Vector3(-1626.514, -847.8896, 10), 318,10),
			new VehicleForSell(new Vector3(-1628.758, -845.8388, 10), 318,11),
			new VehicleForSell(new Vector3(-1631.421, -844.2867, 10), 318,12),
			new VehicleForSell(new Vector3(-1633.788, -842.3535, 10), 318,13),
			new VehicleForSell(new Vector3(-1636.02, -840.2848, 10), 318,14),
			new VehicleForSell(new Vector3(-1638.317, -838.2304, 10), 318,15),
			new VehicleForSell(new Vector3(-1640.753, -836.4019, 10), 318,16),
			new VehicleForSell(new Vector3(-1643.22, -834.1756, 10), 318,17),
			new VehicleForSell(new Vector3(-1611.799, -877.9619, 10), 318,18),
			new VehicleForSell(new Vector3(-1613.909, -875.9293, 10), 318,19),
			new VehicleForSell(new Vector3(-1616.37, -873.9812, 10), 318,20),
			new VehicleForSell(new Vector3(-1618.696, -872.0499, 10), 318,21),
			new VehicleForSell(new Vector3(-1621.123, -870.0228, 10), 318,22),
			new VehicleForSell(new Vector3(-1623.172, -867.6881, 10), 318,23),
			new VehicleForSell(new Vector3(-1625.864, -866.0743, 10), 318,24),
			new VehicleForSell(new Vector3(-1627.99, -863.8143, 10), 318,25),
			new VehicleForSell(new Vector3(-1630.415, -862.035, 10), 318,26),
			new VehicleForSell(new Vector3(-1634.221, -858.9255, 10), 318,27),
			new VehicleForSell(new Vector3(-1636.48, -856.7689, 10), 318,28),
			new VehicleForSell(new Vector3(-1638.83, -854.9257, 10), 318,29),
			new VehicleForSell(new Vector3(-1641.262, -853.1178, 10), 318,30),
			new VehicleForSell(new Vector3(-1643.359, -850.9518, 10), 318,31),
			new VehicleForSell(new Vector3(-1645.657, -848.8409, 10), 318,32),
			new VehicleForSell(new Vector3(-1648.376, -847.1868, 10), 318,33),
			new VehicleForSell(new Vector3(-1650.406, -844.8016, 10), 318,34),
			new VehicleForSell(new Vector3(-1652.76, -842.9169, 10), 318,35)
        };

        public static int FindFreeParkPlace()
        {
            try { 
                var i = 0;
                foreach (bool active in ParkPlaces)
                {
                    if (!active)
                    {
                        return i;
                    }
                    i++;
                }
                    return -1;
            }
            catch (Exception e) { Log.Write("SET: " + e.ToString(), nLog.Type.Error);  return -1; }
        }
        
        public static void RemoveAllVehPly(Player player)
        {
            try
            { 
                foreach (VehicleForSell veh in vehlist)
                {
                    if (veh.Owner != player) continue;
                    var Number = NAPI.Vehicle.GetVehicleNumberPlate(veh.Vehicle);
                    if (!VehicleManager.Vehicles.ContainsKey(Number)) return;
                    var house = Houses.HouseManager.GetHouse(player, true);

                    var apartament = Houses.HouseManager.GetApart(player, true);

                    if (house == null)
                    {
                        if (apartament != null)
                        {
                            house = apartament;
                        }
                    }
                    if (house != null)
                    {
                        //veh.Vehicle.SetSharedData("freeze", false);
                        //Trigger.PlayerEventInRange(veh.Position, 500f, "vehiclefreeze", veh.Vehicle, false);
                        Houses.Garage Garage = Houses.GarageManager.Garages[house.GarageID];
                        Garage.SendVehicleIntoGarage(Number);
                    }

                    veh.Destroy(true);
                }
            }
            catch (Exception e) { Log.Write("SETINGARAGE: " + e.ToString(), nLog.Type.Error); }
        }

        /*[Command("vehby")] DEBUG
        public static void CMD_deleteveh(Player player)
        {
            RemoveAllVehPly(player);
        }*/



        public class VehicleForSell
        {
            public Vector3 Position { get; set; }
            public float Rotation { get; set; }
            public Vehicle Vehicle { get; set; }
            public Player Owner { get; set; }
            public int Place { get; set; }
            public int Cost { get; set; }
            [JsonIgnore]
            public TextLabel Label { get; set; }

            public VehicleForSell(Vector3 position, float rotation, int place)
            {
                Position = position; Rotation = rotation;
                ParkPlaces.Add(false);
                Place = place;
            }

            public void Destroy(bool delete, bool unfreeze = true)
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        ParkPlaces[Place] = false;
                        if (delete)
                            Vehicle.Delete();
                        else
                        {
                            if (unfreeze)
                            {
                                Vehicle.SetSharedData("freeze", false);
                                Vehicle.ResetSharedData("freeze");
                                Trigger.PlayerEventInRange(Position, 500f, "vehiclefreeze", Vehicle, false);
                            }
                            
                            Vehicle.ResetData("PARKCLASS");
                            Vehicle.SetData("ACCESS", "PERSONAL");
                        }
                        Label.Delete();
                        Owner = null; Cost = -1;
                    }
                    catch { }
                });
            }
            public void SetVehicle(Vehicle veh, Player player, int cost)
            {
                try
                {
                    if (!Main.Players.ContainsKey(player)) return;
                    int freeplace = FindFreeParkPlace();
                    if (freeplace == -1)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На парковке нет свободного места", 3000);
                        return;
                    }
                    var Number = NAPI.Vehicle.GetVehicleNumberPlate(veh);
                    if (!VehicleManager.Vehicles.ContainsKey(Number))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Такой машины не существует", 3000);
                        return;
                    }
                    if (veh.GetData<string>("ACCESS") != "PERSONAL" || veh.GetData<Player>("OWNER") != player || VehicleManager.Vehicles[veh.NumberPlate].Holder != player.Name)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Это не Ваша машина!", 3000);
                        return;
                    }
                    if (FineManager.HaveFine(Number, player.Name))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У данного транспорта есть штрафы!", 3000);
                        return;
                    }

                    /*if (VehicleManager.Vehicles[Number].Sell >= 3)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Данную машину можно только продать в ГОС", 3000);
                        return;
                    }*/

                    //VehicleManager.WarpPlayerOutOfVehicle(player);
                    Vehicle = veh; Owner = player; Cost = Math.Abs(cost);

                    VehicleStreaming.SetEngineState(veh, false);
                    VehicleStreaming.SetLockStatus(veh, false);

                    NAPI.Entity.SetEntityPosition(Vehicle, Position);
                    NAPI.Entity.SetEntityRotation(Vehicle, new Vector3(0, 0, Rotation));
                    veh.SetData("ACCESS", "INPARK");
                    veh.SetData("PARKCLASS", this);
                    ParkPlaces[Place] = true;
                    NAPI.Task.Run(() =>
                    {
                        NAPI.Entity.SetEntityPosition(Vehicle, veh.Position + new Vector3(0, 0, 0.2f));
                        NAPI.Entity.SetEntityRotation(Vehicle, new Vector3(0, 0, Rotation));
                        veh.SetSharedData("freeze", true);
                        Trigger.PlayerEventInRange(Position, 500f, "vehiclefreeze", veh, true);
                    }, 150);

                    Label = NAPI.TextLabel.CreateTextLabel($"~b~Цена:~w~ {Cost}$\n~b~{ParkManager.GetNormalName(VehicleManager.Vehicles[Number].Model)}\n~b~Пробег:~w~ {Convert.ToInt32( VehicleManager.Vehicles[Number].Sell)} км.", new Vector3(Position.X, Position.Y, Position.Z + 1.5), 20F, 0.5F, 0, new Color(255, 255, 255), true, 0);
                }
                catch (Exception e) { Log.Write("SET: " + e.ToString(), nLog.Type.Error); }
                
            }

            public void Buy(Player player)
            {
                try
                {
                    if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].Money < Cost)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас не достаточно средств {Cost - Main.Players[player].Money}$", 3000);
                    return;  
                }

                    var house = Houses.HouseManager.GetHouse(player, true);

                    var apartament = Houses.HouseManager.GetApart(player, true);

                    if (house == null)
                    {
                        if (apartament != null)
                        {
                            house = apartament;
                        }
                    }
                if (house == null && VehicleManager.getAllPlayerVehicles(player.Name.ToString()).Count > 1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет личного дома", 3000);
                    return;
                }
                if (house != null)
                {

                    if (house.GarageID == 0 && VehicleManager.getAllPlayerVehicles(player.Name.ToString()).Count > 1)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет гаража", 3000);
                        return;
                    }

                    var garage = Houses.GarageManager.Garages[house.GarageID];
                    if (VehicleManager.getAllPlayerVehicles(player.Name).Count >= Houses.GarageManager.GarageTypes[garage.Type].MaxCars)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас максимальное кол-во машин", 3000);
                        return;
                    }
                }
                else if (VehicleManager.getAllPlayerVehicles(player.Name).Count > 1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас есть нет мест на парковке!", 3000);
                    return;
                }
                var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.CarKey));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нет места для ключей", 3000);
                    return;
                }
                var Number = NAPI.Vehicle.GetVehicleNumberPlate(Vehicle);
                VehicleManager.VehicleData vData = VehicleManager.Vehicles[Number];
                VehicleManager.Vehicles[Number].Holder = player.Name;
                VehicleManager.Vehicles[Number].Plastic = DateTime.Now.AddYears(-1000);
                MySQL.Query($"UPDATE vehicles SET holder='{player.Name}' WHERE number='{Number}'");
                    VehicleManager.Save(Number);

                nInventory.Add(player, new nItem(ItemType.CarKey, 1, $"{Number}_{VehicleManager.Vehicles[Number].KeyNum}"));
                MoneySystem.Wallet.Change(player, -Cost);
                MoneySystem.Wallet.Change(Owner, Cost);
                Notify.Send(Owner, NotifyType.Info, NotifyPosition.BottomCenter, $"У вас купили {ParkManager.GetNormalName(VehicleManager.Vehicles[Number].Model)}", 3000);
                //MoneySystem.Bank.Change(Main.Players[Owner].Bank, Cost, true);
                GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[Owner].UUID})", Cost, $"buyCar({Number})");


                Vehicle.SetData("OWNER", player);

                if (house != null)
                {
                    var garage = Houses.GarageManager.Garages[house.GarageID];
                    garage.SetOutVehicle(Number, Vehicle);
                }
                    Destroy(false);
                }
                catch (Exception e) { Log.Write("SET: " + e.ToString(), nLog.Type.Error); }
            }
        }
    }
}