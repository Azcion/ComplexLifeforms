using System;
using System.Collections.Generic;
using System.Linq;

namespace ComplexLifeforms {

	internal static class Program {

		private static Random _random;

		private static World _world;
		private static Lifeform[] _lifeforms;

		private static void Main () {
			_random = new Random();
			_world = new World(5000000);
			_lifeforms = new Lifeform[10000];

			for (int i = 0; i < _lifeforms.Length; ++i) {
				_lifeforms[i] = new Lifeform(_world, _random, healAmountScale:0.5);
			}

			Console.WriteLine(World.ToStringHeader('|', true));
			Console.WriteLine(_world.ToString('|', true));

			for (int i = 0; i < 550; ++i) {
				int deadCount = 0;
				
				foreach (Lifeform c in _lifeforms) {
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

				if (deadCount == _lifeforms.Length) {
					break;
				}
			}

			_lifeforms = _lifeforms.OrderByDescending(c => c.Age).ToArray();

			Console.WriteLine(_world.ToString('|', true));
			Console.WriteLine();

			// oldest and youngest four
			Console.WriteLine(Lifeform.ToStringHeader('|', true));
			Console.WriteLine(_lifeforms[0].ToString('|', true));
			Console.WriteLine(_lifeforms[1].ToString('|', true));
			Console.WriteLine(_lifeforms[2].ToString('|', true));
			Console.WriteLine(_lifeforms[3].ToString('|', true));
			Console.WriteLine(_lifeforms[_lifeforms.Length - 4].ToString('|', true));
			Console.WriteLine(_lifeforms[_lifeforms.Length - 3].ToString('|', true));
			Console.WriteLine(_lifeforms[_lifeforms.Length - 2].ToString('|', true));
			Console.WriteLine(_lifeforms[_lifeforms.Length - 1].ToString('|', true));
			Console.WriteLine();
			Console.WriteLine(MoodManager.ToStringHeader('|'));
			Console.WriteLine(_lifeforms[0].Mood.ToString('|'));
			Console.WriteLine(_lifeforms[1].Mood.ToString('|'));
			Console.WriteLine(_lifeforms[2].Mood.ToString('|'));
			Console.WriteLine(_lifeforms[3].Mood.ToString('|'));
			Console.WriteLine(_lifeforms[_lifeforms.Length - 4].Mood.ToString('|'));
			Console.WriteLine(_lifeforms[_lifeforms.Length - 3].Mood.ToString('|'));
			Console.WriteLine(_lifeforms[_lifeforms.Length - 2].Mood.ToString('|'));
			Console.WriteLine(_lifeforms[_lifeforms.Length - 1].Mood.ToString('|'));

			int[] cause = new int[5];

			foreach (Lifeform lf in _lifeforms) {
				++cause[(int) lf.DeathBy];
			}

			Console.WriteLine("\nnone |starv|dehyd|oeat |odrin");
			Console.WriteLine($"{cause[0],5}|{cause[1],5}|{cause[2],5}|{cause[3],5}|{cause[4],5}\n");

			// standard deviation of age
			int[] ages = new int[_lifeforms.Length];

			for (int i = 0; i < _lifeforms.Length; ++i) {
				ages[i] = _lifeforms[i].Age;
			}

			/*using (System.IO.StreamWriter file = new StreamWriter("ages.csv")) {
				file.Write(string.Join(",", ages));
			}*/

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
