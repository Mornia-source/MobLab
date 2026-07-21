using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;

namespace MobLab.UI
{
	// Text input box driven by the standard tModLoader Main.GetInputText recipe.
	// Focus is fully self-contained (no external registry needed): a fresh left-click
	// selects this box only if it landed inside it, and deselects it otherwise.
	internal class UITextbox : UIView
	{
		public string Text = "";
		public bool Selected;
		public bool NumericOnly;
		public bool AllowDecimal;
		public string Placeholder = "";

		public event EventHandler TextChanged;

		public override float Width { get; set; } = 120f;
		public override float Height { get; set; } = 24f;

		public override void Update() {
			bool freshLeftClick = MouseLeftDown && !MouseLeftDownPrevious;
			if (freshLeftClick)
				Selected = MouseInside;

			if (Selected) {
				PlayerInput.WritingText = true;
				Main.instance.HandleIME();
				string newText = Main.GetInputText(Text);
				if (NumericOnly)
					newText = FilterNumeric(newText);
				if (newText != Text) {
					Text = newText;
					TextChanged?.Invoke(this, EventArgs.Empty);
				}
			}

			base.Update();
		}

		private string FilterNumeric(string input) {
			var sb = new StringBuilder();
			bool hasDot = false;
			foreach (char c in input) {
				if (char.IsDigit(c)) {
					sb.Append(c);
				}
				else if (AllowDecimal && c == '.' && !hasDot) {
					hasDot = true;
					sb.Append(c);
				}
			}
			return sb.ToString();
		}

		public override void Draw(SpriteBatch spriteBatch) {
			if (!Visible)
				return;

			Vector2 topLeft = DrawPosition - Origin;
			Color boxColor = Selected ? new Color(90, 92, 180) : new Color(50, 52, 120) * 0.9f;
			RoundedPanel.Draw9Slice(spriteBatch, RoundedPanel.PanelTile, topLeft.X, topLeft.Y, Width, Height, boxColor, 6);

			string display = Text.Length > 0 ? Text : Placeholder;
			Color textColor = Text.Length > 0 ? Color.White : Color.Gray;
			if (Selected && Main.GameUpdateCount % 40 < 20)
				display += "|";

			var font = FontAssets.MouseText.Value;
			Vector2 textPos = topLeft + new Vector2(6f, Height / 2f - font.MeasureString(display).Y * 0.9f / 2f);
			Utils.DrawBorderString(spriteBatch, display, textPos, textColor, 0.9f);

			base.Draw(spriteBatch);
		}
	}
}
