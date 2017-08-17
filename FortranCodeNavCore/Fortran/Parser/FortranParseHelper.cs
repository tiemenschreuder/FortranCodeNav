using System;
using System.Collections.Generic;
using System.Linq;
using FortranCodeNavCore.Fortran.Elements;
using Type = FortranCodeNavCore.Fortran.Elements.Type;
using System.Text;

namespace FortranCodeNavCore.Fortran.Parser
{
    public static class FortranParseHelper
    {
        public static string[] ExtractMemberLines(IMember member, bool keepSubMembersAsEmptyLines=true)
        {
            var startOfMember = member.Location.AbsoluteOffset;
            var endOfMember = member.EndLocation.AbsoluteOffset;

            //if a member has submethods (eg, a module), parse up to the first submethod
            var subMember = member.SubItems.OfType<IMethod>().FirstOrDefault();
            if (subMember != null)
            {
                endOfMember = subMember.Location.AbsoluteOffset;
            }

            var memberString = member.Root.CodeFile.Contents.Substring(startOfMember, endOfMember - startOfMember);
            var lines = memberString.Split(new[] { '\n' }).ToList();

            var startLine = member.Location.Line;

            var subItemsMixedWithDeclarations = member.SubItems.OfType<IMember>().
                Where(si => si is Type || si is Interface).
                Reverse();

            foreach (var subItem in subItemsMixedWithDeclarations)
            {
                var subItemStartsAt = (subItem.Location.Line - startLine);
                var numLinesForTypeDefinition = (subItem.EndLocation.Line - subItem.Location.Line) + 1;

                if (keepSubMembersAsEmptyLines)
                {
                    for (int i = 0; i < numLinesForTypeDefinition; i++)
                    {
                        lines[subItemStartsAt + i] = "";
                    }
                }
                else
                {
                    lines.RemoveRange(subItemStartsAt, numLinesForTypeDefinition);
                }
            }

            return lines.ToArray();
        }

        public static IEnumerable<string> CleanLines(IEnumerable<string> lines, bool removeEmptyLines, bool trimBegin)
        {
            return lines
                .Select(line => CleanLine(line, trimBegin))
                .Where(l => !removeEmptyLines || !String.IsNullOrEmpty(l))
                .ToList();
        }

        private static string CleanLine(string line, bool trimBegin)
        {
            var cleanedLine = GetStringUptoEndOfLineOrBeginOfComment(line, 0);

            if (trimBegin)
                return cleanedLine.Trim();
            else
                return cleanedLine.TrimEnd();
        }

        public static IEnumerable<string> CleanLinesAndMerge(IEnumerable<string> lines, bool skipHeader, bool removeEmptyLines)
        {
            var cleanedLines = CleanLines(lines, removeEmptyLines, true);                

            var merged = MergeLines(cleanedLines);

            if (skipHeader)
                return merged.Skip(1); //skip member header
            else
                return merged;
        }

        private static IEnumerable<string> MergeLines(IEnumerable<string> lines)
        {
            string lastLine = "";

            foreach (var line in lines)
            {
                lastLine = Merge(lastLine, line);
                if (!line.TrimEnd().EndsWith("&"))
                {
                    if (!String.IsNullOrEmpty(lastLine))
                    {
                        yield return lastLine;
                    }
                    lastLine = "";
                }
            }
        }

        public static string RemoveBetweenCharacter(string input, char character)
        {
            var insideSlashes = false;
            var newString = new StringBuilder();

            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];

                if (c == character)
                {
                    insideSlashes = !insideSlashes;
                    continue;
                }

                if (!insideSlashes)
                    newString.Append(c);
            }

            return newString.ToString();
        }
        
        private static string Merge(string lastLine, string line)
        {
            line = line.TrimEnd();
            line = line.Replace('&', ' ');
            return RemoveWhitespace(lastLine + line); 
        }

        /// <summary>
        /// Remove duplicate spaces \ tabs..
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string RemoveWhitespace(string line)
        {
            var sb = new StringBuilder(line.Length);
            var previousWasWhitespace = false;
            foreach (var c in line)
            {
                if (IsWhiteSpace(c))
                {
                    if (!previousWasWhitespace)
                    {
                        sb.Append(' ');
                    }
                    previousWasWhitespace = true;
                }
                else
                {
                    sb.Append(c);
                    previousWasWhitespace = false;
                }
            }

            return sb.ToString();
        }

        public static bool IsWordCharacter(char c)
        {
            return Char.IsLetter(c) || Char.IsDigit(c) || c == '_';
        }

        public static bool IsNonStatementCharacter(char c)
        {
            if (c == '_')
                return false;
            if (Char.IsDigit(c))
                return false;
            if (Char.IsLetter(c))
                return false;
            if (IsWhiteSpace(c))
                return false;
            if (c == '%')
                return false;
            return true;
        }

        public static string GetStringUptoEndOfLineOrBeginOfComment(string fileContents, int readPosition)
        {
            int i;
            for (i = readPosition; i < fileContents.Length; i++)
            {
                var c = fileContents[i];
                if (c == '\n' || c == '\r' || c == '!')
                {
                    break;
                }
            }
            return i > readPosition ? fileContents.Substring(readPosition, i - readPosition) : "";
        }

        public static bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        public static bool IsIndexInsideCharacterString(string line, int index)
        {
            var insideCharacterString = false;
            var insideQuoteString = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (!insideCharacterString && c == '"')
                {
                    insideQuoteString = !insideQuoteString; //toggle                    
                }
                else if (!insideQuoteString && c == '\'')
                {
                    insideCharacterString = !insideCharacterString; //toggle
                }

                if (i == index)
                {
                    return insideCharacterString || insideQuoteString;
                }
            }
            return false;
        }

        public static string GetElementName(string line, int cursorIndex, out int beginOfMethodName)
        {
            beginOfMethodName = 0;

            var endOfMethodName = line.Length;

            if (line.Length == 0)
            {
                return "";
            }

            if (cursorIndex >= line.Length)
            {
                cursorIndex = line.Length - 1;
            }

            if (!IsWordCharacter(line[cursorIndex])) 
            {
                //if we start with the caret at the end of the word
                cursorIndex--;
            }

            for (int i = cursorIndex; i >= 0; i--)
            {
                if (IsWordCharacter(line[i]))
                    continue;
                beginOfMethodName = i + 1;
                break;
            }

            for (int i = cursorIndex; i < line.Length; i++)
            {
                if (IsWordCharacter(line[i]))
                    continue;
                endOfMethodName = i;
                break;
            }

            if (endOfMethodName >= 0 && beginOfMethodName >= 0 && endOfMethodName > beginOfMethodName)
            {
                var elementName = line.Substring(beginOfMethodName, endOfMethodName - beginOfMethodName);
                return Char.IsDigit(elementName[0]) ? "" : elementName;
            }
            return "";
        }

        public static IList<string> SplitElementsInStatement(string cleanedStatement, string fullStatement, ref int beginOfStatement, out string filter)
        {
            var elements = cleanedStatement.Split('%');
            filter = elements[elements.Length - 1].Trim();

            if (String.IsNullOrEmpty(filter))
                beginOfStatement += fullStatement.Length;
            else
                beginOfStatement += fullStatement.LastIndexOf(filter);

            return elements.Take(elements.Length - 1).Select(e => e.Trim()).ToList();
        }

        public static IMember GetEnclosingMember(IEnumerable<IMember> members, int lineNumber)
        {
            var matchingMember = members.FirstOrDefault(
                m => m.Location.Line <= lineNumber && m.EndLocation.Line >= lineNumber);

            if (matchingMember != null)
            {
                var subMembers = matchingMember.SubItems.OfType<IMember>().ToList();
                if (subMembers.Count > 0)
                {
                    return GetEnclosingMember(subMembers, lineNumber) ?? matchingMember;
                }
            }
            return matchingMember;
        }
    }
}