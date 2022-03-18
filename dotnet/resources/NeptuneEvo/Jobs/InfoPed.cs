using GTANetworkAPI;
using System.Collections.Generic;
using System;
using NeptuneEVO.GUI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Jobs
{
    class InfoPed : Script
    {


        private static nLog Log = new nLog("InfoPed");

        public static Vector3 NPCPoint1 = new Vector3(-1042.4424, -2738.2603, 20.1);

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            ColShape shape = NAPI.ColShape.CreateCylinderColShape(NPCPoint1, 3f, 10f);
            NAPI.Blip.CreateBlip(606, NPCPoint1, 1, 68, "Информатор", 50, 0, true);
            NAPI.TextLabel.CreateTextLabel("~w~Информатор", NPCPoint1 + new Vector3(0, 0, 1.2f), 20F, 0.5F, 0, new Color(255, 255, 255), true, 0);
            shape.OnEntityEnterColShape += (s, entity) =>
            {
                try 
                {
                    Trigger.PlayerEvent(entity, "JobsEinfo");
                    entity.SetData("INTERACTIONCHECK", 571);
                }
                catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
            };
            shape.OnEntityExitColShape += (s, entity) =>
            {
                try
                {
                    Trigger.PlayerEvent(entity, "JobsEinfo2");
                    entity.SetData("INTERACTIONCHECK", 0);
                }
                catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
            };
        }
        public static void Interact1(Player сlient)
        {
            Trigger.PlayerEvent(сlient, "openInfoMenu");
        }
    }
}

