using System.Collections.Generic;
namespace FortranCodeNavCore.Fortran.Elements
{
    public class Method : Member, IMethod 
    {
        private IList<Variable> parameters;

        public string ResultVariableName { get; set; }

        public void SetParameters(IList<Variable> parameters)
        {
            this.parameters = parameters;
        }

        public IEnumerable<Variable> Parameters
        {
            get
            {
                return parameters;
            }
        }
    }
}