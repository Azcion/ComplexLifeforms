using System;

namespace ComplexLifeforms {

	public class World {

		/// <summary>Column separator for ToString and ToStringHeader methods. Default=' '</summary>
		public static char Separator = ' ';

		/// <summary>Adds statistical data to ToString and ToStringHeader methods. Default=false</summary>
		public static bool Extended = false;

		/// <summary>Constructor parameters.</summary>
		public readonly InitWorld Init;

		private int _food;
		private int _water;

		private int _foodUseCount;
		private int _waterUseCount;

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

			_food = Init.StartingFood;
			_water = Init.StartingWater;
		}

		/// <summary>Amount of available food in the world.</summary>
		public int Food => _food;

		/// <summary>Amount of available water in the world.</summary>
		public int Water => _water;

		public static string ToStringHeader () {
			char s = Separator;
			string data = $"{"size",-10}{s}{"food",-10}{s}{"water",-10}";

			if (Extended) {
				data += $"{s}eaten   {s}drank   ";
			}

			return data;
		}

		/// <summary>
		/// Return lifeform's remaining resources to the world, as well as those making up its body.
		/// </summary>
		public void Decompose (int food, int water) {
			_food += food;
			_water += water;
		}

		/// <summary>
		/// Return the specified amount of food and water to the world.
		/// </summary>
		public void Reclaim (int food, int water) {
			if (food < 0 || water < 0) {
				Console.WriteLine($"Food and water can not be negative. f:{food} w:{water}");
				return;
			}

			_food += food;
			_water += water;
		}

		/// <summary>
		/// Reduce the specified amount of food from the world.
		/// </summary>
		public void UseFood (int amount) {
			if (amount < 0) {
				Console.WriteLine($"Food can not be negative. f:{amount}");
			}

			_food -= amount;

			if (_food < 0) {
				_food = 0;
			}

			++_foodUseCount;
		}

		/// <summary>
		/// Reduce the specified amount of water from the world.
		/// </summary>
		public void UseWater (int amount) {
			if (amount < 0) {
				Console.WriteLine($"Water can not be negative. w:{amount}");
			}

			_water -= amount;

			if (_water < 0) {
				_water = 0;
			}

			++_waterUseCount;
		}

		public override string ToString () {
			char s = Separator;
			string data = $"{Init.Size,10}{s}{_food,10}{s}{_water,10}";

			if (Extended) {
				data += $"{s}{_foodUseCount,8}{s}{_waterUseCount,8}";
			}

			return data;
		}

	}

}