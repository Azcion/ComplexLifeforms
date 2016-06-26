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
			Urge = (byte) lifeform.Mood.Urge;
			Emotion = (byte) lifeform.Mood.Emotion;
			Mood = (byte) lifeform.Mood.Mood;
			DeathBy = (byte) lifeform.DeathBy;
		}

	}

}