using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using GTANetworkAPI;
using NeptuneEVO.SDK;
using NeptuneEVO.Houses;
using System.Linq;
using NeptuneEVO;
namespace Golemo.Families
{
    public class Manager : Script
    {
        private static readonly nLog Log = new nLog("FamilyManager");

        public static int CreatePrice = 7000000; //цена, которая требуется, чтобы создать семью
        public static int NeedLVL = 15; //требуемый уровень игрока, для создания семьи
        public static int Multiplier = 1000000; //доплата за каждые 5 участников

        private static ColShape shape;
        //private static Marker intmarker;
        //private static Blip blip;
        private static readonly Vector3 PositionFamilyCreator = new Vector3(-1898.9615, 2054.3718, 139.77972);

        public static List<Family> Families = new List<Family>();
        public static Dictionary<int, Member> AllMembers = new Dictionary<int, Member>();
        public static Dictionary<Player, Member> Members = new Dictionary<Player, Member>();

        [ServerEvent(Event.ResourceStart)]
        public void StartFamilyManager()
        {
            #region Creating Marker & Colshape & Blip
            try
            {
               //s blip = NAPI.Blip.CreateBlip(675, PositionFamilyCreator, 0.7f, 55, "Семьи", shortRange: true, dimension: 0);
                //intmarker = NAPI.Marker.CreateMarker(2, PositionFamilyCreator + new Vector3(0, 0, 0.5), new Vector3(), new Vector3(), 0.5f, new Color(255, 225, 64), false, 0);
                shape = NAPI.ColShape.CreateCylinderColShape(PositionFamilyCreator, 1.5f, 3, 0);
                shape.OnEntityEnterColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 803);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                };
                shape.OnEntityExitColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
                };

            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
            #endregion

            try
            {
                Family.LoadFamilies();
                Components.LoadComp();
                Member.LoadAllMembers();

                if (Families.Count != 0)
                {
                    Log.Write($"Loaded: {Families.Count}", nLog.Type.Info);
                }
                else { Log.Write("not found Families", nLog.Type.Warn); }

            }
            catch (Exception e) { Log.Write("Loaded Error \n" + e.ToString(), nLog.Type.Error); }
        }

        #region Menu FamilyManager
        [RemoteEvent("openfamilymanager")]
        public static void OpenFamilyMenu(Player player)
        {
            try
            {
                CharacterData acc = Main.Players[player];
                Family family = Families.Find(x => x.FamilyCID == acc.FamilyCID);
                List<object> pairs = new List<object>();
                foreach(var pair in NeptuneEVO.Core.VehicleManager.getAllPlayerVehicles(Main.PlayerNames[ family.Leader]))
                    pairs.Add( new string[] { NeptuneEVO.Core.ParkManager.GetNormalName(NeptuneEVO.Core.VehicleManager.Vehicles[pair].Model), pair, Convert.ToString( family.Vehicles != null && family.Vehicles.ContainsKey(pair) ? family.Vehicles[pair] : 0 ) });

                List<object> data = new List<object>()
                {
                    acc.FamilyRank,
                    Ranks.GetFamilyRankName(acc.FamilyCID, acc.FamilyRank),
                    family.MaxPlayers,
                    family.Players.Count,
                    Member.GetOnlineMembers(family),
                    family.ImageURL,
                    family.Name,
                    family.Desc_1,
                    family.Desc_2,
                    family.AllRanks,
                    pairs,
                    family.Money,
                    family.Components
                };
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                NeptuneEVO.Trigger.PlayerEvent(player, "openFamilyMenu", json);
            }
            catch (Exception e) 
            { 
                Log.Write("Loaded Error \n" + e.ToString(), nLog.Type.Error);
            }
        }

        [RemoteEvent("fcSetmoney")]
        static void fcSetMoney(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (string.IsNullOrEmpty(Main.Players[player].FamilyCID)) return;

                NeptuneEVO.Trigger.PlayerEvent(player, "openInput", $"Положить деньги в казну семьи ({Family.GetFamilyToCid(player).Money}$)", "Введите сколько вы хотите положить", 30, "setMfamily");
            }
            catch { }
        }

        [RemoteEvent("fcGetmoney")]
        static void fcGetMoney(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (string.IsNullOrEmpty(Main.Players[player].FamilyCID)) return;
                if (Main.Players[player].FamilyRank < 9)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Снять деньги может только 9 и 10 ранги", 3000);
                    return;
                }

                NeptuneEVO.Trigger.PlayerEvent(player, "openInput", $"Снять деньги с казны семьи ({Family.GetFamilyToCid(player).Money}$)", "Введите сколько вы хотите снять", 30, "getMfamily");
            }
            catch { }
        }

        [RemoteEvent("fcBuyitem")]
        static void fcBuyitem(Player player, int index)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (string.IsNullOrEmpty(Main.Players[player].FamilyCID)) return;
                if (Main.Players[player].FamilyRank != 10)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Приобрести для семьи может только 'Лидер'", 3000);
                    return;
                }

                Family family = Golemo.Families.Family.GetFamilyToCid(player);

                switch(index)
                {
                    case 0:
                        {
                            if (family.Components < 500)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно компонентов", 3000);
                                return;
                            }
                            family.AddComponents(-500);
                            family.MaxUpdates[0] += 150000;
                            family.SaveUpdates();
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы увеличили общак на 150.000$", 3000);
                            return;
                        }
                    case 1:
                        {
                            if (family.Components < 1000)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно компонентов", 3000);
                                return;
                            }
                            family.AddComponents(-1000);
                            family.MaxUpdates[0] += 300000;
                            family.SaveUpdates();
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы увеличили общак на 300.000$", 3000);
                            return;
                        }
                    case 2:
                        {
                            if (family.Components < 2500)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно компонентов", 3000);
                                return;
                            }
                            family.AddComponents(-2500);
                            family.MaxUpdates[0] += 500000;
                            family.SaveUpdates();
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы увеличили общак на 500.000$", 3000);
                            return;
                        }
                    case 4:
                        {
                            if (family.MaxUpdates[1] == 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет склада", 3000);
                                return;
                            }
                            if (family.Components < 350)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно компонентов", 3000);
                                return;
                            }
                            family.AddComponents(-350);
                            family.MaxUpdates[2] += 50;
                            family.SaveUpdates();
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы увеличили склад на 50 слотов", 3000);
                            return;
                        }
                    case 5:
                        {
                            if (family.MaxUpdates[1] == 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет склада", 3000);
                                return;
                            }
                            if (family.Components < 450)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно компонентов", 3000);
                                return;
                            }
                            family.AddComponents(-450);
                            family.MaxUpdates[2] += 70;
                            family.SaveUpdates();
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы увеличили склад на 70 слотов", 3000);
                            return;
                        }
                    case 6:
                        {
                            if (family.MaxUpdates[1] == 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет склада", 3000);
                                return;
                            }
                            if (family.Components < 750)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно компонентов", 3000);
                                return;
                            }
                            family.AddComponents(-750);
                            family.MaxUpdates[2] += 100;
                            family.SaveUpdates();
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы увеличили склад на 100 слотов", 3000);
                            return;
                        }
                    case 7:
                        {
                            if (family.MaxUpdates[3] + 3 > 60 )
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Максимально +60 мест", 3000);
                                return;
                            }
                            if (family.Components < 3500)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно компонентов", 3000);
                                return;
                            }
                            family.AddComponents(-3500);
                            family.MaxUpdates[3] += 3;
                            family.SaveUpdates();
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы увеличили гараж на 3 места", 3000);
                            return;
                        }
                    case 8:
                        {
                            if (family.MaxUpdates[3] + 5 > 60)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Максимально +60 мест", 3000);
                                return;
                            }
                            if (family.Components < 4500)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно компонентов", 3000);
                                return;
                            }
                            family.AddComponents(-4500);
                            family.MaxUpdates[3] += 5;
                            family.SaveUpdates();
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы увеличили гараж на 5 мест", 3000);
                            return;
                        }
                    case 9:
                        {
                            if (family.MaxUpdates[3] + 10 > 60)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Максимально +60 мест", 3000);
                                return;
                            }
                            if (family.Components < 10000)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно компонентов", 3000);
                                return;
                            }
                            family.AddComponents(-10000);
                            family.MaxUpdates[3] += 10;
                            family.SaveUpdates();
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы увеличили гараж на 10 мест", 3000);
                            return;
                        }
                    case 10:
                        {
                            if (family.MaxUpdates[4] == 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас уже есть функция перебивки номеров", 3000);
                                return;
                            }
                            if (family.Components < 10000)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно компонентов", 3000);
                                return;
                            }
                            family.AddComponents(-10000);
                            family.MaxUpdates[4] = 1;
                            family.SaveUpdates();
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы приобрели перебивку номеров, команда: /badnum", 3000);
                            return;
                        }
                    default:
                        return;
                }


            }
            catch { }
        }

        [RemoteEvent("loadfamilymembers")]
        public static void LoadFamilyMembersToMenu(Player player)
        {
            try
            {
                CharacterData acc = Main.Players[player];
                Family family = Families.Find(x => x.FamilyCID == acc.FamilyCID);
                List<object> players = new List<object>();
                for (int i = 0; i < family.Players.Count; i++)
                {
                    var item = family.Players[i];
                    bool online = false;
					Player exits = null;
					foreach (Player ply in NAPI.Pools.GetAllPlayers())
					if (ply.Name == item.Name)
					{
					exits = ply;
					break;
					}
                    string id = "-";
                    if (Members.Values.FirstOrDefault(m => m.Name == item.Name) != null && exits != null)
                    {
                        online = true;
                        id = exits.Value.ToString();
                    }
                    List<object> data2 = new List<object>()
                    {
                        online,
                        id,
                        item.Name,
                        item.FamilyRank,
                        item.FamilyRankName,
                    };
                    players.Add(data2);
                }
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(players);
                NeptuneEVO.Trigger.PlayerEvent(player, "loadfamilymemberstomenu", json);
            }
            catch (Exception e)
            {
                Log.Write("Loaded Error \n" + e.ToString(), nLog.Type.Error);
            }

        }

        #region Invite Player
        [RemoteEvent("invitePlayerToFamily")]
        public static void InvitePlayerToFamily(Player player, int id)
        {
            try
            {
                Family family = Family.GetFamilyToCid(Main.Players[player].FamilyCID);
                if (family.Players.Count >= family.MaxPlayers)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В семье нет места для новых участников. Лимит: {family.MaxPlayers}", 3000);
                    return;
                }
                string st = id.ToString();
                if (Int32.TryParse(st, out id))
                {
                    Player target = Main.GetPlayerByID(id);
                    if (target.HasData("IS_DYING"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин находится в коме", 3000);
                        return;
                    }
                    if (target == null)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин с таким ID не найден", 3000);
                        return;
                    }
                    Commands.InviteToFamily(player, target);
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }

        }
        #endregion

        #region SetRank
        [RemoteEvent("changefamilyrank")]
        public static void ChangeFamilyRank(Player player, string data1, string data2)
        {
            try
            {
                int id;
                if (!Int32.TryParse(data2, out id))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                    return;
                }
                if (Int32.TryParse(data1, out id))
                {
                    Player target = Main.GetPlayerByID(id);
                    if (target == null)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин с таким ID не найден", 3000);
                        return;
                    }
                    Commands.SetFamilyRank(player, target, Convert.ToInt32(data2));
                }
                else
                {
                    Player target = NAPI.Player.GetPlayerFromName(data1);
                    if (target == null)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин с таким именем не найден", 3000);
                        return;
                    }
                    Commands.SetFamilyRank(player, target, Convert.ToInt32(data2));
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }

        }
        #endregion

        #region Dell Member
        [RemoteEvent("kickfamilymember")]
        public static void DeleteMemberToFamily(Player player, string data, string reason)
        {
            try
            {
                int id;
                if (Int32.TryParse(data, out id))
                {
                    Player target = Main.GetPlayerByID(id);
                    if (target == null)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин с таким ID не найден", 3000);
                        return;
                    }
                    Commands.DeleteFamilyMember(player, target, reason);
                }
                else
                {
                    Player target = NAPI.Player.GetPlayerFromName(data);
                    if (target == null)
                    {
                        if (player.Name == data)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете кикнуть самого себя", 3000);
                            return;
                        }

                        if (!Main.PlayerUUIDs.ContainsKey(data)) return;


                        Family family = Family.GetFamilyToCid(Main.Players[player].FamilyCID);

                        Member memberindex = family.Players.Find(x => x.Name == data);

                        if (Manager.AllMembers.ContainsKey(Main.PlayerUUIDs[data]))
                        {
                            Manager.AllMembers.Remove(Main.PlayerUUIDs[data]);
                        }
                        if (memberindex != null)
                        {
                            family.Players.Remove(memberindex);
                        }

                        MySQL.Query($"UPDATE `characters` SET `familycid`='null', `familyrank`=0  WHERE `uuid`={Main.PlayerUUIDs[data]}");
                        return;
                    }
                    Commands.DeleteFamilyMember(player, target, reason);
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region SaveSettings
        [RemoteEvent("saveFamilySettings")]
        public static void SaveSettingsToFamily(Player player, string desc_1, string desc_2, string nums)
        {
            try
            {
                CharacterData acc = Main.Players[player];
                Family family = Family.GetFamilyToCid(acc.FamilyCID);

                if (!Manager.isHaveFamily(player, family.FamilyCID)) return;
                if (acc.FamilyRank != family.AllRanks.Count)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет доступа", 2000);
                    return;
                }

                if (!string.IsNullOrEmpty(desc_1))
                    family.Desc_1 = desc_1;
                if (!string.IsNullOrEmpty(desc_2))
                    family.Desc_2 = desc_2;
                if (!string.IsNullOrEmpty(nums))
                {

                    List<List<object>> NUMBERS = Newtonsoft.Json.JsonConvert.DeserializeObject<List<List<object>>>(nums);
                    Dictionary<string, int> DICT = new Dictionary<string, int>();
                    for (int i = 0; i < NUMBERS.Count; i++)
                    {
                        try
                        {
                            Convert.ToInt32(NUMBERS[i][2]);
                        }
                        catch { return; }
                       DICT.Add(Convert.ToString( NUMBERS[i][1] ), Convert.ToInt32( NUMBERS[i][2]));
                    }
                    family.Vehicles = DICT;
                }

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Настройки успешно сохранены", 2000);

                Family.SaveFamily(family);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region Saving Changed AllRanks
        [RemoteEvent("saveChangesRanks")]
        public static void SaveChangingsFamilyRanks(Player player, string ranks)
        {
            try
            {
                CharacterData acc = Main.Players[player];
                if (acc.FamilyRank == 0) return;
                List<Ranks> data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Ranks>>(ranks);

                if (ranks != null)
                {
                    Family family = Family.GetFamilyToCid(acc.FamilyCID);
                    if (acc.FamilyRank == family.AllRanks.Count)
                    {
                        family.AllRanks = data;
                        Family.SaveFamily(family);
                        Ranks.SaveRanks(family);

                        foreach (Player p in Members.Keys.ToList())
                        {
                            if (Main.Players[p].FamilyCID == family.FamilyCID)
                            {
                                Member.LoadMembers(p, Main.Players[p].FamilyCID, Main.Players[p].FamilyRank);
                            }
                        }
                        foreach (Member member in AllMembers.Values.ToList().FindAll(x => x.FamilyCID == family.FamilyCID))
                        {
                            if (member.FamilyCID == family.FamilyCID)
                            {
                                Member.UpdateRanksNameToMembers(family, member);
                            }
                        }
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Настройки рангов сохранены", 2000);

                    }
                    else
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Нет доступа", 2000);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region Diband Family
        [RemoteEvent("disbandFamily")]
        public static void DisbandFamily(Player player, string reason)
        {
            try
            {
                CharacterData acc = Main.Players[player];
                Family family = Family.GetFamilyToCid(acc.FamilyCID);
                if (family == null) return;
                if (family.Leader == acc.UUID)
                {
                    foreach (Member member in AllMembers.Values.ToList().FindAll(x => x.FamilyCID == family.FamilyCID))
                    {
                        if (member.FamilyCID == family.FamilyCID)
                        {
                            Member.UnLoadMemberToDisbandFamily(member); //я изначально убираю игрока из всех листов членов семьи, и блять ещё пытаюсь убрать по ключу игрока из базы.ДОЛБАЕБ!!! голем даун
                        }
                    }
                    foreach (Player p in Members.Keys.ToList())
                    {
                        if (Main.Players[p].FamilyCID == family.FamilyCID)
                        {
                            Member.UnLoadMember(p);
                            string msg = reason == null || reason == "" ? "Без причины" : $"По причине: {reason}";
                            Notify.Send(p, NotifyType.Info, NotifyPosition.BottomCenter, $"Семья {family.Name}, была распущена лидером: {player.Name}. {msg}", 2000);
                        }
                    }

                    Family.DeleteFamily(family);
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #endregion

        #region Create & OpenCreatorMenu
        public static void OpenCreatorFamilyMenu(Player player)
        {
            try
            {
                List<object> data = new List<object>()
                {
                    CreatePrice,
                    NeedLVL,
                    Families.Count,
                    Main.Players[player].LVL,
                    Multiplier,
                };
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                NeptuneEVO.Trigger.PlayerEvent(player, "openCreatorFamilyMenu", json);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }

        [RemoteEvent("loadlistfamilies")]
        public static void LoadListFamilies(Player player)
        {
            try
            {
                List<object> families = new List<object>();
                foreach (var item in Families)
                {
                    List<object> data = new List<object>()
                {
                    item.Name,
                    Main.PlayerNames[item.Leader],
                    item.MaxPlayers,
                    item.Players.Count,
                    item.ImageURL,
                };
                    families.Add(data);
                };
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(families);
                NeptuneEVO.Trigger.PlayerEvent(player, "loadlistfamilies", "server", json);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }

        [RemoteEvent("createnewfamily")]
        public static void CreateFamily(Player player, string name, int maxplayers, string img)
        {
            try
            {
                int result = Family.CreateFamilies(player, name, maxplayers, img);
                string msg;
                switch (result)
                {
                    case 1:
                        msg = $"У вас нет своего личного дома";
                        break;
                    case 2:
                        msg = $"У вас недостаточно денег";
                        break;
                    case 3:
                        msg = $"Ваш уровень ниже требуемого";
                        break;
                    case 4:
                        msg = $"Вы уже находитесь в семье: {Family.GetFamilyName(player)}";
                        break;
                    case 5:
                        msg = $"Вы уже находитесь в организации";
                        break;
                    case 6:
                        msg = $"У вас нет сертификата для создания семьи";
                        break;
                    default:
                        msg = $"Семья {name} успешно создана";
                        break;
                }

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, msg, 2000);
                Log.Write(msg, nLog.Type.Info);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region Is Have
        public static bool isHaveFamily(Player player)
        {
            try
            {
                if (Main.Players[player].FamilyRank != 0 || Main.Players[player].FamilyCID != "null" || Main.Players[player].FractionID != 0)
                    return true;
                return false;
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
                return false;
            }
        }

        public static bool isHaveFamily(Player player, string familyCID)
        {
            try
            {
                if (Main.Players[player].FamilyCID == familyCID)
                    return true;
                return false;
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
                return false;
            }
        }
        #endregion
    }
}
