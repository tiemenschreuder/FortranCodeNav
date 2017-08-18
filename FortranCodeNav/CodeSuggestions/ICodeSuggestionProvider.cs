using System.Collections.Generic;

namespace FortranCodeNav.CodeSuggestions
{
    public interface ICodeSuggestionProvider
    {
        IEnumerable<CodeSuggestionInfo> GetSuggestions(string line);
    }
}
