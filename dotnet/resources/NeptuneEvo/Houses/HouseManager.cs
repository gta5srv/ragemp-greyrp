using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using System.Linq;
using System.Data;
using NeptuneEVO.GUI;
using NeptuneEVO.Core.nAccount;
using NeptuneEVO.Businesses;

namespace NeptuneEVO.Houses
{
    #region HouseType Class
    public class HouseType
    {
        public string Name { get; }
        public Vector3 Position { get; }
        public string IPL { get; set; }
        public Vector3 PetPosition { get; }
        public float PetRotation { get; }

        public HouseType(string name, Vector3 position, Vector3 petpos, float rotation, string ipl = "")
        {
            Name = name;
            Position = position;
            IPL = ipl;
            PetPosition = petpos;
            PetRotation = rotation;
        }

        public void Create()
        {
            if (IPL != "") NAPI.World.RequestIpl(IPL);
        }
    }
    #endregion

    #region House Class
    class House
    {
        public int ID { get; }
        public string Owner { get; private set; }
        public int Type { get; private set; }
        public Vector3 Position { get; }
        public int Price { get; set; }
        public bool Locked { get; private set; }
        public int GarageID { get; set; }
        public int BankID { get; set; }
        public List<string> Roommates { get; set; } = new List<string>();
        public int Apart { get; set; }
        [JsonIgnore] public int Dimension { get; set; }

        [JsonIgnore]
        public Blip blip;
        [JsonIgnore]
        public string PetName;
        [JsonIgnore]
        private TextLabel label;
        [JsonIgnore]
        private ColShape shape;

        [JsonIgnore]
        private ColShape intshape;
        [JsonIgnore]
        private Marker intmarker;

        [JsonIgnore]
        private List<GTANetworkAPI.Object> Objects = new List<GTANetworkAPI.Object>();

        [JsonIgnore]
        private List<Entity> PlayersInside = new List<Entity>();

        public House(int id, string owner, int type, Vector3 position, int price, bool locked, int garageID, int bank, List<string> roommates, int apart)
        {
            ID = id;
            Owner = owner;
            Type = type;
            Position = position;
            Price = price;
            Locked = locked;
            GarageID = garageID;
            BankID = bank;
            Roommates = roommates;
            Apart = apart;

            if (Apart != -1)
            {
                
                Position = Apartments.ApartmentList[Apart].Pos;
                GarageManager.Garages[GarageID].SetPos(Apartments.ApartmentList[Apart].GaragePos, new Vector3(0,0, Apartments.ApartmentList[Apart].Heading));
                return;
            }
            else if (Golemo.Families.Family.FalimyHouses.ContainsKey(ID))
            {
                blip = NAPI.Blip.CreateBlip(84, Position, 0.7f, 12, Golemo.Families.Family.FalimyHouses[ID], 255, 0, true, 0, 0);
            }

                #region Creating Marker & Colshape
                shape = NAPI.ColShape.CreateCylinderColShape(position, 1, 2, 0);
                shape.OnEntityEnterColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "HOUSEID", id);
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 6);
                        Jobs.Gopostal.GoPostal_onEntityEnterColShape(s, ent);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                };
                shape.OnEntityExitColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                        NAPI.Data.ResetEntityData(ent, "HOUSEID");
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
                };
                #endregion

                label = NAPI.TextLabel.CreateTextLabel(Main.StringToU16($"House {id}"), position + new Vector3(0, 0, 1.5), 5f, 0.4f, 0, new Color(255, 255, 255), false, 0);
                UpdateLabel();
        }
        public void UpdateLabel()
        {
            try
            {
                if (Apart == -1)
                label.Text = $"~b~~h~Дом: ~w~№{ID}";

            }
            catch (Exception e)
            {
                //blip.Color = 48;
                Console.WriteLine(ID.ToString() + e.ToString());
            }
        }
        public void CreateAllFurnitures()
        {
            if (FurnitureManager.HouseFurnitures.ContainsKey(ID))
            {
                if (FurnitureManager.HouseFurnitures[ID].Count >= 1)
                {
                    foreach (var f in FurnitureManager.HouseFurnitures[ID].Values) if (f.IsSet) CreateFurniture(f);
                }
            }
        }
        public void CreateFurniture(HouseFurniture f)
        {
            try
            {
                var obj = f.Create((uint)Dimension);
                NAPI.Data.SetEntityData(obj, "HOUSE", ID);
                NAPI.Data.SetEntityData(obj, "ID", f.ID);
                NAPI.Entity.SetEntityDimension(obj, (uint)Dimension);
                if (f.Name == "Оружейный сейф") NAPI.Data.SetEntitySharedData(obj, "TYPE", "WeaponSafe");
                else if (f.Name == "Шкаф с одеждой") NAPI.Data.SetEntitySharedData(obj, "TYPE", "ClothesSafe");
                else if (f.Name == "Шкаф с предметами") NAPI.Data.SetEntitySharedData(obj, "TYPE", "SubjectSafe");
                Objects.Add(obj);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR FURNITURE: " + e.ToString());
            }
        }
        public void DestroyFurnitures()
        {
            try
            {
                foreach (var obj in Objects) NAPI.Entity.DeleteEntity(obj);
                Objects = new List<GTANetworkAPI.Object>();
            }
            catch { }
        }
        public void DestroyFurniture(int id)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    foreach (var obj in Objects)
                    {
                        if (obj.HasData("ID") && obj.GetData<int>("ID") == id)
                        {
                            NAPI.Entity.DeleteEntity(obj);
                            //Log.Debug("HOUSEFURNITURE: deleted " + id);
                            break;
                        }
                    }
                }
                catch { }
            });
        }

        public void UpdateBlip()
        {
            /*if (string.IsNullOrEmpty(Owner))
            {
                blip.Sprite = 374;
                blip.Color = 52;
            }
            else
            {
                blip.Sprite = 40;
                blip.Color = 49;
            }*/
        }
        public void Create()
        {
            MySQL.Query($"INSERT INTO `houses`(`id`,`owner`,`type`,`position`,`price`,`locked`,`garage`,`bank`,`roommates`) " +
                $"VALUES ('{ID}','{Owner}',{Type},'{JsonConvert.SerializeObject(Position)}',{Price},{Locked},{GarageID},{BankID},'{JsonConvert.SerializeObject(Roommates)}')");
        }
        public void Save()
        {
            //MoneySystem.Bank.Save(BankID);
            MySQL.Query($"UPDATE `houses` SET `owner`='{Owner}'," +
                $"`locked`={Locked},`garage`={GarageID},`roommates`='{JsonConvert.SerializeObject(Roommates)}' WHERE `id`='{ID}'");
        }
        public void Destroy()
        {
            RemoveAllPlayers();
            //blip.Delete();
            NAPI.ColShape.DeleteColShape(shape);
            NAPI.ColShape.DeleteColShape(intshape);
            label.Delete();
            intmarker.Delete();
            DestroyFurnitures();
        }
        public void SetLock(bool locked)
        {
            Locked = locked;

            UpdateLabel();
            Save();
        }
        public void SetOwner(Player player)
        {
            GarageManager.Garages[GarageID].DestroyCars();
            Owner = (player == null) ? string.Empty : player.Name;
            NAPI.Task.Run(() => { try { 
            if (blip != null)
                blip.Delete();
                }
                catch { }
            });
            UpdateLabel();
            if (player != null)
            {
                //Trigger.PlayerEvent(player, "changeBlipColor", blip, 73);
                if (Apart == -1)
                {
                    Trigger.PlayerEvent(player, "createHouseBlip", Position);
                    Trigger.PlayerEvent(player, "createCheckpoint", 333, 27, GarageManager.Garages[GarageID].Position - new Vector3(0, 0, 0.2f), 1, NAPI.GlobalDimension, 0, 86, 214);
                }
                //Trigger.PlayerEvent(player, "createGarageBlip", GarageManager.Garages[GarageID].Position);
                Hotel.MoveOutPlayer(player);

                /*var vehicles = VehicleManager.getAllPlayerVehicles(Owner);
                if (GarageManager.Garages[GarageID].Type != -1)
                    NAPI.Task.Run(() => { try { GarageManager.Garages[GarageID].SpawnCars(vehicles); } catch { } });*/
            }

            foreach (var r in Roommates)
            {
                var roommate = NAPI.Player.GetPlayerFromName(r);
                if (roommate != null)
                {
                    Notify.Send(roommate, NotifyType.Warning, NotifyPosition.BottomCenter, "Вы были выселены из дома", 3000);
					roommate.TriggerEvent("deleteHouseBlip");
                    roommate.TriggerEvent("deleteCheckpoint", 333);
                    roommate.TriggerEvent("deleteGarageBlip");
                }
            }

            Roommates = new List<string>();
            Save();
        }
        public string GaragePlayerExit(Player player)
        {
            var players = NAPI.Pools.GetAllPlayers();
            var online = players.FindAll(p => Roommates.Contains(p.Name) && p.Name != player.Name);

            var owner = NAPI.Player.GetPlayerFromName(Owner);
            if (Roommates.Contains(player.Name) && owner != null && Main.Players.ContainsKey(owner))
                online.Add(owner);

            var garage = GarageManager.Garages[GarageID];
            var number = garage.SendVehiclesInsteadNearest(online, player);

            return number;
        }
        public void SendPlayer(Player player)
        {
            NAPI.Entity.SetEntityPosition(player, HouseManager.HouseTypeList[Type].Position + new Vector3(0, 0, 1.12));
            NAPI.Entity.SetEntityDimension(player, Convert.ToUInt32(Dimension));
            Main.Players[player].InsideHouseID = ID;
            if (HouseManager.HouseTypeList[Type].PetPosition != null)
            {
                if (PetName!=null && PetName!="null") Trigger.PlayerEvent(player, "petinhouse", PetName, HouseManager.HouseTypeList[Type].PetPosition.X, HouseManager.HouseTypeList[Type].PetPosition.Y, HouseManager.HouseTypeList[Type].PetPosition.Z, HouseManager.HouseTypeList[Type].PetRotation, Dimension);
            }
            DestroyFurnitures();
            CreateAllFurnitures();
            if (!PlayersInside.Contains(player)) PlayersInside.Add(player);
        }
        public void RemovePlayer(Player player, bool exit = true)
        {
            if (exit)
            {
                NAPI.Entity.SetEntityPosition(player, Position + new Vector3(0, 0, 1.12));
                NAPI.Entity.SetEntityDimension(player, 0);
            }
            player.ResetData("InvitedHouse_ID");
            Main.Players[player].InsideHouseID = -1;

            if (PlayersInside.Contains(player)) PlayersInside.Remove(player);
        }
        public void RemoveFromList(Player player)
        {
            if (PlayersInside.Contains(player)) PlayersInside.Remove(player);
        }
        public void RemoveAllPlayers(Player requster = null)
        {
            for (int i = PlayersInside.Count - 1; i >= 0; i--)
            {
                Player player = NAPI.Entity.GetEntityFromHandle<Player>(PlayersInside[i]);
                if (requster != null && player == requster) continue;

                if (player != null)
                {
                    NAPI.Entity.SetEntityPosition(player, Position + new Vector3(0, 0, 1.12));
                    NAPI.Entity.SetEntityDimension(player, 0);

                    player.ResetData("InvitedHouse_ID");
                    Main.Players[player].InsideHouseID = -1;
                }

                PlayersInside.RemoveAt(i);
            }
        }
        public void CreateInterior()
        {
            #region Creating Interior ColShape & Marker
            intmarker = NAPI.Marker.CreateMarker(1, HouseManager.HouseTypeList[Type].Position - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220), false, (uint)Dimension);

            intshape = NAPI.ColShape.CreateCylinderColShape(HouseManager.HouseTypeList[Type].Position - new Vector3(0.0, 0.0, 1.0), 2f, 4f, (uint)Dimension);
            intshape.OnEntityEnterColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 7);
                }
                catch (Exception ex) { Console.WriteLine("intshape.OnEntityEnterColShape: " + ex.Message); }
            };

            intshape.OnEntityExitColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                }
                catch (Exception ex) { Console.WriteLine("intshape.OnEntityExitColShape: " + ex.Message); }
            };
            #endregion
        }

        public void changeOwner(string newName)
        {
            Owner = newName;
            this.UpdateLabel();
            this.Save();
        }
    }
    #endregion

    class HouseManager : Script
    {
        public static nLog Log = new nLog("HouseManager");

        public static List<House> Houses = new List<House>();
        public static List<HouseType> HouseTypeList = new List<HouseType>
        {
            // name, position
            new HouseType("Трейлер", new Vector3(1973.124, 3816.065, 32.30873), new Vector3(), 0.0f, "trevorstrailer"),
            new HouseType("Эконом", new Vector3(151.2052, -1008.007, -100.12), new Vector3(), 0.0f, "hei_hw1_blimp_interior_v_motel_mp_milo_"),
            new HouseType("Эконом+", new Vector3(265.9691, -1007.078, -102.0758), new Vector3(), 0.0f, "hei_hw1_blimp_interior_v_studio_lo_milo_"),
            new HouseType("Комфорт", new Vector3(346.6991, -1013.023, -100.3162), new Vector3(349.5223, -994.5601, -99.7562), 264.0f, "hei_hw1_blimp_interior_v_apart_midspaz_milo_"),
            new HouseType("Комфорт+", new Vector3(-31.35483, -594.9686, 78.9109),  new Vector3(-25.42115, -581.4933, 79.12776), 159.84f, "hei_hw1_blimp_interior_32_dlc_apart_high2_new_milo_"),
            new HouseType("Премиум", new Vector3(-17.85757, -589.0983, 88.99482), new Vector3(-38.84652, -578.466, 88.58952), 50.8f, "hei_hw1_blimp_interior_10_dlc_apart_high_new_milo_"),
            new HouseType("Премиум+", new Vector3(-173.9419, 497.8622, 136.5341), new Vector3(-164.9799, 480.7568, 137.1526), 40.0f, "apa_ch2_05e_interior_0_v_mp_stilts_b_milo_"),
            new HouseType("Элитный", new Vector3(-173.9419, 497.8622, 136.5341), new Vector3(-164.9799, 480.7568, 137.1526), 40.0f, "apa_ch2_05e_interior_0_v_mp_stilts_b_milo_"),
        };
        public static List<int> MaxRoommates = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };

        private static int GetUID()
        {
            int newUID = 1;
            while (Houses.FirstOrDefault(h => h.ID == newUID) != null) newUID++;
            return newUID;
        }

        public static int DimensionID = 10000;

        #region Events

        public static void onResourceStart()
        {
            try
            {
                foreach (HouseType house_type in HouseTypeList) house_type.Create();

                var result = MySQL.QueryRead($"SELECT * FROM `houses`");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    /*House house = JsonConvert.DeserializeObject<House>(Row["data"].ToString());
                    house.Dimension = DimensionID;
                    house.CreateInterior();
                    house.CreateAllFurnitures();

                    Houses.Add(house);
                    DimensionID++;

                    MySQL.Query($"UPDATE houses SET owner='{house.Owner}',type={house.Type},position='{JsonConvert.SerializeObject(house.Position)}',price={house.Price},locked={house.Locked}," +
                        $"garage={house.GarageID},bank={house.BankID},roommates='{JsonConvert.SerializeObject(house.Roommates)}' WHERE id='{house.ID}'");*/

                    try
                    {
                        var id = Convert.ToInt32(Row["id"].ToString());
                        var owner = Convert.ToString(Row["owner"]);
                        var type = Convert.ToInt32(Row["type"]);
                        var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                        var price = Convert.ToInt32(Row["price"]);
                        var locked = Convert.ToBoolean(Row["locked"]);
                        var garage = Convert.ToInt32(Row["garage"]);
                        var bank = Convert.ToInt32(Row["bank"]);
                        var roommates = JsonConvert.DeserializeObject<List<string>>(Row["roommates"].ToString());
                        var apart = Convert.ToInt32(Row["apart"]);

                        House house = new House(id, owner, type, position, price, locked, garage, bank, roommates, apart);
                        house.Dimension = DimensionID;
                        house.CreateInterior();
                        FurnitureManager.Create(id);
                        house.CreateAllFurnitures();

                        Houses.Add(house);
                        DimensionID++;

                    }
                    catch (Exception e)
                    {
                        Log.Write(Row["id"].ToString() + e.ToString(), nLog.Type.Error);
                    }

                }

                NAPI.Object.CreateObject(0x07e08443, new Vector3(-825.2067, -524.3351, -98.62196), new Vector3(0, 0, -109.999962), 255, NAPI.GlobalDimension);
                Log.Write($"Loaded {Houses.Count} houses.", nLog.Type.Success);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static void Event_OnPlayerDeath(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                NAPI.Entity.SetEntityDimension(player, 0);
                RemovePlayerFromHouseList(player);
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }

        public static void Event_OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                RemovePlayerFromHouseList(player);
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        public static void SavingHouses()
        {
            foreach (var h in Houses) h.Save();
            Log.Write("Saved!", nLog.Type.Save);
        }

        [ServerEvent(Event.ResourceStop)]
        public void Event_OnResourceStop()
        {
            try
            {
                SavingHouses();
            }
            catch (Exception e) { Log.Write("ResourceStop: " + e.Message, nLog.Type.Error); }
        }
        #endregion

        #region Methods
        public static House GetHouse(Player player, bool checkOwner = false)
        {
            House house = null;
            foreach (House hou in Houses)
                if (hou.Apart == -1 && hou.Owner == player.Name)
                {
                    house = hou;
                    break;
                }
            if (house != null && house.Apart == -1)
                return house;
            else if (!checkOwner)
            {
                house = null;
                foreach (House hou in Houses)
                    if (hou.Apart == -1 && hou.Roommates.Contains(player.Name))
                    {
                        house = hou;
                        break;
                    }
                return house;
            }
            else
                return null;
        }

        public static House GetApart(Player player, bool checkOwner = false)
        {
            House house = null;
            foreach (House hou in Houses)
                if (hou.Apart != -1 && hou.Owner == player.Name)
                {
                    house = hou;
                    break;
                }
            if (house != null && house.Apart != -1)
                return house;
            else if (!checkOwner )
            {
                house = null;
                foreach (House hou in Houses)
                    if (hou.Apart != -1 && hou.Roommates.Contains(player.Name))
                    {
                        house = hou;
                        break;
                    }
                return house;
            }
            else
                return null;
        }

        public static House GetHouse(string name, bool checkOwner = false)
        {
            House house = Houses.FirstOrDefault(h => h.Owner == name);
            if (house != null && house.Apart == -1)
                return house;
            else if (!checkOwner && house.Apart == -1)
            {
                house = Houses.FirstOrDefault(h => h.Roommates.Contains(name));
                return house;
            }
            else
                return null;
        }

        public static void RemovePlayerFromHouseList(Player player)
        {
            if (Main.Players[player].InsideHouseID != -1)
            {
                House house = Houses.FirstOrDefault(h => h.ID == Main.Players[player].InsideHouseID);
                if (house == null) return;
                house.RemoveFromList(player);
            }
        }

        public static void CheckAndKick(Player player)
        {
            var house = GetHouse(player);
            if (house == null) return;
            if (house.Roommates.Contains(player.Name)) house.Roommates.Remove(player.Name);
        }

        public static void ChangeOwner(string oldName, string newName)
        {
            lock (Houses)
            {
                foreach (House h in Houses)
                {
                    if (h.Owner != oldName) continue;
                    Log.Write($"The house was found! [{h.ID}]");
                    h.changeOwner(newName);
                    h.Save();
                }
            }
        }
        #endregion

        public static void interactPressed(Player player, int id)
        {
            switch (id)
            {
                case 6:
                    {
                        if (player.IsInVehicle) return;
                        if (!player.HasData("HOUSEID")) return;

                        House house = Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
                        if (house == null) return;
                        OpenHouseManager(player, house);
                        return;
                    }
                case 7:
                    {
                        if (Main.Players[player].InsideHouseID == -1) return;

                        House house = Houses.FirstOrDefault(h => h.ID == Main.Players[player].InsideHouseID);
                        if (house == null) return;

                        if (player.HasData("IS_EDITING"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны закончить редактирование", 3000);
                            MenuManager.Close(player);
                            return;
                        }
                        house.RemovePlayer(player);
                        return;
                    }
            }
        }

        public static void OpenHouseManager(Player player, House house)
        {

            List<object> data = new List<object>
                {
                    house.ID,
                    HouseTypeList[house.Type].Name,
                    GarageManager.GarageTypes[GarageManager.Garages[house.GarageID].Type].MaxCars,
                    MaxRoommates[house.Type],
                    house.Price,
                    string.IsNullOrEmpty(house.Owner) ? "Государство" : house.Owner,
                    house.Locked ? "Закрыты" : "Открыты"
                };

            string json = JsonConvert.SerializeObject(data);
            Trigger.PlayerEvent(player, "openhouse", json, Convert.ToInt32(string.IsNullOrEmpty(house.Owner)));

        }

        [RemoteEvent("housecallback")]
        public static void PlayerEvent_CallBack(Player player, int id)
        {
            try
            {
                House house = Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
                if (house == null) return;
                switch (id)
                {
                    case 0:
                        if (!string.IsNullOrEmpty(house.Owner))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В этом доме уже имеется хозяин", 3000);
                            return;
                        }

                        house.SendPlayer(player);
                        break;
                    case 1:
                        if (!string.IsNullOrEmpty(house.Owner))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В этом доме уже имеется хозяин", 3000);
                            return;
                        }

                        if (house.Price > Main.Players[player].Money)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас не хватает средств для покупки дома", 3000);
                            return;
                        }

                        if (GetHouse(player) != null)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете купить больше одного дома", 3000);
                            return;
                        }
                        var vehicles = VehicleManager.getAllPlayerVehicles(player.Name).Count;
                        var maxcars = GarageManager.GarageTypes[GarageManager.Garages[house.GarageID].Type].MaxCars;
                        if (vehicles > maxcars)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Дом, который Вы покупаете, имеет {maxcars} гаражных места, продайте лишние машины", 3000);
                            OpenCarsSellMenu(player);
                            return;
                        }
                        if (HouseTypeList[house.Type].PetPosition != null) house.PetName = Main.Players[player].PetName;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили этот дом, не забудьте внести налог за него в банкомате", 3000);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.Center, $"Не забудьте внести налог за него в банкомате.", 8000);
                        CheckAndKick(player);
                        house.SetLock(true);
                        house.SetOwner(player);
                        house.SendPlayer(player);
                        MoneySystem.Bank.Accounts[house.BankID].Balance = Convert.ToInt32(house.Price / 100 * 0.005) * 2;
                        MoneySystem.Bank.Save(house.BankID);


                        MoneySystem.Wallet.Change(player, -house.Price);

                        var targetVehicles = VehicleManager.getAllPlayerVehicles(player.Name.ToString());
                        var vehicle = "";
                        foreach (var num in targetVehicles)
                        {
                            vehicle = num;
                            break;
                        }


                        foreach (var v in NAPI.Pools.GetAllVehicles())
                        {
                            if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "PERSONAL" && NAPI.Vehicle.GetVehicleNumberPlate(v) == vehicle)
                            {
                                var veh = v;

                                foreach (var ve in NAPI.Pools.GetAllVehicles())
                                    if (ve.Model == NAPI.Util.GetHashKey("flatbed"))
                                        if (ve.HasSharedData("fbAttachVehicle") && ve.GetSharedData<int>("fbAttachVehicle") == v.Id)
                                            return;

                                if (veh == null) return;
                                VehicleManager.Vehicles[vehicle].Fuel = (!veh.HasSharedData("PETROL")) ? VehicleManager.VehicleTank[veh.Class] : veh.GetSharedData<int>("PETROL");
                                NAPI.Entity.DeleteEntity(veh);

                                MoneySystem.Wallet.Change(player, -200);
                                GameLog.Money($"player({Main.Players[player].UUID})", $"server", 200, $"carEvac");
                                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваша машина была отогнана в гараж", 3000);
                                break;
                            }
                        }

                        GameLog.Money($"player({Main.Players[player].UUID})", $"server", house.Price, $"houseBuy({house.ID})");
                        break;
                    case 2:
                        if (house.Locked)
                        {
                            if (Golemo.Families.Family.FalimyHouses.ContainsKey(house.ID))
                                if (Golemo.Families.Family.FamilyNames[Golemo.Families.Family.FalimyHouses[house.ID]] == Main.Players[player].FamilyCID)
                                {
                                    house.SendPlayer(player);
                                    return;
                                }

                            var playerHouse = GetHouse(player);
                            if (playerHouse != null && playerHouse.ID == house.ID)
                                house.SendPlayer(player);
                            else if (player.HasData("InvitedHouse_ID") && player.GetData<int>("InvitedHouse_ID") == house.ID)
                                house.SendPlayer(player);
                            else
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет доступа", 3000);
                        }
                        else
                            house.SendPlayer(player);
                        break;
                    case 3:
                        Garage garage = GarageManager.Garages[house.GarageID];
                        if (garage == null) return;
                        if (garage.Type == -1) return;
                        if (Golemo.Families.Family.FalimyHouses.ContainsKey(house.ID))
                            if (Golemo.Families.Family.FamilyNames[Golemo.Families.Family.FalimyHouses[house.ID]] == Main.Players[player].FamilyCID)
                            {
                                garage.SendPlayer(player);
                                return;
                            }
                        if (house.Owner != player.Name)
                            if (!house.Roommates.Contains(player.Name))
                                return;
                        garage.SendPlayer(player);
                        break;
                }
            }
            catch (Exception e) { Log.Write("Housecallback: " + e.ToString(), nLog.Type.Error); }
            

        }


        #region Menus

        public static void OpenHouseManageMenu(Player player)
        {
            NAPI.Task.Run(() => { 
                House house = HouseManager.GetHouse(player, true);

                House apartament = HouseManager.GetApart(player, true);

                if (apartament != null && player.GetData<bool>("APART"))
                {
                    house = apartament;
                }
                else if (house == null)
                {
                    MenuManager.Close(player);
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет дома", 3000);
                    return;
                }

                Menu menu = new Menu("housemanage", false, false);
                menu.Callback = callback_housemanage;
                menu.SetBackGround("../images/phone/pages/gps.png");

                Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
                menuItem.Text = "Управление домом";
                menu.Add(menuItem);

                menuItem = new Menu.Item("changestate", Menu.MenuItem.Button);
                menuItem.Text = "Открыть/закрыть";
                menu.Add(menuItem);

                menuItem = new Menu.Item("removeall", Menu.MenuItem.Button);
                menuItem.Text = "Выгнать всех";
                menu.Add(menuItem);

                menuItem = new Menu.Item("furniture", Menu.MenuItem.Button);
                menuItem.Text = "Мебель";
                menu.Add(menuItem);

                if (HouseManager.GetHouse(player, true) == null)
                {
                    menuItem = new Menu.Item("cars", Menu.MenuItem.Button);
                    menuItem.Text = "Машины";
                    menu.Add(menuItem);
                }
                else if ( !player.GetData<bool>("APART"))
                {
                    menuItem = new Menu.Item("cars", Menu.MenuItem.Button);
                    menuItem.Text = "Машины";
                    menu.Add(menuItem);
                }

                menuItem = new Menu.Item("roommates", Menu.MenuItem.Button);
                menuItem.Text = "Сожители";
                menu.Add(menuItem);

                menuItem = new Menu.Item("sell", Menu.MenuItem.Button);
                menuItem.Text = $"Продать гос-ву за {Convert.ToInt32(BCore.GetVipCost(player, house.Price))}$";
                menu.Add(menuItem);

                menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn);
                menuItem.Text = "";
                menu.Add(menuItem);

                menu.Open(player);
            }, 200);
        }
        private static void callback_housemanage(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            House house = HouseManager.GetHouse(player, true);

            House apartament = HouseManager.GetApart(player, true);

            if (apartament != null && player.GetData<bool>("APART"))
            {
                house = apartament;
            } 
            else if ( house == null)
            {
                MenuManager.Close(player);
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет дома", 3000);
                return;
            }

            switch (item.ID)
            {
                case "changestate":
                    house.SetLock(!house.Locked);
                    if (house.Locked) Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закрыли дом", 3000);
                    else Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы открыли дом", 3000);
                    return;
                case "removeall":
                    house.RemoveAllPlayers(player);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выселили всех из дома", 3000);
                    return;
                case "furniture":
                    MenuManager.Close(player);
                    OpenFurnitureMenu(player);
                    return;
                case "sell":
                    int price = BCore.GetVipCost(player, house.Price);
                    
                    MenuManager.Close(player);
                    Trigger.PlayerEvent(player, "openDialog", "HOUSE_SELL_TOGOV", $"Вы действительно хотите продать дом за ${price}?");
                    return;
                case "cars":
                    OpenCarsMenu(player);
                    return;
                case "roommates":
                    OpenRoommatesMenu(player);
                    return;
                case "back":
                    MenuManager.Close(player);
                    Main.OpenPlayerMenu(player).Wait();
                    return;
            }
        }
        public static void acceptHouseSellToGov(Player player)
        {
            House house = HouseManager.GetHouse(player, true);

            House apartament = HouseManager.GetApart(player, true);

            if (apartament != null && player.GetData<bool>("APART"))
            {
                house = apartament;
            }
            else if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет дома", 3000);
                return;
            }

            if (Main.Players[player].InsideGarageID != -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны выйти из гаража", 3000);
                return;
            }

            if (Golemo.Families.Family.FalimyHouses.ContainsKey(house.ID))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Нельзя продать семейный дом", 3000);
                return;
            }

            house.RemoveAllPlayers();
            house.SetOwner(null);
            house.PetName = "null";
            if (house.Apart == -1)
                Trigger.PlayerEvent(player, "deleteCheckpoint", 333);
            player.TriggerEvent("deleteHouseBlip");
            if (house.Apart != -1)
                Trigger.PlayerEvent(player, "deleteGarageBlip");
            int price = BCore.GetVipCost(player, house.Price);

            MoneySystem.Wallet.Change(player, price);
            SellCars.RemoveAllVehPly(player);
            GameLog.Money($"server", $"player({Main.Players[player].UUID})", Convert.ToInt32(house.Price * 0.6), $"houseSell({house.ID})");
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали свой дом государству за {price}$", 3000);
        }

        public static void OpenCarsSellMenu(Player player)
        {
            Menu menu = new Menu("carsell", false, false);
            menu.Callback = callback_carsell;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Продажа автомобилей";
            menu.Add(menuItem);
            menu.SetBackGround("../images/phone/pages/gps.png");

            menuItem = new Menu.Item("label", Menu.MenuItem.Card);
            menuItem.Text = "Выберите машину, которую хотите продать";
            menu.Add(menuItem);

            foreach (var v in VehicleManager.getAllPlayerVehicles(player.Name))
            {
                var vData = VehicleManager.Vehicles[v];
                var price = BCore.GetVipCost(player, BCore.CostForCar(vData.Model));
                menuItem = new Menu.Item(v, Menu.MenuItem.Button);
				var cur = "$";
				if (AutoShopI.ProductsList[4].ContainsKey(vData.Model))
				{
					cur = "UP";
				}
                menuItem.Text = $"{ParkManager.GetNormalName(vData.Model)} - {v} ({price}{cur})";
                menu.Add(menuItem);

            }

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_carsell(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (item.ID == "close")
            {
                MenuManager.Close(player);
                return;
            }
            var vData = VehicleManager.Vehicles[item.ID];

            var price = BCore.GetVipCost(player, BCore.CostForCar(vData.Model));

            if (AutoShopI.ProductsList[4].ContainsKey(vData.Model))
            {
                Main.Accounts[player].RedBucks += price;
                MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали {vData.Model} ({item.ID}) за {price} UP", 3000);
            }
            else
            {
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали {vData.Model} ({item.ID}) за {price}$", 3000);
                MoneySystem.Wallet.Change(player, price);
            }

            GameLog.Money($"server", $"player({Main.Players[player].UUID})", price, $"carSell({vData.Model})");
            VehicleManager.Remove(item.ID);
            MenuManager.Close(player);
        }

        public static void OpenFurnitureMenu(Player player)
        {
            try
            {
                House house = null;
                if (player.GetData<bool>("APART"))
                {
                    house = HouseManager.GetApart(player, true);
                }
                else
                {
                    house = HouseManager.GetHouse(player, true);
                }

                Menu menu = new Menu("furnitures", false, false);
                menu.Callback = callback_furniture0;
                menu.SetBackGround("../images/phone/pages/gps.png");

                Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
                menuItem.Text = "Мебель";
                menu.Add(menuItem);

                menuItem = new Menu.Item("buyfurniture", Menu.MenuItem.Button);
                menuItem.Text = "Покупка мебели";
                menu.Add(menuItem);

                menuItem = new Menu.Item("tofurniture", Menu.MenuItem.Button);
                menuItem.Text = "Управление мебелью";
                menu.Add(menuItem);

                menuItem = new Menu.Item("close", Menu.MenuItem.closeBtn);
                menuItem.Text = "";
                menu.Add(menuItem);

                menu.Open(player);
            }
            catch { }
        }

        private static void callback_furniture0(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try
            {
                if (item.ID == "close")
                {
                    MenuManager.Close(player);
                    Main.OpenPlayerMenu(player).Wait();
                    return;
                }
                if (Main.Players[player].InsideHouseID == -1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться дома для этого действия", 3000);
                    MenuManager.Close(player);
                    return;
                }

                House house = null;
                if (player.GetData<bool>("APART"))
                {
                    house = HouseManager.GetApart(player, true);
                }
                else
                {
                    house = HouseManager.GetHouse(player, true);
                }

                if (house.ID != Main.Players[player].InsideHouseID)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться у себя дома для этого действия", 3000);
                    MenuManager.Close(player);
                    return;
                }
                if (item.ID == "tofurniture")
                {
                    if (!FurnitureManager.HouseFurnitures.ContainsKey(house.ID) || FurnitureManager.HouseFurnitures[house.ID].Count() == 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет мебели", 3000);
                        MenuManager.Close(player);
                        return;
                    }
                    Menu nmenu = new Menu("furnitures", false, false);
                    nmenu.Callback = callback_furniture;
                    nmenu.SetBackGround("../images/phone/pages/gps.png");

                    Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
                    menuItem.Text = "Управление мебелью";
                    nmenu.Add(menuItem);

                    menuItem = new Menu.Item("furniture", Menu.MenuItem.List);
                    menuItem.Text = "ID:";
                    var list = new List<string>();
                    foreach (var f in FurnitureManager.HouseFurnitures[house.ID]) list.Add(f.Value.ID.ToString());
                    menuItem.Elements = list;
                    nmenu.Add(menuItem);

                    menuItem = new Menu.Item("sellit", Menu.MenuItem.Button);
                    menuItem.Text = "Продать (350$)";
                    nmenu.Add(menuItem);

                    var furn = FurnitureManager.HouseFurnitures[house.ID][Convert.ToInt32(list[0])];
                    menuItem = new Menu.Item("type", Menu.MenuItem.Card);
                    menuItem.Text = $"Тип: {furn.Name}";
                    nmenu.Add(menuItem);

                    var open = (furn.IsSet) ? "Да" : "Нет";
                    menuItem = new Menu.Item("isSet", Menu.MenuItem.Card);
                    menuItem.Text = $"Установлено: {open}";
                    nmenu.Add(menuItem);

                    menuItem = new Menu.Item("change", Menu.MenuItem.Button);
                    menuItem.Text = "Установить/Убрать";
                    nmenu.Add(menuItem);

                    menuItem = new Menu.Item("close", Menu.MenuItem.Button);
                    menuItem.Text = "Закрыть";
                    nmenu.Add(menuItem);

                    nmenu.Open(player);
                    return;
                }
                else if (item.ID == "buyfurniture")
                {

                    Menu nmenu = new Menu("furnitures", false, false);
                    nmenu.Callback = callback_furniture1;
                    nmenu.SetBackGround("../images/phone/pages/gps.png");

                    Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
                    menuItem.Text = "Покупка мебели";
                    nmenu.Add(menuItem);

                    menuItem = new Menu.Item("buy1", Menu.MenuItem.Button);
                    menuItem.Text = "Оружейный сейф (1000$)";
                    nmenu.Add(menuItem);

                    menuItem = new Menu.Item("buy2", Menu.MenuItem.Button);
                    menuItem.Text = "Шкаф с одеждой (350$)";
                    nmenu.Add(menuItem);

                    menuItem = new Menu.Item("buy3", Menu.MenuItem.Button);
                    menuItem.Text = "Шкаф с предметами (400$)";
                    nmenu.Add(menuItem);

                    menuItem = new Menu.Item("close", Menu.MenuItem.closeBtn);
                    menuItem.Text = "";
                    nmenu.Add(menuItem);

                    nmenu.Open(player);
                }
            }
            catch { }
        }

        private static void callback_furniture1(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try
            {
                if (item.ID == "close")
                {
                    MenuManager.Close(player);
                    Main.OpenPlayerMenu(player).Wait();
                    return;
                }
                if (Main.Players[player].InsideHouseID == -1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться дома для этого действия", 3000);
                    MenuManager.Close(player);
                    return;
                }
                House house = null;
                if (player.GetData<bool>("APART"))
                {
                    house = HouseManager.GetApart(player, true);
                }
                else
                {
                    house = HouseManager.GetHouse(player, true);
                }
                if (house.ID != Main.Players[player].InsideHouseID)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться у себя дома для этого действия", 3000);
                    MenuManager.Close(player);
                    return;
                }
                if (FurnitureManager.HouseFurnitures[house.ID].Count() >= 50)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "В Вашей квартире уже слишком много мебели, продайте что-то", 3000);
                    return;
                }
                if (item.ID == "buy1")
                {
                    if (Main.Players[player].Money < 1000)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно денег на покупку данной мебели.", 3000);
                        return;
                    }
                    MoneySystem.Wallet.Change(player, -1000);
                    FurnitureManager.newFurniture(house.ID, "Оружейный сейф");
                    GameLog.Money("server", $"player({Main.Players[player].UUID})", 1000, $"buyFurn({house.ID} | Оружейный сейф)");
                }
                else if (item.ID == "buy2")
                {
                    if (Main.Players[player].Money < 350)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно денег на покупку данной мебели.", 3000);
                        return;
                    }
                    MoneySystem.Wallet.Change(player, -350);
                    FurnitureManager.newFurniture(house.ID, "Шкаф с одеждой");
                    GameLog.Money("server", $"player({Main.Players[player].UUID})", 350, $"buyFurn({house.ID} | Шкаф с одеждой)");
                }
                else if (item.ID == "buy3")
                {
                    if (Main.Players[player].Money < 400)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно денег на покупку данной мебели.", 3000);
                        return;
                    }
                    MoneySystem.Wallet.Change(player, -400);
                    FurnitureManager.newFurniture(house.ID, "Шкаф с предметами");
                    GameLog.Money("server", $"player({Main.Players[player].UUID})", 400, $"buyFurn({house.ID} | Шкаф с предметами)");
                }
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Поздравляем с успешной покупкой мебели!", 3000);
                MenuManager.Close(player);
            }
            catch { }
        }

        private static void callback_furniture(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try
            {
                if (item.ID == "close")
                {
                    MenuManager.Close(player);
                    Main.OpenPlayerMenu(player).Wait();
                    return;
                }
                if (Main.Players[player].InsideHouseID == -1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться дома для этого действия", 3000);
                    MenuManager.Close(player);
                    return;
                }
                House house = null;
                if (player.GetData<bool>("APART"))
                {
                    house = HouseManager.GetApart(player, true);
                }
                else
                {
                    house = HouseManager.GetHouse(player, true);
                }
                if (house.ID != Main.Players[player].InsideHouseID)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться у себя дома для этого действия", 3000);
                    MenuManager.Close(player);
                    return;
                }
                if (Main.Players[player].InsideHouseID == -1 || Main.Players[player].InsideHouseID != house.ID)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться дома", 3000);
                    MenuManager.Close(player);
                    return;
                }
                if (!FurnitureManager.HouseFurnitures.ContainsKey(house.ID) || FurnitureManager.HouseFurnitures[house.ID].Count() == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет мебели", 3000);
                    MenuManager.Close(player);
                    return;
                }
                int id = Convert.ToInt32(data["1"]["Value"].ToString());
                var f = FurnitureManager.HouseFurnitures[house.ID][id];
                if (item.ID == "sellit")
                {
                    if (f.IsSet)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Уберите мебель перед продажей.", 3000);
                        return;
                    }
                    GameLog.Money($"player({Main.Players[player].UUID})", "server", 80, $"sellFurn({house.ID} | {f.Name})");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно продали {f.Name} за 350$", 3000);
                    house.DestroyFurniture(f.ID);
                    FurnitureManager.HouseFurnitures[house.ID].Remove(id);
                    FurnitureManager.FurnituresItems[house.ID].Remove(id);
                    MoneySystem.Wallet.Change(player, 350);
                    MenuManager.Close(player);
                    return;
                }
                switch (eventName)
                {
                    case "button":
                        switch (f.IsSet)
                        {
                            case true:
                                house.DestroyFurniture(f.ID);
                                f.IsSet = false;
                                menu.Items[4].Text = $"Установлено: Нет";
                                menu.Change(player, 4, menu.Items[4]);
                                return;
                            case false:
                                if (player.HasData("IS_EDITING"))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны закончить редактирование", 3000);
                                    MenuManager.Close(player);
                                    return;
                                }
                                player.SetData("IS_EDITING", true);
                                player.SetData("EDIT_ID", f.ID);
                                Trigger.PlayerEvent(player, "startEditing", f.Model);
                                MenuManager.Close(player);
                                return;
                        }
                    case "listChangeleft":
                    case "listChangeright":

                        menu.Items[3].Text = $"Тип: {f.Name}";
                        menu.Change(player, 3, menu.Items[3]);

                        var open = (f.IsSet) ? "Да" : "Нет";
                        menu.Items[4].Text = $"Установлено: {open}";
                        menu.Change(player, 4, menu.Items[4]);
                        return;
                }
            }
            catch { }
        }

        public static void OpenRoommatesMenu(Player player)
        {
            try
            {
                Menu menu = new Menu("roommates", false, false);
                menu.Callback = callback_roommates;

                Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
                menuItem.Text = "Сожители";
                menu.Add(menuItem);
                menu.SetBackGround("../images/phone/pages/gps.png");

                House house = null;
                if (player.GetData<bool>("APART"))
                {
                    house = HouseManager.GetApart(player, true);
                }
                else
                {
                    house = HouseManager.GetHouse(player, true);
                }

                if (house.Roommates.Count > 0)
                {
                    menuItem = new Menu.Item("label", Menu.MenuItem.Card);
                    menuItem.Text = "Нажмите на имя человека, которого хотите выселить";
                    menu.Add(menuItem);

                    foreach (var p in house.Roommates)
                    {
                        menuItem = new Menu.Item(p, Menu.MenuItem.Button);
                        menuItem.Text = $"{p.Replace('_', ' ')}";
                        menu.Add(menuItem);
                    }
                }
                else
                {
                    menuItem = new Menu.Item("label", Menu.MenuItem.Card);
                    menuItem.Text = "У Вас никто не подселен в дом";
                    menu.Add(menuItem);
                }

                menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn);
                menuItem.Text = "";
                menu.Add(menuItem);

                menu.Open(player);
            }
            catch { }
        }
        private static void callback_roommates(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try
            {
                if (item.ID == "back")
                {
                    MenuManager.Close(player);
                    Main.OpenPlayerMenu(player).Wait();
                    return;
                }

                var mName = item.ID;
                var roomMate = NAPI.Player.GetPlayerFromName(mName);

                House house = null;
                if (player.GetData<bool>("APART"))
                {
                    house = HouseManager.GetApart(player, true);
                }
                else
                {
                    house = HouseManager.GetHouse(player, true);
                }
                if (house.Roommates.Contains(mName)) house.Roommates.Remove(mName);

                house.Save();
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выселили {mName} из своего дома", 3000);
            }
            catch { }
        }

        public static void OpenCarsMenu(Player player)
        {
            try
            {
                Menu menu = new Menu("cars", false, false);
                menu.Callback = callback_cars;

                Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
                menuItem.Text = "Машины";
                menu.Add(menuItem);
                menu.SetBackGround("../images/phone/pages/gps.png");


                foreach (var v in VehicleManager.getAllPlayerVehicles(player.Name))
                {
                    menuItem = new Menu.Item(v, Menu.MenuItem.Button);
                    menuItem.Text = $"{ParkManager.GetNormalName(VehicleManager.Vehicles[v].Model)} <br> Номер: {v} <br> Пробег {Convert.ToInt32( VehicleManager.Vehicles[v].Sell )} км.";
                    menu.Add(menuItem);
                }

                menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn);
                menuItem.Text = "";
                menu.Add(menuItem);

                menu.Open(player);
            }
            catch { }
        }
        private static void callback_cars(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    MenuManager.Close(player);
                    if (item.ID == "back")
                    {
                        MenuManager.Close(player);
                        OpenHouseManageMenu(player);
                        return;
                    }
                    OpenSelectedCarMenu(player, item.ID);
                }
                catch (Exception e) { Log.Write("callback_cars: " + e.Message + e.Message, nLog.Type.Error); }
            });
        }

        public static void OpenSelectedCarMenu(Player player, string number)
        {
            try
            {
                Menu menu = new Menu("selectedcar", false, false);
                menu.Callback = callback_selectedcar;

                var vData = VehicleManager.Vehicles[number];

                House house = null;
                if (player.GetData<bool>("APART"))
                {
                    house = HouseManager.GetApart(player, true);
                }
                else
                {
                    house = HouseManager.GetHouse(player, true);
                }

                var garage = GarageManager.Garages[house.GarageID];
                var check = garage.CheckCar(false, number);
                var check_pos = (string.IsNullOrEmpty(vData.Position)) ? false : true;

                Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
                menuItem.Text = number;
                menu.Add(menuItem);
                menu.SetBackGround("../images/phone/pages/gps.png");

                menuItem = new Menu.Item("model", Menu.MenuItem.Card);
                menuItem.Text = ParkManager.GetNormalName(vData.Model);
                menu.Add(menuItem);

                var vClass = NAPI.Vehicle.GetVehicleClass(NAPI.Util.VehicleNameToModel(vData.Model));

                menuItem = new Menu.Item("repair", Menu.MenuItem.Button);
                menuItem.Text = $"Восстановить {VehicleManager.VehicleRepairPrice[vClass]}$";
                menu.Add(menuItem);

                menuItem = new Menu.Item("key", Menu.MenuItem.Button);
                menuItem.Text = $"Получить дубликат ключа";
                menu.Add(menuItem);

                menuItem = new Menu.Item("changekey", Menu.MenuItem.Button);
                menuItem.Text = $"Сменить замки";
                menu.Add(menuItem);

                    menuItem = new Menu.Item("evac", Menu.MenuItem.Button);
                    menuItem.Text = $"Эвакуировать машину";
                    menu.Add(menuItem);
                if (GarageManager.Garages[house.GarageID].vehiclesOut.ContainsKey(number))
                {
                    menuItem = new Menu.Item("gps", Menu.MenuItem.Button);
                    menuItem.Text = $"Отметить в GPS";
                    menu.Add(menuItem);
                }
                int price = BCore.GetVipCost(player, BCore.CostForCar(vData.Model));
                var cur = "$";
                if (AutoShopI.ProductsList[4].ContainsKey(vData.Model))
                {
                    cur = "UP";
                }
                menuItem = new Menu.Item("sell", Menu.MenuItem.Button);
                menuItem.Text = $"Продать ({price}{cur})";
                menu.Add(menuItem);

                menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn);
                menuItem.Text = "";
                menu.Add(menuItem);

                menu.Open(player);
            }
            catch { }
        }
        private static void callback_selectedcar(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try
            {
                MenuManager.Close(player);


                House house = null;
                if (player.GetData<bool>("APART"))
                {
                    house = HouseManager.GetApart(player, true);
                }
                else
                {
                    house = HouseManager.GetHouse(player, true);
                }

                switch (item.ID)
                {
                    case "sell":
                        player.SetData("CARSELLGOV", menu.Items[0].Text);
                        VehicleManager.VehicleData vData = VehicleManager.Vehicles[menu.Items[0].Text];
                        int price = BCore.GetVipCost(player, BCore.CostForCar(vData.Model));
                        string cur = "$";
                        if (AutoShopI.ProductsList[4].ContainsKey(vData.Model))
                        {
                            cur = "UP";
                        }

                        MenuManager.Close(player);
                        Trigger.PlayerEvent(player, "openDialog", "CAR_SELL_TOGOV", $"Вы действительно хотите продать государству {ParkManager.GetNormalName(vData.Model)} ({menu.Items[0].Text}) за {price}{cur}?");
                        return;
                    case "repair":
                        vData = VehicleManager.Vehicles[menu.Items[0].Text];
                        if (vData.Health > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Машина не нуждается в восстановлении", 3000);
                            return;
                        }

                        var vClass = NAPI.Vehicle.GetVehicleClass(NAPI.Util.VehicleNameToModel(vData.Model));
                        if (!MoneySystem.Wallet.Change(player, -VehicleManager.VehicleRepairPrice[vClass]))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно средств", 3000);
                            return;
                        }
                        vData.Items = new List<nItem>();
                        GameLog.Money($"player({Main.Players[player].UUID})", $"server", VehicleManager.VehicleRepairPrice[vClass], $"carRepair({vData.Model})");
                        vData.Health = 1000;
                        var garage = GarageManager.Garages[house.GarageID];
                        garage.SendVehicleIntoGarage(menu.Items[0].Text);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы восстановили {ParkManager.GetNormalName(vData.Model)} ({menu.Items[0].Text})", 3000);
                        return;
                    case "evac":
                        if (!Main.Players.ContainsKey(player)) return;

                        var number = menu.Items[0].Text;

                        garage = GarageManager.Garages[house.GarageID];
                        var check = garage.CheckCar(false, number);

                        if (!check)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Эта машина стоит в гараже", 3000);
                            return;
                        }
                        if (Main.Players[player].Money < 15)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств (не хватает {15 - Main.Players[player].Money}$)", 3000);
                            return;
                        }

                        var veh = garage.GetOutsideCar(number);

                        if (veh == null) return;
                        if (veh.HasData("PARKCLASS"))
                            veh.GetData<SellCars.VehicleForSell>("PARKCLASS").Destroy(false, false);
                        VehicleManager.Vehicles[number].Fuel = (!veh.HasSharedData("PETROL")) ? VehicleManager.VehicleTank[veh.Class] : veh.GetSharedData<int>("PETROL");
                        NAPI.Entity.DeleteEntity(veh);
                        garage.SendVehicleIntoGarage(number);

                        MoneySystem.Wallet.Change(player, -15);
                        GameLog.Money($"player({Main.Players[player].UUID})", $"server", 15, $"carEvac");
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваша машина была отогнана в гараж", 3000);
                        return;
                    case "evac_pos":
                        if (!Main.Players.ContainsKey(player)) return;

                        number = menu.Items[0].Text;
                        if (string.IsNullOrEmpty(VehicleManager.Vehicles[number].Position))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Машина не нуждается в эвакуации", 3000);
                            return;
                        }

                        VehicleManager.Vehicles[number].Position = null;
                        VehicleManager.Save(number);

                        garage = GarageManager.Garages[house.GarageID];
                        garage.SendVehicleIntoGarage(number);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваша машина была эвакуирована в гараж", 3000);
                        return;
                    case "gps":
                        if (!Main.Players.ContainsKey(player)) return;

                        number = menu.Items[0].Text;
                        garage = GarageManager.Garages[house.GarageID];
                        check = garage.CheckCar(false, number);

                        if (!check)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Эта машина стоит в гараже", 3000);
                            return;
                        }

                        veh = garage.GetOutsideCar(number);
                        if (veh == null) return;

                        Trigger.PlayerEvent(player, "createWaypoint", veh.Position.X, veh.Position.Y);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "В GPS было отмечено расположение Вашей машины", 3000);
                        return;
                    case "key":
                        if (!Main.Players.ContainsKey(player)) return;

                        garage = GarageManager.Garages[house.GarageID];
                        if (garage.Type == -1)
                        {
                            if (player.Position.DistanceTo(garage.Position) > 4)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться около гаража", 3000);
                                return;
                            }
                        }
                        else
                        {
                            if (Main.Players[player].InsideGarageID == -1)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в гараже", 3000);
                                return;
                            }
                        }

                        var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.CarKey));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                            return;
                        }

                        nInventory.Add(player, new nItem(ItemType.CarKey, 1, $"{menu.Items[0].Text}_{VehicleManager.Vehicles[menu.Items[0].Text].KeyNum}"));
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили ключ от машины с номером {menu.Items[0].Text}", 3000);
                        return;
                    case "changekey":
                        if (!Main.Players.ContainsKey(player)) return;



                        garage = GarageManager.Garages[house.GarageID];
                        if (garage.Type == -1)
                        {
                            if (player.Position.DistanceTo(garage.Position) > 4)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться около гаража", 3000);
                                return;
                            }
                        }
                        else
                        {
                            if (Main.Players[player].InsideGarageID == -1)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в гараже", 3000);
                                return;
                            }
                        }

                        if (!MoneySystem.Wallet.Change(player, -100))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Смена замков стоит $100", 3000);
                            return;
                        }

                        VehicleManager.Vehicles[menu.Items[0].Text].KeyNum++;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы сменили замки на машине {menu.Items[0].Text}. Теперь старые ключи не могут быть использованы", 3000);
                        return;
                    case "back":
                        MenuManager.Close(player);
                        Main.OpenPlayerMenu(player).Wait();
                        return;
                }
            }
            catch { }
        }
        #endregion

        #region Commands
        public static void InviteToRoom(Player player, Player guest)
        {
            var house = HouseManager.GetHouse(player, true);

            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет дома", 3000);
                return;
            }

            if (house.Roommates.Count >= MaxRoommates[house.Type])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас в доме проживает максимальное кол-во людей", 3000);
                return;
            }

            if (GetHouse(guest) != null || GetApart(guest) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Человек уже живет в доме", 3000);
                return;
            }

            guest.SetData("ROOM_INVITER", player);
            guest.TriggerEvent("openDialog", "ROOM_INVITE", $"Гражданин ({player.Value}) предложил Вам подселиться к нему");

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили Гражданину ({guest.Value}) подселиться к Вам", 3000);
        }

        public static void InviteToRoomApart(Player player, Player guest)
        {
            var house = HouseManager.GetApart(player, true);

            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет квартиры", 3000);
                return;
            }

            if (house.Roommates.Count >= MaxRoommates[house.Type])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас в квартире проживает максимальное кол-во людей", 3000);
                return;
            }

            if (GetHouse(guest) != null || GetApart(guest) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Человек уже живет в доме", 3000);
                return;
            }

            guest.SetData("ROOM_INVITER", player);
            guest.TriggerEvent("openDialog", "ROOM_INVITE_APART", $"Гражданин ({player.Value}) предложил Вам подселиться к нему");

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили Гражданину ({guest.Value}) подселиться к Вам", 3000);
        }

        public static void acceptRoomInviteApart(Player player)
        {
            Player owner = player.GetData<Player>("ROOM_INVITER");
            if (owner == null || !Main.Players.ContainsKey(owner)) return;

            var house = HouseManager.GetApart(owner, true);

            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет квартиры", 3000);
                return;
            }

            if (house.Roommates.Count >= MaxRoommates[house.Type])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В доме проживает максимальное кол-во людей", 3000);
                return;
            }

            house.Roommates.Add(player.Name);
            //Trigger.PlayerEvent(player, "createCheckpoint", 333, 27, GarageManager.Garages[house.GarageID].Position - new Vector3(0, 0, 0.2f), 1, NAPI.GlobalDimension, 0, 86, 214);
            //Trigger.PlayerEvent(player, "createGarageBlip", GarageManager.Garages[house.GarageID].Position);
            house.Save();

            Notify.Send(owner, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) подселился к Вам", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы подселились к Гражданину ({owner.Value})", 3000);
        }

        public static void acceptRoomInvite(Player player)
        {
            Player owner = player.GetData<Player>("ROOM_INVITER");
            if (owner == null || !Main.Players.ContainsKey(owner)) return;

            var house = HouseManager.GetHouse(owner, true);

            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет дома", 3000);
                return;
            }

            if (house.Roommates.Count >= MaxRoommates[house.Type])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В доме проживает максимальное кол-во людей", 3000);
                return;
            }

            house.Roommates.Add(player.Name);
            Trigger.PlayerEvent(player, "createCheckpoint", 333, 27, GarageManager.Garages[house.GarageID].Position - new Vector3(0, 0, 0.2f), 1, NAPI.GlobalDimension, 0, 86, 214);
            //Trigger.PlayerEvent(player, "createGarageBlip", GarageManager.Garages[house.GarageID].Position);
            house.Save();

            Notify.Send(owner, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) подселился к Вам", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы подселились к Гражданину ({owner.Value})", 3000);
        }

        [Command("cleargarages")]
        public static void CMD_CreateHouse(Player player)
        {
            if (!Group.CanUseCmd(player, "save")) return;

            var list = new List<int>();
            lock (GarageManager.Garages)
            {
                foreach (var g in GarageManager.Garages)
                {
                    var house = Houses.FirstOrDefault(h => h.GarageID == g.Key);
                    if (house == null) list.Add(g.Key);
                }
            }

            foreach (var id in list)
            {
                GarageManager.Garages.Remove(id);
                MySQL.Query($"DELETE FROM `garages` WHERE `id`={id}");
            }
        }

        [Command("createhouse")]
        public static void CMD_CreateHouse(Player player, int type, int price)
        {
            if (!Group.CanUseCmd(player, "save")) return;
            if (type < 0 || type >= HouseTypeList.Count)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Неправильный тип", 3000);
                return;
            }

            var bankId = MoneySystem.Bank.Create(string.Empty, 2, 0);
            House new_house = new House(GetUID(), string.Empty, type, player.Position - new Vector3(0, 0, 1.12), price, false, 0, bankId, new List<string>(), -1);
            DimensionID++;
            new_house.Dimension = DimensionID;
            new_house.Create();
            FurnitureManager.Create(new_house.ID);
            new_house.CreateInterior();

            Houses.Add(new_house);
        }

        [Command("removehouse")]
        public static void CMD_RemoveHouse(Player player, int id)
        {
            if (!Group.CanUseCmd(player, "save")) return;

            House house = Houses.FirstOrDefault(h => h.ID == id);
            if (house == null) return;

            house.Destroy();
            Houses.Remove(house);
            MySQL.Query($"DELETE FROM `houses` WHERE `id`='{house.ID}'");
        }
        [Command("houseis")]
        public static void CMD_HouseIs(Player player)
        {
            if (!Group.CanUseCmd(player, "save")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться на маркере дома", 3000);
                return;
            }
            House house = Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            if (house == null) return;

            NAPI.Chat.SendChatMessageToPlayer(player, $"{player.GetData<int>("HOUSEID")}");
        }
        [Command("housechange")]
        public static void CMD_HouseOwner(Player player, string newOwner)
        {
            if (!Group.CanUseCmd(player, "save")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться на маркере дома", 3000);
                return;
            }
            House house = Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            if (house == null) return;

            house.changeOwner(newOwner);
            SavingHouses();
        }

        [Command("housenewprice")]
        public static void CMD_setHouseNewPrice(Player player, int price)
        {
            if (!Group.CanUseCmd(player, "save")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться на маркере дома", 3000);
                return;
            }

            House house = Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            if (house == null) return;
            house.Price = price;
            house.UpdateLabel();
            house.Save();
        }

        [Command("myguest")]
        public static void CMD_InvitePlayerToHouse(Player player, int id)
        {
            var guest = Main.GetPlayerByID(id);
            if (guest == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин не найден", 3000);
                return;
            }
            if (player.Position.DistanceTo(guest.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы находитесь слишком далеко", 3000);
                return;
            }
            InvitePlayerToHouse(player, guest);
        }

        public static void InvitePlayerToHouse(Player player, Player guest)
        {
            var house = HouseManager.GetHouse(player, true);

            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет дома", 3000);
                return;
            }

            guest.SetData("InvitedHouse_ID", house.ID);
            Notify.Send(guest, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) пригласил Вас в свой дом", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы пригласили Гражданина ({guest.Value}) в свой дом", 3000);
        }

        public static void InvitePlayerToApart(Player player, Player guest)
        {
            var house = HouseManager.GetApart(player, true);

            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет квартиры", 3000);
                return;
            }

            guest.SetData("InvitedHouse_ID", house.ID);
            Notify.Send(guest, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) пригласил Вас в свой дом", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы пригласили Гражданина ({guest.Value}) в свой дом", 3000);
        }

        [Command("sellhouse")]
        public static void CMD_sellHouse(Player player, int id, int price)
        {
            var target = Main.GetPlayerByID(id);
            if (target == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин не найден", 3000);
                return;
            }
            OfferHouseSell(player, target, price);
        }

        public static void OfferHouseSell(Player player, Player target, int price)
        {
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы находитесь слишком далеко от покупателя", 3000);
                return;
            }
            House house = GetHouse(player, true);

            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                return;
            }
            if (Golemo.Families.Family.FalimyHouses.ContainsKey(house.ID))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Нельзя продать семейный дом", 3000);
                return;
            }
            if (GetHouse(target, true) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Гражданина уже есть дом", 3000);
                return;
            }
            if (price > 1000000000 || price < house.Price / 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Слишком большая/маленькая цена", 3000);
                return;
            }
            if (player.Position.DistanceTo(house.Position) > 30)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы находитесь слишком далеко от дома", 3000);
                return;
            }

            target.SetData("HOUSE_SELLER", player);
            target.SetData("HOUSE_PRICE", price);
            Trigger.PlayerEvent(target, "openDialog", "HOUSE_SELL", $"Гражданин ({player.Value}) предложил Вам купить свой дом за ${price}");
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили Гражданину ({target.Value}) купить Ваш дом за {price}$", 3000);
        }

        public static void OfferApartSell(Player player, Player target, int price)
        {
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы находитесь слишком далеко от покупателя", 3000);
                return;
            }
            House house = GetApart(player, true);


            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                return;
            }
            if (GetApart(target, true) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Гражданина уже есть квартира", 3000);
                return;
            }
            if (price > 1000000000 || price < house.Price / 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Слишком большая/маленькая цена", 3000);
                return;
            }
            if (player.Position.DistanceTo(house.Position) > 30)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы находитесь слишком далеко от дома", 3000);
                return;
            }

            target.SetData("HOUSE_SELLER", player);
            target.SetData("HOUSE_PRICE", price);
            Trigger.PlayerEvent(target, "openDialog", "APART_SELL", $"Гражданин ({player.Value}) предложил Вам купить свою квартиру за ${price}");
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили Гражданину ({target.Value}) купить Вашу квартиру за {price}$", 3000);
        }

        public static void acceptApartSell(Player player)
        {
            if (!player.HasData("HOUSE_SELLER") || !Main.Players.ContainsKey(player.GetData<Player>("HOUSE_SELLER"))) return;
            Player seller = player.GetData<Player>("HOUSE_SELLER");

            if (GetApart(player, true) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть квартира", 3000);
                return;
            }

            House house = GetApart(seller, true);

            var price = player.GetData<int>("HOUSE_PRICE");
            if (house == null || house.Owner != seller.Name) return;
            if (!MoneySystem.Wallet.Change(player, -price))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                return;
            }
            CheckAndKick(player);
            MoneySystem.Wallet.Change(seller, price);
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[seller].UUID})", price, $"houseSell({house.ID})");
            seller.TriggerEvent("deleteGarageBlip");
            house.SetOwner(player);
            house.PetName = Main.Players[player].PetName;
            house.Save();

            Notify.Send(seller, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) купил у Вас квартиру", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы купили квартиру у Гражданина ({seller.Value})", 3000);
        }

        public static void acceptHouseSell(Player player)
        {
            if (!player.HasData("HOUSE_SELLER") || !Main.Players.ContainsKey(player.GetData<Player>("HOUSE_SELLER"))) return;
            Player seller = player.GetData<Player>("HOUSE_SELLER");

            if (GetHouse(player, true) != null )
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть дом", 3000);
                return;
            }

            House house = GetHouse(seller, true);

            var price = player.GetData<int>("HOUSE_PRICE");
            if (house == null || house.Owner != seller.Name) return;
            if (!MoneySystem.Wallet.Change(player, -price))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                return;
            }
            CheckAndKick(player);
            MoneySystem.Wallet.Change(seller, price);
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[seller].UUID})", price, $"houseSell({house.ID})");
            seller.TriggerEvent("deleteCheckpoint", 333);
			seller.TriggerEvent("deleteHouseBlip");
            house.SetOwner(player);
            house.PetName = Main.Players[player].PetName;
            house.Save();

            Notify.Send(seller, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) купил у Вас дом", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы купили дом у Гражданина ({seller.Value})", 3000);
        }
        #endregion
    }
}
