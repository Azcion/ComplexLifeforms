using System;
using System.Collections.Generic;
using System.Linq;
using ComplexLifeforms.Enums;
using static ComplexLifeforms.Utils;
using static ComplexLifeforms.SpeciesContainer;

namespace ComplexLifeforms {

	internal static class Program {

		public const int COUNT = 1000;
		public const int SIZE = 0;

		private const int CYCLES = 50000;

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

			Console.Write(World.ToStringHeader() + "|alive |max   |alpha |beta  |gamma ");
			Console.WriteLine(Truncate(seed, Console.WindowWidth - Console.CursorLeft - 1, -1));
			Console.Write(WORLD + $"|{LIFEFORMS.Count,6}|{LIFEFORMS.Count,6}|{Count[0],6}|{Count[1],6}|{Count[2],6}");

			int cTop = Console.CursorTop;
			int cLeft = Console.CursorLeft;
			Console.Write("\n\nProcessing cycles... ");

			Run(cLeft, cTop);
			TopAndBottom(4, 0, false);
			Statistics(false, true, false);

			int cBottom = Console.CursorTop;
			string time = Environment.TickCount - start + "ms";

			Console.SetCursorPosition(cLeft, cTop + 1);
			Console.WriteLine(Truncate(time, Console.WindowWidth - Console.CursorLeft - 1, -1));
			Console.SetCursorPosition(0, cBottom);
			Console.ReadLine();
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
					if (BreakIfKey(cursorLeft, cursorTop, i + 1, updates, maxLifeforms)) {
						return;
					}

					if (!lifeform.Alive) {
						LIMBO.Add(lifeform);
						continue;
					}

					if (lifeform.Age > 20
							&& (lifeform.MM.Mood == Mood.Great || lifeform.MM.Mood == Mood.Good)
							&& lifeform.MM.Urge == Urge.Reproduce
							&& 2 * INIT_COUNT[(int) lifeform.Species] > Count[(int) lifeform.Species]) {
						lifeform.Breeding = true;
						BROTHEL.Add(lifeform);
					}

					lifeform.Update();
					++updates;
				}

				foreach (Lifeform lifeform in LIMBO) {
					GRAVEYARD.Add(new Ghost(lifeform));
				}

				LIFEFORMS.ExceptWith(LIMBO);
				LIMBO.Clear();
				DoBreeding();
				BROTHEL.Clear();

				if (LIFEFORMS.Count == 0) {
					cycles = i;
					break;
				}

				if (LIFEFORMS.Count > maxLifeforms) {
					maxLifeforms = LIFEFORMS.Count;
				}

				Recount();
				Console.SetCursorPosition(0, 2);
				Console.WriteLine(WORLD + $"|{LIFEFORMS.Count,6}|{maxLifeforms,6}"
						+ $"|{Count[0],6}|{Count[1],6}|{Count[2],6}");
			}

			PrintStats(cursorLeft, cursorTop, cycles, updates, maxLifeforms);
		}

		private static bool BreakIfKey (int cLeft, int cTop, int cycles, int updates, int max) {
			if (Console.KeyAvailable) {
				switch (Console.ReadKey(true).Key) {
					case ConsoleKey.Spacebar:
					case ConsoleKey.Escape:
						PrintStats(cLeft, cTop, cycles, updates, max);
						return true;
				}
			}

			return false;
		}

		private static void PrintStats (int cLeft , int cTop, int cycles, int updates, int max) {
			Console.SetCursorPosition(0, 3);
			Console.Write("                                                    ");
			Console.SetCursorPosition(cLeft, cTop);

			string progress = cycles + " cycles, " + updates + " updates";
			int width = Console.WindowWidth - cLeft - 1;

			Console.WriteLine(Truncate(progress, width, -1));
			Console.WriteLine(WORLD + $"|{LIFEFORMS.Count,6}|{max,6}");
			Console.WriteLine();
		}

		private static void DoBreeding () {
			Lifeform parentA = null;

			foreach (Lifeform lifeform in BROTHEL) {
				if (parentA == null) {
					parentA = lifeform;
					continue;
				}

				if (lifeform.Species != parentA.Species) {
					continue;
				}

				Lifeform child = Lifeform.Breed(parentA, lifeform);
				parentA = null;

				if (child != null) {
					LIFEFORMS.Add(child);
				}
			}
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
					Console.WriteLine(lifeform.MM.ToString());
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
				Console.WriteLine(lifeforms[i].MM.ToString());
			}

			for (int i = youngest + 1; i > 1; --i) {
				Console.WriteLine(lifeforms[lifeforms.Length - i].MM.ToString());
			}
		}

		private static void Statistics (bool primary, bool secondary, bool extra) {
			List<List<int[]>> species = new List<List<int[]>>();
			List<List<int>> ages = new List<List<int>>();
			int[] deaths = new int[SPECIES_COUNT];

			for (int i = 0; i < SPECIES_COUNT; ++i) {
				List<int[]> stats = new List<int[]> {
					new int[URGE_COUNT],
					new int[EMOTION_COUNT],
					new int[MOOD_COUNT],
					new int[DEATHBY_COUNT]
				};

				species.Add(stats);
				ages.Add(new List<int>());
			}

			foreach (Ghost ghost in GRAVEYARD) {
				species[ghost.Species][0][ghost.Urge]++;
				species[ghost.Species][1][ghost.Emotion]++;
				species[ghost.Species][2][ghost.Mood]++;
				species[ghost.Species][3][ghost.DeathBy]++;
				deaths[ghost.Species]++;
				ages[ghost.Species].Add(ghost.Age);
			}

			int length = Math.Max(4, GRAVEYARD.Count.ToString().Length);
			MoodManager.TruncateTo = length;
			MoodManager.Extended = false;

			int index = 0;
			foreach (List<int[]> stat in species) {
				Console.WriteLine($"\n#############################################\n>>{(Species) index++}:");

				if (primary) {
					Console.WriteLine("\n" + Truncate("Urges", URGE_COUNT * length + URGE_COUNT - 1, 1)
							+ "||" + Truncate("Emotions", EMOTION_COUNT * length + EMOTION_COUNT - 1, 1));
					Console.WriteLine(MoodManager.ToStringHeader());

					foreach (int u in stat[0]) {
						Console.Write(Truncate(u, length, -1) + "|");
					}

					foreach (int e in stat[1]) {
						Console.Write("|" + Truncate(e, length, -1));
					}

					Console.WriteLine();
				}

				if (secondary) {
					Console.WriteLine("\n" + Truncate("Moods", MOOD_COUNT * length + MOOD_COUNT - 1, 1)
							+ "||" + Truncate("Causes of death", DEATHBY_COUNT * length + DEATHBY_COUNT - 1, 1));

					foreach (string mood in Enum.GetNames(typeof(Mood))) {
						Console.Write(Truncate(mood, length, 1) + "|");
					}

					foreach (string cause in Enum.GetNames(typeof(DeathBy))) {
						Console.Write("|" + Truncate(cause, length, 1));
					}

					Console.WriteLine();
				}

				foreach (int m in stat[2]) {
					Console.Write(Truncate(m, length, -1) + "|");
				}

				foreach (int d in stat[3]) {
					Console.Write("|" + Truncate(d, length, -1));
				}

				Console.WriteLine();
			}

			if (!extra) {
				return;
			}

			for (int i = 0; i < SPECIES_COUNT; ++i) {
				Console.WriteLine($"\n#############################################\n>>{(Species) i}:");
				Console.WriteLine("\ndead: " + deaths[i]);

				double[] res = StandardDeviation(ages[i]);
				Console.WriteLine($"mean: {res[1]:0.####}\nsdev: {res[0]:0.####}");
			}
		}

	}

}