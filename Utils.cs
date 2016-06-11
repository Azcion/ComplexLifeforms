using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ComplexLifeforms {

	[SuppressMessage ("ReSharper", "UnusedMember.Global")]
	public static class Utils {

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

		public static string Truncate (string value, int length) {
			if (string.IsNullOrEmpty(value)) {
				return value;
			}

			if (value.Length <= length) {
				return value;
			}

			return value.Substring(0, length);
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

	}

}