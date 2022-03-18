using GTANetworkAPI;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;


namespace NeptuneEVO.Businesses
{
    // НЕ ЗАБЫТЬ ПРО ДИАЛОГ CARWASH_PAY
    class CarWashI
    {
        public class CarWash : BCore.Bizness
        {
            public static int CostForWash = 1500;

            public CarWash(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 13;
                Name = "Авто мойка";
                BlipColor = 4;
                BlipType = 100;
                Range = 4f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;

                if (!player.IsInVehicle || player.IsInVehicle && player.VehicleSeat != 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в машине", 3000);
                    return;
                }
                Trigger.PlayerEvent(player, "openDialog", "CARWASH_PAY", $"Вы хотите помыть машину за {CostForWash}?");
            }

            public static void Buy(Player player)
            {
                if (!player.IsInVehicle || player.IsInVehicle && player.VehicleSeat != 0) return;
                /*if (VehicleStreaming.GetVehicleDirt(player.Vehicle) >= 0.01f)
                {
                    Notify.Send(player, NotifyType.Alert, NotifyPosition.BottomCenter, "Ваш транспорт не грязный.", 3000);
                }*/
                if (Main.Players[player].Money < CostForWash)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }
                //GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", CostForWash, "carwash");
                MoneySystem.Wallet.Change(player, -CostForWash);

                VehicleStreaming.SetVehicleDirt(player.Vehicle, 0.0f);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Ваш транспорт был помыт.", 3000);
            }

        }
    }
}
