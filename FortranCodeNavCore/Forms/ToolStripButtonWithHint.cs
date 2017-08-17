using System;
using System.Drawing;
using System.Windows.Forms;

namespace FortranCodeNavCore.Forms
{
    public class ToolStripButtonWithHint : ToolStripMenuItem
    {
        public ToolStripButtonWithHint(string text, Image image, string hintText) : base(text, image)
        {
            HintText = hintText;
            EmphasisStart = -1;
            EmphasisEnd = -1;
            BackColor = Color.FromArgb(214, 238, 214);
        }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            var size = base.GetPreferredSize(constrainingSize);

            using (var graphics = Owner.CreateGraphics())
            {
                return new Size(Math.Max(Owner.Width-1, CalculateDesiredWidth(graphics)), size.Height);
            }
        }

        public Size GetMinimumSize(Size constrainingSize)
        {
            var size = base.GetPreferredSize(constrainingSize);

            using (var graphics = Owner.CreateGraphics())
            {
                return new Size(CalculateDesiredWidth(graphics), size.Height);
            }
        }
        
        public string HintText { get; set; }

        public int EmphasisStart { get; set; }
        public int EmphasisEnd { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var hintOffset = Owner.Width - (hintTextWidth + 10);
            TextRenderer.DrawText(e.Graphics, HintText, Font, new Point(hintOffset, 3), Color.Gray);

            if (EmphasisStart > -1)
            {
                var startOffset = MeasureText(e.Graphics, HintText.Substring(0, EmphasisStart));
                var endOffset = MeasureText(e.Graphics, HintText.Substring(0, EmphasisEnd));

                var startEmphasis = hintOffset + 3 + startOffset;
                var endEmphasis = hintOffset + 3 + endOffset;
                e.Graphics.DrawLine(Pens.Purple, new Point(startEmphasis, Height - 5), new Point(endEmphasis, Height - 5));
            }
        }

        private int hintTextWidth;

        private int MeasureText(Graphics g, string text)
        { 
            Size proposedSize = new Size(int.MaxValue, int.MaxValue);
            return (int)TextRenderer.MeasureText(g, text, Font, proposedSize, TextFormatFlags.NoPadding).Width;
        }

        private int CalculateDesiredWidth(Graphics g)
        {
            const int iconOffset = 40;
            hintTextWidth = MeasureText(g, HintText);
            var textWidth = MeasureText(g, Text);
            var desiredWidth = hintTextWidth + textWidth + iconOffset + 10;
            return desiredWidth;
        }
    }
}
