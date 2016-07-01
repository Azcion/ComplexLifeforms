using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using ComplexLifeforms.Enums;

namespace ComplexLifeforms {

	[SuppressMessage ("ReSharper", "UnusedMember.Global")]
	public static class Utils {

		public static readonly int URGE_COUNT = Enum.GetNames(typeof(Urge)).Length;
		public static readonly int EMOTION_COUNT = Enum.GetNames(typeof(Emotion)).Length;
		public static readonly int TIER_COUNT = Enum.GetNames(typeof(Tier)).Length;
		public static readonly int MOOD_COUNT = Enum.GetNames(typeof(Mood)).Length;
		public static readonly int DEATHBY_COUNT = Enum.GetNames(typeof(DeathBy)).Length;
		public static readonly int SPECIES_COUNT = Enum.GetNames(typeof(Species)).Length;

		public static Random Random = new Random();

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

		public static string Truncate (string value, int length, int whitespace=0) {
			int outLength = Math.Min(value.Length, length);
			string result = value.Substring(0, outLength);

			switch (whitespace) {
				case 1:
					return result.PadRight(length);
				case -1:
					return result.PadLeft(length);
				default:
					return result;
			}
		}

		public static string Truncate (object value, int length, int whitespace = 0) {
			return Truncate(value.ToString(), length, whitespace);
		}

		public static int[] EdgeIndexes (IEnumerable<int> array) {
			int maxAIndex = -1;
			int maxBIndex = -1;
			int minAIndex = -1;
			int minBIndex = -1;
			int maxAValue = 0;
			int maxBValue = 0;
			int minAValue = MoodManager.EMOTION_CAP;
			int minBValue = MoodManager.EMOTION_CAP;

			int zeros = 0;
			int index = 0;

			foreach (int value in array) {
				if (value == 0) {
					++zeros;
				}

				if (value.CompareTo(maxAValue) > 0) {
					maxBIndex = maxAIndex;
					maxBValue = maxAValue;
					maxAIndex = index;
					maxAValue = value;
				} else if (value.CompareTo(maxBValue) > 0) {
					maxBIndex = index;
					maxBValue = value;
				}

				if (value.CompareTo(minAValue) < 0) {
					minBIndex = minAIndex;
					minBValue = minAValue;
					minAIndex = index;
					minAValue = value;
				} else if (value.CompareTo(minBValue) < 0) {
					minBIndex = index;
					minBValue = value;
				}

				++index;
			}

			if (zeros > 2) {
				minAIndex = -1;
				minBIndex = -1;
			}

			int[] indexes = {maxAIndex, maxBIndex, minAIndex, minBIndex};
			return indexes;
		}

		public static int MaxIndex (IEnumerable<int> array) {
			int maxIndex = -1;
			int maxValue = 0;

			int index = 0;
			foreach (int value in array) {
				if (value.CompareTo(maxValue) > 0 || maxIndex == -1) {
					maxIndex = index;
					maxValue = value;
				}
				++index;
			}

			return maxIndex;
		}

		public static InitWorld CSVToInitWorld (string csv) {
			object init = new InitWorld();
			FieldInfo[] fields = typeof(InitWorld).GetFields();
			double[] values = Array.ConvertAll(csv.Split(','), double.Parse);

			if (fields.Length != values.Length) {
				Console.WriteLine($"Number of values must match the number of InitWorld properties. v:{values.Length}");
				return null;
			}

			for (int i = 0; i < fields.Length; ++i) {
				fields[i].SetValue(init, values[i]);
			}

			return (InitWorld) init;
		}

		public static InitLifeform CSVToInitLifeform (string csv) {
			object init = new InitLifeform();
			FieldInfo[] fields = typeof(InitLifeform).GetFields();
			double[] values = Array.ConvertAll(csv.Split(','), double.Parse);

			if (fields.Length != values.Length) {
				Console.WriteLine("Number of values must match the number of InitLifeform properties."
						+ $" v:{values.Length}");
				return null;
			}

			for (int i = 0; i < fields.Length; ++i) {
				fields[i].SetValue(init, values[i]);
			}

			return (InitLifeform) init;
		}

		public static string InitToCSV (InitWorld init) {
			if (init == null) {
				Console.WriteLine("InitWorld was null.");
				return "";
			}

			FieldInfo[] fields = typeof(InitWorld).GetFields();
			string csv = fields[0].GetValue(init).ToString();

			for (int i = 1; i < fields.Length; ++i) {
				csv += $",{fields[i].GetValue(init)}";
			}

			return csv;
		}

		public static string InitToCSV (InitLifeform init) {
			if (init == null) {
				Console.WriteLine("InitLifeform was null.");
				return "";
			}

			FieldInfo[] fields = typeof(InitLifeform).GetFields();
			string csv = fields[0].GetValue(init).ToString();

			for (int i = 1; i < fields.Length; ++i) {
				csv += $",{fields[i].GetValue(init)}";
			}

			return csv;
		}

		public static int[] MixDNA () {
			int count = URGE_COUNT + EMOTION_COUNT;
			int threshold = 1 + count / 2 + Random.Next(-3, 4);

			int[] dnaA = Enumerable.Repeat(0, count).ToArray();
			int[] dnaB = Enumerable.Repeat(1, count).ToArray();
			IList<int> dnaC = new List<int>();

			for (int i = 0; i < threshold; ++i) {
				dnaC.Add(dnaA[i]);
			}

			for (int i = threshold; i < count; ++i) {
				dnaC.Add(dnaB[i]);
			}

			return dnaC.ToArray();
		}

		public static Tier[] GenerateUrgeBias (Species? species) {
			Tier[] urgeBias = new Tier[URGE_COUNT];

			for (int i = 0; i < URGE_COUNT; ++i) {
				urgeBias[i] = (Tier) Random.Next(TIER_COUNT);
			}

			if (species == Species.Gamma) {
				urgeBias[(int) Urge.Reproduce] = Tier.Ultra;
				urgeBias[(int) Urge.Heal] = Tier.None;
			}

			return urgeBias;
		}

		public static Tier[] GenerateEmotionBias (Species? species) {
			Tier[] emotionBias = new Tier[EMOTION_COUNT];

			for (int i = 0; i < URGE_COUNT; ++i) {
				emotionBias[i] = (Tier) Random.Next(TIER_COUNT);
			}

			if (species == Species.Gamma) {
				emotionBias[(int) Emotion.Joy] = Tier.Ultra;
				emotionBias[(int) Emotion.Trust] = Tier.Ultra;
			}

			return emotionBias;
		}
	}

}