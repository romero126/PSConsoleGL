using System;

namespace PSConsoleGL.Terminal.Drawing
{
    public class Color {
        public int A;
        public int R;
        public int G;
        public int B;

        public int argb { get { return ToArgb(); } }

        public Color(int r, int g, int b)
        {
            A = 255;
            R = Math.Max(0, Math.Min(255, r));
            G = Math.Max(0, Math.Min(255, g));
            B = Math.Max(0, Math.Min(255, b));
        }

        public Color(int a, int r, int g, int b)
        {
            A = Math.Max(0, Math.Min(255, a));
            R = Math.Max(0, Math.Min(255, r));
            G = Math.Max(0, Math.Min(255, g));
            B = Math.Max(0, Math.Min(255, b));
        }

        public Color(int argb)
        {
            A = (argb >> 24) & 0xFF;
            R = (argb >> 16) & 0xFF;
            G = (argb >> 8) & 0xFF;
            B = argb & 0xFF;
        }

        public int ToArgb()
        {
            return (A << 24) | (R << 16) | (G << 8) | B;
        }

        public static Int32 ToArgb(int a, int r, int g, int b)
        {
            return (a << 24) | (r << 16) | (g << 8) | b;
        }

        public static Int32 ToArgb(int r, int g, int b)
        {
            return ToArgb(255, r, g, b);
        }

        public static Color FromArgb(int r, int g, int b)
        {
            return new Color(255, r, g, b);
        }

        public static Color FromArgb(int a, int r, int g, int b)
        {
            return new Color(a, r, g, b);
        }

        public static Color FromArgb(int argb)
        {
            return new Color(argb);
        }

    }
}