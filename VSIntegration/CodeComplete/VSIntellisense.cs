using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;

namespace VSIntegration.CodeComplete
{
    public class VSIntellisense
    {
        private Native.HookProc KeyboardProcDelegate = null;
        private Native.HookProc MouseProcDelegate = null;
        private IntPtr khook;
        private IntPtr mhook;
        
        private VisualStudioIDE VisualStudio { get; set; }
        public delegate void OnCodeCompleteDelegate(CompletionSession session);
        public OnCodeCompleteDelegate OnCodeCompleteActivating;
        public OnCodeCompleteDelegate OnCodeCompleteUpdating;

        private StatementCompleteList completeList;

        public VSIntellisense(VisualStudioIDE ide)
        {
            VisualStudio = ide;
            VisualStudio.BeforeKeyPress = OnBeforeKeyPress;
            VisualStudio.AfterKeyPress = OnAfterKeyPress;
            VisualStudio.CompleteWordRequested += new EventHandler(VisualStudioCompleteWordRequested);
            InitHook();
        }

        private int MouseProc(int code, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (completeList == null || code != Native.HC_ACTION)
                {
                    return Native.CallNextHookEx(mhook, code, wParam, lParam);
                }

                var mouseHookStruct = (Native.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(Native.MouseHookStruct));

                var mouseCode = (uint)wParam;
                
                if (mouseCode == Native.WM_MOUSEWHEEL)
                {
                    var listBoxHandle = completeList.GetListBoxHandle();
                    if (mouseHookStruct.hwnd != listBoxHandle)
                    {
                        //redirect scrolling to completion list box
                        var mouseHookStructEx = (Native.MouseHookStructEx)Marshal.PtrToStructure(lParam, typeof(Native.MouseHookStructEx));
                        Native.PostMessage(new IntPtr(listBoxHandle), mouseCode, mouseHookStructEx.mouseWheelDelta, 0);
                        return 1; //eat this one
                    }
                }
                else if ((mouseCode == Native.WM_LBUTTONDOWN || mouseCode == Native.WM_NCLBUTTONDOWN ||
                     mouseCode == Native.WM_RBUTTONDOWN || mouseCode == Native.WM_NCRBUTTONDOWN ||
                     mouseCode == Native.WM_MBUTTONDOWN || mouseCode == Native.WM_NCMBUTTONDOWN))
                {
                    //click anywhere but in the code complete window: end of session
                    if (!IsHandleOfCodeCompleteWindow(mouseHookStruct.hwnd))
                    {
                        DismissCodeComplete();
                    }
                }
            }
            catch (Exception)
            { 
                // gulp
            }
            return Native.CallNextHookEx(mhook, code, wParam, lParam);
        }

        private bool IsHandleOfCodeCompleteWindow(int hwnd)
        {
            if (completeList != null)
            {
                return hwnd == completeList.GetListBoxHandle() || 
                    hwnd == (int)completeList.Handle;
            }
            return false;
        }

        private int KeyboardProc(int code, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (completeList == null || code != Native.HC_ACTION)
                {
                    return Native.CallNextHookEx(khook, code, wParam, lParam);
                }

                var key = (int)wParam;
                var transition = (uint)lParam & 0x80000000;
                
                if (transition == Native.TRANSITION_KEY_DOWN)
                {
                    if (key == Native.VK_UP)
                    {
                        completeList.SelectNextInCompleteList(false);
                        return 1;
                    }
                    else if (key == Native.VK_DOWN)
                    {
                        completeList.SelectNextInCompleteList(true);
                        return 1;
                    }
                    else if (key == Native.VK_RETURN || key == Native.VK_TAB || key == Native.VK_SPACE)
                    {
                        CommitCodeComplete(completeList.GetSelectedItem());
                        return 1;
                    }
                    else if (key == Native.VK_ESCAPE)
                    {
                        DismissCodeComplete();
                        return 1;
                    }
                }
                else if (transition == Native.TRANSITION_KEY_UP)
                {
                    DismissCodeCompleteIfCaretOutsideValidRange();
                }
            }
            catch
            {
                
            }
            return Native.CallNextHookEx(khook, code, wParam, lParam);
        }

        private bool DismissCodeCompleteIfCaretOutsideValidRange()
        {
            if (currentSession != null)
            {
                var currentLine = VisualStudio.GetCurrentLineNumber();
                if (currentLine != currentSession.LineNumber)
                {
                    DismissCodeComplete();
                    return true;
                }
                var indexInLine = VisualStudio.GetCursorPositionInLine();
                if (indexInLine < currentSession.InsertionIndexInLine)
                {
                    DismissCodeComplete();
                    return true;
                }
            }
            return false;
        }

        private void DismissCodeComplete()
        {
            CloseCodeComplete();
        }

        public CompletionSession currentSession;
        private bool committing;

        private void CommitCodeComplete(string stringToCommit)
        {
            if (stringToCommit == null)
            {
                DismissCodeComplete();
                return;
            }

            committing = true;
            try
            {
                //insert current item
                var newOffsetInLine = VisualStudio.GetCursorPositionInLine();
                VisualStudio.ReplaceSelection(currentSession.LineNumber, currentSession.InsertionIndexInLine, newOffsetInLine, stringToCommit);
                CloseCodeComplete();
            }
            finally { committing = false; }
        }

        private void UpdateCompleteListFilter()
        {
            if (completeList == null || committing)
                return;

            if (OnCodeCompleteUpdating != null)
            {
                OnCodeCompleteUpdating(currentSession);
                
                if (completeList == null)
                    return;//wtf

                completeList.SetResults(currentSession.FilteredCompletionSet.ToList());
            }
        }
        
        void VisualStudioCompleteWordRequested(object sender, EventArgs e)
        {
            InitiateCodeComplete();
        }

        void OnBeforeKeyPress(string keypress, TextSelection Selection, bool InStatementCompletion, ref bool CancelKeypress)
        {
            if (OnCodeCompleteActivating == null || committing)
                return;

            if (Control.ModifierKeys == Keys.Control && keypress == " ")
            {
                CancelKeypress = true;
                InitiateCodeComplete();                
            }
        }

        private void InitiateCodeComplete()
        {
            //todo: check if happens in right window

            DismissCodeComplete();

            currentSession = new CompletionSession { LineNumber = VisualStudio.GetCurrentLineNumber() };

            OnCodeCompleteActivating(currentSession);

            if (currentSession.Cancel)
            {
                CloseCodeComplete();
                return;
            }

            if (currentSession.FilteredCompletionSet.Count() == 1) //only one item in list, commit directly
            {
                CommitCodeComplete(currentSession.FilteredCompletionSet.First().ToString());
            }
            else //zero, or more than 1 items
            {
                completeList = new StatementCompleteList { Left = currentSession.Coordinate.X, Top = currentSession.Coordinate.Y };
                completeList.SetResults(currentSession.FilteredCompletionSet.ToList());
                completeList.SetSignatureToolTip(currentSession.SignatureToolTip);
                completeList.Deactivate += new EventHandler(CompleteListDeactivate);
                completeList.FormClosing += new FormClosingEventHandler(CompleteListFormClosing);
                completeList.ItemSelected += new EventHandler(CompleteListItemSelected);
                completeList.Show();
                return;
            } 
        }

        void CompleteListItemSelected(object sender, EventArgs e)
        {
            CommitCodeComplete(completeList.GetSelectedItem());
        }

        void OnAfterKeyPress(string keypress, TextSelection selection, bool inStatementCompletion)
        {            
            if (keypress == "\b")
            {
                if (DismissCodeCompleteIfCaretOutsideValidRange())
                    return;
            }
            UpdateCompleteListFilter();
        }

        void CloseCodeComplete()
        {
            if (completeList != null)
            {
                completeList.Close();
            } 
        }

        void CompleteListDeactivate(object sender, EventArgs e)
        {
            CloseCodeComplete();
        }

        void CompleteListFormClosing(object sender, FormClosingEventArgs e)
        {
            if (completeList != null)
                completeList.Dispose();
            completeList = null;
            currentSession = null;
        }
        
        private void InitHook()
        {
            uint id = Native.GetCurrentThreadId();

            this.KeyboardProcDelegate = new Native.HookProc(this.KeyboardProc);
            khook = Native.SetWindowsHookEx(Native.WH_KEYBOARD, this.KeyboardProcDelegate, IntPtr.Zero, id);

            this.MouseProcDelegate = new Native.HookProc(this.MouseProc);
            mhook = Native.SetWindowsHookEx(Native.WH_MOUSE, this.MouseProcDelegate, IntPtr.Zero, id);
        }

        private void Unhook()
        {
            Native.UnhookWindowsHookEx(khook);
            Native.UnhookWindowsHookEx(mhook);
        }
    }
}
