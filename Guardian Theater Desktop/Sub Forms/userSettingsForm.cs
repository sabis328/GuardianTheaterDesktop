using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Guardian_Theater_Desktop
{
    public partial class userSettingsForm : Form
    {

        //9efe9b8eba3042afb081121d447fd981 bkey
        //h7bled1w6wracl3bytlhqwra3d7pr8    tsec
        //abvhdv9zyqhefmnbjz3fljxx3hpc7u    tcli

        MainForm _parentForm;
        public userSettingsForm(MainForm parent)
        {
            InitializeComponent();
            checkBox1.Checked = Properties.Settings.Default.SaveLastSearch;
            _parentForm = parent;
            panel1.BackColor = Properties.Settings.Default.MenuIndicatorColor;
            panel2.BackColor = Properties.Settings.Default.SelectedCharacterColor;
            panel3.BackColor = Properties.Settings.Default.HeaderFooterColor;

            bungiekeybox.Text = Properties.Settings.Default.BungieKey;
            twitchsecretkeybox.Text = Properties.Settings.Default.TwitchSecret;
            twitchclientkeybox.Text = Properties.Settings.Default.TwitchKey;
            parent.BungieKey = bungiekeybox.Text;
            parent.TwitchKey = twitchclientkeybox.Text;
            parent.TwitchAuth = twitchsecretkeybox.Text;


            CheckCustomKeys(parent);
        }


        public void CheckCustomKeys(MainForm parent)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path += "\\" + "GTKeys.txt";

            if (File.Exists(path))
            {
                StreamReader sr = new StreamReader(path);
                //Twitch Key | Twitch Secret | Bungie Key

                string TwitchKey = sr.ReadLine();
                string TwitchSecret = sr.ReadLine();
                string BungieKey = sr.ReadLine();

                Properties.Settings.Default.BungieKey = BungieKey;
                Properties.Settings.Default.TwitchSecret = TwitchSecret;
                Properties.Settings.Default.TwitchKey = TwitchKey;
                Properties.Settings.Default.Save();

                bungiekeybox.Text = BungieKey;
                twitchsecretkeybox.Text = TwitchSecret;
                twitchclientkeybox.Text = TwitchKey;

                parent.BungieKey = bungiekeybox.Text;
                parent.TwitchKey = twitchclientkeybox.Text;
                parent.TwitchAuth = twitchsecretkeybox.Text;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.TwitchKey = twitchclientkeybox.Text;
            Properties.Settings.Default.TwitchSecret = twitchsecretkeybox.Text;
            Properties.Settings.Default.BungieKey = bungiekeybox.Text;
            Properties.Settings.Default.Save();

            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path += "\\" +  "GTKeys.txt";
            bool canWrite = true;
            if (File.Exists(path))
            {
                try
                {
                    File.WriteAllText(path,"");
                    
                }
                catch { canWrite = false; }
            }

            if (canWrite)
            {
                StreamWriter sw = new StreamWriter(path);
                sw.WriteLine(Properties.Settings.Default.TwitchKey);
                sw.WriteLine(Properties.Settings.Default.TwitchSecret);
                sw.WriteLine(Properties.Settings.Default.BungieKey);
                sw.Close();

                System.Diagnostics.Process.Start(path);
            }
        }

        
        //Character Selected
        private void panel2_Click(object sender, EventArgs e)
        {
            DialogResult ColorChanged = colorDialog1.ShowDialog();
            if(ColorChanged != DialogResult.Cancel)
            {
                panel2.BackColor = colorDialog1.Color;
                Properties.Settings.Default.SelectedCharacterColor = panel2.BackColor;
                _parentForm.UpdateFormPaints();
            }
            
        }

        //Header/Footer
        private void panel3_Click(object sender, EventArgs e)
        {
            DialogResult ColorChanged = colorDialog1.ShowDialog();
            if (ColorChanged != DialogResult.Cancel)
            {
                panel3.BackColor = colorDialog1.Color;
                Properties.Settings.Default.HeaderFooterColor = panel3.BackColor;
                _parentForm.UpdateFormPaints();
            }
        }

        //Menu selected
        private void panel1_Click(object sender, EventArgs e)
        {
            DialogResult ColorChanged = colorDialog1.ShowDialog();
            if (ColorChanged != DialogResult.Cancel)
            {
                panel1.BackColor = colorDialog1.Color;
                Properties.Settings.Default.MenuIndicatorColor = panel1.BackColor;
                _parentForm.UpdateFormPaints();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.MenuIndicatorColor = panel1.BackColor;
            Properties.Settings.Default.SelectedCharacterColor = panel2.BackColor;
            Properties.Settings.Default.HeaderFooterColor = panel3.BackColor;
            Properties.Settings.Default.Save();
            _parentForm.UpdateFormPaints();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        { 
           
            Properties.Settings.Default.SaveLastSearch = checkBox1.Checked;
            Properties.Settings.Default.Save();

            if(!checkBox1.Checked)
            {
                Properties.Settings.Default.MyAccountLastCharacterIdentifier = "null";
                Properties.Settings.Default.MyAccountMainID = "null";
                Properties.Settings.Default.MyAccountDisplayName = "null";
                Properties.Settings.Default.MyAccountMainType = "null";
            }
        }
    }
}
