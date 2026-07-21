using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MobLab
{
	public class MobLab : Mod
	{
		internal static MobLab Instance;
		internal static ModKeybind ToggleHotbarKeybind;

		public override void Load() {
			Instance = this;

			if (!Main.dedServ)
				ToggleHotbarKeybind = KeybindLoader.RegisterKeybind(this, "ToggleMobLabHotbar", "L");
		}

		public override void Unload() {
			Instance = null;
			ToggleHotbarKeybind = null;
		}

		internal static string Text(string key) => Language.GetTextValue($"Mods.MobLab.{key}");
	}
}
