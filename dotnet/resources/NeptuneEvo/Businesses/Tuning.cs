using GTANetworkAPI;
using System;
using System.Collections.Generic;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;
using Newtonsoft.Json;
using System.Linq;

namespace NeptuneEVO.Businesses
{
    class TuningI : Script
    {
        public static int CostForTuning = 5;

        private static nLog Log = new nLog("TUNING");

        [Command("dt")]
        public static void DT(Player player)
        {
            BasicSync.DetachObject(player);
        }

        public class Tuning : BCore.Bizness
        {

            public Tuning(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 72;
                Name = "LS Customs";
                BlipColor = 4;
                BlipType = 566;
                Range = 5f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;

                if (!player.IsInVehicle || !player.Vehicle.HasData("ACCESS") || player.Vehicle.GetData<string>("ACCESS") != "PERSONAL")
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в личной машине", 3000);
                    return;
                }
                if (player.Vehicle.Class == 13)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Велосипед не может быть затюнингован", 3000);
                    return;
                }
                /*if (player.Vehicle.Class == 8)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Тюнинг пока что недоступен для мотоциклов :( Скоро исправим", 3000);
                    return;
                }*/
                var vdata = VehicleManager.Vehicles[player.Vehicle.NumberPlate];
                if (!TuningS.ContainsKey(vdata.Model))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В данный момент для Вашего т/с тюнинг не доступен", 3000);
                    return;
                }

                //var occupants = VehicleManager.GetVehicleOccupants(player.Vehicle);
                //for(int i = occupants.Count; i > -1; i--)
                //    if (occupants[i] != player)
                //    VehicleManager.WarpPlayerOutOfVehicle(occupants[i]);
                //
                //Trigger.PlayerEvent(player, "tuningSeatsCheck");

                var occupants = VehicleManager.GetVehicleOccupants(player.Vehicle);
                foreach (var p in occupants)
                {
                    if (p != player)
                        VehicleManager.WarpPlayerOutOfVehicle(p);
                }

                Trigger.PlayerEvent(player, "tuningSeatsCheck");

            }

            public static int GetModelPrice(List<Dictionary<string, int>> table, string model)
            {
                int result = -1;
                foreach (Dictionary<string, int> vit in table)
                    foreach (KeyValuePair<string, int> fat in vit)
                        if (fat.Key == model)
                            result = fat.Value;
                return result;
            }
                
        }

        [RemoteEvent("tuningSeatsCheck")]
        public static void RemoteEvent_tuningSeatsCheck(Player player)
        {
            try
            {
                if (!player.IsInVehicle || !player.Vehicle.HasData("ACCESS") || player.Vehicle.GetData<string>("ACCESS") != "PERSONAL")
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в личной машине", 3000);
                    return;
                }
                if (player.Vehicle.Class == 13)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Велосипед не может быть затюнингован", 3000);
                    return;
                }
                /*if (player.Vehicle.Class == 8)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Тюнинг пока что недоступен для мотоциклов :( Скоро исправим", 3000);
                    return;
                }*/
                var vdata = VehicleManager.Vehicles[player.Vehicle.NumberPlate];
                if (!TuningS.ContainsKey(vdata.Model))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В данный момент для Вашего т/с тюнинг не доступен", 3000);
                    return;
                }

                if (player.GetData<int>("BIZ_ID") == -1) return;
                if (player.HasData("FOLLOWING"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                    return;
                }

                Main.Players[player].TuningShop = player.GetData<int>("BIZ_ID");

                var veh = player.Vehicle;
                var dim = Dimensions.RequestPrivateDimension(player);
                NAPI.Entity.SetEntityDimension(veh, dim);
                NAPI.Entity.SetEntityDimension(player, dim);

                player.SetIntoVehicle(veh, 0);

                NAPI.Entity.SetEntityPosition(veh, new Vector3(-337.7784, -136.5316, 38.7032));
                NAPI.Entity.SetEntityRotation(veh, new Vector3(0.0, 0.0, 148.9986));

                var modelPrice = Tuning.GetModelPrice(AutoShopI.ProductsList, VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model);
                var modelPriceMod = (modelPrice < 150000) ? 1 : 2;

                //NAPI.Entity.SetEntityVelocity(veh, new Vector3(0, 0, 0));

                Trigger.PlayerEvent(player, "openTun", CostForTuning, VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model, modelPriceMod, JsonConvert.SerializeObject(VehicleManager.Vehicles[player.Vehicle.NumberPlate].Components));
            }
            catch (Exception e) { Log.Write("tuningSeatsCheck: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("exitTuning")]
        public static void Exit(Player player)
        {
            try
            {
                int bizID = Main.Players[player].TuningShop;

                var veh = player.Vehicle;
                NAPI.Entity.SetEntityDimension(veh, 0);
                NAPI.Entity.SetEntityDimension(player, 0);

                player.SetIntoVehicle(veh, 0);

                NAPI.Entity.SetEntityPosition(veh, BCore.BizList[bizID].GetPos() + new Vector3(0, 0, 1));
                VehicleManager.ApplyCustomization(veh);
                Dimensions.DismissPrivateDimension(player);
                Main.Players[player].TuningShop = -1;
            }
            catch (Exception e) { Log.Write("ExitTuning: " + e.Message, nLog.Type.Error); }
        }

static Dictionary<int, int> ArmorPrice = new Dictionary<int, int>{
	{-1, 120000},
	{0, 220000},
	{1, 320000},
	{2, 420000},
	{3, 520000},
	{4, 620000},
};

        [RemoteEvent("buyTuning")]
        public static void Buy(Player player, params object[] arguments)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;

                int bizID = Main.Players[player].TuningShop;

                var cat = Convert.ToInt32(arguments[0].ToString());
                var id = Convert.ToInt32(arguments[1].ToString());

                var wheelsType = -1;
                var r = 0;
                var g = 0;
                var b = 0;

                if (cat == 19)
                    wheelsType = Convert.ToInt32(arguments[2].ToString());
                else if (cat == 20)
                {
                    r = Convert.ToInt32(arguments[2].ToString());
                    g = Convert.ToInt32(arguments[3].ToString());
                    b = Convert.ToInt32(arguments[4].ToString());
                }

                var vehModel = VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model;

                var modelPrice = Tuning.GetModelPrice(AutoShopI.ProductsList, vehModel);
                var modelPriceMod = (modelPrice < 150000) ? 1 : 2;

                var price = 0;
                if (cat <= 9)
                    price = Convert.ToInt32(TuningS[vehModel][cat].FirstOrDefault(el => el.Item1 == id).Item3 * CostForTuning / 100.0);
                else if (cat <= 18)
					price = Convert.ToInt32(TuningPrices[cat][id.ToString()] * modelPriceMod * CostForTuning / 100.0);
                else if (cat == 19)
                    price = Convert.ToInt32(TuningWheels[wheelsType][id] * CostForTuning / 100.0);
                else if (cat == 21)
                    price = 5000;
				else if (cat == 26)
                    price = ArmorPrice[id];
                else
                    price = Convert.ToInt32(5000 * CostForTuning / 100.0);
                 
                if (Main.Players[player].Money < price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вам не хватает ещё {price - Main.Players[player].Money}$ для покупки этой модификации", 3000);
                    Trigger.PlayerEvent(player, "tunBuySuccess", -2);
                    return;
                }

                var amount = Convert.ToInt32(price * 0.75 / 2000);
                if (amount <= 0) amount = 1;

                //GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", price, $"buyTuning({player.Vehicle.NumberPlate},{cat},{id})");
                MoneySystem.Wallet.Change(player, -price);
                Trigger.PlayerEvent(player, "tunBuySuccess", id);

                var number = player.Vehicle.NumberPlate;

                switch (cat)
                {
                    case 0:
                        VehicleManager.Vehicles[number].Components.Muffler = id;
                        break;
                    case 1:
                        VehicleManager.Vehicles[number].Components.SideSkirt = id;
                        break;
                    case 2:
                        VehicleManager.Vehicles[number].Components.Hood = id;
                        break;
                    case 3:
                        VehicleManager.Vehicles[number].Components.Spoiler = id;
                        break;
                    case 4:
                        VehicleManager.Vehicles[number].Components.Lattice = id;
                        break;
                    case 5:
                        VehicleManager.Vehicles[number].Components.Wings = id;
                        break;
                    case 6:
                        VehicleManager.Vehicles[number].Components.Roof = id;
                        break;
                    case 7:
                        VehicleManager.Vehicles[number].Components.Vinyls = id;
                        break;
                    case 8:
                        VehicleManager.Vehicles[number].Components.FrontBumper = id;
                        break;
                    case 9:
                        VehicleManager.Vehicles[number].Components.RearBumper = id;
                        break;
                    case 10:
                        VehicleManager.Vehicles[number].Components.Engine = id;
                        break;
                    case 11:
                        VehicleManager.Vehicles[number].Components.Turbo = id;
                        break;
                    case 12:
                        VehicleManager.Vehicles[number].Components.Horn = id;
                        break;
                    case 13:
                        VehicleManager.Vehicles[number].Components.Transmission = id;
                        break;
                    case 14:
                        VehicleManager.Vehicles[number].Components.WindowTint = id;
                        break;
                    case 15:
                        VehicleManager.Vehicles[number].Components.Suspension = id;
                        break;
                    case 16:
                        VehicleManager.Vehicles[number].Components.Brakes = id;
                        break;
                    case 17:
                        VehicleManager.Vehicles[number].Components.Headlights = id;
                        player.Vehicle.SetSharedData("hlcolor", id);
                        Trigger.PlayerEvent(player, "VehStream_SetVehicleHeadLightColor", player.Vehicle.Handle, id);
                        break;
                    case 18:
                        VehicleManager.Vehicles[number].Components.NumberPlate = id;
                        break;
                    case 19:
                        VehicleManager.Vehicles[number].Components.Wheels = id;
                        VehicleManager.Vehicles[number].Components.WheelsType = wheelsType;
                        break;
                    case 20:
                        if (id == 0)
                            VehicleManager.Vehicles[number].Components.PrimColor = new Color(r, g, b);
                        else if (id == 1)
                            VehicleManager.Vehicles[number].Components.SecColor = new Color(r, g, b);
                        else if (id == 2)
                            VehicleManager.Vehicles[number].Components.NeonColor = new Color(r, g, b);
                        else if (id == 3)
                            VehicleManager.Vehicles[number].Components.NeonColor = new Color(0, 0, 0, 0);
                        break;
                    case 21:
                        VehicleManager.Vehicles[number].Components.PrimModColor = id;
                        VehicleManager.Vehicles[number].Components.SecModColor = id;
                        break;
					case 26:
                        VehicleManager.Vehicles[number].Components.Armor = id;
                        break;
                }

                Utils.QuestsManager.AddQuestProcess(player, 7);

                VehicleManager.Save(number);
                VehicleManager.ApplyCustomization(player.Vehicle);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы купили и установили данную модификацию", 3000);
                Trigger.PlayerEvent(player, "tuningUpd", JsonConvert.SerializeObject(VehicleManager.Vehicles[number].Components));
            }
            catch (Exception e) { Log.Write("buyTuning: " + e.Message, nLog.Type.Error); }
        }

        public static Dictionary<string, Dictionary<int, List<Tuple<int, string, int>>>> TuningS = new Dictionary<string, Dictionary<int, List<Tuple<int, string, int>>>>()
        {
			
           {"apriora", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный спойлер 4", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный пер. бампер 4", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(0, "Улучшенная решётка 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная решётка 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенная решётка 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенная решётка 3", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(2, "Улучшенный капот 1", 600000),
             new Tuple<int, string, int>(3, "Улучшенный капот 2", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000)
      }},
               {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный глушитель 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный глушитель 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный глушитель 4", 600000)
      }}   
}},

            {"oka", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"g6", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"19g63", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"z4bmw", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"zentenario", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
{"lamboreventon", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
            {"2019m5", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"viper", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 2", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }}   
}},
            {"v250", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"velar", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
           {"rs318", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 3", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный зад. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный зад. бампер 3", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная крыша 1", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(0, "Улучшенная решётка 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная решётка 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенная решётка 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенная решётка 3", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенные пороги 2", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
            {"gls63", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rs5r", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"lc500", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 3", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный зад. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный зад. бампер 3", 600000)
      }},   
      {5,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартное крыло", 600000),
             new Tuple<int, string, int>(0, "Улучшенное крыло 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенное крыло 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенное крыло 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенное крыло 3", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенные пороги 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенные пороги 3", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 1", 600000)
      }}   
}},
            {"sq72016", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rmodrs7", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }}   
}},
            {"nismo20", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"911turbos", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"19s63", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"swinger", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"g65", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 3", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный зад. бампер 2", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
			{"lx2018", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(0, "Улучшенная решётка 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000)
      }}   
}},
            {"675ltsp", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }}   
}},
            {"amggt63s", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rmodbentleygt", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"jes", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"cyber", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"599xxevo", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"dmc12", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"divo", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"lada2107", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 3", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000)
      }},   
      {5,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартное крыло", 600000),
             new Tuple<int, string, int>(0, "Улучшенное крыло 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенное крыло 1", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(0, "Улучшенная решётка 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная решётка 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенная решётка 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенная решётка 3", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный глушитель 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный глушитель 3", 600000)
      }}   
}},
            {"kalina", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"e34", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(0, "Улучшенная решётка 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная решётка 1", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000)
      }},
        {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный глушитель 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный глушитель 3", 600000)
      }}   
}},
            {"uaz3159", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"s600w220", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"granta", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"m3e46", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000)
      }}   
}},
			{"gemera", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"lanex400", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"tahoe", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
           {"x5e53", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(-1, "Улучшенный спойлер 0", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
            {"e55w211", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"gto65", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"skyline", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(2, "Улучшенный зад. бампер 1", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(2, "Улучшенные пороги 1", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
            {"vwtouareg", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rr12", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000)
      }}   
}},
            {"sti", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"a80", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный пер. бампер 4", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный капот 2", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенные пороги 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенные пороги 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенные пороги 4", 600000)
      }},
                { 0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
            {"qashqai16", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"passat", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 3", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }}   
}},
            {"superb", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"mlbrabus", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
            {"ody18", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"camry18", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"fx50s", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"bmwm2", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"evoque", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000),
             new Tuple<int, string, int>(3, "Улучшенный зад. бампер 2", 600000),
             new Tuple<int, string, int>(4, "Улучшенный зад. бампер 3", 600000)
      }},
                {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
            {"mustangbkit", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rmodcamaro", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный капот 2", 600000)
      }}   
}},
            {"m4f82", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный спойлер 4", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный зад. бампер 2", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный капот 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный капот 3", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
            {"718caymans", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"teslapd", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"c8", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"lp570", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"lex570", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rmodjeep", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000)
      }}   
}},
            {"c63", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный зад. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный зад. бампер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный зад. бампер 4", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная крыша 1", 600000),
             new Tuple<int, string, int>(3, "Улучшенная крыша 2", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(0, "Улучшенная решётка 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная решётка 1", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный капот 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный капот 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный капот 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный капот 5", 600000),
             new Tuple<int, string, int>(6, "Улучшенный капот 6", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000)
      }}   
}},
            {"q820", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"e63amg", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"esv", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"vxr", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"c63coupe", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"cls53", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"levante", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"navigator", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"x5g05", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"x7bmw", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"g770", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"bmci", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный спойлер 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный спойлер 5", 600000),
             new Tuple<int, string, int>(6, "Улучшенный спойлер 6", 600000),
             new Tuple<int, string, int>(7, "Улучшенный спойлер 7", 600000),
             new Tuple<int, string, int>(8, "Улучшенный спойлер 8", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 3", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(0, "Улучшенная решётка 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная решётка 1", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный капот 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный капот 3", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный глушитель 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный глушитель 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный глушитель 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный глушитель 5", 600000),
             new Tuple<int, string, int>(6, "Улучшенный глушитель 6", 600000),
             new Tuple<int, string, int>(7, "Улучшенный глушитель 7", 600000)
      }}   
}},
{"urban", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000)
      }}   
}},
{"fxxk", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
{"sixtyone41", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
{"g63", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(0, "Улучшенная решётка 0", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000)
      }}   
}},
            {"rs7r", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"teslax", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"amggtr", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"cayen19", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"x6mf16", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"r820", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный спойлер 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный спойлер 5", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(0, "Улучшенная решётка 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная решётка 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенная решётка 2", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный капот 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный капот 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный капот 4", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000)
      }}   
}},
            {"f458", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000)
      }}   
}},
            {"panamera17turbo", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"slsamg", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }}   
}},
            {"bentaygast", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"18performante", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"urus", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rrw13", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"laferrari", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {5,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартное крыло", 600000),
             new Tuple<int, string, int>(0, "Улучшенное крыло 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенное крыло 1", 600000)
      }}   
}},
            {"f8t", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"g63amg6x6", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный пер. бампер 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный пер. бампер 5", 600000),
             new Tuple<int, string, int>(6, "Улучшенный пер. бампер 6", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный зад. бампер 2", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная крыша 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенная крыша 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенная крыша 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенная крыша 4", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(0, "Улучшенная решётка 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная решётка 1", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000)
      }}   
}},
            {"cullinan", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {5,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартное крыло", 600000),
             new Tuple<int, string, int>(0, "Улучшенное крыло 0", 600000)
      }}   
}},
            {"18phantom", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"p1", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
           {"918", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная крыша 1", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }}   
}},
            {"agerars", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный глушитель 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный глушитель 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный глушитель 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный глушитель 5", 600000)
      }}   
}},
            {"veneno", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 3", 600000),
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(2, "Улучшенный зад. бампер 1", 600000),
             new Tuple<int, string, int>(3, "Улучшенный зад. бампер 2", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(0, "Улучшенная решётка 0", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(2, "Улучшенные пороги 1", 600000)
      }}   
}},
            {"rmodchiron300", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"e60", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"kia", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"jp12", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный спойлер 4", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный пер. бампер 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный пер. бампер 5", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный зад. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный зад. бампер 3", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная крыша 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенная крыша 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенная крыша 3", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(0, "Улучшенная решётка 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная решётка 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенная решётка 2", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный капот 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный капот 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный капот 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный капот 5", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенные пороги 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенные пороги 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенные пороги 4", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000),
             new Tuple<int, string, int>(4, "Улучшенный глушитель 2", 600000)
      }}   
}},
            {"e63b", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rsvr16", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"demonhawk", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"19s650", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"amggtrr20", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rmodbmwm8", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000)
      }}   
}},
            {"venatus", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
           {"720s", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {5,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартное крыло", 600000),
             new Tuple<int, string, int>(0, "Улучшенное крыло 0", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
            {"huayra", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(5, "Улучшенный пер. бампер 3", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {5,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартное крыло", 600000),
             new Tuple<int, string, int>(0, "Улучшенное крыло 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(2, "Улучшенные пороги 1", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный глушитель 2", 600000)
      }}   
}},
            {"zim", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"m4lb", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный спойлер 4", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
            {"audir8lb", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"boss302", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rmodmi8lb", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный зад. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный зад. бампер 3", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000)
      }}   
}},
            {"gt17", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"manscull", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rmodsian", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"RAPTOR150", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"mansm8", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"2018s63", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"asvj", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"supersport", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rrst", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rmodrs6", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"500gtrlam", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный спойлер 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный спойлер 5", 600000),
             new Tuple<int, string, int>(6, "Улучшенный спойлер 6", 600000),
             new Tuple<int, string, int>(7, "Улучшенный спойлер 7", 600000),
             new Tuple<int, string, int>(8, "Улучшенный спойлер 8", 600000),
             new Tuple<int, string, int>(9, "Улучшенный спойлер 9", 600000),
             new Tuple<int, string, int>(10, "Улучшенный спойлер 10", 600000),
             new Tuple<int, string, int>(11, "Улучшенный спойлер 11", 600000),
             new Tuple<int, string, int>(12, "Улучшенный спойлер 12", 600000),
             new Tuple<int, string, int>(13, "Улучшенный спойлер 13", 600000),
             new Tuple<int, string, int>(14, "Улучшенный спойлер 14", 600000),
             new Tuple<int, string, int>(15, "Улучшенный спойлер 15", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный пер. бампер 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный пер. бампер 5", 600000),
             new Tuple<int, string, int>(6, "Улучшенный пер. бампер 6", 600000),
             new Tuple<int, string, int>(7, "Улучшенный пер. бампер 7", 600000),
             new Tuple<int, string, int>(8, "Улучшенный пер. бампер 8", 600000),
             new Tuple<int, string, int>(9, "Улучшенный пер. бампер 9", 600000),
             new Tuple<int, string, int>(10, "Улучшенный пер. бампер 10", 600000),
             new Tuple<int, string, int>(11, "Улучшенный пер. бампер 11", 600000),
             new Tuple<int, string, int>(12, "Улучшенный пер. бампер 12", 600000),
             new Tuple<int, string, int>(13, "Улучшенный пер. бампер 13", 600000),
             new Tuple<int, string, int>(14, "Улучшенный пер. бампер 14", 600000),
             new Tuple<int, string, int>(15, "Улучшенный пер. бампер 15", 600000),
             new Tuple<int, string, int>(16, "Улучшенный пер. бампер 16", 600000),
             new Tuple<int, string, int>(17, "Улучшенный пер. бампер 17", 600000),
             new Tuple<int, string, int>(18, "Улучшенный пер. бампер 18", 600000),
             new Tuple<int, string, int>(19, "Улучшенный пер. бампер 19", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный зад. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный зад. бампер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный зад. бампер 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный зад. бампер 5", 600000),
             new Tuple<int, string, int>(7, "Улучшенный зад. бампер 6", 600000),
             new Tuple<int, string, int>(8, "Улучшенный зад. бампер 7", 600000),
             new Tuple<int, string, int>(9, "Улучшенный зад. бампер 8", 600000),
             new Tuple<int, string, int>(10, "Улучшенный зад. бампер 9", 600000),
             new Tuple<int, string, int>(11, "Улучшенный зад. бампер 10", 600000),
             new Tuple<int, string, int>(12, "Улучшенный зад. бампер 11", 600000),
             new Tuple<int, string, int>(13, "Улучшенный зад. бампер 12", 600000),
             new Tuple<int, string, int>(14, "Улучшенный зад. бампер 13", 600000),
             new Tuple<int, string, int>(15, "Улучшенный зад. бампер 14", 600000),
             new Tuple<int, string, int>(16, "Улучшенный зад. бампер 15", 600000),
             new Tuple<int, string, int>(17, "Улучшенный зад. бампер 16", 600000)
      }},   
      {5,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартное крыло", 600000),
             new Tuple<int, string, int>(0, "Улучшенное крыло 0", 600000),
             new Tuple<int, string, int>(2, "Улучшенное крыло 1", 600000),
             new Tuple<int, string, int>(3, "Улучшенное крыло 2", 600000),
             new Tuple<int, string, int>(4, "Улучшенное крыло 3", 600000),
             new Tuple<int, string, int>(5, "Улучшенное крыло 4", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный капот 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный капот 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный капот 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный капот 5", 600000),
             new Tuple<int, string, int>(6, "Улучшенный капот 6", 600000),
             new Tuple<int, string, int>(7, "Улучшенный капот 7", 600000),
             new Tuple<int, string, int>(8, "Улучшенный капот 8", 600000),
             new Tuple<int, string, int>(9, "Улучшенный капот 9", 600000),
             new Tuple<int, string, int>(10, "Улучшенный капот 10", 600000),
             new Tuple<int, string, int>(12, "Улучшенный капот 11", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенные пороги 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенные пороги 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенные пороги 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенные пороги 5", 600000),
             new Tuple<int, string, int>(6, "Улучшенные пороги 6", 600000),
             new Tuple<int, string, int>(7, "Улучшенные пороги 7", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный глушитель 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный глушитель 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный глушитель 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный глушитель 5", 600000),
             new Tuple<int, string, int>(6, "Улучшенный глушитель 6", 600000),
             new Tuple<int, string, int>(7, "Улучшенный глушитель 7", 600000),
             new Tuple<int, string, int>(8, "Улучшенный глушитель 8", 600000),
             new Tuple<int, string, int>(9, "Улучшенный глушитель 9", 600000)
      }}   
}},
            {"g63pp", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"faction3", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"ayxz", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"600lt", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"demon", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }}   
}},
            {"rmodf12tdf", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"fc13", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }}   
}},
            {"gtc4", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная крыша 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенная крыша 2", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000)
      }}   
}},
            {"laferrri", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"pistaspider19", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"xc60", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rmodc63amg", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rrmansory", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"g20wide", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"ls430", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"rmodsianr", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"bolide", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"2018brabuss63", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
           {"rmodx6", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный спойлер 4", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000)
      }}   
}},
            {"rmodbugatti", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Faggio2", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Sanchez2", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Enduro", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"PCJ", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Hexer", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"lectro", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Nemesis", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Hakuchou", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Ruffian", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Bmx", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Scorcher", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"BF400", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"CarbonRS", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Bati", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Double", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Diablous", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Cliffhanger", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Akuma", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Thrust", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Nightblade", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Vindicator", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Ratbike", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Blazer", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Gargoyle", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"Sanctus", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"h2carb", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"r6", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"gsx1000", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"cbr1000rrr", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"goldwing", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"cb500x", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rmz2", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"shotaro", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"xxxxx", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"fleet78", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"fairlane66", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000)
      }}   
}},
            {"brabus800", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"bg700w", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"812nlargo", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"812mansory", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"alpinab7", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"lx570es", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"rs6m", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"scaldarsi", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"xc90", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"keyrus2", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"sclkuz", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"walds63", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"16charger", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"evo9", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный зад. бампер 2", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенные пороги 2", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
            {"rmodskyline34", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000)
      }}   
}},
            {"lb750sv", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000)
      }}   
}},
            {"vulcan", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"j50", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная крыша 1", 600000)
      }}   
}},
            {"ghost", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"mig", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"senna", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"17m760i", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"19dbs", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"ast", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
           {"vantage", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
            {"db11", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"aventadorishe", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"s63amg", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"sultan", new Dictionary<int, List<Tuple<int, string, int>>>() {
                  {3,new List<Tuple<int, string, int>>() {
                         new Tuple<int, string, int>(-1, "Стандартный спойлер", 100),
                         new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 100),
                         new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 100),
                         new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 100)
                  }},
                  {8,new List<Tuple<int, string, int>>() {
                         new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 100),
                         new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 100),
                         new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 100)
                  }},
                  {9,new List<Tuple<int, string, int>>() {
                         new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 100),
                         new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 100)
                  }},
                  {6,new List<Tuple<int, string, int>>() {
                         new Tuple<int, string, int>(-1, "Стандартная крыша", 100),
                         new Tuple<int, string, int>(0, "Улучшенная крыша 0", 100)
                  }},
                  {2,new List<Tuple<int, string, int>>() {
                         new Tuple<int, string, int>(-1, "Стандартный капот", 100),
                         new Tuple<int, string, int>(0, "Улучшенный капот 0", 100),
                         new Tuple<int, string, int>(1, "Улучшенный капот 1", 100),
                         new Tuple<int, string, int>(2, "Улучшенный капот 2", 100),
                         new Tuple<int, string, int>(3, "Улучшенный капот 3", 100)
                  }},
                  {1,new List<Tuple<int, string, int>>() {
                         new Tuple<int, string, int>(-1, "Стандартные пороги", 100),
                         new Tuple<int, string, int>(0, "Улучшенные пороги 0", 100)
                  }},
                  {0,new List<Tuple<int, string, int>>() {
                         new Tuple<int, string, int>(-1, "Стандартный глушитель", 100),
                         new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 100)
                  }}
            }},
            {"manspanam", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"toros", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"clssuniversal", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"monza", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"850", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный пер. бампер 4", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный зад. бампер 2", 600000)
      }},   
      {5,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартное крыло", 600000),
             new Tuple<int, string, int>(0, "Улучшенное крыло 0", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенные пороги 2", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный глушитель 2", 600000)
      }}   
}},
            {"m3e92", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"lp610", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rmodspeed", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"rmodzl1", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный капот 2", 600000)
      }}   
}},
            {"techart17", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"760m", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"s15mak", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"fpacehm", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"mb300sl", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(0, "Улучшенная решётка 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная решётка 1", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000)
      }}   
}},
			{"rmodgtr50", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"lfa10", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"c7r", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
			{"cats", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"focurs ", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"63lb", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"chall70", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(0, "Улучшенная решётка 0", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный капот 2", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
			{"ddgp20", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"ram1500", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"cls2015", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 0", 600000)
      }},   
      {5,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартное крыло", 600000),
             new Tuple<int, string, int>(0, "Улучшенное крыло 0", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
			{"a8audi", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный спойлер 4", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 3", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный зад. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный зад. бампер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный зад. бампер 4", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная крыша 1", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(0, "Улучшенная решётка 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная решётка 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенная решётка 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенная решётка 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенная решётка 4", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный капот 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный капот 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный капот 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный капот 5", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенные пороги 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенные пороги 3", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный глушитель 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный глушитель 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный глушитель 4", 600000)
      }}   
}},
			{"bmwe39", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный спойлер 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный спойлер 5", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(1, "Улучшенная решётка 0", 600000),
             new Tuple<int, string, int>(2, "Улучшенная решётка 1", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный глушитель 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный глушитель 3", 600000)
      }}   
}},
			{"brz13", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный спойлер 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный спойлер 5", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный пер. бампер 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный пер. бампер 5", 600000),
             new Tuple<int, string, int>(6, "Улучшенный пер. бампер 6", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный зад. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный зад. бампер 3", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный капот 2", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенные пороги 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенные пороги 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенные пороги 4", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный глушитель 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный глушитель 3", 600000)
      }}   
}},
			{"fd", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"fk8", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {4,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная решётка", 600000),
             new Tuple<int, string, int>(0, "Улучшенная решётка 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная решётка 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенная решётка 2", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный капот 2", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
			{"s30", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"subwrx", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000)
      }}   
}},
			{"z2879", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 3", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 4", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный глушитель 2", 600000)
      }}   
}},
            {"snowmobile", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
            {"e400", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"qx80", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000)
      }}   
}},
			{"gdaq50", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"mlnovitec", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"722s", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная крыша 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенная крыша 2", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000)
      }}   
}},
			{"dbx", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"bugatti", new Dictionary<int, List<Tuple<int, string, int>>>() {}},	
			{"vhrod", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"rc", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"z1000", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"amggtbs", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"hvrod", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"pts21", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"audiq7", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"rmodbacalar", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
			{"i8", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
      }},   
      {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000)
      }}   
		}},
		{"cam8tun", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный спойлер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный спойлер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный спойлер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный спойлер 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный спойлер 5", 600000)
      }},   
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенный пер. бампер 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенный пер. бампер 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенный пер. бампер 5", 600000),
             new Tuple<int, string, int>(6, "Улучшенный пер. бампер 6", 600000),
             new Tuple<int, string, int>(7, "Улучшенный пер. бампер 7", 600000),
             new Tuple<int, string, int>(8, "Улучшенный пер. бампер 8", 600000)
      }},   
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный зад. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный зад. бампер 2", 600000)
      }},   
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный капот 2", 600000)
      }},   
      {1,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартные пороги", 600000),
             new Tuple<int, string, int>(0, "Улучшенные пороги 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенные пороги 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенные пороги 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенные пороги 3", 600000)
      }},
        }},
        {"rmodlp570", new Dictionary<int, List<Tuple<int, string, int>>>() {
      {3,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный спойлер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный спойлер 0", 600000)
      }},
      {8,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный пер. бампер 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный пер. бампер 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенный пер. бампер 2", 600000)
      }},
      {9,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 600000),
             new Tuple<int, string, int>(0, "Улучшенный зад. бампер 0", 600000)
     }},
     {6,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартная крыша", 600000),
             new Tuple<int, string, int>(0, "Улучшенная крыша 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенная крыша 1", 600000),
             new Tuple<int, string, int>(2, "Улучшенная крыша 2", 600000),
             new Tuple<int, string, int>(3, "Улучшенная крыша 3", 600000),
             new Tuple<int, string, int>(4, "Улучшенная крыша 4", 600000),
             new Tuple<int, string, int>(5, "Улучшенная крыша 5", 600000),
             new Tuple<int, string, int>(6, "Улучшенная крыша 6", 600000),
             new Tuple<int, string, int>(7, "Улучшенная крыша 7", 600000),
             new Tuple<int, string, int>(8, "Улучшенная крыша 8", 600000),
             new Tuple<int, string, int>(9, "Улучшенная крыша 9", 600000)

      }},
      {2,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный капот", 600000),
             new Tuple<int, string, int>(0, "Улучшенный капот 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный капот 1", 600000)
      }},
      {0,new List<Tuple<int, string, int>>() {
             new Tuple<int, string, int>(-1, "Стандартный глушитель", 600000),
             new Tuple<int, string, int>(0, "Улучшенный глушитель 0", 600000),
             new Tuple<int, string, int>(1, "Улучшенный глушитель 1", 600000)
      }},
}},
            { "sultanrs", new Dictionary<int, List<Tuple<int, string, int>>>() {
                { 0, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный глушитель", 2000),
                    new Tuple<int, string, int>(0, "Титановый глушитель Tuner", 8000),
                    new Tuple<int, string, int>(1, "Титановый глушитель Tuner", 9000),
                    new Tuple<int, string, int>(2, "Раздвоенный глушитель", 15000),
                    new Tuple<int, string, int>(3, "Раздвоенный короткий глушитель", 14000),
                    new Tuple<int, string, int>(4, "Титановый короткий глушитель Tuner", 10000),
                }},
                { 1, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартные пороги", 3000),
                    new Tuple<int, string, int>(0, "Брызговики чёрного цвета", 9000),
                    new Tuple<int, string, int>(1, "Брызговики основного цвета", 15000),
                    new Tuple<int, string, int>(2, "Брызговики дополнительного цвета", 15000),
                    new Tuple<int, string, int>(3, "Заказные пороги", 12000),
                }},
                { 2, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный капот", 2000),
                    new Tuple<int, string, int>(0, "С двойным забором воздуха", 11000),
                    new Tuple<int, string, int>(1, "Карбоновый капот 1", 14000),
                    new Tuple<int, string, int>(2, "Карбоновый капот 2", 15000),
                    new Tuple<int, string, int>(3, "Карбоновый капот 3", 16000),
                    new Tuple<int, string, int>(4, "Карбоновый капот 4", 17000),
                    new Tuple<int, string, int>(5, "Изрисованный капот", 25000),
                }},
                { 3, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Нет", 1000),
                    new Tuple<int, string, int>(0, "Низкий спойлер 1", 6000),
                    new Tuple<int, string, int>(1, "Приподнятый спойлер 1", 8000),
                    new Tuple<int, string, int>(2, "GT Wing 1", 12000),
                    new Tuple<int, string, int>(3, "Низкий спойлер 2", 11000),
                    new Tuple<int, string, int>(4, "Низкий спойлер 3", 11000),
                    new Tuple<int, string, int>(5, "Низкий спойлер 4", 11000),
                    new Tuple<int, string, int>(6, "Низкий спойлер 5", 11000),
                    new Tuple<int, string, int>(7, "Низкий спойлер 6", 11000),
                    new Tuple<int, string, int>(8, "Приподнятый спойлер 2", 13000),
                    new Tuple<int, string, int>(9, "Приподнятый спойлер 3", 15000),
                    new Tuple<int, string, int>(10, "Карбоновый спойлер 1", 20000),
                    new Tuple<int, string, int>(11, "Карбоновый спойлер 2", 20000),
                    new Tuple<int, string, int>(12, "Карбоновый спойлер 3", 20000),
                    new Tuple<int, string, int>(13, "Массивный карбоновый спойлер", 21000),
                    new Tuple<int, string, int>(14, "Высокий спойлер", 25000),
                    new Tuple<int, string, int>(15, "Комбо-спойлер", 27000),
                }},
                { 4, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный радиатор", 1000),
                    new Tuple<int, string, int>(0, "Заказной радиатор", 10000),
                    new Tuple<int, string, int>(1, "Спортивный радиатор", 15000),
                }},
                { 5, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Нет", 1000),
                    new Tuple<int, string, int>(0, "Расширение основного цвета", 10000),
                    new Tuple<int, string, int>(1, "Расширение черного цвета", 15000),
                    new Tuple<int, string, int>(5, "Максимальное расширение", 20000),
                }},
                { 6, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартная крыша", 5000),
                    new Tuple<int, string, int>(0, "Спойлер на крыше", 15000),
                    new Tuple<int, string, int>(1, "Острая крыша", 10000),
                    new Tuple<int, string, int>(2, "Карбоновая крыша", 15000),
                    new Tuple<int, string, int>(3, "Спойлер с карбоновой крышей", 20000),
                    new Tuple<int, string, int>(4, "Острая карбоновая крыша", 13000),
                }},
                { 7, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Без раскраски", 5000),
                    new Tuple<int, string, int>(0, "Полоса по бокам", 18000),
                    new Tuple<int, string, int>(1, "Черная раскраска SULTAN RS", 20000),
                    new Tuple<int, string, int>(2, "Белая раскраска SULTAN RS", 20000),
                    new Tuple<int, string, int>(3, "Голубая полоса сбоку", 25000),
                    new Tuple<int, string, int>(4, "Раскраска KARIN", 26000),
                    new Tuple<int, string, int>(5, "Раскраска REDWOOD", 26000),
                    new Tuple<int, string, int>(6, "Раскраска KARIN 2", 26000),
                    new Tuple<int, string, int>(7, "Изрисованная раскраска", 40000),
                }},
                { 8, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 5000),
                    new Tuple<int, string, int>(0, "Передний бампер 1", 14000),
                    new Tuple<int, string, int>(1, "Передний бампер 2", 18000),
                    new Tuple<int, string, int>(2, "Передний бампер 3", 20000),
                    new Tuple<int, string, int>(3, "Передний бампер 4", 18000),
                    new Tuple<int, string, int>(4, "Передний бампер 5", 15000),
                    new Tuple<int, string, int>(5, "Передний бампер 6", 17000),
                    new Tuple<int, string, int>(6, "Передний бампер 7", 16000),
                    new Tuple<int, string, int>(7, "Передний бампер 8", 15000),
                    new Tuple<int, string, int>(8, "Передний бампер 9", 20000),
                    new Tuple<int, string, int>(9, "Передний бампер 10", 25000),
                    new Tuple<int, string, int>(10, "Передний бампер 11", 23000),
                    new Tuple<int, string, int>(11, "Передний бампер 12", 20000),
                    new Tuple<int, string, int>(12, "Передний бампер 13", 21000),
                    new Tuple<int, string, int>(13, "Передний бампер 14", 18000),
                    new Tuple<int, string, int>(14, "Передний бампер 15", 30000),
                }},
                { 9, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный зад. бампер", 5000),
                    new Tuple<int, string, int>(0, "Задний бампер 1", 18000),
                    new Tuple<int, string, int>(1, "Задний бампер 2", 20000),
                    new Tuple<int, string, int>(2, "Задний бампер 3", 22000),
                    new Tuple<int, string, int>(3, "Задний бампер 4", 19000),
                    new Tuple<int, string, int>(4, "Задний бампер 5", 21000),
                    new Tuple<int, string, int>(5, "Задний бампер 6", 25000),
                    new Tuple<int, string, int>(6, "Задний бампер 7", 23000),
                    new Tuple<int, string, int>(7, "Задний бампер 8", 20000),
                }},
            }},
            { "kuruma", new Dictionary<int, List<Tuple<int, string, int>>>() {
                { 0, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный глушитель", 5000),
                    new Tuple<int, string, int>(0, "Двойной глушитель", 10000),
                }},
                { 1, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартные пороги", 5000),
                    new Tuple<int, string, int>(0, "Заказные пороги осн.цвета", 11000),
                    new Tuple<int, string, int>(1, "Заказные пороги доп.цвета", 15000),
                    new Tuple<int, string, int>(2, "Заказные карбоновые пороги", 20000),
                }},
                { 3, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный спойлер", 5000),
                    new Tuple<int, string, int>(0, "Спойлер доп.цвета", 7000),
                    new Tuple<int, string, int>(1, "Низкий карбоновый спойлер", 11000),
                    new Tuple<int, string, int>(2, "Низкий спойлер осн.цвета", 13000),
                    new Tuple<int, string, int>(3, "Средний карбоновый спойлер", 15000),
                    new Tuple<int, string, int>(4, "GT Wing", 25000),
                }},
                { 8, new List<Tuple<int, string, int>>() {
                    new Tuple<int, string, int>(-1, "Стандартный пер. бампер", 5000),
                    new Tuple<int, string, int>(0, "Заказной бампер осн.цвета", 11000),
                    new Tuple<int, string, int>(1, "Заказной бампер доп.цвета", 15000),
                    new Tuple<int, string, int>(2, "Заказной карбоновый бампер", 15000),
                }},
            }},

      {"m6f13", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
	  {"c63w205", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
	  {"monster8", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
	  {"gtrsilhouette", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
	  {"jzx100", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
	  {"ie", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
      {"verus", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
      {"italirsx", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
      {"weevil", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
      {"hondansx", new Dictionary<int, List<Tuple<int, string, int>>>() {}},
      {"bmwm4", new Dictionary<int, List<Tuple<int, string, int>>>() {}},

        };

        public static Dictionary<int, int> ColorTypes = new Dictionary<int, int> { 
            { 1, 500 }, 
        };


        public static Dictionary<int, Dictionary<string, int>> TuningPrices = new Dictionary<int, Dictionary<string, int>>()
        {
            { 10, new Dictionary<string, int>() { // engine_menu
                {"-1", 50000 },
                {"0", 80000 },
                {"1", 100000 },
                {"2", 120000 },
                {"3", 150000 },
            }},
            { 11, new Dictionary<string, int>() { // turbo_menu
                {"-1", 50000 },
                {"0", 200000 },
            }},
            { 12, new Dictionary<string, int>() { // horn_menu
                { "-1", 5000 },
                { "0", 7000 },
                { "1", 8000 },
                { "2", 10000 },
                { "3", 10000 },
                { "4", 10000 },
                { "5", 10000 },
                { "6", 10000 },
                { "7", 10000 },
                { "8", 10000 },
                { "9", 10000 },
                { "10", 10000 },
                { "11", 10000 },
                { "12", 10000 },
                { "13", 10000 },
                { "14", 10000 },
                { "15", 10000 },
                { "16", 10000 },
                { "17", 10000 },
                { "18", 10000 },
                { "19", 10000 },
                { "20", 10000 },
                { "21", 10000 },
                { "22", 10000 },
                { "23", 10000 },
                { "24", 10000 },
                { "25", 10000 },
                { "26", 10000 },
                { "27", 10000 },
                { "28", 10000 },
                { "29", 10000 },
                { "30", 10000 },
                { "31", 10000 },
                { "32", 10000 },
                { "33", 10000 },
                { "34", 10000 },
            }},
            { 13, new Dictionary<string, int>() { // transmission_menu
                {"-1", 50000 },
                {"0", 60000 },
                {"1", 105000 },
                {"2", 120000 },
            }},
            { 14, new Dictionary<string, int>() { // glasses_menu
                {"0", 20000 },
                {"3", 30000 },
                {"2", 40000 },
                {"1", 50000 },
            }},
            { 15, new Dictionary<string, int>() { // suspention_menu
                {"-1", 30000 },
                {"0", 45000 },
                {"1", 50000 },
                {"2", 65000 },
                {"3", 80000 },
            }},
            { 16, new Dictionary<string, int>() { // brakes_menu
                {"-1", 50000 },
                {"0", 45000 },
                {"1", 70000 },
                {"2", 105000 },
            }},
            { 17, new Dictionary<string, int>() { // lights_menu
                {"-1", 5000 },
                {"0", 50000 },
                {"1", 50000 },
                {"2", 50000 },
                {"3", 50000 },
                {"4", 50000 },
                {"5", 50000 },
                {"6", 50000 },
                {"7", 50000 },
                {"8", 50000 },
                {"9", 50000 },
                {"10", 50000 },
                {"11", 50000 },
                {"12", 50000 },
            }},
            { 18, new Dictionary<string, int>() { // numbers_menu
                {"0", 20000 },
                {"1", 20000 },
                {"2", 20000 },
                {"3", 20000 },
                {"4", 20000 },
            }},
	     { 26, new Dictionary<string, int>() { // armor_menu
                {"-1", 120000 },
                {"0", 220000 },
                {"1", 320000 },
                {"2", 420000 },
                {"3", 520000 },
		  {"4", 620000 },
            }},
        };
        public static Dictionary<int, Dictionary<int, int>> TuningWheels = new Dictionary<int, Dictionary<int, int>>()
        {
            // спортивные
            { 0, new Dictionary<int, int>() {
				{ -1, 2500000 },
                { 50, 3000000 },
                { 51, 3000000 },
                { 52, 3000000 },
                { 53, 3000000 },
                { 54, 3000000 },
                { 55, 3000000 },
                { 56, 3000000 },
                { 57, 3000000 },
                { 58, 3000000 },
                { 59, 3000000 },
                { 60, 3000000 },
                { 61, 3000000 },
                { 62, 3000000 },
                { 63, 3000000 },
                { 64, 3000000 },
                { 65, 3000000 },
                { 66, 3000000 },
                { 67, 3000000 },
                { 68, 3000000 },
                { 69, 3000000 },
                { 70, 3000000 },
                { 71, 3000000 },
                { 72, 3000000 },
                { 73, 3000000 },
                { 74, 3000000 },
                { 75, 3000000 },
                { 76, 3000000 },
                { 77, 3000000 },
                { 78, 3000000 },
                { 79, 3000000 },
                { 80, 3000000 },
                { 81, 3000000 },
                { 82, 3000000 },
                { 83, 3000000 },
                { 84, 3000000 },
                { 85, 3000000 },
                { 86, 3000000 },
                { 87, 3000000 },
                { 88, 3000000 },
                { 89, 3000000 },
                { 90, 3000000 },
                { 91, 3000000 },
                { 92, 3000000 },
                { 93, 3000000 },
                { 94, 3000000 },
                { 95, 3000000 },
                { 96, 3000000 },
                { 97, 3000000 },
                { 98, 3000000 },
                { 99, 3000000 },
                { 100, 3000000 },
                { 101, 3000000 },
                { 102, 3000000 },
                { 103, 3000000 },
                { 104, 3000000 },
                { 105, 3000000 },
                { 106, 3000000 },
                { 107, 3000000 },
                { 108, 3000000 },
                { 109, 3000000 },
                { 110, 3000000 },
                { 111, 3000000 },
                { 112, 3000000 },
                { 113, 3000000 },
                { 114, 3000000 },
                { 115, 3000000 },
                { 116, 3000000 },
                { 117, 3000000 },
                { 118, 3000000 },
                { 119, 3000000 },
                { 120, 3000000 },
                { 121, 3000000 },
                { 122, 3000000 },
                { 123, 3000000 },
                { 124, 3000000 },
                { 125, 3000000 },
                { 126, 3000000 },
                { 127, 3000000 },
                { 128, 3000000 },
                { 129, 3000000 },
                { 130, 3000000 },
                { 131, 3000000 },
                { 132, 3000000 },
                { 133, 3000000 },
                { 134, 3000000 },
                { 135, 3000000 },
                { 136, 3000000 },
                { 137, 3000000 },
                { 138, 3000000 },
                { 139, 3000000 },
                { 140, 3000000 },
                { 141, 3000000 },
                { 142, 3000000 },
                { 143, 3000000 },
                { 144, 3000000 },
                { 145, 3000000 },
                { 146, 3000000 },
                { 147, 3000000 },
                { 148, 3000000 },
            }},
            // маслкары
            { 1, new Dictionary<int, int>() {
                { -1, 2500 },
				{ 0, 40000 },
				{ 1, 40000 },
				{ 2, 40000 },
				{ 3, 40000 },
				{ 4, 40000 },
				{ 5, 40000 },
				{ 6, 40000 },
				{ 7, 40000 },
				{ 8, 40000 },
				{ 9, 40000 },
				{ 10, 40000 },
				{ 11, 40000 },
				{ 12, 40000 },
				{ 13, 40000 },
				{ 14, 40000 },
				{ 15, 40000 },
				{ 16, 40000 },
				{ 17, 40000 },
            }},
            // лоурайдер
            { 2, new Dictionary<int, int>() {
                { -1, 70000 },
                { 0, 70000 },
                { 1, 70000 },
                { 2, 70000 },
                { 3, 70000 },
                { 4, 70000 },
                { 5, 70000 },
                { 6, 70000 },
                { 7, 70000 },
                { 8, 70000 },
                { 9, 70000 },
                { 10, 70000 },
                { 11, 70000 },
                { 12, 70000 },
                { 13, 70000 },
                { 14, 70000 },
            }},
            // вездеход
            { 3, new Dictionary<int, int>() {
                { -1, 60000 },
                { 0, 60000 },
                { 1, 60000 },
                { 2, 60000 },
                { 3, 60000 },
                { 4, 60000 },
                { 5, 60000 },
                { 6, 60000 },
                { 7, 60000 },
                { 8, 60000 },
                { 9, 60000 },
            }},
            // внедорожник
            { 4, new Dictionary<int, int>() {
                { -1, 50000 },
                { 0, 50000 },
                { 1, 50000 },
                { 2, 50000 },
                { 3, 50000 },
                { 4, 50000 },
                { 5, 50000 },
                { 6, 50000 },
                { 7, 50000 },
                { 8, 50000 },
                { 9, 50000 },
                { 10, 50000 },
                { 11, 50000 },
                { 12, 50000 },
                { 13, 50000 },
                { 14, 50000 },
                { 15, 50000 },
                { 16, 50000 },
            }},
            // тюннер
            { 5, new Dictionary<int, int>() {
                { -1, 60000 },
                { 0, 60000 },
                { 1, 60000 },
                { 2, 60000 },
                { 3, 60000 },
                { 4, 60000 },
                { 5, 60000 },
                { 6, 60000 },
                { 7, 60000 },
                { 8, 60000 },
                { 9, 60000 },
                { 10, 60000 },
                { 11, 60000 },
                { 12, 60000 },
                { 13, 60000 },
                { 14, 60000 },
                { 15, 60000 },
                { 16, 60000 },
                { 17, 60000 },
                { 18, 60000 },
                { 19, 60000 },
                { 20, 60000 },
                { 21, 60000 },
                { 22, 60000 },
                { 23, 60000 },
            }},
            // эксклюзивные
            { 7, new Dictionary<int, int>() {
                { -1, 100000 },
                { 0, 100000 },
                { 1, 100000 },
                { 2, 100000 },
                { 3, 100000 },
                { 4, 100000 },
                { 5, 100000 },
                { 6, 100000 },
                { 7, 100000 },
                { 8, 100000 },
                { 9, 100000 },
                { 10, 100000 },
                { 11, 100000 },
                { 12, 100000 },
                { 13, 100000 },
                { 14, 100000 },
                { 15, 100000 },
                { 16, 100000 },
                { 17, 100000 },
                { 18, 100000 },
                { 19, 100000 },
            }},
        };

    }
}
