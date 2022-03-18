using GTANetworkAPI;
using NeptuneEVO.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using NeptuneEVO.SDK;
using System;

namespace NeptuneEVO.Businesses
{
    class AutoShopI : Script
    {

        private static nLog Log = new nLog("AUTOSHOP");

        private static Dictionary<string, Color> carColors = new Dictionary<string, Color>
        {
            { "Черный", new Color(0, 0, 0) },
            { "Белый", new Color(225, 225, 225) },
            { "Красный", new Color(230, 0, 0) },
            { "Оранжевый", new Color(255, 115, 0) },
            { "Желтый", new Color(240, 240, 0) },
            { "Зеленый", new Color(0, 230, 0) },
            { "Голубой", new Color(0, 205, 255) },
            { "Синий", new Color(0, 0, 230) },
            { "Фиолетовый", new Color(190, 60, 165) },
        };

        public static List<Dictionary<string, int>> ProductsList = new List<Dictionary<string, int>>
        {
            new Dictionary<string, int> {
				{"deluxo", 750000},
            }, // 0
            new Dictionary<string, int> {
				{"Dukes", 1500},
                {"Sultan", 2700},
                {"SultanRS", 3500},
                {"Kuruma", 4000},
                {"Fugitive", 6000},
				{"Tailgater", 7000},
				{"Sentinel", 7500},
				{"F620", 9000},
				{"Schwarzer", 10000},
                {"Exemplar", 15000},
                {"Felon", 17150},
                {"Schafter2", 25900},
                {"Jackal", 35000},
                {"Oracle2", 35400},
                {"Surano", 50000},
                {"Zion", 56300},
                {"Dominator", 60000},
                {"FQ2", 61000},
                {"Gresley", 68400},
                {"Serrano", 68700},
                {"Dubsta", 69490},
                {"Rocoto", 69990},
                {"Cavalcade2", 74600},
				{"XLS", 75200},
                {"Baller2", 80900},
                {"Elegy", 90000},
                {"Banshee", 92500},
				{"Massacro2", 108000},
				{"GP1", 113000},
				
            }, // 1
            new Dictionary<string, int> { 			
                {"Comet2", 120000},
                {"Coquette", 153000},
                {"Ninef", 190000},
                {"Ninef2", 200000},
                {"Jester", 202500},
                {"Elegy2", 222700},
                {"Infernus", 240000 },
                {"Carbonizzare", 271220},
				{"Dubsta2", 273200},
                {"Baller3", 274000},
				{"Huntley", 300000},
				{"Superd", 313500},
                {"Windsor", 314300},
				{"BestiaGTS", 350000},
				{"Banshee2", 455000},
				{"EntityXF", 520000},
                {"Neon", 520000},
                {"Jester2", 520000},
                {"Turismor", 520000},
                {"Penetrator", 520000},
                {"Omnis", 520000},
                {"Reaper", 520000},
                {"Italigtb2", 520000},
                {"Xa21", 520000},
                {"Osiris", 520000},
                {"Pfister811", 520000},
                {"Zentorno", 520000},
                {"Toros", 520000},
            }, // 2
            new Dictionary<string, int> {
                { "Faggio2",1000},
				{ "Sanchez2",2000},
				{ "Enduro",2000},
				{ "PCJ", 3300 },
				{ "Hexer",3500},
				{ "Lectro",5000},
				{ "Nemesis",6000},
				{ "Hakuchou",6780},
				{ "Ruffian",7000},
				{ "Bmx",7200},
				{ "Scorcher",8000},
				{ "BF400",8200},
				{ "CarbonRS",8300},
				{ "Bati", 8300 },
				{ "Double", 8500 },
				{ "Diablous", 8900 },
				{ "Cliffhanger",8900 },
				{ "Akuma",9000 },
				{ "Thrust", 9300 },
				{ "Nightblade",10000 },
				{ "Vindicator",10000 },
				{ "Ratbike",12000 },
				{ "Blazer",14000 },
				{ "Gargoyle",14500 },
				{ "Sanctus", 15000 },
            }, // 3
            new Dictionary<string, int> {
				{ "Tornado3", 699 },
				{ "Tornado4", 999},
				{ "Emperor2", 1499},
                { "Voodoo2", 1799 },
				{ "Regina", 2499 },
				{ "Ingot", 2599 },				
				{ "Emperor", 2699 },
				{ "Picador", 3999 },
				{ "Minivan", 3999 },
                { "Blista2", 3999 },
                { "Manana", 3999 },
                { "Dilettante", 3999 },
                { "Asea", 3999 },
                { "Glendale", 3999 },
                { "Voodoo", 3999 },
                { "Surge", 3999 },
                { "Primo", 3999 },
                { "Stanier", 3999 },
                { "Stratum", 3999 },
                { "Tampa", 3999 },
                { "Prairie", 3999 },
                { "Radi", 3999 },
                { "Blista", 3999 },
                { "Stalion", 3999 },
                { "Asterope", 3999 },
                { "Washington", 3999 },
                { "Premier", 3999 },
                { "Intruder", 3999 },
                { "Ruiner", 3999 },
                { "Oracle", 3999 },
                { "Phoenix", 3999 },
                { "Gauntlet", 3999 },
                { "Buffalo", 3999 },
                { "RancherXL", 3999 },
                { "Seminole", 3999 },
                { "Baller", 3999 },
                { "Landstalker", 3999 },
                { "Cavalcade", 3999 },
                { "BJXL", 3999 },
                { "Patriot", 3999 },
                { "Bison3", 3999 },
                { "Issi2", 3999 },
                { "Panto", 3999 },
            }, // 4
            new Dictionary<string, int> {
				{ "boxville4", 75000 },
                { "nspeedo", 125000 },
                { "mule3", 250000 },
                { "mule4", 350000 },
                { "pounder2", 575000 },
            }, // 5
            new Dictionary<string, int> { // КЕЙСЫ
                {"deluxo", 300000},
            }, // 6
            new Dictionary<string, int> {
                { "microlight", 500000 },
                { "howard", 1500000 },
                { "buzzard2", 2500000 },
                { "seasparrow2", 2700000 },
                { "frogger", 3500000 },
                { "havok", 4500000 },
                { "maverick", 5000000 },
                { "seasparrow", 5500000 },
                { "supervolito", 6000000 },
                { "supervolito2", 8000000 },
                { "swift", 10000000 },
                { "swift2", 12000000 },
                { "volatus", 15000000 },

            } // 7
        };

        [ServerEvent(Event.PlayerDeath)]
        public void onPlayerDeathHandler(Player player, Player entityKiller, uint weapon)
        {
            try {
                if (player.IsInVehicle && player.Vehicle.HasData("ACCESS") && player.Vehicle.GetData<string>("ACCESS") == "TESTDRIVE")
                {
                    endtestdrive(player, player.Vehicle);
                        NAPI.Task.Run(() => { 
                        NAPI.Entity.SetEntityPosition(player, BCore.BizList[player.GetData<int>("MALADOY")].GetPos() + new Vector3(0, 0, 0.5f));
                        Main.Players[player].ExteriorPos = new Vector3();
                        Dimensions.DismissPrivateDimension(player);
                        player.ResetData("IDS");
                        player.ResetData("UP");
                        NAPI.Entity.SetEntityDimension(player, 0);
                        if (player.HasData("ROOMCAR"))
                        {
                            var uveh = player.GetData<Vehicle>("ROOMCAR");
                            uveh.Delete();
                            player.ResetData("ROOMCAR");
                        }
                        Trigger.PlayerEvent(player, "destroyCamera");
                    }, 200);
                }
            }
            catch { }
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public static void onPlayerDissonnectedHandler(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (player.HasData("ROOMCAR"))
                {
                    var uveh = player.GetData<Vehicle>("ROOMCAR");
                    uveh.Delete();
                    player.ResetData("ROOMCAR");
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("createlveh")]
        public static void createveh(Player player, string name, int color1, int color2, int color3, int x, int y, int z)
        {
            try
            {
                if (!player.HasData("MALADOY")) return;
                if (player.HasData("ROOMCAR"))
                {
                    var uveh = player.GetData<Vehicle>("ROOMCAR");
                    uveh.Delete();
                    player.ResetData("ROOMCAR");
                }
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(name);
                Vehicle veh = NAPI.Vehicle.CreateVehicle(vh, new Vector3(x, y, z), new Vector3(0, 0, 150), 0, 0);
                NAPI.Vehicle.SetVehicleCustomSecondaryColor(veh, color1, color2, color3);
                NAPI.Vehicle.SetVehicleCustomPrimaryColor(veh, color1, color2, color3);
                NAPI.Entity.SetEntityDimension(veh, player.Dimension);
                player.SetData("ROOMCAR", veh);
            }
            catch { }
        }

        [RemoteEvent("vehchangecolor")]
        public static void vehchangecolor(Player player, int color1, int color2, int color3)
        {
            try
            {
                if (!player.HasData("MALADOY")) return;
                if (player.HasData("ROOMCAR"))
                {
                    var uveh = player.GetData<Vehicle>("ROOMCAR");
                    NAPI.Vehicle.SetVehicleCustomSecondaryColor(uveh, color1, color2, color3);
                    NAPI.Vehicle.SetVehicleCustomPrimaryColor(uveh, color1, color2, color3);
                }
            }
            catch { }
        }

        public class AirShops : BCore.Bizness
        {
            public static Vector3 CamPosition = new Vector3(-1143.0817, -2863.8562, 18.826023);
            public static Vector3 LookAt = new Vector3(-1146.0817, -2863.8562, 12.826023);
            public static Vector3 CarPoint = new Vector3(-1146.0817, -2863.8562, 12.826023);
            public AirShops(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 25;
                Name = "Воздушный транспорт";
                BlipColor = 4;
                BlipType = 251;
                Range = 2f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                    if (!Main.Players.ContainsKey(player)) return;
                    player.SetData("IDS", 7);
                    player.SetData("MALADOY", ID);
                    EnterAutoShop(player, ProductsList[7], CamPosition, LookAt, CarPoint);
            }
        }

        public class AutoShopTrucks : BCore.Bizness
        {
            public static Vector3 CamPosition = new Vector3(-943.2822, -2098.955, 12.3664);
            public static Vector3 LookAt = new Vector3(-942.2006, -2087.55, 8.179263);
            public static Vector3 CarPoint = new Vector3(-942.2006, -2087.55, 8.179263);
            public AutoShopTrucks(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 23;
                Name = "Грузовой";
                BlipColor = 4;
                BlipType = 67;
                Range = 2f;

                CreateStuff();
                UpdateLabel();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                player.SetData("IDS", 5);
                player.SetData("MALADOY", ID);
                EnterAutoShop(player, ProductsList[5], CamPosition, LookAt, CarPoint);
            }
        }

        public class AutoShopDonate : BCore.Bizness
        {
            public static Vector3 CamPosition = new Vector3(-417.8262, 1191.2859, 331.4177);
            public static Vector3 LookAt = new Vector3(-406.8262, 1185.1859, 324.4177);
            public static Vector3 CarPoint = new Vector3(-407.8262, 1185.2859, 325.4177);
            public AutoShopDonate(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 22;
                Name = "Эксклюзив";
                BlipColor = 4;
                BlipType = 530;
                Range = 2f;

                CreateStuff();
                UpdateLabel();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                player.SetData("IDS", 4);
                player.SetData("MALADOY", ID);
                EnterAutoShop(player, ProductsList[4], CamPosition, LookAt, CarPoint, false);
            }
        }

        public class AutoShopMoto : BCore.Bizness
        {
            public static Vector3 CamPosition = new Vector3(-417.8262, 1191.2859, 331.4177);
            public static Vector3 LookAt = new Vector3(-406.8262, 1185.1859, 324.4177);
            public static Vector3 CarPoint = new Vector3(-407.8262, 1185.2859, 325.4177);
            public AutoShopMoto(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 5;
                Name = "Мотосалон";
                BlipColor = 4;
                BlipType = 522;
                Range = 2f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                player.SetData("IDS", 3);
                player.SetData("MALADOY", ID);
                EnterAutoShop(player, ProductsList[3], CamPosition, LookAt, CarPoint);
            }
        }
        public class AutoShopMiddle : BCore.Bizness
        {
            public static Vector3 CamPosition = new Vector3(-417.8262, 1191.2859, 331.4177);
            public static Vector3 LookAt = new Vector3(-406.8262, 1185.1859, 324.4177);
            public static Vector3 CarPoint = new Vector3(-407.8262, 1185.2859, 325.4177);
            public AutoShopMiddle(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 4;
                Name = "Люкс";
                BlipColor = 4;
                BlipType = 663;
                Range = 2f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                player.SetData("IDS", 2);
                player.SetData("MALADOY", ID);
                EnterAutoShop(player, ProductsList[2], CamPosition, LookAt, CarPoint);
            }
        }
        public class AutoShopEkonom: BCore.Bizness
        {
            public static Vector3 CamPosition = new Vector3(-417.8262, 1191.2859, 331.4177);
            public static Vector3 LookAt = new Vector3(-406.8262, 1185.1859, 324.4177);
            public static Vector3 CarPoint = new Vector3(-407.8262, 1185.2859, 325.4177);
            public AutoShopEkonom(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 3;
                Name = "Эконом";
                BlipColor = 4;
                BlipType = 669;
                Range = 2f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                player.SetData("IDS", 1);
                player.SetData("MALADOY", ID);
                EnterAutoShop(player, ProductsList[1], CamPosition, LookAt, CarPoint);
            }
        }
        public class AutoShopPremium : BCore.Bizness
        {
            public static Vector3 CamPosition = new Vector3(-417.8262, 1191.2859, 331.4177);
            public static Vector3 LookAt = new Vector3(-406.8262, 1185.1859, 324.4177);
            public static Vector3 CarPoint = new Vector3(-407.8262, 1185.2859, 325.4177);
            public AutoShopPremium(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 2;
                Name = "Премиум";
                BlipColor = 4;
                BlipType = 530;
                Range = 2f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                player.SetData("IDS", 0);
                player.SetData("MALADOY", ID);
                EnterAutoShop(player, ProductsList[0], CamPosition, LookAt, CarPoint);
            }
        }

        [RemoteEvent("carroomBuy")]
        public static void Buy(Player player, string vName, string color)
        {
            try
            {
                if (!player.HasData("IDS")) return;
                if (player.GetData<int>("IDS") == 5 )
                {
                    AutoShopI.BuyTruck(player, vName, color);
                    return;
                }
                else if (player.GetData<int>("IDS") == 7)
                {
                    AutoShopI.BuyAir(player, vName, color);
                    return;
                }

                AutoShopI.BuyF(player, vName, color);
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }

        [RemoteEvent("carroomCancel")]
        public static void Cancel(Player player)
        {
            try
            {
                AutoShopI.CancelF(player);
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }

        // OFF TOP //
        public static void EnterAutoShop(Player player, Dictionary<string, int> produ, Vector3 campos, Vector3 angle, Vector3 car, bool donate = true)
        {
            try
            {
                if (NAPI.Player.IsPlayerInAnyVehicle(player)) return;
                Main.Players[player].ExteriorPos = player.Position;
                NAPI.Entity.SetEntityPosition(player, new Vector3(campos.X, campos.Y - 2, campos.Z - 1));
                NAPI.Entity.SetEntityDimension(player, Dimensions.RequestPrivateDimension(player));
                player.SetData("INTERACTIONCHECK", 0);
                Trigger.PlayerEvent(player, "carRoom", campos.X, campos.Y, campos.Z, angle.X, angle.Y, angle.Z);

                List<string> vehnames = new List<string> { };
                var prices = new List<int>();
                foreach (var p in produ)
                { 
                    prices.Add(produ != ProductsList[4] ? (int)Math.Floor(p.Value * 6f) : p.Value); vehnames.Add(p.Key); 
                
                }
                Trigger.PlayerEvent(player, "openAuto", JsonConvert.SerializeObject(vehnames), JsonConvert.SerializeObject(prices), car.X, car.Y, car.Z);
            }
            catch { }
        }

        public static void BuyAir(Player player, string vName, string color)
        {
            try
            {
                int carroom = player.GetData<int>("IDS");
                Dictionary<string, int> products = ProductsList[carroom];
                NAPI.Entity.SetEntityPosition(player, Main.Players[player].ExteriorPos);

                Trigger.PlayerEvent(player, "destroyCamera");
                NAPI.Entity.SetEntityDimension(player, 0);
                Dimensions.DismissPrivateDimension(player);
                Main.Players[player].ExteriorPos = new Vector3();
                player.ResetData("IDS");

                if (player.HasData("ROOMCAR"))
                {
                    var uveh = player.GetData<Vehicle>("ROOMCAR");
                    uveh.Delete();
                    player.ResetData("ROOMCAR");
                }

                if (AirVehicles.getAllAirVehicles(player.Name).Count >= 3)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Нет мест в аэропорту!", 3000);
                    return;
                }


                int cost = (int)Math.Floor(products[vName] * 6f);
                if (Main.Players[player].Money < cost)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }
                MoneySystem.Wallet.Change(player, -cost);

                Utils.QuestsManager.AddQuestProcess(player, 6);

                GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", cost, $"buyCar({vName})");

                AirVehicles.Create(player.Name, vName, carColors[color]);

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы купили {ParkManager.GetNormalName(vName)}", 3000);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"В скором времени транспорт будет доставен в аэропорт", 5000);
            }
            catch { }

        }

        public static void BuyTruck(Player player, string vName, string color)
        {
            try
            {
                int carroom = player.GetData<int>("IDS");
                Dictionary<string, int> products = ProductsList[carroom];
                NAPI.Entity.SetEntityPosition(player, Main.Players[player].ExteriorPos);

                Trigger.PlayerEvent(player, "destroyCamera");
                NAPI.Entity.SetEntityDimension(player, 0);
                Dimensions.DismissPrivateDimension(player);
                Main.Players[player].ExteriorPos = new Vector3();
                player.ResetData("IDS");
                Organization.MaterialsI.MaterialsOrg org = null;

                if (player.HasData("ROOMCAR"))
                {
                    var uveh = player.GetData<Vehicle>("ROOMCAR");
                    uveh.Delete();
                    player.ResetData("ROOMCAR");
                }

                if (string.IsNullOrEmpty(Main.Players[player].Org))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет организации!", 3000);
                    return;
                }
                else
                {
                    org = (Organization.MaterialsI.MaterialsOrg)Organization.OCore.OrgListNAME[Main.Players[player].Org];
                    if (org.Owner != player.Name)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Это не ваша организация!", 3000);
                        return;
                    }
                }
                if (org.GetVehGarage().Count >= org.MaxVehicles)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Нет мест в гараже!", 3000);
                    return;
                }

                int cost = products[vName] * 6;
                if (Main.Players[player].Money < cost)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }
                MoneySystem.Wallet.Change(player, -cost);

                Utils.QuestsManager.AddQuestProcess(player, 6);

                GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", cost, $"buyCar({vName})");

                var vNumber = VehicleManager.Create(org.Name, vName, carColors[color], carColors[color], carColors[color]);
                org.SpawnCar(vNumber);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы купили {vName} с номером {vNumber}", 3000);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"В скором времени грузовик будет доставен на Склад", 5000);

                }
            catch { }
           }

        public static void BuyF(Player player, string vName, string color)
        {
            try
            {
                int carroom = player.GetData<int>("IDS");
                Dictionary<string, int> products = ProductsList[carroom];
                NAPI.Entity.SetEntityPosition(player, Main.Players[player].ExteriorPos);

                Trigger.PlayerEvent(player, "destroyCamera");
                NAPI.Entity.SetEntityDimension(player, 0);
                Dimensions.DismissPrivateDimension(player);
                Main.Players[player].ExteriorPos = new Vector3();
                //player.ResetData("IDS");

                if (player.HasData("ROOMCAR"))
                {
                    var uveh = player.GetData<Vehicle>("ROOMCAR");
                    uveh.Delete();
                    player.ResetData("ROOMCAR");
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


                if (house == null && VehicleManager.getAllPlayerVehicles(player.Name.ToString()).Count > 0)
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

                    if (Main.Players[player].FamilyRank > 0)
                    {
                        Golemo.Families.Family family = Golemo.Families.Family.GetFamilyToCid(player);

                        if (family != null && family.Leader == Main.Players[player].UUID)
                        {
                            var garage = Houses.GarageManager.Garages[house.GarageID];
                            if (VehicleManager.getAllPlayerVehicles(player.Name).Count >= Houses.GarageManager.GarageTypes[garage.Type].MaxCars + family.MaxUpdates[3])
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас максимальное кол-во машин", 3000);
                                return;
                            }
                        }
                        else
                        {
                            var garage = Houses.GarageManager.Garages[house.GarageID];
                            if (VehicleManager.getAllPlayerVehicles(player.Name).Count >= Houses.GarageManager.GarageTypes[garage.Type].MaxCars)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас максимальное кол-во машин", 3000);
                                return;
                            }
                        }

                    }
                    else
                    {
                        var garage = Houses.GarageManager.Garages[house.GarageID];
                        if (VehicleManager.getAllPlayerVehicles(player.Name).Count >= Houses.GarageManager.GarageTypes[garage.Type].MaxCars)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас максимальное кол-во машин", 3000);
                            return;
                        }
                    }
                }

                int cost = products[vName];

                if (Main.Players[player].Money < cost)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }

                if (player.GetData<int>("IDS") == 4)
                {
                    if (Convert.ToInt32((int)Main.Accounts[player].RedBucks) < cost)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                        return;
                    }
                    Main.Accounts[player].RedBucks -= cost;
                    MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");
                }
                else
                {
                    cost = (int)Math.Floor(cost * 6f);
                    if (Main.Players[player].Money < cost)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                        return;
                    }
                    MoneySystem.Wallet.Change(player, -cost);
                }

                GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", cost, $"buyCar({vName})");

                Utils.QuestsManager.AddQuestProcess(player, 6);

                if (house == null)
                {
                    var vNumber = VehicleManager.Create(player.Name, vName, carColors[color], carColors[color], carColors[color]);
                    var vehdata = VehicleManager.Vehicles[vNumber];
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы купили {ParkManager.GetNormalName(vehdata.Model)} с номером {vNumber}", 3000);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваша машина стоит на паркове", 5000);
                    nInventory.Add(player, new nItem(ItemType.CarKey, 1, $"{vNumber}_{VehicleManager.Vehicles[vNumber].KeyNum}"));
                    ParkManager.SpawnCarOnAuto(player, player.GetData<int>("MALADOY"), vNumber);
                }
                else
                {
                    var vNumber = VehicleManager.Create(player.Name, vName, carColors[color], carColors[color], carColors[color]);
                    var vehdata = VehicleManager.Vehicles[vNumber];
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы купили {ParkManager.GetNormalName(vehdata.Model)} с номером {vNumber}", 3000);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваша машина стоит на паркове", 5000);
                    nInventory.Add(player, new nItem(ItemType.CarKey, 1, $"{vNumber}_{VehicleManager.Vehicles[vNumber].KeyNum}"));
                    ParkManager.SpawnCarOnAuto(player, player.GetData<int>("MALADOY"), vNumber);
                }
            }
            catch { }
        }

        public static void CancelF(Player player)
        {
            try
            {
                NAPI.Entity.SetEntityPosition(player, Main.Players[player].ExteriorPos + new Vector3(0, 0, 0.5f));
                Main.Players[player].ExteriorPos = new Vector3();
                Dimensions.DismissPrivateDimension(player);
                player.ResetData("IDS");
                NAPI.Entity.SetEntityDimension(player, 0);
                if (player.HasData("ROOMCAR"))
                {
                    var uveh = player.GetData<Vehicle>("ROOMCAR");
                    uveh.Delete();
                    player.ResetData("ROOMCAR");
                }
                Trigger.PlayerEvent(player, "destroyCamera");
            }
            catch { }
        }

        [RemoteEvent("carromtestdrive")]

        public static void RemoteEvent_carromtestDrive(Player player, string vName, int color1, int color2, int color3)
        {
            try
            {
                var licensec = Main.Players[player].Licenses;

                //NAPI.Entity.SetEntityPositionFrozen(player, false);

                //Main.Players[player].ExteriorPos = new Vector3();
                Dimensions.DismissPrivateDimension(player);
                
                
                NAPI.Entity.SetEntityDimension(player, 0);
                if (!licensec[1])
                {
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            Trigger.PlayerEvent(player, "destroyCamera");
                        }
                        catch { }
                    }, 500);
                    CancelF(player);
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет водительского удостоверения", 3000);
                    return;
                }
                if (ProductsList[7].ContainsKey(vName))
                {
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            Trigger.PlayerEvent(player, "destroyCamera");
                        }
                        catch { }
                    }, 500);
                    CancelF(player);
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На воздушном транспроте запрещён тест драйв", 3000);
                    return;
                }
                NAPI.Entity.SetEntityPosition(player, new Vector3(-843.9716, -172.6021, 40.4));

                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(vName);
                var veh = NAPI.Vehicle.CreateVehicle(vh, new Vector3(-843.9716, -172.6021, 39.4), new Vector3(0, 0, 30), 0, 0);
                NAPI.Vehicle.SetVehicleCustomSecondaryColor(veh, color1, color2, color3);
                NAPI.Vehicle.SetVehicleCustomPrimaryColor(veh, color1, color2, color3);
                VehicleStreaming.SetEngineState(veh, true);
                VehicleStreaming.SetLockStatus(veh, true);

                NAPI.Task.Run(() =>
                {
                    try
                    {
                        player.SetIntoVehicle(veh, 0);
                        Trigger.PlayerEvent(player, "destroyCamera");
                    }
                    catch { }
                }, 1500);
                player.SetData("VEHTEST", veh);
                player.SetData("TEST_TIMER", Timers.StartOnceTask(180000, () => endtestdrive(player, veh)));

                veh.SetData("ACCESS", "TESTDRIVE");
                veh.SetData("OWNER", player);
                veh.SetSharedData("PETROL", 100);
                NAPI.Vehicle.SetVehicleNumberPlate(veh, "DRIVE");
                player.SetData("TESTDRIVESTATE", true);
                


                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"У Вас есть 3 минуты на тест драйв!", 3000);


            }
            catch (Exception e) { Log.Write("CarRoomTestDrive: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "TESTDRIVE")
                {
                    if (player.GetData<bool>("TESTDRIVESTATE") == false) return;
                    endtestdrive(player, player.GetData<Vehicle>("VEHTEST"));
                }
            }
            catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }

        private static void endtestdrive(Player player, Entity veh)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (veh == null) return;
                    if (!player.HasData("IDS")) return;
                    if (player.GetData<bool>("TESTDRIVESTATE") == false) return;
                    if (player.HasData("TEST_TIMER"))
                    {
                        Timers.Stop(player.GetData<string>("TEST_TIMER"));
                        player.ResetData("TEST_TIMER");
                    }
                    player.ResetData("VEHTEST");
                    player.SetData("TESTDRIVESTATE", false);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваш тест драйв закончился!", 3000);
                    NAPI.Entity.DeleteEntity(veh);

                    player.SetData("INTERACTIONCHECK", 0);
                    NAPI.Entity.SetEntityPosition(player, Main.Players[player].ExteriorPos);
                    BCore.BizList[player.GetData<int>("MALADOY")].InteractPress(player);
                    return;
                }
                catch (Exception e) { Log.Write("TEST DRIVE END: " + e.Message, nLog.Type.Error); }
            });
        }

    }

}
