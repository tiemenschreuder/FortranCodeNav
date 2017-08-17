using System.Collections.Generic;
using System.Text;
using System;

namespace FortranCodeNavCore.Fortran.Parser
{
    public class FortranStatementParser
    {
        public string FindEffectiveStatementAtOffset(string line, int offsetInLine, out int beginOfStatement)
        {
            var endOfStatement = offsetInLine - 1;
            var parenthesisLevel = 0;
            int i;
            var statement = new StringBuilder();
            
            for (i = endOfStatement; i >= 0; i--)
            {
                var c = line[i];

                if (c == ')')
                {
                    if (statement.Length == 0)
                        break;
                    parenthesisLevel++;
                }
                else if (parenthesisLevel > 0)
                {
                    //next
                    if (c == '(')
                        parenthesisLevel--;
                }
                else
                {
                    if (FortranParseHelper.IsNonStatementCharacter(c))
                    {
                        break;
                    }
                    else
                    {
                        statement.Insert(0, c);
                    }
                }
            }

            beginOfStatement = i+1;
            var originalBegin = beginOfStatement;

            for (int j = 0; j < statement.Length; j++)
            {
                var c = statement[j];
                if (FortranParseHelper.IsWhiteSpace(c))
                {
                    beginOfStatement++;
                }
                else
                {
                    break;
                }
            }

            return statement.ToString().Substring(beginOfStatement-originalBegin);
        }
        
        public string FindMethodAtOffset(string currentLine, int currentIndex)
        {
            var parenthesisLevel = 0;
            var functionEnd = -1;

            for (int i = currentIndex-1; i >= 0; i--)
            {
                var c = currentLine[i];

                if (c == ')')
                {
                    parenthesisLevel++;
                }
                else if (c == '(')
                {
                    parenthesisLevel--;

                    if (parenthesisLevel == -1)
                    {
                        functionEnd = i;
                        break;
                    }
                }
            }

            if (functionEnd >= 0)
            {
                var lineWithFunctionName = currentLine.Substring(0, functionEnd).TrimEnd();
                var methodName = "";
                try
                {
                    var beginOfMethodName = -1;
                    methodName = FortranParseHelper.GetElementName(lineWithFunctionName, lineWithFunctionName.Length - 1, out beginOfMethodName);
                }
                catch (Exception e)
                {
                    Log.Error(String.Format("GetElementName failed (line: {0})", lineWithFunctionName), e);
                }

                return methodName;
            }
            return null;
        }
    }
}