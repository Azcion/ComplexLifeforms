namespace ComplexLifeforms {

	public class World {

		/// <summary>Constructor parameters.</summary>
		public SInitWorld Init;

		/// <summary>Amount of available food in the world.</summary>
		public double Food { get; private set; }
		/// <summary>Amount of available water in the world.</summary>
		public double Water { get; private set; }

		public int FoodUseCount { get; private set; }
		public int WaterUseCount { get; private set; }

		public World (int size, double startingFood=.1, double startingWater=.4,
				int baseHp=1000, int baseEnergy=1000, int baseFood=1000, int baseWater=1000,
				double healCost=100, double healAmount=100,
				double hpDrain=5, double energyDrain=5,
				double foodDrain=25, double waterDrain=50) {

			Init = new SInitWorld(size, startingFood, startingWater,
					baseHp, baseEnergy, baseFood, baseWater,
					healCost, healAmount, hpDrain, energyDrain, foodDrain, waterDrain);

			Food = Init.StartingFood;
			Water = Init.StartingWater;
		}

		/// <summary>
		/// Return lifeform's remaining resources to the world, as well as those making up its body.
		/// </summary>
		public void Decompose (Lifeform lifeform) {
			Food += lifeform.Food + lifeform.Init.FoodScale * Init.BaseFood;
			Water += lifeform.Water + lifeform.Init.WaterScale * Init.BaseWater;
		}

		/// <summary>
		/// Return the specified amount of food and water to the world.
		/// </summary>
		public void Reclaim (double food, double water) {
			if (food < 0 || water < 0) {
				System.Console.WriteLine($"Food and water can not be negative. f:{food} w:{water}");
				return;
			}

			Food += food;
			Water += water;
		}

		/// <summary>
		/// Reduce the specified amount of food from the world.
		/// </summary>
		public void UseFood (double amount) {
			if (amount < 0) {
				System.Console.WriteLine($"Food can not be negative. f:{amount}");
			}

			Food -= amount;

			if (Food < 0) {
				Food = 0;
			}

			++FoodUseCount;
		}

		/// <summary>
		/// Reduce the specified amount of water from the world.
		/// </summary>
		/// <param name="amount"></param>
		public void UseWater (double amount) {
			if (amount < 0) {
				System.Console.WriteLine($"Water can not be negative. w:{amount}");
			}

			Water -= amount;

			if (Water < 0) {
				Water = 0;
			}

			++WaterUseCount;
		}

		public string ToString (char separator=' ', bool extended=false) {
			char s = separator;
			string data = $"{Init.Size,10}{s}{(int)Food,10}{s}{(int)Water,10}";

			if (extended) {
				data += $"{s}{FoodUseCount,7}{s}{WaterUseCount,7}";
			}

			return data;
		}

		public static string ToStringHeader (char separator=' ', bool extended=false) {
			char s = separator;
			string data = $"{"size",-10}{s}{"food",-10}{s}{"water",-10}";

			if (extended) {
				data += $"{s}eaten  {s}drank  ";
			}

			return data;
		}

	}

}