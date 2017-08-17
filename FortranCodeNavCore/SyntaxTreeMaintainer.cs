using System;
using System.Collections.Generic;
using FortranCodeNavCore.Fortran.Elements;
using FortranCodeNavCore.Logging;
using VSIntegration;

namespace FortranCodeNavCore
{
    public class SyntaxTreeMaintainer
    {
        private readonly ParseQueueProcessor processor;
        private readonly IDictionary<CodeFile, SyntaxTree> syntaxTrees = new Dictionary<CodeFile, SyntaxTree>();
        private bool initialized;

        public SyntaxTreeMaintainer(VisualStudioIDE visualStudio)
        {
            VisualStudio = visualStudio;
            VisualStudio.CodeFileAdded += VisualStudioCodeFileAdded;
            VisualStudio.CodeFileModified += VisualStudioCodeFileModified;
            VisualStudio.CodeFileRemoved += VisualStudioCodeFileRemoved;
            processor = new ParseQueueProcessor(VisualStudio);
        }

        private VisualStudioIDE VisualStudio { get; set; }

        private void VisualStudioCodeFileRemoved(object sender, CodeFile codeFile)
        {
            processor.RemoveCodeFileFromQueue(codeFile);

            if (syntaxTrees.ContainsKey(codeFile))
            {
                syntaxTrees.Remove(codeFile);
            }
        }

        private void VisualStudioCodeFileModified(object sender, CodeFile codeFile)
        {
            Log.Write(String.Format("Queued for re-parsing due to modifications: {0}", codeFile.FilePath));
            processor.AddCodeFileToQueue(codeFile);

            if (syntaxTrees.ContainsKey(codeFile))
            {
                syntaxTrees.Remove(codeFile);
            }
        }

        private void VisualStudioCodeFileAdded(object sender, CodeFile codeFile)
        {
            Log.Write(String.Format("File added: {0}", codeFile.FilePath));
            processor.AddCodeFileToQueue(codeFile);
        }

        public SyntaxTree GetSyntaxTree(CodeFile codeFile)
        {
            ProcessCodeFile(codeFile);
            return syntaxTrees[codeFile];
        }

        private void ProcessCodeFile(CodeFile codeFile)
        {
            if (!syntaxTrees.ContainsKey(codeFile))
            {
                syntaxTrees[codeFile] = processor.ProcessCodeFile(codeFile);
            }
        }

        public ICollection<SyntaxTree> GetSyntaxTrees()
        {
            ProcessQueue();
            return syntaxTrees.Values;
        }

        private void ProcessQueue()
        {
            if (!initialized)
            {
                foreach (var codeFile in VisualStudio.GetCodeFiles())
                {
                    VisualStudioCodeFileAdded(this, codeFile);
                }
                initialized = true;
            }

            var results = processor.ProcessQueue();

            foreach (var codeFile in results.Keys)
            {
                syntaxTrees[codeFile] = results[codeFile];
            }

            if (results.Count > 0)
            {
                Log.Write(string.Format("Number of syntax trees after full queue processing: {0} ({1} added)", syntaxTrees.Count,
                                        results.Count));
            }
        }

        public void ClearCaches()
        {
            syntaxTrees.Clear();
            initialized = false;
            ProcessQueue();
        }
    }
}