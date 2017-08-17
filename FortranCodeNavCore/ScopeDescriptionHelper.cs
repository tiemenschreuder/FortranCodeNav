using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FortranCodeNavCore.Fortran.Elements;
using FortranCodeNavCore.Logging;

namespace FortranCodeNavCore
{
    public static class ScopeDescriptionHelper
    {
        public static string GetScopeDescription(IMember member)
        {
            var description = new StringBuilder();

            try
            {
                var codeFile = member.Root.CodeFile;

                var parent = member.Parent;
                var parents = new List<IMember>();
                while (parent != null)
                {
                    parents.Insert(0, parent);
                    parent = parent.Parent;
                }
                var scope = String.Join(".", parents.Select(p => p.Name).ToArray());
                description.Append(scope);
                description.Append(" (in ");
                description.Append(codeFile.FileName);
                description.Append(", ");
                description.Append(codeFile.ProjectName);
                description.Append(")");
            }
            catch (Exception e)
            {
                Log.Error(String.Format("GetScopeDescription failed for {0} in {1}", member.Name, member.Root.FileName), e);
            }

            return description.ToString();
        }
    }
}