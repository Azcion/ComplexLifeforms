using System;
using System.Reflection;

namespace ComplexLifeforms {

	public class Lifeform {

		private static int _id;

		private bool _pendingKill;

		public readonly int Id;

		/// <summary>Constructor parameters.</summary>
		public readonly SInitLifeform Init;

		/// <summary>World in which resources will be exchanged.</summary>
		public readonly World World;
		public readonly MoodManager Mood;

		public readonly double HealCost;
		public readonly double HealAmount;

		public readonly double HpDrain;
		public readonly double EnergyDrain;
		public readonly double FoodDrain;
		public readonly double WaterDrain;

		public readonly double HealThreshold;
		public readonly double SleepThreshold;
		public readonly double EatThreshold;
		public readonly double DrinkThreshold;

		public bool Alive { get; private set; }
		public bool Asleep { get; private set; }
		public DeathBy DeathBy { get; private set; }

		public double Hp { get; private set; }
		public double Energy { get; private set; }
		public double Food { get; private set; }
		public double Water { get; private set; }

		public int Age { get; private set; }
		public int HealCount { get; private set; }
		public int SleepCount { get; private set; }
		public int EatCount { get; private set; }
		public int DrinkCount { get; private set; }

		/// <summary>
		/// Unpacks SInitLifeform and uses its values.
		/// </summary>
		public Lifeform (World world, SInitLifeform init, Random random=null)
				: this(world, random,
						init.HpScale, init.EnergyScale,
						init.FoodScale, init.WaterScale,
						init.HealCostScale, init.HealAmountScale,
						init.HpDrainScale, init.EnergyDrainScale,
						init.FoodDrainScale, init.WaterDrainScale,
						init.HealThreshold, init.SleepThreshold,
						init.EatThreshold, init.DrinkThreshold) {
		}

		public Lifeform (World world, Random random=null,
				double hpScale=1, double energyScale=1,
				double foodScale=1, double waterScale=1,
				double healCostScale=1, double healAmountScale=1,
				double hpDrainScale=1, double energyDrainScale=1,
				double foodDrainScale=1, double waterDrainScale=1,
				double healThreshold=0.5, double sleepThreshold=0.25,
				double eatThreshold=0.5, double drinkThreshold=0.5) {
			Id = _id++;
			World = world;
			Init = new SInitLifeform(hpScale, energyScale,
					foodScale, waterScale,
					healCostScale, healAmountScale,
					hpDrainScale, energyDrainScale,
					foodDrainScale, waterDrainScale,
					healThreshold, sleepThreshold,
					eatThreshold, drinkThreshold);
			Mood = new MoodManager(this, random);

			SInitWorld w = world.Init;

			HealCost = w.HealCost * healCostScale;
			HealAmount = w.HealAmount * healAmountScale;
			HpDrain = w.HpDrain * hpDrainScale;
			EnergyDrain = w.EnergyDrain * energyDrainScale;
			FoodDrain = w.FoodDrain * foodDrainScale;
			WaterDrain = w.WaterDrain * waterDrainScale;

			Alive = true;
			Asleep = false;
			DeathBy = DeathBy.None;

			Hp = w.BaseHp * hpScale;
			Energy = w.BaseEnergy * energyScale;
			Food = w.BaseFood * foodScale;
			Water = w.BaseWater * waterScale;

			HealThreshold = Hp * healThreshold;
			SleepThreshold = Energy * sleepThreshold;
			EatThreshold = Food * eatThreshold;
			DrinkThreshold = Water * drinkThreshold;
		}

		public void Update () {
			if (!Alive) {
				return;
			}

			++Age;

			if (Asleep) {
				Hp += HpDrain / 10;
				Energy += EnergyDrain * 10;
				Food -= FoodDrain / 10;
				Water -= WaterDrain / 10;
				++SleepCount;

				if (Energy >= World.Init.BaseEnergy * Init.EnergyScale) {
					Asleep = false;
				}
			}

			ProcessBodilyFunctions();
			Mood.Update();

			if (Energy < SleepThreshold) {
				if (Energy < 0) {
					Sleep(true);
				}

				Sleep();
			}

			Heal();

			if (Hp < 0 || _pendingKill) {
				Kill();
			}
		}

		private void ProcessBodilyFunctions () {
			double deltaHp = 0;
			double deltaEnergy = 0;
			double deltaFood = 0;
			double deltaWater = 0;

			if (Food > 0) {
				if (Food > FoodDrain) {
					if (Food > EatThreshold) {
						deltaHp += HpDrain / 2;

						if (!Asleep) {  // excrete
							deltaEnergy -= EnergyDrain * 4;
							deltaFood -= FoodDrain * 2;

							Mood.AffectUrge(Urge.Eat, -1);
							Mood.Action(Urge.Excrete);
						}
					} else {
						deltaHp -= HpDrain / 2;

						if (!Asleep) {
							deltaEnergy -= EnergyDrain;
							deltaFood -= FoodDrain;
						}
					}
				} else {
					deltaHp -= HpDrain * 5;

					if (!Asleep) {
						deltaEnergy -= EnergyDrain / 2;
						deltaFood -= Food;
					}
				}
			} else {
				deltaHp -= HpDrain * 10;

				if (!Asleep) {
					deltaEnergy -= EnergyDrain / 2;
				}
			}

			if (Water > 0) {
				if (Water > WaterDrain) {
					if (Water > DrinkThreshold) {
						deltaHp += HpDrain / 2;

						if (!Asleep) {  // excrete
							deltaEnergy -= EnergyDrain * 4;
							deltaWater -= WaterDrain * 2;

							Mood.AffectUrge(Urge.Drink, -1);
							Mood.Action(Urge.Excrete);
						}
					} else {
						deltaHp -= HpDrain / 2;

						if (!Asleep) {
							deltaEnergy -= EnergyDrain;
							deltaWater -= WaterDrain;
						}
					}
				} else {
					deltaHp -= HpDrain * 10;

					if (!Asleep) {
						deltaEnergy -= EnergyDrain / 2;
						deltaWater -= Water;
					}
				}
			} else {
				deltaHp -= HpDrain * 20;

				if (!Asleep) {
					deltaEnergy -= EnergyDrain / 2;
				}
			}

			World.Reclaim(-deltaFood, -deltaWater);

			Hp += deltaHp;
			Energy += deltaEnergy;
			Food += deltaFood;
			Water += deltaWater;

			if (Food < 0) {
				Food = 0;
			}

			if (Water < 0) {
				Water = 0;
			}
		}

		private void Sleep (bool didPassOut=false) {
			Asleep = true;

			if (didPassOut) {
				Hp -= HpDrain * 10;
				
				if (Hp < 0 && DeathBy == DeathBy.None) {
					DeathBy = DeathBy.Exhaustion;
					_pendingKill = true;
				}
			}

			Mood.Action(Urge.Sleep);
		}

		private void Heal () {
			if (Hp < 0 || Hp > HealThreshold || Asleep) {
				return;
			}

			if (Food <= HealCost) {
				Mood.AffectUrge(Urge.Eat, 1);
				return;
			}

			if (Water <= HealCost) {
				Mood.AffectUrge(Urge.Drink, 1);
				return;
			}

			World.Reclaim(HealCost, HealCost);

			Mood.Action(Urge.Heal);

			Hp += HealAmount;
			Food -= HealCost;
			Water -= HealCost;
			++HealCount;
		}

		private void Kill () {
			if (_pendingKill) {
				_pendingKill = false;
			}

			Alive = false;

			if (DeathBy == DeathBy.None) {
				if (Food <= 0) {
					DeathBy = DeathBy.Starvation;
				} else if (Water <= 0) {
					DeathBy = DeathBy.Dehydration;
				}
			}

			Hp = -1;
			World.Decompose(this);
		}

		public void Eat (double amount) {
			if (!Alive || Asleep || World.Food <= 0) {
				return;
			}

			double deltaHp = 0;
			double deltaFood = 0;
			double deltaWater = 0;

			if (World.Food < amount) {
				amount = World.Food;
			}

			if (Food < EatThreshold) {
				deltaFood += amount;
				deltaWater -= WaterDrain / 2;
			} else {
				deltaHp -= HpDrain * 2;
				deltaFood += amount / 2;
				deltaWater -= WaterDrain * 2;
			}

			if (Water <= -deltaWater) {
				Mood.AffectUrge(Urge.Drink, 1);
				return;
			}

			World.UseFood(deltaFood);
			World.Reclaim(0, -deltaWater);

			Mood.Action(Urge.Eat);

			Hp += deltaHp;
			Food += deltaFood;
			Water += deltaWater;
			++EatCount;

			if (Food > Init.FoodScale * World.Init.BaseFood) {
				Hp -= HpDrain * 4;

				Mood.AffectUrge(Urge.Eat, -2);
				Mood.AffectUrge(Urge.Excrete, 1);
			}

			if (Hp <= 0 && DeathBy == DeathBy.None) {
				DeathBy = DeathBy.Overeating;
				_pendingKill = true;
			}
		}

		public void Drink (double amount) {
			if (!Alive || Asleep || World.Water <= 0) {
				return;
			}

			double deltaHp = 0;
			double deltaWater = 0;

			if (World.Water < amount) {
				amount = World.Water;
			}

			if (Water < DrinkThreshold) {
				deltaWater += amount;
			} else {
				deltaHp -= HpDrain * 2;
				deltaWater += amount / 2;
			}

			World.UseWater(deltaWater);

			Mood.Action(Urge.Drink);

			Hp += deltaHp;
			Water += deltaWater;
			++DrinkCount;

			if (Water > Init.WaterScale * World.Init.BaseWater) {
				Hp -= HpDrain * 4;

				Mood.AffectUrge(Urge.Drink, -2);
				Mood.AffectUrge(Urge.Excrete, 1);
			}

			if (Hp <= 0 && DeathBy == DeathBy.None) {
				DeathBy = DeathBy.Overdrinking;
				_pendingKill = true;
			}
		}

		public string ToString (char separator=' ', bool extended=false) {
			char s = separator;
			string data = $"{Age,5}{s}{(int)Hp,5}{s}{(int)Energy,5}{s}{(int)Food,5}{s}{(int)Water,5}";

			if (extended) {
				data += $"{s}{HealCount,5}{s}{SleepCount,5}{s}{EatCount,5}{s}{DrinkCount,5}"
						+ $"{s}{Mood.Urge,-9}{s}{Mood.Emotion,-12}"
						+ $"{s}{DeathBy,-12}{s}{(Asleep ? "yes" : "no"),-5}";
			}

			return data;
		}

		public static string ToStringHeader (char separator=' ', bool extended=false) {
			char s = separator;
			string data = $"age  {s}hp   {s}energ{s}food {s}water";

			if (extended) {
				data += $"{s}heals{s}slept{s}eaten{s}drank{s}{"urge",-9}{s}{"emotion",-12}"
						+ $"{s}{"death by",-12}{s}asleep";
			}

			return data;
		}

		public static SInitLifeform? CSVToInit (string csv) {
			object init = new SInitLifeform();
			FieldInfo[] fields = typeof(SInitLifeform).GetFields();
			double[] values = Array.ConvertAll(csv.Split(','), double.Parse);

			if (fields.Length != values.Length) {
				Console.WriteLine($"Number of values must match the number of SInitLifeform properties. v:{values.Length}");
				return null;
			}

			for (int i = 0; i < fields.Length; ++i) {
				fields[i].SetValue(init, values[i]);
			}

			return (SInitLifeform?) init;
		}

		public static string InitToCSV (SInitLifeform? init) {
			if (init == null) {
				Console.WriteLine("SInitLifeform was null.");
				return "";
			}

			FieldInfo[] fields = typeof(SInitLifeform).GetFields();
			string csv = fields[0].GetValue(init).ToString();

			for (int i = 1; i < fields.Length; ++i) {
				csv += $",{fields[i].GetValue(init)}";
			}

			return csv;
		}

	}

}