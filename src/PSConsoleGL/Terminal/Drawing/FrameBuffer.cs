using System;
using System.Collections.Generic;
using PSConsoleGL.Terminal;

namespace PSConsoleGL.Terminal.Drawing {
   
    public class FrameBuffer {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Int32[] Buffer { get; private set; }

        public FrameBuffer(int width, int height) {
            Width = width;
            Height = height;
            Buffer = new Int32[width * height];
        }

        public FrameBuffer() {
            // FrameBuffer uses Sixel terminal dimensions
            // This is a workaround for the fact that Console.WindowWidth and Console.WindowHeight
            XTermCellSize cellSize = new XTermCellSize();

            this.Width = Console.WindowWidth * cellSize.Width;
            this.Height = (Console.WindowHeight-1) * cellSize.Height;
            this.Buffer = new Int32[Width * Height];
        }

        public void Clear(Int32 color) {
            for (int i = 0; i < Width * Height; i++) {
                Buffer[i] = color;
            }
        }

        public void Clear() {
            Clear(0);
        }

        public void Clear(int a, int r, int g, int b) {
            Int32 color = (a << 24) | (r << 16) | (g << 8) | b;
            Clear(color);
        }

        public void Clear(int r, int g, int b) {
            Clear(255, r, g, b);
        }

        public void Clear(Color color) {
            Clear(color.argb);
        }

        public Int32 GetRawPixel(Int32 position) {
            if (position < 0 || position >= Buffer.Length) {
                throw new ArgumentOutOfRangeException("Index is out of bounds.");
            }
            return Buffer[position];
        }

        public Int32 GetRawPixel(int x, int y) {
            if (x < 0 || x >= Width || y < 0 || y >= Height) {
                throw new ArgumentOutOfRangeException("Coordinates are out of bounds.");
            }
            return Buffer[y * Width + x];
        }

        public Color GetPixel(Int32 position) {
            return new Color(GetRawPixel(position));
        }

        public Color GetPixel(int x, int y) {
            return new Color(GetRawPixel(x, y));
        }

        public void SetPixel(Int32 position, Int32 argb) {
            if (position < 0 || position >= Buffer.Length) {
                throw new ArgumentOutOfRangeException("Index is out of bounds.");
            }
            Buffer[position] = argb;
        }

        public void SetPixel(int x, int y, Int32 argb) {
            if (x < 0 || x >= Width || y < 0 || y >= Height) {
                throw new ArgumentOutOfRangeException("Coordinates are out of bounds.");
            }
            SetPixel(x + y * Width, argb);
        }

        public void SetPixel(Int32 position, int a, int r, int g, int b) {
            Int32 color = (a << 24) | (r << 16) | (g << 8) | b;
            SetPixel(position, color);
        }

        public void SetPixel(int x, int y, int a, int r, int g, int b) {
            Int32 color = (a << 24) | (r << 16) | (g << 8) | b;
            SetPixel(x, y, color);
        }

        // public void SetPixel(Int32 position, int r, int g, int b) {
        //     SetPixel(position, 255, r, g, b);
        // }

        // public void SetPixel(int x, int y, int r, int g, int b) {
        //     SetPixel(x, y, 255, r, g, b);
        // }

        public void SetPixel(Int32 position, Color color) {
            SetPixel(position, color.argb);
        }
        
        public void SetPixel(int x, int y, Color color) {
            SetPixel(x, y, color.argb);
        }

        public void DrawToScreen() {
            var termCapability = Terminal.XTermCapability();

            if (Array.IndexOf(termCapability, TerminalCapability.Sixel) != -1) {
                // If the terminal supports Sixel graphics, we can draw the framebuffer
                Render.DrawSixelToScreen(this);
            } else {
                // Fallback to other drawing methods if Sixel is not supported
                Console.WriteLine("Sixel graphics not supported by this terminal.");
            }
        }


    }
}