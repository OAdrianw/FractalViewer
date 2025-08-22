using Mandelbrot;
using System;
using OpenTK.Mathematics;
using System.ComponentModel;


namespace Mandelbrot
{
    internal class ControlForm : Form
    {
        private readonly Program _fractalWindow;
        private Label _fpsLabel;
        private Label _frameTimeLabel;
        private Label _nPowerLabel;
        private Label _iterationsLabel;
        private TextBox zoomInput;
        private bool isEditingTextBox = false;
        private bool isEditingXPos = false;
        private bool isEditingYPos = false;
        private bool isEditingZPos = false;
        private Label zoomLabel;
        private NumericUpDown _iterationsControl;
        float sideLength = 3.4f;
        double sideLengthPD = 3.4;
        private Label xValueLabel;
        private TextBox xInput;
        private TextBox yInput;
        private Label yValueLabel;
        private TextBox zInput;
        private Label zValueLabel;
        private RadioButton btnGPU32;
        private Button btnLockPosX;
        private Button btnLockPosY;
        private Button btnLockPosZ;
        private RadioButton btnGPU64;

        private Image lockIMG;
        private Image unlockIMG;

        public ControlForm(Program game)
        {
            _fractalWindow = game;

            Text = "Control Panel";
            Width = 500;
            AutoScroll = true;

            _fractalWindow.OnPerformanceMetricsUpdated += UpdatePerformanceLabels;
            _fractalWindow.OnZoomUpdated += OnZoomUpdated;
            _fractalWindow.OnCenterUpdated += OnCenterUpdate;

            InitializeComponent();

            _iterationsControl.Value = _fractalWindow.MIterations;

            zoomInput.GotFocus += (sender, e) => { isEditingTextBox = true; };
            zoomInput.Validated += (sender, e) => { isEditingTextBox = false; };
            zoomInput.KeyPress += _zoomFactorUpdated;

            xInput.GotFocus += (s, e) => { isEditingXPos = true; };
            xInput.KeyPress += (s, e) => { if (e.KeyChar == '\r') { e.Handled = true; _updateCenterPos('x', xInput, ref isEditingXPos); } };
            xInput.Leave += (s, e) => { isEditingXPos = false; };

            yInput.GotFocus += (s, e) => { isEditingYPos = true; };
            yInput.KeyPress += (s, e) => { if (e.KeyChar == '\r') { e.Handled = true; _updateCenterPos('y', yInput, ref isEditingYPos); } };
            yInput.Leave += (s, e) => { isEditingYPos = false; };


            lockIMG = Image.FromFile("Assets/lock.png");
            unlockIMG = Image.FromFile("Assets/unlock.png");

            btnLockPosX.BackgroundImage = unlockIMG;
            btnLockPosX.MouseEnter += (s, e) => { btnLockPosX.FlatAppearance.MouseOverBackColor = Color.PaleTurquoise; };
            btnLockPosX.MouseLeave += (s, e) => { btnLockPosX.FlatAppearance.MouseDownBackColor = Color.Transparent; };

            btnLockPosY.BackgroundImage = unlockIMG;
            btnLockPosY.MouseEnter += (s, e) => { btnLockPosY.FlatAppearance.MouseOverBackColor = Color.PaleTurquoise; };
            btnLockPosY.MouseLeave += (s, e) => { btnLockPosY.FlatAppearance.MouseDownBackColor = Color.Transparent; };

            btnLockPosZ.BackgroundImage = unlockIMG;
            btnLockPosZ.MouseEnter += (s, e) => { btnLockPosZ.FlatAppearance.MouseOverBackColor = Color.PaleTurquoise; };
            btnLockPosZ.MouseLeave += (s, e) => { btnLockPosZ.FlatAppearance.MouseDownBackColor = Color.Transparent; };

            this.Deactivate += (s, e) =>
            {
                isEditingTextBox = false;
                isEditingXPos = false;
                isEditingYPos = false;
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
                    _fpsLabel.Text = $"FPS: {fps}";
                    _frameTimeLabel.Text = $"FrameTime: {frameTimeMs:F2}ms";
                });

            }
            else
            {
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
                this.Invoke(() =>
                {
                    if (!isEditingXPos && !isEditingYPos && !isEditingZPos)
                    {
                        xInput.Text = $"{value.X:F12}";
                        yInput.Text = $"{value.Y:F12}";
                        zInput.Text = $"0.00";
                    }
                });

            }
            else
            {
                if (!isEditingXPos && !isEditingYPos && !isEditingZPos)
                {
                    xInput.Text = $"{value.X:F12}";
                    yInput.Text = $"{value.Y:F12}";
                    zInput.Text = $"0.00";
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
                    if (!isEditingTextBox)
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
                if (!isEditingTextBox)
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

        private void InitializeComponent()
        {
            _fpsLabel = new Label();
            _frameTimeLabel = new Label();
            _iterationsLabel = new Label();
            _nPowerLabel = new Label();
            _iterationsControl = new NumericUpDown();
            zoomInput = new TextBox();
            zoomLabel = new Label();
            xValueLabel = new Label();
            xInput = new TextBox();
            yInput = new TextBox();
            btnGPU32 = new RadioButton();
            btnGPU64 = new RadioButton();
            yValueLabel = new Label();
            zValueLabel = new Label();
            zInput = new TextBox();
            btnLockPosX = new Button();
            btnLockPosY = new Button();
            btnLockPosZ = new Button();
            ((ISupportInitialize)_iterationsControl).BeginInit();
            SuspendLayout();
            // 
            // _fpsLabel
            // 
            _fpsLabel.BorderStyle = BorderStyle.Fixed3D;
            _fpsLabel.Location = new Point(12, 9);
            _fpsLabel.Name = "_fpsLabel";
            _fpsLabel.Size = new Size(68, 22);
            _fpsLabel.TabIndex = 0;
            _fpsLabel.Text = "FPS:    ";
            _fpsLabel.TextAlign = ContentAlignment.TopCenter;
            // 
            // _frameTimeLabel
            // 
            _frameTimeLabel.BorderStyle = BorderStyle.Fixed3D;
            _frameTimeLabel.Location = new Point(188, 9);
            _frameTimeLabel.Name = "_frameTimeLabel";
            _frameTimeLabel.Size = new Size(160, 22);
            _frameTimeLabel.TabIndex = 1;
            _frameTimeLabel.Text = "FrameTime:       ";
            _frameTimeLabel.TextAlign = ContentAlignment.TopCenter;
            // 
            // _iterationsLabel
            // 
            _iterationsLabel.AutoSize = true;
            _iterationsLabel.BorderStyle = BorderStyle.FixedSingle;
            _iterationsLabel.Location = new Point(13, 37);
            _iterationsLabel.Name = "_iterationsLabel";
            _iterationsLabel.Size = new Size(73, 22);
            _iterationsLabel.TabIndex = 3;
            _iterationsLabel.Text = "Iterations";
            // 
            // _nPowerLabel
            // 
            _nPowerLabel.AutoSize = true;
            _nPowerLabel.BorderStyle = BorderStyle.FixedSingle;
            _nPowerLabel.Location = new Point(264, 37);
            _nPowerLabel.Name = "_nPowerLabel";
            _nPowerLabel.Size = new Size(84, 22);
            _nPowerLabel.TabIndex = 4;
            _nPowerLabel.Text = "Power(n): 2";
            // 
            // _iterationsControl
            // 
            _iterationsControl.Increment = new decimal(new int[] { 16, 0, 0, 0 });
            _iterationsControl.Location = new Point(90, 37);
            _iterationsControl.Maximum = new decimal(new int[] { 65536, 0, 0, 0 });
            _iterationsControl.Name = "_iterationsControl";
            _iterationsControl.Size = new Size(81, 27);
            _iterationsControl.TabIndex = 5;
            _iterationsControl.TextAlign = HorizontalAlignment.Center;
            _iterationsControl.ValueChanged += _iterationsControl_ValueChanged;
            // 
            // zoomInput
            // 
            zoomInput.Location = new Point(67, 105);
            zoomInput.Name = "zoomInput";
            zoomInput.Size = new Size(104, 27);
            zoomInput.TabIndex = 7;
            zoomInput.TextAlign = HorizontalAlignment.Center;
            // 
            // zoomLabel
            // 
            zoomLabel.AutoSize = true;
            zoomLabel.Location = new Point(12, 108);
            zoomLabel.Name = "zoomLabel";
            zoomLabel.Size = new Size(49, 20);
            zoomLabel.TabIndex = 8;
            zoomLabel.Text = "Zoom";
            // 
            // xValueLabel
            // 
            xValueLabel.AutoSize = true;
            xValueLabel.Location = new Point(24, 142);
            xValueLabel.Name = "xValueLabel";
            xValueLabel.Size = new Size(29, 20);
            xValueLabel.TabIndex = 9;
            xValueLabel.Text = "X : ";
            // 
            // xInput
            // 
            xInput.Location = new Point(85, 139);
            xInput.Name = "xInput";
            xInput.Size = new Size(238, 27);
            xInput.TabIndex = 10;
            // 
            // yInput
            // 
            yInput.Location = new Point(85, 170);
            yInput.Name = "yInput";
            yInput.Size = new Size(238, 27);
            yInput.TabIndex = 11;
            // 
            // btnGPU32
            // 
            btnGPU32.AutoSize = true;
            btnGPU32.CheckAlign = ContentAlignment.MiddleRight;
            btnGPU32.Checked = true;
            btnGPU32.Location = new Point(53, 296);
            btnGPU32.Name = "btnGPU32";
            btnGPU32.Size = new Size(74, 24);
            btnGPU32.TabIndex = 12;
            btnGPU32.TabStop = true;
            btnGPU32.Text = "GPU32";
            btnGPU32.TextAlign = ContentAlignment.TopCenter;
            btnGPU32.UseVisualStyleBackColor = true;
            btnGPU32.CheckedChanged += BtnGPU32_CheckedChanged;
            // 
            // btnGPU64
            // 
            btnGPU64.AutoSize = true;
            btnGPU64.CheckAlign = ContentAlignment.MiddleRight;
            btnGPU64.Location = new Point(188, 296);
            btnGPU64.Name = "btnGPU64";
            btnGPU64.Size = new Size(74, 24);
            btnGPU64.TabIndex = 13;
            btnGPU64.Text = "GPU64";
            btnGPU64.TextAlign = ContentAlignment.TopCenter;
            btnGPU64.UseVisualStyleBackColor = true;
            btnGPU64.CheckedChanged += btnGPU64_CheckedChanged;
            // 
            // yValueLabel
            // 
            yValueLabel.AutoSize = true;
            yValueLabel.Location = new Point(24, 174);
            yValueLabel.Name = "yValueLabel";
            yValueLabel.Size = new Size(28, 20);
            yValueLabel.TabIndex = 14;
            yValueLabel.Text = "Y : ";
            // 
            // zValueLabel
            // 
            zValueLabel.AutoSize = true;
            zValueLabel.Enabled = false;
            zValueLabel.Location = new Point(24, 207);
            zValueLabel.Name = "zValueLabel";
            zValueLabel.Size = new Size(29, 20);
            zValueLabel.TabIndex = 15;
            zValueLabel.Text = "Z : ";
            // 
            // zInput
            // 
            zInput.Enabled = false;
            zInput.Location = new Point(85, 203);
            zInput.Name = "zInput";
            zInput.Size = new Size(238, 27);
            zInput.TabIndex = 16;
            // 
            // btnLockPosX
            // 
            btnLockPosX.BackColor = Color.Transparent;
            btnLockPosX.BackgroundImageLayout = ImageLayout.Stretch;
            btnLockPosX.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnLockPosX.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnLockPosX.FlatStyle = FlatStyle.Flat;
            btnLockPosX.Location = new Point(53, 138);
            btnLockPosX.Name = "btnLockPosX";
            btnLockPosX.Size = new Size(26, 29);
            btnLockPosX.TabIndex = 17;
            btnLockPosX.UseVisualStyleBackColor = false;
            btnLockPosX.Click += btnLockPosX_Click;
            // 
            // btnLockPosY
            // 
            btnLockPosY.BackColor = Color.Transparent;
            btnLockPosY.BackgroundImageLayout = ImageLayout.Stretch;
            btnLockPosY.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnLockPosY.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnLockPosY.FlatStyle = FlatStyle.Flat;
            btnLockPosY.Location = new Point(53, 170);
            btnLockPosY.Name = "btnLockPosY";
            btnLockPosY.Size = new Size(26, 29);
            btnLockPosY.TabIndex = 18;
            btnLockPosY.UseVisualStyleBackColor = false;
            btnLockPosY.Click += btnLockPosY_Click;
            // 
            // btnLockPosZ
            // 
            btnLockPosZ.BackColor = Color.Transparent;
            btnLockPosZ.BackgroundImageLayout = ImageLayout.Stretch;
            btnLockPosZ.Enabled = false;
            btnLockPosZ.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnLockPosZ.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnLockPosZ.FlatStyle = FlatStyle.Flat;
            btnLockPosZ.Location = new Point(53, 203);
            btnLockPosZ.Name = "btnLockPosZ";
            btnLockPosZ.Size = new Size(26, 29);
            btnLockPosZ.TabIndex = 19;
            btnLockPosZ.UseVisualStyleBackColor = false;
            // 
            // ControlForm
            // 
            AutoValidate = AutoValidate.Disable;
            ClientSize = new Size(360, 344);
            Controls.Add(btnLockPosZ);
            Controls.Add(btnLockPosY);
            Controls.Add(btnLockPosX);
            Controls.Add(zInput);
            Controls.Add(zValueLabel);
            Controls.Add(yValueLabel);
            Controls.Add(btnGPU64);
            Controls.Add(btnGPU32);
            Controls.Add(yInput);
            Controls.Add(xInput);
            Controls.Add(xValueLabel);
            Controls.Add(zoomLabel);
            Controls.Add(zoomInput);
            Controls.Add(_iterationsControl);
            Controls.Add(_nPowerLabel);
            Controls.Add(_iterationsLabel);
            Controls.Add(_frameTimeLabel);
            Controls.Add(_fpsLabel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "ControlForm";
            Text = "xzq";
            ((ISupportInitialize)_iterationsControl).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private void _iterationsControl_ValueChanged(object sender, EventArgs e)
        {
            _fractalWindow.SetIterations((int)_iterationsControl.Value);
        }

        private void _zoomFactorUpdated(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;

                if (float.TryParse(zoomInput.Text, out float _newZoom))
                {
                    float value = sideLength / _newZoom;
                    _fractalWindow.target_SideLength = value;
                    _fractalWindow.initialSideLength = value;
                }

                isEditingTextBox = false;
                this.SelectNextControl((Control)sender, true, true, true, true);
            }
        }

        private void _updateCenterPos(char axis, TextBox input, ref bool isEditingValue)
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

        private void BtnGPU32_CheckedChanged(object sender, EventArgs e)
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

        private void btnLockPosX_Click(object sender, EventArgs e)
        {
            if (btnLockPosX.BackgroundImage == unlockIMG)
            {
                btnLockPosX.BackgroundImage = lockIMG;
                isEditingXPos = false;
                xInput.Enabled = false;
                _fractalWindow.LockCenterPos('x', true);
            }
            else
            {
                btnLockPosX.BackgroundImage = unlockIMG;
                isEditingXPos = true;
                xInput.Enabled = true;
                _fractalWindow.LockCenterPos('x', false);
            }
        }

        private void btnLockPosY_Click(object sender, EventArgs e)
        {
            if (btnLockPosY.BackgroundImage == unlockIMG)
            {
                btnLockPosY.BackgroundImage = lockIMG;
                isEditingYPos = false;
                yInput.Enabled = false;
                _fractalWindow.LockCenterPos('y', true);
            }
            else
            {
                btnLockPosY.BackgroundImage = unlockIMG;
                isEditingYPos = true;
                yInput.Enabled = true;
                _fractalWindow.LockCenterPos('y', false);
            }
        }
    }
}
