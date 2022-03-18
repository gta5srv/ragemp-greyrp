using System;
using System.Collections.Generic;
using System.Xml.Schema;
using GTANetworkAPI;
using Newtonsoft.Json;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Core
{
	class Containers : Script
	{
		private static nLog Log = new nLog("Containers");
		public static bool CheckDictionary(Dictionary<string, int> table)
		{
			if (table.Count == 0)
			{
				Log.Write($"Table have 0 cars", nLog.Type.Error);
				return false;
			}

			int max = 0;
			foreach (KeyValuePair<string, int> str in table)
				max += str.Value;
			if (max > 100 || max < 100)
			{
				Log.Write($"Table have > 100% or < 100%", nLog.Type.Error);
				return false;
			}
			else
				return true;
		}

		public static Dictionary<string, int> container1 = new Dictionary<string, int>
		{
			{"18Velar", 9},
			{"cyber", 10},
			{"jesko", 21},
			{"urus", 60}
		};

		public static Dictionary<string, int> container2 = new Dictionary<string, int>
		{
			{"bugatti", 5},
			{"m2", 5},
			{"s500w222", 5},
			{"19S650", 5},
			{"720s", 5},
			{"a80", 5},
			{"bmwi8", 5},
			{"dtdpista", 5},
			{"italirsxrod", 5},
			{"na1", 5},
			{"pturismo", 5},
			{"vclass", 5},
			{"wraith", 5},
			{"x5e53", 5},
			{"jes", 10},
			{"srt8", 20}
		};

		public static Dictionary<string, int> container3 = new Dictionary<string, int>
		{
			{"amggtrr20", 5},
			{"bmci", 5},
			{"bmwm8", 5},
			{"e63b", 5},
			{"i8", 5},
			{"f82", 5},
			{"m2", 5},
			{"m6f13", 5},
			{"s63w222", 5},
			{"s600w220", 5},
			{"x5e53", 10},
			{"adder", 20},
			{"zentorno", 20}
		};
		public static Dictionary<string, int> container4 = new Dictionary<string, int>
		{
			{"rsvr16", 15},
			{"srt8b", 15},
			{"rubilord", 20},
			{"x5e53", 20},
			{"XPERIA38", 30}
		};
		public static Dictionary<string, int> container5 = new Dictionary<string, int>
		{
			{"brioso", 5},
			{"asbo", 5},
			{"windsor2", 5},
			{"clique", 5},
			{"dominator3", 5},
			{"faction", 10},
			{"faction2", 10},
			{"faction3", 15},
			{"a80", 20},
			{"x5e53", 20}
		};
		public static Dictionary<string, int> container6 = new Dictionary<string, int>
		{
			{"toros", 5},
			{"rebla", 5},
			{"patriot", 5},
			{"landstalker2", 5},
			{"huntley", 5},
			{"granger", 5},
			{"dubsta", 5},
			{"dubsta2", 5},
			{"fq2", 5},
			{"contender", 5},
			{"gauntlet", 10},
			{"x5e53", 20},
			{"a80", 20}
		};

		public static Dictionary<string, int> container7 = new Dictionary<string, int>
		{
			{"dubsta", 2},
			{"dubsta2", 3},
			{"formula", 5},
			{"formula2", 10},
			{"openwheel1", 15},
			{"openwheel2", 15},
			{"gauntlet4", 20},
			{"gauntlet", 30}
		};


		[ServerEvent(Event.ResourceStart)]
		public void onResourceStart()
		{
			try
			{
				// FALSE - $
				// TRUE - UP

				new Container("Fortune", true, container1, new Vector3(880.9291, -2911.608, 4.780589), 270, 1299);
				new Container("Mercedes-Benz & BMW", true, container2, new Vector3(880.9291, -2918.955, 4.780589), 270, 799);
				new Container("Sports & Premium", true, container3, new Vector3(880.9291, -2926.302, 4.780589), 270, 799);

				new Container("SUV", true, container4, new Vector3(904.9575, -2911.608, 4.780589), 270, 699);
				new Container("Budget & Bikes", true, container5, new Vector3(904.9575, -2918.955, 4.780589), 270, 499);
				new Container("Deutsche & American & World", true, container6, new Vector3(904.9575, -2926.302, 4.780589), 270, 999);

				new Container("Lucky", false, container7, new Vector3(928.9859, -2911.608, 4.780589), 270, 722999);



				Log.Write("Loaded all containers!", nLog.Type.Success);
			}
			catch (Exception e)
			{
				Log.Write("EXCEPTION AT \"CONTAINERS\":\n" + e.ToString(), nLog.Type.Error);
			}
		}

		public static string RandomDictionary(Dictionary<string, int> table)
		{
			try
			{
				List<string> Strings = new List<string> { }; List<int> Floats = new List<int> { };
				foreach (KeyValuePair<string, int> str in table)
				{
					Strings.Add(str.Key); Floats.Add(str.Value);
				}
				int chance = new Random().Next(0, 100);
				Log.Write($"Open container: {chance}", nLog.Type.Info);
				int fend = 0;
				int plus = 0;
				for (int i = 0; i < Floats.Count; i++)
				{

					int start = Floats[i];
					if (i != 0)
					{


						if (i == 1)
							fend = Floats[i - 1];
						else
							fend = Floats[i - 1] + plus;

						plus += Floats[i - 1];

						start = Floats[i] + plus;

					}
					if (i == Floats.Count) { start = 100; }
					if (fend <= chance && chance <= start)
					{
						return Strings[i];
					}

				}
				return "";
			}
			catch (Exception e)
			{
				Log.Write("EXCEPTION AT \"CONTAINERSRND\":\n" + e.ToString(), nLog.Type.Error);
				return "";
			}
		}


		[Command("containerfree")]
		public static void CMD_Containerfree(Player player, string model)
		{
			if (Main.Players[player].AdminLVL > 9)
				new Container("БЕСПЛАТНО", true, new Dictionary<string, int> { { model, 100 } }, new Vector3(880.9291, -2933.649, 4.780589), 270, 0, true);
			NAPI.Chat.SendChatMessageToAll("Уважаемые жители, в наш штат прибыл контейнер с брошеным автомобилем!");
		}

		internal class Container
		{
			public string Name { get; set; }
			public Vector3 Position { get; set; }
			public float Rotation { get; set; }
			public bool Donat { get; set; }
			public Player Opened { get; set; }
			public Dictionary<string, int> Cars { get; set; }
			public int Cost { get; set; }
			public int Vehicle { get; set; }
			private GTANetworkAPI.Object DoorL = null;
			private GTANetworkAPI.Object DoorR = null;
			private GTANetworkAPI.Object ContainerM = null;
			private GTANetworkAPI.Object ContainerML = null;
			private GTANetworkAPI.Object ContainerMR = null;
			private TextLabel Label1 = null;
			private TextLabel Label2 = null;
			private ColShape Shape = null;
			private ColShape Shape2 = null;
			private Marker Marker1 = null;
			private Marker Marker2 = null;
			private float DoorRotate = 0;
			private string timer;
			public bool DestroyM { get; set; }
			private bool IsDesrtoy = false;

			public Container(string name, bool donat, Dictionary<string, int> table, Vector3 position, float rotation, int cost, bool destroy = false)
			{
				if (!CheckDictionary(table)) return;
				// Initalize
				Name = name; Donat = donat; Cars = table; Position = position; Rotation = rotation; Cost = cost; DestroyM = destroy;
				string cur = "$";
				if (donat)
					cur = "UP";
				Label1 = NAPI.TextLabel.CreateTextLabel($"~w~{Name}\n~b~Цена:~w~ {Cost}{cur}", new Vector3(Position.X - 7f, Position.Y, Position.Z + 1.5), 3f, 0.5F, 0, new Color(255, 255, 255), true, 0);
				Label2 = NAPI.TextLabel.CreateTextLabel($"~b~ Возможный выигрыш", new Vector3(Position.X - 7f, Position.Y - 3f, Position.Z + 1.5), 3f, 0.5F, 0, new Color(255, 255, 255), true, 0);
				ContainerM = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_container_ld"), Position, new Vector3(0, 0, Rotation));
				ContainerML = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_cntrdoor_ld_l"), Position + new Vector3(-6.1f, 1.3f, 1.5f), new Vector3(0, 0, Rotation));
				ContainerMR = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_cntrdoor_ld_r"), Position + new Vector3(-6.1f, -1.3f, 1.5f), new Vector3(0, 0, Rotation));
				ContainerML.Transparency = 0;
				ContainerMR.Transparency = 0;
				DoorL = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_cntrdoor_ld_l"), Position + new Vector3(-6.1f, 1.3f, 1.5f), new Vector3(0, 0, Rotation));
				DoorR = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_cntrdoor_ld_r"), Position + new Vector3(-6.1f, -1.3f, 1.5f), new Vector3(0, 0, Rotation));
				Shape = NAPI.ColShape.CreateCylinderColShape(Position + new Vector3(-7f, 0, 0.2), 2f, 3);
				Shape.OnEntityEnterColShape += (s, entity) =>
				{
					try
					{
						NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 71);
						NAPI.Data.SetEntityData(entity, "CONTAINER", this);
					}
					catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
				};
				Shape.OnEntityExitColShape += (s, entity) =>
				{
					try
					{
						NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
						NAPI.Data.ResetEntityData(entity, "CONTAINER");
					}
					catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
				};
				Shape2 = NAPI.ColShape.CreateCylinderColShape(Position + new Vector3(-7f, -3f, 0.2), 1f, 3);
				Shape2.OnEntityEnterColShape += (s, entity) =>
				{
					try
					{
						NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 72);
						NAPI.Data.SetEntityData(entity, "CONTAINER", this);
					}
					catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
				};
				Shape2.OnEntityExitColShape += (s, entity) =>
				{
					try
					{
						NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
						NAPI.Data.ResetEntityData(entity, "CONTAINER");
					}
					catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
				};
				Marker1 = NAPI.Marker.CreateMarker(27, Position + new Vector3(-7f, 0, 0.2f), new Vector3(), new Vector3(), 2f, new Color(0, 86, 214, 220), false, 0);
				Marker2 = NAPI.Marker.CreateMarker(27, Position + new Vector3(-7f, -3f, 0.2f), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220), false, 0);
			}

			public void Open(Player player)
			{
				try
				{
					if (IsDesrtoy) return;
					if (Opened != null)
					{
						Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Кто-то уже открывает контейнер", 3000);
						return;
					}
					var house = Houses.HouseManager.GetHouse(player, true);

					var apartament = Houses.HouseManager.GetApart(player, true);

					if (house == null)
					{
						if (apartament != null)
						{
							house = apartament;
						}
					}


					if (house == null && VehicleManager.getAllPlayerVehicles(player.Name.ToString()).Count > 0)
					{
						Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет личного дома", 3000);
						return;
					}
					if (house != null)
					{

						if (house.GarageID == 0 && VehicleManager.getAllPlayerVehicles(player.Name.ToString()).Count > 1)
						{
							Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет гаража", 3000);
							return;
						}

						var garage = Houses.GarageManager.Garages[house.GarageID];
						if (VehicleManager.getAllPlayerVehicles(player.Name).Count >= Houses.GarageManager.GarageTypes[garage.Type].MaxCars)
						{
							Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас максимальное кол-во машин", 3000);
							return;
						}
					}
					if (Donat)
					{
						if (Main.Accounts[player].RedBucks < Cost)
						{
							Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
							return;
						}
					}
					else
					{
						if (Main.Players[player].Money < Cost)
						{
							Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
							return;
						}
					}

					var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.CarKey));
					if (tryAdd == -1 || tryAdd > 0)
					{
						Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нет места для ключей", 3000);
						return;
					}
					string Model = RandomDictionary(Cars);
					Opened = player;
					var number = VehicleManager.Create(player.Name, Model, new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
					var vehdata = VehicleManager.Vehicles[number];
					VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(vehdata.Model);
					var veh = NAPI.Vehicle.CreateVehicle(vh, Position, new Vector3(0, 0, 87), 0, 0);

					if (house != null)
					{
						if (house.GarageID != 0)
						{
							Houses.Garage Garage = Houses.GarageManager.Garages[house.GarageID];
							Garage.SetOutVehicle(number, veh);
						}
					}
					nInventory.Add(player, new nItem(ItemType.CarKey, 1, $"{number}_{VehicleManager.Vehicles[number].KeyNum}"));

					if (Donat)
					{
						MySQL.Query($"update `accounts` set `redbucks`=`redbucks`-{Cost} where `login`='{Main.Accounts[player].Login}'");
						Main.Accounts[player].RedBucks -= Cost;
					}
					else
					{
						MoneySystem.Wallet.Change(player, -Cost);
					}

					Utils.QuestsManager.AddQuestProcess(player, 16);

					VehicleStreaming.SetEngineState(veh, false);
					VehicleStreaming.SetLockStatus(veh, true);
					vehdata.Holder = player.Name;
					veh.SetData("ACCESS", "PERSONAL");
					veh.SetData("ITEMS", vehdata.Items);
					veh.SetData("OWNER", player);
					veh.SetData("CONTAINER", true);
					veh.SetSharedData("PETROL", vehdata.Fuel);
					NAPI.Vehicle.SetVehicleNumberPlate(veh, number);
					VehicleManager.ApplyCustomization(veh);
					Vehicle = veh.Value;
					Doors(true);
					Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Ваш выигрыш: {ParkManager.GetNormalName(Model)}", 3000);
					if (DestroyM)
					{
						NAPI.Chat.SendChatMessageToAll("Контейнер с брошенным автомобилем был открыт!");
					}

					Timers.StartOnceTask(25000, () =>
					{
						NAPI.Task.Run(() =>
						{
							try
							{
								foreach (Vehicle vehicle in NAPI.Pools.GetAllVehicles())
								{
									if (vehicle.Value == Vehicle && vehicle.HasData("CONTAINER"))
									{
										vehicle.ResetData("CONTAINER");
										if (Opened != null)
										{
											Opened.SetIntoVehicle(vehicle, 0);
											
											NAPI.Task.Run(() =>
											{
												try
												{
													if (Opened != null && Opened.IsInVehicle)
														Opened.Vehicle.Position += new Vector3(0, 3f, 0);
												}
                                                catch { }
											}, 500);
										}
										else
                                        {
											vehicle.Position += new Vector3(0, 3f, 0);
										}
										break;
									}
								}
							}
							catch { }
						});
					});

					Timers.StartOnceTask(30000, () => Refresh());
				}
				catch (Exception e) { Log.Write("OPEN: " + e.ToString(), nLog.Type.Error); }
			}


			public void Doors(bool active)
			{
                try { 
				if (timer != null)
				{
					Timers.Stop(timer); timer = null;
				}
				if (active)
					timer = Timers.StartTask(1, () => OpenDoors());
				else
					timer = Timers.StartTask(1, () => CloseDoors());
				}
				catch (Exception e) { Log.Write("DOORS: " + e.ToString(), nLog.Type.Error); }
			}


			public void OpenDoors()
			{
				NAPI.Task.Run(() => { 
                try { 
				if (DoorRotate < 100)
				{
					DoorRotate += 1.5f;
					NAPI.Entity.SetEntityRotation(DoorL, new Vector3(0, 0, Rotation + -DoorRotate));
					NAPI.Entity.SetEntityRotation(DoorR, new Vector3(0, 0, Rotation + DoorRotate));
				}
				else
				{
							if (timer != null)
							{
								Timers.Stop(timer); timer = null;
							}
				}
				}
				catch (Exception e) { Log.Write("OPENDOORS: " + e.ToString(), nLog.Type.Error); }
				});
			}

			public void CloseDoors()
			{
				NAPI.Task.Run(() =>
				{
					try
					{
						if (DoorRotate > 0)
						{
							DoorRotate -= 1.5f;
							NAPI.Entity.SetEntityRotation(DoorL, new Vector3(0, 0, Rotation + -DoorRotate));
							NAPI.Entity.SetEntityRotation(DoorR, new Vector3(0, 0, Rotation + DoorRotate));
						}
						else
						{
							if (timer != null)
							{
								Timers.Stop(timer); timer = null;
							}
						}
					}
					catch (Exception e) { Log.Write("CLOSEDOORS: " + e.ToString(), nLog.Type.Error); }
				});
			}

			public void Refresh()
			{
                try { 
				if (DestroyM)
					Destroy();

                Doors(false);
				Vehicle = -1;
				Opened = null;
				}
				catch (Exception e) { Log.Write("REFRESH: " + e.ToString(), nLog.Type.Error); }
			}

			public void Destroy()
			{
				NAPI.Task.Run(() =>
				{
					IsDesrtoy = true;
					DoorL.Delete();
					DoorR.Delete();
					ContainerM.Delete();
					ContainerML.Delete();
					ContainerMR.Delete();
					Label1.Delete();
					Label2.Delete();
					Shape.Delete();
					Shape2.Delete();
					Marker1.Delete();
					Marker2.Delete();
					Cars = new Dictionary<string, int> { { "", 100 } };
				});

			}

		}
	}
}
