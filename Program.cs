using System;
using System.Collections.Generic;
using System.Linq;
using ComplexLifeforms.Enums;
using static ComplexLifeforms.Utils;
using static ComplexLifeforms.SpeciesContainer;

namespace ComplexLifeforms {

	internal static class Program {

		public const int COUNT = 500;
		public const int SIZE = 10000000;

		private const int CYCLES = 100000;
		private const int PSTAT_CYCLE = 5000;

		private const bool DO_PRIMARY = false;
		private const bool DO_SECONDARY = true;
		private const bool DO_EXTRA = false;

		private const int IDX_A = DO_PRIMARY ? 2 : 0;
		private const int IDX_B = DO_PRIMARY ? 3 : 1;

		private static readonly HashSet<Lifeform> BROTHEL = new HashSet<Lifeform>();
		private static readonly List<Lifeform> LIMBO = new List<Lifeform>();
		private static readonly List<Ghost> GRAVEYARD = new List<Ghost>();

		private static readonly List<List<int[]>> STATS = new List<List<int[]>>();
		private static readonly List<List<int>> AGES = new List<List<int>>();
		private static readonly int[] DEATHS = new int[SPECIES_COUNT];

		private static int _totalDeaths;

		private static void Main () {
			int start = Environment.TickCount;
			int seed = start;

			Utils.Random = new Random(seed);
			Console.Title = typeof(Program).Assembly.GetName().Version.ToString();
			World.Separator = '|';
			World.Extended = false;
			Lifeform.Separator = '|';
			Lifeform.TruncateTo = 5;
			Lifeform.Extended = true;
			Lifeform.Logging = false;
			MoodManager.Separator = '|';
			MoodManager.Extended = true;

			Console.Write(World.ToStringHeader() + "|alive |max   |alpha |beta  |gamma ");
			Console.WriteLine(Truncate(seed, Console.WindowWidth - Console.CursorLeft - 1, -1));
			Console.Write(WORLD + $"|{LIFEFORMS.Count,6}|{LIFEFORMS.Count,6}"
					+ $"|{Count[0],6}|{Count[1],6}|{Count[2],6}");

			int cTop = Console.CursorTop;
			int cLeft = Console.CursorLeft;
			Console.Write("\n\nProcessing cycles... ");

			InitializeStatistics();
			Run(cLeft, cTop);
			TopAndBottom(4, 0, false);
			Statistics();

			int cBottom = Console.CursorTop;
			string time = Environment.TickCount - start + "ms";

			Console.SetCursorPosition(cLeft, cTop + 1);
			Console.WriteLine(Truncate(time, Console.WindowWidth - Console.CursorLeft - 1, -1));
			Console.SetCursorPosition(0, cBottom);
			Console.ReadKey();
		}

		private static void Run (int cursorLeft, int cursorTop) {
			int cursorCycleTop = Console.CursorTop;
			int cursorCycleLeft = Console.CursorLeft;

			int cycles = CYCLES;
			long updates = 0;
			int maxLifeforms = COUNT;

			for (int i = 0; i < CYCLES; ++i) {
				Console.SetCursorPosition(cursorCycleLeft, cursorCycleTop);
				Console.Write($"[{i}/{CYCLES}]");

				foreach (Lifeform lifeform in LIFEFORMS) {
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

				if (i % PSTAT_CYCLE == 0) {
					PartialStatistics();
				}

				Recount();
				Console.SetCursorPosition(0, 2);
				Console.WriteLine(WORLD + $"|{LIFEFORMS.Count,6}|{maxLifeforms,6}"
						+ $"|{Count[0],6}|{Count[1],6}|{Count[2],6}");
			}

			PartialStatistics();
			PrintStats(cursorLeft, cursorTop, cycles, updates, maxLifeforms);
		}

		private static void PrintStats (int cLeft , int cTop, int cycles, long updates, int max) {
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

		private static void InitializeStatistics () {
			for (int i = 0; i < SPECIES_COUNT; ++i) {
				if (DO_PRIMARY || DO_SECONDARY) {
					List<int[]> stat = new List<int[]>();

					if (DO_PRIMARY) {
						stat.Add(new int[URGE_COUNT]);
						stat.Add(new int[EMOTION_COUNT]);
					}

					if (DO_SECONDARY) {
						stat.Add(new int[MOOD_COUNT]);
						stat.Add(new int[DEATHBY_COUNT]);
					}

					STATS.Add(stat);
				}
				AGES.Add(new List<int>());
			}
		}

		private static void PartialStatistics () {
			foreach (Ghost ghost in GRAVEYARD) {
				if (DO_PRIMARY) {
					STATS[ghost.Species][0][ghost.Urge]++;
					STATS[ghost.Species][1][ghost.Emotion]++;
				}

				if (DO_SECONDARY) {
					STATS[ghost.Species][IDX_A][ghost.Mood]++;
					STATS[ghost.Species][IDX_B][ghost.DeathBy]++;
				}

				if (DO_EXTRA) {
					DEATHS[ghost.Species]++;
					AGES[ghost.Species].Add(ghost.Age);
				}
			}

			_totalDeaths += GRAVEYARD.Count;
			GRAVEYARD.Clear();
		}

		private static void Statistics () {
			int length = Math.Max(4, _totalDeaths.ToString().Length);
			MoodManager.TruncateTo = length;
			MoodManager.Extended = false;
			int index = 0;

			foreach (List<int[]> stat in STATS) {
				Console.WriteLine($"\n#############################################\n>>{(Species) index++}:");

				if (DO_PRIMARY) {
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

				if (DO_SECONDARY) {
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

				foreach (int m in stat[IDX_A]) {
					Console.Write(Truncate(m, length, -1) + "|");
				}

				foreach (int d in stat[IDX_B]) {
					Console.Write("|" + Truncate(d, length, -1));
				}

				Console.WriteLine();
			}

			if (!DO_EXTRA) {
				return;
			}

			for (int i = 0; i < SPECIES_COUNT; ++i) {
				Console.WriteLine($"\n#############################################\n>>{(Species) i}:");
				Console.WriteLine("\ndead: " + DEATHS[i]);

				double[] res = StandardDeviation(AGES[i]);
				Console.WriteLine($"mean: {res[1]:0.####}\nsdev: {res[0]:0.####}");
			}
		}

	}

}