using System;

namespace PSConsoleGL.Terminal.Drawing
{
    // Minimal 3D types and software rasterizer to render into existing FrameBuffer
    public class Vector3
    {
        public float X, Y, Z;
        public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }
        public float Length() => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3 operator *(Vector3 a, float s) => new Vector3(a.X * s, a.Y * s, a.Z * s);
        public static Vector3 operator /(Vector3 a, float s) => new Vector3(a.X / s, a.Y / s, a.Z / s);
        public static Vector3 Cross(Vector3 a, Vector3 b) => new Vector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        public static float Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        public static Vector3 Normalize(Vector3 v) { var len = v.Length(); if (len == 0) return new Vector3(0, 0, 0); return v / len; }
    }

    public class Matrix4
    {
        private float[,] m = new float[4, 4];

        public float this[int r, int c] { get => m[r, c]; set => m[r, c] = value; }

        public static Matrix4 Identity()
        {
            var M = new Matrix4();
            M[0, 0] = M[1, 1] = M[2, 2] = M[3, 3] = 1f;
            return M;
        }

        public static Matrix4 Multiply(Matrix4 a, Matrix4 b)
        {
            var r = new Matrix4();
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    float s = 0;
                    for (int k = 0; k < 4; k++) s += a[i, k] * b[k, j];
                    r[i, j] = s;
                }
            return r;
        }

        public float[] MultiplyVector(float x, float y, float z, float w = 1f)
        {
            float[] r = new float[4];
            r[0] = x * m[0, 0] + y * m[0, 1] + z * m[0, 2] + w * m[0, 3];
            r[1] = x * m[1, 0] + y * m[1, 1] + z * m[1, 2] + w * m[1, 3];
            r[2] = x * m[2, 0] + y * m[2, 1] + z * m[2, 2] + w * m[2, 3];
            r[3] = x * m[3, 0] + y * m[3, 1] + z * m[3, 2] + w * m[3, 3];
            return r;
        }

        // Right-handed perspective matrix (similar to OpenGL). z in NDC will be in [-1,1]
        public static Matrix4 PerspectiveFov(float fovRadians, float aspect, float near, float far)
        {
            var M = new Matrix4();
            float f = 1f / (float)Math.Tan(fovRadians / 2f);
            M[0, 0] = f / aspect;
            M[1, 1] = f;
            M[2, 2] = (far + near) / (near - far);
            M[2, 3] = (2f * far * near) / (near - far);
            M[3, 2] = -1f;
            M[3, 3] = 0f;
            return M;
        }

        public static Matrix4 LookAt(Vector3 eye, Vector3 target, Vector3 up)
        {
            var zaxis = Vector3.Normalize(eye - target); // forward
            var xaxis = Vector3.Normalize(Vector3.Cross(up, zaxis)); // right
            var yaxis = Vector3.Cross(zaxis, xaxis);

            var M = Identity();
            M[0, 0] = xaxis.X; M[0, 1] = xaxis.Y; M[0, 2] = xaxis.Z; M[0, 3] = -Vector3.Dot(xaxis, eye);
            M[1, 0] = yaxis.X; M[1, 1] = yaxis.Y; M[1, 2] = yaxis.Z; M[1, 3] = -Vector3.Dot(yaxis, eye);
            M[2, 0] = zaxis.X; M[2, 1] = zaxis.Y; M[2, 2] = zaxis.Z; M[2, 3] = -Vector3.Dot(zaxis, eye);
            return M;
        }
    }

    public class Mesh
    {
        public Vector3[] Vertices { get; private set; }
        public int[] Indices { get; private set; } // triangles, 3 indices each
        public int ColorArgb { get; private set; }
        public int[] TriangleColors { get; private set; }

        public Mesh(Vector3[] vertices, int[] indices, int colorArgb)
        {
            Vertices = vertices;
            Indices = indices;
            ColorArgb = colorArgb;
            TriangleColors = null;
        }

        public Mesh(Vector3[] vertices, int[] indices, int[] triangleColors)
        {
            Vertices = vertices;
            Indices = indices;
            TriangleColors = triangleColors;
        }

        // Simple factory for a colored cube centered at origin with size 1
        public static Mesh CreateUnitCube(int[] faceColors = null)
        {
            var v = new Vector3[] {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
            };
            int[] idx = new int[] {
                // back
                0,1,2, 0,2,3,
                // front
                4,6,5, 4,7,6,
                // left
                4,5,1, 4,1,0,
                // right
                3,2,6, 3,6,7,
                // top
                1,5,6, 1,6,2,
                // bottom
                4,0,3, 4,3,7
            };
            int triCount = idx.Length / 3;
            int[] triColors = new int[triCount];
            int[] defaultFaces = new int[] {
                unchecked((int)0xFFFF0000), // back - red
                unchecked((int)0xFF00FF00), // front - green
                unchecked((int)0xFF0000FF), // left - blue
                unchecked((int)0xFFFFFF00), // right - yellow
                unchecked((int)0xFFFF00FF), // top - magenta
                unchecked((int)0xFF00FFFF)  // bottom - cyan
            };
            int[] faces = faceColors ?? defaultFaces;
            for (int f = 0; f < 6; f++)
            {
                int color = (f < faces.Length) ? faces[f] : defaultFaces[f];
                triColors[f * 2 + 0] = color;
                triColors[f * 2 + 1] = color;
            }
            return new Mesh(v, idx, triColors);
        }
    }

    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; } = new Vector3(0, 1, 0);
        public float FovRadians { get; set; } = (float)(Math.PI / 3.0);
        public float Near { get; set; } = 0.1f;
        public float Far { get; set; } = 100f;
        public float Aspect { get; set; } = 1f;

        public Camera(Vector3 pos, Vector3 target, float aspect)
        {
            Position = pos; Target = target; Aspect = aspect;
        }

        public Matrix4 GetViewProjection()
        {
            var view = Matrix4.LookAt(Position, Target, Up);
            var proj = Matrix4.PerspectiveFov(FovRadians, Aspect, Near, Far);
            return Matrix4.Multiply(proj, view);
        }

        // Rotate orbiting the camera around the Target. yaw and pitch are in radians.
        public void Orbit(float yawDelta, float pitchDelta)
        {
            Vector3 dir = Position - Target;
            float r = dir.Length();
            if (r <= 1e-6f) r = 1e-6f;

            float yaw = (float)Math.Atan2(dir.Z, dir.X);
            float pitch = (float)Math.Asin(dir.Y / r);

            yaw += yawDelta;
            pitch = Math.Max(-1.49f, Math.Min(1.49f, pitch + pitchDelta)); // clamp to avoid gimbal flip

            float cp = (float)Math.Cos(pitch);
            float x = r * cp * (float)Math.Cos(yaw);
            float y = r * (float)Math.Sin(pitch);
            float z = r * cp * (float)Math.Sin(yaw);

            Position = new Vector3(x, y, z) + Target;
        }

        // Zoom by moving position towards/away from target by delta units (positive moves away)
        public void Zoom(float delta)
        {
            Vector3 dir = Position - Target;
            float r = dir.Length();
            if (r <= 1e-6f) r = 1e-6f;
            float nr = Math.Max(0.01f, r + delta);
            Vector3 ndir = Vector3.Normalize(dir) * nr;
            Position = Target + ndir;
        }

        // Pan moves both Position and Target along camera right and up vectors. rightOffset and upOffset are world units.
        public void Pan(float rightOffset, float upOffset)
        {
            Vector3 forward = Vector3.Normalize(Target - Position);
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Up));
            Vector3 up = Vector3.Normalize(Up);
            Vector3 delta = right * rightOffset + up * upOffset;
            Position = Position + delta;
            Target = Target + delta;
        }
    }

    public static class Renderer3D
    {
        // Render mesh into framebuffer using flat shading and a depth buffer
        public static void RenderMesh(Mesh mesh, Camera cam, FrameBuffer fb, Func<int,int,int,int> transformPixelColor = null)
        {
            int w = fb.Width, h = fb.Height;
            float[] depthBuffer = new float[w * h];
            for (int i = 0; i < depthBuffer.Length; i++) depthBuffer[i] = float.MaxValue;

            var mvp = cam.GetViewProjection();

            int triCount = mesh.Indices.Length / 3;
            for (int t = 0; t < triCount; t++)
            {
                int i0 = mesh.Indices[t * 3 + 0];
                int i1 = mesh.Indices[t * 3 + 1];
                int i2 = mesh.Indices[t * 3 + 2];

                var v0 = mesh.Vertices[i0];
                var v1 = mesh.Vertices[i1];
                var v2 = mesh.Vertices[i2];

                var p0 = TransformToScreen(v0, mvp, w, h);
                var p1 = TransformToScreen(v1, mvp, w, h);
                var p2 = TransformToScreen(v2, mvp, w, h);

                // Cull triangles that are behind camera by checking clip w
                if (p0.W <= 0 || p1.W <= 0 || p2.W <= 0) continue;

                int triColor = (mesh.TriangleColors != null && mesh.TriangleColors.Length > t) ? mesh.TriangleColors[t] : mesh.ColorArgb;
                RasterizeTriangle(p0, p1, p2, triColor, fb, depthBuffer, transformPixelColor);
            }
        }

        private struct ScreenVertex { public float X, Y, Z, W; }

        private static ScreenVertex TransformToScreen(Vector3 v, Matrix4 mvp, int w, int h)
        {
            var v4 = mvp.MultiplyVector(v.X, v.Y, v.Z, 1f);
            float wclip = v4[3];
            if (wclip == 0) wclip = 1e-6f;
            float nx = v4[0] / wclip; // NDC x [-1,1]
            float ny = v4[1] / wclip; // NDC y [-1,1]
            float nz = v4[2] / wclip; // NDC z [-1,1]

            float sx = (nx * 0.5f + 0.5f) * (w - 1);
            float sy = (1f - (ny * 0.5f + 0.5f)) * (h - 1); // flip y for screen coords

            return new ScreenVertex { X = sx, Y = sy, Z = nz, W = wclip };
        }

        private static void RasterizeTriangle(ScreenVertex v0, ScreenVertex v1, ScreenVertex v2, int colorArgb, FrameBuffer fb, float[] depthBuffer, Func<int,int,int,int> transformPixelColor)
        {
            int minX = (int)Math.Max(0, Math.Floor(Math.Min(v0.X, Math.Min(v1.X, v2.X))));
            int maxX = (int)Math.Min(fb.Width - 1, Math.Ceiling(Math.Max(v0.X, Math.Max(v1.X, v2.X))));
            int minY = (int)Math.Max(0, Math.Floor(Math.Min(v0.Y, Math.Min(v1.Y, v2.Y))));
            int maxY = (int)Math.Min(fb.Height - 1, Math.Ceiling(Math.Max(v0.Y, Math.Max(v1.Y, v2.Y))));

            float area = EdgeFunction(v0.X, v0.Y, v1.X, v1.Y, v2.X, v2.Y);
            if (Math.Abs(area) < 1e-6f) return; // degenerate

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float w0 = EdgeFunction(v1.X, v1.Y, v2.X, v2.Y, x + 0.5f, y + 0.5f) / area;
                    float w1 = EdgeFunction(v2.X, v2.Y, v0.X, v0.Y, x + 0.5f, y + 0.5f) / area;
                    float w2 = EdgeFunction(v0.X, v0.Y, v1.X, v1.Y, x + 0.5f, y + 0.5f) / area;

                    if (w0 >= 0 && w1 >= 0 && w2 >= 0)
                    {
                        // interpolate depth
                        float depth = w0 * v0.Z + w1 * v1.Z + w2 * v2.Z;
                        int idx = y * fb.Width + x;
                        // smaller z is nearer if using standard OpenGL-style NDC (-1 near, +1 far) -> but projection used produces more negative near? We'll compare using depth directly (closer means smaller value)
                        if (depth < depthBuffer[idx])
                        {
                            depthBuffer[idx] = depth;
                            int finalColor = colorArgb;
                            if (transformPixelColor != null)
                            {
                                int a = (finalColor >> 24) & 0xFF;
                                int r = (finalColor >> 16) & 0xFF;
                                int g = (finalColor >> 8) & 0xFF;
                                int b = finalColor & 0xFF;
                                finalColor = transformPixelColor(r, g, b);
                            }
                            fb.SetPixel(x, y, finalColor);
                        }
                    }
                }
            }
        }

        private static float EdgeFunction(float ax, float ay, float bx, float by, float cx, float cy)
        {
            return (cx - ax) * (by - ay) - (cy - ay) * (bx - ax);
        }
    }
}
