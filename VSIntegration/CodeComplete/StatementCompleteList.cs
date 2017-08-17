using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace VSIntegration.CodeComplete
{
    public partial class StatementCompleteList : Form
    {
        private const int MaxItemsToShow = 15;
        private bool noResults;
        private int lastClick = -1;
        private int lastSelectedIndex = -1;
        
        public event EventHandler ItemSelected;

        public StatementCompleteList()
        {
            InitializeComponent();
            Deactivate += StatementCompleteListDeactivate;
            resultsListBox.SelectedIndexChanged += ResultsListBoxSelectedIndexChanged;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams p = base.CreateParams;
                p.ExStyle |= (Native.WS_EX_NOACTIVATE | Native.WS_EX_TOOLWINDOW | Native.WS_EX_TOPMOST);
                p.Parent = IntPtr.Zero;
                return p;
            }
        }
        
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        private CompletionItem SelectedItem
        {
            get { return resultsListBox.SelectedItem as CompletionItem; }
        }

        private int SelectedIndex
        {
            get { return resultsListBox.SelectedIndex; }
            set { resultsListBox.SelectedIndex = value; }
        }
        
        private void ResultsListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateToolTipForSelectedItem();
        }

        private void UpdateToolTipForSelectedItem()
        {
            if (resultsListBox.SelectedItem == null)
            {
                toolTip.ToolTipTitle = "";
                return;
            }

            toolTip.Show(GetHintForItem(resultsListBox.SelectedItem), this, Width, 0);
        }


        private void StatementCompleteListDeactivate(object sender, EventArgs e)
        {
            Close();
        }

        private void HandleMouseInteraction()
        {
            int selectedIndex = SelectItemUnderMouse();

            if (selectedIndex == lastSelectedIndex &&
                Environment.TickCount - lastClick < SystemInformation.DoubleClickTime)
            {
                if (selectedIndex != -1)
                {
                    FireItemSelected();
                }
            }
            lastClick = Environment.TickCount;
            lastSelectedIndex = selectedIndex;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Native.WM_MOUSEACTIVATE)
            {
                HandleMouseInteraction();

                m.Result = (IntPtr) Native.MA_NOACTIVATE; // ANDEAT;
            }
            else if (m.Msg == Native.WM_ACTIVATE)
            {
                if ((int) m.WParam != Native.WA_INACTIVE)
                {
                    IntPtr oldWindow = m.LParam;
                    Native.SetActiveWindow(oldWindow);
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        private int SelectItemUnderMouse()
        {
            Point clientCoordinates = resultsListBox.PointToClient(Cursor.Position);
            int index = resultsListBox.IndexFromPoint(clientCoordinates);
            if (index >= 0)
            {
                SelectedIndex = index;
            }
            return index;
        }

        internal void SetResults(IList<CompletionItem> results)
        {
            resultsListBox.SuspendLayout();

            int oldIndex = SelectedIndex;

            resultsListBox.Items.Clear();

            noResults = results.Count == 0;

            if (noResults)
            {
                resultsListBox.Items.Add("(No suggestions)");
            }
            else
            {
                foreach (CompletionItem item in results)
                {
                    resultsListBox.Items.Add(item);
                }
            }

            int numItemsInList = resultsListBox.Items.Count;

            if (numItemsInList != 0)
            {
                SelectedIndex = Math.Max(0, Math.Min(oldIndex, numItemsInList - 1));
            }

            int itemsShowing = Math.Min(MaxItemsToShow, numItemsInList);

            int nonClientHeight = (Height - ClientSize.Height) + 2;
            Height = nonClientHeight + (itemsShowing*resultsListBox.ItemHeight);
            int nonClientWidth = (Width - ClientSize.Width);
            if (numItemsInList > MaxItemsToShow) //scrollbar size
                nonClientWidth += 18;
            Width = nonClientWidth + resultsListBox.CalculateDesiredWidth();

            UpdateToolTipForSelectedItem();

            resultsListBox.ResumeLayout();
        }

        internal void SelectNextInCompleteList(bool down)
        {
            int maxIndex = resultsListBox.Items.Count - 1;
            int currentIndex = SelectedIndex >= 0 ? SelectedIndex : 0;
            int change = down ? 1 : -1;
            int nextIndex = currentIndex + change;
            if (nextIndex < 0)
                nextIndex = maxIndex;
            else if (nextIndex > maxIndex)
                nextIndex = 0;
            SelectedIndex = nextIndex;
        }

        internal string GetSelectedItem()
        {
            if (noResults || SelectedItem == null)
                return null;
            return SelectedItem.Name;
        }

        private static string GetHintForItem(object item)
        {
            if (item is CompletionItem)
            {
                return (item as CompletionItem).ToolTip;
            }
            return "";
        }

        private void FireItemSelected()
        {
            if (SelectedItem != null)
            {
                if (ItemSelected != null)
                {
                    ItemSelected(this, EventArgs.Empty);
                }
            }
        }

        internal int GetListBoxHandle()
        {
            return (int) resultsListBox.Handle;
        }

        internal void SetSignatureToolTip(string tooltip)
        {
            if (!String.IsNullOrEmpty(tooltip))
            {
                signatureToolTip.Show(tooltip, this, -18, -75);
            }
        }
    }
}