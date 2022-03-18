using System;
using System.Collections.Generic;
using GTANetworkAPI;
using NeptuneEVO.SDK;
using NeptuneEVO.MoneySystem;
using NeptuneEVO.Core;
using Newtonsoft.Json;
using System.Data;
using NeptuneEVO.Businesses;

namespace NeptuneEVO.Organization
{



    class OCore : Script
    {
        private static nLog Log = new nLog("Organizations");
        public static Dictionary<int, Organization> OrgList = new Dictionary<int, Organization> { };
        public static Dictionary<string, Organization> OrgListNAME = new Dictionary<string, Organization> { };
        public static int LastID = -1;

        public static Dictionary<int, string> NormalName = new Dictionary<int, string> {
            {0, "Грузоперевозки" }
        };


        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var result = MySQL.QueryRead($"SELECT * FROM organizations");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB org return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    
                    int id = Convert.ToInt32(Row["id"]);
                    LastID = id;
                    Vector3 pos = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                    Vector3 safepos = JsonConvert.DeserializeObject<Vector3>(Row["enterpos"].ToString());
                    List<string> members = JsonConvert.DeserializeObject<List<string>>(Row["members"].ToString());
                    CreateOrganization(Row["name"].ToString(), Convert.ToInt32(Row["type"]), Row["owner"].ToString(), members, Convert.ToInt32(Row["level"]), pos, safepos, Convert.ToInt32(Row["angle"]), id, Convert.ToInt32(Row["price"]), Convert.ToInt32(Row["money"]));
                }
                new MaterialsI.MaterialsGet(479, 4, new Vector3(1204.363, -3104.118, 4.649538));
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"ORGANIZATIONS\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        [Command("createorg") ]
        public static void CreateORG(Player player, int type, int cost, int lvl=0)
        {
            try { 
            if (!Main.Players.ContainsKey(player)) return;
            if (Main.Players[player].AdminLVL < 9) return;
            LastID++;
            var bankID = MoneySystem.Bank.Create("", 3, 1000);
            CreateOrganization("", type, "Государство", new List<string> { }, lvl, player.Position - new Vector3(0, 0, 1.12f), player.Position - new Vector3(0,0,2f), (int)player.Rotation.Z, LastID, cost, bankID);
            MySQL.Query($"INSERT INTO organizations (owner, type, members, level, position, enterpos, name, angle, price, id, money) " +
                   $"VALUES ('Государство', '{type}', '{JsonConvert.SerializeObject(new List<string> { })}', '{lvl}', '{JsonConvert.SerializeObject(player.Position - new Vector3(0, 0, 1.12f))}', '{JsonConvert.SerializeObject(player.Position - new Vector3(0, 0, 1.12f))}', ' ', '{(int)player.Rotation.Z}', '{cost}', '{LastID}', '{bankID}')");
            player.SendChatMessage("Организация успешна создана!");
            }
            catch (Exception e) { Log.Write("createorg " + e.ToString(), nLog.Type.Error); }
        }

        [Command("safepos")]
        public static void SafeORG(Player player, int id)
        {
            try { 
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < 9) return;
                if (!OrgList.ContainsKey(id)) return;
                MaterialsI.MaterialsOrg org = (MaterialsI.MaterialsOrg)OrgList[id];
                org.SafePosition = player.Position + new Vector3(0,0, 0.3f);
                org.Angle = (int)player.Rotation.Z;
                MySQL.Query($"UPDATE organizations SET enterpos='{JsonConvert.SerializeObject(org.SafePosition)}' WHERE id='{id}'");
                MySQL.Query($"UPDATE organizations SET angle='{JsonConvert.SerializeObject(org.Angle)}' WHERE id='{id}'");
                player.SendChatMessage("Успешна создана позиция для транспорта организации!");
            }
            catch (Exception e) { Log.Write("safepos " + e.ToString(), nLog.Type.Error); }
        }

        [Command("sellorg")]
        public static void SellORG(Player player, int id, int price)
        {
            try { 
                if (!Main.Players.ContainsKey(player)) return;
                if (string.IsNullOrEmpty(Main.Players[player].Org) || OrgListNAME[Main.Players[player].Org].Owner != player.Name) return;
                if (price <= 0) return;
                Player target = Main.GetPlayerByID(id);
                if (!string.IsNullOrEmpty(Main.Players[target].Org)) return;
                if (target == null) return;
                if (target.Position.DistanceTo2D(player.Position) > 10) return;
                if (Main.Players[target].OrgLic != 0)
                {
                    Notify.Send(target, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет лицензии!", 3000);
                    Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"У покупателя нет лицензии!", 3000);
                    return;
                }

                target.SetData("SELLER", player);
                target.SetData("COST", price);
                //Trigger.PlayerEvent(target, "openDialog", "ORG_SELLPLAYER", $"Вы хотите приобрести организацию ({OrgListNAME[Main.Players[player].Org].Name}) за {price}$ ?");
                Trigger.PlayerEvent(target, "openDialog", "ORG_SELLPLAYER", $"Вы хотите приобрести организацию ({OrgListNAME[Main.Players[player].Org].Name}) за {price}$ ?");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы предоложили купить организацию {target.Name}!", 3000);
            }
            catch (Exception e) { Log.Write("sellorg " + e.ToString(), nLog.Type.Error); }
        }

        public static void CreateOrganization(string name, int type, string owner, List<string> members, int level, Vector3 pos, Vector3 safepos, int angles, int id, int price, int bankid)
        {
            try
            {
                Organization data;
                switch (type)
                {
                    case 0:
                        data = new MaterialsI.MaterialsOrg(name, type, owner, members, level, pos, safepos, angles, id, price, bankid);
                        OrgList.Add(id, data);
                        if (owner != "Государство")
                            if (!OrgListNAME.ContainsKey(name))
                                OrgListNAME.Add(name, data);
                        break;
                }
            }
            catch (Exception e) { Log.Write("createorganization " + e.ToString(), nLog.Type.Error); }

}

        public static void changeOwner(string oldName, string newName)
        {
            try
            { 
            List<int> toChange = new List<int>();
            bool members = false;
            lock (OrgList)
            {
                foreach (KeyValuePair<int, Organization> biz in OrgList)
                {
                    Organization org = biz.Value;

                    if (org.Owner == oldName)
                    {
                        toChange.Add(biz.Key);
                        members = false;
                        break;

                    }
                    else if (org.Members.Contains(oldName))
                    {
                        toChange.Add(biz.Key);
                        members = true;
                        break;
                    }

                }
                foreach (int id in toChange)
                {
                        if (members)
                        {
                            OrgList[id].Members.Remove(oldName);
                            OrgList[id].Members.Add(newName);
                        }
                        else
                        {
                            OrgList[id].Owner = newName;
                            MySQL.Query($"UPDATE organizations SET owner='{newName}' WHERE id='{OrgList[id].ID}'");
                        }

                    OrgList[id].UpdateLabel();
                    OrgList[id].Save();
                        
                }
            }
            }
            catch (Exception e) { Log.Write("changeowner " + e.ToString(), nLog.Type.Error); }
        }

        public class Organization
        {
            public string Owner { get; set; }
            protected int CostForOrganization { get; set; } = 10000;
            protected int CostForSellOrganization { get; set; } = 5000;
            public List<string> Members { get; set; } = new List<string> { };
            public string Name { get; set; }
            public int Type { get; set; }
            public int Level { get; set; }
            public int ID { get; set; }
            public int Price { get; set; }
            public int BankID { get; set; }

            [JsonIgnore]
            private TextLabel Label;
            [JsonIgnore]
            private ColShape Shape;
            [JsonIgnore]
            private ColShape ShapeM;
            [JsonIgnore]
            private Blip Blip;
            [JsonIgnore]
            private Marker Marker;
            public Organization(string name, int type, string owner, List<string> members, int level, int id, int price, int bankid)
            {
                Name = name; Type = type; Owner = owner; Level = level; Members = members; ID = id; Price = price; BankID = bankid;
            }
            public void SellForPlayer(Player seller, Player buyeer, int cost)
            {
                try { 

                if (!Main.Players.ContainsKey(seller) || !Main.Players.ContainsKey(buyeer) || Owner != seller.Name) return;
                if (Main.Players[buyeer].Money < cost)
                {
                    Notify.Send(buyeer, NotifyType.Info, NotifyPosition.BottomCenter, $"У покупателя недостаточно средств!", 3000);
                    Notify.Send(buyeer, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств!", 3000);
                    return;
                }
                if (Main.Players[buyeer].Org != "")
                {
                    Notify.Send(buyeer, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас уже есть организация!", 3000);
                    Notify.Send(seller, NotifyType.Info, NotifyPosition.BottomCenter, $"У покупателя уже есть организация!", 3000);
                    return;
                }
                MoneySystem.Wallet.Change(seller, cost);
                MoneySystem.Wallet.Change(buyeer, -cost);
                Main.Players[seller].Org = "";
                MySQL.Query($"UPDATE `characters` SET `org`='' WHERE `uuid`='{Main.Players[seller].UUID}'");
                Main.Players[buyeer].Org = Name;
                MySQL.Query($"UPDATE `characters` SET `org`='{Name}' WHERE `uuid`='{Main.Players[buyeer].UUID}'");
                MySQL.Query($"UPDATE `organizations` SET `owner`='{buyeer.Name}' WHERE `id`='{ID}'");
                Notify.Send(buyeer, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили организацию!", 3000);
                Notify.Send(seller, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали организацию!", 3000);
                UpdateLabel();
                Owner = buyeer.Name;
                }
                catch (Exception e) { Log.Write("sellforplayer " + e.ToString(), nLog.Type.Error); }
            }

            public void CreateStuff(Vector3 pos, Vector3 exitpos, uint dim)
            {
                try { 
                Label = NAPI.TextLabel.CreateTextLabel(Main.StringToU16("ORG"), new Vector3(pos.X, pos.Y, pos.Z + 2.3), 1f + 2f, 0.5F, 0, new Color(255, 255, 255), true, 0);
                Blip = NAPI.Blip.CreateBlip(569, pos, 0.7f, Convert.ToByte(4), Main.StringToU16(NormalName[Type]), 255, 0, true);
                Marker = NAPI.Marker.CreateMarker(27, pos + new Vector3(0, 0, 0.2f), new Vector3(), new Vector3(), 3f, new Color(0, 86, 214, 220), false, 0);
                UpdateLabel();

                Shape = NAPI.ColShape.CreateCylinderColShape(pos, 3f, 10, 0);

                Shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 152);
                        NAPI.Data.SetEntityData(entity, "ORG_NAME", Name);
                        NAPI.Data.SetEntityData(entity, "ORG_ID", ID);
                    }
                    catch (Exception e) { Console.WriteLine("OnEnter: " + e.Message); }
                };
                Shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
                        NAPI.Data.SetEntityData(entity, "ORG_NAME", "");
                        NAPI.Data.SetEntityData(entity, "ORG_ID", -1);
                    }
                    catch (Exception e) { Console.WriteLine("OnExit: " + e.Message); }
                };
                if (exitpos != null)
                {
                    ExitPos(exitpos, dim);
                }
                }
                catch (Exception e) { Log.Write("createstuff " + e.ToString(), nLog.Type.Error); }

            }

            public void ExitPos(Vector3 exitpos, uint dim)
            {
                try { 

                Marker = NAPI.Marker.CreateMarker(1, exitpos - new Vector3(0, 0, 0.7f), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220), false, dim);

                ShapeM = NAPI.ColShape.CreateCylinderColShape(exitpos, 3f, 10, dim);
                ShapeM.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 153);
                        NAPI.Data.SetEntityData(entity, "ORG_NAME", Name);
                        NAPI.Data.SetEntityData(entity, "ORG_ID", ID);
                    }
                    catch (Exception e) { Console.WriteLine("OnEnter: " + e.Message); }
                };
                ShapeM.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
                        NAPI.Data.SetEntityData(entity, "ORG_NAME", "");
                        NAPI.Data.SetEntityData(entity, "ORG_ID", ID);
                    }
                    catch (Exception e) { Console.WriteLine("OnExit: " + e.Message); }
                };

            }
            catch (Exception e) { Log.Write("exitpos " + e.ToString(), nLog.Type.Error); }
    }

            public int GetNalog()
            {
                return Convert.ToInt32(Price / 100 * 0.033);
            }

            public virtual void Enter(Player player) { }

            public virtual void Exit(Player player) { }


            public void UpdateLabel()
            {
                NAPI.Task.Run(() => { 
                try { 
                string text = $"{NormalName[Type]}\n";
                text += $"~b~{Name}";
                text += (Owner == "Государство") ? $"~b~Цена: {Price}$\n" : "";
                //text += $"~b~{Owner}\n";
                //if (Type == 0)
                    //text += $"~b~Мест: ~w~{MaterialsI.GarageList[Type].MaxVehicles}";
                Label.Text = text;
                }
                catch (Exception e) { Log.Write("updatelabel " + e.ToString(), nLog.Type.Error); }
                });
            }

            public void BuyS(Player player, string name)
            {
                try
                { 
                if (!Main.Players.ContainsKey(player)) return;
                if (!string.IsNullOrEmpty(Main.Players[player].Org))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы состоите в организации!", 3000);
                    return;
                }
                if (Main.Players[player].Money < Price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств!", 3000);
                    return;
                }
                foreach (Organization org in OrgList.Values)
                {
                    if (org.Name == name)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Название занято!", 3000);
                        return;
                    }
                }
                // if (Main.Players[player].BizIDs.Count > 0)
                // {
                    // Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас есть бизнес!", 3000);
                    // return;
                // }
                MoneySystem.Wallet.Change(player, -Price);
                Owner = player.Name;
                Name = name;
                Main.Players[player].Org = name;
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы приобрели организацию {NormalName[Type]}!", 3000);
                MySQL.Query($"UPDATE organizations SET owner='{Owner}' WHERE id='{ID}'");
                MySQL.Query($"UPDATE characters SET org='{Name}' WHERE uuid='{Main.Players[player].UUID}'");
                if (!OrgListNAME.ContainsKey(name))
                    OrgListNAME.Add(name, this);

                Save();
                UpdateLabel();
                }
                catch (Exception e) { Log.Write("buys " + e.ToString(), nLog.Type.Error); }
            }

            public void Sell(Player player)
            {
                try { 
                if (!Main.Players.ContainsKey(player)) return;
                if (player.Name != Owner) return;

                if (Type == 0)
                {
                    MaterialsI.MaterialsOrg mat = (MaterialsI.MaterialsOrg)this;
                    List<string> vehs = mat.GetVehGarage();
                    int result = 0;
                    foreach(string number in vehs)
                    {
                        result += (int)Math.Floor(BCore.CostForCar(VehicleManager.Vehicles[number].Model) * 0.7);
                    }
                    if (result > 0)
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш транспорт продан за {result}$!", 3000);
                        MoneySystem.Wallet.Change(player, result);
                    }
                    for (int i = 0; i < vehs.Count; i++)
                    {
                        foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                            if (vehs[i] == veh.NumberPlate)
                            {
                                if (mat.VehiclesInGarage.Contains(veh))
                                    mat.VehiclesInGarage.Remove(veh);
                                else if (mat.VehiclesOut.Contains(veh))
                                    mat.VehiclesOut.Remove(veh);
                                VehicleManager.Remove(veh.NumberPlate);
                                NAPI.Task.Run(() =>
                                {
                                    veh.Delete();
                                });
                            }
                    }
                }

                MySQL.Query($"UPDATE characters SET org='' WHERE uuid='{Main.Players[player].UUID}'");
                MySQL.Query($"UPDATE organizations SET owner='Государство', name='' WHERE id='{ID}'");
                Main.Players[player].Org = "";
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали организацию за {Price * 0.4}$!", 3000);
                MoneySystem.Wallet.Change(player, (int)Math.Floor(Price * 0.4));
                if (OrgListNAME.ContainsKey(Name))
                    OrgListNAME.Remove(Name);
                Members = new List<string> { };

                Owner = "Государство";
                Name = "";
                UpdateLabel();
                }
                catch (Exception e) { Log.Write("sell " + e.ToString(), nLog.Type.Error); }

            }
            public void AddMember(Player player)
            {
                try { 
                    if (!Main.Players.ContainsKey(player)) return;
                    //if (Members.Count >= 4) return;
                    Members.Add(player.Name);
                    Main.Players[player].Org = Name;
                    MySQL.Query($"UPDATE `characters` SET `org`='{Name}' WHERE uuid='{Main.Players[player].UUID}'");
                    Save();
                }
                catch (Exception e) { Log.Write("addmember " + e.ToString(), nLog.Type.Error); }
            }
            public void RemoveMember(string nick)
            {
                try { 
                if (!Members.Contains(nick)) return;
                Members.Remove(nick);
                foreach (Player player in NAPI.Pools.GetAllPlayers())
                    if (player.Name == nick)
                        Main.Players[player].Org = "";
                string[] split = nick.Split("_");
                MySQL.Query($"UPDATE `characters` SET `org`='' WHERE `firstname`='{split[0]}' AND `lastname`='{split[1]}'");
                Save();
                }
                catch (Exception e) { Log.Write("removemember " + e.ToString(), nLog.Type.Error); }

            }

            public void Save()
            {
                try 
                { 
                     MySQL.Query($"UPDATE organizations SET members='{JsonConvert.SerializeObject(Members)}',name='{Name}' WHERE id='{ID}'");
                     MoneySystem.Bank.Save(this.BankID);
                }
                catch (Exception e) { Log.Write("memberscall " + e.ToString(), nLog.Type.Error); }
            }
        }


    }
}
