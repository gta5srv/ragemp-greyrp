using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NeptuneEVO.GUI;
using NeptuneEVO.MoneySystem;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Core 
{
    class Phone : Script
    {
        class MySQL_Load
        {
            public int number; 
            public object address_book;
            public object call_log_phone;
            public object sms_chat_list;
        }
        public class MySQL_Update
        {
            public object book { get; set; }
            public object log { get; set; }
            public object sms { get; set; }
        }
        public class PhoneSendSMS
        {
            public string date { get; set; }
            public int news { get; set; }
            public int type { get; set; }
            public string message { get; set; }
        }
        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Player player)
        {
            NAPI.Data.SetEntitySharedData(player, "ActivePhone", false);
            //Main.Players[player].ActivePhone = false;
        }
        [RemoteEvent("playerClickedButton")]
        public void PlayerClickedButton(Player player, string key)
        {
            //if (key == "UP_ARROW_key" && !Main.Players[player].ActivePhone)
            if (key == "UP_ARROW_key")
            {
                if (player.GetSharedData<bool>("ActivePhone") == false)
                {
                    if (player.HasData("AntiAnimDown"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно достать мобильный телефон", 3000);
                        return;
                    }
                    var number_sim = Main.Players[player].Sim;
                    var result = MySQL.QueryRead($"SELECT * FROM `phones_number` WHERE `number` LIKE {number_sim}");
                    if (result == null || result.Rows.Count == 0) return;
                    foreach (DataRow Row in result.Rows)
                    {
                        MySQL_Load obj = new MySQL_Load();
                        obj.number = Convert.ToInt32(Row["number"]);
                        obj.address_book = JsonConvert.DeserializeObject(Convert.ToString(Row["address_book"]));
                        obj.call_log_phone = JsonConvert.DeserializeObject(Convert.ToString(Row["call_log_phone"]));
                        obj.sms_chat_list = JsonConvert.DeserializeObject(Convert.ToString(Row["sms_chat_list"]));
                        Trigger.PlayerEvent(player, "server_to_Player_OpenPhone", JsonConvert.SerializeObject(obj));
                        NAPI.Data.SetEntitySharedData(player, "ActivePhone", true);
                        //Main.Players[player].ActivePhone = true;
                    }
                }
                else
                {
                    Voice.VoicePhoneMetaData playerPhoneMeta = player.GetData<Voice.VoicePhoneMetaData>("PhoneVoip");
                    if (playerPhoneMeta.Target != null) return;

                    NAPI.Data.SetEntitySharedData(player, "ActivePhone", false);
                    Trigger.PlayerEvent(player, "server_to_Player_ClosedPhone");
                }
            }
        }
        [RemoteEvent("Player_to_server_PhoneUpdate")]
        public void Player_to_server_PhoneUpdate(Player player, params object[] args)
        {
            var number_sim = Main.Players[player].Sim;
            string param = args[0].ToString();
            MySQL_Update tmp = JsonConvert.DeserializeObject<MySQL_Update>(param);

            var address_bock = JsonConvert.SerializeObject(tmp.book);
            var call_log_phone = JsonConvert.SerializeObject(tmp.log);
            var sms_chat_list = JsonConvert.SerializeObject(tmp.sms);
            MySQL.QueryRead($"UPDATE `phones_number` SET address_book = '{address_bock}', call_log_phone = '{call_log_phone}', sms_chat_list = '{sms_chat_list}' WHERE number = '{number_sim}'");
        }
        [RemoteEvent("Player_to_server_PhoneSendSMS")]
        public void Player_to_server_PhoneSendSMS(Player player, params object[] args)
        {
            bool proverka = true;
            var player_number_sim = Main.Players[player].Sim;
            var target_number_sim = Convert.ToInt32(args[0]);

            string param = args[1].ToString();
            PhoneSendSMS tmp = JsonConvert.DeserializeObject<PhoneSendSMS>(param);
            foreach (var target in NAPI.Pools.GetAllPlayers())
            {
                if (Main.Players[target].Sim == target_number_sim)
                {
                    //if (Main.Players[target].ActivePhone)
                    if (player.GetSharedData<bool>("ActivePhone") == true)
                    {
                        Trigger.PlayerEvent(target, "server_to_Player_PhoneNewSMS", player_number_sim, JsonConvert.SerializeObject(tmp));
                        proverka = false;
                    }
                    else
                    {
                        /*===[УВЕДОМЛЕНИЯ ИГРОКУ]===*/
                    }
                }
            }
            if(proverka)
            {
                var result = MySQL.QueryRead($"SELECT * FROM `phones_number` WHERE `number` LIKE {target_number_sim}");
                if (result == null || result.Rows.Count == 0) return;
                foreach (DataRow Row in result.Rows)
                {
                    Trigger.PlayerEvent(player, "server_to_Player_Treatment", player_number_sim, target_number_sim, args[1].ToString(), Convert.ToString(Row["sms_chat_list"]));
                }
            }
        }
        [RemoteEvent("Player_to_server_Treatment")]
        public void Player_to_server_Treatment(Player player, params object[] args)
        {
            var target_number_sim = Convert.ToInt32(args[0]);
            MySQL.QueryRead($"UPDATE `phones_number` SET sms_chat_list = '{Convert.ToString(args[1])}' WHERE number = '{target_number_sim}'");
        }
        [RemoteEvent("Player_to_server_OutgoingCall")]
        public void Player_to_server_OutgoingCall(Player player, params object[] args)
        {
            var player_number_sim = Main.Players[player].Sim;
            var target_number_sim = Convert.ToInt32(args[0]);

            if (!Main.SimCards.ContainsKey(target_number_sim))
            {
                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Игрока с таким номером не найдено", 3000);
                return;
            }
            Player target = Main.GetPlayerByUUID(Main.SimCards[target_number_sim]);
            Voice.Voice.PhoneCallCommand(player, target);
        }
        public static void OpenPhoneIsIncoming(Player player)
        {
            Voice.VoicePhoneMetaData playerPhoneMeta = player.GetData<Voice.VoicePhoneMetaData>("PhoneVoip");
            var player_number_sim = Main.Players[player].Sim;
            var target_number_sim = Main.Players[playerPhoneMeta.Target].Sim;
            if (player.GetSharedData<bool>("ActivePhone") == false)
            {
                var result = MySQL.QueryRead($"SELECT * FROM `phones_number` WHERE `number` LIKE {player_number_sim}");
                if (result == null || result.Rows.Count == 0) return;
                foreach (DataRow Row in result.Rows)
                {
                    MySQL_Load obj = new MySQL_Load();
                    obj.number = Convert.ToInt32(Row["number"]);
                    obj.address_book = JsonConvert.DeserializeObject(Convert.ToString(Row["address_book"]));
                    obj.call_log_phone = JsonConvert.DeserializeObject(Convert.ToString(Row["call_log_phone"]));
                    obj.sms_chat_list = JsonConvert.DeserializeObject(Convert.ToString(Row["sms_chat_list"]));
                    Trigger.PlayerEvent(player, "server_to_Player_OpenPhoneIncoming", true, target_number_sim, JsonConvert.SerializeObject(obj));
                    NAPI.Data.SetEntitySharedData(player, "ActivePhone", true);
                    NAPI.Data.SetEntitySharedData(player, "ActivePhoneData", false);
                }
            }
            else
            {
                Trigger.PlayerEvent(player, "server_to_Player_OpenPhoneIncoming", false, target_number_sim, "None");
                NAPI.Data.SetEntitySharedData(player, "ActivePhoneData", true);
            }
        }
        [RemoteEvent("Player_to_server_CallState")]
        public void Player_to_server_CallState(Player player, params object[] args)
        {
            if(Convert.ToString(args[0]) == "accepted")
            {
                Voice.Voice.PhoneCallAcceptCommand(player);
            }
            else if (Convert.ToString(args[0]) == "rejected")
            {
                Voice.Voice.PhoneHCommand(player);
            }
            else if (Convert.ToString(args[0]) == "endcalling")
            {
                Voice.Voice.PhoneHCommand(player);
            }
        }
        public static void ClosedPhoneIsEndCall(Player player)
        {
            if (player.GetSharedData<bool>("ActivePhone") == false)
            {
                NAPI.Data.SetEntitySharedData(player, "ActivePhone", false);
                Trigger.PlayerEvent(player, "server_to_Player_ClosedPhone");
            }
            else Trigger.PlayerEvent(player, "server_to_Player_ClosedPhoneIsEndCall");
        }
    }
}