using NeptuneEVO.SDK;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using GTANetworkAPI;
using System.Linq;
using NeptuneEVO;

namespace Golemo.Families
{
    public class Member
    {
        private static readonly nLog Log = new nLog("FamilyMember");
        public string Name { get; set; }
        public string FamilyName { get; set; }
        public string FamilyCID { get; set; }
        public int FamilyRank { get; set; }
        public string FamilyRankName { get; set; }

        public Member()
        {

        }
        public Member(string name, string familyname, string cid, int rank, string rankname)
        {
            Name = name;
            FamilyName = familyname;
            FamilyCID = cid;
            FamilyRank = rank;
            FamilyRankName = rankname;
        }

        public static void LoadAllMembers()
        {
            try
            {
                var result = MySQL.QueryRead("SELECT `uuid`,`firstname`,`lastname`,`familycid`,`familyrank` FROM `characters`");
                if (result != null)
                {
                    foreach (DataRow Row in result.Rows)
                    {
                        var memberData = new Member();
                        int uuid = Convert.ToInt32(Row["uuid"]);
                        memberData.Name = $"{Convert.ToString(Row["firstname"])}_{Convert.ToString(Row["lastname"])}";
                        memberData.FamilyCID = Convert.ToString(Row["familycid"]) == "" ? "null" : Convert.ToString(Row["familycid"]);
                        if (memberData.FamilyCID != "null")
                        {
                            memberData.FamilyName = Family.GetFamilyName(memberData.FamilyCID);
                            memberData.FamilyRank = Convert.ToInt32(Row["familyrank"]);
                            memberData.FamilyRankName = Ranks.GetFamilyRankName(memberData.FamilyCID, memberData.FamilyRank);


                            Manager.AllMembers.Add(uuid, memberData);
                            Family family = Family.GetFamilyToCid(memberData.FamilyCID);
                            family.Players.Add(memberData);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }

        public static void LoadMembers(Player player, string familycid, int familyrank)
        {
            try
            {
                if (Manager.Members.ContainsKey(player)) Manager.Members.Remove(player);
                Family family = Family.GetFamilyToCid(familycid);
                Member data = new Member();
                data.FamilyCID = familycid;
                data.FamilyName = family.Name;
                data.FamilyRank = familyrank;
                data.FamilyRankName = Ranks.GetFamilyRankName(familycid, familyrank);
                data.Name = player.Name.ToString();

                Manager.Members.Add(player, data);

                if (!Manager.AllMembers.ContainsKey(Main.Players[player].UUID)) Manager.AllMembers.Add(Main.Players[player].UUID, data);
                else
                {
                    Manager.AllMembers[Main.Players[player].UUID] = data;
                }
                int index = family.Players.FindIndex(x => x.Name == data.Name);
                if (index > -1)
                    family.Players[index] = data;

                player.SetSharedData("IS_FAMILY", true);
                player.SetSharedData("familycid", data.FamilyCID);
                player.SetSharedData("familyname", data.FamilyName);
                player.SetSharedData("familyrank", data.FamilyRank);
                player.SetSharedData("familyrankname", data.FamilyRankName);
                Log.Write($"FamilyMember {player.Name} loaded. Family:{data.FamilyName}", nLog.Type.Success);

                Family.SaveFamily(family);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }

        public static void UpdateRanksNameToMembers(Family family, Member member)
        {
            try
            {
                member.FamilyRankName = Ranks.GetFamilyRankName(family.FamilyCID, member.FamilyRank);
                if (Manager.AllMembers.ContainsValue(member))
                {
                    int key = Manager.AllMembers.FirstOrDefault(x => x.Value.Name == member.Name).Key;
                    Manager.AllMembers[key] = member;
                }

                int index = family.Players.FindIndex(x => x.Name == member.Name);
                family.Players[index] = member;

                Family.SaveFamily(family);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }

        public static void UnLoadMember(Player player)
        {
            try
            {
                CharacterData acc = Main.Players[player];
                Family family = Family.GetFamilyToCid(acc.FamilyCID);
                int memberindex = family.Players.FindIndex(x => x.Name == player.Name);

                acc.FamilyCID = "null";
                acc.FamilyRank = 0;


                if (Manager.AllMembers.ContainsKey(acc.UUID))
                {
                    Manager.AllMembers.Remove(acc.UUID);
                }
                if (memberindex > -1)
                {
                    family.Players.RemoveAt(memberindex);
                }
                if (Manager.Members.ContainsKey(player))
                {
                    Manager.Members.Remove(player);
                }

                Main.Players[player].Save(player).Wait();

                player.ResetSharedData("IS_FAMILY");
                player.ResetSharedData("familycid");
                player.ResetSharedData("familyname");
                player.ResetSharedData("familyrank");
                player.ResetSharedData("familyrankname");

                Family.SaveFamily(family);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }

        }

        public static void UnLoadMemberToDisbandFamily(Member player)
        {
            try
            {
                int key = Manager.AllMembers.FirstOrDefault(x => x.Value.Name == player.Name).Key;
                if (Manager.AllMembers.ContainsKey(key))
                {
                    Manager.AllMembers.Remove(key);
                }

                MySQL.Query($"UPDATE `characters` SET `familycid`='null', `familyrank`=0  WHERE `uuid`={key}");
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }

        }


        public static int GetOnlineMembers(Family family)
        {
            try
            {
                List<Member> data = new List<Member>();
                foreach (var item in Manager.Members.Values)
                {
                    if (item.FamilyCID == family.FamilyCID) data.Add(item);
                }
                return data.Count;
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
                return 0;
            }
        }

        public static List<Member> GetFamilyMembers(Family family)
        {
            try
            {
                List<Member> data = new List<Member>();
                foreach (var item in Manager.AllMembers.Values)
                {
                    if (item.FamilyCID == family.FamilyCID) data.Add(item);
                }
                return data;
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
                return new List<Member>();
            }
        }

    }

    public class Ranks
    {
        private static readonly nLog Log = new nLog("FamilyRanks");
        public string FamilyCID { get; set; }
        public int Rank { get; set; }
        public string RankName { get; set; }

        public static List<Ranks> CreateRanks(string familycid)
        {
            try
            {
                List<Ranks> data = new List<Ranks>();
                for (int i = 1; i < 11; i++)
                {
                    Ranks rank = new Ranks();
                    rank.FamilyCID = familycid;
                    rank.Rank = i;
                    rank.RankName = $"Ранг{i}";

                    MySQL.Query($"INSERT INTO `familyranks`(`cid`, `rank`, `rankname`) VALUES('{familycid}', {rank.Rank}, '{rank.RankName}')");
                    //Log.Write($"Я создал запись в базе данных на новый ранг: {familycid} - {rank.Rank} - {rank.RankName}", nLog.Type.Warn);

                    data.Add(rank);
                }
                return data;
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
                return new List<Ranks>();
            }
        }

        public static List<Ranks> LoadRanks(string familycid)
        {
            try
            {
                List<Ranks> data = new List<Ranks>();

                DataTable result = MySQL.QueryRead($"SELECT * FROM `familyranks` WHERE `cid`='{familycid}'");
                if (result == null || result.Rows.Count == 0) return CreateRanks(familycid);
                foreach (DataRow Row in result.Rows)
                {
                    Ranks rank = new Ranks();

                    rank.FamilyCID = Convert.ToString(Row["cid"]);
                    rank.Rank = Convert.ToInt32(Row["rank"]);
                    rank.RankName = Convert.ToString(Row["rankname"]);

                    data.Add(rank);
                }
                return data;
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
                return new List<Ranks>();
            }

        }

        public static void SaveRanks(Family family)
        {
            try
            {
                DataTable result = MySQL.QueryRead($"SELECT * FROM `familyranks` WHERE `cid`='{family.FamilyCID}'");
                if (result == null || result.Rows.Count == 0) return;

                foreach (Ranks rank in family.AllRanks)
                {
                    MySQL.Query($"UPDATE `familyranks` SET  `rankname`='{rank.RankName}'  WHERE `cid`='{family.FamilyCID}' AND `rank`={rank.Rank}");
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }

        }

        public static void DeleteRanks(Family family)
        {
            try
            {
                if (family == null) return;

                MySQL.Query($"DELETE FROM `familyranks` WHERE `cid`='{family.FamilyCID}'");
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }

        public static string GetFamilyRankName(string cid, int rank)
        {
            try
            {
                string rankname = $"Ранг{rank}";
                Family family = Manager.Families.Find(x => x.FamilyCID == cid);
                if (family == null) return rankname;
                rankname = family.AllRanks.Find(x => x.Rank == rank).RankName;
                return rankname;
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
                return $"Ранг{rank}";
            }
        }
    }
}
