using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;

namespace FortranCodeNav.CodeSuggestions.Providers
{
    [Export(typeof(ICodeSuggestionProvider))]
    public class VariableLegacySizeSuggestion : ICodeSuggestionProvider
    {
        private static Regex initializationFortran70StyleRegex = new Regex(@"\b(INTEGER|REAL|COMPLEX)\s*\*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public IEnumerable<CodeSuggestionInfo> GetSuggestions(string line)
        {
            foreach (Match match in initializationFortran70StyleRegex.Matches(line))
            {
                yield return new CodeSuggestionInfo
                {
                    StartIndexInLine = match.Index,
                    EndIndexInLine = match.Index + match.Length,
                    SuggestionTooltip = "Consider using modern syntax to specify the size; e.g., INTEGER(4), REAL(4), REAL(8)"
                };
            }
        }
    }
}
