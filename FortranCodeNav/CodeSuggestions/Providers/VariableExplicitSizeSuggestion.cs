using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;

namespace FortranCodeNav.CodeSuggestions.Providers
{
    [Export(typeof(ICodeSuggestionProvider))]
    public class VariableExplicitSizeSuggestion : ICodeSuggestionProvider
    {
        private static Regex realWithoutSizeRegex = new Regex(@"\b(?<type>REAL)[(\s+\w),:]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        public IEnumerable<CodeSuggestionInfo> GetSuggestions(string line)
        {
            foreach (Match match in realWithoutSizeRegex.Matches(line))
            {
                yield return new CodeSuggestionInfo
                {
                    StartIndexInLine = match.Index,
                    EndIndexInLine = match.Index + match.Groups["type"].Length,
                    SuggestionTooltip = "Consider specifying the size explictly; the compiler now decides whether to use 4 or 8 bytes."
                };
            }
        }
    }
}
