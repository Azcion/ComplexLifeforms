using System;
using System.Reflection;

namespace ComplexLifeforms {

	public class World {

		/// <summary>Constructor parameters.</summary>
		public readonly InitWorld Init;

		/// <summary>Amount of available food in the world.</summary>
		public int Food { get; private set; }
		/// <summary>Amount of available water in the world.</summary>
		public int Water { get; private set; }

		public int FoodUseCount { get; private set; }
		public int WaterUseCount { get; private set; }

		public World (int size, double foodScale =.1, double waterScale=.4,
				int baseHp=1000, int baseEnergy=1000,
				int baseFood=1000, int baseWater=1000,
				int healCost=50, int healAmount=100,
				int hpDrain=5, int energyDrain=50,
				int foodDrain=25, int waterDrain=50) {

			Init = new InitWorld(size, foodScale, waterScale,
					baseHp, baseEnergy,
					baseFood, baseWater,
					healCost, healAmount,
					hpDrain, energyDrain,
					foodDrain, waterDrain);

			Food = Init.StartingFood;
			Water = Init.StartingWater;
		}

		/// <summary>
		/// Return lifeform's remaining resources to the world, as well as those making up its body.
		/// </summary>
		public void Decompose (Lifeform lifeform) {
			Food += lifeform.Food + lifeform.Init.Food;
			Water += lifeform.Water + lifeform.Init.Water;
		}

		/// <summary>
		/// Return the specified amount of food and water to the world.
		/// </summary>
		public void Reclaim (int food, int water) {
			if (food < 0 || water < 0) {
				Console.WriteLine($"Food and water can not be negative. f:{food} w:{water}");
				return;
			}

			Food += food;
			Water += water;
		}

		/// <summary>
		/// Reduce the specified amount of food from the world.
		/// </summary>
		public void UseFood (int amount) {
			if (amount < 0) {
				Console.WriteLine($"Food can not be negative. f:{amount}");
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
		public void UseWater (int amount) {
			if (amount < 0) {
				Console.WriteLine($"Water can not be negative. w:{amount}");
			}

			Water -= amount;

			if (Water < 0) {
				Water = 0;
			}

			++WaterUseCount;
		}

		public string ToString (char separator=' ', bool extended=false) {
			char s = separator;
			string data = $"{Init.Size,10}{s}{Food,10}{s}{Water,10}";

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

		public static InitWorld CSVToInit (string csv) {
			object init = new InitWorld();
			FieldInfo[] fields = typeof(InitWorld).GetFields();
			double[] values = Array.ConvertAll(csv.Split(','), double.Parse);

			if (fields.Length != values.Length) {
				Console.WriteLine($"Number of values must match the number of SInitWorld properties. v:{values.Length}");
				return null;
			}

			for (int i = 0; i < fields.Length; ++i) {
				fields[i].SetValue(init, values[i]);
			}

			return (InitWorld) init;
		}

		public static string InitToCSV (InitWorld init) {
			if (init == null) {
				Console.WriteLine("SInitWorld was null.");
				return "";
			}

			FieldInfo[] fields = typeof(InitWorld).GetFields();
			string csv = fields[0].GetValue(init).ToString();

			for (int i = 1; i < fields.Length; ++i) {
				csv += $",{fields[i].GetValue(init)}";
			}

			return csv;
		}

	}

}