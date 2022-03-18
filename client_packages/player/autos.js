//------------------------------------
const AUTO_NAMES = {    
"cullinan": "Royce Rolls Cullinan",
"subwrx": "Subaru WRX",
"s63w222": "Mercedes-Benz AMG S63 W222",
"m5comp": "BMW M5 Competition",
"cybertruck": "Tesla Cybertruck",
"lc200": "Land Cruiser 200",
"lex570": "Lexus LX570",
"bmwx5mc": "BMW X5M Competition",
"q820": "Audi Q8 2020",
"g65go": "Mercedes-Benz G63 AMG",
"p1": "McLaren P1",
"rs6avant20": "Audi RS6 AVANT",
"qx80": "Infinity QX80",
"agerars": "Koenigsegg Agera",
"amggt63s": "Mercedes-Benz AMG GT63S",
"2016rs7": "Audi RS7",
"720s": "McLaren 720S",
"teslax": "Tesla Model X",
"e63sf": "Mercedes-Benz E63S AMG Restyling",
"camry18": "Toyota Camry XSE 2018",
"asvj": "Lamborghini SVJ",
"chiron": "Bugatti Chiron",
"e36rb": "BMW E36 DRIFT",
"MGT": "Ford Mustang GT",
"gle63": "Mercedes-Benz GLE63",
"m516": "BMW M5 F10",
"w447": "Mercedes-Benz V-Class",
"w210": "Mercedes-Benz W210",
"w140s600": "Mercedes-Benz S600 W140",
"volvo850": "Volvo 850",
"vip8": "Dodge Viper",
"vaz2121": "Niva Urban",
"taycan": "Porsche Taycan",
"skyline": "Nissan Skyline",
"shelbygt500": "Dodge Shelby GT",
"s5audi": "Audi S5",
"s3audi": "Audi S3",
"rsvr16": "Range Rover SVR",
"rs5audi": "Audi RS5",
"rav4": "Toyota Rav 4",
"r8v10": "Audi R8 V10",
"panamera_st": "Porsche Panamera ST",
"octaviavrs": "Skoda Octavia VRS",
"mazda3mps": "Mazda 3MPS",
"m750li": "BMW 750LI",
"lx470": "Lexus LX470",
"lancer9": "Mitsubishi Lancer 9",
"lada2107": "VAZ-2107",
"kiastinger": "Kia Stinger",
"jeepsrt": "Jeep SRT",
"ipace": "Jaguar I-PACE ",
"impala96": "Impala 1996",
"hellcat": "Dodge Hellcat",
"gtr": "Nissan GTR",
"golfgti": "Volkswagen Golf GTI",
"gaz2410": "VAZ-2410",
"g65": "Mercedes-Benz G65 AMG",
"g30": "BMW G30",
"fordraptor": "Ford Raptor",
"focus2003": "Ford Focus 2003",
"f812": "Ferrari F812",
"e46": "BMW M3 E46",
"divo": "Bugatti Divo",
"18performante": "Lamborghini Performante",
"evo10": "Mitsubishi Evo 10",
"escalade": "Cadillac Escalade",
"e39m5": "BMW M5 E39",
"e39bmw": "BMW E39",
"e39alpina": "BMW E39 ALPINA",
"e200": "Mercedes-Benz E200",
"cooperworks": "Mini Couper",
"camryxv55": "Camry XV55",
"c63s": "Mercedes-Benz C63S AMG",
"bmwz4": "BMW Z4",
"bmwm8": "BMW M8 Competition",
"bmwe60": "BMW M5 E60",
"bmwe38": "BMW E38",
"bmwe34": "BMW E34",
"apriora": "Lada Priora",
"ae86": "Toyota AE86",
"a8audi": "Audi A8",
"922smg": "Porsche 911",
"500w124": "Mercedes-Benz W124",
"rrwald": "Royce-Rolls RR WALD",
"audir8lb": "Audi R8 V10 Liberty Walk",
"camry18": "Toyota Camry XSE 2018",
"demonhawk": "Jeep SRT-8 Demonhawk",
"gle": "Mercedes-Benz GLE",
"kiagt": "KIA Stinger GT",
"lc200": "Toyota Land Cruiser 200",
"m4f82": "BMW M4 F82",
"m4lb": "BMW M4 F82 Liberty Walk",
"panamera17turbo": "Porsche Panamera Turbo",
"x5m2016": "BMW X5M F85",
"rs62": "Audi RS6",
"c8": "Chevrolet Corvette",
"jp12": "Jeep Wrangler Rubicon",
"lanex400": "Mitsubishi Lancer X",
"lp570": "Lamborghini Gallardo",
"superb": "Skoda Superb",
"zim": "Gaz Zim",
"chironsport110": "Bugatti Chiron",
"amggtr": "Mercedes-Benz AMG GT R",
"ody18": "Honda Odyssey",
"amggt63s": "Mercedes-Benz AMG GT63S",
"as350": "Helicopter | Police",
"rmodf12tdf": "Ferrari F12 TDF",
"fbi458": "Ferrari 458 | FBI",
"fbiamggt": "Mercedes-Benz AMG GT | FBI",
"fbigs350": "Lexus GS350 | FBI",
"fbilp770": "Lamborghini Centenario | FBI",
"fc13": "Ferrari California",
"gtc4lusso": "Ferrari GTC4 Lusso",
"gurkha": "Bronevik | Police",
"jes": "Koenigsegg Jesko",
"laferrari": "Ferrari LaFerrari",
"newsvan": "News Van | News",
"pistaspider19": "Ferrari 488Pista Spider",
"pullman": "Mercedes-Benz Pullman",
"qrv": "Ford Explorer | EMS",
"gls": "Mercedes-Benz GLS",
"2018s63": "Mercedes-Benz S63 AMG",
"rmodrs7": "Audi RS7 Sportback 2010",
"2019m5": "BMW M5 F90 Competition",
"19g63": "Mercedes-Benz G63 AMG",
"19s63": "Mercedes-Benz s63 AMG Coupe",
"lamboreventon": "Lamborghini Reventon",
"divo": "Bugatti Divo",
"rrst": "Range Rover Hamman",
"xc60": "Volvo XC60",
"oka": "Oka",
"z4bmw": "BMW Z4",
"viper": "Dodge Viper",
"v250": "Mercedes-Benz V-Classe",
"velar": "Range Rover Velar",
"rs318": "Audi RS3 Sportback",
"lc500": "Lexus LC 500",
"rs5r": "Audi RS5-R",
"sq72016": "Audi SQ7",
"nismo20": "Nissan GTR Nismo 2020",
"911turbos": "Porsche 911 Turbo S",
"g65": "Mercedes-Benz G65",
"675ltsp": "McLaren 675LT Spider",
"bnt2018": "Bentley Continental GT",
"ferrari812": "Ferrari 812 Superfast",
"svj63": "Lamborghini Aventador SVJ",
"supersport": "Bugatti Veyron",
"bugatticentodieci": "Bugatti Centodieci 2020",
"600lt": "McLaren 600LT",

"fleet78": "Cadillac Fleetwood",
"fairlane66": "Ford Fairlane",
"brabus800": "Mercedes Benz E63 Brabus 800",
"bg700w": "Mercedes Benz G63 Brabus 700",
"812nlargo": "Ferrari 812 Nlargo",
"16charger": "Dodge Charger",
"evo9": "Mitsubishi EVO 9",
"clssuniversal": "Mercedes-Benz CLS",

//мотосалон
"Bmx": "BMX",
"Faggio2": "Faggio",
"Blazer": "Blazer",
"Enduro": "Enduro",
"Thrust": "Thrust",
"PCJ": "PCJ",
"Hexer": "Hexer",
"lectro": "Lectro",
"Nemesis": "Nemesis",
"Scorcher": "Scorcher",
"Double": "Double",
"Diablous": "Diablous",
"Cliffhanger": "Cliffhanger",
"Nightblade": "Nightblade",
"Vindicator": "Vindicator",
"Gargoyle": "Gargoyle",
"Sanchez2": "Sanchez",
"Akuma": "Akuma",
"Ratbike": "Ratbike",
"CarbonRS": "Carbon RS",
"Ruffian": "Ruffian",
"Hakuchou": "Hakuchou",
"Bati": "Bati",
"BF400": "BF400",
"Sanctus": "Sanctus",

"h2carb": "Kawasaki H2",
"r6": "Yamaha R6",
"gsx1000": "Suzuki GSX1000",
"cbr1000rrr": "Honda CBR1000RR",
"goldwing": "GoldWing",
"cb500x": "Honda CB500x",
"rmz2": "Suzuki RMZ2",

"shatoro": "Shatoro", //мастак

"xxxxx": "Mercedes-Benz X Classe 6X6", // админ авто

"j50": "Ferrari J50",
"ghost": "Rolls Royce Ghost",
"mig": "Ferrari Enzo",
"senna": "McLaren Senna",
"17m760i": "BMW 760i",
"19dbs": "Aston Martin DBS",
"ast": "Aston Martin Vanquish",
"vantage": "Aston Martin Vantage",
"db11": "Aston Martin DB11",
"aventadorishe": "Lamborghini Aventador ONYX",

"850": "BMW 850 SCI 1989",
"clssuniversal": "Mercedes-Benz CLS",
"lp610": "Lamborghini Huracan",
"m3e92": "BMW M3 e92",
"monza": "Ferrari Monza SP2",
"manspanam": "Panamera Sport Turismo Mansory",
"toros": "Toros",
}

//------------------------------------
exports.get = function (value) { return AUTO_NAMES[value] }

