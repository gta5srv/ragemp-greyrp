using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using NeptuneEVO.SDK;

//Disapproved by god himself

//Just use the API functions, you have nothing else to worry about

//Things to note
//More things like vehicle mods will be added in the next version

/* API FUNCTIONS:
public static void SetVehicleWindowState(Vehicle veh, WindowID window, WindowState state)
public static WindowState GetVehicleWindowState(Vehicle veh, WindowID window)
public static void SetVehicleWheelState(Vehicle veh, WheelID wheel, WheelState state)
public static WheelState GetVehicleWheelState(Vehicle veh, WheelID wheel)
public static void SetVehicleDirt(Vehicle veh, float dirt)
public static float GetVehicleDirt(Vehicle veh)
public static void SetDoorState(Vehicle veh, DoorID door, DoorState state)
public static DoorState GetDoorState(Vehicle veh, DoorID door)
public static void SetEngineState(Vehicle veh, bool status)
public static bool GetEngineState(Vehicle veh)
public static void SetLockStatus(Vehicle veh, bool status)
public static bool GetLockState(Vehicle veh)
*/

namespace NeptuneEVO.Core
{
    //Enums for ease of use
    public enum WindowID
    {
        WindowFrontRight,
        WindowFrontLeft,
        WindowRearRight,
        WindowRearLeft
    }

    public enum WindowState
    {
        WindowFixed,
        WindowDown,
        WindowBroken
    }

    public enum DoorID
    {
        DoorFrontLeft,
        DoorFrontRight,
        DoorRearLeft,
        DoorRearRight,
        DoorHood,
        DoorTrunk
    }

    public enum DoorState
    {
        DoorClosed,
        DoorOpen,
        DoorBroken,
    }

    public enum WheelID
    {
        Wheel0,
        Wheel1,
        Wheel2,
        Wheel3,
        Wheel4,
        Wheel5,
        Wheel6,
        Wheel7,
        Wheel8,
        Wheel9
    }

    public enum WheelState
    {
        WheelFixed,
        WheelBurst,
        WheelOnRim,
    }

    public class VehicleStreaming : Script
    {
        private static nLog Log = new nLog("VehicleStreaming");
        //This is the data object which will be synced to vehicles
        public class VehicleSyncData
        {
            // Sync pos
            public Vector3 Position { get; set; } = new Vector3();
            public Vector3 Rotation { get; set; } = new Vector3();

            // Stats
            public bool Locked { get; set; } = false;
            public bool Engine { get; set; } = false;

            // Indicators
            public bool LeftIL { get; set; } = false;
            public bool RightIL { get; set; } = false;

            // Dirt
            public float Dirt { get; set; } = 0.0f;

            // Colors
			public int Color1 { get; set; } = -1;
			public int Color2 { get; set; } = -1;

            // Health
            public float BodyHealth { get; set; } = 1000.0f;
            public float EngineHealth { get; set; } = 1000.0f;

            // Doors 0-7 (0 = closed, 1 = open, 2 = broken) (This uses enums so don't worry about it)
            public List<int> Door { get; set; } = new List<int>(8) { 0, 0, 0, 0, 0, 0, 0, 0 };

            // Windows (0 = up, 1 = down, 2 = smashed) (This uses enums so don't worry about it)
            public int[] Window { get; set; } = new int[4] { 0, 0, 0, 0 };

            // Wheels 0-7, 45/47 (0 = fixed, 1 = flat, 2 = missing) (This uses enums so don't worry about it)
            public int[] Wheel { get; set; } = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }
        public static Dictionary<Entity, VehicleSyncData> VehiclesSyncDatas = new Dictionary<Entity, VehicleSyncData>();

        [RemoteEvent("VehStream_SetDoorData")]
        public void VehStreamSetDoorData(Player player, Vehicle veh, int door1state, int door2state, int door3state, int door4state, int door5state, int door6state, int door7state, int door8state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Door[0] = door1state;
            data.Door[1] = door2state;
            data.Door[2] = door3state;
            data.Door[3] = door4state;
            data.Door[4] = door5state;
            data.Door[5] = door6state;
            data.Door[6] = door7state;
            data.Door[7] = door8state;

            UpdateVehicleSyncData(veh, data);

            //Re-distribute the goods
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleDoorStatus", veh.Handle, door1state, door2state, door3state, door4state, door5state, door6state, door7state, door8state);
        }

        [RemoteEvent("VehStream_SetWindowData")]
        public void VehStreamSetWindowData(Player player, Vehicle veh, int window1state, int window2state, int window3state, int window4state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Window[0] = window1state;
            data.Window[1] = window2state;
            data.Window[2] = window3state;
            data.Window[3] = window4state;

            UpdateVehicleSyncData(veh, data);

            //Re-distribute the goods
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleWindowStatus", veh.Handle, window1state, window2state, window3state, window4state);
        }

        [RemoteEvent("VehStream_SetWheelData")]
        public void VehStreamSetWheelData(Player player, Vehicle veh, int wheel1state, int wheel2state, int wheel3state, int wheel4state, int wheel5state, int wheel6state, int wheel7state, int wheel8state, int wheel9state, int wheel10state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Wheel[0] = wheel1state;
            data.Wheel[1] = wheel2state;
            data.Wheel[2] = wheel3state;
            data.Wheel[3] = wheel4state;
            data.Wheel[4] = wheel5state;
            data.Wheel[5] = wheel6state;
            data.Wheel[6] = wheel7state;
            data.Wheel[7] = wheel8state;
            data.Wheel[8] = wheel9state;
            data.Wheel[9] = wheel10state;
            UpdateVehicleSyncData(veh, data);

            //Re-distribute the goods
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleWheelStatus", veh.Handle, wheel1state, wheel2state, wheel3state, wheel4state, wheel5state, wheel6state, wheel7state, wheel8state, wheel9state, wheel10state);
        }

        [ServerEvent(Event.EntityDeleted)]
        public void Event_EntityDeleted(Entity entity)
        {
            try
            {
                if (NAPI.Entity.GetEntityType(entity) == EntityType.Vehicle && VehiclesSyncDatas.ContainsKey(entity)) VehiclesSyncDatas.Remove(entity);

            }
            catch (Exception e) { Log.Write("Event_EntityDeleted: " + e.Message); }
        }

        [ServerEvent(Event.EntityCreated)]
        public void Event_EntityCreated(Entity entity)
        {
            try
            {
                if (entity.Type == EntityType.Vehicle)
                {
                    
                    Vehicle vehicle = NAPI.Entity.GetEntityFromHandle<Vehicle>(entity);
                    if (vehicle != null) { vehicle.SetSharedData("MODELHASH", vehicle.Model.ToString()); NAPI.Entity.SetEntityModel(vehicle, vehicle.Model); }
                    
                }
            }
            catch (Exception e) { Log.Write("Event_EntityCreated" + e.Message); }
        }

        [ServerEvent(Event.PlayerExitVehicleAttempt)]
        public void VehStreamExitAttempt(Player player, Vehicle veh)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Position = veh.Position;
            data.Rotation = veh.Rotation;

            UpdateVehicleSyncData(veh, data);
            Trigger.PlayerEvent(player, "VehStream_PlayerExitVehicleAttempt", veh, data.Engine);
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void VehStreamEnter(Player player, Vehicle veh, sbyte seat)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Position = veh.Position;
            data.Rotation = veh.Rotation;

            // кастыль
            if (seat == 0)
                SetEngineState(veh, GetEngineState(veh));

            UpdateVehicleSyncData(veh, data);
            Trigger.PlayerEvent(player, "VehStream_PlayerEnterVehicle", veh, seat, data.Engine);
        }

        static Dictionary<int, float> DamageMULT = new Dictionary<int, float>
        {
            {-1, 2f },
            {0, 1.7f },
            {1, 1.4f },
            {2, 1.2f },
            {3, 1.0f },
            {4, 0 },
        };


        [ServerEvent(Event.VehicleDamage)]
        public void VehDamage(Vehicle veh, float bodyHealthLoss, float engineHealthLoss)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            float mult = 2;

            if (VehicleManager.Vehicles.ContainsKey(veh.NumberPlate))
                mult = DamageMULT[ VehicleManager.Vehicles[veh.NumberPlate].Components.Armor ];

            data.BodyHealth -= bodyHealthLoss * mult;
            data.EngineHealth -= engineHealthLoss * mult;

            NAPI.Vehicle.SetVehicleBodyHealth(veh, data.BodyHealth);
            NAPI.Vehicle.SetVehicleEngineHealth(veh, data.EngineHealth);

            //Log.Write($"VehicleDamage {bodyHealthLoss} ? {engineHealthLoss} | {data.BodyHealth} ? {data.EngineHealth} | MULT: {mult}");

            UpdateVehicleSyncData(veh, data);

            //if (NAPI.Vehicle.GetVehicleDriver(veh) != default(Player)) //Doesn't work?
              //  NAPI.ClientEvent.TriggerClientEvent(NAPI.Vehicle.GetVehicleDriver(veh), "VehStream_PlayerExitVehicleAttempt", veh);
        }

        public static WindowState GetVehicleWindowState(Vehicle veh, WindowID window)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return (WindowState)data.Window[(int)window];
        }

        public static void SetVehicleWheelState(Vehicle veh, WheelID wheel, WheelState state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Wheel[(int)wheel] = (int)state;
            UpdateVehicleSyncData(veh, data);
            NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleWheelStatus_Single", veh.Handle, (int)wheel, (int)state);
        }

        public static WheelState GetVehicleWheelState(Vehicle veh, WheelID wheel)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return (WheelState)data.Wheel[(int)wheel];
        }

        public static void SetDoorState(Vehicle veh, DoorID door, DoorState state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Door[(int)door] = (int)state;
            UpdateVehicleSyncData(veh, data);
            Trigger.PlayerEventInRange(veh.Position, 250, "VehStream_SetVehicleDoorStatus_Single", veh, (int)door, (int)state);
        }

        public static DoorState GetDoorState(Vehicle veh, DoorID door)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return (DoorState)data.Door[(int)door];
        }

        public static void SetEngineState(Vehicle veh, bool status)
        {
            NAPI.Task.Run(() => { 
                NAPI.Vehicle.SetVehicleEngineStatus(veh, status);
                VehicleSyncData data = GetVehicleSyncData(veh);
                if (data == default(VehicleSyncData))
                    data = new VehicleSyncData();

                data.Engine = status;
                data.RightIL = false;
                data.LeftIL = false;
                veh.SetSharedData("rightlight", false);
                veh.SetSharedData("leftlight", false);
                veh.EngineStatus = status;
                UpdateVehicleSyncData(veh, data);
                NAPI.Vehicle.SetVehicleEngineStatus(veh, status);
                Trigger.PlayerEventInRange(veh.Position, 250, "VehStream_SetEngineStatus", veh, status, true, data.LeftIL, data.RightIL);
            }, 130);
        }

        public static bool GetEngineState(Vehicle veh)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return data.Engine;
        }

        public static void SetVehicleIndicatorLights(Vehicle veh, int light, bool state) {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData)) data = new VehicleSyncData();
            if(data.Engine) {
                if (light == 0) {
                    veh.SetSharedData("rightlight", state);
                    data.RightIL = state;
                } else {
                    veh.SetSharedData("leftlight", state);
                    data.LeftIL = state;
                }
                UpdateVehicleSyncData(veh, data);
                NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleIndicatorLights_Single", veh.Handle, light, state);
            }
        }

        public static bool GetVehicleIndicatorLights(Vehicle veh, bool light) {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData)) {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            if (!light) return data.RightIL;
            else return data.LeftIL;
        }

        public static void SetLockStatus(Vehicle veh, bool status)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            veh.SetSharedData("LOCKED", status);
            data.Locked = status;
            veh.Locked = status;
            UpdateVehicleSyncData(veh, data);
            Trigger.PlayerEventInRange(veh.Position, 250, "VehStream_SetLockStatus", veh, status);
        }

        public static bool GetLockState(Vehicle veh)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return data.Locked;
        }

        [RemoteEvent("VehStream_RadioChange")]
        public void VehStreamRadioChange(Player Player, Vehicle vehicle, short index)
        {
            try
            {
                if(Player.Vehicle != vehicle) return;
                NAPI.Data.SetEntitySharedData(vehicle, "vehradio", index);
            }
            catch (Exception e) { Log.Write("VehStream_RadioChange: " + e.Message); }
        }

        [RemoteEvent("VehStream_RequestFixStreamIn")]
        public void VehicleFixStreamIn(Player player, Vehicle veh)
        {
            try
            {
                if (veh != null && NAPI.Entity.DoesEntityExist(veh))
                {
                    VehicleSyncData data = GetVehicleSyncData(veh);
                    if (data == default(VehicleSyncData)) data = new VehicleSyncData();
                    UpdateVehicleSyncData(veh, data);

                    List<object> vData = new List<object>()
                    {
                        veh.NumberPlate,
                        veh.PrimaryColor,
                        veh.SecondaryColor,
                    };
                    if (veh.HasData("ACCESS") && veh.GetData<string>("ACCESS") == "PERSONAL")
                    {
                        vData.Add(VehicleManager.Vehicles[veh.NumberPlate].Components);
                        vData.Add(VehicleManager.Vehicles[veh.NumberPlate].Dirt);
                    }
                    Trigger.PlayerEvent(player, "VehStream_FixStreamIn", veh.Handle, JsonConvert.SerializeObject(vData));
                }
                return;
            }
            catch (Exception e) { Log.Write("VehStream_RequestFixStreamIn: " + e.Message, nLog.Type.Error); return; }
        }


		
		public static void SetVehicleColors(Vehicle veh, int color1, int color2)
		{
			VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData)) data = new VehicleSyncData();
			data.Color1 = color1;
			data.Color2 = color2;
			UpdateVehicleSyncData(veh, data);
			Trigger.PlayerEventInRange(veh.Position, 250, "VehStream_SetVehicleColors", color1, color2);
		}

        [RemoteEvent("VehStream_SetVehicleDirt")]
        public void SetVehicleDirtLevel(Player player, Vehicle vehicle, float dirt)
        {
            try
            {
                if (vehicle != null) SetVehicleDirt(vehicle, dirt);
            }
            catch (Exception e) { Log.Write("VehStream_SetVehicleDirt: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("VehStream_SetIndicatorLightsData")]
        public void VehStreamSetIndicatorLightsData(Player player, Vehicle veh, bool leftstate, bool rightstate) {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData)) data = new VehicleSyncData();
            if(data.Engine) {
                data.RightIL = rightstate;
                data.LeftIL = leftstate;
                veh.SetSharedData("rightlight", rightstate);
                veh.SetSharedData("leftlight", leftstate);
                UpdateVehicleSyncData(veh, data);
                Trigger.PlayerEventInRange(veh.Position, 250, "VehStream_SetVehicleIndicatorLights", veh.Handle, leftstate, rightstate);
            }
        }

        [Command("setvehdirt")]
        public static void setvehdirt(Player player, float dirt)
        {
            if (!Group.CanUseCmd(player, "setvehdirt")) return;
            if (player.Vehicle != null)
            {
                SetVehicleDirt(player.Vehicle, dirt);
            }
        }

        public static void SetVehicleDirt(Vehicle veh, float dirt)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData)) data = new VehicleSyncData();
            data.Dirt = dirt;
            UpdateVehicleSyncData(veh, data);

            if (veh.HasData("ACCESS") && veh.GetData<string>("ACCESS") == "PERSONAL")
            {
                if (!VehicleManager.Vehicles.ContainsKey(veh.NumberPlate)) return;

                VehicleManager.Vehicles[veh.NumberPlate].Dirt = dirt;
            }

            Trigger.PlayerEventInDimension(veh.Dimension, "VehStream_SetVehicleDirtLevel", veh.Handle, dirt);
        }

        public static float GetVehicleDirt(Vehicle veh)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return data.Dirt;
        }

        //Used internally only but publicly available in case any of you need it
        public static VehicleSyncData GetVehicleSyncData(Vehicle veh)
        {
            try
            {
                if (veh != null)
                {
                    if (NAPI.Entity.DoesEntityExist(veh))
                    {
                        if (VehiclesSyncDatas.ContainsKey(veh))
                            return VehiclesSyncDatas[veh];
                        else
                        {
                            VehiclesSyncDatas.Add(veh, new VehicleSyncData());
                            return VehiclesSyncDatas[veh];
                        }
                    }
                }
            }
            catch { };

            return default(VehicleSyncData); //null
        }

        //Used internally only but publicly available in case any of you need it
        public static bool UpdateVehicleSyncData(Vehicle veh, VehicleSyncData data)
        {
            try
            {
                if (veh != null)
                {
                    if (NAPI.Entity.DoesEntityExist(veh))
                    {
                        if (data != null)
                        {
                            if (VehiclesSyncDatas.ContainsKey(veh))
                                VehiclesSyncDatas[veh] = data;
                            else
                                VehiclesSyncDatas.Add(veh, data);

                            data.Position = veh.Position;
                            data.Rotation = veh.Rotation;

                            NAPI.Data.SetEntitySharedData(veh, "VehicleSyncData", JsonConvert.SerializeObject(data));
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception e) { Log.Write("UpdateVehicleSyncData: " + e.Message, nLog.Type.Error); return false; }
        }
    }
}
