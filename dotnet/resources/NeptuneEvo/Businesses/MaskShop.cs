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
    class MaskShopI : Script
    {
        public static int CostForMask = 300;

        private static nLog Log = new nLog("MASKSHOP");
        public class MaskShop : BCore.Bizness
        {

            public MaskShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 11;
                Name = "Магазин масок";
                BlipColor = 4;
                BlipType = 362;
                Range = 2f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;

                // if ((player.GetData("ON_DUTY") && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2) || player.GetData("ON_WORK"))
                // {
                    // Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны закончить рабочий день", 3000);
                    // return;
                // }
                Trigger.PlayerEvent(player, "openMasks", CostForMask);
                player.PlayAnimation("amb@world_human_guard_patrol@male@base", "base", 1);
                Customization.ApplyMaskFace(player);
            }

            public static void Buy(Player player, int variation, int texture)
            {
                var tempPrice = Customization.Masks.FirstOrDefault(f => f.Variation == variation).Price;

                var price = Convert.ToInt32((tempPrice / 100.0) * CostForMask);

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

                //GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", price, "buyMask");
                MoneySystem.Wallet.Change(player, -price);

                Utils.QuestsManager.AddQuestProcess(player, 2);

                Customization.AddClothes(player, ItemType.Mask, variation, texture);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы купили новую маску. Она была добавлена в Ваш инвентарь.", 3000);
            }

            public static void Close(Player player)
            {
                player.StopAnimation();
                Customization.ApplyCharacter(player);
                Customization.SetMask(player, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Texture);
            }

        }

        [RemoteEvent("cancelMasks")]
        public static void RemoteEvent_cancelMasks(Player player)
        {
            try
            {
                MaskShopI.MaskShop.Close(player);
            }
            catch (Exception e) { Log.Write("cancelMasks: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("buyMasks")]
        public static void RemoteEvent_buyMasks(Player player, int variation, int texture)
        {
            try
            {
                MaskShopI.MaskShop.Buy(player, variation, texture);
            }
            catch (Exception e) { Log.Write("buyMasks: " + e.Message, nLog.Type.Error); }
        }

    }
}
