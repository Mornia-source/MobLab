using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameInput;

namespace MobLab.UI
{
	// Draggable panel window. Background is our own small-corner-radius RoundedPanel
	// 9-slice instead of CheatSheet's Utils.DrawInvBG (vanilla inventory panel, large corners).
	internal class UIWindow : UIView
	{
		public bool CanMove = true;

		private bool dragging;
		private Vector2 dragGrabOffset;

		public UIWindow() {
			BackgroundColor = new Color(33, 15, 91) * 0.79f;
			OnMouseDown += (s, button) => {
				if (CanMove && button == 0) {
					dragging = true;
					dragGrabOffset = new Vector2(Main.mouseX, Main.mouseY) - (DrawPosition - Origin);
				}
			};
			OnMouseUp += (s, button) => {
				if (button == 0)
					dragging = false;
			};
		}

		public override void Update() {
			if (dragging) {
				if (!MouseLeftDown) {
					dragging = false;
				}
				else {
					Vector2 topLeft = new Vector2(Main.mouseX, Main.mouseY) - dragGrabOffset;
					Position = topLeft + Origin - (Parent != null ? Parent.DrawPosition - Parent.Origin : Vector2.Zero);
				}
			}

			if (MouseInside)
				PlayerInput.LockVanillaMouseScroll("MobLab/UIWindow");

			base.Update();
		}

		public override void Draw(SpriteBatch spriteBatch) {
			if (!Visible)
				return;

			Vector2 topLeft = DrawPosition - Origin;
			RoundedPanel.Draw9Slice(spriteBatch, RoundedPanel.PanelTile, topLeft.X, topLeft.Y, Width, Height, BackgroundColor);

			base.Draw(spriteBatch);
		}
	}
}
