using FortranCodeNavCore.Fortran.Elements;

namespace FortranCodeNavCore.Matches
{
    public class UsageResult
    {
        public UsageResult(SyntaxTree syntaxTree, LocationInFile location, IMember enclosingMember, string line)
        {
            SyntaxTree = syntaxTree;
            Location = location;
            EnclosingMember = enclosingMember;
            Line = line;
        }

        private IMember EnclosingMember { get; set; }
        private string Line { get; set; }
        public SyntaxTree SyntaxTree { get; private set; }
        public LocationInFile Location { get; private set; }
        
        public string Name
        {
            get { return MemberLocation + ", " + FileLocation; }
        }

        public string Hint
        {
            get { return Line; }
        }

        private string FileLocation
        {
            get { return SyntaxTree.CodeFile.FileName + " : " + Location.Line; }
        }

        private string MemberLocation
        {
            get
            {
                var enclosing = EnclosingMember;
                if (enclosing != null)
                {
                    return "in " + enclosing.Name + " (" + enclosing.GetType().Name.ToLower() + ")";
                }
                return "";
            }
        }
    }
}
