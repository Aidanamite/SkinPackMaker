
namespace SkinPackMaker
{
    partial class ColorSelector
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColorSelector));
            this.RPointer = new System.Windows.Forms.PictureBox();
            this.RInput = new System.Windows.Forms.TextBox();
            this.GInput = new System.Windows.Forms.TextBox();
            this.GPointer = new System.Windows.Forms.PictureBox();
            this.BInput = new System.Windows.Forms.TextBox();
            this.BPointer = new System.Windows.Forms.PictureBox();
            this.HInput = new System.Windows.Forms.TextBox();
            this.HPointer = new System.Windows.Forms.PictureBox();
            this.SInput = new System.Windows.Forms.TextBox();
            this.SPointer = new System.Windows.Forms.PictureBox();
            this.LInput = new System.Windows.Forms.TextBox();
            this.LPointer = new System.Windows.Forms.PictureBox();
            this.AInput = new System.Windows.Forms.TextBox();
            this.APointer = new System.Windows.Forms.PictureBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.HexInput = new System.Windows.Forms.TextBox();
            this.ResultDisplay = new SkinPackMaker.CustomPictureBox();
            this.ASlider = new SkinPackMaker.CustomPictureBox();
            this.LSlider = new SkinPackMaker.CustomPictureBox();
            this.SSlider = new SkinPackMaker.CustomPictureBox();
            this.HSlider = new SkinPackMaker.CustomPictureBox();
            this.BSlider = new SkinPackMaker.CustomPictureBox();
            this.GSlider = new SkinPackMaker.CustomPictureBox();
            this.RSlider = new SkinPackMaker.CustomPictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.RPointer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GPointer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BPointer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.HPointer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SPointer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LPointer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.APointer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ResultDisplay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ASlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.HSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RSlider)).BeginInit();
            this.SuspendLayout();
            // 
            // RPointer
            // 
            this.RPointer.Image = ((System.Drawing.Image)(resources.GetObject("RPointer.Image")));
            this.RPointer.InitialImage = null;
            this.RPointer.Location = new System.Drawing.Point(9, 8);
            this.RPointer.Name = "RPointer";
            this.RPointer.Size = new System.Drawing.Size(12, 8);
            this.RPointer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.RPointer.TabIndex = 2;
            this.RPointer.TabStop = false;
            this.RPointer.Tag = "R";
            // 
            // RInput
            // 
            this.RInput.Location = new System.Drawing.Point(7, 146);
            this.RInput.MaxLength = 3;
            this.RInput.Name = "RInput";
            this.RInput.Size = new System.Drawing.Size(31, 20);
            this.RInput.TabIndex = 3;
            this.RInput.Tag = "R";
            this.RInput.Text = "255";
            this.RInput.TextChanged += new System.EventHandler(this.SliderInputChanged);
            this.RInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValidateSliderInput);
            // 
            // GInput
            // 
            this.GInput.Location = new System.Drawing.Point(44, 146);
            this.GInput.MaxLength = 3;
            this.GInput.Name = "GInput";
            this.GInput.Size = new System.Drawing.Size(31, 20);
            this.GInput.TabIndex = 6;
            this.GInput.Tag = "G";
            this.GInput.Text = "255";
            this.GInput.TextChanged += new System.EventHandler(this.SliderInputChanged);
            this.GInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValidateSliderInput);
            // 
            // GPointer
            // 
            this.GPointer.Image = ((System.Drawing.Image)(resources.GetObject("GPointer.Image")));
            this.GPointer.InitialImage = null;
            this.GPointer.Location = new System.Drawing.Point(46, 8);
            this.GPointer.Name = "GPointer";
            this.GPointer.Size = new System.Drawing.Size(12, 8);
            this.GPointer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.GPointer.TabIndex = 5;
            this.GPointer.TabStop = false;
            this.GPointer.Tag = "G";
            // 
            // BInput
            // 
            this.BInput.Location = new System.Drawing.Point(81, 146);
            this.BInput.MaxLength = 3;
            this.BInput.Name = "BInput";
            this.BInput.Size = new System.Drawing.Size(31, 20);
            this.BInput.TabIndex = 9;
            this.BInput.Tag = "B";
            this.BInput.Text = "255";
            this.BInput.TextChanged += new System.EventHandler(this.SliderInputChanged);
            this.BInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValidateSliderInput);
            // 
            // BPointer
            // 
            this.BPointer.Image = ((System.Drawing.Image)(resources.GetObject("BPointer.Image")));
            this.BPointer.InitialImage = null;
            this.BPointer.Location = new System.Drawing.Point(83, 8);
            this.BPointer.Name = "BPointer";
            this.BPointer.Size = new System.Drawing.Size(12, 8);
            this.BPointer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.BPointer.TabIndex = 8;
            this.BPointer.TabStop = false;
            this.BPointer.Tag = "B";
            // 
            // HInput
            // 
            this.HInput.Location = new System.Drawing.Point(118, 146);
            this.HInput.MaxLength = 3;
            this.HInput.Name = "HInput";
            this.HInput.Size = new System.Drawing.Size(31, 20);
            this.HInput.TabIndex = 12;
            this.HInput.Tag = "H";
            this.HInput.Text = "255";
            this.HInput.TextChanged += new System.EventHandler(this.SliderInputChanged);
            this.HInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValidateSliderInput);
            // 
            // HPointer
            // 
            this.HPointer.Image = ((System.Drawing.Image)(resources.GetObject("HPointer.Image")));
            this.HPointer.InitialImage = null;
            this.HPointer.Location = new System.Drawing.Point(120, 8);
            this.HPointer.Name = "HPointer";
            this.HPointer.Size = new System.Drawing.Size(12, 8);
            this.HPointer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.HPointer.TabIndex = 11;
            this.HPointer.TabStop = false;
            this.HPointer.Tag = "H";
            // 
            // SInput
            // 
            this.SInput.Location = new System.Drawing.Point(155, 146);
            this.SInput.MaxLength = 3;
            this.SInput.Name = "SInput";
            this.SInput.Size = new System.Drawing.Size(31, 20);
            this.SInput.TabIndex = 15;
            this.SInput.Tag = "S";
            this.SInput.Text = "255";
            this.SInput.TextChanged += new System.EventHandler(this.SliderInputChanged);
            this.SInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValidateSliderInput);
            // 
            // SPointer
            // 
            this.SPointer.Image = ((System.Drawing.Image)(resources.GetObject("SPointer.Image")));
            this.SPointer.InitialImage = null;
            this.SPointer.Location = new System.Drawing.Point(157, 8);
            this.SPointer.Name = "SPointer";
            this.SPointer.Size = new System.Drawing.Size(12, 8);
            this.SPointer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.SPointer.TabIndex = 14;
            this.SPointer.TabStop = false;
            this.SPointer.Tag = "S";
            // 
            // LInput
            // 
            this.LInput.Location = new System.Drawing.Point(192, 146);
            this.LInput.MaxLength = 3;
            this.LInput.Name = "LInput";
            this.LInput.Size = new System.Drawing.Size(31, 20);
            this.LInput.TabIndex = 18;
            this.LInput.Tag = "L";
            this.LInput.Text = "255";
            this.LInput.TextChanged += new System.EventHandler(this.SliderInputChanged);
            this.LInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValidateSliderInput);
            // 
            // LPointer
            // 
            this.LPointer.Image = ((System.Drawing.Image)(resources.GetObject("LPointer.Image")));
            this.LPointer.InitialImage = null;
            this.LPointer.Location = new System.Drawing.Point(194, 8);
            this.LPointer.Name = "LPointer";
            this.LPointer.Size = new System.Drawing.Size(12, 8);
            this.LPointer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LPointer.TabIndex = 17;
            this.LPointer.TabStop = false;
            this.LPointer.Tag = "L";
            // 
            // AInput
            // 
            this.AInput.Location = new System.Drawing.Point(229, 146);
            this.AInput.MaxLength = 3;
            this.AInput.Name = "AInput";
            this.AInput.Size = new System.Drawing.Size(31, 20);
            this.AInput.TabIndex = 21;
            this.AInput.Tag = "A";
            this.AInput.Text = "255";
            this.AInput.TextChanged += new System.EventHandler(this.SliderInputChanged);
            this.AInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValidateSliderInput);
            // 
            // APointer
            // 
            this.APointer.Image = ((System.Drawing.Image)(resources.GetObject("APointer.Image")));
            this.APointer.InitialImage = null;
            this.APointer.Location = new System.Drawing.Point(231, 8);
            this.APointer.Name = "APointer";
            this.APointer.Size = new System.Drawing.Size(12, 8);
            this.APointer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.APointer.TabIndex = 20;
            this.APointer.TabStop = false;
            this.APointer.Tag = "A";
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(266, 144);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 23;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKClicked);
            // 
            // CancelButton
            // 
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(347, 144);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 24;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(322, 95);
            this.label1.MaximumSize = new System.Drawing.Size(100, 0);
            this.label1.MinimumSize = new System.Drawing.Size(100, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 13);
            this.label1.TabIndex = 25;
            this.label1.Text = "Solid   |   Colour";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // HexInput
            // 
            this.HexInput.Location = new System.Drawing.Point(322, 118);
            this.HexInput.MaxLength = 9;
            this.HexInput.Name = "HexInput";
            this.HexInput.Size = new System.Drawing.Size(100, 20);
            this.HexInput.TabIndex = 26;
            this.HexInput.TextChanged += new System.EventHandler(this.OnHexChanged);
            this.HexInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValidateHexInput);
            // 
            // ResultDisplay
            // 
            this.ResultDisplay.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ResultDisplay.BackgroundImage")));
            this.ResultDisplay.Location = new System.Drawing.Point(322, 12);
            this.ResultDisplay.Name = "ResultDisplay";
            this.ResultDisplay.Size = new System.Drawing.Size(100, 80);
            this.ResultDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.ResultDisplay.TabIndex = 22;
            this.ResultDisplay.TabStop = false;
            // 
            // ASlider
            // 
            this.ASlider.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ASlider.BackgroundImage")));
            this.ASlider.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ASlider.Location = new System.Drawing.Point(243, 12);
            this.ASlider.Name = "ASlider";
            this.ASlider.Size = new System.Drawing.Size(17, 128);
            this.ASlider.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.ASlider.TabIndex = 19;
            this.ASlider.TabStop = false;
            this.ASlider.Tag = "A";
            this.ASlider.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseDown);
            this.ASlider.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseMove);
            this.ASlider.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseUp);
            // 
            // LSlider
            // 
            this.LSlider.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LSlider.Location = new System.Drawing.Point(206, 12);
            this.LSlider.Name = "LSlider";
            this.LSlider.Size = new System.Drawing.Size(17, 128);
            this.LSlider.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.LSlider.TabIndex = 16;
            this.LSlider.TabStop = false;
            this.LSlider.Tag = "L";
            this.LSlider.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseDown);
            this.LSlider.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseMove);
            this.LSlider.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseUp);
            // 
            // SSlider
            // 
            this.SSlider.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SSlider.Location = new System.Drawing.Point(169, 12);
            this.SSlider.Name = "SSlider";
            this.SSlider.Size = new System.Drawing.Size(17, 128);
            this.SSlider.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.SSlider.TabIndex = 13;
            this.SSlider.TabStop = false;
            this.SSlider.Tag = "S";
            this.SSlider.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseDown);
            this.SSlider.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseMove);
            this.SSlider.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseUp);
            // 
            // HSlider
            // 
            this.HSlider.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.HSlider.Location = new System.Drawing.Point(132, 12);
            this.HSlider.Name = "HSlider";
            this.HSlider.Size = new System.Drawing.Size(17, 128);
            this.HSlider.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.HSlider.TabIndex = 10;
            this.HSlider.TabStop = false;
            this.HSlider.Tag = "H";
            this.HSlider.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseDown);
            this.HSlider.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseMove);
            this.HSlider.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseUp);
            // 
            // BSlider
            // 
            this.BSlider.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.BSlider.Location = new System.Drawing.Point(95, 12);
            this.BSlider.Name = "BSlider";
            this.BSlider.Size = new System.Drawing.Size(17, 128);
            this.BSlider.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.BSlider.TabIndex = 7;
            this.BSlider.TabStop = false;
            this.BSlider.Tag = "B";
            this.BSlider.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseDown);
            this.BSlider.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseMove);
            this.BSlider.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseUp);
            // 
            // GSlider
            // 
            this.GSlider.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.GSlider.Location = new System.Drawing.Point(58, 12);
            this.GSlider.Name = "GSlider";
            this.GSlider.Size = new System.Drawing.Size(17, 128);
            this.GSlider.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.GSlider.TabIndex = 4;
            this.GSlider.TabStop = false;
            this.GSlider.Tag = "G";
            this.GSlider.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseDown);
            this.GSlider.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseMove);
            this.GSlider.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseUp);
            // 
            // RSlider
            // 
            this.RSlider.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RSlider.Location = new System.Drawing.Point(21, 12);
            this.RSlider.Name = "RSlider";
            this.RSlider.Size = new System.Drawing.Size(17, 128);
            this.RSlider.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.RSlider.TabIndex = 1;
            this.RSlider.TabStop = false;
            this.RSlider.Tag = "R";
            this.RSlider.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseDown);
            this.RSlider.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseMove);
            this.RSlider.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ColorSliderMouseUp);
            // 
            // ColorSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 174);
            this.Controls.Add(this.HexInput);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.ResultDisplay);
            this.Controls.Add(this.AInput);
            this.Controls.Add(this.ASlider);
            this.Controls.Add(this.APointer);
            this.Controls.Add(this.LInput);
            this.Controls.Add(this.LSlider);
            this.Controls.Add(this.LPointer);
            this.Controls.Add(this.SInput);
            this.Controls.Add(this.SSlider);
            this.Controls.Add(this.SPointer);
            this.Controls.Add(this.HInput);
            this.Controls.Add(this.HSlider);
            this.Controls.Add(this.HPointer);
            this.Controls.Add(this.BInput);
            this.Controls.Add(this.BSlider);
            this.Controls.Add(this.BPointer);
            this.Controls.Add(this.GInput);
            this.Controls.Add(this.GSlider);
            this.Controls.Add(this.GPointer);
            this.Controls.Add(this.RInput);
            this.Controls.Add(this.RSlider);
            this.Controls.Add(this.RPointer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ColorSelector";
            this.Text = "Colour Picker";
            ((System.ComponentModel.ISupportInitialize)(this.RPointer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GPointer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BPointer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.HPointer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SPointer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LPointer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.APointer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ResultDisplay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ASlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.HSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RSlider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CustomPictureBox RSlider;
        private System.Windows.Forms.PictureBox RPointer;
        private System.Windows.Forms.TextBox RInput;
        private System.Windows.Forms.TextBox GInput;
        private CustomPictureBox GSlider;
        private System.Windows.Forms.PictureBox GPointer;
        private System.Windows.Forms.TextBox BInput;
        private CustomPictureBox BSlider;
        private System.Windows.Forms.PictureBox BPointer;
        private System.Windows.Forms.TextBox HInput;
        private CustomPictureBox HSlider;
        private System.Windows.Forms.PictureBox HPointer;
        private System.Windows.Forms.TextBox SInput;
        private CustomPictureBox SSlider;
        private System.Windows.Forms.PictureBox SPointer;
        private System.Windows.Forms.TextBox LInput;
        private CustomPictureBox LSlider;
        private System.Windows.Forms.PictureBox LPointer;
        private System.Windows.Forms.TextBox AInput;
        private CustomPictureBox ASlider;
        private System.Windows.Forms.PictureBox APointer;
        private CustomPictureBox ResultDisplay;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox HexInput;
    }
}