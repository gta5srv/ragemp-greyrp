using GTANetworkAPI;
using System.Collections.Generic;
using System;
using NeptuneEVO.GUI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Jobs
{
    class Diver : Script
    {
        //private static int JobMultiper = 2;
        private static int checkpointPayment = 10;
        private static int JobWorkId = 13;
        private static int JobsMinLVL = 1;

        private static List<int> Quality = new List<int> { };

        private static nLog Log = new nLog("Diver");

        #region Эвенты
        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                ColShape col = NAPI.ColShape.CreateCylinderColShape(new Vector3(1287.885, -2556.801, 42.8497), 2, 2);
                NAPI.TextLabel.CreateTextLabel("~b~Скупка железяк", new Vector3(1287.885, -2556.801, 42.8497 + 2.2f), 10F, 0.5F, 0, new Color(255, 255, 255), true, 0);
                NAPI.Marker.CreateMarker(27, new Vector3(1287.885, -2556.801, 42.8497) + new Vector3(0, 0, 0.12f), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220), false, 0);
                col.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 590);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                col.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };

                col = NAPI.ColShape.CreateCylinderColShape(new Vector3(-694.9155, 76.82492, 54.73539), 2, 2);
                NAPI.TextLabel.CreateTextLabel("~b~Скупка золота", new Vector3(-694.9155, 76.82492, 54.73539 + 2.2f), 10F, 0.5F, 0, new Color(255, 255, 255), true, 0);
                NAPI.Marker.CreateMarker(27, new Vector3(-694.9155, 76.82492, 54.73539) + new Vector3(0, 0, 0.12f), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220), false, 0);
                col.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 591);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                col.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };


                int i = 0;
                int gold = 0;
                foreach (var Check in Checkpoints)
                {
                    col = NAPI.ColShape.CreateCylinderColShape(Check, 1, 2, 0);
                    col.SetData("CHECK", i);
                    col.OnEntityEnterColShape += PlayerEnterCheckpoint;
                    int f = GetNORMAL();
                    
                    GTANetworkAPI.Object obj = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(f == 1 ? "prop_gold_bar" : "prop_ecola_can"), Check, new Vector3(0, 0, 0));
                    col.SetData("OBJECT", obj);
                    col.SetData("POS", Check);
                    //Log.Write($"Create checkpoint: check {i} Type {f}");
                    Quality.Add(f);
                    if (f == 1) gold++;
                    i++;

                };
                Log.Write($"Total Gold: {gold}");

                /*foreach(int f in Quality)
                    Log.Write($"update quality: {f}");*/

            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        #endregion
        #region Логика Появления
        private static Random rnd = new Random();

        public static int GetNORMAL()
        {
            return HaveQuality() ? 0 : GetRND();
        }

        public static int GetRND()
        {
            int i = rnd.Next(0, 3);
            return i == 2 ? 1 : 0;
        }

        public static bool HaveQuality()
        {
            int golds = 0;
            foreach (int i in Quality)
                if (i == 1)
                    golds += 1;
            return golds == 3;
        }
        #endregion
        #region Чекпоинты
        private static List<Vector3> Checkpoints = new List<Vector3>()
        {
            new Vector3(1734.487, 13.29263, 149.8252),
            new Vector3(1773.672, -11.32591, 150.5736),
            new Vector3(1782.208, 18.87999, 147.6415),
            new Vector3(1809.962, 55.5253, 150.6816),
            new Vector3(1860.173, 84.78613, 152.3218),
            new Vector3(1939.862, 97.1432, 154.5702),
            new Vector3(1931.413, 158.8011, 155.6049),
            new Vector3(1976.636, 188.23, 151.737),
            new Vector3(1936.88, 224.0616, 152.1553),
            new Vector3(1939.967, 248.9835, 153.5511),
            new Vector3(1964.86, 277.64, 152.4141),
            new Vector3(1974.194, 226.8112, 148.2248),
            new Vector3(1938.716, 180.2173, 152.5764),
            new Vector3(1934.763, 128.8974, 150.644),
            new Vector3(1878.33, 100.2574, 148.2765),
            new Vector3(1866.46, 48.64326, 148.5168),
            new Vector3(1766.836, 56.35709, 148.4627),
            new Vector3(1788.673, 1.769922, 147.2962),
            new Vector3(1851.856, 38.29123, 147.8622),
            new Vector3(1905.896, 106.5646, 148.6448),
            new Vector3(1983.478, 189.5326, 147.0779),
            new Vector3(1971.811, 240.9919, 148.6227),
            new Vector3(2020.213, 267.9695, 157.1836),
            new Vector3(1927.721, 151.8651, 150.7533),
            new Vector3(1779.356, 34.01603, 144.4495),
        };
        #endregion
        #region Рандом Штаны и куртка Для мужиков
        private static List<string> SetClothes4 = new List<string>()
        {
            "0", // Штаны
            "1", // Штаны
            "2", // Штаны
            "3", // Штаны
        };
        private static List<string> SetClothes11 = new List<string>()
        {
            "0", // Куртка
            "1", // Куртка
            "2", // Куртка
            "3", // Куртка
        };
        #endregion
        #region Рандом Куртка и куртка Для девушек
        private static List<string> SetClothes4_2 = new List<string>()
        {
            "0", // Штаны
            "1", // Штаны
            "2", // Штаны
            "3", // Штаны
        };
        private static List<string> SetClothes11_2 = new List<string>()
        {
            "0", // Куртка
            "1", // Куртка
            "2", // Куртка
            "3", // Куртка
        };
        #endregion
        #region Одеть одежду
        public static void SetClothes(Player player)
        {
            var RandomClothes4 = WorkManager.rnd.Next(0, SetClothes4.Count);
            var RandomClothes11 = WorkManager.rnd.Next(0, SetClothes11.Count);
            var RandomClothes4_2 = WorkManager.rnd.Next(0, SetClothes4_2.Count);
            var RandomClothes11_2 = WorkManager.rnd.Next(0, SetClothes11_2.Count);
            Customization.ClearClothes(player, Main.Players[player].Gender);
            if (Main.Players[player].Gender)
            {
                player.SetClothes(8, 151, 0); // Балон
                player.SetClothes(3, 17, 0); // Перчатки
                player.SetClothes(6, 67, 0); // Ласты
                player.SetClothes(11, 178, RandomClothes11); // Куртка
                player.SetClothes(4, 77, RandomClothes4); // Штаны
            }
            else
            {
                player.SetClothes(8, 187, 0); // Балон
                player.SetClothes(3, 18, 0); // Перчатки
                player.SetClothes(6, 70, 0); // Ласты
                player.SetClothes(11, 180, RandomClothes11_2); // Куртка
                player.SetClothes(4, 79, RandomClothes11_2); // Штаны
            }
        }
        #endregion
        #region Начать рабочий день
        public static void StartWorkDay(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (Main.Players[player].LVL < JobsMinLVL)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Необходим как минимум {JobsMinLVL} уровень", 3000);
                return;
            }
            if (Main.Players[player].WorkID != 13 && player.GetData<bool>("ON_WORK")) return;
            if (Main.Players[player].WorkID != 13)
                Jobs.WorkManager.Layoff(player);

            if (player.GetData<bool>("ON_WORK"))
            {
                for (int i = 0; i < 25; i++)
                {
                    Trigger.PlayerEvent(player, "deleteJobMenusBlip", i);
                }

                Customization.ApplyCharacter(player);
                player.SetData("ON_WORK", false);

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили зарплату в размере: {player.GetData<int>("PAYMENT")}$", 3000);
                MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                Golemo.Families.Family.GiveMoneyOnJob(player, player.GetData<int>("PAYMENT"));
                player.SetData("PAYMENT", 0);
                Trigger.PlayerEvent(player, "stopdiving");
                Main.Players[player].WorkID = 0;
            }
            else
            {
                Main.Players[player].WorkID = 13;

                SetClothes(player);

                for (int i = 0; i < 25; i++)
                {
                    Trigger.PlayerEvent(player, "JobMenusBlip", i, 66, Checkpoints[i], "Мусор", 0);
                }

                player.SetData("ON_WORK", true);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы начали рабочий день! На карте отмечены места с мусором. Соберите этот мусор.", 3000);

                player.SetData("PAYMENT", 0);
                Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));

                Trigger.PlayerEvent(player, "startdiving");
            }

        }
        #endregion
        #region Когда заходишь в чекпоинт
        private static void PlayerEnterCheckpoint(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID != JobWorkId || !player.GetData<bool>("ON_WORK") || !shape.HasData("OBJECT")) return;
                //if (Quality[shape.GetData<int>("CHECK")] == null) return;
                nItem item;
                if (Quality[shape.GetData<int>("CHECK")] == 1)
                {
                    item = new nItem(ItemType.Gold);
                    int tryadd = nInventory.TryAdd(player, item); ;
                    if (tryadd == -1)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нет места для золота!", 3000);
                        return;
                    }
                }
                else
                {
                    item = new nItem(ItemType.Iron);
                    var find = nInventory.Find(Main.Players[player].UUID, ItemType.Iron);
                    if (find != null && find.Count >= 200)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нет места для железяки!", 3000);
                        return;
                    }
                    Utils.QuestsManager.AddQuestProcess(player, 3);

                }


                nInventory.Add(player, item);

                var payment = (checkpointPayment + Main.AddJobPoint(player) * 1);
                var level = Main.Players[player].LVL > 5 ? 6 : 1 * Main.Players[player].LVL;
                player.SetData("PAYMENT", player.GetData<int>("PAYMENT") + (payment + level) * Main.Multipy);

                player.SendNotification($"Дайвер: ~h~~g~+{(payment + level) * Main.Multipy}$", true);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы собрали {nInventory.ItemsNames[item.ID]}", 3000);

                Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));

                NAPI.Task.Run(() => {
                    try
                    {
                        if (shape != null && shape.HasData("OBJECT"))
                        {
                            shape.GetData<GTANetworkAPI.Object>("OBJECT").Delete();
                            shape.ResetData("OBJECT");
                        }

                    }
                    catch { }
                });
                Timers.StartOnceTask(90000, () => CreateNewObject(shape));

            }
            catch (Exception e) { Log.Write("PlayerEnterCheckpoint: " + e.Message, nLog.Type.Error); }
        }
        #endregion
        #region Таймер объекта
        public static void CreateNewObject(ColShape shape)
        {
            NAPI.Task.Run(() => {
                if (shape == null) return;
                int f = GetNORMAL();
                string model = f == 1 ? "prop_gold_bar" : "prop_ecola_can";

                GTANetworkAPI.Object obj = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(model), shape.GetData<Vector3>("POS"), new Vector3(0, 0, 0));
                shape.SetData("OBJECT", obj);
                Quality[shape.GetData<int>("CHECK")] = f;
            });

        }
        #endregion
        #region Если игрок умер
        public static void Event_PlayerDeath(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID == JobWorkId && player.GetData<bool>("ON_WORK"))
                {

                    for (int i = 0; i < 25; i++)
                    {
                        Trigger.PlayerEvent(player, "deleteJobMenusBlip", i);
                    }

                    Customization.ApplyCharacter(player);
                    player.SetData("ON_WORK", false);

                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили зарплату в размере: {player.GetData<int>("PAYMENT")}$", 3000);
                    MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                    Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                    player.SetData("PAYMENT", 0);
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }
        #endregion
        #region Если игрок вышел из игры или его кикнуло
        public static void Event_PlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (Main.Players[player].WorkID == JobWorkId && player.GetData<bool>("ON_WORK"))
                {
                    for (int i = 0; i < 25; i++)
                    {
                        Trigger.PlayerEvent(player, "deleteJobMenusBlip", i);
                    }

                    Customization.ApplyCharacter(player);
                    player.SetData("ON_WORK", false);

                    MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                    player.SetData("PAYMENT", 0);
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }
        #endregion

    }
}
