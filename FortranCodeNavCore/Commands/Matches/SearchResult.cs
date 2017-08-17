using FortranCodeNavCore.Fortran.Elements;
using FortranCodeNavCore.SyntaxTrees;

namespace FortranCodeNavCore.Commands.Matches
{
    public class SearchResult
    {
        public SyntaxTree SyntaxTree { get; set; }
        public IMember Member { get; set; }
    }
}
