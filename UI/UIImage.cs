using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MobLab.UI
{
	// Simple icon/button node: draws a texture, optionally centered over a rounded
	// "slot" background tile so it reads as a clickable button.
	internal class UIImage : UIView
	{
		public Texture2D Texture;
		public float Scale = 1f;
		public bool DrawSlotBackground;
		public Color SlotBackgroundColor = new Color(63, 65, 151) * 0.9f;
		public SpriteEffects SpriteEffect = SpriteEffects.None;

		public UIImage(Texture2D texture, float scale = 1f) {
			Texture = texture;
			Scale = scale;
		}

		public override float Width => DrawSlotBackground ? 38f : (Texture?.Width ?? 0) * Scale;
		public override float Height => DrawSlotBackground ? 38f : (Texture?.Height ?? 0) * Scale;

		public override void Draw(SpriteBatch spriteBatch) {
			if (!Visible)
				return;

			Vector2 topLeft = DrawPosition - Origin;

			if (DrawSlotBackground)
				RoundedPanel.Draw9Slice(spriteBatch, RoundedPanel.PanelTile, topLeft.X, topLeft.Y, Width, Height, SlotBackgroundColor);

			if (Texture != null) {
				Vector2 iconCenter = topLeft + new Vector2(Width, Height) / 2f;
				Vector2 texOrigin = new Vector2(Texture.Width, Texture.Height) / 2f;
				spriteBatch.Draw(Texture, iconCenter, null, ForegroundColor * Opacity, 0f, texOrigin, Scale, SpriteEffect, 0f);
			}

			base.Draw(spriteBatch);
		}
	}
}
