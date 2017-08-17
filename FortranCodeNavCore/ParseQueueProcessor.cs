using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using FortranCodeNavCore.Forms;
using FortranCodeNavCore.Fortran.Elements;
using FortranCodeNavCore.Fortran.Parser;
using FortranCodeNavCore.Logging;
using VSIntegration;

namespace FortranCodeNavCore
{
    public class ParseQueueProcessor
    {
        private static int processedFileIndex;
        private readonly IList<CodeFile> codeFilesQueue = new List<CodeFile>();
        private readonly Dictionary<CodeFile, SyntaxTree> resultSet = new Dictionary<CodeFile, SyntaxTree>();
        private readonly Stopwatch stopwatch = new Stopwatch();

        private event ProgressDialog.UpdateProgressDelegate Processing;
        private event MethodInvoker Finished;
        
        public static bool ShowProgressDialog = true;

        public ParseQueueProcessor(VisualStudioIDE visualStudio)
        {
            VisualStudio = visualStudio;
        }

        public VisualStudioIDE VisualStudio { get; set; }

        public SyntaxTree ProcessCodeFile(CodeFile codeFile)
        {
            return ParseFile(codeFile);
        }

        public Dictionary<CodeFile, SyntaxTree> ProcessQueue()
        {
            resultSet.Clear();

            ProgressDialog progressDialog = null;

            if (ShowProgressDialog)
            {
                progressDialog = CreateProgressDialog();
            }
            
            var thread = new Thread(ThreadedProcessQueue) {Name = "Fortran CodeNav Parse Thread"};
            thread.Start();

            if (progressDialog != null)
            {
                progressDialog.ShowDialog();
                Processing -= progressDialog.UpdateProgress;
                Finished -= progressDialog.ProgressDone;
                progressDialog.Dispose();
            }
            else
            {
                thread.Join(); //wait for thread to finish manually
            }
            
            return resultSet;
        }

        public void AddCodeFileToQueue(CodeFile codeFile)
        {
            lock (codeFilesQueue)
            {
                if (!codeFilesQueue.Contains(codeFile))
                {
                    codeFilesQueue.Add(codeFile);
                }
            }
        }

        public void RemoveCodeFileFromQueue(CodeFile codeFile)
        {
            lock (codeFilesQueue)
            {
                if (!codeFilesQueue.Contains(codeFile))
                    return;

                Log.Write(String.Format("File removed: {0}", codeFile.FilePath));
                codeFilesQueue.Remove(codeFile);
            }
        }

        private SyntaxTree ParseFile(CodeFile codeFile)
        {
            stopwatch.Reset();
            stopwatch.Start();

            var ast = new SyntaxTree();
            try
            {
                var parser = new FortranFileParser();
                codeFile.Contents = VisualStudio.GetCodeFileContents(codeFile);
                ast = parser.ParseFileContents(codeFile.Contents);
            }
            catch (Exception e)
            {
                //gulp
                Log.Error(String.Format("Error while parsing {0}", codeFile.FilePath), e);
            }
            finally
            {
                ast.CodeFile = codeFile;
                ast.FileName = codeFile.FilePath;
            }

            stopwatch.Stop();

            double milliseconds = (((double) stopwatch.ElapsedTicks)/Stopwatch.Frequency)*1000;

            Log.Write(String.Format("Parsing {0} took {1:0.###}ms", codeFile.FilePath, milliseconds));

            return ast;
        }

        private ProgressDialog CreateProgressDialog()
        {
            var progress = new ProgressDialog();
            Rectangle windowBounds = VisualStudio.GetBounds();
            double top = windowBounds.Top + 0.3*windowBounds.Height;
            double left = windowBounds.Left + 0.3*windowBounds.Width;
            progress.Location = new Point((int) left, (int) top);
            Processing += progress.UpdateProgress;
            Finished += progress.ProgressDone;
            return progress;
        }

        private void ThreadedProcessQueue()
        {
            int numFiles;

            var queueWatch = new Stopwatch();
            queueWatch.Reset();
            queueWatch.Start();

            var doneFlag = new ManualResetEvent(false);

            lock (codeFilesQueue)
            {
                numFiles = codeFilesQueue.Count;
                processedFileIndex = 0;

                for (int i = 0; i < numFiles; i++)
                {
                    ThreadPool.QueueUserWorkItem(fileIndex =>
                                                     {
                                                         try
                                                         {
                                                             var index = (int) fileIndex;
                                                             CodeFile codeFile = codeFilesQueue[index];
                                                             OnFileProcessing(processedFileIndex, numFiles,
                                                                              codeFile.FileName);
                                                             SyntaxTree ast = ParseFile(codeFile);
                                                             lock (resultSet)
                                                             {
                                                                 resultSet.Add(codeFile, ast);
                                                             }
                                                         }
                                                         finally
                                                         {
                                                             if (Interlocked.Increment(ref processedFileIndex) ==
                                                                 numFiles)
                                                             {
                                                                 doneFlag.Set();
                                                             }
                                                         }
                                                     }, i);
                }
                if (numFiles > 0)
                {
                    doneFlag.WaitOne(); //wait for all threads to finish
                    codeFilesQueue.Clear(); //clear queue
                }
            }

            if (Finished != null)
            {
                Finished();
            }

            queueWatch.Stop();
            double milliseconds = (((double) queueWatch.ElapsedTicks)/Stopwatch.Frequency)*1000;
            Log.Write(String.Format("Parsing entire queue ({1} files) took {0:0.###}ms", milliseconds, numFiles));
        }

        private void OnFileProcessing(int index, int numFiles, string fileName)
        {
            if (Processing != null)
            {
                Processing(index + 1, numFiles, fileName);
            }
        }
    }
}