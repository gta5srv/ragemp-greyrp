using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using NeptuneEVO.Core.nAccount;
using NeptuneEVO.Core.Character;
using NeptuneEVO.GUI;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using System.Globalization;
using System.Net.Mail;
using NeptuneEVO.Fractions;
using NeptuneEVO.MoneySystem;
using System.Net.Http.Headers;
using NeptuneEVO.Houses;
using NeptuneEVO.Jobs;
using System.Data.SQLite;
using NeptuneEVO.Businesses;
using NeptuneEVO.Organization;
using static NeptuneEVO.Core.VehicleStreaming;
using NeptuneEVO.Utils;

namespace NeptuneEVO
{

    /*
                ░█▀▀▀ ▀█▀ ░█──░█ ░█▀▀▀ ░█─░█ ░█▀▀█ 
                ░█▀▀▀ ░█─ ─░█░█─ ░█▀▀▀ ░█─░█ ░█▄▄█ 
                ░█─── ▄█▄ ──▀▄▀─ ░█▄▄▄ ─▀▄▄▀ ░█───
    */

    public class Main : Script
    {
        public static string Codename { get; } = "FiveUP RolePlay";
        public static string Version { get; } = "2.1b"; // 2.2.4 r.i.p
        public static string Build { get; } = "2.0"; // 1583 r.i.p

        // // // //
        public static string Full { get; } = $"{Codename} {Version} {Build}";
        public static DateTime StartDate { get; } = DateTime.Now;
        public static DateTime CompileDate { get; } = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;

        // // // //

        public static bool BizFree = false;
        public static bool HouseFree = false;
        public static bool CompanyFree = false;

        public static bool BlackDay = false;

        // // // //
        public static oldConfig oldconfig;
        private static Config config = new Config("Main");
        private static byte servernum = config.TryGet<byte>("ServerNumber", "1");

        private static int Slots = NAPI.Server.GetMaxPlayers();
        
        public static Dictionary<string, Tuple<int, int, int>> PromoCodes = new Dictionary<string, Tuple<int, int, int>>();

        // Characters
        public static List<int> UUIDs = new List<int>(); // characters UUIDs
        public static Dictionary<int, string> PlayerNames = new Dictionary<int, string>(); // character uuid - character name
        public static Dictionary<string, int> PlayerBankAccs = new Dictionary<string, int>(); // character name - character bank
        public static Dictionary<string, int> PlayerUUIDs = new Dictionary<string, int>(); // character name - character uuid
        public static Dictionary<int, Tuple<int, int, int, long>> PlayerSlotsInfo = new Dictionary<int, Tuple<int, int, int, long>>(); // character uuid - lvl,exp,fraction,money

        public static Dictionary<string, Player> LoggedIn = new Dictionary<string, Player>();
        public static Dictionary<Player, Character> Players = new Dictionary<Player, Character>(); // character in

        public static Dictionary<int, int> SimCards = new Dictionary<int, int>();
        public static Dictionary<int, Player> MaskIds = new Dictionary<int, Player>();

        public static int Multipy = 1;

        // Accounts
        public static List<string> Usernames = new List<string>(); // usernames
        public static List<string> SocialClubs = new List<string>(); // socialclubnames
        public static Dictionary<string, string> Emails = new Dictionary<string, string>(); // emails
        public static List<string> HWIDs = new List<string>(); // emails
        public static Dictionary<Player, Account> Accounts = new Dictionary<Player, Account>(); // Player's accounts
        public static Dictionary<Player, Tuple<int, string, string, string>> RestorePass = new Dictionary<Player, Tuple<int, string, string, string>>(); // int code, string Login, string SocialClub, string Email

        public ColShape BonyCS = NAPI.ColShape.CreateSphereColShape(new Vector3(3367.203, 5185.236, 1.3402408), 3f, 0);
        public ColShape EmmaCS = NAPI.ColShape.CreateSphereColShape(new Vector3(3313.938, 5179.962, 18.91486), 3f, 0);
        public ColShape FrankCS = NAPI.ColShape.CreateSphereColShape(new Vector3(1924.431, 4922.007, 47.70858), 2f, 0);
        public ColShape FrankQuest0 = NAPI.ColShape.CreateSphereColShape(new Vector3(2043.343, 4853.748, 43.09409), 1.5f, 0);
        public ColShape FrankQuest1 = NAPI.ColShape.CreateSphereColShape(new Vector3(1924.578, 4921.459, 46.576), 290f, 0); // Зона, из которой нельзя выгнать трактор.
        public ColShape FrankQuest1_1 = NAPI.ColShape.CreateSphereColShape(new Vector3(1905.151, 4925.571, 49.52416), 4f, 0); // Зона, куда должен приехать трактор

        public Vehicle FrankQuest1Trac0 = NAPI.Vehicle.CreateVehicle(VehicleHash.Tractor2, new Vector3(1981.87, 5174.382, 48.26282), new Vector3(0.1017629, -0.1177645, 129.811), 70, 70, "Frank0");
        public Vehicle FrankQuest1Trac1 = NAPI.Vehicle.CreateVehicle(VehicleHash.Tractor2, new Vector3(1974.506, 5168.247, 48.2662), new Vector3(0.07581472, -0.08908347, 129.8487), 70, 70, "Frank1");

        public ColShape Zone0 = NAPI.ColShape.CreateCylinderColShape(new Vector3(3282.16, 5186.997, 17.41686), 2f, 3f, 0);
        public ColShape Zone1 = NAPI.ColShape.CreateCylinderColShape(new Vector3(3289.234, 5182.008, 17.42562), 2f, 3f, 0);

        public static char[] stringBlock = { '\'', '@', '[', ']', ':', '"', '[', ']', '{', '}', '|', '`', '%', '\\' };

        public static string BlockSymbols(string check) {
            for (int i = check.IndexOfAny(stringBlock); i >= 0;)
            {
                check = check.Replace(check[i], ' ');
                i = check.IndexOfAny(stringBlock);
            }
            return check;
        }

        public static Random rnd = new Random();

        public static List<string> LicWords = new List<string>()
        {
            "Мото транспорт", // 0
            "Легковой транспорт", // 1
            "Грузовой транспорт", // 2
            "Водный транспорт", // 3
            "Лицензия на вертолёт", // 4
            "Лицензия на самолёт", // 5
            "Лицензия на оружие", // 6
            "Медицинская карта", // 7
            "Лицензия парамедика", // 8
            "Военный билет", // 9
            "Лицензия Адвоката", // 10
        };

        public static int GetJobPoints(Player player)
		{
			switch (Players[player].WorkID)
            {
				case 1:
                    return Players[player].WElectric / 50;
                case 2:
                    return Players[player].WGopostal / 100;
                case 4:
                    return Players[player].WBus / 250;
                case 5:
                    return Players[player].WLawnmower / 150;
                case 6:
                    return Players[player].WTrucker / 15;
                case 7:
                    return Players[player].WCollector / 50;
                case 9:
                    return Players[player].WTraktorist / 200;
                case 10:
                    return Players[player].WTrashCar / 50;
                case 11:
                    return Players[player].WConstructor / 50;
                case 12:
                    return Players[player].WMiner / 75;
				case 13:
                    return Players[player].WDiver / 25;
                case 14:
                    return Players[player].WSnow / 150;
                default:
                    return -1;
			}
		}

        public static int AddJobPoint(Player player) // гениально же возвращать число когда ты его добавляешь, тупа я праграммист, но один минус, если нужно использовать больше одного раза то кидай в переменную. нахрена я это написал )
        {
            switch (Players[player].WorkID)
            {
                case 1:
                    if (Players[player].WElectric >= 250) return 5;
                    Players[player].WElectric += 1;
                    if (Players[player].WElectric % 50 == 0)
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш уровень электрика: {Players[player].WElectric / 50}", 5000);
                    }
                    MySQL.Query($"UPDATE `characters` set `electric`={Players[player].WElectric} where `uuid`='{Main.Players[player].UUID}'");
                    return Players[player].WElectric / 50;
                case 2:
                    if (Players[player].WGopostal >= 500) return 5;
                    Players[player].WGopostal += 1;
                    if (Players[player].WGopostal % 100 == 0)
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш уровень почтальона: {Players[player].WGopostal / 100}", 5000);
                    }
                    MySQL.Query($"UPDATE `characters` set `gopostal`={Players[player].WGopostal} where `uuid`='{Main.Players[player].UUID}'");
                    return Players[player].WGopostal / 100;
                case 4:
                    if (Players[player].WBus >= 1000) return 5;
                    Players[player].WBus += 1;
                    if (Players[player].WBus % 250 == 0)
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш уровень автобусника: {Players[player].WBus / 250}", 5000);
                    }
                    MySQL.Query($"UPDATE `characters` set `bus`={Players[player].WBus} where `uuid`='{Main.Players[player].UUID}'");
                    return Players[player].WBus / 250;
                case 5:
                    if (Players[player].WLawnmower >= 750) return 5;
                    Players[player].WLawnmower += 1;
                    if (Players[player].WLawnmower % 150 == 0)
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш уровень газонокосильщика: {Players[player].WLawnmower / 150}", 5000);
                    }
                    MySQL.Query($"UPDATE `characters` set `lawnmower`={Players[player].WLawnmower} where `uuid`='{Main.Players[player].UUID}'");
                    return Players[player].WLawnmower / 150;
                case 6:
                    if (Players[player].WTrucker >= 75) return 5;
                    Players[player].WTrucker += 1;
                    if (Players[player].WTrucker % 15 == 0)
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш уровень дальнобольщика: {Players[player].WTrucker / 15}", 5000);
                    }
                    MySQL.Query($"UPDATE `characters` set `trucker`={Players[player].WTrucker} where `uuid`='{Main.Players[player].UUID}'");
                    return Players[player].WTrucker / 15;
                case 7:
                    if (Players[player].WCollector >= 50) return 5;
                    Players[player].WCollector += 1;
                    if (Players[player].WCollector % 50 == 0)
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш уровень инкосаторщика: {Players[player].WCollector / 50}", 5000);
                    }
                    MySQL.Query($"UPDATE `characters` set `collector`={Players[player].WCollector} where `uuid`='{Main.Players[player].UUID}'");
                    return Players[player].WCollector / 50;
                case 9:
                    if (Players[player].WTraktorist >= 1000) return 5;
                    Players[player].WTraktorist += 1;
                    if (Players[player].WTraktorist % 200 == 0)
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш уровень тракториста: {Players[player].WTraktorist / 200}", 5000);
                    }
                    MySQL.Query($"UPDATE `characters` set `traktorist`={Players[player].WTraktorist} where `uuid`='{Main.Players[player].UUID}'");
                    return Players[player].WTraktorist / 200;
                case 10:
                    if (Players[player].WTrashCar >= 250) return 5;
                    Players[player].WTrashCar += 1;
                    if (Players[player].WTrashCar % 50 == 0)
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш уровень мусоровозчика: {Players[player].WTrashCar / 50}", 5000);
                    }
                    MySQL.Query($"UPDATE `characters` set `trashcar`={Players[player].WTrashCar} where `uuid`='{Main.Players[player].UUID}'");
                    return Players[player].WTrashCar / 50;
                case 11:
                    if (Players[player].WConstructor >= 250) return 5;
                    Players[player].WConstructor += 1;
                    if (Players[player].WConstructor % 50 == 0)
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш уровень строителя: {Players[player].WConstructor / 50}", 5000);
                    }
                    MySQL.Query($"UPDATE `characters` set `constructor`={Players[player].WConstructor} where `uuid`='{Main.Players[player].UUID}'");
                    return Players[player].WConstructor / 50;
                case 12:
                    if (Players[player].WMiner >= 375) return 5;
                    Players[player].WMiner += 1;
                    if (Players[player].WMiner % 75 == 0)
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш уровень добычи камня: {Players[player].WMiner / 75}", 5000);
                    }
                    MySQL.Query($"UPDATE `characters` set `miner`={Players[player].WMiner} where `uuid`='{Main.Players[player].UUID}'");
                    return Players[player].WMiner / 75;
				case 13:
                    if (Players[player].WDiver >= 100) return 5;
                    Players[player].WDiver += 1;
                    if (Players[player].WDiver % 25 == 0)
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш уровень поисковика: {Players[player].WDiver / 25}", 5000);
                    }
                    MySQL.Query($"UPDATE `characters` set `diver`={Players[player].WDiver} where `uuid`='{Main.Players[player].UUID}'");
                    return Players[player].WDiver / 25;
                case 14:
                    if (Players[player].WSnow >= 750) return 5;
                    Players[player].WSnow += 1;
                    if (Players[player].WSnow % 150 == 0)
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш уровень уборкщика: {Players[player].WSnow / 150}", 5000);
                    }
                    MySQL.Query($"UPDATE `characters` set `snow`={Players[player].WSnow} where `uuid`='{Main.Players[player].UUID}'");
                    return Players[player].WSnow / 150;
                default:
                    return -1;
            }

        }

        /*public class AdminSlotsData {
            public string Nickname { get; set; } = "Undefined_Undefined";
            public int AdminLVL { get; set; } = 1;
            public bool Logged { get; set; } = false;
            public bool SlotUsed { get; set; } = false;

            public AdminSlotsData(string nick, int alvl, bool logged, bool slotused) {
                Nickname = nick;
                AdminLVL = alvl;
                Logged = logged;
                SlotUsed = slotused;
            }
        }

        public static Dictionary <string, AdminSlotsData> AdminSlots = new Dictionary<string, AdminSlotsData>();
        private static ushort AdminSlotsReserved = config.TryGet<ushort>("AdminSlots", "10");
        private static ushort AdminSlotsUsed = 0;*/

        //private static Timer enviromentTimer;
        //private static Timer playedMinutesTimer;
        //private static Timer timer_payDay;
        //private static Timer saveDBtimer;
        private static nLog Log = new nLog("GM");

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            if (vehicle == FrankQuest1Trac0 || vehicle == FrankQuest1Trac1) {
                if (!Players[player].Achievements[8] || Players[player].Achievements[9]) player.WarpOutOfVehicle();
                else {
                    Trigger.PlayerEvent(player, "createWaypoint", 1905.1f, 4925.5f);
                    vehicle.SetSharedData("PETROL", VehicleManager.VehicleTank[vehicle.Class]);
                    vehicle.SetData("ACCESS", "QUEST");
                }
            }
        }

        

        /*[ServerEvent(Event.VehicleDoorBreak)]
        public void VehDoorBreak(Vehicle veh, int index)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            Log.Write($"VehicleDoorBreak {index}");

            data.Door[index] = 2;

            UpdateVehicleSyncData(veh, data);

            NAPI.PlayerEvent.TriggerPlayerEventInDimension(veh.Dimension, "VehStream_SetVehicleDoorStatus", veh.Handle, data.Door[0], data.Door[1], data.Door[2], data.Door[3], data.Door[4], data.Door[5], data.Door[6], data.Door[7]);
        }

        [ServerEvent(Event.VehicleDoorBreak)]
        public void OnVehicleDoorBreak(Vehicle vehicle, int doorIndex)
        {
            NAPI.Util.ConsoleOutput($"{vehicle.DisplayName} - {doorIndex}");
        }*/

        [ServerEvent(Event.PlayerEnterColshape)]
        public void EnterColshape(ColShape colshape, Player player) {
            if (colshape == FrankQuest1) return;
            if (colshape == BonyCS) {
                player.SetData("INTERACTIONCHECK", 500);
                Trigger.PlayerEvent(player, "PressE", true);
            }
            else if (colshape == EmmaCS) {
                player.SetData("INTERACTIONCHECK", 501);
                Trigger.PlayerEvent(player, "PressE", true);
            }
            else if (colshape == FrankCS) {
                player.SetData("INTERACTIONCHECK", 503);
                Trigger.PlayerEvent(player, "PressE", true);
            }
            else if (colshape == Zone0 || colshape == Zone1) {
                player.SetData("INTERACTIONCHECK", 502);
                Trigger.PlayerEvent(player, "PressE", true);
            }
            else if (colshape == FrankQuest0) {
                player.SetData("INTERACTIONCHECK", 504);
                Trigger.PlayerEvent(player, "PressE", true);
            }
            else if (colshape == FrankQuest1_1) {
                player.SetData("INTERACTIONCHECK", 505);
                Trigger.PlayerEvent(player, "PressE", true);
            }
        }

        [ServerEvent(Event.PlayerExitColshape)]
        public void ExitColshape(ColShape colshape, Player player) {
            if (colshape == FrankQuest1) { // Ливнул из зоны тракторов
                if (player.Vehicle == FrankQuest1Trac0 || player.Vehicle == FrankQuest1Trac1) {
                    if (Players[player].Achievements[8] && !Players[player].Achievements[9]) {
                        Vehicle trac = player.Vehicle;
                        player.WarpOutOfVehicle();
                        NAPI.Task.Run(() => {
                            if (trac == FrankQuest1Trac0) {
                                trac.Position = new Vector3(1981.87, 5174.382, 48.26282);
                                trac.Rotation = new Vector3(0.1017629, -0.1177645, 129.811);
                            } else {
                                trac.Position = new Vector3(1974.506, 5168.247, 48.2662);
                                trac.Rotation = new Vector3(0.07581472, -0.08908347, 129.8487);
                            }
                        }, 500);
                        player.SendChatMessage("Ну и зачем мне было пытаться увезти этот трактор, не пойму...");
                    }
                }
                return;
            }
            Trigger.PlayerEvent(player, "PressE", false);
            if (colshape == BonyCS || colshape == EmmaCS || colshape == Zone0 || colshape == Zone1 || colshape == FrankCS || colshape == FrankQuest0 || colshape == FrankQuest1_1) {
                player.SetData("INTERACTIONCHECK", 0);
                Trigger.PlayerEvent(player, "PressE", false);
            }
        }

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                Timers.StartTask(51000, () => timer_online());
                NAPI.TextLabel.CreateTextLabel("~b~Бони", new Vector3(3367.203, 5185.236, 2.8402408), 5f, 0.3f, 0, new Color(255, 255, 255), true, 0);
                NAPI.TextLabel.CreateTextLabel("~b~Эмма", new Vector3(3313.938, 5179.962, 20.81486), 5f, 0.3f, 0, new Color(255, 255, 255), true, 0);
                NAPI.TextLabel.CreateTextLabel("~b~Франк", new Vector3(1925.005, 4922.076, 49.27858), 5f, 0.3f, 0, new Color(255, 255, 255), true, 0);

                NAPI.Server.SetAutoRespawnAfterDeath(false);
                NAPI.Task.Run(() =>
                {
                    NAPI.Server.SetGlobalServerChat(false);
                    NAPI.World.SetTime(DateTime.Now.Hour, 0, 0);
                });

                Timers.StartOnceTask(10000, () => Forbes.SyncMajors());

                DataTable result = MySQL.QueryRead("SELECT `uuid`,`firstname`,`lastname`,`sim`,`lvl`,`exp`,`fraction`,`money`,`bank`,`adminlvl` FROM `characters`");
                if (result != null)
                {
                    foreach (DataRow Row in result.Rows)
                    {
                        try
                        {
                            int uuid = Convert.ToInt32(Row["uuid"]);
                            string name = Convert.ToString(Row["firstname"]);
                            string lastname = Convert.ToString(Row["lastname"]);
                            int lvl = Convert.ToInt32(Row["lvl"]);
                            int exp = Convert.ToInt32(Row["exp"]);
                            int fraction = Convert.ToInt32(Row["fraction"]);
                            long money = Convert.ToInt64(Row["money"]);
                            int adminlvl = Convert.ToInt32(Row["adminlvl"]);
                            int bank = Convert.ToInt32(Row["bank"]);

                            UUIDs.Add(uuid);
                            if (Convert.ToInt32(Row["sim"]) != -1) SimCards.Add(Convert.ToInt32(Row["sim"]), uuid);
                            PlayerNames.Add(uuid, $"{name}_{lastname}");
                            PlayerUUIDs.Add($"{name}_{lastname}", uuid);
                            PlayerBankAccs.Add($"{name}_{lastname}", bank);
                            PlayerSlotsInfo.Add(uuid, new Tuple<int, int, int, long>(lvl, exp, fraction, money));

                            if (adminlvl > 0)
                            {
                                DataTable result2 = MySQL.QueryRead($"SELECT `socialclub` FROM `accounts` WHERE `character1`={uuid} OR `character2`={uuid} OR `character3`={uuid}");
                                if (result2 == null || result2.Rows.Count == 0) continue;
                                string socialclub = Convert.ToString(result2.Rows[0]["socialclub"]);
                                //AdminSlots.Add(socialclub, new AdminSlotsData($"{name}_{lastname}", adminlvl, false, false));
                            }
                        }
                        catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
                    }
                }
                else Log.Write("DB `characters` return null result", nLog.Type.Warn);                                     //

                result = MySQL.QueryRead("SELECT `login`,`socialclub`,`email`,`hwid` FROM `accounts`");
                if (result != null)
                {
                    foreach (DataRow Row in result.Rows)
                    {
                        try
                        {
                            string login = Convert.ToString(Row["login"]);

                            Usernames.Add(login.ToLower());
                            if (SocialClubs.Contains(Convert.ToString(Row["socialclub"]))) Log.Write("ResourceStart: sc contains " + Convert.ToString(Row["socialclub"]), nLog.Type.Error);
                            else SocialClubs.Add(Convert.ToString(Row["socialclub"]));
                            Emails.Add(Convert.ToString(Row["email"]), login);
                            HWIDs.Add(Convert.ToString(Row["hwid"]));

                        }
                        catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
                    }
                }
                else Log.Write("DB `accounts` return null result", nLog.Type.Warn);

                result = MySQL.QueryRead("SELECT `name`,`type`,`count`,`owner` FROM `promocodes`");
                if (result != null)
                {
                    foreach (DataRow Row in result.Rows)
                        PromoCodes.Add(Convert.ToString(Row["name"]), new Tuple<int, int, int>(Convert.ToInt32(Row["type"]), Convert.ToInt32(Row["count"]), Convert.ToInt32(Row["owner"])));
                }
                else Log.Write("DB `promocodes` return null result", nLog.Type.Warn);

                Ban.Sync();

                NightSync();
                

                int time = 3600 - (DateTime.Now.Minute * 60) - DateTime.Now.Second;
                Timers.StartOnceTask("paydayFirst", time * 1000, () =>
                {
                     
                    Timers.StartTask("payday", 3600000, () => payDayTrigger());
                    payDayTrigger();

                });

                // 900000
                Timers.StartTask("savedb", 900000, () => saveDatabase());
                Timers.StartTask("savei", 420000, () => saveInventory());
                //Timers.StartTask("saveb", 720000, () => saveBanks());
                Timers.StartTask("playedMins", 60000, () => playedMinutesTrigger());
                Timers.StartTask("envTimer", 1000, () => enviromentChangeTrigger());
                result = MySQL.QueryRead($"SELECT * FROM `othervehicles`");
                if (result != null)
                {
                    foreach (DataRow Row in result.Rows)
                    {
                        int type = Convert.ToInt32(Row["type"]);

                        string number = Row["number"].ToString();
                        VehicleHash model = (VehicleHash)NAPI.Util.GetHashKey(Row["model"].ToString());
                        Vector3 position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                        Vector3 rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());
                        int color1 = Convert.ToInt32(Row["color1"]);
                        int color2 = Convert.ToInt32(Row["color2"]);
                        int price = Convert.ToInt32(Row["price"]);
                        CarInfo data = new CarInfo(number, model, position, rotation, color1, color2, price);

                        switch (type)
                        {
                            case 0:
                                Rentcar.CarInfos.Add(data);
                                break;
                        }
                    }

                    Rentcar.rentCarsSpawner();
                    Jobs.AutoMechanic.mechanicCarsSpawner();
                }
                else Log.Write("DB `othervehicles` return null result", nLog.Type.Warn);

                Fractions.Configs.LoadFractionConfigs();

				//NAPI.World.SetWeather(config.TryGet<string>("Weather", "CLEAR"));
                NAPI.World.SetWeather("CLEAR"); // сделать зиму XMAS вместо CLEAR - ЗИМАААААА

                if (oldconfig.DonateChecker)
                    MoneySystem.Donations.Start();


                        

                

                // Assembly information //
                //Log.Write(Full + " запущен " + StartDate.ToString("s"), nLog.Type.Success);
                //Log.Write($"Assembly compiled {CompileDate.ToString("s")}", nLog.Type.Success);

                Console.Title = "Сервер" + oldconfig.ServerName;
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static void NightSync()
        {
            if (DateTime.Now.Hour > 0 && DateTime.Now.Hour < 7)
            {
                Log.Write("Night enable!", nLog.Type.Success);
                Main.oldconfig.PaydayMultiplier = 2;
            }
            else
            {
                Main.oldconfig.PaydayMultiplier = 1;
            }

        }

        public static int addonline = 0;

        public static void timer_online()
        {
            var table = MySQL.QueryRead($"SELECT * FROM online");
            foreach (DataRow Row in table.Rows)
            {
                int online = Convert.ToInt32(Row["online"]);
                if (online > addonline)
                {
                    addonline += 1;
                    UpdateOnlineForAllPlayers(addonline);
                }
                else
                {
                    addonline -= 1;
                    UpdateOnlineForAllPlayers(addonline);
                }
            }
        }

        public static void UpdateOnlineForAllPlayers(int online)
        {
            foreach (Player player in NAPI.Pools.GetAllPlayers())
            {
                Trigger.PlayerEvent(player, "onlineud", online);
            }
        }

        public static void UpdateOnlineForPlayer(Player player)
        {
            Trigger.PlayerEvent(player, "onlineud", addonline);
        }


        [ServerEvent(Event.EntityCreated)]
        public void Event_entityCreated(Entity entity)
        {
            try
            {
                if (NAPI.Entity.GetEntityType(entity) != EntityType.Vehicle) return;
                Vehicle vehicle = NAPI.Entity.GetEntityFromHandle<Vehicle>(entity);
                vehicle.SetData("BAGINUSE", false);

                string[] keys = NAPI.Data.GetAllEntityData(vehicle);
                foreach (string key in keys) vehicle.ResetData(key);

                if (VehicleManager.VehicleTank.ContainsKey(vehicle.Class))
                {
                    vehicle.SetSharedData("PETROL", VehicleManager.VehicleTank[vehicle.Class]);
                    vehicle.SetSharedData("MAXPETROL", VehicleManager.VehicleTank[vehicle.Class]);
                }
                
                if (VehicleManager.Vehicles.ContainsKey(vehicle.NumberPlate))
                {
                    vehicle.SetSharedData("MILE", (float)VehicleManager.Vehicles[vehicle.NumberPlate].Sell);
                }
                else
                    vehicle.SetSharedData("MILE", 0f);

                vehicle.SetSharedData("hlcolor", 0);
                vehicle.SetSharedData("LOCKED", false);
                vehicle.SetSharedData("vehradio", 255);
                vehicle.SetData("ITEMS", new List<nItem>());
                vehicle.SetData("SPAWNPOS", vehicle.Position);
                vehicle.SetData("SPAWNROT", vehicle.Rotation);
            } catch (Exception e) { Log.Write("EntityCreated: " + e.Message, nLog.Type.Error); }
        }

        #region Player
        [ServerEvent(Event.PlayerDisconnected)]
        public void Event_OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {

                int Health = NAPI.Player.GetPlayerHealth(player);
                int Armor = NAPI.Player.GetPlayerArmor(player);
               // Log.Write($"HP: {Health}");
               // Log.Write($"ARMOR: {Armor}");
                if (player.HasData("ROOMCAR"))
                {
                    var uveh = player.GetData<Vehicle>("ROOMCAR");
                    uveh.Delete();
                    player.ResetData("ROOMCAR");
                }

                if (player.HasData("VEHTEST"))
                {
                    NAPI.Entity.DeleteEntity(player.GetData<Vehicle>("VEHTEST"));
                }

                NAPI.Task.Run(() => { 
                    if (type == DisconnectionType.Timeout)
                        Log.Write($"{player.Name} crashed", nLog.Type.Warn);
                    Log.Debug($"DisconnectionType: {type.ToString()}");

                
                    if (player.HasData("FAKEGUNT") && player.HasData("FAKEGUNTIMER") && player.GetData<string>("FAKENAME") == player.Name)
                    {
                        Timers.Stop(player.GetData<string>("FAKEGUNTIMER"));
                        Players[player].Licenses[6] = false;
                    }
                });

            if (player.HasData("BIZBLIP"))
                {
                    NAPI.Task.Run(() => {
                        if (player.GetData<Blip>("BIZBLIP") != null)
                        {
                            player.GetData<Blip>("BIZBLIP").Delete();
                        }
                        
                    });
                }

                if (player.HasData("COMPANYBLIP"))
                {
                    NAPI.Task.Run(() =>
                    {
                        if (player.GetData<Blip>("COMPANYBLIP") != null)
                        { 
                            player.GetData<Blip>("COMPANYBLIP").Delete();
                        }
                    });
                }


                Log.Debug("DISCONNECT STARTED");
                /*if(player.HasData("RealSocialClub")) {
                    if(AdminSlots.ContainsKey(player.GetData("RealSocialClub"))) {
                        AdminSlotsData adata = AdminSlots[player.GetData("RealSocialClub")];
                        if(adata.SlotUsed == true) {
                            AdminSlotsUsed--;
                            adata.SlotUsed = false;
                            if(adata.Logged == true) adata.Logged = false;
                        }
                    }
                }*/
                if (Accounts.ContainsKey(player))
                {
                    if (LoggedIn.ContainsKey(Accounts[player].Login)) LoggedIn.Remove(Accounts[player].Login);
                }
                if (Players.ContainsKey(player))
                {
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    try
                    {
                        if (player.HasData("ON_DUTY"))
                            Players[player].OnDuty = player.GetData<bool>("ON_DUTY");
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoad:Unloading onduty\":\n" + e.ToString()); }
                    Log.Debug("STAGE 1 (ON_DUTY)");
                    try
                    {
                        if (player.HasData("CUFFED") && player.GetData<bool>("CUFFED") &&
                            player.HasData("CUFFED_BY_COP") && player.GetData<bool>("CUFFED_BY_COP") && Players[player].DemorganTime <= 0)
                        {
                            if (Players[player].WantedLVL == null)
                                Players[player].WantedLVL = new WantedLevel(3, "Сервер", new DateTime(), "Выход во время задержания");
                            Players[player].ArrestTime = Players[player].WantedLVL.Level * 20 * 60;
                            Players[player].WantedLVL = null;
                        }
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoad:Arresting Player\":\n" + e.ToString()); }
                    Log.Debug("STAGE 2 (CUFFED)");
                    try
                    {
                        Houses.House house = Houses.HouseManager.GetHouse(player, true);
                        Houses.House apart = Houses.HouseManager.GetApart(player, true);
                        if (house == null)
                            if (apart != null)
                                house = apart;

                        if (house != null && string.IsNullOrEmpty(Players[player].FamilyCID) || house != null && Players[player].FamilyCID == "null")
                        {
                            if (GarageManager.Garages.ContainsKey(house.GarageID))
                            {
                                GarageManager.Garages[house.GarageID].SendAllVehiclesToGarage();
                            }
                        }
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoad:Unloading personal car\":\n" + e.ToString()); }
                    Log.Debug("STAGE 3 (VEHICLE)");
                    try
                    {
                        SafeMain.SafeCracker_Disconnect(player, type, reason);
                        VehicleManager.API_onPlayerDisconnected(player, type, reason);
                        DrivingSchool.onPlayerDisconnected(player, type, reason);
                        Rentcar.Event_OnPlayerDisconnected(player);

                        OOrders.EndOrder(player);
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoad:Unloading Neptune.core\":\n" + e.ToString()); }
                    Log.Debug("STAGE 4 (SAFE-VEHICLES)");
                    try
                    {
                        if (player.HasData("PAYMENT")) MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                        AirVehicles.Event_OnPlayerDisconnected(player, type, reason);
                        Jobs.Bus.onPlayerDissconnectedHandler(player, type, reason);
                        Jobs.Gopostal.onPlayerDisconnected(player, type, reason);
                        Jobs.Lawnmower.onPlayerDissconnectedHandler(player, type, reason);
                        Jobs.Taxi.onPlayerDissconnectedHandler(player, type, reason);
                        Jobs.Truckers.onPlayerDissconnectedHandler(player, type, reason);
                        Jobs.Collector.Event_PlayerDisconnected(player, type, reason);
                        Jobs.AutoMechanic.onPlayerDissconnectedHandler(player, type, reason);
                        Jobs.Tractorist.onPlayerDissconnectedHandler(player, type, reason);
                        Jobs.TrashCar.Event_PlayerDisconnected(player, type, reason);
                        Jobs.Construction.Event_PlayerDisconnected(player, type, reason);
                        Jobs.Miner.Event_PlayerDisconnected(player, type, reason);
						Jobs.Diver.Event_PlayerDisconnected(player, type, reason);
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoad:Unloading Neptune.jobs\":\n" + e.ToString()); }
                    Log.Debug("STAGE 5 (JOBS)");
                    try
                    {
                        Fractions.Army.onPlayerDisconnected(player, type, reason);
                        Fractions.Ems.onPlayerDisconnectedhandler(player, type, reason);
                        Fractions.Police.onPlayerDisconnectedhandler(player, type, reason);
                        Fractions.CarDelivery.Event_PlayerDisconnected(player);
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoad:Unloading Neptune.fractions\":\n" + e.ToString()); }
                    Log.Debug("STAGE 6 (FRACTIONS)");
                    try
                    {
                        GUI.Dashboard.Event_OnPlayerDisconnected(player, type, reason);
                        GUI.MenuManager.Event_OnPlayerDisconnected(player, type, reason);
                        Houses.HouseManager.Event_OnPlayerDisconnected(player, type, reason);
                        Houses.GarageManager.Event_PlayerDisconnected(player);
                        Houses.Hotel.Event_OnPlayerDisconnected(player);

                        Fractions.Manager.UNLoad(player);
                        Weapons.Event_OnPlayerDisconnected(player);
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoad:Unloading managers\":\n" + e.ToString()); }
                    Log.Debug("STAGE 7 (HOUSES)");

                    //MoneySystem.Casino.Disconnect(player, type);

                    Voice.Voice.PlayerQuit(player, reason);
                    Players[player].Save(player, new List<float> { player.Position.X, player.Position.Y, player.Position.Z }, Health, Armor).Wait();
                    Accounts[player].Save(player).Wait();
                    nInventory.Save(Players[player].UUID);

                    if (player.HasSharedData("MASK_ID") && MaskIds.ContainsKey(player.GetSharedData<int>("MASK_ID")))
                    {
                        MaskIds.Remove(player.GetSharedData<int>("MASK_ID"));
                        player.ResetSharedData("MASK_ID");
                    }

                    int uuid = Main.Players[player].UUID;
                    NAPI.Task.Run(() => {
                        try
                        {
                            Players.Remove(player);
                            Accounts.Remove(player);
                        }
                         catch { }   
                    });
                    GameLog.Disconnected(uuid);
                    Log.Debug("DISCONNECT FINAL");
                    // // //
                    Character.changeName(player.Name).Wait();
                }
                else if (Accounts.ContainsKey(player))
                {
                    Accounts[player].Save(player).Wait();
                    Accounts.Remove(player);
                }
                foreach (string key in NAPI.Data.GetAllEntityData(player)) player.ResetData(key);
                Log.Write(player.Name + " disconnected from server. (" + reason + ")");

            } catch (Exception e) { Log.Write($"PlayerDisconnected (value: {player.Value}): " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerConnected)]
        public void Event_OnPlayerConnected(Player player)
        {
            try
            {
				player.SetData("RealSocialClub", player.SocialClubName);
				player.SetData("RealHWID", player.Serial);
                if (Admin.IsServerStoping)
                {
                    player.Kick("Рестарт сервера");
                    return;
                }
                if (NAPI.Pools.GetAllPlayers().Count >= 1000)
                {
                    player.Kick();
                    return;
                }

                UpdateOnlineForPlayer(player);
                player.SetSharedData("playermood", 0);
                player.SetSharedData("playerws", 0);
                player.SendChatMessage("Приветствуем тебя на проекте FiveUP RolePlay!");
				player.SendChatMessage("Все для GTA 5 и RAGE:MP - https://ragemp.pro");
                player.Eval("let g_swapDate=Date.now();let g_triggersCount=0;mp._events.add('cefTrigger',(eventName)=>{if(++g_triggersCount>10){let currentDate=Date.now();if((currentDate-g_swapDate)>200){g_swapDate=currentDate;g_triggersCount=0}else{g_triggersCount=0;return!0}}})");
                uint dimension = Dimensions.RequestPrivateDimension(player);
                NAPI.Entity.SetEntityDimension(player, dimension);
                Trigger.PlayerEvent(player, "blackday", Main.BlackDay);
                Trigger.PlayerEvent(player, "ServerNum", servernum);
                Trigger.PlayerEvent(player, "Enviroment_Start", Env_lastTime, Env_lastDate, Env_lastWeather);
                CMD_BUILD(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"MAIN_OnPlayerConnected\":\n" + e.ToString(), nLog.Type.Error); }
        }

        #endregion Player

        #region PlayerEvents

        // [RemoteEvent("callst")]
        // public void PlayerEvent_callst(Player player)
        // {
        // Jobs.Taxi.callTaxi(player);
        // }


        // [RemoteEvent("callsme")]
        // public void PlayerEvent_callsme(Player player)
        // {
        // Fractions.Ems.callEms(player);
        // }


        [RemoteEvent("kickPlayer")]
        public void PlayerEvent_Kick(Player player)
        {
            try
            {
                player.Kick();
            }
            catch (Exception e) { Log.Write("kickPlayer: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("deletearmor")]
        public void PlayerEvent_DeleteArmor(Player player)
        {
            try
            {
                if (player.Armor == 0)
                {
                    nItem aItem = nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor);
                    if (aItem == null || aItem.IsActive == false) return;
                    nInventory.Remove(player, ItemType.BodyArmor, 1);
                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 0;
                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 0;
                    player.SetClothes(9, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture);

                }
            }
            catch (Exception e) { Log.Write("deletearmor: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("teleportWaypoint")]

        public void PlayerEvent_tpWP(Player player, float x, float y)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (Main.Players[player].AdminLVL < 1) return;
            NAPI.Entity.SetEntityPosition(player, new Vector3(x, y, 100));
            //Trigger.PlayerEvent(player, "teleportbyz");
        }

        [RemoteEvent("syncWaypoint")]
        public void PlayerEvent_SyncWP(Player player, float X, float Y, float Z)
        {
            try
            {
                if (player.Vehicle == null) return;
                if (player.HasData("VEH")) return;

                Player driver = null;
                foreach (var ply in NAPI.Pools.GetAllPlayers())
                {
                    var veh = ply.Vehicle;
                    if (veh == null) continue;
                    if (NAPI.Vehicle.GetVehicleNumberPlate(veh) == NAPI.Vehicle.GetVehicleNumberPlate(player.Vehicle))
                    {
                        if(NAPI.Player.GetPlayerVehicleSeat(ply) == 0)
                        {
                            driver = ply;
                            break;
                        }
                    }
                }
                if (driver == player || driver == null) return;
				if (driver.HasData("ON_WORK") && driver.GetData<bool>("ON_WORK") && Main.Players[driver].WorkID != 3 ) return;
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы передали координаты: {driver.Name}!", 3000);
                Notify.Send(driver, NotifyType.Success, NotifyPosition.BottomCenter, $"Вам передали координаты!", 3000);

                if (driver.HasData("MARKERPOINT"))
                {
                    var shp = driver.GetData<ColShape>("MARKERPOINT");
                    shp.Delete();
					driver.ResetData("MARKERPOINT");
                    Trigger.PlayerEvent(driver, "removeGRoute");
                }

                var shape = NAPI.ColShape.CreateCylinderColShape(new Vector3(X, Y, Z), 20f, 10000, 0);
                shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        if (entity.HasData("MARKERPOINT") && entity.GetData<ColShape>("MARKERPOINT") == shape)
                        {
                            shape.Delete();
                            driver.ResetData("MARKERPOINT"); ;
                            Trigger.PlayerEvent(driver, "removeGRoute");
                        } 
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                driver.SetData("MARKERPOINT", shape);
                //Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Z = {Z}!", 3000);
                Trigger.PlayerEvent(driver, "syncWP", X, Y);
            }
            catch
            {
            }
        }

        [RemoteEvent("spawn")]
        public void PlayerEvent_Spawn(Player player, int id)
        {
            int where = -1;
            try
            {
                NAPI.Entity.SetEntityDimension(player.Handle, 0);
                Dimensions.DismissPrivateDimension(player);
                Players[player].IsSpawned = true;
                Players[player].IsAlive = true;
				
				if (Main.Players[player].product == 10)
{
                    var data = Fractions.Stocks.fracStocks[19]; // число может меняться в зависимости от кол-ва фракций в вашем моде
                    data.Product += 10;
                    data.UpdateLabel();
                    Main.Players[player].product = 0;
}

                if (!VehicleManager.Vehicles.ContainsKey(Players[player].LastVeh)) Players[player].LastVeh = "";
                if(Players[player].Unmute > 0) {
					if(!player.HasData("MUTE_TIMER")) {
					player.SetData("MUTE_TIMER", Timers.StartTask(1000, () => Admin.timer_mute(player)));
					} else Log.Write($"PlayerSpawn MuteTime (MUTE) worked avoid", nLog.Type.Warn);
					}
					if (Players[player].VUnmute > 0)
					{
					if (!player.HasData("MUTE_TIMER"))
					{
					player.SetData("MUTE_TIMER", Timers.StartTask(1000, () => Admin.timer_mute(player)));
					player.SetSharedData("voice.muted", true);
					Trigger.PlayerEvent(player, "voice.mute");
					}
					else Log.Write($"PlayerSpawn VMuteTime (MUTE) worked avoid", nLog.Type.Warn);
					}
                if (Players[player].ArrestTime != 0)
                {
                    if(!player.HasData("ARREST_TIMER"))
                    {
                        player.SetData("ARREST_TIMER", Timers.StartTask(1000, () => Fractions.FractionCommands.arrestTimer(player)));
                        NAPI.Entity.SetEntityPosition(player, Fractions.Police.policeCheckpoints[4]);
                    } else Log.Write($"PlayerSpawn ArrestTime (KPZ) worked avoid", nLog.Type.Warn);
                }
                else if (Players[player].DemorganTime != 0)
                {
                    if(!player.HasData("ARREST_TIMER"))
                    {
                        player.SetData("ARREST_TIMER", Timers.StartTask(1000, () => Admin.timer_demorgan(player)));
                        Weapons.RemoveAll(player, true);
                        NAPI.Entity.SetEntityPosition(player, Admin.DemorganPosition + new Vector3(0, 0, 1.5));
                        NAPI.Entity.SetEntityDimension(player, 1337);
						Timers.StartTask(60000, () => Admin.timer_text(player));
                    } else Log.Write($"PlayerSpawn ArrestTime (DEMORGAN) worked avoid", nLog.Type.Warn);
                }
                else
                {
                    switch (id)
                    {
                        case 0:
                            NAPI.Entity.SetEntityPosition(player.Handle, Players[player].SpawnPos);
                            
                            Customization.ApplyCharacter(player);
                            if (Players[player].FractionID > 0) Fractions.Manager.Load(player, Players[player].FractionID, Players[player].FractionLVL);
							if (Players[player].FamilyRank > 0) Golemo.Families.Member.LoadMembers(player, Players[player].FamilyCID, Players[player].FamilyRank);

                            Houses.House house = Houses.HouseManager.GetHouse(player);
                            Houses.House apartament = Houses.HouseManager.GetApart(player);
                            if (house != null)
                            {
                                Houses.Garage garage = Houses.GarageManager.Garages[house.GarageID];
                                if (!string.IsNullOrEmpty(Players[player].LastVeh) && !string.IsNullOrEmpty(VehicleManager.Vehicles[Players[player].LastVeh].Position))
                                {
                                    Vector3 position = JsonConvert.DeserializeObject<Vector3>(VehicleManager.Vehicles[Players[player].LastVeh].Position);
                                    Vector3 rotation = JsonConvert.DeserializeObject<Vector3>(VehicleManager.Vehicles[Players[player].LastVeh].Rotation);
                                    garage.SpawnCarAtPosition(player, Players[player].LastVeh, position, rotation);
                                    Players[player].LastVeh = "";
                                }
                            }
                            else if (apartament != null)
                            {
                                Houses.Garage garage = Houses.GarageManager.Garages[apartament.GarageID];
                                if (!string.IsNullOrEmpty(Players[player].LastVeh) && !string.IsNullOrEmpty(VehicleManager.Vehicles[Players[player].LastVeh].Position))
                                {
                                    Vector3 position = JsonConvert.DeserializeObject<Vector3>(VehicleManager.Vehicles[Players[player].LastVeh].Position);
                                    Vector3 rotation = JsonConvert.DeserializeObject<Vector3>(VehicleManager.Vehicles[Players[player].LastVeh].Rotation);
                                    garage.SpawnCarAtPosition(player, Players[player].LastVeh, position, rotation);
                                    Players[player].LastVeh = "";
                                }
                            }
                            break;
                        case 1:
                            int frac = Players[player].FractionID;
                            NAPI.Entity.SetEntityPosition(player.Handle, Fractions.Manager.FractionSpawns[frac]);
                            if (Accounts[player].VipLvl != 6)
                                nInventory.ClearWithoutClothes(player);

                            Customization.ApplyCharacter(player);
                            if (Players[player].FractionID > 0) Fractions.Manager.Load(player, Players[player].FractionID, Players[player].FractionLVL);
							if (Players[player].FamilyRank > 0) Golemo.Families.Member.LoadMembers(player, Players[player].FamilyCID, Players[player].FamilyRank);

                            house = Houses.HouseManager.GetHouse(player);
                            if (house != null)
                            {
                                Houses.Garage garage = Houses.GarageManager.Garages[house.GarageID];
                                if (!string.IsNullOrEmpty(Players[player].LastVeh) && !string.IsNullOrEmpty(VehicleManager.Vehicles[Players[player].LastVeh].Position))
                                {
                                    VehicleManager.Vehicles[Players[player].LastVeh].Position = null;
                                    VehicleManager.Save(Players[player].LastVeh);
                                    garage.SendVehicleIntoGarage(Players[player].LastVeh);
                                    Players[player].LastVeh = "";
                                }
                            }
                            break;
                        case 3:
                            Golemo.Families.Family family = Golemo.Families.Family.GetFamilyToCid(player);

                            house = HouseManager.Houses.Find(x => x.ID == family.FamilyHouse);

                            if (house == null) return;

                            Customization.ApplyCharacter(player);
                            if (Players[player].FractionID > 0) Fractions.Manager.Load(player, Players[player].FractionID, Players[player].FractionLVL);
                            if (Players[player].FamilyRank > 0) Golemo.Families.Member.LoadMembers(player, Players[player].FamilyCID, Players[player].FamilyRank);

                            NAPI.Entity.SetEntityPosition(player.Handle, house.Position + new Vector3(0, 0, 1.5));

                            break;
                        case 2:
                            house = Houses.HouseManager.GetHouse(player);
                            apartament = Houses.HouseManager.GetApart(player);
                            if (house != null)
                            {
                                NAPI.Entity.SetEntityPosition(player.Handle, house.Position + new Vector3(0, 0, 1.5));
                                //if (Accounts[player].VipLvl != 6)
                                 //   nInventory.ClearWithoutClothes(player);
                            }
                            else if (Players[player].HotelID != -1)
                            {
                                NAPI.Entity.SetEntityPosition(player, Houses.Hotel.HotelEnters[Players[player].HotelID] + new Vector3(0, 0, 1.12));
                            }
                            else if (apartament != null)
                            {
                                NAPI.Entity.SetEntityPosition(player.Handle, apartament.Position + new Vector3(0, 0, 1.5));
                               // if (Accounts[player].VipLvl != 6)
                               //     nInventory.ClearWithoutClothes(player);
                            }
                            else
                            {
                                NAPI.Entity.SetEntityPosition(player.Handle, Players[player].SpawnPos);
                            }
                            
                            Customization.ApplyCharacter(player);
                            if (Players[player].FractionID > 0) Fractions.Manager.Load(player, Players[player].FractionID, Players[player].FractionLVL);
							if (Players[player].FamilyRank > 0) Golemo.Families.Member.LoadMembers(player, Players[player].FamilyCID, Players[player].FamilyRank);

                            if (house != null)
                            {
                                Houses.Garage garage = Houses.GarageManager.Garages[house.GarageID];
                                if (!string.IsNullOrEmpty(Players[player].LastVeh) && !string.IsNullOrEmpty(VehicleManager.Vehicles[Players[player].LastVeh].Position))
                                {
                                    VehicleManager.Vehicles[Players[player].LastVeh].Position = null;
                                    VehicleManager.Save(Players[player].LastVeh);
                                    garage.SendVehicleIntoGarage(Players[player].LastVeh);
                                    Players[player].LastVeh = "";
                                }
                            }
                            else if (apartament != null)
                            {
                                Houses.Garage garage = Houses.GarageManager.Garages[apartament.GarageID];
                                if (!string.IsNullOrEmpty(Players[player].LastVeh) && !string.IsNullOrEmpty(VehicleManager.Vehicles[Players[player].LastVeh].Position))
                                {
                                    VehicleManager.Vehicles[Players[player].LastVeh].Position = null;
                                    VehicleManager.Save(Players[player].LastVeh);
                                    garage.SendVehicleIntoGarage(Players[player].LastVeh);
                                    Players[player].LastVeh = "";
                                }
                            }
                            break;
                    }
                }
                Trigger.PlayerEvent(player, "acpos");
                Trigger.PlayerEvent(player, "ready");
                Trigger.PlayerEvent(player, "redset", Accounts[player].RedBucks);

                player.SetData("spmode", false);
                player.SetSharedData("InDeath", false);

            } catch (Exception e) { Log.Write($"PlayerEvent_Spawn/{where}: " + e.Message, nLog.Type.Error); }
        }

[ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandlerCLASS(Player player, Vehicle vehicle, sbyte seatid)
        {
            if (vehicle.GetData<string>("ACCESS") != "SCHOOL" && seatid == -1)
            {
                var licensec = Players[player].Licenses;
                var classing = vehicle.Class;
                // 1 [A] - МОТО
                // 2 [B] - ЛЕГКОВЫЕ
                // 3 [C] - ГРУЗОВЫЕ
                // 4 [V] - ВОДНЫЙ
                // 5 [LV] - ВЕРТОЛЁТЫ
                // 6 [LS] - САМОЛЁТЫ
                
				// if ((classing == 0 || classing == 8) && !licensec[0])
                // {
                // Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет прав категории [A] Мото!", 3000);
                // VehicleManager.WarpPlayerOutOfVehicle(player);
                // return;
                // }
                if ((classing == 1 || classing == 2 || classing == 3 || classing == 4 || classing == 5 || classing == 6 || classing == 7 || classing == 9) && !licensec[1])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет прав категории [B] Легковые!", 3000);
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    return;
                }
                if ((classing >= 10 && 13 < classing || classing == 17 || classing == 20) && !licensec[2])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет прав категории [C] Грузовые!", 3000);
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    return;
                }
                if (classing == 14 && !licensec[3])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет прав категории [V] Водный!", 3000);
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    return;
                }
                if (classing == 15 && !licensec[4])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет прав категории [LV] Вертолёты!", 3000);
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    return;
                }
                if (classing == 16 && !licensec[5])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет прав категории [LV] Самолет!", 3000);
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                    return;
                }
            }
        }
        
		
        [RemoteEvent("setStock")]
        public void PlayerEvent_setStock(Player player, string stock)
        {
            try
            {
                player.SetData("selectedStock", stock);
            } catch (Exception e) { Log.Write("setStock: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("inputCallback")]
        public void PlayerEvent_inputCallback(Player player, params object[] arguments)
        {
            string callback = "";
            try
            {
                if (arguments[1].ToString() == "") return;
                callback = arguments[0].ToString();
                string text = arguments[1].ToString();
                switch (callback)
                {
                    case "fuelcontrol_city":
                    case "fuelcontrol_police":
                    case "fuelcontrol_ems":
                    case "fuelcontrol_fib":
                    case "fuelcontrol_army":
                    case "fuelcontrol_news":
                        int limit = 0;
                        if (!Int32.TryParse(text, out limit) || limit <= 0)
                        {
                            Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }

                        string fracName = "";
                        int fracID = 6;
                        if (callback == "fuelcontrol_city")
                        {
                            fracName = "Мэрия";
                            fracID = 6;
                        }
                        else if (callback == "fuelcontrol_police")
                        {
                            fracName = "Полиция";
                            fracID = 7;
                        }
                        else if (callback == "fuelcontrol_ems")
                        {
                            fracName = "EMS";
                            fracID = 8;
                        }
                        else if (callback == "fuelcontrol_fib")
                        {
                            fracName = "FBI";
                            fracID = 9;
                        }
                        else if (callback == "fuelcontrol_army")
                        {
                            fracName = "Армия";
                            fracID = 14;
                        }
                        else if (callback == "fuelcontrol_news")
                        {
                            fracName = "News";
                            fracID = 15;
                        }

                        Fractions.Stocks.fracStocks[fracID].FuelLimit = limit;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы установили дневной лимит топлива в ${limit} для {fracName}", 3000);
                        return;
                    case "org_buy":
                        if (text.Length < 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Слишком короткое название!", 3000);
                            return;
                        }
                        if (text == "Государство")
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Не корректное название!", 3000);
                            return;
                        }
                        OCore.OrgList[player.GetData<int>("ORG_ID")].BuyS(player, text);
                        return;
                    case "accept_plastic":
                        {
                            Player targetpl = player.GetData<Player>("PLASTIC_TARGET");
                            if (targetpl == null) return;
                            if (!VehicleManager.Vehicles.ContainsKey(text)) 
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Такого транспорта не существует", 3000);
                                return;
                            } 
                            if (VehicleManager.Vehicles[text].Holder != targetpl.Name)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Данный гражданин не является владельцем этого авто", 3000);
                                return;
                            }
                            if (VehicleManager.HavePlactic(text))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У данного транспорта есть пластик", 3000);
                                return;
                            }
                            if (targetpl.Position.DistanceTo2D(player.Position) > 3f)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин слишком далеко", 3000);
                                return;
                            }
                            bool nearly = false;
                            foreach(Vehicle veh in NAPI.Pools.GetAllVehicles())
                            {
                                if (veh.NumberPlate == text && veh.Position.DistanceTo2D(player.Position) < 6f)
                                {
                                    nearly = true;
                                    break;
                                }
                            }
                            if (!nearly)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Транспорт далеко от вас", 3000);
                                return;
                            }
                            targetpl.SetData("PLASTIC_NUMBER", text);
                            targetpl.SetData("PLASTIC_TARGET", player);
                            Trigger.PlayerEvent(targetpl, "openDialog", "PLASTIC_ACCEPT", $"Вам предлагают купить пластик на автомобиль {ParkManager.GetNormalName(VehicleManager.Vehicles[text].Model)} [{text}] за 15000$");
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили Гражданину({targetpl.Id}) купить пластик за 15000$", 3000);
                            return;
                        }
                    case "dice":
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }

                        int money = Convert.ToInt32(text);

                        if (money > Main.Players[player].Money)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет столько денег!", 3000);
                            return;
                        }

                        if (money <= 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Неправильное значение суммы", 3000);
                            return;
                        }

                        if (!player.HasData("DICE_TARGET") || player.GetData<Player>("DICE_TARGET") == null) return;

                        Player target = player.GetData<Player>("DICE_TARGET");
                        target.SetData("DICE_TARGET", player);
                        target.SetData("DICE_MONEY", money);

                        Trigger.PlayerEvent(target, "openDialog", "DICE", $"Вам предлагают сыграть в кости ({money}$) Гражданин ({player.Value})");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы кинули запрос на игру в кости Гражданину ({target.Value}))", 3000);

                        return;
                    case "gun_enterpass":
                        if (!player.HasData("ARENAID") || !GunGame.Arenas.ContainsKey(player.GetData<int>("ARENAID"))) return;
                        Arena arena = GunGame.Arenas[player.GetData<int>("ARENAID")];

                        player.ResetData("ARENAID");

                        if (arena.Pass != text)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Не правильный пароль!", 3000);
                            return;
                        }

                        NAPI.ClientEvent.TriggerClientEvent(player, "client::openmenu");

                        arena.Players.Add(player);

                        arena.SetLobby(player);
                        arena.RefreshPlayers();

                        player.SetData("ARENA", arena);

                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы вошли в лобби!", 3000);

                        return;
                    case "org_buymats": // ПОКУПКА МАТОВ
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        if (Convert.ToInt32(text) < 1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }

                        MaterialsI.MaterialsGet.SetDist(player, player.GetData<int>("WAYORDER"), Convert.ToInt32(text));
                        return;
                    case "org_enterid": // ВВОД БИЗНЕСА
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        if (Convert.ToInt32(text) < 1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        player.SetData("WAYORDER", Convert.ToInt32(text));
                        Trigger.PlayerEvent(player, "openInput", "Количество материалов", "Введите количество материалов", 8, "org_buymats");
                        return;
                    case "org_enterid2": // ВВОД БИЗНЕСА
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        if (Convert.ToInt32(text) < 1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
						if (!BCore.BizList.ContainsKey(Convert.ToInt32(text))) return;
                        BCore.Bizness biz = BCore.BizList[Convert.ToInt32(text)];
                        Vector3 pos2 = biz.MaterialsPosition;
                        Trigger.PlayerEvent(player, "createWaypoint", pos2.X, pos2.Y);
                        Trigger.PlayerEvent(player, "createCheckpoint", 210, 1, pos2 - new Vector3(0, 0, 1f), 7, 0, 255, 0, 0);
                        player.SetData("ORDER_MAT", Convert.ToInt32(text));
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Поставлен пункт назначения", 3000);
                        return;
                    case "org_otrmats": // ВЫГРУЗКА МАТОВ
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        if (Convert.ToInt32(text) < 1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        int wow = Convert.ToInt32(text);
                        if (!player.IsInVehicle) return;
                        if (wow > player.Vehicle.GetData<int>("MATERIALS"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет столько материалов. В грузовике {player.Vehicle.GetData<int>("MATERIALS")} Мат.", 3000);
                            return;
                        }
                        biz = BCore.BizList[player.GetData<int>("ORDER_MAT")];
                        int max = biz.GetMaxMaterials();
                        int mats = wow;
                        if (biz.Materials + wow > biz.GetMaxMaterials())
						{
							Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В бизнесе нет столько места! {biz.Materials} / {biz.GetMaxMaterials()}", 3000);
                            return;
						}
                        if (player.HasData("ORDER"))
                            if (player.GetData<OOrders.Order>("ORDER").Bizness == player.GetData<int>("ORDER_MAT"))
                                OOrders.ToOrder(player, mats);

                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы доставили груз -{mats} Мат.", 3000);
                        BCore.BizList[player.GetData<int>("ORDER_MAT")].Materials += mats;
                        BCore.BizList[player.GetData<int>("ORDER_MAT")].Save();
                        player.ResetData("ORDER_MAT");

                        int procent = mats / 100;

                        MoneySystem.Bank.Accounts[Organization.OCore.OrgListNAME[Main.Players[player].Org].BankID].Balance += procent * 10;
                        MoneySystem.Bank.Save(Organization.OCore.OrgListNAME[Main.Players[player].Org].BankID);



                        player.Vehicle.SetData("MATERIALS", player.Vehicle.GetData<int>("MATERIALS") - mats);
                        
                        Trigger.PlayerEvent(player, "deleteCheckpoint", 210);
                        Trigger.PlayerEvent(player, "openInput", "Ввод бизнеса", "Введите ID бизнеса", 4, "org_enterid2");
                        return;
                    case "club_setprice":
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        if (Convert.ToInt32(text) < 1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        Fractions.AlcoFabrication.SetAlcoholPrice(player, Convert.ToInt32(text));
                        return;
                    case "player_offerapartsell":
                        int price = 0;
                        if (!Int32.TryParse(text, out price) || price <= 0)
                        {
                            Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }

                        target = player.GetData<Player>("SELECTEDPLAYER");
                        if (!Main.Players.ContainsKey(target) || player.Position.DistanceTo(target.Position) > 2)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин слишком далеко от Вас", 3000);
                            return;
                        }

                        Houses.HouseManager.OfferApartSell(player, target, price);
                        return;
                    case "player_offerhousesell":
                        price = 0;
                        if (!Int32.TryParse(text, out price) || price <= 0)
                        {
                            Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }

                        target = player.GetData<Player>("SELECTEDPLAYER");
                        if (!Main.Players.ContainsKey(target) || player.Position.DistanceTo(target.Position) > 2)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин слишком далеко от Вас", 3000);
                            return;
                        }

                        Houses.HouseManager.OfferHouseSell(player, target, price);
                        return;
                    case "buy_drugs":
                        int amount = 0;
                        if (!Int32.TryParse(text, out amount))
                        {
                            Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        if (amount <= 0) return;

                        Fractions.Gangs.BuyDrugs(player, amount);
                        return;
                    case "mayor_take":
                        if (!Fractions.Manager.isLeader(player, 6)) return;

                        amount = 0;
                        try
                        {
                            amount = Convert.ToInt32(text);
                            if (amount <= 0) return;
                        }
                        catch { return; }

                        if (amount > Fractions.Cityhall.canGetMoney)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете получить больше {Fractions.Cityhall.canGetMoney}$ сегодня", 3000);
                            return;
                        }

                        if (Fractions.Stocks.fracStocks[6].Money < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств в казне", 3000);
                            return;
                        }
                        MoneySystem.Bank.Change(Players[player].Bank, amount);
                        Fractions.Stocks.fracStocks[6].Money -= amount;
                        GameLog.Money($"frac(6)", $"bank({Main.Players[player].Bank})", amount, "treasureTake");
                        return;
                    case "mayor_put":
                        if (!Fractions.Manager.isLeader(player, 6)) return;

                        amount = 0;
                        try
                        {
                            amount = Convert.ToInt32(text);
                            if (amount <= 0) return;
                        }
                        catch { return; }

                        if (!MoneySystem.Bank.Change(Players[player].Bank, -amount))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[6].Money += amount;
                        GameLog.Money($"bank({Main.Players[player].Bank})", $"frac(6)", amount, "treasurePut");
                        return;
                    case "call_police":
                        if (text.Length == 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите причину", 3000);
                            return;
                        }
                        Fractions.Police.callPolice(player, text);
                        break;
                    case "loadmats":
                    case "unloadmats":
                    case "loaddrugs":
                    case "unloaddrugs":
                    case "loadmedkits":
                    case "unloadmedkits":
                        Fractions.Stocks.fracgarage(player, callback, text);
                        break;
                    case "player_givemoney":
                        Selecting.playerTransferMoney(player, text);
                        return;
                    case "player_medkit":
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                            return;
                        }
                        if (!player.HasData("SELECTEDPLAYER") || player.GetData<Player>("SELECTEDPLAYER") == null || !Main.Players.ContainsKey(player.GetData<Player>("SELECTEDPLAYER"))) return;
                        Fractions.FractionCommands.sellMedKitToTarget(player, player.GetData<Player>("SELECTEDPLAYER"), Convert.ToInt32(text));
                        return;
                    case "player_heal":
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                            return;
                        }
                        if (!player.HasData("SELECTEDPLAYER") || player.GetData<Player>("SELECTEDPLAYER") == null || !Main.Players.ContainsKey(player.GetData<Player>("SELECTEDPLAYER"))) return;
                        Fractions.FractionCommands.healTarget(player, player.GetData<Player>("SELECTEDPLAYER"), Convert.ToInt32(text));
                        return;
                    case "put_stock":
                    case "take_stock":
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        if (Convert.ToInt32(text) < 1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        if (Admin.IsServerStoping)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Сервер сейчас не может принять это действие", 3000);
                            return;
                        }
                        Fractions.Stocks.inputStocks(player, 0, callback, Convert.ToInt32(text));
                        return;
                    case "sellcar":
                        if (!player.HasData("SELLCARFOR")) return;
                        target = player.GetData<Player>("SELLCARFOR");
                        if (!Main.Players.ContainsKey(target) || player.Position.DistanceTo(target.Position) > 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин находится слишком далеко от Вас", 3000);
                            return;
                        }
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        price = Convert.ToInt32(text);
                        if (price < 1 || price > 100000000)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }

                        Houses.House house = Houses.HouseManager.GetHouse(target, true);
                        Houses.House apart = Houses.HouseManager.GetApart(target, true);

                        if (house == null)
                            if (apart != null)
                                house = apart;

                        if (house == null && VehicleManager.getAllPlayerVehicles(target.Name.ToString()).Count > 1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Гражданина нет личного дома", 3000);
                            return;
                        }
                        if (house != null)
                        {
                            if (house.GarageID == 0 && VehicleManager.getAllPlayerVehicles(target.Name.ToString()).Count > 1)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Гражданина нет гаража", 3000);
                                return;
                            }
                            Houses.Garage garage = Houses.GarageManager.Garages[house.GarageID];
                            if (VehicleManager.getAllPlayerVehicles(target.Name).Count - 1 >= Houses.GarageManager.GarageTypes[garage.Type].MaxCars)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Гражданина уже максимальное кол-во машин", 3000);
                                return;
                            }
                        }
                        if (Main.Players[target].Money < price)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Гражданина недостаточно средств", 3000);
                            return;
                        }

                        string number = player.GetData<string>("SELLCARNUMBER");
                        if (!VehicleManager.Vehicles.ContainsKey(number))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Такой машины больше не существует", 3000);
                            return;
                        }
						if (FineManager.HaveFine(number, player.Name))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У данного транспорта есть штрафы!", 3000);
                            return;
                        }
                        if (VehicleManager.Vehicles[number].Holder != player.Name)
                        {
                            Commands.SendToAdmins(1, $"Игрок {player.Name} пытается ломать систему, бань гада");
                            return;
                        }


                        string vName = VehicleManager.Vehicles[number].Model;
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили {target.Name} купить Ваш {ParkManager.GetNormalName(vName)} ({number}) за {price}$", 3000);

                        Trigger.PlayerEvent(target, "openDialog", "BUY_CAR", $"{player.Name} предложил Вам купить {ParkManager.GetNormalName(vName)} ({number}) за ${price} . Пробег {Convert.ToInt32( VehicleManager.Vehicles[number].Sell )} км.");
                        target.SetData("SELLDATE", DateTime.Now);
                        target.SetData("CAR_SELLER", player);
                        target.SetData("CAR_NUMBER", number);
                        target.SetData("CAR_PRICE", price);
                        return;
                    case "sellair":
                        if (!player.HasData("SELLCARFOR")) return;
                        target = player.GetData<Player>("SELLCARFOR");
                        if (!Main.Players.ContainsKey(target) || player.Position.DistanceTo(target.Position) > 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин находится слишком далеко от Вас", 3000);
                            return;
                        }
                        try
                        {
                            Convert.ToInt32(text);
                        }
                        catch
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }
                        price = Convert.ToInt32(text);
                        if (price < 1 || price > 600000000)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                            return;
                        }

                        if (Main.Players[target].Money < price)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Гражданина недостаточно средств", 3000);
                            return;
                        }

                        number = player.GetData<string>("SELLAIRNUMBER");
                        if (!AirVehicles.Airs.ContainsKey(number))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Такого воздушного транспорта больше не существует", 3000);
                            return;
                        }

                        vName = AirVehicles.Airs[number].Model;
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили {target.Name} купить Ваш {ParkManager.GetNormalName(vName)} ({number}) за {price}$", 3000);

                        Trigger.PlayerEvent(target, "openDialog", "BUY_AIR", $"{player.Name} предложил Вам купить {ParkManager.GetNormalName(vName)} ({number}) за ${price}");
                        target.SetData("SELLDATE", DateTime.Now);
                        target.SetData("CAR_SELLER", player);
                        target.SetData("CAR_NUMBER", number);
                        target.SetData("CAR_PRICE", price);
                        return;
                    case "setMfamily":
                        {
                            if (!Main.Players.ContainsKey(player)) return;
                            if (string.IsNullOrEmpty(Main.Players[player].FamilyCID)) return;
                            try
                            {
                                Convert.ToInt32(text);
                            }
                            catch
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            int moneyinfamily = Convert.ToInt32(text);

                            if (moneyinfamily < 0) return;

                            Golemo.Families.Family fam = Golemo.Families.Family.GetFamilyToCid(player);

                            if (fam.Money + moneyinfamily > fam.MaxUpdates[0])
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Ваш общак не может столько вместить, максимум {fam.MaxUpdates[0]} $", 3000);
                                return;
                            }

                            MoneySystem.Wallet.Change(player, -moneyinfamily);

                            fam.AddMoney(moneyinfamily);

                            return;
                        }
                    case "getMfamily":
                        {
                            if (!Main.Players.ContainsKey(player)) return;
                            if (string.IsNullOrEmpty(Main.Players[player].FamilyCID)) return;
                            if (Main.Players[player].FamilyRank < 9)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Снять деньги может только 9 и 10 ранги", 3000);
                                return;
                            }
                            try
                            {
                                Convert.ToInt32(text);
                            }
                            catch
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }

                            int moneyinfamily = Convert.ToInt32(text);

                            if (moneyinfamily < 0) return;

                            Golemo.Families.Family fam = Golemo.Families.Family.GetFamilyToCid(player);

                            if (fam.Money < moneyinfamily)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нет столько средств в казне семьи", 3000);
                                return;
                            }

                            MoneySystem.Wallet.Change(player, moneyinfamily);

                            fam.AddMoney(-moneyinfamily);

                            return;
                        }
                    case "gotopark":
                        {
                            if (player.Vehicle == null) return;
                            int carplace = SellCars.FindFreeParkPlace();
                            if (carplace == -1)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На парковке нет свободного места", 3000);
                            }
                            try
                            {
                                Convert.ToInt32(text);
                            }
                            catch
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            SellCars.vehlist[carplace].SetVehicle(player.Vehicle, player, Convert.ToInt32(text));
                            return;
                        }
                    case "donat_biz10days":
                        {
                            if (Main.Accounts[player].RedBucks < 500)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            if (Main.Players[player].BizIDs.Count == 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "У вас нет бизнеса!", 3000);
                                return;
                            }
                            try
                            {
                                Convert.ToInt32(text);
                            }
                            catch
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            int idbiz = Convert.ToInt32(text);
                            BCore.Bizness bizness = BCore.BizList[idbiz];
                            if (bizness == null || bizness.Owner != player.Name || bizness.Cost == 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не владелец данного бизнеса", 3000);
                                return;
                            }
                            Main.Accounts[player].RedBucks -= 500;
                            GameLog.Money(Main.Accounts[player].Login, "server", 500, "donatebiz10");
                            MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");

                                bizness.Upgrade = DateTime.Now.AddDays(10);

                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы приобрели 10% для бизнеса на 10 дней!", 3000);

                            MySQL.Query($"UPDATE businesses SET upgrade='{MySQL.ConvertTime( bizness.Upgrade )}' WHERE id='{bizness.ID}'");

                            return;
                        }
                    case "donat_biz30days":
                        {
                            if (Main.Accounts[player].RedBucks < 1250)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            if (Main.Players[player].BizIDs.Count == 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "У вас нет бизнеса!", 3000);
                                return;
                            }
                            try
                            {
                                Convert.ToInt32(text);
                            }
                            catch
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            int idbiz = Convert.ToInt32(text);
                            BCore.Bizness bizness = BCore.BizList[idbiz];
                            if (bizness == null || bizness.Owner != player.Name || bizness.Cost == 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не владелец данного бизнеса", 3000);
                                return;
                            }
                            Main.Accounts[player].RedBucks -= 1250;
                            GameLog.Money(Main.Accounts[player].Login, "server", 1250, "donatebiz10");
                            MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");

                                bizness.Upgrade = DateTime.Now.AddDays(30);

                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы приобрели 10% для бизнеса на 30 дней!", 3000);

                            MySQL.Query($"UPDATE businesses SET upgrade='{MySQL.ConvertTime(bizness.Upgrade)}' WHERE id='{bizness.ID}'");

                            return;
                        }
                    case "item_drop":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");
                            Character acc = Main.Players[player];
                            List<nItem> items = nInventory.Items[acc.UUID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;
                            if (Int32.TryParse(text, out int dropAmount))
                            {
                                if (dropAmount <= 0) return;
                                if (item.Count < dropAmount)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                    return;
                                }
                                nInventory.Remove(player, item.Type, dropAmount);
                                Items.onDrop(player, new nItem(item.Type, dropAmount, item.Data), null);
                            }
                            else
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Некорректные данные", 3000);
                                return;
                            }
                        }
                        return;
                    case "item_transfer_toveh":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");
                            Character acc = Main.Players[player];
                            List<nItem> items = nInventory.Items[acc.UUID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int transferAmount;
                            if (Int32.TryParse(text, out transferAmount))
                            {
                                if (transferAmount <= 0) return;
                                if (item.Count < transferAmount)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                    return;
                                }

                                Vehicle veh = player.GetData<Vehicle>("SELECTEDVEH");
                                if (veh == null) return;
                                if (veh.Dimension != player.Dimension)
                                {
                                    Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-INVENTORY-EXPLOIT] {player.Name} ({player.Value}) dimension");
                                    return;
                                }
                                if (veh.Position.DistanceTo(player.Position) > 10f)
                                {
                                    Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-INVENTORY-EXPLOIT] {player.Name} ({player.Value}) distance");
                                    return;
                                }

                                if (item.Type == ItemType.Material)
                                {
                                    int maxMats = (Fractions.Stocks.maxMats.ContainsKey(veh.DisplayName)) ? Fractions.Stocks.maxMats[veh.DisplayName] : 600;
                                    if (VehicleInventory.GetCountOfType(veh, ItemType.Material) + transferAmount > maxMats)
                                    {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно загрузить такое кол-во матов", 3000);
                                        return;
                                    }
                                }

                                int tryAdd = VehicleInventory.TryAdd(veh, new nItem(item.Type, transferAmount));
                                if (tryAdd == -1 || tryAdd > 0)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "В машине недостаточно места", 3000);
                                    return;
                                }

                                VehicleInventory.Add(veh, new nItem(item.Type, transferAmount, item.Data));
                                nInventory.Remove(player, item.Type, transferAmount);
                                GameLog.Items($"player({Main.Players[player].UUID})", $"vehicle({veh.NumberPlate})", Convert.ToInt32(item.Type), transferAmount, $"{item.Data}");
                            }
                            else
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Некорретные данные", 3000);
                                return;
                            }
                        }
                        return;
                    case "item_transfer_tosafe":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");
                            Character acc = Main.Players[player];
                            List<nItem> items = nInventory.Items[acc.UUID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int transferAmount = Convert.ToInt32(text);
                            if (transferAmount <= 0) return;
                            if (item.Count < transferAmount)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                return;
                            }

                            if (Main.Players[player].InsideHouseID == -1) return;
                            int houseID = Main.Players[player].InsideHouseID;
                            int furnID = player.GetData<int>("OpennedSafe");

                            int tryAdd = Houses.FurnitureManager.TryAdd(houseID, furnID, item);
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в сейфе", 3000);
                                return;
                            }

                            nInventory.Remove(player, item.Type, transferAmount);
                            Houses.FurnitureManager.Add(houseID, furnID, new nItem(item.Type, transferAmount));
                        }
                        return;
                    case "item_transfer_tofracstock":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");
                            Character acc = Main.Players[player];
                            List<nItem> items = nInventory.Items[acc.UUID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int transferAmount = Convert.ToInt32(text);
                            if (transferAmount <= 0) return;
                            if (item.Count < transferAmount)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                return;
                            }

                            if (!player.HasData("ONFRACSTOCK")) return;
                            int onFraction = player.GetData<int>("ONFRACSTOCK");
                            if (onFraction == 0) return;

                            int tryAdd = Fractions.Stocks.TryAdd(onFraction, item);
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места на складе", 3000);
                                return;
                            }

                            nInventory.Remove(player, item.Type, transferAmount);
                            Fractions.Stocks.Add(onFraction, new nItem(item.Type, transferAmount));
                            GameLog.Items($"player({Main.Players[player].UUID})", $"fracstock({onFraction})", Convert.ToInt32(item.Type), transferAmount, $"{item.Data}");
                            GameLog.Stock(Players[player].FractionID, Players[player].UUID, $"{nInventory.ItemsNames[(int)item.Type]}", transferAmount, false);
                        }
                        return;
                    case "item_transfer_toplayer":
                        {
                            if (!player.HasData("CHANGE_WITH") || !Players.ContainsKey(player.GetData<Player>("CHANGE_WITH")))
                            {
                                player.ResetData("CHANGE_WITH");
                                return;
                            }
                            Player changeTarget = player.GetData<Player>("CHANGE_WITH");

                            if (player.Position.DistanceTo(changeTarget.Position) > 2) return;

                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");
                            Character acc = Main.Players[player];
                            List<nItem> items = nInventory.Items[acc.UUID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int transferAmount = Convert.ToInt32(text);
                            if (transferAmount <= 0) return;
                            if (item.Count < transferAmount)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                GUI.Dashboard.OpenOut(player, new List<nItem>(), changeTarget.Name, 5);
                                return;
                            }


                            int tryAdd = nInventory.TryAdd(changeTarget, new nItem(item.Type, transferAmount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У гражданина недостаточно места", 3000);
                                GUI.Dashboard.OpenOut(player, new List<nItem>(), changeTarget.Name, 5);
                                return;
                            }

                            nInventory.Add(changeTarget, new nItem(item.Type, transferAmount));
                            nInventory.Remove(player, item.Type, transferAmount);
                            GameLog.Items($"player({Main.Players[player].UUID})", $"player({Main.Players[changeTarget].UUID})", Convert.ToInt32(item.Type), transferAmount, $"{item.Data}");

                            GUI.Dashboard.OpenOut(player, new List<nItem>(), changeTarget.Name, 5);
                        }
                        return;
                    case "item_transfer_fromveh":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");

                            Vehicle veh = player.GetData<Vehicle>("SELECTEDVEH");
                            List<nItem> items = veh.GetData<List<nItem>>("ITEMS");
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int count = VehicleInventory.GetCountOfType(veh, item.Type);
                            int transferAmount;
                            if (Int32.TryParse(text, out transferAmount))
                            {
                                if (transferAmount <= 0) return;
                                if (count < transferAmount)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В машине нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                    return;
                                }

                                int tryAdd = nInventory.TryAdd(player, new nItem(item.Type, transferAmount));
                                if (tryAdd == -1 || tryAdd > 0)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                    return;
                                }
                                VehicleInventory.Remove(veh, item.Type, transferAmount);
                                nInventory.Add(player, new nItem(item.Type, transferAmount, item.Data));
                                GameLog.Items($"vehicle({veh.NumberPlate})", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), transferAmount, $"{item.Data}");
                            }
                            else
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Некорретные данные", 3000);
                                return;
                            }
                        }
                        return;
                    case "item_transfer_fromsafe":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");

                            if (Main.Players[player].InsideHouseID == -1) return;
                            int houseID = Main.Players[player].InsideHouseID;
                            int furnID = player.GetData<int>("OpennedSafe");
                            Houses.HouseFurniture furniture = Houses.FurnitureManager.HouseFurnitures[houseID][furnID];

                            List<nItem> items = Houses.FurnitureManager.FurnituresItems[houseID][furnID];
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int count = Houses.FurnitureManager.GetCountOfType(houseID, furnID, item.Type);
                            int transferAmount = Convert.ToInt32(text);
                            if (transferAmount <= 0) return;
                            if (count < transferAmount)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В ящике нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                return;
                            }
                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type, transferAmount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            nInventory.Add(player, new nItem(item.Type, transferAmount));
                            Houses.FurnitureManager.Remove(houseID, furnID, item.Type, transferAmount);
                        }
                        return;
                    case "item_transfer_fromfracstock":
                        {
                            int index = player.GetData<int>("ITEMINDEX");
                            ItemType type = player.GetData<ItemType>("ITEMTYPE");

                            if (!player.HasData("ONFRACSTOCK")) return;
                            int onFraction = player.GetData<int>("ONFRACSTOCK");
                            if (onFraction == 0) return;

                            List<nItem> items = Fractions.Stocks.fracStocks[onFraction].Weapons;
                            if (items.Count <= index) return;
                            nItem item = items[index];
                            if (item.Type != type) return;

                            int count = Fractions.Stocks.GetCountOfType(onFraction, item.Type);
                            int transferAmount = Convert.ToInt32(text);
                            if (transferAmount <= 0) return;
                            if (count < transferAmount)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На складе нет столько {nInventory.ItemsNames[(int)item.Type]}", 3000);
                                return;
                            }
                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type, transferAmount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            nInventory.Add(player, new nItem(item.Type, transferAmount));
                            Fractions.Stocks.Remove(onFraction, new nItem(item.Type, transferAmount));
                            GameLog.Stock(Players[player].FractionID, Players[player].UUID, $"{nInventory.ItemsNames[(int)item.Type]}", transferAmount, true);
                            Manager.FracLogs[Main.Players[player].FractionID].Add(new List<object> { DateTime.Now.ToString("dd.MM.yyyy"), $"{DateTime.Now.Hour}:{DateTime.Now.Minute}", player.Name, $"{nInventory.ItemsNames[(int)item.Type]}", transferAmount });
                            GameLog.Items($"fracstock({onFraction})", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), transferAmount, $"{item.Data}");
                        }
                        return;
                    case "weaptransfer":
                        {
                            int ammo = 0;
                            if (!Int32.TryParse(text, out ammo)) {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            if (ammo <= 0) return;

                        }
                        return;
                    case "extend_hotel_rent":
                        {
                            int hours = 0;
                            if (!Int32.TryParse(text, out hours))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            if (hours <= 0)
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            Houses.Hotel.ExtendHotelRent(player, hours);
                        }
                        return;
						case "banktobiz":
                        {
                            var acc = Main.Players[player];

                            BCore.Bizness bizf = BCore.BizList[acc.BizIDs[0]];

                            if (string.IsNullOrEmpty(text) || text.Contains("'"))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            int nums;
                            if (!Int32.TryParse(text, out nums))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Некорректные данные", 3000);
                                return;
                            }
                                var maxMoney = Convert.ToInt32(bizf.Cost / 100 * 0.005) * 24 * 7;
                            if (Bank.Accounts[bizf.BankID].Balance + Math.Abs(nums) > maxMoney)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно перевести столько средств на счет бизнеса.", 3000);
                                return;
                            }
                            Bank.Transfer(acc.Bank, bizf.BankID, Math.Abs(nums));
                            GameLog.Money($"player({Main.Players[player].UUID})", $"bank({bizf.BankID})", Math.Abs(nums), $"atmBiz");
                            //Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Успешный перевод.", 3000);
                        }
                        break;
                    case "banktoapart":
                        {
                            var acc = Main.Players[player];
                            var houses = Houses.HouseManager.GetApart(player, true);
                            if (houses == null) return;

                            if (string.IsNullOrEmpty(text) || text.Contains("'"))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            int nums;
                            if (!Int32.TryParse(text, out nums))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Некорректные данные", 3000);
                                return;
                            }

                            var maxMoney = Convert.ToInt32(houses.Price / 100 * 0.005) * 24 * 7;
                            if (Bank.Accounts[houses.BankID].Balance + Math.Abs(nums) > maxMoney)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно перевести столько средств на счет квартиры.", 3000);
                                return;
                            }
                            Bank.Transfer(acc.Bank, houses.BankID, Math.Abs(nums));
                            GameLog.Money($"player({Main.Players[player].UUID})", $"bank({houses.BankID})", Math.Abs(nums), $"atmApart");
                            //Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Успешный перевод.", 3000);
                        }
                        break;
                    case "banktohouse":
                        {
                            var acc = Main.Players[player];
                            var houses = Houses.HouseManager.GetHouse(player, true);
                            if (houses == null) return;

                            if (string.IsNullOrEmpty(text) || text.Contains("'"))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            int nums;
                            if (!Int32.TryParse(text, out nums))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Некорректные данные", 3000);
                                return;
                            }

                            var maxMoney = Convert.ToInt32(houses.Price / 100 * 0.005) * 24 * 7;
                            if (Bank.Accounts[houses.BankID].Balance + Math.Abs(nums) > maxMoney)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно перевести столько средств на счет дома.", 3000);
                                return;
                            }
                            Bank.Transfer(acc.Bank, houses.BankID, Math.Abs(nums));
                            GameLog.Money($"player({Main.Players[player].UUID})", $"bank({houses.BankID})", Math.Abs(nums), $"atmHouse");
                            //Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Успешный перевод.", 3000);
                        }
                        break;
                    case "banktoplayer":
                        {
                            var acc = Main.Players[player];

                            if (string.IsNullOrEmpty(text) || text.Contains("'"))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            int nums;
                            if (!Int32.TryParse(text, out nums))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Некорректные данные", 3000);
                                return;
                            }
                            if (Main.Players[player].LVL < 1)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Перевод денег доступен после первого уровня", 3000);
                                return;
                            }
                            if (player.HasData("NEXT_BANK_TRANSFER") && DateTime.Now < player.GetData<DateTime>("NEXT_BANK_TRANSFER"))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Следующая транзакция будет возможна в течение минуты", 3000);
                                return;
                            }
                            int bank = nums;
                            if (!Bank.Accounts.ContainsKey(bank) || bank <= 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Счет не найден!", 3000);
                                return;
                            }
                            if (Bank.Accounts[bank].Type != 1 && Main.Players[player].AdminLVL == 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Счет не найден!", 3000);
                                return;
                            }
                            if (acc.Bank == bank)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Операция отменена.", 3000);
                                return;
                            }
                            player.SetData("ATMTRANS", nums);
                            Trigger.PlayerEvent(player, "openInput", $"Перевод на счёт", "Введите количество:", 7, "banktotrans");
                            return;
                        }
                    case "banktotrans":
                        {
                            var acc = Main.Players[player];

                            if (string.IsNullOrEmpty(text) || text.Contains("'"))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            int nums;
                            if (!Int32.TryParse(text, out nums) || !player.HasData("ATMTRANS"))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Некорректные данные", 3000);
                                return;
                            }
                            Bank.Transfer(acc.Bank, player.GetData<int>("ATMTRANS"), Math.Abs(nums));
                            player.ResetData("ATMTRANS");
                            GameLog.Money($"player({Main.Players[player].UUID})", $"bank({player.GetData<int>("ATMTRANS")})", Math.Abs(nums), $"bankTransfer");
                            if (Main.Players[player].AdminLVL == 0) player.SetData("NEXT_BANK_TRANSFER", DateTime.Now.AddMinutes(1));
                        }
                        break;
                    case "smsadd":
                        {
                            if (string.IsNullOrEmpty(text) || text.Contains("'"))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            int num;
                            if (Int32.TryParse(text, out num))
                            {
                                if (Players[player].Contacts.Count >= Group.GroupMaxContacts[Accounts[player].VipLvl])
                                {
                                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "У Вас записано максимальное кол-во контактов", 3000);
                                    return;
                                }
                                if (Players[player].Contacts.ContainsKey(num))
                                {
                                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Контакт уже записан", 3000);
                                    return;
                                }
                                Players[player].Contacts.Add(num, num.ToString());
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы добавили новый контакт {num}", 3000);
                            }
                            else
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Некорректные данные", 3000);
                                return;
                            }

                        }
                        break;
                    case "numcall":
                        {
                            if (string.IsNullOrEmpty(text))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            int num;
                            if (Int32.TryParse(text, out num))
                            {
                                if (!SimCards.ContainsKey(num))
                                {
                                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Гражданина с таким номером не найдено", 3000);
                                    return;
                                }
                                Player t = GetPlayerByUUID(SimCards[num]);
                                Voice.Voice.PhoneCallCommand(player, t);
                            }
                            else
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                        }
                        return;
                    case "smssend":
                        {
                            if (string.IsNullOrEmpty(text))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            int num = player.GetData<int>("SMSNUM");
                            if (!SimCards.ContainsKey(num))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Гражданина с таким номером не найдено", 3000);
                                return;
                            }
                            Player t = GetPlayerByUUID(SimCards[num]);
                            if (t == null)
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Гражданин оффлайн", 3000);
                                return;
                            }
                            if (!MoneySystem.Bank.Change(Players[player].Bank, -10, false))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Недостаточно средств на банковском счете", 3000);
                                return;
                            }
                            //Fractions.Stocks.fracStocks[6].Money += 10;
                            GameLog.Money($"bank({Main.Players[player].Bank})", $"frac(6)", 10, "sms");
                            int senderNum = Main.Players[player].Sim;
                            string senderName = (Players[t].Contacts.ContainsKey(senderNum)) ? Players[t].Contacts[senderNum] : senderNum.ToString();
                            string msg = $"Сообщение от {senderName}: {text}";
                            t.SendChatMessage("~o~" + msg);
                            Notify.Send(t, NotifyType.Info, NotifyPosition.CenterRight, msg, 2000 + msg.Length * 70);

                            string notif = $"Сообщение для {Players[player].Contacts[num]}: {text}";
                            player.SendChatMessage("~o~" + notif);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.CenterRight, notif, 2000 + msg.Length * 50);
                        }
                        break;
                    case "smsname":
                        {
                            if (string.IsNullOrEmpty(text))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            if (text.Contains('"'.ToString()) || text.Contains("'") || text.Contains("[") || text.Contains("]") || text.Contains(":") || text.Contains("|") || text.Contains("\"") || text.Contains("`") || text.Contains("$") || text.Contains("%") || text.Contains("@") || text.Contains("{") || text.Contains("}") || text.Contains("(") || text.Contains(")"))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Имя содержит запрещенный символ.", 3000);
                                return;
                            }
                            int num = player.GetData<int>("SMSNUM");
                            string oldName = Players[player].Contacts[num];
                            Players[player].Contacts[num] = text;
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы переименовали {oldName} в {text}", 3000);
                        }
                        break;
                    case "make_ad":
                        {
                            if (string.IsNullOrEmpty(text))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }

                            if (player.HasData("NEXT_AD") && DateTime.Now < player.GetData<DateTime>("NEXT_AD"))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не можете подать объявление в данный момент", 3000);
                                return;
                            }

                            if (Fractions.LSNews.AdvertNames.Contains(player.Name))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас уже есть одно объявление в очереди", 3000);
                                return;
                            }

                            if (text.Length < 15)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Слишком короткое объявление", 3000);
                                return;
                            }

                            int adPrice = text.Length / 15 * 6;
                            if (!MoneySystem.Bank.Change(Main.Players[player].Bank, -adPrice, false))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас не хватает денежных средств в банке", 3000);
                                return;
                            }
                            Fractions.LSNews.AddAdvert(player, text, adPrice);
                        }
                        break;
					case "enter_promocode":
                        {
                            if (string.IsNullOrEmpty(text))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                                return;
                            }
                            if (Accounts[player].PromoCodes[0] != "noref" )
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас уже активирован промокод!", 3000);
                                return;
                            }
							if (Players[player].LVL > 1)
							{
								Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас должен быть 0 уровень", 3000);
								return;
							}
                            if (text.Equals("KFC"))
                            {
                                Accounts[player].PromoCodes[0] = text;
                                Main.Accounts[player].PresentGet = true;
                                GameLog.Money($"server", $"player({Main.Players[player].UUID})", 50000, $"KFC");
                                Customization.AddClothes(player, ItemType.Hat, 44, 3);
                                nInventory.Add(player, new nItem(ItemType.Sprunk, 3));
                                nInventory.Add(player, new nItem(ItemType.Сrisps, 3));
                                Main.Players[player].LVL = 1;
                                MoneySystem.Wallet.Change(player, 50000);
                                Main.Accounts[player].VipLvl = 3;
                                Main.Accounts[player].VipDate = DateTime.Now.AddDays(2);
                                GUI.Dashboard.sendStats(player);
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы получили первый уровень, Gold VIP на 2 дня и 50000$ за активацию промокода!", 6000);
                                NAPI.Task.Run(() => { try { Trigger.PlayerEvent(player, "disabledmg", false); } catch { } }, 5000);
                                MySQL.Query($"UPDATE accounts SET promocodes='{JsonConvert.SerializeObject(new List<string> { text })}' WHERE character1={Main.Players[player].UUID}");
                                return;
                            }
							if (!PromoCodes.ContainsKey(text))
							{
								Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Такого промокода не существует!", 3000);
								return;
							}

                            Main.Accounts[player].PresentGet = true;
                            GameLog.Money($"server", $"player({Main.Players[player].UUID})", 65000, $"KFC");
                            Customization.AddClothes(player, ItemType.Hat, 44, 3);
                            nInventory.Add(player, new nItem(ItemType.Sprunk, 3));
                            nInventory.Add(player, new nItem(ItemType.Сrisps, 3));
                            Main.Players[player].LVL = 1;
                            MoneySystem.Wallet.Change(player, 65000);
                            Main.Accounts[player].VipLvl = 4;
                            Main.Accounts[player].VipDate = DateTime.Now.AddDays(2);
                            GUI.Dashboard.sendStats(player);
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы получили первый уровень, Platinum VIP на 2 дня и 65000$ за активацию промокода!", 6000);
                            NAPI.Task.Run(() => { try { Trigger.PlayerEvent(player, "disabledmg", false); } catch { } }, 5000);

                            MySQL.Query($"UPDATE promocodes SET count=count+1 WHERE name='{text}'");
                            MySQL.Query($"UPDATE accounts SET promocodes='{JsonConvert.SerializeObject(new List<string> { text })}' WHERE character1={Main.Players[player].UUID}");
							Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Промокод успешно активирован!", 3000);
                            
                        }
                        break;
                    case "player_ticketsum":
                        int sum = 0;
                        if (!Int32.TryParse(text, out sum))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Некорректные данные", 3000);
                            return;
                        }
						if (sum <= 0)
						{
							Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Некорректные данные", 3000);
                            return;
						}
                        player.SetData("TICKETSUM", sum);
                        Trigger.PlayerEvent(player, "openInput", "Выписать штраф (причина)", "Причина", 50, "player_ticketreason");
                        break;
                    case "player_ticketreason":
                        Fractions.FractionCommands.ticketToTarget(player, player.GetData<Player>("TICKETTARGET"), player.GetData<int>("TICKETSUM"), text);
                        break;
                }
            }
            catch (Exception e) { Log.Write($"inputCallback/{callback}/: {e.ToString()}\n{e.StackTrace}", nLog.Type.Warn); }
        }

        [RemoteEvent("openPlayerMenu")]
        public async Task PlayerEvent_openPlayerMenu(Player player, params object[] arguments)
        {
            try
            {
                if (player.HasData("Phone"))
                {
                    MenuManager.Close(player);
                    //OpenPlayerMenu(player).Wait();
                    return;
                }

                Main.OnAntiAnim(player);

                //if (!player.IsInVehicle)
                    //Core.BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_amb_phone"), 6286, new Vector3(0.06, 0.01, -0.02), new Vector3(80, -10, 110));
                

                await OpenPlayerMenu(player);
                return;
            } catch (Exception e) { Log.Write("openPlayerMenu: " + e.Message, nLog.Type.Error); }
        }
                #region Account
        [RemoteEvent("selectchar")]
        public async void ClientEvent_selectCharacter(Player player, params object[] arguments)
        {
            try
            {
                if (!Accounts.ContainsKey(player)) return;
                await Log.WriteAsync($"{player.Name} select char");

                int slot = Convert.ToInt32(arguments[0].ToString());
                await SelecterCharacterOnTimer(player, player.Value, slot);
            }
            catch (Exception e) { Log.Write("newchar: " + e.Message, nLog.Type.Error); }
        }
        public async Task SelecterCharacterOnTimer(Player player, int value, int slot)
        {
            try
            {
                if (player.Value != value) return;
                if (!Accounts.ContainsKey(player)) return;

                Ban ban = Ban.Get2(Accounts[player].Characters[slot - 1]);
                if(ban != null)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Ты не пройдёшь!", 4000);
                    return;
                }
                
                Character character = new Character();
                await character.Load(player, Accounts[player].Characters[slot - 1]);
                return;
            }
            catch (Exception e) { Log.Write("selectcharTimer: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("newchar")]
        public async Task ClientEvent_newCharacter(Player player, params object[] arguments)
        {
            try
            {
                if (!Accounts.ContainsKey(player)) return;

                int slot = Convert.ToInt32(arguments[0].ToString());
                string firstname = arguments[1].ToString();
                string lastname = arguments[2].ToString();

                await Accounts[player].CreateCharacter(player, slot, firstname, lastname);
                return;
            }
            catch (Exception e) { Log.Write("newchar: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("delchar")]
        public async Task ClientEvent_deleteCharacter(Player player, params object[] arguments)
        {
            try
            {
                if (!Accounts.ContainsKey(player)) return;

                int slot = Convert.ToInt32(arguments[0].ToString());
                string firstname = arguments[1].ToString();
                string lastname = arguments[2].ToString();
                string pass = arguments[3].ToString();
                await Accounts[player].DeleteCharacter(player, slot, firstname, lastname, pass);
                return;
            }
            catch (Exception e) { Log.Write("transferchar: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("restorepass")]
        public async void RestorePassword_event(Player client, byte state, string loginorcode)
        {
            try
            {
                if (state == 0)
                { // Отправка кода
                    if (Emails.ContainsKey(loginorcode))  loginorcode = Emails[loginorcode];
                    else loginorcode = loginorcode.ToLower();
                    DataTable result = MySQL.QueryRead($"SELECT email, socialclub FROM `accounts` WHERE `login`='{loginorcode}'");
                    if (result == null || result.Rows.Count == 0)
                    {
                        Log.Debug($"Ошибка при попытке восстановить пароль от аккаунта!", nLog.Type.Warn);
                        return;
                    }
                    DataRow row = result.Rows[0];
                    string email = Convert.ToString(row["email"]);
                    string sc = row["socialclub"].ToString();
                    if (sc != client.GetData<string>("RealSocialClub"))
                    {
                        Log.Debug($"SocialClub не соответствует SocialClub при регистрации", nLog.Type.Warn);
                        return;
                    }
                    int mycode = Main.rnd.Next(1000, 10000);
                    if (Main.RestorePass.ContainsKey(client)) Main.RestorePass.Remove(client);
                    Main.RestorePass.Add(client, new Tuple<int, string, string, string>(mycode, loginorcode, client.GetData<string>("RealSocialClub"), email));
                    await Task.Run(() => {
                        PasswordRestore.SendEmail(0, email, mycode); // Отправляем сообщение на емейл с кодом для смены пароля
                    });
                }
                else
                { // Ввод кода и проверка
                    if (Main.RestorePass.ContainsKey(client))
                    {
                        if (client.GetData<string>("RealSocialClub") == Main.RestorePass[client].Item3)
                        {
                            if (Convert.ToInt32(loginorcode) == Main.RestorePass[client].Item1)
                            {
                                Log.Debug($"{client.GetData<string>("RealSocialClub")} удачно восстановил пароль!", nLog.Type.Info);
                                int newpas = Main.rnd.Next(1000000, 9999999);
                                await Task.Run(() => {
                                    PasswordRestore.SendEmail(1, Main.RestorePass[client].Item4, newpas); // Отправляем сообщение на емейл с новым паролем
                                });
                                Notify.Send(client, NotifyType.Success, NotifyPosition.BottomCenter, "Ваш пароль был сброшен, новый пароль отправлен на почту, смените его сразу же после входа командой /password", 10000);
                                MySQL.Query($"UPDATE `accounts` SET `password`='{Account.GetSha256(newpas.ToString())}' WHERE `login`='{Main.RestorePass[client].Item2}' AND `socialclub`='{Main.RestorePass[client].Item3}'");
                                await SignInOnTimer(client, Main.RestorePass[client].Item2, newpas.ToString());  // Отправляем в логин по этим данным
                                Main.RestorePass.Remove(client); // Удаляем из списка тех, кто восстанавливает пароль
                            } // тут можно else { // и считать сколько раз он ввёл неправильные данные
                        }
                        else client.Kick(); // Если SocialClub не совпадает, то кикаем от сбоев.
                    }
                    else client.Kick(); // Если его не было найдено в списке, то кикаем от сбоев.
                }
            }
            catch (Exception ex)
            {
                Log.Write("EXCEPTION AT \"RestorePass\":\n" + ex.ToString(), nLog.Type.Error);
                return;
            }
        }
        [RemoteEvent("signin")]
        public Task ClientEvent_signin(Player player, params object[] arguments)
        {
            NAPI.Task.Run( async () => { 
            try
            {
                if (player.HasData("CheatTrigger"))
               /* {
                    int cheatCode = player.GetData<object>("CheatTrigger");
                    if(cheatCode > 1)
                    {
                        Log.Write($"CheatKick: {((Cheat)cheatCode).ToString()} on {player.Name} ", nLog.Type.Warn);
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Непредвиденная ошибка! Попробуйте перезайти.", 10000);
                        player.Kick();
                        return;
                    }
                }*/

                await Log.WriteAsync($"{player.Name} try to signin step 1");
                string login = arguments[0].ToString();
                string pass = arguments[1].ToString();
                
                await SignInOnTimer(player, login, pass);
            }
            catch (Exception e) { Log.Write("signin: " + e.Message, nLog.Type.Error); }
            });
            return Task.CompletedTask;
        }
        public async Task SignInOnTimer(Player player, string login, string pass)
        {

            try
            {
                if (Emails.ContainsKey(login))
                    login = Emails[login];
                else
                    login = login.ToLower();

                Ban ban = Ban.Get1(player);
                if (ban != null)
                {
                    if (ban.isHard && ban.CheckDate())
                    {
                        NAPI.Task.Run(() => Trigger.PlayerEvent(player, "kick", $"Вы заблокированы до {ban.Until.ToString()}. Причина: {ban.Reason} ({ban.ByAdmin})"));
                        return;
                    }
                }

                Account user = new Account();
                LoginEvent result = await user.LoginIn(player, login, pass);
                if (result == LoginEvent.Authorized)
                {
                    user.LoadSlots(player);
                }
                else if (result == LoginEvent.Already)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Аккаунт уже авторизован.", 3000);
                }
                else if (result == LoginEvent.Refused)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Данные введены неверно", 3000);
                }
                if (result == LoginEvent.SclubError)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "SocialClub, с которого Вы подключены, не совпадает с тем, который привязан к аккаунту.", 3000);
                }

                return;
            }
            catch (Exception e) { Log.Write("signin: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("signup")]
        public async Task ClientEvent_signup(Player player, params object[] arguments)
        {
            try
            {
                if (player.HasData("CheatTrigger"))
                {
                    int cheatCode = player.GetData<int>("CheatTrigger");
                    if (cheatCode > 1)
                    {
                        //Log.Write($"CheatKick: {((Cheat)cheatCode).ToString()} on {player.Name} ", nLog.Type.Warn);
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Непредвиденная ошибка! Попробуйте перезайти.", 10000);
                        player.Kick();
                        return;
                    }
                }

                string login = arguments[0].ToString().ToLower();
                string pass = arguments[1].ToString();
                string email = arguments[2].ToString();
                string promo = arguments[3].ToString();

                Ban ban = Ban.Get1(player);
                if (ban != null)
                {
                    if (ban.isHard && ban.CheckDate())
                    {
                        NAPI.Task.Run(() => Trigger.PlayerEvent(player, "kick", $"Вы заблокированы до {ban.Until.ToString()}. Причина: {ban.Reason} ({ban.ByAdmin})"));
                        return;
                    }
                }


                Account user = new Account();
                RegisterEvent result = await user.Register(player, login, pass, email, promo);
                if (result == RegisterEvent.Error)
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Непредвиденная ошибка!", 3000);
                else if (result == RegisterEvent.SocialReg)
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "На этот SocialClub уже зарегистрирован игровой аккаунт!", 3000);
                else if (result == RegisterEvent.UserReg)
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Данное имя пользователя уже занято!", 3000);
                else if (result == RegisterEvent.EmailReg)
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Данный email уже занят!", 3000);
                else if (result == RegisterEvent.DataError)
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ошибка в заполнении полей!", 3000);

                return;
            }
            catch (Exception e) { Log.Write("signup: " + e.Message, nLog.Type.Error); }
        }
        #endregion Account
        
        [RemoteEvent("engineCarPressed")]
        public void PlayerEvent_engineCarPressed(Player player, params object[] arguments)
        {
            try
            {
                VehicleManager.onPlayerEvent(player, "engineCarPressed", arguments);
                return;
            } catch (Exception e) { Log.Write("engineCarPressed: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("lockCarPressed")]
        public void PlayerEvent_lockCarPressed(Player player, params object[] arguments)
        {
            try
            {
                VehicleManager.onPlayerEvent(player, "lockCarPressed", arguments);
                return;
            }
            catch (Exception e) { Log.Write("lockCarPressed: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("OpenSafe")]
        public void PlayerEvent_OpenSafe(Player player, params object[] arguments)
        {
            try
            {
                SafeMain.openSafe(player, arguments);
                return;
            }
            catch (Exception e) { Log.Write("OpenSafe: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("InteractSafe")]
        public void PlayerEvent_InteractSafe(Player player, params object[] arguments)
        {
            try
            {
                SafeMain.interactSafe(player);
                return;
            }
            catch (Exception e) { Log.Write("InteractSafe: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("interactionPressed")]
        public void PlayerEvent_interactionPressed(Player player, params object[] arguments)
        {
            int intid = -404;
            try
            {
                #region
                int id = 0;
                try
                {
                    id = player.GetData<int>("INTERACTIONCHECK");
                    Log.Debug($"{player.Name} INTERACTIONCHECK IS {id}");
                }
                catch { }
                intid = id;
                switch (id)
                {
                    case 1:
                        Fractions.Cityhall.beginWorkDay(player);
                        return;
                    #region cityhall enterdoor
                    case 3:
                    case 4:
                    case 5:
                    case 62:
                        Fractions.Cityhall.interactPressed(player, id);
                        return;
                    #endregion
                    #region ems interact
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 51:
                    case 58:
                    case 63:
                        Fractions.Ems.interactPressed(player, id);
                        return;
                    #endregion
                    case 8:
                        Jobs.Electrician.StartWorkDay(player);
                        return;
                    case 9:
                        Fractions.Cityhall.OpenCityhallGunMenu(player);
                        return;
                    #region police interact
                    case 10:
                    case 11:
                    case 12:
                    case 42:
                    case 44:
                    case 59:
                    case 66:
                        Fractions.Police.interactPressed(player, id);
                        return;
                    #endregion
                    case 13:
                        MoneySystem.ATM.OpenATM(player);
                        return;
                    case 14:
                        SafeMain.interactPressed(player, id);
                        return;
					 case 71:
                        Trigger.PlayerEvent(player, "openDialog", "CONTAINER_OPEN", $"Вы хотите открыть контейнер?");
                        return;
                    case 72:
                        if (!player.HasData("CONTAINER")) return;
                        Dictionary<string, int> cars = player.GetData<Containers.Container>("CONTAINER").Cars;
                        List<List<string>> items = new List<List<string>>();

                        foreach (KeyValuePair<string, int> val in cars)
                        {
                            List<string> item = new List<string>();
                            item.Add(ParkManager.GetNormalName(val.Key));
                            item.Add("");
                            items.Add(item);
                        }

                        string json = JsonConvert.SerializeObject(items);
                        Trigger.PlayerEvent(player, "container", json);
                        return;	
                    #region fbi interact					
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                    case 26:
                    case 27:
                    case 24:
                    case 46:
                    case 61:
                        Fractions.Fbi.interactPressed(player, id);
                        return;
                    #endregion
                    case 28:
                        Jobs.WorkManager.openGoPostalStart(player);
                        return;
                    case 29:
                        Jobs.Gopostal.getGoPostalCar(player);
                        return;
                    case 30:
                        if (!Players.ContainsKey(player)) return;
                        if (player.GetData<int>("BIZ_ID") == -1) return;
                        BCore.BizList[player.GetData<int>("BIZ_ID")].InteractPress(player);
                        return;
                    case 31:
                        Jobs.Truckers.getOrderTrailer(player);
                        return;
                    case 32:
                    case 33:
                        Fractions.Stocks.interactPressed(player, id);
                        return;
                    case 34:
                    case 35:
                    case 36:
                    case 25:
                    case 60:
                        Fractions.Army.interactPressed(player, id);
                        return;
                    case 37:
                        Fractions.MatsWar.interact(player);
                        return;
                    case 38:
                        Customization.SendToCreator(player);
                        return;
                    case 39:
                        DrivingSchool.OpenDriveSchoolMenu(player);
                        return;
                    case 6:
                    case 7:
                        Houses.HouseManager.interactPressed(player, id);
                        return;
                    case 40:
                    case 41:
                        Houses.GarageManager.interactionPressed(player, id);
                        return;
                    case 43:
                        SafeMain.interactSafe(player);
                        return;
                    case 45:
                        Jobs.Collector.CollectorTakeMoney(player);
                        return;
                    case 128:
                        Jobs.TrashCar.SellTrash(player);
                        return;
                    case 129:
                        Jobs.TrashCar.PickupTrash(player, player.GetData<ColShape>("SHAPE"));
                        return;
                    case 47:
                        Fractions.Gangs.InteractPressed(player);
                        return;
                    case 48:
                    case 49:
                    case 50:
                        Houses.Hotel.Event_InteractPressed(player, id);
                        return;
                    case 52:
                        ParkManager.interactionPressed(player, id);
                        return;
                    case 53:
                        Fractions.CarDelivery.Event_InteractPressed(player, id);
                        return;
					#region Jobs Delivery
                    case 404:
                        Jobs.Delivery.interactPressed(player, id);
                        return;
                    #endregion
                    case 54:
                        Married.AgreeMessage(player);
                        return;
                    case 55:
                    case 56:
                        WorkManager.openJobsSelecting(player, player.GetData<int>("JOBID"));
                        return;
                    case 67:
                        if (player.Vehicle == null) return;
                        Trigger.PlayerEvent(player, "openInput", "Поставить машину на продажу", "Введите цену", 30, "gotopark");
                        return;
                    case 57:
                        Fractions.AlcoFabrication.Event_InteractPressed(player, id);
                        return;
                    case 75:
                        switch (player.GetData<int>("JOBID"))
                        {
                            case 10:
                                Jobs.Farm.StartWorkDay(player);
                                return;
                            case 11:
                                Jobs.Construction.StartWorkDay(player);
                                return;
                            case 12:
                                Jobs.Miner.StartWorkDay(player);
                                return;
                            case 13:
                                Jobs.Diver.StartWorkDay(player);
                                return;
                            case 14:
                                WorkManager.openJobsSelecting(player, 10);
                                return;
                            default:
                                return;
                        }
                    case 212:
                        Trigger.PlayerEvent(player, "openDialog", "COCO_SELL", $"Вы хотите продать листья коки {curcoco} / 1 шт. ?");
                        return;
                    case 64:
                        Fractions.Manager.enterInterier(player, player.GetData<int>("FRACTIONCHECK"));
                        return;
                    case 65:
                        Fractions.Manager.exitInterier(player, player.GetData<int>("FRACTIONCHECK"));
                        return;
                    case 80:
                    case 81:
                        Fractions.LSNews.beginWorkDay(player);
                        return;
                    case 82:
                    case 83:
                    case 84:
                    case 85:
                        Fractions.Merryweather.interactPressed(player, id);
                        return;
                    case 86:
                        Fractions.Merryweather.beginWorkDay(player);
                        return;
                    case 87:
                        Fractions.Mafia.OpenCityhallGunMenu(player);
                        return;
					case 88:
                    case 89:
                    case 90:
                    case 91:
                        Fractions.NacBez.interactPressed(player, id);
                        return;
                    case 92:
                        Fractions.NacBez.beginWorkDay(player);
                        return;
                    case 93:
                        Fractions.NacBez.OpenCityhallGunMenu(player);
                        return;	
					case 803: //todo FamilyCreatorMenu
                        Golemo.Families.Manager.OpenCreatorFamilyMenu(player);
                        return;	
                    case 500:
                        if(!Players[player].Achievements[0]) {
                            Players[player].Achievements[0] = true;
                            Trigger.PlayerEvent(player, "ChatPyBed", 0, 0);
                        } else if(!Players[player].Achievements[1]) Trigger.PlayerEvent(player, "ChatPyBed", 1, 0);
                        else if(Players[player].Achievements[2]) {
                            if(!Players[player].Achievements[3]) {
                                Players[player].Achievements[3] = true;
                                MoneySystem.Wallet.Change(player, 500);
                                Trigger.PlayerEvent(player, "ChatPyBed", 9, 0);
                            }
                        }
                        return;
					case 571:
                        InfoPed.Interact1(player);
						return;
                    case 501:
                        if(Players[player].Achievements[0]) {
                            if(!Players[player].Achievements[1]) {
                                player.SetData("CollectThings", 0);
                                Players[player].Achievements[1] = true;
                                if(Players[player].Gender) Trigger.PlayerEvent(player, "ChatPyBed", 2, 0);
                                else Trigger.PlayerEvent(player, "ChatPyBed", 3, 0);
                            } else if(!Players[player].Achievements[2]) {
                                if(player.HasData("CollectThings") && player.GetData<int>("CollectThings") >= 4) {
                                    Players[player].Achievements[2] = true;
                                    MoneySystem.Wallet.Change(player, 500);
                                    Trigger.PlayerEvent(player, "ChatPyBed", 7, 0);
                                } else { 
                                    if(Players[player].Gender) Trigger.PlayerEvent(player, "ChatPyBed", 4, 0);
                                    else Trigger.PlayerEvent(player, "ChatPyBed", 5);
                                }
                            }
                        }
                        return;
                    case 502:
                        if(Players[player].Achievements[1]) {
                            if(player.HasData("CollectThings")) {
                                if(player.GetData<int>("CollectThings") < 4) {
                                    if(!player.HasData("AntiAnimDown")) {
                                        if(Players[player].Gender) {
                                            if(!NAPI.ColShape.IsPointWithinColshape(Zone0, player.Position)) return;
                                        } else {
                                            if(!NAPI.ColShape.IsPointWithinColshape(Zone1, player.Position)) return;
                                        }
                                        OnAntiAnim(player);
                                        player.PlayAnimation("anim@mp_snowball", "pickup_snowball", 39);
                                        NAPI.Task.Run(() => {
                                            if (player != null && Main.Players.ContainsKey(player))
                                            {
                                                player.StopAnimation();
                                                OffAntiAnim(player);
                                                player.SetData("CollectThings", player.GetData<int>("CollectThings") + 1);
                                            }
                                        }, 1300);
                                    }
                                } else Trigger.PlayerEvent(player, "ChatPyBed", 6, 0);
                            }
                        }
                        return;
                    case 503:
                        if(!Players[player].Achievements[4] && !Players[player].Achievements[5]) { // Первый подход к Frank'у
                            if(Players[player].Achievements[2]) { //TODO: ветка, если игроку дали рекомендацию пойти к Фрэнку, Эмма порекомендовала игрока
                                if(Env_lastWeather.Equals("RAIN") || Env_lastWeather.Equals("THUNDER")) {
                                    Players[player].Achievements[4] = true;
                                    MoneySystem.Wallet.Change(player, 250);
                                    Trigger.PlayerEvent(player, "ChatPyBed", 10, 0);
                                } else {
                                    Players[player].Achievements[5] = true;
                                    Trigger.PlayerEvent(player, "ChatPyBed", 10, 1);
                                }
                            } else { //TODO: ветка, если игроку не давали рекомендацию, Фрэнк не слышал об игроке
                            }
                        } else if(Players[player].Achievements[6] && !Players[player].Achievements[7]) { // Подход к Фрэнку после выполнения миссии
                            Players[player].Achievements[7] = true;
                            MoneySystem.Wallet.Change(player, 250);
                            Trigger.PlayerEvent(player, "ChatPyBed", 13, 0);
                        } else if(Players[player].Achievements[7] && !Players[player].Achievements[8]) { // Взять второй квэст у Фрэнка
                            Players[player].Achievements[8] = true;
                            Trigger.PlayerEvent(player, "ChatPyBed", 14, 0);
                        } else if(Players[player].Achievements[8] && !Players[player].Achievements[9])  Trigger.PlayerEvent(player, "ChatPyBed", 15, 0);
                        else if(Players[player].Achievements[8] && Players[player].Achievements[9]) { // 
                            if(!Players[player].Achievements[10]) { // Еще не сдан квест с трактором у фрэнка
                                Players[player].Achievements[10] = true;
                                Trigger.PlayerEvent(player, "ChatPyBed", 16, 0);
                                MoneySystem.Wallet.Change(player, 500);
                            } else Trigger.PlayerEvent(player, "ChatPyBed", 17, 0);
                        }
                        return;
                    case 504:
                        if(Players[player].Achievements[5] && !Players[player].Achievements[6]) { // Если сейчас взята миссия Фрэнка
                            Players[player].Achievements[6] = true;
                            OnAntiAnim(player);
                            player.PlayAnimation("amb@prop_human_movie_studio_light@base", "base", 39);
                            NAPI.Task.Run(() => {
                                if (player != null && Main.Players.ContainsKey(player))
                                {
                                    player.StopAnimation();
                                    OffAntiAnim(player);
                                    player.SendChatMessage("Ну вот, насос включен, можно бежать к Фрэнку!");
                                }
                            }, 3000);
                        }
                        return;
                    case 505:
                        if(!Players[player].Achievements[9]) {
                            if(!player.IsInVehicle) return;
                            if(player.Vehicle != FrankQuest1Trac0 && player.Vehicle != FrankQuest1Trac1) return;
                            Players[player].Achievements[9] = true;
                            Vehicle trac = player.Vehicle;
                            player.WarpOutOfVehicle();
                            NAPI.Task.Run(() => {
                                if(trac == FrankQuest1Trac0) {
                                    trac.Position = new Vector3(1981.87, 5174.382, 48.26282);
                                    trac.Rotation = new Vector3(0.1017629, -0.1177645, 129.811);
                                } else {
                                    trac.Position = new Vector3(1974.506, 5168.247, 48.2662);
                                    trac.Rotation = new Vector3(0.07581472, -0.08908347, 129.8487);
                                }
                            }, 500);
                            player.SendChatMessage("Отлично, трактор на месте, давай скажем Фрэнку?");
                        }
                        return;
					#region Vinograd
                    case 400:
                    case 401:
                        Jobs.Vineyarad.interactPressed(player, id);
                        return;
					#endregion							
                    case 222:
                        SpawnCars.interactionPressed(player, id);
                        return;
                    case 223:
                        if (NAPI.Data.GetEntityData(player, "WORK") == null)
                        {
                            Trigger.PlayerEvent(player, "openDialog", "TRUCKER_RENT", $"Вы действительно хотите начать работу?");
                        }
                        else
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть арендованный грузовик", 3000);
                        return;
                    case 224:
                        if (player.GetData<Vehicle>("WORK") == null)
                        {
                            if (Main.Players[player].Money >= Jobs.Bus.BusRentCost)
                                Trigger.PlayerEvent(player, "openDialog", "BUS_RENT", $"Арендовать автобус за ${Jobs.Bus.BusRentCost}?");
                            else
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас не хватает " + (Jobs.Bus.BusRentCost - Main.Players[player].Money) + "$ на аренду автобуса", 3000);
                        }
                        else
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть арендованный автобус", 3000);
                        return;
                    case 225:
                        if (player.GetData<Vehicle>("WORK") == null)
                        {
                            if (Main.Players[player].Money >= Jobs.Taxi.taxiRentCost)
                                Trigger.PlayerEvent(player, "openDialog", "TAXI_RENT", $"Арендовать такси за ${Jobs.Taxi.taxiRentCost}?");
                            else
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас не хватает " + (Jobs.Taxi.taxiRentCost - Main.Players[player].Money) + "$ на аренду такси", 3000);
                        }
                        else
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть арендованное такси", 3000);
                        return;
                    case 226:
                        if (NAPI.Data.GetEntityData(player, "WORK") == null)
                        {
                            if (Main.Players[player].Money >= 20) 
                                Trigger.PlayerEvent(player, "openDialog", "COLLECTOR_RENT", "Вы действительно хотите начать работу инкассатором и арендовать транспорт за $20?");
                            else
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас не хватает " + (20 - Main.Players[player].Money) + "$ на аренду икассатора", 3000);
                        }
                        else
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть арендованный инкассатор", 3000);
                        return;
                    case 227:
                        if (NAPI.Data.GetEntityData(player, "WORK") == null)
                        {
                            Trigger.PlayerEvent(player, "openDialog", "TRASH_RENT", "Начать работу водителя мусоровоза?");
                        }
                        else
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть арендованный мусоровоз", 3000);
                        return;
                    case 228:
                        if (NAPI.Data.GetEntityData(player, "WORK") == null)
                        {
                            Trigger.PlayerEvent(player, "openDialog", "MOWER_RENT", $"Начать работу газонокосильщиком?");
                        }
                        else
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть арендованный транспорт", 3000);
                        return;
                    case 229:
                        if (NAPI.Data.GetEntityData(player, "WORK") == null)
                        {
                            Trigger.PlayerEvent(player, "openDialog", "TRACTOR_RENT", $"Начать работу трактористом?");
                        }
                        else
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть арендованный транспорт", 3000);
                        return;
                    case 230:
                        if (NAPI.Data.GetEntityData(player, "WORK") == null)
                        {
                            Trigger.PlayerEvent(player, "openDialog", "SNOW_RENT", $"Начать работу уборкщиком?");
                        }
                        else
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть арендованный транспорт", 3000);
                        return;
                    case 590:
                        Trigger.PlayerEvent(player, "openDialog", "IRON_SELL", $"Вы хотите продать железяки 25 шт. {curiron}$ ?");
                        return;
                    case 591:
                        Trigger.PlayerEvent(player, "openDialog", "GOLD_SELL", $"Вы хотите продать золото 5 шт. {curgold}$ ?");
                        return;
                    case 152:
                        if (!player.HasData("ORG_ID") || string.IsNullOrEmpty(player.GetData<int>("ORG_ID").ToString())) return;
                        if (!OCore.OrgList.ContainsKey(player.GetData<int>("ORG_ID"))) return;
                        OCore.Organization org = OCore.OrgList[player.GetData<int>("ORG_ID")];
                        org.Enter(player);
                        return;
                    case 153:
                        if (!player.HasData("ORG_ID") || string.IsNullOrEmpty(player.GetData<int>("ORG_ID").ToString())) return;
                        if (!OCore.OrgList.ContainsKey(player.GetData<int>("ORG_ID"))) return;
                        org = OCore.OrgList[player.GetData<int>("ORG_ID")];
                        org.Exit(player);
                        return;
                    case 154:
                        if (!Main.Players.ContainsKey(player)) return;
                        if (!player.IsInVehicle)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в транспорте!", 3000);
                            return;
                        }
                        if (!VehicleManager.Vehicles.ContainsKey(player.Vehicle.NumberPlate)) return;
                        if (!MaterialsI.CarsMaterials.ContainsKey(VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Этот транспорт не может перевозить материалы!", 3000);
                            return;
                        }
                        if (Main.Players[player].Org == "")
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не состоите в организации!", 3000);
                            return;
                        }
                        Trigger.PlayerEvent(player, "openInput", "Ввод бизнеса", "Введите ID бизнеса", 4, "org_enterid");
                        return;
                    case 155:
                        if (!Players.ContainsKey(player)) return;
                        if (Main.Players[player].Org == "") return;
                        if (!player.IsInVehicle || !MaterialsI.CarsMaterials.ContainsKey(VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model)) return;
                        if (!player.HasData("ORDER_MAT")) return;
                        if (!player.Vehicle.HasData("MATERIALS")) return;
                        Trigger.PlayerEvent(player, "openInput", "Выгрузка материалов", "Введите количество материалов", 8, "org_otrmats");
                        return;
                    case 156:
                        if (!Players.ContainsKey(player)) return;
                        Trigger.PlayerEvent(player, "openlicmenu");
                        return;
                    case 157:
                        if (!Players.ContainsKey(player)) return;
                        Garage.InteractOnType(player);
                        return;
                    case 158:
                        if (!Players.ContainsKey(player)) return;
                        var find = nInventory.Find(Main.Players[player].UUID, ItemType.SnowBall);
                        if (find != null && find.Count >= 20)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нет места для снежков!", 3000);
                            return;
                        }

                        nInventory.Add(player, new nItem(ItemType.SnowBall));

                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы взяли снежок", 3000);

                        return;
                    case 159:
                        if (!Players.ContainsKey(player)) return;
                        TimeSpan result = DateTime.Now - Players[player].Cooldown;
                        string time = result.Minutes < 10 ? "0" : "";

                            if (result.Hours < 24)
                            {
                                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Приходите за подарком через [{result.Hours}:{time}{result.Minutes}]", 3000);
                                return;
                            }

                        var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Present));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно места в инвентаре", 3000);
                            return;
                        }


                        Players[player].Cooldown = DateTime.Now;

                        nInventory.Add(player, new nItem(ItemType.Present));

                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы получили подарок, приходите завтра!", 3000);

                        return;
                    case 510:
                        Houses.Realtor.OpenRealtorMenu(player, 0);
                        return;
                    case 511:
                        Houses.Realtor.OpenRealtorMenu(player, 1);
                        return;
                    case 512:
                        Houses.Realtor.OpenRealtorMenu(player, 2);
                        return;
                    case 513:
                        Houses.Realtor.OpenRealtorMenu(player, 3);
                        return;
                    case 514:
                        Houses.Realtor.OpenRealtorMenu(player, 4);
                        return;
                    case 515:
                        Houses.Realtor.OpenRealtorMenu(player, 5);
                        return;
                    case 516:
                        Houses.Realtor.OpenRealtorMenu(player, 6);
                        return;
                    case 517:
                        MoneySystem.Casino.Roll(player);
                        return;
                    case 518:
                        if (string.IsNullOrEmpty(Main.Players[player].Org)) return;
                        MaterialsI.MaterialsOrg orgf = (MaterialsI.MaterialsOrg)OCore.OrgListNAME[Main.Players[player].Org];

                        List<List<string>> itemsf = new List<List<string>>();

                        foreach (var veh in orgf.GetVehGarage())
                        {
                            List<string> item = new List<string>();
                            item.Add(ParkManager.GetNormalName(VehicleManager.Vehicles[veh].Model));
                            item.Add($"{veh}");
                            itemsf.Add(item);
                        }

                        string jsonf = JsonConvert.SerializeObject(itemsf);
                        Trigger.PlayerEvent(player, "garageauto", jsonf);
                        return;
                    case 519:
                        player.Dimension = 1;
                        player.Heading = -43;
                        player.Position = new Vector3(1089.7303, 206.7629, -49.619724);
                        Main.Players[player].ExteriorPos = new Vector3(935.4563, 46.268734, 80.97579);
                        return;
                    case 520:
                        player.Dimension = 0;
                        player.Heading = 113;
                        player.Position = new Vector3(935.4563, 46.268734, 80.97579);
                        Main.Players[player].ExteriorPos = new Vector3();
                        return;
                    case 521:

                        itemsf = new List<List<string>>();

                        foreach (var veh in AirVehicles.getAllAirVehicles(player.Name).Values)
                        {
                            List<string> item = new List<string>();
                            item.Add(ParkManager.GetNormalName(veh.Model));
                            item.Add($"{veh.Number}");
                            itemsf.Add(item);
                        }

                        jsonf = JsonConvert.SerializeObject(itemsf);
                        Trigger.PlayerEvent(player, "garageauto", jsonf);
                        return;
                    case 522:
                        Trigger.PlayerEvent(player, "client::openmenu");
                        return;
                    case 523:
                        House house = HouseManager.GetHouse(player, false);

                        if ( house != null && WhiteListHouses.Contains( house.ID ))
                            Trigger.PlayerEvent(player, "openDialog", "TO_ISLANDFREE", $"Вы хотите отправиться на остров ?");
                        else
                            Trigger.PlayerEvent(player, "openDialog", "TO_ISLAND", $"Вы хотите купить билет на остров за 2 500$ ?");
                        return;
                    case 524:
                        house = HouseManager.GetHouse(player, false);

                        if (house != null && WhiteListHouses.Contains(house.ID))
                            Trigger.PlayerEvent(player, "openDialog", "FROM_ISLANDFREE", $"Вы хотите отправиться в штат ?");
                        else
                            Trigger.PlayerEvent(player, "openDialog", "FROM_ISLAND", $"Вы хотите купить билет с острова за 2 500$ ?");
                        return;
                    case 525:
                    case 526:
                        if (!player.HasData("APARTMENT")) return;
                        player.GetData<ApartmentParent>("APARTMENT").Interact(player, id);
                        return;
                    case 527:
                        string number = "";
                        foreach (string num in VehicleManager.getAllPlayerVehicles(player.Name))
                            if (VehicleManager.Vehicles[num].OnPlast)
                            {
                                number = num;
                                break;
                            }
                        if (number == "")
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет транспорта на штрафстоянке", 3000);
                            return;
                        }
                        Trigger.PlayerEvent(player, "openDialog", "SHTRAF_ACCEPT", $"Вы хотите забрать {ParkManager.GetNormalName(VehicleManager.Vehicles[number].Model)}[{number}] со штрафстоянки? Размер штрафа: 25000$");
                        return;
                    case 528:
                        if (!player.IsInVehicle) return;
                        if (player.Vehicle.Model != NAPI.Util.GetHashKey("flatbed"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в FlatBed", 3000);
                            return;
                        }
                        if (!player.Vehicle.HasSharedData("fbAttachVehicle") )
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Транспорт отсутствует", 3000);
                            return;
                        }
                        foreach(Vehicle veh in NAPI.Pools.GetAllVehicles())
                            if (veh.Id == player.Vehicle.GetSharedData<int>("fbAttachVehicle") && VehicleManager.Vehicles.ContainsKey(veh.NumberPlate))
                            {
                                if (VehicleManager.HavePlactic(veh.NumberPlate))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете поставить этот транспорт на штрафстоянку", 3000);
                                    return;
                                }
                                if (VehicleManager.Vehicles[veh.NumberPlate].OnPlast) return;

                                MoneySystem.Wallet.Change(player, 300);
                                VehicleManager.Vehicles[veh.NumberPlate].OnPlast = true;
                                VehicleManager.Save(veh.NumberPlate);
                                Trigger.PlayerEvent(player, "client::flatbed:setstate");
                                NAPI.Task.Run(() => { try { veh.Delete(); } catch { } }, 200);
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы доставили транспорт на штрафстоянку за 300$", 3000);

                                break;
                            }
                        return;
                    case 529:
                        if (Main.Players[player].FractionID == 7)
                            Trigger.PlayerEvent(player, "openPc");
                        return;
                    case 530:
                        if (Main.Players[player].FractionID != 14)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет доступа!", 5000);
                            return;
                        }

                        NAPI.Entity.SetEntityPosition(player, Fractions.Army.ArmyCheckpoints[7] + new Vector3(0, 0, 1.2f));
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы были отправлены на 'Авианосец'", 5000);
                        return;
                    case 531:
                        if (Main.Players[player].FractionID != 14)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет доступа!", 5000);
                            return;
                        }

                        NAPI.Entity.SetEntityPosition(player, Fractions.Army.ArmyCheckpoints[6] + new Vector3(0,0,1.2f));
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы были отправлены на 'Базу'", 5000);
                        return;
                    case 532:
                        if (Main.Players[player].FractionID != 14)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет доступа!", 5000);
                            return;
                        }

                        itemsf = new List<List<string>>();

                        foreach (var veh in Fractions.Army.ArmyVehicles)
                        {
                            List<string> item = new List<string>();
                            item.Add(ParkManager.GetNormalName(veh));
                            itemsf.Add(item);
                        }

                        jsonf = JsonConvert.SerializeObject(itemsf);
                        Trigger.PlayerEvent(player, "garagearmy", jsonf);

                        return;
                    default:
                        return;
						
					case 7825:
                    case 7826:
                    case 7827:
                    case 7828:
                    case 7829:
                    case 70001:
                    case 70002:
                    case 70003:
                    case 70004:
                    case 70005:
                    case 70006:
                    case 70007:
                    case 70008:
                    case 70009:

                        Jobs.FermaWork.interactPressed(player, id);
                        return;
                }
                
                #endregion
            }
            catch (Exception e) { Log.Write($"interactionPressed/{intid}/: " + e.ToString(), nLog.Type.Error); }
        }

        static List<int> WhiteListHouses = new List<int> 
        {
            23,
            746,
            747,
            748,
            749,
            750,
            751,
            752     
        };

        public static void OpenAnswer(Player player, string text)
        {
            try
            {
                Trigger.PlayerEvent(player, "client::openanswer", text);
            }
            catch { }
        }

        [RemoteEvent("acceptPressed")]
        public void RemoteEvent_acceptPressed(Player player)
        {
            string req = "";
            try
            {
                if (!Main.Players.ContainsKey(player) || !player.GetData<bool>("IS_REQUESTED")) return;

                string request = player.GetData<string>("REQUEST");
                req = request;
                switch (request)
                {
                    case "acceptPass":
                        GUI.Docs.AcceptPasport(player);
                        break;
                    case "acceptLics":
                        GUI.Docs.AcceptLicenses(player);
                        break;
                    case "acceptPlastic":
                        GUI.Docs.AcceptPlactic(player);
                        break;
                    case "OFFER_ITEMS":
                        Selecting.playerOfferChangeItems(player);
                        break;
                    case "HANDSHAKE":
                        Selecting.hanshakeTarget(player);
                        break;
					case "KISS":
                        Selecting.kissTarget(player);
                        break;	
					case "acceptDocs":
						GUI.Docs.AcceptDocs(player);
						break;	
                }

                player.SetData("IS_REQUESTED", false);
            }
            catch (Exception e) { Log.Write($"acceptPressed/{req}/: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("cancelPressed")]
        public void RemoteEvent_cancelPressed(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !player.GetData<bool>("IS_REQUESTED")) return;
                player.SetData("IS_REQUESTED", false);
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Отмена", 3000);
            }
            catch (Exception e) { Log.Write("cancelPressed: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("dialogCallback")]
        public void RemoteEvent_DialogCallback(Player player, string callback, bool yes)
        {
            try
            {
                if (yes)
                {
                    switch (callback)
                    {
						case "TRUCKER_TRAILER":
                            var rnd = new Random();
                            int uid = rnd.Next(1, Jobs.Truckers.UnloadPoints.Count);

                            player.SetData("ORDERDATE", DateTime.Now.AddMinutes(6));

                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы взяли заказ для доставки.", 3000);
                            var pos = Jobs.Truckers.getProduct[0];
                            Trigger.PlayerEvent(player, "createWaypoint", pos.X, pos.Y);
                            player.SetData("ORDER", uid);
                            int prod = 0;

                            var spawnI = Jobs.Truckers.LastTrailerSpawn[prod];

                            //Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили свой груз!", 3000);
                            Truckers.playerGotProducts(player);
                            return;
                        case "BUS_RENT":
                            if (Main.Players[player].WorkID != 4)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Устроиться автобусником можно у начальника", 3000);
                                return;
                            }
                            if (!Main.Players[player].Licenses[2])
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет лицензии категории C", 3000);
                                return;
                            }
                            Jobs.Bus.busspawn(player);
                            return;
                        case "TAXI_RENT":
                            if (Main.Players[player].WorkID != 3)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Устроиться таксистом можно у начальника", 3000);
                                return;
                            }
                            if (!Main.Players[player].Licenses[1])
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет лицензии категории B", 3000);
                                return;
                            }
                            Jobs.Taxi.taxispawn(player);
                            return;
                        case "COLLECTOR_RENT":
                            if (Main.Players[player].WorkID != 7)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Устроиться инкассаторщиком можно у начальника", 3000);
                                return;
                            }
                            if (!Main.Players[player].Licenses[2])
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет лицензии категории C", 3000);
                                return;
                            }
                            Jobs.Collector.collectspawn(player);
                            return;
                        case "TRUCKER_RENT":
                            if (Main.Players[player].WorkID != 6)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Устроиться дальнобойщиком можно у начальника", 3000);
                                return;
                            }
                            if (!Main.Players[player].Licenses[2])
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет лицензии категории C", 3000);
                                return;
                            }
                            Jobs.Truckers.truckSpawn(player);
                            return;
                        case "TRASH_RENT":
                            if (Main.Players[player].WorkID != 10)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Устроиться водителем мусоровоза можно у начальника", 3000);
                                return;
                            }
                            if (!Main.Players[player].Licenses[2])
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет лицензии категории C", 3000);
                                return;
                            }
                            Jobs.TrashCar.trashspawn(player);
                            return;
                        case "MOWER_RENT":
                            if (Main.Players[player].WorkID != 5)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Устроиться газонокосильщиком можно у начальника", 3000);
                                return;
                            }
                            Jobs.Lawnmower.mowerspawn(player);
                            return;
                        case "TRACTOR_RENT":
                            if (Main.Players[player].WorkID != 9)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Устроиться трактористом можно у начальника", 3000);
                                return;
                            }
                            Jobs.Tractorist.mowerspawn(player);
                            return;
                        case "SNOW_RENT":
                            if (Main.Players[player].WorkID != 14)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Устроиться уборкщиком можно у начальника", 3000);
                                return;
                            }
                            Jobs.Snow.snowspawn(player);
                            return;
                        case "HELP_ME":
							Fractions.NacBez.sendFire(player);
							return;
                        case "IRON_SELL":
                            nItem find = nInventory.Find(Main.Players[player].UUID, ItemType.Iron);
                            if (find == null || find.Count < 25)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет 25 железяк", 3000);
                                return;
                            }
                            MoneySystem.Wallet.Change(player, curiron);
                            nInventory.Remove(player, new nItem(ItemType.Iron, 25));
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы продали 25 шт. железяк за {curiron}$", 3000);
                            return;
                        case "GOLD_SELL":
                            nItem find2 = nInventory.Find(Main.Players[player].UUID, ItemType.Gold);
                            if (find2 == null || find2.Count < 5)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет 5 золота", 3000);
                                return;
                            }
                            MoneySystem.Wallet.Change(player, curgold);
                            nInventory.Remove(player, new nItem(ItemType.Gold, 5));
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы продали 5 шт. золота за {curgold}$", 3000);
                            return;
                        case "PARK_BUY":
                            if (player.Vehicle == null) return;
                            player.Vehicle.GetData<SellCars.VehicleForSell>("PARKCLASS").Buy(player);
                            return;
                        case "PARK_DESTORY":
                            if (player.Vehicle == null) return;
                            player.Vehicle.GetData<SellCars.VehicleForSell>("PARKCLASS").Destroy(false);
                            return;
                        case "CONTAINER_OPEN":
                            if (!player.HasData("CONTAINER")) return;
                            player.GetData<Containers.Container>("CONTAINER").Open(player);
                            return;
                        case "TAXI_PAY":
                            Jobs.Taxi.taxiPay(player);
                            return;
                        
                        case "COCO_SELL":
                            var mItem = nInventory.Find(Main.Players[player].UUID, ItemType.Kokos);
                            var count = (mItem == null) ? 0 : mItem.Count;
                            if (count < 1)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас нет листьев коки", 3000);
                                return;
                            }
                            int pay = count * curcoco;
                            nInventory.Remove(player, ItemType.Kokos, count);
                            MoneySystem.Wallet.Change(player, pay);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы продали {count} штук, за {pay}$", 3000);
                            return;

                        case "ORG_YESMAT":
                            BCore.Bizness biz = BCore.BizList[player.GetData<int>("IDMAT")];
                            int carmats = player.GetData<int>("MAT");
                            int pricef = carmats * MaterialsI.ForMaterial / 4;
                            MoneySystem.Wallet.Change(player, -pricef);
                            Vector3 pos2 = biz.MaterialsPosition;
                            Trigger.PlayerEvent(player, "createWaypoint", pos2.X, pos2.Y);
                            Trigger.PlayerEvent(player, "createCheckpoint", 210, 1, pos2 - new Vector3(0,0,1f), 7, 0, 255, 0, 0);
                            player.Vehicle.SetData("MATERIALS", carmats);
                            player.SetData("ORDER_MAT", player.GetData<int>("IDMAT"));
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы купили материалы {carmats} шт.", 3000);
                            return;

                        case "ORG_INVITE":
                            player.GetData<Organization.OCore.Organization>("ORG_INV").AddMember(player);
                            player.ResetData("INVITED");
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы вступили в организацию!", 3000);
                            return;
                        case "PAY_MEDKIT":
                            Fractions.Ems.payMedkit(player);
                            return;
                        case "PAY_HEAL":
                            Fractions.Ems.payHeal(player);
                            return;
                        case "ORG_MAT":
                            Trigger.PlayerEvent(player, "openInput", "Покупка материалов", "Введите ID бизнеса для доставки", 4, "org_buymats");
                            return;
                        case "BUY_APART":
                            if (!player.HasData("APART_HOUSE")) return;

                            House house = player.GetData<House>("APART_HOUSE");

                            if (!string.IsNullOrEmpty(house.Owner))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В этой квартире уже имеется хозяин", 3000);
                                return;
                            }

                            if (house.Price > Main.Players[player].Money)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас не хватает средств для покупки квартиры", 3000);
                                return;
                            }

                            if (HouseManager.GetApart(player, true) != null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете купить больше одной квартиры", 3000);
                                return;
                            }
                            if (HouseManager.GetHouse(player, true) == null)
                            {
                                var vehicles = VehicleManager.getAllPlayerVehicles(player.Name).Count;
                                var maxcars = GarageManager.GarageTypes[GarageManager.Garages[house.GarageID].Type].MaxCars;
                                if (vehicles > maxcars)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Квартиру, которую Вы покупаете, имеет {maxcars} гаражных места, продайте лишние машины", 3000);
                                    HouseManager.OpenCarsSellMenu(player);
                                    return;
                                }
                            }
                            if (HouseManager.HouseTypeList[house.Type].PetPosition != null) house.PetName = Main.Players[player].PetName;
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили квартиру, не забудьте внести налог за него в банкомате", 3000);
                            Notify.Send(player, NotifyType.Success, NotifyPosition.Center, $"Не забудьте внести налог за него в банкомате.", 8000);
                            HouseManager.CheckAndKick(player);
                            house.SetLock(true);
                            house.SetOwner(player);
                            house.SendPlayer(player);
                            MoneySystem.Bank.Accounts[house.BankID].Balance = Convert.ToInt32(house.Price / 100 * 0.005) * 2;
                            MoneySystem.Bank.Save(house.BankID);


                            MoneySystem.Wallet.Change(player, -house.Price);

                            GameLog.Money($"player({Main.Players[player].UUID})", $"server", house.Price, $"apartBuy({house.ID})");

                            var targetVehicles = VehicleManager.getAllPlayerVehicles(player.Name.ToString());

                            if (targetVehicles.Count < 1) return;
                            var vehicle = targetVehicles[0];

                            foreach (var v in NAPI.Pools.GetAllVehicles())
                            {
                                if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "PERSONAL" && v.NumberPlate == vehicle)
                                {
                                    var veh = v;
                                    if (veh == null) return;
                                    VehicleManager.Vehicles[vehicle].Fuel = (!veh.HasSharedData("PETROL")) ? VehicleManager.VehicleTank[veh.Class] : veh.GetSharedData<int>("PETROL");
                                    NAPI.Entity.DeleteEntity(veh);

                                    MoneySystem.Wallet.Change(player, -200);
                                    GameLog.Money($"player({Main.Players[player].UUID})", $"server", 200, $"carEvac");
                                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваша машина была отогнана в гараж", 3000);
                                    break;
                                }
                            }
                            return;
                        case "PLASTIC_ACCEPT":
                            {
                                Player police = player.GetData<Player>("PLASTIC_TARGET");
                                string number = player.GetData<string>("PLASTIC_NUMBER");

                                if (police == null || number == null || !VehicleManager.Vehicles.ContainsKey(number) || VehicleManager.HavePlactic(number)) return;

                                if (Players[player].Money < 15000)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас недостаточно средств", 3000);
                                    return;
                                }
                                MoneySystem.Wallet.Change(player, -15000);
                                MoneySystem.Wallet.Change(police, 5000);
                                VehicleManager.Vehicles[number].Plastic = DateTime.Now;
                                VehicleManager.Save(number);
                                Fractions.Stocks.fracStocks[6].Money += 10000;
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вам выдали пластик за 15000$ на машину {ParkManager.GetNormalName(VehicleManager.Vehicles[number].Model)}[{number}]", 3000);
                                Notify.Send(police, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали пластик Гражданину({player.Id}) за 15000$ (получив 5000$)", 3000);

                                return;
                            }
                        case "SHTRAF_ACCEPT":
                            {
                                string number = "";
                                foreach(string num in VehicleManager.getAllPlayerVehicles(player.Name))
                                    if (VehicleManager.Vehicles[num].OnPlast)
                                    {
                                        number = num;
                                        break;
                                    }

                                if (Players[player].Money < 25000)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас недостаточно средств", 3000);
                                    return;
                                }
                                MoneySystem.Wallet.Change(player, -25000);
                                VehicleManager.Vehicles[number].OnPlast = false;
                                VehicleManager.Save(number);
                                Fractions.Stocks.fracStocks[6].Money += 25000;
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы забрали со штрафстоянки {ParkManager.GetNormalName(VehicleManager.Vehicles[number].Model)}[{number}] за 25000$", 3000);


                                var vehdata = VehicleManager.Vehicles[number];
                                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(vehdata.Model);
                                var veh = NAPI.Vehicle.CreateVehicle(vh, new Vector3(467.54117, -1066.5992, 29.79993), new Vector3(0.13290739, -0.27171132, 90.43886), 0, 0);

                                var houset = Houses.HouseManager.GetHouse(player, true);

                                var apartament = Houses.HouseManager.GetApart(player, true);

                                if (houset == null)
                                {
                                    if (apartament != null)
                                    {
                                        houset = apartament;
                                    }
                                }

                                if (houset != null)
                                {
                                    if (houset.GarageID != 0)
                                    {
                                        Houses.Garage Garage = Houses.GarageManager.Garages[houset.GarageID];
                                        if (!Garage.vehiclesOut.ContainsKey(number))
                                        Garage.SetOutVehicle(number, veh);
                                    }
                                }


                                VehicleStreaming.SetLockStatus(veh, true);
                                veh.SetData("ACCESS", "PERSONAL");
                                veh.SetData("ITEMS", vehdata.Items);
                                veh.SetData("OWNER", player);
                                veh.SetSharedData("PETROL", vehdata.Fuel);

                                VehicleStreaming.SetEngineState(veh, true);

                                NAPI.Vehicle.SetVehicleNumberPlate(veh, number);
                                VehicleManager.ApplyCustomization(veh);

                                return;
                            }
                        case "TWIST_ACCEPT":
                            {
                                Player twister = player.GetData<Player>("TARGET");
                                string number = player.GetData<string>("NUMBER");
                                int miles = player.GetData<int>("MIL_TWIST");
                                if (twister == null || number == null || miles == 0) return;

                                if (Players[player].Money < 20000)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас недостаточно средств", 3000);
                                    return;
                                }
                                if (!VehicleManager.Vehicles.ContainsKey(number)) return;

                                if (VehicleManager.Vehicles[number].Sell < miles)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно столько скрутить", 3000);
                                    Notify.Send(twister, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно столько скрутить", 3000);
                                    return;
                                }
                                MoneySystem.Wallet.Change(player, -20000);
                                player.GetData<Vehicle>("VEH").SetSharedData("MILE", (float)(VehicleManager.Vehicles[number].Sell - miles));
                                VehicleManager.Vehicles[number].Sell = Convert.ToInt32(VehicleManager.Vehicles[number].Sell - miles );
                                VehicleManager.Save(number);

                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вам скрутили {miles} км. за 20000$ на машине {ParkManager.GetNormalName(VehicleManager.Vehicles[number].Model)}[{number}]", 3000);
                                Notify.Send(twister, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы скрутили пробег Гражданину({player.Id}) за 20000$ (получив +1500$)", 3000);
                                MoneySystem.Wallet.Change(twister, 1500);
                                twister.SetData("COOLDOWN2", DateTime.Now.AddMinutes(10));
                                return;
                            }

                        case "DICE":

                            int money = player.GetData<int>("DICE_MONEY");
                            Player target = player.GetData<Player>("DICE_TARGET");
                            if (target == null) return;

                            if (player.Position.DistanceTo2D(target.Position) > 10)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин слишком далеко от вас", 3000);
                                return;
                            }

                            if (Main.Players[player].Money < money)
                            {
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"У вас нет столько денег!", 3000);
                                return;
                            }

                            if (Main.Players[target].Money < money)
                            {
                                Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"У вас нет столько денег!", 3000);
                                return;
                            }

                            Random dice = new Random();
                            
                            int random = dice.Next(1, 6);
                            int random2 = dice.Next(1, 6);

                            Commands.RPChat("me", player, $"Кинул кости и выпало число ({random})");
                            Commands.RPChat("me", target, $"Кинул кости и выпало число ({random2})");

                            if (random == random2)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас одинаковый счёт. Ничья", 3000);
                                Notify.Send(target, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас одинаковый счёт. Ничья", 3000);
                            }
                            else if (random > random2)
                            {
                                MoneySystem.Wallet.Change(player, money);
                                MoneySystem.Wallet.Change(target, -money);
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выиграли в кости (+{money}$)", 3000);
                                Notify.Send(target, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы проиграли в кости (-{money}$)", 3000);
                            }
                            else
                            {
                                MoneySystem.Wallet.Change(player, -money);
                                MoneySystem.Wallet.Change(target, money);
                                
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы проиграли в кости (-{money}$)", 3000);
                                Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выиграли в кости (+{money}$)", 3000);
                            }
                            return;
                        case "UNJAILK":
                            target = player.GetData<Player>("UNJAILK");
                            int maney = player.GetData<int>("MONEY");
                            if (target != null)
                            {
                                Main.Players[player].States[0] = 0;
                                Main.Players[player].States[1] = 0;
                                Wallet.Change(player, -maney);
                                Wallet.Change(target, maney);
                                Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы сняли статьи гражданину", 3000);
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вам сняли статьи", 3000);
                                player.ResetData("MONEY");
                                player.ResetData("UNJAILK");
                            }
                            return;
                        case "BUY_CAR":
                            {
                                house = Houses.HouseManager.GetHouse(player, true);
                                Houses.House apart = Houses.HouseManager.GetApart(player, true);

                                if (house == null)
                                    if (apart != null)
                                        house = apart;

                                if (house == null && VehicleManager.getAllPlayerVehicles(player.Name.ToString()).Count > 1 )
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет личного дома", 3000);
                                    break;
                                }
                                if (house != null)
                                {
                                    if (house.GarageID == 0 && VehicleManager.getAllPlayerVehicles(player.Name.ToString()).Count > 1)
                                    {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет гаража", 3000);
                                        break;
                                    }
                                    Houses.Garage garage = Houses.GarageManager.Garages[house.GarageID];
                                    if (VehicleManager.getAllPlayerVehicles(player.Name).Count >= Houses.GarageManager.GarageTypes[garage.Type].MaxCars)
                                    {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас максимальное кол-во машин", 3000);
                                        break;
                                    }
                                }

                                Player seller = player.GetData<Player>("CAR_SELLER");
                                Player sellfor = seller.GetData<Player>("SELLCARFOR");
                                if (sellfor != player || sellfor is null)
                                {
                                    Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-SALE-EXPLOIT] {seller.Name} ({seller.Value})");
                                    return;
                                }
                                if (!Main.Players.ContainsKey(seller) || player.Position.DistanceTo(seller.Position) > 3)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин находится слишком далеко от Вас", 3000);
                                    break;
                                }
                                string number = player.GetData<string>("CAR_NUMBER");
                                /*if (VehicleManager.Vehicles[number].Sell >= 3)
                                {
                                    Notify.Send(seller, NotifyType.Error, NotifyPosition.BottomCenter, "Данную машину можно только продать в ГОС", 3000);
                                    return;
                                }*/
                                if (!VehicleManager.Vehicles.ContainsKey(number))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Этой машины больше не существует", 3000);
                                    break;
                                }
                                if (VehicleManager.Vehicles[number].Holder != seller.Name)
                                {
                                    Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-SALE-EXPLOIT] {seller.Name} ({seller.Value})");
                                    return;
                                }
                                int price = player.GetData<int>("CAR_PRICE");
                                if (!MoneySystem.Wallet.Change(player, -price))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно средств", 3000);
                                    break;
                                }

                                VehicleManager.VehicleData vData = VehicleManager.Vehicles[number];
                                VehicleManager.Vehicles[number].Holder = player.Name;
                                MySQL.Query($"UPDATE vehicles SET holder='{player.Name}' WHERE number='{number}'");
                                
                                
                                MoneySystem.Wallet.Change(seller, price);
                                GameLog.Money($"player({Players[player].UUID})", $"player({Players[seller].UUID})", price, $"buyCar({number})");
                                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Пробег {Convert.ToInt32( VehicleManager.Vehicles[number].Sell )} км.", 3000);

                                var houset = Houses.HouseManager.GetHouse(seller, true);
                                var apartt = Houses.HouseManager.GetApart(seller, true);
                                if (houset == null)
                                    if (apartt != null)
                                        houset = apartt;

                                if (houset != null)
                                {
                                    Houses.Garage sellerGarage = Houses.GarageManager.Garages[houset.GarageID];
                                    sellerGarage.DeleteCar(number);
                                }

                                if (house != null)
                                {
                                    Houses.Garage Garage = Houses.GarageManager.Garages[house.GarageID];
                                    Garage.SpawnCar(number);
                                }

                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили {ParkManager.GetNormalName(vData.Model)} ({number}) за {price}$ у {seller.Name}", 3000);
                                Notify.Send(seller, NotifyType.Success, NotifyPosition.BottomCenter, $"{player.Name} купил у Вас {ParkManager.GetNormalName(vData.Model)} ({number}) за {price}$", 3000);
                                break;
                            }
                        case "BUY_AIR":
                            {

                                Player seller = player.GetData<Player>("CAR_SELLER");
                                Player sellfor = seller.GetData<Player>("SELLCARFOR");
                                if (sellfor != player || sellfor is null)
                                {
                                    Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-SALE-EXPLOIT] {seller.Name} ({seller.Value})");
                                    return;
                                }
                                if (!Main.Players.ContainsKey(seller) || player.Position.DistanceTo(seller.Position) > 3)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин находится слишком далеко от Вас", 3000);
                                    break;
                                }
                                string number = player.GetData<string>("CAR_NUMBER");

                                if (!AirVehicles.Airs.ContainsKey(number))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Этого воздушного транспорта больше не существует", 3000);
                                    break;
                                }
                                if (AirVehicles.Airs[number].Holder != seller.Name)
                                {
                                    Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-SALE-EXPLOIT] {seller.Name} ({seller.Value})");
                                    return;
                                }
                                int price = player.GetData<int>("CAR_PRICE");
                                if (!MoneySystem.Wallet.Change(player, -price))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно средств", 3000);
                                    break;
                                }
                                if (AirVehicles.getAllAirVehicles(player.Name).Count >=3 )
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас нет мест в аэропорту!", 3000);
                                    return;
                                }

                                AirVehicle airdata = AirVehicles.Airs[number];
                                AirVehicles.Airs[number].Holder = player.Name;

                                MoneySystem.Wallet.Change(seller, price);
                                GameLog.Money($"player({Players[player].UUID})", $"player({Players[seller].UUID})", price, $"buyAir({number})");
                                MySQL.Query($"UPDATE airvehicles SET holder='{player.Name}' WHERE number='{number}'");

                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили {ParkManager.GetNormalName(airdata.Model)} ({number}) за {price}$ у {seller.Name}", 3000);
                                Notify.Send(seller, NotifyType.Success, NotifyPosition.BottomCenter, $"{player.Name} купил у Вас {ParkManager.GetNormalName(airdata.Model)} ({number}) за {price}$", 3000);
                                break;
                            }
                        case "INVITED":
                            {
                                int fracid = player.GetData<int>("INVITEFRACTION");

                                Players[player].FractionID = fracid;
                                Players[player].FractionLVL = 1;
                                Players[player].WorkID = 0;

                                Fractions.Manager.Load(player, Players[player].FractionID, Players[player].FractionLVL);
                                if (Fractions.Manager.FractionTypes[fracid] == 1) Fractions.GangsCapture.LoadBlips(player);
                                if(fracid == 15) {
                                    Trigger.PlayerEvent(player, "enableadvert", true);
                                    Fractions.LSNews.onLSNPlayerLoad(player); // Загрузка всех объявлений в F7
                                }
                                Dashboard.sendStats(player);
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы вступили в {Fractions.Manager.FractionNames[fracid]}", 3000);
                                try
                                {
                                    Notify.Send(player.GetData<Player>("SENDERFRAC"), NotifyType.Success, NotifyPosition.BottomCenter, $"{player.Name} принял приглашение вступить в Вашу фракцию", 3000);
                                }
                                catch { }
                                return;
                            }
							case "INVITEDTOFAMILY":
                            {
                                try
                                {
                                    string familycid = player.GetData<string>("INVITEFAMILY");

                                    Players[player].FamilyCID = familycid;
                                    Players[player].FamilyRank = 1;
                                    Players[player].WorkID = 0;

                                    Golemo.Families.Member.LoadMembers(player, familycid, 1);
                                    Golemo.Families.Family family = Golemo.Families.Family.GetFamilyToCid(familycid);
                                    family.Players.Add(new Golemo.Families.Member(player.Name.ToString(), family.Name, family.FamilyCID, 1, Golemo.Families.Ranks.GetFamilyRankName(family.FamilyCID, 1)));

                                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы вступили в {Golemo.Families.Family.GetFamilyName(player.GetData<Player>("SENDERFAMILY"))}", 3000);
                                }
                                catch (Exception e)
                                {
                                    Log.Write(e.ToString(), nLog.Type.Error);
                                }
                                try
                                {
                                    Notify.Send(player.GetData<Player>("SENDERFAMILY"), NotifyType.Success, NotifyPosition.BottomCenter, $"{player.Name} принял приглашение вступить в Вашу семью", 3000);
                                }
                                catch { }
                                return;
                            }
                        case "MECHANIC_RENT":
                            Jobs.AutoMechanic.mechanicRent(player);
                            return;
							
                        case "REPAIR_CAR":
                            Jobs.AutoMechanic.mechanicPay(player);
                            return;
                        case "FUEL_CAR":
                            Jobs.AutoMechanic.mechanicPayFuel(player);
                            return;
                        case "HOUSE_SELL":
                            Houses.HouseManager.acceptHouseSell(player);
                            return;
                        case "APART_SELL":
                            Houses.HouseManager.acceptApartSell(player);
                            return;
                        case "HOUSE_SELL_TOGOV":
                            Houses.HouseManager.acceptHouseSellToGov(player);
                            return;
                        case "TO_ISLANDFREE":
                            if (player.IsInVehicle && player.VehicleSeat != 0) return;

                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы были переправлены на остров", 3000);
                            if (player.IsInVehicle)
                                NAPI.Entity.SetEntityPosition(player.Vehicle, new Vector3(4062.6917, -4682.998, 3.0640807));
                            else
                                NAPI.Entity.SetEntityPosition(player, new Vector3(4062.6917, -4682.998, 5.0640807));

                            return;
                        case "FROM_ISLANDFREE":
                            if (player.IsInVehicle && player.VehicleSeat != 0) return;

                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы были переправлены в штат", 3000);
                            if (player.IsInVehicle)
                                NAPI.Entity.SetEntityPosition(player.Vehicle, new Vector3(136.02351, -3332.7708, 4.901928));
                            else
                                NAPI.Entity.SetEntityPosition(player, new Vector3(136.02351, -3332.7708, 5.901928));
                            return;
                        case "TO_ISLAND":
                            if (player.IsInVehicle && player.VehicleSeat != 0) return;
                            if (Main.Players[player].Money < 2500)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас не хватает денег", 3000);
                                return;
                            }
                            MoneySystem.Wallet.Change(player, -2500);
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы были переправлены на остров", 3000);
                            if (player.IsInVehicle)
                                NAPI.Entity.SetEntityPosition(player.Vehicle, new Vector3(4062.6917, -4682.998, 3.0640807));
                            else
                                NAPI.Entity.SetEntityPosition(player, new Vector3(4062.6917, -4682.998, 5.0640807));

                            return;
                        case "FROM_ISLAND":
                            if (player.IsInVehicle && player.VehicleSeat != 0) return;
                            if (Main.Players[player].Money < 2500)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас не хватает денег", 3000);
                                return;
                            }
                            MoneySystem.Wallet.Change(player, -2500);
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы были переправлены в штат", 3000);
                            if (player.IsInVehicle)
                                NAPI.Entity.SetEntityPosition(player.Vehicle, new Vector3(136.02351, -3332.7708, 4.901928));
                            else
                                NAPI.Entity.SetEntityPosition(player, new Vector3(136.02351, -3332.7708, 5.901928));
                            return;
                        case "WIFE_MESSAGE":
                            Player wife = Main.GetNearestPlayer(player, 10);
                            Married.AgreeWife(player, wife);
                            return;
                        case "WIFE_AGREE":
                            Player wifes = Main.GetNearestPlayer(player, 10);
                            if (wifes == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В радиусе 3 метров нет никого!", 3000);
                                return;
                            }
                            Trigger.PlayerEvent(player, "openDialog", "WIFE_MESSAGE", $"Создать семью с {Main.PlayerNames[Main.Players[wifes].UUID]}?");
                            return;
                        case "CAR_SELL_TOGOV":
                            if (player.HasData("CARSELLGOV"))
                            {
                                string vnumber = player.GetData<string>("CARSELLGOV");
                                player.ResetData("CARSELLGOV");
                                VehicleManager.VehicleData vData = VehicleManager.Vehicles[vnumber];

                                if (vData.Holder != player.Name)
                                {
                                    Commands.SendToAdmins(1, $"Игрок {player.Name} пытается ломать систему, бань гада");
                                    return;
                                }

                                int price = BCore.GetVipCost(player, BCore.CostForCar(vData.Model));

                                foreach(Vehicle veh in NAPI.Pools.GetAllVehicles())
                                    if (veh != null && veh.NumberPlate == vnumber)
                                    {
                                        if (veh.HasData("PARKCLASS"))
                                            veh.GetData<SellCars.VehicleForSell>("PARKCLASS").Destroy(false, false);
                                        break;
                                    }


                                if (FineManager.HaveFine(vnumber, player.Name))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У данного транспорта есть штрафы!", 3000);
                                    return;
                                }
						
                                if (AutoShopI.ProductsList[4].ContainsKey(vData.Model))
                                {
                                    Main.Accounts[player].RedBucks += price;
                                    MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");
                                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали {ParkManager.GetNormalName( vData.Model )} ({vnumber}) за {price} UP", 3000);
                                }
                                else
                                {
                                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали {ParkManager.GetNormalName( vData.Model )} ({vnumber}) за {price}$", 3000);
                                    MoneySystem.Wallet.Change(player, price);
                                }

                                foreach(Vehicle veh in NAPI.Pools.GetAllVehicles())
                                {
                                    if (veh.NumberPlate == vnumber)
                                    {
                                        NAPI.Task.Run(() => { veh.Delete(); });
                                        break;
                                    }
                                }

                                GameLog.Money($"server", $"player({Main.Players[player].UUID})", price, $"carSell({vData.Model})");
                                VehicleManager.Remove(vnumber, player);
                            }
                            return;
                        case "AIR_SELL_TOGOV":
                            if (player.HasData("CARSELLGOV"))
                            {
                                string vnumber = player.GetData<string>("CARSELLGOV");
                                player.ResetData("CARSELLGOV");
                                AirVehicle vData = AirVehicles.Airs[vnumber];
                                int price = BCore.GetVipCost(player, BCore.CostForCar(vData.Model));

                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали {ParkManager.GetNormalName(vData.Model)} ({vnumber}) за {price}$", 3000);
                                MoneySystem.Wallet.Change(player, price);

                                foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                                {
                                    if (veh.NumberPlate == vnumber)
                                    {
                                        NAPI.Task.Run(() => { veh.Delete(); });
                                        break;
                                    }
                                }

                                GameLog.Money($"server", $"player({Main.Players[player].UUID})", price, $"carSell({vData.Model})");
                                AirVehicles.Remove(vnumber);
                            }
                            return;
                        case "WIFE_NOMESSAGE":
                            Player wifea = Main.GetNearestPlayer(player, 10);
                            Married.NoAgreeWife(player, wifea);
                            return;
						case "WIFE_NOAGREE":
                            Player wifeg = Main.GetNearestPlayer(player, 10);
                            if (wifeg == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В радиусе 3 метров нет никого!", 3000);
                                return;
                            }
                            Trigger.PlayerEvent(player, "openDialog", "WIFE_NOMESSAGE", $"Вы хотите развестись с {Main.PlayerNames[Main.Players[wifeg].UUID]}?");
                            return;	
                        case "GUN_LIC":
                            Fractions.FractionCommands.acceptGunLic(player);
                            return;
                        case "BUSINESS_BUY":
                            BCore.acceptBuyBusiness(player);
                            return;
                        case "ROOM_INVITE":
                            Houses.HouseManager.acceptRoomInvite(player);
                            return;
                        case "ROOM_INVITE_APART":
                            Houses.HouseManager.acceptRoomInviteApart(player);
                            return;
                        case "RENT_CAR":
                            Rentcar.RentCar(player);
                            return;
                        case "CARWASH_PAY":
                            CarWashI.CarWash.Buy(player);
                            return;
                        case "REMONT_AVTO":
                            CarRepairI.CarRepair.Buy(player);
                            return;
                        case "TICKET":
                            Fractions.FractionCommands.ticketConfirm(player, true);
                            return;
                        case "ORG_BUY":
                            Trigger.PlayerEvent(player, "openInput", "Покупка Склада", "Введите название организации", 25, "org_buy");
                            return;
                        case "ORG_SELL":
                            if (!OCore.OrgListNAME.ContainsKey(Main.Players[player].Org)) return;
                            OCore.Organization org = OCore.OrgListNAME[Main.Players[player].Org];
                            for (int i = org.Members.Count - 1; i >= 0; i--)
                                if (org.Members[i] != null)
                                    org.RemoveMember(org.Members[i]);

                            org.Sell(player);
                            return;
                        case "ORG_SELLPLAYER":
                            if (!player.HasData("COST") || !player.HasData("SELLER")) return;
                            Player sellers = player.GetData<Player>("SELLER");
                            if (string.IsNullOrEmpty(Main.Players[sellers].Org) || OCore.OrgListNAME[Main.Players[sellers].Org].Owner != sellers.Name) return;
                            OCore.OrgListNAME[Main.Players[sellers].Org].SellForPlayer(sellers, player, player.GetData<int>("COST"));
                            return;
                    }
                }
                else
                {
                    switch (callback)
                    {
                        case "ORG_INVITE":
                            player.ResetData("INVITED");
                            return;
                        case "BUS_RENT":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "MOWER_RENT":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "TAXI_RENT":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "TAXI_PAY":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "TRUCKER_RENT":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "PARK_BUY":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "PARK_DESTROY":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "COLLECTOR_RENT":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "RENT_CAR":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "MECHANIC_RENT":
                            VehicleManager.WarpPlayerOutOfVehicle(player);
                            return;
                        case "TICKET":
                            Fractions.FractionCommands.ticketConfirm(player, false);
                            return;
						
                    }
                }
            }
            catch (Exception e) { Log.Write($"dialogCallback ({callback} yes: {yes}): " + e.ToString(), nLog.Type.Error); }
        }

        [RemoteEvent("dialogCallbackMEDIC")]
        public static void PlayerEvent_Death(Player player, bool death)
        {
                Fractions.Ems.DeathConfirm(player, death);
        }

        [RemoteEvent("playerPressCuffBut")]
        public void PlayerEvent_playerPressCuffBut(Player player, params object[] arguments)
        {
            try
            {
                Fractions.FractionCommands.playerPressCuffBut(player);
                return;
            }
            catch (Exception e) { Log.Write("playerPressCuffBut: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("cuffUpdate")]
        public void PlayerEvent_cuffUpdate(Player player, params object[] arguments)
        {
            try
            {
                NAPI.Player.PlayPlayerAnimation(player, 49, "mp_arresting", "idle");
                return;
            }
            catch (Exception e) { Log.Write("cuffUpdate: " + e.Message, nLog.Type.Error); }
        }
        #endregion

        public class TestTattoo
        {
            public List<int> Slots { get; set; }
            public string Dictionary { get; set; }
            public string MaleHash { get; set; }
            public string FemaleHash { get; set; }
            public int Price { get; set; }

            public TestTattoo(List<int> slots, int price, string dict, string male, string female)
            {
                Slots = slots;
                Price = price;
                Dictionary = dict;
                MaleHash = male;
                FemaleHash = female;
            }
        }
        
        public Main()
        {
            Thread.CurrentThread.Name = "Main";

            MySQL.Init();

            try
            {
                oldconfig = new oldConfig
                {
                    ServerName = config.TryGet<string>("ServerName", "RP"),
                    ServerNumber = config.TryGet<string>("ServerNumber", "0"),
                    VoIPEnabled = config.TryGet<bool>("VOIPEnabled", true),
                    RemoteControl = config.TryGet<bool>("RemoteControl", false),
                    DonateChecker = config.TryGet<bool>("DonateChecker", false),
                    DonateSaleEnable = config.TryGet<bool>("Donation_Sale", false),
                    PaydayMultiplier = config.TryGet<int>("PaydayMultiplier", 2),
                    ExpMultiplier = config.TryGet<int>("ExpMultipler", 1),
                    SCLog = config.TryGet<bool>("SCLog", false),
                };
                MoneySystem.Donations.LoadDonations();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                Environment.Exit(0);
            }
            
            Timers.Init();
            
            GameLog.Start();
            
            ReportSys.Init();

            Fractions.LSNews.Init();

            EventSys.Init();

            MoneySystem.Casino.OnResourceStart();
            Fractions.ElectionsSystem.OnResourceStart();

            // НЕ УДАЛЯТЬ!!!!
            List<string> zones = new List<string>()
            {
                "torso",
                "head",
                "leftarm",
                "rightarm",
                "leftleg",
                "rightleg",
            };

            /*var names = new Dictionary<string, List<string>>();
            for (var i = 0; i < 6; i++)
            {
                var list = new List<string>();
                foreach (BusinessTattoo t in BusinessManager.BusinessTattoos[i])
                    list.Add(t.Name);
                names.Add(zones[i], list);
            }

            StreamWriter file = new StreamWriter("newtattoonames.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(names));
            file.Close();*/

            /*var tattoos = new Dictionary<string, List<TestTattoo>>();
            for (var i = 0; i < 6; i++)
            {
                var list = new List<TestTattoo>();
                foreach (BusinessTattoo t in BusinessManager.BusinessTattoos[i])
                    list.Add(new TestTattoo(t.Slots, t.Price, t.Dictionary, t.MaleHash, t.FemaleHash));
                tattoos.Add(zones[i], list);
            }

            var file = new StreamWriter("newtattoo.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(tattoos));
            file.Close();

            StreamWriter file = new StreamWriter("newbarber.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(BusinessManager.BarberPrices));
            file.Close();

            file = new StreamWriter("gangcapture.txt", true, Encoding.UTF8);
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            for (int y = 0; y < 7; y++)
            {
                for (int x = 0; x < 7; x++)
                {
                    var pos = new Vector3(-151.9617, -1762.569, 28.9122) + new Vector3(100 * x, 100 * y, 0);
                    file.Write($"new Vector3({pos.X}, {pos.Y}, {pos.Z}),\r\n");
                }
            }
            file.Write($"\r\n\r\n");
            foreach (var pos in Fractions.GangsCapture.gangZones)
            {
                file.Write("{ 'position': { 'x': " + pos.X + ", 'y': " + pos.Y + ", 'z': " + pos.Z + " }, 'color': 10 },\r\n");
            }

            file.Close();

            file = new StreamWriter("clotheshats.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(Customization.Hats));
            file.Close();

            file = new StreamWriter("clotheslegs.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(Customization.Legs));
            file.Close();

            file = new StreamWriter("clothesfeets.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(Customization.Feets));
            file.Close();

            file = new StreamWriter("clothestops.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(Customization.Tops));
            file.Close();

            file = new StreamWriter("clothesgloves.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(Customization.Gloves));
            file.Close();

            file = new StreamWriter("validgloves.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(Customization.CorrectGloves));
            file.Close();

            var unders = new Dictionary<bool, List<Underwear>>()
            {
                { true, new List<Underwear>() },
                { false, new List<Underwear>() }
            };
            foreach (var u in Customization.Underwears[true])
                unders[true].Add(u.Value);
            foreach (var u in Customization.Underwears[false])
                unders[false].Add(u.Value);

            file = new StreamWriter("clothesunder.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(unders));
            file.Close();

            file = new StreamWriter("validtorsos.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(Customization.CorrectTorso));
            file.Close();

            file = new StreamWriter("emptyslots.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(Customization.EmtptySlots));
            file.Close();

            file = new StreamWriter("clotheswatches.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(Customization.Accessories));
            file.Close();*/

            /*var file = new StreamWriter("clothesglasses.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(Customization.Glasses));
            file.Close();

            file = new StreamWriter("clothesjewerly.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(Customization.Jewerly));
            file.Close();

            /*file = new StreamWriter("clothesmasks.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(Customization.Masks));
            file.Close();*/

            /*StreamWriter file = new StreamWriter("tuningstandart.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(BusinessManager.TuningPrices));
            file.Close();

            file = new StreamWriter("tuning.json", true, Encoding.UTF8);
            file.Write(JsonConvert.SerializeObject(BusinessManager.Tuning));
            file.Close();

            file = new StreamWriter("tuningwheels.json", true, Encoding.UTF8);
            file.Write(Newtonsoft.Json.JsonConvert.SerializeObject(BusinessManager.TuningWheels));
            file.Close();*/

            /*StreamWriter file = new StreamWriter("tuning.json", true, Encoding.UTF8);
            file.Write(JsonConvert.SerializeObject(BusinessManager.Tuning));
            file.Close();*/
        }

        private static void saveBanks()
        {
            /*foreach (int acc in MoneySystem.Bank.Accounts.Keys.ToList())
            {
                if (!MoneySystem.Bank.Accounts.ContainsKey(acc)) continue;
                MoneySystem.Bank.Save(acc);
            }
            Log.Write("Bank Saved", nLog.Type.Save);*/
        }

        private static void saveInventory()
        {
            DateTime now = DateTime.Now;
            nInventory.SaveAll();
            DateTime before = DateTime.Now;
            TimeSpan min = before - now;
            Log.Write($"Inventory saved ({min.TotalSeconds.ToString()})", nLog.Type.Save);
        }

        private static void saveDatabase()
        {


            Log.Write("Saving Database...");

            DateTime now = DateTime.Now;
            DateTime total = now;

            if (NAPI.Pools.GetAllPlayers().Count > 0)
                foreach (Player p in NAPI.Pools.GetAllPlayers())
                {
                    if (!Players.ContainsKey(p)) continue;

                    Accounts[p].Save(p).Wait();
                    Players[p].Save(p).Wait();

                }

            DateTime before = DateTime.Now;
            TimeSpan min = before - now;
            Log.Write($"Characters saved ({min.TotalSeconds.ToString()})", nLog.Type.Save);
            now = DateTime.Now;
            Fractions.Stocks.saveStocksDic();
            before = DateTime.Now;
            min = before - now;
            Log.Write($"Stocks saved ({min.TotalSeconds.ToString()})", nLog.Type.Save);
            Log.Write($"Database was saved ({(before - total).TotalSeconds.ToString()})");
        }

        private static DateTime NextWeatherChange = DateTime.Now.AddMinutes(rnd.Next(30, 70));
        private static List<int> Env_lastDate = new List<int>() { DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year };
        private static List<int> Env_lastTime = new List<int>() { DateTime.Now.Hour, DateTime.Now.Minute };
        private static string Env_lastWeather = config.TryGet<string>("Weather", "CLEAR");
        public static bool SCCheck = config.TryGet<bool>("SocialClubCheck", false);

        public static void changeWeather(byte id) {
            try {
                switch(id) {
                    case 0: Env_lastWeather = "EXTRASUNNY";
                        break;
                    case 1: Env_lastWeather = "CLEAR";
                        break;
                    case 2: Env_lastWeather = "CLOUDS";
                        break;
                    case 3: Env_lastWeather = "SMOG";
                        break;
                    case 4: Env_lastWeather = "FOGGY";
                        break;
                    case 5: Env_lastWeather = "OVERCAST";
                        break;
                    case 6: Env_lastWeather = "RAIN";
                        break;
                    case 7: Env_lastWeather = "THUNDER";
                        break;
                    case 8: Env_lastWeather = "CLEARING";
                        break;
                    case 9: Env_lastWeather = "SMOG";
                        break;
                    case 10: Env_lastWeather = "XMAS";
                        break;
                    case 11: Env_lastWeather = "SNOWLIGHT";
                        break;
                    case 12: Env_lastWeather = "BLIZZARD";
                        break;
                    default: Env_lastWeather = "EXTRASUNNY";
                        break;
                }
                PlayerEventToAll("Enviroment_Weather", Env_lastWeather);
            } catch {
            }
        }
        private static void enviromentChangeTrigger()
        {
            try
            {
                List<int> nowTime = new List<int>() { DateTime.Now.Hour, DateTime.Now.Minute };
                List<int> nowDate = new List<int>() { DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year };

                if (nowTime != Env_lastTime)
                {
                    Env_lastTime = nowTime;
                    PlayerEventToAll("Enviroment_Time", nowTime);
                }

                if (nowDate != Env_lastDate)
                {
                    Env_lastDate = nowDate;
                    PlayerEventToAll("Enviroment_Date", nowDate);
                }

                string newWeather = Env_lastWeather;
                if (DateTime.Now >= NextWeatherChange)
                {
                    int rndWeather = rnd.Next(0, 101);
                    if (rndWeather < 75) {
                        if (rndWeather < 60) newWeather = "EXTRASUNNY";
                        else newWeather = "CLEAR";
                        NextWeatherChange = DateTime.Now.AddMinutes(120);
                    } else {
                        if (rndWeather < 90) newWeather = "RAIN";
                        else newWeather = "FOGGY";
                        NextWeatherChange = DateTime.Now.AddMinutes(rnd.Next(15, 70));
                    }

                    //newWeather = config.TryGet<string>("Weather", "CLEAR");
                }

                if (newWeather != Env_lastWeather)
                {
                    Env_lastWeather = newWeather;
                    PlayerEventToAll("Enviroment_Weather", newWeather);
                }
            }
            catch (Exception e) { Log.Write($"enviromentChangeTrigger: {e.ToString()}"); }
        }
        private static void playedMinutesTrigger()
        {
            try
            {
                if (!oldconfig.SCLog)
                {
                    DateTime now = DateTime.Now;
                    if (now.Hour == 4)
                    {
                        if (now.Minute == 15) NAPI.Chat.SendChatMessageToAll("!{#DF5353}[РЕСТАРТ] В 04:20 произойдёт автоматический рестарт сервера.");
                        else if (now.Minute == 16) NAPI.Chat.SendChatMessageToAll("!{#DF5353}[РЕСТАРТ] В 04:20 произойдёт автоматический рестарт сервера.");
                        else if (now.Minute == 17) NAPI.Chat.SendChatMessageToAll("!{#DF5353}[РЕСТАРТ] В 04:20 произойдёт автоматический рестарт сервера.");
                        else if (now.Minute == 19) 
                        {
                            if (!Admin.IsServerStoping) 
                            {
                                NAPI.Chat.SendChatMessageToAll("!{#DF5353}[РЕСТАРТ] Сейчас произойдёт автоматический рестарт сервера. Сервер будет доступен вновь примерно в течении 2-5 минут.");
                                Admin.stopServer("Автоматическая перезагрузка");
                            }
                        }
                    }
                }
                foreach (Player p in NAPI.Pools.GetAllPlayers())
                {
                    try
                    {
                        if (p != null && !Players.ContainsKey(p)) continue;
                        Players[p].LastHourMin++;
                    }
                    catch (Exception e) { Log.Write($"PlayedMinutesTrigger: " + e.Message, nLog.Type.Error); }
                }
            }
            catch (Exception e) { Log.Write($"playerMinutesTrigger: {e.ToString()}"); }
        }

		private static Random rndf = new Random();
        public static int pluscost = rndf.Next(1, 7);
        public static int curcoco = rndf.Next(50, 150);
        public static int curiron = rndf.Next(250, 600);
        public static int curgold = rndf.Next(350, 2000);

        public static void payDayTrigger()
        {
            NAPI.Task.Run(() =>
            {
                try
                {
					
					Fractions.Cityhall.lastHourTax = 0;
                    Fractions.Ems.HumanMedkitsLefts = 1000;

                    var rndt = new Random();
                    pluscost = rndt.Next(1, 4);
                    curcoco = rndt.Next(5, 30);
                    curiron = rndt.Next(130, 500);
                    curgold = rndt.Next(350, 2000);
                    int intrnd1 = rndt.Next(25, 75);
                    int intrnd2 = rndt.Next(75, 125);
                    int intrnd3 = rndt.Next(125, 200);
                    int intrnd4 = rndt.Next(100, 250);



                    Commands.Kartofel = intrnd1;
                    Commands.Psenica = intrnd2;
                    Commands.Morkov = intrnd3;
                    Commands.Milk = intrnd4;

                    if (DateTime.Now.Hour == 0)
                    {
                        QuestsManager.VerifyQuests();
                    }
                    if (DateTime.Now.Hour > 0 && DateTime.Now.Hour < 7)
                    {
                        Log.Write("X2 Payment");
                        Main.oldconfig.PaydayMultiplier = 2;
                    }
                    else
                    {
                        Main.oldconfig.PaydayMultiplier = 1;
                    }

                    if (DateTime.Now.Hour == 20)
                    {
                        Golemo.Families.Components.StartToWar();
                    }

                    if (QuestsManager.Currectday == 5 && DateTime.Now.Hour == 19 || QuestsManager.Currectday == 2 && DateTime.Now.Hour == 19)
                    {
                        SafeMain.AllowSafe = true;
                        SafeMain.Mafia = false;

                        foreach (Player ply in NAPI.Pools.GetAllPlayers())
                            if (Main.Players.ContainsKey(ply) && Main.Players[ply].FamilyCID != null && Main.Players[ply].FamilyCID != "null")
                            {
                                Notify.Send(ply, NotifyType.Info, NotifyPosition.BottomCenter, "Внимание! Возможен взлом хранилища банка!", 3000);
                            }
                    }
                    else if (QuestsManager.Currectday == 6 && DateTime.Now.Hour == 19 || QuestsManager.Currectday == 3 && DateTime.Now.Hour == 19)
                    {
                        SafeMain.AllowSafe = true;
                        SafeMain.Mafia = true;

                        foreach (Player ply in NAPI.Pools.GetAllPlayers())
                            if (Main.Players.ContainsKey(ply) && Main.Players[ply].FractionID == 10 || Main.Players[ply].FractionID == 11 || Main.Players[ply].FractionID == 12 || Main.Players[ply].FractionID == 13)
                            {
                                Notify.Send(ply, NotifyType.Info, NotifyPosition.BottomCenter, "Внимание! Возможен взлом хранилища банка!", 3000);
                            }
                    }
                    else if (SafeMain.AllowSafe)
                    {
                        SafeMain.isOpen = false;
                        Doormanager.SetDoorLocked(2, true, 0);
                        SafeMain.AllowSafe = false;
                    }






                    /*if (QuestsManager.Currectday == 4 && DateTime.Now.Hour == 20 || QuestsManager.Currectday == 1 && DateTime.Now.Hour == 20)
                    {
                        Fractions.MatsWar.startMatWarTimer();
                    }*/

                    Forbes.SyncMajors();
                    NightSync();

                    foreach (Player player in NAPI.Pools.GetAllPlayers())
                    {
                        try
                        {
                            if (player == null || !Players.ContainsKey(player)) continue;

                            QuestsManager.AddQuestProcess(player, 0);

                            if (Players[player].HotelID != -1)
                            {
                                Players[player].HotelLeft--;
                                if (Players[player].HotelLeft <= 0)
                                {
                                    Houses.Hotel.MoveOutPlayer(player);
                                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Вас выселили из отеля за неуплату", 3000);
                                }
                            }

                            if (Accounts[player].VipLvl > 0 && Accounts[player].VipDate <= DateTime.Now)
                            {
                                Accounts[player].VipLvl = 0;
                                Accounts[player].VipDate = DateTime.Now;
                                Notify.Send(player, NotifyType.Alert, NotifyPosition.BottomCenter, "С вас снят VIP статус", 3000);
                            }

                            if (Players[player].LastHourMin < 15)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны наиграть хотя бы 15 минут, чтобы получить пейдей", 3000);
                                continue;
                            }

                            switch (Fractions.Manager.FractionTypes[Players[player].FractionID])
                            {
                                case -1:
                                case 0:
                                case 1:
                                    if (Players[player].WorkID != 0) break;
                                    int payment = Convert.ToInt32((400 ) + (Group.GroupAddPayment[Accounts[player].VipLvl]));
                                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили пособие по безработице {payment}$", 3000);
                                    MoneySystem.Wallet.Change(player, payment);
                                    GameLog.Money($"server", $"player({Players[player].UUID})", payment, $"allowance");
                                    break;
                                case 2:
                                    payment = Convert.ToInt32((Fractions.Configs.FractionRanks[Players[player].FractionID][Players[player].FractionLVL].Item4) + (Group.GroupAddPayment[Accounts[player].VipLvl]));
                                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили зарплату в {payment}$", 3000);
                                    MoneySystem.Wallet.Change(player, payment);
                                    GameLog.Money($"server", $"player({Players[player].UUID})", payment, $"payday");
                                    break;
                            }

                            Players[player].EXP += 1 * Group.GroupEXP[Accounts[player].VipLvl] * oldconfig.ExpMultiplier;
                            if (Players[player].EXP >= 3 + Players[player].LVL * 3)
                            {
                                Players[player].EXP = Players[player].EXP - (3 + Players[player].LVL * 3);
                                Players[player].LVL += 1;
                                if(Players[player].LVL == 1) {
                                    NAPI.Task.Run(() => { try { Trigger.PlayerEvent(player, "disabledmg", false); } catch { } }, 5000);
                                }
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Поздравляем, у Вас новый уровень ({Players[player].LVL})!", 3000);
								//донат за уровень
								Account acc = Accounts[player];
								switch (Players[player].LVL)
								{
									case 5:
										Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 50UP за достижение 5 уровня", 3000);
                                        MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{50} where `login`='{Main.Accounts[player].Login}'");
                                        acc.RedBucks += 50;
                                        break;
									case 10:
										Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 75UP за достижение 10 уровня", 3000);

                                        MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{75} where `login`='{Main.Accounts[player].Login}'");
                                        acc.RedBucks += 75;
                                        break;	
									case 15:
										Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 100UP за достижение 15 уровня", 3000);

                                        MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{100} where `login`='{Main.Accounts[player].Login}'");
                                        acc.RedBucks += 100;
                                        break;	
									case 20:
										Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 150UP за достижение 20 уровня", 3000);

                                        MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{150} where `login`='{Main.Accounts[player].Login}'");
                                        acc.RedBucks += 150;
                                        break;	
									case 25:
										Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 200UP за достижение 25 уровня", 3000);

                                        MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{200} where `login`='{Main.Accounts[player].Login}'");
                                        acc.RedBucks += 200;
                                        break;
									case 30:
										Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 250UP за достижение 30 уровня", 3000);

                                        MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{250} where `login`='{Main.Accounts[player].Login}'");
                                        acc.RedBucks += 250;
                                        break;	
									case 35:
										Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 300UP за достижение 35 уровня", 3000);

                                        MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{300} where `login`='{Main.Accounts[player].Login}'");
                                        acc.RedBucks += 300;
                                        break;	
									case 40:
										Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 400UP за достижение 40 уровня", 3000);

                                        MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{400} where `login`='{Main.Accounts[player].Login}'");
                                        acc.RedBucks += 400;
                                        break;	
									case 45:
										Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 400UP за достижение 45 уровня", 3000);

										MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{400} where `login`='{Main.Accounts[player].Login}'");
										acc.RedBucks += 400;
										break;
									case 50:
										Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 400UP за достижение 50 уровня", 3000);

										MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{400} where `login`='{Main.Accounts[player].Login}'");
										acc.RedBucks += 400;
										break;	
									case 60:
										Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 400UP за достижение 60 уровня", 3000);

										MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{400} where `login`='{Main.Accounts[player].Login}'");
										acc.RedBucks += 400;
										break;	
									case 70:
										Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 400UP за достижение 70 уровня", 3000);

										MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{400} where `login`='{Main.Accounts[player].Login}'");
										acc.RedBucks += 400;
										break;	
								}
								//донат за уровень конец
                                if (Players[player].LVL == 1 && Accounts[player].PromoCodes[0] != "noref" && PromoCodes.ContainsKey(Accounts[player].PromoCodes[0]))
                                {
                                    if(!Accounts[player].PresentGet) {
                                        Accounts[player].PresentGet = true;
                                        string promo = Accounts[player].PromoCodes[0];
                                        MoneySystem.Wallet.Change(player, 50000);
                                        GameLog.Money($"server", $"player({Players[player].UUID})", 50000, $"promo_{promo}");
                                        Customization.AddClothes(player, ItemType.Hat, 44, 3);
                                        //nInventory.Add(player, new nItem(ItemType.Sprunk, 3)); why?
                                        //nInventory.Add(player, new nItem(ItemType.Сrisps, 3));
										Main.Accounts[player].VipLvl = 3;
										Main.Accounts[player].VipDate = DateTime.Now.AddDays(2);
                                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 50 000$, Gold VIP на 2 дня и вещи в инвентарь за достижение 1 уровня по промокоду {promo}!", 6000);

                                        try
                                        {
                                            bool isGiven = false;
                                            foreach (Player pl in NAPI.Pools.GetAllPlayers())
                                            {
                                                if (Players.ContainsKey(pl) && Players[pl].UUID == PromoCodes[promo].Item3)
                                                {
                                                    MoneySystem.Wallet.Change(pl, 5000);
                                                    Notify.Send(pl, NotifyType.Info, NotifyPosition.Bottom, $"Вы получили $5 000 за достижение 1 уровня гражданином {player.Name}", 4000);
                                                    isGiven = true;
                                                    break;
                                                }
                                            }
                                            if (!isGiven) MySQL.Query($"UPDATE characters SET money=money+5000 WHERE uuid={PromoCodes[promo].Item3}");
                                        }
                                        catch { }
                                    } else Notify.Send(player, NotifyType.Error, NotifyPosition.Bottom, "Этот аккаунт уже получал подарок за активацию промокода", 2000);
                                }
                            }

                            Players[player].LastHourMin = 0;

                            

                            GUI.Dashboard.sendStats(player);
                        }
                        catch (Exception e) { Log.Write($"EXCEPTION AT \"MAIN_PayDayTrigger_Player_{player.Name}\":\n" + e.ToString(), nLog.Type.Error); }
                    }
                    foreach (BCore.Bizness biz in BCore.BizList.Values)
                    {
                        try
                        {
                            
                            if (biz.Owner == "Государство") continue;

                            

                            /*if (DateTime.Now.Hour == 19)
                            {
                                MoneySystem.Bank.Change(biz.BankID, biz.Materials);
                                Log.Write($"Update money biz by {biz.ID} and {biz.Materials}$");
                            }*/
                            int nalog = biz.GetNalog();
							if (!MoneySystem.Bank.Accounts.ContainsKey(biz.BankID))
							{
								Log.Write("Error "+biz.BankID.ToString());
								continue;
							}

                            if (biz.Mafia != -1) Fractions.Stocks.fracStocks[biz.Mafia].Money += 2500;

                            if (DateTime.Now.Hour == 21)
                            {

                                var result = MySQL.QueryRead($"SELECT bank FROM characters WHERE firstname='{biz.Owner.Split('_')[0]}' AND lastname='{biz.Owner.Split('_')[1]}'");
                                if (result.Rows.Count <= 0) continue;
                                MoneySystem.Bank.Change(Convert.ToInt32(result.Rows[0]["bank"]), biz.Materials, true);
                                //MySQL.Query($"UPDATE money SET balance=balance+{biz.Materials} WHERE id={Convert.ToInt32(result.Rows[0]["bank"])}");
                            }

                            if (MoneySystem.Bank.Accounts[biz.BankID].Balance < nalog)
                            {
                                string name = biz.Owner;
                                bool have = false;
                                foreach (Player player in NAPI.Pools.GetAllPlayers())
                                    if (player.Name == name)
                                    {
                                        have = true;
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Государство отобрало у Вас бизнес за неуплату налогов", 3000);
                                        if (!Main.Players.ContainsKey(player)) break;
                                        Main.Players[player].BizIDs.Remove(biz.ID);
                                    }

                                MoneySystem.Bank.Accounts[biz.BankID].Balance = 1000;
                                MoneySystem.Bank.Save(biz.BankID);
                                biz.Owner = "Государство";
                                biz.Save();
                                biz.UpdateLabel();

                                if (have) continue;

                                string[] split = name.Split('_');
                                DataTable data = MySQL.QueryRead($"SELECT biz,money FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                                if (data != null)
                                {
                                    List<int> ownerBizs = new List<int>();

                                    foreach (DataRow Row in data.Rows)
                                    {
                                        ownerBizs = JsonConvert.DeserializeObject<List<int>>(Row["biz"].ToString());
                                    }

                                    ownerBizs.Remove(biz.ID);
                                    MySQL.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(ownerBizs)}',money=money+{Convert.ToInt32(biz.Cost * 0.7)} WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                                }
                            }
                            else
                            {
                                if (!BizFree)
                                    MoneySystem.Bank.Accounts[biz.BankID].Balance -= nalog;
                                if (DateTime.Now.Hour != 21 && DateTime.Now.Hour % 3 == 0)
                                {
                                    int nalogmat = biz.GetNalogMaterials();
                                    if (biz.Materials - nalogmat < 0)
                                        biz.Materials = 0;
                                    else
                                        biz.Materials -= nalogmat;
                                    biz.Save();
                                }
                            }
                        }
                        catch (Exception e) { Log.Write($"EXCEPTION AT \"MAIN_PayDayTrigger_Bizness\":\n" + e.ToString(), nLog.Type.Error); }
                    }
                    foreach (OCore.Organization orgf in OCore.OrgList.Values)
                    {
                        try
                        {
                            MaterialsI.MaterialsOrg org = (MaterialsI.MaterialsOrg)orgf;
                            if (org.Owner == "Государство") continue;
                            int nalog = org.GetNalog();
                            if (MoneySystem.Bank.Accounts[org.BankID].Balance < nalog)
                            {
                                string name = org.Owner;
                                //bool have = false;

                                int money = 0;

                                List<string> vehs = org.GetVehGarage();

                                for (int i = 0; i < vehs.Count; i++)
                                {
                                    foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                                        if (vehs[i] == veh.NumberPlate)
                                        {
                                            if (org.VehiclesInGarage.Contains(veh))
                                                org.VehiclesInGarage.Remove(veh);
                                            else if (org.VehiclesOut.Contains(veh))
                                                org.VehiclesOut.Remove(veh);
                                            money += BCore.CostForCar(VehicleManager.Vehicles[veh.NumberPlate].Model);
                                            VehicleManager.Remove(veh.NumberPlate);
                                            NAPI.Task.Run(() =>
                                            {
                                                veh.Delete();
                                            });
                                        }
                                }

                                bool have = false;
                                NAPI.Task.Run(() => { 
                                foreach (Player player in NAPI.Pools.GetAllPlayers())
                                    if (player.Name == name)
                                    {
                                        have = true;
                                        MoneySystem.Wallet.Change(player, money);
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Государство отобрало у Вас организацию за неуплату налогов", 3000);
                                        if (!Main.Players.ContainsKey(player)) break;
                                        Main.Players[player].Org = "";
                                    }
                                });

                                foreach (string nick in org.Members)
                                    org.RemoveMember(nick);

                                

                                string[] split = name.Split('_');
                                if (!have)
                                    MySQL.Query($"SELECT * FROM characters SET money=money+{money} WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                                MySQL.Query($"SELECT * FROM characters SET org='' WHERE firstname='{split[0]}' AND lastname='{split[1]}'");

                                MoneySystem.Bank.Accounts[org.BankID].Balance = 1000;
                                MoneySystem.Bank.Save(org.BankID);
                                org.Owner = "Государство";
                                org.Save();
                                org.UpdateLabel();


                            }
                            else if (!CompanyFree && DateTime.Now.Hour % 3 == 0)
                            {
                                if (Fractions.Stocks.fracStocks[6].Money > 10000)
                                {
                                    Fractions.Stocks.fracStocks[6].Money -= 500;
                                    MoneySystem.Bank.Accounts[org.BankID].Balance += 500;
                                    MoneySystem.Bank.Save(org.BankID);
                                }
                            }
                            else if (!CompanyFree)
                            {
                                MoneySystem.Bank.Accounts[org.BankID].Balance -= nalog;
                                MoneySystem.Bank.Save(org.BankID);
                            }

                        }
                        catch (Exception e) { Log.Write($"EXCEPTION AT \"MAIN_PayDayTrigger_Organizations\":\n" + e.ToString(), nLog.Type.Error); }
                    }
                    foreach (Houses.House h in Houses.HouseManager.Houses)
                    {
                        try
                        {
                            if (!config.TryGet<bool>("housesTax", true)) return;
                            if (h.Owner == string.Empty) continue;
                            if (Golemo.Families.Family.FalimyHouses.ContainsKey(h.ID)) continue;

                            int tax = Convert.ToInt32(h.Price / 100 * 0.005);
                            if (!HouseFree)
                            MoneySystem.Bank.Change(h.BankID, -tax);
                            Fractions.Stocks.fracStocks[6].Money += tax;
                            Fractions.Cityhall.lastHourTax += tax;

                            if (MoneySystem.Bank.Accounts[h.BankID].Balance > 0) continue;

                            string owner = h.Owner;
                            Player player = NAPI.Player.GetPlayerFromName(owner);

                            if (player != null && Players.ContainsKey(player))
                            {
                                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "У Вас отобрали дом за неуплату налогов", 3000);
                                MoneySystem.Wallet.Change(player, Convert.ToInt32(h.Price / 2.0));
                                    Trigger.PlayerEvent(player, "deleteCheckpoint", 333); 
                                    Trigger.PlayerEvent(player, "deleteGarageBlip");
                            }
                            else
                            {
                                string[] split = owner.Split('_');
                                MySQL.Query($"UPDATE characters SET money=money+{Convert.ToInt32(h.Price / 2.0)} WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                            }
                            h.SetOwner(null);
                        }
                        catch (Exception e) { Log.Write($"EXCEPTION AT \"MAIN_PayDayTrigger_House_{h.Owner}\":\n" + e.ToString(), nLog.Type.Error); }
                    }
                    foreach (Fractions.GangsCapture.GangPoint point in Fractions.GangsCapture.gangPoints.Values) Fractions.Stocks.fracStocks[point.GangOwner].Money += 100;

                    if (DateTime.Now.Hour == 0)
                    {
                        Fractions.Stocks.fracStocks[6].FuelLeft = Fractions.Stocks.fracStocks[6].FuelLimit; // city
                        Fractions.Stocks.fracStocks[7].FuelLeft = Fractions.Stocks.fracStocks[7].FuelLimit; // police
                        Fractions.Stocks.fracStocks[8].FuelLeft = Fractions.Stocks.fracStocks[8].FuelLimit; // fib
                        Fractions.Stocks.fracStocks[9].FuelLeft = Fractions.Stocks.fracStocks[9].FuelLimit; // ems
                        Fractions.Stocks.fracStocks[14].FuelLeft = Fractions.Stocks.fracStocks[14].FuelLimit; // army
                    }
                    Log.Write("Payday time!");
                }
                catch (Exception e) { Log.Write("EXCEPTION AT \"MAIN_PayDayTrigger\":\n" + e.ToString(), nLog.Type.Error); }
            });
        }

        public static void OpenInputMenu(Player player, string title, string func)
        {
            Menu menu = new Menu("inputmenu", false, false);
            menu.Callback = callback_inputmenu;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = title;
            menu.Add(menuItem);

            menuItem = new Menu.Item("inp", Menu.MenuItem.Input);
            menuItem.Text = "*******";
            menu.Add(menuItem);

            menuItem = new Menu.Item(func, Menu.MenuItem.Button);
            menuItem.Text = "ОК";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_inputmenu(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            string func = item.ID;
            string text = data["1"].ToString();
            MenuManager.Close(player);
            switch (func)
            {
                case "fillcar":
                    try
                    {
                        Convert.ToInt32(text);
                    }
                    catch
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                        return;
                    }
                    RefillI.fillCar(player, Convert.ToInt32(text), false);
                    return;
                /*case "load_mats":
                case "unload_mats":
                case "load_drugs":
                case "unload_drugs":
                    try
                    {
                        Convert.ToInt32(text);
                    }
                    catch
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                        return;
                    }
                    if (Convert.ToInt32(text) < 1)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                        return;
                    }
                    Fractions.Stocks.inputStocks(player, 1, func, Convert.ToInt32(text));
                    return;*/
                case "put_stock":
                case "take_stock":
                    try
                    {
                        Convert.ToInt32(text);
                    }
                    catch
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                        return;
                    }
                    if (Convert.ToInt32(text) < 1)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                        return;
                    }
                    Fractions.Stocks.inputStocks(player, 0, func, Convert.ToInt32(text));
                    return;
            }
        }



        #region SMS
        public static void OpenContacts(Player Player)
        {
            if (!Players.ContainsKey(Player)) return;
            Character acc = Players[Player];

            Menu menu = new Menu("contacts", false, true);
            menu.Callback = callback_sms;
			menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Контакты";
            menu.Add(menuItem);

            menuItem = new Menu.Item("call", Menu.MenuItem.Button);
            menuItem.Text = "Позвонить";
            menu.Add(menuItem);

            if (acc.Contacts != null)
            {
                foreach (KeyValuePair<int, string> c in acc.Contacts)
                {
                    menuItem = new Menu.Item(c.Key.ToString(), Menu.MenuItem.Button);
                    menuItem.Text = c.Value;
                    menu.Add(menuItem);
                }
            }

            menuItem = new Menu.Item("add", Menu.MenuItem.Button);
            menuItem.Text = "Добавить номер";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(Player);
        }
        private static void callback_sms(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try
            {
                if (!Players.ContainsKey(player))
                {
                    MenuManager.Close(player);
                    return;
                }
                if (item.ID == "add")
                {
                    MenuManager.Close(player);
                    Trigger.PlayerEvent(player, "openInput", $"Новый контакт", "Номер гражданина", 7, "smsadd");
                    return;
                }
                else if (item.ID == "call")
                {
                    MenuManager.Close(player);
                    Trigger.PlayerEvent(player, "openInput", $"Позвонить", "Номер телефона", 7, "numcall");
                    return;
                }
                else if (item.ID == "back")
                {
                    MenuManager.Close(player);
                    OpenPlayerMenu(player).Wait();
                    return;
                }

                MenuManager.Close(player, false);
                int num = Convert.ToInt32(item.ID);
                player.SetData("SMSNUM", num);
                OpenContactData(player, num.ToString(), Players[player].Contacts[num]);

            } catch (Exception e)
            {
                Log.Write("EXCEPTION AT SMS:\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static void OpenContactData(Player Player, string Number, string Name)
        {
            Menu menu = new Menu("smsdata", false, true);
            menu.Callback = callback_smsdata;
			menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = Number;
            menu.Add(menuItem);

            menuItem = new Menu.Item("name", Menu.MenuItem.Card);
            menuItem.Text = Name;
            menu.Add(menuItem);

            menuItem = new Menu.Item("send", Menu.MenuItem.Button);
            menuItem.Text = "Написать";
            menu.Add(menuItem);

            menuItem = new Menu.Item("call", Menu.MenuItem.Button);
            menuItem.Text = "Позвонить";
            menu.Add(menuItem);

            menuItem = new Menu.Item("rename", Menu.MenuItem.Button);
            menuItem.Text = "Переименовать";
            menu.Add(menuItem);

            menuItem = new Menu.Item("remove", Menu.MenuItem.Button);
            menuItem.Text = "Удалить";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(Player);
        }
        private static void callback_smsdata(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            int num = player.GetData<int>("SMSNUM");
            switch (item.ID)
            {
                case "send":
                    Trigger.PlayerEvent(player, "openInput", $"SMS для {num}", "Введите сообщение", 100, "smssend");
                    break;
                case "call":
                    if (!SimCards.ContainsKey(num))
                    {
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Гражданина с таким номером не найдено", 3000);
                        return;
                    }
                    Player target = GetPlayerByUUID(SimCards[num]);
                    Voice.Voice.PhoneCallCommand(player, target);
                    break;
                case "rename":
                    Trigger.PlayerEvent(player, "openInput", "Переименование", $"Введите новое имя для {num}", 18, "smsname");
                    break;
                case "remove":
                    Notify.Send(player, NotifyType.Alert, NotifyPosition.BottomCenter, $"{num} удален из контактов.", 4000);
                    lock (Players)
                    {
                        Players[player].Contacts.Remove(num);
                    }
                    break;
                case "back":
                    OpenContacts(player);
                    break;
            }
        }
        #endregion SMS

        #region SPECIAL
        [Command("build")]
        public static void CMD_BUILD(Player Player)
        {
            try
            {
            }
            catch { }
        }
        public static int GenerateSimcard(int uuid)
        {
            int result = rnd.Next(1000000, 9999999);
            while (SimCards.ContainsKey(result)) result = rnd.Next(1000000, 9999999);
            SimCards.Add(result, uuid);
            return result;
        }
        public static string StringToU16(string utf8String)
        {
            /*byte[] bytes = Encoding.Default.GetBytes(utf8String);
            byte[] uBytes = Encoding.Convert(Encoding.Default, Encoding.Unicode, bytes);
            return Encoding.Unicode.GetString(uBytes);*/
            return utf8String;
        }
        public static string GetVoiceKey()
        {
            try
            {
                string PrivateKey = "Q9ZXW-7REEJ-WUP96-VLQR8";
                WebClient client = new WebClient();

                string result = client.DownloadString("https://voip.gta5star.ru/request/" + PrivateKey);
                if (string.IsNullOrEmpty(result))
                {
                    Log.Write("VOIP-Master server return NULL result.", nLog.Type.Warn);
                    return null;
                }
                Log.Debug("Temp Key is " + result);
                return result;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"TEMPKEY\":\n" + e.ToString(), nLog.Type.Error);
                return null;
            }
        }
        public static void PlayerEventToAll(string eventName, params object[] args)
        {
            List<Player> players = NAPI.Pools.GetAllPlayers();
            foreach (Player p in players)
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                Trigger.PlayerEvent(p, eventName, args);
            }
        }
        public static List<Player> GetPlayersInRadiusOfPosition(Vector3 position, float radius, uint dimension = 39999999)
        {
            List<Player> players = NAPI.Player.GetPlayersInRadiusOfPosition(radius, position);
            players.RemoveAll(P => !P.HasData("LOGGED_IN"));
            players.RemoveAll(P => P.Dimension != dimension && dimension != 39999999);
            return players;
        }
        public static Player GetNearestPlayer(Player player, int radius)
        {

            List<Player> players = NAPI.Player.GetPlayersInRadiusOfPosition(radius, player.Position);
            Player nearestPlayer = null;
            foreach (Player playerItem in players)
            {
                if (playerItem == player) continue;
                if (playerItem == null) continue;
                if (playerItem.Dimension != player.Dimension) continue;
                if (nearestPlayer == null)
                {
                    nearestPlayer = playerItem;
                    continue;
                }
                if (player.Position.DistanceTo(playerItem.Position) < player.Position.DistanceTo(nearestPlayer.Position)) nearestPlayer = playerItem;
            }
            return nearestPlayer;
        }
        public static Player GetPlayerByID(int id)
        {
            foreach (Player player in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(player)) continue;
                if (player.Value == id) return player;
            }
            return null;
        }
        public static Player GetPlayerByUUID(int UUID)
        {
            lock (Players)
            {
                foreach (KeyValuePair<Player, Character> p in Players)
                {
                    if (p.Value.UUID == UUID)
                        return p.Key;
                }
                return null;
            }
        }
        public static void PlayerEnterInterior(Player player, Vector3 pos)
        {
            if (player.HasData("FOLLOWER"))
            {
                Player target = player.GetData<Player>("FOLLOWER");
                NAPI.Entity.SetEntityPosition(target, pos);

                NAPI.Player.PlayPlayerAnimation(target, 49, "mp_arresting", "idle");
                BasicSync.AttachObjectToPlayer(target, NAPI.Util.GetHashKey("p_cs_cuffs_02_s"), 6286, new Vector3(-0.02f, 0.063f, 0.0f), new Vector3(75.0f, 0.0f, 76.0f));
                Trigger.PlayerEvent(target, "setFollow", true, player);
            }
        } 
        public static void OnAntiAnim(Player player)
        {
            player.SetData("AntiAnimDown", true);
        }
        public static void OffAntiAnim(Player player)
        {
            player.ResetData("AntiAnimDown");

            if (player.HasData("PhoneVoip"))
            {
                Voice.VoicePhoneMetaData playerPhoneMeta = player.GetData<Voice.VoicePhoneMetaData>("PhoneVoip");
                if (playerPhoneMeta.CallingState != "callMe" && playerPhoneMeta.Target != null)
                {
                    player.PlayAnimation("anim@cellphone@in_car@ds", "cellphone_call_listen_base", 49);
                    Core.BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_amb_phone"), 6286, new Vector3(0.06, 0.01, -0.02), new Vector3(80, -10, 110));
                }
            }
        }

        #region MainMenu
        public static async Task OpenPlayerMenu(Player player)
        {
            Menu menu = new Menu("mainmenu", false, false);
            menu.Callback = callback_mainmenu;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "";
            menu.Add(menuItem);
			
			menuItem = new Menu.Item("gps", Menu.MenuItem.gpsBtn);
            menuItem.Column = 2;
            menuItem.Text = "GPS";
            menu.Add(menuItem);

            menuItem = new Menu.Item("contacts", Menu.MenuItem.contactBtn);
            menuItem.Column = 2;
            menuItem.Text = "Контакты";
            menu.Add(menuItem);

            menuItem = new Menu.Item("services", Menu.MenuItem.servicesBtn);
            menuItem.Column = 2;
            menuItem.Text = "Службы";
            menu.Add(menuItem);
			
			menuItem = new Menu.Item("bankmenu", Menu.MenuItem.bankBtn);
            menuItem.Column = 2;
            menuItem.Text = "UPБанк";
            menu.Add(menuItem);

            if (VehicleManager.getAllPlayerVehicles(player.Name).Count > 0)
            {
                menuItem = new Menu.Item("straf", Menu.MenuItem.strafBtn);
                menuItem.Column = 2;
                menuItem.Text = "Штрафы";
                menu.Add(menuItem);
            }

            if (AirVehicles.getAllAirVehicles(player.Name).Count > 0)
            {
                menuItem = new Menu.Item("air", Menu.MenuItem.airBtn);
                menuItem.Column = 2;
                menuItem.Text = "Аэропорт";
                menu.Add(menuItem);
            }

            if (oldconfig.VoIPEnabled)
            {
                Voice.VoicePhoneMetaData vpmd = player.GetData<Voice.VoicePhoneMetaData>("PhoneVoip");
                if (vpmd.Target != null)
                {
                    if (vpmd.CallingState == "callMe")
                    {
                        menuItem = new Menu.Item("acceptcall", Menu.MenuItem.Button);
                        menuItem.Scale = 1;
                        menuItem.Color = Menu.MenuColor.Green;
                        menuItem.Text = "Принять вызов";
                        menu.Add(menuItem);
                    }

                    string text = (vpmd.CallingState == "callMe") ? "Отклонить вызов" : (vpmd.CallingState == "callTo") ? "Отменить вызов" : "Завершить вызов";
                    menuItem = new Menu.Item("endcall", Menu.MenuItem.Button);
                    menuItem.Scale = 1;
                    menuItem.Text = text;
                    menu.Add(menuItem);
                }
            }

            /*if (Main.Players[player].FractionID > 0)
            {
                menuItem = new Menu.Item("frac", Menu.MenuItem.grupBtn);
                menuItem.Column = 2;
                menuItem.Text = "Фракция";
                menu.Add(menuItem);
            }*/

            if (Fractions.Manager.isLeader(player, 6))
            {
                menuItem = new Menu.Item("citymanage", Menu.MenuItem.businessBtn);
                menuItem.Column = 2;
                menuItem.Text = "Мэрия";
                menu.Add(menuItem);
            }

            // if (Main.Players[player].HotelID != -1)
            // {
            // menuItem = new Menu.Item("hotel", Menu.MenuItem.Button);
            // menuItem.Column = 2;
            // menuItem.Text = "Отель";
            // menu.Add(menuItem);
            // }

            menuItem = new Menu.Item("promo", Menu.MenuItem.mailBtn);
            menuItem.Column = 2;
            menuItem.Text = "Промокод";
            menu.Add(menuItem);

            menuItem = new Menu.Item("ad", Menu.MenuItem.ilanBtn);
            menuItem.Text = "Объявление";
            menu.Add(menuItem);
            if (Houses.HouseManager.GetApart(player, true) != null)
            {
                menuItem = new Menu.Item("apart", Menu.MenuItem.homeBtn);
                menuItem.Column = 2;
                menuItem.Text = "Квартира";
                menu.Add(menuItem);
            }
            if (Houses.HouseManager.GetHouse(player, true) != null)
            {
                menuItem = new Menu.Item("house", Menu.MenuItem.homeBtn);
                menuItem.Column = 2;
                menuItem.Text = "Дом";
                menu.Add(menuItem);
            }
            
            else if (Houses.HouseManager.GetHouse(player) != null && Houses.HouseManager.GetHouse(player, true) == null)
            {
                menuItem = new Menu.Item("openhouse", Menu.MenuItem.Button);
                menuItem.Text = "Открыть/Закрыть Дом";
                menu.Add(menuItem);

                menuItem = new Menu.Item("leavehouse", Menu.MenuItem.Button);
                menuItem.Text = "Выселиться из дома";
                menu.Add(menuItem);
            }

            if (Houses.HouseManager.GetHouse(player, true) == null && Houses.HouseManager.GetApart(player, true) == null)
            {
                menuItem = new Menu.Item("park", Menu.MenuItem.lockBtn);
                menuItem.Text = "Парковка";
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("radio", Menu.MenuItem.radioBtn);
            menuItem.Text = "Радио";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);
			
			

            await menu.OpenAsync(player);
        }

        [RemoteEvent("server::openfracmenu")]
        static void RM_openfracmenu(Player player)
        {
            try
            {
                if (Main.Players[player].FractionID > 0)
                    Fractions.Manager.OpenFractionMenu(player);
            }
            catch { }
        }

        private static void callback_mainmenu(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            switch (item.ID)
            {
                case "gps":
                    OpenGPSMenu(player, "Категории", "back");
                    return;
                case "air":
                    AirVehicles.OpenMenu(player);
                    return;
                case "biz":
                    BCore.OpenSelectMenu(player);
                    return;
                case "house":
                    player.SetData("APART", false);
                    Houses.HouseManager.OpenHouseManageMenu(player);
                    return;
                case "apart":
                    player.SetData("APART", true);
                    Houses.HouseManager.OpenHouseManageMenu(player);
                    return;
                case "frac":
                    Fractions.Manager.OpenFractionMenu(player);
                    return;
                case "services":
                    OpenServicesMenu(player);
                    return;
                case "straf":
                    FineManager.OpenFineMenu(player);
                    return;
				case "bankmenu":
                     OpenBankMenu(player);
                     return;	
                case "citymanage":
                    OpenMayorMenu(player);
                    return;  
                case "contacts":
                    if (Main.Players[player].Sim == -1)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет сим-карты", 3000);
                        return;
                    }
                    OpenContacts(player);
                    return;
                case "ad":
                    Trigger.PlayerEvent(player, "openInput", "Объявление", "6$ за каждые 20 символов", 100, "make_ad");
                    return;
                case "openhouse":
                    {
                        Houses.House house = Houses.HouseManager.GetHouse(player);
                        house.SetLock(!house.Locked);
                        if (house.Locked) Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закрыли дом", 3000);
                        else Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы открыли дом", 3000);
                        return;
                    }
                case "leavehouse":
                    {
                        Houses.House house = Houses.HouseManager.GetHouse(player);
                        if (house == null)
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы не живете в доме", 3000);
                            MenuManager.Close(player);
                            return;
                        }
                        if (house.Roommates.Contains(player.Name)) house.Roommates.Remove(player.Name);
                        Trigger.PlayerEvent(player, "deleteCheckpoint", 333);
                        Trigger.PlayerEvent(player, "deleteGarageBlip");
                    }
                    return;
                case "promo":
                    OpenPromoMenu(player);
                    //Trigger.PlayerEvent(player, "openInput", "Промокод", "Введите промокод", 10, "enter_promocode");
                    return;
                case "acceptcall":
                    Voice.Voice.PhoneCallAcceptCommand(player);
                    return;
                case "radio":
                    OpenRadio(player);
                    return;
                case "endcall":
                    Voice.Voice.PhoneHCommand(player);
                    return;
				case "park":
                    ParkManager.OpenMenu(player);
                    return;
					
            }
        }
        private static List<string> MoneyPromos = new List<string>()
        {

        };

        public static void OpenRadio(Player player)
        {
            Menu menu = new Menu("bankmenu", false, true);
            menu.Callback = callback_radio;
            menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Radio Record";
            menu.Add(menuItem);


            menuItem = new Menu.Item("on", Menu.MenuItem.Button);
            menuItem.Text = "Включить радио";
            menu.Add(menuItem);

            menuItem = new Menu.Item("off", Menu.MenuItem.Button);
            menuItem.Text = "Выключить радио";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn); // полоска закрытия
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
        }

        public static void OpenBankMenu(Player player)
        {
            Menu menu = new Menu("bankmenu", false, true);
            menu.Callback = callback_bank;
            menu.SetBackGround("../images/phone/pages/bank.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Мобильный Банк";
            menu.Add(menuItem);
			
			menuItem = new Menu.Item("money", Menu.MenuItem.Card);
            menuItem.Text = $"На счету: {Bank.Accounts[Players[player].Bank].Balance} $";
            menuItem.Color = Menu.MenuColor.Blue;
            menu.Add(menuItem);

            menuItem = new Menu.Item("toplayer", Menu.MenuItem.gButton);
            menuItem.Text = "Перевести на счёт";
            menu.Add(menuItem);
           
            if (Houses.HouseManager.GetHouse(player) != null)
            {
                menuItem = new Menu.Item("tohouse", Menu.MenuItem.gButton);
                menuItem.Text = "Перевести на дом";
                menu.Add(menuItem);
            }

            if (Houses.HouseManager.GetApart(player) != null)
            {
                menuItem = new Menu.Item("toapart", Menu.MenuItem.gButton);
                menuItem.Text = "Перевести на квартиру";
                menu.Add(menuItem);
            }

            

            if (Main.Players[player].BizIDs.Count > 0)
            {
                menuItem = new Menu.Item("tobiz", Menu.MenuItem.gButton);
                menuItem.Text = "Перевести на бизнес";
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("kursi", Menu.MenuItem.gButton);
            menuItem.Text = "Посмотреть курсы";
            menu.Add(menuItem);


            menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn); // полоска закрытия
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
        }

        public static void OpenKursMenu(Player player)
        {
            Menu menu = new Menu("kursi", false, true);
            menu.Callback = callback_kur;
            menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Курсы";
            menu.Add(menuItem);

            foreach(KeyValuePair<string, int> pairs in RodManager.ProductsSellPrice)
            {
                menuItem = new Menu.Item("kus", Menu.MenuItem.Button);
                menuItem.Text = $"{pairs.Key} - {pairs.Value * pluscost}$";
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("kus", Menu.MenuItem.Button);
            menuItem.Text = $"Листья коки - {curcoco}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("kus", Menu.MenuItem.Button);
            menuItem.Text = $"Железяки - {curiron}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("kus", Menu.MenuItem.Button);
            menuItem.Text = $"Золото - {curgold}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("kus", Menu.MenuItem.Button);
            menuItem.Text = $"Картофель - {Commands.Kartofel}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("kus", Menu.MenuItem.Button);
            menuItem.Text = $"Пщеница - {Commands.Psenica}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("kus", Menu.MenuItem.Button);
            menuItem.Text = $"Морковь - {Commands.Morkov}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("kus", Menu.MenuItem.Button);
            menuItem.Text = $"Молоко - {Commands.Milk}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn); // полоска закрытия
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
        }

        public static List<string> GetPromoList(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return null;
            if (Main.Accounts[player].CustomPromo == null) return null;
            var promo = new List<string> { Main.Accounts[player].CustomPromo };
            var result = MySQL.QueryRead($"SELECT * FROM accounts WHERE promocodes='{JsonConvert.SerializeObject(promo)}'");
            var ing = new List<string> { };
            if (result == null) return null;
            foreach (DataRow Row in result.Rows)
            {
                var noresult = MySQL.QueryRead($"SELECT * FROM characters WHERE uuid='{Convert.ToInt32(Row["character1"].ToString())}'");
                foreach (DataRow Rowt in noresult.Rows) { ing.Add(Rowt["firstname"].ToString() + "_" + Rowt["lastname"].ToString()); }
            }
            return ing;
        }

        public static void OpenPromoMenu(Player player)
        {
            Menu menu = new Menu("promomenu", false, true);
            menu.Callback = callback_promo;
			menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Промокод";
            menu.Add(menuItem);

            if (!string.IsNullOrEmpty(Main.Accounts[player].CustomPromo))
            {
                menuItem = new Menu.Item("count", Menu.MenuItem.Card);
                menuItem.Text = $"Промо: {Main.Accounts[player].CustomPromo}";
                menu.Add(menuItem);

                var promolist = GetPromoList(player);

                menuItem = new Menu.Item("count", Menu.MenuItem.Card);
                menuItem.Text = $"Активаций: {promolist.Count}";
                menu.Add(menuItem);

                menuItem = new Menu.Item("promolist", Menu.MenuItem.Button);
                menuItem.Text = "Список активаций";
                menu.Add(menuItem);

            }
            else
            {
                menuItem = new Menu.Item("generatorpromo", Menu.MenuItem.Button);
                menuItem.Text = "Сгенерировать промокод";
                menu.Add(menuItem);
            }

            if (Main.Accounts[player].PromoCodes[0] == "noref" && Players[player].LVL == 0)
            {
                menuItem = new Menu.Item("enterpromo", Menu.MenuItem.Button);
                menuItem.Text = "Ввести промокод";
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
        }

        public static void OpenPromoList(Player player)
        {
            Menu menu = new Menu("promlistomenu", false, true);
            menu.Callback = callback_promo;
			menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Список активаций";
            menu.Add(menuItem);   

            menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            foreach(string str in GetPromoList(player))
            {
                menuItem = new Menu.Item("nameprom", Menu.MenuItem.Card);
                menuItem.Text = str;
                menu.Add(menuItem);
            }

            menu.Open(player);
        }

        private static List<string> whitelist = new List<string>
        {
          "A","B","E","K","M","H","O","P","C","T","Y","X"
        };

        public static bool CheckPromo(string promo)
        {
            var exits = false;
            var result = MySQL.QueryRead($"SELECT * FROM accounts WHERE custompromo='{promo}'");
            if (result != null) exits = true;
            return exits;
        }

        public static void GeneratorPromo(Player player)
        {
            try
            {
            if (!Main.Players.ContainsKey(player)) return;
            //if (Main.Accounts[player].CustomPromo != null) return;
            var Rnd = new Random();
            string number;
            do
            {
                number = "";
                number += (char)Rnd.Next(0x0041, 0x005A);
                for (int i = 0; i < 3; i++)
                    number += (char)Rnd.Next(0x0030, 0x0039);
                number += (char)Rnd.Next(0x0041, 0x005A);
            } while (!CheckPromo(number));
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш промокод {number}", 10000);
            Main.Accounts[player].CustomPromo = number;
            PromoCodes.Add(number, new Tuple<int, int, int>(1, 0, Main.Players[player].UUID));
            MySQL.Query($"INSERT INTO `promocodes` (`name`,`type`,`count`,`owner`,`donate`) VALUES ('{number}',0,0,{Main.Players[player].UUID},0)");
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT PROMOGEN:\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private static void callback_radio(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try
            {
                if (!Players.ContainsKey(player))
                {
                    MenuManager.Close(player);
                    return;
                }

                if (item.ID == "on")
                {
                    Trigger.PlayerEvent(player, "startradio");
                    MenuManager.Close(player);
                    OpenPlayerMenu(player).Wait();
                    return;
                }
                else if (item.ID == "off")
                {
                    Trigger.PlayerEvent(player, "stopradio");
                    MenuManager.Close(player);
                    OpenPlayerMenu(player).Wait();
                    return;
                }
                else if (item.ID == "back")
                {
                    MenuManager.Close(player);
                    OpenPlayerMenu(player).Wait();
                    return;
                }

                MenuManager.Close(player, false);

            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT PROMO:\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private static void callback_promo(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try
            {
                if (!Players.ContainsKey(player))
                {
                    MenuManager.Close(player);
                    return;
                }

                if (item.ID == "promolist") // список активаций
                {
                    OpenPromoList(player);
                    return;
                }
                else if (item.ID == "enterpromo") // ввести промокод
                {
                    MenuManager.Close(player);
                    Trigger.PlayerEvent(player, "openInput", "Промокод", "Введите промокод", 10, "enter_promocode");
                    return;
                }
                else if (item.ID == "generatorpromo") // сгенерировать промокод
                {
                    GeneratorPromo(player);
                    MenuManager.Close(player);
                    //OpenPlayerMenu(player).Wait();
                    return;
                }
                else if (item.ID == "back") // назад
                {
                    MenuManager.Close(player);
                    OpenPlayerMenu(player).Wait();
                    return;
                }

                MenuManager.Close(player, false);

            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT PROMO:\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private static Dictionary<string, List<string>> Category = new Dictionary<string, List<string>>()
        {
            { "Категории", new List<string>(){
                "Гос.структуры",
                "Работы",
                //"Банды",
                "Мафии",
                "Ближайшие места",
            }},
            { "Гос.структуры", new List<string>(){
                "Мэрия",
                "LSPD",
                "Госпиталь",
                "FBI",
            }},
            { "Работы", new List<string>(){
                "Электростанция",
                "Отделение почты",
                "Таксопарк",
                "Автобусный парк",
                "Стоянка газонокосилок",
                "Стоянка дальнобойщиков",
                "Стоянка инкассаторов",
                //"Стоянка автомехаников",
            }},

            { "Организации", new List<string>(){
                "ЧОО",
                //"Русская мафия",
                //"Мексиканская мафия",
                "Итальянское посольство",
            }},
            { "Ближайшие места", new List<string>(){
                "Ближайший банкомат",
                "Ближайшая заправка",
                "Ближайший 24/7",
                "Ближайшая аренда авто",
                "Ближайшая остановка",
            }},
        };
        private static Dictionary<string, Vector3> Points = new Dictionary<string, Vector3>()
        {
            { "Мэрия", new Vector3(-535.0794, -193.727, 46.4230) },
            { "LSPD", new Vector3(-1109.432, -843.9984, 18.1968) },
            { "Госпиталь", new Vector3(294.02036, -583.1621, 42.063076) },
            { "FBI", new Vector3(149.4746, -756.9065, 243.0319) },
            { "Электростанция", new Vector3(724.9625, 133.9959, 79.83643) },
            { "Отделение почты", new Vector3(105.4633, -1568.843, 28.60269) },
            { "Таксопарк", new Vector3(903.3215, -191.7, 73.40494) },
            { "Автобусный парк", new Vector3(462.6476, -605.5295, 27.49518) },
            { "Стоянка газонокосилок", new Vector3(-1331.475, 53.58579, 53.53268) },
            //{ "Стоянка дальнобойщиков", new Vector3(588.2037, -3037.641, 6.303829) },
            { "Стоянка инкассаторов", new Vector3(915.9069, -1265.255, 25.52912) },
            //{ "Стоянка автомехаников", new Vector3(473.9508, -1275.597, 29.60513) },
			//{ "The Families", new Vector3(-22.7138, -1406.353, 28.5081)},    // The Families
            //{ "The Ballas Gang", new Vector3(109.8277, -2014.295, 17.3004)},     // The Ballas Gang
            //{ "Los Santos Vagos", new Vector3(823.0684, -2336.574, 29.4647)},     // Los Santos Vagos
            //{ "Marabunta Grande", new Vector3(1377.317, -2096.102, 51.6482)},     // Marabunta Grande
            //{ "Blood Street", new Vector3(967.5901, -1831.698, 30.2642)},     // Blood Street
            { "ЧОО", new Vector3(1398.391, 1140.743, 113.3335) },
            //{ "Русская мафия", new Vector3(-108.7949, 987.5535, 234.7884) },
            //{ "Мексиканская мафия", new Vector3(-912.1727, -1462.1, 7.8268) },
            { "Итальянское посольство", new Vector3(-1887.016, 2053.428, 140.0099) },
			
        };
        public static void OpenGPSMenu(Player player, string cat, string exit="goback")
        {
            Menu menu = new Menu("gps", false, false);
            menu.Callback = callback_gps;
            menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = cat;
            menu.Add(menuItem);

            foreach (string next in Category[cat])
            {
                menuItem = new Menu.Item(next, Menu.MenuItem.Button);
                menuItem.Text = next;
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item(exit, Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_gps(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            switch (item.ID)
            {
                case "back":
                    MenuManager.Close(player);
                    OpenPlayerMenu(player).Wait();
                    return;
                case "goback":
                    OpenGPSMenu(player, "Категории", "back");
                    return;
                case "Гос.структуры":
                case "Работы":
                case "Банды":
                case "Мафии":
                case "Ближайшие места":
                    OpenGPSMenu(player, item.ID);
                    return;
                case "Мэрия":
                case "LSPD":
                case "Госпиталь":
                case "FBI":
                case "Электростанция":
                case "Отделение почты":
                case "Таксопарк":
                case "Автобусный парк":
                case "Стоянка газонокосилок":
                //case "Стоянка дальнобойщиков":
                case "Стоянка инкассаторов":
                //case "Стоянка автомехаников":
                //case "Marabunta Grande":
                //case "Los Santos Vagos":
                //case "The Ballas Gang":
                //case "The Families":
                //case "Блад Стрит":
                case "ЧОО":
                //case "Русская мафия":
                //case "Мексиканская мафия":
                case "Итальянское посольство":
                    Trigger.PlayerEvent(player, "createWaypoint", Points[item.ID].X, Points[item.ID].Y);
                    return;
                case "Ближайший банкомат":
                    Vector3 waypoint = MoneySystem.ATM.GetNearestATM(player);
                    Trigger.PlayerEvent(player, "createWaypoint", waypoint.X, waypoint.Y);
                    return;
                case "Ближайшая заправка":
                    waypoint = BCore.getNearestBiz(player, 1);
                    Trigger.PlayerEvent(player, "createWaypoint", waypoint.X, waypoint.Y);
                    return;
                case "Ближайший 24/7":
                    waypoint = BCore.getNearestBiz(player, 0);
                    Trigger.PlayerEvent(player, "createWaypoint", waypoint.X, waypoint.Y);
                    return;
                case "Ближайшая аренда авто":
                    waypoint = Rentcar.GetNearestRentArea(player.Position);
                    Trigger.PlayerEvent(player, "createWaypoint", waypoint.X, waypoint.Y);
                    return;
                case "Ближайшая остановка":
                    waypoint = Jobs.Bus.GetNearestStation(player.Position);
                    Trigger.PlayerEvent(player, "createWaypoint", waypoint.X, waypoint.Y);
                    return;
            }
        }

        public static void OpenServicesMenu(Player player)
        {
            Menu menu = new Menu("services", false, false);
            menu.Callback = callback_services;
			menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Вызовы";
            menu.Add(menuItem);

            menuItem = new Menu.Item("taxi", Menu.MenuItem.Button);
            menuItem.Text = "Вызвать такси";
            menu.Add(menuItem);


            menuItem = new Menu.Item("police", Menu.MenuItem.Button);
            menuItem.Text = "Вызвать полицию";
            menu.Add(menuItem);

            menuItem = new Menu.Item("ems", Menu.MenuItem.Button);
            menuItem.Text = "Вызвать скорую";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
        }
		
        private static void callback_services(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "taxi":
                    MenuManager.Close(player);
                    Jobs.Taxi.callTaxi(player);
                    return;
                case "repair":
                    MenuManager.Close(player);
                    Jobs.AutoMechanic.callMechanic(player);
                    return;
                case "police":
                    MenuManager.Close(player);
                    Trigger.PlayerEvent(player, "openInput", "Вызвать полицию", "Что произошло?", 30, "call_police");
                    return;
                case "ems":
                    MenuManager.Close(player);
                    Fractions.Ems.callEms(player);
                    return;
                case "back":
                    MenuManager.Close(player);
                    OpenPlayerMenu(player).Wait();
                    return;
            }
        }
		
		private static void callback_bank(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try
            {
                if (!Players.ContainsKey(player))
                {
                    MenuManager.Close(player);
                    return;
                }
                var acc = Main.Players[player];
                if (item.ID == "toplayer") // перевести на счёт
                {
                    MenuManager.Close(player);
                    Trigger.PlayerEvent(player, "openInput", $"Перевод на счёт", "Введите номер счёта:", 9, "banktoplayer");
                    return;
                }
                else if (item.ID == "tohouse") // перевести на дом
                {
                    var house = Houses.HouseManager.GetHouse(player, true);
                    if (house == null) 
                    {
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "У вас нет дома!", 3000);
                        return;
                    }

                    var maxMoney = Convert.ToInt32(house.Price / 100 * 0.005) * 24 * 7;
                    MenuManager.Close(player);
                    Trigger.PlayerEvent(player, "openInput", $"Перевод на дом", $"Введите количество: {Bank.Accounts[house.BankID].Balance} | {maxMoney}", 7, "banktohouse");
                    return;
                }
                else if (item.ID == "toapart") // перевести на дом
                {
                    var apartament = Houses.HouseManager.GetApart(player, true);
                    if (apartament == null)
                    {
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "У вас нет квартиры!", 3000);
                        return;
                    }

                    var maxMoney = Convert.ToInt32(apartament.Price / 100 * 0.005) * 24 * 7;
                    MenuManager.Close(player);
                    Trigger.PlayerEvent(player, "openInput", $"Перевод на квартиру", $"Введите количество: {Bank.Accounts[apartament.BankID].Balance} | {maxMoney}", 7, "banktoapart");
                    return;
                }
                else if (item.ID == "tobiz") // перевести на бизнес
                {
                    BCore.Bizness biz = BCore.BizList[acc.BizIDs[0]];
                    if (Main.Players[player].BizIDs.Count < 0)
                    {
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "У вас нет бизнеса!", 3000);
                        return;
                    }
                    var maxMoney = Convert.ToInt32(biz.Cost / 100 * 0.005) * 24 * 7;
                    MenuManager.Close(player);
                    Trigger.PlayerEvent(player, "openInput", $"Перевод на бизнес", $"Введите количество: {Bank.Accounts[biz.BankID].Balance} | {maxMoney}", 7, "banktobiz");
                    return;
                }
                
                else if (item.ID == "kursi") // курсы
                {
                    OpenKursMenu(player);
                    return;
                }
                else if (item.ID == "back") // назад
                {
                    MenuManager.Close(player);
                    OpenPlayerMenu(player).Wait();
                    return;
                }

                MenuManager.Close(player, false);

            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT BANK:\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private static void callback_kur(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try
            {
                if (!Players.ContainsKey(player))
                {
                    MenuManager.Close(player);
                    return;
                }
                if (item.ID == "back") // назад
                {
                    OpenBankMenu(player);
                    return;
                }

            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT BANK:\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static void OpenMayorMenu(Player player)
        {
            Menu menu = new Menu("citymanage", false, false);
            menu.Callback = callback_mayormenu;
			menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Казна";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info", Menu.MenuItem.Card);
            menuItem.Text = $"Деньги: {Fractions.Stocks.fracStocks[6].Money}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info2", Menu.MenuItem.Card);
            menuItem.Text = $"Собрано за последний час: {Fractions.Cityhall.lastHourTax}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("take", Menu.MenuItem.Button);
            menuItem.Text = "Получить деньги";
            menu.Add(menuItem);

            menuItem = new Menu.Item("put", Menu.MenuItem.Button);
            menuItem.Text = "Положить деньги";
            menu.Add(menuItem);

            menuItem = new Menu.Item("header2", Menu.MenuItem.Header);
            menuItem.Text = "Управление";
            menu.Add(menuItem);

            menuItem = new Menu.Item("fuelcontrol", Menu.MenuItem.Button);
            menuItem.Text = "Гос.заправка";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_mayormenu(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "take":
                    MenuManager.Close(player);
                    Trigger.PlayerEvent(player, "openInput", "Получить деньги из казны", "Количество", 6, "mayor_take");
                    return;
                case "put":
                    MenuManager.Close(player);
                    Trigger.PlayerEvent(player, "openInput", "Положить деньги в казну", "Количество", 6, "mayor_put");
                    return;
                case "fuelcontrol":
                    OpenFuelcontrolMenu(player);
                    return;
                case "back":
                    MenuManager.Close(player);
                    OpenPlayerMenu(player).Wait();
                    return;
            }
        }
        public static void OpenFuelcontrolMenu(Player player)
        {
            Menu menu = new Menu("fuelcontrol", false, false);
            menu.Callback = callback_fuelcontrol;
			menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Гос.заправка";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_city", Menu.MenuItem.Card);
            menuItem.Text = $"Мэрия. Осталось сегодня: {Fractions.Stocks.fracStocks[6].FuelLeft}/{Fractions.Stocks.fracStocks[6].FuelLimit}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_city", Menu.MenuItem.Button);
            menuItem.Text = "Установить лимит";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_police", Menu.MenuItem.Card);
            menuItem.Text = $"Полиция. Осталось сегодня: {Fractions.Stocks.fracStocks[7].FuelLeft}/{Fractions.Stocks.fracStocks[7].FuelLimit}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_police", Menu.MenuItem.Button);
            menuItem.Text = "Установить лимит";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_ems", Menu.MenuItem.Card);
            menuItem.Text = $"EMS. Осталось сегодня: {Fractions.Stocks.fracStocks[8].FuelLeft}/{Fractions.Stocks.fracStocks[8].FuelLimit}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_ems", Menu.MenuItem.Button);
            menuItem.Text = "Установить лимит";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_fib", Menu.MenuItem.Card);
            menuItem.Text = $"FBI. Осталось сегодня: {Fractions.Stocks.fracStocks[9].FuelLeft}/{Fractions.Stocks.fracStocks[9].FuelLimit}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_fib", Menu.MenuItem.Button);
            menuItem.Text = "Установить лимит";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_army", Menu.MenuItem.Card);
            menuItem.Text = $"Армия. Осталось сегодня: {Fractions.Stocks.fracStocks[14].FuelLeft}/{Fractions.Stocks.fracStocks[14].FuelLimit}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_army", Menu.MenuItem.Button);
            menuItem.Text = "Установить лимит";
            menu.Add(menuItem);

            menuItem = new Menu.Item("info_news", Menu.MenuItem.Card);
            menuItem.Text = $"News. Осталось сегодня: {Fractions.Stocks.fracStocks[15].FuelLeft}/{Fractions.Stocks.fracStocks[15].FuelLimit}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("set_news", Menu.MenuItem.Button);
            menuItem.Text = "Установить лимит";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_fuelcontrol(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            switch (item.ID)
            {
                case "set_city":
                    Trigger.PlayerEvent(player, "openInput", "Установить лимит", "Введите топливный лимит для мэрии в долларах", 5, "fuelcontrol_city");
                    return;
                case "set_police":
                    Trigger.PlayerEvent(player, "openInput", "Установить лимит", "Введите топливный лимит полиции мэрии в долларах", 5, "fuelcontrol_police");
                    return;
                case "set_ems":
                    Trigger.PlayerEvent(player, "openInput", "Установить лимит", "Введите топливный лимит для EMS в долларах", 5, "fuelcontrol_ems");
                    return;
                case "set_fib":
                    Trigger.PlayerEvent(player, "openInput", "Установить лимит", "Введите топливный лимит для FBI в долларах", 5, "fuelcontrol_fib");
                    return;
                case "set_army":
                    Trigger.PlayerEvent(player, "openInput", "Установить лимит", "Введите топливный лимит для армии в долларах", 5, "fuelcontrol_army");
                    return;
                case "set_news":
                    Trigger.PlayerEvent(player, "openInput", "Установить лимит", "Введите топливный лимит для News в долларах", 5, "fuelcontrol_news");
                    return;
                case "back":
                    OpenMayorMenu(player);
                    return;
            }
        }
        #endregion
        #endregion
    }
    public class CarInfo
    {
        public string Number { get; }
        public VehicleHash Model { get; }
        public Vector3 Position { get; }
        public Vector3 Rotation { get; }
        public int Color1 { get; }
        public int Color2 { get; }
        public int Price { get; }

        public CarInfo(string number, VehicleHash model, Vector3 position, Vector3 rotation, int color1, int color2, int price)
        {
            Number = number;
            Model = model;
            Position = position;
            Rotation = rotation;
            Color1 = color1;
            Color2 = color2;
            Price = price;
        }
    }
    public class oldConfig
    {
        public string ServerName { get; set; } = "RP1";
        public string ServerNumber { get; set; } = "1";
        public bool VoIPEnabled { get; set; } = false;
        public bool RemoteControl { get; set; } = false;
        public bool DonateChecker { get; set; } = false;
        public bool DonateSaleEnable { get; set; } = false;
        public int PaydayMultiplier { get; set; } = 1;
        public int ExpMultiplier { get; set; } = 1;
        public bool SCLog { get; set; } = false;
    }
    public class Trigger : Script
    {
        public static void PlayerEvent(Player Player, string eventName, params object[] args)
        {
            if (Thread.CurrentThread.Name == "Main") {
                NAPI.ClientEvent.TriggerClientEvent(Player, eventName, args);
                return;
            }
            NAPI.Task.Run(() =>
            {
                if (Player == null) return;
                NAPI.ClientEvent.TriggerClientEvent(Player, eventName, args);
            });
        }
        public static void PlayerEventInRange(Vector3 pos, float range, string eventName, params object[] args)
        {
            if (Thread.CurrentThread.Name == "Main")
            {
                NAPI.ClientEvent.TriggerClientEventInRange(pos, range, eventName, args);
                return;
            }
            NAPI.Task.Run(() =>
            {
                NAPI.ClientEvent.TriggerClientEventInRange(pos, range, eventName, args);
            });
        }
        public static void PlayerEventInDimension(uint dim, string eventName, params object[] args)
        {
            if (Thread.CurrentThread.Name == "Main")
            {
                NAPI.ClientEvent.TriggerClientEventInDimension(dim, eventName, args);
                return;
            }
            NAPI.Task.Run(() =>
            {
                NAPI.ClientEvent.TriggerClientEventInDimension(dim, eventName, args);
            });
        }
        public static void PlayerEventToPlayers(Player[] players, string eventName, params object[] args)
        {
            if (Thread.CurrentThread.Name == "Main")
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, eventName, args);
                return;
            }
            NAPI.Task.Run(() =>
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(players, eventName, args);
            });
        }
    }

    public static class PasswordRestore
    {

        private static nLog Log = new nLog("PassRestore");
        private static Config config = new Config("PassRestore");

        private static string mailFrom = config.TryGet<string>("From", "fiveuprpinfo@gmail.com");
        private static string mailTitle1 = config.TryGet<string>("Title1", "Password Restore");
        private static string mailTitle2 = config.TryGet<string>("Title2", "New Password");
        private static string mailBody1 = config.TryGet<string>("Body1", "<p>Код для восстановления пароля: {0}</p>");
        private static string mailBody2 = config.TryGet<string>("Body2", "<p>Вы успешно восстановили пароль, Ваш новый пароль: {0}</p>");

        private static string Server = config.TryGet<string>("SMTP", "smtp.gmail.com");
        private static string Password = config.TryGet<string>("Pass", "EvuYBbPO");
        private static int Port = config.TryGet<int>("Port", 587);

        public static void SendEmail(byte type, string email, int textcode)
        {
            try
            {
                MailMessage msg;
                if (type == 0) msg = new MailMessage(mailFrom, email, mailTitle1, string.Format(mailBody1, textcode));
                else msg = new MailMessage(mailFrom, email, mailTitle2, string.Format(mailBody2, textcode));
                msg.IsBodyHtml = true;
                SmtpClient smtpClient = new SmtpClient(Server, Port)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(mailFrom, Password),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };
                smtpClient.Send(msg);
                if (type == 0) Log.Debug($"Сообщение с кодом для восстановления пароля успешно отправлено на {email}!", nLog.Type.Success);
                else Log.Debug($"Сообщение с новым паролем успешно отправлено на {email}!", nLog.Type.Success);
            }
            catch (Exception ex)
            {
                Log.Write("EXCEPTION AT \"SendEmail\":\n" + ex.ToString(), nLog.Type.Error);
            }
        }

        
    }
}
