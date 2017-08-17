using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FortranCodeNavCore.Fortran.Elements;

namespace FortranCodeNavCore.Fortran.Parser
{
    public class FortranSignatureParser
    {
        public void ParseMethodSignature(IMethod method)
        {
            if (method.Parameters != null)
                return; //already set

            var lines = FortranParseHelper.CleanLinesAndMerge(FortranParseHelper.ExtractMemberLines(method), false, true);
            var firstLine = lines.FirstOrDefault();

            if (firstLine == null)
            {
                throw new ArgumentException("Empty header");
            }
            if (!firstLine.StartsWith(method.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("Signature does not start with method name");
            }
                        
            var parameterString = firstLine.Substring(method.Name.Length);
            var resultKeyword = " result";
            var indexOfResult = parameterString.IndexOf(resultKeyword, StringComparison.InvariantCultureIgnoreCase);

            if (indexOfResult >= 0)
            {
                var resultString = parameterString.Substring(indexOfResult+resultKeyword.Length);
                parameterString = parameterString.Substring(0, indexOfResult);
                method.ResultVariableName = RemoveUndesiredCharacters(resultString).Trim();
            }

            parameterString = RemoveUndesiredCharacters(parameterString);
            var parameters = parameterString.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);

            var parameterList = new List<Variable>();
            foreach (var parameter in parameters)
            {
                var parameterName = parameter.Trim();

                if (String.IsNullOrEmpty(parameterName))
                    continue;

                var matchingDeclaration = FindDeclaration(method, parameterName);

                if (matchingDeclaration == null)
                {
                    if (method.ImplicitNone)
                    {
                        throw new ArgumentException(String.Format("Could not find matching declaration in method {0} for parameter {1}, while implicit none is true.", method.Name, parameterName));
                    }
                }
                else
                {
                    parameterList.Add(new Variable { Name = parameterName, Member = method, TypeString = matchingDeclaration.TypeString, IsBuildInType = matchingDeclaration.IsBuildInType });
                }
            }

            method.SetParameters(parameterList );
        }

        private Variable FindDeclaration(IMethod method, string name)
        {
            var var = method.LocalVariables.FirstOrDefault(lv => String.Equals(lv.Name,name, StringComparison.InvariantCultureIgnoreCase));

            if (var == null)
            {
                var matchingSubItem = method.GetSubItemsRecursive().OfType<IMember>().FirstOrDefault(m => String.Equals(m.Name, name, StringComparison.InvariantCultureIgnoreCase));

                if (matchingSubItem != null)
                {
                    var = new Variable() 
                    {
                        Name = matchingSubItem.Name, 
                        IsBuildInType = false,
                        TypeString = "(" + matchingSubItem.GetType().Name + " " + matchingSubItem.Name + ")" 
                    };
                }
            }

            return var;
        }

        private string RemoveUndesiredCharacters(string target)
        {
            target = target.Replace('(', ' ');
            target = target.Replace(')', ' ');
            target = target.Replace(';', ' ');
            return target;
        }
    }
}
