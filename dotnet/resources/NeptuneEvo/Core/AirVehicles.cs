using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NeptuneEVO.GUI;
using NeptuneEVO.MoneySystem;
using NeptuneEVO.SDK;
using System.Threading;
using NeptuneEVO.Houses;
using static NeptuneEVO.Core.VehicleManager;
using System.Security.Cryptography;
using static NeptuneEVO.Houses.GarageManager;
using NeptuneEVO.Businesses;

namespace NeptuneEVO.Core
{
    public class AirVehicle
    {

        public string Model = "";
        public string Holder = "";
        public string Number = "";
        public Color Color = new Color();

        public AirVehicle(string model, string holder, string number, Color color)
        {
            Model = model; Holder = holder; Color = color; Number = number;
        }

    }

    class AirVehicles : Script
    {
        private static nLog Log = new nLog("AirVehicles");

        public static Dictionary<string, AirVehicle> Airs = new Dictionary<string, AirVehicle>();


        [Command("rappel")]
        public static void CMD_rappel(Player player)
        { try { } catch { } }


        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var result = MySQL.QueryRead("SELECT * FROM airvehicles");

                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB rod return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    string number = Row["number"].ToString();
                    Airs.Add(number, new AirVehicle(Row["veh"].ToString(), Row["holder"].ToString(), number, JsonConvert.DeserializeObject<Color>(Row["color"].ToString()) ));
                }


                Vector3 pos = new Vector3(-1133.2745, -2860.2454, 12.826176);
                ColShape shape = NAPI.ColShape.CreateCylinderColShape(pos, 2f, 3, 0);
                shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("AIR", true);
                        entity.SetData("INTERACTIONCHECK", 521);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        entity.ResetData("AIR");
                        entity.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                NAPI.TextLabel.CreateTextLabel("~b~Вызов транспорта", new Vector3(pos.X, pos.Y, pos.Z + 1f), 5F, 0.5F, 0, new Color(255, 255, 255), true, 0);
                NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 0.5f), new Vector3(), new Vector3(), 0.965f, new Color(0, 175, 250, 220), false, 0);
                NAPI.Marker.CreateMarker(27, pos + new Vector3(0,0, 0.14f), new Vector3(), new Vector3(), 1f, new Color(0, 175, 250, 220), false, 0);

            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"AIRVEHICLES\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static Dictionary<string, AirVehicle> getAllAirVehicles(string holder)
        {
            try
            {
                Dictionary<string, AirVehicle> airs = new Dictionary<string, AirVehicle>();

                foreach(AirVehicle air in Airs.Values)
                {
                    if (air.Holder == holder)
                    {
                        airs.Add(air.Number, air);
                    }
                }

                return airs;
            }
            catch (Exception e) { Log.Write("getAllAirVehicles: " + e.ToString(), nLog.Type.Error); return null; }
        }

        public static void Create(string holder, string model, Color color )
        {
            try
            {
                string number = GenerateNumber();
                AirVehicle airveh = new AirVehicle(model, holder, number, color);
                Airs.Add(number, airveh);

                MySQL.Query($"INSERT INTO airvehicles (number, holder, veh, color) " +
                        $"VALUES ('{number}','{holder}','{model}','{JsonConvert.SerializeObject(color)}')");
            }
            catch (Exception e) { Log.Write("CREATE: " + e.ToString(), nLog.Type.Error); }
        }

        private static List<string> whitelist = new List<string>
        {
          "A","B","E","K","M","H","O","P","C","T","Y","X"
        };

        public static string GenerateNumber()
        {
            try
            {
                Random rnd = new Random();
                string number = "";
                do
                {
                    number = "";

                    number += whitelist[rnd.Next(0, whitelist.Count)];
                    number += rnd.Next(0, 9);
                    number += rnd.Next(0, 9);
                    number += whitelist[rnd.Next(0, whitelist.Count)];
                    number += whitelist[rnd.Next(0, whitelist.Count)];
                } while (Vehicles.ContainsKey(number) && Airs.ContainsKey(number));
                return number;
            }
            catch (Exception e) { Log.Write("NUMBER: " + e.ToString(), nLog.Type.Error); return null; }
        }

        public static void Remove(string number)
        {
            try
            {
                Airs.Remove(number);
                MySQL.Query($"DELETE FROM `airvehicles` WHERE number='{number}'");
            }
            catch (Exception e) { Log.Write("REMOVE: " + e.ToString(), nLog.Type.Error); }
        }

        public static void OpenMenu(Player player)
        {
            Menu menu = new Menu("parkcars", false, false);
            menu.Callback = callback_airs;
            menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Аэропорт";
            menu.Add(menuItem);

            foreach (var v in getAllAirVehicles(player.Name).Values)
            {
                menuItem = new Menu.Item(v.Number, Menu.MenuItem.Button);
                menuItem.Text = $"{ParkManager.GetNormalName(v.Model)} <br> Номер: {v.Number} <br>";
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
        }

        private static void callback_airs(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    MenuManager.Close(player);
                    if (item.ID == "back")
                    {
                        MenuManager.Close(player);
                        Main.OpenPlayerMenu(player).Wait();
                        return;
                    }
                    OpenSelectedAirMenu(player, item.ID);
                }
                catch (Exception e) { Log.Write("callback_airs: " + e.Message + e.Message, nLog.Type.Error); }
            });
        }

        public static void OpenSelectedAirMenu(Player player, string number)
        {
            Menu menu = new Menu("selectedcar", false, false);
            menu.Callback = callback_selectedair;
            menu.SetBackGround("../images/phone/pages/gps.png");

            var vData = Airs[number];

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = number;
            menu.Add(menuItem);

            menuItem = new Menu.Item("model", Menu.MenuItem.Card);
            menuItem.Text = ParkManager.GetNormalName(vData.Model);
            menu.Add(menuItem);

            menuItem = new Menu.Item("key", Menu.MenuItem.Button);
            menuItem.Text = $"Дубликат ключей";
            menu.Add(menuItem);

            menuItem = new Menu.Item("evac", Menu.MenuItem.Button);
            menuItem.Text = $"Эвакуировать транспорт";
            menu.Add(menuItem);

            var price = BCore.GetVipCost(player, BCore.CostForCar(vData.Model));

            menuItem = new Menu.Item("sell", Menu.MenuItem.Button);
            menuItem.Text = $"Продать ({price}$)";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
        }

        private static void callback_selectedair(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            switch (item.ID)
            {
                case "sell":
                    player.SetData("CARSELLGOV", menu.Items[0].Text);
                    AirVehicle vData = Airs[menu.Items[0].Text];
                    var price = BCore.GetVipCost(player, BCore.CostForCar(vData.Model));

                    MenuManager.Close(player);
                    Trigger.PlayerEvent(player, "openDialog", "AIR_SELL_TOGOV", $"Вы действительно хотите продать государству {ParkManager.GetNormalName(vData.Model)} ({menu.Items[0].Text}) за ${price}?");
                    return;
                case "evac":
                    if (!Main.Players.ContainsKey(player)) return;

                    var number = menu.Items[0].Text;

                    if (Main.Players[player].Money < 3000)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств (не хватает {3000 - Main.Players[player].Money}$)", 3000);
                        return;
                    }

                    foreach (var v in NAPI.Pools.GetAllVehicles())
                    {
                        if (v.HasData("ACCESS") && (v.GetData<string>("ACCESS") == "AIR") && NAPI.Vehicle.GetVehicleNumberPlate(v) == number)
                        {
                            var veh = v;
                            if (veh == null) return;

                            NAPI.Entity.DeleteEntity(veh);

                            MoneySystem.Wallet.Change(player, -3000);

                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваш воздушный транспорт был отогнан в аэропорт", 3000);
                            player.ResetData("SPAWNAIR");
                            break;
                        }
                    }
                    return;
                case "key":
                    if (!Main.Players.ContainsKey(player)) return;

                    var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.CarKey));
                    if (tryAdd == -1 || tryAdd > 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                        return;
                    }

                    nInventory.Add(player, new nItem(ItemType.CarKey, 1, $"{menu.Items[0].Text}_{menu.Items[0].Text}"));
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили ключ от воздушного транспорта с номером {menu.Items[0].Text}", 3000);
                    return;
                case "close":
                    OpenMenu(player);
                    return;
            }
        }

        public static string GetNumberByKey(Player player, int key)
        {
            try
            {
                int i = 0;
                foreach (string number in getAllAirVehicles(player.Name).Keys)
                    if (i == key)
                        return number;
                    else
                        i++;
                return "";
            }
            catch { return ""; }
        }

        public static void SpawnCar(Player player, int key)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (getAllAirVehicles(player.Name).Count < 1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет воздушного транспорта!", 3000);
                    return;
                }

                if (player.HasData("SPAWNAIR"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас уже вызван воздушный транспорт!", 3000);
                    return;
                }

                

                string number = GetNumberByKey(player, key);



                Vehicle veh = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(Airs[number].Model), new Vector3(-1146.0817, -2863.8562, 13.826023), 150, 0, 0, number);
                NAPI.Vehicle.SetVehicleCustomPrimaryColor(veh, Airs[number].Color.Red, Airs[number].Color.Green, Airs[number].Color.Blue);
                NAPI.Vehicle.SetVehicleCustomSecondaryColor(veh, Airs[number].Color.Red, Airs[number].Color.Green, Airs[number].Color.Blue);
                veh.SetData("ACCESS", "AIR");
                veh.SetData("OWNER", player);

                VehicleStreaming.SetEngineState(veh, false);
                VehicleStreaming.SetLockStatus(veh, false);

                player.SetData("SPAWNAIR", veh);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вас ожидает {ParkManager.GetNormalName(Airs[number].Model)}", 3000);

            }
            catch (Exception e) { Log.Write("SPAWNCAR: " + e.ToString(), nLog.Type.Error); }
        }


        public static void Event_OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {

                try
                {
                    if (player.HasData("SPAWNAIR"))
                    {
                        Vehicle veh = player.GetData<Vehicle>("SPAWNAIR");
                        NAPI.Task.Run(() => {
                            try
                            {
                                veh.Delete();
                            }
                            catch { }
                        });
                    }
                }
                catch (Exception e) { Log.Write("DISCONNECT: " + e.ToString(), nLog.Type.Error); }

        }

    }
}
