using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Core
{
    class SafeZones : Script
    {
        private static nLog Log = new nLog("SafeZones");
        public static void CreateSafeZone(Vector3 position, int height, int width, uint dim=0)
        {
            var colShape = NAPI.ColShape.Create2DColShape(position.X, position.Y, height, width, dim);
            colShape.OnEntityEnterColShape += (shape, player) =>
            {
                try
                {
                    Trigger.PlayerEvent(player, "safeZone", true);
                }
                catch (Exception e) { Log.Write($"SafeZoneEnter: {e.Message}", nLog.Type.Error); }
                
            };
            colShape.OnEntityExitColShape += (shape, player) =>
            {
                try
                {
                    Trigger.PlayerEvent(player, "safeZone", false);
                }
                catch (Exception e) { Log.Write($"SafeZoneExit: {e.Message}", nLog.Type.Error); }
            };
        }

        [ServerEvent(Event.ResourceStart)]
        public void Event_onResourceStart()
        {
            CreateSafeZone(new Vector3(-538.7153, -214.66, 36.52974), 200, 200); // мерия
			CreateSafeZone(new Vector3(-804.4073, -224.9438, 36.10337), 20, 20); // салон
			CreateSafeZone(new Vector3(224.5672, -610.4773, 8.761386), 300, 160); // емс
			CreateSafeZone(new Vector3(906.8802, -2919.172, 7.482737), 70, 70); // контейнеры

            CreateSafeZone(new Vector3(871.4479, 16.622711, 74.95795), 70, 70); // Казино
            CreateSafeZone(new Vector3(1068.882, 175.8862, -61.93705),200, 200, 1); // Внутри казино
        }
    }
}
