using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace FortranCodeNavCore.Forms
{
    public partial class ListFindControl : Form
    {
        public int MaxResults = 15;

        private readonly CamelCaseRegexBuilder regexBuilder = new CamelCaseRegexBuilder();

        public ListFindControl()
        {
            InitializeComponent();
            SearchResultsList.AutoClose = false;
            SearchResultsList.FocusLost += new EventHandler(SearchResultsListLostFocus);
            pictureBox1.Image = Properties.Resources.Search;
            SearchHintText = "";
        }

        private bool settingText;
        public string SearchHintText { get; set; }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (HideSearchDialog)
            {
                Opacity = 0.0;
            }

            searchBox.ForeColor = Color.Gray;
            settingText = true;
            searchBox.Text = SearchHintText;
            settingText = false;
            searchBox.SelectionStart = 0;

            FilteredResults = DataSource;
            ShowResults();
        }

        void SearchResultsListLostFocus(object sender, EventArgs e)
        {
            //apparently we were activated (user clicked on scroll buttons of list)
            //and now user clicked elsewhere, so we're closing.
            Close();
        }

        protected override void OnDeactivate(EventArgs e)
        {            
            base.OnDeactivate(e);

            var clickedInResultList = SearchResultsList.Bounds.Contains(Cursor.Position);

            if (!clickedInResultList)
                Close();

            //focus & control transferred to the result list now
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            SearchResultsList.Close();
        }

        public delegate string GetHintForItemDelegate(object item);
        public delegate Bitmap GetIconForItemDelegate(object item);
        public GetIconForItemDelegate OnGetIconForItem { get; set; }
        public GetHintForItemDelegate OnGetHintForItem { get; set; }
        public Func<object, Point> OnGetHintEmphasisRangeForItem { get; set; }

        private void ShowResults()
        {
            SearchResultsList.SuspendLayout();
            SearchResultsList.Items.Clear();
            
            if (AlwaysShowList || !IsSearchBoxEmpty())
            {
                var topResults = FilteredResults.Count() > MaxResults ? FilteredResults.Take(MaxResults).ToList() : FilteredResults;

                bool first = true;
                foreach (var item in topResults)
                {
                    var name = GetNameForObject(item);
                    var hintText = GetHintForObject(item);
                    var emphasis = GetHintEmphasisRangeForObject(item);
                    var image = OnGetIconForItem != null ? OnGetIconForItem(item) : null;

                    var menuItem = new ToolStripButtonWithHint(name, image, hintText);
                    
                    menuItem.EmphasisStart = emphasis.X;
                    menuItem.EmphasisEnd = emphasis.Y;

                    SearchResultsList.Items.Add(menuItem);

                    if (first)
                    {
                        menuItem.Select();
                        first = false;
                    }

                    menuItem.Click += MenuItemClick;
                }

                var unshownCount = FilteredResults.Count() - topResults.Count();
                if (unshownCount > 0)
                {
                    SearchResultsList.Items.Add(new ToolStripLabel(String.Format(" + {0} more (continue typing to filter further)", unshownCount))
                                                    {ForeColor = Color.Gray});
                }

                SearchResultsList.ResumeLayout();

                if (!SearchResultsList.Visible)
                {
                    SearchResultsList.Show(new Point(Left, Bottom));
                }
            }
            else
            {
                SearchResultsList.ResumeLayout();
                SearchResultsList.Hide();
            }
        }

        private bool IsSearchBoxEmpty()
        {
            return searchBox.Text.Length == 0 || searchBox.Text == SearchHintText;
        }

        void MenuItemClick(object sender, EventArgs e)
        {
            ActOnSelection();
        }

        private string GetNameForObject(object item)
        {
            var prop = item.GetType().GetProperties().FirstOrDefault(p => p.Name == DataMember);
            return prop.GetValue(item, null).ToString();
        }
        
        private Point GetHintEmphasisRangeForObject(object item)
        {
            if (OnGetHintEmphasisRangeForItem != null)
            {
                return OnGetHintEmphasisRangeForItem(item);
            }
            return new Point(-1, -1);
        }
        
        private string GetHintForObject(object item)
        {
            string hint = "";

            if (OnGetHintForItem != null)
            {
                hint = OnGetHintForItem(item);
            }
            else if (!string.IsNullOrEmpty(HintMember))
            {
                var prop = item.GetType().GetProperties().FirstOrDefault(p => p.Name == HintMember);
                hint = prop.GetValue(item, null).ToString();
            }

            return hint;
        }

        public IEnumerable<object> DataSource { get; set; }
        public string DataMember { get; set; }
        public bool AlwaysShowList { get; set; }

        private IEnumerable<object> FilteredResults { get; set; }

        private void SearchBoxTextChanged(object sender, EventArgs e)
        {
            if (settingText)
                return;

            FilteredResults = Filter(DataSource, searchBox.Text);
            ShowResults();
        }

        private IEnumerable<object> Filter(IEnumerable<object> dataSource, string searchTerm)
        {
            Regex regex = regexBuilder.BuildRegex(searchTerm);

            var exactMatches = new List<object>();
            var simpleMatches = new List<object>();
            var regexpMatches = new List<object>();
            foreach(var item in dataSource)
            {
                var name = GetNameForObject(item);

                if (name.ToLower() == searchTerm.ToLower())
                {
                    exactMatches.Add(item);
                }
                else if (SimpleMatch(searchTerm, name))
                {
                    simpleMatches.Add(item);
                }
                else if (regex.Match(name).Success)
                {
                    regexpMatches.Add(item);
                }
                else if (AlwaysShowList && name.IndexOf(searchTerm, StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    simpleMatches.Add(item);
                }
            }
            // concatenate search results by relevance and sort by name
            var resultsList = exactMatches.OrderBy(item => item.ToString()).ToList();
            resultsList.AddRange(simpleMatches.OrderBy(item => item.ToString()));
            resultsList.AddRange(regexpMatches.OrderBy(item => item.ToString()));
            return resultsList;
        }

        private static bool SimpleMatch(string searchTerm, string name)
        {
            if (name.StartsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }

        private void ListFindControlKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                SelectNextItem();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                SelectPreviousItem();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                ActOnSelection();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void ActOnSelection()
        {
            var selectedIndex = GetSelectedItem();

            if (FilteredResults.Count() == 0)
            {
                return;
            }

            if (selectedIndex == -1)
            {
                selectedIndex = 0;
            }

            OnSelectionMade(selectedIndex);

            Close();
        }

        private void OnSelectionMade(int selectedIndex)
        {
            if (ItemChosen != null)
            {
                ItemChosen(FilteredResults.ElementAt(selectedIndex), EventArgs.Empty);
            }
        }

        public event EventHandler ItemChosen;

        private void SelectNextItem()
        {
            var selectedItem = GetSelectedItem();

            if (selectedItem == -1 || selectedItem == GetLastIndex())
            {
                SelectItem(0);
            }
            else
            {
                SelectItem(selectedItem + 1);
            }
        }

        private int GetSelectedItem()
        {
            for(int i = 0; i < SearchResultsList.Items.Count; i++)
            {
                if (SearchResultsList.Items[i].Selected)
                {
                    return i;
                }
            }
            return -1;
        }

        private void SelectPreviousItem()
        {
            var selectedIndex = GetSelectedItem();

            if (selectedIndex <= 0)
            {
                SelectItem(GetLastIndex());
            }
            else
            {
                SelectItem(selectedIndex - 1);
            }
        }

        private int GetLastIndex()
        {
            return SearchResultsList.Items.OfType<ToolStripMenuItem>().Count()-1;
        }

        private void SelectItem(int index)
        {
            if (index >= 0 && index < SearchResultsList.Items.Count)
            {
                SearchResultsList.Items[index].Select();
            }
        }

        private void SearchBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (searchBox.Text == SearchHintText)
            {
                settingText = true;
                searchBox.Text = "";
                settingText = false;
                searchBox.ForeColor = Color.Black;
            }
        }

        public bool HideSearchDialog { get; set; }

        public string HintMember { get; set; }
    }
}
