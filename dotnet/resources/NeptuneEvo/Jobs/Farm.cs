using GTANetworkAPI;
using System.Collections.Generic;
using System;
using NeptuneEVO.GUI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Jobs
{
    class Farm : Script
    {
        private static int checkpointPayment = 10;
        private static nLog Log = new nLog("Farm");

        private static List<ColShape> Shapes = new List<ColShape>();

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            try
            {
                Vector3 Pos = new Vector3(84.4153, 3731.332, 38.65);
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16($"~b~Эла"), Pos + new Vector3(0, 0, 2.2f), 10F, 0.3F, 0, new Color(255, 255, 255));
                NAPI.Marker.CreateMarker(27, Pos + new Vector3(0, 0, 0.12f), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220), false, 0);
                ColShape Shape = NAPI.ColShape.CreateCylinderColShape(Pos, 1.2f, 3);
                Shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        if (!Main.Players.ContainsKey(entity)) return;
                        entity.SetData("INTERACTIONCHECK", 212);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                Shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 0);

                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };



                for (int i = 0; i < Trees.Count; i++)
                {
                    NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_plant_palm_01b"), Trees[i], new Vector3(0,0,0));
                    Shape = NAPI.ColShape.CreateCylinderColShape(Trees[i], 1f, 10, 0);
                    Shape.SetData("NUMBER", i);
                    Shape.Position = Trees[i];
                    NAPI.Task.Run(() => { try { NAPI.Entity.SetEntityPosition(Shape, Trees[Shape.GetData<int>("NUMBER")]); } catch { } }, 1000 );
                    Shape.OnEntityEnterColShape += PlayerEnterCheckpoint;
                    Shapes.Add(Shape);
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        private static List<Vector3> Trees = new List<Vector3>
        {
            new Vector3(2095.15087890625, 4917.89453125, 40.060089111328125),
           
            new Vector3(2087.127685546875, 4925.92431640625, 40.055782318115234),
            new Vector3(2084.645751953125, 4928.59912109375, 40.05384826660156),

            new Vector3(2079.817626953125, 4933.34521484375, 40.067447662353516),
            new Vector3(2077.201416015625, 4936.0439453125, 40.06584930419922),
            new Vector3(2075.232421875, 4938.087890625, 40.06791687011719),
            new Vector3(2073.215576171875, 4940.17578125, 40.07830810546875),

            new Vector3(2077.656494140625, 4932.32861328125, 40.09591293334961),

            new Vector3(2084.807373046875, 4925.19287109375, 40.08658218383789),
            new Vector3(2086.893798828125, 4923.15283203125, 40.082305908203125),

            new Vector3(2093.345947265625, 4916.9208984375, 40.09043502807617),
            new Vector3(2092.280029296875, 4915.11279296875, 40.08655548095703),

            new Vector3(2088.153564453125, 4919.24462890625, 40.08212661743164),
            new Vector3(2085.5341796875, 4921.92041015625, 40.094051361083984),

            new Vector3(2078.377197265625, 4929.12060546875, 40.08147430419922),

            new Vector3(2071.25439453125, 4936.46826171875, 40.1046142578125),
            new Vector3(2069.923828125, 4937.88427734375, 40.10870361328125),
            new Vector3(2068.875244140625, 4936.0244140625, 40.08317565917969),

            new Vector3(2085.85546875, 4918.4912109375, 40.10239028930664),
            new Vector3(2088.5439453125, 4915.859375, 40.11174774169922),
            new Vector3(2091.10009765625, 4913.2783203125, 40.08573913574219),

            new Vector3(2087.507080078125, 4914.13232421875, 40.07948303222656),

            new Vector3(2080.48876953125, 4921.17578125, 40.06708908081055),
            new Vector3(2077.939453125, 4923.779296875, 40.082618713378906),
            new Vector3(2075.4853515625, 4926.43994140625, 40.099918365478516),
            new Vector3(2072.84619140625, 4929.14599609375, 40.114585876464844),

            new Vector3(2066.058349609375, 4933.03564453125, 40.13939666748047),
            new Vector3(2068.51171875, 4930.16162109375, 40.11915588378906),

            new Vector3(2082.55029296875, 4916.07958984375, 40.08663558959961),

            new Vector3(2087.22119140625, 4911.623046875, 40.104496002197266),
            new Vector3(2086.7958984375, 4909.4169921875, 40.10433578491211),

            new Vector3(2079.467529296875, 4916.65234375, 40.072383880615234),
            new Vector3(2077.366943359375, 4918.638671875, 40.07542037963867),

            new Vector3(2072.11962890625, 4924.0576171875, 40.096527099609375),
            new Vector3(2070.046142578125, 4926.25, 40.10494613647461),
            new Vector3(2067.9814453125, 4928.384765625, 40.10980987548828),

            new Vector3(2068.67236328125, 4924.47509765625, 40.090545654296875),
            new Vector3(2071.203125, 4921.84716796875, 40.09149932861328),

            new Vector3(2079.0732421875, 4913.96337890625, 40.09603500366211),
            new Vector3(2081.61376953125, 4911.41064453125, 40.10531234741211),

            new Vector3(2076.02978515625, 4914.27587890625, 40.05704116821289),
            new Vector3(2073.61767578125, 4916.72998046875, 40.052459716796875),
            new Vector3(2071.091552734375, 4919.48583984375, 40.058143615722656),

            new Vector3(2063.210693359375, 4927.4853515625, 40.072120666503906),
            new Vector3(2061.41357421875, 4929.56640625, 40.0703239440918),

            new Vector3(2072.1943359375, 4915.2255859375, 40.04807662963867),

            new Vector3(2081.038818359375, 4906.29931640625, 40.058074951171875),
            new Vector3(2081.174560546875, 4903.61962890625, 40.069828033447266),

            new Vector3(2065.093994140625, 4919.64892578125, 40.11442947387695),

            new Vector3(2057.573974609375, 4924.3955078125, 40.12371063232422),
            new Vector3(2060.141845703125, 4921.68896484375, 40.11265182495117),

            new Vector3(2073.4296875, 4908.40087890625, 40.126834869384766),
            new Vector3(2076.023193359375, 4905.5830078125, 40.07826614379883),

            new Vector3(2070.668701171875, 4908.62890625, 40.05619430541992),

            new Vector3(2059.850341796875, 4919.23974609375, 40.07652282714844),
            new Vector3(2057.1630859375, 4921.87744140625, 40.104854583740234),

            new Vector3(2059.444580078125, 4916.69140625, 40.0893440246582),
            new Vector3(2062.1455078125, 4914.0166015625, 40.085609436035156),

            new Vector3(2072.818115234375, 4903.59521484375, 40.07426834106445),
            new Vector3(2075.535400390625, 4900.94287109375, 40.08000183105469),

            new Vector3(2066.005859375, 4907.49365234375, 40.11477279663086),
            new Vector3(2063.277587890625, 4910.10302734375, 40.119163513183594),

            new Vector3(2058.114501953125, 4915.45556640625, 40.10408401489258),
            new Vector3(2055.48486328125, 4918.19091796875, 40.09882354736328),
            new Vector3(2053.5576171875, 4920.20751953125, 40.09598159790039),

            new Vector3(2058.10986328125, 4912.1943359375, 40.07497787475586),
            new Vector3(2060.838623046875, 4909.4951171875, 40.104217529296875),

            new Vector3(2068.823974609375, 4901.51611328125, 40.12717819213867),
            new Vector3(2071.43505859375, 4898.921875, 40.129398345947266),

            new Vector3(2071.277099609375, 4896.72802734375, 40.089210510253906),
            new Vector3(2068.512451171875, 4899.37451171875, 40.069801330566406),
            new Vector3(2065.888916015625, 4902.07177734375, 40.07734680175781),

            new Vector3(2050.515625, 4917.39208984375, 40.09666061401367),
            new Vector3(2048.6611328125, 4916.74658203125, 40.036224365234375),

            new Vector3(2056.296630859375, 4908.57861328125, 40.09965896606445),
            new Vector3(2058.679443359375, 4906.14794921875, 40.104644775390625),
            new Vector3(2061.225830078125, 4903.35498046875, 40.10835266113281),

            new Vector3(2069.09716796875, 4895.57666015625, 40.07966232299805),

            new Vector3(2065.121337890625, 4896.65185546875, 40.08168411254883),
            new Vector3(2062.46728515625, 4899.35400390625, 40.091243743896484),
            new Vector3(2059.7626953125, 4902.2841796875, 40.09071350097656),

            new Vector3(2048.0791015625, 4910.8681640625, 40.05931091308594),
            new Vector3(2050.58740234375, 4908.15673828125, 40.08427429199219),
            new Vector3(2053.07373046875, 4905.50732421875, 40.09755325317383),

            new Vector3(2063.702880859375, 4894.97119140625, 40.10223388671875),
            new Vector3(2066.405517578125, 4892.30615234375, 40.102806091308594),
            new Vector3(2068.295166015625, 4890.4501953125, 40.06970977783203)
        };


        public static void StartWorkDay(Player player)
        {
            if (player.GetData<bool>("ON_WORK"))
            {
                player.SetData("ON_WORK", false);
                Trigger.PlayerEvent(player, "deleteCheckpoint", 15);
                Trigger.PlayerEvent(player, "deleteWorkBlip");

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вали отсюда уже! Эт, если че привези мне тоже пару пакетиков...", 3000);
                return;
            }
            else
            {

                var check = WorkManager.rnd.Next(0, Shapes.Count - 1);
                player.SetData("WORKCHECK", check);
                Trigger.PlayerEvent(player, "createCheckpoint", 15, 1, Shapes[check].Position - new Vector3(0, 0, 2.5f), 3, 0, 255, 255, 255);
                Trigger.PlayerEvent(player, "createWorkBlip", Shapes[check].Position);

                player.SetData("ON_WORK", true);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Давай шуруй быстрее собирай, пока никого нет", 3000);


                Main.Players[player].States[1] += 1;
                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Вам дали статью 228 (Наркотические вещества)", 3000);

                return;
            }
        }

        private static void PlayerEnterCheckpoint(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (!player.GetData<bool>("ON_WORK") || shape.GetData<int>("NUMBER") != player.GetData<int>("WORKCHECK")) return;

                var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Kokos));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Упс, похоже Вы забили все карманы...", 3000);
                    return;
                }

                var payment = Convert.ToInt32(checkpointPayment * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);
                int level = Main.Players[player].LVL > 5 ? 6 : 1 * Main.Players[player].LVL;
                //player.SetData("PAYMENT", player.GetData("PAYMENT") + payment);
                MoneySystem.Wallet.Change(player, payment + level);


                player.SendNotification($"Ферма: ~h~~g~+{payment + level}$", true);
                Golemo.Families.Family.GiveMoneyOnJob(player, payment + level);

                //GameLog.Money($"server", $"player({Main.Players[player].UUID})", payment, $"electricianCheck");
                Main.OnAntiAnim(player);
                player.PlayAnimation("missmechanic", "work_in", 39);
                player.SetData("WORKCHECK", -1);
                NAPI.Task.Run(() => {
                    try
                    {
                        if (player != null && Main.Players.ContainsKey(player))
                        {

                            player.StopAnimation();
                            Main.OffAntiAnim(player);

                            var nextCheck = WorkManager.rnd.Next(0, Shapes.Count - 1);
                            while (nextCheck == shape.GetData<int>("NUMBER")) nextCheck = WorkManager.rnd.Next(0, Shapes.Count - 1);
                            player.SetData("WORKCHECK", nextCheck);
                            Trigger.PlayerEvent(player, "createCheckpoint", 15, 1, Shapes[nextCheck].Position - new Vector3(0,0,2.5f), 3, 0, 255, 255, 255);
                            Trigger.PlayerEvent(player, "createWorkBlip", Shapes[nextCheck].Position);
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Упс, похоже Вы забили все карманы...", 3000);
                                return;
                            }
                            nInventory.Add(player, new nItem(ItemType.Kokos, 1));
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Оппа... один листик есть, пошли дальше.", 3000);
                            
                        }
                    }
                    catch { }
                }, 4000);

            }
            catch (Exception e) { Log.Write("PlayerEnterCheckpoint: " + e.Message, nLog.Type.Error); }
        }

        internal class Checkpoint
        {
            public Vector3 Position { get; }
            public double Heading { get; }

            public Checkpoint(Vector3 pos, double rot)
            {
                Position = pos;
                Heading = rot;
            }
        }
    }
}
