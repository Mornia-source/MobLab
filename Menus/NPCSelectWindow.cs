using MobLab.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;

namespace MobLab.Menus
{
	// NPC browser: search + category filters over a scrollable grid, adapted from
	// CheatSheet's NPCBrowser/NPCView/NPCSlot trio but simplified (no filter/disable
	// feature, no negative netID town-transform slots) and left-click only *selects*
	// a template instead of spawning immediately.
	internal class NPCSelectWindow : UIWindow
	{
		private const float Spacing = 10f;
		private const int Columns = 7;
		private const float CellSize = 42f;
		private const float CellGap = 4f;

		private readonly List<NPCSlotIcon> allIcons = new List<NPCSlotIcon>();
		private readonly Dictionary<string, List<NPCSlotIcon>> byMod = new Dictionary<string, List<NPCSlotIcon>>();
		private readonly List<string> modNames = new List<string>();
		private int modCycleIndex = -1;

		private readonly UIScrollView grid;
		private readonly UITextbox searchBox;
		private readonly UITextButton allButton;
		private readonly UITextButton bossButton;
		private readonly UITextButton townButton;
		private readonly UITextButton modButton;

		private List<NPCSlotIcon> currentList;
		private NPCSlotIcon selectedIcon;

		public event Action<NPCSlotIcon> NpcSelected;
		public int NpcCount => allIcons.Count;

		public NPCSelectWindow() {
			CanMove = true;
			Width = Columns * (CellSize + CellGap) + CellGap + Spacing * 2f + 14f;
			Height = 430f;
			BackgroundColor = new Color(28, 14, 70) * 0.88f;

			BuildIconCatalog();

			var closeButton = new UIImage(RoundedPanel.CloseIcon, 1f) { Anchor = AnchorPosition.TopRight };
			closeButton.Position = new Vector2(Width - Spacing, Spacing);
			closeButton.OnLeftClick += (s, e) => Visible = false;
			AddChild(closeButton);

			float buttonY = Spacing;
			float buttonW = 60f;
			float buttonH = 22f;
			allButton = new UITextButton(MobLab.Text("Browser.All"), buttonW, buttonH) { Position = new Vector2(Spacing, buttonY), Active = true };
			bossButton = new UITextButton(MobLab.Text("Browser.Bosses"), buttonW, buttonH) { Position = new Vector2(Spacing + (buttonW + 6f), buttonY) };
			townButton = new UITextButton(MobLab.Text("Browser.Town"), buttonW, buttonH) { Position = new Vector2(Spacing + (buttonW + 6f) * 2, buttonY) };
			modButton = new UITextButton(MobLab.Text("Browser.Modded"), buttonW, buttonH) { Position = new Vector2(Spacing + (buttonW + 6f) * 3, buttonY) };
			allButton.OnLeftClick += (s, e) => ShowCategory(allIcons, allButton);
			bossButton.OnLeftClick += (s, e) => ShowCategory(allIcons.Where(i => i.IsBoss).ToList(), bossButton);
			townButton.OnLeftClick += (s, e) => ShowCategory(allIcons.Where(i => i.IsTownNpc).ToList(), townButton);
			modButton.OnLeftClick += (s, e) => CycleModCategory();
			AddChild(allButton);
			AddChild(bossButton);
			AddChild(townButton);
			AddChild(modButton);

			searchBox = new UITextbox { Placeholder = MobLab.Text("Browser.SearchPlaceholder") };
			searchBox.Width = Width - Spacing * 2f;
			searchBox.Position = new Vector2(Spacing, buttonY + buttonH + 8f);
			searchBox.TextChanged += (s, e) => ApplySearch();
			AddChild(searchBox);

			grid = new UIScrollView();
			grid.Width = Width - Spacing * 2f;
			grid.Height = Height - (searchBox.Position.Y + searchBox.Height) - Spacing * 2f;
			grid.Position = new Vector2(Spacing, searchBox.Position.Y + searchBox.Height + Spacing);
			grid.RowHeight = CellSize + CellGap;
			AddChild(grid);

			ShowCategory(allIcons, allButton);
		}

		private void BuildIconCatalog() {
			for (int type = 1; type < TextureAssets.Npc.Length; type++) {
				NPCSlotIcon icon;
				try {
					icon = new NPCSlotIcon(type);
				}
				catch {
					continue;
				}

				if (icon.DisplayName == null || string.IsNullOrWhiteSpace(icon.DisplayName.Value))
					continue;

				icon.OnSelectNpc += SelectIcon;
				allIcons.Add(icon);

				if (icon.ModOwner != null) {
					if (!byMod.TryGetValue(icon.ModOwner, out var list))
						byMod[icon.ModOwner] = list = new List<NPCSlotIcon>();
					list.Add(icon);
				}
			}

			modNames.AddRange(byMod.Keys.OrderBy(n => n));
		}

		private void CycleModCategory() {
			if (modNames.Count == 0) {
				Terraria.Main.NewText(MobLab.Text("Browser.NoModdedNpcs"), Color.OrangeRed);
				return;
			}

			modCycleIndex = (modCycleIndex + 1) % modNames.Count;
			string modName = modNames[modCycleIndex];
			modButton.Label = modName;
			ShowCategory(byMod[modName], modButton);
		}

		private void ShowCategory(List<NPCSlotIcon> icons, UITextButton active) {
			currentList = icons;
			foreach (var b in new[] { allButton, bossButton, townButton, modButton })
				b.Active = b == active;

			searchBox.Text = "";
			LayoutGrid(currentList);
		}

		private void ApplySearch() {
			string query = searchBox.Text.Trim();
			var source = currentList ?? allIcons;
			if (query.Length == 0) {
				LayoutGrid(source);
				return;
			}

			var filtered = source.Where(i => i.DisplayName.Value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
			LayoutGrid(filtered);
		}

		private void LayoutGrid(List<NPCSlotIcon> icons) {
			grid.ClearContent();
			for (int i = 0; i < icons.Count; i++) {
				var icon = icons[i];
				icon.Selected = icon == selectedIcon;
				int col = i % Columns;
				int row = i / Columns;
				icon.Position = new Vector2(CellGap + col * (CellSize + CellGap), CellGap + row * (CellSize + CellGap));
				icon.Offset = Vector2.Zero;
				grid.AddChild(icon);
			}
			int rowCount = (int)Math.Ceiling(icons.Count / (float)Columns);
			grid.ContentHeight = rowCount * (CellSize + CellGap) + CellGap;
			grid.ScrollPosition = 0f;
		}

		private void SelectIcon(NPCSlotIcon icon) {
			selectedIcon = icon;
			foreach (var i in allIcons)
				i.Selected = i == icon;
			NpcSelected?.Invoke(icon);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			if (!Visible)
				return;

			base.Draw(spriteBatch);

			if (MouseInside)
				Terraria.Main.LocalPlayer.mouseInterface = true;
		}
	}
}
