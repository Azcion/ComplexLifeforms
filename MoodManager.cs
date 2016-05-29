using System;
using System.Collections.Generic;

namespace ComplexLifeforms {

	public class MoodManager {

		public Urge Urge { get; private set; }
		public Emotion Emotion { get; private set; }

		public int[] UrgeValues { get; private set;}
		public int[] EmotionValues { get; private set; }

		public Tier[] UrgeBias { get; private set; }
		public Tier[] EmotionBias { get; private set; }

		private Random _random;
		private static readonly int URGE_COUNT = Enum.GetNames(typeof(Urge)).Length;
		private static readonly int EMOTION_COUNT = Enum.GetNames(typeof(Emotion)).Length;
		private static readonly int TIER_COUNT = Enum.GetNames(typeof(Tier)).Length;

		public MoodManager () {
			_random = new Random();

			UrgeValues = new int[URGE_COUNT];
			EmotionValues = new int[EMOTION_COUNT];

			UrgeBias = new Tier[URGE_COUNT];
			EmotionBias = new Tier[EMOTION_COUNT];

			for (int i = 0; i < URGE_COUNT; ++i) {
				UrgeBias[i] = (Tier) _random.Next(TIER_COUNT);
			}

			for (int i = 0; i < EMOTION_COUNT; ++i) {
				EmotionBias[i] = (Tier) _random.Next(TIER_COUNT);
			}
		}

		public void Update () {
			RandomChange();
			ClampValues();

			Urge = (Urge) MaxIndex(UrgeValues);
			Emotion = (Emotion) MaxIndex(EmotionValues);
		}

		private void ClampValues () {
			for (int i = 0; i < URGE_COUNT; ++i) {
				int u = UrgeValues[i];
				if (u < 0) {
					UrgeValues[i] = 0;
				} else
				if (u > 99) {
					UrgeValues[i] = 99;
				}
			}

			for (int i = 0; i < EMOTION_COUNT; ++i) {
				int e = EmotionValues[i];
				if (e < 0) {
					EmotionValues[i] = 0;
				} else 
				if (e > 99) {
					EmotionValues[i] = 99;
				}
			}
		}

		private void RandomChange () {
			for (int i = 0; i < URGE_COUNT; ++i) {
				if (UrgeBias[i] != Tier.None && _random.Next((int) UrgeBias[i], TIER_COUNT + 1) == TIER_COUNT) {
					++UrgeValues[i];
				}
			}

			for (int i = 0; i < EMOTION_COUNT; ++i) {
				if (EmotionBias[i] != Tier.None && _random.Next((int) EmotionBias[i], TIER_COUNT + 1) == TIER_COUNT) {
					++EmotionValues[i];
				}
			}
		}

		public void ApplyTiers (Tier[] urgeBias, Tier[] emotionBias) {
			if (urgeBias.Length != URGE_COUNT || emotionBias.Length != EMOTION_COUNT) {
				Console.WriteLine($"Invalid length of values.  first:{urgeBias.Length} second:{emotionBias.Length}");
				return;
			}

			for (int i = 0; i < URGE_COUNT; ++i) {
				UrgeBias[i] = urgeBias[i];
			}

			for (int i = 0; i < EMOTION_COUNT; ++i) {
				EmotionBias[i] = emotionBias[i];
			}
		}

		public string ToString (char separator=' ') {
			char s = separator;
			string data = $"{UrgeValues[0],2} {(int) UrgeBias[0]}";

			for (int i = 1; i < URGE_COUNT; ++i) {
				data += $"{s}{UrgeValues[i],2} {(int) UrgeBias[i]}";
			}

			data += s;
			for (int i = 0; i < EMOTION_COUNT; ++i) {
				data += $"{s}{EmotionValues[i],2} {(int) EmotionBias[i]}";
			}

			return data;
		}

		public static int MaxIndex<T> (IEnumerable<T> array) where T : IComparable<T> {
			int maxIndex = -1;
			T maxValue = default(T);

			int index = 0;
			foreach (T value in array) {
				if (value.CompareTo(maxValue) > 0 || maxIndex == -1) {
					maxIndex = index;
					maxValue = value;
				}
				++index;
			}

			return maxIndex;
		}

	}

}