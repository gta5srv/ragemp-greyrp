using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;


namespace NeptuneEVO.Jobs
{
    class Snow : Script
    {
        #region Список точек
        private static List<List<Vector3>> Points = new List<List<Vector3>> 
        {
            new List<Vector3> {
                new Vector3(940.2075, -1576.227, 30.53987),
                new Vector3(961.5473, -1574.482, 30.73298),
                new Vector3(970.5746, -1551.013, 30.80326),
                new Vector3(970.5213, -1513.871, 31.39268),
                new Vector3(978.8889, -1442.011, 31.72912),
                new Vector3(1003.481, -1439.465, 34.64633),
                new Vector3(1085.311, -1438.666, 36.94671),
                new Vector3(1163.941, -1439.363, 34.98264),
                new Vector3(1213.235, -1439.32, 35.36303),
                new Vector3(1250.094, -1411.411, 35.3156),
                new Vector3(1247.075, -1312.122, 35.43439),
                new Vector3(1249.112, -1226.76, 36.09295),
                new Vector3(1224.203, -1141.723, 37.96369),
                new Vector3(1189.375, -1082.065, 40.96814),
                new Vector3(1167.727, -1020.5, 44.41185),
                new Vector3(1162.545, -933.7845, 49.71035),
                new Vector3(1167.447, -870.9209, 54.15639),
                new Vector3(1182.963, -825.8423, 55.44838),
                new Vector3(1197.433, -782.6268, 57.17773),
                new Vector3(1202.068, -736.4277, 58.9453),
                new Vector3(1191.396, -672.0944, 61.45473),
                new Vector3(1184.947, -618.931, 63.57051),
                new Vector3(1180.803, -576.6193, 64.44401),
                new Vector3(1184.276, -523.0067, 65.06449),
                new Vector3(1192.671, -480.6252, 66.0493),
                new Vector3(1202.854, -438.6603, 67.21187),
                new Vector3(1212.315, -397.3044, 68.37818),
                new Vector3(1221.081, -343.3904, 69.2858),
                new Vector3(1213.758, -300.5655, 69.25314),
                new Vector3(1187.792, -264.988, 69.27068),
                new Vector3(1116.757, -233.2609, 69.26207),
                new Vector3(1031.767, -197.5002, 70.4072),
                new Vector3(966.324, -156.062, 74.1302),
                new Vector3(901.4658, -114.6154, 77.78189),
                new Vector3(825.511, -64.76611, 80.80482),
                new Vector3(765.1942, -22.66393, 81.73573),
                new Vector3(687.3978, 25.87376, 84.33729),
                new Vector3(605.1454, 60.03883, 91.80864),
                new Vector3(537.0665, 85.54374, 96.47704),
                new Vector3(450.4143, 114.6944, 99.41656),
                new Vector3(350.2159, 152.2466, 103.1549),
                new Vector3(251.1515, 187.5374, 105.1653),
                new Vector3(133.7246, 226.5495, 107.5039),
                new Vector3(67.31169, 245.797, 109.6276),
                new Vector3(22.75361, 237.0767, 109.7629),
                new Vector3(0.8515143, 182.4224, 100.6391),
                new Vector3(-16.42462, 135.6967, 87.5024),
                new Vector3(-38.92843, 75.21461, 73.68155),
                new Vector3(-48.67918, 34.78558, 72.42419),
                new Vector3(-63.44119, -25.76034, 67.00556),
                new Vector3(-73.87687, -67.92223, 58.93295),
                new Vector3(-90.17159, -131.3936, 57.91852),
                new Vector3(-102.7993, -197.8917, 46.40813),
                new Vector3(-127.7471, -270.1406, 42.7313),
                new Vector3(-145.1067, -321.9762, 37.95957),
                new Vector3(-172.1235, -403.3553, 33.96736),
                new Vector3(-190.5789, -460.2114, 34.39198),
                new Vector3(-227.9592, -560.5991, 34.83481),
                new Vector3(-248.5782, -631.7876, 33.83094),
                new Vector3(-240.8231, -678.3496, 33.45089),
                new Vector3(-191.5083, -689.6443, 34.26049),
                new Vector3(-107.811, -721.5348, 43.79955),
                new Vector3(-47.9397, -751.0898, 44.38918),
                new Vector3(-1.512578, -768.3427, 44.35829),
                new Vector3(13.96485, -787.7453, 44.33543),
                new Vector3(-21.93851, -885.5784, 31.72187),
                new Vector3(-51.26134, -971.3655, 29.51041),
                new Vector3(-71.76173, -1023.947, 28.66594),
                new Vector3(-91.70928, -1094.606, 26.47233),
                new Vector3(-65.69162, -1144.428, 26.02337),
                new Vector3(2.60946, -1139.923, 28.52888),
                new Vector3(56.34553, -1137.994, 29.51431),
                new Vector3(162.8602, -1132.298, 29.45089),
                new Vector3(249.4023, -1133.194, 29.52056),
                new Vector3(335.6749, -1134.386, 29.56452),
                new Vector3(412.2923, -1135.199, 29.53318),
                new Vector3(472.8966, -1134.869, 29.55817),
                new Vector3(499.4197, -1165.409, 29.44086),
                new Vector3(499.2747, -1209.037, 29.49072),
                new Vector3(499.6321, -1274.371, 29.44336),
                new Vector3(519.8317, -1330.46, 29.45086),
                new Vector3(532.7302, -1393.24, 29.42433),
                new Vector3(569.0177, -1440.127, 29.78546),
                new Vector3(657.9079, -1439.821, 30.96626),
                new Vector3(722.3219, -1440.016, 31.72309),
                new Vector3(753.1591, -1440.098, 29.09696),
                new Vector3(801.9296, -1479.951, 27.90471),
                new Vector3(827.3412, -1542.027, 29.63187),
                new Vector3(835.2436, -1618.172, 31.93458),
                new Vector3(827.4241, -1709.506, 29.54053),
                new Vector3(878.008, -1760.129, 30.19876),
                new Vector3(919.8581, -1763.461, 31.10154),
                new Vector3(961.5579, -1752.942, 31.31232),
                new Vector3(965.9769, -1707.446, 30.19526),
                new Vector3(969.4399, -1660.198, 29.4258),
                new Vector3(970.018, -1610.076, 30.28793),
                new Vector3(967.2525, -1580.998, 30.68773),
                new Vector3(941.8093, -1575.066, 30.55312),
                new Vector3(920.9653, -1567.58, 30.89722),
                new Vector3(902.5803, -1562.198, 30.94324),
                new Vector3(912.9099, -1572.189, 30.96255),
            },
            new List<Vector3> {
                new Vector3(887.0807, -1567.691, 29.3722),
                new Vector3(865.7719, -1569.282, 29.05243),
                new Vector3(837.0608, -1597.707, 30.51188),
                new Vector3(834.2056, -1638.776, 28.98984),
                new Vector3(830.6744, -1678.344, 27.91862),
                new Vector3(827.7681, -1708.894, 27.91946),
                new Vector3(818.124, -1756.358, 27.88165),
                new Vector3(808.0995, -1825.835, 27.8499),
                new Vector3(801.3337, -1866.186, 27.79056),
                new Vector3(792.3269, -1906.357, 27.77287),
                new Vector3(778.373, -1972.235, 27.7829),
                new Vector3(771.8088, -2020.079, 27.76898),
                new Vector3(766.6989, -2067.198, 27.93852),
                new Vector3(761.5606, -2116.153, 27.81718),
                new Vector3(745.5297, -2224.592, 27.89556),
                new Vector3(743.0958, -2263.832, 27.83056),
                new Vector3(738.0356, -2321.502, 24.9373),
                new Vector3(736.6786, -2338.937, 23.57142),
                new Vector3(725.6379, -2398.89, 19.28303),
                new Vector3(700.4371, -2457.924, 18.38927),
                new Vector3(629.9198, -2493.998, 15.7423),
                new Vector3(584.0295, -2500.754, 15.30943),
                new Vector3(522.743, -2500.391, 14.37245),
                new Vector3(473.7375, -2500.601, 13.59776),
                new Vector3(412.2916, -2500.366, 11.93176),
                new Vector3(351.1767, -2502.366, 4.746487),
                new Vector3(307.4466, -2514.324, 4.502714),
                new Vector3(264.1718, -2532.694, 4.404329),
                new Vector3(196.4091, -2549.636, 4.471641),
                new Vector3(166.0779, -2572.883, 4.563673),
                new Vector3(124.0228, -2599.499, 4.563031),
                new Vector3(79.1087, -2610.486, 4.560555),
                new Vector3(13.43004, -2615.394, 4.559247),
                new Vector3(-57.85886, -2619.706, 4.554786),
                new Vector3(-115.2225, -2619.899, 4.554548),
                new Vector3(-173.8176, -2619.557, 4.553311),
                new Vector3(-213.5555, -2619.281, 4.610529),
                new Vector3(-279.1451, -2626.085, 4.609381),
                new Vector3(-313.8741, -2657.596, 4.608349),
                new Vector3(-332.5821, -2676.679, 4.607957),
                new Vector3(-357.8807, -2722.104, 4.607317),
                new Vector3(-375.2709, -2754.614, 4.576537),
                new Vector3(-403.5056, -2778.187, 4.560912),
                new Vector3(-393.5061, -2792.485, 4.560662),
                new Vector3(-382.9982, -2776.68, 4.561819),
                new Vector3(-364.9572, -2750.794, 4.607491),
                new Vector3(-350.7378, -2718.448, 4.609422),
                new Vector3(-331.8132, -2681.741, 4.608532),
                new Vector3(-299.8898, -2649.227, 4.606364),
                new Vector3(-252.2948, -2625.125, 4.607021),
                new Vector3(-206.2707, -2625.787, 4.600311),
                new Vector3(-139.6127, -2625.243, 4.55471),
                new Vector3(-86.54079, -2625.131, 4.554893),
                new Vector3(-49.33062, -2625.032, 4.553942),
                new Vector3(-11.20079, -2622.539, 4.554702),
                new Vector3(48.47809, -2616.611, 4.5574),
                new Vector3(87.48285, -2613.255, 4.559465),
                new Vector3(142.6795, -2596.727, 4.567116),
                new Vector3(176.0448, -2572.265, 4.540136),
                new Vector3(175.1445, -2548.976, 4.536872),
                new Vector3(163.2244, -2521.06, 4.54746),
                new Vector3(162.5783, -2472.225, 4.545457),
                new Vector3(165.8789, -2431.966, 4.917114),
                new Vector3(182.2209, -2403.092, 5.061085),
                new Vector3(206.6579, -2392.844, 6.446607),
                new Vector3(238.8617, -2401.986, 5.295457),
                new Vector3(238.6893, -2440.35, 4.721133),
                new Vector3(239.1995, -2473.498, 4.692577),
                new Vector3(249.5735, -2513.896, 4.851901),
                new Vector3(278.4917, -2534.356, 4.405339),
                new Vector3(310.612, -2522.89, 4.438721),
                new Vector3(337.4447, -2492.269, 4.039609),
                new Vector3(337.4476, -2458.957, 4.94466),
                new Vector3(345.1089, -2436.768, 7.211268),
                new Vector3(353.1445, -2411.019, 8.591378),
                new Vector3(356.6428, -2364.247, 8.741517),
                new Vector3(356.4759, -2314.465, 8.755678),
                new Vector3(356.104, -2279.687, 8.757204),
                new Vector3(356.7571, -2244.327, 8.76205),
                new Vector3(368.651, -2199.61, 11.74965),
                new Vector3(397.5655, -2167.11, 14.62978),
                new Vector3(441.795, -2120.302, 18.78035),
                new Vector3(463.0529, -2095.867, 21.03732),
                new Vector3(500.4992, -2062.307, 24.3876),
                new Vector3(561.6235, -2056.698, 27.84196),
                new Vector3(609.9904, -2061.488, 27.79838),
                new Vector3(644.662, -2064.265, 27.81004),
                new Vector3(680.8459, -2067.281, 27.81104),
                new Vector3(718.9908, -2070.633, 27.81498),
                new Vector3(758.0906, -2074.255, 27.82009),
                new Vector3(818.6844, -2079.939, 28.03078),
                new Vector3(859.5555, -2083.817, 28.77306),
                new Vector3(901.172, -2087.994, 29.32116),
                new Vector3(999.913, -2091.563, 29.71506),
                new Vector3(1036.766, -2091.305, 29.7172),
                new Vector3(1059.32, -2114.126, 31.1888),
                new Vector3(1050.577, -2207.449, 28.97688),
                new Vector3(1043.17, -2292.872, 29.05886),
                new Vector3(1035.802, -2370.869, 28.99424),
                new Vector3(1028.839, -2448.708, 26.83446),
                new Vector3(1021.021, -2467.542, 27.10663),
                new Vector3(965.8708, -2462.7, 27.11193),
                new Vector3(899.2825, -2456.087, 27.08805),
                new Vector3(804.1986, -2445.819, 21.13133),
                new Vector3(767.4843, -2436.707, 18.41281),
                new Vector3(761.8854, -2397.32, 19.52128),
                new Vector3(769.0604, -2323.47, 24.85295),
                new Vector3(772.4727, -2275.752, 27.6124),
                new Vector3(778.5095, -2204.624, 27.82203),
                new Vector3(785.7607, -2125.035, 27.80793),
                new Vector3(791.2118, -2062.288, 27.91562),
                new Vector3(799.1421, -1998.701, 27.7697),
                new Vector3(815.9525, -1909.056, 27.80668),
                new Vector3(830.6763, -1838.5, 27.68149),
                new Vector3(840.3563, -1749.157, 28.07862),
                new Vector3(850.9602, -1643.908, 28.63532),
                new Vector3(853.605, -1599.073, 30.46786),
                new Vector3(854.4193, -1574.158, 28.98081),
                new Vector3(871.6581, -1559.069, 29.02634),
                new Vector3(887.0496, -1540.818, 28.95019),
            },
            new List<Vector3> {
                new Vector3(938.4463, -1575.841, 28.91083),
                new Vector3(957.0145, -1575.856, 29.04977),
                new Vector3(971.689, -1554.073, 29.18128),
                new Vector3(970.8644, -1521.968, 29.65043),
                new Vector3(970.9487, -1471.781, 29.78406),
                new Vector3(963.5516, -1438.598, 29.97232),
                new Vector3(899.4991, -1432.572, 29.37287),
                new Vector3(857.2243, -1432.568, 27.31892),
                new Vector3(825.94, -1433.225, 25.8994),
                new Vector3(777.6218, -1433.225, 25.63151),
                new Vector3(726.1058, -1433.387, 30.06049),
                new Vector3(667.5602, -1433.12, 29.48587),
                new Vector3(585.084, -1433.391, 28.39739),
                new Vector3(474.6859, -1423.126, 27.90136),
                new Vector3(430.9247, -1395.014, 27.90208),
                new Vector3(379.5121, -1353.031, 30.22605),
                new Vector3(355.1576, -1332.919, 31.10187),
                new Vector3(317.1015, -1305.928, 30.23624),
                new Vector3(272.3954, -1299.415, 28.0474),
                new Vector3(226.5462, -1301.244, 27.8787),
                new Vector3(201.3123, -1314.329, 27.90921),
                new Vector3(165.3187, -1363.047, 27.81736),
                new Vector3(145.8701, -1375.104, 27.74825),
                new Vector3(118.0974, -1357.589, 27.82645),
                new Vector3(90.99818, -1320.456, 27.81693),
                new Vector3(70.95537, -1285.126, 27.80025),
                new Vector3(71.16583, -1242.698, 27.84747),
                new Vector3(72.13927, -1193.918, 27.85802),
                new Vector3(71.0973, -1161.421, 27.83459),
                new Vector3(73.71476, -1107.749, 27.85068),
                new Vector3(88.40543, -1071.927, 27.83165),
                new Vector3(111.5377, -1017.067, 27.89413),
                new Vector3(125.702, -977.8743, 27.87102),
                new Vector3(139.4995, -939.4691, 28.35768),
                new Vector3(153.446, -900.7554, 28.85455),
                new Vector3(174.8566, -841.5159, 29.66579),
                new Vector3(189.5232, -801.0164, 29.69439),
                new Vector3(212.6815, -738.2974, 32.46308),
                new Vector3(228.3437, -696.5632, 34.71912),
                new Vector3(243.6832, -654.7651, 37.50014),
                new Vector3(268.4904, -593.3826, 41.78082),
                new Vector3(295.5856, -530.5435, 41.74234),
                new Vector3(312.8633, -486.8371, 41.8252),
                new Vector3(344.7506, -432.7841, 43.09628),
                new Vector3(373.4988, -411.8761, 44.47178),
                new Vector3(420.8182, -394.5382, 45.4198),
                new Vector3(445.7181, -370.9171, 45.51099),
                new Vector3(471.2545, -342.9629, 44.94066),
                new Vector3(497.5521, -335.0918, 43.08006),
                new Vector3(550.5613, -359.1595, 42.07784),
                new Vector3(600.6277, -383.2153, 42.15242),
                new Vector3(628.8572, -395.5241, 41.73667),
                new Vector3(673.4412, -395.7625, 40.2478),
                new Vector3(708.1709, -373.8872, 40.27688),
                new Vector3(742.3522, -361.0843, 43.05218),
                new Vector3(780.817, -348.2832, 47.93425),
                new Vector3(823.2664, -339.7703, 54.06274),
                new Vector3(853.6873, -337.9685, 58.07969),
                new Vector3(900.34, -333.2515, 63.22886),
                new Vector3(929.9, -324.7396, 65.16709),
                new Vector3(953.4706, -309.1052, 65.53713),
                new Vector3(972.493, -296.5369, 65.48533),
                new Vector3(1003.779, -314.6549, 65.7183),
                new Vector3(1034.533, -333.3619, 65.65817),
                new Vector3(1058.073, -348.6397, 65.62238),
                new Vector3(1078.719, -378.0627, 65.60764),
                new Vector3(1079.763, -404.8215, 65.60805),
                new Vector3(1075.535, -452.2941, 64.04395),
                new Vector3(1068.26, -484.1122, 62.22166),
                new Vector3(1077.734, -519.8024, 61.29201),
                new Vector3(1117.819, -517.5004, 62.37676),
                new Vector3(1141.414, -515.9824, 63.01764),
                new Vector3(1192.607, -516.5697, 63.48547),
                new Vector3(1216.303, -530.3047, 65.08347),
                new Vector3(1249.431, -547.7139, 67.43841),
                new Vector3(1260.697, -576.07, 67.5951),
                new Vector3(1272.359, -616.6842, 67.54124),
                new Vector3(1289.497, -651.0549, 66.03133),
                new Vector3(1286.485, -696.5284, 63.29662),
                new Vector3(1261.229, -735.7355, 61.46847),
                new Vector3(1235.582, -752.5558, 58.7282),
                new Vector3(1191.776, -778.2889, 55.72094),
                new Vector3(1180.85, -815.5879, 54.23944),
                new Vector3(1160.711, -858.7144, 52.87233),
                new Vector3(1150.954, -921.791, 48.83239),
                new Vector3(1151.636, -976.6368, 44.96133),
                new Vector3(1166.927, -1056.928, 40.7069),
                new Vector3(1183.092, -1094.476, 38.50784),
                new Vector3(1224.165, -1169.979, 35.34172),
                new Vector3(1238.537, -1234.558, 33.9603),
                new Vector3(1238.1, -1279.971, 33.48662),
                new Vector3(1226.137, -1354.011, 33.66095),
                new Vector3(1221.452, -1411.843, 33.52414),
                new Vector3(1191.998, -1423.709, 33.75322),
                new Vector3(1128.676, -1432.372, 33.56312),
                new Vector3(1061.758, -1433.323, 35.32639),
                new Vector3(973.1721, -1434.208, 29.99296),
                new Vector3(966.4474, -1460.6, 29.81679),
                new Vector3(966.9537, -1522.04, 29.61254),
                new Vector3(960.0442, -1574.23, 29.11692),
                new Vector3(935.5973, -1576.798, 28.90383),
                new Vector3(916.4827, -1578.855, 29.17359),
            }


        };
        #endregion

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStartHandler()
        {
            try
            {

                for (int i = 0; i < Points.Count; i++)
                {
                    for (int d = 0; d < Points[i].Count; d++)
                    {
                        ColShape shap = NAPI.ColShape.CreateCylinderColShape(Points[i][d], 5F, 10, 0);
                        shap.OnEntityEnterColShape += snowCheckpointEnterWay;
                        shap.SetData("NUMBER", d);
                        shap.SetData("WAY", i);
                    }
                }


                ColShape Shape = NAPI.ColShape.CreateCylinderColShape(new Vector3(906.3733, -1516.33, 29.29401), 1F, 2F);
                NAPI.Marker.CreateMarker(27, new Vector3(906.3733, -1516.33, 29.29401) + new Vector3(0, 0, 0.2f), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220), false, 0);
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Взять транспорт"), new Vector3(906.3733, -1516.33, 29.29401 + 1.2), 30f, 0.5f, 0, new Color(255, 255, 255));
                Shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 230);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                Shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };

            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        static int checkpointPayment = 12;
        private static int lastcar = -1;
        private static nLog Log = new nLog("Snow");
        //private static int JobMultiper = 1;
        private static Random rnd = new Random();

        private static List<Vector3> carspawns = new List<Vector3>
        {
            new Vector3(923.6407, -1524.359, 29.92347),
            new Vector3(899.7582, -1572.008, 29.73121),
            new Vector3(871.7248, -1547.102, 29.25345)
        };

        public static Vector3 getSnowPos()
        {
            if (lastcar + 1 >= carspawns.Count)
                lastcar = 0;
            else
                lastcar += 1;
            return carspawns[lastcar];
        }

        private static void snowCheckpointEnterWay(ColShape shape, Player player)
        {
            try
            {
                if (!NAPI.Player.IsPlayerInAnyVehicle(player)) return;
                var vehicle = player.Vehicle;
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "SNOW" || Main.Players[player].WorkID != 14 || !player.GetData<bool>("ON_WORK") || !shape.HasData("NUMBER") || player.GetData<int>("CHECK") != shape.GetData<int>("NUMBER") || player.GetData<int>("WORKWAY") != shape.GetData<int>("WAY"))
                    return;

                int way = player.GetData<int>("WORKWAY");
                int check = NAPI.Data.GetEntityData(player, "CHECK");

                var payment = Convert.ToInt32((checkpointPayment + Main.AddJobPoint(player) ) * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);

                int level = Main.Players[player].LVL > 5 ? 12 : 2 * Main.Players[player].LVL;

                player.SetData("PAYMENT", player.GetData<int>("PAYMENT") + payment + level);

                player.SendNotification($"Уборка: ~h~~g~+{payment + level}$", true);

                Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));

                //GameLog.Money($"server", $"player({Main.Players[player].UUID})", payment, $"lawnCheck");

                if (check + 1 != Points[way].Count)
                {

                    //player.SendChatMessage($"МЕНЯЕМ {check}");
                    var direction = (check + 2 != Points[way].Count) ? Points[way][check + 2] : Points[way][0] - new Vector3(0, 0, 1.12);
                    Trigger.PlayerEvent(player, "createCheckpoint", 4, 1, Points[way][check + 1] - new Vector3(0, 0, 1.12), 2, 0, 0, 0, 255, direction);
                    Trigger.PlayerEvent(player, "createWaypoint", Points[way][check + 1].X, Points[way][check + 1].Y);
                    NAPI.Data.SetEntityData(player, "CHECK", check + 1);
                }
                else
                {
                    //player.SendChatMessage("МЕНЯЕМ");
                    int newway = rnd.Next(0, 2);
                    player.SetData("WORKWAY", newway);
                    Trigger.PlayerEvent(player, "createCheckpoint", 4, 1, Points[newway][0] - new Vector3(0, 0, 1.12), 2, 0, 255, 0, 0, Points[newway][1] - new Vector3(0, 0, 1.12));
                    Trigger.PlayerEvent(player, "createWaypoint", Points[newway][0].X, Points[newway][0].Y);
                    NAPI.Data.SetEntityData(player, "CHECK", 0);
                }
            }
            catch (Exception ex) { Log.Write("mowerCheckpointEnterWay: " + ex.Message, nLog.Type.Error); }
        }

        public static void onPlayerDissconnectedHandler(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                try { if (!player.GetData<bool>("ON_WORK")) return; }
                catch { return; }
                if (Main.Players[player].WorkID == 14 &&
                    NAPI.Data.GetEntityData(player, "WORK") != null)
                {
                    var vehicle = NAPI.Data.GetEntityData(player, "WORK");
                    NAPI.Task.Run(() =>
                   {
                       try
                       {
                           vehicle.Delete();
                       }
                       catch { }
                   });
                    MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                    Utils.QuestsManager.AddQuestProcess(player, 13, player.GetData<int>("PAYMENT"));

                    Golemo.Families.Family.GiveMoneyOnJob(player, player.GetData<int>("PAYMENT"));

                    Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                    player.SetData("PAYMENT", 0);
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") == "SNOW" &&
                Main.Players[player].WorkID == 14 &&
                NAPI.Data.GetEntityData(player, "ON_WORK") &&
                NAPI.Data.GetEntityData(player, "WORK") == vehicle)
                {
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Если Вы не сядете в машину через 60 секунд, то рабочий день закончится", 3000);
                    NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
                    if (player.HasData("WORK_CAR_EXIT_TIMER"))

                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", 0);

                    NAPI.Data.SetEntityData(player, "WORK_CAR_EXIT_TIMER", Timers.StartTask(1000, () => timer_playerExitWorkVehicle(player, vehicle)));
                }
            }
            catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }

        private void timer_playerExitWorkVehicle(Player player, Vehicle vehicle)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (!player.HasData("WORK_CAR_EXIT_TIMER")) return;
                    if (NAPI.Data.GetEntityData(player, "IN_WORK_CAR"))
                    {

                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        Log.Debug("Player exit work vehicle timer was stoped");
                        return;
                    }
                    if (NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") > 60)
                    {
                        vehicle.Delete();
                        NAPI.Data.SetEntityData(player, "ON_WORK", false);
                        NAPI.Data.SetEntityData(player, "WORK", null);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);
                        Trigger.PlayerEvent(player, "deleteCheckpoint", 4, 0);
                        Utils.QuestsManager.AddQuestProcess(player, 13, player.GetData<int>("PAYMENT"));

                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили зарплату в размере: {player.GetData<int>("PAYMENT")}$", 3000);

                        MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));

                        Golemo.Families.Family.GiveMoneyOnJob(player, player.GetData<int>("PAYMENT"));

                        Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                        player.SetData("PAYMENT", 0);

                        Timers.Stop(NAPI.Data.GetEntityData(player, "WORK_CAR_EXIT_TIMER"));
                        NAPI.Data.ResetEntityData(player, "WORK_CAR_EXIT_TIMER");
                        Customization.ApplyCharacter(player);
                        return;
                    }
                    NAPI.Data.SetEntityData(player, "CAR_EXIT_TIMER_COUNT", NAPI.Data.GetEntityData(player, "CAR_EXIT_TIMER_COUNT") + 1);
                }
                catch (Exception e) { Log.Write("Timer_PlayerExitWorkVehicle_Lawnmower: " + e.Message, nLog.Type.Error); }
            });
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicleHandler(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                if (NAPI.Data.GetEntityData(vehicle, "TYPE") != "SNOW" || player.VehicleSeat != 0) return;

                if (Main.Players[player].WorkID == 14)
                {
                    if (NAPI.Data.GetEntityData(player, "WORK") != vehicle)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У транспорта есть водитель", 3000);
                        VehicleManager.WarpPlayerOutOfVehicle(player);
                    }
                    else NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
                }
                else
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете уборщиком. Устроиться можно у начальника", 3000);
                    VehicleManager.WarpPlayerOutOfVehicle(player);
                }
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }

        public static void snowspawn(Player player)
        {

            player.SetData("ON_WORK", true);
            int way = rnd.Next(0, 2);
            player.SetData("WORKWAY", way);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы начали рабочий день!", 3000);

            player.SetData("PAYMENT", 0);
            Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));

            // 18f350plow

            var veh = NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey("rubble"), getSnowPos(), 135, 4, 4, "SNOW");

            NAPI.Data.SetEntityData(veh, "ACCESS", "WORK");
            NAPI.Data.SetEntityData(veh, "WORK", 14);
            NAPI.Data.SetEntityData(veh, "TYPE", "SNOW");
            NAPI.Data.SetEntityData(veh, "NUMBER", "SNOW");
            NAPI.Data.SetEntityData(veh, "DRIVER", player);
            NAPI.Data.SetEntityData(veh, "ON_WORK", true);
            player.SetData("PAYMENT", 0);
            Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));
            veh.SetSharedData("PETROL", VehicleManager.VehicleTank[veh.Class]);
            Core.VehicleStreaming.SetEngineState(veh, true);
            Core.VehicleStreaming.SetLockStatus(veh, false);
            NAPI.Data.SetEntityData(player, "WORK", veh);
            NAPI.Data.SetEntityData(player, "ON_WORK", true);
            NAPI.Data.SetEntityData(player, "IN_WORK_CAR", true);
            player.SetData("CHECK", 0);
            player.SetIntoVehicle(veh, 0);
            Notify.Send(player, NotifyType.Alert, NotifyPosition.BottomCenter, "Направляйтесь по маршуту!",5000);

            Trigger.PlayerEvent(player, "createCheckpoint", 4, 1, Points[way][0] - new Vector3(0, 0, 1.12), 2, 0, 0, 0, 255, Points[way][1] - new Vector3(0, 0, 1.12));
            Trigger.PlayerEvent(player, "createWaypoint", Points[way][0].X, Points[way][0].Y);

        }


    }
}
