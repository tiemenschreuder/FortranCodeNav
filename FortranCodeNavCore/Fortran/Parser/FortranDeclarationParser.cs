using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FortranCodeNavCore.Fortran.Elements;
using VSIntegration;

namespace FortranCodeNavCore.Fortran.Parser
{
    public class FortranDeclarationParser
    {
        //a couple of hours on regex:
        private static readonly Regex TypeRegex = new Regex(@"^(dimension|procedure|external|real|character|integer|logical|double precision|type|complex)(\W.*)$", RegexOptions.IgnoreCase);
        private static readonly Regex VariableNamesRegex = new Regex(@"(?:(?:([A-Za-z]\w*)\s*(?:\S*?[\(=\*].*?)?\s*,\s*)|(?:([A-Za-z]\w*)\s*(?:\S*?[\(=\*].*?)?\W?$))");
        private static readonly Regex VariableOfCustomTypeRegex = new Regex(@"^type\s*\(\s*(\w+)\s*\)", RegexOptions.IgnoreCase);
        private static readonly Regex UseRegex = new Regex(@"use\s+(\w+)", RegexOptions.IgnoreCase);

        public void ParseDeclarationsAndUses(IMember member)
        {
            if (member.LocalVariables.Any() || member.Uses.Any())
                return; //already parsed (on file modification they are cleared)

            member.ClearLocalVariables();
            member.ClearUses();
            
            var memberLines = FortranParseHelper.ExtractMemberLines(member);
            var cleanedLines = FortranParseHelper.CleanLines(memberLines, false, false).ToArray();

            var mergedLines = FortranParseHelper.CleanLinesAndMerge(memberLines, true, true);

            var lineHint = 1;

            foreach (var line in mergedLines)
            {
                if (TryReadAndEat("#", line)) //preprocessor directives
                    continue;
                if (TryReadAndEat("private", line)) //preprocessor directives
                    continue;
                if (TryReadAndEat("public", line)) //preprocessor directives
                    continue;
                if (TryReadAndEat("write", line)) //allowed?!?!?!
                    continue;
                if (TryReadAndEat("implicit ", line))
                {
                    if (line.IndexOf("none", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        member.ImplicitNone = true;
                    }
                    continue;
                }
                if (TryReadUses(member, line))
                    continue;
                if (TryReadVariableDeclaration(member, line, cleanedLines, ref lineHint))
                    continue;
                if (TryReadAndEat("parameter", line))
                    continue;
                if (TryReadAndEat("data", line))
                    continue;
                if (TryReadAndEat("common", line))
                    continue;
                if (TryReadAndEat("save", line))
                    continue;
                if (TryReadAndEat("intent", line))
                    continue;
                if (TryReadAndEat("include", line))
                    continue;
                //if we get here, it's the end of variable declarations because the line is non-empty and 
                //non-commment, yet doesn't start with an expected string (variable, implicit, etc)
                break;
            }
        }

        private static bool TryReadUses(IMember member, string line)
        {
            var match = UseRegex.Match(line);

            if (match.Success)
            {
                var useModule = match.Groups[1].Value;
                member.AddUse(new Use { Module = useModule, Parent = member });

            }
            return match.Success;
        }

        private static bool TryReadVariableDeclaration(IMember member, string text, string[] allLines, ref int lineHint)
        {
            var match = TypeRegex.Match(text);

            if (match.Success)
            {
                var variableType = match.Groups[1].Value;
                var names = match.Groups[2].Value;

                var doubleDots = names.IndexOf("::", StringComparison.InvariantCultureIgnoreCase);
                var variableNamesString = doubleDots >= 0 ? names.Substring(doubleDots + 2) : names;

                variableNamesString = RemoveBetweenSlashes(RemoveParenthesis(RemoveBetweenQuotes(variableNamesString)));

                var nameMatches = VariableNamesRegex.Matches(variableNamesString);

                foreach(var variableNameMatch in nameMatches.OfType<Match>())
                {
                    var variableName = variableNameMatch.Groups[1].Value; //first group: variable with comma behind it
                    if (String.IsNullOrEmpty(variableName))
                    {
                        variableName = variableNameMatch.Groups[2].Value; //second group: only variable, or last variable in list
                    }

                    var location = FindFirstLocationOfElementNameInLines(member, variableName, allLines, ref lineHint);
                    
                    if (variableType == "type")
                    {
                        var typeMatch = VariableOfCustomTypeRegex.Match(text);
                        if (typeMatch.Success)
                        {
                            var actualType = typeMatch.Groups[1].Value;
                            var variable = new Variable { TypeString = actualType, Name = variableName, Location = location, Member = member, IsBuildInType = false };
                            member.AddLocalVariable(variable);
                        }
                        else
                        {
                            throw new InvalidOperationException("Unable to parse type");
                        }
                    }
                    else
                    {
                        var variable = new Variable { TypeString = variableType.ToLower(), Name = variableName, Location = location, Member = member, IsBuildInType = true };
                        member.AddLocalVariable(variable);
                    }
                }
                if (nameMatches.Count == 0)
                {
                    throw new InvalidOperationException("No variable names found.");
                }
            }
            return match.Success;
        }

        private static LocationInFile FindFirstLocationOfElementNameInLines(IMember member, string variableName, string[] allLines, ref int lineHint)
        {
            for (var i = lineHint; i < allLines.Length; i++)
            {
                var line = allLines[i];
                var startIndex = 0;
                int indexInLine;
                do
                {
                    indexInLine = line.IndexOf(variableName, startIndex, StringComparison.InvariantCultureIgnoreCase);
                    startIndex = indexInLine + 2; //+1 = space, so at least +2
                    if (indexInLine >= 0)
                    {
                        if (indexInLine > 0 && FortranParseHelper.IsWordCharacter(line[indexInLine - 1]))
                            continue; //part of other string

                        var endOfNameIndex = indexInLine + variableName.Length;
                        if (endOfNameIndex < line.Length && FortranParseHelper.IsWordCharacter(line[endOfNameIndex]))
                            continue;

                        lineHint = i;
                        return new LocationInFile(member.Location.Line + i, indexInLine, -1);
                    }
                } while (indexInLine >= 0 && startIndex < line.Length); //while matches in line
            }
            throw new ArgumentException(string.Format("Cannot find variable {0} in variable declarations, in file {1}",
                                                      variableName, member.Root.CodeFile.ProjectName));
        }

        private static string RemoveBetweenQuotes(string input)
        {
            return FortranParseHelper.RemoveBetweenCharacter(input, '\'');
        }

        private static string RemoveBetweenSlashes(string input)
        {
            return FortranParseHelper.RemoveBetweenCharacter(input, '/');
        }

        private static string RemoveParenthesis(string variableNamesString)
        {
            return RemoveEnclosings(variableNamesString, '(', ')');
        }

        private static string RemoveEnclosings(string input, char left, char right)
        {
            int nestingLevel = 0;
            var newString = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];

                if (c == left)
                {
                    nestingLevel++;
                }

                if (nestingLevel == 0)
                    newString.Append(c);

                if (c == right)
                {
                    nestingLevel--;
                }
            }

            return newString.ToString(); 
        }

        private static bool TryReadAndEat(string str, string line)
        {
            return line.StartsWith(str, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}