using System;
using System.Collections.Generic;
using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using NeptuneEVO.GUI;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

namespace NeptuneEVO.Fractions
{
    class NacBez : Script
    {
        private static nLog Log = new nLog("NacBez");

        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        public static List<Vector3> CoordsNac = new List<Vector3>
        {

			new Vector3(-146.246, -582.0376, -31.30447), // Колшэйп входа убрать минусы
            new Vector3(-140.3308, -617.6943, -167.7004), // Колшэйп выход убрать минусы
			new Vector3(2154.641, 2921.034, -102.82243), //
            new Vector3(2033.842, 2942.104, -100.82434), //
            new Vector3(-132.1912, -633.8785, -167.7004), // Колшэйп раздевалки убрать минусы
            new Vector3(-141.3772, -646.2042, -167.7006), // Колшейп крафта убрать минусы
        };

        public static bool warg_mode = false;

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStartHandler()
        {
            try
            {

                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(CoordsNac[0], 1f, 2, 0));
                Cols[0].OnEntityEnterColShape += nacbez_OnEntityEnterColShape;
                Cols[0].OnEntityExitColShape += nacbez_OnEntityExitColShape;
                Cols[0].SetData("INTERACT", 88);

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(CoordsNac[1], 1f, 2, 0));
                Cols[1].OnEntityEnterColShape += nacbez_OnEntityEnterColShape;
                Cols[1].OnEntityExitColShape += nacbez_OnEntityExitColShape;
                Cols[1].SetData("INTERACT", 89);

                Cols.Add(2, NAPI.ColShape.CreateCylinderColShape(CoordsNac[2], 1f, 2, 0));
                Cols[2].OnEntityEnterColShape += nacbez_OnEntityEnterColShape;
                Cols[2].OnEntityExitColShape += nacbez_OnEntityExitColShape;
                Cols[2].SetData("INTERACT", 90);

                Cols.Add(3, NAPI.ColShape.CreateCylinderColShape(CoordsNac[3], 1f, 2, 0));
                Cols[3].OnEntityEnterColShape += nacbez_OnEntityEnterColShape;
                Cols[3].OnEntityExitColShape += nacbez_OnEntityExitColShape;
                Cols[3].SetData("INTERACT", 91);

                Cols.Add(4, NAPI.ColShape.CreateCylinderColShape(CoordsNac[4], 1f, 2, 0));
                Cols[4].SetData("INTERACT", 92);
                Cols[4].OnEntityEnterColShape += nacbez_OnEntityEnterColShape;
                Cols[4].OnEntityExitColShape += nacbez_OnEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Переодеться"), CoordsNac[4] + new Vector3(0, 0, 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                Cols.Add(5, NAPI.ColShape.CreateCylinderColShape(CoordsNac[5], 1f, 2, 0));
                Cols[5].SetData("INTERACT", 93);
                Cols[5].OnEntityEnterColShape += nacbez_OnEntityEnterColShape;
                Cols[5].OnEntityExitColShape += nacbez_OnEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Склад оружия"), CoordsNac[5] + new Vector3(0, 0, 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));



                NAPI.Marker.CreateMarker(1, CoordsNac[0] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, CoordsNac[1] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, CoordsNac[2] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, CoordsNac[3] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, CoordsNac[4] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, CoordsNac[5] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));



            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT\"FRACTIONS_NACBEZ\":\n" + e.ToString(), nLog.Type.Error);
            }
        }



        private void nacbez_OnEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
            }
            catch (Exception e) { Log.Write("nacbez_OnEntityEnterColShape: " + e.Message, nLog.Type.Error); }
        }

        private void nacbez_OnEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception e) { Log.Write("nacbez_OnEntityExitColShape: " + e.Message, nLog.Type.Error); }
        }

        public static void interactPressed(Player player, int interact)
        {
            if (Main.Players[player].FractionID != 18)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не состоите в Gruppe 6", 3000);
                return;
            }
            if (player.IsInVehicle) return;
            if (player.HasData("FOLLOWING"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                return;
            }
            switch (interact)
            {
                case 88:
                    NAPI.Entity.SetEntityPosition(player, CoordsNac[1] + new Vector3(0, 0, 1.12));
                    break;
                case 89:
                    NAPI.Entity.SetEntityPosition(player, CoordsNac[0] + new Vector3(0, 0, 1.12));
                    break;
                case 90:
                    NAPI.Entity.SetEntityPosition(player, CoordsNac[3] + new Vector3(0, 0, 1.12));
                    break;
                case 91:
                    NAPI.Entity.SetEntityPosition(player, CoordsNac[2] + new Vector3(0, 0, 1.12));
                    break;
                case 92:
                    beginWorkDay(player);
                    break;
                case 93:
                    if (Main.Players[player].FractionID != 18)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник Gruppe 6", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[18].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    if (!Manager.canUseCommand(player, "openweaponstock")) return;
                    player.SetData("ONFRACSTOCK", 18);
                    GUI.Dashboard.OpenOut(player, Stocks.fracStocks[18].Weapons, "Склад оружия", 6);
                    return;
            }
        }

        public static void beginWorkDay(Player player)
        {
            if (Main.Players[player].FractionID == 18)
            {
                if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы переоделись в форму Gruppe 6", 3000);
                    Manager.setSkin(player, 18, Main.Players[player].FractionLVL);
                    NAPI.Data.SetEntityData(player, "ON_DUTY", true);
                    return;
                }
                else
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы сняли форму Gruppe 6", 3000);
                    Customization.ApplyCharacter(player);
                    if (player.HasData("HAND_MONEY")) player.SetClothes(5, 45, 0);
                    else if (player.HasData("HEIST_DRILL")) player.SetClothes(5, 41, 0);
                    NAPI.Data.SetEntityData(player, "ON_DUTY", false);
                    return;
                }
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не состоите в Gruppe 6", 3000);
        }
        #region menu
         public static void OpenCityhallGunMenu(Player player)
        {

            if (Main.Players[player].FractionID != 18)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа", 3000);
                return;
            }
            if (!Stocks.fracStocks[18].IsOpen)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                return;
            }
            Trigger.PlayerEvent(player, "nacguns");
        }
		
		[RemoteEvent("helpfire")]
		public static void helpfire(Player player)
		{
            if (!Main.Players.ContainsKey(player)) return;
			int workid = Main.Players[player].WorkID;
			if ( workid != 6 && workid != 7 && workid != 3 && workid != 4 && workid != 14) return;
			if (player.GetData<Vehicle>("WORK") == null) return;
			Trigger.PlayerEvent(player, "openDialog", "HELP_ME", $"Вызвать помощь?");
		}
		
		public static void sendFire(Player player)
		{
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Сигнал был отправлен! Ожидайте", 3000);
            var Blip = NAPI.Blip.CreateBlip(0, player.Position, 1, 59, "Ограбление", 0, 0, true, 0, 0);
            Blip.Transparency = 0;
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].FractionID != 7) continue;
                if (!p.GetData<bool>("ON_DUTY") )return;
                Trigger.PlayerEvent(p, "changeBlipAlpha", Blip, 255);
                Trigger.PlayerEvent(p, "createWaypoint", player.Position.X, player.Position.Y);
            }
			Manager.sendWaypoint(7, player.Position);
            NAPI.Task.Run(() => {
            try
            {
                Blip.Delete();
            } catch { }
            }, 900000);
			Manager.sendFractionMessage(7, "Произошло нападение, на вашем GPS отмечено где оно происходит", true);
		}
		
        [RemoteEvent("nacgun")]
        public static void callback_cityhallGuns(Player Player, int index)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.StunGun, "StunGun");
                        return;
                    case 1:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.CombatPistol, "CombatPistol");
                        return;
                    case 2:
                        var minrank = (warg_mode) ? 2 : 6;
                        Fractions.Manager.giveGun(Player, Weapons.Hash.CombatPDW, "CombatPDW");
                        return;
                    case 3:
                        minrank = (warg_mode) ? 2 : 5;
                        Fractions.Manager.giveGun(Player, Weapons.Hash.CarbineRifle, "CarbineRifle");
                        return;
                    case 4:
                        if (!Manager.canGetWeapon(Player, "armor")) return;
                        if (Fractions.Stocks.fracStocks[18].Materials < Fractions.Manager.matsForArmor)
                        {
                            Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно материалов на складе", 3000);
                            return;
                        }
                        var aItem = nInventory.Find(Main.Players[Player].UUID, ItemType.BodyArmor);
                        if (aItem != null)
                        {
                            Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас уже есть бронежилет", 3000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[18].Materials -= Fractions.Manager.matsForArmor;
                        Fractions.Stocks.fracStocks[18].UpdateLabel();
                        nInventory.Add(Player, new nItem(ItemType.BodyArmor, 1, 100.ToString()));
                        GameLog.Stock(Main.Players[Player].FractionID, Main.Players[Player].UUID, "armor", 1, false);
                        Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили бронежилет", 3000);
                        return;
                    case 5: // medkit
                        if (!Manager.canGetWeapon(Player, "Medkits")) return;
                        if (Fractions.Stocks.fracStocks[18].Medkits == 0)
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
                        Fractions.Stocks.fracStocks[18].Medkits--;
                        Fractions.Stocks.fracStocks[18].UpdateLabel();
                        nInventory.Add(Player, new nItem(ItemType.HealthKit, 1));
                        GameLog.Stock(Main.Players[Player].FractionID, Main.Players[Player].UUID, "medkit", 1, false);
                        Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили аптечку", 3000);
                        return;
                    case 6:
                        Fractions.Manager.giveAmmo(Player, ItemType.PistolAmmo, 12);
                        return;
                    case 7:
                        minrank = (warg_mode) ? 2 : 6;
                        Fractions.Manager.giveAmmo(Player, ItemType.SMGAmmo, 30);
                        return;
                    case 8:
                        minrank = (warg_mode) ? 2 : 5;
                        Fractions.Manager.giveAmmo(Player, ItemType.RiflesAmmo, 30);
                        return;
                }
            }
			catch (Exception e) { Log.Write("Mwcgun: " + e.Message, nLog.Type.Error); }
		}	
        #endregion
    }
}