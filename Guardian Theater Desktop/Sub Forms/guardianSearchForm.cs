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
            cli._BungieApiKey = "9efe9b8eba3042afb081121d447fd981";
            cli._TheaterClientEvent += Cli__TheaterClientEvent;
        }
        TheaterClient cli;
        private void button1_Click(object sender, EventArgs e)
        {
            parent_form.UpdateSelectedUser(null);
            Task.Run(() => cli.SearchBungieAccounts(textBox1.Text, Guardian.BungieAccount.AccountType.Ignore));
        }

        private void Cli__TheaterClientEvent(object sender, TheaterClient.ClientEventType e)
        {
            switch (e)
            {
                case TheaterClient.ClientEventType.SearchComplete:
                    ShowAccounts((List<Guardian>)sender);
                    break;
                case TheaterClient.ClientEventType.CharactersComplete:
                    ShowDetailedAccount((Guardian)sender);
                    break;
                default:
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
            if(InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    ShowAccounts(FoundUsers);
                });
                return;
            }
            foreach(Guardian g in FoundUsers)
            {
                ListViewItem gItem = new ListViewItem();
                gItem.Text = g.MainDisplayName;
                gItem.Tag = g;
                gItem.SubItems.Add(g.MainType.ToString());
                listView1.Items.Add(gItem);
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            label1.Text = "Loading player information";
            parent_form.HideCharacterSubmenu();
            treeView1.Nodes.Clear();
            Guardian playerload = (Guardian)listView1.SelectedItems[0].Tag;
            Task.Run(() => cli.LoadCharacterEntries(playerload));
        }
    }
}
