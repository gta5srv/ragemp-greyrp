using GTANetworkAPI;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;
using System;

namespace NeptuneEVO.Businesses
{
    class RefillI : Script
    {
        private static nLog Log = new nLog("REFILL");

        public static int CostForFuel = 10;
        public class Refill : BCore.Bizness
        {
            public Refill(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat) : base(id, owner, position, matposition, cost, mafia, bankid, mat)
            {
                Type = 1;
                Name = "Заправка";
                BlipColor = 50;
                BlipType = 361;
				BlipSize = 1;
                Range = 10f;

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
                OpenMenu(player);
                
            }

            public static void OpenMenu(Player player)
            {
                Trigger.PlayerEvent(player, "openPetrol", VehicleManager.VehicleTank[player.Vehicle.Class] - player.Vehicle.GetData<int>("PETROL"));
                Notify.Send(player, NotifyType.Info, NotifyPosition.TopCenter, $"Цена за литр: {CostForFuel}$", 7000);
            }

        }

        [RemoteEvent("petrol")]
        public static void fillCar(Player player, int lvl, bool card)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                Vehicle vehicle = player.Vehicle;
                if (vehicle == null) return; //check
                if (player.VehicleSeat != 0) return;
                if (lvl <= 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                    return;
                }
                if (!vehicle.HasSharedData("PETROL"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно заправить эту машину", 3000);
                    return;
                }
                if (Core.VehicleStreaming.GetEngineState(vehicle))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Чтобы начать заправляться - заглушите транспорт.", 3000);
                    return;
                }
                int fuel = vehicle.GetSharedData<int>("PETROL");
                if (fuel >= VehicleManager.VehicleTank[vehicle.Class])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У транспорта полный бак", 3000);
                    return;
                }

                var isGov = false;
                if (lvl == 9999)
                    lvl = VehicleManager.VehicleTank[vehicle.Class] - fuel;
                else if (lvl == 99999)
                {
                    isGov = true;
                    lvl = VehicleManager.VehicleTank[vehicle.Class] - fuel;
                }

                if (lvl < 0) return;

                int tfuel = fuel + lvl;
                if (tfuel > VehicleManager.VehicleTank[vehicle.Class])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                    return;
                }
                if (isGov)
                {
                    int frac = Main.Players[player].FractionID;
                    if (Fractions.Manager.FractionTypes[frac] != 2)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Чтобы заправить транспорт за гос. счет, Вы должны состоять в гос. организации", 3000);
                        return;
                    }
                    if (!vehicle.HasData("ACCESS") || vehicle.GetData<string>("ACCESS") != "FRACTION" || vehicle.GetData<int>("FRACTION") != frac)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете заправить за государственный счет не государственный транспорт", 3000);
                        return;
                    }
                    if (Fractions.Stocks.fracStocks[frac].FuelLeft < lvl * CostForFuel)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Лимит на заправку гос. транспорта за день исчерпан", 3000);
                        return;
                    }
                }
                else
                {
                    if (!card)
                    {
                        if (Main.Players[player].Money < lvl * CostForFuel)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств (не хватает {lvl * CostForFuel - Main.Players[player].Money}$)", 3000);
                            return;
                        }
                    }
                    else if (MoneySystem.Bank.Accounts[Main.Players[player].Bank].Balance < lvl * CostForFuel)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств (не хватает {lvl * CostForFuel - MoneySystem.Bank.Accounts[Main.Players[player].Bank].Balance}$)", 3000);
                        return;
                    }
                }
                if (isGov)
                {
                    Fractions.Stocks.fracStocks[6].Money -= lvl * CostForFuel;
                    Fractions.Stocks.fracStocks[Main.Players[player].FractionID].FuelLeft -= lvl * CostForFuel;
                }
                else
                {
                    if (!card)
                        MoneySystem.Wallet.Change(player, -lvl * CostForFuel);
                    else
                        MoneySystem.Bank.Change(Main.Players[player].Bank, -lvl * CostForFuel);
                }

                vehicle.SetSharedData("PETROL", tfuel);

                if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "PERSONAL")
                {
                    var number = NAPI.Vehicle.GetVehicleNumberPlate(vehicle);
                    VehicleManager.Vehicles[number].Fuel += lvl;
                }
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Транспорт заправлен", 3000);
                Commands.RPChat("me", player, $"заправил(а) транспортное средство");
            }
            catch (Exception e) { Log.Write("Petrol: " + e.Message, nLog.Type.Error); }
        }

    }
}
