using System;
using System.Collections.Generic;
using System.Text;
using NeptuneEVO.SDK;
using GTANetworkAPI;
using NeptuneEVO.Businesses;
using NeptuneEVO.Core;
using NeptuneEVO.MoneySystem;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using static System.Globalization.CalendarWeekRule;
using System.Globalization;

namespace NeptuneEVO.Utils
{

    class QuestsManager : Script
    {

        private static nLog Log = new nLog("Quests");

        public static List<Quest> QuestList = new List<Quest>();

        private static List<List<Quest>> QuestWeek = new List<List<Quest>>();

        public static List<int> QuestTypes = new List<int>();

        public static int Day = DateTime.Now.Day;

        public static int Currectday;

        private static int MaxQuests = 15;

        public static GregorianCalendar Calendar = new GregorianCalendar();

        public static int Week = Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);

        private static List<int> MaxTypes = new List<int> { 4, 2, 1, 100, 4, 7500, 1, 1, 1, 10, 1, 3000, 1, 2000, 3000, 2000, 1 };

        private static List<int> MinTypes = new List<int> { 2, 1, 1, 40, 1, 5000, 1, 1, 1, 5, 1, 1000, 1, 1000, 1500, 1000, 1 };

        private static List<string> Names = new List<string> { "Отыграть несколько часов", "Починить машину", "Купить маску", "Собрать железяки", "Поймать черного амура", "Заработать на автобусе", "Купить любой автомобиль", "Сделать тюнинг авто", "Набить татуировку", "Оплатить штрафы", "Посетить барбершоп", "Заработать на стройке", "Арендовать транспорт", "Заработать на уборке штата", "Заработать на мусоровозе", "Заработать на каменоломне", "Открыть любой контейнер" };


        

        public static Dictionary<string, int> GetDay = new Dictionary<string, int>
        {

            {"Monday", 1 },
            {"Tuesday", 2 },
            {"Wednesday", 3 },
            {"Thursday", 4 },
            {"Friday", 5 },
            {"Saturday", 6 },
            {"Sunday", 7 },

        };

        [RemoteEvent("getquests")]
        public static void RemoteEvent_getquests(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;

                if (!player.HasData("QUESTS"))
                    OptimizeData(player, Main.Players[player].Quests);

                List<object> quests = new List<object>();

                for (int i = 1; i < 5; i++)
                {
                    quests.Add(new List<object> { Names[QuestTypes[i - 1]], player.GetData<List<ActiveQuest>>("QUESTS")[i - 1].Complete ? "ВЫПОЛНЕНО" : $"{player.GetData<List<ActiveQuest>>("QUESTS")[i - 1].Process} / {player.GetData<List<ActiveQuest>>("QUESTS")[i - 1].Quest.Max}" });
                }

                Trigger.PlayerEvent(player, "setquests", JsonConvert.SerializeObject(quests));
            }
            catch (Exception e) { Log.Write("GET: " + e.ToString(), nLog.Type.Error); }

        }

        private static void SendQuest()
        {
            // null
        }

        public static void CheckToDonate(Player player)
        {
            try
            {
                int complete = 0;
                foreach (ActiveQuest quest in player.GetData<List<ActiveQuest>>("QUESTS"))
                    if (quest.Complete && quest.Process == quest.Quest.Max)
                        complete++;
                if (complete == 4)
                {
                    Main.Players[player].Quests.QuestList[Currectday - 1][0][0] = Main.Players[player].Quests.QuestList[Currectday - 1][0][0] + 1;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.CenterRight, "Вы выполнили все задания! +25UP", 3000);
                    Main.Accounts[player].RedBucks += 25;
                    MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");
                    
                }
            }
            catch (Exception e) { Log.Write("CHECK: " + e.ToString()); }
        }

        public static void AddQuestProcess(Player player, int type, int cost = 1)
        {
            try
            {
                if (!QuestTypes.Contains(type)) return;

                int i = 0;

                if (!player.HasData("QUESTS"))
                    OptimizeData(player, Main.Players[player].Quests);

                foreach (ActiveQuest quest in player.GetData<List<ActiveQuest>>("QUESTS"))
                {

                    if (quest.Type == type)
                        if (quest.Complete)
                            return;
                        else
                        {
                            Main.Players[player].Quests.QuestList[Currectday - 1][i][0] = quest.Process + cost;
                            quest.AddProcess(cost, player);
                            return;
                        }
                    i++;
                }
            }
            catch (Exception e) { Log.Write("PROCESS: " + e.ToString()); }
        }

        public static void OptimizeData(Player player, SDK.Utils.QuestDATABASE que)
        {
            try
            {
                List<ActiveQuest> actives = new List<ActiveQuest>();

                for (int i = 1; i < 5; i++)
                {
                    actives.Add(new ActiveQuest(QuestTypes[i - 1], QuestList[i - 1], que.QuestList[Currectday - 1][i - 1][0]));
                }

                player.SetData("QUESTS", actives);
            }
            catch (Exception e) { Log.Write("PROCESS: " + e.ToString()); }
        }

        public static NeptuneEVO.SDK.Utils.QuestDATABASE OptimizePlayer(Player player)
        {
            try
            {
                List<List<List<int>>> quests = new List<List<List<int>>>();

                for (int t = 1; t < 8; t++)
                {
                    List<List<int>> dayquests = new List<List<int>>();
                    for (int i = 1; i < 5; i++)
                    {
                        dayquests.Add(new List<int> { 0, QuestWeek[t - 1][i - 1].Max });
                    }
                    quests.Add(dayquests);
                }

                return new NeptuneEVO.SDK.Utils.QuestDATABASE(Week, quests);
            }
            catch (Exception e) { Log.Write("PROCESS: " + e.ToString()); return null; }
        }

        public static void VerifyQuests()
        {
            try
            {
                Log.Write("Verifing quests");
                if (DateTime.Now.Day != Day)
                {
                    DayOfWeek time = DateTime.Now.DayOfWeek;

                    QuestList.RemoveRange(0, 4);
                    QuestTypes.RemoveRange(0, 4);



                    if (Currectday == 7 && GetDay[time.ToString()] == 0 && Week != Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday))
                    {
                        Log.Write("Creating new quests");
                        CreateQuestOnWeek();
                    }
                    else
                    {
                        Log.Write("Generate quets");
                        GenerateQuests();
                    }

                }
            }
            catch (Exception e) { Log.Write("PROCESS: " + e.ToString()); }
        }


        // { DATETIME:'13.1.2020',List:''}

        private Random rnd = new Random();

        public static void GenerateQuests()
        {
            try
            {
                var result = MySQL.QueryRead("SELECT * FROM `quests`");

                for (int d = 1; d < 8; d++)
                {
                    DataRow Rows = result.Rows[d - 1];
                    List<Quest> Data = new List<Quest>();
                    for (int i = 1; i < 5; i++)
                    {
                        Quest quest = JsonConvert.DeserializeObject<Quest>(Rows[$"quest{i}"].ToString());
                        Data.Add(quest);
                    }
                    QuestWeek.Add(Data);
                }

                DayOfWeek time = DateTime.Now.DayOfWeek;

                Currectday = GetDay[time.ToString()];

                DataRow Row = result.Rows[Currectday - 1];

                for (int i = 1; i < 5; i++)
                {
                    Quest quest = JsonConvert.DeserializeObject<Quest>(Row[$"quest{i}"].ToString());
                    QuestList.Add(quest);
                    QuestTypes.Add(quest.Type);
                }

                SendQuest();
            }
            catch (Exception e) { Log.Write("PROCESS: " + e.ToString()); }
        }

        public static void OptimizeQuests()
        {
            try
            {
                foreach (Player player in NAPI.Pools.GetAllPlayers())
                {
                    if (!Main.Players.ContainsKey(player)) continue;

                    List<List<List<int>>> quests = new List<List<List<int>>>();

                    for (int t = 1; t < 8; t++)
                    {
                        List<List<int>> dayquests = new List<List<int>>();
                        for (int i = 1; i < 5; i++)
                        {
                            dayquests.Add(new List<int> { 0, QuestWeek[t - 1][i - 1].Max });
                        }
                        quests.Add(dayquests);
                    }

                    Main.Players[player].Quests = new NeptuneEVO.SDK.Utils.QuestDATABASE(Week, quests);

                    OptimizeData(player, Main.Players[player].Quests);
                }
            }
            catch (Exception e) { Log.Write("PROCESS: " + e.ToString()); }
        }

        [ServerEvent(Event.ResourceStart)]
        public void ResourceStart()
        {
            try
            {
                var result = MySQL.QueryRead("SELECT * FROM `quests`");

                DayOfWeek time = DateTime.Now.DayOfWeek;

                Currectday = GetDay[time.ToString()];

                if (result.Rows.Count > 0)
                {
                    GenerateQuests();
                }
                else
                {
                    CreateQuestOnWeek();
                }

            }
            catch (Exception e) { Log.Write("RESOURCESTART: " + e.ToString(), nLog.Type.Error); }
        }

        public static int GetRNDByType(int it)
        {
            try
            {
                Random rnd = new Random();

                if (MaxTypes[it] == 1)
                    return 1;

                return rnd.Next(MinTypes[it], MaxTypes[it]);
            }
            catch { return 1; }
        }

        public static void CreateQuestOnWeek()
        {
            try
            {
                MySQL.Query("DELETE FROM `quests`"); ;

                Week = Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);

                DayOfWeek time = DateTime.Now.DayOfWeek;

                Currectday = GetDay[time.ToString()];

                Random rnd = new Random();

                List<List<int>> types = new List<List<int>>();

                for (int f = 0; f < 8; f++)
                {
                    List<int> type = new List<int>();

                    for (int i = 0; i < 5; i++)
                    {
                        int mrnd = 1;
                        do
                        {
                            mrnd = rnd.Next(1, MaxQuests);
                        }
                        while (type.Contains(mrnd));

                        type.Add(mrnd);

                    }

                    types.Add(type);

                }

                List<List<Quest>> quests = new List<List<Quest>>();

                foreach (List<int> array in types)
                {
                    List<Quest> quest = new List<Quest>();
                    foreach (int it in array)
                        quest.Add(new Quest(it, Names[it], GetRNDByType(it)));
                    quests.Add(quest);
                }

                for (int f = 1; f < quests.Count; f++)
                {
                    MySQL.Query($"INSERT INTO `quests`(`day`, `quest1`, `quest2`, `quest3`, `quest4`) VALUES ({f},'{JsonConvert.SerializeObject(quests[f - 1][0])}','{JsonConvert.SerializeObject(quests[f - 1][1])}','{JsonConvert.SerializeObject(quests[f - 1][2])}','{JsonConvert.SerializeObject(quests[f - 1][3])}')");
                    Log.Write("END:" + f.ToString());
                }

                GenerateQuests();
                OptimizeQuests();

                SendQuest();

            }
            catch (Exception e) { Log.Write("PROCESS: " + e.ToString()); }

        }

        public class Quest
        {
            public int Type { get; set; }

            public string Name { get; set; }

            public int Max { get; set; }

            public Quest(int type, string name, int max)
            {
                Type = type; Name = name; Max = max;
            }
        }



        public class ActiveQuest
        {
            public int Type { get; set; }

            public Quest Quest { get; set; }

            public int Process { get; set; }

            public bool Complete { get; set; }

            public ActiveQuest(int type, Quest quest, int process = 0)
            {
                Type = type; Quest = quest; Process = process; Complete = (process >= Quest.Max);
            }

            public bool AddProcess(int add, Player player)
            {
                try
                {
                    if (Process + add > Quest.Max)
                        add = Quest.Max - Process;
                    if (Process + add == Quest.Max)
                    {
                        Complete = true;
                        Process = Quest.Max;
                        MoneySystem.Wallet.Change(player, 500);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.CenterRight, "Выполнено задание! +500$", 3000);
                        CheckToDonate(player);
                        return true;
                    }
                    Process += add;
                    return false;
                }
                catch { return false; }
            }

        }



    }
}
