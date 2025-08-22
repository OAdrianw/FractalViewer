using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot

{

    internal class Renderer
    {

        private Shader _shader32, _shader64, _activeShader;
        private Palette _palette;
        private Vector3[] colors;

        private int _vertexBufferObject;
        private int _vertexArrayObject;

        public Vector2 Center { get; set; }
        public Vector2d CenterPD { get; set; }
        public float SideLength = 3.4f;
        public double SideLengthPD = 3.4d;
        public float MIterations { get; set; }
        public float NPower { get; set; } = 2f;

        public bool _isSelectingRectangle { get; set; }
        public Vector2 selectBegin { get; set; }
        public Vector2 selectEnd { get; set; }

        public int StartX { get; set; } = 0;
        public int StartY { get; set; } = 0;

        private float aspectRatio;
        private float halfSideLength;
        private double halfSideLengthPD;
        public float borderWidthPx = 3.0f;
        private string renderType;

        private readonly float[] _vertices = {
            -1.0f, -1.0f, // Top Right
             1.0f, -1.0f, // Bottom Right
            -1.0f, 1.0f, // Bottom Left
             1.0f, 1.0f  // Top Left
        };

        public Renderer(string type = "GPU32")
        {
            renderType = type;
        }

        public void SetRenderType(string type, IGLFWGraphicsContext context, Vector2i Size)
        {
            context.MakeCurrent(); 
            renderType = type;
            _activeShader = renderType == "GPU32" ? _shader32 : _shader64;
            try
            {
                _activeShader.Use();

                calcCoordinates();          
                _activeShader.SetFloat("N_POWER", NPower);
                _activeShader.SetFloat("MAX_ITERATIONS", MIterations);
                colorFractal();             
                checkSelection(Size);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to switch shader: {ex.Message}");
                throw;
            }
        }



        public void OnLoad()
        {

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            GL.EnableVertexAttribArray(0);
            

            _palette = new Palette();
            _palette.Add(new Vector3(255, 255, 255));
            _palette.Add(new Vector3(255, 165, 0));
            _palette.Add(new Vector3(255, 255, 0));
            _palette.Add(new Vector3(0, 128, 0));
            _palette.Add(new Vector3(0, 0, 255));
            _palette.Add(new Vector3(128, 0, 128));

            colors = _palette.getColorsNorm();


            _shader32 = new Shader("Shaders/mandelbrot.vert", "Shaders/mandelbrot32.frag");
            _shader64 = new Shader("Shaders/mandelbrot.vert", "Shaders/mandelbrot64.frag");
            _activeShader = renderType == "GPU32" ? _shader32 : _shader64;
            _activeShader.Use();

        }

        public void Render(Vector2 Size) {

            GL.Clear(ClearBufferMask.ColorBufferBit);
            _activeShader.Use();

            aspectRatio = Size.X / Size.Y;

            if (renderType == "GPU32") {
                halfSideLength = SideLength / 2.0f;
            }
            else if (renderType == "GPU64") {

                halfSideLengthPD = SideLengthPD / 2.0d;
            }

            _activeShader.SetFloat("N_POWER", NPower);
            _activeShader.SetFloat("MAX_ITERATIONS", MIterations);

            calcCoordinates();
            checkSelection(Size);
            colorFractal();

            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }



        private void calcCoordinates()
        {

            if (renderType == "GPU32") {

                float MinX = Center.X - halfSideLength * aspectRatio;
                float MaxX = Center.X + halfSideLength * aspectRatio;
                float MinY = Center.Y - halfSideLength;
                float MaxY = Center.Y + halfSideLength;

                _activeShader.SetFloat("minx", MinX);
                _activeShader.SetFloat("maxx", MaxX);
                _activeShader.SetFloat("miny", MinY);
                _activeShader.SetFloat("maxy", MaxY);

            }else if (renderType == "GPU64") {

                double MinX = CenterPD.X - halfSideLengthPD * aspectRatio;
                double MaxX = CenterPD.X + halfSideLengthPD * aspectRatio;
                double MinY = CenterPD.Y - halfSideLengthPD;
                double MaxY = CenterPD.Y + halfSideLengthPD;

                _activeShader.SetDouble("minx", MinX);
                _activeShader.SetDouble("maxx", MaxX);
                _activeShader.SetDouble("miny", MinY);
                _activeShader.SetDouble("maxy", MaxY);
            }

        }



        private void checkSelection(Vector2 Size) {

            if (_isSelectingRectangle) {

                float rectMinX_px = Math.Min(selectBegin.X, selectEnd.X);
                float rectMaxX_px = Math.Max(selectBegin.X, selectEnd.X);
                float rectMinY_px = Math.Min(selectBegin.Y, selectEnd.Y);
                float rectMaxY_px = Math.Max(selectBegin.Y, selectEnd.Y);

                Vector2 ndcBegin = new Vector2(
                  (rectMinX_px / Size.X) * 2.0f - 1.0f,
                  1.0f - (rectMaxY_px / Size.Y) * 2.0f
                );

                Vector2 ndcEnd = new Vector2(
                  (rectMaxX_px / Size.X) * 2.0f - 1.0f,
                  1.0f - (rectMinY_px / Size.Y) * 2.0f
                );

                float borderWidthNDC_X = borderWidthPx / Size.X * 2.0f;
                float borderWidthNDC_Y = borderWidthPx / Size.Y * 2.0f;


                _activeShader.SetVector2("beginRect", ndcBegin);
                _activeShader.SetVector2("endRect", ndcEnd);

                _activeShader.SetFloat("drawRectangle", 1.0f);
                _activeShader.SetFloat("u_borderWidth", Math.Max(borderWidthNDC_X, borderWidthNDC_Y));

            } else {
                _activeShader.SetFloat("drawRectangle", 0.0f); // Don't draw rectangle

            }
        }

        private void colorFractal() {
            _activeShader.SetArray3("palette", colors.Length, colors);
            _activeShader.SetInt("palette_size", colors.Length); 
        }

        public void OnUnload()

        {
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
        }

    }


    struct Palette {

        private Vector3[] colors;
        private int maxColors = 10;
        private int index = 0;

        public Palette() {
            colors = new Vector3[maxColors];
        }

        private void CheckCapacity() {

            if (index >= colors.Length)
            {
                throw new InvalidOperationException("Color palette is full.");
            }

        }

        public void Add(Vector3 color) {

            CheckCapacity();
            colors[index] = color;
            index++;

        }

        public void Add(Vector2 color) {

            CheckCapacity();
            colors[index] = new Vector3(color);
            index++;

        }

        public void Add(float color) {

            CheckCapacity();
            colors[index] = new Vector3(color, 0.0f, 0.0f);
            index++;
        }

        public void substract() {
            index--;
        }

        public Vector3[] getColors() {
            return colors;
        }

        public Vector3[] getColorsNorm()
        {
            Vector3[] colorsNorm = new Vector3[index];
            for (int i = 0; i < index; i++) {
                colorsNorm[i].X = colors[i].X / 255.0f;
                colorsNorm[i].Y = colors[i].Y / 255.0f;
                colorsNorm[i].Z = colors[i].Z / 255.0f;
            }

            return colorsNorm;
        }

        public void clear() {
            colors = new Vector3[maxColors];
            index = 0;
        }

    }

}