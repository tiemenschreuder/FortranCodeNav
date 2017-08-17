using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace VSIntegration.CodeComplete
{
    public sealed class ImageListBox : ListBox
    {
        private const int TextStart = 16 + 6;

        public ImageListBox()
        {
            SetStyle(ControlStyles.Selectable, false);
            DrawMode = DrawMode.OwnerDrawFixed; // We're using custom drawing.
            ItemHeight = 25; // Set the item height to 40.   
        }

        private static string GetNameForItem(object item)
        {
            if (item is CompletionItem)
            {
                return (item as CompletionItem).Name;
            }
            return item.ToString();
        }

        private static Bitmap GetImageForItem(object item)
        {
            if (item is CompletionItem)
            {
                return (item as CompletionItem).Image;
            }
            return null;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // Make sure we're not trying to draw something that isn't there.
            if (e.Index >= Items.Count || e.Index <= -1)
                return;

            // Get the item object.
            var item = Items[e.Index];
            
            if (item == null)
                return;

            Brush textBrush;
            const int textOffset = 19;

            var imageBounds = new Rectangle(0, e.Bounds.Y, textOffset, e.Bounds.Height);
            var textBounds = new Rectangle(e.Bounds.X + textOffset, e.Bounds.Y, e.Bounds.Width - textOffset,
                                           e.Bounds.Height);

            e.Graphics.FillRectangle(new SolidBrush(Color.White), imageBounds);

            // Draw the background color depending on 
            // if the item is selected or not.
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                // The item is selected.
                // We want a blue background color.
                e.Graphics.FillRectangle(new SolidBrush(Color.Blue), textBounds);
                textBrush = Brushes.White;
            }
            else
            {
                // The item is NOT selected.
                // We want a white background color.
                e.Graphics.FillRectangle(new SolidBrush(Color.White), textBounds);
                textBrush = Brushes.Black;
            }

            // Draw the item.
            string text = GetNameForItem(item);
            SizeF stringSize = e.Graphics.MeasureString(text, Font);
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.DrawString(text, Font, textBrush,
                                  new PointF(TextStart, e.Bounds.Y + (e.Bounds.Height - stringSize.Height)/2));

            Bitmap image = GetImageForItem(item);
            if (image != null)
            {
                e.Graphics.DrawImage(image, new Point(0, e.Bounds.Y));
            }
        }

        internal int CalculateDesiredWidth()
        {
            using (Graphics graphics = CreateGraphics())
            {
                float maxWidth = 0;

                foreach (object item in Items)
                {
                    string text = GetNameForItem(item);
                    SizeF stringSize = graphics.MeasureString(text, Font);

                    maxWidth = Math.Max(maxWidth, stringSize.Width + TextStart);
                }

                return (int) Math.Ceiling(maxWidth);
            }
        }
    }
}