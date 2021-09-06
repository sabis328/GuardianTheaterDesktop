
namespace Guardian_Theater_Desktop
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.panelCarnageSettings = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonCarnageMenu = new System.Windows.Forms.Button();
            this.panelCharacterMenu = new System.Windows.Forms.Panel();
            this.butttonCharacter3 = new System.Windows.Forms.Button();
            this.buttonCharacter2 = new System.Windows.Forms.Button();
            this.buttonCharacter1 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.buttonGuardianSearch = new System.Windows.Forms.Button();
            this.buttonDashboard = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.characterIndicator = new System.Windows.Forms.Panel();
            this.selectedMenuIndicator = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.buttonExitApplicaiton = new System.Windows.Forms.Button();
            this.FormContainerPanel = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panelCarnageSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.panelCharacterMenu.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.panelCarnageSettings);
            this.panel1.Controls.Add(this.buttonCarnageMenu);
            this.panel1.Controls.Add(this.panelCharacterMenu);
            this.panel1.Controls.Add(this.buttonGuardianSearch);
            this.panel1.Controls.Add(this.buttonDashboard);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.panel5);
            this.panel1.Controls.Add(this.buttonExitApplicaiton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(264, 608);
            this.panel1.TabIndex = 0;
            // 
            // panelCarnageSettings
            // 
            this.panelCarnageSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(50)))), ((int)(((byte)(56)))));
            this.panelCarnageSettings.Controls.Add(this.label3);
            this.panelCarnageSettings.Controls.Add(this.checkBox1);
            this.panelCarnageSettings.Controls.Add(this.numericUpDown1);
            this.panelCarnageSettings.Controls.Add(this.label2);
            this.panelCarnageSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelCarnageSettings.Location = new System.Drawing.Point(10, 392);
            this.panelCarnageSettings.Name = "panelCarnageSettings";
            this.panelCarnageSettings.Size = new System.Drawing.Size(252, 65);
            this.panelCarnageSettings.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(3, 33);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(167, 16);
            this.label3.TabIndex = 3;
            this.label3.Text = "Cache pattern matched names";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox1.ForeColor = System.Drawing.Color.White;
            this.checkBox1.Location = new System.Drawing.Point(186, 32);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(80, 20);
            this.checkBox1.TabIndex = 2;
            this.checkBox1.Text = "(Disabled)";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numericUpDown1.Location = new System.Drawing.Point(186, 6);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(71, 20);
            this.numericUpDown1.TabIndex = 0;
            this.numericUpDown1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown1.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(3, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(147, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Matches to load (max 200)";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // buttonCarnageMenu
            // 
            this.buttonCarnageMenu.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonCarnageMenu.FlatAppearance.BorderSize = 0;
            this.buttonCarnageMenu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCarnageMenu.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCarnageMenu.ForeColor = System.Drawing.Color.White;
            this.buttonCarnageMenu.Location = new System.Drawing.Point(10, 347);
            this.buttonCarnageMenu.Name = "buttonCarnageMenu";
            this.buttonCarnageMenu.Size = new System.Drawing.Size(252, 45);
            this.buttonCarnageMenu.TabIndex = 5;
            this.buttonCarnageMenu.Text = "Carnage Report Settings";
            this.buttonCarnageMenu.UseVisualStyleBackColor = true;
            this.buttonCarnageMenu.Click += new System.EventHandler(this.buttonCarnageMenu_Click);
            // 
            // panelCharacterMenu
            // 
            this.panelCharacterMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.panelCharacterMenu.Controls.Add(this.butttonCharacter3);
            this.panelCharacterMenu.Controls.Add(this.buttonCharacter2);
            this.panelCharacterMenu.Controls.Add(this.buttonCharacter1);
            this.panelCharacterMenu.Controls.Add(this.comboBox1);
            this.panelCharacterMenu.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelCharacterMenu.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelCharacterMenu.Location = new System.Drawing.Point(10, 200);
            this.panelCharacterMenu.Name = "panelCharacterMenu";
            this.panelCharacterMenu.Size = new System.Drawing.Size(252, 147);
            this.panelCharacterMenu.TabIndex = 4;
            // 
            // butttonCharacter3
            // 
            this.butttonCharacter3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(50)))), ((int)(((byte)(56)))));
            this.butttonCharacter3.Dock = System.Windows.Forms.DockStyle.Top;
            this.butttonCharacter3.FlatAppearance.BorderSize = 0;
            this.butttonCharacter3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butttonCharacter3.ForeColor = System.Drawing.Color.White;
            this.butttonCharacter3.Location = new System.Drawing.Point(0, 107);
            this.butttonCharacter3.Name = "butttonCharacter3";
            this.butttonCharacter3.Size = new System.Drawing.Size(252, 40);
            this.butttonCharacter3.TabIndex = 3;
            this.butttonCharacter3.Text = "No Character Laoded";
            this.butttonCharacter3.UseVisualStyleBackColor = false;
            this.butttonCharacter3.Click += new System.EventHandler(this.butttonCharacter3_Click);
            // 
            // buttonCharacter2
            // 
            this.buttonCharacter2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(50)))), ((int)(((byte)(56)))));
            this.buttonCharacter2.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonCharacter2.FlatAppearance.BorderSize = 0;
            this.buttonCharacter2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCharacter2.ForeColor = System.Drawing.Color.White;
            this.buttonCharacter2.Location = new System.Drawing.Point(0, 67);
            this.buttonCharacter2.Name = "buttonCharacter2";
            this.buttonCharacter2.Size = new System.Drawing.Size(252, 40);
            this.buttonCharacter2.TabIndex = 2;
            this.buttonCharacter2.Text = "No Character Loaded";
            this.buttonCharacter2.UseVisualStyleBackColor = false;
            this.buttonCharacter2.Click += new System.EventHandler(this.buttonCharacter2_Click);
            // 
            // buttonCharacter1
            // 
            this.buttonCharacter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(50)))), ((int)(((byte)(56)))));
            this.buttonCharacter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonCharacter1.FlatAppearance.BorderSize = 0;
            this.buttonCharacter1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCharacter1.ForeColor = System.Drawing.Color.White;
            this.buttonCharacter1.Location = new System.Drawing.Point(0, 27);
            this.buttonCharacter1.Name = "buttonCharacter1";
            this.buttonCharacter1.Size = new System.Drawing.Size(252, 40);
            this.buttonCharacter1.TabIndex = 1;
            this.buttonCharacter1.Text = "No Character Loaded";
            this.buttonCharacter1.UseVisualStyleBackColor = false;
            this.buttonCharacter1.Click += new System.EventHandler(this.buttonCharacter1_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.comboBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.comboBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox1.ForeColor = System.Drawing.Color.White;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "\tXbox Characters",
            "\tPSN Characters",
            "\tSteam Characters"});
            this.comboBox1.Location = new System.Drawing.Point(0, 0);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(252, 27);
            this.comboBox1.TabIndex = 4;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            this.comboBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.comboBox1_MouseDown);
            // 
            // buttonGuardianSearch
            // 
            this.buttonGuardianSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonGuardianSearch.FlatAppearance.BorderSize = 0;
            this.buttonGuardianSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonGuardianSearch.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonGuardianSearch.ForeColor = System.Drawing.Color.White;
            this.buttonGuardianSearch.Location = new System.Drawing.Point(10, 155);
            this.buttonGuardianSearch.Name = "buttonGuardianSearch";
            this.buttonGuardianSearch.Size = new System.Drawing.Size(252, 45);
            this.buttonGuardianSearch.TabIndex = 3;
            this.buttonGuardianSearch.Text = "Guardian Search";
            this.buttonGuardianSearch.UseVisualStyleBackColor = true;
            this.buttonGuardianSearch.Click += new System.EventHandler(this.button2_Click);
            // 
            // buttonDashboard
            // 
            this.buttonDashboard.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonDashboard.FlatAppearance.BorderSize = 0;
            this.buttonDashboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDashboard.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDashboard.ForeColor = System.Drawing.Color.White;
            this.buttonDashboard.Location = new System.Drawing.Point(10, 110);
            this.buttonDashboard.Name = "buttonDashboard";
            this.buttonDashboard.Size = new System.Drawing.Size(252, 45);
            this.buttonDashboard.TabIndex = 1;
            this.buttonDashboard.Text = "Dashboard";
            this.buttonDashboard.UseVisualStyleBackColor = true;
            this.buttonDashboard.Click += new System.EventHandler(this.buttonDashboard_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(40)))), ((int)(((byte)(46)))));
            this.panel2.Controls.Add(this.characterIndicator);
            this.panel2.Controls.Add(this.selectedMenuIndicator);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 110);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(10, 451);
            this.panel2.TabIndex = 8;
            // 
            // characterIndicator
            // 
            this.characterIndicator.BackColor = System.Drawing.Color.White;
            this.characterIndicator.Location = new System.Drawing.Point(0, 130);
            this.characterIndicator.Name = "characterIndicator";
            this.characterIndicator.Size = new System.Drawing.Size(10, 40);
            this.characterIndicator.TabIndex = 1;
            this.characterIndicator.Paint += new System.Windows.Forms.PaintEventHandler(this.characterIndicator_Paint);
            // 
            // selectedMenuIndicator
            // 
            this.selectedMenuIndicator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(0)))), ((int)(((byte)(122)))));
            this.selectedMenuIndicator.Location = new System.Drawing.Point(0, 0);
            this.selectedMenuIndicator.Name = "selectedMenuIndicator";
            this.selectedMenuIndicator.Size = new System.Drawing.Size(10, 45);
            this.selectedMenuIndicator.TabIndex = 0;
            // 
            // panel5
            // 
            this.panel5.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel5.BackgroundImage")));
            this.panel5.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel5.Location = new System.Drawing.Point(0, 0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(262, 110);
            this.panel5.TabIndex = 0;
            // 
            // buttonExitApplicaiton
            // 
            this.buttonExitApplicaiton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(50)))), ((int)(((byte)(56)))));
            this.buttonExitApplicaiton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonExitApplicaiton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonExitApplicaiton.FlatAppearance.BorderSize = 0;
            this.buttonExitApplicaiton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonExitApplicaiton.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonExitApplicaiton.ForeColor = System.Drawing.Color.White;
            this.buttonExitApplicaiton.Image = ((System.Drawing.Image)(resources.GetObject("buttonExitApplicaiton.Image")));
            this.buttonExitApplicaiton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonExitApplicaiton.Location = new System.Drawing.Point(0, 561);
            this.buttonExitApplicaiton.Name = "buttonExitApplicaiton";
            this.buttonExitApplicaiton.Size = new System.Drawing.Size(262, 45);
            this.buttonExitApplicaiton.TabIndex = 7;
            this.buttonExitApplicaiton.Text = "Exit";
            this.buttonExitApplicaiton.UseVisualStyleBackColor = false;
            this.buttonExitApplicaiton.Click += new System.EventHandler(this.button9_Click);
            // 
            // FormContainerPanel
            // 
            this.FormContainerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(50)))), ((int)(((byte)(56)))));
            this.FormContainerPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.FormContainerPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FormContainerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.FormContainerPanel.Location = new System.Drawing.Point(264, 38);
            this.FormContainerPanel.Name = "FormContainerPanel";
            this.FormContainerPanel.Size = new System.Drawing.Size(748, 320);
            this.FormContainerPanel.TabIndex = 2;
            this.FormContainerPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.FormContainerPanel_Paint);
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(0)))), ((int)(((byte)(122)))));
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.label1);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(264, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(748, 38);
            this.panel4.TabIndex = 3;
            this.panel4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel4_MouseDown);
            this.panel4.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel4_MouseMove);
            this.panel4.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel4_MouseUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(267, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(184, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Guardian Theater Desktop";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1012, 608);
            this.Controls.Add(this.FormContainerPanel);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Guardian Theater Desktop";
            this.panel1.ResumeLayout(false);
            this.panelCarnageSettings.ResumeLayout(false);
            this.panelCarnageSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.panelCharacterMenu.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panelCharacterMenu;
        private System.Windows.Forms.Button butttonCharacter3;
        private System.Windows.Forms.Button buttonCharacter2;
        private System.Windows.Forms.Button buttonCharacter1;
        private System.Windows.Forms.Button buttonGuardianSearch;
        private System.Windows.Forms.Button buttonDashboard;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel FormContainerPanel;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panelCarnageSettings;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonCarnageMenu;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button buttonExitApplicaiton;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel selectedMenuIndicator;
        private System.Windows.Forms.Panel characterIndicator;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}