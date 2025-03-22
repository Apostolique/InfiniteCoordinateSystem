using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace GameProject {
    public class Camera {
        public Camera(IVirtualViewport virtualViewport) {
            VirtualViewport = virtualViewport;
        }

        public float X {
            get => _xy.X;
            set {
                _xy.X = value;
            }
        }
        public float Y {
            get => _xy.Y;
            set {
                _xy.Y = value;
            }
        }
        public float Exp {
            get => _exp;
            set {
                _exp = value;
            }
        }

        public float Rotation { get; set; } = 0f;

        public Vector2 XY {
            get => _xy;
            set {
                X = value.X;
                Y = value.Y;
            }
        }

        public IVirtualViewport VirtualViewport { get; set; }

        public void SetViewport() {
            VirtualViewport.Set();
        }
        public void ResetViewport() {
            VirtualViewport.Reset();
        }

        public Matrix View => GetView() ;
        public Matrix ViewInvert => GetViewInvert();

        public Matrix GetView() {
            float scaleExp = ExpToScale(_exp);
            return VirtualViewport.Transform(
                Matrix.CreateTranslation(new Vector3(-XY, 0f)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(scaleExp, scaleExp, 1f) *
                Matrix.CreateTranslation(new Vector3(VirtualViewport.Origin, 0f)));
        }
        public Matrix GetViewInvert() => Matrix.Invert(GetView());

        public Matrix GetProjection() {
            return Matrix.CreateOrthographicOffCenter(0, VirtualViewport.Width, VirtualViewport.Height, 0, 0, 1);
        }

        public float WorldToScreenScale() => Vector2.Distance(WorldToScreen(0f, 0f), WorldToScreen(1f, 0f));
        public float ScreenToWorldScale() => Vector2.Distance(ScreenToWorld(0f, 0f), ScreenToWorld(1f, 0f));

        public Vector2 WorldToScreen(float x, float y) => WorldToScreen(new Vector2(x, y));
        public Vector2 WorldToScreen(Vector2 xy) {
            return Vector2.Transform(xy, GetView()) + VirtualViewport.XY;
        }
        public Vector2 ScreenToWorld(float x, float y) => ScreenToWorld(new Vector2(x, y));
        public Vector2 ScreenToWorld(Vector2 xy) {
            return Vector2.Transform(xy - VirtualViewport.XY, GetViewInvert());
        }

        public RectangleF ViewRect => GetViewRect();
        public RectangleF GetViewRect() {
            var frustum = GetBoundingFrustum();
            var corners = frustum.GetCorners();
            var a = corners[0];
            var b = corners[1];
            var c = corners[2];
            var d = corners[3];

            var left = Math.Min(Math.Min(a.X, b.X), Math.Min(c.X, d.X));
            var right = Math.Max(Math.Max(a.X, b.X), Math.Max(c.X, d.X));

            var top = Math.Min(Math.Min(a.Y, b.Y), Math.Min(c.Y, d.Y));
            var bottom = Math.Max(Math.Max(a.Y, b.Y), Math.Max(c.Y, d.Y));

            var width = right - left;
            var height = bottom - top;

            return new RectangleF(left, top, width, height);
        }
        public BoundingFrustum GetBoundingFrustum() {
            Matrix view = GetView();
            Matrix projection = GetProjection();
            return new BoundingFrustum(view * projection);
        }

        public static float ScaleToExp(float scale) {
            return -MathF.Log(scale, 2f);
        }
        public static float ExpToScale(float exp) {
            return MathF.Pow(2f, -exp);
        }

        private Vector2 _xy = Vector2.Zero;
        private float _exp = 0f;
    }
}
