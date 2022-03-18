using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using NeptuneEVO.SDK;
using NeptuneEVO;
using System.Linq;

namespace NeptuneEVO.Utils
{

    #region Ядро
    public class GunGame : Script
    {
        static nLog Log = new nLog("GunGame");
        static int LastID = -1;
        // Дописать функцию дестроя арены ( + )
        // Запретить инвентарь ( + )
        // Убрать меню респавна ( + )
        // Авто респавн с кд 5 секунд ( + )
        // Создать простую систему карт, желательно классом ( + )
        
        #region Список арен
        public static Dictionary<int, Arena> Arenas = new Dictionary<int, Arena>();
        #endregion
        #region Список карт
        public static List<Map> Maps = new List<Map>
        {
            new Map("Остров", new Vector3(4970.987, -5170.7935, 1.1226937)  ,150f, new List<Vector3> {
                new Vector3(4990.1753, -5177.8555, 1.382152),
                new Vector3(4956.223, -5199.5005, 1.537428),
                new Vector3(4915.347, -5195.8066, 1.3769137),
                new Vector3(4921.111, -5238.712, 1.3991603),
                new Vector3(4891.423, -5207.5493, 1.551087),
                new Vector3(4878.8457, -5173.3286, 1.3722018),
                new Vector3(4993.0845, -5131.439, 1.4195458),
                new Vector3(4983.568, -5105.7725, 1.5558234),
                new Vector3(4962.831, -5087.9565, 2.1102433),
                new Vector3(4941.809, -5096.5103, 1.9233516),
                new Vector3(5006.4766, -5196.441, 1.3948175),
                new Vector3(5001.3955, -5215.7524, 1.3845671),
                new Vector3(4972.844, -5219.563, 1.4001626),
                new Vector3(4956.678, -5217.8613, 1.3933545),
                new Vector3(4963.489, -5181.6255, 1.354004),
                new Vector3(4962.6304, -5145.4595, 1.4192888),
                new Vector3(4999.4165, -5165.216, 1.6443433),
                new Vector3(4948.6846, -5180.341, 1.3540424),
                new Vector3(4872.2505, -5194.4937, 1.6842593),
                new Vector3(4934.1523, -5224.2593, 1.4328138)
            })

        };
        #endregion
        #region Создание главного шейпа
        [ServerEvent(Event.ResourceStart)]
        public void CreateGunGame()
        {
            try
            {

                Vector3 pos = new Vector3(-265.5906, -2017.5664, 29.025599);

                ColShape shape = NAPI.ColShape.CreateSphereColShape(pos, 5f);
                NAPI.TextLabel.CreateTextLabel("~b~GunGame", new Vector3(pos.X, pos.Y, pos.Z + 1f), 5F, 0.5F, 0, new Color(255, 255, 255), true, 0);
                NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 0.5f), new Vector3(), new Vector3(), 0.965f, new Color(0, 175, 250, 220), false, 0);
                NAPI.Marker.CreateMarker(27, pos + new Vector3(0, 0, 0.14f), new Vector3(), new Vector3(), 1f, new Color(0, 175, 250, 220), false, 0);
                shape.OnEntityEnterColShape += (s, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 522);
                    }
                    catch (Exception e) { Log.Write("CREATE GUN GAME SHAPE" + e.ToString(), nLog.Type.Error); }
                };
                shape.OnEntityExitColShape += (s, player) =>
                {
                    try
                    {
                        player.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Log.Write("CREATE GUN GAME SHAPE" + e.ToString(), nLog.Type.Error); }
                };
            }
            catch (Exception e) { Log.Write("CREATE GUN GAME" + e.ToString(), nLog.Type.Error); }
        }
        #endregion
        #region Когда игрок умерает
        [ServerEvent(Event.PlayerDeath)]
        public void onPlayerDeathHandler(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                if (!player.HasData("ARENA")) return;
                if (player.GetData<Arena>("ARENA").Active)
                {
                    if (entityKiller != null && player != entityKiller)
                    {
                        entityKiller.SetData("KILLS", entityKiller.GetData<int>("KILLS") + 1);
                        NAPI.ClientEvent.TriggerClientEvent(entityKiller, "client::setkills", entityKiller.GetData<int>("KILLS"));
                    }
                    player.SetData("DEATHS", player.GetData<int>("DEATHS") + 1);

                    NAPI.ClientEvent.TriggerClientEvent(player, "client::setdeaths", player.GetData<int>("DEATHS"));
                    player.GetData<Arena>("ARENA").SpawnPlayer(player);
                    player.SetData("DEATH", true);
                }
                else
                {
                    NAPI.Player.SpawnPlayer(player, new Vector3(-265.5906, -2017.5664, 30.025599));
                    player.Dimension = (uint)(player.GetData<Arena>("ARENA").ID + 2000);
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }
        #endregion
        #region Когда игрок выходит
        [ServerEvent(Event.PlayerDisconnected)]
        public static void onPlayerDissonnectedHandler(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (!player.HasData("ARENA")) return;

                Arena arena = player.GetData<Arena>("ARENA");

                if (arena.Players.Contains(player))
                    arena.Players.Remove(player);

                arena.RefreshPlayers();
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }
        #endregion
        #region Ивенты с клиента

        [RemoteEvent("server::getlobbylist")]
        static void RE_getlobbylist(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;

                List<object> LobbyList = new List<object>();

                foreach(Arena arena in Arenas.Values)
                {
                    if (arena.Active) continue;
                    List<object> Lobby = new List<object> {
                        arena.ID,
                        !string.IsNullOrEmpty(arena.Pass),
                        arena.Players.Count + " / " + arena.MaxPlayers,
                        arena.Money,
                        arena.Weapon
                    };
                    LobbyList.Add(Lobby);
                }
                NAPI.ClientEvent.TriggerClientEvent(player, "client::setlobbylist", JsonConvert.SerializeObject(LobbyList));

            }
            catch (Exception e) { Log.Write("RE_getlobbylist: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("server::kickplayer")]
        static void RE_kickplayer(Player player, string nick)
        {
            try 
            {
                if (!player.HasData("ARENA")) return;
                Arena arena = player.GetData<Arena>("ARENA");
                if (arena.Owner != player) return;
                Player findnick = null;
                
                foreach (Player ply in arena.Players)
                    if (ply.Name == nick)
                        findnick = ply;

                if (findnick == arena.Owner) return;

                arena.Players.Remove(findnick);
                arena.RefreshPlayers();

                NAPI.Entity.SetEntityPosition(findnick, new Vector3(-265.5906, -2017.5664, 30.025599));
                Main.Players[findnick].ExteriorPos = new Vector3();

                NAPI.ClientEvent.TriggerClientEvent(findnick, "client::closemenu");

            }
            catch (Exception e) { Log.Write("RE_kickplayer: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("server::startmatch")]
        static void RE_startmatch(Player player)
        {
            try
            {
                if (!player.HasData("ARENA")) return;
                Arena arena = player.GetData<Arena>("ARENA");

                if (arena.Active && arena.Owner != player) return;

                if (arena.Players.Count != arena.MaxPlayers)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нельзя запустить матч, когда лобби не полное!", 3000);
                    return;
                }

                arena.Start();

            }
            catch (Exception e) { Log.Write("RE_startmatch: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("server::connectlobby")]
        static void RE_connectlobby(Player player, int id)
        {
            try
            {
                if (player.HasData("ARENA")) return;
                if (!Arenas.ContainsKey(id)) return;

                Arena arena = Arenas[id];

                if (arena.Players.Count >= arena.MaxPlayers)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Данное лобби переполнено!", 3000);
                    return;
                }
                if (!string.IsNullOrEmpty(arena.Pass))
                {
                    player.SetData("ARENAID", id);
                    NAPI.ClientEvent.TriggerClientEvent(player, "client::closemenu");
                    NAPI.ClientEvent.TriggerClientEvent(player, "openInput", $"Присоединение к Лобби №{id} ", "Введите пароль чтоб войти", 8, "gun_enterpass") ;
                    return;
                }

                Main.Players[player].ExteriorPos = new Vector3(-265.5906, -2017.5664, 29.025599);

                arena.Players.Add(player);

                arena.SetLobby(player);
                arena.RefreshPlayers();

                player.SetData("ARENA", arena);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы вошли в лобби!", 3000);


            }
            catch (Exception e) { Log.Write("RE_connectlobby: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("server::disconnectlobby")]
        static void RE_disconnectlobby(Player player)
        {
            try
            {
                if (!player.HasData("ARENA")) return;

                Arena arena = player.GetData<Arena>("ARENA");
                arena.Players.Remove(player);

                Trigger.PlayerEvent(player, "client::removeweapon", GunGame.WeaponOnName[arena.Weapon]);

                if (arena.Owner == player)
                {

                    foreach (Player ply in arena.Players)
                    {
                        if (!ply.HasData("ARENA")) continue;
                        ply.ResetData("ARENA");
                        NAPI.ClientEvent.TriggerClientEvent(ply, "client::closemenu");
                        Trigger.PlayerEvent(ply, "client::removeweapon", GunGame.WeaponOnName[arena.Weapon]);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Владелец покинул лобби", 3000);
                    }
                    if (Arenas.ContainsKey(arena.ID))
                        Arenas.Remove(arena.ID);
                    arena.Destroy();
                }

                NAPI.Entity.SetEntityPosition(player, new Vector3(-265.5906, -2017.5664, 30.025599));
                Main.Players[player].ExteriorPos = new Vector3();

                NAPI.Entity.SetEntityDimension(player, 0);

                

                player.ResetData("ARENA");

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы вышли из лобби!", 3000);
            }
            catch (Exception e) { Log.Write("RE_disconnectlobby: " + e.Message, nLog.Type.Error); }
        }

        static List<string> WeaponOnIndex = new List<string> { "Revolver", "SMG", "Rifle", "Knife" };
        public static Dictionary<string, int> WeaponOnName = new Dictionary<string, int> { { "Revolver", -1045183535 }, { "SMG", 736523883 }, { "Rifle", -1074790547 }, { "Knife", -1716189206 } };

        [RemoteEvent("server::sendlobby")]
        static void RE_sendlobby(Player player, string LobbyInfoT)
        {
            try
            {
                if (player.HasData("ARENA")) return;

                List<string> LobbyInfo = JsonConvert.DeserializeObject<List<string>>(LobbyInfoT);

                if (Convert.ToInt32( LobbyInfo[1] ) > 16)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Максимально 16 игроков!", 3000);
                    return;
                }
                if (Convert.ToInt32(LobbyInfo[1]) < 2)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Минимально 2 игрока!", 3000);
                    return;
                }
                if (Convert.ToInt32(LobbyInfo[2]) > 100000 || Convert.ToInt32(LobbyInfo[2]) < 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Максимально 25000$", 3000);
                    return;
                }
                if (Main.Players[player].Money < Convert.ToInt32(LobbyInfo[2]))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }
                if (Arenas.Count >= 16)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "В данный момент список матчей переполнен, подождите", 3000);
                    return;
                }
                LastID++;

                Arena arena = new Arena(player, LastID, WeaponOnIndex[Convert.ToInt32(LobbyInfo[3])], Convert.ToInt32(LobbyInfo[2]), Maps[0], LobbyInfo[0], Convert.ToInt32(LobbyInfo[1])) ;
                arena.Players.Add(player);
                Arenas.Add(LastID, arena);

                MoneySystem.Wallet.Change(player, -Convert.ToInt32(LobbyInfo[2]));

                List<object> Lobby = new List<object>
                {
                    "1 / " + Convert.ToInt32(LobbyInfo[1]),
                    arena.Weapon,
                    arena.Money,
                    arena.ID
                };

                Main.Players[player].ExteriorPos = new Vector3(-265.5906, -2017.5664, 30.025599);

                player.SetData("ARENA", arena);

                NAPI.Entity.SetEntityDimension(player, (uint)(2000 + arena.ID));

                NAPI.ClientEvent.TriggerClientEvent(player, "client::createlobby", JsonConvert.SerializeObject(new List<string> { player.Name }), JsonConvert.SerializeObject(Lobby));

            }
            catch  { }
        }


        #endregion
    }
    #endregion
    #region Конструктор карт
    public class Map
    {
        public string Name;
        public Vector3 Center;
        public float Radius;
        public List<Vector3> SpawnPos;

        public Map(string name, Vector3 center, float radius, List<Vector3> spawns)
        {
            Name = name; Center = center; Radius = radius; SpawnPos = spawns;
        }

    }
    #endregion
    #region Конструктор арены
    public class Arena
    {
        public Player Owner;
        public int ID;
        public bool Active;
        public int MaxPlayers;
        public string Weapon = "none";
        public int Money;
        public string Pass = "";
        public Map Map;
        public List<Player> Players;
        string Timerid;
        int Time;
        int TimeStart;
        int Dimension;
        ColShape CenterColShape;

        public Arena( Player owner, int id, string weapon, int money, Map map, string pass, int maxplayers )
        {
            Owner = owner; ID = id; Active = false; Weapon = weapon; Players = new List<Player>(); Money = money; Map = map; Pass = pass; MaxPlayers = maxplayers;
        }
        #region Когда матч начинается
        public void Start()
        {
            try
            {
                Dimension = 2300 + ID;
                int i = 0;
                foreach (Player player in Players)
                    if (player.HasData("ARENA") && player.GetData<Arena>("ARENA").ID == ID)
                    {

                            NAPI.Entity.SetEntityPosition(player, Map.SpawnPos[i] + new Vector3(0, 0, 1.15));

                        NAPI.ClientEvent.TriggerClientEvent(player, "client::closemenuno");
                        NAPI.ClientEvent.TriggerClientEvent(player, "client::sethud", true);
                        NAPI.ClientEvent.TriggerClientEvent(player, "client::setkills", 0);
                        NAPI.ClientEvent.TriggerClientEvent(player, "client::setdeaths", 0);
                        player.SetData("KILLS", 0);
                        player.SetData("DEATHS", 0);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Начинается матч!", 3000);
                        NAPI.ClientEvent.TriggerClientEvent(player, "client::setweapon", GunGame.WeaponOnName[Weapon]);
                        
                        NAPI.Entity.SetEntityDimension(player, (uint)Dimension);
                        i++;
                    }
                CenterColShape = NAPI.ColShape.CreateSphereColShape(Map.Center, Map.Radius, (uint)Dimension);
                CenterColShape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Entity.SetEntityPosition(entity, GetSpawnPos());
                        Notify.Send(entity, NotifyType.Error, NotifyPosition.BottomCenter, "Вы покинули зону боя, возвращение назад!", 3000);
                    }
                    catch { }
                };
                Time = 900;
                TimeStart = 10;
                Timerid = Timers.StartTask(1000, () => { TimerStart(); });
            }
            catch (Exception e) { Console.WriteLine("start: " + e.ToString()); }
        }
        #endregion
        #region Спавн игрока когда он умер
        public void SpawnPlayer(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("DEATH")) return;
                    NAPI.Player.SpawnPlayer(player, GetSpawnPos());
                    NAPI.Entity.SetEntityDimension(player, (uint)Dimension);
                    NAPI.ClientEvent.TriggerClientEvent(player, "client::setweapon", GunGame.WeaponOnName[Weapon]);
                }
                catch (Exception e) { Console.WriteLine("respawn: " + e.ToString()); }
            }, 5000);
        }
        #endregion
        #region Отчёт времени старта
        public void TimerStart()
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (Players == null)
                    {
                        Timers.Stop(Timerid);
                        return;
                    }

                    string sec = TimeStart % 60 < 10 ? $"0{TimeStart % 60}" : (TimeStart % 60).ToString();
                    string time = $"{TimeStart / 60}:{sec}";

                    foreach (Player player in Players)
                    {
                        NAPI.ClientEvent.TriggerClientEvent(player, "client::settime", time);
                    }

                    TimeStart -= 1;
                    if (TimeStart <= 0)
                    {
                        Timers.Stop(Timerid);
                        Timerid = Timers.StartTask(1000, () => { Timer(); });
                    }
                    Active = true;
                }
                catch (Exception e) { Console.WriteLine("timerstart: " + e.ToString()); }
            });

        }
        #endregion
        #region Таймер
        public void Timer()
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    string sec = Time % 60 < 10 ? $"0{Time % 60}" : (Time % 60).ToString();
                    string time = $"{Time / 60}:{sec}";

                    foreach (Player player in Players)
                    {
                        NAPI.ClientEvent.TriggerClientEvent(player, "client::settime", time);
                    }

                    Time -= 1;
                    if (Time <= 0)
                        Stop();
                }
                catch (Exception e) { Console.WriteLine("timer: " + e.ToString()); }
            });
        }
        #endregion
        #region Уничтожение арены
        public void Destroy()
        {
            try
            {
                Owner = null; ID = -1; Weapon = ""; Players = null; Money = 0; Map = null; Pass = null; MaxPlayers = 0; Time = 0; Timerid = null; NAPI.Task.Run(() => { try { CenterColShape.Delete(); } catch { } });
            }
            catch (Exception e) { Console.WriteLine("destroy: " + e.ToString()); }
        }

        #endregion

        #region Когда матч заканчивается
        public void Stop()
        {
            try
            {
                List<object> Winners = new List<object> { };

                Dictionary<Player, int> ForSort = new Dictionary<Player, int>();


                foreach (Player player in Players)
                    ForSort.Add(player, player.GetData<int>("KILLS"));

                ForSort = ForSort.OrderBy(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

                int i = 0;

                foreach (KeyValuePair<Player, int> pair in ForSort.Reverse().ToDictionary(x => x.Key, x => x.Value))
                {
                    i++;
                    List<object> Winner = new List<object> { pair.Key.Name, pair.Value, pair.Key.GetData<int>("DEATHS"), i == 1 ? Money : 0 };
                    if (i == 1)
                        MoneySystem.Wallet.Change(pair.Key, Money);
                    Winners.Add(Winner);
                }

                foreach (Player player in Players)
                    if (player.HasData("ARENA") && player.GetData<Arena>("ARENA").ID == ID)
                    {
                        if (player.HasData("DEATH"))
                        {
                            NAPI.Player.SpawnPlayer(player, new Vector3(-265.5906, -2017.5664, 30.025599));
                            player.ResetData("DEATH");
                        }
                        NAPI.ClientEvent.TriggerClientEvent(player, "client::sethud", false);
                        NAPI.ClientEvent.TriggerClientEvent(player, "client::sendwinners", JsonConvert.SerializeObject(Winners));
                        NAPI.Entity.SetEntityPosition(player, new Vector3(-265.5906, -2017.5664, 30.025599));
                        NAPI.ClientEvent.TriggerClientEvent(player, "client::removeweapon", GunGame.WeaponOnName[Weapon]);
                        NAPI.Entity.SetEntityDimension(player, 0);
                        Main.Players[player].ExteriorPos = new Vector3();
                        player.ResetData("KILLS");
                        player.ResetData("DEATHS");
                        player.ResetData("ARENA");
                    }
                Active = false;
                if (GunGame.Arenas.ContainsKey(ID))
                    GunGame.Arenas.Remove(ID);

                Timers.Stop(Timerid);

                Destroy();
            }
            catch (Exception e) { Console.WriteLine("stop: " + e.ToString()); }
        }
        #endregion
        #region Функция для изображения лобби для игрока
        public void SetLobby(Player player)
        {
            try
            {
                List<object> LobbyInfo = new List<object> 
                {
                    Players.Count + " / " + MaxPlayers,
                    Weapon,
                    Money,
                    ID
                };

                List<string> NamePlayers = new List<string>();

                foreach (Player players in Players)
                    NamePlayers.Add(players.Name);

                NAPI.Entity.SetEntityDimension(player, (uint)(2000 + ID));

                NAPI.ClientEvent.TriggerClientEvent(player, "client::setlobby", JsonConvert.SerializeObject(NamePlayers), JsonConvert.SerializeObject(LobbyInfo));
            }
            catch (Exception e) { Console.WriteLine("setlobby: " + e.ToString()); }
        }
        #endregion
        #region Обновление данных о лобби, а именно игроков
        public void RefreshPlayers()
        {
            try
            {
                List<string> NamePlayers = new List<string>();

                foreach (Player player in Players)
                    NamePlayers.Add(player.Name);

                foreach (Player player in Players)
                    if (player.HasData("ARENA") && player.GetData<Arena>("ARENA").ID == ID)
                    {
                        NAPI.ClientEvent.TriggerClientEvent(player, "client::refreshlobby", JsonConvert.SerializeObject(NamePlayers));
                    }
            }
            catch (Exception e) { Console.WriteLine("refreshplayers: " + e.ToString()); }
        }
        #endregion

        #region Поиск свободной позиций спавна
        public Vector3 GetSpawnPos()
        {
            try
            {
               /* Dictionary<float, Vector3> Distance = new Dictionary<float, Vector3>();

                foreach (Vector3 pos in Map.SpawnPos)
                {
                    float distan = Players[0].Position.DistanceTo2D(pos);
                    foreach (Player player in Players)
                    {
                        float fist = player.Position.DistanceTo2D(pos);
                        if (fist > distan)
                            distan = fist;
                    }
                    Distance.Add(distan, pos);
                }

                float dist = 0f;

                foreach (KeyValuePair<float, Vector3> pairs in Distance)
                    if (pairs.Key > dist)
                        dist = pairs.Key;*/

                return Map.SpawnPos[new Random().Next(0, Map.SpawnPos.Count)];
            }
            catch (Exception e) { Console.WriteLine("getspawnpos: " + e.ToString()); return new Vector3(); }
        }
        #endregion
    }
#endregion
}
