﻿using System;
using System.Collections.Generic;
using System.Linq;
using ComplexLifeforms.Enums;
using static ComplexLifeforms.Utils;

namespace ComplexLifeforms {

	internal static class Program {

		private const int COUNT = 1000;
		private const int CYCLES = 10000;

		private static readonly World WORLD = new World(0);
		private static readonly HashSet<Lifeform> LIFEFORMS = new HashSet<Lifeform>();
		private static readonly HashSet<Lifeform> BROTHEL = new HashSet<Lifeform>();
		private static readonly HashSet<Lifeform> LIMBO = new HashSet<Lifeform>();
		private static readonly HashSet<Ghost> GRAVEYARD = new HashSet<Ghost>();

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

			Console.WriteLine(World.ToStringHeader() + "|alive |max   " + $"{seed,45}");
			Console.Write(WORLD + $"|{LIFEFORMS.Count,6}|{COUNT,6}");

			int cursorTop = Console.CursorTop;
			int cursorLeft = Console.CursorLeft;
			Console.Write("\n\nProcessing cycles... ");

			Run(cursorLeft, cursorTop);
			TopAndBottom(4, 0, false);
			Statistics(true);

			int cursorBottom = Console.CursorTop;
			Console.SetCursorPosition(cursorLeft, cursorTop + 1);
			Console.WriteLine($"{Environment.TickCount - start + "ms",45}");
			Console.SetCursorPosition(0, cursorBottom);
			
			FindErrors();
		}

		private static void Run (int cursorLeft, int cursorTop) {
			int cursorCycleTop = Console.CursorTop;
			int cursorCycleLeft = Console.CursorLeft;

			int cycles = CYCLES;
			int updates = 0;
			int maxLifeforms = COUNT;

			for (int i = 0; i < CYCLES; ++i) {
				Console.SetCursorPosition(cursorCycleLeft, cursorCycleTop);
				Console.Write($"[{i}/{CYCLES}]");

				foreach (Lifeform lifeform in LIFEFORMS) {
					if (Console.KeyAvailable) {
						ConsoleKeyInfo key = Console.ReadKey(true);

						switch (key.Key) {
							case ConsoleKey.Spacebar:
							case ConsoleKey.Escape:
								Console.SetCursorPosition(0, 3);
								Console.Write("                                                     ");
								Console.SetCursorPosition(cursorLeft, cursorTop);
								Console.WriteLine($"{i + 1 + " cycles, " + updates + " updates",45}");
								Console.WriteLine(WORLD + $"|{LIFEFORMS.Count,6}|{maxLifeforms,6}");
								Console.WriteLine();
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
					GRAVEYARD.Add(new Ghost(lifeform));
				}

				LIFEFORMS.ExceptWith(LIMBO);
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
			Console.Write("                                                     ");
			Console.SetCursorPosition(cursorLeft, cursorTop);
			Console.WriteLine($"{cycles + " cycles, " + updates + " updates",45}");
			Console.WriteLine(WORLD + $"|{LIFEFORMS.Count,6}|{maxLifeforms,6}");
			Console.WriteLine();
		}

		private static void TopAndBottom (int oldest, int youngest, bool data) {
			if (LIFEFORMS.Count == 0) {
				return;
			}

			Lifeform[] lifeforms = LIFEFORMS
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
			int[] urgeStats = new int[URGE_COUNT];
			int[] emotionStats = new int[EMOTION_COUNT];
			int[] moodStats = new int[MOOD_COUNT];
			int[] deathByStats = new int[DEATHBY_COUNT];

			foreach (Ghost ghost in GRAVEYARD) {
				++urgeStats[ghost.Urge];
				++emotionStats[ghost.Emotion];
				++moodStats[ghost.Mood];
				++deathByStats[ghost.DeathBy];
			}

			int length = Math.Max(4, GRAVEYARD.Count.ToString().Length);
			MoodManager.TruncateTo = length;

			MoodManager.Extended = false;
			Console.WriteLine("\n" + Truncate("Urges", URGE_COUNT * length + URGE_COUNT - 1, 1)
					+ "||" + Truncate("Emotions", EMOTION_COUNT * length + EMOTION_COUNT - 1, 1));
			Console.WriteLine(MoodManager.ToStringHeader());
			
			foreach (int u in urgeStats) {
				Console.Write(Truncate(u, length, -1) + "|");
			}

			foreach (int e in emotionStats) {
				Console.Write("|" + Truncate(e, length, -1));
			}

			Console.WriteLine("\n\n" + Truncate("Moods", MOOD_COUNT * length + MOOD_COUNT - 1, 1)
					+ "||" + Truncate("Causes of death", DEATHBY_COUNT * length + DEATHBY_COUNT - 1, 1));

			foreach (string mood in Enum.GetNames(typeof(Mood))) {
				Console.Write(Truncate(mood, length, 1) + "|");
			}

			foreach (string cause in Enum.GetNames(typeof(DeathBy))) {
				Console.Write("|" + Truncate(cause, length, 1));
			}

			Console.WriteLine();

			foreach (int m in moodStats) {
				Console.Write(Truncate(m, length, -1) + "|");
			}

			foreach (int d in deathByStats) {
				Console.Write("|" + Truncate(d, length, -1));
			}

			Console.WriteLine();

			if (!extra) {
				return;
			}

			Console.WriteLine("\ndead: " + GRAVEYARD.Count);

			int[] ages = new int[GRAVEYARD.Count];

			int index = 0;
			foreach (Ghost ghost in GRAVEYARD) {
				ages[index++] = ghost.Age;
			}

			double[] res = StandardDeviation(ages);
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
