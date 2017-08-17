using FortranCodeNavCore.Fortran.Elements;

namespace FortranCodeNavCore.Matches
{
    public class SearchResult
    {
        public SyntaxTree SyntaxTree { get; set; }
        public IMember Member { get; set; }
    }
}
