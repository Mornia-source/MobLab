using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;

namespace MobLab.UI
{
	// Minimal retained-mode UI node, inspired by CheatSheet's UIView approach but
	// rewritten from scratch for MobLab: a tree of positioned/anchored nodes with
	// bubbling mouse events, driven manually from a ModSystem each frame.
	internal class UIView
	{
		public delegate void ClickHandler(object sender, byte button);

		// Shared mouse state for the whole tree, refreshed once per frame by the root.
		protected static bool MouseLeftDown;
		protected static bool MouseLeftDownPrevious;
		protected static bool MouseRightDown;
		protected static bool MouseRightDownPrevious;
		public static string HoverText = "";
		public static int ScrollAmount;

		private static bool mouseClaimedThisFrame;

		private Vector2 position;
		private float width;
		private float height;
		private bool mouseWasInside;
		private string tooltip = "";

		public List<UIView> Children { get; } = new List<UIView>();
		private readonly List<UIView> childrenToRemove = new List<UIView>();

		public UIView Parent { get; set; }

		public event EventHandler OnHover;
		public event EventHandler OnLeftClick;
		public event EventHandler OnRightClick;
		public event EventHandler OnMouseEnter;
		public event EventHandler OnMouseLeave;
		public event ClickHandler OnMouseDown;
		public event ClickHandler OnMouseUp;

		public Vector2 Position { get => position; set => position = value; }
		public float X { get => position.X; set => position.X = value; }
		public float Y { get => position.Y; set => position.Y = value; }

		public virtual float Width { get => width; set => width = value; }
		public virtual float Height { get => height; set => height = value; }

		public AnchorPosition Anchor { get; set; } = AnchorPosition.TopLeft;
		public Vector2 Offset { get; set; } = Vector2.Zero;
		public Color ForegroundColor { get; set; } = Color.White;
		public Color BackgroundColor { get; set; } = Color.White;
		public float Opacity { get; set; } = 1f;
		public bool Visible { get; set; } = true;
		public bool BlocksWorldClicks { get; set; } = true;
		public object Tag { get; set; }

		public string Tooltip {
			get => tooltip;
			set {
				if (value.Length > 0 && tooltip.Length == 0)
					OnHover += ShowTooltip;
				else if (value.Length == 0 && tooltip.Length > 0)
					OnHover -= ShowTooltip;
				tooltip = value;
			}
		}

		public Vector2 Origin {
			get {
				float halfW = Width / 2f;
				float halfH = Height / 2f;
				return Anchor switch {
					AnchorPosition.Top => new Vector2(halfW, 0f),
					AnchorPosition.Bottom => new Vector2(halfW, Height),
					AnchorPosition.Left => new Vector2(0f, halfH),
					AnchorPosition.Right => new Vector2(Width, halfH),
					AnchorPosition.Center => new Vector2(halfW, halfH),
					AnchorPosition.TopRight => new Vector2(Width, 0f),
					AnchorPosition.BottomLeft => new Vector2(0f, Height),
					AnchorPosition.BottomRight => new Vector2(Width, Height),
					_ => Vector2.Zero,
				};
			}
		}

		public Vector2 DrawPosition {
			get {
				if (Parent != null)
					return Parent.DrawPosition - Parent.Origin + Position + Offset;
				return Position + Offset;
			}
		}

		public bool MouseInside => IsMouseInside();

		public void AddChild(UIView child) {
			child.Parent = this;
			Children.Add(child);
		}

		public void RemoveChild(UIView child) => childrenToRemove.Add(child);

		public void RemoveAllChildren() => Children.Clear();

		protected virtual bool IsMouseInside() {
			if (Main.mouseX < 0 || Main.mouseY < 0 || Main.mouseX > Main.screenWidth || Main.mouseY > Main.screenHeight)
				return false;

			Vector2 topLeft = DrawPosition - Origin;
			return Main.mouseX >= topLeft.X && Main.mouseX <= topLeft.X + Width &&
				Main.mouseY >= topLeft.Y && Main.mouseY <= topLeft.Y + Height;
		}

		private void ShowTooltip(object sender, EventArgs e) => HoverText = ((UIView)sender).Tooltip;

		// Called once per frame, from inside the UI-scale interface layer only (see
		// MobLabSystem.ModifyInterfaceLayers/DrawUiLayer for why: Main.mouseX/Y and
		// Main.screenWidth/Height are only in UI-scaled space while inside that layer).
		public void UpdateRoot() {
			// Raw device state rather than Main.mouseLeft/mouseRight, matching CheatSheet's
			// UIView (which also uses Mouse.GetState()).
			var mouseState = Mouse.GetState();
			MouseLeftDownPrevious = MouseLeftDown;
			MouseLeftDown = mouseState.LeftButton == ButtonState.Pressed;
			MouseRightDownPrevious = MouseRightDown;
			MouseRightDown = mouseState.RightButton == ButtonState.Pressed;
			mouseClaimedThisFrame = false;
			HoverText = "";
			ScrollAmount = PlayerInput.ScrollWheelDeltaForUI;

			Update();
			HandleMouseInput();
		}

		public virtual void Update() {
			for (int i = Children.Count - 1; i >= 0; i--)
				Children[i].Update();

			if (childrenToRemove.Count > 0) {
				foreach (var child in childrenToRemove)
					Children.Remove(child);
				childrenToRemove.Clear();
			}
		}

		// Depth-first, front-to-back (last child drawn on top handles input first).
		private void HandleMouseInput() {
			if (!Visible)
				return;

			for (int i = Children.Count - 1; i >= 0; i--)
				Children[i].HandleMouseInput();

			bool inside = MouseInside;
			if (inside && !mouseClaimedThisFrame) {
				if (BlocksWorldClicks)
					mouseClaimedThisFrame = true;

				OnHover?.Invoke(this, EventArgs.Empty);

				if (!mouseWasInside)
					OnMouseEnter?.Invoke(this, EventArgs.Empty);

				if (!MouseLeftDownPrevious && MouseLeftDown)
					OnMouseDown?.Invoke(this, 0);

				if (MouseLeftDownPrevious && !MouseLeftDown) {
					OnMouseUp?.Invoke(this, 0);
					OnLeftClick?.Invoke(this, EventArgs.Empty);
				}

				if (!MouseRightDownPrevious && MouseRightDown)
					OnMouseDown?.Invoke(this, 1);

				if (MouseRightDownPrevious && !MouseRightDown) {
					OnMouseUp?.Invoke(this, 1);
					OnRightClick?.Invoke(this, EventArgs.Empty);
				}
			}
			else if (mouseWasInside) {
				OnMouseLeave?.Invoke(this, EventArgs.Empty);
			}

			mouseWasInside = inside;
		}

		public virtual void Draw(SpriteBatch spriteBatch) {
			if (!Visible)
				return;

			foreach (var child in Children)
				child.Draw(spriteBatch);
		}

		// Was the mouse claimed by any MobLab UI element this frame? Used to gate world-click placement.
		public static bool MouseClaimedByUI => mouseClaimedThisFrame;
	}
}
