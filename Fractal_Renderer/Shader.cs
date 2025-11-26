using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata;

namespace Fractal_Renderer
{
    public class Shader
    {
        public int Handle { get; private set; }

        public Shader(string fractalType, string rendererType)
        {
            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            string[] shader;
            shader = ComposeShader(fractalType, rendererType);
            string vertexPath = shader[0];
            string fragmentPath = shader[1];

            string vertexShaderSource = File.ReadAllText(vertexPath);
            string fragmentShaderSource = File.ReadAllText(fragmentPath);

            
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            CheckCompileErrors(vertexShader, "VERTEX"); 

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            CheckCompileErrors(fragmentShader, "FRAGMENT"); 


            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            GL.LinkProgram(Handle);

            CheckCompileErrors(Handle, "PROGRAM"); 

            

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }

        private void CheckCompileErrors(int shader, string type)
        {
            int success;
            if (type != "PROGRAM")
            {
                GL.GetShader(shader, ShaderParameter.CompileStatus, out success);
                if (success == 0)
                {
                    string infoLog = GL.GetShaderInfoLog(shader);
                    Console.WriteLine($"ERROR::SHADER_COMPILATION_ERROR of type: {type}\n{infoLog}");
                }
            }
            else
            {
                GL.GetProgram(shader, GetProgramParameterName.LinkStatus, out success);
                if (success == 0)
                {
                    string infoLog = GL.GetProgramInfoLog(shader);
                    Console.WriteLine($"ERROR::PROGRAM_LINKING_ERROR of type: {type}\n{infoLog}");
                }
            }
        }


        // --- Uniform Setters ---
        /// <summary>
        /// Sets an integer uniform variable in the shader program.
        /// </summary>
        public void SetInt(string name, int value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, value);
            
        }

        /// <summary>
        /// Sets a float uniform variable in the shader program.
        /// </summary>
        public void SetFloat(string name, float value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, value);
        }

        /// <summary>
        /// Sets a double uniform variable in the shader program.
        /// </summary>
        public void SetDouble(string name, double value)
        { 
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, value);
        }

        /// <summary>
        /// Sets a Vector2 uniform variable in the shader program.
        /// </summary>
        public void SetVector2(string name, Vector2 value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform2(location, value);
        }

        /// <summary>
        /// Sets a Vector2d uniform variable in the shader program.
        /// </summary>
        public void SetVector2d(string name, Vector2d value) 
        { 
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform2(location, value.X, value.Y);
        }

        /// <summary>
        /// Sets a Vector3 uniform variable in the shader program.
        /// </summary>
        public void SetVector3(string name, Vector3 value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform3(location, value);
        }

        /// <summary>
        /// Sets a Vector4 uniform variable in the shader program.
        /// </summary>
        public void SetVector4(string name, Vector4 value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform4(location, value);
        }

        public void SetArray3(string name, int length, Vector3[] array)
        {
            int location = GL.GetUniformLocation(Handle, $"{name}[0]");
            if (location == -1) return;

            float[] floatArray = new float[array.Length * 3];
            for (int i = 0; i < array.Length; i++)
            {
                floatArray[i * 3] = array[i].X;
                floatArray[i * 3 + 1] = array[i].Y;
                floatArray[i * 3 + 2] = array[i].Z;
            }

            GL.Uniform3(location, length, floatArray);
        }

        private string[] ComposeShader(string fractalType, string renderType) {
            Console.WriteLine($"Current Working Directory: {Directory.GetCurrentDirectory()}");
            string frag = $"Shaders/{fractalType}{renderType}Shader.frag";

            File.WriteAllText(frag, string.Empty);

            Encoding encoding = Encoding.UTF8;

            string variablesPath = "Shaders/variables.txt";
            string extraPath = "Shaders/extra.txt";
            string coloringPath = "Shaders/coloring.txt";
            string fractal_core_path = string.Empty;
            string fractal_main = string.Empty;

            string variables = File.ReadAllText(variablesPath, encoding);

            if (renderType == "GPU32")
            {
                variables += "\nuniform float minx;\nuniform float maxx;\nuniform float miny;\nuniform float maxy;\n";
                if (fractalType == "Julia") variables += "uniform vec2 mousePos;";
            }
            else {
                variables += "\nuniform double minx;\nuniform double maxx;\nuniform double miny;\nuniform double maxy;\n";
                if (fractalType == "Julia") variables += "uniform dvec2 mousePos;";
            }
            string extra = File.ReadAllText(extraPath, encoding);
            string coloring = File.ReadAllText(coloringPath);

            if (fractalType == "Mandelbrot")
            {
                fractal_core_path = "Shaders/Mandelbrot.txt";
            }
            else if (fractalType == "Julia") {
                fractal_core_path = "Shaders/Julia.txt";
            }
            string fractal_core = File.ReadAllText(fractal_core_path, encoding);

            fractal_main += "void main() {\n"
                              + ((renderType == "GPU32") ?
                              "    float x_interp = (vPos.x + 1.0) / 2.0; \n" +
                              "    float y_interp = (vPos.y + 1.0) / 2.0; \n" +
                              "    float x_coord  = mix(minx, maxx, x_interp); \n" +
                              "    float y_coord  = mix(miny, maxy, y_interp); \n" +
                              "    vec2 coord = vec2(x_coord, y_coord);\n" +
                              "    vec2 u_center = vec2((minx + maxx) / 2.0, (miny + maxy) / 2.0);\n" :

                              "    double x_interp = (vPos.x + 1.0) / 2.0;\n    " +
                              "    double y_interp = (vPos.y + 1.0) / 2.0;\n    " +
                              "    double x_coord  = mix(minx, maxx, x_interp);\n    " +
                              "    double y_coord  = mix(miny, maxy, y_interp); \n" +
                              "    dvec2 coord = dvec2(x_coord, y_coord);\n" +
                              "    dvec2 u_center = dvec2((minx + maxx) / 2.0, (miny + maxy) / 2.0);\n") +

                              "    coord = applyRotation(coord, u_center, rotation_angle);\n" +
                              "    float i = " + "iterate" + fractalType + renderType + "_optimized(coord);\n" +
                              "    vec4 color;\n" +
                              "    color = colorFractal(i);\n" +
                              "    color += drawSelection();\n"+
                              "    FragColor = color;\n"
                              + "}\n";

            string finalFragmentSource = variables + extra + coloring + fractal_core + fractal_main;
            File.WriteAllText(frag, finalFragmentSource, encoding);

            return new string[] { "Shaders/bidimensional.vert", frag};
        }
    }
}
