using GTANetworkAPI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using NeptuneEVO.GUI;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using Newtonsoft.Json;
using NeptuneEVO.SDK;
using NeptuneEVO.Core.Character;
using NeptuneEVO.MoneySystem;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using NeptuneEVO.Houses;
using System.Diagnostics.Contracts;
using NeptuneEVO.Businesses;
using NeptuneEVO.Organization;

namespace NeptuneEVO.Core
{
    class Commands : Script
    {
		public static int Kartofel = 0;
		public static int Psenica = 0;
		public static int Morkov = 0;
		public static int Milk = 0;
	
        private static nLog Log = new nLog("Commands");
        private static Random rnd = new Random();
        
        #region Chat logic

        public static void SendToAdmins(ushort minLVL, string message)
        {
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].AdminLVL >= minLVL)
                {
                    p.SendChatMessage(message);
                }
            }
        }

        private static string RainbowExploit(Player sender, string message)
        {
            if (message.Contains("!{"))
            {
                foreach (var p in NAPI.Pools.GetAllPlayers())
                {
                    if (!Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].AdminLVL >= 1)
                    {
                        p.SendChatMessage($"~y~[CHAT-EXPLOIT] {sender.Name} ({sender.Value}) - {message}");
                    }
                }
                return Regex.Replace(message, "!", string.Empty);
            }
            return message;
        }

        [ServerEvent(Event.ChatMessage)]
        public void API_onChatMessage(Player sender, string message)
        {
            try
            {
                if (!Main.Players.ContainsKey(sender)) return;
                if (Main.Players[sender].Unmute > 0)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[sender].Unmute/60} минут", 3000);
                    return;
                }
                
                //await Log.WriteAsync($"<{sender.Name.ToString()}> {message}", nLog.Type.Info);

                message = RainbowExploit(sender, message);
                //message = Main.BlockSymbols(message);
                List<Player> players = Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension);
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        int[] id = new int[] { sender.Value };
                        foreach (Player c in players)
                        {
                            Trigger.PlayerEvent(c, "sendRPMessage", "chat", "{name}: " + message, id);
                        }

                        if (!sender.HasData("PhoneVoip")) return;
                        Voice.VoicePhoneMetaData phoneMeta = sender.GetData<Voice.VoicePhoneMetaData>("PhoneVoip");
                        if (phoneMeta.CallingState == "talk" && Main.Players.ContainsKey(phoneMeta.Target))
                        {
                            var pSim = Main.Players[sender].Sim;
                            var contactName = (Main.Players[phoneMeta.Target].Contacts.ContainsKey(pSim)) ? Main.Players[phoneMeta.Target].Contacts[pSim] : pSim.ToString();
                            phoneMeta.Target.SendChatMessage($"[В телефоне] {contactName}: {message}");
                        }
                    }
                    catch (Exception e) { Log.Write("ChatMessage_TaskRun: " + e.Message, nLog.Type.Error); }
                });
                return;
            }
            catch (Exception e) { Log.Write("ChatMessage: " + e.Message, nLog.Type.Error); }
        }
        #endregion Chat logic

        /*[Command("setvip")]
        public static void CMD_setvip(Player player, int type, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < 9) return;
                Player target = Main.GetPlayerByID(id);
                if (target == null) return;
                if (!Main.Accounts.ContainsKey(target)) return;
                Notify.Send(target, NotifyType.Success, NotifyPosition.CenterRight, $"Теперь ваш уровень ВИП > {type}", 5000);
                Main.Accounts[target].VipLvl = type;
            }
            catch (Exception e) { Log.Write("FREEBIZ: " + e.ToString(), nLog.Type.Error); }
        }*/

        /*[RemoteEvent("sc")]
        public static void CMD_sc(Player player)
        {
            Vector3 pos = NAPI.Entity.GetEntityPosition(player);
            pos.Z -= 1.12f;

            StreamWriter saveCoords = new StreamWriter("vectors.txt", true, Encoding.UTF8);
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            saveCoords.Write($"new Vector3({pos.X}, {pos.Y}, {pos.Z}),\r\n");

            saveCoords.Close();
        }*/

        [Command("jailk")]
        public static void CMD_jailk(Player player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].FractionID != 7 && Main.Players[player].FractionID != 9) return;
                Player target = Main.GetPlayerByID(id);
                if (target == null || target == player)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Не существует данного гражданина", 3000);
                    return;
                }
                if (target.Position.DistanceTo2D(player.Position) > 10)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин слишком далеко", 3000);
                    return;
                }
                if (target.Position.DistanceTo2D(new Vector3(1696.3231, 2588.7766, 44.79148)) > 20)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин слишком далеко от тюремной стойки", 3000);
                    return;
                }
                if (Main.Players[target].States.Count == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У гражданина нет статей", 3000);
                    return;
                }

                int time = 0;

                    time += Main.Players[target].States[0] * 1;
                    time += Main.Players[target].States[1] * 1;


                Main.Players[target].States[0] = 0;
                Main.Players[target].States[1] = 0;

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы отправили гражданина в спец. тюрьму!", 3000);
                Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"Вас отправили в спец. тюрьму на {time*60} мин.", 3000);

                Main.Players[target].ArrestTime = 0;
                Main.Players[target].DemorganTime = time*60;
                Fractions.FractionCommands.unCuffPlayer(target);

                NAPI.Entity.SetEntityPosition(target, Admin.DemorganPosition + new Vector3(0, 0, 1.5));
                if (target.HasData("ARREST_TIMER")) Timers.Stop(target.GetData<string>("ARREST_TIMER"));
                NAPI.Data.SetEntityData(target, "ARREST_TIMER", Timers.StartTask(1000, () => Admin.timer_demorgan(target)));
                Timers.StartTask(60000, () => Admin.timer_text(target));
                NAPI.Entity.SetEntityDimension(target, 1337);
                Weapons.RemoveAll(target, true);

            }
            catch { }
        }

        [Command("unjailk")]
        public static void CMD_unjailk(Player player, int id, int money)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].FractionID != 13 ) return;
                Player target = Main.GetPlayerByID(id);
                if (Main.Players[player].FractionLVL < 6)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно прав", 3000);
                    return;
                }
                if (target == null || target == player)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Не существует данного гражданина", 3000);
                    return;
                }
                if (target.Position.DistanceTo2D(player.Position) > 10)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин слишком далеко", 3000);
                    return;
                }
                if (Main.Players[target].States.Count == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У гражданина нет статей", 3000);
                    return;
                }
                if (money > 15000) //изменил уже
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Цена не должна быть выше 15 000$", 3000);
                    return;
                }

                target.SetData("UNJAILK", player);
                target.SetData("MONEY", money);

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили снятие статей за {money}$", 3000);

                Trigger.PlayerEvent(target, "openDialog", "UNJAILK", $"Вам предлагают убрать ваши статьи за {money}$");

            }
            catch { }
        }

        [Command("setgun")]
        public static void CMD_givegun(Player player, int hash)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL > 4)
                    Trigger.PlayerEvent(player, "wgive", (int)hash, 1000, false, true);
            }
            catch { }
        }

        [Command("deletelogin")]
        public static void CMD_deletelogin(Player player, string login)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < 5) return;

                if (Main.Accounts.ContainsKey(Main.LoggedIn[login]))
                    Main.Accounts.Remove(Main.LoggedIn[login]);
                if (Main.LoggedIn.ContainsKey(login))
                    Main.LoggedIn.Remove(login);

                player.SendChatMessage("Успешно убран аккаунт из авторизированных!");

            }
            catch { player.SendChatMessage("Не удалось убрать аккаунт!"); }
        }

        [Command("blackday")]
        public static void CMD_blackday(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (Main.Players[player].AdminLVL < 10) return;

            if (Main.BlackDay)
                Main.BlackDay = false;
            else
                Main.BlackDay = true;

            Trigger.PlayerEventToPlayers(NAPI.Pools.GetAllPlayers().ToArray(), "blackday", Main.BlackDay);
        }

        [Command("promik")]
        public static void CMD_delvip(Player player, string promo, string newpromo)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < 5) return;

                if (!Main.PromoCodes.ContainsKey(promo))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Не существует данного промокода", 3000);
                    return;
                }
                if (Main.PromoCodes.ContainsKey(newpromo))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Данное имя промокода занято!", 3000);
                    return;
                }

                Tuple<int, int, int> tuple = Main.PromoCodes[promo];
                Main.PromoCodes.Remove(promo);
                Main.PromoCodes.Add(newpromo, tuple);
                MySQL.Query($"UPDATE `accounts` SET `custompromo`='{newpromo}' WHERE `custompromo`='{promo}'");
                MySQL.Query($"UPDATE `promocodes` SET `name`='{newpromo}' WHERE `name`='{promo}'");

            }
            catch (Exception e) { Log.Write("DELVIP: " + e.ToString(), nLog.Type.Error); }
        }

        [Command("delvip")]
        public static void CMD_delvip(Player player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < 1) return;

                Player target = Main.GetPlayerByID(id);
                if (target == null || target == player)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Не существует данного гражданина", 3000);
                    return;
                }
                Main.Accounts[target].VipDate = DateTime.Now;
                Main.Accounts[target].VipLvl = 0;

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы убрали вип у {target.Name}", 3000);

            }
            catch (Exception e) { Log.Write("DELVIP: " + e.ToString(), nLog.Type.Error); }
        }

        [Command("givevb")]
        public static void CMD_givevb(Player player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].FractionID != 14) return;
                if (Main.Players[player].FractionLVL < Fractions.Configs.FractionRanks[14].Count - 3) return;
                Player target = Main.GetPlayerByID(id);
                if (target == null || target == player)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Не существует данного гражданина", 3000);
                    return;
                }
                if (target.Position.DistanceTo2D(player.Position) > 10)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин слишком далеко", 3000);
                    return;
                }
                for (int i = 0; i < 11 - Main.Players[target].Licenses.Count; i++)
                {
                    Main.Players[target].Licenses.Add(false);
                }
                if (Main.Players[target].Licenses[9])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин имеет военный билет", 3000);
                    return;
                }
                Main.Players[target].Licenses[9] = true;
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выдали {target.Name} военный билет", 3000);
                Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"Вам выдали военный билет", 3000);
            }
            catch (Exception e) { Log.Write("GIVEVB: " + e.ToString(), nLog.Type.Error); }
        }

        [Command("deletetest")]
        public static void CMD_deletetest(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < 3) return;
                foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                    if (veh.HasData("ACCESS") && veh.GetData<string>("ACCESS") == "TESTDRIVE")
                        NAPI.Task.Run(() => { try { veh.Delete(); } catch { } });
            }
            catch { }
        }

        [Command("advolic")]
        public static void CMD_advolic(Player player, int id)
        {
            try
            { 
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].FractionID != 6) return;
                if (Main.Players[player].FractionLVL < Fractions.Configs.FractionRanks[6].Count - 3) return;
                Player target = Main.GetPlayerByID(id);
                if (target == null || target == player)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Не существует данного гражданина", 3000);
                    return;
                }
                if (target.Position.DistanceTo2D(player.Position) > 10)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин слишком далеко", 3000);
                    return;
                }
                for (int i = 0; i < 11 - Main.Players[target].Licenses.Count; i++)
                {
                    Main.Players[target].Licenses.Add(false);
                }
                if (Main.Players[target].Licenses[10])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин имеет лицензию адвоката", 3000);
                    return;
                } 
                Main.Players[target].Licenses[10] = true;
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выдали {target.Name} лицензию адвоката", 3000);
                Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"Вам выдали лицензию адвоката", 3000);
            }
            catch (Exception e) { Log.Write("GIVEADVO: " + e.ToString(), nLog.Type.Error); }
        }

        [Command("deladvolic")]
        public static void CMD_deladvolic(Player player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].FractionID != 6) return;
                if (Main.Players[player].FractionLVL < Fractions.Configs.FractionRanks[6].Count - 3) return;
                Player target = Main.GetPlayerByID(id);
                if (target == null || target == player)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Не существует данного гражданина", 3000);
                    return;
                }
                if (target.Position.DistanceTo2D(player.Position) > 10)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин слишком далеко", 3000);
                    return;
                }
                for (int i = 0; i < 11 - Main.Players[target].Licenses.Count; i++)
                {
                    Main.Players[target].Licenses.Add(false);
                }
                if (!Main.Players[target].Licenses[10])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин не имеет лицензию адвоката", 3000);
                    return;
                }
                Main.Players[target].Licenses[10] = false;
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы изъяли у {target.Name} лицензию адвоката", 3000);
                Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"Вам изъяли лицензию адвоката", 3000);
            }
            catch (Exception e) { Log.Write("GIVEADVO: " + e.ToString(), nLog.Type.Error); }
        }

        [Command("setjobexp")]
        public static void CMD_setjobexp(Player player, int exp)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (Main.Players[player].AdminLVL < 9) return;
            Main.Players[player].WElectric = exp;
        }

        [Command("setfreebiz")]

        public static void CMD_setfreebiz(Player player, int on)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < 9) return;

                if (on == 1)
                {
                    Main.BizFree = true;
                    NAPI.Chat.SendChatMessageToAll("[BIZNESS] Сегодня всем бизнесам не будут приходить налоги!");
                }
                else if (on == 0)
                {
                    Main.BizFree = false;
                    NAPI.Chat.SendChatMessageToAll("[BIZNESS] Бизнесам опять приходят налоги!");
                }
            }
            catch (Exception e) { Log.Write("FREEBIZ: " + e.ToString(), nLog.Type.Error); }
        }

        [Command("setfreehouse")]

        public static void CMD_setfreehouse(Player player, int on)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < 9) return;

                if (on == 1)
                {
                    Main.HouseFree = true;
                    NAPI.Chat.SendChatMessageToAll("[HOUSE] Сегодня всем домам не будут приходить налоги!");
                }
                else if (on == 0)
                {
                    Main.HouseFree = false;
                    NAPI.Chat.SendChatMessageToAll("[HOUSE] Домам опять приходят налоги!");
                }
            }
            catch (Exception e) { Log.Write("FREEHOUSE: " + e.ToString(), nLog.Type.Error); }
        }

        [Command("setfreecomp")]

        public static void CMD_setfreecomp(Player player, int on)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < 9) return;

                if (on == 1)
                {
                    Main.CompanyFree = true;
                    NAPI.Chat.SendChatMessageToAll("[COMPANY] Сегодня всем компаниям не будут приходить налоги!");
                }
                else if (on == 0)
                {
                    Main.CompanyFree = false;
                    NAPI.Chat.SendChatMessageToAll("[COMPANY] Компаниям опять приходят налоги!");
                }
            }
            catch (Exception e) { Log.Write("FREECOMPANY: " + e.ToString(), nLog.Type.Error); }
        }


        [Command("bc")]
        public static void CMD_bc(Player player)
        {
            try
            {
                if (Main.Players[player].FractionID != 7) return;
                if (Main.Players[player].FractionLVL < 5) return;
                if (player.Dimension != 0) return;

                if (player.HasData("PDOBJECT"))
                {
                    var beacon = NAPI.Data.GetEntityData(player, "PDOBJECT");
                    try
                    {
                        NAPI.Entity.DeleteEntity(beacon);
                        NAPI.ColShape.DeleteColShape(player.GetData<ColShape>("PDOBJECTSHAPE"));
                        NAPI.Data.ResetEntityData(player, "PDOBJECT");
                    }
                    catch
                    {

                    }
                }
                else
                {
                    var beacon = NAPI.Object.CreateObject(10928689, player.Position - new Vector3(0, 0, 1.0), new Vector3(0, 0, 0), 255, 0);
                    beacon.SetData("BEACON", true);
                    var beaconShape = NAPI.ColShape.CreateCylinderColShape(player.Position - new Vector3(0, 0, 1.0), 1, 2, 0);
                    beaconShape.OnEntityEnterColShape += (s, e) =>
                    {
                        if (!Main.Players.ContainsKey(e)) return;
                        e.SetData("PDOBJECT", beacon);
                        e.SetData("PDOBJECTSHAPE", beaconShape);
                    };
                    beaconShape.OnEntityExitColShape += (s, e) =>
                    {
                        if (!Main.Players.ContainsKey(e)) return;
                        e.ResetData("PDOBJECT");
                    };
                }
            }
            catch
            {

            }
        }

        [Command("bcr")]
        public static void CMD_bcr(Player player)
        {
            try
            {
                if (Main.Players[player].FractionID != 7) return;
                if (Main.Players[player].FractionLVL < 5) return;
                if (player.Dimension != 0) return;

                foreach (GTANetworkAPI.Object obj in NAPI.Pools.GetAllObjects())
                {
                    if (player.Position.DistanceTo2D(obj.Position) < 5 && obj.HasData("BEACON"))
                    {
                        NAPI.Task.Run(() =>
                        {
                            obj.Delete();
                        });
                    }
                }
            }
            catch { }
        }

        [Command("newsrecord", GreedyArg = true)]
        public static void CMD_newsrecord(Player player, string text)
        {
            try
            {
                Fractions.LSNews.RecordOnRealTime(player, text);
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }

        [Command("fakepass")]
        public static void CMD_fakeuid(Player player, int id, string firstname, string lastname)
        {
            try
            {
                if (Main.Players[player].FractionID != 13) return; // сюда айди фракции
                if (Main.Players[player].FractionID < 6) // сюда уровень фракции
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Минимальный уровень для комманды 3", 3000);
                    return;
                }
                if (firstname == "" || lastname == "" || firstname == " " || lastname == " " )
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Ошибка в имени!", 3000);
                    return;
                }
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Player target = Main.GetPlayerByID(id);
                var pl = Main.GetNearestPlayer(player, 5);
                if (target == player) return;
                if (pl != target)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрока нет в радиусе 3 метров", 3000);
                    return;
                }
                if (target.HasData("FAKE_TIMER"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока уже есть фейковый номер!", 3000);
                    return;
                }

                target.SetData("FAKE_TIMER", Timers.StartTask(3600000, () => timer_endfakeuiid(target)));
                var fakeuid = Main.Players[target].FAKEGenerateUUID();
                Main.Players[target].FAKEUUID = fakeuid;
                Main.Players[target].FAKEFIRST = firstname;
                Main.Players[target].FAKELAST = lastname;
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы поменяли номер паспорта на {fakeuid}", 3000);
                Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш фейковый номер: {fakeuid}", 3000);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        public static void timer_endfakeuiid(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            Main.Players[player].FAKEUUID = -1;
            Main.Players[player].FAKEFIRST = null;
            Main.Players[player].FAKELAST = null;
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш фейковый номер паспорта был снят!", 3000);
            player.ResetData("FATE_TIMER");
        }


        #region AdminCommands
		
		[Command("createrod")]
        public static void CMD_createRod(Player player, float radius)
        {
            try
            {
                RodManager.createRodAreaCommand(player, radius);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("vehchange")]
        public static void CMD_vehchage(Player Player, string newmodel)
        {
            try
            {
                if (!Group.CanUseCmd(Player, "setvehdirt")) return;

                if (!Player.IsInVehicle) return;

                if (!Player.Vehicle.HasData("ACCESS"))
                    return;
                else if (Player.Vehicle.GetData<string>("ACCESS") == "PERSONAL")
                {
                    VehicleManager.Vehicles[Player.Vehicle.NumberPlate].Model = newmodel;
                    Notify.Send(Player, NotifyType.Warning, NotifyPosition.BottomCenter, "Машина будет доступна после респавна", 3000);
                }
                else if (Player.Vehicle.GetData<string>("ACCESS") == "WORK")
                    return;
                else if (Player.Vehicle.GetData<string>("ACCESS") == "FRACTION")
                    return;
                else if (Player.Vehicle.GetData<string>("ACCESS") == "GANGDELIVERY" || Player.Vehicle.GetData<string>("ACCESS") == "MAFIADELIVERY")
                    return;
            }
            catch { }
        }
		[Command("savefveh")]
        public static void CMD_savefveh(Player Player, string number, string model, int fraction, int rank)
        {
            try
            {
                if (!Group.CanUseCmd(Player, "savefveh")) return;

                if (Fractions.Configs.FractionVehicles[fraction].ContainsKey(number))
                {
                    Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Там уже есть фракционные машины с этим идентификатором", 3000);
                    return;
                }

                string pos = JsonConvert.SerializeObject(Player.Vehicle.Position + new Vector3(0, 0, 1));
                string rot = JsonConvert.SerializeObject(Player.Vehicle.Rotation);

                int color1 = Player.Vehicle.PrimaryColor;
                int color2 = Player.Vehicle.SecondaryColor;

                MySqlCommand queryCommand = new MySqlCommand(@"
                    INSERT INTO `fractionvehicles` (`fraction`, `number`, `model`, `position`, `rotation`, `rank`, `color1`, `color2`)
                    VALUES
                    (@FRACTION, @NUMBER, @MODEL, @POSITION, @ROTATION, @RANK, @COLORONE, @COLORTWO)
                ");

                queryCommand.Parameters.AddWithValue("@FRACTION", fraction);
                queryCommand.Parameters.AddWithValue("@NUMBER", number);
                queryCommand.Parameters.AddWithValue("@MODEL", model);
                queryCommand.Parameters.AddWithValue("@POSITION", pos);
                queryCommand.Parameters.AddWithValue("@ROTATION", rot);
                queryCommand.Parameters.AddWithValue("@RANK", rank);
                queryCommand.Parameters.AddWithValue("@COLORONE", color1);
                queryCommand.Parameters.AddWithValue("@COLORTWO", color2);

                MySQL.Query(queryCommand);

                Player.SendChatMessage($"{number} was saved");
            }
            catch { }
        }

        [Command("delfveh")]
        public static void CMD_deletefveh(Player Player, string number)
        {
            try
            {
                if (!Group.CanUseCmd(Player, "savefveh")) return;

                MySqlCommand queryCommand = new MySqlCommand(@"
                    DELETE FROM `fractionvehicles` WHERE `number` = @NUMBER
                ");

                queryCommand.Parameters.AddWithValue("@NUMBER", number);

                MySQL.Query(queryCommand);

                Player.SendChatMessage($"{number} was deleted");
            }
            catch { }
        }

        [Command("savejveh")]
        public static void CMD_savejveh(Player Player, string number, string model, int type)
        {
            try
            {
                if (!Group.CanUseCmd(Player, "savejveh")) return;

                string pos = JsonConvert.SerializeObject(Player.Vehicle.Position + new Vector3(0, 0, 1));
                string rot = JsonConvert.SerializeObject(Player.Vehicle.Rotation);

                int color1 = Player.Vehicle.PrimaryColor;
                int color2 = Player.Vehicle.SecondaryColor;

                MySqlCommand queryCommand = new MySqlCommand(@"
                    INSERT INTO `othervehicles` (`type`, `number`, `model`, `position`, `rotation`, `color1`, `color2`, `price`)
                    VALUES
                    (@TYPE, @NUMBER, @MODEL, @POSITION, @ROTATION, @COLORONE, @COLORTWO, @PRICE)
                ");

                queryCommand.Parameters.AddWithValue("@TYPE", type);
                queryCommand.Parameters.AddWithValue("@NUMBER", number);
                queryCommand.Parameters.AddWithValue("@MODEL", model);
                queryCommand.Parameters.AddWithValue("@POSITION", pos);
                queryCommand.Parameters.AddWithValue("@ROTATION", rot);
                queryCommand.Parameters.AddWithValue("@COLORONE", color1);
                queryCommand.Parameters.AddWithValue("@COLORTWO", color2);
                queryCommand.Parameters.AddWithValue("@PRICE", 0);

                MySQL.Query(queryCommand);
            }
            catch { }
        }

        [Command("deljveh")]
        public static void CMD_deletejveh(Player Player, string number)
        {
            try
            {
                if (!Group.CanUseCmd(Player, "savejveh")) return;

                MySqlCommand queryCommand = new MySqlCommand(@"
                    DELETE FROM `othervehicles`
                    WHERE `number` = @NUMBER
                ");

                queryCommand.Parameters.AddWithValue("@NUMBER", number);

                MySQL.Query(queryCommand);

                Player.SendChatMessage($"{number} was deleted");
            }
            catch { }
        }
        [Command("findtrailer")]
        public static void CMD_findTrailer(Player player) {
            try
            {
                if (player.HasData("TRAILER")) {
                    Vehicle trailer = player.GetData<Vehicle>("TRAILER");
                    Trigger.PlayerEvent(player, "createWaypoint", trailer.Position.X, trailer.Position.Y);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы успешно установили маркер на карту, там находится Ваш трейлер", 5000);
                }
            }
            catch { }
        }

        [Command("bankfix")]
        public static void CMD_bankfix(Player Player, int bank)
        {
            try
            {
                if (!Group.CanUseCmd(Player, "setvehdirt")) return;
                if(Bank.Accounts.ContainsKey(bank)) {
                    Bank.RemoveByID(bank);
                    Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно удалили банковский счёт номер {bank}", 3000);
                } else Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Банковский счёт {bank} не найден", 3000);
            }
            catch { }
        }

        [Command("gethwid")]
        public static void CMD_gethwid(Player Player, int ID)
        {
            try
            {
                if (!Group.CanUseCmd(Player, "setvehdirt")) return;
                Player target = Main.GetPlayerByID(ID);
                if (target == null)
                {
                    Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин с таким ID не найден", 3000);
                    return;
                }
                Player.SendChatMessage("Реальный HWID у " + target.Name + ": " + target.GetData<string>("RealHWID"));
            }
            catch { }
        }

        [Command("getsocialclub")]
        public static void CMD_getsc(Player Player, int ID)
        {
            try
            {
                if (!Group.CanUseCmd(Player, "setvehdirt")) return;
                Player target = Main.GetPlayerByID(ID);
                if (target == null)
                {
                    Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин с таким ID не найден", 3000);
                    return;
                }
                Player.SendChatMessage("Реальный SocialClub у " + target.Name + ": " + target.GetData<string>("RealSocialClub"));
            }
            catch { }
        }

        [Command("loggedinfix")]
        public static void CMD_loggedinfix(Player player, string login)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if(Main.Players[player].AdminLVL <= 6) return;
                if(Main.LoggedIn.ContainsKey(login)) {
                    if(NAPI.Player.IsPlayerConnected(Main.LoggedIn[login])) {
                        Main.LoggedIn[login].Kick();
                        Main.LoggedIn.Remove(login);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы кикнули персонажа с сервера, через минуту можно будет пытаться зайти в аккаунт.", 3000);
                    } else {
                        Main.LoggedIn.Remove(login);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Персонаж не был в сети, аккаунт удалён из списка авторизовавшихся.", 3000);
                    }
                } else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Аккаунта в сети с логином {login} не найдено", 3000);
            }
            catch { }
        }

        [Command("vconfigload")]
        public static void CMD_loadConfigVehicles(Player player, int type, int number)
        {
            try
            {
                if (!Group.CanUseCmd(player, "vconfigload")) return;
                if (type == 0) // fractionvehicles
                {
                    Fractions.Configs.FractionVehicles[number] = new Dictionary<string, Tuple<VehicleHash, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>>();
                    DataTable result = MySQL.QueryRead($"SELECT * FROM `fractionvehicles` WHERE `fraction`={number}");
                    if (result == null || result.Rows.Count == 0) return;
                    foreach (DataRow Row in result.Rows)
                    {
                        var fraction = Convert.ToInt32(Row["fraction"]);
                        var numberplate = Row["number"].ToString();
                        var model = NAPI.Util.VehicleNameToModel(Row["model"].ToString());
                        var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                        var rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());
                        var minrank = Convert.ToInt32(Row["rank"]);
                        var color1 = Convert.ToInt32(Row["colorprim"]);
                        var color2 = Convert.ToInt32(Row["colorsec"]);
                        VehicleManager.VehicleCustomization components = JsonConvert.DeserializeObject<VehicleManager.VehicleCustomization>(Row["components"].ToString());

                        Fractions.Configs.FractionVehicles[fraction].Add(numberplate, new Tuple<VehicleHash, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>(model, position, rotation, minrank, color1, color2, new VehicleManager.VehicleCustomization()));
                    }

                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            
                            foreach (var v in NAPI.Pools.GetAllVehicles())
                            {
                                if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "FRACTION" && v.GetData<int>("FRACTION") == number)
                                    v.Delete();
                            }
                            Fractions.Configs.SpawnFractionCars(number);
                        }
                        catch { }
                    });
                }
                else // othervehicles
                {
                    var result = MySQL.QueryRead($"SELECT * FROM `othervehicles` WHERE `type`={number}");
                    if (result == null || result.Rows.Count == 0) return;

                    switch (number)
                    {
                        case 0:
                            Rentcar.CarInfos = new List<CarInfo>();
                            break;
                        case 8:
                            Jobs.AutoMechanic.CarInfos = new List<CarInfo>();
                            break;

                    }

                    foreach (DataRow Row in result.Rows)
                    {
                        var numberPlate = Row["number"].ToString();
                        var model = NAPI.Util.VehicleNameToModel(Row["model"].ToString());
                        var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                        var rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());
                        var color1 = Convert.ToInt32(Row["color1"]);
                        var color2 = Convert.ToInt32(Row["color2"]);
                        var price = Convert.ToInt32(Row["price"]);
                        var data = new CarInfo(numberPlate, model, position, rotation, color1, color2, price);

                        switch (number)
                        {
                            case 0:
                                Rentcar.CarInfos.Add(data);
                                break;

                        }
                    }

                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            foreach (var v in NAPI.Pools.GetAllVehicles())
                            {
                                if (v.HasData("ACCESS") && ((v.GetData<string>("ACCESS") == "RENT" && number == 0) || (v.GetData<string>("ACCESS") == "WORK" && v.GetData<int>("WORK") == number)))
                                    v.Delete();
                            }
                            switch (number)
                            {
                                case 0:
                                    Rentcar.rentCarsSpawner();
                                    break;

                            }
                        }
                        catch { }
                    });
                }
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Машины зареспавнены", 3000);
            }
            catch (Exception e) { Log.Write("vconfigload: " + e.Message, nLog.Type.Error); }
        }
        [Command("giveammo")]
        public static void CMD_ammo(Player Player, int ID, int type, int amount = 1)
        {
            try
            {
                if (!Group.CanUseCmd(Player, "giveammo")) return;

                var target = Main.GetPlayerByID(ID);
                if (target == null)
                {
                    Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин с таким ID не найден", 3000);
                    return;
                }

                List<ItemType> types = new List<ItemType>
                {
                    ItemType.PistolAmmo,
                    ItemType.RiflesAmmo,
                    ItemType.ShotgunsAmmo,
                    ItemType.SMGAmmo,
                    ItemType.SniperAmmo
                };

                if (type > types.Count || type < -1) return;

                var tryAdd = nInventory.TryAdd(target, new nItem(types[type], amount));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно места в инвентаре", 3000);
                    return;
                }
                nInventory.Add(target, new nItem(types[type], amount));
            }
            catch { }
        }
        [Command("newvnum")]
        public static void CMD_newVehicleNumber(Player player, string oldNum, string newNum)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "newvnum")) return;
                if (!VehicleManager.Vehicles.ContainsKey(oldNum))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Такой машины не существует", 3000);
                    return;
                }

                if (VehicleManager.Vehicles.ContainsKey(newNum))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Такой номер уже существует", 3000);
                    return;
                }

                var vData = VehicleManager.Vehicles[oldNum];
                VehicleManager.Vehicles.Remove(oldNum);
                VehicleManager.Vehicles.Add(newNum, vData);

                var house = Houses.HouseManager.GetHouse(vData.Holder, true);
                if (house != null)
                {
                    var garage = Houses.GarageManager.Garages[house.GarageID];
                    garage.DeleteCar(oldNum);
                    garage.SpawnCar(newNum);
                }

                MySQL.Query($"UPDATE vehicles SET number='{newNum}' WHERE number='{oldNum}'");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Новый номер для {oldNum} = {newNum}", 3000);
            }
            catch (Exception e) { Log.Write("newvnum: " + e.Message, nLog.Type.Error); }
        }

        [Command("twist")]
        public static void CMD_twist(Player player, string number, int miles)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].FractionID != 13)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет доступа!", 3000);
                    return;
                }
                if (Main.Players[player].FractionLVL < 6)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Минимальный ранг для скрута пробега 6!", 3000);
                    return;
                }
                if (player.HasData("COOLDOWN2") && player.GetData<DateTime>("COOLDOWN2") > DateTime.Now)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Подождите 10 минут перед скруткой пробега!", 3000);
                    return;
                }
                if (miles < 300 || miles > 10000)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Можно скручивать только от 300 км. до 10000 км.", 3000);
                    return;
                }
                if (!VehicleManager.Vehicles.ContainsKey(number))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Нет транспорта с таким номером", 3000);
                    return;
                }
                Vehicle vehicle = null;
                foreach(Vehicle veh in NAPI.Pools.GetAllVehicles())
                {
                    if (veh.NumberPlate == number && veh.Position.DistanceTo2D(player.Position) < 10f && VehicleManager.Vehicles[veh.NumberPlate].Holder != player.Name)
                    {
                        vehicle = veh;
                        break;
                    }
                }
                if (vehicle == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Рядом нет такого автомобиля", 3000);
                    return;
                }
                if (VehicleManager.Vehicles[vehicle.NumberPlate].Sell < miles)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно столько скрутить", 3000);
                    return;
                }
                Player holder = null;
                foreach(Player hol in NAPI.Pools.GetAllPlayers())
                {
                    if (hol.Name == VehicleManager.Vehicles[vehicle.NumberPlate].Holder && vehicle.Position.DistanceTo2D(hol.Position) < 10f)
                    {
                        holder = hol;
                        break;
                    }
                }
                if (holder == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Рядом нет владельца авто", 3000);
                    return;
                }
                holder.SetData("TARGET", player);
                holder.SetData("NUMBER", number);
                holder.SetData("VEH", vehicle);
                holder.SetData("MIL_TWIST", miles);
                Trigger.PlayerEvent(holder, "openDialog", "TWIST_ACCEPT", $"Вам предлагают скрутить пробег на {miles} км. за 20000$ [{number}]");
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили Гражданину({holder.Id}) скрутить пробег на {miles} км.", 3000);
            }
            catch { }
        }

        [Command("flatbedonveh")]
        public static void CN_pox(Player player)
        {
            try
            {
                player.SendChatMessage("VEHICLE ON YOUR FLATBED: ID " + player.Vehicle.GetSharedData<int>("fbAttachVehicle"));
                foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                    if (veh.Id == player.Vehicle.GetSharedData<int>("fbAttachVehicle"))
                    {
                        veh.ResetSharedData("fbAttachVehicle");
                        NAPI.Task.Run(() => { veh.Delete(); });
                    }
            }
            catch
            {
            }
        }
		[Command("badnum")]
        public static void CMD_newVehicleMafiaNumber(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].FamilyRank == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны состоять в семье!", 3000);
                    return;
                }
                if (Golemo.Families.Family.GetFamilyToCid(player).MaxUpdates[4] == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас не куплено улучшение перебивка номеров!", 3000);
                    return;
                }
                if (player.Vehicle == null || player.Vehicle.GetData<string>("ACCESS") != "PERSONAL")
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не в машине!", 3000);
                    return;
                }
                if (player.HasData("COOLDOWN") && player.GetData<DateTime>("COOLDOWN") > DateTime.Now)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Подождите 10 секунд перед сменой номера!", 3000);
                    return;
                }
               var players = Main.GetPlayersInRadiusOfPosition(player.Position, 10);
                var result = false;
                Player owner = null;
                foreach(Player Player in players)
                {
                    if (Player == player) continue;
                    if (Player == player.Vehicle.GetData<Player>("OWNER"))
                    {
                        result = true;
                        owner = Player;
                        break;
                    }
                }
                if (!result)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В районе 10 метров нет владельца!", 3000);
                    return;
                }
                var oldNum = player.Vehicle.NumberPlate;
                if (!VehicleManager.Vehicles.ContainsKey(oldNum))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Такой машины не существует", 3000);
                    return;
                }

                var house = Houses.HouseManager.GetHouse(owner, true);
                if (house != null)
                {
                    var garage = Houses.GarageManager.Garages[house.GarageID];
                    if (garage.CheckCar(true, oldNum))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Машина должна быть на улице!", 3000);
                        return;
                    }
                }

                var newNum = VehicleManager.GenerateNumber();

                var vData = VehicleManager.Vehicles[oldNum];
                VehicleManager.Vehicles.Add(newNum, vData);               
                var newData = VehicleManager.Vehicles[newNum];

                bool have = false;

                foreach (nItem item in nInventory.Items[Main.Players[player].UUID])
                    if (item.Data == $"{oldNum}_{VehicleManager.Vehicles[oldNum].KeyNum}")
                    {
                        have = true;
                        nInventory.Remove(player, item);
                        break;
                    }

                if (have)
                {
                    var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.CarKey));
                    if (tryAdd == -1 || tryAdd > 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно места в инвентаре", 3000);
                        return;
                    }

                    nInventory.Add(player, new nItem(ItemType.CarKey, 1, $"{newNum}_{VehicleManager.Vehicles[newNum].KeyNum}"));
                }

                if (house != null)
                {
                    var garage = Houses.GarageManager.Garages[house.GarageID];
                    garage.DeleteOffCar(oldNum);
                    garage.SetOutVehicle(newNum, player.Vehicle);
                }
                player.Vehicle.NumberPlate = newNum;
                try
                {
                    if (house != null)
                    {
                        Houses.Garage garage = Houses.GarageManager.Garages[house.GarageID];
                        garage.DeleteCar(oldNum);
                    }
                }
                catch { }
                VehicleManager.Vehicles.Remove(oldNum);
                MySQL.Query($"UPDATE vehicles SET number='{newNum}' WHERE number='{oldNum}'");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Новый номер для {oldNum} = {newNum}", 3000);

                NAPI.Task.Run(() =>
                {
                    var res = MySQL.QueryRead($"SELECT * FROM vehicles WHERE holder='{player.Name}'");

                    List<string> fnumbers = new List<string> { };

                    foreach (DataRow Row in res.Rows)
                        fnumbers.Add(Row["number"].ToString());

                    foreach (string number in VehicleManager.getAllPlayerVehicles(player.Name))
                        if (!fnumbers.Contains(number))
                            VehicleManager.Vehicles.Remove(number);

                }, 1000);


                player.SetData("COOLDOWN", DateTime.Now.AddSeconds(10));
            }
            catch (Exception e) { Log.Write("newvnum: " + e.Message, nLog.Type.Error); }
        }
        [Command("fakegunlic")]
        public static void CMD_fakegunlic(Player player, int ID)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (Main.Players[player].FractionID != 10)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет доступа!", 3000);
                return;
            }
            var target = Main.GetPlayerByID(ID);
            if (target == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин с таким ID не найден", 3000);
                return;
            }
            if (Main.Players[target].Licenses[6])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У гражданина уже есть лицензия!", 3000);
                return;
            }
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин слишком далеко", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выдали гражданину лицензию на оружие (2 часа)!", 3000);
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Вам выдали лицензию на оружие (2 часа)!", 3000);
            Main.Players[target].Licenses[6] = true;
            target.SetData("FAKENAME", target.Name);
            target.SetData("FAKEGUNT", 0);
            target.SetData("FAKEGUNTIMER", Timers.StartTask(1000,() => FakeGunLic(target, target.Name)));
            GUI.Dashboard.sendStats(target);
        }

        public static void FakeGunLic(Player player, string nick)
        {
            try
            {
                if (!player.HasData("FAKEGUNT") || !player.HasData("FAKEGUNTIMER")) return;
                if (player.GetData<int>("FAKEGUNT") >= 7200 && player.Name == nick)
                {
                    Main.Players[player].Licenses[6] = false;
                    Timers.Stop(player.GetData<string>("FAKEGUNTIMER"));
                    player.ResetData("FAKEGUNT");
                    player.ResetData("FAKEGUNTIMER");
                }
                player.SetData("FAKEGUNT", player.GetData<string>("FAKEGUNT") + 1);
            }
            catch (Exception e) { Log.Write("FAKE GUN: " + e.ToString(), nLog.Type.Error); }
        }

        [Command("checkpass")]
        public static void CMD_checkpass(Player player, int idpass)
        {
            if (Main.Players[player].FractionID != 9)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет доступа!", 3000);
                return;
            }
            var result = MySQL.QueryRead("SELECT * FROM characters");
            string name = "Не найдено";
            foreach(DataRow Row in result.Rows)
            {
                if (Row["uuid"].ToString() != idpass.ToString()) continue;
                name = Row["firstname"].ToString() + " " + Row["lastname"].ToString();
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Результат поиска: {name}!", 3000);
        }
        [Command("gpsuser")]
        public static void CMD_gpsuser(Player player, string firstname, string lastname)
        {
            if (Main.Players[player].FractionID != 9)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет доступа!", 3000);
                return;
            }
            if (string.IsNullOrEmpty(firstname) || string.IsNullOrEmpty(lastname))
            {
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Не коректные данные!", 3000);
                return;
            }
            if (player.HasData("KDFORSKY"))
            {
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"В данный момент спутник не находится над штатом!", 3000);
                return;
            }
            foreach (Player target in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(target)) continue;
                if (Main.Players[target].FirstName == firstname && Main.Players[target].LastName == lastname)
                {
                    player.SetData("KDFORSKY", Timers.StartOnceTask(600000, () => TimerSky(player)));
                    Trigger.PlayerEvent(player, "createWaypoint", target.Position.X, target.Position.Y);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Поставлена метка на Ваш GPS!", 3000);
                    return;
                }
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Данного гражданина нет в этом штате!", 3000);
        }

        private static void TimerSky(Player player)
        {
            if (!player.HasData("KDFORSKY")) return;
            player.ResetData("KDFORSKY");
        }


        [Command("redname")]
        public static void CMD_redname(Player player)
        {
            try
            {
                if (!Group.CanUseCmd(player, "redname")) return;

                if (!player.HasSharedData("REDNAME") || !player.GetSharedData<bool>("REDNAME"))
                {
                    player.SendChatMessage("~r~Redname ON");
                    player.SetSharedData("REDNAME", true);
                }
                else
                {
                    player.SendChatMessage("~r~Redname OFF");
                    player.SetSharedData("REDNAME", false);
                }

            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("hidenick")]
        public static void CMD_hidenick(Player player) {
            if (!Group.CanUseCmd(player, "setvehdirt")) return;
            if (!player.HasSharedData("HideNick") || !player.GetSharedData<bool>("HideNick"))
                {
                    player.SendChatMessage("~g~HideNick ON");
                    player.SetSharedData("HideNick", true);
                }
                else
                {
                    player.SendChatMessage("~g~HideNick OFF");
                    player.SetSharedData("HideNick", false);
                }

        }
        [Command("giveup")]
        public static void CMD_givereds(Player player, int id, int amount)
        {
            try
            {
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.sendRedbucks(player, target, amount);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("checkprop")]
        public static void CMD_checkProperety(Player player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "checkprop")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин с таким ID не найден", 3000);
                    return;
                }

                if (!Main.Players.ContainsKey(target)) return;
                var house = Houses.HouseManager.GetHouse(target);
                if (house != null)
                {
                    if (house.Owner == target.Name)
                    {
                        player.SendChatMessage($"Гражданин имеет дом стоимостью ${house.Price} класса '{Houses.HouseManager.HouseTypeList[house.Type].Name}'");
                        var targetVehicles = VehicleManager.getAllPlayerVehicles(target.Name);
                        foreach (var num in targetVehicles)
                        player.SendChatMessage($"У игрока есть машина '{VehicleManager.Vehicles[num].Model}' с номером '{num}'");
                    }
                    else
                        player.SendChatMessage($"Игрок заселен в дом к {house.Owner} стоимостью ${house.Price}");
                }
                else
                    player.SendChatMessage("У игрока нет дома");
            }
            catch (Exception e)
            {
                Log.Write("checkprop: " + e.Message, nLog.Type.Error);
            }
        }
        [Command("id", "~y~/id [имя/id]")]
        public static void CMD_checkId(Player player, string target)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "id")) return;

                int id;
                if (Int32.TryParse(target, out id))
                {
                    foreach (var p in NAPI.Pools.GetAllPlayers())
                    {
                        if (!Main.Players.ContainsKey(p)) continue;
                        if (p.Value == id)
                        {
                            player.SendChatMessage($"ID: {p.Value} | {p.Name}");
                            return;
                        }
                    }
                    player.SendChatMessage("Игрок с таким ID не найден");
                }
                else
                {
                    var players = 0;
                    foreach (var p in NAPI.Pools.GetAllPlayers())
                    {
                        if (!Main.Players.ContainsKey(p)) continue;
                        if (p.Name.ToUpper().Contains(target.ToUpper()))
                        {
                            player.SendChatMessage($"ID: {p.Value} | {p.Name}");
                            players++;
                        }
                    }
                    if (players == 0)
                        player.SendChatMessage("Не найдено игрока с таким именем");
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\"/id/:\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("setdim")]
        public static void CMD_setDim(Player player, int id, int dim)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "setdim")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                if (!Main.Players.ContainsKey(target)) return;
                target.Dimension = Convert.ToUInt32(dim);
                GameLog.Admin($"{player.Name}", $"setDim({dim})", $"{target.Name}");
            } catch(Exception e)
            {
                Log.Write("setdim: " + e.Message, nLog.Type.Error);
            }
        }
        [Command("checkdim")]
        public static void CMD_checkDim(Player player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "checkdim")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                if (!Main.Players.ContainsKey(target)) return;
                GameLog.Admin($"{player.Name}", $"checkDim", $"{target.Name}");
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Измерение игрока - {target.Dimension.ToString()}", 4000);
            }
            catch (Exception e)
            {
                Log.Write("checkdim: " + e.Message, nLog.Type.Error);
            }
        }
        [Command("setbizmafia")]
        public static void CMD_setBizMafia(Player player, int mafia)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "setbizmafia")) return;
                if (player.GetData<int>("BIZ_ID") == -1) return;
                if (mafia < 10 || mafia > 13) return;

                BCore.Bizness biz = BCore.BizList[player.GetData<int>("BIZ_ID")];
                biz.Mafia = mafia;
                biz.UpdateLabel();
                GameLog.Admin($"{player.Name}", $"setBizMafia({biz.ID},{mafia})", $"");
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"{mafia} мафия теперь владеет бизнесом №{biz.ID}", 3000);
            }
            catch (Exception e) { Log.Write("setbizmafia: " + e.Message, nLog.Type.Error); }
        }
        [Command("newsimcard")]
        public static void CMD_newsimcard(Player player, int id, int newnumber)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "newsimcard")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                if (!Main.Players.ContainsKey(target)) return;
                if (Main.SimCards.ContainsKey(newnumber))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Такой номер уже существует", 3000);
                    return;
                }

                Main.SimCards.Remove(newnumber);
                Main.SimCards.Add(newnumber, Main.Players[target].UUID);
                Main.Players[target].Sim = newnumber;
                GUI.Dashboard.sendStats(target);
                GameLog.Admin($"{player.Name}", $"newsim({newnumber})", $"{target.Name}");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Новый номер для {target.Name} = {newnumber}", 3000);
            }
            catch (Exception e) { Log.Write("newsimcard: " + e.Message, nLog.Type.Error); }
        }
        [Command("takeoffbiz")]
        public static void CMD_takeOffBusiness(Player admin, int bizid, bool byaclear = false)
        {
            try
            {
                if (!Main.Players.ContainsKey(admin)) return;
                if (!Group.CanUseCmd(admin, "takeoffbiz")) return;

                var biz = BCore.BizList[bizid];
                var owner = biz.Owner;
                var player = NAPI.Player.GetPlayerFromName(owner);

                if (player != null && Main.Players.ContainsKey(player))
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Администратор отобрал у Вас бизнес", 3000);
                    MoneySystem.Wallet.Change(player, Convert.ToInt32(biz.Cost * 0.8));
                    Main.Players[player].BizIDs.Remove(biz.ID);
                }
                else
                {
                    var split = biz.Owner.Split('_');
                    var data = MySQL.QueryRead($"SELECT biz,money FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                    List<int> ownerBizs = new List<int>();
                    var money = 0;

                    foreach (DataRow Row in data.Rows)
                    {
                        ownerBizs = JsonConvert.DeserializeObject<List<int>>(Row["biz"].ToString());
                        money = Convert.ToInt32(Row["money"]);
                    }

                    ownerBizs.Remove(biz.ID);
                    MySQL.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(ownerBizs)}',money={money + Convert.ToInt32(biz.Cost * 0.8)} WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                }

                MoneySystem.Bank.Accounts[biz.BankID].Balance = 0;
                MoneySystem.Bank.Save(biz.BankID);
                biz.Owner = "Государство";
                biz.UpdateLabel();
                GameLog.Money($"server", $"player({Main.PlayerUUIDs[owner]})", Convert.ToInt32(biz.Cost * 0.8), $"takeoffBiz({biz.ID})");
                Notify.Send(admin, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы отобрали бизнес у {owner}", 3000);
                if(!byaclear) GameLog.Admin($"{player.Name}", $"takeoffBiz({biz.ID})", $"");
            }
            catch (Exception e) { Log.Write("takeoffbiz: " + e.Message, nLog.Type.Error); }
        }
        [Command("paydaymultiplier")]
        public static void CMD_paydaymultiplier(Player player, int multi)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "paydaymultiplier")) return;
                if (multi < 1 || multi > 5)
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Возможно установить только от 1 до 5", 3000);
                    return;
                }

                Main.oldconfig.PaydayMultiplier = multi;
                GameLog.Admin($"{player.Name}", $"paydayMultiplier({multi})", $"");
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"PaydayMultiplier изменен на {multi}", 3000);
            }
            catch (Exception e) { Log.Write("paydaymultiplier: " + e.Message, nLog.Type.Error); }
        }
        [Command("expmultiplier")]
        public static void CMD_expmultiplier(Player player, int multi)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "expmultiplier")) return;
                if (multi < 1 || multi > 5)
                {
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Возможно установить только от 1 до 5", 3000);
                    return;
                }

                Main.oldconfig.ExpMultiplier = multi;
                GameLog.Admin($"{player.Name}", $"expMultiplier({multi})", $"");
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"ExpMultiplier изменен на {multi}", 3000);
            }
            catch (Exception e) { Log.Write("paydaymultiplier: " + e.Message, nLog.Type.Error); }
        }
        [Command("offdelfrac")] 
        public static void CMD_offlineDelFraction(Player player, string name)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "offdelfrac")) return;

                var split = name.Split('_');
                MySQL.Query($"UPDATE `characters` SET fraction=0,fractionlvl=0 WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы уволили игрока {name} из Вашей фракции", 3000);

                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                GameLog.Admin($"{player.Name}", $"delfrac", $"{name}");
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы сняли фракцию с {name}", 3000);
            }
            catch (Exception e) { Log.Write("offdelfrac: " + e.Message, nLog.Type.Error); }
        }
        [Command("removeobj")]
        public static void CMD_removeObject(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "removeobj")) return;

                player.SetData("isRemoveObject", true);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Следующий подобранный предмет будет в бане", 3000);
            }
            catch (Exception e) { Log.Write("removeobj: " + e.Message, nLog.Type.Error); }
        }
        [Command("unwarn")]
        public static void CMD_unwarn(Player player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "unwarn")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                if (!Main.Players.ContainsKey(target)) return;
                if (Main.Players[target].Warns <= 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока нет варнов", 3000);
                    return;
                }

                Main.Players[target].Warns--;
                GUI.Dashboard.sendStats(target);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы сняли варн у игрока {target.Name}, у него {Main.Players[target].Warns} варнов", 3000);
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"У вас сняли варн, осталось {Main.Players[target].Warns} варнов", 3000);
                GameLog.Admin($"{player.Name}", $"unwarn", $"{target.Name}");
            }
            catch (Exception e) { Log.Write("unwarn: " + e.Message, nLog.Type.Error); }
        }
        [Command("offunwarn")]
        public static void CMD_offunwarn(Player player, string target)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "unwarn")) return;

                if (!Main.PlayerNames.ContainsValue(target))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок не найден", 3000);
                    return;
                }
                if (NAPI.Player.GetPlayerFromName(target) != null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок онлайн", 3000);
                    return;
                }

                var split = target.Split('_');
                var data = MySQL.QueryRead($"SELECT warns FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                var warns = 0;
                foreach (System.Data.DataRow Row in data.Rows)
                {
                    warns = Convert.ToInt32(Row["warns"]);
                }

                if (warns <= 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока нет варнов", 3000);
                    return;
                }

                warns--;
                GameLog.Admin($"{player.Name}", $"offUnwarn", $"{target}");
                MySQL.Query($"UPDATE characters SET warns={warns} WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы сняли варн у игрока {target}, у него {warns} варнов", 3000);
            }
            catch (Exception e) { Log.Write("offunwarn: " + e.Message, nLog.Type.Error); }
        }
        [Command("rescar")]
        public static void CMD_respawnCar(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "rescar")) return;
                if (!player.IsInVehicle) return;
                var vehicle = player.Vehicle;

                if (!vehicle.HasData("ACCESS"))
                    return;
                else if (vehicle.GetData<string>("ACCESS") == "PERSONAL")
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "На данный момент функция восстановления личной машины игрока отключена", 3000);
                else if (vehicle.GetData<string>("ACCESS") == "WORK")
                    Admin.RespawnWorkCar(vehicle);
                else if (vehicle.GetData<string>("ACCESS") == "FRACTION")
                    Admin.RespawnFractionCar(vehicle);
                else if (vehicle.GetData<string>("ACCESS") == "GANGDELIVERY" || vehicle.GetData<string>("ACCESS") == "MAFIADELIVERY")
                    NAPI.Entity.DeleteEntity(vehicle);

                GameLog.Admin($"{player.Name}", $"rescar", $"");
            }
            catch (Exception e) { Log.Write("ResCar: " + e.Message, nLog.Type.Error); }
        }
        [Command("bansync")]
        public static void CMD_banlistSync(Player Player)
        {
            try
            {
                if (!Group.CanUseCmd(Player, "ban")) return;
                Notify.Send(Player, NotifyType.Warning, NotifyPosition.BottomCenter, "Начинаю процедуру синхронизации...", 4000);
                Ban.Sync();
                Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, "Процедура завершена!", 3000);
            }
            catch (Exception e) { Log.Write("bansync: " + e.Message, nLog.Type.Error); }
        }
        [Command("setcolour")]
        public static void CMD_setTerritoryColor(Player player, int gangid)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "setcolour")) return;

                if (player.GetData<int>("GANGPOINT") == -1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не находитесь ни на одном из регионов", 3000);
                    return;
                }
                var terrid = player.GetData<int>("GANGPOINT");

                if (!Fractions.GangsCapture.gangPointsColor.ContainsKey(gangid))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Банды с таким ID нет", 3000);
                    return;
                }

                Fractions.GangsCapture.gangPoints[terrid].GangOwner = gangid;
                Main.PlayerEventToAll("setZoneColor", Fractions.GangsCapture.gangPoints[terrid].ID, Fractions.GangsCapture.gangPointsColor[gangid]);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Теперь территорией №{terrid} владеет {Fractions.Manager.FractionNames[gangid]}", 3000);
                GameLog.Admin($"{player.Name}", $"setColour({terrid},{gangid})", $"");
            }
            catch (Exception e) { Log.Write("CMD_SetColour: " + e.Message, nLog.Type.Error); }
        }
        [Command("sc")]
        public static void CMD_setClothes(Player player, int id, int draw, int texture)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "allspawncar")) return;
                player.SetClothes(id, draw, texture);
                if (id == 11) player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][draw], 0);
                if (id == 1) Customization.SetMask(player, draw, texture);
            } catch { }
        }
        [Command("sac")]
        public static void CMD_setAccessories(Player player, int id, int draw, int texture)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "allspawncar")) return;
            if (draw > -1)
                player.SetAccessories(id, draw, texture);
            else
                player.ClearAccessory(id);
        }
        [Command("checkwanted")]
        public static void CMD_checkwanted(Player player, int id)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "checkwanted")) return;
            var target = Main.GetPlayerByID(id);
            if (target == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                return;
            }
            var stars = (Main.Players[target].WantedLVL == null) ? 0 : Main.Players[target].WantedLVL.Level;
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Кол-во звёзд - {stars}", 3000);
        }
        [Command("fixcar")]
        public static void CMD_fixcar(Player player)
        {
            try
            {
                if (!Group.CanUseCmd(player, "fixcar")) return;
                if (!player.IsInVehicle) return;
                VehicleManager.RepairCar(player.Vehicle);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_fixcar\":" + e.ToString(), nLog.Type.Error);
            }
        }
        [Command("stats")]
        public static void CMD_showPlayerStats(Player admin, int id)
        {
            try
            {
                if (!Group.CanUseCmd(admin, "stats")) return;

                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(admin, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                Player player = Main.GetPlayerByID(id);
                if(player == admin) return;
                var acc = Main.Players[player];
                string status =
                    (acc.AdminLVL >= 1) ? "Администратор" :
                    (Main.Accounts[player].VipLvl > 0) ? $"{Group.GroupNames[Main.Accounts[player].VipLvl]} до {Main.Accounts[player].VipDate.ToString("dd.MM.yyyy")}" :
                    $"{Group.GroupNames[Main.Accounts[player].VipLvl]}";

                long bank = (acc.Bank != 0) ? MoneySystem.Bank.Accounts[acc.Bank].Balance : 0;

                var lic = "";
                for (int i = 0; i < acc.Licenses.Count; i++)
                    if (acc.Licenses[i]) lic += $"{Main.LicWords[i]} / ";
                if (lic == "") lic = "Отсутствуют";

                string work = (acc.WorkID > 0) ? Jobs.WorkManager.JobStats[acc.WorkID - 1] : "Безработный";
                string fraction = (acc.FractionID > 0) ? Fractions.Manager.FractionNames[acc.FractionID] : "Нет";

                var number = (acc.Sim == -1) ? "Нет сим-карты" : Main.Players[player].Sim.ToString();

                List<object> data = new List<object>
                {
                    acc.LVL,
                    $"{acc.EXP}/{3 + acc.LVL * 3}",
                    number,
                    status,
                    0,
                    acc.Warns,
                    lic,
                    acc.CreateDate.ToString("dd.MM.yyyy"),
                    acc.UUID,
                    acc.Bank,
                    work,
                    fraction,
                    acc.FractionLVL,
                };

                string json = JsonConvert.SerializeObject(data);
                Trigger.PlayerEvent(admin, "board", 2, json);
                admin.SetData("CHANGE_WITH", player);
                GUI.Dashboard.OpenOut(admin, nInventory.Items[acc.UUID], player.Name, 20);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_showPlayerStats\":" + e.ToString(), nLog.Type.Error);
            }
        }
        [Command("admins")]
        public static void CMD_AllAdmins(Player Player)
        {
            try
            {
                if (!Group.CanUseCmd(Player, "admins")) return;

                Player.SendChatMessage("=== ADMINS ONLINE ===");
                foreach (var p in Main.Players)
                {
                    if (p.Value.AdminLVL < 1) continue;
                    Player.SendChatMessage($"[{p.Key.Value}] {p.Key.Name} - {p.Value.AdminLVL}");
                }
                Player.SendChatMessage("=== ADMINS ONLINE ===");

            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_AllAdmins\":" + e.ToString(), nLog.Type.Error);
            }
        }
        [Command("changebizprice")]
        public static void CMD_changeBusinessPrice(Player player, int newPrice)
        {
            if (!Group.CanUseCmd(player, "changebizprice")) return;
            if (player.GetData<int>("BIZ_ID") == -1)
            {
                player.SendChatMessage("~r~Вы должны находиться на одном из бизнесов");
                return;
            }
            BCore.Bizness biz = BCore.BizList[player.GetData<int>("BIZ_ID")];
            biz.Cost = newPrice;
            biz.UpdateLabel();
        }
        [Command("pa")]
        public static void CMD_playAnimation(Player player, string dict, string anim, int flag)
        {
            if (!Group.CanUseCmd(player, "pa")) return;
            player.PlayAnimation(dict, anim, flag);
        }
        [Command("sa")]
        public static void CMD_stopAnimation(Player player)
        {
            if (!Group.CanUseCmd(player, "sa")) return;
            player.StopAnimation();
        }
        [Command("changestock")]
        public static void CMD_changeStock(Player player, int fracID, string item, int amount)
        {
            if (!Group.CanUseCmd(player, "changestock")) return;
            if (!Fractions.Stocks.fracStocks.ContainsKey(fracID))
            {
                player.SendChatMessage("~r~Склада такой фракции нет");
                return;
            }
            switch (item)
            {
                case "mats":
                    Fractions.Stocks.fracStocks[fracID].Materials += amount;
                    Fractions.Stocks.fracStocks[fracID].UpdateLabel();
                    return;
                case "drugs":
                    Fractions.Stocks.fracStocks[fracID].Drugs += amount;
                    Fractions.Stocks.fracStocks[fracID].UpdateLabel();
                    return;
                case "medkits":
                    Fractions.Stocks.fracStocks[fracID].Medkits += amount;
                    Fractions.Stocks.fracStocks[fracID].UpdateLabel();
                    return;
                case "money":
                    Fractions.Stocks.fracStocks[fracID].Money += amount;
                    Fractions.Stocks.fracStocks[fracID].UpdateLabel();
                    return;
            }
            player.SendChatMessage("~r~mats - материалы");
            player.SendChatMessage("~r~drugs - наркотики");
            player.SendChatMessage("~r~medkits - мед. аптечки");
            player.SendChatMessage("~r~money - деньги");
            GameLog.Admin($"{player.Name}", $"changeStock({item},{amount})", $"");
        }
        [Command("tpc")]
        public static void CMD_tpCoord(Player player, double x, double y, double z)
        {
            if (!Group.CanUseCmd(player, "tpc")) return;
            NAPI.Entity.SetEntityPosition(player, new Vector3(x, y, z));
        }
        [Command("inv")]
        public static void CMD_ToogleInvisible(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "inv")) return;

            BasicSync.SetInvisible(player, !BasicSync.GetInvisible(player));
        }
        [Command("delfrac")]
        public static void CMD_delFrac(Player player, int id)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (Main.GetPlayerByID(id) == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                return;
            }
            Admin.delFrac(player, Main.GetPlayerByID(id));
        }
        [Command("sendcreator")]
        public static void CMD_SendToCreator(Player player, int id)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "sendcreator")) return;
            var target = Main.GetPlayerByID(id);
            if (target == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                return;
            }
            Customization.SendToCreator(target);
            GameLog.Admin($"{player.Name}", $"sendCreator", $"{target.Name}");
        }
        [Command("afuel")]
        public static void CMD_setVehiclePetrol(Player player, int fuel)
        {
            try
            {
                if (!Group.CanUseCmd(player, "afuel")) return;
                if (!player.IsInVehicle) return;
                player.Vehicle.SetSharedData("PETROL", fuel);
                GameLog.Admin($"{player.Name}", $"afuel({fuel})", $"");
            }
            catch (Exception e) { Log.Write("afuel: " + e.Message, nLog.Type.Error); }
        }
        [Command("changename", GreedyArg = true)]
        public static void CMD_changeName(Player Player, string curient, string newName)
        {
            try
            {
                if (!Group.CanUseCmd(Player, "changename")) return;
                if (!Main.PlayerNames.ContainsValue(curient)) return;

                try
                {
                    string[] split = newName.Split("_");
                    Log.Debug($"SPLIT: {split[0]} {split[1]}");
                }
                catch (Exception e)
                {
                    Log.Write("ERROR ON CHANGENAME COMMAND\n" + e.ToString());
                }
                

                if (Main.PlayerNames.ContainsValue(newName))
                {
                    Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "Такое имя уже существует!", 3000);
                    return;
                }

                Player target = NAPI.Player.GetPlayerFromName(curient);
                Character.Character.toChange.Add(curient, newName);

                if (target == null || target.IsNull)
                {
                    Notify.Send(Player, NotifyType.Alert, NotifyPosition.BottomCenter, "Игрок оффлайн, меняем...", 3000);
                    Task changeTask = Character.Character.changeName(curient);
                }
                else
                {
                    Notify.Send(Player, NotifyType.Alert, NotifyPosition.BottomCenter, "Игрок онлайн, кикаем...", 3000);
                    NAPI.Player.KickPlayer(target);
                }

                Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, "Ник изменен!", 3000);
                GameLog.Admin($"{Player.Name}", $"changeName({newName})", $"{curient}");

            } catch(Exception e)
            {
                Log.Write("EXCEPTION AT \"CMD_CHANGENAME\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        [Command("startmatwars")]
        public static void CMD_startMatWars(Player player)
        {
            try
            {
                if (!Group.CanUseCmd(player, "startmatwars")) return;
                if (Fractions.MatsWar.isWar)
                {
                    player.SendChatMessage("~r~Война за маты уже идёт");
                    return;
                }
                Fractions.MatsWar.startMatWarTimer();
                player.SendChatMessage("~r~Начата война за маты");
                GameLog.Admin($"{player.Name}", $"startMatwars", $"");
            }
            catch (Exception e) { Log.Write("startmatwars: " + e.Message, nLog.Type.Error); }
        }
        [Command("giveexp")]
        public static void CMD_giveExp(Player player, int id, int exp)
        {
            try
            {
                if (!Group.CanUseCmd(player, "giveexp")) return;
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Main.Players[target].EXP += exp;
                if (Main.Players[target].EXP >= 3 + Main.Players[target].LVL * 3)
                {
                    Main.Players[target].EXP = Main.Players[target].EXP - (3 + Main.Players[target].LVL * 3);
                    Main.Players[target].LVL += 1;
                    if(Main.Players[target].LVL == 1) {
                        NAPI.Task.Run(() => { try { Trigger.PlayerEvent(target, "disabledmg", false); } catch { } }, 5000);
                    }
                }
                Dashboard.sendStats(target);
                GameLog.Admin($"{player.Name}", $"giveExp({exp})", $"{target.Name}");
            }
            catch (Exception e) { Log.Write("giveexp" + e.Message, nLog.Type.Error); }
        }
        [Command("housetypeprice")]
        public static void CMD_replaceHousePrices(Player player, int type, int newPrice)
        {
            if (!Group.CanUseCmd(player, "housetypeprice")) return;
            foreach (var h in Houses.HouseManager.Houses)
            {
                if (h.Type != type) continue;
                h.Price = newPrice;
                h.UpdateLabel();
                h.Save();
            }
        }
        [Command("delhouseowner")]
        public static void CMD_deleteHouseOwner(Player player)
        {
            if (!Group.CanUseCmd(player, "delhouseowner")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться на маркере дома", 3000);
                return;
            }

            Houses.House house = Houses.HouseManager.Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            if (house == null) return;

            house.SetOwner(null);
            house.UpdateLabel();
            house.Save();
            GameLog.Admin($"{player.Name}", $"delHouseOwner({house.ID})", $"");
        }
        [Command("stt")]
        public static void CMD_SetTurboTorque(Player player, float power, float torque)
        {
            try
            {
                if (!Group.CanUseCmd(player, "stt")) return;
                if (!player.IsInVehicle) return;
                Trigger.PlayerEvent(player, "svem", power, torque);
            } catch(Exception e)
            {
                Log.Write("Error at \"STT\":" + e.ToString(), nLog.Type.Error);
            }
        }
        [Command("svm")]
        public static void CMD_SetVehicleMod(Player player, int type, int index)
        {
            try
            {
                if (!Group.CanUseCmd(player, "svm")) return;
                if (!player.IsInVehicle) return;
                player.Vehicle.SetMod(type, index);

            } catch (Exception e)
            {
                Log.Write("Error at \"SVM\":" + e.ToString(), nLog.Type.Error);
            }
        }
        [Command("svn")]
        public static void CMD_SetVehicleNeon(Player player, byte r, byte g, byte b, byte alpha)
        {
            try
            {
                if (!Group.CanUseCmd(player, "svm")) return;
                if (!player.IsInVehicle) return;
                Vehicle v = player.Vehicle;
                if (alpha != 0) {
                    NAPI.Vehicle.SetVehicleNeonState(v, true);
                    NAPI.Vehicle.SetVehicleNeonColor(v, r, g, b);
                } else {
                    NAPI.Vehicle.SetVehicleNeonColor(v, 255, 255, 255);
                    NAPI.Vehicle.SetVehicleNeonState(v, false);
                }
            } catch (Exception e)
            {
                Log.Write("Error at \"SVN\":" + e.ToString(), nLog.Type.Error);
            }
        }
        [Command("svhid")]
        public static void CMD_SetVehicleHeadlightColor(Player player, int hlcolor)
        {
            try
            {
                if (!Group.CanUseCmd(player, "svm")) return;
                if (!player.IsInVehicle) return;
                Vehicle v = player.Vehicle;
                if (hlcolor >= 0 && hlcolor <= 12) {
                    v.SetSharedData("hlcolor", hlcolor);
                    Trigger.PlayerEventInRange(v.Position, 250f, "VehStream_SetVehicleHeadLightColor", v.Handle, hlcolor);
                } else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Цвет фар может быть от 0 до 12.", 3000);
            } catch (Exception e)
            {
                Log.Write("Error at \"SVN\":" + e.ToString(), nLog.Type.Error);
            }
        }
        [Command("svh")]
        public static void CMD_SetVehicleHealth(Player player, int health = 100)
        {
            try
            {
                if (!Group.CanUseCmd(player, "svh")) return;
                if (!player.IsInVehicle) return;
                Vehicle v = player.Vehicle;
                v.Repair();
                v.Health = health;

            } catch (Exception e)
            {
                Log.Write("Error at \"SVH\":" + e.ToString(), nLog.Type.Error);
            }
            
        }
        [Command("delacars")]
        public static void CMD_deleteAdminCars(Player player)
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (!Group.CanUseCmd(player, "delacars")) return;
                        foreach (var v in NAPI.Pools.GetAllVehicles())
                        {
                            if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "ADMIN")
                                v.Delete();
                        }
                        GameLog.Admin($"{player.Name}", $"delacars", $"");
                    } catch { }
                });
            }
            catch (Exception e) { Log.Write("delacars: " + e.Message, nLog.Type.Error); }
        }
        [Command("delacar")]
        public static void CMD_deleteThisAdminCar(Player Player)
        {
            if (!Group.CanUseCmd(Player, "delacar")) return;
            if (!Player.IsInVehicle) return;
            Vehicle veh = Player.Vehicle;
            if (veh.HasData("ACCESS") && veh.GetData<string>("ACCESS") == "ADMIN")
                veh.Delete();
            GameLog.Admin($"{Player.Name}", $"delacar", $"");
        }
        [Command("delmycars", "dmcs")]
        public static void CMD_delMyCars(Player Player)
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (!Group.CanUseCmd(Player, "vehc")) return;
                        foreach (var v in NAPI.Pools.GetAllVehicles())
                        {
                            if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "GOD")
                            {
                                if (v.GetData<string>("BY") == Player.Name)
                                    v.Delete();
                            }
                        }
                        GameLog.Admin($"{Player.Name}", $"delmycars", $"");
                    } catch { }
                });
            }
            catch (Exception e) { Log.Write("delacars: " + e.Message, nLog.Type.Error); }
        }
        [Command("allspawncar")]
        public static void CMD_allSpawnCar(Player player)
        {
            Admin.respawnAllCars(player);
        }
        [Command("save")]
        public static void CMD_saveCoord(Player player, string name)
        {
            Admin.saveCoords(player, name);
        }
        [Command("setfractun")]
        public static void ACMD_setfractun(Player player, int cat = -1, int id = -1)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "setvehdirt")) return;
                if (!player.IsInVehicle)
                {
                    player.SendChatMessage("Вы должны сидеть в машине фракции, которую хотите изменить");
                    return;
                }
                if (player.Vehicle.HasData("ACCESS") && player.Vehicle.GetData<string>("ACCESS") == "FRACTION")
                {
                    if (!Fractions.Configs.FractionVehicles[player.Vehicle.GetData<int>("FRACTION")].ContainsKey(player.Vehicle.NumberPlate)) return;
                    int fractionid = player.Vehicle.GetData<int>("FRACTION");
                    if (cat < 0)
                    {
                        Core.VehicleManager.FracApplyCustomization(player.Vehicle, fractionid);
                        return;
                    }

                    string number = player.Vehicle.NumberPlate;
                    Tuple<VehicleHash, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization> oldtuple = Fractions.Configs.FractionVehicles[fractionid][number];
                    VehicleHash oldvehhash = oldtuple.Item1;
                    Vector3 oldvehpos = oldtuple.Item2;
                    Vector3 oldvehrot = oldtuple.Item3;
                    int oldvehrank = oldtuple.Item4;
                    int oldvehc1 = oldtuple.Item5;
                    int oldvehc2 = oldtuple.Item6;
                    VehicleManager.VehicleCustomization oldvehdata = oldtuple.Item7;
                    switch (cat)
                    {
                        case 0:
                            oldvehdata.Spoiler = id;
                            break;
                        case 1:
                            oldvehdata.FrontBumper = id;
                            break;
                        case 2:
                            oldvehdata.RearBumper = id;
                            break;
                        case 3:
                            oldvehdata.SideSkirt = id;
                            break;
                        case 4:
                            oldvehdata.Muffler = id;
                            break;
                        case 5:
                            oldvehdata.Wings = id;
                            break;
                        case 6:
                            oldvehdata.Roof = id;
                            break;
                        case 7:
                            oldvehdata.Hood = id;
                            break;
                        case 8:
                            oldvehdata.Vinyls = id;
                            break;
                        case 9:
                            oldvehdata.Lattice = id;
                            break;
                        case 10:
                            oldvehdata.Engine = id;
                            break;
                        case 11:
                            oldvehdata.Turbo = id;
                            var turbo = (oldvehdata.Turbo == 0);
                            player.Vehicle.SetSharedData("TURBO", turbo);
                            break;
                        case 12:
                            oldvehdata.Horn = id;
                            break;
                        case 13:
                            oldvehdata.Transmission = id;
                            break;
                        case 14:
                            oldvehdata.WindowTint = id;
                            break;
                        case 15:
                            oldvehdata.Suspension = id;
                            break;
                        case 16:
                            oldvehdata.Brakes = id;
                            break;
                        case 17:
                            oldvehdata.Headlights = id;
                            break;
                        case 18:
                            oldvehdata.NumberPlate = id;
                            break;
                        case 19:
                            oldvehdata.NeonColor.Red = id;
                            break;
                        case 20:
                            oldvehdata.NeonColor.Green = id;
                            break;
                        case 21:
                            oldvehdata.NeonColor.Blue = id;
                            break;
                        case 22:
                            oldvehdata.NeonColor.Alpha = id;
                            break;
                        case 23:
                            oldvehdata.WheelsType = id;
                            break;
                        case 24:
                            oldvehdata.Wheels = id;
                            break;
                        case 25:
                            oldvehdata.WheelsColor = id;
                            break;
						case 26:
                            oldvehdata.Armor = id;
                            break;	
                    }
                    Fractions.Configs.FractionVehicles[fractionid][number] = new Tuple<VehicleHash, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>(oldvehhash, oldvehpos, oldvehrot, oldvehrank, oldvehc1, oldvehc2, oldvehdata);
                    MySqlCommand cmd = new MySqlCommand
                    {
                        CommandText = "UPDATE `fractionvehicles` SET `components`=@com WHERE `number`=@num"
                    };
                    cmd.Parameters.AddWithValue("@com", JsonConvert.SerializeObject(oldvehdata));
                    cmd.Parameters.AddWithValue("@num", player.Vehicle.NumberPlate);
                    MySQL.Query(cmd);
                    Core.VehicleManager.FracApplyCustomization(player.Vehicle, fractionid);
                    player.SendChatMessage("Вы изменили тюнинг этой машины для фракции.");
                }
                else player.SendChatMessage("Вы должны сидеть в машине фракции, которую хотите изменить");
            }
            catch { }
        }
        [Command("setfracveh")]
        public static void ACMD_setfracveh(Player player, string vehname, int rank, int c1, int c2)
        {
            try
            {
                if (!Group.CanUseCmd(player, "setfracveh")) return;
                if (!player.IsInVehicle)
                {
                    player.SendChatMessage("Вы должны сидеть в машине фракции, которую хотите изменить");
                    return;
                }
                if (rank <= 0 || c1 < 0 || c1 >= 160 || c2 < 0 || c2 >= 160) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(vehname);
                if (vh == 0) return;
                Vehicle vehicle = player.Vehicle;
                if (vehicle.HasData("ACCESS") && vehicle.GetData<string>("ACCESS") == "FRACTION")
                {
                    if (!Fractions.Configs.FractionVehicles[vehicle.GetData<int>("FRACTION")].ContainsKey(vehicle.NumberPlate)) return;

                    var canmats = (vh == VehicleHash.Barracks || vh == VehicleHash.Youga || vh == VehicleHash.Burrito3 || vh == VehicleHash.Rumpo3);
                    var candrugs = (vh == VehicleHash.Youga || vh == VehicleHash.Burrito3);
                    var canmeds = (vh == VehicleHash.Ambulance);
                    int fractionid = vehicle.GetData<int>("FRACTION");
                    NAPI.Data.SetEntityData(vehicle, "CANMATS", false);
                    NAPI.Data.SetEntityData(vehicle, "CANDRUGS", false);
                    NAPI.Data.SetEntityData(vehicle, "CANMEDKITS", false);
                    if (canmats) NAPI.Data.SetEntityData(vehicle, "CANMATS", true);
                    if (candrugs) NAPI.Data.SetEntityData(vehicle, "CANDRUGS", true);
                    if (canmeds) NAPI.Data.SetEntityData(vehicle, "CANMEDKITS", true);
                    NAPI.Data.SetEntityData(vehicle, "MINRANK", rank);
                    Vector3 pos = NAPI.Entity.GetEntityPosition(vehicle) + new Vector3(0, 0, 0.5);
                    Vector3 rot = NAPI.Entity.GetEntityRotation(vehicle);
                    VehicleManager.VehicleCustomization data = Fractions.Configs.FractionVehicles[fractionid][vehicle.NumberPlate].Item7;
                    if (Fractions.Configs.FractionVehicles[fractionid][vehicle.NumberPlate].Item1 != vh) data = new VehicleManager.VehicleCustomization();
                    Fractions.Configs.FractionVehicles[fractionid][vehicle.NumberPlate] = new Tuple<VehicleHash, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>(vh, pos, rot, rank, c1, c2, data);
                    MySqlCommand cmd = new MySqlCommand
                    {
                        CommandText = "UPDATE `fractionvehicles` SET `model`=@mod,`position`=@pos,`rotation`=@rot,`rank`=@ra,`colorprim`=@col,`colorsec`=@sec,`components`=@com WHERE `number`=@num"
                    };
                    cmd.Parameters.AddWithValue("@mod", vehname);
                    cmd.Parameters.AddWithValue("@pos", JsonConvert.SerializeObject(pos));
                    cmd.Parameters.AddWithValue("@rot", JsonConvert.SerializeObject(rot));
                    cmd.Parameters.AddWithValue("@ra", rank);
                    cmd.Parameters.AddWithValue("@col", c1);
                    cmd.Parameters.AddWithValue("@sec", c2);
                    cmd.Parameters.AddWithValue("@com", JsonConvert.SerializeObject(data));
                    cmd.Parameters.AddWithValue("@num", vehicle.NumberPlate);
                    MySQL.Query(cmd);
                    vehicle.PrimaryColor = c1;
                    vehicle.SecondaryColor = c2;
                    NAPI.Entity.SetEntityModel(vehicle, (uint)vh);
                    VehicleManager.FracApplyCustomization(vehicle, fractionid);
                    player.SendChatMessage("Вы изменили данные этой машины для фракции.");
                }
                else player.SendChatMessage("Вы должны сидеть в машине фракции, которую хотите изменить");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"ACMD_setfracveh\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("payday")]
        public static void payDay(Player player, string text = null)
        {
            if (!Group.CanUseCmd(player, "payday")) return;
            GameLog.Admin($"{player.Name}", $"payDay", "");
            Main.payDayTrigger();
        }
        [Command("setleader")]
        public static void CMD_setLeader(Player player, int id, int fracid)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.setFracLeader(player, Main.GetPlayerByID(id), fracid);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("sp")]
        public static void CMD_spectateMode(Player player, int id)
        {
            if (!Group.CanUseCmd(player, "sp")) return;
            try
            {
                AdminSP.Spectate(player, id);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("usp")]
        public static void CMD_unspectateMode(Player player)
        {
            if (!Group.CanUseCmd(player, "sp")) return;
            try
            {
                AdminSP.UnSpectate(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("metp")]
        public static void CMD_teleportToMe(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.teleportTargetToPlayer(player, Main.GetPlayerByID(id), false);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("gethere")]
        public static void CMD_teleportVehToMe(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.teleportTargetToPlayer(player, Main.GetPlayerByID(id), true);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("kill")]
        public static void CMD_kill(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.killTarget(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("hp")]
        public static void CMD_adminHeal(Player player, int id, int hp)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.healTarget(player, Main.GetPlayerByID(id), hp);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("ar")]
        public static void CMD_adminArmor(Player player, int id, int ar)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.armorTarget(player, Main.GetPlayerByID(id), ar);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("fz")]
        public static void CMD_adminFreeze(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.freezeTarget(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("unfz")]
        public static void CMD_adminUnFreeze(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.unFreezeTarget(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("setadmin")]
        public static void CMD_setAdmin(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.setPlayerAdminGroup(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("deladmin")]
        public static void CMD_delAdmin(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.delPlayerAdminGroup(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("setadminrank")]
        public static void CMD_setAdminRank(Player player, int id, int rank)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.setPlayerAdminRank(player, Main.GetPlayerByID(id), rank);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("guns")]
        public static void CMD_adminGuns(Player player, int id, string wname, string serial)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.giveTargetGun(player, Main.GetPlayerByID(id), wname, serial);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("giveclothes")]
        public static void CMD_adminClothes(Player player, int id, string wname, string serial)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.giveTargetClothes(player, Main.GetPlayerByID(id), wname, serial);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("setskin")]
        public static void CMD_adminSetSkin(Player player, int id, string pedModel)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.giveTargetSkin(player, Main.GetPlayerByID(id), pedModel);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("oguns")]
        public static void CMD_adminOGuns(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.takeTargetGun(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("givemoney")]
        public static void CMD_adminGiveMoney(Player player, int id, int money)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.giveMoney(player, Main.GetPlayerByID(id), money);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("delleader")]
        public static void CMD_delleader(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.delFracLeader(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("deljob")]
        public static void CMD_deljob(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.delJob(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("vehc")]
        public static void CMD_createVehicleCustom(Player player, string name, int r, int g, int b)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "vehc")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(name);
                if (vh == 0) throw null;
                var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                veh.Dimension = player.Dimension;
                veh.NumberPlate = "GOD";
                veh.CustomPrimaryColor = new Color(r, g, b);
                veh.CustomSecondaryColor = new Color(r, g, b);
                veh.SetData("ACCESS", "ADMIN");
                veh.SetData("BY", player.Name);
                VehicleStreaming.SetEngineState(veh, true);
                Log.Debug($"vehc {name} {r} {g} {b}" );
                GameLog.Admin($"{player.Name}", $"vehCreate({name})", $"");
            }
            catch { }
        }
        [Command("veh")]
        public static void CMD_createVehicle(Player player, string name, int a, int b)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "vehc")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(name);
                player.SendChatMessage("vh " + name);
                if (vh == 0)
                {
                    player.SendChatMessage("vh return");
                    return;
                }
                var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);

                veh.Dimension = player.Dimension;
                veh.NumberPlate = "GOD";
                veh.PrimaryColor = a;
                veh.SecondaryColor = b;
                veh.SetData("ACCESS", "ADMIN");
                veh.SetData("BY", player.Name);
                player.SetData("VEHNAME", name);
                VehicleStreaming.SetEngineState(veh, false);
                GameLog.Admin($"{player.Name}", $"vehCreate({name}) {veh.Model}", $"");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD_veh\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("vehs")]
        public static void CMD_createVehicles(Player player, string name, int a, int b, int count)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "vehc")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(name);
                if (vh == 0) throw null;
                for(int i = count; i > 0; i--)
                {
                    var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                    veh.Dimension = player.Dimension;
                    veh.NumberPlate = "ADMIN";
                    veh.PrimaryColor = a;
                    veh.SecondaryColor = b;
                    veh.SetData("ACCESS", "ADMIN");
                    veh.SetData("BY", player.Name);
                    VehicleStreaming.SetEngineState(veh, true);
                }
                GameLog.Admin($"{player.Name}", $"vehsCreate({name})", $"");
            }
            catch { }
        }
        [Command("vehcs")]
        public static void CMD_createVehicleCustoms(Player player, string name, int r, int g, int b, int count)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "vehc")) return;
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(name);
                if (vh == 0) throw null;
                for (int i = count; i > 0; i--)
                {
                    var veh = NAPI.Vehicle.CreateVehicle(vh, player.Position, player.Rotation.Z, 0, 0);
                    veh.Dimension = player.Dimension;
                    veh.NumberPlate = "ADMIN";
                    veh.CustomPrimaryColor = new Color(r, g, b);
                    veh.CustomSecondaryColor = new Color(r, g, b);
                    veh.SetData("ACCESS", "ADMIN");
                    veh.SetData("BY", player.Name);
                    VehicleStreaming.SetEngineState(veh, true);
                    Log.Debug($"vehc {name} {r} {g} {b}");
                }
                GameLog.Admin($"{player.Name}", $"vehsCreate({name})", $"");
            }
            catch { }
        }
        [Command("vehcustompcolor")]
        public static void CMD_ApplyCustomPColor(Player Player, int r, int g, int b, int mod = -1)
        {
            try
            {
                if (!Main.Players.ContainsKey(Player)) return;
                if (!Group.CanUseCmd(Player, "setvehdirt")) return;
                Color color = new Color(r, g, b);

                var number = Player.Vehicle.NumberPlate;

                VehicleManager.Vehicles[number].Components.PrimColor = color;
                VehicleManager.Vehicles[number].Components.PrimModColor = mod;

                VehicleManager.ApplyCustomization(Player.Vehicle);

            } catch { }
        }
        [Command("aclear")]
        public static void ACMD_aclear(Player player, string target) {
            try {
                if (!Main.Players.ContainsKey(player)) return;
                if (!Group.CanUseCmd(player, "aclear")) return;
                if (!Main.PlayerNames.ContainsValue(target))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок не найден", 3000);
                    return;
                }
                if (NAPI.Player.GetPlayerFromName(target) != null)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Невозможно очистить персонажа, который находится в игре", 3000);
                    return;
                }
                string[] split = target.Split('_');
                int tuuid = 0;
                // CLEAR BIZ
                DataTable result = MySQL.QueryRead($"SELECT uuid,adminlvl,biz FROM `characters` WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                if (result != null && result.Rows.Count != 0) {
                    DataRow row = result.Rows[0];
                    if(Convert.ToInt32(row["adminlvl"]) >= Main.Players[player].AdminLVL) {
                        SendToAdmins(3, $"!{{#d35400}}[ACLEAR-DENIED] {player.Name} ({player.Value}) попытался очистить {target} (offline), который имеет выше уровень администратора.");
                        return;
                    }
                    tuuid = Convert.ToInt32(row["uuid"]);
                    List<int> TBiz = JsonConvert.DeserializeObject<List<int>>(row["biz"].ToString());
                    if(TBiz.Count >= 1 && TBiz[0] >= 1) {
                        var biz = BCore.BizList[TBiz[0]];
                        var owner = biz.Owner;
                        var ownerplayer = NAPI.Player.GetPlayerFromName(owner);

                        if (ownerplayer != null && Main.Players.ContainsKey(player))
                        {
                            Notify.Send(ownerplayer, NotifyType.Warning, NotifyPosition.BottomCenter, $"Администратор отобрал у Вас бизнес", 3000);
                            MoneySystem.Wallet.Change(ownerplayer, Convert.ToInt32(biz.Cost * 0.8));
                            Main.Players[ownerplayer].BizIDs.Remove(biz.ID);
                        }
                        else
                        {
                            var split1 = biz.Owner.Split('_');
                            var data = MySQL.QueryRead($"SELECT biz,money FROM characters WHERE firstname='{split1[0]}' AND lastname='{split1[1]}'");
                            List<int> ownerBizs = new List<int>();
                            var money = 0;

                            foreach (DataRow Row in data.Rows)
                            {
                                ownerBizs = JsonConvert.DeserializeObject<List<int>>(Row["biz"].ToString());
                                money = Convert.ToInt32(Row["money"]);
                            }

                            ownerBizs.Remove(biz.ID);
                            MySQL.Query($"UPDATE characters SET biz='{JsonConvert.SerializeObject(ownerBizs)}',money={money + Convert.ToInt32(biz.Cost * 0.8)} WHERE firstname='{split1[0]}' AND lastname='{split1[1]}'");
                        }

                        MoneySystem.Bank.Accounts[biz.BankID].Balance = 0;
                        MoneySystem.Bank.Save(biz.BankID);
                        biz.Owner = "Государство";
                        biz.UpdateLabel();
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы отобрали бизнес у {owner}", 3000);
                    }
                } else {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Не удалось найти персонажа в базе данных", 3000);
                    return;
                }
                // CLEAR HOUSE
                result = MySQL.QueryRead($"SELECT id FROM `houses` WHERE `owner`='{target}'");
                if (result != null && result.Rows.Count != 0) {
                    DataRow row = result.Rows[0];
                    Houses.House house = Houses.HouseManager.Houses.FirstOrDefault(h => h.ID == Convert.ToInt32(row[0]));
                    if (house != null) {
                        house.SetOwner(null);
                        house.UpdateLabel();
                        house.Save();
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы отобрали дом у {target}", 3000);
                    }
                }
                // CLEAR VEHICLES
                result = MySQL.QueryRead($"SELECT `number` FROM `vehicles` WHERE `holder`='{target}'");
                if (result != null && result.Rows.Count != 0) {
                    DataRowCollection rows = result.Rows;
                    foreach(DataRow row in rows) {
                        VehicleManager.Remove(row[0].ToString());
                    }
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы отобрали у {target} все машины.", 3000);
                }

                // CLEAR MONEY, HOTEL, FRACTION, SIMCARD, PET
                MySQL.Query($"UPDATE `characters` SET `money`=0,`fraction`=0,`fractionlvl`=0,`hotel`=-1,`hotelleft`=0,`sim`=-1,`PetName`='null' WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                // CLEAR BANK MONEY
                Bank.Data bankAcc = Bank.Accounts.FirstOrDefault(a => a.Value.Holder == target).Value;
                if(bankAcc != null) {
                    bankAcc.Balance = 0;
                    MySQL.Query($"UPDATE `money` SET `balance`=0 WHERE `holder`='{target}'");
                }
                // CLEAR ITEMS
                if(tuuid != 0) MySQL.Query($"UPDATE `inventory` SET `items`='[]' WHERE `uuid`={tuuid}");
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы забрали у игрока все вещи, деньги с рук и банковского счёта у {target}", 3000);
                GameLog.Admin($"{player.Name}", $"aClear", $"{target}");
            } catch (Exception e) { Log.Write("EXCEPTION AT aclear\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("vehcustomscolor")]
        public static void CMD_ApplyCustomSColor(Player Player, int r, int g, int b, int mod = -1)
        {
            try
            {
                if (!Main.Players.ContainsKey(Player)) return;
                if (!Group.CanUseCmd(Player, "setvehdirt")) return;
                Color color = new Color(r, g, b);

                var number = Player.Vehicle.NumberPlate;

                VehicleManager.Vehicles[number].Components.SecColor = color;
                VehicleManager.Vehicles[number].Components.SecModColor = mod;

                VehicleManager.ApplyCustomization(Player.Vehicle);

            } catch { }
        }

        [Command("findbyveh")]
        public static void CMD_FindByVeh(Player player, string number) {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "findbyveh")) return;
            if(number.Length > 8) {
                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Количество символов в номерном знаке не может превышать 8.", 3000);
                return;
            }
            if(VehicleManager.Vehicles.ContainsKey(number)) Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Номер машины: {number} | Модель: {VehicleManager.Vehicles[number].Model} | Владелец: {VehicleManager.Vehicles[number].Holder}", 6000);
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Не найдено машины с таким номерным знаком.", 3000);
        }

        [Command("vehcustom")]
        public static void CMD_ApplyCustom(Player Player, int cat = -1, int id = -1)
        {
            try
            {
                if (!Main.Players.ContainsKey(Player)) return;
                if (!Group.CanUseCmd(Player, "setvehdirt")) return;

                if (!Player.IsInVehicle) return;

                if (cat < 0)
                {
                    Core.VehicleManager.ApplyCustomization(Player.Vehicle);
                    return;
                }

                var number = Player.Vehicle.NumberPlate;

                switch (cat)
                {
                    case 0:
                        VehicleManager.Vehicles[number].Components.Muffler = id;
                        break;
                    case 1:
                        VehicleManager.Vehicles[number].Components.SideSkirt = id;
                        break;
                    case 2:
                        VehicleManager.Vehicles[number].Components.Hood = id;
                        break;
                    case 3:
                        VehicleManager.Vehicles[number].Components.Spoiler = id;
                        break;
                    case 4:
                        VehicleManager.Vehicles[number].Components.Lattice = id;
                        break;
                    case 5:
                        VehicleManager.Vehicles[number].Components.Wings = id;
                        break;
                    case 6:
                        VehicleManager.Vehicles[number].Components.Roof = id;
                        break;
                    case 7:
                        VehicleManager.Vehicles[number].Components.Vinyls = id;
                        break;
                    case 8:
                        VehicleManager.Vehicles[number].Components.FrontBumper = id;
                        break;
                    case 9:
                        VehicleManager.Vehicles[number].Components.RearBumper = id;
                        break;
                    case 10:
                        VehicleManager.Vehicles[number].Components.Engine = id;
                        break;
                    case 11:
                        VehicleManager.Vehicles[number].Components.Turbo = id;
                        var turbo = (VehicleManager.Vehicles[number].Components.Turbo == 0);
                        Player.Vehicle.SetSharedData("TURBO", turbo);
                        break;
                    case 12:
                        VehicleManager.Vehicles[number].Components.Horn = id;
                        break;
                    case 13:
                        VehicleManager.Vehicles[number].Components.Transmission = id;
                        break;
                    case 14:
                        VehicleManager.Vehicles[number].Components.WindowTint = id;
                        break;
                    case 15:
                        VehicleManager.Vehicles[number].Components.Suspension = id;
                        break;
                    case 16:
                        VehicleManager.Vehicles[number].Components.Brakes = id;
                        break;
                    case 17:
                        VehicleManager.Vehicles[number].Components.Headlights = id;
                        break;
                    case 18:
                        VehicleManager.Vehicles[number].Components.NumberPlate = id;
                        break;
                    case 19:
                        VehicleManager.Vehicles[number].Components.NeonColor.Red = id;
                        break;
                    case 20:
                        VehicleManager.Vehicles[number].Components.NeonColor.Green = id;
                        break;
                    case 21:
                        VehicleManager.Vehicles[number].Components.NeonColor.Blue = id;
                        break;
                    case 22:
                        VehicleManager.Vehicles[number].Components.NeonColor.Alpha = id;
                        break;
                    case 23:
                        VehicleManager.Vehicles[number].Components.WheelsType = id;
                        break;
                    case 24:
                        VehicleManager.Vehicles[number].Components.Wheels = id;
                        break;
                    case 25:
                        VehicleManager.Vehicles[number].Components.WheelsColor = id;
                        break;
					case 26:
                        VehicleManager.Vehicles[number].Components.Armor = id;
                        break;	
                }

                Core.VehicleManager.ApplyCustomization(Player.Vehicle);
            }
            catch { }
        }
        [Command("sw")]
        public static void CMD_setWeather(Player player, byte weather)
        {
            if (!Group.CanUseCmd(player, "sw")) return;
            Main.changeWeather(weather);
            GameLog.Admin($"{player.Name}", $"setWeather({weather})", $"");
        }
        [Command("st")]
        public static void CMD_setTime(Player player, int hours, int minutes, int seconds)
        {
            if (!Group.CanUseCmd(player, "st")) return;
            NAPI.World.SetTime(hours, minutes, seconds);
        }
        [Command("tp")]
        public static void CMD_teleport(Player player, int id)
        {
            try
            {
                if (!Group.CanUseCmd(player, "tp")) return;

                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                NAPI.Entity.SetEntityPosition(player, target.Position + new Vector3(1,0,1.5));
                NAPI.Entity.SetEntityDimension(player, NAPI.Entity.GetEntityDimension(target));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("goto")]
        public static void CMD_teleportveh(Player player, int id)
        {
            try
            {
                if (!Group.CanUseCmd(player, "tp")) return;
                if(!player.IsInVehicle) return;
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                NAPI.Entity.SetEntityDimension(player.Vehicle, NAPI.Entity.GetEntityDimension(target));
                NAPI.Entity.SetEntityPosition(player.Vehicle, target.Position + new Vector3(2,2,2));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("flip")]
        public static void CMD_flipveh(Player player, int id)
        {
            try
            {
                if (!Group.CanUseCmd(player, "tp")) return;
                Player target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if(!target.IsInVehicle) {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок не находится в машине", 3000);
                    return;
                }
                NAPI.Entity.SetEntityPosition(target.Vehicle, target.Vehicle.Position + new Vector3(0,0,2.5f));
                NAPI.Entity.SetEntityRotation(target.Vehicle, new Vector3(0,0,target.Vehicle.Rotation.Z));
                GameLog.Admin($"{player.Name}", $"flipVeh", $"{target.Name}");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("mtp")]
        public static void CMD_maskTeleport(Player player, int id)
        {
            try
            {
                if (!Group.CanUseCmd(player, "mtp")) return;

                if (!Main.MaskIds.ContainsKey(id))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Маска с таким ID не найдена", 3000);
                    return;
                }
                var target = Main.MaskIds[id];

                NAPI.Entity.SetEntityPosition(player, target.Position);
                NAPI.Entity.SetEntityDimension(player, NAPI.Entity.GetEntityDimension(target));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("createsafe", GreedyArg = true)]
        public static void CMD_createSafe(Player player, int id, float distance, int min, int max, string address)
        {
            try
            {
                SafeMain.CMD_CreateSafe(player, id, distance, min, max, address);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("removesafe")]
        public static void CMD_removeSafe(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    SafeMain.CMD_RemoveSafe(player);
                }
                catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
            });
        }
        [Command("createstock")]
        public static void CMD_createStock(Player player, int frac, int drugs, int mats, int medkits, int money)
        {
            try
            {
                //MySQL.Query($"INSERT INTO fractions (id,drugs,mats,medkits,money) VALUES ({frac},{drugs},{mats},{medkits},{money})");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("demorgan", GreedyArg = true)]
        public static void CMD_sendTargetToDemorgan(Player player, int id, int time, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.sendPlayerToDemorgan(player, Main.GetPlayerByID(id), time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("loadipl")]
        public static void CMD_LoadIPL(Player player, string ipl) {
            try {
                if (!Group.CanUseCmd(player, "setvehdirt")) return;
                NAPI.World.RequestIpl(ipl);
                player.SendChatMessage("Вы подгрузили IPL: " + ipl);
            } catch {
            }
        }
        [Command("unloadipl")]
        public static void CMD_UnLoadIPL(Player player, string ipl) {
            try {
                if (!Group.CanUseCmd(player, "setvehdirt")) return;
                NAPI.World.RemoveIpl(ipl);
                player.SendChatMessage("Вы выгрузили IPL: " + ipl);
            } catch {
            }
        }

        [Command("starteffect")]
        public static void CMD_StartEffect(Player player, string effect, int dur = 0, bool loop = false) {
            try {
                if (!Group.CanUseCmd(player, "setvehdirt")) return;
                Trigger.PlayerEvent(player, "startScreenEffect", effect, dur, loop);
                player.SendChatMessage("Вы включили Effect: " + effect);
            } catch {
            }
        }
        [Command("stopeffect")]
        public static void CMD_StopEffect(Player player, string effect) {
            try {
                if (!Group.CanUseCmd(player, "setvehdirt")) return;
                Trigger.PlayerEvent(player, "stopScreenEffect", effect);
                player.SendChatMessage("Вы выключили Effect: " + effect);
            } catch {
            }
        }
        [Command("udemorgan")]
        public static void CMD_releaseTargetFromDemorgan(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.releasePlayerFromDemorgan(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        
        [Command("offjail", GreedyArg = true)]
        public static void CMD_offlineJailTarget(Player player, string target, int time, string reason)
        {
            try
            {
                if (!Group.CanUseCmd(player, "offjail")) return;
                if (!Main.PlayerNames.ContainsValue(target))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок не найден", 3000);
                    return;
                }
                if(player.Name.Equals(target)) return;
                if (NAPI.Player.GetPlayerFromName(target) != null)
                {
                    Admin.sendPlayerToDemorgan(player, NAPI.Player.GetPlayerFromName(target), time, reason);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Игрок был онлайн, поэтому offjail заменён на demorgan", 3000);
                    return;
                }

                var firstTime = time * 60;
                var deTimeMsg = "м";
                if (time > 60)
                {
                    deTimeMsg = "ч";
                    time /= 60;
                    if (time > 24)
                    {
                        deTimeMsg = "д";
                        time /= 24;
                    }
                }

                var split = target.Split('_');
                MySQL.QueryRead($"UPDATE `characters` SET `demorgan`={firstTime},`arrest`=0 WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} посадил игрока {target} в спец. тюрьму на {time}{deTimeMsg} ({reason})");
                GameLog.Admin($"{player.Name}", $"demorgan({time}{deTimeMsg},{reason})", $"{target}");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("offwarn", GreedyArg = true)]
        public static void CMD_offlineWarnTarget(Player player, string target, int time, string reason)
        {
            try
            {
                if (!Group.CanUseCmd(player, "offwarn")) return;
                if (!Main.PlayerNames.ContainsValue(target))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок не найден", 3000);
                    return;
                }
                if(player.Name.Equals(target)) return;
                if (NAPI.Player.GetPlayerFromName(target) != null)
                {
                    Admin.warnPlayer(player, NAPI.Player.GetPlayerFromName(target), reason);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Игрок был онлайн, поэтому offwarn был заменён на warn", 3000);
                    return;
                } else {
                    string[] split1 = target.Split('_');
                    DataTable result = MySQL.QueryRead($"SELECT adminlvl FROM characters WHERE firstname='{split1[0]}' AND lastname='{split1[1]}'");
                    DataRow row = result.Rows[0];
                    int targetadminlvl = Convert.ToInt32(row[0]);
                    if(targetadminlvl >= Main.Players[player].AdminLVL) {
                        SendToAdmins(3, $"!{{#d35400}}[OFFWARN-DENIED] {player.Name} ({player.Value}) попытался забанить {target} (offline), который имеет выше уровень администратора.");
                        return;
                    }
                }

                
                var split = target.Split('_');
                var data = MySQL.QueryRead($"SELECT warns FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                var warns = Convert.ToInt32(data.Rows[0]["warns"]);
                warns++;

                if (warns >= 3)
                {
                    MySQL.Query($"UPDATE `characters` SET `warns`=0 WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                    Ban.Offline(target, DateTime.Now.AddMinutes(43200), false, "Warns 3/3", "Server_Serverniy");
                }
                else
                    MySQL.Query($"UPDATE `characters` SET `unwarn`='{MySQL.ConvertTime(DateTime.Now.AddDays(14))}',`warns`={warns},`fraction`=0,`fractionlvl`=0 WHERE firstname='{split[0]}' AND lastname='{split[1]}'");

                NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} выдал предупреждение игроку {target} ({warns}/3 | {reason})");
                GameLog.Admin($"{player.Name}", $"warn({time},{reason})", $"{target}");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("ban", GreedyArg = true)]
        public static void CMD_banTarget(Player player, int id, int time, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.banPlayer(player, Main.GetPlayerByID(id), time, reason, false);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("hardban", GreedyArg = true)]
        public static void CMD_hardbanTarget(Player player, int id, int time, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.hardbanPlayer(player, Main.GetPlayerByID(id), time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("offban", GreedyArg = true)]
        public static void CMD_offlineBanTarget(Player player, string name, int time, string reason)
        {
            try
            {
                if (!Main.PlayerNames.ContainsValue(name))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрока с таким именем не найдено", 3000);
                    return;
                }
                Admin.offBanPlayer(player, name, time, reason, false);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("soffban", GreedyArg = true)]
        public static void CMD_sofflineBanTarget(Player player, string name, int time, string reason)
        {
            try
            {
                if (!Main.PlayerNames.ContainsValue(name))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрока с таким именем не найдено", 3000);
                    return;
                }
                Admin.offBanPlayer(player, name, time, reason, true);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("offhardban", GreedyArg = true)]
        public static void CMD_offlineHardbanTarget(Player player, string name, int time, string reason)
        {
            try
            {
                if (!Main.PlayerNames.ContainsValue(name))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрока с таким именем не найдено", 3000);
                    return;
                }
                Admin.offHardBanPlayer(player, name, time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("unban", GreedyArg = true)]
        public static void CMD_unbanTarget(Player player, string name)
        {
            if (!Group.CanUseCmd(player, "ban")) return;
            try
            {
                Admin.unbanPlayer(player, name);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("unhardban", GreedyArg = true)]
        public static void CMD_unhardbanTarget(Player player, string name)
        {
            if (!Group.CanUseCmd(player, "ban")) return;
            try
            {
                Admin.unhardbanPlayer(player, name);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("offgiveup")]
        public static void CMD_offredbaks(Player Player, string name, long amount)
        { 
            if (!Group.CanUseCmd(Player, "offredbucks")) return;
            try
            {
                name = name.ToLower();
                KeyValuePair<Player, nAccount.Account> acc = Main.Accounts.FirstOrDefault(x => x.Value.Login == name);
                if (acc.Value != null)
                {
                    Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок онлайн! {acc.Key.Name}:{acc.Key.Value}", 8000);
                    return;
                }
                MySQL.Query($"update `accounts` set `redbucks`=`redbucks`+{amount} where `login`='{name}'");
                GameLog.Admin(Player.Name, $"offgivereds({amount})", name);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
		 
        [Command("mute", GreedyArg = true)]
        public static void CMD_muteTarget(Player player, int id, int time, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.mutePlayer(player, Main.GetPlayerByID(id), time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("offmute", GreedyArg = true)]
        public static void CMD_offlineMuteTarget(Player player, string target, int time, string reason)
        {
            try
            {
                if (!Main.PlayerNames.ContainsValue(target))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок не найден", 3000);
                    return;
                }
                Admin.OffMutePlayer(player, target, time, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("unmute")]
        public static void CMD_muteTarget(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.unmutePlayer(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("sban", GreedyArg = true)]
        public static void CMD_silenceBan(Player player, int id, int time)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.banPlayer(player, Main.GetPlayerByID(id), time, "", true);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("kick", GreedyArg = true)]
        public static void CMD_kick(Player player, int id, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.kickPlayer(player, Main.GetPlayerByID(id), reason, false);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("skick")]
        public static void CMD_silenceKick(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.kickPlayer(player, Main.GetPlayerByID(id), "Silence kick", true);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("gm")]
        public static void CMD_checkGamemode(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.checkGamemode(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("agm")]
        public static void CMD_enableGodmode(Player player)
        {
            try
            {
                if (!Group.CanUseCmd(player, "agm")) return;
                if (!player.HasSharedData("AGM") || !player.GetSharedData<bool>("AGM")) {
                    Trigger.PlayerEvent(player, "AGM", true);
                    player.SetSharedData("AGM", true);
                } else {
                    Trigger.PlayerEvent(player, "AGM", false);
                    player.SetSharedData("AGM", false);
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("warn", GreedyArg = true)]
        public static void CMD_warnTarget(Player player, int id, string reason)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.warnPlayer(player, Main.GetPlayerByID(id), reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("asms", GreedyArg = true)]
        public static void CMD_adminSMS(Player player, int id, string msg)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.adminSMS(player, Main.GetPlayerByID(id), msg);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("ans", GreedyArg = true)]
        public static void CMD_answer(Player player, int id, string answer)
        {
            try
            {
                var sender = Main.GetPlayerByID(id);
                if (sender == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.answerReport(player, sender, answer);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("global", GreedyArg = true)]
        public static void CMD_adminGlobalChat(Player player, string message)
        {
            try
            {
                Admin.adminGlobal(player, message);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("a", GreedyArg = true)]
        public static void CMD_adminChat(Player player, string message)
        {
            try
            {
                Admin.adminChat(player, message);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("setvip")]
        public static void CMD_setVip(Player player, int id, int rank)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.setPlayerVipLvl(player, Main.GetPlayerByID(id), rank);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [Command("checkmoney")]
        public static void CMD_checkMoney(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Admin.checkMoney(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        #endregion
        
        #region VipCommands
        [Command("leave")]
        public static void CMD_leaveFraction(Player player)
        {
            try
            {
                if (Main.Accounts[player].VipLvl == 0) return;

                Fractions.Manager.UNLoad(player);

                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == player.Name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                Main.Players[player].FractionID = 0;
                Main.Players[player].FractionLVL = 0;

                Customization.ApplyCharacter(player);
                if (player.HasData("HAND_MONEY")) player.SetClothes(5, 45, 0);
                else if (player.HasData("HEIST_DRILL")) player.SetClothes(5, 41, 0);
                player.SetData("ON_DUTY", false);
                NAPI.Player.RemoveAllPlayerWeapons(player);

                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Вы покинули организацию", 3000);
                return;
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        #endregion

        [Command("ticket", GreedyArg = true)]
        public static void CMD_govTicket(Player player, int id, int sum, string reason)
        {
            try
            {
                var target = Main.GetPlayerByID(id);
                if (sum < 1) return;
                if (target == null || !Main.Players.ContainsKey(target))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (target.Position.DistanceTo(player.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                    return;
                }
                Fractions.FractionCommands.ticketToTarget(player, target, sum, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("respawn")]
        public static void CMD_respawnFracCars(Player player)
        {
            try
            {
                Fractions.FractionCommands.respawnFractionCars(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("givemedlic")]
        public static void CMD_givemedlic(Player player, int id)
        {
            try
            {
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (target.Position.DistanceTo(player.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                    return;
                }
                Fractions.FractionCommands.giveMedicalLic(player, target);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
		
		[Command("giveparamed")]
        public static void CMD_giveparamedlic(Player player, int id)
        {
            try
            {
                var target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (target.Position.DistanceTo(player.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                    return;
                }
                Fractions.FractionCommands.giveParamedicLic(player, target);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("password")]
        public static void CMD_ResetPassword(Player player, string new_password)
        {
            if (!Main.Players.ContainsKey(player)) return;
            Main.Accounts[player].changePassword(new_password);
            Notify.Send(player, NotifyType.Alert, NotifyPosition.BottomCenter, "Вы сменили пароль! Перезайдите с новым.", 3000);
        }

        [Command("time")]
        public static void CMD_checkPrisonTime(Player player)
        {
            try
            {
                if (Main.Players[player].ArrestTime != 0)
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вам осталось сидеть {Convert.ToInt32(Main.Players[player].ArrestTime / 60.0)} минут", 3000);
                else if (Main.Players[player].DemorganTime != 0)
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вам осталось сидеть {Convert.ToInt32(Main.Players[player].DemorganTime / 60.0)} минут", 3000);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("ptime")]
        public static void CMD_pcheckPrisonTime(Player player, int id)
        {
            try
            {
                if (!Group.CanUseCmd(player, "a")) return;
                Player target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (Main.Players[target].ArrestTime != 0)
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Игроку {target.Name} осталось сидеть {Convert.ToInt32(Main.Players[target].ArrestTime / 60.0)} минут", 3000);
                else if (Main.Players[target].DemorganTime != 0)
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Игроку {target.Name} осталось сидеть {Convert.ToInt32(Main.Players[target].DemorganTime / 60.0)} минут", 3000);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("sellcars")]
        public static void CMD_sellCars(Player player)
        {
            try
            {
                Houses.HouseManager.OpenCarsSellMenu(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("dep", GreedyArg = true)]
        public static void CMD_govFracChat(Player player, string msg)
        {
            try
            {
                Fractions.Manager.govFractionChat(player, msg);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("gov", GreedyArg = true)]
        public static void CMD_gov(Player player, string msg)
        {
            try
            {
                if (!Fractions.Manager.canUseCommand(player, "gov")) return;
                int frac = Main.Players[player].FractionID;
                int lvl = Main.Players[player].FractionLVL;
                string[] split = player.Name.Split('_');

                NAPI.Chat.SendChatMessageToAll($"~y~[{Fractions.Manager.GovTags[frac]} | {split[0]} {split[1]}] {msg}");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("call", GreedyArg = true)]
        public static void CMD_gov(Player player, int number, string msg)
        {
            try
            {
                if (number == 112)
                    Fractions.Police.callPolice(player, msg);
                else if (number == 911)
                    Fractions.Ems.callEms(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        // [Command("q")]
        // public static void CMD_disconnect(Player player)
        // {
            // Trigger.PlayerEvent(player, "quitcmd");
        // }

        [Command("report", GreedyArg = true)]
        public static void CMD_report(Player player, string message)
        {
            try
            {
                if (message.Length > 150) //вернуть 150 символов
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Слишком длинное сообщение!", 3000); // вернуть текст - Слишком длинное сообщение
                    return;
                }
                if (Main.Accounts[player].VipLvl == 0 && player.HasData("NEXT_REPORT"))
                {
                    DateTime nextReport = player.GetData<DateTime>("NEXT_REPORT");
                    if (DateTime.Now < nextReport)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Попробуйте отправить жалобу через некоторое время", 3000);
                        return;
                    }
                }
                if (player.HasData("MUTE_TIMER")) {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нельзя отправлять репорт во время Mute, дождитесь его окончания", 3000);
                    return;
                }
                ReportSys.AddReport(player, message);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("takegunlic")]
        public static void CMD_takegunlic(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.takeGunLic(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("givegunlic")]
        public static void CMD_givegunlic(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.giveGunLic(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("pd")]
        public static void CMD_policeAccept(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.Police.acceptCall(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("eject")]
        public static void CMD_ejectTarget(Player player, int id)
        {
            try
            {
                var target = Main.GetPlayerByID(id);
                if (player.GetData<bool>("CUFFED")) return;
                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (!player.IsInVehicle || player.VehicleSeat != 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не находитесь в машине или не на пассажирском месте", 3000);
                    return;
                }
                if (!target.IsInVehicle || player.Vehicle != target.Vehicle) return;
                VehicleManager.WarpPlayerOutOfVehicle(target);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выкинули игрока ({target.Value}) из машины", 3000);
                Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Игрок ({player.Value}) выкинул Вас из машины", 3000);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("ems")]
        public static void CMD_emsAccept(Player player, int id)
        {
            try
            {
                try { Convert.ToInt32(id); }
                catch { return; };

                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.Ems.acceptCall(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("pocket")]
        public static void CMD_pocketTarget(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                if (player.Position.DistanceTo(Main.GetPlayerByID(id).Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок слишком далеко", 3000);
                    return;
                }

                Fractions.FractionCommands.playerChangePocket(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("setrank")]
        public static void CMD_setRank(Player player, int id, int newrank)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрока с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.SetFracRank(player, Main.GetPlayerByID(id), newrank);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("invite")]
        public static void CMD_inviteFrac(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.InviteToFraction(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("uninvite")]
        public static void CMD_uninviteFrac(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.UnInviteFromFraction(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("f", GreedyArg = true)]
        public static void CMD_fracChat(Player player, string msg)
        {
            try
            {
                Fractions.Manager.fractionChat(player, msg);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("fb", GreedyArg = true)]
        public static void CMD_fracChatB(Player player, string msg)
        {
            try
            {
                Fractions.Manager.fractionChat(player, "(( " + msg + " ))");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("arrest")]
        public static void CMD_arrest(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.arrestTarget(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("rfp")]
        public static void CMD_rfp(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.releasePlayerFromPrison(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("follow")]
        public static void CMD_follow(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.targetFollowPlayer(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("unfollow")]
        public static void CMD_unfollow(Player player)
        {
            try
            {
                Fractions.FractionCommands.targetUnFollowPlayer(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("su", GreedyArg = true)]
        public static void CMD_suByPassport(Player player, int pass, int stars, string reason)
        {
            try
            {
                Fractions.FractionCommands.suPlayer(player, pass, stars, reason);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("c")]
        public static void CMD_getCoords(Player player)
        {
            try
            {
                if (!Group.CanUseCmd(player, "a")) return;
                NAPI.Chat.SendChatMessageToPlayer(player, "Coords", NAPI.Entity.GetEntityPosition(player).ToString());
                NAPI.Chat.SendChatMessageToPlayer(player, "Rot", NAPI.Entity.GetEntityRotation(player).ToString());
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("incar")]
        public static void CMD_inCar(Player player, int id, int trunk)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.playerInCar(player, Main.GetPlayerByID(id), trunk);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("pull")]
        public static void CMD_pullOut(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.playerOutCar(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("warg")]
        public static void CMD_warg(Player player)
        {
            try
            {
                Fractions.FractionCommands.setWargPoliceMode(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("medkit")]
        public static void CMD_medkit(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.sellMedKitToTarget(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("accept")]
        public static void CMD_accept(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.acceptEMScall(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("heal")]
        public static void CMD_heal(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Fractions.FractionCommands.healTarget(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("capture")]
        public static void CMD_capture(Player player)
        {
            try
            {
                Fractions.GangsCapture.CMD_startCapture(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("repair")]
        public static void CMD_mechanicRepair(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Jobs.AutoMechanic.mechanicRepair(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("sellfuel")]
        public static void CMD_mechanicSellFuel(Player player, int id, int fuel, int pricePerLitr)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Jobs.AutoMechanic.mechanicFuel(player, Main.GetPlayerByID(id), fuel, pricePerLitr);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("buyfuel")]
        public static void CMD_mechanicBuyFuel(Player player, int fuel)
        {
            try
            {
                Jobs.AutoMechanic.buyFuel(player, fuel);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("ma")]
        public static void CMD_acceptMechanic(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Jobs.AutoMechanic.acceptMechanic(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("cmechanic")]
        public static void CMD_cancelMechanic(Player player)
        {
            try
            {
                Jobs.AutoMechanic.cancelMechanic(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("tprice")]
        public static void CMD_tprice(Player player, int id, int price)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Jobs.Taxi.offerTaxiPay(player, Main.GetPlayerByID(id), price);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("ta")]
        public static void CMD_taxiAccept(Player player, int id)
        {
            try
            {
                if (Main.GetPlayerByID(id) == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }
                Jobs.Taxi.acceptTaxi(player, Main.GetPlayerByID(id));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
		
[Command("armorelp")]
        public static void CMD_armorelp(Player Player, int id)
        {
            try
            {
                if (!Group.CanUseCmd(Player, "armorelp")) return;
                Player target = Main.GetPlayerByID(id);
                if (target == null)
                {
                    Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок с таким ID не найден", 3000);
                    return;
                }
                target.StopAnimation();
                NAPI.Entity.SetEntityPosition(target, target.Position + new Vector3(0, 0, 0.5));
                target.SetSharedData("InDeath", false);
                Trigger.PlayerEvent(target, "DeathTimer", false);
                target.Health = 100;
                target.ResetData("IS_DYING");
                target.ResetData("IS_STATE");
                Main.Players[target].IsAlive = true;
                Main.OffAntiAnim(target);
                if (target.HasData("DYING_TIMER"))
                {
                    Timers.Stop(target.GetData<string>("DYING_TIMER"));
                    target.ResetData("DYING_TIMER");
                }
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({Player.Value}) реанимировал Вас", 3000);
                Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы реанимировали Гражданина ({target.Value})", 3000);

                if (target.HasData("CALLEMS_BLIP"))
                {
                    NAPI.Entity.DeleteEntity(target.GetData<Blip>("CALLEMS_BLIP"));
                }
                if (target.HasData("CALLEMS_COL"))
                {
                    NAPI.ColShape.DeleteColShape(target.GetData<ColShape>("CALLEMS_COL"));
                }
            }
            catch
            {

            }

        }
		
        [Command("ctaxi")]
        public static void CMD_cancelTaxi(Player player)
        {
            try
            {
                Jobs.Taxi.cancelTaxi(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("taxi")]
        public static void CMD_callTaxi(Player player)
        {
            try
            {
                Jobs.Taxi.callTaxi(player);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        [Command("me", GreedyArg = true)]
        public static async Task CMD_chatMe(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute/60} минут", 3000);
                return;
            }
            msg = RainbowExploit(player, msg);
            await RPChatAsync("me", player, msg);
        }

        [Command("do", GreedyArg = true)]
        public static async Task CMD_chatDo(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute/60} минут", 3000);
                return;
            }
            msg = RainbowExploit(player, msg);
            await RPChatAsync("do", player, msg);
        }

        [Command("todo", GreedyArg = true)]
        public static async Task CMD_chatToDo(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute/60} минут", 3000);
                return;
            }
            await RPChatAsync("todo", player, msg);
        }

        [Command("s", GreedyArg = true)]
        public static async Task CMD_chatS(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute/60} минут", 3000);
                return;
            }
            await RPChatAsync("s", player, msg);
        }

        [Command("b", GreedyArg = true)]
        public static async Task CMD_chatB(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute/60} минут", 3000);
                return;
            }
            await RPChatAsync("b", player, msg);
        }

        [Command("vh", GreedyArg = true)]
        public static async Task CMD_chatVh(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute/60} минут", 3000);
                return;
            }
            await RPChatAsync("vh", player, msg);
        }

        [Command("m", GreedyArg = true)]
        public static async Task CMD_chatM(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute/60} минут", 3000);
                return;
            }
            await RPChatAsync("m", player, msg);
        }

        [Command("t", GreedyArg = true)]
        public static async Task CMD_chatT(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute/60} минут", 3000);
                return;
            }
            await RPChatAsync("t", player, msg);
        }

        [Command("try", GreedyArg = true)]
        public static void CMD_chatTry(Player player, string msg)
        {
            if (Main.Players[player].Unmute > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[player].Unmute/60} минут", 3000);
                return;
            }
            Try(player, msg);
        }
        [Command("dice")]
        public static void CMD_chatDice(Player player, int id, int summa )
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;

                Player target = Main.GetPlayerByID(id);

                if (target == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                    return;
                }

                if (player.Position.DistanceTo2D(target.Position) > 10)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин слишком далеко от вас", 3000);
                    return;
                }

                if (summa > Main.Players[player].Money)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет столько денег!", 3000);
                    return;
                }

                if (summa <= 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Неправильное значение суммы", 3000);
                    return;
                }

                target.SetData("DICE_MONEY", summa);
                target.SetData("DICE_TARGET", player);

                Trigger.PlayerEvent(target, "openDialog", "DICE", $"Вам предлагают сыграть в кости ({summa}$) Гражданин({player.Value})");
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы кинули запрос на игру в кости Гражданину({id}))", 3000);
            }
            catch { }
        }

        #region Try command handler
        private static void Try(Player sender, string message)
        {
            try
            {
                //Random
                int result = rnd.Next(5);
                Log.Debug("Random result: " + result.ToString());
                //
                switch (result)
                {
                    case 3:
                        foreach (Player player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.PlayerEvent(player, "sendRPMessage", "try", "!{#BF11B7}{name} " + message + " | !{#277C6B}" + " удачно", new int[] { sender.Value });
                        return;
                    default:
                        foreach (Player player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.PlayerEvent(player, "sendRPMessage", "try", "!{#BF11B7}{name} " + message + " | !{#FF0707}" + " неудачно", new int[] { sender.Value });
                        return;
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        private static void Kosti(Player sender)
        {
            try
            {
                //Random
                int result = rnd.Next(7);
                Log.Debug("Random result: " + result.ToString());
                //
                sender.SendChatMessage("!{#30C204}" + $"{sender.Name}" + " на кубике выпало число: " + "!{#FFFFFF}" + $"{result}");
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }

        #endregion Try command handler



        #region RP Chat
        public static void RPChat(string cmd, Player sender, string message, Player target = null)
        {
            try
            {
                if (!Main.Players.ContainsKey(sender)) return;
                var names = new int[] { sender.Value };
                if (target != null) names = new int[] { sender.Value, target.Value };
                switch (cmd)
                {
                    case "me":
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.PlayerEvent(player, "sendRPMessage", "me", "!{#E066FF}{name} " + message, names);
                        return;
                    case "todo":
                        var args = message.Split('*');
                        var msg = args[0];
                        var action = args[1];
                        var genderCh = (Main.Players[sender].Gender) ? "" : "а";
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.PlayerEvent(player, "sendRPMessage", "todo", msg + ",!{#E066FF} - сказал" + genderCh + " {name}, " + action, names);
                        return;
                    case "do":
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.PlayerEvent(player, "sendRPMessage", "do", "!{#EA6875}" + message + " ({name})", names);
                        return;
                    case "s":
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 30f, sender.Dimension))
                            Trigger.PlayerEvent(player, "sendRPMessage", "s", "{name} кричит: " + message, names);
                        return;
                    case "b":
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                            Trigger.PlayerEvent(player, "sendRPMessage", "b", "(( {name}: " + message + " ))", names);
                        return;
                    case "m":
                        if ((Main.Players[sender].FractionID != 7 && Main.Players[sender].FractionID != 9) || !NAPI.Player.IsPlayerInAnyVehicle(sender)) return;
                        var vehicle = sender.Vehicle;
                        if (vehicle.GetData<string>("ACCESS") != "FRACTION") return;
                        if (vehicle.GetData<int>("FRACTION") != 7 && vehicle.GetData<int>("FRACTION") != 9) return;
                        foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 120f, sender.Dimension))
                            Trigger.PlayerEvent(player, "sendRPMessage", "m", "~r~[Мегафон] {name}: " + message, names);
                        return;
                    case "t":
                        if (!Main.Players.ContainsKey(sender) || Main.Players[sender].WorkID != 6) return;
                        foreach (var p in NAPI.Pools.GetAllPlayers())
                        {
                            if (p == null || !Main.Players.ContainsKey(p)) continue;

                            if (Main.Players[p].WorkID == 6)
                            {
                                if (p.HasData("ON_WORK") && p.GetData<bool>("ON_WORK") && p.IsInVehicle)
                                    p.SendChatMessage($"~y~[Рация дальнобойщиков] [{sender.Name}]: {message}");
                            }
                        }
                        return;
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
        }
        public static Task RPChatAsync(string cmd, Player sender, string message)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!Main.Players.ContainsKey(sender)) return;
                    var names = new int[] { sender.Value };
                    switch (cmd)
                    {
                        case "me":
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                                Trigger.PlayerEvent(player, "sendRPMessage", "me", "!{#E066FF}{name} " + message, names);
                            return;
                        case "todo":
                            var args = message.Split('*');
                            var msg = args[0];
                            var action = args[1];
                            var genderCh = (Main.Players[sender].Gender) ? "" : "а";
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                                Trigger.PlayerEvent(player, "sendRPMessage", "todo", msg + ",!{#E066FF} - сказал" + genderCh + " {name}, " + action, names);
                            return;
                        case "do":
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                                Trigger.PlayerEvent(player, "sendRPMessage", "do", "!{#EA6875}" + message + " ({name})", names);
                            return;
                        case "s":
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 30f, sender.Dimension))
                                Trigger.PlayerEvent(player, "sendRPMessage", "s", "{name} кричит: " + message, names);
                            return;
                        case "b":
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 10f, sender.Dimension))
                                Trigger.PlayerEvent(player, "sendRPMessage", "b", "(( {name}: " + message + " ))", names);
                            return;
                        case "m":
                            if ((Main.Players[sender].FractionID != 7 && Main.Players[sender].FractionID != 9) || !NAPI.Player.IsPlayerInAnyVehicle(sender)) return;
                            var vehicle = sender.Vehicle;
                            if (vehicle.GetData<string>("ACCESS") != "FRACTION") return;
                            if (vehicle.GetData<int>("FRACTION") != 7 && vehicle.GetData<int>("FRACTION") != 9) return;
                            foreach (var player in Main.GetPlayersInRadiusOfPosition(sender.Position, 120f, sender.Dimension))
                                Trigger.PlayerEvent(player, "sendRPMessage", "m", "!{#FF4D4D}[Мегафон] {name}: " + message, names);
                            return;
                        case "t":
                            if (!Main.Players.ContainsKey(sender) || Main.Players[sender].WorkID != 6) return;
                            foreach (var p in NAPI.Pools.GetAllPlayers())
                            {
                                if (p == null || !Main.Players.ContainsKey(p)) continue;

                                if (Main.Players[p].WorkID == 6)
                                {
                                    if (p.HasData("ON_WORK") && p.GetData<bool>("ON_WORK") && p.IsInVehicle)
                                        p.SendChatMessage($"~y~[Рация дальнобойщиков] [{sender.Name}]: {message}");
                                }
                            }
                            return;
                    }
                }
                catch (Exception e) { Log.Write("EXCEPTION AT \"CMD\":\n" + e.ToString(), nLog.Type.Error); }
            });
            return Task.CompletedTask;
        }
        #endregion RP Chat
    }
}