using GTANetworkAPI;
using NeptuneEVO.SDK;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using NeptuneEVO.Houses;
using NeptuneEVO;
using NeptuneEVO.MoneySystem;

namespace Golemo.Families
{
    public class Family
    {
        private static readonly nLog Log = new nLog("Family");
        private static readonly Random Rnd = new Random();
        public static Dictionary<int, string> FalimyHouses = new Dictionary<int, string>();
        public static Dictionary<string, string> FamilyNames = new Dictionary<string, string>();

        public string FamilyCID { get; set; }
        public string Name { get; set; }
        public int Leader { get; set; }
        public int FamilyHouse { get; set; }
        public int MaxPlayers { get; set; }
        public string ImageURL { get; set; }
        public string Desc_1 { get; set; }
        public string Desc_2 { get; set; }
        public int Money { get; set; }
        public int Components { get; set; }
        public Dictionary<string, int> Vehicles { get; set; } = new Dictionary<string, int> { };

        public List<Member> Players = new List<Member>();
        public List<Ranks> AllRanks = new List<Ranks>();
        public List<int> MaxUpdates = new List<int>();
        public Family(string familycid, string name, int leader, int house, int maxplayers = 10, string imgurl = null)
        {
            FamilyCID = familycid;
            Name = name;
            Leader = leader;
            FamilyHouse = house;
            if (!FalimyHouses.ContainsKey(house))
            FalimyHouses.Add(house, name);
            if (!FamilyNames.ContainsKey(name))
                FamilyNames.Add(name, familycid);
            MaxPlayers = maxplayers;
            ImageURL = imgurl;
        }

        public void AddComponents(int count)
        {
            try
            {
                if (count > 0 && Components == 15000) return;
                int add = count;
                if (Components + add > 15000)
                    add = 15000 - Components;
                Components += add;
                MySQL.Query($"UPDATE `family` SET `comp`={Components} WHERE `cid`='{FamilyCID}'");
            }
            catch { }
        }

        public void SaveUpdates()
        {
            try
            {
                MySQL.Query($"UPDATE `family` SET `updates`='{Newtonsoft.Json.JsonConvert.SerializeObject(MaxUpdates)}' WHERE `cid`='{FamilyCID}'");
            }
            catch { }
        }

        public void AddMoney( int count)
        {
            try
            {
                Money += count;
                MySQL.Query($"UPDATE `family` SET `money`={Money} WHERE `cid`='{FamilyCID}'");
            }
            catch { }
        }

        public static void GiveMoneyOnJob(Player player, int payment)
        {
            try
            {
                if (string.IsNullOrEmpty(Main.Players[player].FamilyCID) ) return;

                Family fam = GetFamilyToCid(player);
                if (fam != null)
                    fam.AddMoney(Convert.ToInt32( payment / 100 * 10 ));

            }
            catch { }
        }

        public static int CreateFamilies(Player player, string name, int maxpl, string img)
        {
            try
            {
                House pHouse = HouseManager.GetHouse(player, true);
                if (pHouse == null) return 1;
                CharacterData acc = Main.Players[player];
                int price = Manager.CreatePrice + (Manager.Multiplier * (maxpl / 10));
                if (maxpl == 15)
                    price = Manager.CreatePrice;
                if (acc.Money < price) return 2;
                if (acc.LVL < Manager.NeedLVL) return 3;
                if (acc.FamilyCID != "null" && acc.FamilyRank != 0) return 4;
                if (acc.FractionID != 0) return 5;
                //if (!player.HasData("FAMILY_SET")) return 6;


                var cid = GenerateFamilyCid();
                var familyname = name;
                var leader = Main.Players[player].UUID;
                var house = pHouse.ID;
                var maxplayers = maxpl;
                var imgurl = img == null ? "images/avatar.png" : img;

                Family data = new Family(cid, name, leader, house, maxplayers, imgurl);

                MySQL.Query($"INSERT INTO `family`(`cid`, `name`, `house`, `maxplayers`, `leader`, `imgurl`, `vehicles`, `money`, `comp`) VALUES('{cid}', '{familyname}', {house}, {maxplayers}, {leader}, '{imgurl}', '{Newtonsoft.Json.JsonConvert.SerializeObject(data.Vehicles)}', 0, 0)");

                data.AllRanks = Ranks.CreateRanks(cid);
                data.Players.Add(new Member(player.Name.ToString(), familyname, cid, 10, Ranks.GetFamilyRankName(cid, 10)));
                data.Money = 0;
                data.MaxUpdates = new List<int> { 100000, 0, 50, 0, 0 };

                Manager.Families.Add(data);
                acc.FamilyCID = cid;
                acc.FamilyRank = 10;
                //FalimyHouses.Add(pHouse.ID, name);
                Blip blip = NAPI.Blip.CreateBlip(84, pHouse.Position, 0.7f, 12, name, 255, 0, true, 0, 0);
                pHouse.blip = blip;
                NeptuneEVO.MoneySystem.Wallet.Change(player, -price);
                Main.Players[player].Save(player).Wait();
                Member.LoadMembers(player, cid, 10); //пересмотреть
                return 0;
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
                return 0;
            }

        }

        public static void LoadFamilies()
        {
            try
            {
                DataTable result = MySQL.QueryRead("SELECT * FROM `family`");
                if (result == null || result.Rows.Count == 0) return;
                foreach (DataRow Row in result.Rows)
                {
                    var cid = Convert.ToString(Row["cid"]);
                    var name = Convert.ToString(Row["name"]);
                    var leader = Convert.ToInt32(Row["leader"]);
                    var house = Convert.ToInt32(Row["house"]);
                    var maxplayers = Convert.ToInt32(Row["maxplayers"]);
                    var imgurl = Convert.ToString(Row["imgurl"]);
                    var desc_1 = Convert.ToString(Row["desc_1"]);
                    var desc_2 = Convert.ToString(Row["desc_2"]);
                    var money = Convert.ToInt32(Row["money"]);
                    var comp = Convert.ToInt32(Row["comp"]);

                    Family data = new Family(cid, name, leader, house, maxplayers, imgurl);
                    data.AllRanks = Ranks.LoadRanks(cid);
                    data.Vehicles = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(Row["vehicles"].ToString());
                    data.Desc_1 = desc_1;
                    data.Desc_2 = desc_2;
                    data.Money = money;
                    data.Components = comp;
                    data.MaxUpdates = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(Row["updates"].ToString());

                    Manager.Families.Add(data);
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
           
        }

        public static void SaveFamily(Family family)
        {
            try
            {
                if (family == null) return;

                DataTable result = MySQL.QueryRead($"SELECT * FROM `family` WHERE `cid`='{family.FamilyCID}'");
                if (result == null || result.Rows.Count == 0) return;

                MySQL.Query($"UPDATE `family` SET `name`='{family.Name}', `house`={family.FamilyHouse}, `maxplayers`={family.MaxPlayers}, `leader`={family.Leader}, `imgurl`='{family.ImageURL}', `desc_1`='{family.Desc_1}', `desc_2`='{family.Desc_2}', `vehicles`='{Newtonsoft.Json.JsonConvert.SerializeObject(family.Vehicles)}' WHERE `cid`='{family.FamilyCID}'");


                if (FamilyNames.ContainsKey(family.Name))
                {
                    FamilyNames.Remove(family.Name);
                    FamilyNames.Add(family.Name, family.FamilyCID);
                }

                if (Manager.Families.Contains(family))
                {
                    int index = Manager.Families.FindIndex(x => x.FamilyCID == family.FamilyCID);

                    Manager.Families[index] = family;
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }

        public static void DeleteFamily(Family family)
        {
            try
            {
                if (family == null) return;

                Ranks.DeleteRanks(family);
                DataTable result = MySQL.QueryRead($"SELECT * FROM `family` WHERE `cid`='{family.FamilyCID}'");
                if (result == null || result.Rows.Count == 0) return;

                MySQL.Query($"DELETE FROM `family` WHERE `cid`='{family.FamilyCID}'");

                NAPI.Task.Run(() => {
                    try
                    {
                        if (FalimyHouses.ContainsKey(family.FamilyHouse))
                        {
                            House house = null;
                            foreach (House ho in HouseManager.Houses)
                                if (ho.ID == family.FamilyHouse)
                                {
                                    house = ho;
                                    break;
                                }
                            house.blip.Delete();
                            FalimyHouses.Remove(family.FamilyHouse);

                        }
                    }
                    catch { }
                });

                if (Manager.Families.Contains(family))
                {
                    int index = Manager.Families.FindIndex(x => x.FamilyCID == family.FamilyCID);

                    Manager.Families.RemoveAt(index);
                }

            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }

        private static string GenerateFamilyCid()
        {
            try
            {
                string result = "";
                do
                {
                    result += (char)Rnd.Next(0x0041, 0x005A);
                    result += (char)Rnd.Next(0x0030, 0x0039);
                    result += (char)Rnd.Next(0x0041, 0x005A);
                    result += (char)Rnd.Next(0x0030, 0x0039);

                } while (Manager.Families.FindAll(x => x.FamilyCID == result) == null);
                return result;
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
                return null;
            }
        }

        public static Family GetFamilyToCid(Player player)
        {
            try
            {
                CharacterData acc = Main.Players[player];
                if (acc.FamilyCID == null) return null;

                if (Manager.Families.FindAll(x => x.FamilyCID == acc.FamilyCID) != null)
                {
                    return Manager.Families.Find(x => x.FamilyCID == acc.FamilyCID);
                }
                return null;
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
                return null;
            }
        }

        public static Family GetFamilyToCid(string cid)
        {
            try
            {
                if (Manager.Families.FindAll(x => x.FamilyCID == cid) != null)
                {
                    return Manager.Families.Find(x => x.FamilyCID == cid);
                }
                return null;
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
                return null;
            }

        }

        public static string GetFamilyName(Player player)
        {
            try
            {
                CharacterData acc = Main.Players[player];
                if (acc.FamilyCID == null || acc.FamilyCID == "") return null;
                else { return Manager.Families.Find(x => x.FamilyCID == acc.FamilyCID).Name; }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
                return null;
            }
        }

        public static string GetFamilyName(string cid)
        {
            try
            {
                Family family = Manager.Families.Find(x => x.FamilyCID == cid);
                if (family != null) return family.Name;
                else return null;
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
                return null;
            }
        }
    }

}
