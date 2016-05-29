namespace ComplexLifeforms {

	public struct SInitLifeform {

		public readonly double HpScale;
		public readonly double FoodScale;
		public readonly double WaterScale;

		public readonly double HealCostScale;
		public readonly double HealAmountScale;
		public readonly double HpDrainScale;
		public readonly double FoodDrainScale;
		public readonly double WaterDrainScale;

		public readonly double HealThreshold;
		public readonly double EatThreshold;
		public readonly double DrinkThreshold;

		public SInitLifeform (double hs, double fs, double ws, double hcs, double has, double hds,
				double fds, double wds, double ht, double et, double dt) {
			HpScale = hs;
			FoodScale = fs;
			WaterScale = ws;

			HealCostScale = hcs;
			HealAmountScale = has;
			HpDrainScale = hds;
			FoodDrainScale = fds;
			WaterDrainScale = wds;

			HealThreshold = ht;
			EatThreshold = et;
			DrinkThreshold = dt;
		}

	}

}