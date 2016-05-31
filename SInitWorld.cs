namespace ComplexLifeforms {

	public struct SInitWorld {

		public readonly int Size;
		public readonly double StartingFood;
		public readonly double StartingWater;

		public readonly int BaseHp;
		public readonly int BaseEnergy;
		public readonly int BaseFood;
		public readonly int BaseWater;

		public readonly double HealCost;
		public readonly double HealAmount;

		public readonly double HpDrain;
		public readonly double EnergyDrain;
		public readonly double FoodDrain;
		public readonly double WaterDrain;

		public SInitWorld (int size, double startingFood, double startingWater,
				int baseHp, int baseEnergy, int baseFood, int baseWater,
				double healCost, double healAmount,
				double hpDrain, double energyDrain,
				double foodDrain, double waterDrain) {
			Size = size;
			StartingFood = size * startingFood;
			StartingWater = size * startingWater;

			BaseHp = baseHp;
			BaseEnergy = baseEnergy;
			BaseFood = baseFood;
			BaseWater = baseWater;

			HealCost = healCost;
			HealAmount = healAmount;

			HpDrain = hpDrain;
			EnergyDrain = energyDrain;
			FoodDrain = foodDrain;
			WaterDrain = waterDrain;
		}

	}

}