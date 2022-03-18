
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using NeptuneEVO.GUI;
using NeptuneEVO.Core.nAccount;
using NeptuneEVO.Core.Character;
using static NeptuneEVO.Core.VehicleManager;
using Newtonsoft.Json;

namespace NeptuneEVO.MoneySystem
{
    class Donations : Script
    {
        public static Queue<KeyValuePair<string, string>> toChange = new Queue<KeyValuePair<string, string>>();
        public static Queue<string> newNames = new Queue<string>();
        private static DateTime lastCheck = DateTime.Now;
        private static nLog Log = new nLog("Donations");
        private static Timer scanTimer;

        private static Config config = new Config("Donations");

        private static string SYNCSTR;
        private static string CHNGSTR;
        private static string NEWNSTR;

        private static string Connection;

        public static void LoadDonations()
        {
            Connection =
                $"Host={config.TryGet<string>("Host", "127.0.0.1")};" +
                $"User={config.TryGet<string>("User", "root")};" +
                $"Password={config.TryGet<string>("Password", "тут пароль")};" +
                $"Database={config.TryGet<string>("Database", "тут база")};" +
                $"{config.TryGet<string>("SSL", "SslMode=None;")}";

            SYNCSTR = string.Format("select * from completed where srv={0}", Main.oldconfig.ServerNumber);
            CHNGSTR = "update nicknames SET name='{0}' WHERE name='{1}' and srv={2}";
            NEWNSTR = "insert into nicknames(srv, name) VALUES ({0}, '{1}')";
        }
        #region Работа с таймером
        public static void Start()
        {
            scanTimer = new Timer(new TimerCallback(Tick), null, 90000, 90000);
        }

        public static void Stop()
        {
            scanTimer.Change(Timeout.Infinite, 0);
        }
        #endregion

        #region Проверка никнеймов и донатов
        private static void Tick(object state)
        {
            try
            {
                Log.Debug("Donate time");

                using (MySqlConnection connection = new MySqlConnection(Connection))
                {
                    connection.Open();

                    MySqlCommand command = new MySqlCommand();
                    command.Connection = connection;

                    while (toChange.Count > 0)
                    {
                        KeyValuePair<string, string> kvp = toChange.Dequeue();
                        command.CommandText = string.Format(CHNGSTR, kvp.Value, kvp.Key, Main.oldconfig.ServerNumber);
                        command.ExecuteNonQuery();
                    }

                    while (newNames.Count > 0)
                    {
                        string nickname = newNames.Dequeue();
                        command.CommandText = string.Format(NEWNSTR, Main.oldconfig.ServerNumber, nickname);
                        command.ExecuteNonQuery();
                    }

                    command.CommandText = SYNCSTR;
                    MySqlDataReader reader = command.ExecuteReader();

                    DataTable result = new DataTable();
                    result.Load(reader);
                    reader.Close();

                    foreach (DataRow Row in result.Rows)
                    {
                        int id = Convert.ToInt32(Row["id"]);
                        string name = Convert.ToString(Row["account"]).ToLower();
                        long reds = Convert.ToInt64(Row["amount"]);

                        try
                        {
                            if (Main.oldconfig.DonateSaleEnable)
                            {
                                reds = SaleEvent(reds);
                            }

                            if (!Main.Usernames.Contains(name))
                            {
                                Log.Write($"Can't find registred name for {name}!", nLog.Type.Warn);
                                continue;
                            }

                            var Player = Main.Accounts.FirstOrDefault(a => a.Value.Login == name).Key;
                            if (Player == null || Player.IsNull || !Main.Accounts.ContainsKey(Player))
                            {
                                MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{reds} where `login`='{name}'");
                            }
                            else
                            {
                                lock (Main.Players)
                                {
                                    Main.Accounts[Player].RedBucks += reds;
                                }
                                NAPI.Task.Run(() =>
                                {
                                    try
                                    {
                                        if (!Main.Accounts.ContainsKey(Player)) return;
                                        Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вам пришли {reds} UP", 3000);
                                        Trigger.PlayerEvent(Player, "starset", Main.Accounts[Player].RedBucks);
                                    }
                                    catch { }
                                });
                            }
                            //TODO: новый лог денег
                            //GameLog.Money("donate", $"player({Main.PlayerUUIDs[name]})", +stars);
                            GameLog.Money("server", name, reds, "donateRed");

                            command.CommandText = $"delete from completed where id={id}";
                            command.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            Log.Write($"Exception At Tick_Donations on {name}:\n" + e.ToString(), nLog.Type.Error);
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Log.Write("Exception At Tick_Donations:\n" + e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region Действия в донат-меню
        internal enum Type
        {
            Character,
            Nickname,
            Convert,
            BronzeVIP,
            SilverVIP,
            GoldVIP,
            PlatinumVIP,
            Warn,
			BusinessVIP,
            BigDaddyVIP,
            LuckyWheell3,
            LuckyWheell5,
            LuckyWheell10,
            LuckyWheell15,
            Business10Days,
            Business30Days,


        }

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                Timers.StartTask(5000, () => timer_donate());
				Timers.StartTask(60000, () => timer_vehicles());
                
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"DONATE\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

       

        public void timer_donate()
        {
            foreach (var player in NAPI.Pools.GetAllPlayers())
            {
				if (!Main.Accounts.ContainsKey(player)) continue;
                Account acc = Main.Accounts[player];
                var table = MySQL.QueryRead($"SELECT * FROM accounts WHERE `login`='{acc.Login}'");
                foreach (DataRow Row in table.Rows)
                {
                    int reds = Convert.ToInt32(Row["redbucks"]);
                    acc.RedBucks = reds;
                    Trigger.PlayerEvent(player, "redset", acc.RedBucks);
                    break;
                }
                
            }
        }

        
        public void timer_vehicles()
        {
            try
            {
                foreach (Player player in NAPI.Pools.GetAllPlayers())
                {
                    if (!Main.Players.ContainsKey(player)) continue;
                    if (!Main.Accounts.ContainsKey(player)) continue;
                    Main.Players[player].TimeMinutes += 1;
                    Main.Players[player].SessionTime += 1;
                    if (Main.Players[player].TimeMinutes % 180 == 0)
                    {
                        if (Main.Accounts[player].VipLvl == 6)
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 3 прокрута колеса фортуны!", 3000);
                            Main.Players[player].LuckyWheell += 3;
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 1 прокрут колеса фортуны!", 3000);
                            Main.Players[player].LuckyWheell += 1;
                        }
                        
                    }
                    GUI.Dashboard.sendStats(player);
                }
            }
            catch { }
        }

        
public void FindNewCars(DataTable table)
        {

            NAPI.Task.Run(() => {
                try
                {
                    foreach (DataRow Row in table.Rows)
                    {
                       if (VehicleManager.Vehicles.ContainsKey(Row["number"].ToString())) continue;
                       AddNewDonateCar(Row);
                    }
                    Log.Write($"New vehicles {VehicleManager.Vehicles.Count}!", nLog.Type.Info);
                }
                catch { }
            }, 1000);
        }

        public static void AddNewDonateCar(DataRow Row)
        {
            VehicleData data = new VehicleData();
            data.Holder = Convert.ToString(Row["holder"]);
            data.Model = Convert.ToString(Row["model"]);
            data.Health = Convert.ToInt32(Row["health"]);
            data.Fuel = Convert.ToInt32(Row["fuel"]);
            data.Price = Convert.ToInt32(Row["price"]);
            data.Components = JsonConvert.DeserializeObject<VehicleCustomization>(Row["components"].ToString());
            //if (Row["components"].ToString() == "null") data.Components = new VehicleCustomization();
            data.Items = JsonConvert.DeserializeObject<List<nItem>>(Row["items"].ToString());
            data.Position = Convert.ToString(Row["position"]);
            data.Rotation = Convert.ToString(Row["rotation"]);
            data.KeyNum = Convert.ToInt32(Row["keynum"]);
            data.Sell = Convert.ToInt32(Row["sell"]);
            data.Dirt = (float)Row["dirt"];
            Vehicles.Add(Convert.ToString(Row["number"]), data);
            Log.Write($"Add new vehicle!", nLog.Type.Info);
			var house = Houses.HouseManager.GetHouse(Convert.ToString(Row["holder"]), true);
                if (house != null)
                {
                     var garage = Houses.GarageManager.Garages[house.GarageID];
                     garage.SpawnCar(Convert.ToString(Row["number"]));
                }
        }
		
        [RemoteEvent("getdonate")]
        public static void GetNewReds(Player player)
        {
            
            Account acc = Main.Accounts[player];
            var result = MySQL.QueryRead($"SELECT * FROM accounts");
            var reds = acc.RedBucks;
            foreach (DataRow Row in result.Rows)
            {
                string login = Convert.ToString(Row["login"]);
                if (acc.Login != login) continue;
                reds = Convert.ToInt32(Row["redbucks"]);
            }
            acc.RedBucks = reds;

            Trigger.PlayerEvent(player, "redset", Main.Accounts[player].RedBucks);

        }

        [RemoteEvent("donate")]
        public void MakeDonate(Player Player, int id, string data)
        {
            try
            {
                Log.Write($"Data: {id} {data}");
                if (!Main.Accounts.ContainsKey(Player)) return;
                Account acc = Main.Accounts[Player];
                Type type = (Type)id;
                var result = MySQL.QueryRead($"SELECT * FROM accounts");
                var reds = acc.RedBucks;
                foreach (DataRow Row in result.Rows)
                {
                    string login = Convert.ToString(Row["login"]);
                    if (acc.Login != login) continue;
                    reds = Convert.ToInt32(Row["redbucks"]);
                }
                acc.RedBucks = reds;

                switch (type)
                {
                    case Type.Character:
                        {
                            if (acc.RedBucks < 100)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            Main.Accounts[Player].RedBucks -= 100;
                            GameLog.Money(acc.Login, "server", 100, "donateChar");
                            Customization.SendToCreator(Player);
                            break;
                        }
                    case Type.Nickname:
                        {
                            if (acc.RedBucks < 300)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }

                            if (!Main.PlayerNames.ContainsValue(Player.Name)) return;
                            try
                            {
                                string[] split = data.Split("_");
                                Log.Debug($"SPLIT: {split[0]} {split[1]}");

                                if (split[0] == "null" || string.IsNullOrEmpty(split[0]))
                                {
                                    Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Вы не указали имя!", 3000);
                                    return;
                                }
                                else if (split[1] == "null" || string.IsNullOrEmpty(split[1]))
                                {
                                    Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Вы не указали фамилию!", 3000);
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Write("ERROR ON CHANGENAME DONATION\n" + e.ToString());
                                return;
                            }

                            if (Main.PlayerNames.ContainsValue(data))
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Такое имя уже существует!", 3000);
                                return;
                            }

                            Player target = NAPI.Player.GetPlayerFromName(Player.Name);

                            if (target == null || target.IsNull) return;
                            else
                            {
                                Character.toChange.Add(Player.Name, data);
                                Main.Accounts[Player].RedBucks -= 300;
                                NAPI.Player.KickPlayer(target, "Смена ника");
                            }
                            GameLog.Money(acc.Login, "server", 300, "donateName");
                            break;
                        }
                    case Type.Convert:
                        {
                            int amount = 0;
                            if (!Int32.TryParse(data, out amount))
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Возникла ошибка, попоробуйте еще раз", 3000);
                                return;
                            }
                            amount = Math.Abs(amount);
                            if (amount <= 0)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Введите количество, равное 1 или больше.", 3000);
                                return;
                            }
                            if (Main.Accounts[Player].RedBucks < amount)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            Main.Accounts[Player].RedBucks -= amount;
                            GameLog.Money(acc.Login, "server", amount, "donateConvert");
                            amount = amount * 350;
                            MoneySystem.Wallet.Change(Player, +amount);
                            GameLog.Money($"donate", $"player({Main.Players[Player].UUID})", amount, $"donate");
                            break;
                        }
                    case Type.BronzeVIP:
                        {
                            if (Main.Accounts[Player].VipLvl >= 1)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "У вас уже куплен VIP статус!", 3000);
                                return;
                            }
                            if (acc.RedBucks < 150)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            Main.Accounts[Player].RedBucks -= 150;
                            GameLog.Money(acc.Login, "server", 150, "donateBVip");
                            Main.Accounts[Player].VipLvl = 1;
                            Main.Accounts[Player].VipDate = DateTime.Now.AddDays(7);
                            Dashboard.sendStats(Player);
                            break;
                        }
                    case Type.SilverVIP:
                        {
                            if (acc.VipLvl >= 1)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "У вас уже куплен VIP статус!", 3000);
                                return;
                            }
                            if (acc.RedBucks < 300)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            Main.Accounts[Player].RedBucks -= 300;
                            GameLog.Money(acc.Login, "server", 300, "donateSVip");
                            Main.Accounts[Player].VipLvl = 2;
                            Main.Accounts[Player].VipDate = DateTime.Now.AddDays(7);
                            Dashboard.sendStats(Player);
                            break;
                        }
                    case Type.GoldVIP:
                        {
                            if (Main.Accounts[Player].VipLvl >= 1)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "У вас уже куплен VIP статус!", 3000);
                                return;
                            }
                            if (acc.RedBucks < 500)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            Main.Accounts[Player].RedBucks -= 500;
                            GameLog.Money(acc.Login, "server", 500, "donateGVip");
                            Main.Accounts[Player].VipLvl = 3;
                            Main.Accounts[Player].VipDate = DateTime.Now.AddDays(7);
                            Dashboard.sendStats(Player);
                            break;
                        }
                    case Type.PlatinumVIP:
                        {
                            if (Main.Accounts[Player].VipLvl >= 1)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "У вас уже куплен VIP статус!", 3000);
                                return;
                            }
                            if (acc.RedBucks < 750)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            Main.Accounts[Player].RedBucks -= 750;
                            GameLog.Money(acc.Login, "server", 750, "donatePVip");
                            Main.Accounts[Player].VipLvl = 4;
                            Main.Accounts[Player].VipDate = DateTime.Now.AddDays(15);
                            Dashboard.sendStats(Player);
                            break;
                        }
                    case Type.Warn:
                        {
                            if (Main.Players[Player].Warns <= 0)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "У вас нет Warn'a!", 3000);
                                return;
                            }
                            if (acc.RedBucks < 2000)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            Main.Accounts[Player].RedBucks -= 2000;
                            GameLog.Money(acc.Login, "server", 2000, "donateWarn");
                            Main.Players[Player].Warns -= 1;
                            Dashboard.sendStats(Player);
                            break;
                        }
					case Type.BusinessVIP:
                        {
                            if (Main.Accounts[Player].VipLvl >= 1)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "У вас уже куплен VIP статус!", 3000);
                                return;
                            }
                            if (acc.RedBucks < 2500)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            Main.Accounts[Player].RedBucks -= 2500;
                            GameLog.Money(acc.Login, "server", 2500, "donateBIZVip");
                            Main.Accounts[Player].VipLvl = 5;
                            Main.Accounts[Player].VipDate = DateTime.Now.AddDays(7);
                            Dashboard.sendStats(Player);
                            break;
                        }
                    case Type.BigDaddyVIP:
                        {
                            if (Main.Accounts[Player].VipLvl >= 1)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "У вас уже куплен VIP статус!", 3000);
                                return;
                            }
                            if (acc.RedBucks < 3500)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            Main.Accounts[Player].RedBucks -= 3500;
                            GameLog.Money(acc.Login, "server", 3500, "donateBDVip");
                            Main.Accounts[Player].VipLvl = 6;
                            Main.Accounts[Player].VipDate = DateTime.Now.AddDays(14);
                            Dashboard.sendStats(Player);
                            break;
                        }
                    case Type.LuckyWheell3:
                        {
                            if (acc.RedBucks < 75)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            Main.Accounts[Player].RedBucks -= 75;
                            GameLog.Money(acc.Login, "server", 75, "donateLucky3");
                            Main.Players[Player].LuckyWheell += 3;
                            Dashboard.sendStats(Player);
                            Notify.Send(Player, NotifyType.Success, NotifyPosition.TopCenter, "Вы приобрели 3 колесоа фортуны", 3000);
                            break;
                        }
                    case Type.LuckyWheell5:
                        {
                            if (acc.RedBucks < 100)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            Main.Accounts[Player].RedBucks -= 100;
                            GameLog.Money(acc.Login, "server", 100, "donateLucky5");
                            Main.Players[Player].LuckyWheell += 5;
                            Dashboard.sendStats(Player);
                            Notify.Send(Player, NotifyType.Success, NotifyPosition.TopCenter, "Вы приобрели 5 колесов фортуны", 3000);
                            break;
                        }
                    case Type.LuckyWheell10:
                        {
                            if (acc.RedBucks < 180)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            Main.Accounts[Player].RedBucks -= 180;
                            GameLog.Money(acc.Login, "server", 180, "donateLucky10");
                            Main.Players[Player].LuckyWheell += 10;
                            Dashboard.sendStats(Player);
                            Notify.Send(Player, NotifyType.Success, NotifyPosition.TopCenter, "Вы приобрели 10 колесов фортуны", 3000);
                            break;
                        }
                    case Type.LuckyWheell15:
                        {
                            if (acc.RedBucks < 230)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            Main.Accounts[Player].RedBucks -= 230;
                            GameLog.Money(acc.Login, "server", 230, "donateLucky15");
                            Main.Players[Player].LuckyWheell += 15;

                            Dashboard.sendStats(Player);
                            Notify.Send(Player, NotifyType.Success, NotifyPosition.TopCenter, "Вы приобрели 15 колесов фортуны", 3000);
                            break;
                        }
                    case Type.Business10Days:
                        {
                            if (acc.RedBucks < 500)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            if (Main.Players[Player].BizIDs.Count == 0)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "У вас нет бизнеса!", 3000);
                                return;
                            }
                            NAPI.ClientEvent.TriggerClientEvent(Player, "client::closetablet");
                            Trigger.PlayerEvent(Player, "openInput", "Улучшение бизнеса на 10 дней", "Введите ID вашего бизнеса", 5, "donat_biz10days");

                            break;
                        }
                    case Type.Business30Days:
                        {
                            if (acc.RedBucks < 1250)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                                return;
                            }
                            if (Main.Players[Player].BizIDs.Count == 0)
                            {
                                Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "У вас нет бизнеса!", 3000);
                                return;
                            }
                            NAPI.ClientEvent.TriggerClientEvent(Player, "client::closetablet");
                            Trigger.PlayerEvent(Player, "openInput", "Улучшение бизнеса на 30 дней", "Введите ID вашего бизнеса", 5, "donat_biz30days");
                            break;
                        }
                    case (Type)16:
                    case (Type)17:
                    case (Type)18:
                    case (Type)19:
                    case (Type)20:
                    case (Type)21:
                    case (Type)22:
                    case (Type)23:
                    case (Type)24:
                    case (Type)25:
                    case (Type)26:
                        int lic = id - 16;
                        for (int i = 0; i < 11 - Main.Players[Player].Licenses.Count; i++)
                        {
                            Main.Players[Player].Licenses.Add(false);
                        }

                        if (Main.Players[Player].Licenses[lic])
                        {
                            Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "У вас уже есть эта лицензия", 3000);
                            return;
                        }

                        if (acc.RedBucks < Prices[lic])
                        {
                            Notify.Send(Player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                            return;
                        }

                        GUI.Dashboard.sendStats(Player);

                        Main.Accounts[Player].RedBucks -= Prices[lic];

                        Main.Players[Player].Licenses[lic] = true;

                        Notify.Send(Player, NotifyType.Success, NotifyPosition.TopCenter, "Вы приобрели лицензию!", 3000);

                        break;


                }
                //Log.Write(Main.Players[Player.Handle].Starbucks.ToString(), Logger.Type.Debug);
                MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[Player].RedBucks} where `login`='{Main.Accounts[Player].Login}'");
                Trigger.PlayerEvent(Player, "redset", Main.Accounts[Player].RedBucks);
            }
            catch (Exception e) { Log.Write("donate: " + e.Message, nLog.Type.Error); }
        }
        #endregion

        static List<int> Prices = new List<int>
        {
            15, // 16
            30, // 17
            35, // 18
            0, // 19
            50, // 20
            75, // 21
            0, // 22
            150, // 23
            150, // 24
            700, // 25
            100, // 26
            100 // 27
        };

        [RemoteEvent("changenum")]
        private static void ChangeNum(Player player, int index, string number)
        {
            NAPI.Task.Run(() => { 
                try
                {
                    if (!Main.Players.ContainsKey(player) || index < 0) return;
                    List<string> vehicles = VehicleManager.getAllPlayerVehicles(player.Name);

                    Account acc = Main.Accounts[player];

                    if (vehicles[index] == null) return;

                    if (acc.RedBucks < 899)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Недостаточно UP!", 3000);
                        return;
                    }

                    if(VehicleManager.Vehicles.ContainsKey(number))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.TopCenter, "Уже существует такой номер!", 3000);
                        return;
                    }

                    string num = vehicles[index];
                    foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                    {
                        if (veh.NumberPlate == num)
                        {
                            Houses.House house = Houses.HouseManager.GetHouse(player, true);
                            Houses.House apart = Houses.HouseManager.GetApart(player, true);
                            if (house == null)
                                if (apart != null)
                                    house = apart;

                            if (house.GarageID != 0)
                            {
                                if (Houses.GarageManager.Garages[house.GarageID].entityVehicles.ContainsKey(num))
                                    Houses.GarageManager.Garages[house.GarageID].entityVehicles.Remove(num);
                                else if (Houses.GarageManager.Garages[house.GarageID].vehiclesOut.ContainsKey(num))
                                    Houses.GarageManager.Garages[house.GarageID].vehiclesOut.Remove(num);
                            }

                            veh.Delete();

                            var vData = VehicleManager.Vehicles[num];
                            VehicleManager.Vehicles.Add(number, vData);

                            VehicleManager.Vehicles.Remove(num);
                            MySQL.Query($"UPDATE vehicles SET number='{num}' WHERE number='{number}'");


                            Main.Accounts[player].RedBucks -= 899;

                            MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");
                            Trigger.PlayerEvent(player, "redset", Main.Accounts[player].RedBucks);
                            Notify.Send(player, NotifyType.Success, NotifyPosition.TopCenter, $"Вы сменили номер на {ParkManager.GetNormalName(VehicleManager.Vehicles[number].Model)}", 3000);

                            Utils.Forbes.RM_getforbes(player);


                            break;
                        }
                    }



                }
                catch { }
            });
        }

        public static long SaleEvent(long input)
        {
            if (input < 1000) return input;
            if (input < 3000) return input + (input / 30 * 20);
            if (input < 5000) return input + (input / 30 * 25);
            if (input < 10000) return input + (input / 30 * 30);
            if (input < 14000) return input + (input / 30 * 35);
            if (input >= 14000) return input + (input / 30 * 50);
            // else, but never used
            return input;
        }

        public static void Rename(string Old, string New)
        {
            toChange.Enqueue(
                new KeyValuePair<string, string>(Old, New));
        }
    }
}
