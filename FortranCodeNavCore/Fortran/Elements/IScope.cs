namespace FortranCodeNavCore.Fortran.Elements
{
    public interface IScope
    {
        IMember Parent { get; set; }
    }
}