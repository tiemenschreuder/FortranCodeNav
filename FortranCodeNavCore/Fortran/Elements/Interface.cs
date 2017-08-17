using System.Collections.Generic;
using System.Linq;

namespace FortranCodeNavCore.Fortran.Elements
{
    public class Interface : Member
    {
        public override IEnumerable<IScope> SubItems
        {
            //we don't want to return the subitems of the interface: they would be duplicates of the real routines
            get
            {
                yield break; 
            }
        }

        public IEnumerable<IMember> References
        {
            get { return subItems.OfType<IMember>(); }
        }
    }
}