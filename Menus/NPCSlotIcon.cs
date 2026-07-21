using MobLab.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MobLab.Menus
{
	// One grid cell in the NPC browser. Computes its template stats once (via a throwaway
	// NPC.SetDefaults call) so NPCConfigPanel can prefill from them, mirroring the
	// life/damage/defense/knockback lookup CheatSheet's NPCBrowser tooltip does.
	internal class NPCSlotIcon : UIView
	{
		public readonly int NpcType;
		public readonly int NetID;
		public readonly LocalizedText DisplayName;
		public readonly string ModOwner;
		public readonly bool IsBoss;
		public readonly bool IsTownNpc;

		public readonly int LifeMax;
		public readonly int Defense;
		public readonly int Damage;
		public readonly float KnockbackResist;
		public readonly float NativeScale;
		public readonly float HitboxWidth;
		public readonly float HitboxHeight;

		public bool Selected;

		public event Action<NPCSlotIcon> OnSelectNpc;

		public override float Width { get; set; } = 42f;
		public override float Height { get; set; } = 42f;

		public NPCSlotIcon(int npcType) {
			NpcType = npcType;

			var npc = new NPC();
			npc.SetDefaults(npcType);
			NetID = npc.netID;
			LifeMax = npc.lifeMax;
			Defense = npc.defDefense;
			Damage = npc.defDamage;
			KnockbackResist = npc.knockBackResist;
			NativeScale = npc.scale;
			HitboxWidth = npc.width;
			HitboxHeight = npc.height;
			IsBoss = npc.boss;
			IsTownNpc = npc.townNPC;
			// npc.ModNPC (HEROsMod's MobSpawner.NPCStats uses exactly this, off the same
			// bare `new NPC(); SetDefaults()` pattern) usually resolves it; NPCLoader.GetNPC
			// (CheatSheet's NPCBrowser approach, a registry lookup by type rather than an
			// instance property) as a fallback in case a particular NPC's ModNPC clone
			// doesn't get attached for some reason.
			ModOwner = npc.ModNPC?.Mod.Name ?? (npcType >= Terraria.ID.NPCID.Count ? NPCLoader.GetNPC(npcType)?.Mod.Name : null);
			DisplayName = Lang.GetNPCName(npc.netID);

			Tooltip = DisplayName.Value + (ModOwner != null ? $" [{ModOwner}]" : "");
			OnLeftClick += (s, e) => OnSelectNpc?.Invoke(this);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			if (!Visible)
				return;

			Vector2 topLeft = DrawPosition - Origin;
			Color bg = Selected ? new Color(150, 152, 235) : (MouseInside ? new Color(90, 92, 180) : new Color(63, 65, 151) * 0.9f);
			RoundedPanel.Draw9Slice(spriteBatch, RoundedPanel.PanelTile, topLeft.X, topLeft.Y, Width, Height, bg, 7);

			Vector2 center = topLeft + new Vector2(Width, Height) / 2f;
			NpcIconRenderer.DrawFittedFrame(spriteBatch, NpcType, center, Width * 0.64f, Color.White);

			base.Draw(spriteBatch);
		}
	}
}
