using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MobLab.UI
{
	// Small rounded-rect scrollbar thumb, resized/positioned by the owning UIScrollView.
	internal class UIScrollBar : UIView
	{
		public override float Width { get; set; } = 8f;
		public override float Height { get; set; } = 100f;

		public override void Draw(SpriteBatch spriteBatch) {
			if (!Visible)
				return;

			Vector2 topLeft = DrawPosition - Origin;
			Color color = MouseInside ? new Color(140, 142, 220) : new Color(100, 102, 190);
			RoundedPanel.Draw9Slice(spriteBatch, RoundedPanel.PanelTile, topLeft.X, topLeft.Y, Width, Height, color, 4);

			base.Draw(spriteBatch);
		}
	}
}
