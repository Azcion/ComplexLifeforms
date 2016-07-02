using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ComplexLifeforms {

	[SuppressMessage ("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage ("ReSharper", "NotAccessedField.Global")]
	public class InitLifeform {

		public readonly int Hp;
		public readonly int Energy;
		public readonly int Food;
		public readonly int Water;

		public readonly double HpScale;
		public readonly double EnergyScale;
		public readonly double FoodScale;
		public readonly double WaterScale;

		public readonly double HealCostScale;
		public readonly double HealAmountScale;

		public readonly double HpDrainScale;
		public readonly double EnergyDrainScale;
		public readonly double FoodDrainScale;
		public readonly double WaterDrainScale;

		public readonly double HealThresholdScale;
		public readonly double SleepThresholdScale;
		public readonly double EatThresholdScale;
		public readonly double DrinkThresholdScale;

		public readonly int HealThreshold;
		public readonly int SleepThreshold;
		public readonly int EatThreshold;
		public readonly int DrinkThreshold;

		public readonly int HpDrain;
		public readonly int EnergyDrain;
		public readonly int FoodDrain;
		public readonly int WaterDrain;

		public readonly int HealCost;
		public readonly int HealAmount;

		public readonly int EatChance;
		public readonly int EatChanceRangeLower;
		public readonly int EatChanceRangeUpper;
		public readonly int DrinkChance;
		public readonly int DrinkChanceRangeLower;
		public readonly int DrinkChanceRangeUpper;

		public InitLifeform () {
		}

		public InitLifeform (InitWorld bases, IReadOnlyList<double> scales, IReadOnlyList<int> chances) {
			Hp = (int) (bases.BaseHp * scales[0]);
			Energy = (int) (bases.BaseEnergy * scales[1]);
			Food = (int) (bases.BaseFood * scales[2]);
			Water = (int) (bases.BaseWater * scales[3]);

			HpScale = scales[0];
			EnergyScale = scales[1];
			FoodScale = scales[2];
			WaterScale = scales[3];

			HealCostScale = scales[4];
			HealAmountScale = scales[5];

			HpDrainScale = scales[6];
			EnergyDrainScale = scales[7];
			FoodDrainScale = scales[8];
			WaterDrainScale = scales[9];

			HealThresholdScale = scales[10];
			SleepThresholdScale = scales[11];
			EatThresholdScale = scales[12];
			DrinkThresholdScale = scales[13];

			HealThreshold = (int) (Hp * HealThresholdScale);
			SleepThreshold = (int) (Energy * SleepThresholdScale);
			EatThreshold = (int) (Food * EatThresholdScale);
			DrinkThreshold = (int) (Water * DrinkThresholdScale);

			HpDrain = (int) (bases.HpDrain * HpDrainScale);
			EnergyDrain = (int) (bases.EnergyDrain * EnergyDrainScale);
			FoodDrain = (int) (bases.FoodDrain * FoodDrainScale);
			WaterDrain = (int) (bases.WaterDrain * WaterDrainScale);

			HealCost = (int) (bases.HealCost * HealCostScale);
			HealAmount = (int) (bases.HealAmount * HealAmountScale);

			EatChance = chances[0];
			EatChanceRangeLower = chances[1];
			EatChanceRangeUpper = chances[2];
			DrinkChance = chances[3];
			DrinkChanceRangeLower = chances[4];
			DrinkChanceRangeUpper = chances[5];
		}

	}

}