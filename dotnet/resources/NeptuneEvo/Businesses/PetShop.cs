using GTANetworkAPI;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace NeptuneEVO.Businesses
{
    class PetShopI : Script
    {
        public static int CostForPet = 100;

        private static nLog Log = new nLog("PETSHOP");

        public static List<string> PetNames = new List<string>() {
            "Хаски",
            "Пудель",
            "Мопс",
            "Ретривер",
            "Ротвейлер",
            "Шеперд",
            "Вест-терьер",
            "Кошка",
            "Кролик",
        };

        public static List<int> PetHashes = new List<int>() {
            1318032802,
            1125994524,
            1832265812,
            882848737,
            -1788665315,
            1126154828,
            -1384627013,
            1462895032,
            -541762431,
        };
        public class PetShop : BCore.Bizness
        {

            

            public PetShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 14;
                Name = "Магазин питомцев";
                BlipColor = 4;
                BlipType = 273;
                Range = 2f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;

                Main.Players[player].ExteriorPos = player.Position;
                uint mydim = (uint)(player.Value + 500);
                NAPI.Entity.SetEntityDimension(player, mydim);
                NAPI.Entity.SetEntityPosition(player, new Vector3(-758.3929, 319.5044, 175.302));
                player.PlayAnimation("amb@world_human_sunbathe@male@back@base", "base", 39);
                player.SetData("INTERACTIONCHECK", 0);
                var prices = new List<int>();
                for (byte i = 0; i != 9; i++)
                {
                    prices.Add(CostForPet);
                }
                Trigger.PlayerEvent(player, "openPetshop", JsonConvert.SerializeObject(PetNames), JsonConvert.SerializeObject(PetHashes), JsonConvert.SerializeObject(prices), mydim);
            }

            public static void Buy(Player player, string petName)
            {
                player.StopAnimation();
                NAPI.Entity.SetEntityPosition(player, Main.Players[player].ExteriorPos);
                NAPI.Entity.SetEntityDimension(player, 0);
                Main.Players[player].ExteriorPos = new Vector3();
                Trigger.PlayerEvent(player, "destroyCamera");
                Dimensions.DismissPrivateDimension(player);

                Houses.House house = Houses.HouseManager.GetHouse(player, true);
                if (house == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет личного дома", 3000);
                    return;
                }
                if (Houses.HouseManager.HouseTypeList[house.Type].PetPosition == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Ваше место проживания не подходит для жизни петомцев", 3000);
                    return;
                }
                if (Main.Players[player].Money < CostForPet)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }
                MoneySystem.Wallet.Change(player, -CostForPet);
                //GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", CostForPet, $"buyPet({petName})");
                house.PetName = petName;
                Main.Players[player].PetName = petName;
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Теперь Вы являетесь счастливым хозяином {petName}!", 3000);
            }

            public static void Close(Player player)
            {
                player.StopAnimation();
                NAPI.Entity.SetEntityDimension(player, 0);
                NAPI.Entity.SetEntityPosition(player, Main.Players[player].ExteriorPos);
                Main.Players[player].ExteriorPos = new Vector3();
                Dimensions.DismissPrivateDimension(player);
                Trigger.PlayerEvent(player, "destroyCamera");
            }

        }

        [RemoteEvent("petshopBuy")]
        public static void Buy(Player player, string petname)
        {
            try
            {
                PetShopI.PetShop.Buy(player, petname);
            }
            catch (Exception e) { Log.Write("buyPet: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("petshopCancel")]
        public static void Cancel(Player player)
        {
            try
            {
                PetShopI.PetShop.Close(player);
            }
            catch (Exception e) { Log.Write("cancelPet: " + e.Message, nLog.Type.Error); }
        }

    }
}
