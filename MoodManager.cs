using System;
using System.Collections.Generic;

namespace ComplexLifeforms {

	public class MoodManager {

		/// <summary>Represents the current strongest urge.</summary>
		public Urge Urge { get; private set; }
		/// <summary>Represents the current strongest emotion.</summary>
		public Emotion Emotion { get; private set; }

		public int[] UrgeValues { get; private set;}
		public int[] EmotionValues { get; private set; }

		public Tier[] UrgeBias { get; private set; }
		public Tier[] EmotionBias { get; private set; }

		private Random _random;
		private static readonly int URGE_COUNT = Enum.GetNames(typeof(Urge)).Length;
		private static readonly int EMOTION_COUNT = Enum.GetNames(typeof(Emotion)).Length;
		private static readonly int TIER_COUNT = Enum.GetNames(typeof(Tier)).Length;

		public MoodManager (Random random=null) {
			_random = random ?? new Random();

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

		internal void Action (Urge action) { // todo change to variables
			int iaction = (int) action;
			int[] indexes = EdgeIndexes(UrgeValues);
			int maxA = indexes[0];
			int maxB = indexes[1];
			int minA = indexes[2];
			int minB = indexes[3];

			switch (action) {
				case Urge.Eat:
				case Urge.Drink:
					if (iaction == maxA) {
						// highest
						EmotionValues[(int) Emotion.Joy] += 5;
						EmotionValues[(int) Emotion.Surprise] += 2;
						EmotionValues[(int) Emotion.Trust] += 1;
					} else if (iaction == maxB) {
						// second highest
						EmotionValues[(int) Emotion.Joy] += 2;
						EmotionValues[(int) Emotion.Anticipation] += 1;
					} else if (iaction == minA) {
						// lowest
						EmotionValues[(int) Emotion.Disgust] += 5;
						EmotionValues[(int) Emotion.Sadness] += 2;
						EmotionValues[(int) Emotion.Anger] += 1;
					} else if (iaction == minB) {
						// second lowest
						EmotionValues[(int) Emotion.Disgust] += 2;
						EmotionValues[(int) Emotion.Sadness] += 1;
					} else {
						EmotionValues[(int) Emotion.Anticipation] += 1;
						EmotionValues[(int) Emotion.Sadness] += 1;
					}
					break;
				case Urge.Excrete:
				case Urge.Reproduce:
					if (iaction == maxA) {  // highest
						EmotionValues[(int) Emotion.Joy] += 5;
						EmotionValues[(int) Emotion.Trust] += 2;
						EmotionValues[(int) Emotion.Surprise] += 1;
					} else
					if (iaction == maxB) {  // second highest
						EmotionValues[(int) Emotion.Joy] += 2;
						EmotionValues[(int) Emotion.Anticipation] += 1;
					} else
					if (iaction == minA) {  // lowest
						EmotionValues[(int) Emotion.Disgust] += 5;
						EmotionValues[(int) Emotion.Anger] += 2;
						EmotionValues[(int) Emotion.Fear] += 1;
					} else
					if (iaction == minB) {  // second lowest
						EmotionValues[(int) Emotion.Disgust] += 2;
						EmotionValues[(int) Emotion.Anger] += 1;
					} else {
						EmotionValues[(int) Emotion.Anticipation] += 1;
						EmotionValues[(int) Emotion.Sadness] += 1;
					}
					break;
				case Urge.Sleep:
				case Urge.Heal:
					if (iaction == maxA) {  // highest
						EmotionValues[(int) Emotion.Joy] += 5;
						EmotionValues[(int) Emotion.Fear] += 2;
						EmotionValues[(int) Emotion.Anticipation] += 1;
					} else
					if (iaction == maxB) {  // second highest
						EmotionValues[(int) Emotion.Fear] += 2;
						EmotionValues[(int) Emotion.Joy] += 1;
					} else
					if (iaction == minA) {  // lowest
						EmotionValues[(int) Emotion.Fear] += 5;
						EmotionValues[(int) Emotion.Anticipation] += 2;
						EmotionValues[(int) Emotion.Anger] += 1;
					} else
					if (iaction == minB) {  // second lowest
						EmotionValues[(int) Emotion.Fear] += 2;
						EmotionValues[(int) Emotion.Anticipation] += 1;
					} else {
						EmotionValues[(int) Emotion.Fear] += 1;
						EmotionValues[(int) Emotion.Anticipation] += 1;
					}
					break;
			}

			UrgeValues[iaction] -= 5;
		}

		public void AffectUrge (Urge urge, int delta) {
			UrgeValues[(int) urge] += delta;
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

		public static string ToStringHeader (char separator=' ') {
			char s = separator;
			string data = "";

			bool first = true;
			foreach (string urge in Enum.GetNames(typeof(Urge))) {
				if (first) {
					data += $"{Truncate(urge, 4),-4}";
					first = false;
					continue;
				}

				data += $"{s}{Truncate(urge, 4),-4}";
			}

			data += s;
			foreach (string emotion in Enum.GetNames(typeof(Emotion))) {
				data += $"{s}{Truncate(emotion, 4),-4}";
			}

			return data;
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

		public static int[] EdgeIndexes<T> (IEnumerable<T> array) where T : IComparable<T> {
			int maxAIndex = -1;
			int maxBIndex = -1;
			int minAIndex = -1;
			int minBIndex = -1;
			T maxAValue = default(T);
			T maxBValue = default(T);
			T minAValue = default(T);
			T minBValue = default(T);

			int index = 0;
			foreach (T value in array) {
				if (value.CompareTo(maxAValue) > 0 || maxAIndex == -1) {
					maxAIndex = index;
					maxAValue = value;
				}
				if (value.CompareTo(maxBValue) > 0 || maxBIndex == -1) {
					maxBIndex = index;
					maxBValue = value;
				}
				if (value.CompareTo(minAValue) < 0 || minBIndex == -1) {
					minAIndex = index;
					minAValue = value;
				}
				if (value.CompareTo(minBValue) < 0 || minBIndex == -1) {
					minBIndex = index;
					minBValue = value;
				}
				++index;
			}

			int[] indexes = {maxAIndex, maxBIndex, minAIndex, minBIndex};
			return indexes;
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