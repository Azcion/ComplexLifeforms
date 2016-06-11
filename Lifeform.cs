using System;
using System.Diagnostics.CodeAnalysis;
using ComplexLifeforms.Enums;

namespace ComplexLifeforms {

	public class Lifeform {

		private static int _id;

		private bool _pendingKill;

		public readonly int Id;

		/// <summary>Constructor parameters.</summary>
		public readonly InitLifeform Init;
		/// <summary>World in which resources will be exchanged.</summary>
		public readonly World World;
		/// <summary>Urge and emotion processor.</summary>
		public readonly MoodManager Mood;

		public readonly int HealCost;
		public readonly int HealAmount;

		public readonly int HpDrain;
		public readonly int EnergyDrain;
		public readonly int FoodDrain;
		public readonly int WaterDrain;

		public readonly int HealThreshold;
		public readonly int SleepThreshold;
		public readonly int EatThreshold;
		public readonly int DrinkThreshold;

		public bool Alive { get; private set; }
		public DeathBy DeathBy { get; private set; }

		public int Hp { get; private set; }
		public int Energy { get; private set; }
		public int Food { get; private set; }
		public int Water { get; private set; }

		public int Age { get; private set; }
		public int HealCount { get; private set; }
		public int SleepCount { get; private set; }
		public int EatCount { get; private set; }
		public int DrinkCount { get; private set; }

		/// <summary>
		/// Unpacks SInitLifeform and uses its values.
		/// </summary>
		[SuppressMessage ("ReSharper", "UnusedMember.Global")]
		public Lifeform (World world, InitLifeform init, Random random=null)
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
			Mood = new MoodManager(this, random);
			Alive = true;
			DeathBy = DeathBy.None;

			InitWorld w = world.Init;

			Init = new InitLifeform(w.BaseHp, w.BaseEnergy,
					w.BaseFood, w.BaseWater,
					hpScale, energyScale,
					foodScale, waterScale,
					healCostScale, healAmountScale,
					hpDrainScale, energyDrainScale,
					foodDrainScale, waterDrainScale,
					healThreshold, sleepThreshold,
					eatThreshold, drinkThreshold);

			HealCost = (int) (w.HealCost * healCostScale);
			HealAmount = (int) (w.HealAmount * healAmountScale);

			HpDrain = (int) (w.HpDrain * hpDrainScale);
			EnergyDrain = (int) (w.EnergyDrain * energyDrainScale);
			FoodDrain = (int) (w.FoodDrain * foodDrainScale);
			WaterDrain = (int) (w.WaterDrain * waterDrainScale);

			Hp = (int) (w.BaseHp * hpScale);
			Energy = (int) (w.BaseEnergy * energyScale);
			Food = (int) (w.BaseFood * foodScale);
			Water = (int) (w.BaseWater * waterScale);

			HealThreshold = (int) (Hp * healThreshold);
			SleepThreshold = (int) (Energy * sleepThreshold);
			EatThreshold = (int) (Food * eatThreshold);
			DrinkThreshold = (int) (Water * drinkThreshold);
		}

		public void Update () {
			if (!Alive) {
				return;
			}

			++Age;

			ProcessBodilyFunctions();
			Heal();

			if (Mood.Asleep) {
				Hp += HpDrain * 2;
				Energy += EnergyDrain * 8;
				++SleepCount;

				if (Energy >= Init.Energy || Hp < HpDrain * 32) {
					Mood.Asleep = false;
				}
			}

			if (!Mood.Asleep && Energy < SleepThreshold) {
				if (Energy < 0) {
					Sleep(true);
				}

				Sleep();
			}

			Mood.Update();

			if (Hp <= 0 || _pendingKill || DeathBy != DeathBy.None) {
				Kill();
			}
		}

		private void ProcessBodilyFunctions () {
			bool excreteOne = false;
			bool excreteTwo = false;
			int deltaHp = 0;
			int deltaEnergy = 0;
			int deltaFood = 0;
			int deltaWater = 0;
			DeathBy deltaDeathBy = DeathBy.None;

			if (Water > Init.Water) {
				excreteOne = true;
				deltaHp -= HpDrain * 8;
				deltaEnergy -= EnergyDrain * 8;
				deltaWater -= WaterDrain * 8;
				deltaDeathBy = DeathBy.Overhydration;
			} else if (Water > DrinkThreshold) {
				excreteOne = true;
				deltaEnergy -= EnergyDrain * 4;
				deltaWater -= WaterDrain * 4;
			} else if (Water > WaterDrain) {
				deltaHp -= HpDrain / 2;
				deltaEnergy -= EnergyDrain;
				deltaWater -= WaterDrain;
			} else if (Water > 0) {
				deltaHp -= HpDrain * 4;
				deltaEnergy -= EnergyDrain * 2;
				deltaWater -= Water;
			} else {
				deltaHp -= HpDrain * 16;
				deltaEnergy -= EnergyDrain * 4;

				if (deltaDeathBy == DeathBy.None) {
					deltaDeathBy = DeathBy.Dehydration;
				}
			}

			if (Food > Init.Food) {
				excreteTwo = true;
				deltaHp -= HpDrain * 8;
				deltaEnergy -= EnergyDrain * 8;
				deltaFood -= FoodDrain * 8;
				deltaDeathBy = DeathBy.Overeating;
			} else if (Food > EatThreshold) {
				excreteTwo = true;
				deltaEnergy -= EnergyDrain * 4;
				deltaFood -= FoodDrain * 4;
			} else if (Food > FoodDrain) {
				deltaHp -= HpDrain / 2;
				deltaEnergy -= EnergyDrain;
				deltaFood -= FoodDrain;
			} else if (Food > 0) {
				deltaHp -= HpDrain * 4;
				deltaEnergy -= EnergyDrain * 2;
				deltaFood -= Food;
			} else {
				deltaHp -= HpDrain * 16;
				deltaEnergy -= EnergyDrain * 4;
				deltaDeathBy = DeathBy.Starvation;
			}

			if (Mood.Asleep) {
				deltaEnergy = 0;
				deltaFood = 0;
				deltaWater = 0;
			} else {
				if (excreteOne) {
					Mood.AffectUrge(Urge.Drink, -1);
					Mood.Action(Urge.Excrete);
				}

				if (excreteTwo) {
					Mood.AffectUrge(Urge.Eat, -1);
					Mood.Action(Urge.Excrete);
				}
			}

			World.Reclaim(-deltaFood, -deltaWater);

			Hp += deltaHp;
			Energy += deltaEnergy;
			Food += deltaFood;
			Water += deltaWater;

			if (Hp < 0 && DeathBy == DeathBy.None && deltaDeathBy != DeathBy.None) {
				DeathBy = deltaDeathBy;
				_pendingKill = true;
			}

			if (Energy < 0) {
				Energy = 0;
			}

			if (Food < 0) {
				Food = 0;
			}

			if (Water < 0) {
				Water = 0;
			}
		}

		private void Sleep (bool didPassOut=false) {
			Mood.Asleep = true;

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
			if (Hp > HealThreshold || Mood.Asleep) {
				return;
			}

			int deltaEnergy = 0;
			int deltaFood = 0;
			int deltaWater = 0;
			int cost = HealCost + Age;
			int effectiveness = cost + cost;

			if (Food <= cost) {
				Mood.AffectUrge(Urge.Eat, 1);

				if (Food > 0) {
					deltaEnergy -= cost / 2;
					deltaFood -= Food;
					effectiveness -= Food;
				} else {
					effectiveness -= cost;
				}
			} else {
				deltaEnergy -= cost;
				deltaFood -= cost;
			}

			if (Water <= cost) {
				Mood.AffectUrge(Urge.Drink, 1);

				if (Water > 0) {
					deltaEnergy -= cost / 2;
					deltaWater -= Water;
					effectiveness -= Water;
				} else {
					effectiveness -= cost;
				}
			} else {
				deltaEnergy -= cost;
				deltaWater -= cost;
			}

			World.Reclaim(-deltaFood, -deltaWater);
			Mood.Action(Urge.Heal);
			Mood.AffectUrge(Urge.Sleep, 2);

			Hp += effectiveness / (cost + cost) * HealAmount;
			Energy += deltaEnergy;
			Food += deltaFood;
			Water += deltaWater;
			++HealCount;
		}

		private void Kill () {
			if (_pendingKill) {
				_pendingKill = false;
			}

			Alive = false;

			if (DeathBy == DeathBy.None) {
				if (Energy <= 0) {
					DeathBy = DeathBy.Exhaustion;
				} else if (Food <= 0) {
					DeathBy = DeathBy.Starvation;
				} else if (Water <= 0) {
					DeathBy = DeathBy.Dehydration;
				}
			}

			Hp = -1;
			World.Decompose(this);
		}

		public void Eat (int amount) {
			if (!Alive || Mood.Asleep || _pendingKill) {
				return;
			}

			int deltaEnergy = 0;
			int deltaFood = 0;
			int deltaWater = 0;

			if (World.Food < amount) {
				amount = World.Food;
			}

			if (Food < EatThreshold) {
				deltaEnergy -= EnergyDrain;
				deltaFood += amount;
				deltaWater -= WaterDrain / 2;
			} else {
				deltaEnergy -= EnergyDrain / 2;
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

			Energy += deltaEnergy;
			Food += deltaFood;
			Water += deltaWater;
			++EatCount;

			if (Food > Init.Food) {
				Hp -= HpDrain * 4;

				Mood.AffectUrge(Urge.Eat, -2);
				Mood.AffectUrge(Urge.Excrete, 1);
			}

			if (Hp <= 0 && DeathBy == DeathBy.None) {
				DeathBy = DeathBy.Overeating;
				_pendingKill = true;
			}
		}

		public void Drink (int amount) {
			if (!Alive || Mood.Asleep || _pendingKill) {
				return;
			}

			int deltaEnergy = 0;
			int deltaWater = 0;

			if (World.Water < amount) {
				amount = World.Water;
			}

			if (Water < DrinkThreshold) {
				deltaEnergy -= EnergyDrain;
				deltaWater += amount;
			} else {
				deltaEnergy -= EnergyDrain / 2;
				deltaWater += amount / 2;
			}

			World.UseWater(deltaWater);
			Mood.Action(Urge.Drink);

			Energy += deltaEnergy;
			Water += deltaWater;
			++DrinkCount;

			if (Water > Init.Water) {
				Hp -= HpDrain * 4;

				Mood.AffectUrge(Urge.Drink, -2);
				Mood.AffectUrge(Urge.Excrete, 1);
			}

			if (Hp <= 0 && DeathBy == DeathBy.None) {
				DeathBy = DeathBy.Overhydration;
				_pendingKill = true;
			}
		}

		public string ToString (char separator=' ', bool extended=false) {
			char s = separator;
			string data = $"{Age,5}{s}{Hp,5}{s}{Energy,5}{s}{Food,5}{s}{Water,5}";

			if (extended) {
				string emotion = MoodManager.EmotionName(Mood.EmotionValues);

				data += $"{s}{HealCount,5}{s}{SleepCount,5}{s}{EatCount,5}{s}{DrinkCount,5}"
						+ $"{s}{Mood.Urge,-9}{s}{emotion,-12}{s}{Mood.Mood,-8}"
						+ $"{s}{DeathBy,-13}{s}{(Mood.Asleep ? "yes" : "no"),-6}";
			}

			return data;
		}

		public static string ToStringHeader (char separator=' ', bool extended=false) {
			char s = separator;
			string data = $"age  {s}hp   {s}energ{s}food {s}water";

			if (extended) {
				data += $"{s}heals{s}slept{s}eaten{s}drank{s}{"urge",-9}{s}{"emotion",-12}"
						+ $"{s}{"mood",-8}{s}{"death by",-13}{s}asleep";
			}

			return data;
		}

	}

}