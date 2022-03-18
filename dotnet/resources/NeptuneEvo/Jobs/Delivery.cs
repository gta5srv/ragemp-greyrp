using GTANetworkAPI;
using System.Linq;
using NeptuneEVO.Core;
using NeptuneEVO.GUI;
using MySql.Data.MySqlClient;
using NeptuneEVO.MoneySystem;
using NeptuneEVO.SDK;
using System;
using System.Collections.Generic;

namespace NeptuneEVO.Jobs
{
    class Delivery : Script
    {
        private static nLog Log = new nLog("Delivery");
        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        #region ColShape
        private void Deliverye_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
            }
            catch (Exception ex) { Log.Write("Deliverye_onEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
        }
        private void Deliverye_onEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception ex) { Log.Write("Deliverye_onEntityExitColShape: " + ex.Message, nLog.Type.Error); }
        }
        #endregion
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try
            {
                //NAPI.Blip.CreateBlip(126, new Vector3(168.48282, -1632.5292, 28.171669), 0.7f, 52, Main.StringToU16("Курьер 5UP"), 255, 0, true, 0, 0);

                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(new Vector3(168.48282, -1632.5292, -28.171669), 1, 2, 0));// Начать работать. Убрать минус у Z
                Cols[0].OnEntityEnterColShape += Deliverye_onEntityEnterColShape;
                Cols[0].OnEntityExitColShape += Deliverye_onEntityExitColShape;
                Cols[0].SetData("INTERACT", 404);
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("Устроится в 5UP"), new Vector3(168.48282, -1632.5292, -28.171669) + new Vector3(0, 0, 1.4), 10F, 0.6F, 0, new Color(0, 180, 0)); //Убрать минус у Z

                int i = 0;
                foreach (var Check in Checkpoints1)
                {
                    var col = NAPI.ColShape.CreateCylinderColShape(Check.Position, 1, 2, 0);
                    col.SetData("NUMBER:Deliverye", i);
                    col.OnEntityEnterColShape += EnterCheckpoint;
                    i++;
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        #region Events
        [RemoteEvent("StartDeliverys")]
        public static void StartDelivery(Player player)
        {
            if (player.GetData<bool>("ON_WORK:Deliverye"))
            {
                var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Delivery));
                if (tryAdd == 10 || tryAdd > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы уже взяли 10 заказов", 3000);
                    return;
                }
                Customization.ApplyCharacter(player);
                player.SetData("ON_WORK:Deliverye", false);
                Trigger.PlayerEvent(player, "deleteCheckpoint", 15);
                Trigger.PlayerEvent(player, "deleteDeliveryBlip");
                Trigger.PlayerEvent(player, "deleteWaypoint");
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили доставку", 3000);
                return;
            }
            else
            {
                var check = WorkManager.rnd.Next(0, Checkpoints1.Count - 1);
                player.SetData("WORKCHECK:Deliverye", check);
                Trigger.PlayerEvent(player, "createCheckpoint", 15, 1, Checkpoints1[check].Position, 1, 0, 255, 0, 0);
                Trigger.PlayerEvent(player, "createWaypoint", Checkpoints1[check].Position.X, Checkpoints1[check].Position.Y);
                Trigger.PlayerEvent(player, "createDeliveryBlip", Checkpoints1[check].Position);
                player.SetData("ON_WORK:Deliverye", true);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы начали работать курьером", 3000);
                return;
            }
        }
        [RemoteEvent("TakeDeliverys")]
        public static void TakeDelivery(Player player)
        {
            if (player.HasData("ON_WORK:Deliverye"))
            {
                var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Delivery));
                if (tryAdd == 10 || tryAdd > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы уже взяли 10 заказов", 3000);
                    return;
                }
                nInventory.Add(player, new nItem(ItemType.Delivery, 10, ""));
            }
        }
        #endregion
        public static void interactPressed(Player player, int index)
        {
            switch (index)
            {
                case 404:
                    try
                    {
                        if (!Main.Players.ContainsKey(player)) return;
                        Trigger.PlayerEvent(player, "OpenDelivery", true);
                        return;
                    }
                    catch (Exception e) { Log.Write("interactDelivery: " + e.Message, nLog.Type.Error); }
                    return;
            }
        }
        private static void EnterCheckpoint(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!player.GetData<bool>("ON_WORK:Deliverye") || shape.GetData<int>("NUMBER:Deliverye") != player.GetData<int>("WORKCHECK:Deliverye")) return;
                if (Checkpoints1[(int)shape.GetData<int>("NUMBER:Deliverye")].Position.DistanceTo(player.Position) > 3) return;
                NAPI.Entity.SetEntityPosition(player, Checkpoints1[shape.GetData<int>("NUMBER:Deliverye")].Position + new Vector3(0, 0, 1.2));
                NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, Checkpoints1[shape.GetData<int>("NUMBER:Deliverye")].Heading));
                Main.OnAntiAnim(player);
                player.PlayAnimation("anim@heists@money_grab@briefcase", "put_down_case", 39);
                BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_irish_sign_06"), 6286, new Vector3(0, 0, 0), new Vector3(85, -15, -90));
                player.SetData("WORKCHECK:Deliverye", -1);
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (player != null && Main.Players.ContainsKey(player))
                        {
                            nInventory.Remove(player, ItemType.Delivery, 1);
                            player.StopAnimation();
                            Main.OffAntiAnim(player);
                            BasicSync.DetachObject(player);
                            var nextCheck = WorkManager.rnd.Next(0, Checkpoints1.Count - 1);
                            while (nextCheck == shape.GetData<int>("NUMBER:Deliverye"))
                                nextCheck = WorkManager.rnd.Next(0, Checkpoints1.Count - 1);
                            player.SetData("WORKCHECK:Deliverye", nextCheck);
                            Trigger.PlayerEvent(player, "createCheckpoint", 15, 1, Checkpoints1[nextCheck].Position, 1, 0, 255, 0, 0);
                            Trigger.PlayerEvent(player, "createWaypoint", Checkpoints1[nextCheck].Position.X, Checkpoints1[nextCheck].Position.Y);
                            Trigger.PlayerEvent(player, "createDeliveryBlip", Checkpoints1[nextCheck].Position);
                        }
                    }
                    catch { }
                }, 2200);
            }
            catch (Exception e) { Log.Write("EnterCheckpoint: " + e.Message, nLog.Type.Error); }
        }
        #region Checks
        private static List<Checkpoint> Checkpoints1 = new List<Checkpoint>()
        {
           new Checkpoint(new Vector3(-19.876028, 6387.78, 30.232035), 47.18349),
           new Checkpoint(new Vector3(-52.412647, 6355.2227, 30.262545), 132.54846),
        };
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
        #endregion
    }
}
