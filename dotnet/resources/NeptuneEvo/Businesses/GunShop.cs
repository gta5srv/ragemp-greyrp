using GTANetworkAPI;
using System.Collections.Generic;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;
using NeptuneEVO.MoneySystem;
using System;
using Newtonsoft.Json;

namespace NeptuneEVO.Businesses
{
    class GunShopI : Script
    {
        private static nLog Log = new nLog("GUNSHOP");

        public class GunShop : BCore.Bizness
        {
            public static int CostForAmmo = 7;

            static new readonly Dictionary<string, int> Products = new Dictionary<string, int>
            {
                {"Pistol", 3000 },
                {"CombatPistol", 15000 },
				{"HeavyPistol", 25000},
				{"DoubleAction", 150000},
                {"Revolver", 20000},
                {"BullpupShotgun", 10000},
                {"CombatPDW", 30000},
                {"MachinePistol", 27500},
				{"Hammer", 150},
				{"Crowbar", 200},
				{"GolfClub", 300},
				{"Hatchet", 800},
				{"BattleAxe", 1000},
				{"Bat", 350},
				{"KnuckleDuster", 600},
				{"Knife", 2500},
            };

            public GunShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 6;
                Name = "Магазин оружия";
                BlipColor = 4;
                BlipType = 110;
                Range = 2f;

                SetProducts(Products);
                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;

                List<List<int>> prices = new List<List<int>>();

                for (int i = 0; i < 3; i++)
                {
                    List<int> p = new List<int>();
                    foreach (string g in Products.Keys)
                    {
                        if (gunsCat[i].Contains(g))
                            p.Add(Products[g]);
                    }
                    prices.Add(p);
                }

                var ammoPrice = CostForAmmo;
                prices.Add(new List<int>());
                foreach (var ammo in AmmoPrices)
                    prices[3].Add(Convert.ToInt32(ammo));

                string json = JsonConvert.SerializeObject(prices);
                Log.Debug(json);
                Trigger.PlayerEvent(player, "openWShop", 50, json);
            }

            public static void BuyAmmo(Player player, string text1, string text2)
            {
                try
                {
                    var category = Convert.ToInt32(text1.Replace("wbuyslider", null));
                    var needMoney = Math.Abs(Convert.ToInt32(text2.Trim('$')));
                    var ammo = needMoney / AmmoPrices[category];

                    if (!Main.Players[player].Licenses[6])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет лицензии на оружие", 3000);
                        return;
                    }

                    if (ammo == 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не указали количество патрон", 3000);
                        return;
                    }

                    var tryAdd = nInventory.TryAdd(player, new nItem(AmmoTypes[category], ammo));
                    if (tryAdd == -1 || tryAdd > 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                        return;
                    }

                    var totalPrice = ammo * Convert.ToInt32(AmmoPrices[category]);

                    if (Main.Players[player].Money < totalPrice)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                        return;
                    }

                    MoneySystem.Wallet.Change(player, -totalPrice);
                    //GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", totalPrice, $"buyWShop(ammo)");
                    nInventory.Add(player, new nItem(AmmoTypes[category], ammo));
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили {nInventory.ItemsNames[(int)AmmoTypes[category]]} x{ammo} за {totalPrice}$", 3000);
                }
                catch (Exception e) { Log.Write("BuyWeapons: " + e.Message, nLog.Type.Error); }
            }
            public static void Buy(Player player, int cat, int index)
            {
                try
                {
                    var prodName = gunsCat[cat][index];
                    if (!Main.Players[player].Licenses[6])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет лицензии на оружие", 3000);
                        return;
                    }
                    int cost = Products[prodName];

                    if (Main.Players[player].Money < cost)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                        return;
                    }

                    ItemType wType = (ItemType)Enum.Parse(typeof(ItemType), prodName);

                    var tryAdd = nInventory.TryAdd(player, new nItem(wType));
                    if (tryAdd == -1 || tryAdd > 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                        return;
                    }

                    Random rnd = new Random();
                    MoneySystem.Wallet.Change(player, -cost);
                    //GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", cost, $"buyWShop({prodName})");
                    Weapons.GiveWeapon(player, wType, GetRandomSerial());

                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили {prodName} за {cost}$", 3000);
                }
                catch (Exception e) { Log.Write("BuyWeapons: " + e.Message, nLog.Type.Error); }
            }

            public static string GetRandomSerial()
            {
                Random rnd = new Random();
                string str = rnd.Next(1,9).ToString();
                
                for(int i = 0; i < 9; i++)
                {
                    str += rnd.Next(0, 9);
                }
                return str;
            }

        }

        // REMOTE EVENTS //

        [RemoteEvent("wshopammo")]
        public static void BuyAmmo(Player player, string text1, string text2)
        {
            try
            {
                GunShopI.GunShop.BuyAmmo(player, text1, text2);
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }

        [RemoteEvent("wshop")]
        public static void Buy(Player player, int cat, int index)
        {
            try
            {
                 GunShopI.GunShop.Buy(player, cat, index);
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }

        private static List<int> AmmoPrices = new List<int>()
        {
            1, // pistol 
            3, // smg 
            4, // rifles 
            5, // sniperrifles 
            4, // shotguns 
        };
        private static List<ItemType> AmmoTypes = new List<ItemType>()
        {
            ItemType.PistolAmmo, // pistol
            ItemType.SMGAmmo, // smg
            ItemType.RiflesAmmo, // rifles
            ItemType.SniperAmmo, // sniperrifles
            ItemType.ShotgunsAmmo, // shotguns
        };

        private static List<List<string>> gunsCat = new List<List<string>>()
        {
            new List<string>()
            {
                "Pistol",
                "CombatPistol",
                "HeavyPistol",
				"DoubleAction",
				"Revolver",
				"BullpupShotgun",
				"CombatPDW",
                "MachinePistol",
				"Hammer",
				"Crowbar",
				"GolfClub",
				"Hatchet",
				"BattleAxe",
				"Bat",
				"KnuckleDuster",
				"Knife",
            },
            new List<string>()
            {
					
			},
            new List<string>()
            {  },
        };

    }
}
