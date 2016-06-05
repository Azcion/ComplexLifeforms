namespace ComplexLifeforms {

	public class InitWorld {

		public readonly int Size;
		public readonly int StartingFood;
		public readonly int StartingWater;

		public readonly int BaseHp;
		public readonly int BaseEnergy;
		public readonly int BaseFood;
		public readonly int BaseWater;

		public readonly int HealCost;
		public readonly int HealAmount;

		public readonly int HpDrain;
		public readonly int EnergyDrain;
		public readonly int FoodDrain;
		public readonly int WaterDrain;

		public InitWorld () {
		}

		public InitWorld (int size, double foodScale, double waterScale,
				int baseHp, int baseEnergy, int baseFood, int baseWater,
				int healCost, int healAmount,
				int hpDrain, int energyDrain,
				int foodDrain, int waterDrain) {
			Size = size;
			StartingFood = (int) (size * foodScale);
			StartingWater = (int) (size * waterScale);

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