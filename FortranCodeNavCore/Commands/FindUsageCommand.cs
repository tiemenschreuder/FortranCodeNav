using System;
using System.Drawing;
using System.Linq;
using FortranCodeNavCore.Commands.Matches;
using FortranCodeNavCore.Fortran.Parser;
using FortranCodeNavCore.SyntaxTrees;
using VSIntegration;

namespace FortranCodeNavCore.Commands
{
    internal class FindUsageCommand : FortranCommandBase
    {
        public FindUsageCommand(VisualStudioIDE visualStudio, SyntaxTreeMaintainer syntaxTreeMaintainer) : base(visualStudio, syntaxTreeMaintainer)
        {
        }

        public void FindUsage()
        {
            try
            {
                if (!VisualStudio.IsDocumentOpen())
                    return;

                var coords = VisualStudio.GetCaretPositionInScreenCoordinates();
                var cursorPosition = VisualStudio.GetCursorPositionInLine();
                var line = VisualStudio.GetCurrentLine();

                int beginIndex;
                var memberName = FortranParseHelper.GetElementName(line, cursorPosition, out beginIndex);

                if (String.IsNullOrEmpty(memberName.Trim()))
                    return;

                var asts = SyntaxTreeMaintainer.GetSyntaxTrees().ToList();

                var matchingMember = asts.SelectMany(a => a.GetAllMembers())
                                         .FirstOrDefault(m => String.Equals(m.Name, memberName, StringComparison.InvariantCultureIgnoreCase));

                var searchObj = matchingMember != null ?
                                    new MemberOrSearchTerm(matchingMember) :
                                    new MemberOrSearchTerm(memberName);

                var usages = new FortranUsageSearcher().FindInFiles(searchObj, asts);

                if (usages.Count == 0)
                {

                    return;
                }
                if (usages.Count == 1)
                {
                    GotoUsage(usages.First());
                    return;
                }

                dialog = CreateListFindControl();
                dialog.Location = coords;
                dialog.MaxResults = 500;//something big
                dialog.DataSource = usages.OfType<object>().ToList();
                dialog.AlwaysShowList = true;
                dialog.HideSearchDialog = true;
                dialog.OnGetIconForItem = o => null;
                dialog.DataMember = "Name";
                dialog.HintMember = "Hint";
                dialog.OnGetHintEmphasisRangeForItem = o =>
                    {
                        var usageRes = (UsageResult)o;
                        var start = usageRes.Location.Offset;
                        var end = start + memberName.Length;
                        return new Point(start, end);
                    };
                dialog.ItemChosen += (chosenUsage, e) => GotoUsage((UsageResult) chosenUsage);
                dialog.Show();
            }
            catch (Exception e)
            {
                Log.Error("FindUsage failed", e, GetContext());
            }
        }

        private void GotoUsage(UsageResult result)
        {
            VisualStudio.Goto(result.SyntaxTree.CodeFile, result.Location.Line, result.Location.Offset + 1);
        }
    }
}