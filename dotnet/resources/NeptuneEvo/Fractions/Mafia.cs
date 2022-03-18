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
    class Mafia : Script
    {

        private static nLog Log = new nLog("Mafia");

        public static Dictionary<int, Vector3> EnterPoints = new Dictionary<int, Vector3>()
        {
            { 10, new Vector3(1394.805, 1141.719, -113.3233) },
            //{ 11, new Vector3(-113.4213, 985.761, -234.6341) },
           // { 12, new Vector3(-1549.331, -90.05454, -253.80917) },
            { 13, new Vector3(-1805.049, 438.1696, -127.5874) },
        };
        public static Dictionary<int, Vector3> ExitPoints = new Dictionary<int, Vector3>()
        {
            { 10, new Vector3(1396.62, 1142.823, -83.24014) },
            //{ 11, new Vector3(-123.8163, 975.3881, -58.63158) },
           // { 12, new Vector3(-1550.298, -94.81767, -293.2058) },
            { 13, new Vector3(-1812.82, 466.4906, -185.7867) },
        };

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            //NAPI.TextLabel.CreateTextLabel("~g~Vladimir Medvedev", new Vector3(-113.9224, 985.793, 236.754), 5f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
            //NAPI.TextLabel.CreateTextLabel("~g~Kaha Panosyan", new Vector3(-1811.368, 438.4105, 129.7074), 5f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
            //NAPI.TextLabel.CreateTextLabel("~g~Jotaro Josuke", new Vector3(-1549.287, -89.35114, 55.92917), 5f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);
            //NAPI.TextLabel.CreateTextLabel("~g~Solomon Gambino", new Vector3(1392.098, 1155.892, 115.4433), 5f, 0.3f, 0, new Color(255, 255, 255), true, NAPI.GlobalDimension);

            ColShape shape = NAPI.ColShape.CreateCylinderColShape(new Vector3(1407.6854, 1115.2834, 114.71761), 1f, 2, 0);
            shape.SetData("INTERACT", 87);
            shape.OnEntityEnterColShape += mws_OnEntityEnterColShape;
            shape.OnEntityExitColShape += mws_OnEntityExitColShape;
            NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Крафт оружия"), new Vector3(1407.6854, 1115.2834, 113.91761) + new Vector3(0, 0, 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));
            NAPI.Marker.CreateMarker(1, new Vector3(1407.6854, 1115.2834, 113.71761) - new Vector3(0, 0, 0.5f), new Vector3(), new Vector3(), 0.965f, new Color(0, 175, 250, 220), false, 0);
            NAPI.Marker.CreateMarker(27, new Vector3(1407.6854, 1115.2834, 113.71761) + new Vector3(0, 0, 0.14f), new Vector3(), new Vector3(), 1f, new Color(0, 175, 250, 220), false, 0);

            foreach (var point in EnterPoints)
            {
                NAPI.Marker.CreateMarker(1, point.Value - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220), false, NAPI.GlobalDimension);

                var col = NAPI.ColShape.CreateCylinderColShape(point.Value, 1.2f, 2, NAPI.GlobalDimension);
                col.SetData("FRAC", point.Key);

                col.OnEntityEnterColShape += (s, e) =>
                {
                    if (!Main.Players.ContainsKey(e)) return;
                    e.SetData("FRACTIONCHECK", s.GetData<int>("FRAC"));
                    e.SetData("INTERACTIONCHECK", 64);
                };
                col.OnEntityExitColShape += (s, e) =>
                {
                    if (!Main.Players.ContainsKey(e)) return;
                    e.SetData("INTERACTIONCHECK", -1);
                };
            }

            foreach (var point in ExitPoints)
            {
                NAPI.Marker.CreateMarker(1, point.Value - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220), false, NAPI.GlobalDimension);

                var col = NAPI.ColShape.CreateCylinderColShape(point.Value, 1.2f, 2, NAPI.GlobalDimension);
                col.SetData("FRAC", point.Key);

                col.OnEntityEnterColShape += (s, e) =>
                {
                    if (!Main.Players.ContainsKey(e)) return;
                    e.SetData("FRACTIONCHECK", s.GetData<int>("FRAC"));
                    e.SetData("INTERACTIONCHECK", 65);
                };
                col.OnEntityExitColShape += (s, e) =>
                {
                    if (!Main.Players.ContainsKey(e)) return;
                    e.SetData("INTERACTIONCHECK", -1);
                };
            }
        }

        private void mws_OnEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
            }
            catch (Exception e) { Log.Write("mws_OnEntityEnterColShape: " + e.Message, nLog.Type.Error); }
        }

        private void mws_OnEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception e) { Log.Write("mws_OnEntityExitColShape: " + e.Message, nLog.Type.Error); }
        }

        public static void OpenCityhallGunMenu(Player player)
        {

            if (Main.Players[player].FractionID != 10) return;
            if (Main.Players[player].FractionLVL < 7) // krasava
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Доступ запрещён. Разрещено от 7 ранга", 3000);
                return;
            }
            if (!Stocks.fracStocks[10].IsOpen)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                return;
            }
            Trigger.PlayerEvent(player, "mwcguns");
        }


        [RemoteEvent("mwcgun")]
        public static void callback_cityhallGuns(Player Player, int index)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.StunGun, "StunGun");
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
                    case 4:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.CombatPistol, "CombatPistol");
                        return;
                    case 5:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.CarbineRifle, "CarbineRifle");
                        return;                
                    case 6:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.Pistol50, "Pistol50");
                        return;
                    case 7:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.VintagePistol, "VintagePistol");
                        return;
                    case 8:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.MachinePistol, "MachinePistol");
                        return;
                    case 9:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.AssaultSMG, "AssaultSMG");
                        return;
                    case 10:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.CombatMG, "CombatMG");
                        return;
                    case 11:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.SawnOffShotgun, "SawnOffShotgun");
                        return;
                    case 12:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.BullpupShotgun, "BullpupShotgun");
                        return;
                    case 13:
                        Fractions.Manager.giveGun(Player, Weapons.Hash.HeavyShotgun, "HeavyShotgun");
                        return;
                    case 14:
                        //if (!Manager.canGetWeapon(Player, "armor")) return;
                        if (Fractions.Stocks.fracStocks[10].Materials < Fractions.Manager.matsForArmor)
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
                        Fractions.Stocks.fracStocks[10].Materials -= Fractions.Manager.matsForArmor;
                        Fractions.Stocks.fracStocks[10].UpdateLabel();
                        nInventory.Add(Player, new nItem(ItemType.BodyArmor, 1, 100.ToString()));
                        GameLog.Stock(Main.Players[Player].FractionID, Main.Players[Player].UUID, "armor", 1, false);
                        Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили бронежилет", 3000);
                        Manager.FracLogs[Main.Players[Player].FractionID].Add(new List<object> { DateTime.Now.ToString("dd.MM.yyyy"), $"{DateTime.Now.Hour}:{(DateTime.Now.Minute < 10 ? "0" : "")}{DateTime.Now.Minute}", Player.Name, "Бронежилет", 1, "скрафтил" });
                        return;
                    case 15: // medkit
                        //if (!Manager.canGetWeapon(Player, "Medkits")) return;
                        if (Fractions.Stocks.fracStocks[10].Medkits == 0)
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
                        Fractions.Stocks.fracStocks[10].Medkits--;
                        Fractions.Stocks.fracStocks[10].UpdateLabel();
                        nInventory.Add(Player, new nItem(ItemType.HealthKit, 1));
                        GameLog.Stock(Main.Players[Player].FractionID, Main.Players[Player].UUID, "medkit", 1, false);
                        Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили аптечку", 3000);
                        Manager.FracLogs[Main.Players[Player].FractionID].Add(new List<object> { DateTime.Now.ToString("dd.MM.yyyy"), $"{DateTime.Now.Hour}:{(DateTime.Now.Minute < 10 ? "0" : "")}{DateTime.Now.Minute}", Player.Name, "Аптечку", 1, "скрафтил" });
                        return;
                    case 16:
                        Fractions.Manager.giveAmmo(Player, ItemType.PistolAmmo, 12);
                        return;
                    case 17:
                        Fractions.Manager.giveAmmo(Player, ItemType.SMGAmmo, 30);
                        return;
                    case 18:
                        Fractions.Manager.giveAmmo(Player, ItemType.RiflesAmmo, 30);
                        return;
                    case 19: // shotgun ammo
                        Fractions.Manager.giveAmmo(Player, ItemType.ShotgunsAmmo, 6);
                        return;
                }
            }
            catch (Exception e) { Log.Write("Mwcgun: " + e.Message, nLog.Type.Error); }
        }

    }
}
