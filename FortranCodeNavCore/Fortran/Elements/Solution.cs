using System.Collections.Generic;
using FortranCodeNavCore.SyntaxTrees;

namespace FortranCodeNavCore.Fortran.Elements
{
    public class Solution
    {
        public string Directory { get; set;}
        public string FileName { get; set; }
        public IEnumerable<SyntaxTree> Files;
    }
}
