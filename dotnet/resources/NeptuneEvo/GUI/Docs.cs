using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using System;
using System.Collections.Generic;
using System.Text;
using NeptuneEVO.Fractions;

namespace NeptuneEVO.GUI
{
    class Docs : Script
    {
        private static nLog Log = new nLog("Docs");
        [RemoteEvent("passport")]
        public static void Event_Passport(Player player, params object[] arguments)
        {
            try
            {
                Player to = (Player)arguments[0];
                Log.Debug(to.Name.ToString());
                Passport(player, to);
            } catch(Exception e)
            {
                Log.Write("EXCEPTION AT \"EVENT_PASSPORT\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        [RemoteEvent("licenses")]
        public static void Event_Licenses(Player player, params object[] arguments)
        {
            try
            {
                Player to = (Player)arguments[0];
                Licenses(player, to);
            } catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"EVENT_LICENSES\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
		
		public static void Documents(Player from, Player to)
        {
            Vector3 pos = to.Position;
            if (from.Position.DistanceTo(pos) > 2)
            {
                Notify.Send(from, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин находится слишком далеко", 3000);
                return;
            }
            to.SetData("REQUEST", "acceptDocs");
            to.SetData("IS_REQUESTED", true);
           // Notify.Send(to, NotifyType.Warning, NotifyPosition.BottomCenter, $"Гражданин ({from.Value}) хочет показать удостоверение. Y/N - принять/отклонить", 3000);
            Main.OpenAnswer(to, $"Гражданин ({from.Value}) хочет показать удостоверение.");
            NAPI.Data.SetEntityData(to, "DOCFROM", from);
        }

        public static void Passport(Player from, Player to)
        {
            Vector3 pos = to.Position;
            if (from.Position.DistanceTo(pos) > 2)
            {
                Notify.Send(from, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин находится слишком далеко", 3000);
                return;
            }
            to.SetData("REQUEST", "acceptPass");
            to.SetData("IS_REQUESTED", true);
            //Notify.Send(to, NotifyType.Warning, NotifyPosition.BottomCenter, $"Гражданин ({from.Value}) хочет показать паспорт. Y/N - принять/отклонить", 3000);
            Main.OpenAnswer(to, $"Гражданин ({from.Value}) хочет показать паспорт.");
            NAPI.Data.SetEntityData(to, "DOCFROM", from);
        }
        public static void Plastic(Player from, Player to)
        {
            Vector3 pos = to.Position;
            if (from.Position.DistanceTo(pos) > 2)
            {
                Notify.Send(from, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин находится слишком далеко", 3000);
                return;
            }
            string num = "";
            foreach (string number in VehicleManager.getAllPlayerVehicles(from.Name))
                foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                    if (veh.NumberPlate == number && veh.Position.DistanceTo2D(from.Position) < 10f)
                    {
                        num = number;
                        break;
                    }
            if (num == "") {
                Notify.Send(from, NotifyType.Error, NotifyPosition.BottomCenter, "Рядом нет транспорта", 3000);
                return;
            }
            if (!VehicleManager.HavePlactic(num))
            {
                Notify.Send(from, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет документов от автомобиля", 3000);
                return;
            }
            to.SetData("REQUEST", "acceptPlastic");
            to.SetData("PLACTIC_NUMBER", num);
            to.SetData("IS_REQUESTED", true);
            //Notify.Send(to, NotifyType.Warning, NotifyPosition.BottomCenter, $"Гражданин ({from.Value}) хочет показать паспорт. Y/N - принять/отклонить", 3000);
            Main.OpenAnswer(to, $"Гражданин ({from.Value}) хочет показать вам пластик.");
            NAPI.Data.SetEntityData(to, "DOCFROM", from);
        }
        public static void Licenses(Player from, Player to)
        {
            Vector3 pos = to.Position;
            if (from.Position.DistanceTo(pos) > 2)
            {
                Notify.Send(from, NotifyType.Error, NotifyPosition.BottomCenter, "Гражданин находится слишком далеко", 3000);
                return;
            }
            to.SetData("REQUEST", "acceptLics");
            to.SetData("IS_REQUESTED", true);
            //Notify.Send(to, NotifyType.Warning, NotifyPosition.BottomCenter, $"Гражданин ({from.Value}) хочет показать лицензии. Y/N - принять/отклонить", 3000);
            Main.OpenAnswer(to, $"Гражданин ({from.Value}) хочет показать лицензии.");
            NAPI.Data.SetEntityData(to, "DOCFROM", from);
        }
        public static void AcceptPlactic(Player player)
        {
            try
            {
                Player from = NAPI.Data.GetEntityData(player, "DOCFROM");
                string number = player.GetData<string>("PLACTIC_NUMBER");
                VehicleManager.VehicleData vData = VehicleManager.Vehicles[number];
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({from.Value}) показал Вам пластик", 5000);
                Notify.Send(from, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы показали пластик Гражданину ({player.Value})", 5000);
                List<object> data = new List<object> { number, ParkManager.GetNormalName(vData.Model), vData.Holder.Replace('_', ' '), vData.Plastic.ToString("dd.MM.yyyy") };
                Trigger.PlayerEvent(player, "plastic", Newtonsoft.Json.JsonConvert.SerializeObject(data));
            }
            catch { }
        }

        public static void AcceptPasport(Player player)
        {
            Player from = NAPI.Data.GetEntityData(player, "DOCFROM");
            var acc = Main.Players[from];
            string gender = (acc.Gender) ? "Мужской" : "Женский";
            string fraction = (acc.FractionID > 0) ? Fractions.Manager.FractionNames[acc.FractionID] : "Нет";
            string work = "Безработный";
            int uid = acc.UUID;
            string firstname = acc.FirstName;
            string lastname = acc.LastName;
            if (acc.FAKEUUID != -1)
                uid = acc.FAKEUUID;
            if (acc.FAKEFIRST != null)
                firstname = acc.FAKEFIRST;
            if (acc.FAKELAST != null)
                lastname = acc.FAKELAST;
            List<object> data = new List<object>
                    {
                        uid,
                        firstname,
                        lastname,
                        acc.CreateDate.ToString("dd.MM.yyyy"),
                        gender,
                        fraction,
                        work
                    };
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({from.Value}) показал Вам паспорт", 5000);
            Notify.Send(from, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы показали паспорт Гражданину ({player.Value})", 5000);
            Log.Debug(json);
            Trigger.PlayerEvent(player, "passport", json);
            Trigger.PlayerEvent(player, "newPassport", from, uid);
        }
        public static void AcceptLicenses(Player player)
        {
            Player from = NAPI.Data.GetEntityData(player, "DOCFROM");
            var acc = Main.Players[from];
            string gender = (acc.Gender) ? "Мужской" : "Женский";
            
            var lic = "";
            for (int i = 0; i < acc.Licenses.Count; i++)
                if (acc.Licenses[i]) lic += $"{Main.LicWords[i]} / ";
            if (lic == "") lic = "Отсутствуют";

            string firstname = acc.FirstName;
            string lastname = acc.LastName;
            if (acc.FAKEFIRST != null)
                firstname = acc.FAKEFIRST;
            if (acc.FAKELAST != null)
                lastname = acc.FAKELAST;
            List<object> data = new List<object>
                    {
                        firstname,
                        lastname,
                        acc.CreateDate.ToString("dd.MM.yyyy"),
                        gender,
                        lic
                    };

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({from.Value}) показал Вам лицензии", 5000);
            Notify.Send(from, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы показали лицензии Гражданину ({player.Value})", 5000);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            Trigger.PlayerEvent(player, "licenses", json);
        }
		
		public static void AcceptDocs(Player player)
        {
            Player from = NAPI.Data.GetEntityData(player, "DOCFROM");
            var acc = Main.Players[from];
            string gender = (acc.Gender) ? "Мужской" : "Женский";
            string fraction = (acc.FractionID > 0) ? Configs.FractionRanks[acc.FractionID][acc.FractionLVL].Item1 : "Нет";
            string work = (acc.WorkID > 0) ? Jobs.WorkManager.JobStats[acc.WorkID] : "Безработный";
            int uid = acc.UUID;
            string firstname = acc.FirstName;
            string lastname = acc.LastName;
            if (acc.FAKEUUID != -1)
                uid = acc.FAKEUUID;
            if (acc.FAKEFIRST != null)
                firstname = acc.FAKEFIRST;
            if (acc.FAKELAST != null)
                lastname = acc.FAKELAST;
            List<object> data = new List<object>
                    {
                        uid,
                        firstname,
                        lastname,
                        acc.CreateDate.ToString("dd.MM.yyyy"),
                        gender,
                        fraction,
                        work
                    };
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({from.Value}) показал Вам удостоверение", 5000);
            Notify.Send(from, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы показали удостоверение Гражданину ({player.Value})", 5000);
            Log.Debug(json);
            switch (acc.FractionID)
            {
				case 6:
                    Trigger.PlayerEvent(player, "ydovgov", json);
                    break;
                case 7:
                    Trigger.PlayerEvent(player, "ydovpolice", json);
                    break;
				case 8:
                    Trigger.PlayerEvent(player, "ydovems", json);
                    break;
				case 9:
                    Trigger.PlayerEvent(player, "ydovfib", json);
                    break;
				case 14:
                    Trigger.PlayerEvent(player, "ydovarmy", json);
                    break;
				case 15:
                    Trigger.PlayerEvent(player, "ydovnews", json);
                    break;	
				case 17:
                    Trigger.PlayerEvent(player, "ydovmws", json);
                    break;	
				case 18:
                    Trigger.PlayerEvent(player, "ydovgr6", json);
                    break;	
                default:
                    return;
            }

            Trigger.PlayerEvent(player, "newPassport", from, uid);
        }

    }
}
