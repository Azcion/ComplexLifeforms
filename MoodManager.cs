using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ComplexLifeforms.Enums;

namespace ComplexLifeforms {

	public class MoodManager {

		public Lifeform Lifeform;

		/// <summary>Represents the current general mood.</summary>
		public Mood Mood { get; private set; }
		/// <summary>Represents the current strongest urge.</summary>
		public Urge Urge { get; private set; }
		/// <summary>Represents the current strongest emotion.</summary>
		public Emotion Emotion { get; private set; }

		public int MoodValue { get; private set; }

		public int[] UrgeValues { get; }
		public int[] EmotionValues { get; }

		public Tier[] UrgeBias { get; }
		public Tier[] EmotionBias { get; }

		protected internal bool Asleep;

		private readonly Random _random;

		private static readonly int[] EMOTION_MOOD_EFFECT = { 10, 3, -1, 0, -4, -3, -5, 0 };
		private static readonly int[,] TYPE_VALUES = { { 1, 1, 1 }, { 2, 1, 1 }, { 3, 2, 1 } };
		private static readonly string[,] EMOTION_NAMES = {
				{ "Serenity", "Acceptance", "Apprehension", "Distraction",
						"Pensiveness", "Boredom", "Annoyance", "Interest" },
				{ "Joy", "Trust", "Fear", "Surprise",
						"Sadness", "Disgust", "Anger", "Anticipation" },
				{ "Ecstasy", "Admiration", "Terror", "Amazement",
						"Grief", "Loathing", "Rage", "Vigilance" },
				{ "Love", "Submission", "Awe", "Disapproval",
						"Remorse", "Contempt", "Aggression", "Optimism" }
		};

		public static readonly int URGE_COUNT = Enum.GetNames(typeof(Urge)).Length;
		public static readonly int EMOTION_COUNT = Enum.GetNames(typeof(Emotion)).Length;
		public static readonly int TIER_COUNT = Enum.GetNames(typeof(Tier)).Length;

		public const int URGE_CAP = 50;
		public const int EMOTION_CAP = 99;

		[SuppressMessage("ReSharper", "UnusedMember.Global")]
		public MoodManager (Lifeform lifeform,
				IReadOnlyList<Tier> urgeBias, IReadOnlyList<Tier> emotionBias, Random random = null)
				: this(lifeform, random) {
			if (urgeBias.Count != URGE_COUNT || emotionBias.Count != EMOTION_COUNT) {
				Console.WriteLine($"Invalid length of values.  first:{urgeBias.Count} second:{emotionBias.Count}");
				return;
			}

			for (int i = 0; i < URGE_COUNT; ++i) {
				UrgeBias[i] = urgeBias[i];
			}

			for (int i = 0; i < EMOTION_COUNT; ++i) {
				EmotionBias[i] = emotionBias[i];
			}
		}

		public MoodManager (Lifeform lifeform, Random random=null) {
			_random = random ?? new Random();
			Lifeform = lifeform;
			Asleep = false;

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

			Update();
		}

		public void Update () {
			ProcessChanges();
			ClampValues();
			ProcessMood();

			Urge = (Urge) MaxIndex(UrgeValues);
			Emotion = (Emotion) MaxIndex(EmotionValues);
		}

		private void ProcessChanges () {
			for (int i = 0; i < URGE_COUNT; ++i) {
				if (_random.Next((int) UrgeBias[i], TIER_COUNT + 1) == TIER_COUNT) {
					if (!Asleep) {
						++UrgeValues[i];
					}
				}
			}

			for (int i = 0; i < EMOTION_COUNT; ++i) {
				if (Asleep && _random.Next(TIER_COUNT - (int) EmotionBias[i] + 1, TIER_COUNT + 1) == TIER_COUNT) {
					EmotionValues[i] -= TIER_COUNT - (int) EmotionBias[i] + 1;
				}
			}
		}

		private void AffectEmotions (IReadOnlyList<Emotion> emotions, int type) {
			for (int i = 0; i < emotions.Count; ++i) {
				EmotionValues[(int) emotions[i]] += TYPE_VALUES[type, i];
			}
		}

		private void ClampValues () {
			for (int i = 0; i < URGE_COUNT; ++i) {
				if (UrgeBias[i] == Tier.None) {
					UrgeValues[i] = 0;
					continue;
				}

				int u = UrgeValues[i];

				if (u < 0) {
					UrgeValues[i] = 0;
				} else if (u > URGE_CAP) {
					UrgeValues[i] = URGE_CAP;
				}
			}

			for (int i = 0; i < EMOTION_COUNT; ++i) {
				if (EmotionBias[i] == Tier.None) {
					EmotionValues[i] = 0;
					continue;
				}

				int e = EmotionValues[i];

				if (e < 0) {
					EmotionValues[i] = 0;
				} else if (e > EMOTION_CAP) {
					EmotionValues[i] = EMOTION_CAP;
				}
			}
		}

		private void ProcessMood () {
			const int optimal = 30;
			const int good = optimal / 4;
			const int neutral = 0;
			const int bad = -good;
			const int terrible = -optimal;

			int mood = MoodValue / 2;

			for (int i = 0; i < EMOTION_COUNT; ++i) {
				int intensity = EmotionIntensity(EmotionValues[i]);
				if (EmotionValues[i] != 0) {
					mood += (intensity + 1) * EMOTION_MOOD_EFFECT[i];
				}
			}

			if (mood > good) {
				Mood = Mood.Great;
			} else if (mood > neutral) {
				Mood = Mood.Good;
			} else if (mood > bad) {
				Mood = Mood.Neutral;
			} else if (mood > terrible) {
				Mood = Mood.Bad;
			} else {
				Mood = Mood.Terrible;
			}

			MoodValue = mood;
		}

		private static int[] EdgeIndexes (IEnumerable<int> array) {
			int maxAIndex = -1;
			int maxBIndex = -1;
			int minAIndex = -1;
			int minBIndex = -1;
			int maxAValue = 0;
			int maxBValue = 0;
			int minAValue = EMOTION_CAP;
			int minBValue = EMOTION_CAP;

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

		public void Action (Urge action) {
			int iaction = (int) action;
			int[] indexes = EdgeIndexes(UrgeValues);
			int maxA = indexes[0];
			int maxB = indexes[1];
			int minA = indexes[2];
			int minB = indexes[3];

			Emotion[] emotions;
			int type = 0;

			switch (action) {
				case Urge.Eat:
				case Urge.Drink:
					if (iaction == maxA) {
						// highest
						emotions = new[] { Emotion.Joy, Emotion.Surprise, Emotion.Trust };
						type = 2;
					} else if (iaction == maxB) {
						// second highest
						emotions = new[] { Emotion.Joy, Emotion.Anticipation };
						type = 1;
					} else if (iaction == minA) {
						// lowest
						emotions = new[] { Emotion.Disgust, Emotion.Sadness, Emotion.Anger };
						type = 2;
					} else if (iaction == minB) {
						// second lowest
						emotions = new[] { Emotion.Disgust, Emotion.Sadness };
						type = 1;
					} else {
						emotions = new[] { Emotion.Anticipation, Emotion.Sadness, Emotion.Trust };
					}
					break;
				case Urge.Excrete:
				case Urge.Reproduce:
					if (iaction == maxA) {
						// highest
						emotions = new[] { Emotion.Joy, Emotion.Trust, Emotion.Surprise };
						type = 2;
					} else if (iaction == maxB) {
						// second highest
						emotions = new[] { Emotion.Joy, Emotion.Anticipation };
						type = 1;
					} else if (iaction == minA) {
						// lowest
						emotions = new[] { Emotion.Disgust, Emotion.Anger, Emotion.Fear };
						type = 2;
					} else if (iaction == minB) {
						// second lowest
						emotions = new[] { Emotion.Disgust, Emotion.Anger };
						type = 1;
					} else {
						emotions = new[] { Emotion.Anticipation, Emotion.Sadness, Emotion.Trust };
					}
					break;
				case Urge.Sleep:
					if (iaction == maxA) {
						// highest
						emotions = new[] { Emotion.Joy, Emotion.Fear };
						type = 2;
					} else if (iaction == maxB) {
						// second highest
						emotions = new[] { Emotion.Joy, Emotion.Fear };
						type = 1;
					} else if (iaction == minA) {
						// lowest
						emotions = new[] { Emotion.Anger, Emotion.Joy };
						type = 2;
					} else if (iaction == minB) {
						// second lowest
						emotions = new[] { Emotion.Anger, Emotion.Joy };
						type = 1;
					} else {
						emotions = new[] { Emotion.Joy, Emotion.Fear, Emotion.Trust };
					}
					break;
				case Urge.Heal:
					if (iaction == maxA) {
						// highest
						emotions = new[] { Emotion.Joy, Emotion.Fear, Emotion.Anticipation };
						type = 2;
					} else if (iaction == maxB) {
						// second highest
						emotions = new[] { Emotion.Fear, Emotion.Joy };
						type = 1;
					} else if (iaction == minA) {
						// lowest
						emotions = new[] { Emotion.Fear, Emotion.Anticipation, Emotion.Anger };
						type = 2;
					} else if (iaction == minB) {
						// second lowest
						emotions = new[] { Emotion.Fear, Emotion.Anticipation };
						type = 1;
					} else {
						emotions = new[] { Emotion.Fear, Emotion.Anticipation, Emotion.Trust };
					}
					break;
				default:
					Console.WriteLine($"Unimplemented action. a:{action}");
					return;
			}

			AffectEmotions(emotions, type);
			UrgeValues[iaction] -= 5;
		}

		public void AffectUrge (Urge urge, int delta) {
			// todo expand
			UrgeValues[(int) urge] += delta;
		}

		public string ToString (char separator=' ', bool mood=false) {
			char s = separator;
			string data = $"{UrgeValues[0],2} {(int) UrgeBias[0]}";

			for (int i = 1; i < URGE_COUNT; ++i) {
				data += $"{s}{UrgeValues[i],2} {(int) UrgeBias[i]}";
			}

			data += s;
			for (int i = 0; i < EMOTION_COUNT; ++i) {
				data += $"{s}{EmotionValues[i],2} {(int) EmotionBias[i]}";
			}

			if (mood) {
				data += $"{s}{s}{MoodValue,4}";
			}

			return data;
		}

		public static string ToStringHeader (char separator=' ', bool mood=false) {
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

			if (mood) {
				data += $"{s}{s}mood";
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

		public static int EmotionIntensity (int value) {
			const int high = (int) (EMOTION_CAP * 0.75);
			const int low = (int) (EMOTION_CAP * 0.25);
			int intensity = 1;

			if (value >= high) {
				intensity = 2;
			} else if (value <= low) {
				intensity = 0;
			}

			return intensity;
		}

		public static int[] EmotionIntensity (int[] values) {
			const double threshold = 0.25;
			int emotionIndex = MaxIndex(values);
			int emotionValue = values[emotionIndex];
			int indexLeft;
			int indexRight;

			if (emotionIndex == 0) {
				indexLeft = values.Length - 1;
			} else {
				indexLeft = emotionIndex - 1;
			}

			if (emotionIndex == values.Length - 1) {
				indexRight = 0;
			} else {
				indexRight = emotionIndex + 1;
			}

			double valueLeft = values[indexLeft];
			double valueRight = values[indexRight];

			int[] result = { EmotionIntensity(emotionValue), emotionIndex };

			if (valueLeft > valueRight && valueLeft / emotionValue >= threshold) {
				result = new[] { 3, indexLeft };
			} else if (valueRight > valueLeft && valueRight / emotionValue >= threshold) {
				result = new[] { 3, indexRight };
			}

			return result;
		}

		public static string EmotionName (int[] values) {
			int[] result = EmotionIntensity(values);
			return EMOTION_NAMES[result[0], result[1]];
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

	}

}