using GTANetworkAPI;
using System;
using NeptuneEVO.GUI;
using NeptuneEVO.Houses;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Core
{
    class Selecting : Script
    {
        private static nLog Log = new nLog("Selecting");

        [RemoteEvent("oSelected")]
        public static void objectSelected(Player player, GTANetworkAPI.Object entity)
        {
            try
            {
                //var entity = (GTANetworkAPI.Object)arguments[0]; // error "Object referance not set to an instance of an object"
                if (entity == null || player == null || !Main.Players.ContainsKey(player)) return;
                if (entity.HasSharedData("PICKEDT") && entity.GetSharedData<bool>("PICKEDT") == true) {
                    Commands.SendToAdmins(3, $"!{{#d35400}}[PICKUP-ITEMS-EXPLOIT] {player.Name} ({player.Value}) ");
                    return;
                }
                entity.SetSharedData("PICKEDT", true);
                var objType = NAPI.Data.GetEntitySharedData(entity, "TYPE");
                switch (objType)
                {
                    case "DROPPED":
                        {
                            if (player.HasData("isRemoveObject"))
                            {
                                NAPI.Task.Run(() => {
                                    try
                                    {
                                        NAPI.Entity.DeleteEntity(entity);
                                    } catch { }
                                });
                                player.ResetData("isRemoveObject");
                                return;
                            }

                            var id = entity.GetData<int>("ID");
                            if (Items.InProcessering.Contains(id)) {
                                entity.SetSharedData("PICKEDT", false);
                                return;
                            }
                            Items.InProcessering.Add(id);

                            nItem item = NAPI.Data.GetEntityData(entity, "ITEM");
                            if (item.Type == ItemType.BodyArmor && nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor) != null)
                            {
                                entity.SetSharedData("PICKEDT", false);
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                Items.InProcessering.Remove(id);
                                return;
                            }
                            
                            var tryAdd = nInventory.TryAdd(player, item);
                            if (tryAdd == -1 || (tryAdd > 0 && nInventory.WeaponsItems.Contains(item.Type)))
                            {
                                entity.SetSharedData("PICKEDT", false);
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                Items.InProcessering.Remove(id);
                                return;
                            }
                            else if (tryAdd > 0)
                            {
                                entity.SetSharedData("PICKEDT", false);
                                nInventory.Add(player, new nItem(item.Type, item.Count - tryAdd, item.Data));
                                GameLog.Items($"ground", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), item.Count - tryAdd, $"{item.Data}");
                                item.Count = tryAdd;
                                entity.SetData("ITEM", item);
                                Items.InProcessering.Remove(id);
                            }
                            else
                            {
                                NAPI.Task.Run(() => { try { NAPI.Entity.DeleteEntity(entity); } catch { } });
                                nInventory.Add(player, item);
                                GameLog.Items($"ground", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), item.Count, $"{item.Data}");
                            }
                            Main.OnAntiAnim(player);
                            player.PlayAnimation("random@domestic", "pickup_low", 39);
                            NAPI.Task.Run(() => { try { player.StopAnimation(); Main.OffAntiAnim(player); } catch { } }, 1700);
                            return;
                        }
                    case "WeaponSafe":
                    case "SubjectSafe":
                    case "ClothesSafe":
                        {
                            entity.SetSharedData("PICKEDT", false);
                            if (Main.Players[player].InsideHouseID == -1) return;
                            int houseID = Main.Players[player].InsideHouseID;
                            House house = HouseManager.Houses.FirstOrDefault(h => h.ID == Main.Players[player].InsideHouseID);
                            if(house == null) return;
                            if(!house.Owner.Equals(player.Name)) {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Пользоваться мебелью может только владелец дома.", 3000);
                                return;
                            }
                            var furnID = NAPI.Data.GetEntityData(entity, "ID");
                            HouseFurniture furniture = FurnitureManager.HouseFurnitures[houseID][furnID];
                            if (FurnitureManager.FurnituresItems[houseID].ContainsKey(furnID)) return;
                            var items = FurnitureManager.FurnituresItems[houseID][furnID];
                            if(items == null) return;
                            player.SetData("OpennedSafe", furnID);
                            player.SetData("OPENOUT_TYPE", FurnitureManager.SafesType[furniture.Name]);
                            Dashboard.OpenOut(player, items, furniture.Name, FurnitureManager.SafesType[furniture.Name]);
                            return;
                        }
                    case "MoneyBag":
                        {
                            if (player.HasData("HEIST_DRILL") || NAPI.Data.HasEntityData(player, "HAND_MONEY"))
                            {
                                entity.SetSharedData("PICKEDT", false);
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть сумка", 3000);
                                return;
                            }

                            var money = NAPI.Data.GetEntityData(entity, "MONEY_IN_BAG");

                            player.SetClothes(5, 45, 0);
                            var item = new nItem(ItemType.BagWithMoney, 1, $"{money}");
                            nInventory.Items[Main.Players[player].UUID].Add(item);
                            Dashboard.sendItems(player);
                            player.SetData("HAND_MONEY", true);
                            NAPI.Task.Run(() => { try { NAPI.Entity.DeleteEntity(entity); } catch { } });
                            Main.OnAntiAnim(player);
                            player.PlayAnimation("random@domestic", "pickup_low", 39);
                            NAPI.Task.Run(() => { try { player.StopAnimation(); Main.OffAntiAnim(player); } catch { } }, 1700);
                            return;
                            }
                    case "DrillBag":
                        {
                            if (player.HasData("HEIST_DRILL") || player.HasData("HAND_MONEY"))
                            {
                                entity.SetSharedData("PICKEDT", false);
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть дрель или деньги в руках", 3000);
                                return;
                            }
                            
                            player.SetClothes(5, 41, 0);
                            nInventory.Add(player, new nItem(ItemType.BagWithDrill));
                            player.SetData("HEIST_DRILL", true);
                            
                            NAPI.Task.Run(() => { try { NAPI.Entity.DeleteEntity(entity); } catch { } });
                            Main.OnAntiAnim(player);
                            player.PlayAnimation("random@domestic", "pickup_low", 39);
                            NAPI.Task.Run(() => { try { player.StopAnimation(); Main.OffAntiAnim(player); } catch { } }, 1700);
                            return;
                        }
                }
            }
            catch (Exception e) { Log.Write($"oSelected/: {e.ToString()}\n{e.StackTrace}", nLog.Type.Error); }
        }

        [RemoteEvent("server::getvehmanage")]
        public static void RM_getvehmanage(Player player)
        {
            try
            {
                if (player.IsInVehicle) return;
                Vehicle veh = VehicleManager.getNearestVehicle(player, 20);

                if (veh == null || !VehicleManager.Vehicles.ContainsKey(veh.NumberPlate) || VehicleManager.Vehicles.ContainsKey(veh.NumberPlate) && VehicleManager.Vehicles[veh.NumberPlate].Holder != player.Name || !veh.HasData("ACCESS") || veh.GetData<string>("ACCESS") != "PERSONAL")
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "В пределах 20 метров нет вашего транспорта", 3000);
                    return;
                }
                

                Trigger.PlayerEvent(player, "client::setvehmanage", ParkManager.GetNormalName(VehicleManager.Vehicles[veh.NumberPlate].Model));

            }
            catch { }
        }

        [RemoteEvent("server::sendvehicle")]
        public static void RM_sendvehicle(Player player, int index)
        {
            try
            {
                if (player.IsInVehicle) return;
                Vehicle veh = VehicleManager.getNearestVehicle(player, 20);

                if (veh == null || !VehicleManager.Vehicles.ContainsKey(veh.NumberPlate) || VehicleManager.Vehicles.ContainsKey(veh.NumberPlate) && VehicleManager.Vehicles[veh.NumberPlate].Holder != player.Name || !veh.HasData("ACCESS") || veh.GetData<string>("ACCESS") == "GARAGE")
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "В пределах 20 метров нет вашего транспорта", 3000);
                    return;
                }

                switch (index)
                {
                    case 1:
                        {
                            VehicleStreaming.SetLockStatus(veh, !VehicleStreaming.GetLockState(veh));
                            return;
                        }
                    case 2:
                        {
                            VehicleStreaming.SetEngineState(veh, !VehicleStreaming.GetEngineState(veh));
                            return;
                        }
                    case 3:
                        {
                            if (VehicleStreaming.GetDoorState(veh, DoorID.DoorTrunk) == DoorState.DoorClosed)
                            {
                                if (VehicleStreaming.GetLockState(veh))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Машина закрыта", 3000);
                                    return;
                                }
                                VehicleStreaming.SetDoorState(veh, DoorID.DoorTrunk, DoorState.DoorOpen);
                            }
                            else VehicleStreaming.SetDoorState(veh, DoorID.DoorTrunk, DoorState.DoorClosed);

                            return;
                        }
                    case 4:
                        {
                            if (VehicleStreaming.GetDoorState(veh, DoorID.DoorHood) == DoorState.DoorClosed)
                            {
                                if (VehicleStreaming.GetLockState(veh))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Машина закрыта", 3000);
                                    return;
                                }
                                VehicleStreaming.SetDoorState(veh, DoorID.DoorHood, DoorState.DoorOpen);
                            }
                            else VehicleStreaming.SetDoorState(veh, DoorID.DoorHood, DoorState.DoorClosed);

                            return;
                        }
                    case 5:
                        {
                            if (VehicleStreaming.GetDoorState(veh, DoorID.DoorRearLeft) == DoorState.DoorClosed)
                            {
                                if (VehicleStreaming.GetLockState(veh))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Машина закрыта", 3000);
                                    return;
                                }
                                VehicleStreaming.SetDoorState(veh, DoorID.DoorFrontLeft, DoorState.DoorOpen);
                                VehicleStreaming.SetDoorState(veh, DoorID.DoorFrontRight, DoorState.DoorOpen);
                                VehicleStreaming.SetDoorState(veh, DoorID.DoorRearLeft, DoorState.DoorOpen);
                                VehicleStreaming.SetDoorState(veh, DoorID.DoorRearRight, DoorState.DoorOpen);
                            }
                            else
                            {
                                VehicleStreaming.SetDoorState(veh, DoorID.DoorFrontLeft, DoorState.DoorClosed);
                                VehicleStreaming.SetDoorState(veh, DoorID.DoorFrontRight, DoorState.DoorClosed);
                                VehicleStreaming.SetDoorState(veh, DoorID.DoorRearLeft, DoorState.DoorClosed);
                                VehicleStreaming.SetDoorState(veh, DoorID.DoorRearRight, DoorState.DoorClosed);
                            }

                            return;
                        }
                    case 6:
                        {
                            if (!VehicleStreaming.GetEngineState(veh))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Машина не заведена", 3000);
                                return;
                            }
                            if (!VehicleStreaming.GetVehicleIndicatorLights(veh, false))
                            {
                                VehicleStreaming.SetVehicleIndicatorLights(veh, 0, true);
                                VehicleStreaming.SetVehicleIndicatorLights(veh, 1, true);
                            }
                            else
                            {
                                VehicleStreaming.SetVehicleIndicatorLights(veh, 0, false);
                                VehicleStreaming.SetVehicleIndicatorLights(veh, 1, false);
                            }
                            
                            return;
                        }
                    default:
                        return;
                
                }
                

            }
            catch { }
        }


        [RemoteEvent("vehicleSelected")]
        public static void vehicleSelected(Player player, params object[] arguments)
        {
            try
            {
                var vehicle = (Vehicle)arguments[0];
                int index = (int)arguments[1];
                if (vehicle == null || player.Position.DistanceTo(vehicle.Position) > 5)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Машина находится далеко от Вас", 3000);
                    return;
                }
                switch (index)
                {
                    case 0:
                        if (player.IsInVehicle)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не можете открыть/закрыть капот, находясь в машине", 3000);
                            return;
                        }
                        if (VehicleStreaming.GetDoorState(vehicle, DoorID.DoorHood) == DoorState.DoorClosed)
                        {
                            if (VehicleStreaming.GetLockState(vehicle))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не можете открыть капот, пока машина закрыта", 3000);
                                return;
                            }
                            VehicleStreaming.SetDoorState(vehicle, DoorID.DoorHood, DoorState.DoorOpen);
                        }
                        else VehicleStreaming.SetDoorState(vehicle, DoorID.DoorHood, DoorState.DoorClosed);
                        return;
                    case 1:
                        if (player.IsInVehicle)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не можете открыть/закрыть багажник, находясь в машине", 3000);
                            return;
                        }
                        if (vehicle.GetData<string>("ACCESS") == "INPARK") return;
                        if (VehicleStreaming.GetDoorState(vehicle, DoorID.DoorTrunk) == DoorState.DoorOpen)
                        {
                            Commands.RPChat("me", player, $"закрыл(а) багажник");
                            VehicleStreaming.SetDoorState(vehicle, DoorID.DoorTrunk, DoorState.DoorClosed);
                            foreach (var p in NAPI.Pools.GetAllPlayers())
                            {
                                if (p == null || !Main.Players.ContainsKey(p)) continue;
                                if (p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 2 && p.HasData("SELECTEDVEH") && p.GetData<Vehicle>("SELECTEDVEH") == vehicle) GUI.Dashboard.Close(p);
                            }
                        }
                        else
                        {
                            if (vehicle.HasData("ACCESS") && (vehicle.GetData<string>("ACCESS") == "PERSONAL" || vehicle.GetData<string>("ACCESS") == "GARAGE"))
                            {
                                var access = VehicleManager.canAccessByNumber(player, vehicle.NumberPlate);
                                if (!access)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет ключей от этого транспорта", 3000);
                                    return;
                                }
                            }
                            if (vehicle.HasData("ACCESS") && vehicle.GetData<string>("ACCESS") == "FRACTION" && vehicle.GetData<int>("FRACTION") != Main.Players[player].FractionID)
                            {
                                if (Main.Players[player].FractionID != 7 && Main.Players[player].FractionID != 9)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не можете открыть багажник у этой машины", 3000);
                                    return;
                                }
                            }
                            VehicleStreaming.SetDoorState(vehicle, DoorID.DoorTrunk, DoorState.DoorOpen);
                            Commands.RPChat("me", player, $"открыл(а) багажник");
                        }
                        return;
                    case 2:
                        VehicleManager.ChangeVehicleDoors(player, vehicle);
                        return;
                    case 3:
                        if (player.IsInVehicle)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не можете открыть инвентарь, находясь в машине", 3000);
                            return;
                        }
                        if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "WORK" || vehicle.Class == 13 || vehicle.Class == 8)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Эта транспортное средство не поддерживает инвентарь", 3000);
                            return;
                        }
                        if (Main.Players[player].AdminLVL == 0 && VehicleStreaming.GetDoorState(vehicle, DoorID.DoorTrunk) == DoorState.DoorClosed)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не можете открыть инвентарь машины, пока багажник закрыт", 3000);
                            return;
                        }
                        if(vehicle.GetData<bool>("BAGINUSE") == true) {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Дождитесь, пока другой человек перестанет пользоваться багажником.", 3000);
                            return;
                        }
                        vehicle.SetData("BAGINUSE", true);
                        GUI.Dashboard.OpenOut(player, vehicle.GetData<List<nItem>>("ITEMS"), "Багажник", 2);
                        player.SetData("SELECTEDVEH", vehicle);
                        return;
					case 4:
						if (nInventory.Find(Main.Players[player].UUID, ItemType.Repair) == null) 
						{
							Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет Рем. комплекта", 3000);
							return;
						}
						nInventory.Remove(player, ItemType.Repair, 1);
						Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Ваш транспорт был отремонтирован!", 3000);
						VehicleManager.RepairCar(vehicle);
                        Utils.QuestsManager.AddQuestProcess(player, 1);
                        return;
                    case 5:
                        if (vehicle.Class == 8 || vehicle.Class == 14 || vehicle.Class == 15 || vehicle.Class == 16 || vehicle.Class == 21) return;
                        if (VehicleStreaming.GetDoorState(vehicle, DoorID.DoorTrunk) != DoorState.DoorOpen && VehicleStreaming.GetDoorState(vehicle, DoorID.DoorTrunk) != DoorState.DoorBroken)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Багажник закрыт!", 3000);
                            return;
                        }
                        if (vehicle.HasData("TRUNK"))
                        {
                            if (vehicle.GetData<Player>("TRUNK") == player)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы вылезли из багажника", 3000);
                                vehicle.ResetData("TRUNK");
                                player.ResetSharedData("attachToVehicleTrunk");
                                Trigger.PlayerEventInRange(player.Position, 500, "vehicledeattach", player);
                                Main.OffAntiAnim(player);
                                player.StopAnimation();
                                player.ResetData("VEH");
                                return;
                            }
                            else
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "В багажнике уже кто-то есть", 3000);
                                return;
                            }
                        }
                        if (player.HasData("LastActiveWeap"))
                        {
                            var wHash = Weapons.GetHash(player.GetData<string>("LastActiveWeap").ToString());
                            Trigger.PlayerEvent(player, "takeOffWeapon", (int)wHash);
                            Commands.RPChat("me", player, $"убрал(а) {nInventory.ItemsNames[(int)player.GetData<int>("LastActiveWeap")]}");
                        }
                        Main.OnAntiAnim(player);
                        player.PlayAnimation("amb@world_human_bum_slumped@male@laying_on_right_side@base", "base", 35);
                        vehicle.SetData("TRUNK", player);
                        player.SetSharedData("attachToVehicleTrunk", vehicle.Value);
                        player.SetData("VEH", vehicle);
                        
                        Trigger.PlayerEventInRange(player.Position, 500, "vehicleattach", player, vehicle);

                        

                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы залезли в багажник!", 3000);
                        
                        return;
                }
            }
            catch (Exception e) { Log.Write("vSelected: " + e.Message, nLog.Type.Error); }

        }

        [RemoteEvent("deattachfromtrunk")]
        public static void PlayerEvent_Trunk(Player player, bool water)
        {
            if (!player.HasData("VEH")) return;

            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы вылезли из багажника", 3000);
            player.GetData<Vehicle>("VEH").ResetData("TRUNK");
            player.ResetSharedData("attachToVehicleTrunk");
            Trigger.PlayerEventInRange(player.Position, 500, "vehicledeattach", player);
            Main.OffAntiAnim(player);
            player.StopAnimation();
            player.ResetData("VEH");
            return;
        }

        [RemoteEvent("pSelected")]
        public static void playerSelected(Player player, params object[] arguments)
        {
            try
            {
                if (arguments[0] == null) return;
                if (player.HasData("ARENA")) return;
                var target = (Player)arguments[0];
                if (target == null || player.Position.DistanceTo(target.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин находится далеко от Вас", 3000);
                    return;
                }
                if (target.HasData("ROOLING") || player.HasData("ROOLING")) return;
                player.SetData("SELECTEDPLAYER", target);
                
                if (arguments.Length == 1) return;
                string action;
                try
                {
                    action = arguments[1].ToString();
                }
                catch { return; }
                switch (action)
                {
                    case "Сыграть в кости":
                        if (!Main.Players.ContainsKey(player)) return;

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

                        player.SetData("DICE_TARGET", target);

                        Trigger.PlayerEvent(player, "openInput", "Игра в кости", "Введите сумму игры в кости", 20, "dice");

                        return;
                    case "Пожать руку":
                        if (player.IsInVehicle) return;
                        playerHandshakeTarget(player, target);
                        return;
					case "Поцеловать":
                        if (player.IsInVehicle) return;
                        playerKissTarget(player, target);
                        return;
                    case "Вести за собой":
                        if (player.IsInVehicle) return;
                        Fractions.FractionCommands.targetFollowPlayer(player, target);
                        return;
                    case "Выдать сертификат":

                        target.SetData("FAMILY_SET", true);
                        Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, "Вам выдали сертификат для создания семьи", 10000);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы выдали сертификат для создания семьи", 10000);

                        return;
                    //case "Арестовать":
                    //    if (player.IsInVehicle) return;
                    //    Fractions.FractionCommands.arrestTarget(player, target);
                    //    return;
                    case "Ограбить":
                        if (player.IsInVehicle) return;
                        Fractions.FractionCommands.robberyTarget(player, target);
                        return;
                    case "Отпустить":
                        if (player.IsInVehicle) return;
                        if (!target.HasData("FOLLOWING"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Этого гражданина никто не тащит", 3000);
                            return;
                        }
                        if (!player.HasData("FOLLOWER") || player.GetData<Player>("FOLLOWER") != target)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Этого гражданина тащит кто-то другой", 3000);
                            return;
                        }
                        Fractions.FractionCommands.unFollow(player, target);
                        return;
                    case "Показать пластик":
                        Docs.Plastic(player, target);
                        return;
                    case "Выдать пластик":
                        {
                            if (Main.Players[player].FractionID != 7)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет доступа!", 3000);
                                return;
                            }
                            if (Main.Players[player].FractionLVL < 8)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Минимальный ранг для выдачи пластика 8!", 3000);
                                return;
                            }

                            player.SetData("PLASTIC_TARGET", target);
                            Trigger.PlayerEvent(player, "openInput", "Выдача пластика", "Введите номер транспорта", 30, "accept_plastic");

                            return;
                        }
                    case "Обыскать":
                        if (player.IsInVehicle) return;
                        {
                            if (!target.GetData<bool>("CUFFED"))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин не в наручниках", 3000);
                                return;
                            }

                            var items = nInventory.Items[Main.Players[target].UUID];
                            List<string> itemNames = new List<string>();
                            List<string> weapons = new List<string>();
                            foreach (var i in items)
                            {
                                if (nInventory.ClothesItems.Contains(i.Type)) continue;
                                if (nInventory.WeaponsItems.Contains(i.Type))
                                    weapons.Add($"{nInventory.ItemsNames[(int)i.Type]} {i.Data}");
                                else
                                    itemNames.Add($"{nInventory.ItemsNames[(int)i.Type]} x{i.Count}");
                            }

                            var data = new SearchObject();
                            data.Name = target.Name.Replace('_', ' ');
                            data.Weapons = weapons;
                            data.Items = itemNames;

                            Trigger.PlayerEvent(player, "newPassport", target, Main.Players[target].UUID);
                            Trigger.PlayerEvent(player, "bsearchOpen", JsonConvert.SerializeObject(data));
                            return;
                        }
                    case "Посмотреть паспорт":
                        if (player.IsInVehicle) return;
                        {
                            if (!target.GetData<bool>("CUFFED"))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин не в наручниках", 3000);
                                return;
                            }

                            var acc = Main.Players[target];
                            string gender = (acc.Gender) ? "Мужской" : "Женский";
                            string fraction = (acc.FractionID > 0) ? Fractions.Manager.FractionNames[acc.FractionID] : "Нет";
                            string work = "Безработный";
                            List<object> data = new List<object>
                            {
                                acc.UUID,
                                acc.FirstName,
                                acc.LastName,
                                acc.CreateDate.ToString("dd.MM.yyyy"),
                                gender,
                                fraction,
                                work
                            };
                            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                            Trigger.PlayerEvent(player, "passport", json);
                            Trigger.PlayerEvent(player, "newPassport", target, acc.UUID);
                        }
                        return;
                    case "Посмотреть лицензии":
                        if (player.IsInVehicle) return;
                        {
                            if (!target.GetData<bool>("CUFFED"))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданина не в наручниках", 3000);
                                return;
                            }

                            var acc = Main.Players[target];
                            string gender = (acc.Gender) ? "Мужской" : "Женский";

                            var lic = "";
                            for (int i = 0; i < acc.Licenses.Count; i++)
                                if (acc.Licenses[i]) lic += $"{Main.LicWords[i]} / ";
                            if (lic == "") lic = "Отсутствуют";

                            List<string> data = new List<string>
                            {
                                acc.FirstName,
                                acc.LastName,
                                acc.CreateDate.ToString("dd.MM.yyyy"),
                                gender,
                                lic
                            };
                            
                            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                            Trigger.PlayerEvent(player, "licenses", json);
                        }
                        return;
					case "Показать удостоверение": //Удостоверение
                        GUI.Docs.Documents(player, target);
                        return;	
                    case "Изъять оружие":
                        if (player.IsInVehicle) return;
                        playerTakeGuns(player, target);
                        return;
                    case "Изъять нелегал":
                        if (player.IsInVehicle) return;
                        playerTakeIlleagal(player, target);
                        return;
                    case "Продать аптечку":
                        Trigger.PlayerEvent(player, "openInput", "Продать аптечку", "Цена $$$", 6, "player_medkit");
                        return;
                    case "Предложить лечение":
                        if (player.IsInVehicle) return;
                        Trigger.PlayerEvent(player, "openInput", "Предложить лечение", "Цена $$$", 6, "player_heal");
                        return;
                    case "Вылечить":
                        if (player.IsInVehicle) return;
                        playerHealTarget(player, target);
                        return;
                    case "Продать машину":
                        VehicleManager.sellCar(player, target);
                        return;
                    case "Продать квартиру":
                        House house = HouseManager.GetApart(player, true);
                        if (house == null)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет квартиры", 3000);
                            return;
                        }
                        Trigger.PlayerEvent(player, "openInput", "Продать квартиру", "Цена $$$", 10, "player_offerapartsell");
                        return;
                    case "Заселить в квартиру":
                        HouseManager.InviteToRoomApart(player, target);
                        return;
                    case "Пригласить в квартиру":
                        HouseManager.InvitePlayerToApart(player, target);
                        return;
                    case "Продать дом":
                        house = HouseManager.GetHouse(player, true);
                        if (house == null)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                            return;
                        }
                        Trigger.PlayerEvent(player, "openInput", "Продать дом", "Цена $$$", 10, "player_offerhousesell");
                        return;
                    case "Заселить в дом":
                        HouseManager.InviteToRoom(player, target);
                        return;
                    case "Пригласить в дом":
                        HouseManager.InvitePlayerToHouse(player, target);
                        return;
                    case "Передать деньги":
                        if (Main.Players[player].LVL < 1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Перевод денег доступен после первого уровня", 3000);
                            return;
                        }
                        Trigger.PlayerEvent(player, "openInput", "Передать деньги", "Сумма $$$", 10, "player_givemoney");
                        return;
                    case "Предложить обмен":
                        target.SetData("OFFER_MAKER", player);
                        target.SetData("REQUEST", "OFFER_ITEMS");
                        target.SetData("IS_REQUESTED", true);
                        //Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) предложил Вам обменяться предметами. Y/N - принять/отклонить", 3000);
                        Main.OpenAnswer(target, $"Гражданин ({player.Value}) предложил Вам обменяться предметами");
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили гражданину ({target.Value}) обменяться предметами.", 3000);
                        return;
                    case "Мешок":
                        if (player.IsInVehicle) return;
                        Fractions.FractionCommands.playerChangePocket(player, target);
                        return;
                    case "Сорвать маску":
                        if (player.IsInVehicle) return;
                        Fractions.FractionCommands.playerTakeoffMask(player, target);
                        return;
                    case "Посадить в КПЗ":
                        if (player == target)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно применить на себе", 3000);
                            return;
                        }
                        if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                            return;
                        }
                        if (player.Position.DistanceTo(target.Position) > 2)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин слишком далеко", 3000);
                            return;
                        }
                        if (!NAPI.Data.GetEntityData(player, "IS_IN_ARREST_AREA"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны быть возле камеры", 3000);
                            return;
                        }
                        if (Main.Players[target].ArrestTime != 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин уже в тюрьме", 3000);
                            return;
                        }
                        if (!NAPI.Data.GetEntityData(target, "CUFFED"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин не в наручниках", 3000);
                            return;
                        }
                        player.SetData("TARGET", target);
                        Trigger.PlayerEvent(player, "openkpz", $"Гражданин {target.Name}");
                        Log.Write("OPEN MENU");
                        return;
                    case "Выписать штраф":
                        if (player.IsInVehicle) return;
                        player.SetData("TICKETTARGET", target);
                        Trigger.PlayerEvent(player, "openInput", "Выписать штраф (сумма)", "Сумма от 0 до 999 999$", 6, "player_ticketsum");
                        return;
                    default:
                        return;
                }
            }
            catch (Exception e) { Log.Write($"pSelected: " + e.ToString(), nLog.Type.Error); }
        }

        [RemoteEvent("sendkpz")]
        static void SendKpz(Player player, string name, string time)
        {
            try
            {
                if (!player.HasData("TARGET")) return;

                Player target = player.GetData<Player>("TARGET");
                int mytime = 0;
                if (string.IsNullOrEmpty(name))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Неправильно указана причина", 3000);
                    return;
                }

                try
                {
                    mytime = Convert.ToInt32(time);
                }
                catch
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Неправильно указано время", 3000);
                    return;
                }

                if (player == target)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно применить на себе", 3000);
                    return;
                }
                if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                    return;
                }
                if (player.Position.DistanceTo(target.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин слишком далеко", 3000);
                    return;
                }
                if (!NAPI.Data.GetEntityData(player, "IS_IN_ARREST_AREA"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны быть возле камеры", 3000);
                    return;
                }
                if (Main.Players[target].ArrestTime != 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин уже в тюрьме", 3000);
                    return;
                }
                if (!NAPI.Data.GetEntityData(target, "CUFFED"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин не в наручниках", 3000);
                    return;
                }
                if (NAPI.Data.HasEntityData(target, "FOLLOWING"))
                {
                     Fractions.FractionCommands.unFollow(target.GetData<Player>("FOLLOWING"), target);
                }
                Fractions.FractionCommands.unCuffPlayer(target);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы посадили Гражданина ({target.Value}) на {time} минут за ({name})", 3000);
                Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) посадил Вас на {time} минут за ({name})", 3000);
                Commands.RPChat("me", player, " поместил {name} в КПЗ", target);
                Fractions.Manager.sendFractionMessage(7, $"{player.Name} посадил в КПЗ {target.Name} ({name})", true);
                Fractions.Manager.sendFractionMessage(9, $"{player.Name} посадил в КПЗ {target.Name} ({name})", true);
                Main.Players[target].ArrestTime = mytime * 60;
                GameLog.Arrest(Main.Players[player].UUID, Main.Players[target].UUID, name, Convert.ToInt32(time), player.Name, target.Name);
                Fractions.FractionCommands.arrestPlayer(target);

            }
            catch { }
        }

        public static void playerTransferMoney(Player player, string arg)
        {
            if(Main.Players[player].LVL < 1) {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Передача денег будет доступна начиная с 1 уровня.", 3000);
                return;
            }
            try
            {
                Convert.ToInt32(arg);
            }
            catch
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                return;
            }
            var amount = Convert.ToInt32(arg);
            if (amount < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                return;
            }
            Player target = player.GetData<Player>("SELECTEDPLAYER");
            if (!Main.Players.ContainsKey(target) || player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин слишком далеко от Вас", 3000);
                return;
            }
            if (amount > Main.Players[player].Money)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно средств", 3000);
                return;
            }
            if (player.HasData("NEXT_TRANSFERM") && DateTime.Now < player.GetData<DateTime>("NEXT_TRANSFERM") && Main.Players[player].AdminLVL == 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "С момента последней передачи денег прошло мало времени.", 3000);
                return;
            }
            player.SetData("NEXT_TRANSFERM", DateTime.Now.AddMinutes(1));
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) передал Вам {amount}$", 3000);
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы передали гражданину ({target.Value}) {amount}$", 3000);
            MoneySystem.Wallet.Change(target, amount);
            MoneySystem.Wallet.Change(player, -amount);
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[target].UUID})", amount, $"transfer");
            Commands.RPChat("me", player, $"передал(а) {amount}$ " + "{name}", target);
        }
        public static void playerHealTarget(Player player, Player target)
        {
            try
            {
                if (player.Position.DistanceTo(target.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин слишком далеко от Вас", 3000);
                    return;
                }
                var item1 = nInventory.Find(Main.Players[player].UUID, ItemType.HealthKit);
                    if (item1 == null || item1.Count < 1)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет аптечки", 3000);
                        return;
                    }
                    nInventory.Remove(player, ItemType.HealthKit, 1);


                if (target.HasData("IS_DYING"))
                {
                    player.PlayAnimation("amb@medic@standing@tendtodead@idle_a", "idle_a", 39);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы начали реанимирование гражданина ({target.Value})", 3000);
                    Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) начал реанимировать Вас", 3000);
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            player.StopAnimation();
                            NAPI.Entity.SetEntityPosition(player, player.Position + new Vector3(0, 0, 0.5));

                            if (Main.Players[player].FractionID != 8)
                            {
                                var random = new Random();
                                var rnd = random.Next(1, 10);
                                if (Main.Players[player].Licenses.Count >= 8 && Main.Players[player].Licenses[8] == false)
                                {
                                    if (rnd <= 5)
                                    {
                                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({target.Value}) чуть ласты не склеил. У Вас не вышло его реанимировать", 3000);
                                        return;
                                    }
                                }
                                
                            }
                            else
                            {
                                if (!target.HasData("NEXT_DEATH_MONEY") || DateTime.Now > target.GetData<DateTime>("NEXT_DEATH_MONEY"))
                                {
                                    MoneySystem.Wallet.Change(player, 150);
                                    GameLog.Money($"server", $"player({Main.Players[player].UUID})", 150, $"revieve({Main.Players[target].UUID})");
                                    target.SetData("NEXT_DEATH_MONEY", DateTime.Now.AddMinutes(15));
                                }
                            }

                            target.StopAnimation();
                            NAPI.Entity.SetEntityPosition(target, target.Position + new Vector3(0, 0, 0.5));
                            target.SetSharedData("InDeath", false);
                            Trigger.PlayerEvent(target, "DeathTimer", false);
                            target.Health = 80;
                            target.ResetData("IS_DYING");
                            Main.Players[target].IsAlive = true;
                            Main.OffAntiAnim(target);
                            if (target.HasData("DYING_TIMER"))
                            {
                                //Main.StopT(target.GetData("DYING_TIMER"), "timer_18");
                                Timers.Stop(target.GetData<string>("DYING_TIMER"));
                                target.ResetData("DYING_TIMER");
                            }
                            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) реанимировал Вас", 3000);
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы реанимировали гражданина ({target.Value})", 3000);
							

                            if (target.HasData("CALLEMS_BLIP"))
                            {
                                NAPI.Entity.DeleteEntity(target.GetData<Blip>("CALLEMS_BLIP"));
								Fractions.Manager.sendFractionMessage(8, $"Гражданин {player.Name.Replace('_', ' ')} был вылечен кем то другим. Можете не выезжать.");
                            }
                            if (target.HasData("CALLEMS_COL"))
                            {
                                NAPI.ColShape.DeleteColShape(target.GetData<ColShape>("CALLEMS_COL"));
                            }
                        }
                        catch (Exception e) { Log.Write("playerHealedtarget: " + e.Message, nLog.Type.Error); }
                    }, 15000);
                }
                else
                {
                    Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) вылечил Вас с помощью аптечки", 3000);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы вылечили гражданина ({target.Value}) с помощью аптечки", 3000);
                    target.Health = 100;
                }
                return;
            }
            catch (Exception e) { Log.Write("playerHealTarget: " + e.Message); }
        }
        public static void playerTakeGuns(Player player, Player target)
        {
            try
            {
                if (player.Position.DistanceTo(target.Position) > 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин слишком далеко от Вас", 3000);
                    return;
                }
                if (!Fractions.Manager.canUseCommand(player, "takeguns")) return;
                foreach (nItem item in nInventory.Items[Main.Players[target].UUID])
                {
                    if (nInventory.WeaponsItems.Contains(item.Type))
                    {
                        if (Fractions.Stocks.TryAdd(Main.Players[player].FractionID, new nItem(item.Type, 1)) != 0) continue;
                        Fractions.Stocks.Add(Main.Players[player].FractionID, item);
                    }
                }


                Weapons.RemoveAll(target, true);
                Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) изъял у Вас всё оружие", 3000);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы изъяли всё оружие у гражданина ({target.Value})", 3000);
                return;
            }
            catch (Exception e) { Log.Write("playerTakeGuns: " + e.ToString(), nLog.Type.Error); }
        }
        public static void playerTakeIlleagal(Player player, Player target)
        {
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин слишком далеко от Вас", 3000);
                return;
            }
            var matItem = nInventory.Find(Main.Players[target].UUID, ItemType.Material);
            var drugItem = nInventory.Find(Main.Players[target].UUID, ItemType.Drugs);
            var materials = (matItem == null) ? 0 : matItem.Count;
            var drugs = (drugItem == null) ? 0 : drugItem.Count;
            if (materials < 1 && drugs < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин не имеет ничего запрещённого", 3000);
                return;
            }
            nInventory.Remove(target, ItemType.Material, materials);
            nInventory.Remove(target, ItemType.Drugs, drugs);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) изъял у Вас запрещённые предметы", 3000);
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы изъяили у гражданина {target.Value} запрещённые предметы", 3000);
            return;
        }
        public static void playerOfferChangeItems(Player player)
        {
            if (!Main.Players.ContainsKey(player) || !player.HasData("OFFER_MAKER") || !Main.Players.ContainsKey(player.GetData<Player>("OFFER_MAKER"))) return;
            Player offerMaker = player.GetData<Player>("OFFER_MAKER");
            if (Main.Players[player].ArrestTime > 0 || Main.Players[offerMaker].ArrestTime > 0)
            {
                player.ResetData("OFFER_MAKER");
                return;
            }
            if (player.Position.DistanceTo(offerMaker.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин слишком далеко", 3000);
                return;
            }

            player.SetData("CHANGE_WITH", offerMaker);
            offerMaker.SetData("CHANGE_WITH", player);

            GUI.Dashboard.OpenOut(player, new List<nItem>(), offerMaker.Name, 5);
            GUI.Dashboard.OpenOut(offerMaker, new List<nItem>(), player.Name, 5);

            player.ResetData("OFFER_MAKER");
        }
        public static void playerHandshakeTarget(Player player, Player target)
        {
            if((!player.HasData("CUFFED") && !player.HasSharedData("InDeath")) || player.HasData("CUFFED") && player.GetData<bool>("CUFFED") == false && player.HasSharedData("InDeath") && player.GetSharedData<bool>("InDeath") == false) {
                if((!target.HasData("CUFFED") && !target.HasSharedData("InDeath")) || target.HasData("CUFFED") && target.GetData<bool>("CUFFED") == false && target.HasSharedData("InDeath") && target.GetSharedData<bool>("InDeath") == false) {
                    target.SetData("HANDSHAKER", player);
                    target.SetData("REQUEST", "HANDSHAKE");
                    target.SetData("IS_REQUESTED", true);
                    //Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) хочет познакомится. Y/N - принять/отклонить", 3000);
                    Main.OpenAnswer(target, $"Гражданин ({player.Value}) хочет познакомится");
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили гражданину ({target.Value}) пожать руку.", 3000);
                } else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно пожать руку гражданину в данный момент", 3000);
            } else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно пожать руку гражданину в данный момент", 3000);
        }
        public static void hanshakeTarget(Player player)
        {
            if (!Main.Players.ContainsKey(player) || !player.HasData("HANDSHAKER") || !Main.Players.ContainsKey(player.GetData<Player>("HANDSHAKER"))) return;
            Player target = player.GetData<Player>("HANDSHAKER");
            if((!player.HasData("CUFFED") && !player.HasSharedData("InDeath")) || player.HasData("CUFFED") && player.GetData<bool>("CUFFED") == false && player.HasSharedData("InDeath") && player.GetSharedData<bool>("InDeath") == false) {
                if((!target.HasData("CUFFED") && !target.HasSharedData("InDeath")) || target.HasData("CUFFED") && target.GetData<bool>("CUFFED") == false && target.HasSharedData("InDeath") && target.GetSharedData<bool>("InDeath") == false) {
                    player.PlayAnimation("mp_ped_interaction", "handshake_guy_a", 39);
                    target.PlayAnimation("mp_ped_interaction", "handshake_guy_b", 39);

                    Trigger.PlayerEvent(player, "newFriend", target);
                    Trigger.PlayerEvent(target, "newFriend", player);

                    Main.OnAntiAnim(player);
                    Main.OnAntiAnim(target);

                    NAPI.Task.Run(() => { try { Main.OffAntiAnim(player); Main.OffAntiAnim(target); player.StopAnimation(); target.StopAnimation(); } catch { } }, 4500);
                }
            }
        }
		
		
		public static void playerKissTarget(Player player, Player target)
        {
            if((!player.HasData("CUFFED") && !player.HasSharedData("InDeath")) || player.HasData("CUFFED") && player.GetData<bool>("CUFFED") == false && player.HasSharedData("InDeath") && player.GetSharedData<bool>("InDeath") == false) {
                if((!target.HasData("CUFFED") && !target.HasSharedData("InDeath")) || target.HasData("CUFFED") && target.GetData<bool>("CUFFED") == false && target.HasSharedData("InDeath") && target.GetSharedData<bool>("InDeath") == false) {
                    target.SetData("KISS", player);
                    target.SetData("REQUEST", "KISS");
                    target.SetData("IS_REQUESTED", true);
                    //Notify.Send(target, NotifyType.Warning, NotifyPosition.BottomCenter, $"({player.Value}) хочет поцеловать Вас. Y/N - согласиться/отказаться", 3000);
                    Main.OpenAnswer(target, $"Гражданин ({player.Name}) хочет поцеловать Вас");
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили ({target.Value}) поцеловаться.", 3000);
                } else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Не хочет целоваться", 3000);
            } else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Не может поцеловаться", 3000);
        }
        public static void kissTarget(Player player)
        {
            if (!Main.Players.ContainsKey(player) || !player.HasData("KISS") || !Main.Players.ContainsKey(player.GetData<Player>("KISS"))) return;
            Player target = player.GetData<Player>("KISS");
            if((!player.HasData("CUFFED") && !player.HasSharedData("InDeath")) || player.HasData("CUFFED") && player.GetData<bool>("CUFFED") == false && player.HasSharedData("InDeath") && player.GetSharedData<bool>("InDeath") == false) {
                if((!target.HasData("CUFFED") && !target.HasSharedData("InDeath")) || target.HasData("CUFFED") && target.GetData<bool>("CUFFED") == false && target.HasSharedData("InDeath") && target.GetSharedData<bool>("InDeath") == false) {
                    player.PlayAnimation("mp_ped_interaction", "kisses_guy_a", 39);
                    target.PlayAnimation("mp_ped_interaction", "kisses_guy_b", 39);

                    //Trigger.PlayerEvent(player, "newFriend", target);
                    //Trigger.PlayerEvent(target, "newFriend", player);

                    Main.OnAntiAnim(player);
                    Main.OnAntiAnim(target);

                    NAPI.Task.Run(() => { try { Main.OffAntiAnim(player); Main.OffAntiAnim(target); player.StopAnimation(); target.StopAnimation(); } catch { } }, 4500);
                }
            }
        }
		
        internal class SearchObject
        {
            public string Name { get; set; }
            public List<string> Weapons { get; set; }
            public List<string> Items { get; set; }
        }

        [RemoteEvent("aSelected")]
        public static void animationSelected(Player player, int category, int animation)
        {
            try
            {
                if (player.HasData("AntiAnimDown") || player.HasData("FOLLOWING") || player.IsInVehicle
                || Main.Players[player].ArrestTime > 0 || Main.Players[player].DemorganTime > 0 || player.HasData("ROOLING")) return;

                if (category == -1)
                {
                    player.ResetData("HANDS_UP");
                    player.StopAnimation();
                    if (player.HasData("LastAnimFlag") && player.GetData<int>("LastAnimFlag") == 39)
                        NAPI.Entity.SetEntityPosition(player, player.Position + new Vector3(0, 0, 0.2));
                    return;
                }
                if (category == 8)
                { // Лицевые эмоции
                    player.SetSharedData("playermood", animation);
                    NAPI.ClientEvent.TriggerClientEventInRange(player.Position, 250, "Player_SetMood", player, animation);
                    return;
                }
                else if (category == 6)
                { // Стили походки
                    player.SetSharedData("playerws", animation);
                    NAPI.ClientEvent.TriggerClientEventInRange(player.Position, 250, "Player_SetWalkStyle", player, animation);
                    return;
                }
                else
                {
                    if (animation >= AnimList[category].Count) return;
                    player.PlayAnimation(AnimList[category][animation].Dictionary, AnimList[category][animation].Name, AnimList[category][animation].Flag);
                    if (category == 0 && animation == 0) NAPI.Entity.SetEntityPosition(player, player.Position - new Vector3(0, 0, 0.3));

                    if (AnimList[category][animation].Dictionary == "random@arrests@busted" && AnimList[category][animation].Name == "idle_c") player.SetData("HANDS_UP", true);

                    player.SetData("LastAnimFlag", AnimList[category][animation].Flag);
                    if (AnimList[category][animation].StopDelay != -1)
                    {
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (player != null && !player.HasData("AntiAnimDown") && !player.HasData("FOLLOWING"))
                                {
                                    player.StopAnimation();
                                }
                            }
                            catch { }
                        }, AnimList[category][animation].StopDelay);
                    }
                }
            }
            catch (Exception e) { Log.Write("aSelected: " + e.Message, nLog.Type.Error); }
        }

        public static List<List<Animation>> AnimList = new List<List<Animation>>()
        {
            new List<Animation>()
            {
				//Социальные
			    new Animation("random@arrests@busted", "idle_c", 35),
                new Animation("random@arrests@busted", "idle_c", 35),
                new Animation("busted", "idle_2_hands_up", 2),
                new Animation("amb@code_human_cower@female@idle_a", "idle_c", 35),
                new Animation("special_ped@clinton@monologue_6@monologue_6d", "war_crimes_3", 35),
                new Animation("rcmfanatic1celebrate", "celebrate", 135),
                new Animation("amb@world_human_cheering@female_a", "base", 35),
                new Animation("random@street_race", "_streetracer_accepted", 35),
                new Animation("amb@world_human_cheering@male_b", "base", 35),
                new Animation("random@prisoner_lift", "arms_waving", 35),
                new Animation("anim@mp_player_intcelebrationfemale@bro_love", "bro_love", 1),
                new Animation("anim@mp_player_intupperslow_clap", "idle_a", 35),
                new Animation("amb@world_human_cheering@female_d", "base", 35),
                new Animation("amb@world_human_cheering@male_a", "base", 1),
                new Animation("mp_player_int_uppersalute", "mp_player_int_salute_enter", 50),
                new Animation("misscarsteal4@actor", "actor_berating_loop", 35),
                new Animation("anim@mp_player_intincarno_waybodhi@ps@", "idle_a_fp", 35),
                new Animation("anim@mp_player_intcelebrationfemale@face_palm", "face_palm", 35),
                new Animation("amb@code_human_in_car_mp_actions@tit_squeeze@std@ps@base", "idle_a", 35),
                new Animation("amb@code_human_in_car_mp_actions@rock@bodhi@rps@base", "idle_a", 35),
                new Animation("anim@mp_player_intincaryou_locobodhi@ds@", "idle_a_fp", 35),
                new Animation("anim@mp_player_intselfieblow_kiss", "exit", 35),
                new Animation("misscarsteal4@director_grip", "end_loop_director", 35),
                new Animation("anim@mp_player_intupperpeace", "idle_a", 35),
                new Animation("anim@mp_player_intupperthumbs_up", "idle_a_fp", 35),
                new Animation("anim@mp_player_intupperyou_loco", "idle_a", 35),
                new Animation("anim@mp_player_intcelebrationfemale@finger_kiss", "finger_kiss", 1),
                new Animation("random@street_race", "_car_a_flirt_girl", 35),
                new Animation("random@robbery", "f_cower_01", 35),
            },
            new List<Animation>()
            {
				//Сидеть/Лежать
			new Animation("amb@world_human_bum_slumped@male@laying_on_left_side@base", "base", 35),
            new Animation("amb@lo_res_idles@", "lying_face_up_lo_res_base", 35),
            new Animation("rcmtmom_2leadinout", "tmom_2_leadout_loop", 35),
            new Animation("amb@lo_res_idles@", "lying_face_down_lo_res_base", 35),
            new Animation("amb@world_human_sunbathe@female@front@idle_a", "idle_c", 35),
            new Animation("combat@damage@writhe", "writhe_loop", 35),
            new Animation("timetable@denice@ig_1", "base", 35),
            new Animation("anim@heists@fleeca_bank@ig_7_jetski_owner", "owner_idle", 35),
            new Animation("misstrevor3_beatup", "guard_beatup_kickidle_dockworker", 35),
            new Animation("amb@medic@standing@kneel@base", "base", 1),
            new Animation("missfam2leadinoutmcs3", "onboat_leadin_pornguy_a", 35),
            new Animation("random@robbery", "sit_down_idle_01", 35),
            new Animation("timetable@reunited@ig_10", "base_amanda", 35),
            new Animation("misstrevor2", "gang_chatting_base_a", 35),
            new Animation("amb@world_human_picnic@female@base", "base", 35),
            new Animation("switch@michael@tv_w_kids", "001520_02_mics3_14_tv_w_kids_idle_trc", 35),
            new Animation("amb@world_human_picnic@male@base", "base", 35),
            new Animation("amb@world_human_stupor@male@base", "base", 35),
            new Animation("anim@amb@nightclub@lazlow@lo_alone@", "lowalone_dlg_longrant_laz", 35),
            new Animation("missheistdockssetup1ig_10@base", "talk_pipe_base_worker1", 35),
            },
            new List<Animation>()
            {
				//Физ. упражнения
			new Animation("amb@world_human_sit_ups@male@base", "base", 35),
            new Animation("amb@world_human_push_ups@male@base", "base", 35),
            new Animation("anim@arena@celeb@flat@solo@no_props@", "flip_a_player_a", 35),
            new Animation("anim@mp_player_intupperknuckle_crunch", "idle_a", 35),
            new Animation("amb@world_human_yoga@female@base", "base_b", 35),
            new Animation("amb@world_human_yoga@female@base", "base_c", 35),
            new Animation("missfam5_yoga", "f_yogapose_b", 35),
            new Animation("missfam5_yoga", "f_yogapose_b", 35),
            new Animation("missfam5_yoga", "a3_pose", 35),
            new Animation("missfam5_yoga", "a2_pose", 35),
            new Animation("rcmfanatic1", "jogging_on_spot", 35),
            new Animation("amb@world_human_muscle_flex@arms_in_front@base", "base", 35),
            new Animation("rcmfanatic1maryann_stretchidle_b", "idle_e", 35),
            new Animation("rcmcollect_paperleadinout@", "meditiate_idle", 35),

            },
            new List<Animation>()
            {
				//Неприличные
			new Animation("veh@driveby@first_person@driver@unarmed", "intro_0", 50),
            new Animation("mp_player_int_upperwank", "mp_player_int_wank_01", 35),
            new Animation("anim@mp_player_intcelebrationfemale@chicken_taunt", "chicken_taunt", 35),
            new Animation("switch@trevor@mocks_lapdance", "001443_01_trvs_28_idle_stripper", 35),
            new Animation("anim@mp_player_intincardockbodhi@rds@", "idle_a_fp", 35),
            new Animation("anim@mp_player_intuppernose_pick", "idle_a", 35),
            new Animation("anim@mp_player_intcelebrationfemale@jazz_hands", "jazz_hands", 35),
            new Animation("anim@mp_player_intcelebrationfemale@thumb_on_ears", "thumb_on_ears", 35),
            new Animation("anim@mp_corona_idles@male_a@idle_a", "idle_e", 35),
            new Animation("mini@strip_club@backroom@", "stripper_b_backroom_idle_b", 35),
            new Animation("mp_player_int_uppergrab_crotch", "mp_player_int_grab_crotch", 35),
            new Animation("anim@mp_player_intcelebrationfemale@air_shagging", "air_shagging", 35),
            new Animation("rcmpaparazzo_2", "shag_action_a", 35),
            new Animation("rcmpaparazzo_2", "shag_action_poppy", 35),
            new Animation("rcmpaparazzo_2", "shag_loop_a", 35),
            new Animation("rcmpaparazzo_2", "shag_loop_poppy", 35),
            new Animation("timetable@trevor@skull_loving_bear", "skull_loving_bear", 35),
            },
            new List<Animation>()
            {
				//Стойка
			new Animation("amb@world_human_stand_guard@male@base", "base", 35),
            new Animation("amb@world_human_stand_impatient@female@no_sign@base", "base", 35),
            new Animation("anim@amb@casino@hangout@ped_male@stand@02b@base", "base", 35),
            new Animation("amb@world_human_prostitute@hooker@base", "base", 35),
            new Animation("switch@franklin@lamar_tagging_wall", "lamar_tagging_wall_loop_franklin", 35),
            new Animation("amb@world_human_hang_out_street@female_arms_crossed@base", "base", 35),
            new Animation("amb@world_human_leaning@male@wall@back@legs_crossed@base", "base", 35),
            new Animation("anim@amb@nightclub@gt_idle@", "base", 35),
            new Animation("amb@world_human_leaning@male@wall@back@foot_up@base", "base", 35),
            new Animation("anim@miss@low@fin@vagos@", "idle_ped06", 35),
            new Animation("missfam5_yoga", "c1_pose", 35),
            new Animation("rcmbarry", "base", 35),
            new Animation("timetable@amanda@ig_2", "ig_2_base_amanda", 35),
            new Animation("amb@world_human_hang_out_street@female_hold_arm@base", "base", 35),
            },
            new List<Animation>()
            {
				//танцы
            new Animation("special_ped@mountain_dancer@monologue_3@monologue_3a", "mnt_dnc_buttwag", 35),
            new Animation("misschinese2_crystalmazemcs1_ig", "dance_loop_tao", 35),
            new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_a@", "high_center", 35),
            new Animation("mini@strip_club@private_dance@part2", "priv_dance_p2", 35),
            new Animation("mp_safehouse", "lap_dance_girl", 35),
            new Animation("mini@strip_Club@private_dance@part3", "priv_dance_p3", 35),
            new Animation("mini@strip_club@lap_dance_2g@ld_2g_p2", "ld_2g_p2_s1", 35),
            new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_hi_11_buttwiggle_f_laz", 35),
            new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_hi_11_turnaround_laz", 35),
            new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_hi_15_crazyrobot_laz", 35),
            new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_mi_15_robot_laz", 35),
            new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_hi_17_spiderman_laz", 35),
            new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_hi_17_smackthat_laz", 35),
            new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_li_17_ethereal_laz", 35),
            new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_mi_17_crotchgrab_laz", 35),
            new Animation("anim@amb@nightclub@lazlow@hi_railing@", "ambclub_10_mi_hi_crotchhold_laz", 35),
            new Animation("move_clown@p_m_two_idles@", "fidget_short_dance", 35),
            new Animation("mini@strip_club@idles@dj@idle_04", "idle_04", 35),
            new Animation("anim@mp_player_intupperchicken_taunt", "idle_a", 35),
            new Animation("missfbi3_sniping", "dance_m_default", 35),
            new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_a@", "low_center_up", 35),
            new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_b@", "high_center", 35),
            new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_b@", "med_center_down", 35),
            new Animation("anim@amb@nightclub@dancers@black_madonna_entourage@", "hi_dance_facedj_09_v2_male^5", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_09_v1_female^6", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_09_v1_male^2", 35),
            new Animation("timetable@tracy@ig_5@idle_a", "idle_a", 35),
            new Animation("timetable@tracy@ig_5@idle_a", "idle_b", 35),
            new Animation("timetable@tracy@ig_5@idle_a", "idle_c", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_15_v2_male^6", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_17_v2_male^6", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "mi_dance_facedj_15_v2_female^6", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj@hi_intensity", "hi_dance_facedj_09_v1_male^1", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj@hi_intensity", "hi_dance_facedj_15_v1_female^6", 35),
            new Animation("anim@amb@nightclub@mini@dance@dance_solo@female@var_a@", "high_center_down", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@", "trans_dance_facedj_mi_to_hi_08_v1_male^1", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_17_v1_female^6", 35),
            new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_a@", "med_center_up", 35),
            new Animation("anim@amb@casino@mini@dance@dance_solo@female@var_b@", "high_right_down", 35),
            new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_hi_13_flyingv_laz", 35),
            new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_mi_11_pointthrust_laz", 35),
            new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_mi_15_shimmy_laz", 35),
            new Animation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_mi_17_teapotthrust_laz", 35),
            new Animation("anim@amb@nightclub@mini@dance@dance_solo@female@var_a@", "med_center_up", 35),
            new Animation("anim@amb@nightclub@mini@dance@dance_solo@male@var_b@", "high_center_down", 35),
            new Animation("anim@amb@nightclub@mini@dance@dance_solo@male@var_b@", "high_center", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@", "trans_dance_facedj_li_to_mi_11_v1_male^4", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_15_v2_male^2", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_17_v1_male^3", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj@", "hi_dance_facedj_17_v2_female^3", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj@hi_intensity", "hi_dance_facedj_17_v2_male^2", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@", "trans_dance_facedj_hi_to_mi_09_v1_male^4", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@", "trans_dance_facedj_li_to_mi_11_v1_female^3", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@", "trans_dance_facedj_mi_to_hi_09_v1_female^3", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@from_hi_intensity", "trans_dance_facedj_hi_to_li_09_v1_female^3", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@from_low_intensity", "trans_dance_facedj_li_to_hi_09_v1_female^2", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@from_med_intensity", "trans_dance_facedj_mi_to_hi_08_v1_female^3", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@", "trans_dance_facedj_mi_to_li_09_v1_female^3", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_09_v1_female^3", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_09_v2_female^1", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_09_v2_female^3", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_09_v2_female^5", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_11_v1_female^3", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_11_v1_female^1", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_11_v1_male^4", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_13_v2_female^1", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_15_v2_female^1", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_15_v2_female^3", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_17_v1_female^2", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_17_v2_female^2", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "hi_dance_crowd_17_v2_male^4", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "li_dance_crowd_09_v2_female^3", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "mi_dance_crowd_13_v2_female^1", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "mi_dance_crowd_13_v2_female^5", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "mi_dance_crowd_10_v2_female^5", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "mi_dance_crowd_17_v2_female^1", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@", "mi_dance_crowd_17_v2_female^6", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@hi_intensity", "hi_dance_crowd_09_v2_female^3", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@hi_intensity", "hi_dance_crowd_11_v1_female^1", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_groups@hi_intensity", "hi_dance_crowd_17_v2_female^2", 35),
            new Animation("anim@amb@nightclub@dancers@crowddance_facedj_transitions@", "trans_dance_facedj_hi_to_mi_09_v1_female^1", 35),
                },
            new List<Animation>() // NOT USED
            {
                null,
            },
            new List<Animation>()
            {
            },
            new List<Animation>()
            {
            },
            new List<Animation>()
            {
            },
            new List<Animation>()
            {
            },

            new List<Animation>() // NOT USED
            {
                null,
            },
            new List<Animation>()
            {
            },
        };

        internal class Animation
        {
            public string Dictionary { get; }
            public string Name { get; }
            public int Flag { get; }
            public int StopDelay { get; }

            public Animation(string dict, string name, int flag, int stopDelay = -1)
            {
                Dictionary = dict;
                Name = name;
                Flag = flag;
                StopDelay = stopDelay;
            }
        }
    }
}
