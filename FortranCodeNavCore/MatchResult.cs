using FortranCodeNavCore.Fortran.Elements;

namespace FortranCodeNavCore
{
    public class MatchResult
    {
        public SyntaxTree SyntaxTree { get; set; }
        public IMember Member { get; set; }
    }
}
