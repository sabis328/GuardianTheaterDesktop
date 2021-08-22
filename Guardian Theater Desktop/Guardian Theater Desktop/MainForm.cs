using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace Guardian_Theater_Desktop
{
    public partial class MainForm : Form
    {

        public Guardian savedGuardian;
        public string BungieKey { get; set; }
        public string TwitchKey { get; set; }
        public string TwitchAuth { get; set; }

        public int ReportCount { get; set; }

        #region AutoUpdater

        public void AutoUpdate()
        {
            //https://raw.githubusercontent.com/sabis328/GuardianTheaterDesktop/main/Guardian%20Theater%20Desktop/Properties/AssemblyInfo.cs
            //https://github.com/sabis328/GuardianTheaterDesktop/blob/main/Guardian%20Theater%20Desktop/bin/Debug/Guardian%20Theater%20Desktop.exe?raw=true

            HttpWebRequest client = (HttpWebRequest)WebRequest.Create("https://raw.githubusercontent.com/sabis328/GuardianTheaterDesktop/main/Guardian%20Theater%20Desktop/Properties/AssemblyInfo.cs");
            string LatestVerion;
            using (HttpWebResponse response = (HttpWebResponse)client.GetResponse())
            {
                string text = new StreamReader(response.GetResponseStream()).ReadToEnd();
                System.Diagnostics.Debug.Print(text);
                int start = 0;
                int end = 0;

                //AssemblyFileVersion("

                string fileVersionSearch = "AssemblyFileVersion(\"";
                start = text.IndexOf(fileVersionSearch, end) + fileVersionSearch.Length;
                end = text.IndexOf("\"", start);
                LatestVerion = text.Substring(start, end - start);
            }

            if (Application.ProductVersion != LatestVerion)
            {
                UpdateGuardianClient();
            }
        }

        private void UpdateGuardianClient()
        {

            if (InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                { UpdateGuardianClient(); });
                return;
            }
            System.Diagnostics.Debug.Print("updating application");
            WebClient Downloader = new WebClient();

            byte[] filebuffer = Downloader.DownloadData("https://github.com/sabis328/GuardianTheaterDesktop/blob/main/Guardian%20Theater%20Desktop/bin/Debug/Guardian%20Theater%20Desktop.exe?raw=true");
            var appLoc = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string CurrentPath = Path.GetFileName(appLoc);
            string CurrentPathTrimmed = Path.Combine(Path.GetDirectoryName(appLoc), Path.GetFileNameWithoutExtension(appLoc));

            File.WriteAllBytes(CurrentPathTrimmed + "Update.exe", filebuffer);

            using (var BatchUpdater = new StreamWriter(File.Create(CurrentPathTrimmed + "Update.bat")))
            {
                BatchUpdater.WriteLine("@ECHO OFF");
                BatchUpdater.WriteLine("TIMEOUT /t 1 /nobreak > NUL");
                BatchUpdater.WriteLine("TASKKILL /IM \"{0}\" > NUL", CurrentPath);
                BatchUpdater.WriteLine("MOVE \"{0}\" \"{1}\"", CurrentPathTrimmed + "Update.exe", appLoc);
                BatchUpdater.WriteLine("DEL \"%~f0\" & START \"\" /B \"{0}\"", appLoc);
            }

            ProcessStartInfo startBatch = new ProcessStartInfo(CurrentPathTrimmed + "Update.bat");
            startBatch.WorkingDirectory = Path.GetDirectoryName(appLoc);
            Process.Start(startBatch);

            Environment.Exit(0);
        }


        #endregion


        public void UpdateFormPaints()
        {
            panel4.BackColor = Properties.Settings.Default.HeaderFooterColor;
            selectedMenuIndicator.BackColor = Properties.Settings.Default.MenuIndicatorColor;
            characterIndicator.BackColor = Properties.Settings.Default.SelectedCharacterColor;
            buttonExitApplicaiton.BackColor = Properties.Settings.Default.HeaderFooterColor;
            ReportForm.Updatepaint();
            SearchForm.Updatepaint();
        }

        public MainForm()
        {
            InitializeComponent();
            this.BringToFront();
            Task.Run(() => AutoUpdate());

            

            ReportCount = (int)numericUpDown1.Value;
            #region Load and hide all child forms
            SearchForm = new guardianSearchForm(this);
            ReportForm = new carnageReportsForm(this);
            DashBoardForm = new userSettingsForm(this);

            SearchForm.Visible = false;
            DashBoardForm.Visible = false;
            ReportForm.Visible = false;

            SearchForm.TopLevel = false;
            ReportForm.TopLevel = false;
            DashBoardForm.TopLevel = false;

            FormContainerPanel.Controls.Add(SearchForm);
            FormContainerPanel.Controls.Add(ReportForm);
            FormContainerPanel.Controls.Add(DashBoardForm);
            SearchForm.Dock = DockStyle.Fill;
            ReportForm.Dock = DockStyle.Fill;
            DashBoardForm.Dock = DockStyle.Fill;
            HideCharacterSubmenu();
            HideReportsSubmenu();
            ShowDashboard();
            UpdateFormPaints();


            if(Properties.Settings.Default.SaveLastSearch)
            {
                if(Properties.Settings.Default.MyAccountDisplayName != "null")
                {
                    ShowSearchMenu();
                }
            }
            #endregion


            numericUpDown1.Value = Properties.Settings.Default.ReportCounter;

        }
        #region Allow moving form without border
        private bool mouseDown;
        private Point lastLocation;
        private void panel4_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }

        }

        private void panel4_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        private void panel4_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        #endregion

        #region show/hide childforms
        private guardianSearchForm SearchForm;
        private carnageReportsForm ReportForm;
        private userSettingsForm DashBoardForm;


        private void ShowReportsForm(int charNum)
        {
            if (!ReportForm.Visible)
            {
                ReportForm.Visible = true;
                ReportForm.BringToFront();
                SearchForm.Visible = false;
                DashBoardForm.Visible = false;
            }

           
            ReportForm.SetCharacter((Guardian)buttonCharacter1.Tag, charNum);

            if(Properties.Settings.Default.SaveLastSearch)
            {
                Guardian g = (Guardian)buttonCharacter1.Tag;
                Properties.Settings.Default.MyAccountDisplayName = g.MainDisplayName;
                Properties.Settings.Default.MyAccountMainID = g.MainAccountIdentifier;
                Properties.Settings.Default.MyAccountMainType = g.MainType.ToString();
                Properties.Settings.Default.MyAccountLastCharacterIdentifier = g.CharacterEntries[charNum].CharacterIdentifier;
                Properties.Settings.Default.Save();
            }
            
        }

        private void ShowReportsFormFromSaved()
        {
            if (!ReportForm.Visible)
            {
                ReportForm.Visible = true;
                ReportForm.BringToFront();
                SearchForm.Visible = false;
                DashBoardForm.Visible = false;
            }
            System.Diagnostics.Debug.Print("Showing reports for saved character : " + Properties.Settings.Default.MyAccountLastCharacterIdentifier);
            ReportForm.SetFromSavedCharacter(savedGuardian,Properties.Settings.Default.MyAccountLastCharacterIdentifier);
        }
        private void ShowDashboard()
        {
            DashBoardForm.Visible = true;
            DashBoardForm.BringToFront();
            SearchForm.Visible = false;
            ReportForm.Visible = false;
        }
        private void ShowSearchMenu()
        {
            SearchForm.Visible = true;
            SearchForm.BringToFront();
            ReportForm.Visible = false;
            DashBoardForm.Visible = false;
        }

        #endregion
        #region Show/hide Menus and update indicator
        private void updateSelectedIdeicatorLocating(int y)
        {
            selectedMenuIndicator.Location = new Point(0, y - 110);
        }
        private void ShowCharacterSubMenu(int cout = 3)
        {
            panelCharacterMenu.Visible = true;
        }
        public void HideCharacterSubmenu()
        {
            panelCharacterMenu.Visible = false;
            characterIndicator.Visible = false;
        }
        private void ShowReportsSubmen()
        {
            panelCarnageSettings.Visible = true;
        }
        private void HideReportsSubmenu()
        {
            panelCarnageSettings.Visible = false;
        }

        private void buttonCarnageMenu_Click(object sender, EventArgs e)
        {
            updateSelectedIdeicatorLocating(buttonCarnageMenu.Location.Y);
            if (panelCarnageSettings.Visible)
            {
                HideReportsSubmenu();
            }
            else
            {
                ShowReportsSubmen();
            }
        }

        private void buttonDashboard_Click(object sender, EventArgs e)
        {
            updateSelectedIdeicatorLocating(buttonDashboard.Location.Y);
            ShowDashboard();
        }

        #endregion

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }



        private void button9_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }



        #region Search for guardian related methods/controls
        private void button2_Click(object sender, EventArgs e)
        {
            updateSelectedIdeicatorLocating(buttonGuardianSearch.Location.Y);
            fromUser = false;
            ShowSearchMenu();
            if (!userSelected)
            {
                HideCharacterSubmenu();
            }
            
        }

        public bool userSelected = false;

        //Used to update the character buttons for loading carnage reports
        //Button is tagged with the guardian, will need to call which char based on which button
        public void UpdateSelectedUser(Guardian selected)
        {
            if(selected != null)
            {
                savedGuardian = selected;
                userSelected = true;
                if(selected.CharacterEntries.Count > 0)
                {
                    switch(selected.CharacterEntries.Count)
                    {
                        case 1:
                            
                            buttonCharacter1.Visible = true;
                            buttonCharacter2.Visible = false;
                            butttonCharacter3.Visible = false;

                            buttonCharacter1.Tag = selected;
                            buttonCharacter1.Text = selected.MainDisplayName + " | " + selected.CharacterEntries[0].CharacterClass.ToString();
                            break;
                        case 2:
                           
                            buttonCharacter1.Visible = true;
                            buttonCharacter2.Visible = true;
                            butttonCharacter3.Visible = false;
                            buttonCharacter1.Tag = selected;
                            buttonCharacter1.Text = selected.MainDisplayName + " | " + selected.CharacterEntries[0].CharacterClass.ToString();
                            buttonCharacter2.Tag = selected;
                            buttonCharacter2.Text = selected.MainDisplayName + " | " + selected.CharacterEntries[1].CharacterClass.ToString();
                            break;
                        case 3:
                           
                            buttonCharacter1.Visible = true;
                            buttonCharacter2.Visible = true;
                            butttonCharacter3.Visible = true;
                            buttonCharacter1.Tag = selected;
                            buttonCharacter1.Text = selected.MainDisplayName + " | " + selected.CharacterEntries[0].CharacterClass.ToString();
                            buttonCharacter2.Tag = selected;
                            buttonCharacter2.Text = selected.MainDisplayName + " | " + selected.CharacterEntries[1].CharacterClass.ToString();
                            butttonCharacter3.Tag = selected;
                            butttonCharacter3.Text = selected.MainDisplayName + " | " + selected.CharacterEntries[2].CharacterClass.ToString();
                            break;
                    }

                    switch(selected.MainType)
                    {
                        case Guardian.BungieAccount.AccountType.Xbox:
                            comboBox1.SelectedIndex = 0;
                            break;
                        case Guardian.BungieAccount.AccountType.PSN:
                            comboBox1.SelectedIndex = 1;
                            break;
                        case Guardian.BungieAccount.AccountType.Steam:
                            comboBox1.SelectedIndex = 2;
                            break;
                    }

                    if (Properties.Settings.Default.SaveLastSearch && Properties.Settings.Default.MyAccountLastCharacterIdentifier != "null")
                    {
                        System.Diagnostics.Debug.Print("Need to load last reports from saved users");
                        savedGuardian = selected;
                        Properties.Settings.Default.MyAccountDisplayName = selected.MainDisplayName;
                        Properties.Settings.Default.MyAccountMainID = selected.MainAccountIdentifier;
                        Properties.Settings.Default.MyAccountMainType = selected.MainType.ToString();
                        Properties.Settings.Default.Save();
                        ShowReportsFormFromSaved();
                        ShowCharacterSubMenu();
                        return;
                    }
                    else
                    {
                        ShowCharacterSubMenu();
                        fromUser = true;
                    }
                }
               
            }
            else
            {
                HideCharacterSubmenu();
            }
        }

        public void UpdateSelectedUserType(Guardian.BungieAccount.AccountType accType)
        {
            fromUser = false;
            if(Properties.Settings.Default.SaveLastSearch)
            {
                Properties.Settings.Default.MyAccountMainType = accType.ToString();
                Properties.Settings.Default.Save();
            }
            bool changed = false;
            foreach(Guardian.BungieAccount bacc in savedGuardian.LinkedAccounts)
            {
                if(bacc.UserType == accType)
                {
                    savedGuardian.MainAccountIdentifier = bacc.AccountIdentifier;
                    savedGuardian.MainDisplayName = bacc.DisplayName;
                    savedGuardian.MainType = accType;
                    changed = true;
                    break;
                }
            }

            
            if(changed)
            {
                SearchForm.LoadAlternatePlatform(savedGuardian);
                ShowSearchMenu();
                HideCharacterSubmenu();
            }
           
        }

        public void SetSelectedCharacter(int buttonOption)
        {
            System.Diagnostics.Debug.Print("setting location of char marker" + characterIndicator.Location.ToString());
            characterIndicator.Visible = true;

            switch(buttonOption)
            {
                case 0:
                    characterIndicator.Location = new Point(0, 117);
                    break;
                case 1:
                    characterIndicator.Location = new Point(0, 157);
                    break;
                case 2:
                    characterIndicator.Location = new Point(0, 197);
                    break;
            }

            System.Diagnostics.Debug.Print("adjusted location of char marker" + characterIndicator.Location.ToString());

        }
        #endregion

        #region LoadCarnageReports
        private void buttonCharacter1_Click(object sender, EventArgs e)
        {
            ShowReportsForm(0);
           // SetSelectedCharacter(buttonCharacter1);
        }

        private void buttonCharacter2_Click(object sender, EventArgs e)
        {
            ShowReportsForm(1);
            //SetSelectedCharacter(buttonCharacter1);
        }
        private void butttonCharacter3_Click(object sender, EventArgs e)
        {
            ShowReportsForm(2);
            //SetSelectedCharacter(buttonCharacter1);
        }
        #endregion

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            ReportCount = (int)numericUpDown1.Value;
            Properties.Settings.Default.ReportCounter = (int)numericUpDown1.Value;
            Properties.Settings.Default.Save();
        }

        private void characterIndicator_Paint(object sender, PaintEventArgs e)
        {

        }

        private void FormContainerPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private bool fromUser { get; set; }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (fromUser)
            {
                Guardian.BungieAccount.AccountType mainType = Guardian.BungieAccount.AccountType.Ignore;
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        mainType = Guardian.BungieAccount.AccountType.Xbox;
                        break;
                    case 1:
                        mainType = Guardian.BungieAccount.AccountType.PSN;
                        break;
                    case 2:
                        mainType = Guardian.BungieAccount.AccountType.Steam;
                        break;
                }

                if (mainType != Guardian.BungieAccount.AccountType.Ignore)
                {
                    UpdateSelectedUserType(mainType);
                }
            }
        }

        private void comboBox1_MouseDown(object sender, MouseEventArgs e)
        {
            fromUser = true;
        }
    }
}
