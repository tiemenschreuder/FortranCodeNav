using System.Windows.Forms;

namespace FortranCodeNavCore.Forms
{
    public partial class ProgressDialog : Form
    {
        private const string ProcessMessage = "Processing {1} of {2}: {0}";
        private const int MillisecondsBetweenUpdates = 250;

        private bool finished;
        private long lastUpdateTicks;

        public ProgressDialog()
        {
            Application.EnableVisualStyles();
            Opacity = 0;
            InitializeComponent();
            Shown += ProgressDialogShown;
        }

        void ProgressDialogShown(object sender, System.EventArgs e)
        {
            if (finished)
            {
                ProgressDone();
            }
        }

        public delegate void UpdateProgressDelegate(int index, int totalFiles, string fileName);
        
        public void UpdateProgress(int index, int totalFiles, string fileName)
        {
            if (!IsHandleCreated)
                return; //ignore

            if (InvokeRequired)
            {
                //limit the number of context switches
                if ((System.Environment.TickCount - lastUpdateTicks) > MillisecondsBetweenUpdates)
                {
                    lastUpdateTicks = System.Environment.TickCount;
                    BeginInvoke(new UpdateProgressDelegate(UpdateProgress),
                                       new object[] { index, totalFiles, fileName });
                }
            }
            else
            {
                if (IsDisposed || progressBar.IsDisposed)
                    return;

                progressBar.Value = (index*100)/totalFiles;
                progressLabel.Text = string.Format(ProcessMessage, fileName, index, totalFiles);
                if (Opacity == 0.0 && index > 20) //after at least 20 files
                {
                    Opacity = 1.0;
                }
            }
        }

        public void ProgressDone()
        {
            finished = true;
            
            if (InvokeRequired)
            {
                if (IsDisposed)
                    return;

                BeginInvoke(new MethodInvoker(ProgressDone));
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
