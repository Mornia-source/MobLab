using MobLab.Menus;
using MobLab.Spawning;
using MobLab.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace MobLab
{
	// Drives the whole custom UI tree and the cursor-placement state machine each frame.
	// Kept as a single ModSystem instead of scattering setup across the Mod class, unlike
	// CheatSheet which builds its windows directly in CheatSheet.SetupUI().
	internal class MobLabSystem : ModSystem
	{
		public static MobLabSystem Instance;

		private UIView root;
		private MobHotbar hotbar;
		private NPCSelectWindow npcSelectWindow;
		private NPCConfigPanel npcConfigPanel;
		private PlacementController placementController;

		public override void OnModLoad() {
			if (Main.dedServ)
				return;

			Instance = this;
		}

		// Building the NPC catalog calls NPC.SetDefaults for every type, which reads
		// per-type data (NPCID.Sets arrays etc.) that other mods are still resizing during
		// their own autoload step. Doing this from OnModLoad raced other mods loading and
		// threw IndexOutOfRangeException. PostSetupContent should in principle already run
		// after every mod's content is registered, but modded NPCs were still consistently
		// failing NPCLoader.GetNPC(type) lookups from there. CheatSheet builds its
		// equivalent NPC list from PostSetupRecipes (a later hook, after PostSetupContent)
		// and doesn't have that problem, so match its timing instead.
		public override void PostSetupRecipes() {
			if (Main.dedServ)
				return;

			// Still not guaranteed to be the main thread; creating GPU resources (procedural
			// RoundedPanel textures, Main.instance.LoadItem's immediate load) requires it,
			// so defer the whole setup there.
			Main.QueueMainThreadAction(SetupUI);
		}

		private void SetupUI() {
			try {
				RoundedPanel.Load(Main.graphics.GraphicsDevice);

				Main.instance.LoadItem(ItemID.SlimeStaff);
				Texture2D hotbarIcon = TextureAssets.Item[ItemID.SlimeStaff].Value;

				hotbar = new MobHotbar(hotbarIcon);
				npcSelectWindow = new NPCSelectWindow { Position = new Vector2(220f, 140f), Visible = false };
				npcConfigPanel = new NPCConfigPanel { Position = new Vector2(220f + npcSelectWindow.Width + 12f, 140f) };
				placementController = new PlacementController();

				hotbar.SpawnMenuClicked += (s, e) => npcSelectWindow.Visible = !npcSelectWindow.Visible;
				npcSelectWindow.NpcSelected += icon => npcConfigPanel.SetTemplate(icon);
				npcConfigPanel.PlaceRequested += overrides => {
					placementController.BeginPlacement(overrides);
					npcSelectWindow.Visible = false;
					npcConfigPanel.Visible = false;
				};

				root = new UIView();
				root.AddChild(hotbar);
				root.AddChild(npcSelectWindow);
				root.AddChild(npcConfigPanel);
			}
			catch (Exception e) {
				MobLab.Instance.Logger.Error("SetupUI failed", e);
			}
		}

		public override void OnWorldLoad() {
			if (Main.dedServ)
				return;

			placementController?.Cancel();
		}

		public void ToggleHotbar() {
			if (hotbar == null)
				return;

			if (hotbar.IsHidden)
				hotbar.Show();
			else
				hotbar.Hide();
		}

		// Two interface layers, matching HEROsMod's approach (HerosModSystem.ModifyInterfaceLayers):
		// - UI scale: our menu/hotbar tree. Main.screenWidth/Height and Main.mouseX/Y only read as
		//   UI-scale-adjusted values while inside a UI-scale layer; that's also the space our own
		//   Draw()-computed positions live in, so hit-testing needs to happen here too, not from
		//   PostUpdateInput (which sees the raw, unscaled values instead).
		// - Game scale: cursor-follow NPC placement. This needs Main.mouseX/Y in the same raw/world
		//   space as Main.screenPosition to compute a correct world click position; mixing a
		//   UI-scaled mouse position with world-scale screenPosition silently biased every spawn
		//   toward the screen's top-left corner whenever UIScale != 1.
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			if (Main.dedServ)
				return;

			int mouseTextIndex = layers.FindIndex(l => l.Name == "Vanilla: Mouse Text");
			if (mouseTextIndex != -1) {
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("MobLab: UI", DrawUiLayer, InterfaceScaleType.UI));
			}

			int rulerIndex = layers.FindIndex(l => l.Name == "Vanilla: Ruler");
			if (rulerIndex != -1) {
				layers.Insert(rulerIndex, new LegacyGameInterfaceLayer("MobLab: Placement", DrawPlacementLayer, InterfaceScaleType.Game));
			}
		}

		private bool DrawUiLayer() {
			if (Main.gameMenu || root == null)
				return true;

			root.UpdateRoot();
			root.Draw(Main.spriteBatch);

			if (UIView.HoverText.Length > 0) {
				Vector2 pos = new Vector2(Main.mouseX, Main.mouseY) + new Vector2(20f);
				Utils.DrawBorderString(Main.spriteBatch, UIView.HoverText, pos, Color.White, 1f);
			}

			return true;
		}

		private bool DrawPlacementLayer() {
			if (Main.gameMenu || placementController == null)
				return true;

			placementController.Update();
			placementController.DrawGhostCursor(Main.spriteBatch);
			return true;
		}

		public override void Unload() {
			Instance = null;
			root = null;
			hotbar = null;
			npcSelectWindow = null;
			npcConfigPanel = null;
			placementController = null;
			if (!Main.dedServ)
				RoundedPanel.Unload();
		}
	}
}
