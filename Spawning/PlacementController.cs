using MobLab.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace MobLab.Spawning
{
	// Cursor-follow placement mode: instead of CheatSheet's "spawn 200px in front of the
	// player", the player carries the chosen NPC's texture on the cursor and left-clicks
	// anywhere on screen to spawn it at that exact world position.
	internal class PlacementController
	{
		private bool prevLeft;
		private bool prevRight;
		private bool prevEscape;

		public StatOverrides Pending { get; private set; }
		public bool Active => Pending != null;

		public void BeginPlacement(StatOverrides overrides) => Pending = overrides;

		public void Cancel() => Pending = null;

		public void Update() {
			var mouseState = Mouse.GetState();
			bool leftDown = mouseState.LeftButton == ButtonState.Pressed;
			bool rightDown = mouseState.RightButton == ButtonState.Pressed;
			bool escapeDown = Keyboard.GetState().IsKeyDown(Keys.Escape);

			bool leftClick = leftDown && !prevLeft;
			bool rightClick = rightDown && !prevRight;
			bool escapePress = escapeDown && !prevEscape;

			prevLeft = leftDown;
			prevRight = rightDown;
			prevEscape = escapeDown;

			if (!Active)
				return;

			if (rightClick || escapePress) {
				Cancel();
				return;
			}

			if (!leftClick)
				return;

			// Only block on clicks our own UI already claimed this frame (e.g. the browser
			// window is still open). Main.LocalPlayer.mouseInterface is intentionally not
			// checked here - DrawGhostCursor sets it every frame during placement mode to
			// suppress the player's held item, which would otherwise block every click.
			if (UIView.MouseClaimedByUI)
				return;

			if (Main.netMode != NetmodeID.SinglePlayer) {
				Main.NewText(MobLab.Text("Placement.MultiplayerUnsupported"), Color.OrangeRed);
				Cancel();
				return;
			}

			// Called from a Game-scale interface layer (see MobLabSystem.ModifyInterfaceLayers),
			// where Main.mouseX/Y are in the same raw/world pixel space as Main.screenPosition -
			// matches how HEROsMod's FlyCam computes cursorWorldPosition.
			Vector2 worldPosition = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY);
			SpawnLogic.ConfiguredSpawn(worldPosition, Pending);

			// Holding Shift keeps the same configured NPC on the cursor for repeated
			// placement, instead of returning to the browser after every single spawn.
			bool keepPlacing = Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift);
			if (!keepPlacing)
				Cancel();
		}

		public void DrawGhostCursor(SpriteBatch spriteBatch) {
			if (!Active)
				return;

			NpcAssetLoader.EnsureLoaded(Pending.NpcType);
			var texture = TextureAssets.Npc[Pending.NpcType].Value;
			if (texture == null)
				return;

			int frameCount = Math.Max(1, Main.npcFrameCount[Pending.NpcType]);
			var srcRect = new Rectangle(0, 0, texture.Width, texture.Height / frameCount);

			// Real display scale (not fit-to-a-box like the grid icons) - the ghost should
			// show how big the NPC will actually look once placed.
			float scale = Pending.NativeScale;

			// Anchored on the sprite frame's geometric center, matching SpawnLogic's use of
			// the NPC's hitbox center as the spawn target - center-anchoring is far less
			// sensitive to per-sprite padding (idle animation overhang etc.) than a feet
			// anchor, since it doesn't matter whether that padding sits above or below.
			Vector2 position = new Vector2(Main.mouseX, Main.mouseY);
			Vector2 origin = new Vector2(srcRect.Width, srcRect.Height) / 2f;
			spriteBatch.Draw(texture, position, srcRect, Color.White * 0.65f, 0f, origin, scale, SpriteEffects.None, 0f);

			Main.LocalPlayer.mouseInterface = true;
			Main.LocalPlayer.cursorItemIconEnabled = false;

			string hint = MobLab.Text("Placement.HintText");
			var font = FontAssets.MouseText.Value;
			Vector2 hintPos = position + new Vector2(20f, 20f);
			Utils.DrawBorderString(spriteBatch, hint, hintPos, Color.White, 0.85f);
		}
	}
}
