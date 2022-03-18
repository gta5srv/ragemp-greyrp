using GTANetworkAPI;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;


namespace NeptuneEVO.Businesses
{
    class CarRepairI
    {
        public class CarRepair : BCore.Bizness
        {
            public static int CostForRepair = 1500;

            public CarRepair(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 15;
                Name = "Ремонт Авто";
                BlipColor = 4;
                BlipType = 544;
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
                Trigger.PlayerEvent(player, "openDialog", "REMONT_AVTO", $"Вы хотите отремонтировать машину за {CostForRepair}?");
            }

            public static void Buy(Player player)
            {
                if (!player.IsInVehicle || player.IsInVehicle && player.VehicleSeat != 0) return;
                /*if (player.Vehicle.Health <= 1000)
                {
                    Notify.Send(player, NotifyType.Alert, NotifyPosition.BottomCenter, "Ваш транспорт не повреждён.", 3000);
                }*/
                if (Main.Players[player].Money < CostForRepair)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }
                //GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", CostForRepair, "carrepair");
                MoneySystem.Wallet.Change(player, -CostForRepair);

                NAPI.Vehicle.RepairVehicle(NAPI.Player.GetPlayerVehicle(player));
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Ваш транспорт был отремонтирован.", 3000);
            }

        }
    }
}
