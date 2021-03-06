﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ComplexLifeforms.Enums;
using static ComplexLifeforms.Utils;

namespace ComplexLifeforms {

	[SuppressMessage ("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
	public class MoodManager {

		public const int URGE_CAP = 50;
		public const int EMOTION_CAP = 99;

		/// <summary>Column separator for ToString and ToStringHeader methods. Default=' '</summary>
		public static char Separator = ' ';

		/// <summary>Column width limit for ToString and ToStringHeader methods. Default=4</summary>
		public static int TruncateTo = 4;

		/// <summary>Adds _moodValue to ToString and ToStringHeader methods. Default=false</summary>
		public static bool Extended = false;

		/// <summary>Represents the lifeform which this object belongs to.</summary>
		[SuppressMessage ("ReSharper", "NotAccessedField.Global")]
		public Lifeform Lifeform;

		protected internal bool Asleep;

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

		private readonly Tier[] _emotionBias;
		private readonly Tier[] _urgeBias;
		private readonly int[] _emotionValues;
		private readonly int[] _urgeValues;

		private Mood _mood;
		private Urge _urge;
		private Emotion _emotion;

		private int _moodValue;

		public MoodManager (Lifeform lifeform=null) : this(lifeform, true) {
			Lifeform = lifeform;
			Asleep = false;

			_urgeValues = new int[URGE_COUNT];
			_emotionValues = new int[EMOTION_COUNT];

			Update();
		}

		public MoodManager (Tier[] urgeBias, Tier[] emotionBias, Lifeform lifeform=null) : this(lifeform) {
			_urgeBias = urgeBias;
			_emotionBias = emotionBias;
		}

		private MoodManager (Lifeform lifeform, bool generateBias) {
			if (generateBias) {
				_urgeBias = GenerateUrgeBias(lifeform?.Species);
				_emotionBias = GenerateEmotionBias(lifeform?.Species);
			}
		}

		/// <summary>Represents the current general mood.</summary>
		public Mood Mood => _mood;

		/// <summary>Represents the current strongest urge.</summary>
		public Urge Urge => _urge;

		/// <summary>Represents the current strongest emotion.</summary>
		public Emotion Emotion => _emotion;

		public Tier[] UrgeBias => _urgeBias;
		public Tier[] EmotionBias => _emotionBias;

		public static string EmotionName (MoodManager mood) {
			int[] result = EmotionIntensity(mood._emotionValues);
			return EMOTION_NAMES[result[0], result[1]];
		}

		public static string ToStringHeader () {
			char s = Separator;
			string data = "";

			bool first = true;
			foreach (string urge in Enum.GetNames(typeof(Urge))) {
				if (first) {
					data += Truncate(urge, TruncateTo, 1);
					first = false;
					continue;
				}

				data += s + Truncate(urge, TruncateTo, 1);
			}

			data += s;
			foreach (string emotion in Enum.GetNames(typeof(Emotion))) {
				data += s + Truncate(emotion, TruncateTo, 1);
			}

			if (Extended) {
				data += $"{s}{s}" + Truncate("mood", TruncateTo, 1);
			}

			return data;
		}

		public void Update () {
			ProcessChanges();
			ClampValues();
			ProcessMood();

			_urge = (Urge) Utils.MaxIndex(_urgeValues);
			_emotion = (Emotion) Utils.MaxIndex(_emotionValues);
		}

		public void Action (Urge action) {
			int iaction = (int) action;
			int[] indexes = EdgeIndexes(_urgeValues);
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

			for (int i = 0; i < emotions.Length; ++i) {
				_emotionValues[(int) emotions[i]] += TYPE_VALUES[type, i];
			}

			_urgeValues[iaction] -= 5;
		}

		public void AffectUrge (Urge urge, int delta) {
			// todo expand
			_urgeValues[(int) urge] += delta;
		}

		public override string ToString () {
			char s = Separator;
			string data = $"{_urgeValues[0],2} {(int) _urgeBias[0]}";

			for (int i = 1; i < URGE_COUNT; ++i) {
				data += $"{s}{_urgeValues[i],2} {(int) _urgeBias[i]}";
			}

			data += s;
			for (int i = 0; i < EMOTION_COUNT; ++i) {
				data += $"{s}{_emotionValues[i],2} {(int) _emotionBias[i]}";
			}

			if (Extended) {
				data += $"{s}{s}{_moodValue,4}";
			}

			return data;
		}

		private static int EmotionIntensity (int value) {
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

		private static int[] EmotionIntensity (IReadOnlyList<int> values) {
			const double threshold = 0.25;
			int emotionIndex = Utils.MaxIndex(values);
			int emotionValue = values[emotionIndex];
			int indexLeft;
			int indexRight;

			if (emotionIndex == 0) {
				indexLeft = values.Count - 1;
			} else {
				indexLeft = emotionIndex - 1;
			}

			if (emotionIndex == values.Count - 1) {
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

		private void ProcessChanges () {
			for (int i = 0; i < URGE_COUNT; ++i) {
				if (Utils.Random.Next((int) _urgeBias[i], TIER_COUNT + 1) == TIER_COUNT) {
					if (!Asleep) {
						++_urgeValues[i];
					}
				}
			}

			for (int i = 0; i < EMOTION_COUNT; ++i) {
				if (Asleep && Utils.Random.Next(TIER_COUNT - (int) _emotionBias[i] + 1, TIER_COUNT + 1) == TIER_COUNT) {
					_emotionValues[i] -= TIER_COUNT - (int) _emotionBias[i] + 1;
				}
			}
		}

		private void ProcessMood () {
			const int good = 15;
			const int neutral = 5;
			const int bad = -5;
			const int terrible = -15;

			int mood = _moodValue / 2;

			for (int i = 0; i < EMOTION_COUNT; ++i) {
				int intensity = EmotionIntensity(_emotionValues[i]);
				if (_emotionValues[i] != 0) {
					mood += (intensity + 1) * EMOTION_MOOD_EFFECT[i];
				}
			}

			if (mood > good) {
				_mood = Mood.Great;
			} else if (mood > neutral) {
				_mood = Mood.Good;
			} else if (mood > bad) {
				_mood = Mood.Neutral;
			} else if (mood > terrible) {
				_mood = Mood.Bad;
			} else {
				_mood = Mood.Terrible;
			}

			_moodValue = mood;
		}

		private void ClampValues () {
			for (int i = 0; i < URGE_COUNT; ++i) {
				if (_urgeBias[i] == Tier.None) {
					_urgeValues[i] = 0;
					continue;
				}

				int u = _urgeValues[i];

				if (u < 0) {
					_urgeValues[i] = 0;
				} else if (u > URGE_CAP) {
					_urgeValues[i] = URGE_CAP;
				}
			}

			for (int i = 0; i < EMOTION_COUNT; ++i) {
				if (_emotionBias[i] == Tier.None) {
					_emotionValues[i] = 0;
					continue;
				}

				int e = _emotionValues[i];

				if (e < 0) {
					_emotionValues[i] = 0;
				} else if (e > EMOTION_CAP) {
					_emotionValues[i] = EMOTION_CAP;
				}
			}
		}

	}

}