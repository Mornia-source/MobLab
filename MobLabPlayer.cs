using Terraria.GameInput;
using Terraria.ModLoader;

namespace MobLab
{
	// Keybind fallback (default "L") for showing/hiding the hotbar, independent of
	// clicking the small fold tab - useful the first time, or if the tab is ever missed.
	internal class MobLabPlayer : ModPlayer
	{
		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (MobLab.ToggleHotbarKeybind != null && MobLab.ToggleHotbarKeybind.JustPressed)
				MobLabSystem.Instance?.ToggleHotbar();
		}
	}
}
