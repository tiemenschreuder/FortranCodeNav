using System;
using System.Linq;
using FortranCodeNavCore.Fortran;
using FortranCodeNavCore.Fortran.Elements;
using FortranCodeNavCore.SyntaxTrees;
using VSIntegration;

namespace FortranCodeNavCore.Commands
{
    internal class ShowFileMembersCommand : FortranCommandBase
    {
        public ShowFileMembersCommand(VisualStudioIDE visualStudio, SyntaxTreeMaintainer syntaxTreeMaintainer) : base(visualStudio, syntaxTreeMaintainer)
        {
        }

        public void ShowFileMembers()
        {
            try
            {
                var currentFile = VisualStudio.GetCurrentCodeFile();
                if (currentFile == null)
                    return;

                var ast = SyntaxTreeMaintainer.GetSyntaxTree(currentFile);

                dialog = CreateListFindControl();
                dialog.DataSource = ast.GetAllMembers().Cast<object>().ToList();
                dialog.AlwaysShowList = true;
                dialog.SearchHintText = "Search current file...";
                dialog.MaxResults = 75; // (scrollable)
                dialog.OnGetIconForItem = FortranIconProvider.GetIconForMember;
                dialog.DataMember = "Name";
                dialog.ItemChosen += (s, e) =>
                    {
                        var chosenMember = (IMember) s;
                        VisualStudio.Goto(currentFile, chosenMember.Location.Line, chosenMember.Location.Offset);
                    };
                dialog.Show();
            }
            catch (Exception e)
            {
                Log.Error("ShowFileMembers failed", e, GetContext());
            }
        }
    }
}