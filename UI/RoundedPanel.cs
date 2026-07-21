using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MobLab.UI
{
	// All MobLab panel/button/icon art is generated procedurally at Load time instead of
	// shipping PNGs. This lets us keep a rounded-rectangle look (matching CheatSheet's
	// aesthetic) while using a deliberately smaller corner radius than CheatSheet/vanilla.
	internal static class RoundedPanel
	{
		private const int TileSize = 32;

		// Corner radius used by our rounded rectangles. CheatSheet reuses vanilla's
		// InventoryBack9 (radius ~14-16px on a similar tile size) - keep ours clearly smaller.
		private const float CornerRadius = 5f;

		// How much of the tile's edge is "corner" and must not be stretched when 9-slicing.
		public const int CornerSize = 9;

		public static Texture2D PanelTile { get; private set; }
		public static Texture2D CloseIcon { get; private set; }
		public static Texture2D TriangleArrow { get; private set; }
		public static Texture2D CheckMark { get; private set; }

		public static void Load(GraphicsDevice device) {
			PanelTile = BuildRoundedRect(device, TileSize, TileSize, CornerRadius);
			CloseIcon = BuildCross(device, 16);
			TriangleArrow = BuildTriangle(device, 18, 10);
			CheckMark = BuildCheckMark(device, 16);
		}

		public static void Unload() {
			PanelTile = null;
			CloseIcon = null;
			TriangleArrow = null;
			CheckMark = null;
		}

		// Draws `tile` stretched to fill (x,y,width,height) as a 9-slice, using `corner`
		// pixels from each edge of the tile as the non-stretched corner/edge pieces.
		public static void Draw9Slice(SpriteBatch spriteBatch, Texture2D tile, float x, float y, float width, float height, Color color, int corner = CornerSize) {
			int tw = tile.Width;
			int th = tile.Height;
			int cornerW = Math.Min(corner, tw / 2);
			int cornerH = Math.Min(corner, th / 2);

			int midSrcW = tw - cornerW * 2;
			int midSrcH = th - cornerH * 2;
			float midDstW = width - cornerW * 2;
			float midDstH = height - cornerH * 2;

			var srcTL = new Rectangle(0, 0, cornerW, cornerH);
			var srcTR = new Rectangle(tw - cornerW, 0, cornerW, cornerH);
			var srcBL = new Rectangle(0, th - cornerH, cornerW, cornerH);
			var srcBR = new Rectangle(tw - cornerW, th - cornerH, cornerW, cornerH);
			var srcTop = new Rectangle(cornerW, 0, midSrcW, cornerH);
			var srcBottom = new Rectangle(cornerW, th - cornerH, midSrcW, cornerH);
			var srcLeft = new Rectangle(0, cornerH, cornerW, midSrcH);
			var srcRight = new Rectangle(tw - cornerW, cornerH, cornerW, midSrcH);
			var srcCenter = new Rectangle(cornerW, cornerH, midSrcW, midSrcH);

			// Corners (fixed size).
			spriteBatch.Draw(tile, new Rectangle((int)x, (int)y, cornerW, cornerH), srcTL, color);
			spriteBatch.Draw(tile, new Rectangle((int)(x + width - cornerW), (int)y, cornerW, cornerH), srcTR, color);
			spriteBatch.Draw(tile, new Rectangle((int)x, (int)(y + height - cornerH), cornerW, cornerH), srcBL, color);
			spriteBatch.Draw(tile, new Rectangle((int)(x + width - cornerW), (int)(y + height - cornerH), cornerW, cornerH), srcBR, color);

			// Edges (stretched along one axis).
			if (midDstW > 0) {
				spriteBatch.Draw(tile, new Rectangle((int)(x + cornerW), (int)y, (int)midDstW, cornerH), srcTop, color);
				spriteBatch.Draw(tile, new Rectangle((int)(x + cornerW), (int)(y + height - cornerH), (int)midDstW, cornerH), srcBottom, color);
			}
			if (midDstH > 0) {
				spriteBatch.Draw(tile, new Rectangle((int)x, (int)(y + cornerH), cornerW, (int)midDstH), srcLeft, color);
				spriteBatch.Draw(tile, new Rectangle((int)(x + width - cornerW), (int)(y + cornerH), cornerW, (int)midDstH), srcRight, color);
			}

			// Center.
			if (midDstW > 0 && midDstH > 0) {
				spriteBatch.Draw(tile, new Rectangle((int)(x + cornerW), (int)(y + cornerH), (int)midDstW, (int)midDstH), srcCenter, color);
			}
		}

		private static Texture2D BuildRoundedRect(GraphicsDevice device, int width, int height, float radius) {
			var data = new Color[width * height];
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float coverage = RoundedRectCoverage(x, y, width, height, radius);
					data[y * width + x] = Color.White * coverage;
				}
			}
			var tex = new Texture2D(device, width, height, false, SurfaceFormat.Color);
			tex.SetData(data);
			return tex;
		}

		private static float RoundedRectCoverage(int px, int py, float width, float height, float radius) {
			const int samples = 4;
			int hits = 0;
			for (int sy = 0; sy < samples; sy++) {
				for (int sx = 0; sx < samples; sx++) {
					float x = px + (sx + 0.5f) / samples;
					float y = py + (sy + 0.5f) / samples;
					if (InsideRoundedRect(x, y, width, height, radius))
						hits++;
				}
			}
			return hits / (float)(samples * samples);
		}

		private static bool InsideRoundedRect(float x, float y, float width, float height, float radius) {
			float qx = 0f;
			float qy = 0f;
			if (x < radius)
				qx = radius - x;
			else if (x > width - radius)
				qx = x - (width - radius);
			if (y < radius)
				qy = radius - y;
			else if (y > height - radius)
				qy = y - (height - radius);

			if (qx <= 0f || qy <= 0f)
				return true;

			return qx * qx + qy * qy <= radius * radius;
		}

		private static Texture2D BuildCross(GraphicsDevice device, int size) {
			var data = new Color[size * size];
			float thickness = 1.6f;
			for (int y = 0; y < size; y++) {
				for (int x = 0; x < size; x++) {
					float d1 = DistanceToSegment(x + 0.5f, y + 0.5f, 2, 2, size - 2, size - 2);
					float d2 = DistanceToSegment(x + 0.5f, y + 0.5f, size - 2, 2, 2, size - 2);
					float d = MathHelper.Min(d1, d2);
					float alpha = MathHelper.Clamp(thickness - d + 0.5f, 0f, 1f);
					data[y * size + x] = Color.White * alpha;
				}
			}
			var tex = new Texture2D(device, size, size, false, SurfaceFormat.Color);
			tex.SetData(data);
			return tex;
		}

		private static Texture2D BuildCheckMark(GraphicsDevice device, int size) {
			var data = new Color[size * size];
			float thickness = 1.7f;
			float x0 = size * 0.15f, y0 = size * 0.55f;
			float x1 = size * 0.42f, y1 = size * 0.82f;
			float x2 = size * 0.87f, y2 = size * 0.2f;
			for (int y = 0; y < size; y++) {
				for (int x = 0; x < size; x++) {
					float px = x + 0.5f, py = y + 0.5f;
					float d = MathHelper.Min(DistanceToSegment(px, py, x0, y0, x1, y1), DistanceToSegment(px, py, x1, y1, x2, y2));
					float alpha = MathHelper.Clamp(thickness - d + 0.5f, 0f, 1f);
					data[y * size + x] = Color.White * alpha;
				}
			}
			var tex = new Texture2D(device, size, size, false, SurfaceFormat.Color);
			tex.SetData(data);
			return tex;
		}

		// Points up by default; callers can flip vertically via SpriteEffects when drawing.
		private static Texture2D BuildTriangle(GraphicsDevice device, int width, int height) {
			var data = new Color[width * height];
			Vector2 a = new Vector2(width / 2f, 0f);
			Vector2 b = new Vector2(0f, height);
			Vector2 c = new Vector2(width, height);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					float coverage = TriangleCoverage(x, y, a, b, c);
					data[y * width + x] = Color.White * coverage;
				}
			}
			var tex = new Texture2D(device, width, height, false, SurfaceFormat.Color);
			tex.SetData(data);
			return tex;
		}

		private static float TriangleCoverage(int px, int py, Vector2 a, Vector2 b, Vector2 c) {
			const int samples = 4;
			int hits = 0;
			for (int sy = 0; sy < samples; sy++) {
				for (int sx = 0; sx < samples; sx++) {
					var p = new Vector2(px + (sx + 0.5f) / samples, py + (sy + 0.5f) / samples);
					if (PointInTriangle(p, a, b, c))
						hits++;
				}
			}
			return hits / (float)(samples * samples);
		}

		private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c) {
			float d1 = Cross(p - a, b - a);
			float d2 = Cross(p - b, c - b);
			float d3 = Cross(p - c, a - c);
			bool hasNeg = d1 < 0 || d2 < 0 || d3 < 0;
			bool hasPos = d1 > 0 || d2 > 0 || d3 > 0;
			return !(hasNeg && hasPos);
		}

		private static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

		private static float DistanceToSegment(float px, float py, float ax, float ay, float bx, float by) {
			float dx = bx - ax, dy = by - ay;
			float lenSq = dx * dx + dy * dy;
			float t = lenSq > 0f ? ((px - ax) * dx + (py - ay) * dy) / lenSq : 0f;
			t = MathHelper.Clamp(t, 0f, 1f);
			float cx = ax + t * dx, cy = ay + t * dy;
			float ddx = px - cx, ddy = py - cy;
			return (float)System.Math.Sqrt(ddx * ddx + ddy * ddy);
		}
	}
}
