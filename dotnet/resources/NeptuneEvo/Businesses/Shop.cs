using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;

namespace NeptuneEVO.Businesses
{
    class ShopI : Script
    {

        private static nLog Log = new nLog("SHOP");

        public static List<Dictionary<string, int>> ProductsList = new List<Dictionary<string, int>>
        {
            new Dictionary<string, int> {
                { "Канистра", 300 },
                { "Бинт", 1000 },
                { "Аптечки", 3500 },
                { "Таблетки", 2500 },
                { "Шприц адреналина", 5000 },
                { "Связка ключей", 250 },
                { "Рем. комплект", 1700 },
                { "Фонарик", 240 },
                { "Ключ", 210 },
                { "Сим-карта", 200 },
                { "Чипсы", 60 },
                { "Пицца", 200 },
				{ "Бургер", 150 },
                { "Хот-Дог", 150 },
                { "Сэндвич", 80 },
                { "eCola", 90 },
                { "Sprunk", 80 }
				//{ "Сумка", 21 }
            },
            new Dictionary<string, int> {
                { "Бургер", 150 },
                { "Хот-Дог", 150 },
                { "Сэндвич", 80 },
                { "Чипсы", 60 },
                { "Пицца", 200 },
                { "eCola", 90 },
                { "Sprunk", 80 }
            },
            new Dictionary<string, int> {
                { "Сим-карта", 200 }
            },
            new Dictionary<string, int> {
                { "Удочка", 3500 },
                { "Улучшенная удочка", 7000 },
                { "Удочка MK2", 15000 },
                { "Наживка", 150 }
            },
            new Dictionary<string, int> {
                {"Корюшка",52},
                {"Кунджа",58},
                {"Лосось",50},
                {"Окунь",25},
                {"Осётр",30},
                {"Скат",63},
                {"Тунец",75},
                {"Угорь",25},
                {"Чёрный амур",53},
                {"Щука",32}
            },
            new Dictionary<string, int> {
                { "Martini Asti", 3700 },
                { "Sambuca", 700 },
                { "Водка с лимоном", 350 },
                { "Водка на бруснике", 350 },
                { "Русский стандарт", 500 },
                { "Коньяк Дживан", 400 },
                { "Коньяк Арарат", 350 },
                { "Пиво разливное", 250 },
                { "Пиво бутылочное", 200 },
                { "Кальян", 950 }
            },
            new Dictionary<string, int> {
                { "Канистра", 300 },
                { "Связка ключей", 250 },
                { "Рем. комплект", 1700 },
                { "Фонарик", 240 },
                { "Ключ", 210 }
            },
            new Dictionary<string, int> {
                { "Бинт", 1000 },
                { "Аптечки", 3500 },
                { "Таблетки", 2500 },
                { "Шприц адреналина", 5000 }
            },
            new Dictionary<string, int> {
                { "Бургер", 300 },
                { "Хот-Дог", 400 },
                { "Сэндвич", 400 },
                { "Чипсы", 200 },
                { "Пицца", 200 },
                { "eCola", 250 },
                { "Sprunk", 250 },
                { "Martini Asti", 3500 },
                { "Sambuca", 700 },
                { "Водка с лимоном", 540 },
                { "Водка на бруснике", 540 },
                { "Русский стандарт", 540 },
                { "Коньяк Дживан", 600 },
                { "Коньяк Арарат", 600 },
                { "Пиво разливное", 450 },
                { "Пиво бутылочное", 450 },
                { "Кальян", 2500 }
            },
            new Dictionary<string, int> {
                { "Семена картофеля", 200 },
                { "Семена моркови", 350 },
                { "Семена пшеницы", 300 },
                { "Семена клевера", 400 }
            }
        };

        public class Semena : BCore.Bizness
        {
            public Semena(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 26;
                Name = "Магазин семян";
                BlipColor = 66;
                BlipType = 468;
                Range = 1f;

                SetProducts(ProductsList[9]);
                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                SimpleInteract(player, 9);
            }
        }

        public class Club : BCore.Bizness
        {
            public Club(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 24;
                Name = "Club";
                BlipColor = 4;
                BlipType = 136;
				BlipSize = 0.5f;
                Range = 1.7f;

                SetProducts(ProductsList[7]);
                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                SimpleInteract(player, 8);
            }
        }
        public class HealShop : BCore.Bizness
        {
            public HealShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 21;
                Name = "Аптека";
                BlipColor = 1;
                BlipType = 51;
				BlipSize = 0.5f;
                Range = 1f;

                SetProducts(ProductsList[7]);
                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                SimpleInteract(player, 7);
            }
        }
        public class AutoShop : BCore.Bizness
        {
            public AutoShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 20;
                Name = "Авто магазин";
                BlipColor = 1;
                BlipType = 434;
                BlipSize = 0.5f;
                Range = 1f;

                SetProducts(ProductsList[6]);
                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                SimpleInteract(player, 6);
            }
        }
        public class BarShop : BCore.Bizness
        {
            public BarShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 19;
                Name = "Бар";
                BlipColor = 4;
                BlipType = 206;
                Range = 1f;

                SetProducts(ProductsList[5]);
                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                SimpleInteract(player, 5);
            }
        }
        public class SellShop : BCore.Bizness
        {
            public SellShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 18;
                Name = "Скупка рыбы";
                BlipColor = 3;
                BlipType = 628;
                Range = 1f;

                SetProducts(ProductsList[4]);
                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                SimpleInteract(player, 4, true);
            }
        }
        public class FishShop : BCore.Bizness
        {
            public FishShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 17;
                Name = "Рыболовный магазин";
                BlipColor = 3;
                BlipType = 356;
                Range = 1f;

                SetProducts(ProductsList[3]);
                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                SimpleInteract(player, 3);
            }
        }

        public class SimShop : BCore.Bizness
        {
            public SimShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 16;
                Name = "Магазин Sim-карт";
                BlipColor = 15;
                BlipSize = 0.5f;
                BlipType = 682;
                Range = 1f;

                SetProducts(ProductsList[2]);
                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                SimpleInteract(player, 2);
            }
        }

        public class EatShop : BCore.Bizness
        {
            public EatShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 8;
                Name = "Закусочная";
                BlipColor = 70;
                BlipSize = 0.5f;
                BlipType = 277;
                Range = 1f;

                SetProducts(ProductsList[1]);
                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                SimpleInteract(player, 1);
            }
        }

        public class Shop : BCore.Bizness
        {
            public Shop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 0;
                Name = "24/7";
                BlipColor = 4;
                BlipType = 52;
                Range = 1f;

                SetProducts(ProductsList[0]);
                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                SimpleInteract(player, 0);
            }
        }

        // REMOTE EVENTS //

        [RemoteEvent("shop")]
        public static void Buy(Player player, int index)
        {
            try
            {
                if (!player.HasData("IDS")) return;
                    Buys(player, index);
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }

        [RemoteEvent("fishshop")]
        public static void Sell(Player player, int index)
        {
            try
            {
                if (!player.HasData("IDS")) return;
                if (!Main.Players.ContainsKey(player)) return;
                Dictionary<string, int> Products = ProductsList[player.GetData<int>("IDS")];
                string product = BCore.Bizness.GetProductByIndex(Products, index);

                var aItem = nInventory.Find(Main.Players[player].UUID, BCore.Bizness.GetItemByName(product));
                if (aItem == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет {product}", 3000);
                    return;
                }

                var prices = Products[product] * Main.pluscost;

                nInventory.Remove(player, BCore.Bizness.GetItemByName(product), 1);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали {product}", 3000);
                MoneySystem.Wallet.Change(player, +prices);
                //GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", prices, $"sellShop");
            }
            catch (Exception e) { Log.Write($"SellShop: {e.ToString()}\n{e.StackTrace}", nLog.Type.Error); }
        }

        // OFF TOP //

        public static void SimpleInteract(Player player, int index, bool sell=false)
        {
            List<List<string>> items = new List<List<string>>();
            string json;
            if (sell)
            {
                foreach (KeyValuePair<string, int> p in ProductsList[index])
                {
                    List<string> item = new List<string>();
                    item.Add(p.Key);
                    item.Add($"{p.Value * Main.pluscost}$");
                    items.Add(item);
                }
                json = JsonConvert.SerializeObject(items);
                player.SetData("IDS", index);
                Trigger.PlayerEvent(player, "fishshop", json);
                return;
            }
            foreach (KeyValuePair<string, int> p in ProductsList[index])
            {
                List<string> item = new List<string>();
                item.Add(p.Key);
                item.Add($"{p.Value}$");
                items.Add(item);
            }
            json = JsonConvert.SerializeObject(items);
            player.SetData("IDS", index);
            Trigger.PlayerEvent(player, "shop", json);
        }

        public static void Buys(Player player, int index)
        {
            if (!Main.Players.ContainsKey(player)) return;
            Dictionary<string, int> Products = ProductsList[player.GetData<int>("IDS")];
            string product = BCore.Bizness.GetProductByIndex(Products, index);
            if (product == "") return;
            int cost = Products[product];
            if (Main.Players[player].Money < cost)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                return;
            }
            ItemType itemproduct = BCore.Bizness.GetItemByName(product);
            var tryAdd = nInventory.TryAdd(player, new nItem(itemproduct));
            if (tryAdd == -1 || tryAdd > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Ваш инвентарь больше не может вместить {product}", 3000);
                return;
            }
            if (product == "Сим-карта")
            {
                if (Main.Players[player].Sim != -1) Main.SimCards.Remove(Main.Players[player].Sim);
                Main.Players[player].Sim = Main.GenerateSimcard(Main.Players[player].UUID);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили сим-карту с номером {Main.Players[player].Sim}", 3000);
                GUI.Dashboard.sendStats(player);
            }
            else
            {
                nItem item = (itemproduct == ItemType.KeyRing) ? new nItem(ItemType.KeyRing, 1, "") : new nItem(itemproduct);

                if (product == "Сумка")
                    Customization.AddClothes(player, ItemType.Bag, 82, 0);
                else
                    nInventory.Add(player, item);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили {product}", 3000);
            }
            MoneySystem.Wallet.Change(player, -cost);
            //GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", cost, $"buyShop");
        }


    }
}
