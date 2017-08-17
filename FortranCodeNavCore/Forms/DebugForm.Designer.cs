namespace FortranCodeNavCore.Forms
{
    partial class DebugForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
         this.label1 = new System.Windows.Forms.Label();
         this.label2 = new System.Windows.Forms.Label();
         this.versionLbl = new System.Windows.Forms.Label();
         this.label4 = new System.Windows.Forms.Label();
         this.label5 = new System.Windows.Forms.Label();
         this.label6 = new System.Windows.Forms.Label();
         this.linkLabel1 = new System.Windows.Forms.LinkLabel();
         this.shortcutsDetails = new System.Windows.Forms.TextBox();
         this.pictureBox1 = new System.Windows.Forms.PictureBox();
         this.dataGridView1 = new System.Windows.Forms.DataGridView();
         this.Command = new System.Windows.Forms.DataGridViewTextBoxColumn();
         this.Shortcut = new System.Windows.Forms.DataGridViewTextBoxColumn();
         this.Conflicts = new System.Windows.Forms.DataGridViewTextBoxColumn();
         this.btnClearCache = new System.Windows.Forms.Button();
         this.btnErrorLog = new System.Windows.Forms.Button();
         this.linkLabel2 = new System.Windows.Forms.LinkLabel();
         this.label3 = new System.Windows.Forms.Label();
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
         this.SuspendLayout();
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.label1.Location = new System.Drawing.Point(147, 13);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(238, 31);
         this.label1.TabIndex = 0;
         this.label1.Text = "Fortran CodeNav";
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.label2.Location = new System.Drawing.Point(150, 52);
         this.label2.Margin = new System.Windows.Forms.Padding(5);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(42, 13);
         this.label2.TabIndex = 0;
         this.label2.Text = "Version";
         // 
         // versionLbl
         // 
         this.versionLbl.AutoSize = true;
         this.versionLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.versionLbl.Location = new System.Drawing.Point(198, 52);
         this.versionLbl.Margin = new System.Windows.Forms.Padding(5);
         this.versionLbl.Name = "versionLbl";
         this.versionLbl.Size = new System.Drawing.Size(57, 13);
         this.versionLbl.TabIndex = 0;
         this.versionLbl.Text = "1.0.3243";
         // 
         // label4
         // 
         this.label4.AutoSize = true;
         this.label4.Location = new System.Drawing.Point(198, 75);
         this.label4.Margin = new System.Windows.Forms.Padding(5);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(94, 13);
         this.label4.TabIndex = 2;
         this.label4.Text = "Tiemen Schreuder";
         // 
         // label5
         // 
         this.label5.AutoSize = true;
         this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.label5.Location = new System.Drawing.Point(150, 75);
         this.label5.Margin = new System.Windows.Forms.Padding(5);
         this.label5.Name = "label5";
         this.label5.Size = new System.Drawing.Size(38, 13);
         this.label5.TabIndex = 2;
         this.label5.Text = "Author";
         // 
         // label6
         // 
         this.label6.AutoSize = true;
         this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.label6.Location = new System.Drawing.Point(150, 121);
         this.label6.Margin = new System.Windows.Forms.Padding(5);
         this.label6.Name = "label6";
         this.label6.Size = new System.Drawing.Size(45, 13);
         this.label6.TabIndex = 3;
         this.label6.Text = "Old site:";
         // 
         // linkLabel1
         // 
         this.linkLabel1.AutoSize = true;
         this.linkLabel1.Location = new System.Drawing.Point(198, 121);
         this.linkLabel1.Margin = new System.Windows.Forms.Padding(5);
         this.linkLabel1.Name = "linkLabel1";
         this.linkLabel1.Size = new System.Drawing.Size(198, 13);
         this.linkLabel1.TabIndex = 4;
         this.linkLabel1.TabStop = true;
         this.linkLabel1.Text = "http://publicwiki.deltares.nl/display/FCN";
         this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
         // 
         // shortcutsDetails
         // 
         this.shortcutsDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.shortcutsDetails.Location = new System.Drawing.Point(13, 168);
         this.shortcutsDetails.Multiline = true;
         this.shortcutsDetails.Name = "shortcutsDetails";
         this.shortcutsDetails.ReadOnly = true;
         this.shortcutsDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
         this.shortcutsDetails.Size = new System.Drawing.Size(740, 472);
         this.shortcutsDetails.TabIndex = 5;
         // 
         // pictureBox1
         // 
         this.pictureBox1.Image = global::FortranCodeNavCore.Properties.Resources.fortran_codenav_128;
         this.pictureBox1.Location = new System.Drawing.Point(13, 13);
         this.pictureBox1.Name = "pictureBox1";
         this.pictureBox1.Size = new System.Drawing.Size(128, 128);
         this.pictureBox1.TabIndex = 1;
         this.pictureBox1.TabStop = false;
         // 
         // dataGridView1
         // 
         this.dataGridView1.AllowUserToAddRows = false;
         this.dataGridView1.AllowUserToDeleteRows = false;
         this.dataGridView1.AllowUserToResizeColumns = false;
         this.dataGridView1.AllowUserToResizeRows = false;
         this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.Control;
         this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
         this.dataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
         this.dataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
         this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
         this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Command,
            this.Shortcut,
            this.Conflicts});
         this.dataGridView1.Location = new System.Drawing.Point(551, 13);
         this.dataGridView1.MultiSelect = false;
         this.dataGridView1.Name = "dataGridView1";
         this.dataGridView1.ReadOnly = true;
         this.dataGridView1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
         this.dataGridView1.RowHeadersVisible = false;
         this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
         this.dataGridView1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
         this.dataGridView1.Size = new System.Drawing.Size(202, 149);
         this.dataGridView1.TabIndex = 7;
         this.dataGridView1.Visible = false;
         // 
         // Command
         // 
         this.Command.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
         this.Command.HeaderText = "Command";
         this.Command.Name = "Command";
         this.Command.ReadOnly = true;
         this.Command.Width = 79;
         // 
         // Shortcut
         // 
         this.Shortcut.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
         this.Shortcut.HeaderText = "Shortcut";
         this.Shortcut.Name = "Shortcut";
         this.Shortcut.ReadOnly = true;
         this.Shortcut.Width = 72;
         // 
         // Conflicts
         // 
         this.Conflicts.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
         this.Conflicts.HeaderText = "Conflicts";
         this.Conflicts.Name = "Conflicts";
         this.Conflicts.ReadOnly = true;
         // 
         // btnClearCache
         // 
         this.btnClearCache.Location = new System.Drawing.Point(655, 646);
         this.btnClearCache.Name = "btnClearCache";
         this.btnClearCache.Size = new System.Drawing.Size(98, 26);
         this.btnClearCache.TabIndex = 8;
         this.btnClearCache.Text = "Clear caches";
         this.btnClearCache.UseVisualStyleBackColor = true;
         this.btnClearCache.Click += new System.EventHandler(this.btnClearCache_Click);
         // 
         // btnErrorLog
         // 
         this.btnErrorLog.Location = new System.Drawing.Point(533, 646);
         this.btnErrorLog.Name = "btnErrorLog";
         this.btnErrorLog.Size = new System.Drawing.Size(116, 26);
         this.btnErrorLog.TabIndex = 9;
         this.btnErrorLog.Text = "Show error log";
         this.btnErrorLog.UseVisualStyleBackColor = true;
         this.btnErrorLog.Visible = false;
         this.btnErrorLog.Click += new System.EventHandler(this.btnErrorLog_Click);
         // 
         // linkLabel2
         // 
         this.linkLabel2.AutoSize = true;
         this.linkLabel2.Location = new System.Drawing.Point(198, 98);
         this.linkLabel2.Margin = new System.Windows.Forms.Padding(5);
         this.linkLabel2.Name = "linkLabel2";
         this.linkLabel2.Size = new System.Drawing.Size(258, 13);
         this.linkLabel2.TabIndex = 11;
         this.linkLabel2.TabStop = true;
         this.linkLabel2.Text = "https://www.youtube.com/watch?v=ApumbtDAqMM";
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.label3.Location = new System.Drawing.Point(150, 98);
         this.label3.Margin = new System.Windows.Forms.Padding(5);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(42, 13);
         this.label3.TabIndex = 10;
         this.label3.Text = "HowTo";
         // 
         // DebugForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(766, 685);
         this.Controls.Add(this.linkLabel2);
         this.Controls.Add(this.label3);
         this.Controls.Add(this.btnErrorLog);
         this.Controls.Add(this.btnClearCache);
         this.Controls.Add(this.dataGridView1);
         this.Controls.Add(this.shortcutsDetails);
         this.Controls.Add(this.linkLabel1);
         this.Controls.Add(this.label6);
         this.Controls.Add(this.label5);
         this.Controls.Add(this.label4);
         this.Controls.Add(this.pictureBox1);
         this.Controls.Add(this.versionLbl);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.label1);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
         this.Name = "DebugForm";
         this.Padding = new System.Windows.Forms.Padding(10);
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "About: Fortran CodeNav";
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label versionLbl;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.TextBox shortcutsDetails;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Command;
        private System.Windows.Forms.DataGridViewTextBoxColumn Shortcut;
        private System.Windows.Forms.DataGridViewTextBoxColumn Conflicts;
        private System.Windows.Forms.Button btnClearCache;
        private System.Windows.Forms.Button btnErrorLog;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.Label label3;
    }
}