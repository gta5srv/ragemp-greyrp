using GTANetworkAPI;
using System;
using System.Collections.Generic;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;
using Newtonsoft.Json;
using NeptuneEVO.MoneySystem;
using NeptuneEVO.GUI;
using System.Data;

namespace NeptuneEVO.Businesses
{

    public class BCore : Script
    {
        private static int LastID = 0;
        private static int KD = 0;
        private static nLog Log = new nLog("Business");
        public static Dictionary<int, Bizness> BizList = new Dictionary<int, Bizness> { };

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                 var result = MySQL.QueryRead($"SELECT * FROM businesses");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB biz return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    Vector3 enterpoint = JsonConvert.DeserializeObject<Vector3>(Row["enterpoint"].ToString());
                    Vector3 unloadpoint = JsonConvert.DeserializeObject<Vector3>(Row["unloadpoint"].ToString());


                    DateTime time = DateTime.Now;

                    if (Row["upgrade"].ToString() != "")
                        time = (DateTime)Row["upgrade"];
                    else
                        time = DateTime.Now.AddYears(-1);

                    CreateBusiness(Convert.ToInt32(Row["type"]), Convert.ToInt32(Row["id"]), Row["owner"].ToString(), enterpoint, unloadpoint, Convert.ToInt32(Row["sellprice"]), Convert.ToInt32(Row["mafia"]), Convert.ToInt32(Row["money"]), Convert.ToInt32(Row["materials"]), time);
                    LastID = Convert.ToInt32(Row["id"]);
                }

                Log.Write($"Business work!", nLog.Type.Success);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"BUSINESSES\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static void SaveBusinesses()
        {
            foreach (Bizness biz in BizList.Values)
                biz.Save();
        }


        public static void CreateBusiness(int type, int id, string owner, Vector3 enterpoint, Vector3 unloadpoint, int price, int mafia, int bank, int mat, DateTime upg)
        {
            NAPI.Task.Run(() => { 

            
            Bizness data;

            switch (type)
            {

                case 0:
                    data = new ShopI.Shop(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 1:
                    data = new RefillI.Refill(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 2:
                    data = new AutoShopI.AutoShopPremium(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 3:
                    data = new AutoShopI.AutoShopEkonom(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 4:
                    data = new AutoShopI.AutoShopMiddle(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 5:
                    data = new AutoShopI.AutoShopMoto(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 6:
                    data = new GunShopI.GunShop(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 7:
                    data = new ClothesShopI.ClothesShop(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 8:
                    data = new ShopI.EatShop(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 9:
                    data = new TattooShopI.TattooShop(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 10:
                    data = new BarberShopI.BarberShop(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 11:
                    data = new MaskShopI.MaskShop(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 12:
                    data = new TuningI.Tuning(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 13:
                    data = new CarWashI.CarWash(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 14:
                    data = new PetShopI.PetShop(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 15:
                    data = new CarRepairI.CarRepair(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 16:
                    data = new ShopI.SimShop(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 17:
                    data = new ShopI.FishShop(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 18:
                    data = new ShopI.SellShop(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 19:
                    data = new ShopI.BarShop(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 20:
                    data = new ShopI.AutoShop(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 21:
                    data = new ShopI.HealShop(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 22:
                    data = new AutoShopI.AutoShopDonate(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 23:
                    data = new AutoShopI.AutoShopTrucks(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                case 24:
                    data = new ShopI.Club(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                 case 25:
                    data = new AutoShopI.AirShops(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                 case 26:
                    data = new ShopI.Semena(id, owner, enterpoint, unloadpoint, price, mafia, bank, mat);
                        data.Upgrade = upg;
                        LastID = id;
                    BizList.Add(id, data);
                    break;
                }
                
                KD += 250;
            }, KD);
        }

        [Command("createbiz")]
        public static void CreateBiz(Player player, int price, int type)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < 9) return;
                var pos = player.Position; pos.Z -= 1.12F;
                LastID++;
                var bankID = MoneySystem.Bank.Create("", 3, 1000);
                MySQL.Query($"INSERT INTO businesses (id, owner, sellprice, type, enterpoint, unloadpoint, money, mafia) " +
                    $"VALUES ({LastID}, 'Государство', {price}, {type}, '{JsonConvert.SerializeObject(pos)}', '{JsonConvert.SerializeObject(new Vector3())}', {bankID}, -1)");
                CreateBusiness(type, LastID, "Государство", pos, new Vector3(), price, -1, bankID, 1000, DateTime.Now.AddDays(-1));
                player.SendChatMessage("Успешно создан бизнес!");
            }
            catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); }
        }

        [Command("materialpos")]
        public static void SafeORG(Player player, int id)
        {
            try 
            { 
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < 9) return;
                if (!BizList.ContainsKey(id)) return;
                Bizness biz = BizList[id];
                biz.MaterialsPosition = player.Position - new Vector3(0, 0, 1f);
                MySQL.Query($"UPDATE businesses SET unloadpoint='{JsonConvert.SerializeObject(player.Position - new Vector3(0, 0, 1f))}' WHERE id='{id}'");
                player.SendChatMessage("Успешна создана позиция для материалов!");
            }
            catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); }
        }

        [Command("buybiz")]
        public static void BuyBiz(Player player)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;

                if (!player.HasData("BIZ_ID") || player.GetData<int>("BIZ_ID") == -1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться около бизнеса", 3000);
                    return;
                }
                int id = player.GetData<int>("BIZ_ID");
                Bizness biz = BizList[id];
				if (biz.GetTypes() == 23 || biz.GetTypes() == 26 || biz.Cost == 0) return;
                if (Main.Players[player].BizIDs.Count >= Group.GroupMaxBusinesses[Main.Accounts[player].VipLvl])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете приобрести больше {Main.Players[player].BizIDs.Count} бизнеса", 3000);
                    return;
                }
                if (biz.Owner == "Государство")
                {
                    biz.BuyS(player);
                }
                else if (biz.Owner == player.Name.ToString())
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Этот бизнес принадлежит Вам", 3000);
                    return;
                }
                else if (biz.Owner != player.Name.ToString())
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Этот бизнес принадлежит другому гражданину", 3000);
                    return;
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("sellbiz")]
        public static void CMD_sellBiz(Player player, int id, int price, int bizid)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин с таким ID не найден", 3000);
                    return;
                }
                BCore.sellBusinessCommand(player, Main.GetPlayerByID(id), price, bizid);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("deletebiz")]
        public static void DeleteBiz(Player player)
        {
            try {
                if (player == null || !Main.Players.ContainsKey(player)) return;

                if (!player.HasData("BIZ_ID") || player.GetData<int>("BIZ_ID") == -1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться около бизнеса", 3000);
                    return;
                }
                Bizness biz = BizList[player.GetData<int>("BIZ_ID")];
                biz.Destroy();
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        public static void sellBusinessCommand(Player player, Player target, int price, int id)
        {
            try { 

            if (!Main.Players.ContainsKey(player) || !Main.Players.ContainsKey(target)) return;

            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин слишком далеко", 3000);
                return;
            }

            if (!Main.Players[player].BizIDs.Contains(id))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет бизнеса", 3000);
                return;
            }

            if (Main.Players[target].BizIDs.Count >= Group.GroupMaxBusinesses[Main.Accounts[target].VipLvl])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин купил максимум бизнесов", 3000);
                return;
            }

            var biz = BizList[id];
            if (price < biz.Cost / 2 || price > biz.Cost * 3)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно продать бизнес за такую цену. Укажите цену от {biz.Cost / 2}$ до {biz.Cost * 3}$", 3000);
                return;
            }

            if (Main.Players[target].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Гражданина недостаточно денег", 3000);
                return;
            }

            Trigger.PlayerEvent(target, "openDialog", "BUSINESS_BUY", $"{player.Name} предложил Вам купить {biz.GetName()} за ${price}");
            target.SetData("SELLER", player);
            target.SetData("SELLPRICE", price);
            target.SetData("SELLBIZID", biz.ID);

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили Гражданину ({target.Value}) купить Ваш бизнес за {price}$", 3000);

            }
            catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); }
        }

        public static void acceptBuyBusiness(Player player)
        {
            try { 
            Player seller = player.GetData<Player>("SELLER");
            if (!Main.Players.ContainsKey(seller) || !Main.Players.ContainsKey(player)) return;

            if (player.Position.DistanceTo(seller.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин слишком далеко", 3000);
                return;
            }

            var price = player.GetData<int>("SELLPRICE");
            if (Main.Players[player].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно денег", 3000);
                return;
            }

            Bizness biz = BizList[player.GetData<int>("SELLBIZID")];
            if (!Main.Players[seller].BizIDs.Contains(biz.ID))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Бизнес больше не принадлежит Гражданину", 3000);
                return;
            }

            if (Main.Players[player].BizIDs.Count >= Group.GroupMaxBusinesses[Main.Accounts[player].VipLvl])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас максимальное кол-во бизнесов", 3000);
                return;
            }

            Main.Players[player].BizIDs.Add(biz.ID);
            Main.Players[seller].BizIDs.Remove(biz.ID);

            biz.Owner = player.Name.ToString();
            var split1 = seller.Name.Split('_');
            var split2 = player.Name.Split('_');
            MySQL.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(Main.Players[seller].BizIDs)}' WHERE firstname='{split1[0]}' AND lastname='{split1[1]}'");
            MySQL.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(Main.Players[player].BizIDs)}' WHERE firstname='{split2[0]}' AND lastname='{split2[1]}'");
            MySQL.Query($"UPDATE businesses SET owner='{biz.Owner}' WHERE id='{biz.ID}'");
            biz.UpdateLabel();

            MoneySystem.Wallet.Change(player, -price);
            MoneySystem.Wallet.Change(seller, price);
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[seller].UUID})", price, $"buyBiz({biz.ID})");

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы купили у {seller.Name.Replace('_', ' ')} {biz.GetName()} за {price}$", 3000);
            Notify.Send(seller, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name.Replace('_', ' ')} купил у Вас {biz.GetName()} за {price}$", 3000);
            }
            catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); }
        }

        public static void changeOwner(string oldName, string newName)
        {
            try { 
                List<int> toChange = new List<int>();
                lock (BizList)
                {
                    foreach (KeyValuePair<int, Bizness> biz in BizList)
                    {
                        if (biz.Value.Owner != oldName) continue;
                        Log.Write($"The biz was found! [{biz.Key}]");
                        toChange.Add(biz.Key);
                    }
                    foreach (int id in toChange)
                    {
                        BizList[id].Owner = newName;
                        BizList[id].UpdateLabel();
                        BizList[id].Save();
                    }
                }
            }
            catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); }
        }

        public static Vector3 getNearestBiz(Player player, int type)
        {
            try { 
                Vector3 nearestBiz = new Vector3();
                foreach (var b in BizList)
                {
                    Bizness biz = BizList[b.Key];
                    if (biz.GetTypes() != type) continue;
                    if (nearestBiz == null) nearestBiz = biz.GetPos();
                    if (player.Position.DistanceTo(biz.GetPos()) < player.Position.DistanceTo(nearestBiz))
                        nearestBiz = biz.GetPos();
                }
                return nearestBiz;
            }
            catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); return new Vector3(0,0,0); }
        }

        [ServerEvent(Event.ResourceStop)]
        public void OnResourceStop()
        {
            try
            {
                SaveBusinesses();
            }
            catch (Exception e) { Log.Write("ResourceStop: " + e.Message, nLog.Type.Error); }
        }

        public class Bizness
        {

            protected int Type { get; set; } = -1;
            public int ID { get; set; } = -1;
            protected string Name { get; set; } = "None";
            public string Owner { get; set; } = "Государство";
            protected Vector3 Position { get; set; } = new Vector3();
            public Vector3 MaterialsPosition { get; set; } = new Vector3();
            protected int BlipType { get; set; } = -1;
            protected int BlipColor { get; set; } = -1;
            public Dictionary<string, int> Products { get; set; } = new Dictionary<string, int> { };
            public int Materials { get; set; } = 0;
            protected float Range { get; set; } = 2f;
            public int Cost { get; set; } = -1;
            public int Mafia { get; set; } = -1;
            public int BankID { get; set; } = -1;
            protected float BlipSize { get; set; } = 1;
            public DateTime Upgrade { get; set; } = DateTime.Now.AddYears(-1);
            //Private for destroy
            [JsonIgnore]
            private TextLabel Label;
            [JsonIgnore]
            private TextLabel MafiaLabel;
            [JsonIgnore]
            private ColShape Shape;
            [JsonIgnore]
            private ColShape ShapeM;
            [JsonIgnore]
            private Blip Blip;
            [JsonIgnore]
            private Marker Marker;

            public Bizness(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat)
            {

                try
                {
                    ID = id; Owner = owner; Position = position; MaterialsPosition = matposition; Cost = cost; Mafia = mafia; BankID = bankid; Materials = mat;
                    

                    ShapeM = NAPI.ColShape.CreateCylinderColShape(MaterialsPosition, 5f, 6, 0);
                    ShapeM.SetData("ORDER_ID", ID);
                    ShapeM.OnEntityEnterColShape += (s, entity) =>
                    {
                        try
                        {
                            if (entity.HasData("ORDER_MAT") && entity.GetData<int>("ORDER_MAT") == s.GetData<int>("ORDER_ID"))
                                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 155);
                        }
                        catch (Exception e) { Console.WriteLine("OnEnter: " + e.Message); }
                    };
                    ShapeM.OnEntityExitColShape += (s, entity) =>
                    {
                        try
                        {
                            NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
                        }
                        catch (Exception e) { Console.WriteLine("OnExit: " + e.Message); }
                    };

                }
                catch { };

            }

            public void CreateStuff()
            {
                Label = NAPI.TextLabel.CreateTextLabel(Main.StringToU16("Business"), new Vector3(Position.X, Position.Y, Position.Z + 1.5), Range + 2f, 0.5F, 0, new Color(255, 255, 255), true, 0);
                Blip = NAPI.Blip.CreateBlip(BlipType, Position, BlipSize, Convert.ToByte(BlipColor), Main.StringToU16(Name), 255, 0, true);
                Marker = NAPI.Marker.CreateMarker(27, Position + new Vector3(0, 0, 0.2f), new Vector3(), new Vector3(), Range, new Color(153, 153, 255, 255), false, 0);
                MafiaLabel = NAPI.TextLabel.CreateTextLabel(Main.StringToU16(""), new Vector3(Position.X, Position.Y, Position.Z + 2), 5F, 0.5F, 0, new Color(255, 255, 255), true, 0);
                Shape = NAPI.ColShape.CreateCylinderColShape(Position, Range, 10, 0);

                Shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 30);
                        NAPI.Data.SetEntityData(entity, "BIZ_ID", ID);
                        Trigger.PlayerEvent(entity, "setbizT");
                    }
                    catch (Exception e) { Console.WriteLine("OnEnter: " + e.Message); }
                };
                Shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
                        NAPI.Data.SetEntityData(entity, "BIZ_ID", -1);
                        Trigger.PlayerEvent(entity, "setbizF");
                    }
                    catch (Exception e) { Console.WriteLine("OnExit: " + e.Message); }
                };
                UpdateLabel();
            }

            public int GetTypes()
            {
                return Type;
            }

            public string GetName()
            {
                return this.Name;
            }

            public Vector3 GetPos()
            {
                return this.Position;
            }

            public int GetNalog()
            {
                return Convert.ToInt32(Cost / 100 * 0.005 * 2);
            }

            public int GetNalogMaterials()
            {
                return GetMaxMaterials() / 3 / 8;
            }

            public int GetDay()
            {
                return Materials;
            }

            public int GetMaxMaterials()
            {
                if (DateTime.Now < Upgrade)
                    return (int)Math.Floor(Cost * 0.25);
                else
                    return (int)Math.Floor(Cost * 0.15);
            }

            public int GetBlip()
            {
                return BlipType;
            }



            public void BuyS(Player player)
            {
                try { 
                if (Owner != "Государство") return;
                if (Main.Players[player].Money < Cost)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                    return;
                }
                MoneySystem.Wallet.Change(player, -Cost);
                GameLog.Money($"player({Main.Players[player].UUID})", $"server", Cost, $"buyBiz({ID})");
                Owner = player.Name;
                Main.Players[player].BizIDs.Add(ID);
                string[] split = player.Name.Split("_");
                MySQL.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(Main.Players[player].BizIDs)}' WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                    MySQL.Query($"UPDATE businesses SET owner='{Owner}' WHERE id='{ID}'");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем! Вы купили {Name}, не забудьте внести налог за него в банкомате", 3000);
                UpdateLabel();
                Save();
                }
                catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); }
            }
            public void Sell(Player player)
            {
                try { 
                Owner = "Государство";
                int price = BCore.GetVipCost(player, Cost);
                MoneySystem.Wallet.Change(player, price);
                Main.Players[player].BizIDs.Remove(ID);
                string[] split = player.Name.Split("_");
                MySQL.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(Main.Players[player].BizIDs)}' WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                    MySQL.Query($"UPDATE businesses SET owner='{Owner}' WHERE id='{ID}'");
                    Save();
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали бизнес {Name}({ID}) за {price}$", 3000);
                UpdateLabel();
                }
                catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); }

            }
            public void UpdateLabel()
            {
                NAPI.Task.Run(() => { 
                    try { 
                        string text = $"~b~ID:~w~{ID}\n";
                        if (Type == 1)
                            text += $"~b~Литр:~w~{RefillI.CostForFuel}$\n";
                        Label.Text = text;
                    }
                    catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); }
                });
            }

            public void SetMafia(int mafia)
            {
                Mafia = mafia;
                UpdateLabel();
                Save();
            }

            public void SetProducts(Dictionary<string, int> products)
            {
                try { 
                Products = products;
                }
                catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); }
            }

            public static string GetProductByIndex(Dictionary<string, int> products,int index)
            {
                try { 
                string result = "";
                int i = 0;
                foreach(KeyValuePair<string, int> pairs in products)
                {
                    if (i == index)
                    {
                        result = pairs.Key;
                    }
                    i++;
                }
                return result;
                }
                catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); return ""; }
            }

            public virtual void InteractPress(Player player){}

            public void Destroy()
            {
                NAPI.Task.Run(() =>
                {
                    Label.Delete();
                    MafiaLabel.Delete();
                    Blip.Delete();
                    Shape.Delete();
                    Marker.Delete();
                    MySQL.Query($"DELETE FROM businesses WHERE id={ID}");
                });
            }

            public static ItemType GetItemByName(string name)
            {
                try { 
                int id = 0;
                foreach (KeyValuePair<int, string> pair in nInventory.ItemsNames)
                {
                    if (pair.Value == name) { id = pair.Key; break; }
                }
                return (ItemType)id;
                }
                catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); return 0; }
            }

            public void Save()
            {
                try { 
                    MySQL.Query($"UPDATE businesses SET owner='{Owner}',mafia={Mafia},materials={Materials} WHERE id={ID}");
                }
                catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); }
            }

            

        }

        public static int CostForCar(string name)
        {
            try { 
                Dictionary<string, int> parse = new Dictionary<string, int> { };
                foreach (Dictionary<string, int> noparse in AutoShopI.ProductsList)
                    foreach (KeyValuePair<string, int> keys in noparse)
                        if (!parse.ContainsKey(keys.Key))
                            parse.Add(keys.Key, keys.Value);
                    
                int result = 0;
                if (parse.ContainsKey(name))
                    result = parse[name];


                return AutoShopI.ProductsList[4].ContainsKey( name ) || AutoShopI.ProductsList[5].ContainsKey(name) ? result : (int)Math.Floor( result * 1.3 );
            }
            catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); return 0; }
        }


        public static int GetVipCost(Player player, int cost)
        {
            try { 
            if (!Main.Players.ContainsKey(player)) return 0;
            switch (Main.Accounts[player].VipLvl)
            {
                case 0:
                    return Convert.ToInt32(cost * 0.40);
                case 1:
                    return Convert.ToInt32(cost * 0.50);
                case 2:
                    return Convert.ToInt32(cost * 0.60);
                case 3:
                    return Convert.ToInt32(cost * 0.70);
                case 4:
                    return Convert.ToInt32(cost * 0.80);
                case 6:
                    return Convert.ToInt32(cost * 0.80);
                default:
                    return Convert.ToInt32(cost * 0.40);
            }
            }
            catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); return 0; }
        }

        public static void OpenSelectMenu(Player player)
        {
            try { 
            if (Main.Players[player].BizIDs.Count <= 0) return;

            Menu menu = new Menu("bizmanage", false, false);
            menu.Callback = callback_bizselect;
            menu.SetBackGround("../images/phone/pages/gps.png");

            if (Main.Players[player].BizIDs.Count > 1)
            {
                Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
                menuItem.Text = "Выбор бизнеса";
                menu.Add(menuItem);

                foreach(int ids in Main.Players[player].BizIDs)
                {
                    menuItem = new Menu.Item(ids.ToString(), Menu.MenuItem.Button);
                    menuItem.Text = $"{ BizList[ids].GetName()} - (ID: {ids})";
                    menu.Add(menuItem);
                }

                menuItem = new Menu.Item("close", Menu.MenuItem.Button);
                menuItem.Text = "Закрыть";
                menu.Add(menuItem);

                menu.Open(player);
                return;
            }
            OpenBiznessMenu(player, Main.Players[player].BizIDs[0]);
            player.SetData("SELECTEDBIZ", Main.Players[player].BizIDs[0]);
            }
            catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); }
        }

        private static void callback_bizselect(Player Player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try { 
            switch (item.ID)
            {
                case "close":
                    MenuManager.Close(Player);
                    return;
                default:
                    MenuManager.Close(Player);
                    OpenBiznessMenu(Player, Convert.ToInt32(item.ID));
                    Player.SetData("SELECTEDBIZ", Convert.ToInt32(item.ID));
                    return;
            }
            }
            catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); }
        }


        public static void OpenBiznessMenu(Player player, int id)
        {
            try { 
            if (!Main.Players[player].BizIDs.Contains(id))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас больше нет этого бизнеса", 3000);
                return;
            }

            Menu menu = new Menu("bizmanage", false, false);
            menu.Callback = callback_bizmanage;
            menu.SetBackGround("../images/phone/pages/gps.png");

            BCore.Bizness biz = BCore.BizList[id];

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = $"{biz.GetName()} - (ID: {id})";
            menu.Add(menuItem);

           
			menuItem = new Menu.Item("money", Menu.MenuItem.Card);
            menuItem.Text = $"Баланс: {MoneySystem.Bank.Accounts[biz.BankID].Balance}$";
            menu.Add(menuItem);
			
            menuItem = new Menu.Item("tax", Menu.MenuItem.Card);
            menuItem.Text = $"Налог: {biz.GetNalog() * 24}$ в сутки";
            menu.Add(menuItem);
			
			menuItem = new Menu.Item("materials", Menu.MenuItem.Card);
            menuItem.Text = $"Доход в сутки: {biz.GetDay()}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("materials", Menu.MenuItem.Card);
            menuItem.Text = $"{biz.Materials} мат. из {biz.GetMaxMaterials()} мат. (Материалы = Прибыль)";
            menu.Add(menuItem);

            menuItem = new Menu.Item("sell", Menu.MenuItem.Button);
            menuItem.Text = "Продать бизнес";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
            }
            catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); }
        }

        private static void callback_bizmanage(Player Player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try { 
            switch (item.ID)
            {
                case "sell":
                    MenuManager.Close(Player);
                    OpenBizSellMenu(Player);
                    return;
                case "close":
                        _ = Main.OpenPlayerMenu(Player);
                        
                    return;
            }
            }
            catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); }
        }

        [RemoteEvent("sendbiz")]
        static void SendBiz(Player player, int index)
        {
            try
            {
                if (index < 0 || player == null) return;
                if (Main.Players[player].BizIDs[index] == null) return;
                if (!BCore.BizList.ContainsKey(Main.Players[player].BizIDs[index])) return;

                BCore.BizList[Main.Players[player].BizIDs[index]].Sell(player);
            }
            catch { }
        }

        public static void OpenBizSellMenu(Player player)
        {
            try { 
            Menu menu = new Menu("bizsell", false, false);
            menu.Callback = callback_bizsell;
            menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Продажа";
            menu.Add(menuItem);

            var bizID = player.GetData<int>("SELECTEDBIZ");
            Bizness biz = BizList[bizID];

            int price = BCore.GetVipCost(player, biz.Cost);

            menuItem = new Menu.Item("govsell", Menu.MenuItem.Button);
            menuItem.Text = $"Продать государству (${price})";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
            }
            catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); }
        }
        private static void callback_bizsell(Player Player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try { 
            if (!Player.HasData("SELECTEDBIZ") || !Main.Players[Player].BizIDs.Contains(Player.GetData<int>("SELECTEDBIZ")))
            {
                MenuManager.Close(Player);
                return;
            }

            var bizID = Player.GetData<int>("SELECTEDBIZ");
            Bizness biz = BizList[bizID];
            MenuManager.Close(Player);
            switch (item.ID)
            {
                case "govsell":
                    biz.Sell(Player);
                    return;
                case "back":
                    OpenBiznessMenu(Player, bizID);
                    return;
            }
            }
            catch (Exception e) { Log.Write("ERROR: " + e.ToString(), nLog.Type.Error); }
        }

    }


}
