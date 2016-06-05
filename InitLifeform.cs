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

		public InitLifeform (int baseHp, int baseEnergy,
				int baseFood, int baseWater,
				double hpScale, double energyScale,
				double foodScale, double waterScale,
				double healCostScale, double healAmountScale,
				double hpDrainScale, double energyDrainScale,
				double foodDrainScale, double waterDrainScale,
				double healThreshold, double sleepThreshold,
				double eatThreshold, double drinkThreshold) {
			Hp = (int) (baseHp * hpScale);
			Energy = (int) (baseEnergy * energyScale);
			Food = (int) (baseFood * foodScale);
			Water = (int) (baseWater * waterScale);

			HpScale = hpScale;
			EnergyScale = energyScale;
			FoodScale = foodScale;
			WaterScale = waterScale;

			HealCostScale = healCostScale;
			HealAmountScale = healAmountScale;

			HpDrainScale = hpDrainScale;
			EnergyDrainScale = energyDrainScale;
			FoodDrainScale = foodDrainScale;
			WaterDrainScale = waterDrainScale;

			HealThreshold = healThreshold;
			SleepThreshold = sleepThreshold;
			EatThreshold = eatThreshold;
			DrinkThreshold = drinkThreshold;
		}

	}

}