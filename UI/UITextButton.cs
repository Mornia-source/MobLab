using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace MobLab.UI
{
	// Small rounded-rect button with a centered text label (category filters, action buttons).
	internal class UITextButton : UIView
	{
		public string Label;
		public bool Active;
		public float TextScale = 0.85f;

		public override float Width { get; set; }
		public override float Height { get; set; }

		public UITextButton(string label, float width, float height) {
			Label = label;
			Width = width;
			Height = height;
		}

		public override void Draw(SpriteBatch spriteBatch) {
			if (!Visible)
				return;

			Vector2 topLeft = DrawPosition - Origin;
			Color color = Active ? new Color(140, 142, 230) : (MouseInside ? new Color(90, 92, 180) : new Color(63, 65, 151) * 0.9f);
			RoundedPanel.Draw9Slice(spriteBatch, RoundedPanel.PanelTile, topLeft.X, topLeft.Y, Width, Height, color, 6);

			var font = FontAssets.MouseText.Value;
			float scale = TextScale;
			float rawWidth = font.MeasureString(Label).X;
			float maxWidth = Width - 8f;
			if (rawWidth * scale > maxWidth && rawWidth > 0f)
				scale = maxWidth / rawWidth;

			Vector2 textSize = font.MeasureString(Label) * scale;
			Vector2 textPos = topLeft + new Vector2(Width / 2f - textSize.X / 2f, Height / 2f - textSize.Y / 2f);
			Utils.DrawBorderString(spriteBatch, Label, textPos, Color.White, scale);

			base.Draw(spriteBatch);
		}
	}
}
