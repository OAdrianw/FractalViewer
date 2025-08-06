using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
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
        private Shader _shader;

        private int _vertexBufferObject;
        private int _vertexArrayObject;

        public Vector2 Center { get; set; }
        public float SideLength = 3.4f;
        public float MIterations { get; set; } = 1500;
        public float NPower { get; set; } = 2f;

        public bool _isSelectingRectangle { get; set; }
        public Vector2 selectBegin { get; set; }
        public Vector2 selectEnd { get; set; }

        public int StartX { get; set; } = 0;
        public int StartY { get; set; } = 0;

        private float aspectRatio;
        private float halfSideLength;
        public float borderWidthPx = 3.0f;

        private readonly float[] _vertices = {
            -1.0f, -1.0f, // Top Right
             1.0f, -1.0f, // Bottom Right
            -1.0f,  1.0f, // Bottom Left
             1.0f,  1.0f  // Top Left
        };

        public Renderer() {
        }
        
        public void OnLoad() {  
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();
        }

        public void Render(Vector2i Size)
        {

            GL.Clear(ClearBufferMask.ColorBufferBit);
            _shader.Use();

            aspectRatio = Size.X / (float)Size.Y;
            halfSideLength = SideLength / 2.0f;

            calcCoordinates();

            _shader.SetFloat("N_POWER", NPower);
            _shader.SetFloat("MAX_ITERATIONS", MIterations);

            checkSelection(Size);

            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

        }

        private void calcCoordinates() {
            float MinX = Center.X - halfSideLength * aspectRatio;
            float MaxX = Center.X + halfSideLength * aspectRatio;
            float MinY = Center.Y - halfSideLength;
            float MaxY = Center.Y + halfSideLength;

            _shader.SetFloat("minx", MinX);
            _shader.SetFloat("maxx", MaxX);
            _shader.SetFloat("miny", MinY);
            _shader.SetFloat("maxy", MaxY);
        }

        private void checkSelection(Vector2i Size)
        {
            if (_isSelectingRectangle)
            {

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

                _shader.SetVector2("beginRect", ndcBegin);
                _shader.SetVector2("endRect", ndcEnd);
                _shader.SetFloat("drawRectangle", 1.0f);

                float borderWidthNDC_X = borderWidthPx / Size.X * 2.0f;
                float borderWidthNDC_Y = borderWidthPx / Size.Y * 2.0f;

                _shader.SetFloat("u_borderWidth", Math.Max(borderWidthNDC_X, borderWidthNDC_Y));
            }
            else
            {
                _shader.SetFloat("drawRectangle", 0.0f); // Don't draw rectangle
            }

        }

        public void OnUnload()
        {
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
        }

    }
}
