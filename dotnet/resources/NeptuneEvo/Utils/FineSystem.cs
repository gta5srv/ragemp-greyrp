using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.GUI;
using NeptuneEVO.MoneySystem;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Core
{
    class FineManager : Script
    {

        private static nLog Log = new nLog("FineManager");
        private static int LastID = 0;
        private static List<Fine> fines = new List<Fine> { };

        [ServerEvent(Event.ResourceStart)]

        public void onResourceStart()
        {
            try
            {
                var result = MySQL.QueryRead("SELECT * FROM fines");
                foreach(DataRow Row in result.Rows)
                {
                    fines.Add(new Fine(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["price"]), Row["info"].ToString(), Row["holder"].ToString(), Row["number"].ToString()));
                    LastID = Convert.ToInt32(Row["id"]);
                }
                new CameraSpeed(350, 170, new Vector3(-1126.418, -930.9616, 1.570667), 15f);
				new CameraSpeed(350, 170, new Vector3(25.56756, -788.5834, 30.4721), 15f);

				new CameraSpeed(350, 170, new Vector3(-402.6801, -242.0038, 35.1313), 15f);    

				new CameraSpeed(350, 170, new Vector3(-562.8818, -298.0926, 34.0624), 15f);
				new CameraSpeed(350, 170, new Vector3(-780.8859, -260.1862, 35.98249), 15f);

				new CameraSpeed(350, 170, new Vector3(-1688.191, -726.7332, 9.457568), 15f);
				new CameraSpeed(350, 170, new Vector3(-1073.215, -1294.037, 4.736717), 15f);

				new CameraSpeed(350, 170, new Vector3(-483.5497, -838.3188, 29.34708), 15f);

				new CameraSpeed(350, 170, new Vector3(483.3389, -954.8856, 26.31596), 15f);
				new CameraSpeed(350, 170, new Vector3(333.2746, -659.9154, 28.17453), 15f);

				new CameraSpeed(350, 170, new Vector3(-215.1885, 259.7779, 90.96346), 15f);

				new CameraSpeed(350, 170, new Vector3(-922.7404, 267.3181, 69.14259), 15f);
				new CameraSpeed(350, 170, new Vector3(-1089.51, 269.7665, 62.76591), 15f);
				new CameraSpeed(350, 170, new Vector3(-1404.556, 215.0268, 57.18405), 15f);

				new CameraSpeed(350, 170, new Vector3(-1350.244, 2368.809, 33.64351), 15f);
				new CameraSpeed(350, 170, new Vector3(-855.8818, 2751.583, 22.0183), 15f);

				new CameraSpeed(350, 170, new Vector3(284.7818, 2636.61, 43.51397), 15f);

            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"RODINGS\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public class Fine
        {
            public int ID { get; set; }

            public int Price { get; set; }

            public string Why { get; set; }

            public string Holder { get; set; }

            public string Number { get; set; }

            public Fine(int id, int price, string why, string holder, string number)
            {
                ID = id; Price = price; Why = why; Holder = holder; Number = number;
            }
        }


        public class CameraSpeed
        { 
            public int Cost { get; set; }
            public int Speed { get; set; }
            public Vector3 Position { get; set; }
            public float Radius { get; set; }
            public CameraSpeed(int cost, int speed, Vector3 pos, float radius)
            {
                Cost = cost; Speed = speed; Position = pos; Radius = radius;
                NAPI.Blip.CreateBlip(604, pos, 0.5f, 4, "Камера", 255, 0, true);

                ColShape shape = NAPI.ColShape.CreateCylinderColShape(pos, radius, 3, 0);

                shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        if (entity.IsInVehicle && entity.VehicleSeat == 0 && VehicleManager.Vehicles.ContainsKey(entity.Vehicle.NumberPlate) && VehicleManager.Vehicles[entity.Vehicle.NumberPlate].Holder == entity.Name)
                        {
                            Vector3 velocity = NAPI.Entity.GetEntityVelocity(entity.Vehicle.Handle);
                            double speeds = Math.Sqrt((velocity.X * velocity.X) + (velocity.Y * velocity.Y) + (velocity.Z * velocity.Z)) * 3.6;
                            if (GetHaveFine(entity.Vehicle.NumberPlate, entity.Name.ToString()) == 10) return;
                            if (speeds > Speed + 20)
                            {
                                LastID++;
                                Notify.Send(entity, NotifyType.Alert, NotifyPosition.BottomCenter, $"Вы получили штраф в размере {Cost}$!", 3000);
                                MySQL.Query($"INSERT INTO fines (info, price, number, holder, id) VALUES ('Превышение скорости','{Cost}','{entity.Vehicle.NumberPlate}','{entity.Name}','{LastID}')");
                                AddFine(LastID, Cost, "Превышение скорости",  entity.Name, entity.Vehicle.NumberPlate);
                            }
                        }
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };

            }
        }

        public static void AddFine(int id, int price, string why, string holder, string number)
        {
            fines.Add(new Fine(id, price, why, holder, number));
        }

        public static Fine GetFineByID(int index)
        {
            try { 
            foreach (Fine myfine in fines)
                if (myfine.ID == index)
                    return myfine;
            return null;
            }
            catch (Exception e) { Log.Write("SET: " + e.ToString(), nLog.Type.Error); return null; }
        }

        public static void RemoveFineByID(int index)
        {
            try {
                Fine fine = null;
                foreach (Fine myfine in fines)
                    if (myfine.ID == index)
                        fine = myfine;

                fines.Remove(fine);

            }
            catch (Exception e) { Log.Write("SET: " + e.ToString(), nLog.Type.Error); }
        }

        public static List<Fine> GetAllFines(Player player)
        {
            try{ 
                List<Fine> yeafine = new List<Fine> { };
                foreach (Fine myfine in fines)
                    if (myfine.Holder == player.Name)
                        yeafine.Add(myfine);
                return yeafine;
            }
            catch (Exception e) { Log.Write("SET: " + e.ToString(), nLog.Type.Error); return null; }
        }
		public static bool HaveFine(string number,string name)
        {
            try { 
            bool have = false;
            foreach (Fine fin in fines)
                if (fin.Number == number && fin.Holder == name)
                {
                    have = true;
                    break;
                }
            return have;
            }
            catch (Exception e) { Log.Write("SET: " + e.ToString(), nLog.Type.Error); return false; }
        }

        public static int GetHaveFine(string number, string holder)
        {
            try
            {
                int i = 0;
                foreach (Fine fin in fines)
                    if (fin.Number == number && fin.Holder == holder)
                        i++;
                return i;
            }
            catch (Exception e) { Log.Write("GET: " + e.ToString(), nLog.Type.Error); return 0; }
        }


        public static void OpenFineMenu(Player player)
        {
            try { 
            Menu menu = new Menu("finesmenu", false, false);
            menu.Callback = callback_mainmenu;
            menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Штрафы";
            menu.Add(menuItem);


            List<Fine> myfines = GetAllFines(player);

            menuItem = new Menu.Item("strafov", Menu.MenuItem.Card);
            menuItem.Column = 2;
            menuItem.Text = $"Штрафов: {myfines.Count}";
            menu.Add(menuItem);

            if (myfines.Count > 0)
            {
                int result = 0;
                foreach (Fine nofine in myfines)
                    result += nofine.Price;

                menuItem = new Menu.Item("summa", Menu.MenuItem.Card);
                menuItem.Column = 2;
                menuItem.Text = $"Итог: {result}$";
                menu.Add(menuItem);

                menuItem = new Menu.Item("oplatit", Menu.MenuItem.Button);
                menuItem.Text = "Оплатить";
                menu.Add(menuItem);

                menuItem = new Menu.Item("oplatamani", Menu.MenuItem.Button);
                menuItem.Text = "Оплатить все";
                menu.Add(menuItem);
            }

            menuItem = new Menu.Item("close", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
            }
            catch (Exception e) { Log.Write("SET: " + e.ToString(), nLog.Type.Error); }
        }

        private static void callback_mainmenu(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try { 
            MenuManager.Close(player);
            switch (item.ID)
            {
                case "oplatit":
                    OpenFinesMenu(player);
                    return;
                case "oplatamani":

                    List<Fine> myfines = GetAllFines(player);

                    int price = 0;
					int i = 0;

                    foreach (Fine nofine in myfines)
					{
						i += 1;
                        price += nofine.Price;
					}

                    if (MoneySystem.Bank.Accounts[Main.Players[player].Bank].Balance < price)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас недостаточно средств!", 3000);
                        return;
                    }
					
					 Utils.QuestsManager.AddQuestProcess(player, 9, i);

                    foreach (Fine nofine in myfines)
                    {
                        fines.Remove(nofine);
                        MySQL.Query($"DELETE FROM fines WHERE id='{nofine.ID}'");
                    }
                    MoneySystem.Bank.Change(Main.Players[player].Bank, -price, false);
                    
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы оплатили штрафы на сумму {price}$!", 3000);
                    return;
                case "close":
                        _ = Main.OpenPlayerMenu(player);
                    return;
            }
            }
            catch (Exception e) { Log.Write("SET: " + e.ToString(), nLog.Type.Error); }
        }

        public static void OpenFinesMenu(Player player)
        {
            try { 
            Menu menu = new Menu("finesmenu", false, false);
            menu.Callback = callback_fines;
            menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Оплата штрафов";
            menu.Add(menuItem);

            List<Fine> myfines = GetAllFines(player);

            foreach(Fine nofine in myfines)
                {
                    menuItem = new Menu.Item(nofine.ID.ToString(), Menu.MenuItem.Button);
                    menuItem.Text = $"{nofine.Number} - {nofine.Price}$";
                    menu.Add(menuItem);
                }

            menuItem = new Menu.Item("close", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
            }
            catch (Exception e) { Log.Write("SET: " + e.ToString(), nLog.Type.Error); }
        }

        private static void callback_fines(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try { 
                MenuManager.Close(player);
                switch (item.ID)
                {
                    case "close":
                        OpenFineMenu(player);
                        return;
                    default:
                        OpenCurMenu(player, Convert.ToInt32(item.ID));
                        return;
                }
            }
            catch (Exception e) { Log.Write("SET: " + e.ToString(), nLog.Type.Error); }
        }

        public static void OpenCurMenu(Player player, int index)
        {
            try { 
            Menu menu = new Menu("finesmenu", false, false);
            menu.Callback = callback_cusmenu;
            menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Информация";
            menu.Add(menuItem);

            Fine myfine = GetFineByID(index);

                    menuItem = new Menu.Item("info", Menu.MenuItem.Card);
                    menuItem.Text = "Причина: " + myfine.Why;
                    menu.Add(menuItem);

                        menuItem = new Menu.Item("info", Menu.MenuItem.Card);
                        menuItem.Text = "Сумма $ : " + myfine.Price;
                        menu.Add(menuItem);

            menuItem = new Menu.Item(index.ToString(), Menu.MenuItem.Button);
            menuItem.Text = "Оплатить";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
            }
            catch (Exception e) { Log.Write("SET: " + e.ToString(), nLog.Type.Error); }
        }

        private static void callback_cusmenu(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try { 
                MenuManager.Close(player);
                switch (item.ID)
                {
                    case "close":
                        OpenFinesMenu(player);
                        return;
                    default:
                        int index = Convert.ToInt32(item.ID);
                        Fine myfine = GetFineByID(index);
                        
                        if (MoneySystem.Bank.Accounts[Main.Players[player].Bank].Balance < myfine.Price)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас недостаточно средств!", 3000);
                            return;
                        }
                        MoneySystem.Bank.Change(Main.Players[player].Bank, -myfine.Price, false);
                        MySQL.Query($"DELETE FROM fines WHERE id='{myfine.ID}'");
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы оплатили штраф {myfine.Price}$!", 3000);
					    Utils.QuestsManager.AddQuestProcess(player, 9);
                        RemoveFineByID(index);

                        OpenFinesMenu(player);
                        return;
                }
            }
            catch (Exception e) { Log.Write("SET: " + e.ToString(), nLog.Type.Error); }
        }

    }
}
