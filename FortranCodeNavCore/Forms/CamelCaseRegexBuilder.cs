using System;
using System.Text;
using System.Text.RegularExpressions;

namespace FortranCodeNavCore.Forms
{
    public class CamelCaseRegexBuilder
    {
        public Regex BuildRegex(string searchTerm)
        {
            var regexString = new StringBuilder();
            
            foreach (var c in searchTerm)
            {
                if (Char.IsUpper(c))
                {
                    if (regexString.Length != 0) //first char is leading
                    {
                        regexString.Append(".*?");
                    }
                    regexString.Append("[" + c + Char.ToLower(c) + "]");
                }
                else
                {
                    regexString.Append(c);
                }
            }

            regexString.Insert(0, "^"); //enforce begin of string

            Regex regex;
            try
            {
                regex = new Regex(regexString.ToString());
            }
            catch(ArgumentException)
            {
                regex = new Regex("should_match_nothing"); //can happen when user enters invalid pattern, such as "\"
            }
            return regex;
        }
    }
}
