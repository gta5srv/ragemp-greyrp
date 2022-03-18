using GTANetworkAPI;
using System;
using System.Collections.Generic;

namespace NeptuneEVO.SDK
{
    public class CharacterData
    {
        public int UUID { get; set; } = -1;
        public Vector3 SpawnPos { get; set; } = new Vector3(0, 0, 0);
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime BirthDate { get; set; } = DateTime.Now;
        public string FirstName { get; set; } = null;
        public string LastName { get; set; } = null;
        public bool Gender { get; set; } = true;
        public int Health { get; set; } = 100;
        public int Armor { get; set; } = 0;
        public int LVL { get; set; } = 0;
        public int EXP { get; set; } = 0;
        public long Money { get; set; } = 500;
        public int Bank { get; set; } = 0;
        public int WorkID { get; set; } = 0;
        public int FractionID { get; set; } = 0;
        public int FractionLVL { get; set; } = 0;
		public string FamilyCID { get; set; } = "null";
		public int FamilyRank { get; set; } = 0;
        public int ArrestTime { get; set; } = 0;
        public int DemorganTime { get; set; } = 0;
        public WantedLevel WantedLVL { get; set; } = null;
        public List<int> BizIDs { get; set; } = new List<int>();
        public int AdminLVL { get; set; } = 0;
        public List<bool> Licenses { get; set; } = new List<bool>();
        public DateTime Unwarn { get; set; } = DateTime.Now;
        public int Unmute { get; set; } = 0;
        public int Warns { get; set; } = 0;
        public string LastVeh { get; set; } = null;
        public bool OnDuty { get; set; } = false;
        public int LastHourMin { get; set; } = 0;
        public int HotelID { get; set; } = -1;
        public int HotelLeft { get; set; } = 0;
        public int Sim { get; set; } = -1;
        public string PetName { get; set; } = "null";
        public Dictionary<int, string> Contacts = new Dictionary<int, string>();
        public List<bool> Achievements = new List<bool>();
		public int Eat { get; set; } = 75;
        public int Water { get; set; } = 75;
		public string Married { get; set; } = "";
        public int VUnmute { get; set; } = 0;

        public int WBus { get; set; } = 0;
        public int WLawnmower { get; set; } = 0;
        public int WCollector { get; set; } = 0;
        public int WTrucker { get; set; } = 0;
        public int WTraktorist { get; set; } = 0;
        public int WGopostal { get; set; } = 0;
        public int WElectric { get; set; } = 0;
        public int WTrashCar { get; set; } = 0;
        public int WConstructor { get; set; } = 0;
		public int WMiner { get; set; } = 0;
		public int WDiver { get; set; } = 0;
        public int WSnow { get; set; } = 0;
		public int product { get; set; } = 0;

        public int TimeMinutes { get; set; } = 0;
        public int SessionTime { get; set; } = 0;

        public string Org { get; set; } = "";
        public int OrgLic { get; set; } = -1;

        public DateTime Cooldown { get; set; }
        public Utils.QuestDATABASE Quests { get; set; }

        public int LuckyWheell { get; set; }

        public int FAKEUUID { get; set; } = -1;
        public string FAKEFIRST { get; set; } = null;
        public string FAKELAST { get; set; } = null;

        public List<int> States = new List<int> { 0, 0 };

        public bool VoiceMuted = false;
		

        // temperory data
        public int InsideHouseID = -1;
        public int InsideGarageID = -1;
        public Vector3 ExteriorPos = new Vector3();
        public int InsideHotelID = -1;
        public int TuningShop = -1;
        public bool IsAlive = false;
        public bool IsSpawned = false;
    }

    public class WantedLevel
    {
        public int Level { get; set; }
        public string WhoGive { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; }

        public WantedLevel(int level, string whoGive, DateTime date, string reason)
        {
            Level = level;
            WhoGive = whoGive;
            Date = date;
            Reason = reason;
        }
    }
}
