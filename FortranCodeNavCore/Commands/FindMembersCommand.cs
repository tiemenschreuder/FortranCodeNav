using System;
using System.Linq;
using FortranCodeNavCore.Fortran;
using FortranCodeNavCore.Fortran.Elements;
using FortranCodeNavCore.SyntaxTrees;
using VSIntegration;

namespace FortranCodeNavCore.Commands
{
    internal class FindMembersCommand : FortranCommandBase
    {
        public FindMembersCommand(VisualStudioIDE visualStudio, SyntaxTreeMaintainer syntaxTreeMaintainer) : base(visualStudio, syntaxTreeMaintainer)
        {
        }

        public void FindMembers()
        {
            try
            {
                var asts = SyntaxTreeMaintainer.GetSyntaxTrees();
                dialog = CreateListFindControl();
                dialog.DataSource = asts.SelectMany(a => a.GetAllMembers()).Cast<object>().ToList();
                dialog.AlwaysShowList = false;
                dialog.MaxResults = 15;
                dialog.SearchHintText = "Search solution...";
                dialog.OnGetIconForItem = FortranIconProvider.GetIconForMember;
                dialog.OnGetHintForItem = o => ((IMember) o).GetScopeDescription();
                dialog.DataMember = "Name";
                dialog.ItemChosen += (s, e) =>
                    {
                        var chosenMember = (IMember)s;
                        VisualStudio.Goto(chosenMember.Root.CodeFile, chosenMember.Location.Line, chosenMember.Location.Offset);
                    };

                dialog.Show();
            }
            catch (Exception e)
            {
                Log.Error("FindMembers failed", e, GetContext());
            }
        }
    }
}