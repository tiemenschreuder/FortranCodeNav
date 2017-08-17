using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.TextManager.Interop;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace VSIntegration
{
    public class VisualStudioIDE
    {
        #region Delegates

        public delegate void CodeFileEventHandler(object sender, CodeFile codeFile);

        public delegate bool FileFilterDelegate(string filename);

        #endregion

        public DocumentManager.OnAfterKeyPressHandler AfterKeyPress;
        public DocumentManager.OnBeforeKeyPressHandler BeforeKeyPress;
        public event EventHandler CompleteWordRequested;
        public FileFilterDelegate FilePassesFilterDelegate;
        private IList<CodeFile> codeFiles;
        private DTEEvents dteEvents;
        private ProjectItemsEvents projectItemEvents;
        private SolutionEvents solutionEvents;
        private TextEditorEvents textEditorEvents;
        private DocumentEvents documentEvents;
        private CommandEvents commandEvents;

        public VisualStudioIDE() : this(null)
        {
        }

        private VisualStudioIDE(DocumentManager documentManager)
        {
            DocumentManager = documentManager ?? new DocumentManager();
        }

        public DTE2 ApplicationObject { get; set; }
        
        private DocumentManager DocumentManager { get; set; }

        /// <summary>
        /// Get the caret position
        /// </summary>
        public Point GetCaretPositionInScreenCoordinates()
        {
            var guiInfo = new Native.GUITHREADINFO();
            guiInfo.cbSize = (uint) Marshal.SizeOf(guiInfo);
            Native.GetGUIThreadInfo(0, out guiInfo);

            var coordinates = new Point((int) guiInfo.rcCaret.Right, (int) guiInfo.rcCaret.Bottom);
            IntPtr editorHwnd = guiInfo.hwndCaret;

            if (editorHwnd == IntPtr.Zero)
            {
                return GetCaretPositionInScreenCoordinatesForVs2010();
            }
            else
            {
                Native.ClientToScreen(editorHwnd, ref coordinates);
                return coordinates;
            }
        }

        private Point GetCaretPositionInScreenCoordinatesForVs2010()
        {
            var serviceProvider = (IServiceProvider) ApplicationObject;
            Guid SID = typeof (SVsTextManager).GUID;
            Guid IID = typeof (IVsTextManager).GUID;
            IntPtr output;
            serviceProvider.QueryService(ref SID, ref IID, out output);
            var txtMgr = (IVsTextManager) Marshal.GetObjectForIUnknown(output);
            IVsTextView textView;
            txtMgr.GetActiveView(1, null, out textView);

            object wpfTextView = textView.GetType().GetProperty("WpfTextView").GetValue(textView, null);
            var wpfViewType = wpfTextView.GetType();
            var viewPortTop = (double) wpfViewType.GetProperty("ViewportTop").GetValue(wpfTextView, null);

            object caret = wpfTextView.GetType().GetProperty("Caret").GetValue(wpfTextView, null);
            Type caretType = caret.GetType();
            var left = (double) caretType.GetProperty("Left").GetValue(caret, null);
            var bottom = (double) caretType.GetProperty("Bottom").GetValue(caret, null);

            IntPtr editorHwnd = textView.GetWindowHandle();
            var coordinates = new Point((int) left + 33, 5 + (int) (bottom - viewPortTop));

            Native.ClientToScreen(editorHwnd, ref coordinates);

            return coordinates;
        }
        
        public void OnConnect(DTE2 applicationObject)
        {
            ApplicationObject = applicationObject;
            
            DocumentManager.BeforeKeyPress = OnBeforeKeyPress;
            DocumentManager.AfterKeyPress = OnAfterKeyPress;
            DocumentManager.Setup(applicationObject);
            
            var events = (Events2) applicationObject.Events;

            solutionEvents = events.SolutionEvents;
            projectItemEvents = events.ProjectItemsEvents;
            textEditorEvents = events.get_TextEditorEvents(null);
            documentEvents = events.DocumentEvents;
            dteEvents = events.DTEEvents;

            dteEvents.OnBeginShutdown += OnBeginShutdown;
            solutionEvents.Opened += OnSolutionOpened;
            solutionEvents.BeforeClosing += OnSolutionClosing;
            
            projectItemEvents.ItemAdded += OnSolutionItemAdded;
            projectItemEvents.ItemRenamed += OnSolutionItemRenamed;
            projectItemEvents.ItemRemoved += OnSolutionItemRemoved;
            
            textEditorEvents.LineChanged += OnTextEditorLineChanged;
            documentEvents.DocumentSaved += DocumentEvents_DocumentSaved;

            Command cmd = ApplicationObject.Commands.Item("Edit.CompleteWord", -1);
            commandEvents = ApplicationObject.Events.get_CommandEvents(cmd.Guid, cmd.ID);
            commandEvents.BeforeExecute += OnBeforeCompleteWordRequested;
        }

        public void RemoveKeyBindingFromAllCommands(string keyBindingWithScope)
        {
            var keyBinding = TrimScopeFromKeyBinding(keyBindingWithScope);
            var commands = ApplicationObject.Commands;
            foreach (Command command in commands)
            {
                var bindings = ((object[])command.Bindings).Cast<string>().ToArray();
                var newBindings = new List<object>();
                for (var i = 0; i < bindings.Length; i++)
                {
                    var binding = TrimScopeFromKeyBinding(bindings[i]);
                    if (!keyBinding.Equals(binding))
                        newBindings.Add(bindings[i]);
                }

                if (bindings.Length != newBindings.Count)
                    command.Bindings = newBindings.ToArray();
            }
        }

        private static string TrimScopeFromKeyBinding(string keyBindingWithScope)
        {
            var scopeEnd = keyBindingWithScope.IndexOf("::", StringComparison.Ordinal) + 2;

            return scopeEnd >= 0 ? keyBindingWithScope.Substring(scopeEnd) : keyBindingWithScope;
        }

        private void OnBeforeCompleteWordRequested(string Guid, int ID, object CustomIn, object CustomOut,
                                                   ref bool CancelDefault)
        {
            var activeDocument = ApplicationObject.ActiveDocument;

            if (activeDocument == null || activeDocument.ProjectItem == null)
                return;

            if (FilePassesFilterDelegate == null || FilePassesFilterDelegate(activeDocument.ProjectItem.GetFileName()))
            {
                CancelDefault = true;

                if (CompleteWordRequested != null)
                {
                    CompleteWordRequested(this, EventArgs.Empty);
                }
            }
        }

        private void OnBeforeKeyPress(string keypress, TextSelection Selection, bool InStatementCompletion,
                                      ref bool CancelKeypress)
        {
            if (BeforeKeyPress != null)
            {
                BeforeKeyPress(keypress, Selection, InStatementCompletion, ref CancelKeypress);
            }
        }

        private void OnAfterKeyPress(string keypress, TextSelection Selection, bool InStatementCompletion)
        {
            if (AfterKeyPress != null)
            {
                AfterKeyPress(keypress, Selection, InStatementCompletion);
            }
        }

        private void OnBeginShutdown()
        {
            if (VisualStudioClosing != null)
            {
                VisualStudioClosing(this, EventArgs.Empty);
            }
        }

        private void OnSolutionOpened()
        {
            foreach (var codeFile in GenerateCodeFileList())
            {
                AddCodeFile(codeFile);
            }
        }

        /// <summary>
        /// Happens on save-file (with any changes)
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="hint"></param>
        private void OnTextEditorLineChanged(TextPoint startPoint, TextPoint endPoint, int hint)
        {
            //OnCurrentFileModified();
        }

        private void DocumentEvents_DocumentSaved(Document Document)
        {
            OnCurrentFileModified();
        }

        private void OnSolutionClosing()
        {
            if (codeFiles == null)
            {
                return;
            }

            foreach (var codeFile in codeFiles.ToList())
            {
                RemoveCodeFile(codeFile);
            }
            codeFiles = null;
        }

        private void OnCurrentFileModified()
        {
            if (ApplicationObject.ActiveDocument == null)
                return;

            var currentProjectItem = ApplicationObject.ActiveDocument.ProjectItem;

            if (currentProjectItem == null)
                return;

            var codeFile = (codeFiles != null
                                ? codeFiles.FirstOrDefault(cf => IsCodeFileForProjectItem(cf, currentProjectItem))
                                : null) ?? AddCodeFileForItemIfItMatches(currentProjectItem);

            if (codeFile != null && CodeFileModified != null)
            {
                CodeFileModified(this, codeFile);
            }
        }

        private static bool IsCodeFileForProjectItem(CodeFile codeFile, ProjectItem projectItem)
        {
            return codeFile.FilePath == projectItem.GetFileName();
        }

        private void OnSolutionItemRenamed(ProjectItem projectItem, string oldName)
        {
            var oldPath = Path.Combine(Path.GetDirectoryName(projectItem.GetFileName()), oldName);
            var existingFile = codeFiles != null ? codeFiles.FirstOrDefault(cf => cf.FilePath == oldPath) : null;

            if (existingFile != null)
            {
                var newName = projectItem.GetFileName();
                if (FilePassesFilterDelegate(newName))
                {
                    //modify codefile to have new name / path
                    existingFile.FilePath = newName;
                }
                else
                {
                    //file is no longer a file we're interested in (extension changed, for example)
                    RemoveCodeFile(existingFile);
                }
            }
            else
            {
                AddCodeFileForItemIfItMatches(projectItem);
            }
        }

        private CodeFile AddCodeFileForItemIfItMatches(ProjectItem currentProjectItem)
        {
            CodeFile codeFile = null;

            string fileName = currentProjectItem.GetFileName();
            if (string.IsNullOrEmpty(fileName))
                return null;

            if (FilePassesFilterDelegate(fileName))
            {
                codeFile = CreateCodeFileFromItem(currentProjectItem);
                AddCodeFile(codeFile);
            }
            return codeFile;
        }

        private void OnSolutionItemRemoved(ProjectItem projectItem)
        {
            if (codeFiles == null)
                return;

            var codeFile = codeFiles.FirstOrDefault(cf => IsCodeFileForProjectItem(cf, projectItem));
            if (codeFile != null)
            {
                RemoveCodeFile(codeFile);
            }
        }

        private void RemoveCodeFile(CodeFile codeFile)
        {
            codeFiles.Remove(codeFile);

            if (CodeFileRemoved != null)
            {
                CodeFileRemoved(this, codeFile);
            }
        }

        private void AddCodeFile(CodeFile codeFile)
        {
            if (codeFiles == null)
            {
                codeFiles = new List<CodeFile>();
            }
            codeFiles.Add(codeFile);

            if (CodeFileAdded != null)
            {
                CodeFileAdded(this, codeFile);
            }
        }

        private void OnSolutionItemAdded(ProjectItem projectItem)
        {
            //maybe we shouldn't react here: item is still in strange state
            AddCodeFileForItemIfItMatches(projectItem);
        }
        
        public event CodeFileEventHandler CodeFileAdded;
        public event CodeFileEventHandler CodeFileModified;
        public event CodeFileEventHandler CodeFileRemoved;
        public event EventHandler VisualStudioClosing;

        public IEnumerable<CodeFile> GetCodeFiles()
        {
            return codeFiles ?? (codeFiles = GenerateCodeFileList());
        }

        private IList<CodeFile> GenerateCodeFileList()
        {
            var res = new List<CodeFile>();
            var solution = ApplicationObject.Solution;

            if (solution != null)
            {
                VSLogger.Write(string.Format("Getting files from solution... {0}", solution.FullName));
                VSLogger.Write(string.Format(" top level project count {0} / {1}", solution.Count,
                                             solution.Projects.Count));

                if (solution.Count == 0) //unexpected..strange solution, fallback to parsing the solution files
                {
                    if (string.IsNullOrEmpty(solution.FullName))                    
                        return res;

                    VSLogger.Write("Error getting projects from solution, using manual parsing as fallback");
                    return new SolutionParser().ParseSln(solution.FullName, FilePassesFilter);
                }

                var totalFiles = 0;
                foreach (Project project in solution.Projects)
                {
                    VSLogger.Write(string.Format(" - Project {0}", project.UniqueName));

                    if (project.ProjectItems == null)
                        continue; // project not loaded atm

                    foreach (var item in project.ProjectItems)
                    {
                        var projectItem = item as ProjectItem;
                        foreach (var subItem in GetProjectItems(projectItem))
                        {
                            VSLogger.Write(string.Format("  - {0}", subItem.Name));
                            res.Add(CreateCodeFileFromItem(subItem));
                            totalFiles++;
                        }
                    }
                }
                VSLogger.Write(string.Format("Done with getting files from solution (total files: {0})", totalFiles));
            }
            else
            {
                VSLogger.Write(string.Format("Unexpected: Solution null..."));
            }
            return res.Distinct(new FilePathComparer()).ToList(); //the same file can be included from multiple projects
        }

        private class FilePathComparer : IEqualityComparer<CodeFile>
        {
            public bool Equals(CodeFile x, CodeFile y)
            {
                return Equals(x.FilePath, y.FilePath);
            }

            public int GetHashCode(CodeFile obj)
            {
                return obj.FilePath.GetHashCode();
            }
        }

        private IEnumerable<ProjectItem> GetProjectItems(ProjectItem item)
        {
            if (item.Kind == Constants.vsProjectItemKindPhysicalFile)
            {
                if (FilePassesFilter(item.GetFileName()))
                {
                    yield return item;
                }
                yield break;
            }

            var subItems = item.ProjectItems ??
                                    ((item.SubProject != null) ? item.SubProject.ProjectItems : null);

            if (subItems != null && subItems.Count > 0)
            {
                foreach (var subItem in subItems)
                {
                    var subProjectItem = subItem as ProjectItem;
                    if (subProjectItem != null)
                    {
                        var subSubItems = GetProjectItems(subProjectItem);
                        foreach (var subSubItem in subSubItems)
                        {
                            yield return subSubItem;
                        }
                    }
                }
            }
        }

        private bool FilePassesFilter(string fileName)
        {
            return FilePassesFilterDelegate == null || FilePassesFilterDelegate(fileName);
        }

        private static CodeFile CreateCodeFileFromItem(ProjectItem subItem)
        {
            var filePath = subItem.GetFileName();
            var projectName = subItem.ContainingProject != null ? subItem.ContainingProject.Name : "";

            return new CodeFile
                       {
                           FilePath = filePath,
                           ProjectName = projectName,
                           Contents = "",
                       };
        }

        public static string GetCodeFileContents(CodeFile codeFile)
        {
            var contents = "";
            try
            {
                var fileName = codeFile.FilePath;
                if (File.Exists(fileName)) //can not exist if using svn templates
                {
                    contents = File.ReadAllText(fileName);
                }
            }
            catch (Exception e)
            {
                //gulp
                VSLogger.Error(string.Format("Unable to read file contents of file {0}", codeFile.FilePath), e);
            } 

            return contents;
        }
        
        public bool IsDocumentOpen()
        {
            return ApplicationObject.ActiveDocument != null;
        }

        public bool Goto(CodeFile codeFile, int line, int offset)
        {
            if (OpenFile(codeFile))
            {
                GotoOffsetInActiveDocument(line, offset);
            }
            return true;
        }

        public bool OpenFile(CodeFile codeFile)
        {
            Window window;
            try
            {
                window = ApplicationObject.ItemOperations.OpenFile(codeFile.FilePath, Constants.vsViewKindCode);
            }
            catch (Exception)
            {
                return false;
            }

            return window != null;
        }

        public bool GotoOffsetInActiveDocument(int line, int offset)
        {
            if (line <= 0)
            {
                return false;
            }

            var selection = (ApplicationObject.ActiveDocument.Selection as TextSelection);
            if (selection != null)
            {
                selection.EndOfDocument(false);
                var maxLine = selection.CurrentLine;

                //first go to surrounding locations to make sure location is not at edge of window
                selection.MoveToLineAndOffset(Math.Min(maxLine, Math.Max(1, line - 20)), 1, false);
                selection.MoveToLineAndOffset(Math.Min(maxLine, line + 20), 1, false);

                var gotoLine = Math.Min(line, maxLine);
                selection.MoveToLineAndOffset(gotoLine, offset, false);

                return true;
            }
            return false;
        }

        public void JumpToActiveFileInSolutionExplorer()
        {
            var properties = ApplicationObject.get_Properties("Environment", "ProjectsAndSolution");
            if (properties != null)
            {
                var trackingProperty = properties.Item("TrackFileSelectionInExplorer");
                if (trackingProperty != null)
                {
                    var permanentTrackingEnabled = (bool) trackingProperty.Value;
                    if (!permanentTrackingEnabled)
                    {
                        trackingProperty.Value = true; //triggers VS to jump to current file
                        trackingProperty.Value = false; //disable it again
                    }
                }
            }
            try
            {
                ApplicationObject.Windows.Item(Constants.vsWindowKindSolutionExplorer).Activate();
            }
            catch (Exception )
            { }
        }
        
        public int GetCursorPositionInLine()
        {
            var selection = (ApplicationObject.ActiveDocument.Selection as TextSelection);
            var sp = selection.ActivePoint.CreateEditPoint();
            return sp.LineCharOffset - 1;
        }

        public string GetCurrentLine()
        {
            var selection = (ApplicationObject.ActiveDocument.Selection as TextSelection);
            var sp = selection.ActivePoint.CreateEditPoint();
            return GetLineText(sp);
        }

        public int GetCurrentLineNumber()
        {
            var selection = (ApplicationObject.ActiveDocument.Selection as TextSelection);
            var sp = selection.ActivePoint.CreateEditPoint();
            return sp.Line;
        }

        public void ReplaceSelection(int lineNumber, int offsetInLine, int endOffsetInLine, string replacement)
        {
            var selection = (ApplicationObject.ActiveDocument.Selection as TextSelection);

            var startPoint = selection.AnchorPoint.CreateEditPoint();
            startPoint.MoveToLineAndOffset(lineNumber, offsetInLine + 1);
            var endPoint = selection.ActivePoint.CreateEditPoint();
            endPoint.MoveToLineAndOffset(lineNumber, endOffsetInLine + 1);

            selection.MoveToPoint(startPoint, false);
            selection.MoveToPoint(endPoint, true);

            selection.Text = replacement;
        }

        private static string GetLineText(EditPoint sp)
        {
            var ep = sp.CreateEditPoint();
            ep.StartOfLine();
            var eol = sp.CreateEditPoint();
            eol.EndOfLine();
            return ep.GetText(eol);
        }

        public CodeFile GetCurrentCodeFile()
        {
            if (ApplicationObject.ActiveDocument == null)
            {
                return null;
            }

            var projectItem = ApplicationObject.ActiveDocument.ProjectItem;
            return GetCodeFiles().FirstOrDefault(c => IsCodeFileForProjectItem(c, projectItem));
        }

        public Rectangle GetBounds()
        {
            var window = ApplicationObject.MainWindow;

            return window != null
                       ? new Rectangle(window.Left, window.Top, window.Width, window.Height)
                       : new Rectangle(0, 0, 1024, 768);
        }

        public string GetSolutionName()
        {
            return ApplicationObject.Solution != null ? ApplicationObject.Solution.FullName : "<none>";
        }

        public IEnumerable<string> GetConflictingCommandsForKeyBinding(string commandName, string keyBindingWithScope)
        {
            var allCommands = GetCommandsForKeyBinding(keyBindingWithScope);
            return allCommands.Where(cm => cm != commandName).ToList();
        }

        public IEnumerable<string> GetCommandsForKeyBinding(string keyBindingWithScope)
        {
            var commandNames = new List<string>();

            var keyBinding = TrimScopeFromKeyBinding(keyBindingWithScope);

            var commands = ApplicationObject.Commands;
            foreach (Command command in commands)
            {
                var bindings = (object[])command.Bindings;
                foreach (string bindingWithScope in bindings)
                {
                    var binding = TrimScopeFromKeyBinding(bindingWithScope);
                    if (keyBinding.Equals(binding))
                    {
                        commandNames.Add(command.Name);
                    }
                }
            }
            return commandNames; 
        }
        
        public IWin32Window MainWindowHandle
        {
            get { return new MainWindowWrapper(ApplicationObject.MainWindow.HWnd); }
        }

        private class MainWindowWrapper : IWin32Window
        {
            public IntPtr Handle { get; private set; }

            public MainWindowWrapper(int hWnd)
            {
                Handle = new IntPtr(hWnd);
            }
        }
    }

    public static class ProjectItemExtensions
    {
        public static string GetFileName(this ProjectItem projItem)
        { 
            //do not use indexed property!!
            return projItem.get_FileNames(1);
        }
    }
}