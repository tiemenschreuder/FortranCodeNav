using System;
using System.Drawing;
using System.Linq;
using FortranCodeNavCore.Forms;
using FortranCodeNavCore.Fortran.Elements;
using FortranCodeNavCore.Fortran.Parser;
using FortranCodeNavCore.SyntaxTrees;
using VSIntegration;

namespace FortranCodeNavCore.Commands
{
    public abstract class FortranCommandBase
    {
        protected VisualStudioIDE VisualStudio { get; set; }
        protected SyntaxTreeMaintainer SyntaxTreeMaintainer { get; set; }

        protected FortranCommandBase(VisualStudioIDE visualStudio, SyntaxTreeMaintainer syntaxTreeMaintainer)
        {
            VisualStudio = visualStudio;
            SyntaxTreeMaintainer = syntaxTreeMaintainer;
        }

        protected static ListFindControl dialog;
        protected ListFindControl CreateListFindControl()
        {
            if (dialog != null)
            {
                dialog.Close();
                dialog = null;
            }

            dialog = new ListFindControl();

            var windowBounds = VisualStudio.GetBounds();
            var top = windowBounds.Top + 0.3 * windowBounds.Height;
            var left = windowBounds.Left + 0.3 * windowBounds.Width;
            dialog.Location = new Point((int)left, (int)top);

            return dialog;
        }

        protected IMember GetCurrent<T>() where T: class, IMember
        {
            var current = VisualStudio.GetCurrentCodeFile();
            if (current != null)
            {
                var ast = SyntaxTreeMaintainer.GetSyntaxTree(current);

                var allMethods = ast.GetAllMembers().OfType<T>();
                var lineNumber = VisualStudio.GetCurrentLineNumber();

                return FortranParseHelper.GetEnclosingMember(allMethods.OfType<IMember>(), lineNumber);
            }
            return null;
        }

        protected string GetContext()
        {
            try
            {
                string context = "\n\n\nError context:";
                context += "\nSolution: " + VisualStudio.GetSolutionName();
                var codeFile = VisualStudio.GetCurrentCodeFile();
                if (codeFile != null)
                {
                    context += "\nCurrent file: " + codeFile.FilePath;
                    context += "\nLineNumber: " + VisualStudio.GetCurrentLine();
                    context += "\nCursor position: " + VisualStudio.GetCursorPositionInLine();
                }
                else
                {
                    context += "\nCurrent file: <none>";
                }
                context += "\n\n";

                return context;
            }
            catch (Exception)
            {
                return "<exception during GetContext()>";
            }
        }
    }
}