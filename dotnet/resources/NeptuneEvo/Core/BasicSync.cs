using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Core
{
    class BasicSync : Script
    {
        private static nLog Log = new nLog("BasicSync");

        public static void AttachLabelToObject(string text, Vector3 posOffset, Entity obj)
        {
            var attachedLabel = new AttachedLabel(text, posOffset);
            switch (obj.Type)
            {
                case EntityType.Player:
                    var player = NAPI.Entity.GetEntityFromHandle<Player>(obj);
                    player.SetSharedData("attachedLabel", JsonConvert.SerializeObject(attachedLabel));
                    Trigger.PlayerEventInRange(player.Position, 550, "attachLabel", player);
                    break;
                case EntityType.Vehicle:
                    var vehicle = NAPI.Entity.GetEntityFromHandle<Vehicle>(obj);
                    vehicle.SetSharedData("attachedLabel", JsonConvert.SerializeObject(attachedLabel));
                    Trigger.PlayerEventInRange(vehicle.Position, 550, "attachLabel", vehicle);
                    break;
            }
        }

        public static void DetachLabel(Entity obj)
        {
            switch (obj.Type)
            {
                case EntityType.Player:
                    var player = NAPI.Entity.GetEntityFromHandle<Player>(obj);
                    player.ResetSharedData("attachedLabel");
                    Trigger.PlayerEventInRange(player.Position, 550, "detachLabel");
                    break;
                case EntityType.Vehicle:
                    var vehicle = NAPI.Entity.GetEntityFromHandle<Vehicle>(obj);
                    vehicle.ResetSharedData("attachedLabel");
                    Trigger.PlayerEventInRange(vehicle.Position, 550, "detachLabel");
                    break;
            }
        }

        public static void AttachObjectToPlayer(Player player, uint model, int bone, Vector3 posOffset, Vector3 rotOffset)
        {
            DetachObject(player);
            var attObj = new AttachedObject(model, bone, posOffset, rotOffset);
            player.SetSharedData("attachedObject", JsonConvert.SerializeObject(attObj));
            Trigger.PlayerEventInRange(player.Position, 550, "attachObject", player);
        }

        public static void DetachObject(Player player)
        {
            player.ResetSharedData("attachedObject");
            Trigger.PlayerEventInRange(player.Position, 550, "detachObject", player);
        }

        [RemoteEvent("invisible")]
        public static void SetInvisible(Player player, bool toggle)
        {
            try
            {
                if(Main.Players[player].AdminLVL == 0) return;
                player.SetSharedData("INVISIBLE", toggle);
                Trigger.PlayerEventInRange(player.Position, 550, "toggleInvisible", player, toggle);
            }
            catch (Exception e) { Log.Write("InvisibleEvent: " + e.Message, nLog.Type.Error); }
        }

        public static bool GetInvisible(Player player)
        {
            if (!player.HasSharedData("INVISIBLE") || !player.GetSharedData<bool>("INVISIBLE"))
                return false;
            else
                return true;
        }

        internal class PlayAnimData
        {
            public string Dict { get; set; }
            public string Name { get; set; }
            public int Flag { get; set; }

            public PlayAnimData(string dict, string name, int flag)
            {
                Dict = dict;
                Name = name;
                Flag = flag;
            }
        }

        internal class AttachedObject
        {
            public uint Model { get; set; }
            public int Bone { get; set; }
            public Vector3 PosOffset { get; set; }
            public Vector3 RotOffset { get; set; }

            public AttachedObject(uint model, int bone, Vector3 pos, Vector3 rot)
            {
                Model = model;
                Bone = bone;
                PosOffset = pos;
                RotOffset = rot;
            }
        }

        internal class AttachedLabel
        {
            public string Text { get; set; }
            public Vector3 PosOffset { get; set; }

            public AttachedLabel(string text, Vector3 pos)
            {
                Text = text;
                PosOffset = pos;
            }
        }
    }
}
