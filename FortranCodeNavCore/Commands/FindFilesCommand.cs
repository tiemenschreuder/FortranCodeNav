using System;
using System.Linq;
using FortranCodeNavCore.Fortran;
using FortranCodeNavCore.SyntaxTrees;
using VSIntegration;

namespace FortranCodeNavCore.Commands
{
    internal class FindFilesCommand : FortranCommandBase
    {
        public FindFilesCommand(VisualStudioIDE visualStudio, SyntaxTreeMaintainer syntaxTreeMaintainer) : base(visualStudio, syntaxTreeMaintainer)
        {
        }
        
        public void FindFiles()
        {
            try
            {
                var asts = SyntaxTreeMaintainer.GetSyntaxTrees();
                dialog = CreateListFindControl();
                dialog.DataSource = asts.Select(a => a.CodeFile).Cast<object>().ToList();
                dialog.AlwaysShowList = false;
                dialog.MaxResults = 15;
                dialog.SearchHintText = "Find files...";
                dialog.OnGetIconForItem = FortranIconProvider.GetIconForFile;
                dialog.OnGetHintForItem = o => ((CodeFile) o).ProjectName;
                dialog.DataMember = "FileName";
                dialog.ItemChosen += (s, e) => VisualStudio.Goto(s as CodeFile, 1, 1);
                dialog.Show();
            }
            catch (Exception e)
            {
                Log.Error("FindFiles failed", e, GetContext());
            }
        }
    }
}