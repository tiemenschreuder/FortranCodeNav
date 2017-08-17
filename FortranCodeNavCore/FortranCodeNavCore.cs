using System;
using FortranCodeNavCore.Commands;
using FortranCodeNavCore.Forms;
using FortranCodeNavCore.SyntaxTrees;
using VSIntegration;
using VSIntegration.CodeComplete;

namespace FortranCodeNavCore
{
    public class FortranCodeNavCore
    {
        private VisualStudioIDE VisualStudio { get; set; }
        private SyntaxTreeMaintainer SyntaxTreeMaintainer { get; set; }
        private VSIntellisense IntellisenseController { get; set; }

        private Version Version { get; set; }
        
        public FortranCodeNavCore(VisualStudioIDE visualStudio)
        {
            Version = GetType().Assembly.GetName().Version;

            Log.Write(string.Format("Fortran CodeNav {0} initializing...", Version));
            
            SyntaxTreeMaintainer = new SyntaxTreeMaintainer(visualStudio);
            VisualStudio = visualStudio;
            VSLogger.RealLogger = new LogRedirector();
            
            VisualStudio.FilePassesFilterDelegate = KeepFile;
            VisualStudio.VisualStudioClosing += VisualStudioClosing;

            // intellisense
            var fortranIntellisense = new FortranIntellisenseProvider(VisualStudio, SyntaxTreeMaintainer);
            IntellisenseController = new VSIntellisense(VisualStudio)
                                         {
                                             OnCodeCompleteActivating = fortranIntellisense.OnCodeCompleteActivating,
                                             OnCodeCompleteUpdating = fortranIntellisense.OnCodeCompleteUpdating
                                         };
        }

        public static bool KeepFile(string filename)
        {
            return filename.EndsWith(".f90", StringComparison.CurrentCultureIgnoreCase) ||
                   filename.EndsWith(".f95", StringComparison.CurrentCultureIgnoreCase) || //just in case it works   
                   filename.EndsWith(".f03", StringComparison.CurrentCultureIgnoreCase) || //..
                   filename.EndsWith(".f", StringComparison.CurrentCultureIgnoreCase) ||   //..
                   filename.EndsWith(".for", StringComparison.CurrentCultureIgnoreCase);   //..
        }

        private static void VisualStudioClosing(object sender, EventArgs e)
        {
            Log.Write("Visual Studio closing...");
            Log.CloseLog();
        }

        public void ClearCaches()
        {
            SyntaxTreeMaintainer.ClearCaches();
        }

        public void BrowseTo()
        {
            new BrowseToCommand(VisualStudio, SyntaxTreeMaintainer).BrowseTo();
        }

        public void SyncSolutionExplorer()
        {
            new SyncSolutionExplorerCommand(VisualStudio, SyntaxTreeMaintainer).Sync();
        }

        public void FindFiles()
        {
            new FindFilesCommand(VisualStudio, SyntaxTreeMaintainer).FindFiles();
        }

        public void FindMembers()
        {
            new FindMembersCommand(VisualStudio, SyntaxTreeMaintainer).FindMembers();
        }

        public void ShowFileMembers()
        {
            new ShowFileMembersCommand(VisualStudio, SyntaxTreeMaintainer).ShowFileMembers();
        }

        public void FindUsage()
        {
            new FindUsageCommand(VisualStudio, SyntaxTreeMaintainer).FindUsage();
        }

        public void AboutDialog()
        {
            try
            {
                var about = new DebugForm(VisualStudio, Version, this);
                about.ShowDialog();
            }
            catch (Exception e)
            {
                Log.Error("AboutDialog failed", e);
            }
        }
    }
}
