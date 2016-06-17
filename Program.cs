using System;
using System.Linq;
using ComplexLifeforms.Enums;

namespace ComplexLifeforms {

	internal static class Program {

		private const int CYCLES = 1000;
		private const bool LOGGING = false;

		private static readonly World WORLD = new World(5000000);
		private static readonly Lifeform[] LIFEFORMS = new Lifeform[10000];
		private static readonly string[][] LOG = new string[LIFEFORMS.Length][];

		private static Random _random;

		private static void Main () {
			Console.Title = typeof(Program).Assembly.GetName().Version.ToString();

			int seed = Environment.TickCount;
			_random = new Random(seed);

			for (int i = 0; i < LIFEFORMS.Length; ++i) {
				LIFEFORMS[i] = new Lifeform(WORLD, _random);

				if (LOGGING) {
					LOG[i] = new string[CYCLES];
				}
			}

			Console.WriteLine(World.ToStringHeader('|', true) + "|alive " + $"{seed,54}");
			Console.Write(WORLD.ToString('|', true) + $"|{LIFEFORMS.Length,6}");

			int cursorTop = Console.CursorTop;
			int cursorLeft = Console.CursorLeft;
			Console.WriteLine("\nProcessing cycles...");
			
			int updates = 0;
			for (int i = 0; i < CYCLES; ++i) {
				int deadCount = 0;

				for (int j = 0; j < LIFEFORMS.Length; ++j) {
					Lifeform c = LIFEFORMS[j];

					if (!c.Alive) {
						++deadCount;
						continue;
					}

					if (_random.Next(10) == 0) {
						c.Eat(_random.Next(10, 20) * WORLD.Init.FoodDrain * 3);
					}

					if (_random.Next(5) == 0) {
						c.Drink(_random.Next(2, 10) * WORLD.Init.WaterDrain * 3);
					}

					c.Update();
					++updates;

					if (LOGGING) {
						LOG[j][i] = c.ToString(extended: true);
					}
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

			Console.SetCursorPosition(cursorLeft, cursorTop);
			Console.WriteLine($"{updates + " cycles",54}");
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

			Console.WriteLine($"\n{"Urges",-29}||{"Emotions",-39}||{"Causes of death",-36}");
			Console.WriteLine(MoodManager.ToStringHeader('|') + "||none|strv|dhyd|oeat|ohyd|exhs|maln");

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

			double[] res = Utils.StandardDeviation(ages);
			Console.WriteLine($"\nmean: {res[1]:0.####}\nsdev: {res[0]:0.####}");

			int cursorBottom = Console.CursorTop;
			Console.SetCursorPosition(cursorLeft, cursorTop + 1);
			Console.WriteLine($"{Environment.TickCount - seed + " ms",54}");
			Console.SetCursorPosition(0, cursorBottom);
			
			// debugging
			if (!LOGGING) {
				return;
			}

			Console.WriteLine();

			for (int i = 0; i < LIFEFORMS.Length; ++i) {
				Lifeform lifeform = LIFEFORMS[i];

				if (!lifeform.Alive || lifeform.DeathBy != DeathBy.None) {
					continue;
				}

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

}
