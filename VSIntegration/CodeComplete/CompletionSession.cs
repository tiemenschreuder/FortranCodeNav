using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace VSIntegration.CodeComplete
{
    public class CompletionSession
    {
        public string SignatureToolTip { get; set; }
        public Point Coordinate { get; set; }
        public bool Cancel { get; set; }
        public string Filter { get; private set; }
        public int LineNumber { get; set; }
        public int InsertionIndexInLine { get; set; }
        public IEnumerable<CompletionItem> FilteredCompletionSet { get; private set; }

        public void SetCompletionSet(IEnumerable<CompletionItem> set, string filter)
        {
            Filter = filter;
            FilteredCompletionSet = CreateFilteredSet(set, filter);
        }

        private static IEnumerable<CompletionItem> CreateFilteredSet(IEnumerable<CompletionItem> set, string filter)
        {
            var hasFilter = !String.IsNullOrEmpty(filter);
            
            var items = new List<CompletionItem>();

            if (!hasFilter)
            {
                items = set.OrderBy(item => item.Name).ToList(); //alphabetical order
            }
            else
            {
                //preferred ordering: first 'startWith' matches, then 'index of'
                foreach (var item in set)
                {
                    if (item.Name.StartsWith(filter, StringComparison.InvariantCultureIgnoreCase))
                    {
                        items.Add(item);
                    }
                }

                foreach (var item in set)
                {
                    if (item.Name.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase)>= 1)
                    {
                        if (!items.Contains(item)) //prevent duplicates
                            items.Add(item);
                    }
                }
            }

            return items;
        }
    }
}