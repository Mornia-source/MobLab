using MobLab.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MobLab.Menus
{
	// Collapsible toolbar, one button for now ("open the mob spawner window").
	// Deliberately anchored to the bottom-RIGHT of the screen and slid out with its own
	// fold button, so it never sits on top of CheatSheet's fold button (bottom-left-ish,
	// slightly right of center) even if both mods are loaded together.
	internal class MobHotbar : UIView
	{
		private const float MoveSpeed = 8f;
		private const float RightMargin = 20f;
		private const float BottomMargin = 12f;
		private const float FoldButtonWidth = 56f;
		private const float FoldButtonHeight = 16f;

		private float lerp;
		private bool hidden;

		private readonly UITextButton foldButton;
		private readonly UIImage spawnMenuButton;

		public event EventHandler SpawnMenuClicked;

		private float ShownY => Main.screenHeight - Height - BottomMargin;
		private float HiddenY => Main.screenHeight;
		private float BarX => Main.screenWidth - Width - RightMargin;

		public MobHotbar(Texture2D spawnMenuIcon) {
			Width = 54f;
			Height = 54f;
			BackgroundColor = new Color(33, 15, 91) * 0.79f;

			spawnMenuButton = new UIImage(spawnMenuIcon, 1f) {
				DrawSlotBackground = true,
				Tooltip = MobLab.Text("Hotbar.OpenSpawner"),
			};
			spawnMenuButton.Position = new Vector2(8f, 8f);
			spawnMenuButton.OnLeftClick += (s, e) => SpawnMenuClicked?.Invoke(this, EventArgs.Empty);
			AddChild(spawnMenuButton);

			foldButton = new UITextButton("", FoldButtonWidth, FoldButtonHeight) { TextScale = 0.7f };
			foldButton.OnLeftClick += (s, e) => {
				if (hidden)
					Show();
				else
					Hide();
			};
			AddChild(foldButton);

			// Collapsed by default - the fold tab alone stays out of the way until the
			// player wants the hotbar.
			hidden = true;
			UpdateFoldButtonLabel();
		}

		public bool IsHidden => hidden;

		public void Show() {
			hidden = false;
			UpdateFoldButtonLabel();
		}

		public void Hide() {
			hidden = true;
			UpdateFoldButtonLabel();
		}

		private void UpdateFoldButtonLabel() {
			// Shown while expanded: "Collapse". Shown while collapsed: "Expand".
			foldButton.Label = hidden ? MobLab.Text("Hotbar.Expand") : MobLab.Text("Hotbar.Collapse");
		}

		public override void Update() {
			lerp = MathHelper.Clamp(lerp + (hidden ? -1f : 1f) * 0.01f * MoveSpeed, 0f, 1f);

			base.Update();
		}

		public override void Draw(SpriteBatch spriteBatch) {
			if (!Visible)
				return;

			// Main.screenWidth/Height only read as UI-scale-adjusted values while inside
			// MobLabSystem's UI-scale interface layer (i.e. right here, during Draw()).
			// Computing position from Update() instead put the bar at a screen-space
			// coordinate that only made sense pre-scaling, sliding it off the visible area
			// whenever UIScale != 1. So the position is computed here instead.
			float y = MathHelper.SmoothStep(HiddenY, ShownY, lerp);
			Position = new Vector2(BarX, y);
			foldButton.Position = new Vector2(Width / 2f - foldButton.Width / 2f, -foldButton.Height - 4f);

			Vector2 topLeft = DrawPosition - Origin;
			RoundedPanel.Draw9Slice(spriteBatch, RoundedPanel.PanelTile, topLeft.X, topLeft.Y, Width, Height, BackgroundColor);

			if (MouseInside || spawnMenuButton.MouseInside)
				Main.LocalPlayer.mouseInterface = true;

			base.Draw(spriteBatch);
		}
	}
}
