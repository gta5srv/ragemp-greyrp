using GTANetworkAPI;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using NeptuneEVO.MoneySystem;
using System;
using System.Linq;


namespace NeptuneEVO.Businesses
{
    class ClothesShopI : Script
    {
        private static nLog Log = new nLog("CLOTHES");

        public class ClothesShop : BCore.Bizness
        {
            public static int CostForClothes = 100;

            public ClothesShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 7;
                Name = "Магазин одежды";
                BlipColor = 4;
                BlipType = 73;
                Range = 2f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;

                //if ((player.GetData("ON_DUTY") && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2) || player.GetData("ON_WORK"))
                //{
                    // Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны закончить рабочий день", 3000);
                    // return;
                    // }
                    Trigger.PlayerEvent(player, "openClothes", CostForClothes);
                    player.PlayAnimation("amb@world_human_guard_patrol@male@base", "base", 1);
                    NAPI.Entity.SetEntityDimension(player, Dimensions.RequestPrivateDimension(player));
                }

                public static void Buy(Player player, int type, int variation, int texture)
                {
                    try
                    {

                        var tempPrice = 0;
                        switch (type)
                        {
                            case 0:
                                tempPrice = Customization.Hats[Main.Players[player].Gender].FirstOrDefault(h => h.Variation == variation).Price;
                                break;
                            case 1:
                                tempPrice = Customization.Tops[Main.Players[player].Gender].FirstOrDefault(t => t.Variation == variation).Price;
                                break;
                            case 2:
                                tempPrice = Customization.Underwears[Main.Players[player].Gender].FirstOrDefault(h => h.Value.Top == variation).Value.Price;
                                break;
                            case 3:
                                tempPrice = Customization.Legs[Main.Players[player].Gender].FirstOrDefault(l => l.Variation == variation).Price;
                                break;
                            case 4:
                                tempPrice = Customization.Feets[Main.Players[player].Gender].FirstOrDefault(f => f.Variation == variation).Price;
                                break;
                            case 5:
                                tempPrice = Customization.Gloves[Main.Players[player].Gender].FirstOrDefault(f => f.Variation == variation).Price;
                                break;
                            case 6:
                                tempPrice = Customization.Accessories[Main.Players[player].Gender].FirstOrDefault(f => f.Variation == variation).Price;
                                break;
                            case 7:
                                tempPrice = Customization.Glasses[Main.Players[player].Gender].FirstOrDefault(f => f.Variation == variation).Price;
                                break;
                            case 8:
                                tempPrice = Customization.Jewerly[Main.Players[player].Gender].FirstOrDefault(f => f.Variation == variation).Price;
                                break;
							case 9:
                                tempPrice = Customization.Bag[Main.Players[player].Gender].FirstOrDefault(l => l.Variation == variation).Price;
                                break;
                        }

                        var price = Convert.ToInt32((tempPrice / 100.0) * ClothesShop.CostForClothes);

                        var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Top));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно места в инвентаре", 3000);
                            return;
                        }
                        if (Main.Players[player].Money < price)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                            return;
                        }

                        var amount = Convert.ToInt32(price * 0.75 / 50);
                        if (amount <= 0) amount = 1;
                        //GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", price, "buyClothes");
                        MoneySystem.Wallet.Change(player, -price);

                        switch (type)
                        {
                            case 0:
                                Customization.AddClothes(player, ItemType.Hat, variation, texture);
                                break;
                            case 1:
                                Customization.AddClothes(player, ItemType.Top, variation, texture);
                                break;
                            case 2:
                                var id = Customization.Underwears[Main.Players[player].Gender].FirstOrDefault(u => u.Value.Top == variation);
                                Customization.AddClothes(player, ItemType.Undershit, id.Key, texture);
                                break;
                            case 3:
                                Customization.AddClothes(player, ItemType.Leg, variation, texture);
                                break;
                            case 4:
                                Customization.AddClothes(player, ItemType.Feet, variation, texture);
                                break;
                            case 5:
                                Customization.AddClothes(player, ItemType.Gloves, variation, texture);
                                break;
                            case 6:
                                Customization.AddClothes(player, ItemType.Accessories, variation, texture);
                                break;
                            case 7:
                                Customization.AddClothes(player, ItemType.Glasses, variation, texture);
                                break;
                            case 8:
                                Customization.AddClothes(player, ItemType.Jewelry, variation, texture);
                                break;
							case 9:
                                Customization.AddClothes(player, ItemType.Bag, variation, texture);
                                break;	
                        }

                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы купили новую одежду. Она была добавлена в Ваш инвентарь.", 3000);
                    }
                    catch (Exception e) { Log.Write($"BuyClothes:  [var: {variation} | tex: {texture}]" + e.ToString(), nLog.Type.Error); }
                }

                public static void Cancel(Player player)
                {
                    try
                    {
                        player.StopAnimation();
                        Customization.ApplyCharacter(player);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        Dimensions.DismissPrivateDimension(player);
                    }
                    catch (Exception e) { Log.Write("cancelClothes: " + e.Message, nLog.Type.Error); }
                }

            }

            // REMOTE EVENTS //

            [RemoteEvent("cancelClothes")]
            public static void Cancel(Player player)
            {
                try
                {
                    ClothesShop.Cancel(player);
                }
                catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
            }

            [RemoteEvent("buyClothes")]
            public static void Buy(Player player, int type, int variation, int texture)
            {
                try
                {
                    ClothesShop.Buy(player, type, variation, texture);
                }
                catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
            }
        }

    } 
