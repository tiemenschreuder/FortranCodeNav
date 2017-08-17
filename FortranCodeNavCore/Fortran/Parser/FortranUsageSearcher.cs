using System;
using System.Collections.Generic;
using System.Linq;
using FortranCodeNavCore.Commands.Matches;
using FortranCodeNavCore.Fortran.Elements;
using FortranCodeNavCore.SyntaxTrees;
using VSIntegration;

namespace FortranCodeNavCore.Fortran.Parser
{
    public class FortranUsageSearcher
    {
        public IList<UsageResult> FindInFiles(MemberOrSearchTerm memberUsageToFind, IEnumerable<SyntaxTree> syntaxTrees)
        {
            return syntaxTrees.SelectMany(st => FindInFile(memberUsageToFind, st)).ToList();
        }

        public IEnumerable<UsageResult> FindInFile(MemberOrSearchTerm memberUsageToFind, SyntaxTree syntaxTree)
        {
            var results = new List<UsageResult>();
            var fileContents = syntaxTree.CodeFile.Contents;
            var locations = FindInFileContents(memberUsageToFind, fileContents);
            
            foreach (var location in locations)
            {
                var enclosingMember = FortranParseHelper.GetEnclosingMember(syntaxTree.Members, location.Line);

                if (enclosingMember != null && enclosingMember == memberUsageToFind.Member)
                    continue; //definition/declaration, not usage

                results.Add(new UsageResult(syntaxTree, location, enclosingMember, location.LineStr));
            }

            return results;
        }

        public IEnumerable<LocationInFile> FindInFileContents(MemberOrSearchTerm memberUsageToFind, string fileContents)
        {
            var results = new List<LocationInFile>();
            
            //remove comments:
            var cleanedLines = FortranParseHelper.CleanLines(fileContents.Split('\n'), false, false).ToList();

            int lineNr = 0;
            foreach (var line in cleanedLines)
            {
                lineNr++;
                SearchInLine(line, memberUsageToFind.Name, lineNr, results);
            }

            return results;
        }

        private void SearchInLine(string line, string searchString, int lineNr, IList<LocationInFile> results)
        {
            int indexOf = -1;
            var lastIndex = 0;

            do
            {
                indexOf = line.IndexOf(searchString, lastIndex, StringComparison.InvariantCultureIgnoreCase);

                if (indexOf >= 0 && CheckNotFalsePositive(line, indexOf, searchString))
                {
                    results.Add(new LocationInFile(lineNr, indexOf, -1) { LineStr = line });
                }
                lastIndex = indexOf + 1;
            }
            while (indexOf >= 0);
        }

        private bool CheckNotFalsePositive(string line, int matchOffset, string searchString)
        {
            if (matchOffset > 0)
            {
                //begin of match is begin of word
                var previousChar = line[matchOffset - 1];
                if (FortranParseHelper.IsWordCharacter(previousChar))
                {
                    return false;
                }
            }
            var endOfMatch = searchString.Length + matchOffset;
            if (endOfMatch < line.Length)
            {
                //end of match is end of word
                var nextChar = line[endOfMatch];
                if (FortranParseHelper.IsWordCharacter(nextChar))
                {
                    return false;
                }
            }

            return !FortranParseHelper.IsIndexInsideCharacterString(line, matchOffset);
        }
    }

    public class MemberOrSearchTerm
    {
        public MemberOrSearchTerm(IMember member)
        {
            Member = member;
        }
        public MemberOrSearchTerm(string searchTerm)
        {
            this.searchTerm = searchTerm;
        }

        public IMember Member { get; private set; }
        private readonly string searchTerm;

        public string Name
        {
            get
            {
                if (Member != null)
                {
                    return Member.Name;
                }
                return searchTerm;
            }
        }
    }
}
