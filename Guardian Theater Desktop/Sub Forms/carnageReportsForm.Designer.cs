
namespace Guardian_Theater_Desktop
{
    partial class carnageReportsForm
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
            this.treeviewCarnageList = new System.Windows.Forms.TreeView();
            this.treeviewStreamList = new System.Windows.Forms.TreeView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonExportStreams = new System.Windows.Forms.Button();
            this.buttonRefreshMatches = new System.Windows.Forms.Button();
            this.CarnageHeader = new System.Windows.Forms.Label();
            this.StreamHeader = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeviewCarnageList
            // 
            this.treeviewCarnageList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.treeviewCarnageList.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeviewCarnageList.ForeColor = System.Drawing.Color.White;
            this.treeviewCarnageList.LineColor = System.Drawing.Color.White;
            this.treeviewCarnageList.Location = new System.Drawing.Point(12, 61);
            this.treeviewCarnageList.Name = "treeviewCarnageList";
            this.treeviewCarnageList.Size = new System.Drawing.Size(298, 420);
            this.treeviewCarnageList.TabIndex = 0;
            // 
            // treeviewStreamList
            // 
            this.treeviewStreamList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.treeviewStreamList.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeviewStreamList.ForeColor = System.Drawing.Color.White;
            this.treeviewStreamList.Location = new System.Drawing.Point(328, 61);
            this.treeviewStreamList.Name = "treeviewStreamList";
            this.treeviewStreamList.Size = new System.Drawing.Size(392, 380);
            this.treeviewStreamList.TabIndex = 1;
            this.treeviewStreamList.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeviewStreamList_NodeMouseDoubleClick);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(0)))), ((int)(((byte)(122)))));
            this.panel1.Controls.Add(this.StatusLabel);
            this.panel1.Controls.Add(this.progressBar1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 530);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(748, 40);
            this.panel1.TabIndex = 2;
            // 
            // StatusLabel
            // 
            this.StatusLabel.BackColor = System.Drawing.Color.Transparent;
            this.StatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StatusLabel.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StatusLabel.ForeColor = System.Drawing.Color.White;
            this.StatusLabel.Location = new System.Drawing.Point(0, 0);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(748, 30);
            this.StatusLabel.TabIndex = 8;
            this.StatusLabel.Text = "Idle";
            this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(0, 30);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(748, 10);
            this.progressBar1.TabIndex = 0;
            // 
            // buttonExportStreams
            // 
            this.buttonExportStreams.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.buttonExportStreams.FlatAppearance.BorderSize = 0;
            this.buttonExportStreams.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonExportStreams.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonExportStreams.ForeColor = System.Drawing.Color.White;
            this.buttonExportStreams.Location = new System.Drawing.Point(549, 447);
            this.buttonExportStreams.Name = "buttonExportStreams";
            this.buttonExportStreams.Size = new System.Drawing.Size(171, 34);
            this.buttonExportStreams.TabIndex = 6;
            this.buttonExportStreams.Text = "Export Streams";
            this.buttonExportStreams.UseVisualStyleBackColor = false;
            // 
            // buttonRefreshMatches
            // 
            this.buttonRefreshMatches.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.buttonRefreshMatches.FlatAppearance.BorderSize = 0;
            this.buttonRefreshMatches.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonRefreshMatches.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRefreshMatches.ForeColor = System.Drawing.Color.White;
            this.buttonRefreshMatches.Location = new System.Drawing.Point(328, 447);
            this.buttonRefreshMatches.Name = "buttonRefreshMatches";
            this.buttonRefreshMatches.Size = new System.Drawing.Size(215, 34);
            this.buttonRefreshMatches.TabIndex = 7;
            this.buttonRefreshMatches.Text = "Refresh Matches";
            this.buttonRefreshMatches.UseVisualStyleBackColor = false;
            this.buttonRefreshMatches.Click += new System.EventHandler(this.button2_Click);
            // 
            // CarnageHeader
            // 
            this.CarnageHeader.BackColor = System.Drawing.Color.Transparent;
            this.CarnageHeader.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CarnageHeader.ForeColor = System.Drawing.Color.White;
            this.CarnageHeader.Location = new System.Drawing.Point(12, 22);
            this.CarnageHeader.Name = "CarnageHeader";
            this.CarnageHeader.Size = new System.Drawing.Size(298, 36);
            this.CarnageHeader.TabIndex = 8;
            this.CarnageHeader.Text = "Carnage Reports";
            this.CarnageHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // StreamHeader
            // 
            this.StreamHeader.BackColor = System.Drawing.Color.Transparent;
            this.StreamHeader.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StreamHeader.ForeColor = System.Drawing.Color.White;
            this.StreamHeader.Location = new System.Drawing.Point(328, 22);
            this.StreamHeader.Name = "StreamHeader";
            this.StreamHeader.Size = new System.Drawing.Size(392, 36);
            this.StreamHeader.TabIndex = 9;
            this.StreamHeader.Text = "Streams Found";
            this.StreamHeader.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(329, 484);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(391, 43);
            this.label1.TabIndex = 10;
            this.label1.Text = "Double click a stream link to copy it to your clipboard, or export all found stre" +
    "ams to a text file on your desktop";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(12, 487);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(298, 34);
            this.button1.TabIndex = 11;
            this.button1.Text = "Cancel Current Search";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // carnageReportsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(50)))), ((int)(((byte)(56)))));
            this.ClientSize = new System.Drawing.Size(748, 570);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.StreamHeader);
            this.Controls.Add(this.CarnageHeader);
            this.Controls.Add(this.buttonRefreshMatches);
            this.Controls.Add(this.buttonExportStreams);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.treeviewStreamList);
            this.Controls.Add(this.treeviewCarnageList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "carnageReportsForm";
            this.Text = "carnageReportsForm";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeviewCarnageList;
        private System.Windows.Forms.TreeView treeviewStreamList;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label StatusLabel;
        private System.Windows.Forms.Button buttonExportStreams;
        private System.Windows.Forms.Button buttonRefreshMatches;
        private System.Windows.Forms.Label CarnageHeader;
        private System.Windows.Forms.Label StreamHeader;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
    }
}