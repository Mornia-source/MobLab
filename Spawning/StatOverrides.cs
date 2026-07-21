namespace MobLab.Spawning
{
	// The values a player has configured in NPCConfigPanel for the currently selected
	// NPC template, ready to hand off to SpawnLogic once the player clicks a world tile.
	internal class StatOverrides
	{
		public int NpcType;
		public int LifeMax;
		public int Defense;
		public int Damage;
		public float KnockbackResist;
		public float NativeScale = 1f;
		public float HitboxWidth = 16f;
		public float HitboxHeight = 16f;
		public bool HasAI = true;

		// Null keeps the NPC's own AI style (its normal behavior).
		public int? AiStyleOverride;
	}
}
