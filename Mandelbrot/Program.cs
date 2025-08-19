using Mandelbrot;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Concurrent;
using System.ComponentModel;
using OTK = OpenTK.Windowing.GraphicsLibraryFramework;


namespace Mandelbrot

{

    public class Program : GameWindow

    {
        private InputHandler _inputHandler;
        private Renderer _renderer;
 
        private double _timeSinceLastFpsUpdate = 0.0;
        private int _frameCount = 0;
        private int _currentFps = 0;
        private float _frameTimeMs = 0.0f;

        public float CurrentFps => _currentFps;
        public float _minFps = float.MaxValue;
        public float _maxFps = float.MinValue;
        public float FrameTimeMs => _frameTimeMs;


        public event Action<int, float> OnPerformanceMetricsUpdated; 
        public event Action<float, float> OnUpdateExtremeMetrics; 
        public event Action<double> OnZoomUpdated; 
        public event Action<Vector3d> OnCenterUpdated;

        int canvasWidth = 640, canvasHeight = 480;
        public string renderType = "GPU32";


        public int StartX { get; set; } = 0;
        public int StartY { get; set; } = 0;


        public int NPower { get; set; } = 2;
        public int MIterations { get; set; } = 1500;
        public double UTime { get; private set; }


        public float initialSideLength = 3.4f;
        public Vector2 currentCenter;
        public float target_SideLength;
        public Vector2 target_Center;
        public Vector2 rectanglePosBegin, rectanglePosEnd;


        public double initialSideLengthPD = 3.4d;
        public Vector2d currentCenterPD;
        public double target_SideLengthPD;
        public Vector2d target_CenterPD;
        public Vector2 rectanglePosBeginPD, rectanglePosEndPD;

        public Vector2i target_Location;
        public Vector2i target_Size;

        public bool isSelectingRectangle;
        public bool allowDebugging = false;



        public Program(NativeWindowSettings nativeWindowSettings)
          : base(GameWindowSettings.Default, nativeWindowSettings)
        {

            target_Location = Location;
            target_Size = Size;
            target_SideLength = initialSideLength;
            target_SideLengthPD = initialSideLengthPD;

            _renderer = new Renderer(renderType);

            _inputHandler = new InputHandler(
                      (cursor) => Cursor = cursor,
                      (state) => CursorState = state,
                      (pts) => PointToScreen(pts),
                      getCanvasArea());


            _inputHandler.MouseUp += OnMouseUp;
            _inputHandler.MouseDown += OnMouseDown;
            _inputHandler.MouseMove += OnMouseMove;
            _inputHandler.MouseWheel += OnMouseWheel;
            _inputHandler.KeyDown += OnKeyDown;
            _inputHandler.KeyUp += OnKeyUp;
            _inputHandler.requestPanning += HandlePan;
            _inputHandler.requestZooming += HandleZoom;
            _inputHandler.requestWindowDragging += HandleWindowDrag;
            _inputHandler.requestWindowResize += HandleWindowResize;
            _inputHandler.rectanglePositionUpdated += UpdateRectanglePos;
            _inputHandler.selectRectangle += HandleRectangleSelection;
            _inputHandler.ApplyRectangleSelection += ApplySelectionZoom;


            canvasWidth = nativeWindowSettings.ClientSize.X;
            canvasHeight = nativeWindowSettings.ClientSize.Y;

            nativeWindowSettings.Location = new Vector2i(StartX, StartY);
        }


        public void SetIterations(int iterations)
        {
            MIterations = iterations;
        }

        public void SetCenterPos(char c, float val) { 
            if (c == 'x') { currentCenter.X = val; 
                target_Center.X = val; 
            } else if (c == 'y') {
                currentCenter.Y = val; 
                target_Center.Y = val; 
            } 
        }

        public void SetCenterPos(char c, double val) {
            if (c == 'x') { 
                currentCenterPD.X = val; 
                target_CenterPD.X = val; 
            } else if (c == 'y') { 
                currentCenterPD.Y = val; 
                target_CenterPD.Y = val; 
            }
        }

        public void changeRenderer(string newRendererType)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            _renderer.OnLoad();
        }


        private void HandlePan(Vector2 pixelDelta)
        {
            if (renderType == "GPU32")
            {

                float dx = -(pixelDelta.X / Size.X) * initialSideLength;
                float dy = (pixelDelta.Y / Size.Y) * initialSideLength;

                target_Center.X += dx;
                target_Center.Y += dy;

            }
            else if (renderType == "GPU64")
            {

                double dx = -(pixelDelta.X / Size.X) * initialSideLengthPD;
                double dy = (pixelDelta.Y / Size.Y) * initialSideLengthPD;

                target_CenterPD.X += dx;
                target_CenterPD.Y += dy;
            }

        }

        private void HandleZoom(float scrollDelta, Vector2 mousePos)
        {
            const float zoomFactor = 0.22f;

            float windowWidth = Size.X;
            float windowHeight = Size.Y;
            float aspectRatio = windowWidth / windowHeight;

            float mouseNormX = mousePos.X / windowWidth;
            float mouseNormY = mousePos.Y / windowHeight;


            if (renderType == "GPU32")
            {

                float halfSide = target_SideLength / 2.0f;
                float minX = target_Center.X - halfSide * aspectRatio;
                float maxX = target_Center.X + halfSide * aspectRatio;
                float minY = target_Center.Y - halfSide;
                float maxY = target_Center.Y + halfSide;
                float mouseWorldX = minX + mouseNormX * (maxX - minX);
                float mouseWorldY = maxY - mouseNormY * (maxY - minY);

                // Zoom
                if (scrollDelta > 0)
                    target_SideLength /= (1 + zoomFactor);
                else if (scrollDelta < 0)
                    target_SideLength *= (1 + zoomFactor);

                // Move center towards mouse
                float newHalfSide = target_SideLength / 2.0f;
                float newMinX = target_Center.X - newHalfSide * aspectRatio;
                float newMaxX = target_Center.X + newHalfSide * aspectRatio;
                float newMinY = target_Center.Y - newHalfSide;
                float newMaxY = target_Center.Y + newHalfSide;

                float newMouseWorldX = newMinX + mouseNormX * (newMaxX - newMinX);
                float newMouseWorldY = newMaxY - mouseNormY * (newMaxY - newMinY);


                target_Center.X += mouseWorldX - newMouseWorldX;
                target_Center.Y += mouseWorldY - newMouseWorldY;

            }

            else if (renderType == "GPU64")
            {

                double halfSide = target_SideLengthPD / 2.0f;
                double minX = target_CenterPD.X - halfSide * aspectRatio;
                double maxX = target_CenterPD.X + halfSide * aspectRatio;
                double minY = target_CenterPD.Y - halfSide;
                double maxY = target_CenterPD.Y + halfSide;

                double mouseWorldX = minX + mouseNormX * (maxX - minX);
                double mouseWorldY = maxY - mouseNormY * (maxY - minY);

                // Zoom
                if (scrollDelta > 0)
                    target_SideLengthPD /= (1 + zoomFactor);
                else if (scrollDelta < 0)
                    target_SideLengthPD *= (1 + zoomFactor);

                // Move center towards mouse
                double newHalfSide = target_SideLengthPD / 2.0f;
                double newMinX = target_CenterPD.X - newHalfSide * aspectRatio;
                double newMaxX = target_CenterPD.X + newHalfSide * aspectRatio;
                double newMinY = target_CenterPD.Y - newHalfSide;
                double newMaxY = target_CenterPD.Y + newHalfSide;

                double newMouseWorldX = newMinX + mouseNormX * (newMaxX - newMinX);
                double newMouseWorldY = newMaxY - mouseNormY * (newMaxY - newMinY);

                target_CenterPD.X += mouseWorldX - newMouseWorldX;
                target_CenterPD.Y += mouseWorldY - newMouseWorldY;
            }

        }

        private void HandleWindowDrag(Vector2i delta)
        {
            target_Location = delta;
        }

        private void HandleWindowResize(Vector2i Location, Vector2i Size)
        {
            target_Location = Location;
            target_Size = Size;
        }

        private void UpdateRectanglePos(Vector2 start, Vector2 end)
        {
            rectanglePosBegin = start;
            rectanglePosEnd = end;
        }

        private void HandleRectangleSelection(bool value)
        {
            isSelectingRectangle = value;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {

            base.OnRenderFrame(args);

            _timeSinceLastFpsUpdate += args.Time;
            _frameCount++;
            _frameTimeMs = (float)(args.Time * 1000.0);


            if (_timeSinceLastFpsUpdate >= 1.0)
            {
                _currentFps = (int)(_frameCount / (float)_timeSinceLastFpsUpdate);
                _frameCount = 0;

                if (_currentFps < _minFps)
                {
                    _minFps = _currentFps;
                }

                if (_currentFps > _maxFps)
                {
                    _maxFps = _currentFps;
                }

                _timeSinceLastFpsUpdate = 0.0;

                OnPerformanceMetricsUpdated?.Invoke(_currentFps, _frameTimeMs);

            }

            if (renderType == "GPU32")
            {
                _renderer.Center = currentCenter;
                _renderer.SideLength = initialSideLength;
            }else if (renderType == "GPU64")
            {
                _renderer.CenterPD = currentCenterPD;
                _renderer.SideLengthPD = initialSideLengthPD;
            }

            _renderer.Render(Size);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            _inputHandler._kSpacePressed = KeyboardState.IsKeyDown(OTK.Keys.Space);
            _inputHandler._ctrlSelected = KeyboardState.IsKeyDown(OTK.Keys.LeftControl) || KeyboardState.IsKeyDown(OTK.Keys.RightControl);

            float deltaTime = (float)args.Time;
            float dT1 = deltaTime * 10f;
            float dT2 = deltaTime * 3.5f;


            if (renderType == "GPU32")
            {
                initialSideLength = MathHelper.Lerp(initialSideLength, target_SideLength, dT2);
                currentCenter = Vector2.Lerp(currentCenter, target_Center, dT2);
                OnCenterUpdated?.Invoke(new Vector3d(currentCenter.X, currentCenter.Y, 0.0));
                OnZoomUpdated?.Invoke(initialSideLength);
            }
            else if (renderType == "GPU64")
            {
                initialSideLengthPD = MathHelper.Lerp(initialSideLengthPD, target_SideLengthPD, dT2);
                currentCenterPD = Vector2d.Lerp(currentCenterPD, target_CenterPD, dT2);
                OnCenterUpdated?.Invoke(new Vector3d(currentCenterPD.X, currentCenterPD.Y, 0.0));
                OnZoomUpdated?.Invoke(initialSideLengthPD);
            }
            Location = target_Location;

            Size = new Vector2i(
              (int)MathHelper.Lerp(Size.X, target_Size.X, dT1),
              (int)MathHelper.Lerp(Size.Y, target_Size.Y, dT1)
            );


            _inputHandler.UpdateCursor(MousePosition, Size);
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            _renderer.OnUnload();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e) => _inputHandler.Handle_MouseUp(e);
        protected override void OnMouseDown(MouseButtonEventArgs e) => _inputHandler.Handle_MouseDown(e, MousePosition, Size, Location);
        protected override void OnMouseMove(MouseMoveEventArgs e) => _inputHandler.Handle_MouseMove(e, MousePosition, Size);
        protected override void OnMouseWheel(MouseWheelEventArgs e) => _inputHandler.Handle_MouseWheel(e, MousePosition);

        protected override void OnKeyDown(KeyboardKeyEventArgs e) => _inputHandler.Handle_KeyDown(e, MousePosition);
        protected override void OnKeyUp(KeyboardKeyEventArgs e) => _inputHandler.Handle_KeyUp(e, MousePosition);

        public Vector4 getCanvasArea() {
            return new Vector4(currentCenter.X, currentCenter.Y, currentCenter.X + canvasWidth, currentCenter.Y + canvasHeight);
        }

        public void ApplySelectionZoom()
        {

            float px1 = rectanglePosBegin.X;
            float py1 = rectanglePosBegin.Y;
            float px2 = rectanglePosEnd.X;
            float py2 = rectanglePosEnd.Y;

            float rectLeft_px = Math.Min(px1, px2);
            float rectRight_px = Math.Max(px1, px2);
            float rectTop_px = Math.Min(py1, py2);
            float rectBottom_px = Math.Max(py1, py2);

            float rectWidth_px = rectRight_px - rectLeft_px;
            float rectHeight_px = rectBottom_px - rectTop_px;

            if (rectWidth_px < 10 || rectHeight_px < 10)
            {
                Console.WriteLine("Selection too small for zoom.");
                return;
            }


            float aspectRatio = Size.X / (float)Size.Y;
            float halfSideLength = initialSideLength / 2.0f;

            float MinX = currentCenter.X - halfSideLength * aspectRatio;
            float MaxX = currentCenter.X + halfSideLength * aspectRatio;
            float MinY = currentCenter.Y - halfSideLength;
            float MaxY = currentCenter.Y + halfSideLength;

            float currentFractalWidth = MaxX - MinX;
            float currentFractalHeight = MaxY - MinY;

            float selectedFractalMinX = MinX + (rectLeft_px / Size.X) * currentFractalWidth;
            float selectedFractalMaxX = MinX + (rectRight_px / Size.X) * currentFractalWidth;
            float selectedFractalMinY = MinY + (1.0f - rectBottom_px / Size.Y) * currentFractalHeight;
            float selectedFractalMaxY = MinY + (1.0f - rectTop_px / Size.Y) * currentFractalHeight;


            currentCenter.X = (selectedFractalMinX + selectedFractalMaxX) / 2.0f;
            currentCenter.Y = (selectedFractalMinY + selectedFractalMaxY) / 2.0f;
            target_Center = currentCenter;

            float newFractalWidth = selectedFractalMaxX - selectedFractalMinX;
            float newFractalHeight = selectedFractalMaxY - selectedFractalMinY;

            float currentWindowAspectRatio = (float)Size.X / Size.Y;
            float selectedRectAspectRatio = newFractalWidth / newFractalHeight;


            if (selectedRectAspectRatio > currentWindowAspectRatio)
            {
                initialSideLength = newFractalWidth / currentWindowAspectRatio;
                target_SideLength = initialSideLength;
            }else
            {
                initialSideLength = newFractalHeight;
                target_SideLength = initialSideLength;

            }

            _renderer.Center = currentCenter;
            _renderer.Center = currentCenter;
        }
    }
}