using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ComplexLifeforms.Enums;
using static ComplexLifeforms.SpeciesContainer;
using static ComplexLifeforms.Utils;
using DataGridTextColumn = FirstFloor.ModernUI.Windows.Controls.DataGridTextColumn;

namespace ComplexLifeforms {

	public partial class Program {

		public const int COUNT = 500;
		public const int SIZE = 10000000;

		private const int CYCLES = 5000;
		private const int PSTAT_CYCLE = 5000;

		private const bool DO_PRIMARY = false;
		private const bool DO_SECONDARY = false;
		private const bool DO_EXTRA = true;

		private const int IDX_A = DO_PRIMARY ? 2 : 0;
		private const int IDX_B = DO_PRIMARY ? 3 : 1;

		private static readonly HashSet<Lifeform> BROTHEL = new HashSet<Lifeform>();
		private static readonly List<Lifeform> LIMBO = new List<Lifeform>();
		private static readonly List<Ghost> GRAVEYARD = new List<Ghost>();

		private static readonly List<List<int[]>> STATS = new List<List<int[]>>();
		private static readonly List<List<int>> AGES = new List<List<int>>();
		private static readonly int[] DEATHS = new int[SPECIES_COUNT];

		private static readonly Stopwatch SW = new Stopwatch();

		private static readonly object LOCKER = new object();

		private static readonly int[] WIDTHS_WORLD = {
			120, 120, 120, 85, 85, 85, 85, 85
		};

		private static readonly int[] WIDTHS_TOP_AND_BOTTOM = {
			60, 50, 50, 50, 50, 50, 85, 85, 75, 85, 50
		};

		private static readonly double TOTAL_WIDTH_WORLD = WIDTHS_WORLD.Sum();
		private static readonly double TOTAL_WIDTH_TOP_AND_BOTTOM = WIDTHS_TOP_AND_BOTTOM.Sum();


		private static readonly string[] COLUMNS_WORLD = {
			"Size", "Food", "Water", "Alive", "Max", "Alpha", "Beta", "Gamma"
		};

		private static readonly string[] COLUMNS_TOP_AND_BOTTOM = {
			"Species", "Age", "HP", "Energy", "Food", "Water",
			"Urge", "Emotion", "Mood", "Death by", "Asleep"
		};

		private static readonly int[] FIRST_ROW = {
			WORLD.Init.Size,
			WORLD.Init.StartingFood,
			WORLD.Init.StartingWater,
			LIFEFORMS.Count,
			LIFEFORMS.Count,
			Count[0],
			Count[1],
			Count[2]
		};

		private static int _totalDeaths;
		private static int _maxLifeforms;

		private static bool _doEndCycles;

		private readonly BackgroundWorker _worker = new BackgroundWorker();

		public Program () {
			InitializeComponent();

			StopButton.LayoutTransform = StartButton.LayoutTransform;

			_worker.WorkerSupportsCancellation = true;
			_worker.WorkerReportsProgress = true;
			_worker.DoWork += Worker_DoWork;
			_worker.ProgressChanged += Worker_ProgressChanged;
			_worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

			_maxLifeforms = LIFEFORMS.Count;

			Style style = new Style();
			style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));

			for (int i = 0; i < COLUMNS_WORLD.Length; ++i) {
				WorldGrid.Columns.Add(new DataGridTextColumn {
					Foreground = (Brush) new BrushConverter().ConvertFromString("#FFBFBFBF"),
					Width = (int) (WIDTHS_WORLD[i] / TOTAL_WIDTH_WORLD * (Width - 18)),
					Header = COLUMNS_WORLD[i],
					Binding = new Binding($"[{i}]"),
					CellStyle = style,
					IsReadOnly = true
				});
			}

			for (int i = 0; i < COLUMNS_TOP_AND_BOTTOM.Length; ++i) {
				TopAndBottomGrid.Columns.Add(new DataGridTextColumn {
					Foreground = (Brush) new BrushConverter().ConvertFromString("#FFBFBFBF"),
					Width = (int) (WIDTHS_TOP_AND_BOTTOM[i] / TOTAL_WIDTH_TOP_AND_BOTTOM * (Width - 18)),
					Header = COLUMNS_TOP_AND_BOTTOM[i],
					Binding = new Binding($"[{i}]"),
					CellStyle = style,
					IsReadOnly = true
				});
			}

			WorldGrid.AutoGenerateColumns = false;
			WorldGrid.ItemsSource = new List<object> {
				FIRST_ROW,
				new[] { '-', '-', '-', '-', '-', '-', '-', '-' }
			};

			int start = Environment.TickCount;

			Setup(start);
			InitializeStatistics();
		}

		private static void Setup (int seed) {
			Utils.Random = new Random(seed);
			World.Separator = '|';
			World.Extended = false;
			Lifeform.Separator = '|';
			Lifeform.TruncateTo = 5;
			Lifeform.Extended = true;
			Lifeform.Logging = false;
			MoodManager.Separator = '|';
			MoodManager.Extended = true;
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

		private static List<object[]> TopAndBottom (int best, int worst, bool data) {
			List<object[]> itemsSource = new List<object[]>();

			if (LIFEFORMS.Count == 0) {
				return itemsSource;
			}

			Lifeform[] lifeforms = LIFEFORMS
					.OrderByDescending(c => c.BreedCount)
					.ThenByDescending(c => c.Age)
					.ToArray();

			if (lifeforms.Length < best + worst) {
				foreach (Lifeform lifeform in lifeforms) {
					itemsSource.Add(lifeform.ToObjectArray());
				}

				if (!data) {
					return itemsSource;
				}

				Console.WriteLine();
				Console.WriteLine(MoodManager.ToStringHeader());

				foreach (Lifeform lifeform in lifeforms) {
					Console.WriteLine(lifeform.MM.ToString());
				}

				return itemsSource;
			}

			for (int i = 0; i < best; ++i) {
				itemsSource.Add(lifeforms[i].ToObjectArray());
			}

			for (int i = worst + 1; i > 1; --i) {
				itemsSource.Add(lifeforms[lifeforms.Length - i].ToObjectArray());
			}

			if (!data) {
				return itemsSource;
			}

			Console.WriteLine($"\n{"Urges",-29}||{"Emotions",-39}");
			Console.WriteLine(MoodManager.ToStringHeader());

			for (int i = 0; i < best; ++i) {
				Console.WriteLine(lifeforms[i].MM.ToString());
			}

			for (int i = worst + 1; i > 1; --i) {
				Console.WriteLine(lifeforms[lifeforms.Length - i].MM.ToString());
			}

			return itemsSource;
		}

		private void Run () {
			int cycles = CYCLES;
			long updates = 0;
			double oldProgress = 0;
			SW.Start();

			for (int i = 0; i < CYCLES; ++i) {
				foreach (Lifeform lifeform in LIFEFORMS) {
					if (!lifeform.Alive) {
						LIMBO.Add(lifeform);
						continue;
					}

					lock (LOCKER) {
						// breeding conditions
						if (lifeform.Age > 20
								&& (lifeform.MM.Mood == Mood.Great || lifeform.MM.Mood == Mood.Good)
								&& lifeform.MM.Urge == Urge.Reproduce
								&& 2 * INIT_COUNT[(int) lifeform.Species] > Count[(int) lifeform.Species]) {
							lifeform.Breeding = true;
							BROTHEL.Add(lifeform);
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
				DoBreeding();
				BROTHEL.Clear();

				if (LIFEFORMS.Count == 0) {
					cycles = i;
					break;
				}

				if (LIFEFORMS.Count > _maxLifeforms) {
					_maxLifeforms = LIFEFORMS.Count;
				}

				if (i % PSTAT_CYCLE == 0) {
					PartialStatistics();
				}

				Recount();

				double progress = (i + 1.0) / CYCLES * 100;

				if (_doEndCycles) {
					_worker.ReportProgress(100);
					Thread.Sleep(50);
					break;
				}

				if (SW.ElapsedMilliseconds > 100 && progress > oldProgress + 0.5) {
					oldProgress = progress;

					_worker.ReportProgress((int) progress);
					Thread.Sleep(50);

					SW.Restart();
				}
			}
		}

		private void Window_UpdateSize (object sender, RoutedEventArgs e) {
			for (int i = 0; i < COLUMNS_WORLD.Length; ++i) {
				WorldGrid.Columns[i].Width = (int) (WIDTHS_WORLD[i] / TOTAL_WIDTH_WORLD * (Width - 18));
			}

			for (int i = 0; i < COLUMNS_TOP_AND_BOTTOM.Length; ++i) {
				TopAndBottomGrid.Columns[i].Width =
						(int) (WIDTHS_TOP_AND_BOTTOM[i] / TOTAL_WIDTH_TOP_AND_BOTTOM * (Width - 18));
			}
		}

		private void StartButton_Click (object sender, RoutedEventArgs e) {
			ProgressBar.Value = 0;

			StartButton.Visibility = Visibility.Collapsed;
			StopButton.Visibility = Visibility.Visible;

			_worker.RunWorkerAsync();
		}

		private void StopButton_Click (object sender, RoutedEventArgs e) {
			StopButton.Visibility = Visibility.Collapsed;
			_doEndCycles = true;
		}

		private void Worker_DoWork (object sender, DoWorkEventArgs e) {
			Run();
		}

		private void Worker_ProgressChanged (object sender, ProgressChangedEventArgs e) {
			ProgressBar.Value = e.ProgressPercentage;

			lock (LOCKER) {
				WorldGrid.ItemsSource = new[] {
					FIRST_ROW,
					new[] {
						WORLD.Init.Size,
						WORLD.Food,
						WORLD.Water,
						LIFEFORMS.Count,
						_maxLifeforms,
						Count[0],
						Count[1],
						Count[2]
					}
				};
			}
		}

		private void Worker_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e) {
			ProgressBar.Value = 100;

			StopButton.Visibility = Visibility.Collapsed;
			TopAndBottomGrid.Visibility = Visibility.Visible;

			_worker.Dispose();
			TopAndBottomGrid.ItemsSource = TopAndBottom(8, 0, false);
		}

	}

}