using GTANetworkAPI;
using NeptuneEVO.SDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeptuneEVO.Core
{
    class Married : Script
    {

        private static nLog Log = new nLog("Married");

        private static Vector3 pos = new Vector3(-765.95074, -682.59125, 30.2);

        // Костыль
        //private static List<Player> nearply = new List<Player> { };

        private static int cost = 250;
        private static int nocost = 300;

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                //var blip = NAPI.Blip.CreateBlip(280, pos, 1, 4, "Церковь", 225, 0, true);
                var shape = NAPI.ColShape.CreateCylinderColShape(pos, 2f, 3, 0);
                shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 54);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                var label = NAPI.TextLabel.CreateTextLabel("~b~Церемония", new Vector3(pos.X, pos.Y, pos.Z), 20F, 0.5F, 0, new Color(255, 255, 255), true, 0);
                var marker = NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 1.8f), new Vector3(), new Vector3(), 1f, new Color(0, 175, 250, 220), false, 0);
                Log.Write("Married system loaded", nLog.Type.Success);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"MARSTART\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static void AgreeMessage(Player player)
        {
            if (Main.Players[player].Gender != true)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Только муж может начать церемонию!", 3000);
                return;
            }
            if (string.IsNullOrEmpty(Main.Players[player].Married))
            {
                Trigger.PlayerEvent(player, "openDialog", "WIFE_AGREE", $"Вы действительно хотите начать церемонию? {cost} $");
                return;
            }
            Trigger.PlayerEvent(player, "openDialog", "WIFE_NOAGREE", $"Вы действительно хотите развестись? {nocost} $");
        }
        public static void AgreeWife(Player player, Player wife)
        {
            string name = Main.PlayerNames[Main.Players[wife].UUID];

            if (wife == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В радиусе 3 метров нет данного человека!", 3000);
                return;
            }
            if (Main.Players[wife].Gender != false)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Данный человек мужчина!", 3000);
                Notify.Send(wife, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не являетесь женщиной!", 3000);
                return;
            }
            if (!string.IsNullOrEmpty(Main.Players[wife].Married))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У данного человека уже есть супруг!", 3000);
                Notify.Send(wife, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас уже есть супруг!", 3000);
                return;
            }
            if (Main.Players[player].Money < cost)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас не хватает {cost - Main.Players[player].Money} $!", 3000);
                return;
            }
            MoneySystem.Wallet.Change(player, -cost);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Поздравляем, ваша жена: {name}!", 3000);
            Notify.Send(wife, NotifyType.Info, NotifyPosition.BottomCenter, $"Поздравляем, ваш муж: {Main.PlayerNames[Main.Players[player].UUID]}!", 3000);
            Main.Players[wife].Married = Main.PlayerNames[Main.Players[player].UUID];
            Main.Players[player].Married = Main.PlayerNames[Main.Players[wife].UUID];
            GUI.Dashboard.sendStats(player);
            GUI.Dashboard.sendStats(wife);
            
            MySQL.Query($"UPDATE `characters` set `married`='{wife.Name}' where `firstname`='{player.Name.Split("_")[0]}' and `lastname`='{player.Name.Split("_")[1]}'");
            MySQL.Query($"UPDATE `characters` set `married`='{player.Name}' where `firstname`='{wife.Name.Split("_")[0]}' and `lastname`='{wife.Name.Split("_")[1]}'");
        }

        public static void NoAgreeWife(Player player, Player wife)
        {
            //Player wife = Main.GetNearestPlayer(player, 10);
            string name = Main.PlayerNames[Main.Players[wife].UUID];

            if (wife == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В радиусе 3 метров нет данного человека!", 3000);
                return;
            }
            if (Main.Players[wife].Married != player.Name)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не женаты с ним!", 3000);
                Notify.Send(wife, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не женаты с ним!", 3000);
                return;
            }
            if (Main.Players[player].Money < nocost)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас не хватает {nocost - Main.Players[player].Money} $!", 3000);
                return;
            }
            MoneySystem.Wallet.Change(player, -nocost);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы развелись с {name}!", 3000);
            Notify.Send(wife, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы развелись с {Main.PlayerNames[Main.Players[player].UUID]}!", 3000);
            Main.Players[wife].Married = "";
            Main.Players[player].Married = "";
            GUI.Dashboard.sendStats(player);
            GUI.Dashboard.sendStats(wife);
            MySQL.Query($"UPDATE `characters` set `married`='' where `firstname`='{player.Name.Split("_")[0]}' and `lastname`='{player.Name.Split("_")[1]}'");
            MySQL.Query($"UPDATE `characters` set `married`='' where `firstname`='{wife.Name.Split("_")[0]}' and `lastname`='{wife.Name.Split("_")[1]}'");
        }
    }
}
