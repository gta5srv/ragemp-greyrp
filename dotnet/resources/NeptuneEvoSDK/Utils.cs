using GTANetworkAPI;
using System.Collections.Generic;

namespace NeptuneEVO.SDK
{

    public class Utils
    {

        public class QuestDATABASE
        {
            public int Week { get; set; }

            public List<List<List<int>>> QuestList { get; set; }

            public QuestDATABASE(int week, List<List<List<int>>> quests)
            {
                Week = week; QuestList = quests;
            }

        }

        public static AccountData GetAccount(Player client)
        {
            //client.GetExternalData<AccountData>(0);
            //return new AccountData();
            return client.GetData<AccountData>("AccData");
        }
        public static CharacterData GetCharacter(Player client)
        {
            //client.GetExternalData<CharacterData>(1);
            //return new CharacterData();
            return client.GetData<CharacterData>("CharData");
        }
    }
}
