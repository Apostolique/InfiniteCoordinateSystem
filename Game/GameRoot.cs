using System;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Apos.Input;
using Track = Apos.Input.Track;
using System.Text.Json.Serialization.Metadata;
using Apos.Shapes;
using Apos.Tweens;
using System.Collections.Generic;
using FontStashSharp;

namespace GameProject {
    public class GameRoot : Game {
        public GameRoot() {
            _graphics = new GraphicsDeviceManager(this) {
                GraphicsProfile = GraphicsProfile.HiDef
            };
            IsMouseVisible = true;
            Content.RootDirectory = "Content";

            _settings = EnsureJson("Settings.json", SettingsContext.Default.Settings);

            Test.TestAll();
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

            _grid = Content.Load<Effect>("grid");
            _pixel = Content.Load<Texture2D>("pixel");

            _fontSystem = new FontSystem();
            _fontSystem.AddFont(TitleContainer.OpenStream($"{Content.RootDirectory}/source-code-pro-medium.ttf"));

            // TODO: use this.Content to load your game content here
            InputHelper.Setup(this);

            _camera1 = new Camera(new DefaultViewport(GraphicsDevice, Window));
            _camera2 = new Camera(new DefaultViewport(GraphicsDevice, Window));
            _camera3 = new Camera(new DefaultViewport(GraphicsDevice, Window));

            _coords.Add(new AposNumber(0, 0, 0));
            _coords.Add(new AposNumber(-1, 0, 0));

            // -2,147,483,648 to 2,147,483,647
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

            if (_loadCam1.Pressed()) {
                SetExpTween(0f);
            }
            if (_loadCam2.Pressed()) {
                SetExpTween(1f);
            }
            if (_loadCam3.Pressed()) {
                SetExpTween(2f);
            }

            InputHelper.UpdateCleanup();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(TWColor.Neutral950);

            // DrawGrid(_camera1, 100f * MathF.Pow(2f, _expScaling * 0f), 1f, TWColor.Red900);
            // DrawGrid(_camera1, 100f * MathF.Pow(2f, _expScaling * 1f), 1f, TWColor.Green900);
            // DrawGrid(_camera1, 100f * MathF.Pow(2f, _expScaling * 2f), 1f, TWColor.Blue900);

            float linear = ExpToLinear(_exp.Value);
            if (linear < -1) {
                DrawGrid(_camera1, 100f * MathF.Pow(2f, _expScaling * 0f), 1f, TWColor.Red900);
                DrawGrid(_camera2, 100f * MathF.Pow(2f, _expScaling * 0f), 1f, TWColor.Green900);
                DrawGrid(_camera2, 100f * MathF.Pow(2f, _expScaling * 1f), 1f, TWColor.Blue900);
            } else if (linear < 0) {
                DrawGrid(_camera2, 100f * MathF.Pow(2f, _expScaling * 0f), 1f, TWColor.Green900);
                DrawGrid(_camera3, 100f * MathF.Pow(2f, _expScaling * 0f), 1f, TWColor.Blue900);
            } else {
                DrawGrid(_camera3, 100f * MathF.Pow(2f, _expScaling * 0f), 1f, TWColor.Blue900);
            }

            _camera1.SetViewport();

            _sb.Begin(view: _camera3.View);
            _sb.BorderRectangle(new Vector2(-MathF.Pow(2f, 24f) - 1f) * MathF.Pow(2f, _expScaling * -1f), new Vector2(MathF.Pow(2f, 24f) - 1f) * MathF.Pow(2f, _expScaling * -1f) * 2f, TWColor.White, 1f);
            _sb.DrawCircle(new Vector2(100, 100), 10f, TWColor.Blue500, TWColor.White, 2f);
            _sb.DrawCircle(new Vector2(0, 0), 4f, TWColor.Black * 0.2f, TWColor.White * 0.2f, 2f);
            _sb.End();

            _sb.Begin(view: _camera2.View);
            _sb.BorderRectangle(new Vector2(-MathF.Pow(2f, 24f) - 1f) * MathF.Pow(2f, _expScaling * -1f), new Vector2(MathF.Pow(2f, 24f) - 1f) * MathF.Pow(2f, _expScaling * -1f) * 2f, TWColor.White, 1f);
            _sb.DrawCircle(new Vector2(100, 100), 10f, TWColor.Green500, TWColor.White, 2f);
            _sb.End();

            _sb.Begin(view: _camera1.View);
            _sb.DrawCircle(new Vector2(100, 100), 10f, TWColor.Red500, TWColor.White, 2f);
            _sb.End();

            _sb.Begin();
            var camExp = _camera1.Exp;
            if (_zoomSidebarTween.Value > 0f) {
                var length = _minExp - _maxExp;
                var percent = (camExp - _maxExp) / length;
                _sb.DrawLine(new Vector2(0, GraphicsDevice.Viewport.Height), new Vector2(0, GraphicsDevice.Viewport.Height * percent), 10f, TWColor.White * _zoomSidebarTween.Value, TWColor.Black, 2f);
            }
            _sb.End();
            _camera1.ResetViewport();

            var font = _fontSystem.GetFont(24);
            _s.Begin();
            _s.DrawString(font, $"{Math.Round(_exp.Value, 4, MidpointRounding.ToZero).ToString("F4")} -- {Math.Round(ExpToLinear(_exp.Value), 4, MidpointRounding.ToZero).ToString("F4")}", new Vector2(10, GraphicsDevice.Viewport.Height - 24), TWColor.White);
            _s.End();

            base.Draw(gameTime);
        }

        private void DrawGrid(Camera cam, float gridSize, float thickness, Color c) {
            _grid.Parameters["view_projection"].SetValue(Matrix.Identity * cam.GetProjection());
            _grid.Parameters["tex_transform"].SetValue(cam.GetViewInvert());

            float screenToWorld = cam.ScreenToWorldScale();
            _grid.Parameters["ps"].SetValue(screenToWorld);

            _grid.Parameters["line_size"].SetValue(thickness);
            _grid.Parameters["grid_size"].SetValue(new Vector2(gridSize));
            _s.Begin(effect: _grid, samplerState: SamplerState.LinearWrap);
            _s.Draw(_pixel, Vector2.Zero, _s.GraphicsDevice.Viewport.Bounds, c);
            _s.End();
        }

        private void UpdateCamera() {
            if (_dragZoom.Held()) {
                if (_dragZoom.Pressed()) {
                    _expStart = _targetExp;
                    _zoomStart = new Vector2(InputHelper.NewMouse.X, InputHelper.NewMouse.Y);
                    _dragAnchor1 = _camera1.ScreenToWorld(InputHelper.NewMouse.X, InputHelper.NewMouse.Y);
                    _dragAnchor2 = _camera2.ScreenToWorld(InputHelper.NewMouse.X, InputHelper.NewMouse.Y);
                    _dragAnchor3 = _camera3.ScreenToWorld(InputHelper.NewMouse.X, InputHelper.NewMouse.Y);
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

            _camera1.Exp = _exp.Value + LinearToExp(2f);
            _camera1.Rotation = _rotation.Value;

            _camera2.Exp = _exp.Value + LinearToExp(1f);
            _camera2.Rotation = _rotation.Value;

            _camera3.Exp = _exp.Value + LinearToExp(0f);
            _camera3.Rotation = _rotation.Value;

            if (_dragZoom.Held()) {
                SetXYTween(_xy1, _xy1.Value + _dragAnchor1 - _camera1.ScreenToWorld(_pinCamera), 0);
                SetXYTween(_xy2, _xy2.Value + _dragAnchor2 - _camera2.ScreenToWorld(_pinCamera), 0);
                SetXYTween(_xy3, _xy3.Value + _dragAnchor3 - _camera3.ScreenToWorld(_pinCamera), 0);

                _mouseWorld1 = _camera1.ScreenToWorld(InputHelper.NewMouse.X, InputHelper.NewMouse.Y);
                _mouseWorld2 = _camera2.ScreenToWorld(InputHelper.NewMouse.X, InputHelper.NewMouse.Y);
                _mouseWorld3 = _camera3.ScreenToWorld(InputHelper.NewMouse.X, InputHelper.NewMouse.Y);
            } else {
                _mouseWorld1 = _camera1.ScreenToWorld(InputHelper.NewMouse.X, InputHelper.NewMouse.Y);
                _mouseWorld2 = _camera2.ScreenToWorld(InputHelper.NewMouse.X, InputHelper.NewMouse.Y);
                _mouseWorld3 = _camera3.ScreenToWorld(InputHelper.NewMouse.X, InputHelper.NewMouse.Y);

                if (_dragCamera.Pressed()) {
                    _dragAnchor1 = _mouseWorld1;
                    _dragAnchor2 = _mouseWorld2;
                    _dragAnchor3 = _mouseWorld3;
                }
                if (_dragCamera.Held()) {
                    SetXYTween(_xy1, _xy1.Value + _dragAnchor1 - _mouseWorld1, 0);
                    SetXYTween(_xy2, _xy2.Value + _dragAnchor2 - _mouseWorld2, 0);
                    SetXYTween(_xy3, _xy3.Value + _dragAnchor3 - _mouseWorld3, 0);
                    _mouseWorld1 = _dragAnchor1;
                    _mouseWorld2 = _dragAnchor2;
                    _mouseWorld3 = _dragAnchor3;
                }
            }

            _camera1.XY = _xy1.Value;
            _camera2.XY = _xy2.Value;
            _camera3.XY = _xy3.Value;

            // Console.WriteLine($"{_mouseWorld1} -- {_mouseWorld2} -- {_mouseWorld3} -- {MathF.Floor(ExpToLinear(_exp.Value))}");
        }

        private void SetXYTween(Vector2Tween vt, float targetX, float targetY, long duration = 1200) {
            SetXYTween(vt, new Vector2(targetX, targetY), duration);
        }
        private void SetXYTween(Vector2Tween vt, Vector2 target, long duration = 1200) {
            vt.A = duration > 0 ? vt.Value : target;
            vt.B = target;
            vt.StartTime = TweenHelper.TotalMS;
            vt.Duration = duration;
        }
        private void SetExpTween(float target, long duration = 1200) {
            _targetExp = target;
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

        private float ExpToLinear(float exp) {
            return exp / _expScaling;
        }
        private float LinearToExp(float linear) {
            return linear * _expScaling;
        }

        readonly GraphicsDeviceManager _graphics;
        SpriteBatch _s;
        ShapeBatch _sb;

        Effect _grid;
        Texture2D _pixel;
        FontSystem _fontSystem = null!;

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

        readonly ICondition _loadCam1 = new Track.KeyboardCondition(Keys.D1);
        readonly ICondition _loadCam2 = new Track.KeyboardCondition(Keys.D2);
        readonly ICondition _loadCam3 = new Track.KeyboardCondition(Keys.D3);

        Camera _camera1;
        Camera _camera2;
        Camera _camera3;

        Vector2 _mouseWorld1;
        Vector2 _mouseWorld2;
        Vector2 _mouseWorld3;
        Vector2 _dragAnchor1 = Vector2.Zero;
        Vector2 _dragAnchor2 = Vector2.Zero;
        Vector2 _dragAnchor3 = Vector2.Zero;
        float _expStart;
        Vector2 _zoomStart;
        Vector2 _pinCamera;

        float _targetExp = 0f;
        readonly float _expDistance = 0.002f;
        readonly float _maxExp = -400f;
        readonly float _minExp = 400f;

        static readonly FloatTween _zoomSidebarStart = new(0f, 0.2f, 1000, Easing.QuintOut);
        static readonly ITween<float> _zoomSidebarWait = _zoomSidebarStart.Wait(1000);
        readonly ITween<float> _zoomSidebarTween = _zoomSidebarWait.To(0f, 1000, Easing.QuintOut);

        readonly Vector2Tween _xy1 = new(Vector2.Zero, Vector2.Zero, 0, Easing.QuintOut);
        readonly Vector2Tween _xy2 = new(Vector2.Zero, Vector2.Zero, 0, Easing.QuintOut);
        readonly Vector2Tween _xy3 = new(Vector2.Zero, Vector2.Zero, 0, Easing.QuintOut);
        readonly FloatTween _exp = new(0f, 0f, 0, Easing.QuintOut);
        readonly FloatTween _rotation = new(0f, 0f, 0, Easing.QuintOut);

        private float _expScaling = 8f;

        List<AposNumber> _coords = [];
    }
}
