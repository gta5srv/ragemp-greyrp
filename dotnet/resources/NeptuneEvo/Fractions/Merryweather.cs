using System;
using System.Collections.Generic;
using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using NeptuneEVO.GUI;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

namespace NeptuneEVO.Fractions
{
    class Merryweather : Script
    {
        private static nLog Log = new nLog("Merryweather");

        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        public static List<Vector3> Coords = new List<Vector3>
        {
            new Vector3(5012.2744, -5749.2544, -270.825144), // Колшэйп входа сделать 27.825144
            new Vector3(5012.365, -5747.028, -140.364433), // Колшэйп выхода сделать 14.364433
			new Vector3(183.58861, -3319.5757, -5.378965), // Колшэйп на остров  убрать минус
            new Vector3(5088.765, -5721.8823, -15.353768), // Колшэйп с острова убрать минус
            //new Vector3(5012.347, -5747.1514, 14.364428), // Колшэйп входа на другой этаж
            //new Vector3(5012.2603, -5749.2344, 27.824953), // Колшэйп изнутри этажа, чтобы вернуться назад
            new Vector3(5005.525, -5754.2207, -270.725288), // Колшэйп раздевалкиs сделать 27.725288
            new Vector3(4996.555, -5748.6943, -130.720463), // Колшейп крафта сделать 13.720463
			
			
        };

        public static bool warg_mode = false;

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStartHandler()
        {
            try
            {

                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(Coords[0], 1f, 2, 0));
                Cols[0].OnEntityEnterColShape += mws_OnEntityEnterColShape;
                Cols[0].OnEntityExitColShape += mws_OnEntityExitColShape;
                Cols[0].SetData("INTERACT", 82);

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(Coords[1], 1f, 2, 0));
                Cols[1].OnEntityEnterColShape += mws_OnEntityEnterColShape;
                Cols[1].OnEntityExitColShape += mws_OnEntityExitColShape;
                Cols[1].SetData("INTERACT", 83);

                Cols.Add(2, NAPI.ColShape.CreateCylinderColShape(Coords[2], 1f, 2, 0));
                Cols[2].OnEntityEnterColShape += mws_OnEntityEnterColShape;
                Cols[2].OnEntityExitColShape += mws_OnEntityExitColShape;
                Cols[2].SetData("INTERACT", 84);

                Cols.Add(3, NAPI.ColShape.CreateCylinderColShape(Coords[3], 1f, 2, 0));
                Cols[3].OnEntityEnterColShape += mws_OnEntityEnterColShape;
                Cols[3].OnEntityExitColShape += mws_OnEntityExitColShape;
                Cols[3].SetData("INTERACT", 85);

                Cols.Add(4, NAPI.ColShape.CreateCylinderColShape(Coords[4], 1f, 2, 0));
                Cols[4].SetData("INTERACT", 86);
                Cols[4].OnEntityEnterColShape += mws_OnEntityEnterColShape;
                Cols[4].OnEntityExitColShape += mws_OnEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Переодеться"), Coords[4] + new Vector3(0, 0, 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(5, NAPI.ColShape.CreateCylinderColShape(Coords[5], 1f, 2, 0));
                Cols[5].SetData("INTERACT", 87);
                Cols[5].OnEntityEnterColShape += mws_OnEntityEnterColShape;
                Cols[5].OnEntityExitColShape += mws_OnEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Склад оружия"), Coords[5] + new Vector3(0, 0, 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));
								
				

                NAPI.Marker.CreateMarker(1, Coords[0] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, Coords[1] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, Coords[2] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1.7f, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, Coords[3] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1.7f, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, Coords[4] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, Coords[5] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));


            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT\"FRACTIONS_MERRYWEATHER\":\n" + e.ToString(), nLog.Type.Error);
            }
        }



        private void mws_OnEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
            }
            catch (Exception e) { Log.Write("mws_OnEntityEnterColShape: " + e.Message, nLog.Type.Error); }
        }

        private void mws_OnEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception e) { Log.Write("mws_OnEntityExitColShape: " + e.Message, nLog.Type.Error); }
        }

        public static void interactPressed(Player player, int interact)
        {
            if (Main.Players[player].FractionID != 17)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Доступ запрещён", 3000);
                return;
            }
            if (interact != 84 && interact != 85 && player.IsInVehicle) return;
            if (player.HasData("FOLLOWING"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                return;
            }
            switch (interact)
            {
                case 82:
                    NAPI.Entity.SetEntityPosition(player, Coords[1] + new Vector3(0, 0, 1.12));
                    break;
                case 83:
                    NAPI.Entity.SetEntityPosition(player, Coords[0] + new Vector3(0, 0, 1.12));
                    break;
                case 84:
                    if (player.IsInVehicle)
                    {
                        Vehicle veh = player.Vehicle;
                        NAPI.Entity.SetEntityPosition(veh, Coords[3] + new Vector3(0, 0, 1.12));
                        player.SetIntoVehicle(veh, 0);
                    }
                    else
                        NAPI.Entity.SetEntityPosition(player, Coords[3] + new Vector3(0, 0, 1.12));
                   
                    
				
                    break;
                case 85:
                    if (player.IsInVehicle)
                    {
                        Vehicle veh = player.Vehicle;
                        NAPI.Entity.SetEntityPosition(veh, Coords[2] + new Vector3(0, 0, 1.12));
                        player.SetIntoVehicle(veh, 0);
                    }
                    else
                        NAPI.Entity.SetEntityPosition(player, Coords[2] + new Vector3(0, 0, 1.12));
                    break;
                case 86:
                    beginWorkDay(player);
                    break;
                case 87:
                    if (Main.Players[player].FractionID != 17)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Доступ запрещён", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[17].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    if (!Manager.canUseCommand(player, "openweaponstock")) return;
                    player.SetData("ONFRACSTOCK", 17);
                    GUI.Dashboard.OpenOut(player, Stocks.fracStocks[17].Weapons, "Склад оружия", 6);
                    return;
					
            }
        }

        public static void beginWorkDay(Player player)
        {
            if (Main.Players[player].FractionID != 17) return;
            Menu menu = new Menu("meryclothes", false, false);
            menu.Callback = callback_meryclothes;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Одежда";
            menu.Add(menuItem);

            menuItem = new Menu.Item("change", Menu.MenuItem.Button);
            menuItem.Text = "Переодеться";
            menu.Add(menuItem);

            menuItem = new Menu.Item("combat", Menu.MenuItem.Button);
            menuItem.Text = "Боевая форма";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }

        private static void callback_meryclothes(Player Player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "change":
                    if (Main.Players[Player].FractionLVL < 0)
                    {
                        Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете переодеться/раздеться", 3000);
                        return;
                    }
                    if (!Player.GetData<bool>("ON_DUTY"))
                    {
                        Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы переоделись в служебную форму", 3000);
                        Manager.setSkin(Player, 17, Main.Players[Player].FractionLVL);
                        Player.SetData("ON_DUTY", true);
                    }
                    else
                    {
                        Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы переоделись в повседневную одежду", 3000);
                        Customization.ApplyCharacter(Player);
                        if (Player.HasData("HAND_MONEY")) Player.SetClothes(5, 45, 0);
                        else if (Player.HasData("HEIST_DRILL")) Player.SetClothes(5, 41, 0);
                        Player.SetData("ON_DUTY", false);
                    }
                    return;
                case "combat":
                    MenuManager.Close(Player);
                    OpenMeryCombatMenu(Player);
                    return;
                case "close":
                    MenuManager.Close(Player);
                    return;
            }
        }

        public static void OpenMeryCombatMenu(Player player)
        {
            Menu menu = new Menu("merycombat", false, false);
            menu.Callback = callback_armycombat;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Боевая форма";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam1", Menu.MenuItem.Button);
            menuItem.Text = "Бело-серая форма";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam2", Menu.MenuItem.Button);
            menuItem.Text = "Черная форма";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam6", Menu.MenuItem.Button);
            menuItem.Text = "Зелёная форма";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam3", Menu.MenuItem.Button);
            menuItem.Text = "Синяя форма";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam4", Menu.MenuItem.Button);
            menuItem.Text = "Синяя форма (Горка)";
            menu.Add(menuItem);

            menuItem = new Menu.Item("takeoff", Menu.MenuItem.Button);
            menuItem.Text = "Снять форму";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button);
            menuItem.Text = "Назад";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_armycombat(Player Player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (item.ID == "back")
            {
                MenuManager.Close(Player);
                beginWorkDay(Player);
                return;
            }
            if (Main.Players[Player].FractionID != 17)
            {
                Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Доступ запрещён", 3000);
                return;
            }
            if (!Player.GetData<bool>("ON_DUTY"))
            {
                Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                return;
            }
            Player.SetData("IN_CP_MODE", true);
            var gender = Main.Players[Player].Gender;
            Customization.ClearClothes(Player, gender);
            switch (item.ID)
            {
                case "cam1":
                    if (gender)
                    {
                        Player.SetClothes(2, 0, 0);
                        Customization.SetHat(Player, 106, 22);
                        Player.SetClothes(11, 220, 22);
                        Player.SetClothes(4, 31, 2);
                        Player.SetClothes(3, 179, 3);
                        Player.SetClothes(9, 16, 0);
                        Player.SetClothes(6, 24, 0);
                    }
                    else
                    {
                        Player.SetClothes(2, 0, 0);
                        Customization.SetHat(Player, 105, 22);
                        Player.SetClothes(11, 230, 22);
                        Player.SetClothes(4, 30, 2);
                        Player.SetClothes(3, 179, 3);
                        Player.SetClothes(9, 18, 0);
                        Player.SetClothes(6, 24, 0);
                    }
                        return;
                case "cam2":
                    if (gender)
                    {
                        Player.SetClothes(2, 0, 0);
                        Customization.SetHat(Player, 106, 20);
                        Player.SetClothes(11, 220, 20);
                        Player.SetClothes(3, 179, 0);
                        Player.SetClothes(4, 130, 1);
                        Player.SetClothes(9, 16, 2);
                        Player.SetClothes(6, 24, 0);
                    }
                    else
                    {
                        Player.SetClothes(2, 0, 0);
                        Customization.SetHat(Player, 105, 20);
                        Player.SetClothes(11, 230, 20);
                        Player.SetClothes(3, 213, 0);
                        Player.SetClothes(4, 136, 1);
                        Player.SetClothes(9, 18, 2);
                        Player.SetClothes(6, 24, 0);
                    }
                    return;
                case "cam6":
                    if (gender)
                    {
                        Player.SetClothes(2, 0, 0);
                        Customization.SetHat(Player, 106, 15);
                        Player.SetClothes(11, 220, 15);
                        Player.SetClothes(3, 154, 15);
                        Player.SetClothes(4, 87, 17);
                        Player.SetClothes(9, 16, 2);
                        Player.SetClothes(6, 24, 0);

                    }
                    else
                    {
                        Player.SetClothes(2, 0, 0);
                        Customization.SetHat(Player, 105, 15);
                        Player.SetClothes(11, 230, 15);
                        Player.SetClothes(3, 154, 15);
                        Player.SetClothes(4, 90, 17);
                        Player.SetClothes(9, 18, 2);
                        Player.SetClothes(6, 24, 0);
                    }
                    return;
                case "cam3":
                    if (gender)
                    {
                        Player.SetClothes(2, 0, 0);
                        Customization.SetHat(Player, 106, 10);
                        Player.SetClothes(11, 220, 10);
                        Player.SetClothes(4, 87, 10);
                        Player.SetClothes(3, 139, 10);
                        Player.SetClothes(9, 16, 2);
                        Player.SetClothes(6, 24, 0);
                    }
                    else
                    {
                        Player.SetClothes(2, 0, 0);
                        Customization.SetHat(Player, 105, 10);
                        Player.SetClothes(11, 230, 10);
                        Player.SetClothes(4, 90, 10);
                        Player.SetClothes(3, 174, 10);
                        Player.SetClothes(9, 18, 2);
                        Player.SetClothes(6, 24, 0);
                    }
                    return; 
                case "cam4":
                    if (gender)
                    {
                        Player.SetClothes(11, 251, 8);
                        Player.SetClothes(4, 97, 8);
                        Player.SetClothes(3, 139, 10);
                        Player.SetClothes(9, 16, 2);
                        Player.SetClothes(6, 70, 8);
                    }
                    else
                    {
                        Player.SetClothes(11, 261, 8);
                        Player.SetClothes(4, 100, 8);
                        Player.SetClothes(3, 174, 10);
                        Player.SetClothes(9, 18, 2);
                        Player.SetClothes(6, 73, 8);
                    }
                    return;
                case "takeoff":
                    Manager.setSkin(Player, Main.Players[Player].FractionID, Main.Players[Player].FractionLVL);
                    Player.SetData("IN_CP_MODE", false);
                    return;
            }
        }

        #region menu
       
        #endregion
    }
}