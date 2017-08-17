using System.Collections.Generic;
using FortranCodeNavCore.SyntaxTrees;
using VSIntegration;

namespace FortranCodeNavCore.Fortran.Elements
{
    public interface IMember : IScope, INameable
    {
        IEnumerable<Use> Uses { get; }
        void AddUse(Use use);
        void ClearUses();

        bool ImplicitNone { get; set; }

        IEnumerable<Variable> LocalVariables { get; }
        void AddLocalVariable(Variable variable);
        void ClearLocalVariables();

        SyntaxTree Root { get; set; }
        LocationInFile Location { get; set; }        
        IEnumerable<IScope> SubItems { get; }
        LocationInFile EndLocation { get; set; }
        void AddScope(IScope subItem);

        IEnumerable<IScope> GetSubItemsRecursive();
    }
}