using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ComplexLifeforms.Enums;

namespace ComplexLifeforms {

	internal static class Program {

		private static Random _random;

		private static readonly World WORLD = new World(5000000);
		private static readonly Lifeform[] LIFEFORMS = new Lifeform[1000];

		private const int CYCLES = 1000;

		private static readonly string[][] LOG = new string[LIFEFORMS.Length][];

		[STAThread]
		private static void Main () {
			_random = new Random();

			for (int i = 0; i < LIFEFORMS.Length; ++i) {
				LIFEFORMS[i] = new Lifeform(WORLD, _random, healAmountScale:0.5);
				LOG[i] = new string[CYCLES];
			}

			Console.WriteLine(World.ToStringHeader('|', true) + "|alive ");
			Console.WriteLine(WORLD.ToString('|', true) + $"|{LIFEFORMS.Length,6}");

			for (int i = 0; i < CYCLES; ++i) {
				int deadCount = 0;
				
				for (int j = 0; j < LIFEFORMS.Length; ++j) {
					Lifeform c = LIFEFORMS[j];

					if (!c.Alive) {
						++deadCount;
						continue;
					}

					if (_random.Next(10) == 0) {
						c.Eat(_random.Next(2, 10) * WORLD.Init.FoodDrain * 3);
					}

					if (_random.Next(6) == 0) {
						c.Drink(_random.Next(2, 10) * WORLD.Init.WaterDrain * 3);
					}

					c.Update();

					LOG[j][i] = c.ToString(extended: true);
				}

				if (deadCount == LIFEFORMS.Length) {
					break;
				}
			}

			int alive = 0;

			foreach (Lifeform lifeform in LIFEFORMS) {
				if (lifeform.Alive) {
					++alive;
				}
			}

			Console.WriteLine(WORLD.ToString('|', true) + $"|{alive,6}");
			Console.WriteLine();

			Lifeform[] lifeforms = LIFEFORMS.OrderByDescending(c => c.Age).ToArray();

			if (lifeforms.Length < 8) {
				Console.WriteLine(Lifeform.ToStringHeader('|', true));

				foreach (Lifeform lifeform in lifeforms) {
					Console.WriteLine(lifeform.ToString('|', true));
				}

				Console.WriteLine();
				Console.WriteLine(MoodManager.ToStringHeader('|'));

				foreach (Lifeform lifeform in lifeforms) {
					Console.WriteLine(lifeform.Mood.ToString('|'));
				}

				return;
			}

			// oldest and youngest four
			Console.WriteLine(Lifeform.ToStringHeader('|', true));

			for (int i = 0; i < 4; ++i) {
				Console.WriteLine(lifeforms[i].ToString('|', true));
			}

			for (int i = 5; i > 1; --i) {
				Console.WriteLine(lifeforms[lifeforms.Length - i].ToString('|', true));
			}

			Console.WriteLine($"\n{"Urges",-29}||{"Emotions",-39}");
			Console.WriteLine(MoodManager.ToStringHeader('|', true));

			for (int i = 0; i < 4; ++i) {
				Console.WriteLine(lifeforms[i].Mood.ToString('|', true));
			}

			for (int i = 5; i > 1; --i) {
				Console.WriteLine(lifeforms[lifeforms.Length - i].Mood.ToString('|', true));
			}

			// statistics
			int[] urgeStats = new int[MoodManager.URGE_COUNT];
			int[] emotionStats = new int[MoodManager.EMOTION_COUNT];
			int[] deathByStats = new int[Enum.GetNames(typeof(DeathBy)).Length];

			foreach (Lifeform lifeform in lifeforms) {
				++urgeStats[(int) lifeform.Mood.Urge];
				++emotionStats[(int) lifeform.Mood.Emotion];
				++deathByStats[(int) lifeform.DeathBy];
			}

			Console.WriteLine($"\n{"Urges",-29}||{"Emotions",-39}||{"Causes of death",-31}");
			Console.WriteLine(MoodManager.ToStringHeader('|') + "||none|strv|dhyd|oeat|ohyd|exhs");

			foreach (int u in urgeStats) {
				Console.Write($"{u,4}|");
			}
			
			Console.Write('|');

			foreach (int e in emotionStats) {
				Console.Write($"{e,4}|");
			}

			foreach (int d in deathByStats) {
				Console.Write($"|{d,4}");
			}

			Console.WriteLine();

			// standard deviation of age
			int[] ages = new int[lifeforms.Length];

			for (int i = 0; i < lifeforms.Length; ++i) {
				ages[i] = lifeforms[i].Age;
			}

			double[] res = StandardDeviation(ages);
			Console.WriteLine($"\nmean: {res[1]:0.####}\nsdev: {res[0]:0.####}");

			// copy ages as CSV to clipboard
			Clipboard.SetText(string.Join(",", ages));

			// debugging
			Console.WriteLine();

			for (int i = 0; i < LIFEFORMS.Length; ++i) {
				Lifeform lifeform = LIFEFORMS[i];

				if (lifeform.Hp == -1 && lifeform.DeathBy == DeathBy.None) {
					foreach (string cycle in LOG[i]) {
						if (string.IsNullOrEmpty(cycle)) {
							continue;
						}

						Console.WriteLine(cycle);
					}

					break;
				}
			}
		}

		public static double[] StandardDeviation (IEnumerable<int> values) {
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
