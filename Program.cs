using System;
using System.Threading;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using System.Configuration;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Mandelbrot;

namespace Fractal_Viewer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Vector2i dimension = new Vector2i(800, 600);

            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = dimension,
                Title = "Fractal Viewer",
                WindowBorder = WindowBorder.Hidden,
                Location = new Vector2i(Screen.PrimaryScreen.WorkingArea.Width / 2 - dimension.X / 2, Screen.PrimaryScreen.WorkingArea.Height / 2 - dimension.Y / 2),
            };
            Mandelbrot.Program game = new Mandelbrot.Program(nativeWindowSettings);


            Thread formThread = new Thread(() =>
            {
                Application.Run(new ControlForm(game));
            });
            formThread.SetApartmentState(ApartmentState.STA);
            formThread.Start();
 
            game.Run();
        }
    }
}