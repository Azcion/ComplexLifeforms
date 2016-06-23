namespace ComplexLifeforms {

	public struct Ghost {

		public readonly ushort Age;

		public readonly byte Urge;
		public readonly byte Emotion;
		public readonly byte Mood;
		public readonly byte DeathBy;

		public Ghost (Lifeform lifeform) {
			Age = (ushort) lifeform.Age;

			Urge = (byte) lifeform.Mood.Urge;
			Emotion = (byte) lifeform.Mood.Emotion;
			Mood = (byte) lifeform.Mood.Mood;
			DeathBy = (byte) lifeform.DeathBy;
		}

	}

}