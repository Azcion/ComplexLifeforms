using System.Collections.Generic;

namespace ComplexLifeforms {

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

		public readonly double HealThreshold;
		public readonly double SleepThreshold;
		public readonly double EatThreshold;
		public readonly double DrinkThreshold;

		public InitLifeform () {
		}

		public InitLifeform (IReadOnlyList<int> bases, IReadOnlyList<double> scales) {
			Hp = (int) (bases[0] * scales[0]);
			Energy = (int) (bases[1] * scales[1]);
			Food = (int) (bases[2] * scales[2]);
			Water = (int) (bases[3] * scales[3]);

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

			HealThreshold = scales[10];
			SleepThreshold = scales[11];
			EatThreshold = scales[12];
			DrinkThreshold = scales[13];
		}

	}

}