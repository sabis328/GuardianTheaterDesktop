using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;

namespace Guardian_Theater_Desktop
{
    public partial class carnageReportsForm : Form
    {

        //NZXT Purple Color.FromArgb(81, 0, 122);
        private Guardian CurrentGuardian;
        private Guardian.CharacterEntry CurrentCharacter;

        private TheaterClient CarnageClient;
        private MainForm parentForm;

        private bool IsBusy = false;
        public carnageReportsForm(MainForm parent)
        {
            InitializeComponent();
            parentForm = parent;
            CarnageClient = new TheaterClient();
            CarnageClient.CancelAction = false;
            CarnageClient._BungieApiKey = parent.BungieKey;
            CarnageClient._TwitchApiKey = parent.TwitchKey;
            CarnageClient._TwitchApiSecret = parent.TwitchAuth;
            CarnageClient._TheaterClientEvent += CarnageClient__TheaterClientEvent;
        }

        public void Updatepaint()
        {
            panel1.BackColor = Properties.Settings.Default.HeaderFooterColor;
        }
        public void SetCharacter(Guardian user, int charnum = 0)
        {
            if(user.CharacterEntries[charnum] == CurrentCharacter)
            {
                return;
            }
            if (!IsBusy)
            {
                CarnageClient.CancelAction = false;
                parentForm.SetSelectedCharacter(charnum);
                treeviewCarnageList.Nodes.Clear();
                treeviewStreamList.Nodes.Clear();
                CarnageClient._BungieApiKey = parentForm.BungieKey;
                CarnageClient._TwitchApiKey = parentForm.TwitchKey;
                CarnageClient._TwitchApiSecret = parentForm.TwitchAuth;

                CarnageHeader.Text = "Carnage Reports for " + user.MainDisplayName + " | " + user.CharacterEntries[charnum].CharacterClass.ToString();
                CurrentGuardian = user;
                CurrentCharacter = CurrentGuardian.CharacterEntries[charnum];
                CarnageClient.PlayerExclusion = user.MainDisplayName;
                CarnageClient.ReportsToLoad = parentForm.ReportCount;
                progressBar1.Value = 0;
                SetStatusMessage("Loading carnage reports", 0, CarnageClient.ReportsToLoad);

                if (Properties.Settings.Default.SaveLastSearch)
                {
                    System.Diagnostics.Debug.Print("Setting character to " + user.CharacterEntries[charnum].CharacterIdentifier);
                    Properties.Settings.Default.MyAccountLastCharacterIdentifier = user.CharacterEntries[charnum].CharacterIdentifier;
                    Properties.Settings.Default.Save();
                }
                Task.Run(() => CarnageClient.LoadCarnageReportList(CurrentCharacter, CurrentGuardian, CarnageClient.ReportsToLoad));
                IsBusy = true;
            }
        }

        public void SetFromSavedCharacter(Guardian user, string CharID)
        {

            if (CharID == "null")
            {
                return;
            }
            if (!IsBusy)
            {
                IsBusy = true;
                CarnageClient.CancelAction = false;
                int i = 0;
                foreach (Guardian.CharacterEntry c in user.CharacterEntries)
                {
                    if (c.CharacterIdentifier == CharID)
                    {
                        break;
                    }
                    i += 1;
                }

                if(i == 3)
                {

                    i = 0;
                }
                System.Diagnostics.Debug.Print(CharID + " = " + i.ToString() + " numbered character");
                parentForm.SetSelectedCharacter(i);
                treeviewCarnageList.Nodes.Clear();
                treeviewStreamList.Nodes.Clear();

                CarnageClient._BungieApiKey = parentForm.BungieKey;
                CarnageClient._TwitchApiKey = parentForm.TwitchKey;
                CarnageClient._TwitchApiSecret = parentForm.TwitchAuth;

                CarnageHeader.Text = "Carnage Reports for " + user.MainDisplayName + " | " + user.CharacterEntries[i].CharacterClass.ToString();
                CurrentGuardian = user;
                CurrentCharacter = CurrentGuardian.CharacterEntries[i];
                CarnageClient.PlayerExclusion = user.MainDisplayName;
                CarnageClient.ReportsToLoad = parentForm.ReportCount;
                progressBar1.Value = 0;
                SetStatusMessage("Loading carnage reports", 0, CarnageClient.ReportsToLoad);

                Task.Run(() => CarnageClient.LoadCarnageReportList(CurrentCharacter, CurrentGuardian, CarnageClient.ReportsToLoad));
                
            }
        
        }
        private void CarnageClient__TheaterClientEvent(object sender, TheaterClient.ClientEventType e)
        {
            switch(e)
            {
                case TheaterClient.ClientEventType.SingleCarnageComplete:
                    SetStatusMessage("Matches loaded " + CarnageClient.ReportsLoaded + "/" + CarnageClient.ReportsToLoad, 1, 0);
                    break;
                case TheaterClient.ClientEventType.SingleCarnageFail:
                    //Match Failed to load, but increment bar
                    SetStatusMessage("Matches loaded " + CarnageClient.ReportsLoaded + "/" + CarnageClient.ReportsToLoad, 1, 0);
                    break;
                case TheaterClient.ClientEventType.AllCarnageComplete:
                    System.Diagnostics.Debug.Print("All carnage complete");
                    IsBusy = false;
                    DisplayMatchesAndPlayers();
                    break;
                case TheaterClient.ClientEventType.AllCarnageFail:
                    IsBusy = false;
                    break;
                case TheaterClient.ClientEventType.CheckLinkedAccountsComplete:
                    playersChecked += 1;
                    SetStatusMessage("Checking players for linked twitch accounts " + playersChecked + "/" + CarnageClient.RecentPlayers.Count, 1, 0);
                    RecentPlayersQueue.Add((Guardian)sender);
                    if(RecentPlayersQueue[RecentPlayersQueue.Count -1].HasTwitch)
                    { TwitchLinkedGuardians.Add((Guardian)sender); }
                    if(playersChecked == playersToCheck)
                    {
                        System.Diagnostics.Debug.Print("All players loaded" + "\n_________________________________________________________________");
                        ProcessRecentPlayerList();
                    }
                    break;
                case TheaterClient.ClientEventType.CheckLinkedAccountsFail:
                    playersChecked +=1;
                    SetStatusMessage("Checking players for linked twitch accounts " + playersChecked + "/" + CarnageClient.RecentPlayers.Count, 1, 0);
                    if (playersChecked == playersToCheck)
                    {
                        System.Diagnostics.Debug.Print("All players loaded" + "\n_________________________________________________________________");
                        ProcessRecentPlayerList();
                    }
                    break;
                case TheaterClient.ClientEventType.CancelAll:

                    threadsClosed += 1;
                    SettoIdle();
                    break;
            }
        }

        //Loads all matches and players into the trees so that the user sees some progress
        private void DisplayMatchesAndPlayers()
        {
            if(InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                { DisplayMatchesAndPlayers(); });
                return;
            }
            StreamHeader.Text = "Recent players";

            foreach (CarnageReport PGCR in CarnageClient.RecentMatches)
                {
                    TreeNode MatchNode = new TreeNode(PGCR.ActivityTypeID + " | " + PGCR.ActivitySpaceID);
                    MatchNode.Nodes.Add(PGCR.ActivityStart.ToString());
                    MatchNode.Nodes.Add("Game hash :" + PGCR.ActivityHash);
                    MatchNode.Nodes.Add("Location hash :" + PGCR.LocationHash);
                    MatchNode.Tag = PGCR;

                    foreach (Guardian player in PGCR.ActivityPlayers)
                    {
                        TreeNode PlayerNode = new TreeNode(player.MainDisplayName);
                        foreach (Guardian.Weapon wep in player.UsedWeapons)
                        {
                            TreeNode wepNode = new TreeNode(wep.WeaponIdentifier);
                            wepNode.Nodes.Add("Kills : " + wep.WeaponKills);
                            wepNode.Nodes.Add("Precision Ratio : " + wep.WeaponPrecisionRatio);

                            if (wep.Suspected)
                            {
                                wepNode.BackColor = Color.Red;
                                PlayerNode.BackColor = Color.Red;
                                MatchNode.BackColor = Color.Red;
                            }
                            PlayerNode.Nodes.Add(wepNode);
                        }
                        MatchNode.Nodes.Add(PlayerNode);

                    }
                    treeviewCarnageList.Nodes.Add(MatchNode);
                }

                foreach (Guardian sortedPlayer in CarnageClient.RecentPlayers)
                {
                    treeviewStreamList.Nodes.Add(sortedPlayer.MainDisplayName);
                }
                StatusLabel.Text = "Recent players loaded, checking for twitch accounts";

                Task.Run(() => QueueRecentPlayerList());
            
           
        }

        private int playersChecked = 0;
        private int playersToCheck = 0;
        private List<Guardian> RecentPlayersQueue;
        private List<Guardian> TwitchLinkedGuardians;

        //Starts Checking all indepth data for recent players

        List<TheaterClient> OpenClients = null;
        private void QueueRecentPlayerList()
        {
            if(InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
               { QueueRecentPlayerList(); });
                return;
            }
            if (CarnageClient.RecentPlayers.Count > 0)
            {
                OpenClients = new List<TheaterClient>();
                IsBusy = true;
                RecentPlayersQueue = new List<Guardian>();
                TwitchLinkedGuardians = new List<Guardian>();
                playersChecked = 0;
                playersToCheck = CarnageClient.RecentPlayers.Count;

                SetStatusMessage("Checking recent players for linked twitch accounts", 0, playersToCheck);
                foreach (Guardian player in CarnageClient.RecentPlayers)
                {
                    TheaterClient subClinet = new TheaterClient();
                    subClinet.CancelAction = false;
                    subClinet._BungieApiKey = CarnageClient._BungieApiKey;
                    subClinet._TheaterClientEvent += CarnageClient__TheaterClientEvent;

                    OpenClients.Add(subClinet);
                    Task.Run(() => subClinet.LoadLinkedAccounts(player, false));
                }
            }
            else
            {
                StreamHeader.Text = "No streams found";
                SetStatusMessage();
            }
        }


        public bool canCheck = false;
        //Sorts out hard linked twitch accounts, and goes back through matches to update player data
        private void ProcessRecentPlayerList()
        {
            if (InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                { ProcessRecentPlayerList(); });
                return;
            }
            treeviewStreamList.Nodes.Clear();

            foreach(Guardian player in RecentPlayersQueue)
            {
               
                    foreach (Guardian.BungieAccount bacc in player.LinkedAccounts)
                    {

                        if (bacc.UserType == Guardian.BungieAccount.AccountType.Steam)
                        {
                            if (bacc.DisplayName.ToLower().Trim().Contains("twtich.tv/") || bacc.DisplayName.ToLower().Trim().Contains("twitch/") || bacc.DisplayName.ToLower().Trim().Contains("ttv") || bacc.DisplayName.ToLower().Trim().Contains("ttv/") || bacc.DisplayName.ToLower().Trim().Contains("ttvbtw")
                               || bacc.DisplayName.ToLower().Trim().Contains("twitch-") || bacc.DisplayName.ToLower().Trim().Contains("ttv.") || bacc.DisplayName.ToLower().Trim().Contains("t.tv") || bacc.DisplayName.ToLower().Trim().Contains("live") || bacc.DisplayName.ToLower().Trim().Contains("twitch_") || bacc.DisplayName.ToLower().Trim().Contains("twitch"))
                            {
                                System.Diagnostics.Debug.Print("Matched a pattern name : " + bacc.DisplayName);

                                string tempName = bacc.DisplayName.ToLower();
                                tempName = tempName.Replace("twitch.tv/", " ");
                                tempName = tempName.Replace("twitch/", " ");
                                tempName = tempName.Replace("ttv", " ");
                                tempName = tempName.Replace("ttvbtw", " ");
                                tempName = tempName.Replace("t.tv", " ");
                                tempName = tempName.Replace("live", " ");
                                tempName = tempName.Replace("twitch_", " ");
                                tempName = tempName.Replace("twitch", " ");
                                tempName = tempName.Replace("ttv/", " ");
                                tempName = tempName.Replace("ttv.", " ");
                                tempName = tempName.Replace("(btw)", " ");


                                tempName = tempName.Trim();
                                tempName = Regex.Replace(tempName, "[^a-zA-Z0-9 _]", string.Empty);

                                player.AlternateTwitch = tempName;

                            if (player.HasTwitch)
                            {

                                System.Diagnostics.Debug.Print("Comparing current linked twitch to steam twitch \n " + player.TwitchName + " : " + player.AlternateTwitch);
                                if (!player.TwitchName.ToLower().Contains(player.AlternateTwitch.ToLower()))
                                {
                                    System.Diagnostics.Debug.Print("Possible alternate twitch found");

                                    Guardian playerAlt = new Guardian();
                                    playerAlt.MainDisplayName = player.MainDisplayName;
                                    playerAlt.LinkedMatchTimes = player.LinkedMatchTimes;
                                    playerAlt.LinkedMatches = player.LinkedMatches;

                                    playerAlt.TwitchName = tempName;
                                    playerAlt.HasTwitch = true;
                                    TwitchLinkedGuardians.Add(playerAlt);
                                }

                            }
                            else
                            {
                                player.TwitchName = player.AlternateTwitch;
                                System.Diagnostics.Debug.Print("Corrected name to : " + player.TwitchName);
                                TwitchLinkedGuardians.Add(player);
                            }
                                break;
                            }
                        }


                    }
               
            }

            //Awful nested loops to re-add all the player data to existing player nodes
            foreach(TreeNode MatchNode in treeviewCarnageList.Nodes)
            {
                foreach(TreeNode playerNode in MatchNode.Nodes)
                {
                    foreach(Guardian detailedPlayer in RecentPlayersQueue)
                    {
                        if(playerNode.Text == detailedPlayer.MainDisplayName)
                        {


                            foreach(Guardian.BungieAccount bacc in detailedPlayer.LinkedAccounts)
                            {
                                playerNode.Nodes.Add(bacc.DisplayName + " | " + bacc.UserType);
                                
                            }
                            if(detailedPlayer.HasTwitch)
                            {
                                playerNode.BackColor = Color.FromArgb(81, 0, 122);
                                playerNode.Nodes.Add("twitch.tv/" + detailedPlayer.TwitchName);
                            }

                            
                            break;
                        }
                    }
                }
            }

            foreach(Guardian hasTwitch in TwitchLinkedGuardians)
            {
                TreeNode UpdatedUserNode = new TreeNode(hasTwitch.MainDisplayName);
                UpdatedUserNode.Tag = hasTwitch;
                foreach(DateTime matchTime in hasTwitch.LinkedMatchTimes)
                {
                    UpdatedUserNode.Nodes.Add(matchTime.ToString());
                }
                treeviewStreamList.Nodes.Add(UpdatedUserNode);
            }
            IsBusy = false;
            canCheck = true;
            
            if (TwitchLinkedGuardians.Count > 0)
            {
                StreamHeader.Text = "Checking users for vods";
                Task.Run(() => CheckTwitchVods());
            }
            else
            {
                StreamHeader.Text = "No possible streamers found";
            }
        }

       
        private void CheckTwitchVods()
        {
            if(!canCheck)
            {
                return;
            }
            if(treeviewStreamList.Nodes.Count > 0)
            {
                canCheck = false;
                IsBusy = true;

                List<TreeNode> ResetStreamNodes = new List<TreeNode>();
               
                SetStatusMessage("Checking users for vods", 0, treeviewStreamList.Nodes.Count);

                Twitch_Client vodClient = new Twitch_Client();
                vodClient.Twitch_ClientID = parentForm.TwitchKey;
                vodClient.Twitch_Client_Secret = parentForm.TwitchAuth;
                vodClient.Validate_Twitch_Client();

                if(vodClient.Is_Validated == Twitch_Client.Twitch_Validation_Status.Success)
                {
                    try
                    {
                        foreach (Guardian linkedGuardian in TwitchLinkedGuardians)
                        {
                            vodClient.Twitch_Find_Channels(linkedGuardian.TwitchName, true);

                            if (vodClient.Found_Channels.Count > 0)
                            {

                                TwitchCreator possibleStreaamer = vodClient.Found_Channels[0];
                                vodClient.Load_Channel_Videos(possibleStreaamer);

                                linkedGuardian.liveNow = possibleStreaamer.Live_Now;

                                System.Diagnostics.Debug.Print("user " + possibleStreaamer.Username + " is live : " + linkedGuardian.liveNow.ToString());

                                if (possibleStreaamer.Channel_Saved_Videos != null && possibleStreaamer.Channel_Saved_Videos.Count > 0)
                                {

                                    TreeNode MatchedStreamNode = new TreeNode(linkedGuardian.MainDisplayName + " | twitch/" + linkedGuardian.TwitchName);
                                    MatchedStreamNode.Tag = linkedGuardian;

                                    foreach (TwitchVideo vod in possibleStreaamer.Channel_Saved_Videos)
                                    {
                                        int i = 0;

                                        while (i < linkedGuardian.LinkedMatchTimes.Count)
                                        {
                                            DateTime AccountforDuration = vod.videoCreated;
                                            AccountforDuration += vod.videoDuration;
                                            DateTime CheckAgainst = linkedGuardian.LinkedMatchTimes[i];

                                            if (vod.videoCreated.Date == CheckAgainst.Date || CheckAgainst.Ticks < AccountforDuration.Ticks)
                                            {
                                                if (CheckAgainst.Ticks > vod.videoCreated.Ticks && CheckAgainst.Ticks < AccountforDuration.Ticks)
                                                {
                                                    System.Diagnostics.Debug.Print("MATCH FOUND  " + linkedGuardian.MainDisplayName + "    " + vod.videoCreated.ToString());
                                                  

                                                    TimeSpan offset = CheckAgainst.TimeOfDay - vod.videoCreated.TimeOfDay;

                                                    System.Diagnostics.Debug.Print("offseting : " + offset.ToString());

                                                    if (offset.Hours < 0)
                                                    {
                                                        offset = offset.Add(new TimeSpan(24, 0, 0));
                                                        System.Diagnostics.Debug.Print("Corrected Negative offset : " + offset.ToString());
                                                    }


                                                    string twitchLink = "http://www.twitch.tv"; 

                                                    if (vod.videoDuration.ToString().Contains("."))
                                                    {
                                                        int Voddays = 0;
                                                        
                                                        string[] daySplit = vod.videoDuration.ToString().Split('.');
                                                        
                                                        Voddays = Convert.ToInt32(daySplit[0].ToString());
                                                        
                                                        Voddays *= 24;

                                                        Voddays += offset.Hours;

                                                        twitchLink = vod.videoLink + "?t=" + Voddays.ToString() + "h" + offset.Minutes + "m" + offset.Seconds + "s";

                                                    }
                                                    else
                                                    {
                                                       twitchLink = vod.videoLink + "?t=" + offset.Hours + "h" + offset.Minutes + "m" + offset.Seconds + "s";
                                                    }

                                                   
                                                    

                                                    System.Diagnostics.Debug.Print("ADDING MATCH NODE : for " + linkedGuardian.MainDisplayName + " | linked matches found : " + linkedGuardian.LinkedMatches.Count.ToString() + " on match : " + i.ToString());
                                                    TreeNode twitchNode = new TreeNode(twitchLink);

                                                    twitchNode.Nodes.Add(linkedGuardian.LinkedMatchTimes[i].ToString());
                                                    twitchNode.Nodes.Add(linkedGuardian.LinkedMatches[i].ActivityTypeID + " | " + linkedGuardian.LinkedMatches[i].ActivitySpaceID);
                                                    twitchNode.Tag = twitchLink;
                                                    twitchNode.Nodes[0].Tag = twitchLink;
                                                    twitchNode.Nodes[1].Tag = twitchLink;
                                                    MatchedStreamNode.Nodes.Add(twitchNode);

                                                }

                                            }
                                            i += 1;
                                        }
                                    }

                                    if (MatchedStreamNode.Nodes.Count > 0)
                                    {
                                        ResetStreamNodes.Add(MatchedStreamNode);
                                    }

                                }


                            }


                            SetStatusMessage("Processing linked twitch accounts for vods | " + (progressBar1.Value + 1).ToString() + "/" + progressBar1.Maximum.ToString(), 1, 0);
                        }

                        ResetStreamTree(ResetStreamNodes);
                    }
                    catch
                    { SetStatusMessage("Error occured loading vods, try refreshing matches"); }

                    IsBusy = false;
                }
                else
                {
                    SetStatusMessage("Failed to validate twitch client", 0, 0);
                    IsBusy = false;
                }
            }
            IsBusy = false;
        }

        private void ResetStreamTree(List<TreeNode> streamers)
        {
            if(InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                { ResetStreamTree(streamers); });
                return;
            }
            List<DateTime> MatchedTimes = new List<DateTime>();
            treeviewStreamList.Nodes.Clear();
            foreach(TreeNode streamerNode in streamers)
            {
                //treeviewStreamList.Nodes.Add(streamerNode);
                //Sort by date and time here and sub match date and time here
                //lRecentMatches.Sort((x, y) => y.ActivityStart.CompareTo(x.ActivityStart));
                //Create a list of dates for each streamer, sort the sub dates, then sort the main streamers by sub date[0]
                if (streamerNode.Nodes.Count > 1)
                {
                   
                    List<TreeNode> SubMatches = new List<TreeNode>();
                    List<DateTime> SubMatchTimes = new List<DateTime>();
                    foreach (TreeNode linkNode in streamerNode.Nodes)
                    {
                        SubMatchTimes.Add(Convert.ToDateTime(linkNode.Nodes[0].Text));
                        
                    }

                    SubMatchTimes.Sort((x, y) => y.CompareTo(x));
                    foreach (DateTime matchCompare in SubMatchTimes)
                    {
                        foreach (TreeNode readd in streamerNode.Nodes)
                        {
                            if (readd.Nodes[0].Text == matchCompare.ToString())
                            {
                                SubMatches.Add(readd);
                            }
                        }
                    }

                    streamerNode.Nodes.Clear();
                    foreach(TreeNode addTo in SubMatches)
                    {
                        streamerNode.Nodes.Add(addTo);
                    }

                    MatchedTimes.Add(Convert.ToDateTime(streamerNode.Nodes[0].Nodes[0].Text));
                }
                else
                {
                    MatchedTimes.Add(Convert.ToDateTime(streamerNode.Nodes[0].Nodes[0].Text));
                }

            }
            MatchedTimes.Sort((x, y) => y.CompareTo(x));
            //Reorder all streamerNodes

            List<TreeNode> reAddStreams = new List<TreeNode>();
            foreach (DateTime compareTo in MatchedTimes)
            {
                foreach (TreeNode streamerNode in streamers)
                {
                    //System.Diagnostics.Debug.Print("Comparing time " + streamerNode.Nodes[0].Nodes[0])
                    if (streamerNode.Nodes[0].Nodes[0].Text == compareTo.ToString() && !reAddStreams.Contains(streamerNode))
                    {
                        reAddStreams.Add(streamerNode);
                    }
                }
            }

            foreach(TreeNode stream in reAddStreams)
            { treeviewStreamList.Nodes.Add(stream);  }

            StreamHeader.Text = "Streams Found : " + treeviewStreamList.Nodes.Count;


            //Additional Loop here to run through and show if somebody is pressently live, indicating that somebody might have streamed this match
            List<Guardian> CurrentLive = new List<Guardian>();
            foreach (Guardian g in TwitchLinkedGuardians)
            {
                if(g.liveNow)
                {
                    CurrentLive.Add(g);
                    
                }
            }

            foreach(Guardian g in CurrentLive)
            {
                System.Diagnostics.Debug.Print("Live user ; " + g.MainDisplayName);

            }
            foreach(TreeNode matchNode in treeviewCarnageList.Nodes)
            {
                System.Diagnostics.Debug.Print("Checking match : " + matchNode.Text);
                //CarnageReport pgcr = (CarnageReport)matchNode.Tag;
                bool foundLive = false;
                foreach(TreeNode matchSubNodes in matchNode.Nodes)
                {
                    foreach(Guardian g in CurrentLive)
                    {
                        if (matchSubNodes.Text.ToLower() == g.MainDisplayName.ToLower())
                        {
                            //System.Diagnostics.Debug.Print(matchNode.Text + " Contained user : " + g.MainDisplayName);
                            foundLive = true;
                            //System.Diagnostics.Debug.Print("updating node colors");
                            matchSubNodes.BackColor = Color.LimeGreen;
                            matchSubNodes.ForeColor = Color.Black;
                                
                        }
                        
                    }
                    
                    
                }
                if(foundLive)
                {
                    matchNode.Text += " : a user from this match is currently live";
                    //matchNode.BackColor = Color.Yellow;
                }
            }

            treeviewCarnageList.Update();
             
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!IsBusy)
            {
                treeviewCarnageList.Nodes.Clear();
                treeviewStreamList.Nodes.Clear();
                CarnageClient.CancelAction = false;
                CarnageClient.ReportsToLoad = parentForm.ReportCount;
                progressBar1.Value = 0;
                SetStatusMessage("Loading carnage reports", 0, CarnageClient.ReportsToLoad);

                Task.Run(() => CarnageClient.LoadCarnageReportList(CurrentCharacter, CurrentGuardian, CarnageClient.ReportsToLoad));
                IsBusy = true;
            }
        }

        private void SetStatusMessage(string status = "Idle",int add = 0,int setmax = 0)
        {

            if(InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                { SetStatusMessage(status, add, setmax); });
                return;
            }
            StatusLabel.Text = status;
            StatusLabel.Update();

            if (setmax > 0)
            {
                progressBar1.Maximum = setmax;
            }

            if (progressBar1.Value + add <= progressBar1.Maximum)
            {
                progressBar1.Value += add;
            }
            else
            {
                progressBar1.Value = progressBar1.Maximum;
            }
            
            if (progressBar1.Value == progressBar1.Maximum)
            {
                StatusLabel.Text = "Idle";
                progressBar1.Maximum = 0;
                progressBar1.Value = 0;
            }
            progressBar1.Update();
        }

        private void SettoIdle()
        {
            if (InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                { SettoIdle(); });
                return;
            }

            try
            {
                treeviewStreamList.Nodes.Clear();
                treeviewCarnageList.Nodes.Clear();
                if (OpenClients != null)
                {
                    progressBar1.Value = threadsClosed;
                    progressBar1.Maximum = OpenClients.Count;

                    //System.Threading.Thread.Sleep(5000);
                    StatusLabel.Text = "Attempting to close threads " + threadsClosed + "/" + OpenClients.Count;
                    StatusLabel.Update();
                }

                else { StatusLabel.Text = "Canceling queued match"; }
            }
            catch
            {

            }
        }

        private void treeviewStreamList_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag != null && e.Node.Tag.GetType() == typeof(string))
            {
                Clipboard.SetText((string)e.Node.Tag);
                e.Node.Expand();
            }
        }


        public int threadsClosed = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            threadsClosed = 0;
            CarnageClient.CancelAction = true;
            


            if (OpenClients != null)
            {
                SetStatusMessage("Attempting to close threads 0/" + OpenClients.Count);
                progressBar1.Value = threadsClosed;
                progressBar1.Maximum = OpenClients.Count;
                foreach (TheaterClient subclient in OpenClients)
                {
                    subclient.CancelAction = true;
                }
            }
            IsBusy = false;

            
        }

        private void buttonExportStreams_Click(object sender, EventArgs e)
        {
            if (treeviewStreamList.Nodes.Count > 0)
            {
                string StreamList = "";
                foreach (TreeNode streamHeader in treeviewStreamList.Nodes)
                {
                    StreamList += streamHeader.Text + "\n\n";
                    foreach (TreeNode streamText in streamHeader.Nodes)
                    {
                        StreamList += streamText.Text + "\n" + streamText.Nodes[0].Text + "\n" + streamText.Nodes[1].Text + "\n\n";
                    }
                }

                string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string ticks = Environment.TickCount.ToString();
                path += "\\" + ticks + ".txt";

                StreamWriter sw = new StreamWriter(path);
                sw.WriteLine(StreamList);
                sw.Close();

                System.Diagnostics.Process.Start(path);
            }
        }
    }
}
