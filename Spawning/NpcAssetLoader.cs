using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;

namespace MobLab.Spawning
{
	internal static class NpcAssetLoader
	{
		public static void EnsureLoaded(int npcType) {
			if (TextureAssets.Npc[npcType].State == AssetState.NotLoaded)
				Main.Assets.Request<Texture2D>(TextureAssets.Npc[npcType].Name, AssetRequestMode.AsyncLoad);
		}
	}
}
