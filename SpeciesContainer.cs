using System;
using ComplexLifeforms.Enums;
using System.Collections.Generic;

namespace ComplexLifeforms {

	public static class SpeciesContainer {

		public static readonly World WORLD;
		public static readonly HashSet<Lifeform> LIFEFORMS;
		public static readonly Dictionary<Species, InitLifeform> INIT;

		public static int[] Count = {
				(int) (0.6 * Program.COUNT),
				(int) (0.25 * Program.COUNT),
				(int) (0.15 * Program.COUNT)
		};

		private static readonly int[] COUNT_PER_SPECIES = Count;

		private static readonly double[][] INIT_VALUES = {
				new[] {1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 0.50, 0.25, 0.50, 0.50},
				new[] {5.00, 2.50, 5.00, 5.00, 2.00, 1.00, 2.50, 5.00, 5.00, 5.00, 0.75, 0.25, 0.75, 0.75},
				new[] {0.25, 1.50, 0.25, 0.25, 0.10, 0.50, 0.25, 0.05, 0.10, 0.05, 0.15, 0.10, 0.50, 0.50}
		};

		static SpeciesContainer () {
			WORLD = new World(Program.SIZE);
			LIFEFORMS = new HashSet<Lifeform>();
			INIT = new Dictionary<Species, InitLifeform>();

			int[] bases = {
					WORLD.Init.BaseHp,
					WORLD.Init.BaseEnergy,
					WORLD.Init.BaseFood,
					WORLD.Init.BaseWater
			};
			
			foreach (Species species in Enum.GetValues(typeof(Species))) {
				INIT.Add(species, Init(bases, species));

				for (int i = 0; i < COUNT_PER_SPECIES[(int) species]; ++i) {
					LIFEFORMS.Add(new Lifeform(WORLD, species));
				}
			}
		}

		public static void Recount () {
			int[] count = new int[Utils.SPECIES_COUNT];

			foreach (Lifeform lifeform in LIFEFORMS) {
				++count[(int) lifeform.Species];
			}

			Count = count;
		}

		private static InitLifeform Init (IReadOnlyList<int> bases, Species species) {
			return new InitLifeform(bases, INIT_VALUES[(int) species]);
		}

	}

}