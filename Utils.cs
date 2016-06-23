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
			double[] values = Array.ConvertAll(csv.Split(','), Double.Parse);

			if (fields.Length != values.Length) {
				Console.WriteLine($"Number of values must match the number of SInitWorld properties. v:{values.Length}");
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
			double[] values = Array.ConvertAll(csv.Split(','), Double.Parse);

			if (fields.Length != values.Length) {
				Console.WriteLine("Number of values must match the number of SInitLifeform properties."
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
				Console.WriteLine("SInitWorld was null.");
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
				Console.WriteLine("SInitLifeform was null.");
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

			int[] dnaA = CreateDNA(0, count);
			int[] dnaB = CreateDNA(1, count);
			IList<int> dnaC = new List<int>();

			for (int i = 0; i < threshold; ++i) {
				dnaC.Add(dnaA[i]);
			}

			for (int i = threshold; i < count; ++i) {
				dnaC.Add(dnaB[i]);
			}

			return dnaC.ToArray();
		}

		public static int[] CreateDNA (int identifier, int count) {
			return Enumerable.Repeat(identifier, count).ToArray();
		}

	}

}