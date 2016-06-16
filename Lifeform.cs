using System;
using System.Diagnostics.CodeAnalysis;
using ComplexLifeforms.Enums;

namespace ComplexLifeforms {

	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public class Lifeform {

		/// <summary>Unique ID assigned to this object.</summary>
		[SuppressMessage ("ReSharper", "NotAccessedField.Global")]
		public readonly int Id;

		/// <summary>Constructor parameters.</summary>
		public readonly InitLifeform Init;

		/// <summary>Mood, urge and emotion processor.</summary>
		public readonly MoodManager Mood;

		/// <summary>World in which resources will be exchanged.</summary>
		public readonly World World;

		private static int _id;

		private readonly int _healThreshold;
		private readonly int _sleepThreshold;
		private readonly int _eatThreshold;
		private readonly int _drinkThreshold;

		private readonly int _hpDrain;
		private readonly int _energyDrain;
		private readonly int _foodDrain;
		private readonly int _waterDrain;

		private readonly int _healCost;
		private readonly int _healAmount;

		private int _hp;
		private int _energy;
		private int _food;
		private int _water;

		private DeathBy _deathBy;
		private bool _alive;
		private bool _pendingKill;

		private int _age;
		private int _healCount;
		private int _sleepCount;
		private int _eatCount;
		private int _drinkCount;

		/// <summary>
		/// Unpacks SInitLifeform and uses its values.
		/// </summary>
		[SuppressMessage("ReSharper", "UnusedMember.Global")]
		public Lifeform (World world, InitLifeform init, Random random=null)
				: this(world, random,
						init.HpScale, init.EnergyScale,
						init.FoodScale, init.WaterScale,
						init.HealCostScale, init.HealAmountScale,
						init.HpDrainScale, init.EnergyDrainScale,
						init.FoodDrainScale, init.WaterDrainScale,
						init.HealThreshold, init.SleepThreshold,
						init.EatThreshold, init.DrinkThreshold) {
		}

		public Lifeform (World world, Random random=null,
				double hpScale=1, double energyScale=1,
				double foodScale=1, double waterScale=1,
				double healCostScale=1, double healAmountScale=1,
				double hpDrainScale=1, double energyDrainScale=1,
				double foodDrainScale=1, double waterDrainScale=1,
				double healThreshold=0.5, double sleepThreshold=0.25,
				double eatThreshold=0.5, double drinkThreshold=0.5) {
			Id = _id++;
			World = world;
			Mood = new MoodManager(this, random);
			_alive = true;
			_deathBy = DeathBy.None;

			InitWorld w = world.Init;

			Init = new InitLifeform(w.BaseHp, w.BaseEnergy,
					w.BaseFood, w.BaseWater,
					hpScale, energyScale,
					foodScale, waterScale,
					healCostScale, healAmountScale,
					hpDrainScale, energyDrainScale,
					foodDrainScale, waterDrainScale,
					healThreshold, sleepThreshold,
					eatThreshold, drinkThreshold);

			_healCost = (int) (w.HealCost * healCostScale);
			_healAmount = (int) (w.HealAmount * healAmountScale);

			_hpDrain = (int) (w.HpDrain * hpDrainScale);
			_energyDrain = (int) (w.EnergyDrain * energyDrainScale);
			_foodDrain = (int) (w.FoodDrain * foodDrainScale);
			_waterDrain = (int) (w.WaterDrain * waterDrainScale);

			_hp = (int) (w.BaseHp * hpScale);
			_energy = (int) (w.BaseEnergy * energyScale);
			_food = (int) (w.BaseFood * foodScale);
			_water = (int) (w.BaseWater * waterScale);

			_healThreshold = (int) (_hp * healThreshold);
			_sleepThreshold = (int) (_energy * sleepThreshold);
			_eatThreshold = (int) (_food * eatThreshold);
			_drinkThreshold = (int) (_water * drinkThreshold);
		}

		public int Age => _age;
		public bool Alive => _alive;
		public DeathBy DeathBy => _deathBy;

		public static string ToStringHeader (char separator=' ', bool extended=false) {
			char s = separator;
			string data = $"age  {s}hp   {s}energ{s}food {s}water";

			if (extended) {
				data += $"{s}heals{s}slept{s}eaten{s}drank{s}{"urge",-9}{s}{"emotion",-12}"
						+ $"{s}{"mood",-8}{s}{"death by",-13}{s}asleep";
			}

			return data;
		}

		public void Update () {
			if (!_alive) {
				return;
			}

			++_age;

			ProcessBodilyFunctions();
			Heal();

			if (Mood.Asleep) {
				_hp += _hpDrain * 2;
				_energy += _energyDrain * 8;
				++_sleepCount;

				if (_energy >= Init.Energy || _hp < _hpDrain * 32) {
					Mood.Asleep = false;
				}
			}

			if (!Mood.Asleep && _energy < _sleepThreshold) {
				Sleep();
			}

			ClampValues();
			Mood.Update();

			if (_hp <= 0 || _pendingKill || _deathBy != DeathBy.None) {
				Kill();
			}
		}

		public void Eat (int amount) {
			if (!_alive || Mood.Asleep || _pendingKill) {
				return;
			}

			int deltaEnergy = 0;
			int deltaFood = 0;
			int deltaWater = 0;

			if (World.Food < amount) {
				amount = World.Food;
			}

			if (_food < _eatThreshold) {
				deltaEnergy -= _energyDrain;
				deltaFood += amount;
				deltaWater -= _waterDrain / 2;
			} else {
				deltaEnergy -= _energyDrain / 2;
				deltaFood += amount / 2;
				deltaWater -= _waterDrain * 2;
			}

			if (_water <= -deltaWater) {
				Mood.AffectUrge(Urge.Drink, 1);
				return;
			}

			World.UseFood(deltaFood);
			World.Reclaim(0, -deltaWater);
			Mood.Action(Urge.Eat);

			_energy += deltaEnergy;
			_food += deltaFood;
			_water += deltaWater;
			++_eatCount;

			if (_food > Init.Food) {
				_hp -= _hpDrain * 4;

				Mood.AffectUrge(Urge.Eat, -2);
				Mood.AffectUrge(Urge.Excrete, 1);
			}

			if (_hp <= 0 && _deathBy == DeathBy.None) {
				_deathBy = DeathBy.Overeating;
				_pendingKill = true;
			}
		}

		public void Drink (int amount) {
			if (!_alive || Mood.Asleep || _pendingKill) {
				return;
			}

			int deltaEnergy = 0;
			int deltaWater = 0;

			if (World.Water < amount) {
				amount = World.Water;
			}

			if (_water < _drinkThreshold) {
				deltaEnergy -= _energyDrain;
				deltaWater += amount;
			} else {
				deltaEnergy -= _energyDrain / 2;
				deltaWater += amount / 2;
			}

			World.UseWater(deltaWater);
			Mood.Action(Urge.Drink);

			_energy += deltaEnergy;
			_water += deltaWater;
			++_drinkCount;

			if (_water > Init.Water) {
				_hp -= _hpDrain * 4;

				Mood.AffectUrge(Urge.Drink, -2);
				Mood.AffectUrge(Urge.Excrete, 1);
			}

			if (_hp <= 0 && _deathBy == DeathBy.None) {
				_deathBy = DeathBy.Overhydration;
				_pendingKill = true;
			}
		}

		public string ToString (char separator=' ', bool extended=false) {
			char s = separator;
			string data = $"{_age,5}{s}{_hp,5}{s}{_energy,5}{s}{_food,5}{s}{_water,5}";

			if (extended) {
				string emotion = MoodManager.EmotionName(Mood);

				data += $"{s}{_healCount,5}{s}{_sleepCount,5}{s}{_eatCount,5}{s}{_drinkCount,5}"
						+ $"{s}{Mood.Urge,-9}{s}{emotion,-12}{s}{Mood.Mood,-8}"
						+ $"{s}{_deathBy,-13}{s}{(Mood.Asleep ? "yes" : "no"),-6}";
			}

			return data;
		}

		private void ProcessBodilyFunctions () {
			bool excreteOne = false;
			bool excreteTwo = false;
			int deltaHp = 0;
			int deltaEnergy = 0;
			int deltaFood = 0;
			int deltaWater = 0;
			DeathBy deltaDeathBy = DeathBy.None;

			if (_water > Init.Water) {
				excreteOne = true;
				deltaHp -= _hpDrain * 8;
				deltaEnergy -= _energyDrain * 8;
				deltaWater -= _waterDrain * 8;
				deltaDeathBy = DeathBy.Overhydration;
			} else if (_water > _drinkThreshold) {
				excreteOne = true;
				deltaEnergy -= _energyDrain * 4;
				deltaWater -= _waterDrain * 4;
			} else if (_water > _waterDrain) {
				deltaHp -= _hpDrain / 2;
				deltaEnergy -= _energyDrain;
				deltaWater -= _waterDrain;
			} else if (_water > 0) {
				deltaHp -= _hpDrain * 4;
				deltaEnergy -= _energyDrain * 2;
				deltaWater -= _water;
			} else {
				deltaHp -= _hpDrain * 16;
				deltaEnergy -= _energyDrain * 4;

				if (deltaDeathBy == DeathBy.None) {
					deltaDeathBy = DeathBy.Dehydration;
				}
			}

			if (_food > Init.Food) {
				excreteTwo = true;
				deltaHp -= _hpDrain * 8;
				deltaEnergy -= _energyDrain * 8;
				deltaFood -= _foodDrain * 8;
				deltaDeathBy = DeathBy.Overeating;
			} else if (_food > _eatThreshold) {
				excreteTwo = true;
				deltaEnergy -= _energyDrain * 4;
				deltaFood -= _foodDrain * 4;
			} else if (_food > _foodDrain) {
				deltaHp -= _hpDrain / 2;
				deltaEnergy -= _energyDrain;
				deltaFood -= _foodDrain;
			} else if (_food > 0) {
				deltaHp -= _hpDrain * 4;
				deltaEnergy -= _energyDrain * 2;
				deltaFood -= _food;
			} else {
				deltaHp -= _hpDrain * 16;
				deltaEnergy -= _energyDrain * 4;
				deltaDeathBy = DeathBy.Starvation;
			}

			if (Mood.Asleep) {
				deltaEnergy = 0;
				deltaFood = 0;
				deltaWater = 0;
			} else {
				if (excreteOne) {
					Mood.AffectUrge(Urge.Drink, -1);
					Mood.Action(Urge.Excrete);
				}

				if (excreteTwo) {
					Mood.AffectUrge(Urge.Eat, -1);
					Mood.Action(Urge.Excrete);
				}
			}

			World.Reclaim(-deltaFood, -deltaWater);

			_hp += deltaHp;
			_energy += deltaEnergy;
			_food += deltaFood;
			_water += deltaWater;

			if (_hp < 0 && _deathBy == DeathBy.None && deltaDeathBy != DeathBy.None) {
				_deathBy = deltaDeathBy;
				_pendingKill = true;
			}
		}

		private void ClampValues () {
			if (_energy < 0) {
				_energy = 0;
			}

			if (_food < 0) {
				_food = 0;
			}

			if (_water < 0) {
				_water = 0;
			}
		}

		private void Sleep () {
			Mood.Asleep = true;

			if (_energy < 0) {
				_hp -= _hpDrain * 10;
				
				if (_hp < 0 && _deathBy == DeathBy.None) {
					_deathBy = DeathBy.Exhaustion;
					_pendingKill = true;
				}
			}

			Mood.Action(Urge.Sleep);
		}

		private void Heal () {
			if (_hp > _healThreshold || Mood.Asleep) {
				return;
			}

			int deltaEnergy = 0;
			int deltaFood = 0;
			int deltaWater = 0;
			int cost = _healCost + _age;
			int effectiveness = cost + cost;

			if (_food <= cost) {
				Mood.AffectUrge(Urge.Eat, 1);

				if (_food > 0) {
					deltaEnergy -= cost / 2;
					deltaFood -= _food;
					effectiveness -= _food;
				} else {
					effectiveness -= cost;
				}
			} else {
				deltaEnergy -= cost;
				deltaFood -= cost;
			}

			if (_water <= cost) {
				Mood.AffectUrge(Urge.Drink, 1);

				if (_water > 0) {
					deltaEnergy -= cost / 2;
					deltaWater -= _water;
					effectiveness -= _water;
				} else {
					effectiveness -= cost;
				}
			} else {
				deltaEnergy -= cost;
				deltaWater -= cost;
			}

			World.Reclaim(-deltaFood, -deltaWater);
			Mood.Action(Urge.Heal);
			Mood.AffectUrge(Urge.Sleep, 2);

			_hp += effectiveness / (cost + cost) * _healAmount;
			_energy += deltaEnergy;
			_food += deltaFood;
			_water += deltaWater;
			++_healCount;
		}

		private void Kill () {
			if (_pendingKill) {
				_pendingKill = false;
			}

			_alive = false;

			if (_deathBy == DeathBy.None) {
				if (_energy <= 0) {
					_deathBy = DeathBy.Exhaustion;
				} else if (_food <= 0) {
					_deathBy = DeathBy.Starvation;
				} else if (_water <= 0) {
					_deathBy = DeathBy.Dehydration;
				}
			}

			_hp = -1;
			World.Decompose(_food, _water, Init);
		}

	}

}