using EnvDTE;
using EnvDTE80;

namespace VSIntegration
{
    public class DocumentManager
    {
        #region Delegates

        public delegate void OnAfterKeyPressHandler(string keypress, TextSelection Selection, bool InStatementCompletion);

        public delegate void OnBeforeKeyPressHandler(string keypress, TextSelection Selection, bool InStatementCompletion, ref bool CancelKeypress);

        #endregion

        public OnAfterKeyPressHandler AfterKeyPress;
        public OnBeforeKeyPressHandler BeforeKeyPress;

        private TextDocumentKeyPressEvents TextDocKeyEvents { get; set; }
        private WindowEvents WindowEvents { get; set; }
        private DTE2 ApplicationObject { get; set; }

        public void Setup(DTE2 applicationObject)
        {
            ApplicationObject = applicationObject;

            if (ApplicationObject.ActiveDocument != null)
            {
                AddKeyboardEvents(ApplicationObject.ActiveDocument.Name);
            }

            SetUpWindowEventsHandler();
        }

        public void SetUpWindowEventsHandler()
        {
            var events = (Events2) ApplicationObject.Events;

            WindowEvents = events.get_WindowEvents(null);

            WindowEvents.WindowActivated += WindowActivated;
            WindowEvents.WindowCreated += WindowCreated;
        }

        private void RemoveWindowsEvents()
        {
            if (WindowEvents != null)
            {
                WindowEvents.WindowActivated -= WindowActivated;
                WindowEvents.WindowCreated -= WindowCreated;
            }
        }

        private void SetUpKeyboardEventsHandler()
        {
            var events = (Events2) ApplicationObject.Events;

            TextDocKeyEvents = events.get_TextDocumentKeyPressEvents(null);

            TextDocKeyEvents.BeforeKeyPress += TextEditorBeforeKeyPress;
            TextDocKeyEvents.AfterKeyPress += TextEditorAfterKeyPress;
        }

        private void RemoveKeyboardEvents()
        {
            if (TextDocKeyEvents != null)
            {
                TextDocKeyEvents.BeforeKeyPress -= TextEditorBeforeKeyPress;
                TextDocKeyEvents.AfterKeyPress -= TextEditorAfterKeyPress;
            }
        }

        private void WindowCreated(Window created)
        {
            AddKeyboardEvents(created.Caption);
        }

        private void WindowActivated(Window gotFocus, Window lostFocus)
        {
            AddKeyboardEvents(gotFocus.Caption);
        }

        private void AddKeyboardEvents(string fileName)
        {
            RemoveKeyboardEvents();

            //todo: add filtering here

            SetUpKeyboardEventsHandler();
        }

        private void TextEditorBeforeKeyPress(string keypress, TextSelection selection, bool inStatementCompletion,
                                              ref bool cancelKeypress)
        {
            if (BeforeKeyPress != null)
            {
                BeforeKeyPress(keypress, selection, inStatementCompletion, ref cancelKeypress);
            }
        }

        private void TextEditorAfterKeyPress(string keypress, TextSelection selection, bool inStatementCompletion)
        {
            if (AfterKeyPress != null)
            {
                AfterKeyPress(keypress, selection, inStatementCompletion);
            }
        }
    }
}