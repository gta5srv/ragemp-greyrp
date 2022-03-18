using GTANetworkAPI;
using System;
using NeptuneEVO.GUI;
using NeptuneEVO.Houses;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;
using Golemo.Families;
using NeptuneEVO;

namespace Golemo.Families
{
    public class Components : Script
    {

        static int AddComp = 500; // Сколько добавлять компонентов

        static int CompWarTimer = 0;
        static int ToWar = 0;
        static int[] Times = new int[] { 30, 10, 5, 1 };
        static string Timer = null;
        //static bool WarIsGoing = false;

        static bool ComponentsExits = false;
        static GTANetworkAPI.Object ComponentObject = null;
        static GTANetworkAPI.Marker ComponentMarker1 = null;
        static GTANetworkAPI.Marker ComponentMarker2 = null;
        static Vector3 ComponentPosition = new Vector3(999.7778, -1976.607, 29.95);

        public static void LoadComp()
        {
            try
            {
                NAPI.Blip.CreateBlip(478, ComponentPosition, 0.7f, Convert.ToByte(5), Main.StringToU16("Война за компоненты"), 255, 0, true);
                var shape = NAPI.ColShape.CreateCylinderColShape(ComponentPosition, 1.5f, 3, 0);
                shape.OnEntityEnterColShape += (s, ent) =>
                {
                    try
                    {
                        if (ComponentsExits && Main.Players.ContainsKey(ent) && Main.Players[ent].FamilyCID != null && ComponentObject != null)
                        {
                            Family fam = Family.GetFamilyToCid(ent);

                            if (fam == null) return;

                            ComponentsExits = false;

                            NAPI.Task.Run(() => { try { ComponentObject.Delete(); ComponentObject = null; ComponentMarker1.Delete(); ComponentMarker1 = null; ComponentMarker2.Delete(); ComponentMarker2 = null; } catch { } });

                            fam.AddComponents(AddComp);

                            foreach (Player ply in NAPI.Pools.GetAllPlayers())
                                if (Main.Players.ContainsKey(ply) && Main.Players[ply].FamilyCID != null)
                                {
                                    Notify.Send(ply, NotifyType.Info, NotifyPosition.BottomCenter, $"Внимание! Компоненты были захвачены одной из семей. Компоненты зачислены на их счёт", 3000);
                                }
                            //WarIsGoing = false;
                            Timers.Stop(Timer);
                            Timer = null;


                        }
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                };

            }
            catch { }
        }

        public static void StartToWar()
        {
            try
            {
                ToWar = 60; // Минут

                Timer = Timers.Start(60000, () => OnToTimer() );

                foreach (Player ply in NAPI.Pools.GetAllPlayers())
                    if (Main.Players.ContainsKey(ply) && Main.Players[ply].FamilyCID != null && Main.Players[ply].FamilyCID != "null")
                    {
                        Notify.Send(ply, NotifyType.Info, NotifyPosition.BottomCenter, "Внимание! Через 1 час будет начата война за компоненты", 3000);
                    }
            }
            catch { }
        }

        [Command("startwar")]
        static void StartWar(Player player)
        {
            try
            {
                if (Main.Players[player].AdminLVL < 9) return;

                //WarIsGoing = true;
                CompWarTimer = 4;
                Timer = Timers.Start(5000, () => { OnWarTimer(); });
            }
            catch { }
        }

        private static void OnWarTimer()
        {
            NAPI.Task.Run(() => { 
            try
            {
                CompWarTimer -= 1;

                if (CompWarTimer == 3)
                {
                    foreach (Player ply in NAPI.Pools.GetAllPlayers())
                        if (Main.Players.ContainsKey(ply) && Main.Players[ply].FamilyCID != null)
                        {
                            Notify.Send(ply, NotifyType.Info, NotifyPosition.BottomCenter, $"Внимание! В центре завода появились компоненты, заберите их", 3000);
                        }
                    ComponentsExits = true;
                    ComponentObject = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_champ_box_01"), ComponentPosition, new Vector3(0, 0, 0), 255);

                    ComponentMarker1 = NAPI.Marker.CreateMarker(1, ComponentPosition - new Vector3(0, 0, 0.5f), new Vector3(), new Vector3(), 0.965f, new Color(0, 175, 250, 220), false, 0);
                    ComponentMarker2 = NAPI.Marker.CreateMarker(27, ComponentPosition + new Vector3(0, 0, 0.5f), new Vector3(), new Vector3(), 1f, new Color(0, 175, 250, 220), false, 0);


                    }

                if (CompWarTimer <= 0)
                {
                    foreach (Player ply in NAPI.Pools.GetAllPlayers())
                        if (Main.Players.ContainsKey(ply) && Main.Players[ply].FamilyCID != null && Main.Players[ply].FamilyCID != "null")
                        {
                            Notify.Send(ply, NotifyType.Info, NotifyPosition.BottomCenter, $"Внимание! Война за компоненты закончена", 3000);
                        }
                    //WarIsGoing = false;
                    Timers.Stop(Timer);
                    Timer = null;

                        NAPI.Task.Run(() => { try { ComponentObject.Delete(); ComponentObject = null; ComponentMarker1.Delete(); ComponentMarker1 = null; ComponentMarker2.Delete(); ComponentMarker2 = null; } catch { } });
                    }
                


            }
            catch { return; }
            });
        }

        private static void OnToTimer()
        {
            try
            {
                ToWar -= 1;

                if (Times.Contains(ToWar))
                {
                    foreach (Player ply in NAPI.Pools.GetAllPlayers())
                        if (Main.Players.ContainsKey(ply) && Main.Players[ply].FamilyCID != null && Main.Players[ply].FamilyCID != "null")
                        {
                            Notify.Send(ply, NotifyType.Info, NotifyPosition.BottomCenter, $"Внимание! Через {ToWar} минут{(ToWar == 1 ? "у" : "")} будет начата война за компоненты", 3000);
                        }
                }

                if (ToWar <= 0)
                {
                    Timers.Stop(Timer);
                    //WarIsGoing = true;
                    CompWarTimer = 10;
                    Timer = Timers.Start(60000, () => { OnWarTimer(); });

                    foreach (Player ply in NAPI.Pools.GetAllPlayers())
                        if (Main.Players.ContainsKey(ply) && Main.Players[ply].FamilyCID != null && Main.Players[ply].FamilyCID != "null")
                        {
                            Notify.Send(ply, NotifyType.Info, NotifyPosition.BottomCenter, $"Внимание! Начата война за компоненты, ожидайте их в открытом боксе через 7 минут войны. У Вас есть 3 минуты чтобы заполучить компоненты", 3000);
                        }

                }


            }
            catch { return; }
        }

    }
}
