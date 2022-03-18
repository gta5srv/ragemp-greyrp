using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;

namespace NeptuneEVO.Utils
{
    class NewYear : Script
    {

        private static bool Active = false; // Включить ли режим нового года

        private static nLog Log = new nLog("New Year");

        [ServerEvent(Event.ResourceStart)]

        public void onResourceStart()
        {
            try
            {
                if (!Active) return;

                ColShape shape = NAPI.ColShape.CreateSphereColShape(new Vector3(191.9436, -930.8194, 30.7338), 5.5f);
                ColShape shape2 = NAPI.ColShape.CreateSphereColShape(new Vector3(204.1075, -939.4277, 30.6458), 5.5f);
                shape.OnEntityEnterColShape += OnEnter;
                shape2.OnEntityEnterColShape += OnEnter;
                shape.OnEntityExitColShape += OnExit;
                shape2.OnEntityExitColShape += OnExit;

                /*ColShape elka = NAPI.ColShape.CreateSphereColShape(new Vector3(225.6532, -891.8182, 29.57199), 3f);
                elka.OnEntityEnterColShape += (s, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 159);
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"NEWYEAR\":\n" + e.ToString(), nLog.Type.Error); }
                };
                elka.OnEntityExitColShape += (s, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"NEWYEAR\":\n" + e.ToString(), nLog.Type.Error); }
                };*/

                Log.Write("Actived!", nLog.Type.Success);
            } 
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"NEWYEAR\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private static void OnEnter(ColShape shape, Player player)
        {
            try {
                player.SetData("INTERACTIONCHECK", 158);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"NEWYEAR\":\n" + e.ToString(), nLog.Type.Error); }
        }

        private static void OnExit(ColShape shape, Player player)
        {
            try
            {
                player.SetData("INTERACTIONCHECK", 0);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"NEWYEAR\":\n" + e.ToString(), nLog.Type.Error); }
        }


    }
}
