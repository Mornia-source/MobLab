using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MobLab.UI
{
	// Scrollable container. Content items are regular children added after the scrollbar
	// (always Children[0]); ScrollPosition is applied to all of them uniformly.
	//
	// Scrolling is snapped to whole RowHeight increments rather than continuous pixels.
	// That's a deliberate trade for robustness: a scissor-rect clip (tried twice before
	// this) needs a pixel-accurate rectangle kept in sync with Main.UIScaleMatrix across
	// several nested Begin/End calls, and got that wrong more than once, at one point
	// hiding an entire window. Snapping to rows means a row is always either fully shown
	// or fully hidden, so plain Visible=false on out-of-view rows is enough for both
	// drawing and click-blocking - no scissor rect, no partial-row edge cases.
	internal class UIScrollView : UIView
	{
		private readonly UIScrollBar scrollBar = new UIScrollBar();

		private bool draggingScrollBar;
		private float dragGrabY;
		private float scrollPosition;

		public float ContentHeight { get; set; }

		// Set by the owner to the height of one row (including gap) for row-snapped
		// scrolling. Leave at the default (1f) for effectively continuous scrolling.
		public float RowHeight = 1f;

		private float MaxScroll => Math.Max(0f, ContentHeight - Height);

		public float ScrollPosition {
			get => scrollPosition;
			set => scrollPosition = SnapToRow(MathHelper.Clamp(value, 0f, MaxScroll));
		}

		public override float Width { get; set; } = 300f;
		public override float Height { get; set; } = 200f;

		public Color BackgroundColor2 = new Color(20, 10, 55) * 0.6f;

		public UIScrollView() {
			AddChild(scrollBar);
			scrollBar.OnMouseDown += (s, button) => {
				if (button == 0) {
					draggingScrollBar = true;
					dragGrabY = Main.mouseY - scrollBar.Position.Y;
				}
			};
		}

		private float SnapToRow(float value) {
			if (RowHeight <= 1f)
				return value;
			return MathHelper.Clamp((float)Math.Round(value / RowHeight) * RowHeight, 0f, MaxScroll);
		}

		public void ClearContent() {
			if (Children.Count > 1)
				Children.RemoveRange(1, Children.Count - 1);
			scrollPosition = 0f;
		}

		private float TrackHeight() {
			float thumb = MathHelper.Clamp(Height * Height / MathHelper.Max(ContentHeight, 1f), 20f, Height);
			return Height - thumb;
		}

		public override void Update() {
			if (!MouseLeftDown)
				draggingScrollBar = false;

			if (draggingScrollBar) {
				float track = TrackHeight();
				if (track > 0f) {
					float y = MathHelper.Clamp(Main.mouseY - dragGrabY, 0f, track);
					ScrollPosition = y / track * MaxScroll;
				}
			}
			else if (ScrollAmount != 0 && MouseInside) {
				// One row per wheel notch, like a conventional list box, rather than a raw
				// pixel amount - keeps the "always whole rows" guarantee regardless of how
				// big a single wheel tick's delta happens to be.
				ScrollPosition -= Math.Sign(ScrollAmount) * RowHeight;
			}

			float thumbHeight = MathHelper.Clamp(Height * Height / MathHelper.Max(ContentHeight, 1f), 20f, Height);
			float track2 = Height - thumbHeight;
			float scrollFraction = ContentHeight > Height ? ScrollPosition / MathHelper.Max(1f, MaxScroll) : 0f;
			scrollBar.Height = thumbHeight;
			scrollBar.Position = new Vector2(Width - scrollBar.Width - 2f, scrollFraction * track2);
			scrollBar.Visible = ContentHeight > Height;

			for (int i = 1; i < Children.Count; i++) {
				var child = Children[i];
				child.Offset = new Vector2(child.Offset.X, -ScrollPosition);
				float localTop = child.Position.Y + child.Offset.Y;
				float localBottom = localTop + child.Height;
				child.Visible = localTop >= 0f && localBottom <= Height;
			}

			base.Update();
		}

		public override void Draw(SpriteBatch spriteBatch) {
			if (!Visible)
				return;

			Vector2 topLeft = DrawPosition - Origin;
			RoundedPanel.Draw9Slice(spriteBatch, RoundedPanel.PanelTile, topLeft.X, topLeft.Y, Width, Height, BackgroundColor2);

			for (int i = 1; i < Children.Count; i++) {
				var child = Children[i];
				if (child.Visible)
					child.Draw(spriteBatch);
			}

			scrollBar.Draw(spriteBatch);
		}
	}
}
