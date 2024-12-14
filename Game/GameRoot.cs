using System;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Apos.Input;
using System.Text.Json.Serialization.Metadata;
using Apos.Camera;
using Apos.Shapes;
using Apos.Tweens;

namespace GameProject {
    public class GameRoot : Game {
        public GameRoot() {
            _graphics = new GraphicsDeviceManager(this) {
                GraphicsProfile = GraphicsProfile.HiDef
            };
            IsMouseVisible = true;
            Content.RootDirectory = "Content";

            _settings = EnsureJson("Settings.json", SettingsContext.Default.Settings);
        }

        protected override void Initialize() {
            // TODO: Add your initialization logic here
            Window.AllowUserResizing = true;

            IsFixedTimeStep = _settings.IsFixedTimeStep;
            _graphics.SynchronizeWithVerticalRetrace = _settings.IsVSync;

            _settings.IsFullscreen = _settings.IsFullscreen || _settings.IsBorderless;

            RestoreWindow();
            if (_settings.IsFullscreen) {
                ApplyFullscreenChange(false);
            }

            base.Initialize();
        }

        protected override void LoadContent() {
            _s = new SpriteBatch(GraphicsDevice);
            _sb = new ShapeBatch(GraphicsDevice, Content);

            // TODO: use this.Content to load your game content here
            InputHelper.Setup(this);

            _camera = new Camera(new DefaultViewport(GraphicsDevice, Window));
        }

        protected override void UnloadContent() {
            if (!_settings.IsFullscreen) {
                SaveWindow();
            }

            SaveJson("Settings.json", _settings, SettingsContext.Default.Settings);

            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime) {
            InputHelper.UpdateSetup();
            TweenHelper.UpdateSetup(gameTime);

            if (_quit.Pressed())
                Exit();

            if (_toggleFullscreen.Pressed()) {
                ToggleFullscreen();
            }
            if (_toggleBorderless.Pressed()) {
                ToggleBorderless();
            }

            UpdateCamera();

            // TODO: Add your update logic here

            InputHelper.UpdateCleanup();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            _camera.SetViewport();
            _sb.Begin(view: _camera.View);
            _sb.DrawCircle(new Vector2(0, 0), 10f, TWColor.Red500, TWColor.Black, 2f);
            _sb.End();

            _sb.Begin();
            var camExp = ScaleToExp(_camera.ZToScale(_camera.Z, 0f));
            if (_zoomSidebarTween.Value > 0f) {
                var length = _minExp - _maxExp;
                var percent = (camExp - _maxExp) / length;
                _sb.DrawLine(new Vector2(0, GraphicsDevice.Viewport.Height), new Vector2(0, GraphicsDevice.Viewport.Height * percent), 10f, TWColor.White * _zoomSidebarTween.Value, TWColor.Black, 2f);
            }
            _sb.End();
            _camera.ResetViewport();

            base.Draw(gameTime);
        }

        private void UpdateCamera() {
            if (_dragZoom.Held()) {
                if (_dragZoom.Pressed()) {
                    _expStart = _targetExp;
                    _zoomStart = new Vector2(InputHelper.NewMouse.X, InputHelper.NewMouse.Y);
                    _dragAnchor = _camera.ScreenToWorld(InputHelper.NewMouse.X, InputHelper.NewMouse.Y);
                    _pinCamera = new Vector2(InputHelper.NewMouse.X, InputHelper.NewMouse.Y);
                }
                var diffY = (InputHelper.NewMouse.Y - _zoomStart.Y) / 100f;
                SetExpTween(MathHelper.Clamp(_expStart + diffY, _maxExp, _minExp), 0);

                ShowZoomSidebar();
            } else if (MouseCondition.Scrolled()) {
                SetExpTween(MathHelper.Clamp(_targetExp - MouseCondition.ScrollDelta * _expDistance, _maxExp, _minExp));

                ShowZoomSidebar();
            }

            if (_rotateLeft.Pressed()) {
                SetRotationTween(_rotation.B + MathHelper.PiOver4);
            }
            if (_rotateRight.Pressed()) {
                SetRotationTween(_rotation.B - MathHelper.PiOver4);
            }

            _camera.Z = _camera.ScaleToZ(ExpToScale(_exp.Value), 0f);
            _camera.Rotation = _rotation.Value;

            if (_dragZoom.Held()) {
                SetXYTween(_xy.Value + _dragAnchor - _camera.ScreenToWorld(_pinCamera), 0);
                _mouseWorld = _camera.ScreenToWorld(InputHelper.NewMouse.X, InputHelper.NewMouse.Y);
            } else {
                _mouseWorld = _camera.ScreenToWorld(InputHelper.NewMouse.X, InputHelper.NewMouse.Y);

                if (_dragCamera.Pressed()) {
                    _dragAnchor = _mouseWorld;
                }
                if (_dragCamera.Held()) {
                    SetXYTween(_xy.Value + _dragAnchor - _mouseWorld, 0);
                    _mouseWorld = _dragAnchor;
                }
            }

            _camera.XY = _xy.Value;
        }

        private static float ScaleToExp(float scale) {
            return -MathF.Log(scale);
        }
        private static float ExpToScale(float exp) {
            return MathF.Exp(-exp);
        }

        private void SetXYTween(float targetX, float targetY, long duration = 1200) {
            SetXYTween(new Vector2(targetX, targetY), duration);
        }
        private void SetXYTween(Vector2 target, long duration = 1200) {
            _xy.A = duration > 0 ? _xy.Value : target;
            _xy.B = target;
            _xy.StartTime = TweenHelper.TotalMS;
            _xy.Duration = duration;
        }
        private void SetExpTween(float target, long duration = 1200) {
            _targetExp = target;
            _exp.A = duration > 0 ? _exp.Value : _targetExp;
            _exp.B = _targetExp;
            _exp.StartTime = TweenHelper.TotalMS;
            _exp.Duration = duration;
            ShowZoomSidebar();
        }
        private void SetZTween(float target, long duration = 1200) {
            _targetExp = ScaleToExp(_camera.ZToScale(target, 0f));
            _exp.A = duration > 0 ? _exp.Value : _targetExp;
            _exp.B = _targetExp;
            _exp.StartTime = TweenHelper.TotalMS;
            _exp.Duration = duration;
            ShowZoomSidebar();
        }
        private void SetRotationTween(float target, long duration = 1200) {
            _rotation.A = duration > 0 ? _rotation.Value : target;
            _rotation.B = target;
            _rotation.StartTime = TweenHelper.TotalMS;
            _rotation.Duration = duration;
        }
        private void ShowZoomSidebar() {
            if (TweenHelper.TotalMS >= _zoomSidebarTween.StartTime + _zoomSidebarTween.Duration) {
                _zoomSidebarStart.StartTime = TweenHelper.TotalMS;
                _zoomSidebarStart.A = 0f;
                _zoomSidebarStart.B = 0.2f;
            } else if (TweenHelper.TotalMS < _zoomSidebarStart.StartTime + _zoomSidebarStart.Duration) {

            } else if (TweenHelper.TotalMS < _zoomSidebarTween.StartTime + _zoomSidebarTween.Duration) {
                _zoomSidebarStart.A = _zoomSidebarTween.Value;
                _zoomSidebarStart.StartTime = TweenHelper.TotalMS;
            } else {
                _zoomSidebarStart.StartTime = TweenHelper.TotalMS - _zoomSidebarStart.Duration;
            }
        }

        private void ToggleFullscreen() {
            bool oldIsFullscreen = _settings.IsFullscreen;

            if (_settings.IsBorderless) {
                _settings.IsBorderless = false;
            } else {
                _settings.IsFullscreen = !_settings.IsFullscreen;
            }

            ApplyFullscreenChange(oldIsFullscreen);
        }
        private void ToggleBorderless() {
            bool oldIsFullscreen = _settings.IsFullscreen;

            _settings.IsBorderless = !_settings.IsBorderless;
            _settings.IsFullscreen = _settings.IsBorderless;

            ApplyFullscreenChange(oldIsFullscreen);
        }

        public static string GetPath(string name) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
        public static T LoadJson<T>(string name, JsonTypeInfo<T> typeInfo) where T : new() {
            T json;
            string jsonPath = GetPath(name);

            if (File.Exists(jsonPath)) {
                json = JsonSerializer.Deserialize<T>(File.ReadAllText(jsonPath), typeInfo);
            } else {
                json = new T();
            }

            return json;
        }
        public static void SaveJson<T>(string name, T json, JsonTypeInfo<T> typeInfo) {
            string jsonPath = GetPath(name);
            string jsonString = JsonSerializer.Serialize(json, typeInfo);
            File.WriteAllText(jsonPath, jsonString);
        }
        public static T EnsureJson<T>(string name, JsonTypeInfo<T> typeInfo) where T : new() {
            T json;
            string jsonPath = GetPath(name);

            if (File.Exists(jsonPath)) {
                json = JsonSerializer.Deserialize<T>(File.ReadAllText(jsonPath), typeInfo);
            } else {
                json = new T();
                string jsonString = JsonSerializer.Serialize(json, typeInfo);
                File.WriteAllText(jsonPath, jsonString);
            }

            return json;
        }

        private void ApplyFullscreenChange(bool oldIsFullscreen) {
            if (_settings.IsFullscreen) {
                if (oldIsFullscreen) {
                    ApplyHardwareMode();
                } else {
                    SetFullscreen();
                }
            } else {
                UnsetFullscreen();
            }
        }
        private void ApplyHardwareMode() {
            _graphics.HardwareModeSwitch = !_settings.IsBorderless;
            _graphics.ApplyChanges();
        }
        private void SetFullscreen() {
            SaveWindow();

            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.HardwareModeSwitch = !_settings.IsBorderless;

            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
        }
        private void UnsetFullscreen() {
            _graphics.IsFullScreen = false;
            RestoreWindow();
        }
        private void SaveWindow() {
            _settings.X = Window.ClientBounds.X;
            _settings.Y = Window.ClientBounds.Y;
            _settings.Width = Window.ClientBounds.Width;
            _settings.Height = Window.ClientBounds.Height;
        }
        private void RestoreWindow() {
            Window.Position = new Point(_settings.X, _settings.Y);
            _graphics.PreferredBackBufferWidth = _settings.Width;
            _graphics.PreferredBackBufferHeight = _settings.Height;
            _graphics.ApplyChanges();
        }

        readonly GraphicsDeviceManager _graphics;
        SpriteBatch _s;
        ShapeBatch _sb;

        readonly Settings _settings;

        readonly ICondition _quit =
            new AnyCondition(
                new KeyboardCondition(Keys.Escape),
                new GamePadCondition(GamePadButton.Back, 0)
            );
        readonly ICondition _toggleFullscreen =
            new AllCondition(
                new KeyboardCondition(Keys.LeftAlt),
                new KeyboardCondition(Keys.Enter)
            );
        readonly ICondition _toggleBorderless = new KeyboardCondition(Keys.F11);

        readonly ICondition _dragZoom =
            new AllCondition(
                new AnyCondition(
                    new KeyboardCondition(Keys.LeftControl),
                    new KeyboardCondition(Keys.RightControl)
                ),
                new MouseCondition(MouseButton.MiddleButton)
            );
        readonly ICondition _rotateLeft = new KeyboardCondition(Keys.OemComma);
        readonly ICondition _rotateRight = new KeyboardCondition(Keys.OemPeriod);

        readonly ICondition _dragCamera =
            new AnyCondition(
                new MouseCondition(MouseButton.RightButton),
                new MouseCondition(MouseButton.MiddleButton),
                new KeyboardCondition(Keys.X)
            );

        Camera _camera;

        Vector2 _mouseWorld;
        Vector2 _dragAnchor = Vector2.Zero;
        float _expStart;
        Vector2 _zoomStart;
        Vector2 _pinCamera;

        float _targetExp = 0f;
        readonly float _expDistance = 0.002f;
        readonly float _maxExp = -4f;
        readonly float _minExp = 4f;

        static readonly FloatTween _zoomSidebarStart = new(0f, 0.2f, 1000, Easing.QuintOut);
        static readonly ITween<float> _zoomSidebarWait = _zoomSidebarStart.Wait(1000);
        readonly ITween<float> _zoomSidebarTween = _zoomSidebarWait.To(0f, 1000, Easing.QuintOut);

        readonly Vector2Tween _xy = new(Vector2.Zero, Vector2.Zero, 0, Easing.QuintOut);
        readonly FloatTween _exp = new(0f, 0f, 0, Easing.QuintOut);
        readonly FloatTween _rotation = new(0f, 0f, 0, Easing.QuintOut);
    }
}
