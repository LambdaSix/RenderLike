using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace RenderLike {
    public class RLConsole {
        internal readonly int Width;
        internal readonly int Height;
        internal Surface _rootSurface;
        private int surfaceIdx = 0;


        public GraphicsDevice Graphics { get; set; }
        public Font Font { get; set; }
        public SpriteBatch SpriteBatch { get; set; }
        public RenderTarget2D RenderTarget { get; set; }

        public int CharacterWidth { get { return Font == null ? 0 : Font.CharacterWidth; } }

        public int CharacterHeight { get { return Font == null ? 0 : Font.CharacterHeight; } }

        public RLConsole(GraphicsDevice device, Font font, int width, int height) {
            if (device == null)
                throw new ArgumentNullException("device");
            if (font == null)
                throw new ArgumentNullException("font");
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height");

            Graphics = device;
            Font = font;
            SpriteBatch = new SpriteBatch(device);

            Width = width;
            Height = height;

            RenderTarget = AllocateTarget(font);
            _rootSurface = new Surface(width, height, Font, this, surfaceIdx);
        }

        public void ChangeFont(Font font) {
            if (font == null)
                throw new ArgumentNullException("font");
            if (font == Font)
                return;

            Font = font;
            RenderTarget = AllocateTarget(font);
        }

        public RenderTarget2D Flush() {
            return RenderSurfaceToTarget(_rootSurface, RenderTarget);
        }

        public Surface CreateSurface(int width, int height) {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height");

            return new Surface(width, height, Font, this, ++surfaceIdx);
        }

        /// <summary>
        /// Draw from a Surface into a RenderTarget2D
        /// </summary>
        /// <param name="src">Surface to take cells from</param>
        /// <param name="target">Target to draw into</param>
        /// <returns>The modified RenderTarget2D</returns>
        internal RenderTarget2D RenderSurfaceToTarget(Surface src, RenderTarget2D target) {
            return src.RenderSurface(target);
        }

        /// <summary>
        /// Blit from a Surface to the console's surface for final rendering
        /// </summary>
        /// <param name="src">Source surface to blit from</param>
        /// <param name="srcRect">Bounding rectangle to blit from src</param>
        /// <param name="destX">Destination X (top-left)</param>
        /// <param name="destY">Destination Y (top-left)</param>
        public void Blit(Surface src, Rectangle srcRect, int destX, int destY) {
            Blit(src, _rootSurface, srcRect, destX, destY);
        }

        public void Blit(Surface src, Surface dst, int destX, int destY) {
            if (src == null)
                throw new ArgumentNullException("src");
            if (dst == null)
                throw new ArgumentNullException("dst");

            Blit(src, dst, new Rectangle(0, 0, src.Width, src.Height), destX, destY);
        }

        public void Blit(Surface src, Surface dst, Rectangle srcRect, int destX, int destY) {
            Debug.WriteLine($"Blitting from Surface-{src.Index} into Surface-{dst.Index}");
            if (src == null)
                throw new ArgumentNullException("src");
            if (dst == null)
                throw new ArgumentNullException("dst");

            var blitRect = new Rectangle(destX, destY, srcRect.Width, srcRect.Height);
            int deltaX = srcRect.Left - blitRect.Left;
            int deltaY = srcRect.Top - blitRect.Top;

            blitRect = Rectangle.Intersect(blitRect, new Rectangle(0, 0, dst.Width, dst.Height));

            for (int y = blitRect.Top; y < blitRect.Bottom; y++) {
                for (int x = blitRect.Left; x < blitRect.Right; x++) {
                    int sx = deltaX + x;
                    int sy = deltaY + y;

                    dst.Cells[x + y*dst.Width].Back = src.Cells[sx + sy*src.Width].Back;
                    dst.Cells[x + y*dst.Width].Fore = src.Cells[sx + sy*src.Width].Fore;
                    dst.Cells[x + y*dst.Width].Char = src.Cells[sx + sy*src.Width].Char;

                    dst.DirtyCells[x + y*dst.Width] = true;
                }
            }
        }

        public void BlitAlpha(Surface src, Rectangle srcRect, int destX, int destY, float fgAlpha, float bgAlpha) {
            BlitAlpha(src, _rootSurface, srcRect, destX, destY, fgAlpha, bgAlpha);
        }

        public void BlitAlpha(Surface src, Surface dst, Rectangle srcRect, int destX, int destY, float fgAlpha,
            float bgAlpha) {
            Debug.WriteLine($"Blitting from Surface-{src.Index} into Surface-{dst.Index}");
            if (src == null)
                throw new ArgumentNullException("src");
            if (dst == null)
                throw new ArgumentNullException("dst");

            fgAlpha = MathHelper.Clamp(fgAlpha, 0f, 1.0f);
            bgAlpha = MathHelper.Clamp(bgAlpha, 0f, 1.0f);

            var blitRect = new Rectangle(destX, destY, srcRect.Width, srcRect.Height);
            int deltaX = srcRect.Left - blitRect.Left;
            int deltaY = srcRect.Top - blitRect.Top;

            blitRect = Rectangle.Intersect(blitRect, new Rectangle(0, 0, dst.Width, dst.Height));
            Color backCol, foreCol;
            char ch;

            for (int y = blitRect.Top; y < blitRect.Bottom; y++) {
                for (int x = blitRect.Left; x < blitRect.Right; x++) {
                    int sx = deltaX + x;
                    int sy = deltaY + y;

                    backCol = dst.Cells[x + y*dst.Width].Back;
                    backCol.A = (byte) (bgAlpha*255.0f + 0.5f);

                    if (src.Cells[sx + sy*src.Width].Char == ' ') {
                        foreCol = dst.Cells[x + y*dst.Width].Fore;
                        foreCol.A = (byte) (fgAlpha*255.0f + 0.5f);
                        ch = dst.Cells[x + y*dst.Width].Char;
                    }
                    else {
                        foreCol = src.Cells[sx + sy*src.Width].Fore;
                        ch = src.Cells[sx + sy*src.Width].Char;
                    }

                    dst.Cells[x + y*dst.Width].Back = backCol;
                    dst.Cells[x + y*dst.Width].Fore = foreCol;
                    dst.Cells[x + y*dst.Width].Char = ch;

                    dst.DirtyCells[x + y*dst.Width] = true;
                }
            }
        }

        public void BlitAlpha(Surface src, Rectangle srcRect, int destX, int destY, float alpha) {
            BlitAlpha(src, _rootSurface, srcRect, destX, destY, alpha);
        }

        public void BlitAlpha(Surface src, Surface dst, Rectangle srcRect, int destX, int destY, float alpha) {
            Debug.WriteLine($"Blitting from Surface-{src.Index} into Surface-{dst.Index}");

            if (src == null)
                throw new ArgumentNullException("src");
            if (dst == null)
                throw new ArgumentNullException("dst");

            alpha = MathHelper.Clamp(alpha, 0f, 1f);

            var blitRect = new Rectangle(destX, destY, srcRect.Width, srcRect.Height);
            int deltaX = srcRect.Left - blitRect.Left;
            int deltaY = srcRect.Top - blitRect.Top;

            blitRect = Rectangle.Intersect(blitRect, new Rectangle(0, 0, dst.Width, dst.Height));
            Color backCol, foreCol;
            char ch;

            for (int y = blitRect.Top; y < blitRect.Bottom; y++) {
                for (int x = blitRect.Left; x < blitRect.Right; x++) {
                    int sx = deltaX + x;
                    int sy = deltaY + y;

                    backCol = Color.Lerp(dst.Cells[x + y*dst.Width].Back, src.Cells[sx + sy*src.Width].Back, alpha);

                    if (src.Cells[sx + sy*src.Width].Char == ' ') {
                        foreCol = Color.Lerp(dst.Cells[x + y*dst.Width].Fore, src.Cells[sx + sy*src.Width].Back, alpha);
                        ch = dst.Cells[x + y*dst.Width].Char;
                    }
                    else {
                        foreCol = src.Cells[sx + sy*src.Width].Fore;
                        ch = src.Cells[sx + sy*src.Width].Char;
                    }

                    dst.Cells[x + y*dst.Width].Back = backCol;
                    dst.Cells[x + y*dst.Width].Fore = foreCol;
                    dst.Cells[x + y*dst.Width].Char = ch;

                    dst.DirtyCells[x + y*dst.Width] = true;
                }
            }
        }

        public void Clear() {
            RootClear();
        }

        internal void RootClear() {
            Graphics.SetRenderTarget(RenderTarget);
            Graphics.Clear(_rootSurface.DefaultBackground);
            Graphics.SetRenderTarget(null);
        }

        /// <summary>
        /// Allocate a new RenderTarget2D
        /// </summary>
        /// <param name="font">Font object to use</param>
        /// <returns></returns>
        internal RenderTarget2D AllocateTarget(Font font) {
            return new RenderTarget2D(Graphics,
                font.CharacterWidth*Width,
                font.CharacterHeight*Height,
                false, // Mipmap
                SurfaceFormat.Color,
                DepthFormat.None,
                1, // PreferredMultiSampleCount
                RenderTargetUsage.PreserveContents
                );
        }
    }
}