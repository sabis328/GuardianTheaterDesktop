using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Guardian_Theater_Desktop
{
    public partial class guardianSearchForm : Form
    {
        MainForm parent_form;
        public guardianSearchForm(MainForm parent)
        {
            InitializeComponent();
            parent_form = parent;
            
            cli = new TheaterClient();
            cli._BungieApiKey = Properties.Settings.Default.BungieKey;
            cli._TheaterClientEvent += Cli__TheaterClientEvent;

            if(Properties.Settings.Default.SaveLastSearch)
            {
                if(Properties.Settings.Default.MyAccountMainID != "null")
                {
                    LoadFromSettings();
                }
            }
        }
        TheaterClient cli;


        public void Updatepaint()
        {
            panel2.BackColor = Properties.Settings.Default.HeaderFooterColor;
        }

        private bool IsBusy = false;
        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.AlternateAccount = false;
            Properties.Settings.Default.Save();
            if (textBox1.TextLength > 0)
            {
                if (!IsBusy)
                {
                    if (Properties.Settings.Default.SaveLastSearch)
                    {
                        Properties.Settings.Default.MyAccountDisplayName = "null";
                        Properties.Settings.Default.MyAccountMainType = "null";
                        Properties.Settings.Default.MyAccountMainID = "null";
                        Properties.Settings.Default.MyAccountLastCharacterIdentifier = "null";
                        Properties.Settings.Default.Save();
                    }

                    IsBusy = true;
                    parent_form.UpdateSelectedUser(null);
                    label2.Text = "Searching for user : " + textBox1.Text;
                    Task.Run(() => cli.SearchBungieAccounts(textBox1.Text, Guardian.BungieAccount.AccountType.Ignore));
                }
            }
        }

        public void LoadFromSettings()
        {
            if(!IsBusy)
            {
                System.Diagnostics.Debug.Print("Loading from settings : ");
                System.Diagnostics.Debug.Print(Properties.Settings.Default.MyAccountDisplayName);
                System.Diagnostics.Debug.Print(Properties.Settings.Default.MyAccountMainType.ToString());
                System.Diagnostics.Debug.Print(Properties.Settings.Default.MyAccountMainID);
                System.Diagnostics.Debug.Print(Properties.Settings.Default.MyAccountLastCharacterIdentifier);

                IsBusy = true;
                textBox1.Text = Properties.Settings.Default.MyAccountDisplayName;
                parent_form.UpdateSelectedUser(null);
                label2.Text = "Loading for user : " + textBox1.Text;
                Guardian SavedUser = new Guardian();
                SavedUser.MainDisplayName = Properties.Settings.Default.MyAccountDisplayName;
                SavedUser.MainAccountIdentifier = Properties.Settings.Default.MyAccountMainID;
                SavedUser.MainType = cli.AccountTypeFromString(Properties.Settings.Default.MyAccountMainType);

                treeView1.Nodes.Clear();
                ListViewItem gItem = new ListViewItem();
                gItem.Text = SavedUser.MainDisplayName;
                gItem.Tag = SavedUser;
                gItem.SubItems.Add(SavedUser.MainType.ToString());
                listView1.Items.Add(gItem);

                Task.Run(() => cli.LoadCharacterEntries(SavedUser,Properties.Settings.Default.AlternateAccount));
            }
        }

        private bool fromAltLoad = false;
        public void LoadAlternatePlatform(Guardian loadFor)
        {
            fromAltLoad = true;
            Properties.Settings.Default.AlternateAccount = true;
           
            treeView1.Nodes.Clear();
            ListViewItem gItem = new ListViewItem();
            gItem.Text = loadFor.MainDisplayName;
            gItem.Tag = loadFor;
            gItem.SubItems.Add(loadFor.MainType.ToString());
            listView1.Items.Add(gItem);

            Task.Run(() => cli.LoadCharacterEntries(loadFor,true));
        }

        private void Cli__TheaterClientEvent(object sender, TheaterClient.ClientEventType e)
        {
            switch (e)
            {
                case TheaterClient.ClientEventType.SearchComplete:
                    IsBusy = false;
                    fromAltLoad = false;
                    ShowAccounts((List<Guardian>)sender);
                    break;
                case TheaterClient.ClientEventType.CharactersComplete:
                    ShowDetailedAccount((Guardian)sender);
                    IsBusy = false;
                    fromAltLoad = false;
                    break;
                default:
                    IsBusy = false;
                    fromAltLoad = false;
                    break;
            }
        }


        private void ShowDetailedAccount(Guardian loadedPlayer)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    ShowDetailedAccount(loadedPlayer);
                });
                return;
            }

            label2.Text = "Showing detailed information for " + loadedPlayer.MainDisplayName;
            treeView1.Nodes.Clear();
            label1.Text = loadedPlayer.MainDisplayName + " : infomation";
            TreeNode playerNode = new TreeNode(loadedPlayer.MainDisplayName);
            playerNode.Nodes.Add(loadedPlayer.MainAccountIdentifier);
            playerNode.Nodes.Add(loadedPlayer.MainType.ToString());

            if (loadedPlayer.LinkedAccounts != null)
            {
                foreach (Guardian.BungieAccount bacc in loadedPlayer.LinkedAccounts)
                {
                    playerNode.Nodes.Add(bacc.DisplayName + " | " + bacc.UserType.ToString());
                }
                playerNode.Expand();
            }
            if (!fromAltLoad)
            {
                if (Properties.Settings.Default.SaveLastSearch)
                {
                    Properties.Settings.Default.MyAccountDisplayName = loadedPlayer.MainDisplayName;
                    Properties.Settings.Default.MyAccountMainType = loadedPlayer.MainType.ToString();
                    Properties.Settings.Default.MyAccountMainID = loadedPlayer.MainAccountIdentifier;

                    System.Diagnostics.Debug.Print("Finished loading data : checking for saved char value");
                    System.Diagnostics.Debug.Print(Properties.Settings.Default.MyAccountLastCharacterIdentifier);

                    if (Properties.Settings.Default.MyAccountLastCharacterIdentifier == "null")
                    {
                        Properties.Settings.Default.MyAccountLastCharacterIdentifier = "null";
                    }

                    System.Diagnostics.Debug.Print(Properties.Settings.Default.MyAccountLastCharacterIdentifier);
                    Properties.Settings.Default.Save();
                }
            }
            treeView1.Nodes.Add(playerNode);
            parent_form.UpdateSelectedUser(loadedPlayer);
            fromAltLoad = false;
        }
        private void ShowAccounts(List<Guardian> FoundUsers)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    ShowAccounts(FoundUsers);
                });
                return;
            }
            label2.Text = "Found " + FoundUsers.Count + " users for " + textBox1.Text;
           
            foreach (Guardian g in FoundUsers)
            {
                ListViewItem gItem = new ListViewItem();
                gItem.Text = g.MainDisplayName;
                gItem.Tag = g;
                gItem.SubItems.Add(g.MainType.ToString());
                listView1.Items.Add(gItem);
            }

            if (FoundUsers.Count == 1)
            {
                IsBusy = true;

                label1.Text = "Loading player information";
                label2.Text = "Loading detailed player information";
                parent_form.HideCharacterSubmenu();
                treeView1.Nodes.Clear();
                Guardian playerload = FoundUsers[0];
                Task.Run(() => cli.LoadCharacterEntries(playerload,false));
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!IsBusy)
            {
                Properties.Settings.Default.AlternateAccount = false;
                Properties.Settings.Default.Save();
                IsBusy = true;
                if (Properties.Settings.Default.SaveLastSearch)
                {
                    Properties.Settings.Default.MyAccountDisplayName = "null";
                    Properties.Settings.Default.MyAccountMainType = "null";
                    Properties.Settings.Default.MyAccountMainID = "null";
                    Properties.Settings.Default.MyAccountLastCharacterIdentifier = "null";
                    Properties.Settings.Default.Save();
                }
                label1.Text = "Loading player information";
                label2.Text = "Loading detailed player information";
                parent_form.HideCharacterSubmenu();
                treeView1.Nodes.Clear();
                Guardian playerload = (Guardian)listView1.SelectedItems[0].Tag;
                Task.Run(() => cli.LoadCharacterEntries(playerload, false)) ;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(!IsBusy)
            {
                listView1.Items.Clear();
                label2.Text = "Idle";
            }
        }
    }
}
