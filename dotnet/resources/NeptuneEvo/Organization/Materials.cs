using GTANetworkAPI;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;
using System;
using System.Collections.Generic;
using NeptuneEVO.Businesses;
using System.Data;
using NeptuneEVO.GUI;
using Newtonsoft.Json;

namespace NeptuneEVO.Organization
{
    class MaterialsI : Script
    {
        private static nLog Log = new nLog("Organization");

        public static uint DimensionID = 2000;

        public static int ForMaterial = 1;

        public static Dictionary<string, int> CarsMaterials = new Dictionary<string, int>
        {
            { "boxville4", 35000 },
            { "nspeedo", 75000 },
            { "mule3", 100000 },
            { "mule4", 130000 },
            { "sprinter211", 170000 },
            { "pounder2", 250000 }
        };

        public class MaterialsGet
        {
            private int Type = 0;
            private int Color = 4;
            private Vector3 Position;
            public MaterialsGet(int bliptype, int blipcolor, Vector3 pos)
            {
                try
                { 

                Type = bliptype; Color = blipcolor; Position = pos;

                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~[~w~E~b~]~w~ Купить материалы"), new Vector3(pos.X, pos.Y, pos.Z + 1.5), 1f + 2f, 0.5F, 0, new Color(255, 255, 255), true, 0);
                NAPI.Blip.CreateBlip(bliptype, pos, 0.7f, Convert.ToByte(blipcolor), Main.StringToU16("Покупка материалов"), 255, 0, true);
                NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 2.1f), new Vector3(), new Vector3(), 3f, new Color(255, 255, 255, 220), false, 0);

                ColShape Shape = NAPI.ColShape.CreateCylinderColShape(pos, 3f, 10, 0);

                Shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 154);
                    }
                    catch (Exception e) { Console.WriteLine("OnEnter: " + e.Message); }
                };
                Shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Console.WriteLine("OnExit: " + e.Message); }
                };

            }
            catch (Exception e) { Log.Write("matarialsget " + e.ToString(), nLog.Type.Error); }

    }

            public static void SetDist(Player player, int id, int carmats)
            {
                try
                { 
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].Org == "") return;
                if (!BCore.BizList.ContainsKey(id)) return;
                if (!VehicleManager.Vehicles.ContainsKey(player.Vehicle.NumberPlate)) return;
                if (!player.IsInVehicle || !CarsMaterials.ContainsKey(VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model)) return;
                //if (player.HasData("ORDER_MAT")) return;
                if (CarsMaterials[VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model] < carmats)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Перегруз! Макс. {CarsMaterials[VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model]} мат.", 3000);
                    return;
                }
                int price = carmats * MaterialsI.ForMaterial / 4;
                if (Main.Players[player].Money < price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас недостаточно средств!", 3000);
                    return;
                }
                BCore.Bizness biz = BCore.BizList[id];


                

                player.SetData("MAT", carmats);
                player.SetData("IDMAT", id);

                Trigger.PlayerEvent(player, "openDialog", "ORG_YESMAT", $"Купить {carmats} мат. за {price} $  ?");
                //Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили материалы за {price}$", 3000);
                }
                catch (Exception e) { Log.Write("setdist " + e.ToString(), nLog.Type.Error); }
            }
        }

        public static string GetNumberByKey(Player player, int key, List<string> cars)
        {
            try
            {
                int i = 0;
                foreach (string number in cars)
                    if (i == key)
                        return number;
                    else
                        i++;
                return "";
            }
            catch { return ""; }
        }

        public static void SpawnCar(Player player, int key)
        {
            try
            {
                if (string.IsNullOrEmpty(Main.Players[player].Org)) return;
                MaterialsI.MaterialsOrg org = (MaterialsI.MaterialsOrg)OCore.OrgListNAME[Main.Players[player].Org];

                string number = GetNumberByKey(player, key, org.GetVehGarage());
                if (number == "") return;
                Vehicle veh = null;
                foreach (Vehicle ve in NAPI.Pools.GetAllVehicles())
                    if (ve.NumberPlate == number)
                    {
                        veh = ve;
                        break;
                    }
                if (veh == null)
                {
                    org.SpawnCar(number);
                    Notify.Send(player, NotifyType.Alert, NotifyPosition.CenterRight, "Транспорт был вызван!", 2000);
                    return;
                }

                if (org.VehiclesInGarage.Contains(veh))
                {
                    Notify.Send(player, NotifyType.Alert, NotifyPosition.CenterRight, "Транспорт был убран!", 2000);
                    org.VehiclesInGarage.Remove(veh);

                    NAPI.Task.Run(() =>
                   {
                       try
                       {
                           veh.Delete();
                       }
                       catch { }
                   });
                    
                }
                else if (org.VehiclesOut.Contains(veh))
                {
                    Notify.Send(player, NotifyType.Alert, NotifyPosition.CenterRight, "Транспорт был убран!", 2000);
                    org.VehiclesOut.Remove(veh);

                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            veh.Delete();
                        }
                        catch { }
                    });
                }
            }
            catch (Exception e) { Log.Write("SPAWN: " + e.ToString(), nLog.Type.Error); }

        }


        public class MaterialsOrg : OCore.Organization
        {
            public List<Player> PlayersInGarage { get; set; } = new List<Player> { };
            public List<Vehicle> VehiclesInGarage { get; set; } = new List<Vehicle> { };
            public List<Vehicle> VehiclesOut { get; set; } = new List<Vehicle> { };
            public int MaxVehicles { get; set; } = 0;
            public Vector3 Position { get; set; }
            public Vector3 SafePosition { get; set; }
            public List<string> MembersGet { get; set; }
            public GarageInfo Garage { get; set; }
            public uint Dimension { get; set; } = 0;
            public int Angle { get; set; }

            public MaterialsOrg(string name, int type, string owner, List<string> members, int level, Vector3 pos, Vector3 safepos, int angle, int id, int cost, int bankid) : base(name, type, owner, members, level, id, cost, bankid)
            {
                try
                {
                    Dimension = DimensionID; SafePosition = safepos; Angle = angle;
                    DimensionID++;
                    Garage = GarageList[level];
                    Position = pos;
                    MaxVehicles += Garage.MaxVehicles;
                    CostForOrganization = 15000;
                    CostForSellOrganization = 7500;
                    SpawnCars(GetVehGarage());
                    CreateStuff(pos, Garage.Exitpos, Dimension);

                    Vector3 posi = new Vector3(968.548, -3002.039, -40.76699);

                    NAPI.TextLabel.CreateTextLabel("Вызов транспорта", new Vector3(posi.X, posi.Y, posi.Z + 2.3), 1f + 2f, 0.5F, 0, new Color(255, 255, 255), true, (uint)Dimension);
                    NAPI.Marker.CreateMarker(27, posi + new Vector3(0, 0, 0.2f), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220), false, (uint)Dimension);
                    ColShape Shape = NAPI.ColShape.CreateCylinderColShape(posi, 1f, 10, (uint)Dimension);

                    Shape.OnEntityEnterColShape += (s, entity) =>
                    {
                        try
                        {
                            NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 518);
                            entity.SetData("ORG", true);
                        }
                        catch (Exception e) { Console.WriteLine("OnEnter: " + e.Message); }
                    };
                    Shape.OnEntityExitColShape += (s, entity) =>
                    {
                        try
                        {
                            NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
                            entity.ResetData("ORG");
                        }
                        catch (Exception e) { Console.WriteLine("OnExit: " + e.Message); }
                    };

                }
                catch (Exception e) { Log.Write("materialorg " + e.ToString(), nLog.Type.Error); }
            }

            public List<string> GetVehGarage()
            {
                try
                {
                    List<string> numbers = new List<string> { };
                    var result = MySQL.QueryRead($"SELECT * FROM vehicles WHERE holder='{Name}'");
                    foreach (DataRow Row in result.Rows)
                        numbers.Add(Row["number"].ToString());
                    
                    return numbers;
                }
                catch (Exception e) { Log.Write("getvehgarage " + e.ToString(), nLog.Type.Error); return null; }
            }

            public override void Enter(Player player)
            {
                try
                {
                    if (!Main.Players.ContainsKey(player)) return;
                
                    if (Owner == "Государство")
                    {

                        if (Main.Players[player].OrgLic != 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет лицензии на Грузоперевозки", 3000);
                            return;
                        }

                        Trigger.PlayerEvent(player, "openDialog", "ORG_BUY", $"Купить Склад за {Price}$ ?");
                        return;
                    }
                    if (player.Name != Owner)
                        if (!Members.Contains(player.Name)) return;
                    if (PlayersInGarage.Contains(player)) return;
                    Main.Players[player].ExteriorPos = player.Position;
                    if (player.IsInVehicle && player.Vehicle.GetData<string>("ACCESS") == "PERSONAL" && VehicleManager.Vehicles.ContainsKey(player.Vehicle.NumberPlate) && player.VehicleSeat == 0 && VehicleManager.Vehicles[player.Vehicle.NumberPlate].Holder == Name)
                    {
                        InGarage(player);
                        return;
                    }

                    Main.PlayerEnterInterior(player, Garage.Exitpos);
                    NAPI.Entity.SetEntityPosition(player, Garage.Exitpos + new Vector3(0, 0, 1.13));
                    NAPI.Entity.SetEntityDimension(player, Dimension);
                    PlayersInGarage.Add(player);
                }
                catch (Exception e) { Log.Write("enter " + e.ToString(), nLog.Type.Error); }
            }

            public override void Exit(Player player)
            {
                try
                {
                    if (!Main.Players.ContainsKey(player)) return;
                    if (!PlayersInGarage.Contains(player)) return;

                    Main.Players[player].ExteriorPos = new Vector3();
                    Main.PlayerEnterInterior(player, Position + new Vector3(0,0,1.5));
                    NAPI.Entity.SetEntityPosition(player, Position + new Vector3(0, 0, 1.5));
                    NAPI.Entity.SetEntityDimension(player, 0);
                    PlayersInGarage.Remove(player);
                }
                catch (Exception e) { Log.Write("exit " + e.ToString(), nLog.Type.Error); }
            }


            public int GetPark()
            {
                try
                {
                    int result = -1;
                    List<bool> free = new List<bool> { };
                    for (int i = 0; i < 5 + 1; i++)
                        free.Add(false);
                    foreach (Vehicle veh in VehiclesInGarage)
                        if (veh.HasData("PARK"))
                            free[veh.GetData<int>("PARK")] = true;
                    for (int i = 0; i < 5 + 1; i++)
                        if (!free[i])
                            result = i;
                    return result;
                }
                catch (Exception e) { Log.Write("getpark " + e.ToString(), nLog.Type.Error); return 0; }
            }

            public void SpawnCar(string number)
            {
                try
                {
                    int i = GetPark();
                    if (i == -1) return;
                    if (i > 5) return;
                    Vector3 pos = Garage.GetPos(i);
                    int angle = Garage.GetAngle(i);
                    if (!VehicleManager.Vehicles.ContainsKey(number)) return;
                    var vehData = VehicleManager.Vehicles[number];
                    if (vehData.Health < 1) return;
                    var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vehData.Model), pos + new Vector3(0, 0, 0.25), angle, 0, 0);
                    veh.NumberPlate = number;
                    NAPI.Entity.SetEntityDimension(veh, (uint)Dimension);
                    VehicleStreaming.SetEngineState(veh, false);
                    VehicleStreaming.SetLockStatus(veh, true);
                    veh.SetData("ACCESS", "GARAGEORG");
                    veh.SetData("ITEMS", vehData.Items);
                    veh.SetData("PARK", i);
                    veh.SetData("PARKCL", this);
                    veh.SetSharedData("PETROL", vehData.Fuel);
                    VehicleManager.ApplyCustomization(veh);
                    VehiclesInGarage.Add(veh);
                }
                catch (Exception e) { Log.Write("spawncar " + e.ToString(), nLog.Type.Error); }
            }

            public void InGarage(Player player)
            {
                try
                {
                    Vehicle veh = player.Vehicle;
                    if (VehiclesInGarage.Contains(veh) || !VehiclesOut.Contains(veh)) return;

                    if (veh.HasData("MATERIALS"))
                    {
                        MoneySystem.Wallet.Change(player, veh.GetData<int>("MATERIALS") * MaterialsI.ForMaterial / 4);
                    }

                    //SpawnCar(veh.NumberPlate);
                    NAPI.Task.Run(() =>
                    {
                       try
                       {
                           veh.Delete();
                       }
                       catch { }
                    });
                    VehiclesOut.Remove(veh);

                    Main.Players[player].ExteriorPos = player.Position;

                    NAPI.Entity.SetEntityDimension(veh, Dimension);
                    NAPI.Entity.SetEntityPosition(player, Garage.Exitpos);

                    PlayersInGarage.Add(player);
                }
                catch (Exception e) { Log.Write("ingarage " + e.ToString(), nLog.Type.Error); }
            }

            public void OutGarage(Player player)
            {
                try
                {
                    Vehicle vehi = player.Vehicle;
                    if (!VehiclesInGarage.Contains(vehi) || VehiclesOut.Contains(vehi)) return;
                    VehiclesInGarage.Remove(vehi);
                    PlayersInGarage.Remove(player);

                    Main.Players[player].ExteriorPos = new Vector3();

                    string num = vehi.NumberPlate;

                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            vehi.Delete();
                        }
                        catch {  }
                    });

                    var vehData = VehicleManager.Vehicles[num];
                    NAPI.Entity.SetEntityPosition(player, SafePosition + new Vector3(0,0,1.2));
                    NAPI.Entity.SetEntityDimension(player, 0);

                    var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vehData.Model), SafePosition + new Vector3(0, 0, 0.25), Angle, 0, 0);
                    veh.NumberPlate = num;
                    VehicleStreaming.SetEngineState(veh, true);
                    veh.SetData("ACCESS", "PERSONAL");
                    veh.SetData("ITEMS", vehData.Items);
                    veh.SetData("PARKCL", this);
                    veh.SetSharedData("PETROL", vehData.Fuel);
                    VehicleManager.ApplyCustomization(veh);
                    VehiclesOut.Add(veh);
                    player.SetIntoVehicle(veh, 0);
                    NAPI.Task.Run(() => { player.SetIntoVehicle(veh, 0); }, 1000);
                    
                }
                catch (Exception e) { Log.Write("outgarage " + e.ToString(), nLog.Type.Error); }
            }

            public void SpawnCars(List<string> numbers)
            {
                try
                {
                    if (Garage == null) return;
                    for(int i = 1; i < numbers.Count + 1; i++)
                    {
                        if (i > Garage.MaxVehicles) break;
                        if (i > 5) break;
                        Vector3 pos = Garage.GetPos(i);
                        int angle = Garage.GetAngle(i);
                        string number = numbers[i - 1];
                        if (!VehicleManager.Vehicles.ContainsKey(number)) return;
                        var vehData = VehicleManager.Vehicles[number];
                        if (vehData.Health < 1) continue;
                        var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vehData.Model), pos + new Vector3(0, 0, 0.25), angle, 0, 0);
                        veh.NumberPlate = number;
                        NAPI.Entity.SetEntityDimension(veh, (uint)Dimension);
                        VehicleStreaming.SetEngineState(veh, false);
                        VehicleStreaming.SetLockStatus(veh, true);
                        veh.SetData("ACCESS", "GARAGEORG");
                        veh.SetData("ITEMS", vehData.Items);
                        veh.SetData("PARK", i);
                        veh.SetData("PARKCL", this);
                        veh.SetSharedData("PETROL", vehData.Fuel);
                        VehicleManager.ApplyCustomization(veh);
                        VehiclesInGarage.Add(veh);
                    }
                }
                catch (Exception e) { Log.Write("spawncars " + e.ToString(), nLog.Type.Error); }
            }

        }

        public static List<GarageInfo> GarageList = new List<GarageInfo>
        { 
            new GarageInfo(0, 10, new Vector3(970.5966, -2989.244, -40.4), new Dictionary<Vector3, int> { 
                { new Vector3(970.4229, -3020.779, -40.04155), 1 }, 
                { new Vector3(1001.493, -3021.141, -40.03812), 91 },
                { new Vector3(1000.588, -2994.007, -40.02647), 0 },
                { new Vector3(978.3574, -3002.067, -40.01514), 89 },
                { new Vector3(994.3049, -2994.925, -40.03991), 1 },
            })

        };


        internal class GarageInfo
        {
            public int Type { get; set; } = -1;
            public int MaxVehicles { get; set; } = -1;
            public Vector3 Exitpos { get; set; } = new Vector3(0, 0, 0);
            public string IPL { get; set; } = "none";
            public Dictionary<Vector3, int> VehiclePos { get; set;} = new Dictionary<Vector3, int> {};
            public GarageInfo(int type, int maxveh, Vector3 exitpos, Dictionary<Vector3, int> vehpos)
            {
                Type = type; MaxVehicles = maxveh; Exitpos = exitpos; VehiclePos = vehpos;
            }

            public Vector3 GetPos(int index)
            {
                try
                { 
                    int i = 0;
                
                    foreach(KeyValuePair<Vector3, int> pairs in VehiclePos)
                    {
                        i++;
                        if (i == index)
                            return pairs.Key;
                    }
                    return new Vector3(0, 0, 0);
                }
                catch (Exception e) { Log.Write("getpos " + e.ToString(), nLog.Type.Error); return new Vector3(0,0,0); }
            }

            public int GetAngle(int index)
            {
                try
                { 
                    int i = 0;

                    foreach (KeyValuePair<Vector3, int> pairs in VehiclePos)
                    {
                        i++;
                        if (i == index)
                            return pairs.Value;
                    }
                    return -1;
                }
                catch (Exception e) { Log.Write("getangle " + e.ToString(), nLog.Type.Error); return 0; }
            }
        }

        [RemoteEvent("openonside")]
        public static void PlayerEvent_OpenTable(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (string.IsNullOrEmpty(Main.Players[player].Org)) return;
                if (!Organization.OCore.OrgListNAME.ContainsKey(Main.Players[player].Org)) return;

                MaterialsI.MaterialsOrg org = (MaterialsI.MaterialsOrg)Organization.OCore.OrgListNAME[Main.Players[player].Org];

                bool isowner = false;

                if (org.Owner == player.Name)
                    isowner = true;

                List<object> datamembers = new List<object>();

                foreach(string nick in org.Members)
                    datamembers.Add(nick);

                List<object> datacars = new List<object>();
           
                foreach (string number in org.GetVehGarage())
                {
                    var list = new List<object> { number, ParkManager.GetNormalName( VehicleManager.Vehicles[number].Model ) };
                    datacars.Add(list);
                }

                List<object> data = new List<object> { org.Name, MoneySystem.Bank.Accounts[org.BankID].Balance, org.GetNalog() };

            Trigger.PlayerEvent(player, "opentableorg", JsonConvert.SerializeObject(datamembers), JsonConvert.SerializeObject(datacars), JsonConvert.SerializeObject(data), Convert.ToInt32(isowner));
                }
                catch (Exception e) { Log.Write("opentable " + e.ToString(), nLog.Type.Error); }
            }

        [RemoteEvent("memberscall")]
        public static void PlayerEvent_Members(Player player, string nick)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (string.IsNullOrEmpty(Main.Players[player].Org)) return;

                MaterialsI.MaterialsOrg org = (MaterialsI.MaterialsOrg)Organization.OCore.OrgListNAME[Main.Players[player].Org];

                if (org.Owner != player.Name) return;

                if (!org.Members.Contains(nick)) return;

                org.RemoveMember(nick);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выгнали из организации: {nick} !", 3000);
            }
            catch (Exception e) { Log.Write("memberscall " + e.ToString(), nLog.Type.Error); }
        }

        [RemoteEvent("maladoyinput")]
        public static void PlayerEvent_Input(Player player, string inp)
        {
            try
            { 
                if (!Main.Players.ContainsKey(player)) return;
                if (string.IsNullOrEmpty(Main.Players[player].Org)) return;

                MaterialsI.MaterialsOrg org = (MaterialsI.MaterialsOrg)Organization.OCore.OrgListNAME[Main.Players[player].Org];

                if (org.Owner != player.Name) return;

                try
                {
                    Convert.ToInt32(inp);
                }
                catch
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                    return;
                }

                int id = Convert.ToInt32(inp);
                List<Player> players = Main.GetPlayersInRadiusOfPosition(player.Position, 5f);
                bool yes = false;
                foreach (Player ply in players)
                    if (ply.Value == id && ply.Value != player.Value)
                    {
                        yes = true;
                        break;
                    }
                if (!yes)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Данного гражданина нет в радиусе 10 метров!", 3000);
                    return;
                }
                if (player.HasData("INVITED") && player.GetData<string>("INVITED") == org.Name )
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже пригласили данного гражданина!", 3000);
                    return;
                }
                
                Player target = Main.GetPlayerByID(id);

                if (string.IsNullOrEmpty(Main.Players[player].Org)) return;
                if (Main.Players[target].Org != "")
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин состоит в организации!", 3000);
                    return;
                }

                target.SetData("ORG_INV", org);
                target.SetData("INVITED", org.Name);

                Trigger.PlayerEvent(target, "openDialog", "ORG_INVITE", $"Вы хотите вступить в организациию {org.Name}?");
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы пригласили {target.Name} в организацию", 3000);

            }
            catch (Exception e) { Log.Write("callinv " + e.ToString(), nLog.Type.Error); }
        }

        [RemoteEvent("callsell")]

        public static void PlayerEvent_Sell(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (string.IsNullOrEmpty(Main.Players[player].Org)) return;

                MaterialsI.MaterialsOrg org = (MaterialsI.MaterialsOrg)Organization.OCore.OrgListNAME[Main.Players[player].Org];

                if (org.Owner != player.Name) return;

                string secondary = "";
                if (org.Type == 0)
                {
                    MaterialsOrg morg = (MaterialsI.MaterialsOrg)org;
                    List<string> cars = morg.GetVehGarage();
                    int result = 0;
                    foreach (string number in cars)
                    {
                        result += (int)Math.Floor(BCore.CostForCar(VehicleManager.Vehicles[number].Model) * 0.7);
                    }
                    if (result != 0)
                        secondary += $"+{result}$";
                }

                Trigger.PlayerEvent(player, "openDialog", "ORG_SELL", $"Продать организацию за {org.Price * 0.4}${secondary} ?");
            }
            catch (Exception e) { Log.Write("callsell " + e.ToString(), nLog.Type.Error); }
           
        }

        [RemoteEvent("carscall")]
        public static void PlayerEvent_Cars(Player player, string number, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
            if (string.IsNullOrEmpty(Main.Players[player].Org)) return;

            MaterialsI.MaterialsOrg org = (MaterialsI.MaterialsOrg)Organization.OCore.OrgListNAME[Main.Players[player].Org];

            if (id == 2 && org.Owner != player.Name) return;

            if (!org.GetVehGarage().Contains(number)) return;

            switch (id)
            {
                case 0:
                    var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.CarKey));
                    if (tryAdd == -1 || tryAdd > 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                        return;
                    }

                    nInventory.Add(player, new nItem(ItemType.CarKey, 1, $"{number}_{VehicleManager.Vehicles[number].KeyNum}"));
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили ключ от машины с номером {number}", 3000);
                    return;
                case 1:
                    Vehicle result = null;

                        foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                            if (veh.NumberPlate == number)
                            {
                                result = veh;
                                break;
                            }

                    if (result == null) return;

                    foreach (Player ply in NAPI.Pools.GetAllPlayers())
                        if (ply.IsInVehicle && player.Vehicle == result)
                            return;

                    if (org.VehiclesInGarage.Contains(result))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Машина не нуждается в эвакуации", 3000);
                        return;
                    }
                    else if (org.VehiclesOut.Contains(result))
                    {
                        if (org.VehiclesInGarage.Contains(result) || !org.VehiclesOut.Contains(result)) return;
                        org.SpawnCar(result.NumberPlate);
                        org.VehiclesOut.Remove(result);
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                result.Delete();
                            }
                            catch { }
                        }); 

                        VehicleManager.Vehicles[number].Position = null;
                        VehicleManager.Save(number);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваш транспорт был эвакуирован в гараж", 3000);
                    }
                    return;
                case 2:
                    int money = BCore.GetVipCost(player, BCore.CostForCar(VehicleManager.Vehicles[number].Model));
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш транспорт продан за {money}$!", 3000);
                    MoneySystem.Wallet.Change(player, money);
                    VehicleManager.Remove(number);
                    foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                        if (veh.NumberPlate == number)
                        {
                            if (org.VehiclesInGarage.Contains(veh))
                                org.VehiclesInGarage.Remove(veh);
                            else if (org.VehiclesOut.Contains(veh))
                                org.VehiclesOut.Remove(veh);
                            NAPI.Task.Run(() =>
                            {
                                try
                                {
                                    veh.Delete();
                                }
                                catch { }
                            });
                                break;
                        }
                    return;
                
            }

            }
            catch (Exception e) { Log.Write("callcars " + e.ToString(), nLog.Type.Error); }


        }



    }
}
