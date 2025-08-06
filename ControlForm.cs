using Mandelbrot;
using System;


namespace Mandelbrot
{
    internal class ControlForm : Form
    {
        private readonly Program _fractalWindow;
        private NumericUpDown _iterationsControl;
        private TrackBar _nPowerSlider;
        private Label _nPowerLabel;
        private Label _iterationsLabel;
        private Label _fpsLabel; 
        private Label _frameTimeLabel; 
        private Label _SideLength; 

        public ControlForm(Program game)
        {
            _fractalWindow = game;

            Text = "Control Panel";
            Width = 500;
            AutoScroll = true;

            _iterationsLabel = new Label { Text = "Iterations:", Dock = DockStyle.Top };
            _iterationsControl = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 10000,
                Value = _fractalWindow.MIterations,
                Dock = DockStyle.Top,
                Increment = 10,
                DecimalPlaces = 0,
                

            };
            _iterationsControl.ValueChanged += (s, e) =>
            {
                _fractalWindow.SetIterations((int)_iterationsControl.Value);
            };

            float minFloat = 0f;
            float maxFloat = 10f;
            float step = 0.1f;
            int scale = (int)(1 / step);  
            _nPowerLabel = new Label { Text = "Power (n):", Dock = DockStyle.Top };
            _nPowerSlider = new TrackBar
            {
                Minimum = (int)(minFloat * scale),
                Maximum = (int)(maxFloat * scale),
                Value = _fractalWindow.NPower * scale,
                TickFrequency = 1,
                Dock = DockStyle.Top
            };
            _nPowerSlider.ValueChanged += (s, e) =>
            {
                float floatValue = _nPowerSlider.Value / (float)scale;
                _fractalWindow.NPower = (int)floatValue;
                _nPowerLabel.Text = $"Power ({floatValue})";
            };

            _fpsLabel = new Label
            {
                Text = $"FPS: {_fractalWindow.CurrentFps:F2}",
                Dock = DockStyle.Top
            };
            _frameTimeLabel = new Label
            {
                Text = $"Frame Time: {_fractalWindow.FrameTimeMs:F2} ms",
                Dock = DockStyle.Top
            };
            _SideLength = new Label
            {
                Text = $"Side Length: {_fractalWindow.initialSideLength:F15}",
                Dock = DockStyle.Top
            };

            _fractalWindow.OnPerformanceMetricsUpdated += UpdatePerformanceLabels;
            _fractalWindow.OnSideLengthUpdated += OnSideLengthUpdated;

            Controls.Add(_frameTimeLabel);
            Controls.Add(_fpsLabel);
            Controls.Add(_nPowerSlider);
            Controls.Add(_nPowerLabel);
            Controls.Add(_iterationsControl);
            Controls.Add(_iterationsLabel);
            Controls.Add(_SideLength);
        }

        private void UpdatePerformanceLabels(float fps, float frameTimeMs)
        {

            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdatePerformanceLabels(fps, frameTimeMs)));
                return;
            }

            _fpsLabel.Text = $"FPS: {fps:F2}";
            _frameTimeLabel.Text = $"Frame Time: {frameTimeMs:F2} ms";
        }

        private void OnSideLengthUpdated(float sideLength)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnSideLengthUpdated(sideLength)));
                return;
            }
            _SideLength.Text = $"Side Length: {sideLength:F15}";
        }


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _fractalWindow.OnPerformanceMetricsUpdated -= UpdatePerformanceLabels;
            _fractalWindow.Close();
            base.OnFormClosing(e);
        }

    }
}
