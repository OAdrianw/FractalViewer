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
        private Dictionary<string, Palette> _palettes;
        private string _currentPaletteName = "Classic";
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

        public Vector2 mPos;
        public Vector2d mPosPD;
        private float aspectRatio;
        private float halfSideLength;
        private double halfSideLengthPD;
        public float borderWidthPx = 3.0f;
        private string renderType;
        private string fractalType;

        private readonly float[] _vertices = {
            -1.0f, -1.0f, // Top Right
             1.0f, -1.0f, // Bottom Right
            -1.0f, 1.0f, // Bottom Left
             1.0f, 1.0f  // Top Left
        };

        public Renderer(string type = "GPU32", string fractal = "Mandelbrot")
        {
            renderType = type;
            fractalType = fractal;

            InitializePalettes();
        }

        private void InitializePalettes()
        {
            _palettes = new Dictionary<string, Palette>();

            // Classic Palette
            var classic = new Palette();
            classic.Add(new Vector3(255, 255, 255)); 
            classic.Add(new Vector3(224, 224, 224));
            classic.Add(new Vector3(192, 192, 192));
            classic.Add(new Vector3(160, 160, 160));
            classic.Add(new Vector3(128, 128, 128)); 
            classic.Add(new Vector3(96, 96, 96));
            classic.Add(new Vector3(64, 64, 64));
            classic.Add(new Vector3(32, 32, 32));
            classic.Add(new Vector3(0, 0, 0));       
            _palettes.Add("Classic", classic);

            // Fiery Nebula Palette
            var fieryNebula = new Palette();
            fieryNebula.Add(new Vector3(0, 0, 0));       
            fieryNebula.Add(new Vector3(25, 7, 26));     
            fieryNebula.Add(new Vector3(139, 0, 0));     
            fieryNebula.Add(new Vector3(255, 69, 0));   
            fieryNebula.Add(new Vector3(255, 140, 0));   
            fieryNebula.Add(new Vector3(255, 165, 0));   
            fieryNebula.Add(new Vector3(255, 215, 0));  
            fieryNebula.Add(new Vector3(255, 255, 0));   
            fieryNebula.Add(new Vector3(255, 255, 224)); 
            fieryNebula.Add(new Vector3(255, 255, 255)); 
            _palettes.Add("Fiery Nebula", fieryNebula);

            // Arctic Ice Palette
            var arcticIce = new Palette();
            arcticIce.Add(new Vector3(0, 0, 0));         
            arcticIce.Add(new Vector3(7, 25, 42));       
            arcticIce.Add(new Vector3(14, 49, 81));      
            arcticIce.Add(new Vector3(20, 74, 121));    
            arcticIce.Add(new Vector3(48, 120, 179));    
            arcticIce.Add(new Vector3(100, 164, 218));  
            arcticIce.Add(new Vector3(154, 209, 255));   
            arcticIce.Add(new Vector3(200, 230, 255));   
            arcticIce.Add(new Vector3(230, 245, 255));   
            arcticIce.Add(new Vector3(255, 255, 255));   
            _palettes.Add("Arctic Ice", arcticIce);

            // Psychedelic Trip Palette
            var psychedelicTrip = new Palette();
            psychedelicTrip.Add(new Vector3(255, 0, 255));  
            psychedelicTrip.Add(new Vector3(148, 0, 211));   
            psychedelicTrip.Add(new Vector3(75, 0, 130));    
            psychedelicTrip.Add(new Vector3(0, 0, 255));     
            psychedelicTrip.Add(new Vector3(0, 255, 0));     
            psychedelicTrip.Add(new Vector3(255, 255, 0));   
            psychedelicTrip.Add(new Vector3(255, 127, 0));   
            psychedelicTrip.Add(new Vector3(255, 0, 0));     
            _palettes.Add("Psychedelic Trip", psychedelicTrip);

            // Royal Gold Palette
            var royalGold = new Palette();
            royalGold.Add(new Vector3(19, 11, 36));      
            royalGold.Add(new Vector3(45, 20, 60));      
            royalGold.Add(new Vector3(87, 39, 87));      
            royalGold.Add(new Vector3(145, 78, 109));    
            royalGold.Add(new Vector3(203, 128, 131));   
            royalGold.Add(new Vector3(244, 181, 153));   
            royalGold.Add(new Vector3(255, 217, 169));   
            royalGold.Add(new Vector3(255, 239, 192));   
            royalGold.Add(new Vector3(240, 240, 240));   
            _palettes.Add("Royal Gold", royalGold);

            // Forest Floor Palette
            var forestFloor = new Palette();
            forestFloor.Add(new Vector3(20, 30, 10));      
            forestFloor.Add(new Vector3(34, 52, 23));     
            forestFloor.Add(new Vector3(53, 75, 34));      
            forestFloor.Add(new Vector3(87, 101, 48));     
            forestFloor.Add(new Vector3(130, 125, 64));    
            forestFloor.Add(new Vector3(101, 67, 33));     
            forestFloor.Add(new Vector3(60, 41, 20));      
            forestFloor.Add(new Vector3(30, 20, 10));      
            _palettes.Add("Forest Floor", forestFloor);

            // Grayscale Palette
            var grayscale = new Palette();
            grayscale.Add(new Vector3(0, 0, 0));
            grayscale.Add(new Vector3(32, 32, 32));
            grayscale.Add(new Vector3(64, 64, 64));
            grayscale.Add(new Vector3(96, 96, 96));
            grayscale.Add(new Vector3(128, 128, 128));
            grayscale.Add(new Vector3(160, 160, 160));
            grayscale.Add(new Vector3(192, 192, 192));
            grayscale.Add(new Vector3(224, 224, 224));
            grayscale.Add(new Vector3(255, 255, 255));
            _palettes.Add("Grayscale", grayscale);

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

        public void SetFractalType(string fractal, IGLFWGraphicsContext context) {
            context.MakeCurrent();
            fractalType = fractal;

            try
            {

                _shader32?.Dispose();
                _shader64?.Dispose();

                SetShaders();

                _activeShader = renderType == "GPU32" ? _shader32 : _shader64;
                _activeShader.Use();

                calcCoordinates();
                _activeShader.SetFloat("N_POWER", NPower);
                _activeShader.SetFloat("MAX_ITERATIONS", MIterations);
                colorFractal();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to switch shader: {ex.Message}");
                throw;
            }
        }

        public void SetShaders() {
            switch (fractalType)
            {
                case "Mandelbrot":
                    _shader32 = new Shader("Shaders/bidimensional.vert", "Shaders/mandelbrot32.frag");
                    _shader64 = new Shader("Shaders/bidimensional.vert", "Shaders/mandelbrot64.frag");
                    break;
                case "Julia":
                    _shader32 = new Shader("Shaders/bidimensional.vert", "Shaders/julia32.frag");
                    _shader64 = new Shader("Shaders/bidimensional.vert", "Shaders/julia64.frag");
                    break;
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

            colors = _palettes[_currentPaletteName].getColorsNorm();

            SetShaders();
            
            _activeShader = renderType == "GPU32" ? _shader32 : _shader64;
            _activeShader.Use();
        }

        public void Render(Vector2 Size) {

            GL.Clear(ClearBufferMask.ColorBufferBit);
            _activeShader.Use();

            aspectRatio = Size.X / Size.Y;

            if (renderType == "GPU32") {
                halfSideLength = SideLength / 2.0f;
                _activeShader.SetVector2("mousePos", mPos);
            }
            else if (renderType == "GPU64") {
                halfSideLengthPD = SideLengthPD / 2.0d;
                _activeShader.SetVector2d("mousePos", mPosPD);
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

        public void SetPalette(string paletteName)
        {
            if (_palettes.ContainsKey(paletteName))
            {
                _currentPaletteName = paletteName;
                colors = _palettes[paletteName].getColorsNorm();
                colorFractal();
            }
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