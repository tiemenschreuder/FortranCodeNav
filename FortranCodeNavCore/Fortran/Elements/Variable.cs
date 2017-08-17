using VSIntegration;

namespace FortranCodeNavCore.Fortran.Elements
{
    public class Variable : INameable
    {
        public LocationInFile Location { get; set; }
        public IMember Member { get; set; }
        public string Name { get; set; }

        public string TypeString { get; set; }
        public bool IsBuildInType { get; set; }

        public override string ToString()
        {
            return TypeString + " " + Name;
        }
    }
}
