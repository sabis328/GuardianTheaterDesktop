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
        }
        TheaterClient cli;


        public void Updatepaint()
        {
            panel2.BackColor = Properties.Settings.Default.HeaderFooterColor;
        }

        private bool IsBusy = false;
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.TextLength > 0)
            {
                if (!IsBusy)
                {
                    IsBusy = true;
                    parent_form.UpdateSelectedUser(null);
                    label2.Text = "Searching for user : " + textBox1.Text;
                    Task.Run(() => cli.SearchBungieAccounts(textBox1.Text, Guardian.BungieAccount.AccountType.Ignore));
                }
            }
        }

        private void Cli__TheaterClientEvent(object sender, TheaterClient.ClientEventType e)
        {
            switch (e)
            {
                case TheaterClient.ClientEventType.SearchComplete:
                    IsBusy = false;
                    ShowAccounts((List<Guardian>)sender);
                    break;
                case TheaterClient.ClientEventType.CharactersComplete:
                    ShowDetailedAccount((Guardian)sender);
                    IsBusy = false;
                    break;
                default:
                    IsBusy = false;
                    
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

            foreach(Guardian.BungieAccount bacc in loadedPlayer.LinkedAccounts)
            {
                playerNode.Nodes.Add(bacc.DisplayName + " | " + bacc.UserType.ToString());
            }
            playerNode.Expand();
            treeView1.Nodes.Add(playerNode);

            parent_form.UpdateSelectedUser(loadedPlayer);
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
                Task.Run(() => cli.LoadCharacterEntries(playerload));
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!IsBusy)
            {
                IsBusy = true;
                label1.Text = "Loading player information";
                label2.Text = "Loading detailed player information";
                parent_form.HideCharacterSubmenu();
                treeView1.Nodes.Clear();
                Guardian playerload = (Guardian)listView1.SelectedItems[0].Tag;
                Task.Run(() => cli.LoadCharacterEntries(playerload));
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
