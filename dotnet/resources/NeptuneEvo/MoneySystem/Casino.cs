using GTANetworkAPI;
using System;
using System.Collections.Generic;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using System.Linq;
using NeptuneEVO.GUI;

namespace NeptuneEVO.MoneySystem
{
    class Casino : Script
    {
        private static nLog Log = new nLog("Casino");

        private static ColShape colShape;

        // 1 - Деньги
        // 2 - UP
        // 3 - Дополнительный прокрут
        // 4 - VIP
        // 5 - Маска
        // 6 - EXP
        // 7 - Машина

        static bool Rolling = false;
        static new readonly List<int> GetType = new List<int> { 
            6, 
            1, 
            7, 
            4, 
            6, 
            5, // 1/4
            1, 
            4, 
            6, 
            2, 
            5, // 2/4
            4, 
            6, 
            5, 
            1, 
            4, // 3/4
            3, 
            5, 
            1, 
            4 
        };
        static List<string> Names = new List<string> { "Деньги", "UP", "Дополнительный прокрут", "VIP", "Маска", "EXP", "Машина" };
        static int Index = 0;

        static Random rnd = new Random();
        static string Car = "xc90";

        public static void OnResourceStart()
        {
           // NAPI.TextLabel.CreateTextLabel(Main.StringToU16("Казино"), new Vector3(935.4563, 46.268734, 79.97579 + 1.5), 3f, 0.5F, 0, new Color(255, 255, 255), true, 0);
            NAPI.Marker.CreateMarker(27, new Vector3(935.4563, 46.268734, 79.97579) + new Vector3(0, 0, 0.2f), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220), false, 0);
            NAPI.Blip.CreateBlip(679, new Vector3(923.31537, 47.571545, 79.98634 + 1.5), 1f, Convert.ToByte(4), Main.StringToU16("Diamond Casino"), 255, 0, true);
            ColShape enter = NAPI.ColShape.CreateCylinderColShape(new Vector3(935.4563, 46.268734, 79.97579), 2f, 5, 0);
            enter.OnEntityEnterColShape += (ColShape shape, Player Player) =>
            {
                NAPI.Data.SetEntityData(Player, "INTERACTIONCHECK", 519);
            };
            enter.OnEntityExitColShape += (ColShape shape, Player Player) =>
            {
                NAPI.Data.SetEntityData(Player, "INTERACTIONCHECK", 0);
            }; 

            NAPI.Marker.CreateMarker(27, new Vector3(1089.7303, 206.7629, -50.119724) + new Vector3(0, 0, 0.2f), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220), false, 1);
            ColShape exit = NAPI.ColShape.CreateCylinderColShape(new Vector3(1089.7303, 206.7629, -50.119724), 2f, 5, 1);
            exit.OnEntityEnterColShape += (ColShape shape, Player Player) =>
            {
                NAPI.Data.SetEntityData(Player, "INTERACTIONCHECK", 520);
            };
            exit.OnEntityExitColShape += (ColShape shape, Player Player) =>
            {
                NAPI.Data.SetEntityData(Player, "INTERACTIONCHECK", 0);
            };


            colShape = NAPI.ColShape.CreateCylinderColShape(new Vector3(1111.05, 229.81, -49.15), 3f, 2);
            colShape.OnEntityEnterColShape += (ColShape shape, Player Player) =>
            {
                Notify.Send(Player, NotifyType.Info, NotifyPosition.Bottom, $"Нажмите [E] чтобы крутить", 3000);
                NAPI.Data.SetEntityData(Player, "INTERACTIONCHECK", 517);
            };
            colShape.OnEntityExitColShape += (ColShape shape, Player Player) =>
            {
                NAPI.Data.SetEntityData(Player, "INTERACTIONCHECK", 0);
            };
        }

        public static void Roll(Player player)
        {
            try {
                if (!Main.Players.ContainsKey(player)) return;
                if (Rolling)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.Bottom, $"Подождите, колесо кто-то крутит!", 3000);
                    return;
                }

                if (Main.Players[player].LuckyWheell == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.Bottom, $"У вас нет прокрутов, приходите позже", 3000);
                    return;
                }
                Main.Players[player].LuckyWheell -= 1;
                Dashboard.sendStats(player);

                Rolling = true;
                Index = rnd.Next(0, 19);
                if (Index == 2)
                    if (rnd.Next(0, 10) != 7)
                        Index = 3;
                if (Index == 9)
                    if (rnd.Next(0, 5) != 2)
                        Index = 10;

                player.SetData("ROOLING", true);
                Main.OnAntiAnim(player);
                Trigger.PlayerEventInRange(player.Position, 500f, "casino.luckywheel.roll", player.Value, Index);
                NAPI.Task.Run(() => { try { if (player != null) Finish(player); } catch (Exception e) { Log.Write("ROLL: " + e.ToString(), nLog.Type.Error); } }, 9000);
            }
            catch (Exception e) { Log.Write("ROLL: " + e.ToString(), nLog.Type.Error); }
        }


        public static void Finish(Player player)
        {
            try
            {
                if (player == null) return;
                Rolling = false;

                switch (GetType[Index])
                {
                    case 1:
                        int money = rnd.Next(10000, 50000);
                        MoneySystem.Wallet.Change(player, money);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.Bottom, $"Поздравляем, Вы выиграли {money}$", 3000);
                        break;
                    case 2:
                        int donate = rnd.Next(50, 200);
                        MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{donate} where `login`='{Main.Accounts[player].Login}'");
                        Main.Accounts[player].RedBucks += donate;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.Bottom, $"Поздравляем, Вы выиграли {donate}UP", 3000);
                        break;
                    case 3:
                        Main.Players[player].LuckyWheell += 1;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.Bottom, $"Поздравляем, Вы выиграли ещё один прокрут!", 3000);
                        GUI.Dashboard.sendStats(player);
                        break;
                    case 4:
                        GiveVIP(player, rnd.Next(1, 3));
                        break;
                    case 5:
                        GiveMask(player);
                        break;
                    case 6:
                        GiveEXP(player);
                        break;
                    case 7:
                        GiveCar(player);
                        break;
                    default:
                        break;
                
                }


                //Notify.Send(player, NotifyType.Success, NotifyPosition.Bottom, "Вы выиграли: " + name == "Машина" ? Car : name + " !", 3000);

                Main.OffAntiAnim(player);
                player.ResetData("ROOLING");
            }
            catch (Exception e) { Log.Write("FINISH: " + e.ToString(), nLog.Type.Error); }
        }

        public static void GiveMask(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Top));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно места в инвентаре", 3000);
                    return;
                }
                Customization.AddClothes(player, ItemType.Mask, rnd.Next(1,20), 0);
                Notify.Send(player, NotifyType.Success, NotifyPosition.Bottom, $"Поздравляем, Вы выиграли маску!", 3000);
                GUI.Dashboard.sendStats(player);
            }
            catch { }
        }

        public static void GiveVIP(Player player, int lvl)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Accounts[player].VipLvl > lvl)
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.Bottom, "Поздравляем, Вы выиграли VIP статус, но у он уже есть у Вас, поэтому ловите ещё один прокрут!", 3000);
                    Main.Players[player].LuckyWheell += 1;
                    GUI.Dashboard.sendStats(player);
                    return;
                }
                if (Main.Accounts[player].VipLvl == lvl)
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.Bottom, "Поздравляем, Вы выиграли VIP статус, но у он уже есть у Вас, поэтому ловите ещё один прокрут!", 3000);
                    Main.Players[player].LuckyWheell += 1;
                    GUI.Dashboard.sendStats(player);
                    return;
                }
                Main.Accounts[player].VipLvl = lvl;
                Main.Accounts[player].VipDate = DateTime.Now.AddDays(3);
                Notify.Send(player, NotifyType.Info, NotifyPosition.Bottom, $"Поздравляем, Вы выиграли VIP({Group.GroupNames[lvl]}) статус на 3 дня", 3000);
                GUI.Dashboard.sendStats(player);
            }
            catch { }
        }

        public static void GiveEXP(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Поздравляем, Вы выиграли 1 EXP!", 3000);
                Main.Players[player].EXP += 1 * Group.GroupEXP[Main.Accounts[player].VipLvl] * Main.oldconfig.ExpMultiplier;
                if (Main.Players[player].EXP >= 3 + Main.Players[player].LVL * 3)
                {
                    Main.Players[player].EXP = Main.Players[player].EXP - (3 + Main.Players[player].LVL * 3);
                    Main.Players[player].LVL += 1;
                    if (Main.Players[player].LVL == 1)
                    {
                        NAPI.Task.Run(() => { try { Trigger.PlayerEvent(player, "disabledmg", false); } catch { } }, 5000);
                    }
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Поздравляем, у Вас новый уровень ({Main.Players[player].LVL})!", 3000);
                    //донат за уровень
                    Core.nAccount.Account acc = Main.Accounts[player];
                    switch (Main.Players[player].LVL)
                    {
                        case 5:
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 50UP за достижение 5 уровня", 3000);
                            MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{50} where `login`='{Main.Accounts[player].Login}'");
                            acc.RedBucks += 50;
                            break;
                        case 10:
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 75UP за достижение 10 уровня", 3000);

                            MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{75} where `login`='{Main.Accounts[player].Login}'");
                            acc.RedBucks += 75;
                            break;
                        case 15:
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 100UP за достижение 15 уровня", 3000);

                            MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{100} where `login`='{Main.Accounts[player].Login}'");
                            acc.RedBucks += 100;
                            break;
                        case 20:
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 150UP за достижение 20 уровня", 3000);

                            MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{150} where `login`='{Main.Accounts[player].Login}'");
                            acc.RedBucks += 150;
                            break;
                        case 25:
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 200UP за достижение 25 уровня", 3000);

                            MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{200} where `login`='{Main.Accounts[player].Login}'");
                            acc.RedBucks += 200;
                            break;
                        case 30:
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 250UP за достижение 30 уровня", 3000);

                            MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{250} where `login`='{Main.Accounts[player].Login}'");
                            acc.RedBucks += 250;
                            break;
                        case 35:
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 300UP за достижение 35 уровня", 3000);

                            MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{300} where `login`='{Main.Accounts[player].Login}'");
                            acc.RedBucks += 300;
                            break;
                        case 40:
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 400UP за достижение 40 уровня", 3000);

                            MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{400} where `login`='{Main.Accounts[player].Login}'");
                            acc.RedBucks += 400;
                            break;
						case 45:
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 400UP за достижение 45 уровня", 3000);

                            MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{400} where `login`='{Main.Accounts[player].Login}'");
                            acc.RedBucks += 400;
                            break;
						case 50:
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 400UP за достижение 50 уровня", 3000);

                            MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{400} where `login`='{Main.Accounts[player].Login}'");
                            acc.RedBucks += 400;
                            break;	
						case 60:
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 400UP за достижение 60 уровня", 3000);

                            MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{400} where `login`='{Main.Accounts[player].Login}'");
                            acc.RedBucks += 400;
                            break;	
						case 70:
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Поздравляем, Вы получили 400UP за достижение 70 уровня", 3000);

                            MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{400} where `login`='{Main.Accounts[player].Login}'");
                            acc.RedBucks += 400;
                            break;									
							
                    }
                    GUI.Dashboard.sendStats(player);
                }
            }
            catch (Exception e) { Log.Write("GIVEEXP: " + e.ToString(), nLog.Type.Error); }
        }

        public static void GiveCar(Player player)
        {
            NAPI.Task.Run(() => { 
                try
                {
                    if (!Main.Players.ContainsKey(player)) return;
                    VehicleManager.Create(player.Name, Car, new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0, 0));
                    Notify.Send(player, NotifyType.Success, NotifyPosition.Bottom, $"Поздравляем, Вы выиграли самый ценный приз, а именно машину {ParkManager.GetNormalName(Car)} !", 3000);
                }
                catch { }
            });
        }

    }

}
