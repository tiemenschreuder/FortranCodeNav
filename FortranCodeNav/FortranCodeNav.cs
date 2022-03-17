//------------------------------------------------------------------------------
// <copyright file="FortranCodeNav.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE80;
using EnvDTE;
using System.Threading;

namespace FortranCodeNav
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(FortranCodeNav.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
        Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class FortranCodeNav : AsyncPackage
    {
        /// <summary>
        /// FortranCodeNav GUID string.
        /// </summary>
        public const string PackageGuidString = "5d1bbb80-4a80-4b8c-a22a-c354e6a7fd91";

        /// <summary>
        /// Initializes a new instance of the <see cref="FortranCodeNav"/> class.
        /// </summary>
        public FortranCodeNav()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        private readonly VSIntegration.VisualStudioIDE vsIntegration = new VSIntegration.VisualStudioIDE();
        private FortranCodeNavCore.FortranCodeNavCore fortranCodeNav;

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            InitializeCore();
            await base.InitializeAsync(cancellationToken, progress);
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        private void InitializeCore()
        {
            var applicationObject = (DTE2)GetService(typeof(DTE));
            vsIntegration.OnConnect(applicationObject);
            fortranCodeNav = new FortranCodeNavCore.FortranCodeNavCore(vsIntegration);            
            
            AboutCommand.Initialize(this, fortranCodeNav.AboutDialog);
            BrowseToCommand.Initialize(this, fortranCodeNav.BrowseTo);
            FindUsageCommand.Initialize(this, fortranCodeNav.FindUsage);
            FindMembersCommand.Initialize(this, fortranCodeNav.FindMembers);
            FileMembersCommand.Initialize(this, fortranCodeNav.ShowFileMembers);
            SyncSolutionExplorerCommand.Initialize(this, fortranCodeNav.SyncSolutionExplorer);
            FindFilesCommand.Initialize(this, fortranCodeNav.FindFiles);
                        
            SetDefaultBinding(AboutCommand.CommandSet, AboutCommand.CommandId, "Global::Alt+F1");
            SetDefaultBinding(BrowseToCommand.CommandSet, BrowseToCommand.CommandId, "Global::Ctrl+Q");
            SetDefaultBinding(FindUsageCommand.CommandSet, FindUsageCommand.CommandId, "Global::Alt+Q");
            SetDefaultBinding(FindMembersCommand.CommandSet, FindMembersCommand.CommandId, "Global::Ctrl+Alt+Q");
            SetDefaultBinding(FileMembersCommand.CommandSet, FileMembersCommand.CommandId, "Global::Ctrl+Shift+Q");
            SetDefaultBinding(SyncSolutionExplorerCommand.CommandSet, SyncSolutionExplorerCommand.CommandId, "Global::Shift+Alt+Q");
            SetDefaultBinding(FindFilesCommand.CommandSet, FindFilesCommand.CommandId, "Global::Ctrl+Shift+Alt+Q");     
        }

        private void SetDefaultBinding(Guid commandSet, int commandId, string defaultBinding)
        {
            try
            {
                var command = vsIntegration.ApplicationObject.Commands.Item(commandSet, commandId);

                var currentBindings = (object[])command.Bindings;

                if (currentBindings == null || currentBindings.Length == 0)
                {
                    vsIntegration.RemoveKeyBindingFromAllCommands(defaultBinding);
                    command.Bindings = defaultBinding;
                }
            }
            catch (Exception e)
            {
                VSIntegration.VSLogger.Error("Failed to set keyboard binding for command: " + commandId, e);
            }
        }

        #endregion
    }
}
