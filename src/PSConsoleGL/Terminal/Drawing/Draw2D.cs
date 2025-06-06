using System;
using PSConsoleGL.Terminal;
using PSConsoleGL.Terminal.Drawing;

namespace PSConsoleGL.Terminal.Drawing
{
    public class Point {
        public int X;
        public int Y;

        public Point(int x, int y) {
            X = x;
            Y = y;
        }

        // Returns a percentage of the point in the framebuffer
        public Point (FrameBuffer framebuffer, float x, float y) {
            if (x < 0 || x > 1 || y < 0 || y > 1) {
                throw new ArgumentOutOfRangeException("x and y must be between 0 and 1.");
            }
            if (framebuffer == null) {
                throw new ArgumentNullException(nameof(framebuffer), "FrameBuffer cannot be null.");
            }
            X = (int)(x * framebuffer.Width);
            Y = (int)(y * framebuffer.Height);
        }
    }

    public class Draw2D {
        public void Clear(FrameBuffer framebuffer, Int32 value) {
            framebuffer.Clear(value);
        }

        public void Clear(FrameBuffer framebuffer, int a, int r, int g, int b) {
            framebuffer.Clear(a, r, g, b);
        }

        public void Clear(FrameBuffer framebuffer, int r, int g, int b) {
            framebuffer.Clear(r, g, b);
        }

        public void Clear(FrameBuffer framebuffer, Color color) {
            framebuffer.Clear(color);
        }

        public void Clear(FrameBuffer framebuffer) {
            framebuffer.Clear();
        }

        public static Color GetPixel(FrameBuffer framebuffer, Int32 i) {
            return framebuffer.GetPixel(i);
        }

        public static Color GetPixel(FrameBuffer framebuffer, int x, int y) {
            return framebuffer.GetPixel(x, y);
        }

        public static void SetPixel(FrameBuffer framebuffer, Int32 i, Int32 argb) {
            framebuffer.SetPixel(i, argb);
        }

        public static void SetPixel(FrameBuffer framebuffer, int x, int y, Int32 argb) {
            framebuffer.SetPixel(x, y, argb);
        }

        public static void SetPixel(FrameBuffer framebuffer, int x, int y, int a, int r, int g, int b) {
            framebuffer.SetPixel(x, y, a, r, g, b);
        }

        // public static void SetPixel(FrameBuffer framebuffer, Int32 i, int r, int g, int b) {
        //     framebuffer.SetPixel(i, r, g, b);
        // }

        // public static void SetPixel(FrameBuffer framebuffer, int x, int y, int r, int g, int b) {
        //     framebuffer.SetPixel(x, y, r, g, b);
        // }

        public static void SetPixel(FrameBuffer framebuffer, Int32 i, Color color) {
            framebuffer.SetPixel(i, color.argb);
        }

        public static void SetPixel(FrameBuffer framebuffer, int x, int y, Color color) {
            framebuffer.SetPixel(x, y, color);
        }

        public static void DrawLine(FrameBuffer framebuffer, int x1, int y1, int x2, int y2, Int32 argb) {
            int dx = x2 - x1;
            int dy = y2 - y1;
            int x = x1;
            int y = y1;
            int x_inc, y_inc;
            if (dx < 0) {
                dx = -dx;
                x_inc = -1;
            } else {
                x_inc = 1;
            }
            if (dy < 0) {
                dy = -dy;
                y_inc = -1;
            } else {
                y_inc = 1;
            }
            int dx2 = dx << 1;
            int dy2 = dy << 1;
            if (dx > dy) {
                int error = dy2 - dx;
                for (int i = 0; i <= dx; i++) {
                    if (x >= 0 && x < framebuffer.Width && y >= 0 && y < framebuffer.Height)
                        framebuffer.SetPixel(x, y, argb);

                    if (error > 0) {
                        error -= dx2;
                        y += y_inc;
                    }
                    error += dy2;
                    x += x_inc;
                }
            } else {
                int error = dx2 - dy;
                for (int i = 0; i <= dy; i++) {
                    if (x >= 0 && x < framebuffer.Width && y >= 0 && y < framebuffer.Height)
                        framebuffer.SetPixel(x, y, argb);

                    if (error > 0) {
                        error -= dy2;
                        x += x_inc;
                    }
                    error += dx2;
                    y += y_inc;
                }
            }
        }

        public static void DrawLine(FrameBuffer framebuffer,  int x1, int y1, int x2, int y2, Color color) {
            DrawLine(framebuffer, x1, y1, x2, y2, color.argb);
        }

        public static bool PointInTriangle(int x, int y, int x1, int y1, int x2, int y2, int x3, int y3) {
            int o1 = (x - x1) * (y2 - y1) - (y - y1) * (x2 - x1);
            int o2 = (x - x2) * (y3 - y2) - (y - y2) * (x3 - x2);
            int o3 = (x - x3) * (y1 - y3) - (y - y3) * (x1 - x3);
            return (o1 > 0 && o2 > 0 && o3 > 0) || (o1 < 0 && o2 < 0 && o3 < 0);
        }

        public static void DrawTriangle(FrameBuffer framebuffer, int x1, int y1, int x2, int y2, int x3, int y3, Int32 argb, bool fill = false) {
            DrawLine(framebuffer, x1, y1, x2, y2, argb);
            DrawLine(framebuffer, x2, y2, x3, y3, argb);
            DrawLine(framebuffer, x3, y3, x1, y1, argb);

            if (fill) {
                int xMin = Math.Min(x1, Math.Min(x2, x3));
                int xMax = Math.Max(x1, Math.Max(x2, x3));
                int yMin = Math.Min(y1, Math.Min(y2, y3));
                int yMax = Math.Max(y1, Math.Max(y2, y3));
                for (int x = xMin; x < xMax; x++) {
                    for (int y = yMin; y < yMax; y++) {
                        if (x >= 0 && x < framebuffer.Width && y >= 0 && y < framebuffer.Height) {
                            if (PointInTriangle(x, y, x1, y1, x2, y2, x3, y3)) {
                                framebuffer.SetPixel(x, y, argb);
                            }
                        }
                    }
                }
            }
        }

        public static void DrawTriangle(FrameBuffer framebuffer, int x1, int y1, int x2, int y2, int x3, int y3, Color color, bool fill = false) {
            DrawTriangle(framebuffer, x1, y1, x2, y2, x3, y3, color.argb, fill);
        }

        public static bool PointInPolygon(int x, int y, Point[] points) {
            int intersections = 0;
            for (int i = 0, j = points.Length - 1; i < points.Length; j = i++) {
                if ((points[i].Y > y) != (points[j].Y > y) && x < (points[j].X - points[i].X) * (y - points[i].Y) / (points[j].Y - points[i].Y) + points[i].X) {
                    intersections++;
                }
            }
            return intersections % 2 == 1;
        }

        public static void DrawPolygon(FrameBuffer framebuffer, Point[] points, Int32 argb, bool fill = false)
        {
            for (int i = 0; i < points.Length - 1; i++) {
                DrawLine(framebuffer, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y, argb);
            }
            if (fill) {
                int minY = points[0].Y;
                int maxY = points[0].Y;
                for (int i = 1; i < points.Length; i++) {
                    if (points[i].Y < minY) {
                        minY = points[i].Y;
                    }
                    if (points[i].Y > maxY) {
                        maxY = points[i].Y;
                    }
                }
                for (int y = minY; y < maxY; y++) {
                    for (int x = 0; x < framebuffer.Width; x++) {
                        if (PointInPolygon(x, y, points)) {
                            framebuffer.SetPixel(x, y, argb);
                        }
                    }
                }
            }
        }

        public static void DrawPolygon(FrameBuffer framebuffer, Point[] points, Color color, bool fill = false) {
            DrawPolygon(framebuffer, points, color.argb, fill);
        }

        public static void DrawArc(FrameBuffer framebuffer, int x, int y, int radius, int startAngle, int endAngle, Int32 argb, bool fill = false)
        {
            if (startAngle > endAngle) {
                int temp = startAngle;
                startAngle = endAngle;
                endAngle = temp;
            }

            Point[] points = new Point[endAngle - startAngle + 1];

            if (fill) {
                points[0] = new Point(x, y);
            }
            for (int i = startAngle; i < endAngle; i++) {
                int x1 = (int)(x + radius * Math.Cos(i * Math.PI / 180));
                int y1 = (int)(y + radius * Math.Sin(i * Math.PI / 180));
                framebuffer.SetPixel(x1, y1, argb);
                if (fill) {
                    points[i - startAngle + 1] = new Point(x1, y1);
                }
            }
            
            if (fill) {
                DrawPolygon(framebuffer, points, argb, true);
            }
        }

        public static void DrawArc(FrameBuffer framebuffer, int x, int y, int radius, int startAngle, int endAngle, Color color, bool fill = false) {
            DrawArc(framebuffer, x, y, radius, startAngle, endAngle, color.argb, fill);
        }

        public static bool PointInRect(int x, int y, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
        {
            Point[] points = new Point[4];
            points[0] = new Point(x1, y1);
            points[1] = new Point(x2, y2);
            points[2] = new Point(x3, y3);
            points[3] = new Point(x4, y4);

            return PointInPolygon(x, y, points);
        }

        public static void DrawRect(FrameBuffer framebuffer, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, Int32 argb, bool fill = false)
        {
            DrawLine(framebuffer, x1, y1, x2, y2, argb);
            DrawLine(framebuffer, x2, y2, x3, y3, argb);
            DrawLine(framebuffer, x3, y3, x4, y4, argb);
            DrawLine(framebuffer, x4, y4, x1, y1, argb);

            if (fill) {
                int xMin = Math.Min(x1, Math.Min(x2, Math.Min(x3, x4)));
                int xMax = Math.Max(x1, Math.Max(x2, Math.Max(x3, x4)));
                int yMin = Math.Min(y1, Math.Min(y2, Math.Min(y3, y4)));
                int yMax = Math.Max(y1, Math.Max(y2, Math.Max(y3, y4)));
                for (int x = xMin; x < xMax; x++) {
                    for (int y = yMin; y < yMax; y++) {
                        if (x >= 0 && x < framebuffer.Width && y >= 0 && y < framebuffer.Height) {
                            if (PointInRect(x, y, x1, y1, x2, y2, x3, y3, x4, y4)) {
                                framebuffer.SetPixel(x, y, argb);
                            }
                        }
                    }
                }
            }
        }
        
        public static void DrawRect(FrameBuffer framebuffer, int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, Color color, bool fill = false) {
            DrawRect(framebuffer, x1, y1, x2, y2, x3, y3, x4, y4, color.argb, fill);
        }

        public static void DrawRect(FrameBuffer framebuffer, int x, int y, int w, int h, Int32 argb, bool fill = false) {
            DrawRect(framebuffer, x, y, x + w, y, x + w, y + h, x, y + h, argb, fill);
        }

        public static void DrawRect(FrameBuffer framebuffer, int x, int y, int w, int h, Color color, bool fill = false) {
            DrawRect(framebuffer, x, y, x + w, y, x + w, y + h, x, y + h, color.argb, fill);
        }

        public static void DrawRoundedRect(FrameBuffer framebuffer, int x, int y, int w, int h, int r, Int32 argb, bool fill = false)
        {
            // Todo: 
            // Change this to use polygon points instead of lines and arcs for better performance and accuracy
            if (r < 0) {
                throw new ArgumentException("Radius cannot be negative.");
            }

            /*
                var points = new Point[];
                points[0] = new Point(x + r, y); // Top Left
                points[1] = new Point(x + w - r, y); // Top Right
                points[2] = new Point(x + w, y + r); // Right Top
                points[3] = new Point(x + w, y + h - r); // Right Bottom


            */

            // Draw the outer parts of the rectangle with cutouts for the rounded corners
            DrawLine(framebuffer, x + r, y, x + w - r, y, argb);
            DrawLine(framebuffer, x + w, y + r, x + w, y + h - r, argb);
            DrawLine(framebuffer, x + w - r, y + h, x + r, y + h, argb);
            DrawLine(framebuffer, x, y + h - r, x, y + r, argb);

            // This part auto fills the rounded rectangle
            DrawArc(framebuffer, x + r, y + r, r, 180, 270, argb, fill); // Top Left
            DrawArc(framebuffer, x + w - r, y + r, r, 270, 360, argb, fill); // Top Right (May need to shift down a pixel?)
            DrawArc(framebuffer, x + r, y + h - r, r, 90, 180 , argb, fill); // Bottom Left
            DrawArc(framebuffer, x + w - r, y + h - r, r, 0, 90, argb, fill); // Bottom Right

            if (fill) {
                DrawRect(framebuffer, x + r, y, w - r * 2, r, argb, fill); // Top
                DrawRect(framebuffer, x + w - r, y + r, r, h - r * 2, argb, fill); // Right
                DrawRect(framebuffer, x + r, y + h - r, w - r * 2, r, argb, fill); // Bottom
                DrawRect(framebuffer, x, y + r, r, h - r * 2, argb, fill); // Left
                DrawRect(framebuffer, x + r, y + r, w - r * 2, h - r * 2, argb, fill); // Center
            }
        }

        public static void DrawRoundedRect(FrameBuffer framebuffer, int x, int y, int w, int h, int r, Color color, bool fill = false) {
            DrawRoundedRect(framebuffer, x, y, w, h, r, color.argb, fill);
        }

        public static bool PointInEllipse(int x, int y, int cx, int cy, int rx, int ry)
        {
            return (Math.Pow(x - cx, 2) / Math.Pow(rx, 2) + Math.Pow(y - cy, 2) / Math.Pow(ry, 2)) <= 1;
        }

        public static void DrawEllipse(FrameBuffer framebuffer, int cx, int cy, int rw, int rh, Int32 argb, bool fill = false) {
            // Draw an ellipse with center cx, cy, radius rw, rh, and color argb
            // If fill is true, fill the ellipse with the color argb, otherwise draw the outline
            // This is a modified version of the Bresenham's ellipse algorithm
            // https://en.wikipedia.org/wiki/Ellipse#Bresenham's_algorithm

            int x = 0;
            int y = rh;
            int width = 1;
            long a2 = (long)rw * rw;
            long b2 = (long)rh * rh;
            long crit1 = -(a2 / 4 + rw % 2 + b2);
            long crit2 = -(b2 / 4 + rh % 2 + a2);
            long crit3 = -(b2 / 4 + rh % 2);
            long t = -a2 * y;
            long dxt = 2 * b2 * x;
            long dyt = -2 * a2 * y;
            long d2xt = 2 * b2;
            long d2yt = 2 * a2;

            while (y >= 0 && x <= rw) {
                if (x != 0 || y != 0) {
                    framebuffer.SetPixel(cx + x, cy + y, argb);

                    if (x != 0 || y != 0) {
                        framebuffer.SetPixel(cx - x, cy - y, argb);
                    }
                    if (x != 0 && y != 0) {
                        framebuffer.SetPixel(cx + x, cy - y, argb);
                        framebuffer.SetPixel(cx - x, cy + y, argb);
                    }

                    if (t + b2*x <= crit1 || t + a2*y <= crit3) {
                        x++; dxt += d2xt; t += dxt;
                        width += 2;
                    }
                    else if (t - a2*y > crit2) {
                        if (fill) {
                            DrawLine(framebuffer, cx - x, cy - y, cx - x + width, cy - y, argb);
                            if (y != 0) {
                                DrawLine(framebuffer, cx - x, cy + y, cx - x + width, cy + y, argb);
                            }
                        }
                        y--; dyt += d2yt; t += dyt;
                    }
                    else {
                        if (fill) {
                            DrawLine(framebuffer, cx - x, cy - y, cx - x + width, cy - y, argb);
                            if (y != 0) {
                                DrawLine(framebuffer, cx - x, cy + y, cx - x + width, cy + y, argb);
                            }
                        }
                        x++; dxt += d2xt; t += dxt;
                        y--; dyt += d2yt; t += dyt;
                        width += 2;
                    }
                    if (fill && rw == 0) {
                        DrawLine(framebuffer, cx - x, cy, cx + x + (2*rw+1), cy, argb);
                    }
                }
            }
        }
        public static void DrawEllipse(FrameBuffer framebuffer, int cx, int cy, int rw, int rh, Color color, bool fill = false) {
            DrawEllipse(framebuffer, cx, cy, rw, rh, color.argb, fill);
        }

        public static bool PointInCircle(int x, int y, int cx, int cy, int r) {
            return (x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r;
        }

        public static void DrawCircle(FrameBuffer framebuffer, int x, int y, int r, Int32 argb, bool fill = false) {
            int x1 = 0;
            int y1 = r;
            int p = 3 - 2 * r;
            if (r == 0) return;
           
            // only formulate 1/8 of circle
            while (y1 >= x1) {
                if (fill) {
                    // We draw a line filling the circle between the symmetric points
                    DrawLine(framebuffer, x - x1, y - y1, x + x1, y - y1, argb);
                    DrawLine(framebuffer, x - y1, y - x1, x + y1, y - x1, argb);
                    DrawLine(framebuffer, x - x1, y + y1, x + x1, y + y1, argb);
                    DrawLine(framebuffer, x - y1, y + x1, x + y1, y + x1, argb);
                } else {
                    // We draw the 8 symmetric points of the circle
                    framebuffer.SetPixel(x + x1, y - y1, argb); // upper right right
                    framebuffer.SetPixel(x + y1, y - x1, argb); // upper upper right
                    framebuffer.SetPixel(x + y1, y + x1, argb); // lower lower right
                    framebuffer.SetPixel(x + x1, y + y1, argb); // lower right right
                    framebuffer.SetPixel(x - x1, y - y1, argb); // upper left left
                    framebuffer.SetPixel(x - y1, y - x1, argb); // upper upper left
                    framebuffer.SetPixel(x - y1, y + x1, argb); // lower lower left
                    framebuffer.SetPixel(x - x1, y + y1, argb); // lower left left
                }
                if (p < 0) {
                    p += 4 * x1++ + 6;
                } else {
                    p += 4 * (x1++ - y1--) + 10;
                }
            }
        }

        public static void DrawCircle(FrameBuffer framebuffer, int x, int y, int r, Color color, bool fill = false)
        {
            DrawCircle(framebuffer, x, y, r, color.argb, fill);
        }
    }
}