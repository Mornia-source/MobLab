using MobLab.Spawning;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;

namespace MobLab.Menus
{
	// Shared "take frame 0 of a (possibly animated) NPC texture and scale it to fit inside
	// a maxDim x maxDim box" logic. Used by both the browser grid icons and the config
	// panel preview so neither ever draws a raw multi-frame strip at real size.
	internal static class NpcIconRenderer
	{
		public static void DrawFittedFrame(SpriteBatch spriteBatch, int npcType, Vector2 center, float maxDim, Color color) {
			NpcAssetLoader.EnsureLoaded(npcType);
			Texture2D texture = TextureAssets.Npc[npcType].Value;
			if (texture == null)
				return;

			int frameCount = Math.Max(1, Main.npcFrameCount[npcType]);
			var srcRect = new Rectangle(0, 0, texture.Width, texture.Height / frameCount);

			float scale = 1f;
			if (srcRect.Width > maxDim || srcRect.Height > maxDim)
				scale = maxDim / Math.Max(srcRect.Width, srcRect.Height);

			Vector2 origin = new Vector2(srcRect.Width, srcRect.Height) / 2f;
			spriteBatch.Draw(texture, center, srcRect, color, 0f, origin, scale, SpriteEffects.None, 0f);
		}
	}
}
