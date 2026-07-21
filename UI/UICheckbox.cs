using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MobLab.UI
{
	internal class UICheckbox : UIView
	{
		private const float BoxSize = 18f;

		public bool Checked;
		public event EventHandler CheckedChanged;

		public UICheckbox(bool startChecked = false) {
			Checked = startChecked;
			OnLeftClick += (s, e) => {
				Checked = !Checked;
				CheckedChanged?.Invoke(this, EventArgs.Empty);
			};
		}

		public override float Width { get => BoxSize; set { } }
		public override float Height { get => BoxSize; set { } }

		public override void Draw(SpriteBatch spriteBatch) {
			if (!Visible)
				return;

			Vector2 topLeft = DrawPosition - Origin;
			Color boxColor = MouseInside ? new Color(90, 92, 180) : new Color(63, 65, 151) * 0.9f;
			RoundedPanel.Draw9Slice(spriteBatch, RoundedPanel.PanelTile, topLeft.X, topLeft.Y, BoxSize, BoxSize, boxColor, 6);

			if (Checked) {
				Vector2 center = topLeft + new Vector2(BoxSize, BoxSize) / 2f;
				Vector2 origin = new Vector2(RoundedPanel.CheckMark.Width, RoundedPanel.CheckMark.Height) / 2f;
				spriteBatch.Draw(RoundedPanel.CheckMark, center, null, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
			}

			base.Draw(spriteBatch);
		}
	}
}
