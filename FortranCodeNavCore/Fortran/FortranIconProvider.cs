using System.Drawing;
using FortranCodeNavCore.Fortran.Elements;
using FortranCodeNavCore.Properties;
using VSIntegration;

namespace FortranCodeNavCore.Fortran
{
    public static class FortranIconProvider
    {
        private static readonly Bitmap FunctionIcon = Resources.Function_Icon;
        private static readonly Bitmap ModuleIcon = Resources.Module_Icon;
        private static readonly Bitmap SubroutineIcon = Resources.Subroutine_Icon;
        private static readonly Bitmap InterfaceIcon = Resources.Interface_Icon;
        private static readonly Bitmap TypeIcon = Resources.Type_Icon;
        private static readonly Bitmap UserTypeVariableIcon = Resources.variable_type;
        private static readonly Bitmap FloatVariableIcon = Resources.variable_float;
        private static readonly Bitmap IntVariableIcon = Resources.variable_int;
        private static readonly Bitmap CharVariableIcon = Resources.variable_char;
        private static readonly Bitmap LogicalVariableIcon = Resources.variable_logical;

        public static Bitmap GetIconForMember(object item)
        {
            if (item is Function)
            {
                return FunctionIcon;
            }
            else if (item is Module)
            {
                return ModuleIcon;
            }
            else if (item is Subroutine)
            {
                return SubroutineIcon;
            }
            else if (item is Interface)
            {
                return InterfaceIcon;
            }
            else if (item is Type)
            {
                return TypeIcon;
            }
            else if (item is Variable)            
            {
                var variable = item as Variable;

                if (!variable.IsBuildInType)
                {
                    return UserTypeVariableIcon;
                }

                switch (variable.TypeString)
                {
                    case "double precision":
                    case "real":
                        return FloatVariableIcon;
                    case "integer":
                        return IntVariableIcon;
                    case "character":
                        return CharVariableIcon;
                    case "logical":
                        return LogicalVariableIcon;                    
                }
            }
            return null;
        }

        public static Bitmap GetIconForFile(object item)
        {
            var codeFile = item as CodeFile;
            return codeFile != null ? FunctionIcon : null;
        }
    }
}