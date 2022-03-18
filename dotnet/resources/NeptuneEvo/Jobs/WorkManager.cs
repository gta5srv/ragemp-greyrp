using GTANetworkAPI;
using System;
using System.Linq;
using System.Collections.Generic;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using NeptuneEVO.GUI;
using Newtonsoft.Json;

namespace NeptuneEVO.Jobs
{
    class WorkManager : Script
    {
        private static nLog Log = new nLog("WorkManager");
        public static Random rnd = new Random();

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                //Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(Points[0], 1, 2, 0)); // job placement
                //Cols[0].OnEntityEnterColShape += JobMenu_onEntityEnterColShape; // job placement point handler
                //NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Центр занятости"), new Vector3(Points[0].X, Points[0].Y, Points[0].Z + 0.5), 10F, 0.3F, 0, new Color(255, 255, 255));
                //NAPI.Marker.CreateMarker(1, Points[0] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
				
				//new WorkPoint(3, new Vector3(-535.9867, -211.6378, 36.52979), 90, "Jonh");
				
				new WorkPoint(1, new Vector3(739.3245, 137.4899, 79.61736), -1, "Начальник");
				new WorkPoint(2, new Vector3(109.3144, -1572.031, 28.48263), -1, "Начальник");
				new WorkPoint(3, new Vector3(893.3738, -183.3333, 73.58035), -1, "Начальник");
				new WorkPoint(4, new Vector3(449.0078, -658.6881, 27.34096), -1, "Начальник");
				//new WorkPoint(5, new Vector3(-1322.242, 60.02873, 52.48398), -1, "Начальник");
				//new WorkPoint(6, new Vector3(592.5595, -3046.608, 5.049734), -1, "Начальник"); //дальнобойщик
				new WorkPoint(7, new Vector3(904.1635, -1263.684, 24.6826), -1, "Начальник");
				new WorkPoint(7, new Vector3(-1475.528, -515.9003, 33.61669), -1, "Начальник");
				new WorkPoint(7, new Vector3(-139.6105, 6365.558, 30.36527), -1, "Начальник");
				//new WorkPoint(8, new Vector3(2932.042, 4624.178, 47.60336), -1, "Начальник");
                new WorkPoint(9, new Vector3(1090.414, -2234.13, 30.18403), -1, "Начальник");
                new WorkPoint(10, new Vector3(2030.0756, 4980.156, 41), -1, "Сбор листьев коки", true);
                new WorkPoint(11, new Vector3(144.8581, -373.5612, 42.64737), -1, "Начальник", true);
                new WorkPoint(12, new Vector3(2946.686, 2746.836, 42.50), -1, "Начальник", true);
                new WorkPoint(13, new Vector3(1695.806, 43.05446, 160.7861), -1, "Начальник", true);
                new WorkPoint(14, new Vector3(915.187, -1515.19, 30.01137), -1, "Начальник", true);

                // blips
                NAPI.Blip.CreateBlip(354, new Vector3(724.9625, 133.9959, 79.83643), 1, 46, Main.StringToU16("Электростанция"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(408, new Vector3(105.4633, -1568.843, 28.60269), 1, 3, Main.StringToU16("Почтальон"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(198, new Vector3(903.3215, -191.7, 73.40494), 1, 46, Main.StringToU16("Такси"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(513, new Vector3(462.6476, -605.5295, 27.49518), 1, 30, Main.StringToU16("Водитель автобуса"), 255, 0, true, 0, 0);
                //NAPI.Blip.CreateBlip(512, new Vector3(-1331.475, 53.58579, 53.53268), 0.7f, 2, Main.StringToU16("Газонокосильщик"), 255, 0, true, 0, 0);
                //NAPI.Blip.CreateBlip(477, new Vector3(588.2037, -3037.641, 6.303829), 0.7f, 4, Main.StringToU16("Дальнобойщик"), 255, 0, true, 0, 0);
				//NAPI.Blip.CreateBlip(515, new Vector3(274.8017, -3015.355, 4.578117), 0.7f, 64, Main.StringToU16("Доставка заказов"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(67, new Vector3(915.9069, -1265.255, 25.52912), 1, 63, Main.StringToU16("Инкасатор"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(67, new Vector3(-1481.75537, -508.08847, 31.6868382), 1, 63, Main.StringToU16("Инкасатор"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(67, new Vector3(-144.374817, 6354.90869, 30.3706112), 1, 63, Main.StringToU16("Инкасатор"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(635, new Vector3(1090.414, -2234.13, 30.18403), 1, 63, Main.StringToU16("Мусоровозчик"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(728, new Vector3(982.7246, -2547.97, 27.18197), 1, 26, Main.StringToU16("Переработка мусора"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(140, new Vector3(2030.0756, 4980.156, 41), 1, 69, Main.StringToU16("Сбор листьев коки"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(566, new Vector3(145.0445, -373.0724, 42.29742), 1, 46, Main.StringToU16("Строительство"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(618, new Vector3(2947.133, 2747.014, 42.54741), 1, 22, Main.StringToU16("Каменоломня"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(729, new Vector3(1695.163, 42.85501, 160.6473), 1, 46, Main.StringToU16("Поисковик"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(532, new Vector3(906.3733, -1516.33, 29.29401), 1, 4, Main.StringToU16("Уборка штата"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(501, new Vector3(84.4153, 3731.332, 39.612), 1, 4, Main.StringToU16("Сбыт коки"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(225, new Vector3(-522.8767, 52.41306, 51.45988), 1, 33, Main.StringToU16("Аренда"), 255, 0, true, 0, 0);
                //NAPI.Blip.CreateBlip(666, new Vector3(2923.87, 4679.295, 48.71234), 0.7f, 2, Main.StringToU16("Тракторист"), 255, 0, true, 0, 0);

                NAPI.Blip.CreateBlip(677, new Vector3(2249.0852, 4875.559, 39.75508), 1, 52, Main.StringToU16("Коровник"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(514, new Vector3(2016.344, 4987.024, 41.05), 1, 64, Main.StringToU16("Ферма"), 255, 0, true, 0, 0);
               // NAPI.Blip.CreateBlip(474, new Vector3(1906.6538, 4927.4214, 47.800243), 0.8f, 70, Main.StringToU16("Фермерский склад"), 255, 0, true, 0, 0);
               // NAPI.Blip.CreateBlip(365, new Vector3(1539.6118, 6336.243, 22.95399), 1, 47, Main.StringToU16("Точка сбыта продуктов"), 255, 0, true, 0, 0);
               // NAPI.Blip.CreateBlip(474, new Vector3(-2220.969, 3484.6597, 29.049479), 0.8f, 70, Main.StringToU16("Продуктовый склад армии"), 255, 0, true, 0, 0);

                NAPI.Blip.CreateBlip(467, new Vector3(1287.885, -2556.801, 42.8497), 1, 59, Main.StringToU16("Скупка железяк"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(467, new Vector3(-694.9155, 76.82492, 54.73539), 1, 59, Main.StringToU16("Скупка золота"), 255, 0, true, 0, 0);

                NAPI.Blip.CreateBlip(471, new Vector3(4764.2095, -4719.2944, 1.057212), 1, 38, Main.StringToU16("Аренда"), 255, 0, true, 0, 0);
				NAPI.Blip.CreateBlip(471, new Vector3(-866.9688, -3094.1375, 1.4056743), 1, 38, Main.StringToU16("Аренда"), 255, 0, true, 0, 0);
				NAPI.Blip.CreateBlip(634, new Vector3(4483.158, -4457.144, 3.1297169), 1, 33, Main.StringToU16("Аренда"), 255, 0, true, 0, 0);
				
				NAPI.Blip.CreateBlip(491, new Vector3(-253.67767, -2027.8801, 28.826025), 1, 33, Main.StringToU16("GunArena"), 255, 0, true, 0, 0);

                // markers
                NAPI.Marker.CreateMarker(1, new Vector3(105.4633, -1568.843, 28.60269) - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, new Vector3(106.2007, -1563.748, 28.60272) - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, new Vector3(-0.51, -436.71, 38.74) - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        private static SortedDictionary<int, ColShape> Cols = new SortedDictionary<int, ColShape>();
        public static List<string> JobStats = new List<string>
        {
            "Электрик",
            "Почтальон",
            "Таксист",
            "Водитель автобуса",
            "Газонокосильщик",
            "Дальнобойщик",
            "Инкассатор",
			"Автомеханик",
			"Тракторист",
            "Мусоровозчик",
            "Строитель",
            "Каменщик",
			"Поисковик",
            "Уборка штата"
        };
        public static SortedList<int, Vector3> Points = new SortedList<int, Vector3>
        {
            {0, new Vector3(-1029.837, -1402.057, 4.437821) },  // Трудоустройство
            {1, new Vector3(724.9625, 133.9959, 79.83643) },  // Electrician job
            {2, new Vector3(105.4633, -1568.843, 28.60269) },  // Postal job
            {3, new Vector3(903.3215,-191.7,73.40494) },      // Taxi job
            {4, new Vector3(406.2858, -649.6152, 28.49641) }, // Bus driver job
            {5, new Vector3(-1331.475, 53.58579, 53.53268) },  // Газонокосильщик
            {6, new Vector3(588.2037, -3037.641, 6.303829) },  // Trucker job
            {7, new Vector3(915.9069, -1265.255, 25.52912) },  // Collector job
            //{8, new Vector3(473.9508, -1275.597, 29.60513) },  // AutoMechanic job
			{9, new Vector3(2923.87, 4679.295, 48.71234) },  // Тракторист
            {10, new Vector3(1090.414, -2234.13, 30.18403) },  // Мусоровозчик
            {14, new Vector3(906.3733, -1516.33, 29.29401) },  // Уборка штата
        };
        private static SortedList<int, string> JobList = new SortedList<int, string>
        {
            {1, "электриком" },
            {2, "почтальоном" },
            {3, "таксистом" },
            {4, "водителем автобуса" },
            {5, "газонокосильщиком" },
            {6, "дальнобойщиком" },
            {7, "инкассатором" },
            {8, "автомехаником" },
			{9, "тракторист" },
            {10, "мусоровозщик" },
            {11, "Строитель" },
            {12, "Каменщик" },
            {13, "Поисковик" },
            {14, "уборщик" }
        };
        private static SortedList<int, int> JobsMinLVL = new SortedList<int, int>()
        {
            { 1, 0 },
            { 2, 1 },
            { 3, 1 },
            { 4, 2 },
            { 5, 0 },
            { 6, 5 },
            { 7, 4 },
            { 8, 4 },
			{ 9, 1 },
            { 10, 3},
            { 11, 1},
            { 12, 1},
            { 13, 1},
            { 14, 1}
        };

        public static void Layoff(Player player)
        {
            if (NAPI.Data.GetEntityData(player, "ON_WORK") == true)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны сначала закончить рабочий день", 3000);
                return;
            }
            if (Main.Players[player].WorkID != 0)
            {
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы уволились с работы", 3000);
                Main.Players[player].WorkID = 0;
                Dashboard.sendStats(player);
                Trigger.PlayerEvent(player, "showJobMenu", Main.Players[player].LVL, Main.Players[player].WorkID);
            }
            else
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы безработный", 3000);
        }
        public static void JobJoin(Player player, int job)
        {

            if (Main.Players[player].WorkID != 0)
            {
                if (NAPI.Data.GetEntityData(player, "ON_WORK") == true)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны сначала закончить рабочий день", 3000);
                    return;
                }
                Layoff(player);
            }

            if (Main.Players[player].WorkID == job)
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже работаете {JobList[job]}", 3000);
            else
            {
                if (Main.Players[player].LVL < JobsMinLVL[job])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Необходим как минимум {JobsMinLVL[job]} уровень", 3000);
                    return;
                }
                if ((job == 3 || job == 8) && !Main.Players[player].Licenses[1])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет лицензии категории B", 3000);
                    return;
                }
                if ((job == 4 || job == 6 || job == 7 || job == 10) && !Main.Players[player].Licenses[2])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет лицензии категории C", 3000);
                    return;
                }
                Main.Players[player].WorkID = job;
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы устроились работать {JobList[job]}. Доберитесь до точки начала работы", 3000);
                Trigger.PlayerEvent(player, "createWaypoint", Points[job].X, Points[job].Y);
                Dashboard.sendStats(player);
                Trigger.PlayerEvent(player, "showJobMenu", Main.Players[player].LVL, Main.Players[player].WorkID);
            }
        }
        // REQUIRED REFACTOR //
        public static void load(Player player)
        {
            NAPI.Data.SetEntityData(player, "ON_WORK", false);
            NAPI.Data.SetEntityData(player, "PAYMENT", 0);
            NAPI.Data.SetEntityData(player, "BUS_ONSTOP", false);
            NAPI.Data.SetEntityData(player, "IS_CALL_TAXI", false);
            NAPI.Data.SetEntityData(player, "IS_REQUESTED", false);
            NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
            player.SetData("PACKAGES", 0);
            NAPI.Data.SetEntityData(player, "WORK", null);
            NAPI.Data.SetEntityData(player, "WORKWAY", -1);
            NAPI.Data.SetEntityData(player, "IS_PRICED", false);
            NAPI.Data.SetEntityData(player, "ON_DUTY", false);
            NAPI.Data.SetEntityData(player, "CUFFED", false);
            NAPI.Data.SetEntityData(player, "IN_CP_MODE", false);
            NAPI.Data.SetEntityData(player, "WANTED", 0);
            NAPI.Data.SetEntityData(player, "REQUEST", "null");
            player.SetData("IS_IN_ARREST_AREA", false);
            player.SetData("PAYMENT", 0);
            player.SetData("INTERACTIONCHECK", 0);
            player.SetData("IN_HOSPITAL", false);
            player.SetData("MEDKITS", 0);
            player.SetData("GANGPOINT", -1);
            player.SetData("CUFFED_BY_COP", false);
            player.SetData("CUFFED_BY_MAFIA", false);
            player.SetData("IS_CALL_MECHANIC", false);
            NAPI.Data.SetEntityData(player, "CARROOM_CAR", null);
        }

        #region Jobs
        #region Jobs Selecting
        public static void openJobsSelecting(Player player, int id)
        {
            Trigger.PlayerEvent(player, "showJobMenu", Main.Players[player].LVL, Main.Players[player].WorkID, id);
        }
        [RemoteEvent("jobjoin")]
        public static void callback_jobsSelecting(Player Player, int act)
        {
            try
            {
                switch (act)
                {
                    case -1:
                        Layoff(Player);
                        return;
                    default:
                        JobJoin(Player, act);
                        return;
                }
            }
            catch (Exception e) { Log.Write("jobjoin: " + e.Message, nLog.Type.Error); }
        }
        #endregion
        #region GoPostal Job
        public static void openGoPostalStart(Player player)
        {
            Menu menu = new Menu("gopostal", false, false);
            menu.Callback = callback_gpStartMenu;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Склад";
            menu.Add(menuItem);

            menuItem = new Menu.Item("start", Menu.MenuItem.Button);
            menuItem.Text = "Начать работу";
            menu.Add(menuItem);

            menuItem = new Menu.Item("get", Menu.MenuItem.Button);
            menuItem.Text = "Взять посылки";
            menu.Add(menuItem);

            menuItem = new Menu.Item("finish", Menu.MenuItem.Button);
            menuItem.Text = "Закончить работу";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }

        private static void callback_gpStartMenu(Player Player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (!Main.Players.ContainsKey(Player) || Player.Position.DistanceTo(Gopostal.Coords[0]) > 15)
            {
                MenuManager.Close(Player);
                return;
            }
            switch (item.ID)
            {
                case "start":
                    if (Main.Players[Player].WorkID == 2)
                    {
                        if (!NAPI.Data.GetEntityData(Player, "ON_WORK"))
                        {
                            if (Houses.HouseManager.Houses.Count == 0) return;
                            Player.SetData("PACKAGES", 10);
                            Notify.Send(Player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили 10 посылок, развезите их по домам", 3000);
                            Player.SetData("ON_WORK", true);

                            Player.SetData("PAYMENT", 0);
                            Trigger.PlayerEvent(Player, "JobStatsInfo", Player.GetData<int>("PAYMENT"));

                            Player.SetData("W_LASTPOS", Player.Position);
                            Player.SetData("W_LASTTIME", DateTime.Now);
                            var next = Jobs.WorkManager.rnd.Next(0, Houses.HouseManager.Houses.Count - 1);
                            while (Houses.HouseManager.Houses[next].Position.DistanceTo2D(Player.Position) < 200)
                                next = Jobs.WorkManager.rnd.Next(0, Houses.HouseManager.Houses.Count - 1);

                            Player.SetData("NEXTHOUSE", Houses.HouseManager.Houses[next].ID);
                            Trigger.PlayerEvent(Player, "createCheckpoint", 1, 1, Houses.HouseManager.Houses[next].Position, 1, 0, 255, 0, 0);
                            Trigger.PlayerEvent(Player, "createWaypoint", Houses.HouseManager.Houses[next].Position.X, Houses.HouseManager.Houses[next].Position.Y);
                            Trigger.PlayerEvent(Player, "createWorkBlip", Houses.HouseManager.Houses[next].Position);

                            var gender = Main.Players[Player].Gender;
                            Customization.ClearClothes(Player, gender);
                            if (gender)
                            {
                                Customization.SetHat(Player, 76, 10);
                                Player.SetClothes(11, 38, 3);
                                Player.SetClothes(4, 17, 0);
                                Player.SetClothes(6, 1, 7);
                                Player.SetClothes(3, Core.Customization.CorrectTorso[gender][38], 0);
                            }
                            else
                            {
                                Customization.SetHat(Player, 75, 10);
                                Player.SetClothes(11, 0, 6);
                                Player.SetClothes(4, 25, 2);
                                Player.SetClothes(6, 1, 2);
                                Player.SetClothes(3, Core.Customization.CorrectTorso[gender][0], 0);
                            }

                            int x = Jobs.WorkManager.rnd.Next(0, Gopostal.GoPostalObjects.Count);
                            BasicSync.AttachObjectToPlayer(Player, Jobs.Gopostal.GoPostalObjects[x], 60309, new Vector3(0.03, 0, 0.02), new Vector3(0, 0, 50));
                        }
                        else Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже начали рабочий день", 3000);
                    }
                    else Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете курьером. Устроиться можно у начальника", 3000);
                    return;
                case "get":
                    {
                        if (Main.Players[Player].WorkID != 2)
                        {
                            Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете курьером", 3000);
                            return;
                        }
                        if (!Player.GetData<bool>("ON_WORK"))
                        {
                            Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не начали рабочий день", 3000);
                            return;
                        }
                        if (Player.GetData<int>("PACKAGES") != 0)
                        {
                            Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не раздали все посылки. У Вас осталось ещё {Player.GetData<int>("PACKAGES")} штук", 3000);
                            return;
                        }
                        if (Houses.HouseManager.Houses.Count == 0) return;
                        Player.SetData("PACKAGES", 10);
                        Notify.Send(Player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили 10 посылок. Развезите их по домам", 3000);

                        Player.SetData("W_LASTPOS", Player.Position);
                        Player.SetData("W_LASTTIME", DateTime.Now);
                        var next = Jobs.WorkManager.rnd.Next(0, Houses.HouseManager.Houses.Count - 1);
                        while (Houses.HouseManager.Houses[next].Position.DistanceTo2D(Player.Position) < 200)
                            next = Jobs.WorkManager.rnd.Next(0, Houses.HouseManager.Houses.Count - 1);
                        Player.SetData("NEXTHOUSE", Houses.HouseManager.Houses[next].ID);

                        Trigger.PlayerEvent(Player, "createCheckpoint", 1, 1, Houses.HouseManager.Houses[next].Position, 1, 0, 255, 0, 0);
                        Trigger.PlayerEvent(Player, "createWaypoint", Houses.HouseManager.Houses[next].Position.X, Houses.HouseManager.Houses[next].Position.Y);
                        Trigger.PlayerEvent(Player, "createWorkBlip", Houses.HouseManager.Houses[next].Position);

                        int y = Jobs.WorkManager.rnd.Next(0, Jobs.Gopostal.GoPostalObjects.Count);
                        BasicSync.AttachObjectToPlayer(Player, Jobs.Gopostal.GoPostalObjects[y], 60309, new Vector3(0.03, 0, 0.02), new Vector3(0, 0, 50));
                        return;
                    }
                case "finish":
                    if (Main.Players[Player].WorkID == 2)
                    {
                        if (NAPI.Data.GetEntityData(Player, "ON_WORK"))
                        {
                            Trigger.PlayerEvent(Player, "deleteCheckpoint", 1, 0);
                            BasicSync.DetachObject(Player);

                            Notify.Send(Player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);
                            Trigger.PlayerEvent(Player, "deleteWorkBlip");

                            Notify.Send(Player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили зарплату в размере: {Player.GetData<int>("PAYMENT")}$", 3000);

                            MoneySystem.Wallet.Change(Player, Player.GetData<int>("PAYMENT"));
                            Trigger.PlayerEvent(Player, "CloseJobStatsInfo", Player.GetData<int>("PAYMENT"));
                            Golemo.Families.Family.GiveMoneyOnJob(Player, Player.GetData<int>("PAYMENT"));
                            Player.SetData("PAYMENT", 0);

                            Customization.ApplyCharacter(Player);
                            if (Player.HasData("HAND_MONEY")) Player.SetClothes(5, 45, 0);
                            else if (Player.HasData("HEIST_DRILL")) Player.SetClothes(5, 41, 0);

                            Player.SetData("PACKAGES", 0);
                            Player.SetData("ON_WORK", false);

                            if (Player.GetData<Vehicle>("WORK") != null)
                            {
                                NAPI.Entity.DeleteEntity(Player.GetData<Vehicle>("WORK"));
                                Player.ResetData("WORK");
                            }
                        }
                        else Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете", 3000);

                    }
                    else Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете курьером", 3000);
                    return;
                case "close":
                    MenuManager.Close(Player);
                    return;
            }
        }
        #endregion
		
		public class WorkPoint
        {
            public int Job { get; set; }
            public Vector3 Pos { get; set; }
            public int Heading { get; set; }
            public string Name { get; set; }
            public int Type { get; set; }
            public bool Interact { get; set; }
            [JsonIgnore]
            public ColShape Shape { get; set; }

            public WorkPoint(int job, Vector3 pos, int heading, string name, bool interact = false)
            {
                Pos = pos;Heading = heading;Job = job;Name = name;Interact = interact;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16($"~b~{Name}"), pos + new Vector3(0, 0, 2.2f), 10F, 0.3F, 0, new Color(255, 255, 255));
                NAPI.Marker.CreateMarker(27, Pos + new Vector3(0, 0, 0.12f), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220), false, 0);
                Shape = NAPI.ColShape.CreateCylinderColShape(pos, 1.2f, 3);
                Shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        if (!Main.Players.ContainsKey(entity)) return;
                        if (Interact) entity.SetData("INTERACTIONCHECK", 75);
                        else entity.SetData("INTERACTIONCHECK", 56);
                        entity.SetData("JOBID", Job);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                Shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        
                        entity.SetData("INTERACTIONCHECK", 0);
                        entity.SetData("JOBID", -1);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };

            }
        }
        #endregion
    }
}