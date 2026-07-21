using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MobLab.Spawning
{
	// Per-NPC override data for mobs spawned through MobLab.
	//
	// HasAI=false blocks AI() from running: no target acquisition, no attacks, no turning
	// to face the player. A regular NPC is blocked from tick 0 - it never needs to run
	// AI at all. A boss gets exactly one AI tick first (see hadFirstTick below), because
	// multi-segment bosses (Golem's fists, Skeletron's hands, Wall of Flesh's eyes...)
	// spawn their extra pieces from inside that first tick; skipping AI outright meant
	// those pieces were simply never created. From the second tick on (or immediately for
	// non-bosses), position/velocity are also pinned every tick - just skipping AI wasn't
	// enough by itself, since whatever velocity a mob already had (from that one allowed
	// tick, or some other source) kept being integrated into its position afterward.
	internal class MobLabGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public bool HasAI = true;

		private bool hadFirstTick;
		private Vector2 anchorPosition;
		private bool anchorCaptured;

		// Tracks the boss currently inside its one allowed AI tick, so any NPC it spawns
		// during that tick (Golem's fists, a worm's body segments...) can inherit "no AI"
		// even if it doesn't use npc.realLife to link back to its parent - not every
		// multi-part boss does.
		private static int spawningParentWhoAmI = -1;

		public override void OnSpawn(NPC npc, IEntitySource source) {
			if (spawningParentWhoAmI >= 0 && spawningParentWhoAmI != npc.whoAmI)
				HasAI = false;
		}

		public override bool PreAI(NPC npc) {
			// A piece spawned outside its parent's tick window above (or in some other
			// way) can still often be identified through vanilla's own npc.realLife link.
			if (npc.realLife >= 0 && npc.realLife < Main.maxNPCs && npc.realLife != npc.whoAmI) {
				if (!Main.npc[npc.realLife].GetGlobalNPC<MobLabGlobalNPC>().HasAI)
					HasAI = false;
			}

			if (HasAI)
				return true;

			// Only bosses get the one-tick grace period - an ordinary enemy has no parts
			// to spawn, so it would only spend that tick getting a free attack off (e.g. a
			// Demon getting one shot out before "no AI" ever kicks in).
			if (!hadFirstTick && npc.boss) {
				hadFirstTick = true;
				spawningParentWhoAmI = npc.whoAmI;
				return true;
			}

			if (!anchorCaptured) {
				anchorPosition = npc.position;
				anchorCaptured = true;
			}

			npc.position = anchorPosition;
			npc.velocity = Vector2.Zero;

			// Several bosses (Golem, Duke Fishron, King Slime, Empress of Light...) fade in
			// from transparent over their first several AI ticks by counting npc.alpha down
			// from 255 to 0 - a generic "spawn entrance" convention, not something specific
			// to any one boss. Only letting one tick through left that countdown barely
			// started, so the boss stayed almost fully transparent (or fully invisible)
			// forever. Force it opaque here instead of chasing this per boss.
			npc.alpha = 0;

			return false;
		}

		public override void PostAI(NPC npc) {
			if (spawningParentWhoAmI == npc.whoAmI)
				spawningParentWhoAmI = -1;
		}
	}
}
