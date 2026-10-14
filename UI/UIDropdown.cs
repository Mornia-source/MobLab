using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;

namespace MobLab.UI
{
	// Minimal dropdown: click the header to expand a fixed list of options below it.
	// Rows aren't real UIView children (list is small and rebuilt often), so option
	// clicks are hit-tested manually in Update() using the same frame's click transition
	// that the base class tracks.
	internal class UIDropdown : UIView
	{
		public class Option
		{
			public string Label;
			public object Value;

			public Option(string label, object value) {
				Label = label;
				Value = value;
			}
		}

		private const float RowHeight = 22f;
		private const int MaxVisibleRows = 6;

		public List<Option> Options { get; } = new List<Option>();
		public int SelectedIndex;
		public event EventHandler SelectionChanged;

		private bool expanded;
		private int scrollOffset;

		public override float Width { get; set; } = 170f;
		public override float Height { get; set; } = RowHeight;

		public Option Selected => Options.Count > 0 ? Options[Math.Clamp(SelectedIndex, 0, Options.Count - 1)] : null;

		private int VisibleRowCount => Math.Min(MaxVisibleRows, Options.Count);
		private int MaxScrollOffset => Math.Max(0, Options.Count - VisibleRowCount);

		public UIDropdown() {
			OnLeftClick += (s, e) => expanded = !expanded;
		}

		protected override void OnHidden() => expanded = false;

		public override void Update() {
			base.Update();

			if (!expanded)
				return;

			Vector2 topLeft = DrawPosition - Origin;
			var listRect = new Rectangle((int)topLeft.X, (int)(topLeft.Y + Height), (int)Width, (int)(VisibleRowCount * RowHeight));

			if (ScrollAmount != 0 && listRect.Contains(Main.mouseX, Main.mouseY))
				scrollOffset = Math.Clamp(scrollOffset - Math.Sign(ScrollAmount), 0, MaxScrollOffset);

			bool freshClick = MouseLeftDown && !MouseLeftDownPrevious;
			if (!freshClick)
				return;

			bool clickedRow = false;
			for (int i = 0; i < VisibleRowCount; i++) {
				int optionIndex = scrollOffset + i;
				var rowRect = new Rectangle((int)topLeft.X, (int)(topLeft.Y + Height + i * RowHeight), (int)Width, (int)RowHeight);
				if (rowRect.Contains(Main.mouseX, Main.mouseY)) {
					SelectedIndex = optionIndex;
					SelectionChanged?.Invoke(this, EventArgs.Empty);
					clickedRow = true;
					break;
				}
			}

			if (clickedRow || (!MouseInside && !listRect.Contains(Main.mouseX, Main.mouseY)))
				expanded = false;
		}

		public override void Draw(SpriteBatch spriteBatch) {
			if (!Visible)
				return;

			Vector2 topLeft = DrawPosition - Origin;
			Color headerColor = MouseInside ? new Color(90, 92, 180) : new Color(63, 65, 151) * 0.9f;
			RoundedPanel.Draw9Slice(spriteBatch, RoundedPanel.PanelTile, topLeft.X, topLeft.Y, Width, Height, headerColor, 6);

			var font = FontAssets.MouseText.Value;
			string label = Selected?.Label ?? "";
			Vector2 textPos = topLeft + new Vector2(6f, Height / 2f - font.MeasureString(label).Y * 0.85f / 2f);
			Utils.DrawBorderString(spriteBatch, label, textPos, Color.White, 0.85f);

			Vector2 arrowCenter = topLeft + new Vector2(Width - 12f, Height / 2f);
			Vector2 arrowOrigin = new Vector2(RoundedPanel.TriangleArrow.Width, RoundedPanel.TriangleArrow.Height) / 2f;
			var effect = expanded ? SpriteEffects.FlipVertically : SpriteEffects.None;
			spriteBatch.Draw(RoundedPanel.TriangleArrow, arrowCenter, null, Color.White, 0f, arrowOrigin, 0.7f, effect, 0f);

			if (expanded) {
				for (int i = 0; i < VisibleRowCount; i++) {
					int optionIndex = scrollOffset + i;
					Vector2 rowTopLeft = topLeft + new Vector2(0f, Height + i * RowHeight);
					var rowRect = new Rectangle((int)rowTopLeft.X, (int)rowTopLeft.Y, (int)Width, (int)RowHeight);
					bool hovered = rowRect.Contains(Main.mouseX, Main.mouseY);
					Color rowColor = optionIndex == SelectedIndex ? new Color(110, 112, 200) : (hovered ? new Color(80, 82, 165) : new Color(50, 52, 120) * 0.95f);
					RoundedPanel.Draw9Slice(spriteBatch, RoundedPanel.PanelTile, rowTopLeft.X, rowTopLeft.Y, Width, RowHeight, rowColor, 4);
					Vector2 rowTextPos = rowTopLeft + new Vector2(6f, RowHeight / 2f - font.MeasureString(Options[optionIndex].Label).Y * 0.8f / 2f);
					Utils.DrawBorderString(spriteBatch, Options[optionIndex].Label, rowTextPos, Color.White, 0.8f);
				}
			}

			base.Draw(spriteBatch);
		}
	}
}
