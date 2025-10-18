using Mandelbrot;
using System;
using OpenTK.Mathematics;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;


namespace Mandelbrot
{
    internal class ControlForm : Form
    {
        private readonly Program _fractalWindow;
        private string _fractalType;
        
        private NumericUpDown _iterationsControl;
        float sideLength = 3.4f;
        double sideLengthPD = 3.4;

        private Image lockIMG;
        private Button btnColorPick;
        private Image unlockIMG;
        private bool isEditingTextBox;

        public ControlForm(Program game)
        {
            _fractalWindow = game;
            _fractalType = _fractalWindow._fractalType;

            _fractalWindow.OnPerformanceMetricsUpdated += UpdatePerformanceLabels;
            _fractalWindow.OnCenterUpdated += OnCenterUpdate;
            _fractalWindow.OnZoomUpdated += OnZoomUpdated;
            
            lockIMG = Image.FromFile("Assets/lock.png");
            unlockIMG = Image.FromFile("Assets/unlock.png");

            InitializeComponent();

            BuildControlUI();

            this.Deactivate += (s, e) =>
            {
                isEditingTextBox = false;
                //isEditingXPos = false;
                //isEditingYPos = false;
            };

        }

        private void UpdatePerformanceLabels(int fps, float frameTimeMs)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                this.Invoke(() =>
                {
                    Label _fpsLabel = Controls.OfType<Label>().FirstOrDefault(tb => tb.Tag is ControlObjectAttributes attr && attr.ID == "fps");
                    Label _frameTimeLabel = Controls.OfType<Label>().FirstOrDefault(tb => tb.Tag is ControlObjectAttributes attr && attr.ID == "frameTime");

                    _fpsLabel.Text = $"FPS: {fps}";
                    _frameTimeLabel.Text = $"FrameTime: {frameTimeMs:F2}ms";
                });

            }
            else
            {
                Label _fpsLabel = Controls.OfType<Label>().FirstOrDefault(tb => tb.Tag is ControlObjectAttributes attr && attr.ID == "fps");
                Label _frameTimeLabel = Controls.OfType<Label>().FirstOrDefault(tb => tb.Tag is ControlObjectAttributes attr && attr.ID == "frameTime");

                _fpsLabel.Text = $"FPS: {fps}";
                _frameTimeLabel.Text = $"FrameTime: {frameTimeMs:F2}ms";
            }


        }

        private void OnCenterUpdate(Vector3d value)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
            {
                return;
            }

            if (this.InvokeRequired)
            {
                IEnumerable<TextBox> positionInputControls = this.Controls
                                                        .OfType<TextBox>()
                                                        .Where(tb => tb.Tag is ControlObjectAttributes attr && attr.Type == "Position");

                this.Invoke(() =>
                {
                    bool isEditingPosition = false;
                    foreach (TextBox axisInput in positionInputControls)
                    {
                        if (((ControlObjectAttributes)axisInput.Tag).IsEditable)
                        {
                            isEditingPosition = true;
                            break;
                        }
                    }

                    if (!isEditingPosition)
                    {
                        foreach (var input in positionInputControls)
                        {
                            var attr = (ControlObjectAttributes)input.Tag;

                            switch (attr.ID)
                            {
                                case "x":
                                    input.Text = $"{value.X:F12}";
                                    break;
                                case "y":
                                    input.Text = $"{value.Y:F12}";
                                    break;
                                case "z":
                                    input.Text = $"{value.Z:F12}";
                                    break;
                            }
                        }
                    }
                });

            }
            else
            {
                IEnumerable<TextBox> positionInputControls = this.Controls
                                                       .OfType<TextBox>()
                                                       .Where(tb => tb.Tag is ControlObjectAttributes attr && attr.Type == "Position");
                bool isEditingPosition = false;
                foreach (TextBox axisInput in positionInputControls)
                {
                    if (((ControlObjectAttributes)axisInput.Tag).IsEditable)
                    {
                        isEditingPosition = true;
                        break;
                    }
                }

                if (!isEditingPosition)
                {
                    foreach (var input in positionInputControls)
                    {
                        var attr = (ControlObjectAttributes)input.Tag;

                        switch (attr.ID)
                        {
                            case "x":
                                input.Text = $"{value.X:F12}";
                                break;
                            case "y":
                                input.Text = $"{value.Y:F12}";
                                break;
                            case "z":
                                input.Text = $"{value.Z:F12}";
                                break;
                        }
                    }
                }
            }

        }

        private void OnZoomUpdated(double newLength)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
            {
                return;
            }


            if (this.InvokeRequired)
            {
                this.Invoke(() =>
                {
                    TextBox zoomInput = Controls.OfType<TextBox>().FirstOrDefault(tb => tb.Tag is ControlObjectAttributes attr && attr.Type == "Zoom");
                    if (zoomInput == null) return;
                    if (!((ControlObjectAttributes)zoomInput.Tag).IsEditable)
                    {
                        double value = (sideLength / newLength);

                        if (value > 10e3)
                        {
                            int exponent = (int)Math.Log10(value);
                            value = value % 10;
                            zoomInput.Text = $"{value:F2} * 10^{exponent}";
                        }
                        else
                        {
                            zoomInput.Text = $"{value:F4}";
                        }
                    }
                });

            }
            else
            {
                TextBox zoomInput = Controls.OfType<TextBox>().FirstOrDefault(tb => tb.Tag is ControlObjectAttributes attr && attr.Type == "Zoom");
                if (zoomInput == null) return;
                if (!((ControlObjectAttributes)zoomInput.Tag).IsEditable)
                {
                    double value = (sideLength / newLength);

                    if (value > 10e3)
                    {
                        int exponent = (int)Math.Log10(value);
                        value = value % 10;
                        zoomInput.Text = $"{value:F2} * 10^{exponent}";
                    }
                    else
                    {
                        zoomInput.Text = $"{value:F4}";
                    }
                }
            }

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _fractalWindow.OnPerformanceMetricsUpdated -= UpdatePerformanceLabels;
            _fractalWindow.OnZoomUpdated -= OnZoomUpdated;
            _fractalWindow.OnCenterUpdated -= OnCenterUpdate;
            _fractalWindow.Close();
            base.OnFormClosing(e);
        }

        private void InitializeComponent() { 
            SuspendLayout();

            ClientSize = new Size(360, 344);
            Text = $"Fractal Panel ({_fractalType})";
            Name = "ControlForm";

            AutoScroll = true;
            ResumeLayout(false);
        }

        private void BuildControlUI() {
            SuspendLayout();

            Text = $"Fractal Panel ({_fractalType})";

            ResumeLayout(false);

            switch (_fractalType)
            {
                case "Mandelbrot":
                    SetupUI_Mandelbrot();
                    break;
                case "Julia":
                    SetupUI_Julia();
                    break;
            }
        }

        private void SetupUI_Julia()
        {

            SuspendLayout();

            Controls.Clear();

            AddControlUI(ControlType.Fps);
            AddControlUI(ControlType.FrameTime);
            AddControlUI(ControlType.Iterations);
            AddControlUI(ControlType.NPower);
            AddControlUI(ControlType.Position, ["x", "138"]);
            AddControlUI(ControlType.Position, ["y", "166"]);
            AddControlUI(ControlType.Zoom);
            AddControlUI(ControlType.RenderType, ["GPU32", "53", "296"]);
            AddControlUI(ControlType.RenderType, ["GPU64", "188", "296"]);
            AddControlUI(ControlType.ListMenu, ["ColorList", "Classic", "64", "70"]);
            AddControlUI(ControlType.ListMenu, ["FractalList", "Julia", "64", "240"]);

            ResumeLayout(true);
        }

        private void SetupUI_Mandelbrot()
        {
            SuspendLayout();

            Controls.Clear();

            AddControlUI(ControlType.Fps);
            AddControlUI(ControlType.FrameTime);
            AddControlUI(ControlType.Iterations);
            AddControlUI(ControlType.NPower);
            AddControlUI(ControlType.Position, ["x", "138"]);
            AddControlUI(ControlType.Position, ["y", "166"]);
            AddControlUI(ControlType.Zoom);
            AddControlUI(ControlType.RenderType, ["GPU32", "53", "296"]);
            AddControlUI(ControlType.RenderType, ["GPU64", "188", "296"]);
            AddControlUI(ControlType.ListMenu, ["ColorList", "Classic", "64", "70"]);
            AddControlUI(ControlType.ListMenu, ["FractalList", "Julia", "64", "240"]);

            ResumeLayout(true);
        }

        private void _iterationsControl_ValueChanged(object sender, EventArgs e)
        {
            _fractalWindow.SetIterations((int)_iterationsControl.Value);
        }
        
        private ContextMenuStrip SetupFractalMenu(Button dropdownList) {
            ContextMenuStrip _fractalMenu = new ContextMenuStrip();
            _fractalMenu.AutoSize = false;
            _fractalMenu.Size = new Size(dropdownList.Width, 200);

            var width = dropdownList.Width;

            var fractals = new List<Button> {
                        new Button { Text = "Mandelbrot"},
                        new Button { Text = "Julia"},
            };

            foreach (var fractal in fractals)
            {
                ToolStripMenuItem item = new ToolStripMenuItem
                {
                    Text = fractal.Text,
                    BackColor = Color.DimGray,
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoSize = false,
                    Width = width - 1,
                    Height = 30,
                };

                var colour = item.BackColor;

                item.MouseEnter += (s, e) => { colour = Color.FromArgb(Math.Min(colour.R + 20, 255), Math.Min(colour.G + 20, 255), Math.Min(colour.B + 20, 255)); };
                item.MouseLeave += (s, e) => { colour = Color.FromArgb(Math.Max(colour.R - 20, 0), Math.Max(colour.G - 20, 0), Math.Max(colour.B - 20, 0)); };
                item.Click += (s, e) =>
                {
                    dropdownList.Text = item.Text;
                    _fractalWindow.changeFractal(item.Text);
                    _fractalType = item.Text;
                    BuildControlUI();
                };
                _fractalMenu.Items.Add(item);
            }
                return _fractalMenu;
        }

        private ContextMenuStrip SetupColorMenu(Button dropdownList)
        {
            ContextMenuStrip _colorMenu = new ContextMenuStrip();
            _colorMenu.AutoSize = false;
            _colorMenu.Size = new Size(dropdownList.Width, 200);

            var width = dropdownList.Width;

            var colors = new List<Button> {
                        new Button { Text = "Classic"},
                        new Button { Text = "Fiery Nebula"},
                        new Button { Text = "Arctic Ice"},
                        new Button { Text = "Psychedelic Trip"},
                        new Button { Text = "Royal Gold"},
                        new Button { Text = "Forest Floor"},
                        new Button { Text = "Grayscale"},
            };

            foreach (var color in colors) {
                ToolStripMenuItem item = new ToolStripMenuItem
                {
                    Text = color.Text,
                    BackColor = Color.DimGray,
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoSize = false,
                    Width = width - 1,
                    Height = 30,
                };
                
                var colour = item.BackColor;

                item.MouseEnter += (s, e) => { colour = Color.FromArgb(Math.Min(colour.R + 20, 255), Math.Min(colour.G + 20, 255), Math.Min(colour.B + 20, 255)); };
                item.MouseLeave += (s, e) => { colour = Color.FromArgb(Math.Max(colour.R - 20, 0), Math.Max(colour.G - 20, 0), Math.Max(colour.B - 20, 0)); };
                item.Click += (s, e) =>
                {
                    dropdownList.Text = item.Text;
                    _fractalWindow.SetColorPalette(item.Text);
                };
                _colorMenu.Items.Add(item);
            }

            return _colorMenu;

        }

        private void _zoomFactorUpdated(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;

                TextBox zoomInput = Controls.OfType<TextBox>().FirstOrDefault(tb => tb.Tag is ControlObjectAttributes attr && attr.Type == "Zoom");
                if (float.TryParse(zoomInput.Text, out float _newZoom))
                {
                    float value = sideLength / _newZoom;
                    _fractalWindow.target_SideLength = value;
                    _fractalWindow.initialSideLength = value;
                }

                ((ControlObjectAttributes)zoomInput.Tag).IsEditable = false;
                this.SelectNextControl((Control)sender, true, true, true, true);
            }
        }

        private void _updateCenterPos(char axis, TextBox input)
        {

            if (_fractalWindow.renderType == "GPU32")
            {
                if (float.TryParse(input.Text, out float _newPos))
                {
                    _fractalWindow.SetCenterPos(axis, _newPos);
                }
            }
            else if (_fractalWindow.renderType == "GPU64")
            {

                if (double.TryParse(input.Text, out double _newPos))
                {
                    _fractalWindow.SetCenterPos(axis, _newPos);
                }
            }

        }

        private void btnGPU32_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                _fractalWindow.changeRenderer("GPU32");
            }
        }

        private void btnGPU64_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                _fractalWindow.changeRenderer("GPU64");
            }
        }

        private void LockAxis(Button btnLock, TextBox posInput, string axis)
        {
            if (btnLock.BackgroundImage == unlockIMG)
            {
                btnLock.BackgroundImage = lockIMG;
                ((ControlObjectAttributes)posInput.Tag).IsEditable = false;
                posInput.Enabled = false;
                _fractalWindow.LockCenterPos(axis[0], true);
            }
            else
            {
                btnLock.BackgroundImage = unlockIMG;
                ((ControlObjectAttributes)posInput.Tag).IsEditable = true;
                posInput.Enabled = true;
                _fractalWindow.LockCenterPos(axis[0], false);
            }
        }

        public void AddControlUI(ControlType type, string[] args = null) {

            switch (type) { 
                case ControlType.Fps:
                    Label _fpsLabel = new Label();
                    _fpsLabel.Tag = new ControlObjectAttributes { ID = "fps" };
                    _fpsLabel.BorderStyle = BorderStyle.Fixed3D;
                    _fpsLabel.Location = new Point(12, 9);
                    _fpsLabel.Name = "_fpsLabel";
                    _fpsLabel.Size = new Size(68, 22);
                    _fpsLabel.TabIndex = 0;
                    _fpsLabel.Text = "FPS:    ";
                    _fpsLabel.TextAlign = ContentAlignment.TopCenter;
                    Controls.Add(_fpsLabel);
                    break;

                case ControlType.FrameTime:
                    Label _frameTimeLabel = new Label();
                    _frameTimeLabel.Tag = new ControlObjectAttributes { ID = "frameTime" };
                    _frameTimeLabel.BorderStyle = BorderStyle.Fixed3D;
                    _frameTimeLabel.Location = new Point(188, 9);
                    _frameTimeLabel.Name = "_frameTimeLabel";
                    _frameTimeLabel.Size = new Size(160, 22);
                    _frameTimeLabel.TabIndex = 1;
                    _frameTimeLabel.Text = "FrameTime:       ";
                    _frameTimeLabel.TextAlign = ContentAlignment.TopCenter;
                    Controls.Add(_frameTimeLabel);
                    break;

                case ControlType.Iterations:
                    Label _iterationsLabel = new Label();
                    _iterationsLabel.Tag = new ControlObjectAttributes { ID = "iterations" };
                    _iterationsLabel.AutoSize = true;
                    _iterationsLabel.BorderStyle = BorderStyle.FixedSingle;
                    _iterationsLabel.Location = new Point(13, 37);
                    _iterationsLabel.Name = "_iterationsLabel";
                    _iterationsLabel.Size = new Size(73, 22);
                    _iterationsLabel.TabIndex = 3;
                    _iterationsLabel.Text = "Iterations";

                    _iterationsControl = new NumericUpDown();
                    _iterationsControl.Increment = new decimal(new int[] { 16, 0, 0, 0 });
                    _iterationsControl.Location = new Point(90, 37);
                    _iterationsControl.Maximum = new decimal(new int[] { 65536, 0, 0, 0 });
                    _iterationsControl.Value = _fractalWindow.MIterations;
                    _iterationsControl.Name = "_iterationsControl";
                    _iterationsControl.Size = new Size(81, 27);
                    _iterationsControl.TabIndex = 5;
                    _iterationsControl.TextAlign = HorizontalAlignment.Center;
                    _iterationsControl.ValueChanged += _iterationsControl_ValueChanged;

                    Controls.Add(_iterationsLabel);
                    Controls.Add(_iterationsControl);
                    break;

                case ControlType.NPower:
                    Label _nPowerLabel = new Label();
                    _nPowerLabel.AutoSize = true;
                    _nPowerLabel.BorderStyle = BorderStyle.FixedSingle;
                    _nPowerLabel.Location = new Point(264, 37);
                    _nPowerLabel.Name = "_nPowerLabel";
                    _nPowerLabel.Size = new Size(84, 22);
                    _nPowerLabel.TabIndex = 4;
                    _nPowerLabel.Text = "Power(n): 2";

                    Controls.Add(_nPowerLabel);
                    break;

                case ControlType.Position:

                    TextBox posInput = new TextBox();
                    posInput.Tag = new ControlObjectAttributes { IsEditable = false, Type = "Position", ID = args[0] };
                    posInput.Location = new Point(85, int.Parse(args[1]));
                    posInput.Name = $"{args[0]}Input";
                    posInput.Size = new Size(238, 28);
                    posInput.TabIndex = 10;
                    posInput.GotFocus += (s, e) => { ((ControlObjectAttributes)posInput.Tag).IsEditable = true; };
                    posInput.KeyPress += (s, e) => { if (e.KeyChar == '\r') { e.Handled = true; _updateCenterPos('x', posInput); } };
                    posInput.Leave += (s, e) => { ((ControlObjectAttributes)posInput.Tag).IsEditable = false; };

                    Label posLabel = new Label();
                    posLabel.AutoSize = true;
                    posLabel.Location = new Point(24, int.Parse(args[1]));
                    posLabel.Name = $"{args[0]}ValueLabel";
                    posLabel.Size = new Size(29, 28);
                    posLabel.TabIndex = 9;
                    posLabel.Text = $"{args[0].ToUpper()} : ";

                    Button btnLockAxis = new Button();
                    btnLockAxis.BackColor = Color.Transparent;
                    btnLockAxis.BackgroundImageLayout = ImageLayout.Stretch;
                    btnLockAxis.FlatAppearance.MouseDownBackColor = Color.Transparent;
                    btnLockAxis.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    btnLockAxis.FlatStyle = FlatStyle.Flat;
                    btnLockAxis.Location = new Point(53, int.Parse(args[1]));
                    btnLockAxis.Name = $"btnLockAxis_{args[0]}";
                    btnLockAxis.Size = new Size(26, 28);
                    btnLockAxis.TabIndex = 17;
                    btnLockAxis.UseVisualStyleBackColor = false;
                    btnLockAxis.BackgroundImage = unlockIMG;
                    btnLockAxis.Click += (s, e) => LockAxis(btnLockAxis, posInput, args[0]);
                    btnLockAxis.MouseEnter += (s, e) => { btnLockAxis.FlatAppearance.MouseOverBackColor = Color.PaleTurquoise; };
                    btnLockAxis.MouseLeave += (s, e) => { btnLockAxis.FlatAppearance.MouseDownBackColor = Color.Transparent; };

                    Controls.Add(posInput);
                    Controls.Add(posLabel);
                    Controls.Add(btnLockAxis);
                    break;

                case ControlType.Zoom:

                    Label zoomLabel = new Label();
                    zoomLabel.AutoSize = true;
                    zoomLabel.Location = new Point(12, 108);
                    zoomLabel.Size = new Size(49, 20);
                    zoomLabel.TabIndex = 8;
                    zoomLabel.Text = "Zoom";

                    TextBox zoomInput = new TextBox();
                    zoomInput.Tag = new ControlObjectAttributes { IsEditable = false, Type = "Zoom" };
                    zoomInput.Location = new Point(67, 105);
                    zoomInput.Size = new Size(104, 27);
                    zoomInput.TabIndex = 7;
                    zoomInput.TextAlign = HorizontalAlignment.Center;

                    zoomInput.GotFocus += (s, e) => { ((ControlObjectAttributes)zoomInput.Tag).IsEditable = true; };
                    zoomInput.Validated += (s, e) => { ((ControlObjectAttributes)zoomInput.Tag).IsEditable = false; };
                    zoomInput.KeyPress += _zoomFactorUpdated;

                    Controls.Add(zoomLabel);
                    Controls.Add(zoomInput);
                    break;

                case ControlType.RenderType:
                    RadioButton btnSelector = new RadioButton();

                    btnSelector.AutoSize = true;
                    btnSelector.CheckAlign = ContentAlignment.MiddleRight;
                    btnSelector.Checked = (args[0] == _fractalWindow.renderType) ? true : false;
                    btnSelector.Location = new Point(int.Parse(args[1]), int.Parse(args[2]));
                    btnSelector.Size = new Size(74, 24);
                    btnSelector.Name = $"btn{args[0]}";
                    btnSelector.TabIndex = 12;
                    btnSelector.TabStop = true;
                    btnSelector.Text = args[0];
                    btnSelector.TextAlign = ContentAlignment.TopCenter;
                    btnSelector.UseVisualStyleBackColor = true;

                    switch (args[0]) {
                        case "GPU32": btnSelector.CheckedChanged += btnGPU32_CheckedChanged; break;
                        case "GPU64": btnSelector.CheckedChanged += btnGPU64_CheckedChanged; break;
                    }

                    Controls.Add(btnSelector);
                    break;

                case ControlType.ListMenu:

                    Button dropdownList = new Button();
                    dropdownList.Location = new Point(int.Parse(args[2]), int.Parse(args[3]));
                    dropdownList.Size = new Size(259, 29);
                    dropdownList.TabIndex = 20;
                    dropdownList.Text = args[1];
                    dropdownList.UseVisualStyleBackColor = true;

                    switch (args[0]) {
                        case "ColorList": {
                                var list = SetupColorMenu(dropdownList);
                                dropdownList.Tag = new ControlObjectAttributes { ChildElement = list};
                                dropdownList.Click += (s, e) =>
                                {
                                    if (((ControlObjectAttributes)dropdownList.Tag).ChildElement is ToolStripDropDown attachedMenu)
                                    {
                                        if (attachedMenu.Visible)
                                        {
                                            attachedMenu.Close();
                                        }
                                        else
                                        {
                                            attachedMenu.Show(dropdownList, new Point(0, dropdownList.Height));
                                        }
                                    }
                                };
                            } break;

                        case "FractalList":
                            {
                                var list = SetupFractalMenu(dropdownList);
                                dropdownList.Tag = new ControlObjectAttributes { ChildElement = list };
                                dropdownList.Click += (s, e) =>
                                {
                                    if (((ControlObjectAttributes)dropdownList.Tag).ChildElement is ToolStripDropDown attachedMenu)
                                    {
                                        if (attachedMenu.Visible)
                                        {
                                            attachedMenu.Close();
                                        }
                                        else
                                        {
                                            attachedMenu.Show(dropdownList, new Point(0, dropdownList.Height));
                                        }
                                    }
                                };
                            }
                            break;
                    }

                    Controls.Add(dropdownList);
                    break;
            }
        }
    }

    public enum ControlType { 
        Fps,
        FrameTime,
        Iterations,
        NPower,
        Zoom,
        Position,
        RenderType,
        ListMenu
    }

    public class ControlObjectAttributes {
        public bool IsEditable { get; set; }
        public string Type { get; set; }
        public string ID { get; set; }
        public object ChildElement { get; set; }
    }
}
