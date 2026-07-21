using MobLab.Spawning;
using MobLab.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Globalization;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace MobLab.Menus
{
	// Stat/AI configuration panel shown once a template is picked in NPCSelectWindow.
	// This is the piece CheatSheet doesn't have at all - it never lets you edit a spawn
	// before it happens, it just spawns the vanilla NPC immediately.
	internal class NPCConfigPanel : UIWindow
	{
		// Localization key (under "AIStyle.") + NPCAIStyleID value for each curated option.
		private static readonly (string Key, int Style)[] CuratedAiStyles = {
			("Fighter", NPCAIStyleID.Fighter),
			("Flying", NPCAIStyleID.Flying),
			("Passive", NPCAIStyleID.Passive),
			("Bat", NPCAIStyleID.Bat),
			("Bird", NPCAIStyleID.Bird),
			("Vulture", NPCAIStyleID.Vulture),
			("Jellyfish", NPCAIStyleID.Jellyfish),
			("FlyingFish", NPCAIStyleID.FlyingFish),
			("Firefly", NPCAIStyleID.Firefly),
			("Butterfly", NPCAIStyleID.Butterfly),
			("Snail", NPCAIStyleID.Snail),
			("Duck", NPCAIStyleID.Duck),
			("Spider", NPCAIStyleID.Spider),
		};

		private const float Spacing = 10f;

		private NPCSlotIcon template;
		private int? previewNpcType;

		private readonly UIImage preview;
		private readonly UITextbox lifeBox;
		private readonly UITextbox defenseBox;
		private readonly UITextbox damageBox;
		private readonly UITextbox knockbackBox;
		private readonly UICheckbox hasAiCheckbox;
		private readonly UIDropdown aiDropdown;
		private readonly UITextButton placeButton;
		private readonly UITextButton cancelButton;

		public event Action<StatOverrides> PlaceRequested;
		public event Action CancelRequested;

		public NPCConfigPanel() {
			CanMove = true;
			Width = 250f;
			Height = 400f;
			BackgroundColor = new Color(28, 14, 70) * 0.88f;
			Visible = false;

			var closeButton = new UIImage(RoundedPanel.CloseIcon, 1f) { Anchor = AnchorPosition.TopRight };
			closeButton.Position = new Vector2(Width - Spacing, Spacing);
			closeButton.OnLeftClick += (s, e) => Cancel();
			AddChild(closeButton);

			preview = new UIImage(null, 1f) { DrawSlotBackground = true };
			preview.Position = new Vector2(Width / 2f - preview.Width / 2f, Spacing + 4f);
			AddChild(preview);

			float y = Spacing + 4f + 42f + 12f;
			lifeBox = AddStatRow(MobLab.Text("Config.Life"), ref y);
			defenseBox = AddStatRow(MobLab.Text("Config.Defense"), ref y);
			damageBox = AddStatRow(MobLab.Text("Config.Damage"), ref y);
			knockbackBox = AddStatRow(MobLab.Text("Config.Knockback"), ref y, allowDecimal: true);

			y += 4f;
			hasAiCheckbox = new UICheckbox(true) { Position = new Vector2(Spacing, y) };
			AddChild(hasAiCheckbox);
			y += 26f;

			aiDropdown = new UIDropdown { Position = new Vector2(Spacing, y), Width = Width - Spacing * 2f };
			aiDropdown.Options.Add(new UIDropdown.Option(MobLab.Text("Config.AIDefault"), null));
			foreach (var style in CuratedAiStyles)
				aiDropdown.Options.Add(new UIDropdown.Option(MobLab.Text("AIStyle." + style.Key), style.Style));
			y += 30f;

			placeButton = new UITextButton(MobLab.Text("Config.Place"), (Width - Spacing * 2f - 8f) / 2f, 28f);
			placeButton.Position = new Vector2(Spacing, Height - Spacing - placeButton.Height);
			placeButton.OnLeftClick += (s, e) => RequestPlacement();
			AddChild(placeButton);

			cancelButton = new UITextButton(MobLab.Text("Config.Cancel"), (Width - Spacing * 2f - 8f) / 2f, 28f);
			cancelButton.Position = new Vector2(Spacing + placeButton.Width + 8f, Height - Spacing - placeButton.Height);
			cancelButton.OnLeftClick += (s, e) => Cancel();
			AddChild(cancelButton);

			// Added last (drawn/hit-tested last = on top) so its expanded option list
			// renders over the Place/Cancel buttons instead of being covered by them.
			AddChild(aiDropdown);
		}

		private UITextbox AddStatRow(string label, ref float y, bool allowDecimal = false) {
			var box = new UITextbox {
				NumericOnly = true,
				AllowDecimal = allowDecimal,
				Position = new Vector2(Width - Spacing - 90f, y),
				Width = 90f,
				Height = 22f,
			};
			AddChild(box);
			y += 28f;
			return box;
		}

		public void SetTemplate(NPCSlotIcon icon) {
			template = icon;
			previewNpcType = icon.NpcType;

			lifeBox.Text = icon.LifeMax.ToString(CultureInfo.InvariantCulture);
			defenseBox.Text = icon.Defense.ToString(CultureInfo.InvariantCulture);
			damageBox.Text = icon.Damage.ToString(CultureInfo.InvariantCulture);
			knockbackBox.Text = icon.KnockbackResist.ToString("0.##", CultureInfo.InvariantCulture);
			hasAiCheckbox.Checked = true;
			aiDropdown.SelectedIndex = 0;

			Visible = true;
		}

		private void RequestPlacement() {
			if (template == null)
				return;

			var overrides = new StatOverrides {
				NpcType = template.NpcType,
				LifeMax = ParseIntOrDefault(lifeBox.Text, template.LifeMax, min: 1),
				Defense = ParseIntOrDefault(defenseBox.Text, template.Defense, min: 0),
				Damage = ParseIntOrDefault(damageBox.Text, template.Damage, min: 0),
				KnockbackResist = ParseFloatOrDefault(knockbackBox.Text, template.KnockbackResist),
				NativeScale = template.NativeScale,
				HitboxWidth = template.HitboxWidth,
				HitboxHeight = template.HitboxHeight,
				HasAI = hasAiCheckbox.Checked,
				AiStyleOverride = (int?)aiDropdown.Selected?.Value,
			};

			PlaceRequested?.Invoke(overrides);
		}

		private void Cancel() {
			template = null;
			Visible = false;
			CancelRequested?.Invoke();
		}

		private static int ParseIntOrDefault(string text, int fallback, int min) {
			if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
				return Math.Max(min, value);
			return fallback;
		}

		private static float ParseFloatOrDefault(string text, float fallback) {
			if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
				return Math.Max(0f, value);
			return fallback;
		}

		public override void Draw(SpriteBatch spriteBatch) {
			if (!Visible)
				return;

			base.Draw(spriteBatch);

			if (MouseInside)
				Main.LocalPlayer.mouseInterface = true;

			if (previewNpcType.HasValue) {
				Vector2 previewCenter = preview.DrawPosition - preview.Origin + new Vector2(preview.Width, preview.Height) / 2f;
				NpcIconRenderer.DrawFittedFrame(spriteBatch, previewNpcType.Value, previewCenter, preview.Width * 0.7f, Color.White);
			}

			DrawLabelLeftOf(spriteBatch, MobLab.Text("Config.Life"), lifeBox);
			DrawLabelLeftOf(spriteBatch, MobLab.Text("Config.Defense"), defenseBox);
			DrawLabelLeftOf(spriteBatch, MobLab.Text("Config.Damage"), damageBox);
			DrawLabelLeftOf(spriteBatch, MobLab.Text("Config.Knockback"), knockbackBox);

			Vector2 checkboxTextPos = hasAiCheckbox.DrawPosition - hasAiCheckbox.Origin + new Vector2(hasAiCheckbox.Width + 6f, 1f);
			Utils.DrawBorderString(spriteBatch, MobLab.Text("Config.HasAI"), checkboxTextPos, Color.White, 0.85f);
		}

		private void DrawLabelLeftOf(SpriteBatch spriteBatch, string label, UIView field) {
			var font = FontAssets.MouseText.Value;
			Vector2 panelTopLeft = DrawPosition - Origin;
			Vector2 fieldTopLeft = field.DrawPosition - field.Origin;
			Vector2 pos = new Vector2(panelTopLeft.X + Spacing, fieldTopLeft.Y + field.Height / 2f - font.MeasureString(label).Y * 0.8f / 2f);
			Utils.DrawBorderString(spriteBatch, label, pos, Color.White, 0.8f);
		}
	}
}
