using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FortranCodeNavCore.Forms
{
    public class ContextMenuStripWithHint : ContextMenuStrip
    {
        public ContextMenuStripWithHint(IContainer container) : base(container)
        {
        }

        public event EventHandler FocusLost;

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            if (FocusLost != null)
            {
                FocusLost(this, e);
            }
        }

        public override System.Drawing.Size GetPreferredSize(System.Drawing.Size proposedSize)
        {
            var baseSize = base.GetPreferredSize(proposedSize);
            var maxWidth = 0;
            foreach(var item in Items.OfType<ToolStripButtonWithHint>())
            {
                var itemSize = item.GetMinimumSize(Size.Empty);
                maxWidth = Math.Max(maxWidth, itemSize.Width);
            }
            Width = maxWidth + 1;

            return new Size(Width, baseSize.Height);
        }
    }
}
