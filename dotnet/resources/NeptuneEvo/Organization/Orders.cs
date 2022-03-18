using System;
using System.Collections.Generic;
using GTANetworkAPI;
using NeptuneEVO.SDK;
using NeptuneEVO.MoneySystem;
using NeptuneEVO.Core;
using Newtonsoft.Json;
using System.Data;
using NeptuneEVO.Businesses;

namespace NeptuneEVO.Organization
{
    class OOrders : Script
    {
        private static nLog Log = new nLog("Orders");

       private static List<Order> OrderList = new List<Order> { /*new Order("Eva_malita", 149, 1000, 226543), new Order("Miron_Rali", 149, 1500, 335534), new Order("Geron_Lemani", 149, 500, 653654)*/ };


        public class Order
        {
            public string Owner { get; set; }
            public int Materials { get; set; }
            public int Bizness { get; set; }
            public bool See { get; set; }
            public string Taker { get; set; } = "";
            public int SIM { get; set; }
            public Order(string owner, int biz, int mats, int sim)
            {
                Owner = owner; Materials = mats; Bizness = biz; See = true; SIM = sim;
            }

            public void SetSee(bool set)
            {
                See = set;
            }

        }

        [RemoteEvent("getaccess")]
        public static void GetAccess(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!string.IsNullOrEmpty(Main.Players[player].Org))
            {
                Trigger.PlayerEvent(player, "setaccess", 2);
                return;
            }
            if (Main.Players[player].BizIDs.Count > 0)
            {
                if (HaveOrder(player.Name) == null)
                {
                    Trigger.PlayerEvent(player, "setaccess", 1);
                    return;
                }
                else
                {
                    Trigger.PlayerEvent(player, "setaccess", 3);
                    return;
                }
            }

            Trigger.PlayerEvent(player, "setaccess", 0);
        }

        [RemoteEvent("getlist")]
        public static void GetList(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (string.IsNullOrEmpty(Main.Players[player].Org)) return;

            List<object> data = new List<object>();

            foreach (Order order in OrderList)
                if (order.See)
                    data.Add(new List<object> { order.Owner, IsOnline(order) ? "Online" : "Offline", order.SIM, order.Bizness, order.Materials });
                else if (order.Taker == player.Name)
                    data.Add(new List<object> { order.Owner, IsOnline(order) ? "Online" : "Offline", order.SIM, order.Bizness, order.Materials, 1 });

            Trigger.PlayerEvent(player, "setorders", JsonConvert.SerializeObject(data));

        }

        public static bool IsOnline(Order order)
        {
            foreach (Player ply in NAPI.Pools.GetAllPlayers())
                if (order.Owner == ply.Name)
                    return true;
            return false;
        }

        public static Order HaveOrder(string owner)
        {
            foreach (Order ord in OrderList)
                if (ord.Owner == owner)
                    return ord;
            return null;
        }

        public static void ToOrder(Player player, int mats)
        {
            if (!player.HasData("ORDER")) return;
            if (!Main.Players.ContainsKey(player)) return;
            if (string.IsNullOrEmpty(Main.Players[player].Org)) return;

            Order order = player.GetData<Order>("ORDER");

            if (order.Materials <= mats)
            {
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы выполнили заказ!", 5000);

                player.ResetData("ORDER");

                order.Taker = null;
                order.Bizness = -1;
                order.Materials = -1;
                order.Owner = null;
                order.SIM = -1;
                order.SetSee(false);
                OrderList.Remove(order);

            }
            else
            {
                order.Materials -= mats;
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Осталось привезти: {order.Materials}!", 5000);
            }
        }

        [RemoteEvent("untake")]
        public static void EndOrder(Player player)
        {
            if (!player.HasData("ORDER"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не брали заказ!", 5000);
                return;
            }
            if (!Main.Players.ContainsKey(player)) return;
            if (string.IsNullOrEmpty(Main.Players[player].Org)) return;

            Order order = player.GetData<Order>("ORDER");

            if (order.Taker != player.Name) 
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не выполняете данный заказ!", 5000);
                return;
            }
            

            player.ResetData("ORDER");

            order.Taker = "";
            order.SetSee(true);

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы отменили заказ!", 5000);

        }

        [RemoteEvent("takeorder")]
        public static void TakeOrder(Player player, string owner)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (string.IsNullOrEmpty(Main.Players[player].Org)) return;

            Order order = HaveOrder(owner);

            if (order == null || order.Taker != "" || order.See == false)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Такого заказа не существует!", 5000);
                return;
            }
            if (player.HasData("ORDER"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас уже имеется заказ. Для отмены /cancelorder!", 5000);
                return;
            }

            order.SetSee(false);
            order.Taker = player.Name;
            player.SetData("ORDER", order);
            player.SendChatMessage($"Вы взяли заказ от {owner} . Телефон для связи: {order.SIM} . Бизнес: {order.Bizness}");
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы взяли заказ!", 5000);
            foreach (Player ply in NAPI.Pools.GetAllPlayers())
                if (ply.Name == owner)
                {
                    Notify.Send(ply, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш заказ по доставке был взят {player.Name} | Тел. {Main.Players[player].Sim}!", 5000);
                    break;
                }
        }

        [RemoteEvent("addorder")]
        public static void AddOrder(Player player, string ids, string materialss)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (Main.Players[player].BizIDs.Count == 0) return;

            try
            {
                Convert.ToInt32(ids);
                Convert.ToInt32(materialss);
            }
            catch
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                return;
            }

            int id = Convert.ToInt32(ids);
            int materials = Convert.ToInt32(materialss);

            if (!Main.Players[player].BizIDs.Contains(id))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Этот бизнес вам не принадлежит", 3000);
                return;
            }
            if (Main.Players[player].Sim == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет сим-карты", 3000);
                return;
            }
            if (HaveOrder(player.Name) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас уже имеется заказ", 5000);
                return;
            }
            OrderList.Add(new Order(player.Name, id, materials, Main.Players[player].Sim));
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно сделали заказ Бизнес: {id} Материалы: {materials}", 5000);
        }

        [RemoteEvent("removeorder")]
        public static void RemoveOrder(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            Order order = HaveOrder(player.Name);
            if (order == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет заказа", 5000);
                return;
            }
            if (order.Taker != "")
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ваш заказ выполняется, нельзя отменить!", 5000);
                return;
            }
            order.Bizness = -1;
            order.Materials = -1;
            order.Owner = null;
            order.SIM = -1;
            order.SetSee(false);
            OrderList.Remove(order);

            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы отменили свой заказ", 5000);
        }


    }
}
