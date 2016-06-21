using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ComplexLifeforms.Enums;

namespace ComplexLifeforms {

	internal static class Program {

		private const int COUNT = 1000;
		private const int CYCLES = 10000;

		private static readonly World WORLD = new World(5000000);
		private static readonly HashSet<Lifeform> LIFEFORMS = new HashSet<Lifeform>();
		private static readonly HashSet<Lifeform> BROTHEL = new HashSet<Lifeform>();
		private static readonly HashSet<Lifeform> LIMBO = new HashSet<Lifeform>();
		private static readonly HashSet<Lifeform> GRAVEYARD = new HashSet<Lifeform>();

		private static void Main () {
			int start = Environment.TickCount;
			int seed = start;
			Utils.Random = new Random(seed);

			Console.Title = typeof(Program).Assembly.GetName().Version.ToString();
			World.Separator = '|';
			World.Extended = true;
			Lifeform.Separator = '|';
			Lifeform.TruncateTo = 5;
			Lifeform.Extended = true;
			Lifeform.Logging = false;
			MoodManager.Separator = '|';
			MoodManager.Extended = true;

			for (int i = 0; i < COUNT; ++i) {
				LIFEFORMS.Add(new Lifeform(WORLD));
			}

			Console.WriteLine(World.ToStringHeader() + "|alive |max   " + $"{seed,47}");
			Console.Write(WORLD + $"|{LIFEFORMS.Count,6}|     0");

			int cursorTop = Console.CursorTop;
			int cursorLeft = Console.CursorLeft;
			Console.WriteLine("\n\nProcessing cycles...");

			Run(cursorLeft, cursorTop);
			TopAndBottom(4, 0, false);
			Statistics(true);

			int cursorBottom = Console.CursorTop;
			Console.SetCursorPosition(cursorLeft, cursorTop + 1);
			Console.WriteLine($"{Environment.TickCount - start + "ms",47}");
			Console.SetCursorPosition(0, cursorBottom);
			
			FindErrors();
		}

		private static void Run (int cursorLeft, int cursorTop) {
			int cycles = CYCLES;
			int updates = 0;
			int maxLifeforms = COUNT;

			for (int i = 0; i < CYCLES; ++i) {
				foreach (Lifeform lifeform in LIFEFORMS) {
					if (Console.KeyAvailable) {
						ConsoleKeyInfo key = Console.ReadKey(true);

						switch (key.Key) {
							case ConsoleKey.Spacebar:
							case ConsoleKey.Escape:
								return;

						}
					}

					if (!lifeform.Alive) {
						LIMBO.Add(lifeform);
						continue;
					}

					if (lifeform.Age > 25 && lifeform.Age < 50
							&& lifeform.Mood.Mood == Mood.Great
							&& lifeform.Mood.Urge == Urge.Reproduce) {
						BROTHEL.Add(lifeform);
					} else {
						if (Utils.Random.Next(5) == 0) {
							lifeform.Eat(Utils.Random.Next(5, 10) * WORLD.Init.FoodDrain * 3);
						}

						if (Utils.Random.Next(3) == 0) {
							lifeform.Drink(Utils.Random.Next(1, 10) * WORLD.Init.WaterDrain * 3);
						}
					}

					lifeform.Update();
					++updates;
				}

				foreach (Lifeform lifeform in LIMBO) {
					GRAVEYARD.Add(lifeform);
					LIFEFORMS.Remove(lifeform);
				}

				LIMBO.Clear();

				Lifeform parentA = null;

				foreach (Lifeform lifeform in BROTHEL) {
					if (parentA == null) {
						parentA = lifeform;
						continue;
					}

					Lifeform child = Lifeform.Breed(parentA, lifeform);
					parentA = null;

					if (child != null) {
						LIFEFORMS.Add(child);
					}
				}

				BROTHEL.Clear();

				if (LIFEFORMS.Count == 0) {
					cycles = i;
					break;
				}

				if (LIFEFORMS.Count > maxLifeforms) {
					maxLifeforms = LIFEFORMS.Count;
				}

				Console.SetCursorPosition(0, 2);
				Console.WriteLine(WORLD + $"|{LIFEFORMS.Count,6}|{maxLifeforms,6}");
			}

			Console.SetCursorPosition(0, 3);
			Console.Write("                    ");
			Console.SetCursorPosition(cursorLeft, cursorTop);
			Console.WriteLine($"{cycles + " cycles, " + updates + " updates",47}");
			Console.WriteLine(WORLD + $"|{LIFEFORMS.Count,6}|{maxLifeforms,6}");
			Console.WriteLine();
		}

		private static void TopAndBottom (int oldest, int youngest, bool data) {
			Lifeform[] lifeforms = GRAVEYARD
					.OrderByDescending(c => c.BreedCount)
					.ThenByDescending(c => c.Age)
					.ToArray();

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

		private static void Statistics (bool extra) {
			int[] urgeStats = new int[Utils.URGE_COUNT];
			int[] emotionStats = new int[Utils.EMOTION_COUNT];
			int[] moodStats = new int[Utils.MOOD_COUNT];
			int[] deathByStats = new int[Utils.DEATHBY_COUNT];

			foreach (Lifeform lifeform in GRAVEYARD) {
				++urgeStats[(int) lifeform.Mood.Urge];
				++emotionStats[(int) lifeform.Mood.Emotion];
				++moodStats[(int) lifeform.Mood.Mood];
				++deathByStats[(int) lifeform.DeathBy];
			}

			const int length = 5;
			MoodManager.TruncateTo = length;

			MoodManager.Extended = false;
			Console.WriteLine("\n" + Utils.Truncate("Urges", 35, 1)
					+ "||" + Utils.Truncate("Emotions", 47, 1));
			Console.WriteLine(MoodManager.ToStringHeader());
			
			foreach (int u in urgeStats) {
				Console.Write(Utils.Truncate(u.ToString(), length, -1) + "|");
			}

			foreach (int e in emotionStats) {
				Console.Write("|" + Utils.Truncate(e.ToString(), length, -1));
			}

			Console.WriteLine("\n\n" + Utils.Truncate("Moods", 29, 1)
					+ "||" + Utils.Truncate("Causes of death", 40, 1));

			foreach (string mood in Enum.GetNames(typeof(Mood))) {
				Console.Write(Utils.Truncate(mood, length, 1) + "|");
			}

			foreach (string cause in Enum.GetNames(typeof(DeathBy))) {
				Console.Write("|" + Utils.Truncate(cause, length, 1));
			}

			Console.WriteLine();

			foreach (int m in moodStats) {
				Console.Write(Utils.Truncate(m.ToString(), length, -1) + "|");
			}

			foreach (int d in deathByStats) {
				Console.Write("|" + Utils.Truncate(d.ToString(), length, -1));
			}

			Console.WriteLine();

			if (!extra) {
				return;
			}

			Console.WriteLine("\ndead: " + GRAVEYARD.Count);

			int[] ages = new int[GRAVEYARD.Count];

			int index = 0;
			foreach (Lifeform lifeform in GRAVEYARD) {
				ages[index++] = lifeform.Age;
			}

			double[] res = Utils.StandardDeviation(ages);
			Console.WriteLine($"mean: {res[1]:0.####}\nsdev: {res[0]:0.####}");
		}

		private static void FindErrors () {
			if (!Lifeform.Logging) {
				return;
			}

			Console.WriteLine();

			foreach (Lifeform lifeform in GRAVEYARD) {
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
				lifeforms.Add(new Lifeform(world));
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
