using System;
using System.Collections.Generic;
using NeptuneEVO.SDK;
using GTANetworkAPI;
using NeptuneEVO.Businesses;
using NeptuneEVO.Core;
using System.Data;
using System.Linq;
using Newtonsoft.Json;

namespace NeptuneEVO.Utils
{
    class Forbes : Script
    {

        private static nLog Log = new nLog("Forbes");

        private static Dictionary<string, int> Majors = new Dictionary<string, int>();

        public static void SyncMajors()
        {
            NAPI.Task.Run(() => { 
                try
                {
                    var database = MySQL.QueryRead($"SELECT * FROM `characters`");
                    Dictionary<string, int> nosync = new Dictionary<string, int> { };
                    Majors = new Dictionary<string, int> { };
                    foreach (DataRow Row in database.Rows)
                    {
                        if (Convert.ToInt32(Row["adminlvl"]) != 0) continue;

                        string nick = Row["firstname"].ToString() + "_" + Row["lastname"].ToString();
                        int money = GetPlayerAllMoney(nick, Convert.ToInt32(MoneySystem.Bank.Accounts[Convert.ToInt32(Row["bank"])].Balance) + Convert.ToInt32(Row["money"]));
                        nosync.Add(nick, money);
                    }

                    nosync = nosync.OrderBy(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
                    int max = 30;
                    if (nosync.Count < 30)
                        max = nosync.Count;
                    int i = 0;
                    foreach (KeyValuePair<string, int> pair in nosync.Reverse().ToDictionary(x => x.Key, x => x.Value))
                    {
                        if (i > max - 1) break;
                        Majors.Add(pair.Key, pair.Value); i++;
                    }
                }
                catch (Exception e) { Log.Write("MAJORS: " + e.ToString(), nLog.Type.Error); }
            }, 2000);
        }

        public static int GetPlayerAllMoney(string Name, int add)
        {
            try
            {
                int result = add;
                foreach (Houses.House house in Houses.HouseManager.Houses)
                    if (house.Owner == Name)
                    {
                        result += house.Price;
                        break;
                    }
                foreach (string number in VehicleManager.getAllPlayerVehicles(Name))
                {
                    result += BCore.CostForCar(VehicleManager.Vehicles[number].Model);
                }

                foreach(AirVehicle air in AirVehicles.getAllAirVehicles(Name).Values)
                {
                    result += BCore.CostForCar(air.Model);
                }

                foreach(Organization.OCore.Organization org in Organization.OCore.OrgList.Values)
                    if (org.Owner == Name)
                    {
                        result += Convert.ToInt32( MoneySystem.Bank.Accounts[org.BankID].Balance ) + org.Price;
                        break;
                    }


                foreach (BCore.Bizness biz in BCore.BizList.Values)
                    if (biz.Owner == Name)
                    {
                        result += biz.Cost;
                        break;
                    }

                return result;
            }
            catch (Exception e) { Log.Write("MAJORS: " + e.ToString(), nLog.Type.Error); return 0; }
        }

        [RemoteEvent("server::getbuy")]
        public static void GetBuy(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !player.HasData("BIZ_ID") || player.GetData<int>("BIZ_ID") == -1 || !BCore.BizList.ContainsKey(player.GetData<int>("BIZ_ID"))) return;

               BCore.BuyBiz(player);

            }
            catch { }
        }

        [RemoteEvent("bizinfo")]
        public static void RM_bizinfo(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !player.HasData("BIZ_ID") || player.GetData<int>("BIZ_ID") == -1 || !BCore.BizList.ContainsKey(player.GetData<int>("BIZ_ID"))) return;

                BCore.Bizness biz = Businesses.BCore.BizList[player.GetData<int>("BIZ_ID")];

                List<object> json = new List<object>
                {
                    $"{biz.GetName()} #{biz.ID}", biz.Owner, Fractions.Manager.getName(biz.Mafia), biz.Cost, biz.GetNalog() * 24, biz.GetMaxMaterials(), biz.GetNalogMaterials() * 6
                };
                
                Trigger.PlayerEvent(player, "client::setbizinfo", JsonConvert.SerializeObject(json));

            }
            catch { }
        }

        [RemoteEvent("getforbes")]
        public static void RM_getforbes(Player player)
        {
            try
            {
                List<object> data = new List<object>();
                foreach (KeyValuePair<string, int> obj in Majors)
                    data.Add(new List<object> { obj.Key, obj.Value });

                List<object> cars = new List<object>();
                List<object> bizlist = new List<object>();
                foreach (string number in VehicleManager.getAllPlayerVehicles(player.Name))
                    cars.Add($"{VehicleManager.Vehicles[number].Model} ({number})");

                foreach (int id in Main.Players[player].BizIDs)
                    if (Businesses.BCore.BizList.ContainsKey(id))
                    {
                        BCore.Bizness biz = Businesses.BCore.BizList[id];
                        bizlist.Add(new List<object> { $"{biz.GetName()} #{biz.ID}", MoneySystem.Bank.Accounts[biz.BankID].Balance, biz.GetNalog() * 24, biz.GetDay(), biz.Materials + "/" + biz.GetMaxMaterials(), BCore.GetVipCost(player, biz.Cost) });
                    }

                Trigger.PlayerEvent(player, "setforbes", JsonConvert.SerializeObject(data), JsonConvert.SerializeObject(cars), JsonConvert.SerializeObject(bizlist));
            }
            catch (Exception e) { Log.Write("GETFORBES: " + e.ToString(), nLog.Type.Error); }
        }

    }
}
