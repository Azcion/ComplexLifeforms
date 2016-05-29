using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ComplexLifeforms {

	internal static class Program {

		private static Random _random;

		private static World _world;
		private static Lifeform[] _creatures;

		private static void Main () {
			_random = new Random();
			_world = new World(5000000);
			_creatures = new Lifeform[100];

			for (int i = 0; i < _creatures.Length; ++i) {
				_creatures[i] = new Lifeform(_world);
			}

			Console.WriteLine(World.ToStringHeader('|', true));
			Console.WriteLine(_world.ToString('|', true));

			for (int i = 0; i < 5000; ++i) {
				int deadCount = 0;
				
				foreach (Lifeform c in _creatures) {
					if (!c.Alive) {
						++deadCount;
						continue;
					}

					if (_random.Next(5) == 0) {
						c.Eat(_random.Next(1, 5) * _world.Init.FoodDrain * 3);
					}

					if (_random.Next(3) == 0) {
						c.Drink(_random.Next(1, 5) * _world.Init.WaterDrain * 3);
					}

					c.Update();
				}

				if (deadCount == _creatures.Length) {
					break;
				}
			}

			_creatures = _creatures.OrderByDescending(c => c.Age).ToArray();

			Console.WriteLine(_world.ToString('|', true));
			Console.WriteLine();
			Console.WriteLine(Lifeform.ToStringHeader('|', true));

			// oldest and youngest
			Console.WriteLine(_creatures.First().ToString('|', true));
			Console.WriteLine(_creatures.Last().ToString('|', true));
			Console.WriteLine();
			Console.WriteLine(_creatures.First().Mood.ToString('|'));
			Console.WriteLine(_creatures.Last().Mood.ToString('|'));

			// standard deviation of age
			int[] ages = new int[_creatures.Length];

			for (int i = 0; i < _creatures.Length; ++i) {
				ages[i] = _creatures[i].Age;
			}

			using (StreamWriter file = new StreamWriter("ages.csv")) {
				file.Write(string.Join(",", ages));
			}

			double[] res = StandardDeviation(ages);
			Console.WriteLine($"\nmean: {res[1]}\nsdev: {res[0]}");
		}

		private static double[] StandardDeviation (IEnumerable<int> values) {
			double mean = 0;
			double sum = 0;
			int i = 0;

			foreach (int val in values) {
				double delta = val - mean;
				mean += delta / ++i;
				sum += delta * (val - mean);
			}

			double[] res = {Math.Sqrt(sum / i), mean};

			return res;
		}

	}

}
