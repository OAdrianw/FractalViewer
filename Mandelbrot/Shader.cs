using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot
{
    public class Shader
    {
        public int Handle { get; private set; }

        public Shader(string vertexPath, string fragmentPath)
        {
          
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
    }
}
