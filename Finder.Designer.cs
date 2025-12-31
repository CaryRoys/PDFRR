namespace PdfRedactionRemover
{
    partial class Finder
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnBrowse = new Button();
            txtFolderPath = new TextBox();
            lblFolderPath = new Label();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            saveReportToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            aboutRedactionFinderToolStripMenuItem = new ToolStripMenuItem();
            txtReport = new TextBox();
            btnSearch = new Button();
            cbRecurse = new CheckBox();
            pbFileProgress = new ProgressBar();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(449, 57);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(75, 23);
            btnBrowse.TabIndex = 0;
            btnBrowse.Text = "&Browse";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // txtFolderPath
            // 
            txtFolderPath.Location = new Point(12, 57);
            txtFolderPath.Name = "txtFolderPath";
            txtFolderPath.Size = new Size(431, 23);
            txtFolderPath.TabIndex = 1;
            // 
            // lblFolderPath
            // 
            lblFolderPath.AutoSize = true;
            lblFolderPath.Location = new Point(12, 30);
            lblFolderPath.Name = "lblFolderPath";
            lblFolderPath.Size = new Size(67, 15);
            lblFolderPath.TabIndex = 2;
            lblFolderPath.Text = "Folder Path";
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, aboutToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 24);
            menuStrip1.TabIndex = 3;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { saveReportToolStripMenuItem, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            fileToolStripMenuItem.Click += fileToolStripMenuItem_Click;
            // 
            // saveReportToolStripMenuItem
            // 
            saveReportToolStripMenuItem.Name = "saveReportToolStripMenuItem";
            saveReportToolStripMenuItem.Size = new Size(136, 22);
            saveReportToolStripMenuItem.Text = "&Save Report";
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(136, 22);
            exitToolStripMenuItem.Text = "&Exit";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutRedactionFinderToolStripMenuItem });
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(52, 20);
            aboutToolStripMenuItem.Text = "&About";
            // 
            // aboutRedactionFinderToolStripMenuItem
            // 
            aboutRedactionFinderToolStripMenuItem.Name = "aboutRedactionFinderToolStripMenuItem";
            aboutRedactionFinderToolStripMenuItem.Size = new Size(199, 22);
            aboutRedactionFinderToolStripMenuItem.Text = "A&bout Redaction Finder";
            // 
            // txtReport
            // 
            txtReport.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtReport.Enabled = false;
            txtReport.Location = new Point(12, 112);
            txtReport.Multiline = true;
            txtReport.Name = "txtReport";
            txtReport.Size = new Size(776, 326);
            txtReport.TabIndex = 4;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(530, 57);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(75, 23);
            btnSearch.TabIndex = 5;
            btnSearch.Text = "&Search";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // cbRecurse
            // 
            cbRecurse.AutoSize = true;
            cbRecurse.Location = new Point(619, 57);
            cbRecurse.Name = "cbRecurse";
            cbRecurse.Size = new Size(87, 19);
            cbRecurse.TabIndex = 6;
            cbRecurse.Text = "Subfolders?";
            cbRecurse.UseVisualStyleBackColor = true;
            // 
            // pbFileProgress
            // 
            pbFileProgress.Location = new Point(12, 83);
            pbFileProgress.Name = "pbFileProgress";
            pbFileProgress.Size = new Size(776, 23);
            pbFileProgress.TabIndex = 7;
            // 
            // Finder
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(pbFileProgress);
            Controls.Add(cbRecurse);
            Controls.Add(btnSearch);
            Controls.Add(txtReport);
            Controls.Add(lblFolderPath);
            Controls.Add(txtFolderPath);
            Controls.Add(btnBrowse);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Finder";
            Text = "Find Redacted PDF's";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnBrowse;
        private TextBox txtFolderPath;
        private Label lblFolderPath;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem saveReportToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem aboutRedactionFinderToolStripMenuItem;
        private TextBox txtReport;
        private Button btnSearch;
        private CheckBox cbRecurse;
        private ProgressBar pbFileProgress;
    }
}
