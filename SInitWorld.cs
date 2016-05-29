namespace ComplexLifeforms {

	public struct SInitWorld {

		public readonly int Size;
		public readonly double StartingFood;
		public readonly double StartingWater;

		public readonly int BaseHp;
		public readonly int BaseFood;
		public readonly int BaseWater;

		public readonly double HealCost;
		public readonly double HealAmount;
		public readonly double HpDrain;
		public readonly double FoodDrain;
		public readonly double WaterDrain;

		public SInitWorld (int s, double sf, double sw, int bh, int bf, int bw,
				double hc, double ha, double hd, double fd, double wd) {
			Size = s;
			StartingFood = s * sf;
			StartingWater = s * sw;

			BaseHp = bh;
			BaseFood = bf;
			BaseWater = bw;

			HealCost = hc;
			HealAmount = ha;
			HpDrain = hd;
			FoodDrain = fd;
			WaterDrain = wd;
		}

	}

}