using System;
using OpenTK.Mathematics;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using Fractal_Renderer;
using System.Windows.Forms;

namespace Fractal_Controller
{
    internal class ControlForm : Form
    {
        private readonly Fractal_Renderer.Program _fractalWindow;
        private string _fractalType;
        
        private NumericUpDown _iterationsControl;
        float sideLength = 3.4f;
        double sideLengthPD = 3.4;

        private Image lockIMG, unlockIMG, resetIMG, rotateRIMG, rotateLIMG, arrowRIMG, arrowLIMG;
        public bool isEditingTextBox;

        public ControlForm(Fractal_Renderer.Program game)
        {
            _fractalWindow = game;
            _fractalType = _fractalWindow._fractalType;

            _fractalWindow.OnValueLabelsUpdated += UpdateLabels;
            _fractalWindow.OnInputValueUpdated += OnInputValueUpdated;
            _fractalWindow.OnZoomUpdated += OnZoomUpdated;
            
            lockIMG = Image.FromFile("Assets/lock.png");
            unlockIMG = Image.FromFile("Assets/unlock.png");
            resetIMG = Image.FromFile("Assets/reset.png");

            rotateRIMG = Image.FromFile("Assets/rotate.png");
            rotateLIMG = (Image)rotateRIMG.Clone();
            rotateLIMG.RotateFlip(RotateFlipType.Rotate180FlipY);

            arrowRIMG = Image.FromFile("Assets/arrow.png");
            arrowLIMG = (Image)arrowRIMG.Clone();
            arrowLIMG.RotateFlip(RotateFlipType.Rotate180FlipY);

            InitializeComponent();

            this.KeyPreview = true;

            BuildControlUI();

            this.Deactivate += (s, e) =>
            {
                isEditingTextBox = false;
            };

        }

        private void UpdateLabels(string id, object value)
        {
            if (this.IsDisposed)
            {
                return;
            }

            Action updateLabels = () => {
                var label = Controls.OfType<Label>()
                                     .FirstOrDefault(lb => lb.Tag is ControlObjectAttributes attr && attr.ID == id);

                if (label == null) return;

                switch (id)
                {
                    case "fps":
                        label.Text = $"FPS: {(int)value}";
                        break;
                    case "frameTime":
                        label.Text = $"FrameTime: {(float)value:F2}ms";
                        break;
                    case "mouse":
                        label.Text = $"Coord change rate: {(float)value:F7}";
                        break;
                }
            };

            if (this.InvokeRequired)
            {
                this.Invoke(updateLabels);
            }
            else
            {
                updateLabels();
            }


        }

        private void OnInputValueUpdated(Vector3d value, string updatedType)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
            {
                return;
            }

            Action updateInputValue = () => {
                IEnumerable<TextBox> originInputControls = this.Controls
                                                            .OfType<TextBox>()
                                                            .Where(tb => tb.Tag is ControlObjectAttributes attr && attr.Type == "Origin");
                IEnumerable<TextBox> coordInputControls = this.Controls
                                                        .OfType<TextBox>()
                                                        .Where(tb => tb.Tag is ControlObjectAttributes attr && attr.Type == "Coordinates");
                IEnumerable<TextBox> rotatationControls = this.Controls
                                                        .OfType<TextBox>()
                                                        .Where(tb => tb.Tag is ControlObjectAttributes attr && attr.Type == "Rotation");

                bool isEditingPosition = false;
                foreach (TextBox axisInput in originInputControls)
                {
                    if (((ControlObjectAttributes)axisInput.Tag).IsEditable)
                    {
                        isEditingPosition = true;
                        break;
                    }
                }
                foreach (TextBox axisInput in coordInputControls)
                {
                    if (((ControlObjectAttributes)axisInput.Tag).IsEditable)
                    {
                        isEditingPosition = true;
                        break;
                    }
                }
                foreach (TextBox rotationInput in rotatationControls)
                {
                    if (((ControlObjectAttributes)rotationInput.Tag).IsEditable)
                    {
                        isEditingPosition = true;
                        break;
                    }
                }

                if (!isEditingPosition)
                {
                    if (updatedType == "Origin")
                    {

                        foreach (var input in originInputControls)
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
                    else if (updatedType == "Coordinates")
                    {

                        foreach (var input in coordInputControls)
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
                            }
                        }
                    }
                    else if (updatedType == "Rotation")
                    {
                        foreach (var input in rotatationControls)
                        {
                            input.Text = $"{value.X:F2}";
                        }
                    }
                }
            };

            if (this.InvokeRequired)
            {
                this.Invoke(updateInputValue);
            }
            else {
                updateInputValue();
            }

        }

        private void OnZoomUpdated(double newLength)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
            {
                return;
            }

            Action updateZoom = () =>
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
            };

            if (this.InvokeRequired)
            {
                this.Invoke(updateZoom);
            }
            else
            {
                updateZoom();
            }

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _fractalWindow.OnValueLabelsUpdated -= UpdateLabels;
            _fractalWindow.OnZoomUpdated -= OnZoomUpdated;
            _fractalWindow.OnInputValueUpdated -= OnInputValueUpdated;
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
            ClientSize = new Size(360, 544);
            Controls.Clear();

            AddControlUI(ControlType.UpdatingLabel, ["fps", "12", "9"]);
            AddControlUI(ControlType.UpdatingLabel, ["frameTime", "200", "9"]);
            AddControlUI(ControlType.Iterations);
            AddControlUI(ControlType.NPower);
            AddControlUI(ControlType.Zoom);


            AddControlUI(ControlType.Label, ["Origin:", "85", "138"]);
            object origX = AddControlUI(ControlType.Position, ["Origin", "x", "45", "166"]);
            AddControlUI(ControlType.FineControl, reference: origX);
            object origY = AddControlUI(ControlType.Position, ["Origin", "y", "45", "194"]);
            AddControlUI(ControlType.FineControl, reference: origY);


            AddControlUI(ControlType.Label, ["Coordinates:", "85", "222"]);
            object coordX = AddControlUI(ControlType.Position, ["Coordinates", "x", "45", "250"]);
            AddControlUI(ControlType.FineControl, reference: coordX);
            object coordY = AddControlUI(ControlType.Position, ["Coordinates", "y", "45", "278"]);
            AddControlUI(ControlType.FineControl, reference: coordY);

            AddControlUI(ControlType.RenderType, ["GPU32", "53", "320"]);
            AddControlUI(ControlType.RenderType, ["GPU64", "188", "320"]);
            AddControlUI(ControlType.ListMenu, ["ColorList", "Classic", "64", "70"]);
            AddControlUI(ControlType.ListMenu, ["FractalList", "Julia", "64", "490"]);

            AddControlUI(ControlType.UpdatingLabel, ["mouse", "100", "350"]);

            AddControlUI(ControlType.Label, ["Rotation:", "15", "385"]);
            AddControlUI(ControlType.RotateControl, ["x", "85", "385"]);

            AddControlUI(ControlType.Button, ["Reset", "Assets/reset.png", "85", "460", "left", "Reset:"]);

            ResumeLayout(true);
        }

        private void SetupUI_Mandelbrot()
        {
            SuspendLayout();
            ClientSize = new Size(360, 344);
            Controls.Clear();

            AddControlUI(ControlType.UpdatingLabel, ["fps", "12", "9"]);
            AddControlUI(ControlType.UpdatingLabel, ["frameTime", "200", "9"]);
            AddControlUI(ControlType.Iterations);
            AddControlUI(ControlType.NPower);
            AddControlUI(ControlType.Zoom);

            AddControlUI(ControlType.Label, ["Origin:", "85", "138"]);
            object origX = AddControlUI(ControlType.Position, ["Origin", "x", "45", "166"]);
            AddControlUI(ControlType.FineControl, reference: origX);
            object origY = AddControlUI(ControlType.Position, ["Origin", "y", "45", "194"]);
            AddControlUI(ControlType.FineControl, reference: origY);

            AddControlUI(ControlType.RenderType, ["GPU32", "53", "220"]);
            AddControlUI(ControlType.RenderType, ["GPU64", "188", "220"]);
            AddControlUI(ControlType.ListMenu, ["ColorList", "Classic", "64", "70"]);
            AddControlUI(ControlType.ListMenu, ["FractalList", "Julia", "64", "240"]);

            AddControlUI(ControlType.Label, ["Rotation:", "15", "275"]);
            AddControlUI(ControlType.RotateControl, ["x", "85", "275"]);

            AddControlUI(ControlType.Button, ["Reset", "Assets/reset.png", "85", "300", "left", "Reset:"]);

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

            List<Button> fractals = new List<Button>();

            foreach (FractalOptions option in Enum.GetValues(typeof(FractalOptions)))
            {
                if (option.ToString() != _fractalType) fractals.Add(new Button { Text = option.ToString()});
            }

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

        private void _updateCoordinatePos(char axis, TextBox input)
        {
            if (_fractalWindow.renderType == "GPU32")
            {
                if (float.TryParse(input.Text, out float _newPos))
                {
                    _fractalWindow.SetCoordinatePos(axis, _newPos);
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

        private void _updateRotationValue(TextBox input) {
            if (float.TryParse(input.Text, out float _newValue))
            {
                _fractalWindow.target_rotation = _newValue;
            }
        }

        private void _controlButtonEvent(string param) {

            switch (param) {
                case "Reset": _fractalWindow.resetFractalValues(); break;
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

        public void LockInput(Button btnLock, TextBox posInput, string axis)
        {
            if (btnLock.BackgroundImage == unlockIMG)
            {
                btnLock.BackgroundImage = lockIMG;
                ((ControlObjectAttributes)posInput.Tag).IsEditable = false;
                posInput.Enabled = false;
                _fractalWindow.LockInputValue(((ControlObjectAttributes)posInput.Tag).Type, axis[0], true);
            }
            else
            {
                btnLock.BackgroundImage = unlockIMG;
                ((ControlObjectAttributes)posInput.Tag).IsEditable = true;
                posInput.Enabled = true;
                _fractalWindow.LockInputValue(((ControlObjectAttributes)posInput.Tag).Type, axis[0], false);
            }
        }

        public object AddControlUI(ControlType type, string[] args = null, object reference = null) {

            switch (type) { 

                case ControlType.UpdatingLabel:
                    Label _updLabel = new Label();
                    _updLabel.Tag = new ControlObjectAttributes { ID = args[0] };
                    _updLabel.BorderStyle = BorderStyle.Fixed3D;
                    _updLabel.Location = new Point(int.Parse(args[1]), int.Parse(args[2]));
                    _updLabel.AutoSize = true;
                    _updLabel.Name = $"lb_{args[0]}";
                    Controls.Add(_updLabel);
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
                    _iterationsControl.ValueChanged += (s, e) =>
                    {
                        _iterationsControl_ValueChanged(s, e);
                        _iterationsControl.Value = _fractalWindow.MIterations;
                    };

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
                    posInput.Tag = new ControlObjectAttributes { IsEditable = false, Type = args[0], ID = args[1] };
                    posInput.Location = new Point(int.Parse(args[2]) + 58, int.Parse(args[3]));
                    posInput.Name = $"{args[0]}_{args[1]}Input";
                    posInput.Size = new Size(128, 28);
                    posInput.TabIndex = 10;
                    posInput.GotFocus += (s, e) => { isEditingTextBox = true; };
                    posInput.Leave += (s, e) => { isEditingTextBox = false; };
                    posInput.KeyPress += (s, e) => 
                    { if (e.KeyChar == '\r') 
                        { 
                            e.Handled = true;
                            if (args[0] == "Origin") _updateCenterPos(args[1][0], posInput);
                            else if (args[0] == "Coordinates") _updateCoordinatePos(args[1][0], posInput);
                        } 
                    };
                    posInput.Leave += (s, e) => { ((ControlObjectAttributes)posInput.Tag).IsEditable = false; };

                    Label posLabel = new Label();
                    posLabel.AutoSize = true;
                    posLabel.Location = new Point(int.Parse(args[2]), int.Parse(args[3]));
                    posLabel.Name = $"{args[0]}_{args[1]}ValueLabel";
                    posLabel.Size = new Size(29, 28);
                    posLabel.TabIndex = 9;
                    posLabel.Text = $"{args[1].ToUpper()} : ";

                    Button btnLockAxis = new Button();
                    btnLockAxis.BackColor = Color.Transparent;
                    btnLockAxis.BackgroundImageLayout = ImageLayout.Stretch;
                    btnLockAxis.FlatAppearance.MouseDownBackColor = Color.Transparent;
                    btnLockAxis.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    btnLockAxis.FlatStyle = FlatStyle.Flat;
                    btnLockAxis.Location = new Point(int.Parse(args[2]) + 32, int.Parse(args[3]));
                    btnLockAxis.Name = $"btnLockAxis_{args[0]}_{args[1]}";
                    btnLockAxis.Size = new Size(26, 28);
                    btnLockAxis.TabIndex = 17;
                    btnLockAxis.UseVisualStyleBackColor = false;
                    btnLockAxis.BackgroundImage = unlockIMG;
                    btnLockAxis.Click += (s, e) => LockInput(btnLockAxis, posInput, args[1]);
                    btnLockAxis.MouseEnter += (s, e) => { btnLockAxis.FlatAppearance.MouseOverBackColor = Color.PaleTurquoise; };
                    btnLockAxis.MouseLeave += (s, e) => { btnLockAxis.FlatAppearance.MouseDownBackColor = Color.Transparent; };

                    Controls.Add(posInput);
                    Controls.Add(posLabel);
                    Controls.Add(btnLockAxis);

                    return posInput;

                case ControlType.Button:

                    Button button = new Button();
                    button.BackColor = Color.LightGray;
                    button.BackgroundImageLayout = ImageLayout.Stretch;
                    button.FlatAppearance.MouseDownBackColor = Color.Transparent;
                    button.FlatAppearance.MouseOverBackColor = Color.Transparent;
                    button.FlatStyle = FlatStyle.Flat;
                    button.Location = new Point(int.Parse(args[2]), int.Parse(args[3]));
                    button.Size = new Size(26, 28);
                    if (args.Length >= 4) button.BackgroundImage = Image.FromFile(args[1]);
                    button.MouseEnter += (s, e) => { button.FlatAppearance.MouseOverBackColor = Color.PaleTurquoise; };
                    button.MouseLeave += (s, e) => { button.FlatAppearance.MouseDownBackColor = Color.Transparent; };
                    button.Click += (s, e) => { 
                        _controlButtonEvent(args[0]);
                        _iterationsControl.Value = _fractalWindow.MIterations;
                    };

                    if (args.Length >= 6) {
                        Label btnLabel = new Label();
                        btnLabel.Text = args[5];
                        btnLabel.AutoSize = true;
                        btnLabel.TextAlign = ContentAlignment.MiddleLeft;

                        string position = args[4]; 
                        int verticalCenterOffset = (button.Height - btnLabel.Height) / 2;
                        Size labelSize = TextRenderer.MeasureText(btnLabel.Text, btnLabel.Font);

                        switch (position)
                        {
                            case "left":
                                btnLabel.Location = new Point(button.Location.X - labelSize.Width, button.Location.Y + verticalCenterOffset);
                                break;
                            case "right":
                                btnLabel.Location = new Point(button.Location.X + button.Width, button.Location.Y + verticalCenterOffset);
                                break;
                        }

                        Controls.Add(btnLabel);
                    }

                    Controls.Add(button);

                    break;

                case ControlType.RotateControl:
                    int x = int.Parse(args[1]);
                    int y = int.Parse(args[2]);

                    Button rotateL = new Button();
                    rotateL.BackgroundImage = rotateLIMG;
                    rotateL.BackgroundImageLayout = ImageLayout.Stretch;
                    rotateL.Location = new Point(x, y);
                    rotateL.Size = new Size(26, 26);
                    rotateL.Click += (s, e) => {
                        _fractalWindow.target_rotation -= 10f;
                    };

                    Button rotateR = new Button();
                    rotateR.BackgroundImage = rotateRIMG;
                    rotateR.BackgroundImageLayout = ImageLayout.Stretch;
                    rotateR.Location = new Point(x + 26, y);
                    rotateR.Size = new Size(26, 26);
                    rotateR.Click += (s, e) => {
                        _fractalWindow.target_rotation += 10f;
                    };

                    TextBox rotateInput = new TextBox();
                    rotateInput.Tag = new ControlObjectAttributes { IsEditable = false, Type = "Rotation", ID = args[0] };
                    rotateInput.Size = new Size(52, 26);
                    rotateInput.Location = new Point(x + 52, y);
                    rotateInput.Text = _fractalWindow.target_rotation.ToString("F0");
                    rotateInput.GotFocus += (s, e) => { 
                        isEditingTextBox = true;
                        ((ControlObjectAttributes)rotateInput.Tag).IsEditable = true;
                    };
                    rotateInput.Leave += (s, e) => { 
                        isEditingTextBox = false;
                        ((ControlObjectAttributes)rotateInput.Tag).IsEditable = false;
                    };
                    rotateInput.KeyPress += (s, e) => {
                        if (e.KeyChar == '\r') {
                            _updateRotationValue(rotateInput);
                        }
                    };

                    Controls.Add(rotateR);
                    Controls.Add(rotateL);
                    Controls.Add(rotateInput);

                    break;

                case ControlType.FineControl:
                    TextBox tb = reference as TextBox;
                    x = tb.Location.X + 132;
                    y = tb.Location.Y;

                    TextBox stepInput = new TextBox();
                    stepInput.Tag = new ControlObjectAttributes { IsEditable = false, Type = "StepSize" };
                    stepInput.Text = "0.1";
                    stepInput.Size = new Size(52, 26);
                    stepInput.Location = new Point(x + 52, y);
                    stepInput.GotFocus += (s, e) => { isEditingTextBox = true; };
                    stepInput.Leave += (s, e) => { isEditingTextBox = false; };

                    Button arrowL = new Button();
                    arrowL.BackgroundImage = arrowLIMG;
                    arrowL.BackgroundImageLayout = ImageLayout.Stretch;
                    arrowL.Location = new Point(x, y);
                    arrowL.Size = new Size(26, 26);
                    arrowL.Click += (s, e) => {
                        tb.Invoke((MethodInvoker)delegate
                        {
                            if (double.TryParse(tb.Text, out double currentValue))
                            {
                                double stepSize = double.Parse(stepInput.Text);
                                double newValue = currentValue + stepSize;
                                tb.Text = newValue.ToString("F12");
                                if (((ControlObjectAttributes)tb.Tag).Type == "Origin") _updateCenterPos(((ControlObjectAttributes)tb.Tag).ID[0], tb);
                                else if (((ControlObjectAttributes)tb.Tag).Type == "Coordinates") _updateCoordinatePos(((ControlObjectAttributes)tb.Tag).ID[0], tb);
                            }
                        });
                    };

                    Button arrowR = new Button();
                    arrowR.BackgroundImage = arrowRIMG;
                    arrowR.BackgroundImageLayout = ImageLayout.Stretch;
                    arrowR.Location = new Point(x + 26, y);
                    arrowR.Size = new Size(26, 26);
                    arrowR.Click += (s, e) => {
                        tb.Invoke((MethodInvoker)delegate
                        {
                            if (double.TryParse(tb.Text, out double currentValue))
                            {
                                double stepSize = double.Parse(stepInput.Text);
                                double newValue = currentValue - stepSize;
                                tb.Text = newValue.ToString("F12");
                                if (((ControlObjectAttributes)tb.Tag).Type == "Origin") _updateCenterPos(((ControlObjectAttributes)tb.Tag).ID[0], tb);
                                else if (((ControlObjectAttributes)tb.Tag).Type == "Coordinates") _updateCoordinatePos(((ControlObjectAttributes)tb.Tag).ID[0], tb);
                            }
                        });
                    };

                    Controls.Add(arrowR);
                    Controls.Add(arrowL);
                    Controls.Add(stepInput);

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

                case ControlType.Label:
                    Label label = new Label();
                    label.AutoSize = true;
                    label.Location = new Point(int.Parse(args[1]), int.Parse(args[2]));
                    label.Text = args[0];
                    label.Name = $"lb{args[0]}";
                    label.TextAlign = ContentAlignment.MiddleCenter;

                    Controls.Add(label);
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
                                dropdownList.Text = "Select fractal...";
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

            return 0;
        }
    }

    public enum ControlType { 
        Fps,
        UpdatingLabel,
        Iterations,
        NPower,
        Zoom,
        Position,
        Button,
        RotateControl,
        FineControl,
        RenderType,
        ListMenu,
        Label,
    }

    public enum FractalOptions { 
        Mandelbrot,
        Julia
    }

    public class ControlObjectAttributes {
        public bool IsEditable { get; set; }
        public string Type { get; set; }
        public string ID { get; set; }
        public object ChildElement { get; set; }
    }
}
