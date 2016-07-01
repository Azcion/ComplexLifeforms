namespace ComplexLifeforms {

	public struct Ghost {

		public readonly ushort Id;
		public readonly ushort Age;

		public readonly byte Species;
		public readonly byte Urge;
		public readonly byte Emotion;
		public readonly byte Mood;
		public readonly byte DeathBy;

		public Ghost (Lifeform lifeform) {
			Id = (ushort) lifeform.Id;
			Age = (ushort) lifeform.Age;

			Species = (byte) lifeform.Species;
			Urge = (byte) lifeform.MM.Urge;
			Emotion = (byte) lifeform.MM.Emotion;
			Mood = (byte) lifeform.MM.Mood;
			DeathBy = (byte) lifeform.DeathBy;
		}

	}

}