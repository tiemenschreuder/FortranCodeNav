using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using VSIntegration;

namespace FortranCodeNavCore.Forms
{
    public partial class DebugForm : Form
    {
        private FortranCodeNavCore CodeNav { get; set; }

        public DebugForm (VisualStudioIDE ide, Version version, FortranCodeNavCore codeNav)
        {
            CodeNav = codeNav;
            InitializeComponent();
            
            versionLbl.Text = version + " (Beta)";

            using (var stream = GetType().Assembly.GetManifestResourceStream("FortranCodeNavCore.Resources.readme.txt"))
            using (var reader = new StreamReader(stream))
            {
                shortcutsDetails.Text = reader.ReadToEnd();
            }
            
            // TODO
            //var commands = ide.CommandManager.Commands;
            //dataGridView1.Columns[2].DefaultCellStyle.ForeColor = Color.Red;
            //foreach (var commandName in commands.Keys)
            //{
            //    var vsCommand = ide.CommandManager.Commands[commandName];
            //    var keyboardBinding = ide.GetKeyBindingForCommand(vsCommand);
            //    var conflicts = keyboardBinding != null
            //                           ? String.Join(", ",ide.GetConflictingCommandsForKeyBinding(vsCommand.Name, keyboardBinding).ToArray())
            //                           : "shortcut missing!";
            //
            //    dataGridView1.Rows.Add(new[] { vsCommand.Name, keyboardBinding, conflicts });
            //}
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(((Control) sender).Text);
        }

        private void btnClearCache_Click(object sender, EventArgs e)
        {
            CodeNav.ClearCaches();
        }

        private void btnErrorLog_Click(object sender, EventArgs e)
        {

        }
    }
}
