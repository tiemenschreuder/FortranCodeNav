using System;
using FortranCodeNavCore.SyntaxTrees;
using VSIntegration;

namespace FortranCodeNavCore.Commands
{
    public class SyncSolutionExplorerCommand : FortranCommandBase
    {
        public SyncSolutionExplorerCommand(VisualStudioIDE visualStudio, SyntaxTreeMaintainer syntaxTreeMaintainer) : base(visualStudio, syntaxTreeMaintainer)
        {
        }

        public void Sync()
        {
            try
            {
                VisualStudio.JumpToActiveFileInSolutionExplorer();
            }
            catch (Exception e)
            {
                Log.Error("SyncSolutionExplorer failed", e, GetContext());
            }
        }
    }
}