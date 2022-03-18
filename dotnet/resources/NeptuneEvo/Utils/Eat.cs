using GTANetworkAPI;
using System;
using System.Linq;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Core
{
    class EatManager : Script
    {

        private static nLog Log = new nLog("EatManager");

        [ServerEvent(Event.ResourceStart)]

        public void onResourceStart()
        {
            Timers.StartTask("checkwater", 180000, () => CheckWater());
            Timers.StartTask("checkeat", 300000, () => CheckEat());
        }

        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Player player, Player killer, uint reason)
        {
            //SetEat(player, 40);
            //SetWater(player, 40);
        }

        public static void SetEat(Player player, int change)
        {
            try
            {
                Main.Players[player].Eat = change;
                MySQL.Query($"UPDATE characters SET eat={Main.Players[player].Eat} WHERE uuid={Main.Players[player].UUID}");
                GUI.Dashboard.sendStats(player);
                Trigger.PlayerEvent(player, "UpdateEat", Main.Players[player].Eat, Convert.ToString(change));
            }
            catch { Log.Write("ERROR SET EAT", nLog.Type.Error); }
        }
        public static void AddEat(Player player, int change)
        {
            if (Main.Players[player].Eat + change > 100)
            {
                Main.Players[player].Eat = 100;
            }
            else if (change < 0 && Main.Players[player].Eat + change < 0)
            {
                Main.Players[player].Eat = 0;
            }
            else
            {
                Main.Players[player].Eat += change;
            }
            MySQL.Query($"UPDATE characters SET eat={Main.Players[player].Eat} WHERE uuid={Main.Players[player].UUID}");
            Trigger.PlayerEvent(player, "UpdateEat", Main.Players[player].Eat, Convert.ToString(change));
            GUI.Dashboard.sendStats(player);
        }
        public static void SetWater(Player player, int change)
        {
            try
            {
                Main.Players[player].Water = change;
                MySQL.Query($"UPDATE characters SET water={Main.Players[player].Water} WHERE uuid={Main.Players[player].UUID}");
                Trigger.PlayerEvent(player, "UpdateWater", Main.Players[player].Water, Convert.ToString(change));
                GUI.Dashboard.sendStats(player);
            }
            catch { Log.Write("ERROR SET WATER", nLog.Type.Error); }
        }
        public static void AddWater(Player player, int change)
        {
            if (Main.Players[player].Water + change > 100)
            {
                Main.Players[player].Water = 100;
            }
            else if (change < 0 && Main.Players[player].Water + change < 0)
            {
                Main.Players[player].Water = 0;
            }
            else
            {
                Main.Players[player].Water += change;
            }
            MySQL.Query($"UPDATE characters SET water={Main.Players[player].Water} WHERE uuid={Main.Players[player].UUID}");
            Trigger.PlayerEvent(player, "UpdateWater", Main.Players[player].Water, Convert.ToString(change));
            GUI.Dashboard.sendStats(player);
        }

        public static void CheckEat()
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    foreach (Player player in NAPI.Pools.GetAllPlayers())
                    {

                        if (!Main.Players.ContainsKey(player)) continue;
                        if (player.HasSharedData("AGM") && player.GetSharedData<bool>("AGM")) continue;
                        if (player.Health > 0)
                        {
                            var rnd = new Random();
                            int intrnd = rnd.Next(2, 5);
                            if (Main.Players[player].Eat > 0 && Main.Players[player].Eat - intrnd > 0)
                            {
                                if (player.IsInVehicle)
                                {
                                    AddEat(player, -1);
                                }
                                else
                                {
                                    AddEat(player, -intrnd);
                                }
                            }
                            else if (Main.Players[player].Eat - intrnd < 0)
                            {
                                SetEat(player, 0);
                            }
                            if (Main.Players[player].Eat == 0 && player.Health >= 10)
                            {
                                player.Health -= 5;
                            }
                            else if (Main.Players[player].Water == 0 && Main.Players[player].Eat == 0)
                            {
                                player.Health -= 8;
                            }
                            if (Main.Players[player].Eat >= 80 && Main.Players[player].Water >= 80)
                            {
                                if (player.Health + 2 > 100)
                                {
                                    player.Health = 100;
                                }
                                else
                                {
                                    player.Health += 2;
                                }
                            }
                        }


                    }
                }
                catch { }
            });
        }
        public static void CheckWater()
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    foreach (Player player in NAPI.Pools.GetAllPlayers())
                    {
                        if (!Main.Players.ContainsKey(player)) continue;
                        if (player.HasSharedData("AGM") && player.GetSharedData<bool>("AGM")) continue;

                        if (player.Health > 0)
                        {
                            if (Main.Players[player].Water > 0 && Main.Players[player].Water - 2 >= 0)
                            {
                                if (player.IsInVehicle)
                                {
                                    AddWater(player, -1);
                                }
                                else
                                {
                                    AddWater(player, -2);
                                }
                            }
                            else if (Main.Players[player].Water - 2 < 0)
                            {
                                SetWater(player, 0);
                            }
                            if (Main.Players[player].Water == 0 && player.Health >= 10)
                            {
                                player.Health -= 2;
                            }
                        }
                    }

                }
                catch { }
            });
        }
    }
}
