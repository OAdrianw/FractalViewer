using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OTK = OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.GraphicsLibraryFramework;


namespace Fractal_Renderer
{

    public delegate void SetCursorDelegate(MouseCursor cursor);
    public delegate void SetCursorStateDelegate(CursorState state);

    public delegate Vector2i PointToScreenDelegate(Vector2i pos);

    internal class InputHandler
    {
        private SetCursorDelegate _setCursor;
        private SetCursorStateDelegate _setCursorState;
        private PointToScreenDelegate _pointToScreen;

        public event Action<Vector2> MouseMoved;
        public event Action<MouseButtonEventArgs> MouseUp, MouseDown;
        public event Action<MouseMoveEventArgs> MouseMove;
        public event Action<MouseWheelEventArgs> MouseWheel;
        public event Action<KeyboardKeyEventArgs> KeyDown, KeyUp;

        public event Action<bool> selectRectangle;
        public event Action<Vector2, Vector2> rectanglePositionUpdated;
        public event Action<Vector2, Vector2> requestMousePosition;
        public event Action ApplyRectangleSelection;
        public event Action<Vector2> requestPanning;
        public event Action<float, Vector2> requestZooming;
        public event Action<Vector2i> requestWindowDragging;
        public event Action<Vector2i, Vector2i> requestWindowResize;


        public Vector2i _windowDragOffset = Vector2i.Zero;
        public bool _isSelectingRectangle;
        public bool _isFractalPanning;
        public bool _isWindowDragging;
        public bool _isResizingWindow;
        public bool _kSpacePressed, _ctrlSelected;
        public bool _isResizeLeft, _isResizeRight, _isResizeTop, _isResizeBottom;
        public bool lockMousePos = false;
        public int _resizeBorderThickness = 25;

        public Vector4 canvasArea;
        public Vector2i target_Location, target_Size;

        public Vector2 _lastMousePos;
        public Vector2 _selectStart;
        public Vector2 _selectEnd;
        public Vector2i _initialWindowSize, _initialWindowLocation, _initialMousePositionScreen;
        private MouseCursor _previousCursor = MouseCursor.Default;

        public InputHandler(
                            SetCursorDelegate cursor,
                            SetCursorStateDelegate cState,
                            PointToScreenDelegate PTS,
                            Vector4 canvas
                            )
        {
            _pointToScreen = PTS;
            _setCursor = cursor;
            _setCursorState = cState;

            canvasArea = canvas;
        }

        public void Handle_MouseUp(MouseButtonEventArgs e) {

            if (e.Button == MouseButton.Left)
            {
                // Apply zoom from selection
                if (_isSelectingRectangle && _ctrlSelected)
                {
                    ApplyRectangleSelection?.Invoke();

                }
                _isFractalPanning = false;
                _isWindowDragging = false;
                _isResizingWindow = false;
                _isSelectingRectangle = false;
                selectRectangle?.Invoke(false); 

                _isResizeLeft = _isResizeRight = _isResizeTop = _isResizeBottom = false;
                _windowDragOffset = Vector2i.Zero;


            }
        }

        public void Handle_MouseDown(MouseButtonEventArgs e, Vector2 mousePos, Vector2i Size, Vector2i Location) {

            _lastMousePos = mousePos;

            if (e.Button == MouseButton.Left)
            {

                if (GetResizeCursor(mousePos, Size) != MouseCursor.Default)
                {

                    int border = _resizeBorderThickness;

                    _isResizeLeft = mousePos.X <= border;
                    _isResizeRight = mousePos.X >= Size.X - border;
                    _isResizeTop = mousePos.Y <= border;
                    _isResizeBottom = mousePos.Y >= Size.Y - border;

                    if (_isResizeLeft || _isResizeRight || _isResizeTop || _isResizeBottom)
                    {

                        _isResizingWindow = true;
                        _initialWindowSize = Size;
                        _initialWindowLocation = Location;
                        _initialMousePositionScreen = _pointToScreen((Vector2i)mousePos);
                    }
                }
                else if (_kSpacePressed) 
                {
                    _isWindowDragging = true;
                    _windowDragOffset = new Vector2i((int)mousePos.X, (int)mousePos.Y);
                    _setCursorState(CursorState.Hidden); 
                }
                else if (_ctrlSelected)
                {
                    _selectStart = mousePos; 
                    _selectEnd = mousePos;
                    _isSelectingRectangle = true; 
                    _isFractalPanning = false; 
                    _isWindowDragging = false;

                    selectRectangle?.Invoke(true); 
                    rectanglePositionUpdated?.Invoke(_selectStart, _selectEnd);

                }
                else 
                {
                    _isFractalPanning = true;
                    _setCursor(MouseCursor.PointingHand); 
                }
            }
        }

        public void Handle_MouseMove(MouseMoveEventArgs e, Vector2 mousePos, Vector2i Size) {

            Vector2 pixelDelta = mousePos - _lastMousePos;
            _lastMousePos = mousePos;

            requestMousePosition?.Invoke(mousePos, pixelDelta);

            if (_isWindowDragging)
            {

                var newLocation = target_Location;
                newLocation.X = (int)mousePos.X - _windowDragOffset.X;
                newLocation.Y = (int)mousePos.Y - _windowDragOffset.Y;
                var screenPoint = _pointToScreen(newLocation);

                requestWindowDragging?.Invoke(screenPoint);
            }
            else if (_isFractalPanning && !_ctrlSelected)
            {
                requestPanning?.Invoke(pixelDelta);
            }
            else if (_isResizingWindow)
            {
                var screenMousePos = _pointToScreen((Vector2i)mousePos);
                Vector2 delta = screenMousePos - _initialMousePositionScreen;

                int newWidth = _initialWindowSize.X;
                int newHeight = _initialWindowSize.Y;
                int newX = _initialWindowLocation.X;
                int newY = _initialWindowLocation.Y;

                if (_isResizeLeft)
                {
                    newWidth -= (int)delta.X;
                    newX += (int)delta.X;
                }
                if (_isResizeRight)
                {
                    newWidth += (int)delta.X;
                }
                if (_isResizeTop)
                {
                    newHeight -= (int)delta.Y;
                    newY += (int)delta.Y;
                }
                if (_isResizeBottom)
                {
                    newHeight += (int)delta.Y;
                }

                requestWindowResize?.Invoke(new Vector2i(newX, newY), new Vector2i(Math.Max(100, newWidth), Math.Max(100, newHeight)));

            }
            else if (_isSelectingRectangle)
            {
                _selectEnd = mousePos;
                rectanglePositionUpdated?.Invoke(_selectStart, _selectEnd);
            }

        }

        public void Handle_MouseWheel(MouseWheelEventArgs e, Vector2 mousePos) {

            if (_isWindowDragging || _isResizingWindow) return;

            requestZooming?.Invoke(e.OffsetY, mousePos);
        }


        public void Handle_KeyDown(KeyboardKeyEventArgs e, Vector2 mousePos) {
            if (e.Key == OTK.Keys.Space)
            {
                _setCursor(MouseCursor.Empty);
                _kSpacePressed = true;
            }
            else if (e.Key == OTK.Keys.LeftControl &&
                isInside(mousePos, canvasArea))
            {

                _setCursor(MouseCursor.Crosshair);
                _ctrlSelected = true;
            }
        }

        public void Handle_KeyUp(KeyboardKeyEventArgs e, Vector2 mousePos) {
            if (e.Key == OTK.Keys.Space)
            {
                _kSpacePressed = false;
                _setCursor(_previousCursor);
            }
            else if (e.Key == OTK.Keys.LeftControl)
            {
                _ctrlSelected = false;
                _setCursor(_previousCursor);
            } 
        }

        public void UpdateCursor(Vector2 mousePos, Vector2i Size)
        {
            if (_isWindowDragging || _kSpacePressed)
            {

                _setCursorState(CursorState.Hidden);
            }
            else if (_ctrlSelected)
            {
                _setCursor(MouseCursor.Crosshair);
                _setCursorState(CursorState.Normal);
            }
            else if (_isFractalPanning)
            {
                _setCursor(MouseCursor.PointingHand);
                _setCursorState(CursorState.Normal);
            }
            else if (_isResizingWindow)
            {
                _setCursorState(CursorState.Normal);
            }
            else
            {
                _setCursor(GetResizeCursor(mousePos, Size));
                _setCursorState(CursorState.Normal);
            }
        }

        private MouseCursor GetResizeCursor(Vector2 mousePos, Vector2i Size)
        {
            
            float x = mousePos.X;
            float y = mousePos.Y;
            float width = Size.X;
            float height = Size.Y;
            int border = _resizeBorderThickness;

            bool onLeft = x <= border;
            bool onRight = x >= width - border;
            bool onTop = y <= border;
            bool onBottom = y >= height - border;

            if (onLeft && onTop) return MouseCursor.ResizeNWSE;
            else if (onRight && onTop) return MouseCursor.ResizeAll;
            else if (onLeft && onBottom) return MouseCursor.ResizeAll;
            else if (onRight && onBottom) return MouseCursor.ResizeNWSE;

            return MouseCursor.Default;
        }

        private bool isInside(Vector2 target, Vector4 Area)
        {
            return (target.X >= Area.X && target.X <= Area.X + Area.Z &&
                    target.Y >= Area.Y && target.Y <= Area.Y + Area.W);
        }

    }
}
