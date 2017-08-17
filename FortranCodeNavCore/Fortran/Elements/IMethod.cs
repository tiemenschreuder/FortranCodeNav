using System.Collections.Generic;
namespace FortranCodeNavCore.Fortran.Elements
{
    public interface IMethod : IMember
    {
        void SetParameters(IList<Variable> parameters);
        IEnumerable<Variable> Parameters { get; }
        string ResultVariableName { get; set; }
    }
}