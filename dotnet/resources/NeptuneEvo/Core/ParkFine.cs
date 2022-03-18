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
using System.Threading;
using NeptuneEVO.Houses;
using static NeptuneEVO.Core.VehicleManager;
using System.Security.Cryptography;
using static NeptuneEVO.Houses.GarageManager;
using NeptuneEVO.Businesses;

// code: koltr

namespace NeptuneEVO.Core
{
    class ParkManager : Script
    {

        private static nLog Log = new nLog("ParkManager");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var colsp = new ParkBuy(new Vector3(4.42, -1073.18, 38.15));

                Vector3 pos = new Vector3(-911.34705, -2039.7671, 9.284781);
                ColShape shape = NAPI.ColShape.CreateCylinderColShape(pos, 2f, 3, 0);
                shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 156);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                NAPI.TextLabel.CreateTextLabel("~b~Организации", new Vector3(pos.X, pos.Y, pos.Z), 20F, 0.5F, 0, new Color(255, 255, 255), true, 0);
                NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 1f), new Vector3(), new Vector3(), 1f, new Color(0, 175, 250, 220), false, 0);


            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"PARKMAMAGER\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        [RemoteEvent("liccallback")]

        public static void PlayerEvent_Lic(Player player, int id)
        {
            if (!Main.Players.ContainsKey(player)) return;
            switch (id)
            {
                case 0:
                    if (Main.Players[player].OrgLic == 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас уже есть лицензия!", 3000);
                        return;
                    }
                    if (Main.Players[player].Money < 2000000)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств, цена: 2 000 000$", 3000);
                        return;
                    }
                    MoneySystem.Wallet.Change(player, -2000000);
                    Main.Players[player].OrgLic = 0;
                    MySQL.Query($"UPDATE `characters` set `orglic`=0 where `uuid`='{Main.Players[player].UUID}'");
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы приобрели лицензию на грузоперевозки", 3000);
                    return;
            
            }
            
        }

        public static Dictionary<string, string> ModelList = new Dictionary<string, string>()
        {
			
			//автомобили
            {"sultan", "Sultan"},
            {"sultanrs", "Sultan RS"},
            {"kuruma", "Kuruma"},
            {"fx50s", "Infiniti FX 50S"},
            {"gls63", "Mercedes-AMG GLS 63"},
            {"rs7r", "Audi RS7 Sportback"},
            {"e60", "BMW M5 E60"},
            {"ayxz", "Rolls-Royce Dawn ONYX"},
            {"mansm8", "BMW M8 Competition MANSAUG"},
            {"500gtrlam", "Lamborghini Diablo"},
            {"vwtouareg", "Volkswagen Touareg"},
            {"c63", "Mercedes Benz C63 w204"},
            {"amggtrr20", "M-B AMG GT-R Roadster"},
            {"q820", "Audi Q8 2020"},
            {"rrw13", "Rolls-Royce Wraith Black"},
            {"uaz3159", "UAZ"},
            {"rmodmi8lb", "BMW I8 Liberty Walk Roadster"},
            {"g63pp", "Mercedes-Benz G63 Mansory"},
            {"x7bmw", "BMW X7"},
            {"venatus", "Lamborghini URUS Mansory"},
            {"rmodbmwm8", "BMW M8"},
            {"bmwm2", "BMW M2 Competition"},
            {"manscull", "Rolls-Royce Cullinan Mansory"},
            {"rrmansory", "Rolls-Royce Mansory"},
            {"2778647937", "Rolls-Royce Mansory"},
            {"bmci", "BMW M5 F90"},
            {"rmodc63amg", "Mercedes-Benz C63 AMG Coupe"},
            {"e63amg", "M-B E63 AMG Brabus"},
            {"e63b", "Mercedes-Benz E63 Brabus"},
			{"keyrus2", "Lamborghini Urus Keyvany"},
            {"rmodsian", "Lamborghini Sián"},
            {"urus", "Lamborghini Urus"},
			{"vulcan", "Aston Martin Vulcan"},
			{"jes", "Koenigsegg Jesko"},
			{"cyber", "Tesla CyberTruck"},
            {"r820", "Audi R8 v10"},
            {"19S650", "Mercedes-Benz Maybach S650"},
			{"19s650", "Mercedes-Benz Maybach S650"},
            {"g63amg6x6", "Mercedes-Benz G63 Amg 6x6"},
            {"cayen19", "Porsche Cayenne Turbo S"},
            {"a80", "Toyota Supra"},
            {"s600w220", "Mercedes Benz S600 w220"},
			{"481790297", "Mercedes Benz S600 w220"},
            {"bentaygast", "Bentley Bentayga"},
            {"teslapd", "Tesla Model S"},
            {"agerars", "Koenigsegg Agera RS"},
            {"f8t", "Ferrari F8 Tributo"},
            {"veneno", "Lamborghini Veneno"},
            {"demon", "Dodge Challenger Demon"},
            {"teslax", "Tesla Model X"},
            {"f458", "Ferrari 458"},
            {"evoque", "Range Rover Evoque"},
            {"rsvr16", "Range Rover Sport SVR"},
            {"rr12", "Range Rover Supercharged"},
            {"mustangbkit", "Ford Mustang GT"},
            {"gt17", "Ford GT"},
            {"huayra", "Pagani Huayra"},
            {"gto65", "Pontiac GTO 65"},
            {"esv", "Cadillac Escalade"},
            {"sti", "Subaru Impreza STI"},
            {"lex570", "Lexus LX570"},
            {"polchiron", "Bugatti Chiron |Police"},
            {"polgt17", "Ford GT | Police"},
            {"g63amg6x6cop", "Mercedes-Benz G63 AMG 6x6 | Police"},
            {"ghispo2", "Maserati Ghibli | Police"},
            {"polgs350", "Lexus GS 350 | Police"},
            {"mlbrabus", "Mercedes-Benz ML Brabus"},
            {"18performante", "Lamborghini Huracan Performante"},
            {"lada2107", "VAZ-2107"},
            {"p1", "McLaren P1"},
            {"720s", "McLaren 720S"},
            {"rmodjeep", "Jeep Grandcherokee Trackhawk"},
            {"slsamg", "Mercedes Benz SLS AMG"},
            {"pol458", "Ferrari 458 Italia | Police"},
            {"polrevent", "Lamborghini Reventon | Police"},
            {"rmodcamaro", "Chevrolet Camaro Zl1"},
            {"tahoe", "Chevrolet Tahoe"},
            {"x5e53", "BMW X5 E53"},
            {"skyline", "Nissan Skyline GT-R R34"},
            {"navigator", "Lincoln Navigator"},
            {"e34", "BMW M5 E34"},
            {"m3e46", "BMW M3 E46"},
            {"pthrust", "Bike | Police"},
            {"kalina", "Lada Kalina"},
            {"e55w211", "Mercedes-Benz E320 W211"},
            {"passat", "Volkswagen Passat B8"},
            {"918", "Porsche 918 Spyder"},
            {"boss302", "Ford Mustang Boss 302"},
            {"cullinan", "Rolls-Royce Cullinan"},
            {"18phantom", "Rolls-Royce Phantom"},
            {"718caymans", "Porsche 718 Caymans"},
            {"apriora", "Lada Priora"},
            {"audir8lb", "Audi R8 V10 Liberty Walk"},
            {"camry18", "Toyota Camry XSE 2018"},
            {"demonhawk", "Jeep SRT-8 Demonhawk"},
            {"gle", "Mercedes-Benz GLE"},
            {"kia", "KIA Stinger GT"},
            {"vxr", "Toyota Land Cruiser 200"},	
			{"g20wide", "BMW G20 320d Wide"},	
			{"ls430", "Lexus LS430"},
			{"rmodsianr", "Lamborghini Sian Roadster"},
			{"bolide", "Bugatti Bolide"},
            {"m4f82", "BMW M4 F82"},
            {"m4lb", "BMW M4 F82 Liberty Walk"},
            {"panamera17turbo", "Porsche Panamera Turbo"},
            {"x5g05", "BMW X5 30D"},
            {"rmodrs6", "Audi RS6"},
            {"c8", "Chevrolet Corvette"},
            {"jp12", "Jeep Wrangler Rubicon"},
            {"lanex400", "Mitsubishi Lancer X"},
            {"lp570", "Lamborghini Gallardo"},
            {"zim", "Gaz Zim"},
            {"rmodchiron300", "Buggati Chiron Super Sport"},
            {"amggtr", "Mercedes-Benz AMG GT R"},
            {"ody18", "Honda Odyssey"},
            {"amggt63s", "Mercedes-Benz AMG GT63S"},
            {"as350", "Helicopter | Police"},
            {"rmodf12tdf", "Ferrari F12 TDF"},
            {"fbi458", "Ferrari 458 | FBI"},
            {"fbiamggt", "Mercedes-Benz AMG GT | FBI"},
            {"fbigs350", "Lexus GS350 | FBI"},
            {"fbilp770", "Lamborghini Centenario | FBI"},
            {"fc13", "Ferrari California"},
            {"gtc4", "Ferrari GTC4 Lusso"},
            {"gurkha", "Bronevik | Police"},
            {"laferrari", "Ferrari LaFerrari"},
			{"3746687124", "Ferrari LaFerrari"},
            {"newsvan", "News Van | News"},
            {"pistaspider19", "Ferrari 488Pista Spider"},
            {"pullman", "Mercedes-Benz Pullman"},
            {"qrv", "Ford Explorer | EMS"},
            {"gls", "Mercedes-Benz GLS"},
            {"2018s63", "Mercedes-Benz S63 AMG"},
            {"rmodrs7", "Audi RS7 Sportback 2010"},
            {"2019m5", "BMW M5 F90 Competition"},
            {"19g63", "Mercedes-Benz G63 AMG"},
			{"g63", "Mercedes-Benz G63 AMG"},
			{"g6", "Onyx G6"},
			{"fxxk", "Ferrari FXX-K Hybrid"},
			{"sixtyone41", "1941 Cadillac Series 61"},
			{"urban", "Niva Urban"},
            {"19s63", "Mercedes-Benz S63 AMG Coupe"},
            {"lamboreventon", "Lamborghini Reventon"},
            {"divo", "Bugatti Divo"},
            {"rrst", "Range Rover Hamman"},
            {"xc60", "Volvo XC60"},
            {"oka", "Oka"},
            {"z4bmw", "BMW Z4"},
            {"viper", "Dodge Viper"},
            {"v250", "Mercedes-Benz V-Classe"},
            {"velar", "Range Rover Velar"},
            {"rs318", "Audi RS3 Sportback"},
            {"lc500", "Lexus LC 500"},
            {"rs5r", "Audi RS5-R"},
            {"sq72016", "Audi SQ7"},
            {"nismo20", "Nissan GTR Nismo 2020"},
            {"911turbos", "Porsche 911 Turbo S"},
            {"g65", "Mercedes-Benz G65"},
            {"675ltsp", "McLaren 675LT Spider"},
            {"rmodbentleygt", "Bentley Continental GT"},
            {"ferrari812", "Ferrari 812 Superfast"},
            {"asvj", "Lamborghini Aventador SVJ"},
            {"supersport", "Bugatti Veyron"},
            {"599xxevo", "Ferrari 599xx EVO"},
			{"dmc12", "DMC 12 Delorean"},
            {"600lt", "McLaren 600LT"},
            {"s63amg", "Mercedes S63 AMG"},
            {"fleet78", "Cadillac Fleetwood"},
            {"fairlane66", "Ford Fairlane"},
            {"brabus800", "Mercedes Benz E63 Brabus 800"},
            {"bg700w", "Mercedes Benz G63 Brabus 700"},
            {"812nlargo", "Ferrari 812 Nlargo"},
            //{"16charger", "Dodge Charger"},
            {"evo9", "Mitsubishi EVO 9"},
			{"gemera", "Koenigsegg Gemera"},
			{"cam8tun", "Toyota Camry XSE 2018"},
			{"jzx100", "Tayota Mark II"},
			{"ie", "Apollo Intensa Emozione"},
			
			{"3243052612", "LSPD Contender"},
			{"1392424197", "LSPD Wintergreen"},
			{"905482654", "LSPD Buffalo S 2"},
			{"213223130", "LSPD Merit"},
			{"3260112141", "LSPD Riot"},
			{"1887487254", "LSPD Scout"},
			{"police", "LSPD Stanier"},
			{"police2", "LSPD Buffalo S"},
			{"police3", "LSPD Interceptor"},
			{"police4", "LSPD Stanier"},
			{"pbus", "LSPD Bus"},
           
		    //мото
			{"Bmx", "BMX"},
            {"Faggio2", "Faggio"},
            {"Blazer", "Blazer"},
            {"Enduro", "Enduro"},
            {"Thrust", "Thrust"},
            {"PCJ", "PCJ"},
            {"Hexer", "Hexer"},
            {"lectro", "Lectro"},
            {"Nemesis", "Nemesis"},
            {"Scorcher", "Scorcher"},
            {"Double", "Double"},
            {"Diablous", "Diablous"},
            {"Cliffhanger", "Cliffhanger"},
            {"Nightblade", "Nightblade"},
            {"Vindicator", "Vindicator"},
            {"Gargoyle", "Gargoyle"},
            {"Sanchez2", "Sanchez"},
            {"Akuma", "Akuma"},
            {"Ratbike", "Ratbike"},
            {"CarbonRS", "Carbon RS"},
            {"Ruffian", "Ruffian"},
            {"Hakuchou", "Hakuchou"},
            {"Bati", "Bati"},
            {"BF400", "BF400"},
            {"Sanctus", "Sanctus"},
            {"shotaro", "Shotaro"},
            {"snowmobile", "SnowMoto"},
			
			//кейсовые
			{"h2carb", "Kawasaki H2"},
            {"r6", "Yamaha R6"},
            {"gsx1000", "Suzuki GSX1000"},
            {"cbr1000rrr", "Honda CBR1000RR"},
            {"goldwing", "GoldWing"},
            {"cb500x", "Honda CB500x"},
            {"rmz2", "Suzuki RMZ2"},
            {"xxxxx", "Mercedes-Benz X Classe 6X6"},
            {"2910952364", "Range Rover Supercharged"},
            {"2862422018", "Chevrolet Impala"},
            {"3652845261", "Pontiac GTO"},
            {"905399718", "Toyota Supra"},
            {"Youga", "Youga"},
            {"2521542582", "Volkswagen Passat"},
            {"2314362986", "Bentley Bentayga"},
            {"2003317544", "RollsRoyce Wraith"},
            {"3379916195", "Mercedes-Benz Pullman"},
            {"1056539313", "Mercedes-Benz Maybach"},
            {"280771221", "Bentley Continental GT"},
            {"Nimbus", "Nimbus"},
            {"3990109732", "Maseratti GT"},
            {"1067067984", "Mercedes-Benz G63 6x6"},
            {"Policeb", "Police Bike"},
            {"1636930844", "Gurkha"},
            {"Riot", "Riot"},
            {"Sheriff2", "Sherif Car"},
            {"2758198655", "Bugatti Chiron"},
            {"3329187363", "Ferrari 458"},
            {"2318674365", "Ford GT"},
            {"4273849759", "Lamborghini Reventon"},
            {"2882719101", "Mercedes-Benz V250"},
            {"Ambulance", "Ambulance"},
            {"3718673910", "Ford Explorer"},
            {"2951793544", "Jeep DemonHawk"},
            {"1021110150", "Mercedes-Benz AMG GT"},
            {"1075136237", "Lexus GS350"},
            {"3537728814", "Ferrari 458"},
            {"146326091", "Lamborghini Centenario"},
            {"Burrito3", "Van"},
            {"885421525", "Dodge Challenger Demon"},
            {"1035045347", "Cadillac Escalade"},
            {"2407168974", "Jeep Wrangler"},
            {"2190216177", "Lincoln Navigator"},
            {"1894775923", "Ford Mustang GT"},
            {"3144451903", "Chevrolet Corvette"},
            {"178350184", "Mercedes-Benz G65"},
			{"swinger", "Swinger"},
            {"1937686957", "Mercedes-Benz GLS"},
            {"3504245224", "Mercedes-Benz W140"},
            {"1769548661", "Mercedes-Benz C63W205"},
            {"2900664462", "Mercedes-Benz AMG GT R"},
            {"2883413658", "ferrari F12 Berlinetta"},
            {"110033087", "Ferrari F8 Tributto"},
            {"3403558667", "Bugatti Chiron SuperSport"},
            {"3774753162", "Lamborghini Urus"},
            {"1742020875", "Lamborghini Reventon"},
            {"Annihilator", "Annihilator"},
            {"Barracks", "Barracks"},
            {"BARRAGE", "BARRAGE"},
            {"1542143200", "Scarab"},
            {"Insurgent2", "Insurgent"},
            {"HalfTrack", "HalfTrack"},
            {"Kuruma2", "Kuruma"},
            {"Insurgent3", "Insurgent Gun"},
            {"NightShark", "NightShark"},
            {"Rhino", "Tank"},
            {"KHANJALI", "New Tank"},
            {"Apc", "BTR"},
            {"457850242", "Mercedes-Benz Brabus 700"},
            {"2890648288", "BMW M8"},
            {"1969115674", "Bmw E60"},
            {"2237496584", "BMW M2"},
            {"1093697054", "BMW M5 F90"},
            {"1896411446", "M3 E46"},
            {"Rumpo", "News Van"},
            {"1576185249", "BMW X5M"},
            {"3263286761", "Range Rover SVR"},
            {"Insurgent", "Insurgent Gun"},
            {"Phantom2", "Phantom"},
            {"3801556919", "Ford Raptor"},
            {"Hauler2", "Hauler"},
            {"3632063247", "Mercedes-Benz E63"},
            {"1813965170", "Audi RS7"},
            {"Shotaro", "Shotaro"},
            {"4203417990", "Mercedes-Benz S63"},
            {"4220940234", "News Van"},
            {"2245719813", "Police Bike"},
            {"4098721392", "Infiniti FX50"},
            {"2811753682", "Toyota Camry"},
            {"381357986", "Lexus LX570"},
			{"lx2018", "Lexus LX570"},
            {"689090322", "Lexus LC500"},
            {"128072929", "Nissan GTR Nismo"},
            {"j50", "Ferrari J50"},
            {"ghost", "Rolls Royce Ghost"},
            {"mig", "Ferrari Enzo"},
            {"senna", "McLaren Senna"},
            {"17m760i", "BMW 760i"},
            {"19dbs", "Aston Martin DBS"},
            {"ast", "Aston Martin Vanquish"},
            {"vantage", "Aston Martin Vantage"},
            {"db11", "Aston Martin DB11"},
            {"aventadorishe", "Lamborghini Aventador ONYX"},
            {"manspanam", "Panamera Sport Turismo Mansory"},
			{"toros", "Toros"},
            {"clssuniversal", "Mercedes-Benz CLS"},
            {"monza", "Ferrari Monza SP2"},
            {"850", "BMW e31 850i"},
            {"m3e92", "BMW M3 Coupe E92"},
            {"lp610", "Lamborghini Huracan Coupe"},
			{"rmodx6police", "BMW X6M Police"},
			{"rmodskyline34", "Nissan Skyline r34"},
			{"lb750sv", "Lamborghini Aventador Liberty Walk"},
			{"rmodx6", "BMW X6M"},
			{"rmodspeed", "Maclaren Speedtail"},
			{"rmodzl1", "Chevrolet Camarro Zl1 Widebody"},
			{"techart17", "Porsche Panamera Turbo Techart"},
			{"3898874505", "Porsche Panamera Turbo Techart"},
			{"29976887", "Ferrari F12"},
			{"2196811320", "Jeep Trackhawk"},
			{"3544899937", "Bentley Bentayga"},
			{"3288565317", "Rolls-Royce Phantom"},
			{"2924855946", "McLaren 720S"},
			{"3865192449", "Tesla Model X"},
			{"slamvan3", "Slamvan"},
			{"virgo3", "Virgo"},
			{"patriot", "Patriot"},
			{"2760888186", "Cadillac Fleetwood"},	
			{"stafford", "Stafford"},	
			{"cavalcadev", "Cavalcadev"},
			{"1341031731", "Toyota Land Cruiser 200"},
			{"838150527", "Ford Explorer"},
			{"831758577", "Lexus GS 350"},
			{"15164328", "Ford Mustang"},
			{"545242128", "Ford Crown Vik"},
			{"3692922108", "Jeep SRT-8"},
			{"ram1500", "Dodge Ram"},
			{"760m", "BMW M760 Manhart"},
			{"s15mak", "Nissan Silvia"},
			{"fpacehm", "Jaguar F-pace Hamann"},
			{"i8", "BMW I8"},
			{"mb300sl", "Mercedes Benz 300sl"},
			{"lfa10", "Lexus LFA"},
			{"rmodgtr50", "Nissan GTR 50 Universary edition"},
			{"16charger", "Dodge Charger Demon 2019 "},
			{"c7r", "Chevrollet Corvette Truck"},
			{"cats", "Cadillac ATSV"},
			{"focusrs", "Ford Focus RS"},
			{"chall70", "Dodge Challenger 1970"},
			{"ddgp20", "Ducati Desmosedici GP20"},
			{"flhxs_streetglide_special18", "Harley-Davidson FLHXS"},
			{"63lb", "Volkswagen Golf R"},
			{"cls2015", "Mercedes Benz CLS63 AMG"},			
			{"s30", "Nissan Fairlady"},
			{"z2879", "Cevrolet Camaro Zl28"},
			{"subwrx", "Subaru WRX STI"},
			{"fd", "Mazda RX7"},
			{"a8audi", "AUDI A8"},
			{"fk8", "Honda Civic Type R"},
			{"brz13", "Subaru BRZ 2013"},
			{"bmwe39", "BMW M5 E39"},
			{"amggtbs", "Mercedes-Benz AMG GT BS"},
			{"e15082", "Ford E-150 Van 1982"},
			{"nspeedo", "Vapid Speedo Express"},
			{"mule3", "Mule 3"},
			{"mule4", "Mule 4"},
			{"pounder2", "Pounder"},
			{"boxville4", "Boxville"},			
			{"hvrod", "Harley V Rod"},
			{"rc", "KRM RC 390"},
			{"z1000", "Kawasaki Z1000"},			
			{"722s", "Mercedes Benz SLR McLaren"},
			{"mlnovitec", "Maserati Lavente Novitec"},
			{"e400", "Mercedes Benz E400 C238"},
			{"qx80", "Infiniti QX80"},
			{"gdaq50", "Infiniti Q50"},
			{"pts21", "Porsche 911 Turbo S 2021"},
			{"audiq7", "Audi Q7 TFSI"},
			{"rmodbacalar", "Bentley Mulliner Bacalar"},
			{"rmodf12gtk", "Ferrari F12 TDF"},	
			{"dbx", "Aston Martin DBX"},
			{"bugatti", "Bugatti Veyron Exclusive"},
			{"m6f13", "BMW M6 Coupe"},
			{"c63w205", "Mercedes-Benz AMG C63s"},
			{"sprinter211", "Mercedes-Benz Sprinter"},
			{"699188170", "McLaren P1 Gruppe6"},
			{"59561272", "MB GT63S Gruppe6"},
			{"577497474", "BMW M5 E39"},
			{"2258467760", "Dodge Ram"},
			{"194366558", "Porsche Panamera Turbo"},
			{"836213613", "Lamborghini Huracan Performante"},
			{"3619375046", "Lamborghini SVJ"},
			{"monster8", "BIGDICK"},
            {"rmodlp570", "Lamborghini Gallardo Exclusive"},
			{"g770", "Mercedes G-Class G770"},
			{"gtrsilhouette", "Nissan GTR R35 Liberty Walk"},

                { "buzzard2", "Buzzard"},
                { "frogger", "Frogger"},
                { "frogger2", "Frogger 2"},
                { "havok", "Havok"},
                { "maverick", "Maverick"},
                { "seasparrow", "Seasparrow"},
                { "supervolito", "Supervolito"},
                { "supervolito2", "Supervolito 2"},
                { "swift", "Swift"},
                { "swift2", "Swift 2"},
                { "volatus", "Volatus"},
                { "microlight", "Microlight"},
                { "howard", "Howard"},
                { "seasparrow2", "Seasparrow"},
                { "verus", "Verus"},
                { "italirsx", "Grotti ItaliRSX"},
                { "weevil", "Weevil"},
                { "hondansx", "1992 Honda NSX"},
                { "bmwm4", "BMW M4 2021"},
                { "flatbed", "Flad bed"},
				
				{ "812mansory", "Ferrari 812 Mansory"},
				{ "4034777830", "Ferrari 812 Mansory"},
				{ "2018brabuss63", "M-B S63 Brabus 850"},
				{ "alpinab7", "BMW Alpina B7 2020"},
				{ "lx570es", "Lexus LX570ES"},
				{ "walds63", "M-B S63 Coupe Wald"},
				{ "rs6m", "Audi RS6 Mansory"},
				{ "sclkuz", "Toyota Land Cruiser 200"},
				{ "xc90", "Volvo XC90"},
				{ "scaldarsi", "Mercedes-Benz Emperor"},
        };
		

        public static string GetNormalName(string model)
        {
            if (!ModelList.ContainsKey(model))
            {
                return model;
            }
            else
            {
                return ModelList[model];
            }
        }

        public static List<Vector3> ParkList = new List<Vector3>()
        {
            new Vector3(24.95, -1060.78, 38.15), // 1
            new Vector3(23.96, -1063.61, 38.15), // 2
            new Vector3(22.90, -1066.22, 38.15), // 3
            new Vector3(21.75, -1068.68, 38.15), // 4
        };
        public static void BuyParkPlace(Player player)
        {
            var costcar = 5;
            if (Main.Players[player].Money < costcar)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств, цена: [{costcar}$]", 3000);
                return;
            }

            if (Houses.HouseManager.GetHouse(player, true) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас уже есть дом!", 3000);
                return;
            }
            if (Houses.HouseManager.GetApart(player, true) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас уже есть квартира!", 3000);
                return;
            }
            var targetVehicles = VehicleManager.getAllPlayerVehicles(player.Name.ToString());
            var vehicle = "";
            foreach (var num in targetVehicles)
            {
                vehicle = num;
                break;
            }
            if (vehicle == "" || vehicle == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет машины", 3000);
                return;
            }
            if (FineManager.GetHaveFine(vehicle, player.Name.ToString()) == 10)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.CenterRight, "У вашего транспорта 6 штрафов!", 2000);
                return;
            }
            foreach (var v in NAPI.Pools.GetAllVehicles())
            {
                if (v.HasData("ACCESS") && (v.GetData<string>("ACCESS") == "PERSONAL" || v.GetData<string>("ACCESS") == "INPARK") && NAPI.Vehicle.GetVehicleNumberPlate(v) == vehicle)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Машина была уже вызвана ранее!", 3000);
                    return;
                }
            }
            MoneySystem.Wallet.Change(player, -costcar);
            SetCarInFreeParkPlace(player, vehicle);
        }

        public static string FindFirstCarNum(Player player)
        {
            var targetVehicles = VehicleManager.getAllPlayerVehicles(player.Name.ToString());
            var vehicle = "";
            foreach (string num in targetVehicles)
            {
                vehicle = num;
                break;
            }
            return vehicle;
        }
        public static void interactionPressed(Player player, int id)
        {
            try
            {
                switch (id)
                {
                    case 52:
                        BuyParkPlace(player);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"PARK_INTERACTION\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private static Dictionary<int, List<Vector3>> BizVector = new Dictionary<int, List<Vector3>>
        {


            {291, new List<Vector3> { new Vector3(-899.6517, -232.3591, 38.78878), new Vector3(-899.6517, -232.3591, 38.78878), new Vector3(-899.6517, -232.3591, 38.78878), new Vector3(-899.6517, -232.3591, 38.78878) }  },
            {292, new List<Vector3> { new Vector3(-809.2053, -227.3651, 37.20964), new Vector3(-809.2053, -227.3651, 37.20964), new Vector3(-809.2053, -227.3651, 37.20964), new Vector3(-809.2053, -227.3651, 37.20964) }  },
            {293, new List<Vector3> { new Vector3(-37.76995, -1101.887, 25.30233), new Vector3(-37.76995, -1101.887, 25.30233), new Vector3(-37.76995, -1101.887, 25.30233), new Vector3(-37.76995, -1101.887, 25.30233) }  },
            {294, new List<Vector3> { new Vector3(264.9019, -1159.411, 28.10543), new Vector3(264.9019, -1159.411, 28.10543), new Vector3(264.9019, -1159.411, 28.10543), new Vector3(264.9019, -1159.411, 28.10543) }  },
            {318, new List<Vector3> { new Vector3(-45.02219, -1098.849, 25.30233), new Vector3(-45.02219, -1098.849, 25.30233), new Vector3(-45.02219, -1098.849, 25.30233), new Vector3(-45.02219, -1098.849, 25.30233) }  },


        };

        private static Dictionary<int, List<Vector3>> BizAngle = new Dictionary<int, List<Vector3>>
        {
            {291, new List<Vector3> { new Vector3(0, 0, 150.1884), new Vector3(0, 0, 150.1884), new Vector3(0, 0, 150.1884), new Vector3(0, 0, 150.1884) }  },
            {292, new List<Vector3> { new Vector3(0.56777227, 2.295356, 27.989887), new Vector3(0.56777227, 2.295356, 27.989887), new Vector3(0.56777227, 2.295356, 27.989887), new Vector3(0.56777227, 2.295356, 27.989887) }  },
            {293, new List<Vector3> { new Vector3(0, 0, 336.5976), new Vector3(0, 0, 336.5976), new Vector3(0, 0, 336.5976), new Vector3(0, 0, 336.5976) }  },
            {294, new List<Vector3> { new Vector3(0, 0, 88.00644), new Vector3(0, 0, 88.00644), new Vector3(0, 0, 88.00644), new Vector3(0, 0, 88.00644) }  },
            {318, new List<Vector3> { new Vector3(0, 0, 277.7001), new Vector3(0, 0, 277.7001), new Vector3(0, 0, 277.7001), new Vector3(0, 0, 277.7001) }  },

        };

        public static void SpawnCarOnAuto(Player player, int ids, string number)
        {
            var table = BizVector[ids];
            var table2 = BizAngle[ids];

            var rnd = new Random();
            var count = rnd.Next(1, table.Count);
            var vehdata = VehicleManager.Vehicles[number];
            VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(vehdata.Model);
            var veh = NAPI.Vehicle.CreateVehicle(vh, table[count], table2[count], 0, 0);

            var house = Houses.HouseManager.GetHouse(player, true);

            var apartament = Houses.HouseManager.GetApart(player, true);

            if (house == null)
            {
                if (apartament != null)
                {
                    house = apartament;
                }
            }

            if (house != null)
            {
                if (house.GarageID != 0)
                {
                    Houses.Garage Garage = Houses.GarageManager.Garages[house.GarageID];
                    Garage.SetOutVehicle(number, veh);
                }
            }

            
            VehicleStreaming.SetLockStatus(veh, true);
            vehdata.Holder = player.Name;
            veh.SetData("ACCESS", "PERSONAL");
            veh.SetData("ITEMS", vehdata.Items);
            veh.SetData("OWNER", player);
            veh.SetSharedData("PETROL", vehdata.Fuel);

            VehicleStreaming.SetEngineState(veh, true);

            NAPI.Vehicle.SetVehicleNumberPlate(veh, number);
            VehicleManager.ApplyCustomization(veh);

        }
        public static void SetCarInFreeParkPlace(Player player, string number)
        {
            var rnd = new Random();
            var id = rnd.Next(1, ParkList.Count);
            var vehdata = VehicleManager.Vehicles[number];
            VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(vehdata.Model);
            var veh = NAPI.Vehicle.CreateVehicle(vh, ParkList[id], new Vector3(0, 0, 70), 0, 0);

            VehicleStreaming.SetEngineState(veh, false);
            VehicleStreaming.SetLockStatus(veh, true);
            vehdata.Holder = player.Name;
            veh.SetData("ACCESS", "PERSONAL");
            veh.SetData("ITEMS", vehdata.Items);
            veh.SetData("OWNER", player);
            veh.SetSharedData("PETROL", vehdata.Fuel);
            NAPI.Vehicle.SetVehicleNumberPlate(veh, number);
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Парковочное место: №{id}", 3000);
            VehicleManager.ApplyCustomization(veh);

        }

        internal class ParkBuy
        {
            public Vector3 Position { get; }

            [JsonIgnore]
            private Blip blip = null;
            [JsonIgnore]
            private ColShape shape = null;
            [JsonIgnore]
            private TextLabel label = null;
            [JsonIgnore]
            private Marker marker = null;

            public ParkBuy(Vector3 pos)
            {
                Position = pos;
                blip = NAPI.Blip.CreateBlip(255, pos, 1, 4, "Парковка", 225, 0, true);
                shape = NAPI.ColShape.CreateCylinderColShape(pos, 2f, 3, 0);
                shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 52);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                label = NAPI.TextLabel.CreateTextLabel("~b~Cтоянка", new Vector3(pos.X, pos.Y, pos.Z), 20F, 0.5F, 0, new Color(255, 255, 255), true, 0);
                marker = NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 1f), new Vector3(), new Vector3(), 1f, new Color(0, 175, 250, 220), false, 0);
            }
        }

        public static void OpenMenu(Player player)
        {
            Menu menu = new Menu("parkcars", false, false);
            menu.Callback = callback_cars;
            menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Парковка";
            menu.Add(menuItem);

            foreach (var v in VehicleManager.getAllPlayerVehicles(player.Name))
            {
                menuItem = new Menu.Item(v, Menu.MenuItem.Button);
                menuItem.Text = $"{ParkManager.GetNormalName(VehicleManager.Vehicles[v].Model)} <br> Номер: {v} <br> Пробег {Convert.ToInt32( VehicleManager.Vehicles[v].Sell)}";
                menu.Add(menuItem);
                break;
            }

            // menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            // menuItem.Text = "Закрыть";
            // menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn); // полоска закрытия
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
        }

        private static void callback_cars(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    MenuManager.Close(player);
                    if (item.ID == "back")
                    {
                        MenuManager.Close(player);
                        Main.OpenPlayerMenu(player).Wait();
                        return;
                    }
                    OpenSelectedCarMenu(player, item.ID);
                }
                catch (Exception e) { Log.Write("callback_cars: " + e.Message + e.Message, nLog.Type.Error); }
            });
        }

        public static void OpenSelectedCarMenu(Player player, string number)
        {
            Menu menu = new Menu("selectedcar", false, false);
            menu.Callback = callback_selectedcar;
            menu.SetBackGround("../images/phone/pages/gps.png");

            var vData = VehicleManager.Vehicles[number];

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = number;
            menu.Add(menuItem);

            menuItem = new Menu.Item("model", Menu.MenuItem.Card);
            menuItem.Text = ParkManager.GetNormalName(vData.Model);
            menu.Add(menuItem);

            var vClass = NAPI.Vehicle.GetVehicleClass(NAPI.Util.VehicleNameToModel(vData.Model));

            menuItem = new Menu.Item("repair", Menu.MenuItem.Button);
            menuItem.Text = $"Восстановить {VehicleManager.VehicleRepairPrice[vClass]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("key", Menu.MenuItem.Button);
            menuItem.Text = $"Получить дубликат ключа";
            menu.Add(menuItem);

            menuItem = new Menu.Item("changekey", Menu.MenuItem.Button);
            menuItem.Text = $"Сменить замки";
            menu.Add(menuItem);

            menuItem = new Menu.Item("evac", Menu.MenuItem.Button);
            menuItem.Text = $"Эвакуировать машину";
            menu.Add(menuItem);


            var price = BCore.GetVipCost(player, BCore.CostForCar(vData.Model));

            menuItem = new Menu.Item("sell", Menu.MenuItem.Button);
            menuItem.Text = $"Продать ({price}$)";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_selectedcar(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            MenuManager.Close(player);
            switch (item.ID)
            {
                case "sell":
                    player.SetData("CARSELLGOV", menu.Items[0].Text);
                    VehicleManager.VehicleData vData = VehicleManager.Vehicles[menu.Items[0].Text];
                    var price = BCore.GetVipCost(player, BCore.CostForCar(vData.Model));

                    MenuManager.Close(player);
                    Trigger.PlayerEvent(player, "openDialog", "CAR_SELL_TOGOV", $"Вы действительно хотите продать государству {GetNormalName(vData.Model)} ({menu.Items[0].Text}) за ${price}?");
                    return;
                case "repair":
                    vData = VehicleManager.Vehicles[menu.Items[0].Text];
                    if (vData.Health > 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Машина не нуждается в восстановлении", 3000);
                        return;
                    }

                    var vClass = NAPI.Vehicle.GetVehicleClass(NAPI.Util.VehicleNameToModel(vData.Model));
                    if (!MoneySystem.Wallet.Change(player, -VehicleManager.VehicleRepairPrice[vClass]))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно средств", 3000);
                        return;
                    }
                    vData.Items = new List<nItem>();
                    GameLog.Money($"player({Main.Players[player].UUID})", $"server", VehicleManager.VehicleRepairPrice[vClass], $"carRepair({vData.Model})");
                    vData.Health = 1000;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы восстановили {ParkManager.GetNormalName(vData.Model)} ({menu.Items[0].Text})", 3000);
                    return;
                case "evac":
                    if (!Main.Players.ContainsKey(player)) return;

                    var number = menu.Items[0].Text;

                    if (Main.Players[player].Money < 15)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств (не хватает {15 - Main.Players[player].Money}$)", 3000);
                        return;
                    }

                    

                    var targetVehicles = VehicleManager.getAllPlayerVehicles(player.Name.ToString());
                    var vehicle = "";
                    foreach (var num in targetVehicles)
                    {
                        vehicle = num;
                        break;
                    }



                    foreach (var v in NAPI.Pools.GetAllVehicles())
                    {
                        if (v.HasData("ACCESS") && (v.GetData<string>("ACCESS") == "PERSONAL" || v.GetData<string>("ACCESS") == "INPARK") && v.NumberPlate == vehicle)
                        {

                            var veh = v;
                            if (veh == null) return;
                            if (veh.HasData("PARKCLASS"))
                                veh.GetData<SellCars.VehicleForSell>("PARKCLASS").Destroy(false, false);
                            VehicleManager.Vehicles[number].Fuel = (!veh.HasSharedData("PETROL")) ? VehicleManager.VehicleTank[veh.Class] : veh.GetSharedData<int>("PETROL");
                            NAPI.Entity.DeleteEntity(veh);

                            MoneySystem.Wallet.Change(player, -15);
                            GameLog.Money($"player({Main.Players[player].UUID})", $"server", 15, $"carEvac");
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваша машина была отогнана на стоянку", 3000);
                            break;
                        }
                    }
                    return;
                case "key":
                    if (!Main.Players.ContainsKey(player)) return;

                    var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.CarKey));
                    if (tryAdd == -1 || tryAdd > 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                        return;
                    }

                    nInventory.Add(player, new nItem(ItemType.CarKey, 1, $"{menu.Items[0].Text}_{VehicleManager.Vehicles[menu.Items[0].Text].KeyNum}"));
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили ключ от машины с номером {menu.Items[0].Text}", 3000);
                    return;
                case "changekey":
                    if (!Main.Players.ContainsKey(player)) return;

                    if (!MoneySystem.Wallet.Change(player, -100))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Смена замков стоит $100", 3000);
                        return;
                    }

                    VehicleManager.Vehicles[menu.Items[0].Text].KeyNum++;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы сменили замки на машине {menu.Items[0].Text}. Теперь старые ключи не могут быть использованы", 3000);
                    return;
                case "close":
                    OpenMenu(player);
                    return;
            }
        }

    }
}
