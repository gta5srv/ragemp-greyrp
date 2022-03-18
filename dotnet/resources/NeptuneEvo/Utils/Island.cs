using System;
using System.Collections.Generic;
using System.Xml.Schema;
using GTANetworkAPI;
using Newtonsoft.Json;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Core
{
    class Island : Script
    {

        static nLog Log = new nLog("Island");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {

                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("h4_prop_h4_gate_r_03a") , new Vector3(4990.681, -5715.106, 21.78103), 100f);
                NAPI.World.DeleteWorldProp(NAPI.Util.GetHashKey("h4_prop_h4_gate_l_03a"), new Vector3(4987.587, -5718.635, 21.78103), 100f);
                //NAPI.Object.CreateObject(2059017853, new Vector3(4448.839, -4477.361, 6.429066), new Vector3(0,0, 0));

                NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_doghouse_01"), new Vector3(4975.62, -5711.7705, 18.93), new Vector3(0, 0, 190));

                Vector3 pos = new Vector3(136.02351, -3332.7708, 4.901928);

                NAPI.Blip.CreateBlip(765, pos, 0.7f, Convert.ToByte(49), Main.StringToU16("Переправа"), 255, 0, true);

                ColShape shape = NAPI.ColShape.CreateSphereColShape(pos, 5f);
                NAPI.TextLabel.CreateTextLabel("~b~Переправа", new Vector3(pos.X, pos.Y, pos.Z + 1f), 5F, 0.5F, 0, new Color(255, 255, 255), true, 0);
                NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 2f), new Vector3(), new Vector3(), 3.965f, new Color(0, 175, 250, 220), false, 0);
                NAPI.Marker.CreateMarker(27, pos + new Vector3(0, 0, 0.14f), new Vector3(), new Vector3(), 4f, new Color(0, 175, 250, 220), false, 0);
                shape.OnEntityEnterColShape += (s, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 523);
                    }
                    catch (Exception e) { Log.Write("ISLAND SHAPE" + e.ToString(), nLog.Type.Error); }
                };
                shape.OnEntityExitColShape += (s, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Log.Write("ISLAND SHAPE" + e.ToString(), nLog.Type.Error); }
                };

                pos = new Vector3(4062.6917, -4682.998, 3.0640807);

                NAPI.Blip.CreateBlip(765, pos, 0.7f, Convert.ToByte(49), Main.StringToU16("Переправа"), 255, 0, true);

                ColShape shape2 = NAPI.ColShape.CreateSphereColShape(pos, 5f);
                NAPI.TextLabel.CreateTextLabel("~b~Переправа", new Vector3(pos.X, pos.Y, pos.Z + 1f), 5F, 0.5F, 0, new Color(255, 255, 255), true, 0);
                NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 2f), new Vector3(), new Vector3(), 3.965f, new Color(0, 175, 250, 220), false, 0);
                NAPI.Marker.CreateMarker(27, pos + new Vector3(0, 0, 0.14f), new Vector3(), new Vector3(), 4f, new Color(0, 175, 250, 220), false, 0);
                shape2.OnEntityEnterColShape += (s, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 524);
                    }
                    catch (Exception e) { Log.Write("ISLAND SHAPE" + e.ToString(), nLog.Type.Error); }
                };
                shape2.OnEntityExitColShape += (s, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Log.Write("ISLAND SHAPE" + e.ToString(), nLog.Type.Error); }
                };

            }
            catch (Exception e) { Log.Write("Island on start: " + e.ToString(), nLog.Type.Error); }
        }

    }
}
