using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GTANetworkAPI;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NeptuneEVO.Houses;
using NeptuneEVO.GUI;
using MySql.Data.MySqlClient;
using NeptuneEVO.SDK;
using NeptuneEVO.Businesses;
using NeptuneEVO.Organization;
using NeptuneEVO.Utils;

namespace NeptuneEVO.Core.Character
{
    public class Character : CharacterData
    {
        private static nLog Log = new nLog("Character");
        private static Random Rnd = new Random();
        
        public void Spawn(Player player)
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    try
                    {
                        player.SetSharedData("IS_MASK", false);

                        // Logged in state, money, phone init
                        Trigger.PlayerEvent(player, "loggedIn");
                        player.SetData("LOGGED_IN", true);
                        NAPI.Task.Run(() => { try{ Trigger.PlayerEvent(player, "UpdateMoney", Money); } catch { } }, 500);
                        NAPI.Task.Run(() => { try { Trigger.PlayerEvent(player, "UpdateBank", MoneySystem.Bank.Accounts[Bank].Balance); } catch { } }, 1000);
                        NAPI.Task.Run(() => { try { Trigger.PlayerEvent(player, "UpdateEat", Main.Players[player].Eat); } catch { } }, 1500);
                        NAPI.Task.Run(() => { try { Trigger.PlayerEvent(player, "UpdateWater", Main.Players[player].Water); } catch { } }, 2000);
                        NAPI.Task.Run(() => { try { Trigger.PlayerEvent(player, "initPhone"); } catch { } }, 2500);
                        
						
                        
                        
                        Jobs.WorkManager.load(player);

                        // Skin, Health, Armor, RemoteID
                        player.SetSkin((Gender) ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);
                        player.Health = (Health > 5) ? Health : 5;
                        player.Armor = Armor;

                        player.ResetSharedData("attachToVehicleTrunk");

                        player.SetSharedData("REMOTE_ID", player.Value);
                        

                        Voice.Voice.PlayerJoin(player);

                        player.SetSharedData("voipmode", -1);

                        if (Fractions.Manager.FractionTypes[FractionID] == 1 || AdminLVL > 0) Fractions.GangsCapture.LoadBlips(player);
                        if (WantedLVL != null) Trigger.PlayerEvent(player, "setWanted", WantedLVL.Level);
                        
                        player.SetData("RESIST_STAGE", 0);
                        player.SetData("RESIST_TIME", 0);
						player.SetSharedData("ALVL", AdminLVL);
                        if (AdminLVL > 0) player.SetSharedData("IS_ADMIN", true);

                        Dashboard.sendStats(player);
                        Dashboard.sendItems(player);
                        if(Main.Players[player].LVL == 0) {
                            NAPI.Task.Run(() => { try { Trigger.PlayerEvent(player, "disabledmg", true); } catch { } }, 5000);
                        }

                        House house = HouseManager.GetHouse(player);
                        if (house != null)
                        {
                            // House blips & checkpoints
                            house.PetName = Main.Players[player].PetName;

                            Trigger.PlayerEvent(player, "changeBlipColor", house.blip, 73);

                            Trigger.PlayerEvent(player, "createCheckpoint", 333, 27, GarageManager.Garages[house.GarageID].Position - new Vector3(0, 0, 0.2f), 1, NAPI.GlobalDimension, 0, 86, 214);
                            //Trigger.PlayerEvent(player, "createGarageBlip", GarageManager.Garages[house.GarageID].Position);
							Trigger.PlayerEvent(player, "createHouseBlip", house.Position);
                        }

                        House apartment = HouseManager.GetApart(player);
                        if (apartment != null)
                        {
                            // House blips & checkpoints
                            apartment.PetName = Main.Players[player].PetName;

                            Trigger.PlayerEvent(player, "createCheckpoint", 334, 27, GarageManager.Garages[apartment.GarageID].Position - new Vector3(0, 0, 0.2f), 1, NAPI.GlobalDimension, 0, 86, 214);
                            Trigger.PlayerEvent(player, "createGarageBlip", GarageManager.Garages[apartment.GarageID].Position);
                           // Trigger.PlayerEvent(player, "createHouseBlip", apartment.Position);
                        }

                        if (!Customization.CustomPlayerData.ContainsKey(UUID) || !Customization.CustomPlayerData[UUID].IsCreated)
                        {
                            Trigger.PlayerEvent(player, "spawnShow", false);
                            Customization.CreateCharacter(player);
                        }
                        else
                        {
                            try
                            {
                                NAPI.Entity.SetEntityPosition(player, Main.Players[player].SpawnPos);
                                List<bool> prepData = new List<bool>
                                {
                                    true,
                                    (FractionID > 0) ? true : false,
                                    (house != null || HotelID != -1 || apartment != null) ? true : false,
                                    (Main.Players[player].FamilyCID != null && Main.Players[player].FamilyCID != "null") ? true : false
                                };
                                Trigger.PlayerEvent(player, "spawnShow", JsonConvert.SerializeObject(prepData));
                                Customization.ApplyCharacter(player);
                            }
                            catch { }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Write($"EXCEPTION AT \"Spawn.NAPI.Task.Run\":\n" + e.ToString(), nLog.Type.Error);
                    }
                });

                if (Warns > 0 && DateTime.Now > Unwarn)
                {
                    Warns--;

                    if (Warns > 0)
                        Unwarn = DateTime.Now.AddDays(14);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Одно предупреждение было снято. У Вас осталось {Warns}", 3000);
                }

                if (!Dashboard.isopen.ContainsKey(player))
                    Dashboard.isopen.Add(player, false);

                nInventory.Check(UUID);
                if (nInventory.Find(UUID, ItemType.BagWithMoney) != null)
                    nInventory.Remove(player, ItemType.BagWithMoney, 1);
                if (nInventory.Find(UUID, ItemType.BagWithDrill) != null)
                    nInventory.Remove(player, ItemType.BagWithDrill, 1);

                if(FractionID == 15) {
                    Trigger.PlayerEvent(player, "enableadvert", true);
                    Fractions.LSNews.onLSNPlayerLoad(player);
                }
                if(AdminLVL > 0)
                {
                    ReportSys.onAdminLoad(player);
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"Spawn\":\n" + e.ToString());
            }
        }

        public async Task Load(Player player, int uuid)
        {
            try
            {
                if (Main.Players.ContainsKey(player))
                    Main.Players.Remove(player);

                DataTable result = await MySQL.QueryReadAsync($"SELECT * FROM `characters` WHERE uuid={uuid}");
                if (result == null || result.Rows.Count == 0) return;

                foreach (DataRow Row in result.Rows)
                {
                    UUID = Convert.ToInt32(Row["uuid"]);
                    FirstName = Convert.ToString(Row["firstname"]);
                    LastName = Convert.ToString(Row["lastname"]);
                    Gender = Convert.ToBoolean(Row["gender"]);
                    Health = Convert.ToInt32(Row["health"]);
                    Armor = Convert.ToInt32(Row["armor"]);
                    LVL = Convert.ToInt32(Row["lvl"]);
                    EXP = Convert.ToInt32(Row["exp"]);
                    Money = Convert.ToInt64(Row["money"]);
                    Bank = Convert.ToInt32(Row["bank"]);
                    WorkID = Convert.ToInt32(Row["work"]);
                    FractionID = Convert.ToInt32(Row["fraction"]);
                    FractionLVL = Convert.ToInt32(Row["fractionlvl"]);
					FamilyCID = Convert.ToString(Row["familycid"]); //выставить в базе данных стандартное значение текстом null
					FamilyRank = Convert.ToInt32(Row["familyrank"]); //выставить в базе данных стандартное значение 0
                    ArrestTime = Convert.ToInt32(Row["arrest"]);
                    DemorganTime = Convert.ToInt32(Row["demorgan"]);
                    WantedLVL = JsonConvert.DeserializeObject<WantedLevel>(Row["wanted"].ToString());
                    BizIDs = JsonConvert.DeserializeObject<List<int>>(Row["biz"].ToString());
                    AdminLVL = Convert.ToInt32(Row["adminlvl"]);
					product = Convert.ToInt32(Row["product"]);
                    Licenses = JsonConvert.DeserializeObject<List<bool>>(Row["licenses"].ToString());
                    Unwarn = ((DateTime)Row["unwarn"]);
                    Unmute = Convert.ToInt32(Row["unmute"]);
                    Warns = Convert.ToInt32(Row["warns"]);
                    LastVeh = Convert.ToString(Row["lastveh"]);
                    OnDuty = Convert.ToBoolean(Row["onduty"]);
                    LastHourMin = Convert.ToInt32(Row["lasthour"]);
                    HotelID = Convert.ToInt32(Row["hotel"]);
                    HotelLeft = Convert.ToInt32(Row["hotelleft"]);
                    Contacts = JsonConvert.DeserializeObject<Dictionary<int, string>>(Row["contacts"].ToString());
                    Achievements = JsonConvert.DeserializeObject<List<bool>>(Row["achiev"].ToString());
					Eat = Convert.ToInt32(Row["eat"]);
                    Water = Convert.ToInt32(Row["water"]);
					Married = Convert.ToString(Row["married"]);
                    WBus = Convert.ToInt32(Row["bus"]);
                    WLawnmower = Convert.ToInt32(Row["lawnmower"]);
                    WCollector = Convert.ToInt32(Row["collector"]);
                    WTrucker = Convert.ToInt32(Row["trucker"]);
                    WTraktorist = Convert.ToInt32(Row["traktorist"]);
                    WGopostal = Convert.ToInt32(Row["gopostal"]);
                    WElectric = Convert.ToInt32(Row["electric"]);
                    WTrashCar = Convert.ToInt32(Row["trashcar"]);
                    WConstructor = Convert.ToInt32(Row["constructor"]);
                    WMiner = Convert.ToInt32(Row["miner"]);
					WDiver = Convert.ToInt32(Row["diver"]);
                    WSnow = Convert.ToInt32(Row["snow"]);
                    NAPI.Task.Run(() => { try { player.SetSharedData("UID", Convert.ToInt32(Row["idkey"])); } catch { } });

                    TimeMinutes = Convert.ToInt32(Row["time"]);

                    VUnmute = Convert.ToInt32(Row["vunmute"]);

                    OrgLic = Convert.ToInt32(Row["orglic"]);
                    Org = Row["org"].ToString();

                    LuckyWheell = Convert.ToInt32(Row["luckywheell"]);

                    States = JsonConvert.DeserializeObject<List<int>>(Row["states"].ToString());

                    FAKEUUID = -1;
                    if(Achievements == null) {
                        Achievements = new List<bool>();
                        for(uint i = 0; i != 401; i++) Achievements.Add(false);
                    }
                    Sim = Convert.ToInt32(Row["sim"]);
                    PetName =  Convert.ToString(Row["PetName"]);
                    CreateDate = ((DateTime)Row["createdate"]);

                    // TINKOFF BANK GOOD <3                                                                                                                                                                                                                                                 [very shit]

                    if (string.IsNullOrEmpty(Row["quests"].ToString()))
                    {
                        Quests = QuestsManager.OptimizePlayer(player);
                    }
                    else
                    {
                        Quests = JsonConvert.DeserializeObject<NeptuneEVO.SDK.Utils.QuestDATABASE>(Row["quests"].ToString());
                        if (Quests.Week != QuestsManager.Week)
                        {
                            Quests = QuestsManager.OptimizePlayer(player);
                        }
                    }
                    



                    /* if (!string.IsNullOrEmpty(Row["cooldown"].ToString()))
                         Cooldown = ((DateTime)Row["cooldown"]);
                     else
                     {
                         Cooldown = DateTime.Now;
                         Cooldown.AddHours( -25 );
                     } */

                    SpawnPos = JsonConvert.DeserializeObject<Vector3>(Row["pos"].ToString());
                    if (Row["pos"].ToString().Contains("NaN") || SpawnPos == new Vector3())
                    {
                        Log.Debug("Detected wrong coordinates!", nLog.Type.Warn);
                        if(LVL <= 1) SpawnPos = new Vector3(-1037.928, -2738.124, 19.04927); // На спавне новичков
                        else SpawnPos = new Vector3(-1037.928, -2738.124, 19.04927); // У мэрии
                    }
                    QuestsManager.OptimizeData(player, Quests);
                }
                //CreateBlipsForPlayer(player, BizIDs, Org, UUID);
                NAPI.Task.Run(() => { player.Name = FirstName + "_" + LastName; GameLog.Connected(player.Name, UUID, player.GetData<string>("RealSocialClub"), player.GetData<string>("RealHWID"), player.Value, player.Address);  });
                
                Main.Players.Add(player, this);
                CheckAchievements(player);
                
                Spawn(player);
				
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"Load\":\n" + e.ToString());
            }
        }
		
		public static void CreateBlipsForPlayer(Player player, List<int> BizIDs, string Org, int UUID)
		{
			try
			{
				NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (player == null) return;
                            if (BizIDs.Count > 0)
                            {
                                foreach (int id in BizIDs)
                                {
                                    if (!BCore.BizList.ContainsKey(id)) return;
                                    if (BCore.BizList[id].GetPos() == null) return;
                                    var Blip = NAPI.Blip.CreateBlip(BCore.BizList[id].GetBlip(), BCore.BizList[id].GetPos(), 1, 3, $"Ваш бизнес ID: {id}", 0, 0, true, 0, 0);
                                    Blip.Transparency = 0;
                                    player.SetData("BIZBLIP", Blip);
                                    foreach (var p in NAPI.Pools.GetAllPlayers())
                                    {
                                        if (!Main.Players.ContainsKey(p)) continue;
                                        if (Main.Players[p].UUID != UUID) continue;
                                        Trigger.PlayerEvent(p, "changeBlipAlpha", Blip, 255);
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(Org))
                            {
                                MaterialsI.MaterialsOrg org = (MaterialsI.MaterialsOrg)OCore.OrgListNAME[Org];
                                var Blip = NAPI.Blip.CreateBlip(569, org.Position, 1, 3, $"Ваша компания", 0, 0, true, 0, 0);
                                Blip.Transparency = 0;
                                player.SetData("COMPANYBLIP", Blip);
                                foreach (var p in NAPI.Pools.GetAllPlayers())
                                {
                                    if (!Main.Players.ContainsKey(p)) continue;
                                    if (Main.Players[p].UUID != UUID) continue;
                                    Trigger.PlayerEvent(p, "changeBlipAlpha", Blip, 255);
                                }
                            }
                        }
                        catch { }
                    }, 1000);
			}
			catch (Exception e) { Log.Write("createblip: " + e.ToString());}
		}

        public static void CheckAchievements(Player player) {
            try {
                if(Main.Players[player].Achievements[1] && !Main.Players[player].Achievements[2]) player.SetData("CollectThings", 0);
                else if(Main.Players[player].Achievements[2] && !Main.Players[player].Achievements[4] && !Main.Players[player].Achievements[5]) Trigger.PlayerEvent(player, "createWaypoint", 1924.4f, 4922.0f);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CheckAchievements\":\n" + e.ToString());
            }
        }
        public async Task<bool> Save(Player player, List<float> floats = null, int health=0, int armor=0)
        {

            try
            {
                Customization.SaveCharacter(Main.Players[player].UUID);
                NAPI.Task.Run(async () =>
                {

                    bool inveh = NAPI.Player.IsPlayerInAnyVehicle(player);

                    string pos;
                    if (floats == null)
                    { 
                        Vector3 LPos = (inveh) ? player.Vehicle.Position + new Vector3(0, 0, 0.5) : player.Position;
                        pos = JsonConvert.SerializeObject(LPos);
                    }else
                    {
                        Vector3 posit = new Vector3(floats[0], floats[1], floats[2]);
                        Vector3 LPos = (inveh) ? posit + new Vector3(0, 0, 0.5) : posit;
                        pos = JsonConvert.SerializeObject(LPos);
                    }
                    try
                    {
                        if (InsideHouseID != -1)
                        {
                            House house = HouseManager.Houses.FirstOrDefault(h => h.ID == InsideHouseID);
                            if (house != null)
                                pos = JsonConvert.SerializeObject(house.Position + new Vector3(0, 0, 1.12));
                        }
                        if (InsideGarageID != -1)
                        {
                            Garage garage = GarageManager.Garages[InsideGarageID];
                            pos = JsonConvert.SerializeObject(garage.Position + new Vector3(0, 0, 1.12));
                        }
                        if (ExteriorPos != new Vector3())
                        {
                            Vector3 position = ExteriorPos;
                            pos = JsonConvert.SerializeObject(position + new Vector3(0, 0, 1.12));
                        }
                        if (InsideHotelID != -1)
                        {
                            Vector3 position = Houses.Hotel.HotelEnters[InsideHotelID];
                            pos = JsonConvert.SerializeObject(position + new Vector3(0, 0, 1.12));
                        }
                        if (TuningShop != -1)
                        {
                            Vector3 position = BCore.BizList[TuningShop].GetPos();
                            pos = JsonConvert.SerializeObject(position + new Vector3(0, 0, 1.12));
                        }
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoadPos\":\n" + e.ToString()); }

                    NAPI.Task.Run(() => { 
                        try
                        {
                            
                            if (IsSpawned && !IsAlive)
                            {
                                pos = JsonConvert.SerializeObject(Fractions.Ems.emsCheckpoints[2]);
                                Health = 20;
                                Armor = 0;
                            }
                            else
                            {
                                if (health != 0)
                                {
                                    Health = health;
                                    Armor = armor;
                                }
                                else
                                {
                                    Health = NAPI.Player.GetPlayerHealth(player);
                                    Armor = NAPI.Player.GetPlayerArmor(player);
                                }
                            }
                        }
                        catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoadHP\":\n" + e.ToString()); }
                    });

                    try
                    {
                        var aItem = nInventory.Find(UUID, ItemType.BodyArmor);
                        if (aItem != null && aItem.IsActive)
                            aItem.Data = $"{Armor}";
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoadArmorItem\":\n" + e.ToString()); }

                    try
                    {
                        var all_vehicles = VehicleManager.getAllPlayerVehicles(player.Name);
                        foreach (var number in all_vehicles)
                            VehicleManager.Save(number);
                    }
                    catch (Exception e) { Log.Write("EXCEPTION AT \"UnLoadVehicles\":\n" + e.ToString()); }

                    if (!IsSpawned)
                        pos = JsonConvert.SerializeObject(SpawnPos);

                    Main.PlayerSlotsInfo[UUID] = new Tuple<int, int, int, long>(LVL, EXP, FractionID, Money);

                    await MySQL.QueryAsync($"UPDATE `characters` SET `pos`='{pos}',`gender`={Gender},`health`={Health},`armor`={armor},`lvl`={LVL},`exp`={EXP}," +
                        $"`money`={Money},`bank`={Bank},`work`={WorkID},`fraction`={FractionID},`fractionlvl`={FractionLVL},`familycid`='{FamilyCID}', `familyrank`={FamilyRank},`arrest`={ArrestTime}," +
                        $"`wanted`='{JsonConvert.SerializeObject(WantedLVL)}',`biz`='{JsonConvert.SerializeObject(BizIDs)}',`adminlvl`={AdminLVL},`product`={product}," +
                        $"`licenses`='{JsonConvert.SerializeObject(Licenses)}',`unwarn`='{MySQL.ConvertTime(Unwarn)}',`unmute`='{Unmute}'," +
                        $"`warns`={Warns},`hotel`={HotelID},`hotelleft`={HotelLeft},`lastveh`='{LastVeh}',`onduty`={OnDuty},`lasthour`={LastHourMin}," +
                        $"`demorgan`={DemorganTime},`contacts`='{JsonConvert.SerializeObject(Contacts)}',`achiev`='{JsonConvert.SerializeObject(Achievements)}'," +
                        $"`sim`={Sim},`PetName`='{PetName}',`eat`='{Eat}',`water`='{Water}', `luckywheell`={LuckyWheell}," +
                        $"`time`='{TimeMinutes}', `org`='{Org}', `quests`='{JsonConvert.SerializeObject(Quests)}', `states`='{JsonConvert.SerializeObject(States)}' WHERE `uuid`={UUID}");
                    });
                
                    MoneySystem.Bank.Save(Bank);
                    await Log.DebugAsync($"Player [{FirstName}:{LastName}] was saved.");
                    return true;
                }
                    catch (Exception e)
                {
                    Log.Write("EXCEPTION AT \"Save\":\n" + e.ToString());
                    return false;
                }
        }

        public async Task<int> Create(Player player, string firstName, string lastName)
        {
            try
            {
                if (Main.Players.ContainsKey(player))
                {
                    Log.Debug("Main.Players.ContainsKey(player)", nLog.Type.Error);
                    return -1;
                }

                if (firstName.Length < 1 || lastName.Length < 1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Ошибка в длине имени/фамилии", 3000);
                    return -1;
                }
                if (Main.PlayerNames.ContainsValue($"{firstName}_{lastName}"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Данное имя уже занято", 3000);
                    return -1;
                }

                UUID = GenerateUUID();

                FirstName = firstName;
                LastName = lastName;

                Bank = MoneySystem.Bank.Create($"{firstName}_{lastName}");

                Main.PlayerBankAccs.Add($"{firstName}_{lastName}", Bank);

                Licenses = new List<bool>() { false, false, false, false, false, false, false, false };

                Achievements = new List<bool>();

                for(uint i = 0; i != 401; i++) Achievements.Add(false);

                SpawnPos = new Vector3(-1037.928, -2738.124, 19.04927);

                Main.PlayerSlotsInfo.Add(UUID, new Tuple<int, int, int, long>(LVL, EXP, FractionID, Money));
                Main.PlayerUUIDs.Add($"{firstName}_{lastName}", UUID);
                Main.PlayerNames.Add(UUID, $"{firstName}_{lastName}");

                Quests = QuestsManager.OptimizePlayer(player);

                await MySQL.QueryAsync($"INSERT INTO `characters`(`uuid`,`firstname`,`lastname`,`gender`,`health`,`armor`,`lvl`,`exp`,`money`,`bank`,`work`,`fraction`,`fractionlvl`,`familycid`,`familyrank`,`arrest`,`demorgan`,`wanted`," +
                    $"`biz`,`adminlvl`,`product`,`licenses`,`unwarn`,`unmute`,`warns`,`lastveh`,`onduty`,`lasthour`,`hotel`,`hotelleft`,`contacts`,`achiev`,`sim`,`pos`,`createdate`,`eat`,`water`,`married`,`vunmute`,`bus`," +
                    $"`lawnmower`,`collector`,`trucker`,`traktorist`,`gopostal`,`electric`,`trashcar`,`constructor`,`miner`,`diver`,`snow`,`time`,`org`,`orglic`,`cooldown`,`quests`,`luckywheell`,`states`) " +
                    $"VALUES({UUID},'{FirstName}','{LastName}',{Gender},{Health},{Armor},{LVL},{EXP},{Money},{Bank},{WorkID},{FractionID},{FractionLVL},'{FamilyCID}',{FamilyRank},{ArrestTime},{DemorganTime}," +
                    $"'{JsonConvert.SerializeObject(WantedLVL)}','{JsonConvert.SerializeObject(BizIDs)}',{AdminLVL},{product},'{JsonConvert.SerializeObject(Licenses)}','{MySQL.ConvertTime(Unwarn)}'," +
                    $"'{Unmute}',{Warns},'{LastVeh}',{OnDuty},{LastHourMin},{HotelID},{HotelLeft},'{JsonConvert.SerializeObject(Contacts)}','{JsonConvert.SerializeObject(Achievements)}',{Sim}," +
                    $"'{JsonConvert.SerializeObject(SpawnPos)}','{MySQL.ConvertTime(CreateDate)}','{Eat}','{Water}','{Married}','{VUnmute}',{WBus},{WLawnmower},{WCollector},{WTrucker}," +
                    $"{WTraktorist},{WGopostal},{WElectric},{WTrashCar},{WConstructor},{WMiner},{WDiver},{WSnow},'{TimeMinutes}','{Org}','{OrgLic}','{MySQL.ConvertTime(Cooldown)}','{JsonConvert.SerializeObject(Quests)}',{LuckyWheell},'{JsonConvert.SerializeObject(States)}')");
                NAPI.Task.Run(() => { player.Name = FirstName + "_" + LastName; });
                nInventory.Check(UUID);
                Main.Players.Add(player, this);

                return UUID; 
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"Create\":\n" + e.ToString());
                return -1;
            }
        }
		

        private int GenerateUUID()
        {
            var result = 000001;
            while (Main.UUIDs.Contains(result))
                result = Rnd.Next(000001, 999999);

            Main.UUIDs.Add(result);
            return result;
        }
		public int FAKEGenerateUUID()
        {
            string result = "111";
            int num;
            num = Rnd.Next(000001, 999999);
            num.ToString();
            result += num;
            Int32.TryParse(result, out num);

            return num;
        }
        
        public static Dictionary<string, string> toChange = new Dictionary<string, string>();
        private static MySqlCommand nameCommand;

        public Character()
        {
            nameCommand = new MySqlCommand("UPDATE `characters` SET `firstname`=@fn, `lastname`=@ln WHERE `uuid`=@uuid");
        }

        public static async Task changeName(string oldName)
        {
            try
            {
                if (!toChange.ContainsKey(oldName)) return;

                string newName = toChange[oldName];

                //int UUID = Main.PlayerNames.FirstOrDefault(u => u.Value == oldName).Key;
                int Uuid = Main.PlayerUUIDs.GetValueOrDefault(oldName);
                if (Uuid <= 0)
                {
                    await Log.WriteAsync($"Cant'find UUID of player [{oldName}]", nLog.Type.Warn);
                    return;
                }

                string[] split = newName.Split("_");

                Main.PlayerNames[Uuid] = newName;
                Main.PlayerUUIDs.Remove(oldName);
                Main.PlayerUUIDs.Add(newName, Uuid);
                try { 
                    if(Main.PlayerBankAccs.ContainsKey(oldName)) { 
                        int bank = Main.PlayerBankAccs[oldName];
                        Main.PlayerBankAccs.Add(newName, bank);
                        Main.PlayerBankAccs.Remove(oldName);
                    }
                } catch { }

                MySqlCommand cmd = nameCommand;
                cmd.Parameters.AddWithValue("@fn", split[0]);
                cmd.Parameters.AddWithValue("@ln", split[1]);
                cmd.Parameters.AddWithValue("@uuid", Uuid);
                await MySQL.QueryAsync(cmd);

                NAPI.Task.Run(() =>
                {
                    try
                    {
                        VehicleManager.changeOwner(oldName, newName);
                        BCore.changeOwner(oldName, newName);
                        OCore.changeOwner(oldName, newName);
                        MoneySystem.Bank.changeHolder(oldName, newName);
                        Houses.HouseManager.ChangeOwner(oldName, newName);
                    }
                    catch { }
                });

                await Log.DebugAsync("Nickname has been changed!", nLog.Type.Success);
                toChange.Remove(oldName);
                MoneySystem.Donations.Rename(oldName, newName);
                GameLog.Name(Uuid, oldName, newName);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"CHANGENAME\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
    }
}
