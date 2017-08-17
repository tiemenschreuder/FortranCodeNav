namespace VSIntegration.CodeComplete
{
    partial class StatementCompleteList
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
            this.components = new System.ComponentModel.Container();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.signatureToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.resultsListBox = new VSIntegration.CodeComplete.ImageListBox();
            this.SuspendLayout();
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 0;
            this.toolTip.ReshowDelay = 100;
            // 
            // signatureToolTip
            // 
            this.signatureToolTip.AutoPopDelay = 5000;
            this.signatureToolTip.InitialDelay = 0;
            this.signatureToolTip.IsBalloon = true;
            this.signatureToolTip.ReshowDelay = 100;
            // 
            // resultsListBox
            // 
            this.resultsListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.resultsListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultsListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.resultsListBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resultsListBox.ItemHeight = 16;
            this.resultsListBox.Items.AddRange(new object[] {
            "Initialize",
            "Finalize",
            "TreeViewImageKeyConverter"});
            this.resultsListBox.Location = new System.Drawing.Point(0, 0);
            this.resultsListBox.Name = "resultsListBox";
            this.resultsListBox.Size = new System.Drawing.Size(182, 162);
            this.resultsListBox.TabIndex = 0;
            // 
            // StatementCompleteList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(182, 162);
            this.ControlBox = false;
            this.Controls.Add(this.resultsListBox);
            this.Name = "StatementCompleteList";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.ResumeLayout(false);

        }

        #endregion

        private ImageListBox resultsListBox;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ToolTip signatureToolTip;

    }
}