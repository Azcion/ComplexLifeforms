using System;
using ComplexLifeforms.Enums;
using System.Collections.Generic;

namespace ComplexLifeforms {

	public static class SpeciesContainer {

		public static readonly World WORLD;
		public static readonly HashSet<Lifeform> LIFEFORMS;
		public static readonly Dictionary<Species, InitLifeform> INIT;

		public static readonly int[] INIT_COUNT = {
				(int) (0.50 * Program.COUNT),
				(int) (0.30 * Program.COUNT),
				(int) (0.20 * Program.COUNT)
		};

		public static int[] Count = INIT_COUNT;

		private static readonly int[] COUNT_PER_SPECIES = Count;

		private static readonly double[][] SCALES = {
				//     hp    energ food  water hCost hAmnt hpD   enrgD foodD watrD heal  sleep eat   drink  
				new[] {1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 0.50, 0.25, 0.50, 0.50},
				new[] {5.00, 5.00, 5.00, 7.50, 2.00, 5.00, 2.00, 2.00, 5.00, 5.00, 0.85, 0.30, 0.75, 0.90},
				new[] {1.50, 2.50, 0.25, 0.25, 0.10, 0.01, 0.50, 0.05, 0.10, 0.05, 0.25, 0.05, 0.25, 0.25}
		};

		private static readonly int[][] CHANCES = {
				//     eat       drink
				new[] {2, 5, 10, 2, 1, 10},
				new[] {1, 5, 10, 1, 5, 10}
		};

		static SpeciesContainer () {
			WORLD = new World(Program.SIZE);
			LIFEFORMS = new HashSet<Lifeform>();
			INIT = new Dictionary<Species, InitLifeform>();
			
			foreach (Species species in Enum.GetValues(typeof(Species))) {
				INIT.Add(species, Init(WORLD.Init, (int) species));

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

		private static InitLifeform Init (InitWorld bases, int species) {
			return new InitLifeform(bases, SCALES[species], CHANCES[species]);
		}

	}

}