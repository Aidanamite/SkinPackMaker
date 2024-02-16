using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace SkinPackMaker
{
    public partial class ColorSelector : Form
    {
        static Dictionary<string, int> max = new Dictionary<string, int>
        {
            { "R", 255 },
            { "G", 255 },
            { "B", 255 },
            { "A", 255 },
            { "H", 359 },
            { "L", 255 },
            { "S", 255 }
        };
        Dictionary<string, (TextBox input, PictureBox pointer, PictureBox slider)> myControls = new Dictionary<string, (TextBox, PictureBox, PictureBox)>();
        public Color Color
        {
            get => _c;
            set
            {
                if (_c == value)
                    return;
                UpdateSliders(value);
            }
        }
        Color _c = Color.Black;
        (int, int, int) HSL;
        public ColorSelector()
        {
            InitializeComponent();
            var w = 3;
            RSlider.Image = new Bitmap(w, 256);
            GSlider.Image = new Bitmap(w, 256);
            BSlider.Image = new Bitmap(w, 256);
            HSlider.Image = new Bitmap(w, 360);
            SSlider.Image = new Bitmap(w, 256);
            LSlider.Image = new Bitmap(w, 256);
            ASlider.Image = new Bitmap(w, 256);
            ResultDisplay.Image = new Bitmap(20, 4);
            myControls["R"] = (RInput, RPointer, RSlider);
            myControls["G"] = (GInput, GPointer, GSlider);
            myControls["B"] = (BInput, BPointer, BSlider);
            myControls["H"] = (HInput, HPointer, HSlider);
            myControls["S"] = (SInput, SPointer, SSlider);
            myControls["L"] = (LInput, LPointer, LSlider);
            myControls["A"] = (AInput, APointer, ASlider);
        }

        void UpdateSliders(Color nColor, string called = null, int value = 0)
        {
            SuspendLayout();
            _c = nColor;
            if (called == "A")
            {
                UpdateSlider(called, _c.A);
            }
            else if (called == "H" || called == "L" || called == "S")
            {
                if (called == "H")
                    HSL.Item1 = value;
                else if (called == "S")
                    HSL.Item2 = value;
                else
                    HSL.Item3 = value;
                UpdateSlider(called, value);
                UpdateSlider("R", _c.R);
                UpdateSlider("G", _c.G);
                UpdateSlider("B", _c.B);
                UpdateGradients();
            }
            else
            {
                if (called == "R" || called == "G" || called == "B")
                    UpdateSlider(called, value);
                else
                {
                    UpdateSlider("R", _c.R);
                    UpdateSlider("G", _c.G);
                    UpdateSlider("B", _c.B);
                    UpdateSlider("A", _c.A);
                }
                _c.ToHSL(out var h, out var s, out var l);
                HSL = (h, s, l);
                UpdateSlider("H", h);
                UpdateSlider("S", s);
                UpdateSlider("L", l);
                UpdateGradients();
            }
            if (called != "HEX")
            {
                updatingInput = true;
                HexInput.Text = _c.ToHex();
                updatingInput = false;
            }
            var solid = Color.FromArgb(_c.R, _c.G, _c.B);
            if (ResultDisplay.Image is Bitmap bit)
                for (int i = 0; i < bit.Width; i++)
                    for (int j = 0; j < bit.Height; j++)
                        bit.SetPixel(i, j, i < bit.Width / 2 ? solid : _c);
            ResultDisplay.Invalidate();
            ResumeLayout();
        }

        void UpdateGradients()
        {
            GenerateGradient(RSlider.Image, x => Color.FromArgb((byte)x, _c.G, _c.B));
            GenerateGradient(GSlider.Image, x => Color.FromArgb(_c.R, (byte)x, _c.B));
            GenerateGradient(BSlider.Image, x => Color.FromArgb(_c.R, _c.G, (byte)x));
            GenerateGradient(HSlider.Image, x => ColorConvert.FromHSL(x, HSL.Item2, HSL.Item3));
            GenerateGradient(SSlider.Image, x => ColorConvert.FromHSL(HSL.Item1, x, HSL.Item3));
            GenerateGradient(LSlider.Image, x => ColorConvert.FromHSL(HSL.Item1, HSL.Item2, x));
            GenerateGradient(ASlider.Image, x => Color.FromArgb((byte)x, _c.R, _c.G, _c.B));
            foreach (var p in myControls)
                p.Value.slider.Invalidate();
        }

        static void GenerateGradient(Image img, Func<int,Color> gradient)
        {
            if (img is Bitmap bit)
                for (int i = 0; i < img.Height; i++)
                {
                    var c = gradient(img.Height - i - 1);
                    for (int j = 0; j < img.Width; j++)
                        bit.SetPixel(j, i, c);
                }
        }

        bool updatingInput = false;
        void UpdateSlider(string tag, int value)
        {
            if (myControls.TryGetValue(tag,out var v))
            {
                var l = v.pointer.Location;
                l.Y = v.slider.ClientRectangle.Location.Y + v.slider.ClientRectangle.Height + v.slider.Location.Y - value * v.slider.ClientRectangle.Height / max[tag] - v.pointer.Bounds.Height / 2;
                v.pointer.Location = l;
                updatingInput = true;
                v.input.Text = value.ToString();
                updatingInput = false;
            }
        }

        bool isHolding = false;
        void ColorSliderMouseDown(object sender, MouseEventArgs e)
        {
            if (sender is Control c)
            {
                Cursor.Clip = c.RectangleToScreen(c.ClientRectangle);
                isHolding = true;
            }
        }

        private void ColorSliderMouseUp(object sender, MouseEventArgs e)
        {
            Cursor.Clip = Rectangle.Empty;
            isHolding = false;
        }

        private void ColorSliderMouseMove(object sender, MouseEventArgs e)
        {
            if (isHolding && sender is Control c && myControls.TryGetValue(c.Tag.ToString(), out var v) && max.TryGetValue(c.Tag.ToString(),out var m))
            {
                var value = m - (e.Y - c.ClientRectangle.Y) * m / (c.ClientRectangle.Height - 1);
                UpdateValue(c.Tag.ToString(), value);
            }
        }

        void UpdateValue(string tag, int value)
        {
            if (value < 0)
                value = 0;
            else if (value > max[tag])
                value = max[tag];
            Color nc;
            if (tag == "A")
                nc = Color.FromArgb(value, _c.R, _c.G, _c.B);
            else if (tag == "R")
                nc = Color.FromArgb(_c.A, value, _c.G, _c.B);
            else if (tag == "G")
                nc = Color.FromArgb(_c.A, _c.R, value, _c.B);
            else if (tag == "B")
                nc = Color.FromArgb(_c.A, _c.R, _c.G, value);
            else if (tag == "H")
                nc = ColorConvert.FromHSL(value, HSL.Item2, HSL.Item3);
            else if (tag == "S")
                nc = ColorConvert.FromHSL(HSL.Item1, value, HSL.Item3);
            else if (tag == "L")
                nc = ColorConvert.FromHSL(HSL.Item1, HSL.Item2, value);
            else
                throw new ArgumentOutOfRangeException();
            UpdateSliders(nc, tag, value);
        }

        void ValidateSliderInput(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
                && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void SliderInputChanged(object sender, EventArgs e)
        {
            if (!updatingInput && sender is TextBox t)
            {
                var str = new StringBuilder();
                foreach (var c in t.Text)
                    if (char.IsDigit(c))
                        str.Append(c);
                var nt = str.ToString();
                if (nt != t.Text)
                    t.Text = nt;
                UpdateValue(t.Tag.ToString(), nt == "" ? 0 : Math.Min(int.Parse(nt),max[t.Tag.ToString()]));
            }
        }

        void OnHexChanged(object sender, EventArgs e)
        {
            if (!updatingInput && sender is TextBox t)
            {
                var str = new StringBuilder();
                foreach (var c in t.Text)
                    if (char.IsDigit(c))
                        str.Append(c);
                var nt = str.ToString();
                if (nt != t.Text)
                    t.Text = nt;
                UpdateSliders(nt.HexToColor(),"HEX");
            }
        }

        void ValidateHexInput(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
                && !char.IsDigit(e.KeyChar)
                && !(e.KeyChar >= 'A' && e.KeyChar <= 'F')
                && !(e.KeyChar >= 'a' && e.KeyChar <= 'f'))
                e.Handled = true;
        }

        void OKClicked(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelClicked(object sender, EventArgs e)
        {
            Close();
        }
    }

    public class CustomPictureBox : PictureBox
    {
        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            base.OnPaint(pe);
        }
    }
}
