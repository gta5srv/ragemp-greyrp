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
    class Vineyarad : Script
    {
        private static nLog Log = new nLog("Vineyard");
        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        #region ColShape
        private void Vineyard_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
            }
            catch (Exception ex) { Log.Write("Vineyard_onEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
        }
        private void Vineyard_onEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception ex) { Log.Write("Vineyard_onEntityExitColShape: " + ex.Message, nLog.Type.Error); }
        }
        #endregion
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try
            {
                //NAPI.Blip.CreateBlip(140, new Vector3(2220.438, 5614.343, 53.60628), 1, 41, Main.StringToU16("Сбор Винограда"), 255, 0, true, 0, 0);

                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(new Vector3(-1887.9357, 2076.2156, 140.11741), 1, 2, 0));// Начать сбор винограда.
                Cols[0].OnEntityEnterColShape += Vineyard_onEntityEnterColShape;
                Cols[0].OnEntityExitColShape += Vineyard_onEntityExitColShape;
                Cols[0].SetData("INTERACT", 400);
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Cбор винограда"), new Vector3(-1887.9357, 2076.2156, 139.11741) + new Vector3(0, 0, 2.85), 10F, 0.6F, 0, new Color(0, 0, 0));

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(new Vector3(-1871.59, 2069.9512, 140.11741), 1, 2, 0)); // Продать виноград.
                Cols[1].OnEntityEnterColShape += Vineyard_onEntityEnterColShape;
                Cols[1].OnEntityExitColShape += Vineyard_onEntityExitColShape;
                Cols[1].SetData("INTERACT", 401);
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Продать виноград"), new Vector3(-1871.59, 2069.9512, 139.11741) + new Vector3(0, 0, 2.85), 10F, 0.6F, 0, new Color(0, 0, 0));
                int i = 0;
                foreach (var Check in Checkpoints1)
                {
                    var col = NAPI.ColShape.CreateCylinderColShape(Check.Position, 1, 2, 0);
                    col.SetData("NUMBER1", i);
                    col.OnEntityEnterColShape += EnterCheckpoint;
                    i++;
                }

            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        public static void Start(Player player)
        {

            if (player.GetData<bool>("ON_WORK1"))
            {
                Customization.ApplyCharacter(player);
                player.SetData("ON_WORK1", false);
                Trigger.PlayerEvent(player, "deleteCheckpoint", 15);
                Trigger.PlayerEvent(player, "deleteWorkBlip");
                //int UUID = Main.Players[player].UUID;
                //var Vinograd = nInventory.Items[UUID].Find(t => t.Type == ItemType.Vinograd);
                //if (Vinograd != null)
                //{
                //    nInventory.Remove(player, Vinograd.Type, Vinograd.Count);
                //    Dashboard.sendItems(player);
                //    int payment = (int)(Vinograd.Count * 200);
                //    Wallet.Change(player, payment);
                //    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно продали {Vinograd.Count} штук винограда. За: {payment}$", 3000);
                //}
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили сбор винограда.", 3000);
                return;
            }
            else
            {
                var check = WorkManager.rnd.Next(0, Checkpoints1.Count - 1);
                player.SetData("WORKCHECK1", check);
                Trigger.PlayerEvent(player, "createCheckpoint", 15, 1, Checkpoints1[check].Position, 1, 0, 255, 0, 0);
                Trigger.PlayerEvent(player, "createWorkBlip", Checkpoints1[check].Position);
                player.SetData("ON_WORK1", true);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы начали сбор винограда.", 3000);
                return;
            }
        }
        public static void interactPressed(Player player, int index)
        {
            switch (index)
            {
                case 400:
                    try
                    {
                        if (!Main.Players.ContainsKey(player)) return;
                        Start(player);
                    }
                    catch (Exception e) { Log.Write("StartWorkVinograd: " + e.Message, nLog.Type.Error); }
                    return;
                case 401:
                    try
                    {
                        if (!Main.Players.ContainsKey(player)) return;
                        int UUID = Main.Players[player].UUID;
                        var Vinograd = nInventory.Items[UUID].Find(t => t.Type == ItemType.Vinograd);
                        if (Vinograd == null)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет винограда", 3000);
                            return;
                        }
                        nInventory.Remove(player, Vinograd.Type, Vinograd.Count);
                        Dashboard.sendItems(player);
                        int payment = (int)(Vinograd.Count * 200);
                        Wallet.Change(player, payment);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали {Vinograd.Count} гроздь винограда за: {payment}$", 3000);
                    }
                    catch (Exception e) { Log.Write("SellVinograd: " + e.Message, nLog.Type.Error); }
                    return;
            }

        }
        private static void EnterCheckpoint(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!player.GetData<bool>("ON_WORK1") || shape.GetData<int>("NUMBER1") != player.GetData<int>("WORKCHECK1")) return;
                if (Checkpoints1[(int)shape.GetData<int>("NUMBER1")].Position.DistanceTo(player.Position) > 3) return;
                NAPI.Entity.SetEntityPosition(player, Checkpoints1[shape.GetData<int>("NUMBER1")].Position + new Vector3(0, 0, 1.2));
                NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, Checkpoints1[shape.GetData<int>("NUMBER1")].Heading));
                Main.OnAntiAnim(player);
                player.PlayAnimation("amb@prop_human_movie_studio_light@base", "base", 39);
                player.SetData("WORKCHECK1", -1);
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (player != null && Main.Players.ContainsKey(player))
                        {
                            player.StopAnimation();
                            Main.OffAntiAnim(player);
                            var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Vinograd, 1));
                            if (tryAdd == -1 || tryAdd > 0) 
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно места в инвентаре", 3000);
                            else 
                            nInventory.Add(player, new nItem(ItemType.Vinograd, 1, ""));
                            var nextCheck = WorkManager.rnd.Next(0, Checkpoints1.Count - 1);
                            while (nextCheck == shape.GetData<int>("NUMBER1")) 
                            nextCheck = WorkManager.rnd.Next(0, Checkpoints1.Count - 1);
                            player.SetData("WORKCHECK1", nextCheck);
                            Trigger.PlayerEvent(player, "createCheckpoint", 15, 1, Checkpoints1[nextCheck].Position, 1, 0, 255, 0, 0);
                            Trigger.PlayerEvent(player, "createWorkBlip", Checkpoints1[nextCheck].Position);
                        }
                    }
                    catch { }
                }, 4000);
            }
            catch (Exception e) { Log.Write("EnterCheckpoint: " + e.Message, nLog.Type.Error); }
        }
        #region Checks
        private static List<Checkpoint> Checkpoints1 = new List<Checkpoint>()
        {
           new Checkpoint(new Vector3(-1873.5573, 2098.0247, 138.33388), 1.4958386),
           new Checkpoint(new Vector3(-1877.0454, 2097.8062, 138.77295), 4.2231193),
           new Checkpoint(new Vector3(-1880.213, 2097.5369, 138.99448), 1.9370142),
           new Checkpoint(new Vector3(-1883.709, 2097.925, 138.74509), -2.3200648),
           new Checkpoint(new Vector3(-1886.3248, 2098.3833, 138.45619), 4.4064555),
           new Checkpoint(new Vector3(-1889.7186, 2098.8315, 138.07822), 14.547811),
           new Checkpoint(new Vector3(-1892.0507, 2098.8875, 137.81697), 13.671183),
           new Checkpoint(new Vector3(-1898.4114, 2099.713, 136.43787), -4.3597913),
           new Checkpoint(new Vector3(-1900.8599, 2100.2068, 135.56851), 0.38429374),
           new Checkpoint(new Vector3(-1905.1821, 2100.6191, 134.22183), 1.8109562),
           new Checkpoint(new Vector3(-1909.5898, 2101.3936, 132.69275), -1.0996604),
           new Checkpoint(new Vector3(-1911.2906, 2101.3552, 132.16386), -10.685243),
           new Checkpoint(new Vector3(-1910.8218, 2105.5603, 130.44553), -31.64977),
           new Checkpoint(new Vector3(-1908.3895, 2105.2793, 131.32608), -22.44807),
           new Checkpoint(new Vector3(-1905.7473, 2105.1475, 132.1211), -19.55464),
           new Checkpoint(new Vector3(-1903.2211, 2104.863, 132.93643), -5.368201),
           new Checkpoint(new Vector3(-1900.9945, 2104.6848, 133.57817), -28.636017),
           new Checkpoint(new Vector3(-1910.2777, 2110.2432, 128.43814), -24.235695),
           new Checkpoint(new Vector3(-1907.2229, 2109.5083, 129.7313), 52.758358),
           new Checkpoint(new Vector3(-1901.778, 2108.6846, 131.6778), -19.434305),
           new Checkpoint(new Vector3(-1898.782, 2108.714, 132.48288), 7.414484),
           new Checkpoint(new Vector3(-1895.732, 2108.7402, 133.20502), -11.601977),
           new Checkpoint(new Vector3(-1892.7444, 2108.1301, 134.15683), -5.4254923),
           new Checkpoint(new Vector3(-1889.8402, 2108.1975, 134.65302), -7.7345195),
           new Checkpoint(new Vector3(-1887.0225, 2107.8108, 135.01447), -13.762025),
           new Checkpoint(new Vector3(-1882.6798, 2107.5671, 135.53844), -4.480112),
           new Checkpoint(new Vector3(-1880.6321, 2107.4094, 135.73589), 1.5760524),
           new Checkpoint(new Vector3(-1877.8942, 2107.5479, 135.78358), -1.936181),
           new Checkpoint(new Vector3(-1874.3134, 2107.3374, 135.59148), -15.314745),
           new Checkpoint(new Vector3(-1872.1239, 2107.013, 135.54828), -11.74522),
           new Checkpoint(new Vector3(-1868.8508, 2106.912, 135.47614), -18.580599),
           new Checkpoint(new Vector3(-1866.4186, 2106.7422, 135.49068), -14.15737),
           new Checkpoint(new Vector3(-1865.1602, 2110.688, 134.0428), 15.269734),
           new Checkpoint(new Vector3(-1867.9045, 2111.1873, 133.859), 5.8216705),
           new Checkpoint(new Vector3(-1871.4818, 2111.2717, 134.118), 9.351089),
        };
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
