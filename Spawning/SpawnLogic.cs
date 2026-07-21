using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace MobLab.Spawning
{
	// Single-player only for this first version (see plan): applies the configured
	// overrides right after NPC.NewNPC creates the mob at the clicked world position.
	internal static class SpawnLogic
	{
		public static void ConfiguredSpawn(Vector2 worldPosition, StatOverrides overrides) {
			// NPC.NewNPC anchors (X,Y) at the NPC's feet (position.Y = Y - height), not its
			// center - texture frames often have padding beyond the hitbox (idle animation
			// overhang etc.), so a feet-anchored ghost cursor didn't reliably line up with
			// where the sprite visually ends up. Anchoring both the ghost and the spawn
			// target on the NPC's geometric CENTER instead is far less sensitive to that
			// per-sprite padding, since it doesn't matter whether the padding is above or
			// below the hitbox.
			int spawnY = (int)(worldPosition.Y + overrides.HitboxHeight / 2f);
			int index = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)worldPosition.X, spawnY, overrides.NpcType);
			if (index < 0 || index >= Main.maxNPCs)
				return;

			NPC npc = Main.npc[index];
			if (!npc.active)
				return;

			npc.lifeMax = Math.Max(1, overrides.LifeMax);
			npc.life = npc.lifeMax;
			npc.defDefense = Math.Max(0, overrides.Defense);
			npc.defense = npc.defDefense;
			npc.defDamage = Math.Max(0, overrides.Damage);
			npc.damage = npc.defDamage;
			npc.knockBackResist = MathHelper.Clamp(overrides.KnockbackResist, 0f, 1f);

			if (overrides.AiStyleOverride.HasValue) {
				npc.aiStyle = overrides.AiStyleOverride.Value;
				for (int i = 0; i < npc.ai.Length; i++)
					npc.ai[i] = 0f;
			}

			npc.GetGlobalNPC<MobLabGlobalNPC>().HasAI = overrides.HasAI;
		}
	}
}
