using GTANetworkAPI;
using System.Collections.Generic;
using System;
using NeptuneEVO.GUI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Jobs
{
    class Miner : Script
    {
       // private static int JobMultiper = 3;
        private static int checkpointPayment = 5; // Цена 1 точки
        private static int checkpointPayment2 = 10; // Цена 2 точки
        private static int checkpointPayment3 = 15; // Цена 3 точки
        private static int JobWorkId = 12; // Номер работы
        private static int JobsMinLVL = 1; // С какого уровня можно устроиться на работу
        private static nLog Log = new nLog("Miner");

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                ColShape col;

                int i = 0;
                foreach (var Check in Checkpoints)
                {
                    col = NAPI.ColShape.CreateCylinderColShape(Check.Position, 1, 2, 0);
                    col.SetData("NUMBER2", i);
                    col.OnEntityEnterColShape += PlayerEnterCheckpoint;
                    i++;
                };

                i = 0;
                foreach (var Check in Checkpoints2)
                {
                    col = NAPI.ColShape.CreateCylinderColShape(Check.Position, 4, 2, 0);
                    col.SetData("NUMBER3", i);
                    col.OnEntityEnterColShape += PlayerEnterCheckpoint;
                    i++;
                };
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        #region Чекпоинты 
        private static List<Checkpoint> Checkpoints = new List<Checkpoint>()
        {
            new Checkpoint(new Vector3(2985.421, 2789.584, 43.76892), 215.7225), // Взять камень 0
            new Checkpoint(new Vector3(2979.828, 2795.692, 43.00959), 71.75107), // Взять камень 1
            new Checkpoint(new Vector3(2983.816, 2794.697, 42.81905), 357.624), // Взять камень 2
            new Checkpoint(new Vector3(2990.245, 2781.003, 42.4916), 17.26049), // Взять камень 3
            new Checkpoint(new Vector3(2990.063, 2777.616, 42.04763), 111.4968), // Взять камень 4
            new Checkpoint(new Vector3(2993.527, 2774.679, 41.8222), 182.0327), // Взять камень 5
            new Checkpoint(new Vector3(2997.51, 2756.311, 41.88037), 285.6414), // Взять камень 6
            new Checkpoint(new Vector3(2995.744, 2756.501, 41.79721), 154.5374), // Взять камень 7
        };
        private static List<Checkpoint> Checkpoints2 = new List<Checkpoint>()
        {
            new Checkpoint(new Vector3(2962.704, 2822.617, 42.75938), 178.5201), // Выгрузить камень
        };
        #endregion

        #region Начать рабочий день

        public static void StartWorkDay(Player player)
        {
            if (Main.Players[player].LVL < JobsMinLVL)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Необходим как минимум {JobsMinLVL} уровень", 3000);
                return;
            }
            if (Main.Players[player].WorkID != 12)
            {
                Jobs.WorkManager.Layoff(player);
            }



            Main.Players[player].WorkID = 12;
            if (player.GetData<bool>("ON_WORK"))
            {
                if (player.GetData<int>("PACKAGES") == 1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Сдайте камень прежде чем закончить рабочий день", 3000);
                    return;

                }
                Customization.ApplyCharacter(player);
                player.SetData("PACKAGES", 0);
                player.SetData("ON_WORK", false);
                Trigger.PlayerEvent(player, "deleteCheckpoint", 15);
                Trigger.PlayerEvent(player, "deleteWorkBlip");

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили зарплату в размере: {player.GetData<int>("PAYMENT")}$", 3000);

                MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
				
				Utils.QuestsManager.AddQuestProcess(player, 15, player.GetData<int>("PAYMENT"));

                Golemo.Families.Family.GiveMoneyOnJob(player, player.GetData<int>("PAYMENT"));

                Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                player.SetData("PAYMENT", 0);

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день!", 3000);
                return;
            }
            else
            {
                Customization.ClearClothes(player, Main.Players[player].Gender);
                if (Main.Players[player].Gender)
                {
                    player.SetClothes(3, 16, 0); //Торс
                    player.SetClothes(5, 40, 0); //Сумка
                    player.SetClothes(11, 251, 0); //куртка 
                    player.SetClothes(4, 97, 5); //Штаны
                    player.SetClothes(6, 81, 0); //Ботинки
                }
                else
                {
                    player.SetClothes(3, 17, 0); //Торс
                    player.SetClothes(5, 40, 0); //Сумка
                    player.SetClothes(11, 259, 0); //куртка 
                    player.SetClothes(4, 100, 5); //Штаны
                    player.SetClothes(6, 77, 0); //Ботинки
                }

                var check = WorkManager.rnd.Next(0, Checkpoints.Count - 1);
                player.SetData("WORKCHECK", check);
                Trigger.PlayerEvent(player, "createCheckpoint", 15, 1, Checkpoints[check].Position - new Vector3(0, 0, 2.5f), 3, 0, 255, 255, 255);
                Trigger.PlayerEvent(player, "createWorkBlip", Checkpoints[check].Position);
                player.SetData("PACKAGES", 0);

                player.SetData("PAYMENT", 0);
                Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));

                player.SetData("ON_WORK", true);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы начали работать шахтёром!", 3000);
                GUI.Dashboard.sendStats(player);
                return;
            }
        }
        #endregion

        #region Зашел на чекпоинт
        private static void PlayerEnterCheckpoint(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID != JobWorkId || !player.GetData<bool>("ON_WORK")) return;

                if (player.GetData<int>("PACKAGES") == 0)
                {
                    if (shape.GetData<int>("NUMBER2") == player.GetData<int>("WORKCHECK"))
                    {
                        player.SetData("PACKAGES", player.GetData<int>("PACKAGES") + 1);

                        if (shape.GetData<int>("NUMBER2") < 3)
                        { // 0 1 2
                            var payment = Convert.ToInt32((checkpointPayment + Main.AddJobPoint(player) * 4 ) * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);
                            int level = Main.Players[player].LVL > 5 ? 6 : 1 * Main.Players[player].LVL;

                            player.SetData("PAYMENT", player.GetData<int>("PAYMENT") + payment + level);
                            Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));
                            player.SendNotification($"Шахтёр: ~h~~g~+{payment + level }$", true);

                        }
                        else if (shape.GetData<int>("NUMBER2") > 2 && shape.GetData<int>("NUMBER2") < 6) // 3 4 5
                        {
                            var payment2 = Convert.ToInt32((checkpointPayment2 + Main.AddJobPoint(player) * 4) * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);
                            int level = Main.Players[player].LVL > 5 ? 6 : 1 * Main.Players[player].LVL;

                            player.SetData("PAYMENT", player.GetData<int>("PAYMENT") + payment2 * Main.Multipy);
                            Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));
                            player.SendNotification($"Шахтёр: ~h~~g~+{payment2 * Main.Multipy}$", true);
                        }
                        else if (shape.GetData<int>("NUMBER2") > 5)
                        { // 6 7 8
                            var payment3 = Convert.ToInt32((checkpointPayment3 + Main.AddJobPoint(player) * 4) * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);
                            int level = Main.Players[player].LVL > 5 ? 6 : 1 * Main.Players[player].LVL;

                            player.SetData("PAYMENT", player.GetData<int>("PAYMENT") + payment3 + level);
                            Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));
                            player.SendNotification($"Шахтёр: ~h~~g~+{payment3 + level}$", true);
                        }

                        NAPI.Entity.SetEntityPosition(player, Checkpoints[shape.GetData<int>("NUMBER2")].Position + new Vector3(0, 0, 1.2));
                        NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, Checkpoints[shape.GetData<int>("NUMBER2")].Heading));

                        Main.OnAntiAnim(player);
                        BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_tool_pickaxe"), 18905, new Vector3(0.1, 0, 0), new Vector3(60, 0, -180));
                        player.PlayAnimation("melee@large_wpn@streamed_core", "ground_attack_on_spot", 47);

                        player.SetData("WORKCHECK", -1);
                        NAPI.Task.Run(() => {try{if (player != null && Main.Players.ContainsKey(player)){ BasicSync.DetachObject(player); player.PlayAnimation("anim@mp_snowball", "pickup_snowball", 47);}}catch { }}, 3000);
                        NAPI.Task.Run(() => {
                            try{
                                if (player != null && Main.Players.ContainsKey(player)) {

                                    Main.OnAntiAnim(player);
                                    BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_rock_5_smash3"), 18905, new Vector3(0.1, 0.1, 0.2), new Vector3(-10, -75, -40));
                                    player.PlayAnimation("anim@heists@box_carry@", "idle", 49);
                                    

                                    var check = WorkManager.rnd.Next(0, Checkpoints2.Count - 1);
                                    player.SetData("WORKCHECK", check);
                                    Trigger.PlayerEvent(player, "createCheckpoint", 15, 1, Checkpoints2[check].Position, 5, 0, 255, 0, 0);
                                    Trigger.PlayerEvent(player, "createWorkBlip", Checkpoints2[check].Position);
                                }
                            }
                            catch { }
                        }, 3400);
                    }
                }
                else
                {
                    if (shape.GetData<int>("NUMBER3") == player.GetData<int>("WORKCHECK"))
                    {
                        player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1);

                        player.PlayAnimation("anim@mp_snowball", "pickup_snowball", 47);

                        player.SetData("WORKCHECK", -1);
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (player != null && Main.Players.ContainsKey(player))
                                {
                                    BasicSync.DetachObject(player);
                                    player.StopAnimation();
                                    Main.OffAntiAnim(player);

                                    var check = WorkManager.rnd.Next(0, Checkpoints.Count - 1);
                                    player.SetData("WORKCHECK", check);
                                    Trigger.PlayerEvent(player, "createCheckpoint", 15, 1, Checkpoints[check].Position, 2, 0, 255, 0, 0);
                                    Trigger.PlayerEvent(player, "createWorkBlip", Checkpoints[check].Position);
                                }
                            }
                            catch { }
                        }, 150);
                    }
                }

            }
            catch (Exception e) { Log.Write("PlayerEnterCheckpoint: " + e.Message, nLog.Type.Error); }
        }
        #endregion
        #region Если выкинуло из игры
        public static void Event_PlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (Main.Players[player].WorkID == JobWorkId && player.GetData<bool>("ON_WORK"))
                {
                    Customization.ApplyCharacter(player);
                    player.SetData("ON_WORK", false);
                    Trigger.PlayerEvent(player, "deleteCheckpoint", 15);
                    Trigger.PlayerEvent(player, "deleteWorkBlip");
                    player.SetData("PACKAGES", 0);

                    MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                    Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                    player.SetData("PAYMENT", 0);

                    player.StopAnimation();
                    Main.OffAntiAnim(player);
                    BasicSync.DetachObject(player);

                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
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
                    Customization.ApplyCharacter(player);
                    player.SetData("ON_WORK", false);
                    Trigger.PlayerEvent(player, "deleteCheckpoint", 15);
                    Trigger.PlayerEvent(player, "deleteWorkBlip");
                    player.SetData("PACKAGES", 0);

                    MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                    Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили зарплату в размере: {player.GetData<int>("PAYMENT")}$", 3000);
                    player.SetData("PAYMENT", 0);

                    player.StopAnimation();
                    Main.OffAntiAnim(player);
                    BasicSync.DetachObject(player);
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }
        #endregion


        internal class Checkpoint
        {
            public Vector3 Position { get; }
            public double Heading { get; }

            public Checkpoint(Vector3 pos, double rot)
            {
                Position = pos;
                Heading = rot;
            }
        }
    }
}
