using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeptuneEVO.Core
{
    class Fingerpointing : Script
    {
        [RemoteEvent("fpsync.update")]
        public void FingerSyncUpdate(Player Player, float camPitch, float camHeading)
        {
            NAPI.ClientEvent.TriggerClientEventInRange(Player.Position, 100f, "fpsync.update", Player.Value, camPitch, camHeading);
        }
        [RemoteEvent("pointingStop")]
        public void FingerStop(Player Player)
        {
            NAPI.Player.StopPlayerAnimation(Player);
        }
    }
}
