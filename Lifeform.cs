using System;

namespace ComplexLifeforms {

	public class Lifeform {

		/// <summary>Constructor parameters.</summary>
		public readonly SInitLifeform Init;

		/// <summary>World in which resources will be exchanged.</summary>
		public readonly World World;
		public readonly MoodManager Mood;

		public readonly double HealCost;
		public readonly double HealAmount;
		public readonly double HpDrain;
		public readonly double FoodDrain;
		public readonly double WaterDrain;

		public readonly double HealThreshold;
		public readonly double EatThreshold;
		public readonly double DrinkThreshold;

		private static int _id;
		public readonly int Id;

		public bool Alive { get; private set; }
		public DeathBy DeathBy { get; private set; }

		public double Hp { get; private set; }
		public double Food { get; private set; }
		public double Water { get; private set; }

		public int Age { get; private set; }
		public int HealCount { get; private set; }
		public int EatCount { get; private set; }
		public int DrinkCount { get; private set; }

		public Lifeform (World world, Random random=null,
				double hpScale=1.0, double foodScale=1.0, double waterScale=1.0,
				double healCostScale=1.0, double healAmountScale=1.0,
				double hpDrainScale=1.0, double foodDrainScale=1.0, double waterDrainScale=1.0,
				double healThreshold=0.5, double eatThreshold=0.5, double drinkThreshold=0.5) {
			Id = _id++;
			World = world;
			Init = new SInitLifeform(hpScale, foodScale, waterScale, healCostScale,
					healAmountScale, hpDrainScale, foodDrainScale, waterDrainScale,
					healThreshold, eatThreshold, drinkThreshold);
			Mood = new MoodManager(random);

			SInitWorld init = world.Init;

			HealCost = init.HealCost * healCostScale;
			HealAmount = init.HealAmount * healAmountScale;
			HpDrain = init.HpDrain * hpDrainScale;
			FoodDrain = init.FoodDrain * foodDrainScale;
			WaterDrain = init.WaterDrain * waterDrainScale;

			Alive = true;
			DeathBy = DeathBy.None;

			Hp = init.BaseHp * hpScale;
			Food = init.BaseFood * foodScale;
			Water = init.BaseWater * waterScale;

			HealThreshold = Hp * healThreshold;
			EatThreshold = Food * eatThreshold;
			DrinkThreshold = Water * drinkThreshold;
		}

		public void Update () {
			if (!Alive) {
				return;
			}

			++Age;

			ProcessBodilyFunctions();
			Mood.Update();

			if (Hp > 0 && Hp < HealThreshold) {
				Heal();
			}

			if (Hp < 0) {
				Kill();
			}
		}

		private void ProcessBodilyFunctions () {
			double deltaHp = 0;
			double deltaFood = 0;
			double deltaWater = 0;

			if (Food > 0) {
				if (Food > FoodDrain) {
					if (Food > EatThreshold) {
						deltaHp += HpDrain / 2;
						deltaFood -= FoodDrain * 2;
						
						Mood.AffectUrge(Urge.Eat, -1);
						Mood.Action(Urge.Excrete);
					} else {
						deltaHp -= HpDrain / 2;
						deltaFood -= FoodDrain;
					}
				} else {
					deltaHp -= HpDrain * 5;
					deltaFood -= Food;
				}
			} else {
				deltaHp -= HpDrain * 10;
			}

			if (Water > 0) {
				if (Water > WaterDrain) {
					if (Water > DrinkThreshold) {
						deltaHp += HpDrain / 2;
						deltaWater -= WaterDrain * 2;

						Mood.AffectUrge(Urge.Drink, -1);
						Mood.Action(Urge.Excrete);
					} else {
						deltaHp -= HpDrain / 2;
						deltaWater -= WaterDrain;
					}
				} else {
					deltaHp -= HpDrain * 10;
					deltaWater -= Water;
				}
			} else {
				deltaHp -= HpDrain * 20;
			}

			World.Reclaim(-deltaFood, -deltaWater);

			Hp += deltaHp;
			Food += deltaFood;
			Water += deltaWater;

			if (Food < 0) {
				Food = 0;
			}

			if (Water < 0) {
				Water = 0;
			}
		}

		private void Heal () {
			bool valid = true;
			if (Food <= HealCost) {
				Mood.AffectUrge(Urge.Eat, 1);
				valid = false;
			}

			if (Water <= HealCost) {
				Mood.AffectUrge(Urge.Drink, 1);
				valid = false;
			}

			if (!valid) {
				return;
			}

			World.Reclaim(HealCost, HealCost);

			Mood.Action(Urge.Heal);

			Hp += HealAmount;
			Food -= HealCost;
			Water -= HealCost;
			++HealCount;
		}

		public void Eat (double amount) {
			if (!Alive || World.Food <= 0) {
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

			if (Hp <= 0) {
				DeathBy = DeathBy.Overeating;
			}
		}

		public void Drink (double amount) {
			if (!Alive || World.Water <= 0) {
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

			if (Hp <= 0) {
				DeathBy = DeathBy.Overdrinking;
			}
		}

		private void Kill () {
			Alive = false;

			if (Food <= 0) {
				DeathBy = DeathBy.Starvation;
			} else
			if (Water <= 0) {
				DeathBy = DeathBy.Dehydration;
			}

			Hp = -1;
			World.Decompose(this);
		}

		public string ToString (char separator=' ', bool extended=false) {
			char s = separator;
			string data = $"{Age,5}{s}{(int)Hp,5}{s}{(int)Food,5}{s}{(int)Water,5}";

			if (extended) {
				data += $"{s}{HealCount,5}{s}{EatCount,5}{s}{DrinkCount,5}"
						+ $"{s}{Mood.Urge,-9}{s}{Mood.Emotion,-12}{s}{DeathBy,-12}";
			}

			return data;
		}

		public static string ToStringHeader (char separator=' ', bool extended=false) {
			char s = separator;
			string data = $"age  {s}hp   {s}food {s}water";

			if (extended) {
				data += $"{s}heals{s}eaten{s}drank{s}{"urge",-9}{s}{"emotion",-12}{s}{"death by",-12}";
			}

			return data;
		}

	}

}