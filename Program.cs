using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ComplexLifeforms.Enums;

namespace ComplexLifeforms {

	internal static class Program {

		private const int COUNT = 10000;
		private const int CYCLES = 1000;

		private static readonly World WORLD = new World(5000000);
		private static readonly HashSet<Lifeform> LIFEFORMS = new HashSet<Lifeform>();

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

			OldestAndYoungest(4, 2, false);
			Statistics(true);

			int cursorBottom = Console.CursorTop;
			Console.SetCursorPosition(cursorLeft, cursorTop + 1);
			Console.WriteLine($"{Environment.TickCount - seed + " ms",54}");
			Console.SetCursorPosition(0, cursorBottom);
			
			FindErrors();
		}

		private static void OldestAndYoungest (int oldest, int youngest, bool data) {
			Lifeform[] lifeforms = LIFEFORMS.OrderByDescending(c => c.Age).ToArray();

			if (lifeforms.Length < oldest + youngest) {
				Console.WriteLine(Lifeform.ToStringHeader());

				foreach (Lifeform lifeform in lifeforms) {
					Console.WriteLine(lifeform.ToString());
				}

				if (!data) {
					return;
				}

				Console.WriteLine();
				Console.WriteLine(MoodManager.ToStringHeader());

				foreach (Lifeform lifeform in lifeforms) {
					Console.WriteLine(lifeform.Mood.ToString());
				}

				return;
			}

			Console.WriteLine(Lifeform.ToStringHeader());

			for (int i = 0; i < oldest; ++i) {
				Console.WriteLine(lifeforms[i].ToString());
			}

			for (int i = youngest + 1; i > 1; --i) {
				Console.WriteLine(lifeforms[lifeforms.Length - i].ToString());
			}

			if (!data) {
				return;
			}

			Console.WriteLine($"\n{"Urges",-29}||{"Emotions",-39}");
			Console.WriteLine(MoodManager.ToStringHeader());

			for (int i = 0; i < oldest; ++i) {
				Console.WriteLine(lifeforms[i].Mood.ToString());
			}

			for (int i = youngest + 1; i > 1; --i) {
				Console.WriteLine(lifeforms[lifeforms.Length - i].Mood.ToString());
			}
		}

		private static void Statistics (bool sdev) {
			int[] urgeStats = new int[MoodManager.URGE_COUNT];
			int[] emotionStats = new int[MoodManager.EMOTION_COUNT];
			int[] moodStats = new int[Enum.GetNames(typeof(Mood)).Length];
			int[] deathByStats = new int[Enum.GetNames(typeof(DeathBy)).Length];

			foreach (Lifeform lifeform in LIFEFORMS) {
				++urgeStats[(int) lifeform.Mood.Urge];
				++emotionStats[(int) lifeform.Mood.Emotion];
				++moodStats[(int) lifeform.Mood.Mood];
				++deathByStats[(int) lifeform.DeathBy];
			}

			string data = Enum.GetNames(typeof(Mood)).Aggregate(
					"|", (current, mood) => current + $"|{Utils.Truncate(mood, 4),-4}"
			);

			MoodManager.Extended = false;
			Console.WriteLine($"\n{"Urges",-29}||{"Emotions",-39}||{"Moods",-24}");
			Console.WriteLine(MoodManager.ToStringHeader() + data);

			foreach (int u in urgeStats) {
				Console.Write($"{u,4}|");
			}

			foreach (int e in emotionStats) {
				Console.Write($"|{e,4}");
			}

			Console.Write('|');

			foreach (int m in moodStats) {
				Console.Write($"|{m,4}");
			}

			Console.WriteLine($"\n\n{"Causes of death",-36}");
			Console.WriteLine("none|strv|dhyd|oeat|ohyd|exhs|maln");

			Console.Write($"{deathByStats[0],4}");
			for (int i = 1; i < deathByStats.Length; ++i) {
				Console.Write($"|{deathByStats[i],4}");
			}

			Console.WriteLine();

			if (!sdev) {
				return;
			}

			int[] ages = new int[LIFEFORMS.Count];

			int index = 0;
			foreach (Lifeform lifeform in LIFEFORMS) {
				ages[index++] = lifeform.Age;
			}

			double[] res = Utils.StandardDeviation(ages);
			Console.WriteLine($"\nmean: {res[1]:0.####}\nsdev: {res[0]:0.####}");
		}

		private static void FindErrors () {
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

				foreach (string cycle in lifeform.Log) {
					if (string.IsNullOrEmpty(cycle)) {
						continue;
					}

					Console.WriteLine(cycle);
				}

				break;
			}
		}

		[SuppressMessage ("ReSharper", "UnusedMember.Local")]
		private static void RunSeeds () {
			int line = Console.CursorTop + 1;

			for (int seed = 0; seed < 500; ++seed) {
				Console.SetCursorPosition(0, 0);
				Console.WriteLine($"Processing seed {seed}...");
				Console.SetCursorPosition(0, line);

				if (FindNones(seed)) {
					++line;
				}
			}
		}

		private static bool FindNones (int seed) {
			World world = new World(5000000);
			HashSet<Lifeform> lifeforms = new HashSet<Lifeform>();
			Random random = new Random(seed);

			for (int i = 0; i < lifeforms.Count; ++i) {
				lifeforms.Add(new Lifeform(world, random));
			}

			for (int i = 0; i < CYCLES; ++i) {
				int deadCount = 0;

				foreach (Lifeform lifeform in lifeforms) {
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

				if (deadCount == lifeforms.Count) {
					break;
				}
			}

			int nones = lifeforms.Count(lifeform => lifeform.DeathBy == DeathBy.None);

			if (nones <= 0) {
				return false;
			}

			Console.WriteLine($"nones: {nones,5} seed: {seed}");
			return true;
		}

	}

}
