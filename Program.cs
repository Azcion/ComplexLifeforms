using System;
using System.Collections.Generic;
using System.Linq;
using ComplexLifeforms.Enums;

namespace ComplexLifeforms {

	internal static class Program {

		private const int COUNT = 10000;
		private const int CYCLES = 1000;

		private static readonly World WORLD = new World(5000000);
		private static readonly HashSet<Lifeform> LIFEFORMS = new HashSet<Lifeform>();
		private static readonly string[][] LOG = new string[COUNT][];

		private static Random _random;

		private static void Main () {
			Console.Title = typeof(Program).Assembly.GetName().Version.ToString();
			World.Separator = '|';
			World.Extended = true;
			Lifeform.Separator = '|';
			Lifeform.Extended = true;
			Lifeform.Logging = false;
			MoodManager.Separator = '|';
			MoodManager.Extended = true;

			int seed = Environment.TickCount;
			_random = new Random(seed);

			for (int i = 0; i < COUNT; ++i) {
				LIFEFORMS.Add(new Lifeform(WORLD, _random));
			}

			Console.WriteLine(World.ToStringHeader() + "|alive " + $"{seed,54}");
			Console.Write(WORLD + $"|{LIFEFORMS.Count,6}");

			int cursorTop = Console.CursorTop;
			int cursorLeft = Console.CursorLeft;
			Console.WriteLine("\nProcessing cycles...");
			
			int updates = 0;
			for (int i = 0; i < CYCLES; ++i) {
				int deadCount = 0;

				foreach (Lifeform lifeform in LIFEFORMS) {
					if (!lifeform.Alive) {
						++deadCount;
						continue;
					}

					if (_random.Next(10) == 0) {
						lifeform.Eat(_random.Next(10, 20) * WORLD.Init.FoodDrain * 3);
					}

					if (_random.Next(5) == 0) {
						lifeform.Drink(_random.Next(2, 10) * WORLD.Init.WaterDrain * 3);
					}

					lifeform.Update();
					++updates;
				}

				if (deadCount == LIFEFORMS.Count) {
					break;
				}
			}

			int alive = LIFEFORMS.Count(lifeform => lifeform.Alive);

			Console.SetCursorPosition(cursorLeft, cursorTop);
			Console.WriteLine($"{updates + " cycles",54}");
			Console.WriteLine(WORLD + $"|{alive,6}");
			Console.WriteLine();

			Lifeform[] lifeforms = LIFEFORMS.OrderByDescending(c => c.Age).ToArray();

			if (lifeforms.Length < 8) {
				Console.WriteLine(Lifeform.ToStringHeader());

				foreach (Lifeform lifeform in lifeforms) {
					Console.WriteLine(lifeform.ToString());
				}

				Console.WriteLine();
				Console.WriteLine(MoodManager.ToStringHeader());

				foreach (Lifeform lifeform in lifeforms) {
					Console.WriteLine(lifeform.Mood.ToString());
				}

				return;
			}

			// oldest and youngest four
			Console.WriteLine(Lifeform.ToStringHeader());

			for (int i = 0; i < 4; ++i) {
				Console.WriteLine(lifeforms[i].ToString());
			}

			for (int i = 5; i > 1; --i) {
				Console.WriteLine(lifeforms[lifeforms.Length - i].ToString());
			}

			Console.WriteLine($"\n{"Urges",-29}||{"Emotions",-39}");
			Console.WriteLine(MoodManager.ToStringHeader());

			for (int i = 0; i < 4; ++i) {
				Console.WriteLine(lifeforms[i].Mood.ToString());
			}

			for (int i = 5; i > 1; --i) {
				Console.WriteLine(lifeforms[lifeforms.Length - i].Mood.ToString());
			}

			// statistics
			int[] urgeStats = new int[MoodManager.URGE_COUNT];
			int[] emotionStats = new int[MoodManager.EMOTION_COUNT];
			int[] moodStats = new int[Enum.GetNames(typeof(Mood)).Length];
			int[] deathByStats = new int[Enum.GetNames(typeof(DeathBy)).Length];

			foreach (Lifeform lifeform in lifeforms) {
				++urgeStats[(int) lifeform.Mood.Urge];
				++emotionStats[(int) lifeform.Mood.Emotion];
				++moodStats[(int) lifeform.Mood.Mood];
				++deathByStats[(int) lifeform.DeathBy];
			}

			MoodManager.Extended = false;
			Console.WriteLine($"\n{"Urges",-29}||{"Emotions",-39}");
			Console.WriteLine(MoodManager.ToStringHeader());

			foreach (int u in urgeStats) {
				Console.Write($"{u,4}|");
			}

			foreach (int e in emotionStats) {
				Console.Write($"|{e,4}");
			}

			Console.WriteLine($"\n\n{"Moods",-24}||{"Causes of death",-36}");
			Console.WriteLine($"grea|good|neut|bad |terr||none|strv|dhyd|oeat|ohyd|exhs|maln");

			foreach (int m in moodStats) {
				Console.Write($"{m,4}|");
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
			if (!Lifeform.Logging) {
				return;
			}

			Console.WriteLine();

			foreach (Lifeform lifeform in LIFEFORMS) {
				if (!lifeform.Alive && lifeform.DeathBy != DeathBy.None) {
					continue;
				}

				Console.WriteLine("ID: " + lifeform.Id);
				Console.WriteLine(Lifeform.ToStringHeader());

				foreach (string cycle in LOG[lifeform.Id]) {
					if (string.IsNullOrEmpty(cycle)) {
						continue;
					}

					Console.WriteLine(cycle);
				}

				break;
			}
		}

		private static void RunSeeds () {
			int line = Console.CursorTop + 1;

			for (int seed = 0; seed < 500; ++seed) {
				Console.SetCursorPosition(0, 0);
				Console.WriteLine($"Processing seed {seed}...");
				Console.SetCursorPosition(0, line);

				if (Run(seed)) {
					++line;
				}
			}
		}

		private static bool Run (int seed) {
			World world = new World(5000000);
			Random random = new Random(seed);

			for (int i = 0; i < LIFEFORMS.Count; ++i) {
				LIFEFORMS.Add(new Lifeform(world, random));
			}

			for (int i = 0; i < CYCLES; ++i) {
				int deadCount = 0;

				foreach (Lifeform lifeform in LIFEFORMS) {
					if (!lifeform.Alive) {
						++deadCount;
						continue;
					}

					if (random.Next(10) == 0) {
						lifeform.Eat(random.Next(10, 20) * world.Init.FoodDrain * 3);
					}

					if (random.Next(5) == 0) {
						lifeform.Drink(random.Next(2, 10) * world.Init.WaterDrain * 3);
					}

					lifeform.Update();
				}

				if (deadCount == LIFEFORMS.Count) {
					break;
				}
			}

			int nones = LIFEFORMS.Count(lifeform => lifeform.DeathBy == DeathBy.None);

			if (nones <= 0) {
				return false;
			}

			Console.WriteLine($"nones: {nones,5} seed: {seed}");
			return true;
		}

	}

}
