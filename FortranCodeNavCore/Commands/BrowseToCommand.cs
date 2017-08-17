using System;
using System.Collections.Generic;
using System.Linq;
using FortranCodeNavCore.Commands.Matches;
using FortranCodeNavCore.Fortran;
using FortranCodeNavCore.Fortran.Elements;
using FortranCodeNavCore.Fortran.Parser;
using FortranCodeNavCore.SyntaxTrees;
using VSIntegration;

namespace FortranCodeNavCore.Commands
{
    public class BrowseToCommand : FortranCommandBase
    {
        public BrowseToCommand(VisualStudioIDE visualStudio, SyntaxTreeMaintainer syntaxTreeMaintainer) : base(visualStudio, syntaxTreeMaintainer)
        {
        }

        public void BrowseTo()
        {
            try
            {
                if (!VisualStudio.IsDocumentOpen())
                    return;

                var cursorPosition = VisualStudio.GetCursorPositionInLine();
                var line = VisualStudio.GetCurrentLine();

                int beginIndex;
                var elementName = FortranParseHelper.GetElementName(line, cursorPosition, out beginIndex);

                if (String.IsNullOrEmpty(elementName))
                    return;

                var memberMatches = SyntaxTreeMaintainer.GetSyntaxTrees()
                                        .SelectMany(st => st.GetAllMembers()
                                                      .Where(m => Equals(elementName, m.Name))
                                                      .Select(m => new SearchResult { SyntaxTree = st, Member = m }))
                                        .ToList();
                
                if (memberMatches.Any())
                {
                    BrowseToCore(memberMatches);
                }
                else
                {
                    BrowseToVariable(elementName);
                }
            }
            catch (Exception e)
            {
                Log.Error("BrowseTo failed", e, GetContext());
            }
        }

        private void BrowseToVariable(string elementName)
        {
            //find variables
            var elementsInScope = FortranSyntaxTreeModel.GetElementsAvailableInScope(
                SyntaxTreeMaintainer.GetSyntaxTrees(),
                GetCurrentMember());

            var firstMatch = elementsInScope.OfType<Variable>().FirstOrDefault(el => Equals(elementName, el.Name));

            if (firstMatch == null)
            {
                // second chance: try items in types:
                var typesInScope = elementsInScope.OfType<Fortran.Elements.Type>().ToList();

                foreach (var type in typesInScope)
                    new FortranDeclarationParser().ParseDeclarationsAndUses(type);                

                firstMatch = elementsInScope.OfType<Fortran.Elements.Type>()
                    .SelectMany(t => t.LocalVariables)
                    .FirstOrDefault(el => Equals(elementName, el.Name));

                if (firstMatch == null)
                {
                    return;
                }
            }

            VisualStudio.Goto(firstMatch.Member.Root.CodeFile, firstMatch.Location.Line,
                              firstMatch.Location.Offset + 1);
        }

        private IMember GetCurrentMember()
        {
            return GetCurrent<IMember>() ?? GetCurrent<Module>();
        }

        private bool Equals(string a, string b)
        {
            return string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
        }

        private void BrowseToCore(IEnumerable<SearchResult> _matches)
        {
            var matches = _matches.ToList();

            if (matches.Count == 1)
            {
                var match = matches.First();

                if (match.Member is Interface)
                {
                    BrowseToCore(FortranSyntaxTreeModel.FindInterfaceReferences(match.Member as Interface, match.SyntaxTree));
                }
                else
                {
                    VisualStudio.Goto(match.SyntaxTree.CodeFile, match.Member.Location.Line,
                                      match.Member.Location.Offset);
                }
            }
            else if (matches.Count > 1)
            {
                //let user make selection & then do recursive call
                dialog = CreateListFindControl();
                dialog.DataSource = matches.Select(m => m.Member).OfType<object>().ToList();
                dialog.AlwaysShowList = true;
                dialog.HideSearchDialog = true;
                dialog.MaxResults = 25;
                dialog.OnGetIconForItem = FortranIconProvider.GetIconForMember;
                dialog.OnGetHintForItem = o => ((IMember) o).GetScopeDescription();
                dialog.DataMember = "Name";
                dialog.ItemChosen += (s, e) =>
                    {
                        var chosenMember = s as IMember;
                        var match = matches.First(m => m.Member == chosenMember);
                        BrowseToCore(new[] { match }); //recursive: perhaps user chose interface
                    };
                dialog.Show();
            }
        }
    }
}