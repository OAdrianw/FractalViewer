using System;
using System.Threading;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using System.Configuration;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Gma.System.MouseKeyHook;
using Fractal_Renderer;
using System.Xml.Linq;
using WFK = System.Windows.Forms.Keys;
using System.CodeDom;

namespace Fractal_Controller
{
    internal static class Program
    {
        private static IKeyboardMouseEvents _globalHook;
        private static Fractal_Renderer.Program _fractal;
        private static ControlForm _controlPanel;

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

            _fractal = new Fractal_Renderer.Program(nativeWindowSettings);
            _controlPanel = new ControlForm(_fractal);

            Thread controlThread = new Thread(() =>
            {
                _globalHook = Hook.GlobalEvents();
                _globalHook.KeyDown += GlobalHook_KeyDown;
                Application.Run(_controlPanel);
            });

            controlThread.SetApartmentState(ApartmentState.STA);
            controlThread.Start();

            _fractal.Run();

            _globalHook?.Dispose();
        }

        private static void GlobalHook_KeyDown(object sender, KeyEventArgs e) { 
            
            if (_controlPanel.isEditingTextBox) return;

            if (e.KeyCode == WFK.M)
            {
                _fractal.LockInputValue("Coordinates", 'x', _fractal.lockCoordinate[0] ? false : true);
                _fractal.LockInputValue("Coordinates", 'y', _fractal.lockCoordinate[1] ? false : true);
            }
            else if (e.KeyCode == WFK.Q)
            {
                _fractal.target_rotation -= 10f;
            }
            else if (e.KeyCode == WFK.E)
            {
                _fractal.target_rotation += 10f;
            }
            else if (e.KeyCode == WFK.Left)
            {
                if (_fractal.renderType == "GPU32")
                {

                    float displacement = _fractal.target_SideLength * 0.05f;
                    var rotatedDelta = _fractal.computeRotation(new Vector2(-1, 0));

                    _fractal.target_Center.X -= (float)rotatedDelta.X * displacement;
                    _fractal.target_Center.Y -= (float)rotatedDelta.Y * displacement;

                }
                else if (_fractal.renderType == "GPU64")
                {
                    double displacement = _fractal.target_SideLengthPD * 0.05d;
                    var rotatedDelta = _fractal.computeRotation(new Vector2(-1, 0));


                    _fractal.target_CenterPD.X -= rotatedDelta.X * displacement;
                    _fractal.target_CenterPD.Y -= rotatedDelta.Y * displacement;
                }
            }
            else if (e.KeyCode == WFK.Right)
            {
                if (_fractal.renderType == "GPU32")
                {

                    float displacement = _fractal.target_SideLength * 0.05f;
                    var rotatedDelta = _fractal.computeRotation(new Vector2(1, 0));

                    _fractal.target_Center.X -= (float)rotatedDelta.X * displacement;
                    _fractal.target_Center.Y -= (float)rotatedDelta.Y * displacement;

                }
                else if (_fractal.renderType == "GPU64")
                {
                    double displacement = _fractal.target_SideLengthPD * 0.05d;
                    var rotatedDelta = _fractal.computeRotation(new Vector2(1, 0));

                    _fractal.target_CenterPD.X -= rotatedDelta.X * displacement;
                    _fractal.target_CenterPD.Y -= rotatedDelta.Y * displacement;
                }
            }
            else if (e.KeyCode == WFK.Up)
            {
                if (_fractal.renderType == "GPU32")
                {

                    float displacement = _fractal.target_SideLength * 0.05f;
                    var rotatedDelta = _fractal.computeRotation(new Vector2(0, 1));

                    _fractal.target_Center.X -= (float)rotatedDelta.X * displacement;
                    _fractal.target_Center.Y -= (float)rotatedDelta.Y * displacement;

                }
                else if (_fractal.renderType == "GPU64")
                {
                    double displacement = _fractal.target_SideLengthPD * 0.05d;
                    var rotatedDelta = _fractal.computeRotation(new Vector2(0, 1));

                    _fractal.target_CenterPD.X -= rotatedDelta.X * displacement;
                    _fractal.target_CenterPD.Y -= rotatedDelta.Y * displacement;
                }
            }
            else if (e.KeyCode == WFK.Down)
            {
                if (_fractal.renderType == "GPU32")
                {

                    float displacement = _fractal.target_SideLength * 0.05f;
                    var rotatedDelta = _fractal.computeRotation(new Vector2(0, -1));

                    _fractal.target_Center.X -= (float)rotatedDelta.X * displacement;
                    _fractal.target_Center.Y -= (float)rotatedDelta.Y * displacement;

                }
                else if (_fractal.renderType == "GPU64")
                {
                    double displacement = _fractal.target_SideLengthPD * 0.05d;
                    var rotatedDelta = _fractal.computeRotation(new Vector2(0, -1));

                    _fractal.target_CenterPD.X -= rotatedDelta.X * displacement;
                    _fractal.target_CenterPD.Y -= rotatedDelta.Y * displacement;
                }
            }
            else if (e.KeyCode == WFK.T)
            {
                _fractal.julia_changeRate /= 2.0f;
            }
            else if (e.KeyCode == WFK.Y)
            {
                _fractal.julia_changeRate *= 2.0f;
            }
            else if (e.KeyCode == WFK.R) {
                _fractal.resetFractalValues();
            }

            e.Handled = true;
        }
    }
}