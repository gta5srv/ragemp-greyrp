using GTANetworkAPI;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;
using NeptuneEVO.MoneySystem;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace NeptuneEVO.Businesses
{
    class BarberShopI : Script
    {
        private static nLog Log = new nLog("BARBER");
        public class BarberShop : BCore.Bizness
        {

            public static int CostForBarber = 50;

            public BarberShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 10;
                Name = "Парикмахерская";
                BlipColor = 4;
                BlipType = 71;
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
                Main.Players[player].ExteriorPos = player.Position;
                var dim = Dimensions.RequestPrivateDimension(player);
                NAPI.Entity.SetEntityDimension(player, dim);
                NAPI.Entity.SetEntityPosition(player, new Vector3(324.9798, 180.6418, 103.6665));
                player.Rotation = new Vector3(0, 0, 101.0228);
                player.PlayAnimation("amb@world_human_guard_patrol@male@base", "base", 1);
                Customization.ClearClothes(player, Main.Players[player].Gender);

                Trigger.PlayerEvent(player, "openBody", true, CostForBarber);
            }

            public static void Buy(Player player, string id, int style, int color)
            {
                if ((id == "lipstick" || id == "blush") && Main.Players[player].Gender && style != 255)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Доступно только для персонажей женского пола", 3000);
                    return;
                }

                var prod = CostForBarber;
                double price;
                if (id == "hair")
                {
                    if (style >= 23) price = BarberPrices[id][23] / 100.0 * prod;
                    else price = (style == 255) ? BarberPrices[id][0] / 100.0 * prod : BarberPrices[id][style] / 100.0 * prod;
                }
                else price = (style == 255) ? BarberPrices[id][0] / 100.0 * prod : BarberPrices[id][style] / 100.0 * prod;
                if (Main.Players[player].Money < Convert.ToInt32(price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }
                //GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", Convert.ToInt32(price), "buyBarber");
                MoneySystem.Wallet.Change(player, -Convert.ToInt32(price));

                switch (id)
                {
                    case "hair":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Hair = new HairData(style, color, color);
                        break;
                    case "beard":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[1].Value = style;
                        Customization.CustomPlayerData[Main.Players[player].UUID].BeardColor = color;
                        break;
                    case "eyebrows":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[2].Value = style;
                        Customization.CustomPlayerData[Main.Players[player].UUID].EyebrowColor = color;
                        break;
                    case "chesthair":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[10].Value = style;
                        Customization.CustomPlayerData[Main.Players[player].UUID].ChestHairColor = color;
                        break;
                    case "lenses":
                        Customization.CustomPlayerData[Main.Players[player].UUID].EyeColor = style;
                        break;
                    case "lipstick":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[8].Value = style;
                        Customization.CustomPlayerData[Main.Players[player].UUID].LipstickColor = color;
                        break;
                    case "blush":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[5].Value = style;
                        Customization.CustomPlayerData[Main.Players[player].UUID].BlushColor = color;
                        break;
                    case "makeup":
                        Customization.CustomPlayerData[Main.Players[player].UUID].Appearance[4].Value = style;
                        break;
                }
				
				Utils.QuestsManager.AddQuestProcess(player, 10);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы оплатили услугу Барбер-Шопа ({Convert.ToInt32(price)}$)", 3000);
            }

        }

        [RemoteEvent("buyBarber")]
        public static void Buy(Player player, string id, int style, int color)
        {
            try
            {
                BarberShopI.BarberShop.Buy(player, id, style, color);
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }

        public static Dictionary<string, List<int>> BarberPrices = new Dictionary<string, List<int>>()
        {
            { "hair", new List<int>() {
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
				1500,
            }},
            { "beard", new List<int>() {
                120,
                120,
                120,
                120,
                120,
                160,
                160,
                160,
                120,
                120,
                240,
                240,
                120,
                120,
                240,
                200,
                120,
                160,
                380,
                360,
                360,
                180,
                180,
                260,
                120,
                120,
                240,
                200,
                120,
                160,
                380,
                360,
                360,
                180,
                180,
                260,
                120,
                180,
                180,
            }},
            { "eyebrows", new List<int>() {
                100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100
            }},
            { "chesthair", new List<int>() {
                100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100,100
            }},
            { "lenses", new List<int>() {
                200,
                400,
                400,
                200,
                200,
                400,
                200,
                400,
                1000,
                1000,
            }},
            { "lipstick", new List<int>() {
                200,
                400,
                400,
                200,
                200,
                400,
                200,
                400,
                1000,
                300,
            }},
            { "blush", new List<int>() {
                200,
                400,
                400,
                200,
                200,
                400,
                200,
            }},
            { "makeup", new List<int>() {
                120,
                120,
                120,
                120,
                120,
                160,
                160,
                160,
                120,
                120,
                240,
                240,
                120,
                120,
                240,
                200,
                120,
                160,
                380,
                360,
                360,
                180,
                180,
                260,
                120,
                120,
                240,
                200,
                120,
                160,
                380,
                360,
                360,
                180,
                180,
                260,
                120,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
                180,
            }},
        };

    }
}
