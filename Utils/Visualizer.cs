using System.Drawing;
using System.Windows.Forms;

namespace AeroHear.Utils
{
    public class Visualizer : Control
    {
        private float[] _values = new float[0];

        public void Update(float[] newValues)
        {
            _values = newValues;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(Color.Black);
            if (_values.Length == 0) return;

            using var pen = new Pen(Color.Lime, 1);
            int width = ClientSize.Width;
            int height = ClientSize.Height;

            for (int i = 1; i < _values.Length; i++)
            {
                float x1 = (i - 1) * width / _values.Length;
                float y1 = height / 2 - _values[i - 1] * height / 2;
                float x2 = i * width / _values.Length;
                float y2 = height / 2 - _values[i] * height / 2;
                g.DrawLine(pen, x1, y1, x2, y2);
            }
        }
    }
}
