using System;
using System.Collections.Generic;
using System.Data;
using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using NeptuneEVO.GUI;
using NeptuneEVO.Core.Character;
using Newtonsoft.Json;

namespace NeptuneEVO.Fractions
{
    class Police : Script
    {
        private static nLog Log = new nLog("Police");


        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        public static List<Vector3> policeCheckpoints = new List<Vector3>()
        {
            new Vector3(484.82407, -1008.7053, 25.15315), // метка возле камеры       0 ------OK
            new Vector3(484.2706, -1001.7895, 24.614668), // guns     1 ------OK
            new Vector3(470.81918, -987.36914, 24.614666), // переодевалка     2 ------OK
            new Vector3(473.56644, -987.7084, 24.614664), // режим ЧС     3 ------OK
            new Vector3(485.73236, -1005.90594, 26.153131), // корды спавна в камере   4  ------OK
            new Vector3(426.17114, -976.3408, 30.59056), // место спавна из тюрьмы     5 ------OK
            new Vector3(438.53647, -981.85815, 29.569485), // buy gun licence     6 ------OK
            new Vector3(441.7351, -981.3741, 30.26933), // сдать сумку    7 ------OK
            new Vector3(485.2441, -1006.7815, 24.614536),  // открыть склад     8 ------OK
            new Vector3(451.5937, -976.125, 25.57979),  // ускорение авто       9 ------OK
			
			//ТЮРЬМА
			new Vector3(1712.96, 2576.008, -44.4687), // метка возле камеры       0 / 10
			new Vector3(1675.039, 2587.174, -44.46871), // корды спавна в камере  4 / 11
			new Vector3(1854.8, 2615.034, -44.552), // место спавна из тюрьмы     5 /12
            //ШТРАФ СТОЯНКА
            new Vector3(452.0411, -1075.1226, 28.09104), // место где взять транспорт /13
            new Vector3(450.5819, -1066.2113, 28.09104), // место где сдать транспорт /14
            //КОМПЬЮТЕР
            new Vector3(452.59915, -985.1671, 29.569492), // включить комп
        };

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                //NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("v_ilev_arm_secdoor"), new Vector3(453.0793, -983.1894, 30.83926), 30f);

                //NAPI.TextLabel.CreateTextLabel("~g~Alonzo Harris", new Vector3(452.2527, -993.119, 31.6896), 5f, 0.4f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
                //NAPI.TextLabel.CreateTextLabel("~g~Nancy Spungen", new Vector3(441.169, -978.3074, 31.6896), 5f, 0.4f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
                //NAPI.TextLabel.CreateTextLabel("~g~Bones Bulldog", new Vector3(454.121, -980.0575, 31.6896), 5f, 0.4f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);

                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[0], 6, 3, 0));
                Cols[0].OnEntityEnterColShape += arrestShape_onEntityEnterColShape;
                Cols[0].OnEntityExitColShape += arrestShape_onEntityExitColShape;

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[1], 1, 2, 0));
                Cols[1].SetData("INTERACT", 10);
                Cols[1].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[1].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Выдача оружия"), new Vector3(policeCheckpoints[1].X, policeCheckpoints[1].Y, policeCheckpoints[1].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(2, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[2], 1, 2, 0));
                Cols[2].SetData("INTERACT", 11);
                Cols[2].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[2].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Переодевалка"), new Vector3(policeCheckpoints[2].X, policeCheckpoints[2].Y, policeCheckpoints[2].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(3, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[3], 1, 2, 0));
                Cols[3].SetData("INTERACT", 12);
                Cols[3].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[3].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Режим ЧС"), new Vector3(policeCheckpoints[3].X, policeCheckpoints[3].Y, policeCheckpoints[3].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(5, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[7], 1, 2, 0));
                Cols[5].SetData("INTERACT", 42);
                Cols[5].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[5].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Сдать награбленное"), new Vector3(policeCheckpoints[7].X, policeCheckpoints[7].Y, policeCheckpoints[7].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(6, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[8], 1, 2, 0));
                Cols[6].SetData("INTERACT", 59);
                Cols[6].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[6].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Склад оружия"), new Vector3(policeCheckpoints[8].X, policeCheckpoints[8].Y, policeCheckpoints[8].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(7, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[9], 4, 5, 0));
                Cols[7].SetData("INTERACT", 66);
                Cols[7].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[7].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Тюнинг авто"), new Vector3(policeCheckpoints[9].X, policeCheckpoints[9].Y, policeCheckpoints[9].Z + 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));
				
				Cols.Add(10, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[10], 6, 3, 0));
                Cols[10].OnEntityEnterColShape += arrestShape_onEntityEnterColShapeL;
                Cols[10].OnEntityExitColShape += arrestShape_onEntityExitColShapeL;

                Cols.Add(13, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[13], 1, 2, 0));
                Cols[13].SetData("INTERACT", 527);
                Cols[13].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[13].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel("~h~~b~Штрафстоянка", new Vector3(policeCheckpoints[13].X, policeCheckpoints[13].Y, policeCheckpoints[13].Z + 1f), 5F, 0.5F, 0, new Color(255, 255, 255), true, 0);

                Cols.Add(14, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[14], 3, 5, 0));
                Cols[14].SetData("INTERACT", 528);
                Cols[14].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[14].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel("~h~~b~Поставить на Штрафстоянку", new Vector3(policeCheckpoints[14].X, policeCheckpoints[14].Y, policeCheckpoints[14].Z + 1f), 5F, 0.5F, 0, new Color(255, 255, 255), true, 0);

                Cols.Add(15, NAPI.ColShape.CreateCylinderColShape(policeCheckpoints[15], 1, 2, 0));
                Cols[15].SetData("INTERACT", 529);
                Cols[15].OnEntityEnterColShape += onEntityEnterColshape;
                Cols[15].OnEntityExitColShape += onEntityExitColshape;
                NAPI.TextLabel.CreateTextLabel("~h~~b~Проверка по базе", new Vector3(policeCheckpoints[15].X, policeCheckpoints[15].Y, policeCheckpoints[15].Z + 1f), 5F, 0.5F, 0, new Color(255, 255, 255), true, 0);

                NAPI.Marker.CreateMarker(1, policeCheckpoints[1] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, policeCheckpoints[2] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, policeCheckpoints[3] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, policeCheckpoints[7] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, policeCheckpoints[8] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, policeCheckpoints[9] - new Vector3(0, 0, 3.7), new Vector3(), new Vector3(), 4, new Color(255, 0, 0, 220));
				NAPI.Marker.CreateMarker(1, policeCheckpoints[10] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));

                NAPI.Marker.CreateMarker(1, policeCheckpoints[14] - new Vector3(0, 0, 2f), new Vector3(), new Vector3(), 2.965f, new Color(0, 175, 250, 220), false, 0);
                NAPI.Marker.CreateMarker(27, policeCheckpoints[14] + new Vector3(0, 0, 0.14f), new Vector3(), new Vector3(), 3f, new Color(0, 175, 250, 220), false, 0);

                NAPI.Marker.CreateMarker(1, policeCheckpoints[15], new Vector3(), new Vector3(), 0.965f, new Color(0, 175, 250, 220), false, 0);
                NAPI.Marker.CreateMarker(27, policeCheckpoints[15] + new Vector3(0, 0, 0.14f), new Vector3(), new Vector3(), 1f, new Color(0, 175, 250, 220), false, 0);

                NAPI.Blip.CreateBlip(636, policeCheckpoints[13], 0.7f, 38, Main.StringToU16("Штрафстоянка"), 255, 0, true, 0, 0);
                NAPI.Marker.CreateMarker(1, policeCheckpoints[13] - new Vector3(0, 0, 0.5f), new Vector3(), new Vector3(), 0.965f, new Color(0, 175, 250, 220), false, 0);
                NAPI.Marker.CreateMarker(27, policeCheckpoints[13] + new Vector3(0, 0, 0.14f), new Vector3(), new Vector3(), 1f, new Color(0, 175, 250, 220), false, 0);

            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
		
		private void arrestShape_onEntityEnterColShapeL(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "IS_IN_ARREST_AREA", true);
            }
            catch (Exception ex) { Log.Write("arrestShape_onEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
        }

        private void arrestShape_onEntityExitColShapeL(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                NAPI.Data.SetEntityData(player, "IS_IN_ARREST_AREA", false);
                if (Main.Players[player].ArrestTime != 0)
                {
                    NAPI.Entity.SetEntityPosition(player, Police.policeCheckpoints[12]);
                }
            }
            catch (Exception ex) { Log.Write("arrestShape_onEntityExitColShape: " + ex.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_OnPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (player.VehicleSeat != 0 || player.VehicleSeat != 1) return;
                if (Main.Players[player].FractionID != 7 || Main.Players[player].FractionID != 9) return;
                Trigger.PlayerEvent(player, "closePc");
            }
            catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }

        public static void callPolice(Player player, string reason)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (Manager.countOfFractionMembers(7) == 0 && Manager.countOfFractionMembers(9) == 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нет полицейских в Вашем районе. Попробуйте позже", 3000);
                        return;
                    }
                    if (player.HasData("NEXTCALL_POLICE") && DateTime.Now < player.GetData<DateTime>("NEXTCALL_POLICE"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы уже вызвали полицию, попробуйте позже", 3000);
                        return;
                    }
                    player.SetData("NEXTCALL_POLICE", DateTime.Now.AddMinutes(7));

                    if (player.HasData("CALLPOLICE_BLIP"))
                        NAPI.Entity.DeleteEntity(player.GetData<Blip>("CALLPOLICE_BLIP"));

                    var Blip = NAPI.Blip.CreateBlip(0, player.Position, 1, 70, "Call from " + player.Name.Replace('_', ' ') + $" ({player.Value})", 0, 0, true, 0, 0);
                    Blip.Transparency = 0;
                    foreach (var p in NAPI.Pools.GetAllPlayers())
                    {
                        if (!Main.Players.ContainsKey(p)) continue;
                        if (Main.Players[p].FractionID != 7 && Main.Players[p].FractionID != 9) continue;
                        p.TriggerEvent("changeBlipAlpha", Blip, 255);
                    }
                    player.SetData("CALLPOLICE_BLIP", Blip);

                    var colshape = NAPI.ColShape.CreateCylinderColShape(player.Position, 70, 4, 0);
                    colshape.OnEntityExitColShape += (s, e) =>
                    {
                        if (e == player)
                        {
                            try
                            {
                                Blip.Delete();
                                e.ResetData("CALLPOLICE_BLIP");

                                Manager.sendFractionMessage(7, $"{e.Name.Replace('_', ' ')} отменил вызов");
                                Manager.sendFractionMessage(9, $"{e.Name.Replace('_', ' ')} отменил вызов");

                                colshape.Delete();

                                e.ResetData("CALLPOLICE_COL");
                                e.ResetData("IS_CALLPOLICE");
                            }
                            catch (Exception ex) { Log.Write("EnterPoliceCall: " + ex.Message); }
                        }
                    };
                    player.SetData("CALLPOLICE_COL", colshape);

                    player.SetData("IS_CALLPOLICE", true);
                    Manager.sendFractionMessage(7, $"Поступил вызов от гражданина ({player.Value}) - {reason}");
                    Manager.sendFractionMessage(7, $"~b~Поступил вызов от гражданина ({player.Value}) - {reason}", true);
                    Manager.sendFractionMessage(9, $"Поступил вызов от гражданинка ({player.Value}) - {reason}");
                    Manager.sendFractionMessage(9, $"~b~Поступил вызов от гражданина ({player.Value}) - {reason}", true);
                }
                catch { }
            });
        }

        public static void acceptCall(Player player, Player target)
        {
            try
            {
                if (!Manager.canUseCommand(player, "pd")) return;
                if (target == null || !NAPI.Entity.DoesEntityExist(target)) return;
                if (!target.HasData("IS_CALLPOLICE"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин не вызывал полицию или этот вызов уже кто-то принял", 3000);
                    return;
                }
                Blip blip = target.GetData<Blip>("CALLPOLICE_BLIP");

                Trigger.PlayerEvent(player, "changeBlipColor", blip, 38);
                Trigger.PlayerEvent(player, "createWaypoint", blip.Position.X, blip.Position.Y);

                ColShape colshape = target.GetData<ColShape>("CALLPOLICE_COL");
                colshape.OnEntityEnterColShape += (s, e) =>
                {
                    if (e == player)
                    {
                        try
                        {
                            NAPI.Task.Run(() =>
                            {
                                try
                                {
                                    NAPI.Entity.DeleteEntity(target.GetData<Blip>("CALLPOLICE_BLIP"));
                                    target.ResetData("CALLPOLICE_BLIP");
                                    colshape.Delete();
                                }
                                catch { }
                            });
                        }
                        catch (Exception ex) { Log.Write("EnterPoliceCall: " + ex.Message); }
                    }
                };

                Manager.sendFractionMessage(7, $"{player.Name.Replace('_', ' ')} принял вызов от гражданина ({target.Value})");
                Manager.sendFractionMessage(7, $"~b~{player.Name.Replace('_', ' ')} принял вызов от гражданина ({target.Value})", true);
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) принял Ваш вызов", 3000);
            }
            catch
            {
            }
        }

        [RemoteEvent("clearWantedLvl")]
        public static void clearWantedLvl(Player sender, params object[] arguments)
        {
            try
            {
                var target = (string)arguments[0];
                Player player = null;
                try
                {
                    var pasport = Convert.ToInt32(target);
                    if (!Main.PlayerNames.ContainsKey(pasport))
                    {
                        Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomRight, $"Паспорта с таким номером не существует", 3000);
                        return;
                    }
                    player = NAPI.Player.GetPlayerFromName(Main.PlayerNames[pasport]);
                    target = Main.PlayerNames[pasport];
                }
                catch
                {
                    target.Replace(' ', '_');
                    if (!Main.PlayerNames.ContainsValue(target))
                    {
                        Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomRight, $"Гражданин не найден", 3000);
                        return;
                    }
                    player = NAPI.Player.GetPlayerFromName(target);
                }

                var split = target.Split('_');
                MySQL.Query($"UPDATE characters SET wanted=null WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                try
                {
                    setPlayerWantedLevel(player, null);
                }
                catch { }
                Notify.Send(sender, NotifyType.Success, NotifyPosition.BottomRight, $"Вы сняли розыск с владельца паспорта {target}", 3000);
            }
            catch (Exception e) { Log.Write("ClearWantedLvl: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("checkNumber")]
        public static void checkNumber(Player sender, params object[] arguments)
        {
            try
            {
                var number = (string)arguments[0];
                VehicleManager.VehicleData vehicle;
                try
                {
                    vehicle = VehicleManager.Vehicles[number];
                }
                catch
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Машины с таким номером не найдено", 3000);
                    return;
                }
                Trigger.PlayerEvent(sender, "executeCarInfo", Convert.ToString(ParkManager.GetNormalName( vehicle.Model )), vehicle.Holder.Replace('_', ' ') + ( VehicleManager.HavePlactic(number) ? " [Имеется пластик]" : " [Нет пластика]"));
            }
            catch (Exception e) { Log.Write("checkNumber: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("checkPerson")]
        public static void checkPerson(Player sender, params object[] arguments)
        {
            try
            {
                var target = (string)arguments[0];
                Player player = null;
                try
                {
                    var pasport = Convert.ToInt32(target);
                    if (!Main.PlayerNames.ContainsKey(pasport))
                    {
                        Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomRight, $"Паспорта с таким номером не существует", 3000);
                        return;
                    }
                    player = NAPI.Player.GetPlayerFromName(Main.PlayerNames[pasport]);
                    target = Main.PlayerNames[pasport];
                }
                catch
                {
                    target.Replace(' ', '_');
                    if (!Main.PlayerNames.ContainsValue(target))
                    {
                        Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomRight, $"Гражданин не найден", 3000);
                        return;
                    }
                    player = NAPI.Player.GetPlayerFromName(target);
                }

                try
                {
                    var acc = Main.Players[player];
                    var wantedLvl = (acc.WantedLVL == null) ? 0 : acc.WantedLVL.Level;
                    var gender = (acc.Gender) ? "Мужской" : "Женский";
                    var lic = "";
                    for (int i = 0; i < acc.Licenses.Count; i++)
                        if (acc.Licenses[i]) lic += $"{Main.LicWords[i]} / ";
                    if (lic == "") lic = "Отсутствуют";

                    var states = acc.States;

                    var strstates = "";

                    if (states[0] == 0 && states[1] == 0)
                        strstates = "Нет";
                    else
                        strstates = $"Убийств: {states[0]}, Другие: {states[1]}";

                    var number = acc.Sim;

                    Trigger.PlayerEvent(sender, "executePersonInfo", $"{acc.FirstName}", $"{acc.LastName}", $"{acc.UUID}", $"{gender}", $"{wantedLvl}", $"{lic}", $"{number}", strstates);
                }
                catch
                {
                    var split = target.Split('_');
                    var result = MySQL.QueryRead($"SELECT * FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                    foreach (DataRow Row in result.Rows)
                    {
                        var firstName = Convert.ToString(Row["firstname"]);
                        var lastName = Convert.ToString(Row["lastname"]);
                        var genderBool = Convert.ToBoolean(Row["gender"]);
                        var uuid = Convert.ToInt32(Row["uuid"].ToString());
                        var gender = (genderBool) ? "Мужской" : "Женский";
                        var wanted = Newtonsoft.Json.JsonConvert.DeserializeObject<WantedLevel>(Row["wanted"].ToString());
                        var wantedLvl = (wanted == null) ? 0 : wanted.Level;
                        var licenses = Newtonsoft.Json.JsonConvert.DeserializeObject<List<bool>>(Convert.ToString(Row["licenses"]));
                        var states = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(Convert.ToString(Row["states"]));

                        var strstates = "";

                        if (states[0] == 0 && states[1] == 0)
                            strstates = "Нет";
                        else
                            strstates = $"Убийств: {states[0]}, Другие: {states[1]}";

                        var lic = "";
                        for (int i = 0; i < licenses.Count; i++)
                            if (licenses[i]) lic += $"{Main.LicWords[i]} / ";
                        if (lic == "") lic = "Отсутствуют";

                        Trigger.PlayerEvent(sender, "executePersonInfo", $"{firstName}", $"{lastName}", $"{uuid}", $"{gender}", $"{wantedLvl}", $"{lic}", "Лицензия на оружие", "Водительские права", strstates);
                    }
                }
            }
            catch (Exception e) { Log.Write("checkPerson: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("checkWantedList")]
        public static void checkWantedList(Player sender, params object[] arguments)
        {
            try
            {
                List<string> list = new List<string>();
                foreach (var p in NAPI.Pools.GetAllPlayers())
                {
                    if (!Main.Players.ContainsKey(p)) continue;
                    var acc = Main.Players[p];
                    var wantedLvl = (acc.WantedLVL == null) ? 0 : acc.WantedLVL.Level;
                    if (wantedLvl != 0) list.Add($"{acc.FirstName} {acc.LastName} - {wantedLvl}*");
                }
                var json = JsonConvert.SerializeObject(list);
                Log.Debug(json);
                Trigger.PlayerEvent(sender, "executeWantedList", json);
            }
            catch (Exception e) { Log.Write("checkWantedList: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("openCopCarMenu")]
        public static void openCopcarmenu(Player sender, params object[] arguments)
        {
            try
            {
                if (!NAPI.Player.IsPlayerInAnyVehicle(sender)) return;
                var vehicle = sender.Vehicle;
                if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "FRACTION" &&
                    (NAPI.Data.GetEntityData(vehicle, "FRACTION") == 7 || NAPI.Data.GetEntityData(vehicle, "FRACTION") == 9) &&
                    (sender.VehicleSeat == 0 || sender.VehicleSeat == 1))
                {
                    MenuManager.Close(sender);
                    if (Main.Players[sender].FractionID == 7 || Main.Players[sender].FractionID == 9)
                    {
                        Trigger.PlayerEvent(sender, "openPc");
                        Commands.RPChat("me", sender, "включил(а) бортовой компьютер");
                    }
                }
                return;
            }
            catch (Exception e) { Log.Write("openCopCarMenu: " + e.Message, nLog.Type.Error); }
        }

        public static void Event_PlayerDeath(Player player, Player killer, uint reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    if (NAPI.Data.GetEntityData(player, "IN_CP_MODE"))
                    {
                        Manager.setSkin(player, Main.Players[player].FractionID, Main.Players[player].FractionLVL);
                        NAPI.Data.SetEntityData(player, "IN_CP_MODE", false);
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }

        public static void interactPressed(Player player, int interact)
        {
            if (!Main.Players.ContainsKey(player)) return;
            switch (interact)
            {
                case 10:
                    if (Main.Players[player].FractionID != 7)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник полиции", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[7].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    OpenPoliceGunMenu(player);
                    return;
                case 11:
                    beginWorkDay(player);
                    break;
                case 12:
                    if (Main.Players[player].FractionID != 7)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник полиции", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (!is_warg)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Не включен режим ЧП", 3000);
                        return;
                    }
                    OpenSpecialPoliceMenu(player);
                    return;
                case 42:
                    if (!player.HasData("HAND_MONEY") && !player.HasData("HEIST_DRILL"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ни сумки с деньгами, ни сумки с дрелью", 3000);
                        return;
                    }
                    if (player.HasData("HAND_MONEY"))
                    {
                        nInventory.Remove(player, ItemType.BagWithMoney, 1);
                        player.SetClothes(5, 0, 0);
                        player.ResetData("HAND_MONEY");
                    }
                    if (player.HasData("HEIST_DRILL"))
                    {
                        nInventory.Remove(player, ItemType.BagWithDrill, 1);
                        player.SetClothes(5, 0, 0);
                        player.ResetData("HEIST_DRILL");
                    }
                    MoneySystem.Wallet.Change(player, 175);
                    GameLog.Money($"server", $"player({Main.Players[player].UUID})", 175, $"policeAward");
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили вознаграждение в 175$", 3000);
                    return;
                case 44:
                    if (Main.Players[player].Licenses[6])
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть лицензия на оружие", 3000);
                        return;
                    }
                    if (!MoneySystem.Wallet.Change(player, -2150))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно средств.", 3000);
                        return;
                    }
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили лицензию на оружие", 3000);
                    Main.Players[player].Licenses[6] = true;
                    Dashboard.sendStats(player);
                    return;
                case 59:
                    if (Main.Players[player].FractionID != 7)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник полиции", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[7].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    if (!Manager.canUseCommand(player, "openweaponstock")) return;
                    player.SetData("ONFRACSTOCK", 7);
                    GUI.Dashboard.OpenOut(player, Stocks.fracStocks[7].Weapons, "Склад оружия", 6);
                    return;
                case 66:
                    if (Main.Players[player].FractionID != 7)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник полиции", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    //тут тюнинг авто прописать
                    if (!player.IsInVehicle || (player.Vehicle.Model != NAPI.Util.GetHashKey("beachp") && player.Vehicle.Model != NAPI.Util.GetHashKey("lspdb") && player.Vehicle.Model != NAPI.Util.GetHashKey("police") && player.Vehicle.Model != NAPI.Util.GetHashKey("police2") && player.Vehicle.Model != NAPI.Util.GetHashKey("police3") && player.Vehicle.Model != NAPI.Util.GetHashKey("police4") && player.Vehicle.Model != NAPI.Util.GetHashKey("police42") && player.Vehicle.Model != NAPI.Util.GetHashKey("polmerit2")))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в рабочей машине", 3000);
                        return;
                    }
                    Trigger.PlayerEvent(player, "svem", 40, 40);
                    player.Vehicle.SetSharedData("BOOST", 40);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш автомобиль был протюнингован", 3000);
                    return;
            }
        }

        #region shapes
        private void arrestShape_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "IS_IN_ARREST_AREA", true);
            }
            catch (Exception ex) { Log.Write("arrestShape_onEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
        }

        private void arrestShape_onEntityExitColShape(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                NAPI.Data.SetEntityData(player, "IS_IN_ARREST_AREA", false);
                if (Main.Players[player].ArrestTime != 0)
                {
                    NAPI.Entity.SetEntityPosition(player, Police.policeCheckpoints[4]);
                }
            }
            catch (Exception ex) { Log.Write("arrestShape_onEntityExitColShape: " + ex.Message, nLog.Type.Error); }
        }

        private void onEntityEnterColshape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
            }
            catch (Exception ex) { Log.Write("onEntityEnterColshape: " + ex.Message, nLog.Type.Error); }
        }

        private void onEntityExitColshape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception ex) { Log.Write("onEntityExitColshape: " + ex.Message, nLog.Type.Error); }
        }
        #endregion

        //форма одежды начало
        public static void beginWorkDay(Player player)
        {
            if (Main.Players[player].FractionID != 7) return;
            Menu menu = new Menu("pdclothes", false, false);
            menu.Callback = callback_pdclothes;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Одежда";
            menu.Add(menuItem);

            menuItem = new Menu.Item("change", Menu.MenuItem.Button);
            menuItem.Text = "Начать работу";
            menu.Add(menuItem);

            menuItem = new Menu.Item("combat", Menu.MenuItem.Button);
            menuItem.Text = "Стиль одежды";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }

        private static void callback_pdclothes(Player Player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "change":
                    if (Main.Players[Player].FractionLVL < 0)
                    {
                        Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете переодеться", 3000);
                        return;
                    }
                    if (!Player.GetData<bool>("ON_DUTY"))
                    {
                        Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы переоделись в служебную форму", 3000);
                        Manager.setSkin(Player, 7, Main.Players[Player].FractionLVL);
                        Player.SetData("ON_DUTY", true);
                    }
                    else
                    {
                        Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы переоделись в повседневную одежду", 3000);
                        Customization.ApplyCharacter(Player);
                        if (Player.HasData("HAND_MONEY")) Player.SetClothes(5, 45, 0);
                        else if (Player.HasData("HEIST_DRILL")) Player.SetClothes(5, 41, 0);
                        Player.SetData("ON_DUTY", false);
                    }
                    return;
                case "combat":
                    MenuManager.Close(Player);
                    OpenPdMenu(Player);
                    return;
                case "close":
                    MenuManager.Close(Player);
                    return;
            }
        }
        public static void OpenPdMenu(Player player)
        {
            Menu menu = new Menu("pdcombat", false, false);
            menu.Callback = callback_pdcombat;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Выбрать форму";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam1", Menu.MenuItem.Button);
            menuItem.Text = "Повседневная";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam2", Menu.MenuItem.Button);
            menuItem.Text = "Патруль";
            menu.Add(menuItem);

            menuItem = new Menu.Item("cam3", Menu.MenuItem.Button);
            menuItem.Text = "Камуфляж";
            menu.Add(menuItem);

            menuItem = new Menu.Item("takeoff", Menu.MenuItem.Button);
            menuItem.Text = "Снять форму";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button);
            menuItem.Text = "Назад";
            menu.Add(menuItem);

            menu.Open(player);
        }

        private static void callback_pdcombat(Player Player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            if (item.ID == "back")
            {
                MenuManager.Close(Player);
                beginWorkDay(Player);
                return;
            }
            if (Main.Players[Player].FractionID != 7)
            {
                Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Доступ запрещён", 3000);
                return;
            }
            if (!Player.GetData<bool>("ON_DUTY"))
            {
                Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                return;
            }
            Player.SetData("IN_CP_MODE", true);
            var gender = Main.Players[Player].Gender;
            Customization.ClearClothes(Player, gender);
            switch (item.ID)
            {
                case "cam1":
                    if (gender)
                    {
                        Player.SetClothes(11, 13, 0);
                        Player.SetClothes(3, 11, 0);
                        Player.SetClothes(4, 96, 0);
                        Player.SetClothes(6, 15, 0);
                        Player.SetClothes(7, 125, 0);
                    }
                    else
                    {
                    }
                    return;
                case "cam2":
                    if (gender)
                    {
                        Player.SetClothes(11, 13, 1);
                        Player.SetClothes(3, 11, 0);
                        Player.SetClothes(4, 96, 0);
                        Player.SetClothes(6, 15, 0);
                        Player.SetClothes(7, 125, 0);
                    }
                    else
                    {
                    }
                    return;
                case "cam3":
                    if (gender)
                    {
                        Player.SetClothes(2, 0, 0);
                        Customization.SetHat(Player, 106, 20);
                        Player.SetClothes(11, 220, 17);
                        Player.SetClothes(4, 87, 15);
                        Player.SetClothes(6, 24, 0);
                    }
                    else
                    {
                    }
                    return;
                case "takeoff":
                    Manager.setSkin(Player, Main.Players[Player].FractionID, Main.Players[Player].FractionLVL);
                    Player.SetData("IN_CP_MODE", false);
                    return;
            }
        }
        //форма одежды конец

        public static void onPlayerDisconnectedhandler(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (NAPI.Data.HasEntityData(player, "ARREST_TIMER"))
                {
                    //Main.StopT(NAPI.Data.GetEntityData(player, "ARREST_TIMER"), "onPlayerDisconnectedhandler_arrest");
                    Timers.Stop(NAPI.Data.GetEntityData(player, "ARREST_TIMER"));
                }

                if (NAPI.Data.HasEntityData(player, "FOLLOWING"))
                {
                    Player target = NAPI.Data.GetEntityData(player, "FOLLOWING");
                    NAPI.Data.ResetEntityData(target, "FOLLOWER");
                }
                else if (NAPI.Data.HasEntityData(player, "FOLLOWER"))
                {
                    Player target = NAPI.Data.GetEntityData(player, "FOLLOWER");
                    NAPI.Data.ResetEntityData(target, "FOLLOWING");
                    //target.FreezePosition = false;
                    Trigger.PlayerEvent(target, "follow", false);
                }

                if (player.HasData("CALLPOLICE_BLIP"))
                {
                    NAPI.Entity.DeleteEntity(player.GetData<Blip>("CALLPOLICE_BLIP"));

                    Manager.sendFractionMessage(7, $"{player.Name.Replace('_', ' ')} отменил вызов");
                    Manager.sendFractionMessage(9, $"{player.Name.Replace('_', ' ')} отменил вызов");
                }
                if (player.HasData("CALLPOLICE_COL"))
                {
                    NAPI.ColShape.DeleteColShape(player.GetData<ColShape>("CALLPOLICE_COL"));
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        public static void setPlayerWantedLevel(Player player, WantedLevel wantedlevel)
        {
            Main.Players[player].WantedLVL = wantedlevel;
            if (wantedlevel != null) Trigger.PlayerEvent(player, "setWanted", wantedlevel.Level);
            else Trigger.PlayerEvent(player, "setWanted", 0);
			MySQL.Query($"UPDATE characters SET wanted='{JsonConvert.SerializeObject(wantedlevel)}' WHERE uuid='{Main.Players[player].UUID}'");
        }

        public static bool is_warg = false;

        #region menus
        public static void OpenPoliceGunMenu(Player player)
        {
            Trigger.PlayerEvent(player, "policeg");
        }
        [RemoteEvent("lspdgun")]
        public static void callback_policeGuns(Player Player, int index)
        {
            try
            {
                switch (index)
                {
                    case 0: //nightstick
                        Fractions.Manager.giveGun(Player, Weapons.Hash.Nightstick, "Nightstick");
                        return;
                    case 1: //pistol
                        Fractions.Manager.giveGun(Player, Weapons.Hash.Pistol, "Pistol");
                        return;
                    case 2: //smg
                        Fractions.Manager.giveGun(Player, Weapons.Hash.SMG, "SMG");
                        return;
                    case 3: //pumpshotgun
                        Fractions.Manager.giveGun(Player, Weapons.Hash.PumpShotgun, "PumpShotgun");
                        return;
                    case 4: //stungun
                        Fractions.Manager.giveGun(Player, Weapons.Hash.StunGun, "StunGun");
                        return;
                    case 5:
                        if (!Manager.canGetWeapon(Player, "armor")) return;
                        if (Fractions.Stocks.fracStocks[7].Materials < Fractions.Manager.matsForArmor)
                        {
                            Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "На складе недостаточно материала", 3000);
                            return;
                        }
                        var aItem = nInventory.Find(Main.Players[Player].UUID, ItemType.BodyArmor);
                        if (aItem != null)
                        {
                            Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас уже есть бронежилет", 3000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[7].Materials -= Fractions.Manager.matsForArmor;
                        Fractions.Stocks.fracStocks[7].UpdateLabel();
                        nInventory.Add(Player, new nItem(ItemType.BodyArmor, 1, 100.ToString()));
                        Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили бронежилет", 3000);
                        GameLog.Stock(Main.Players[Player].FractionID, Main.Players[Player].UUID, "armor", 1, false);
                        Manager.FracLogs[Main.Players[Player].FractionID].Add(new List<object> { DateTime.Now.ToString("dd.MM.yyyy"), $"{DateTime.Now.Hour}:{(DateTime.Now.Minute < 10 ? "0" : "" )}{DateTime.Now.Minute}", Player.Name, "Бронежилет", 1, "скрафтил" });
                        return;
                    case 6: // medkit
                        if (!Manager.canGetWeapon(Player, "Medkits")) return;
                        if (Fractions.Stocks.fracStocks[7].Medkits == 0)
                        {
                            Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "На складе нет аптечек", 3000);
                            return;
                        }
                        var hItem = nInventory.Find(Main.Players[Player].UUID, ItemType.HealthKit);
                        if (hItem != null)
                        {
                            Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас уже есть аптечка", 3000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[7].Medkits--;
                        Fractions.Stocks.fracStocks[7].UpdateLabel();
                        nInventory.Add(Player, new nItem(ItemType.HealthKit, 1));
                        GameLog.Stock(Main.Players[Player].FractionID, Main.Players[Player].UUID, "medkit", 1, false);
                        Manager.FracLogs[Main.Players[Player].FractionID].Add(new List<object> { DateTime.Now.ToString("dd.MM.yyyy"), $"{DateTime.Now.Hour}:{(DateTime.Now.Minute < 10 ? "0" : "" )}{DateTime.Now.Minute}", Player.Name, "Аптечку", 1, "скрафтил" });
                        Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили аптечку", 3000);
                        return;
                    case 7: // pistol ammo
                        if (!Manager.canGetWeapon(Player, "PistolAmmo")) return;
                        Fractions.Manager.giveAmmo(Player, ItemType.PistolAmmo, 12);
                        return;
                    case 8: // smg ammo
                        if (!Manager.canGetWeapon(Player, "SMGAmmo")) return;
                        Fractions.Manager.giveAmmo(Player, ItemType.SMGAmmo, 30);
                        return;
                    case 9: // shotgun ammo
                        if (!Manager.canGetWeapon(Player, "ShotgunsAmmo")) return;
                        Fractions.Manager.giveAmmo(Player, ItemType.ShotgunsAmmo, 6);
                        return;

                }
            }
            catch (Exception e)
            {
                Log.Write($"Lspdgun: " + e.Message, nLog.Type.Error);
            }
        }

        public static void OpenSpecialPoliceMenu(Player player)
        {
            Menu menu = new Menu("policeSpecial", false, false);
            menu.Callback += callback_policeSpecial;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Оружейная";
            menu.Add(menuItem);

            menuItem = new Menu.Item("changeclothes", Menu.MenuItem.Button);
            menuItem.Text = "Переодеться";
            menu.Add(menuItem);

            menuItem = new Menu.Item("pistol50", Menu.MenuItem.Button);
            menuItem.Text = "Desert Eagle";
            menu.Add(menuItem);

            menuItem = new Menu.Item("carbineRifle", Menu.MenuItem.Button);
            menuItem.Text = "Штурмовая винтовка";
            menu.Add(menuItem);

            menuItem = new Menu.Item("riflesammo", Menu.MenuItem.Button);
            menuItem.Text = "Автоматный калибр x30";
            menu.Add(menuItem);

            menuItem = new Menu.Item("heavyshotgun", Menu.MenuItem.Button);
            menuItem.Text = "Тяжелый дробовик";
            menu.Add(menuItem);

            menuItem = new Menu.Item("stungun", Menu.MenuItem.Button);
            menuItem.Text = "Tazer";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            menuItem.Text = "Закрыть";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_policeSpecial(Player Player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "changeclothes":
                    if (!NAPI.Data.GetEntityData(Player, "IN_CP_MODE"))
                    {
                        bool gender = Main.Players[Player].Gender;
                        Customization.ApplyCharacter(Player);
                        Customization.ClearClothes(Player, gender);
                        if (gender)
                        {
                            Customization.SetHat(Player, 39, 0);
                            //Player.SetClothes(1, 52, 0);
                            Player.SetClothes(11, 53, 0);
                            Player.SetClothes(4, 31, 0);
                            Player.SetClothes(6, 25, 0);
                            Player.SetClothes(9, 15, 2);
                            Player.SetClothes(3, 49, 0);
                        }
                        else
                        {
                            Customization.SetHat(Player, 38, 0);
                            //Player.SetClothes(1, 57, 0);
                            Player.SetClothes(11, 46, 0);
                            Player.SetClothes(4, 30, 0);
                            Player.SetClothes(6, 25, 0);
                            Player.SetClothes(9, 17, 2);
                            Player.SetClothes(3, 53, 0);
                        }
                        if (Player.HasData("HAND_MONEY")) Player.SetClothes(5, 45, 0);
                        else if (Player.HasData("HEIST_DRILL")) Player.SetClothes(5, 41, 0);
                        NAPI.Data.SetEntityData(Player, "IN_CP_MODE", true);
                        return;
                    }
                    Fractions.Manager.setSkin(Player, 7, Main.Players[Player].FractionLVL);
                    Player.SetData("IN_CP_MODE", false);
                    return;
                case "pistol50":
                    Fractions.Manager.giveGun(Player, Weapons.Hash.Pistol50, "pistol50");
                    return;
                case "carbineRifle":
                    Fractions.Manager.giveGun(Player, Weapons.Hash.CarbineRifle, "carbineRifle");
                    return;
                case "riflesammo":
                    if (!Manager.canGetWeapon(Player, "RiflesAmmo")) return;
                    Fractions.Manager.giveAmmo(Player, ItemType.RiflesAmmo, 30);
                    return;
                case "heavyshotgun":
                    Fractions.Manager.giveGun(Player, Weapons.Hash.HeavyShotgun, "heavyshotgun");
                    return;
                case "stungun":
                    Fractions.Manager.giveGun(Player, Weapons.Hash.StunGun, "stungun");
                    return;
                case "close":
                    MenuManager.Close(Player);
                    return;
            }
        }
        #endregion
    }
}
