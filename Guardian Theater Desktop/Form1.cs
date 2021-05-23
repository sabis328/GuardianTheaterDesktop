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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            apiClient = new BungieCrawler();
            apiClient._APIKEY = "9efe9b8eba3042afb081121d447fd981";
            apiClient.CrawlerEvent += ApiClient_CrawlerEvent;
        }
        public BungieCrawler apiClient;

        private string PlayerExclusion;
        private void ApiClient_CrawlerEvent(object sender, BungieCrawler.CrawlerEventType e)
        {
           switch(e)
            {
                case BungieCrawler.CrawlerEventType.SearchForAccount_Complete:
                    HandleSearchedAccountComplete((List<DestinyPlayer>)sender);
                    break;
                case BungieCrawler.CrawlerEventType.AccountCharactersLoaded_Complete:
                    HandleSeachedAccountProcessed((DestinyPlayer)sender);
                    break;
                case BungieCrawler.CrawlerEventType.CheckLinkedAccounts_Complete:
                    playersProcessed += 1;
                    UpdateStatusBar(1, 0,0, "Checking players for twitch accounts : " + playersProcessed.ToString() + "/" + PossibleStreams.Count.ToString());
                    HandleProcessStreamers((DestinyPlayer)sender);
                    break;
                case BungieCrawler.CrawlerEventType.SingleCarnageLoaded:
                    UpdateStatusBar(1, 0, (int)numericUpDown1.Value, "Loading matches " + (progressBar1.Value + 1).ToString() + "/" + numericUpDown1.Value);
                    AddCarnageReport((ActivityReport)sender);
                    break;
                case BungieCrawler.CrawlerEventType.LoadedCarnageReports_Complete:
                    DisplayMatchPlayers();
                    break;
                case BungieCrawler.CrawlerEventType.AccountCharactersLoaded_Failed:
                    UpdateStatusBar(0, 0, 0, "Failed to load carnage reports");
                    EnableInput();
                    break;
                case BungieCrawler.CrawlerEventType.SearchForAccount_Failed:
                    UpdateStatusBar(0, 0, 0, "No matching accounts found");
                    EnableInput();
                    break;
                default:
                    UpdateStatusBar(1, 0);
                    playersProcessed += 1;
                    break;
            }
        }

        private void CheckForTwitchVods()
        {
            if(treeView3.Nodes.Count > 0)
            {
                List<TreeNode> ResetStreamers = new List<TreeNode>();
                LastKnownReport = "Processing possible linked streams VODs";
                UpdateStatusBar(0, 0, treeView3.Nodes.Count, "Processing linked twitch accounts for possible streams");

                Twitch_Client vodClient = new Twitch_Client();
                vodClient.Validate_Twitch_Client();


                if(vodClient.Is_Validated == Twitch_Client.Twitch_Validation_Status.Success)
                {
                    foreach(TreeNode StreamerNode in treeView3.Nodes)
                    {

                        if (StreamerNode.Tag != null)
                        {
                            DestinyPlayer possibleStreamer = (DestinyPlayer)StreamerNode.Tag;
                            System.Diagnostics.Debug.Print("Checking Streamer : " + possibleStreamer.TwitchName);

                            vodClient.Twitch_Find_Channels(possibleStreamer.TwitchName, true);



                            if (vodClient.Found_Channels.Count > 0)
                            {
                                TwitchCreator Streamer = vodClient.Found_Channels[0];
                                vodClient.Load_Channel_Videos(Streamer);

                                //if (Streamer.Live_Now)
                               // {
                                 //   TreeNode matchedStream = new TreeNode(possibleStreamer.MainDisplayName + " | " + possibleStreamer.TwitchName);
                                //    string twitchLink = "User is live now, check back later for vod timestamp";
                                  //  matchedStream.Tag = possibleStreamer;
                                    //TreeNode twitchNode = new TreeNode("https://twitch.tv/" + Streamer.Username);
                                    //twitchNode.Nodes.Add(twitchLink);
                                    //twitchNode.Tag = twitchNode.Text;
                                    //matchedStream.Nodes.Add(twitchNode);
                                    //ResetStreamers.Add(matchedStream);
                               // }

                                if (Streamer.Channel_Saved_Videos != null)
                                {
                                    if (Streamer.Channel_Saved_Videos.Count > 0)
                                    {

                                        TreeNode matchedStream = new TreeNode(possibleStreamer.MainDisplayName + " | " + possibleStreamer.TwitchName);
                                        matchedStream.Tag = possibleStreamer;

                                        foreach (TwitchVideo vod in Streamer.Channel_Saved_Videos)
                                        {
                                            System.Diagnostics.Debug.Print("Checking " + Streamer.Username + " : vod " + vod.videoTitle);

                                            int i = 0;
                                            while (i < possibleStreamer.LinkedMatchTimes.Count)
                                            {

                                                DateTime AccountforDuration = vod.videoCreated;
                                                AccountforDuration += vod.videoDuration;
                                                DateTime CheckAgainst = possibleStreamer.LinkedMatchTimes[i];

                                                if (vod.videoCreated.Date == CheckAgainst.Date || CheckAgainst.Ticks < AccountforDuration.Ticks)
                                                {
                                                    if (CheckAgainst.Ticks > vod.videoCreated.Ticks && CheckAgainst.Ticks < AccountforDuration.Ticks)
                                                    {
                                                        System.Diagnostics.Debug.Print("MATCH FOUND  " + possibleStreamer.MainDisplayName + "    " + vod.videoCreated.ToString());
                                                        TimeSpan offset = CheckAgainst.TimeOfDay - vod.videoCreated.TimeOfDay;

                                                        System.Diagnostics.Debug.Print("offseting : " + offset.ToString());

                                                        if (offset.Hours < 0)
                                                        {
                                                            offset = offset.Add(new TimeSpan(24, 0, 0));
                                                            System.Diagnostics.Debug.Print("Corrected Negative offset : " + offset.ToString());
                                                        }


                                                        

                                                        string twitchLink = vod.videoLink + "?t=" + offset.Hours + "h" + offset.Minutes + "m" + offset.Seconds + "s";
                                                       


                                                        System.Diagnostics.Debug.Print("ADDING MATCH NODE : for " + possibleStreamer.MainDisplayName + " | linked matches found : " + possibleStreamer.LinkedMatches.Count.ToString() + " on match : " + i.ToString());
                                                        TreeNode twitchNode = new TreeNode(twitchLink);

                                                        twitchNode.Nodes.Add(possibleStreamer.LinkedMatchTimes[i].ToString());
                                                        twitchNode.Nodes.Add(possibleStreamer.LinkedMatches[i].ActivityTypeID + " | " + possibleStreamer.LinkedMatches[i].ActivitySpaceID);
                                                        twitchNode.Tag = twitchLink;
                                                        twitchNode.Nodes[0].Tag = twitchLink;
                                                        twitchNode.Nodes[1].Tag = twitchLink;
                                                        matchedStream.Nodes.Add(twitchNode);
                                                       
                                                    }

                                                }
                                                i += 1;
                                            }
                                            
                                        }

                                        if (matchedStream.Nodes.Count > 0)
                                        {
                                            System.Diagnostics.Debug.Print("added node : " + matchedStream.Text + " sub nodes : " + matchedStream.Nodes.Count.ToString());
                                            ResetStreamers.Add(matchedStream);
                                        }

                                    }
                                }
                            }
                        }
                        UpdateStatusBar(1, 0, 0, "Processing linked twitch accounts for possible streams | " + (progressBar1.Value + 1).ToString() + " / " + progressBar1.Maximum.ToString());
                    }
                    ResetStreamerTree(ResetStreamers);

                }
                else
                {
                    UpdateStatusBar(0, 0, 0, "Twitch client validation failed");
                }
            }

            EnableInput();
        }

        private void ResetStreamerTree(List<TreeNode> Streamers)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    ResetStreamerTree(Streamers);
                });
                return;
            }

            treeView3.Nodes.Clear();
            foreach (TreeNode steamNode in Streamers)
            {
                treeView3.Nodes.Add(steamNode);
            }
            LastKnownReport = "Streams Found";
            UpdateStatusBar(0, 0, 0, "Idle");
        }


        private void DisableInput()
        {
            if (InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                { DisableInput(); });
                return;
            }

            button1.Enabled = false;
            button2.Enabled = false;
           
            textBox1.Enabled = false;

        }

        private void EnableInput()
        {
            if (InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                { EnableInput(); });
                return;
            }

            button1.Enabled = true;
            button2.Enabled = true;
            textBox1.Enabled = true;
        }


        public List<DestinyPlayer> PossibleStreams;
        public List<DestinyPlayer> ConfirmedTwitchLinked;
        public List<DestinyPlayer> RecentPlayersDetailed;

        public List<string> PlayersToProcess;
        //Used for player count processing in the progressbar
        int playersProcessed = 0;

        private string LastKnownReport = "No players loaded";
        //Shows all the players sorted out of the recent games into the last treeview
        private void DisplayMatchPlayers()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                { DisplayMatchPlayers(); });
                return;
            }

            foreach(DestinyPlayer player in PossibleStreams)
            {
                TreeNode StreamerNode = new TreeNode(player.MainDisplayName);
                StreamerNode.Tag = player;

                foreach(DateTime matchtime in player.LinkedMatchTimes)
                {
                    StreamerNode.Nodes.Add(matchtime.ToString());
                }
                treeView3.Nodes.Add(StreamerNode);
            }

            LastKnownReport = "Processing players from carnage reports";
            label3.Text = "Processing players from carnage reports";

           
            Task.Run(() => ProcessMatchPlayers());
        }

        //Loads all the players from recent games into the client and searches for linked twitch accounts
        private void ProcessMatchPlayers()
        {
            LastKnownReport = "Checking recent players for twitch accounts";
            UpdateStatusBar(0, 0, PossibleStreams.Count, "Checking for linked twitch accounts");
            
            playersProcessed = 0;
            ConfirmedTwitchLinked = new List<DestinyPlayer>();
            RecentPlayersDetailed = new List<DestinyPlayer>();
            foreach (DestinyPlayer possible in PossibleStreams)
            {
                BungieCrawler playerCrawler = new BungieCrawler();
                playerCrawler._APIKEY = "9efe9b8eba3042afb081121d447fd981";
                playerCrawler.CrawlerEvent += ApiClient_CrawlerEvent;
               
                Task.Run(() => playerCrawler.LoadPlayerLinkedAccounts(possible, false));
               // System.Threading.Thread.Sleep(500);
            }
        }

        //Sorts and adds all players witha  linked twitch account to the last treeview
        private void HandleProcessStreamers(DestinyPlayer checkPlayer)
        {
            if (playersProcessed == PossibleStreams.Count || playersProcessed > PossibleStreams.Count)
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    { HandleProcessStreamers(checkPlayer); });
                    return;
                }
                treeView3.Nodes.Clear();
                treeView4.Nodes.Clear();

                foreach (DestinyPlayer cplay in RecentPlayersDetailed)
                {
                    System.Diagnostics.Debug.Print("Combing recent detailed for : " + cplay.MainDisplayName);
                    if (!checkPlayer.TwitchLinked)
                    {
                        System.Diagnostics.Debug.Print("Checking player : " + cplay.MainDisplayName + " for twitch pattern");
                        foreach (DestinyPlayer.BungieAccount bacc in cplay.LinkedAccounts)
                        {
                            if (bacc.DisplayName.ToLower().Trim().Contains("ttv") || bacc.DisplayName.ToLower().Trim().Contains("twitch") || bacc.DisplayName.ToLower().Trim().Contains("live")
                                 || bacc.DisplayName.ToLower().Trim().Contains("live") || bacc.DisplayName.ToLower().Trim().Contains("sub2") || bacc.DisplayName.ToLower().Trim().Contains("plays"))
                            {
                                TreeNode AltStreamer = new TreeNode(cplay.MainDisplayName);
                                AltStreamer.Nodes.Add(bacc.DisplayName);
                                treeView4.Nodes.Add(AltStreamer);
                                break;
                            }
                        }
                    }
                }
                List<DestinyPlayer> SortablePlayerList = new List<DestinyPlayer>();
                foreach (DestinyPlayer player in ConfirmedTwitchLinked)
                {
                    if (player.TwitchLinked)
                    {
                        TreeNode streamerNode = new TreeNode(player.MainDisplayName + " | twitch/" + player.TwitchName);

                        player.LinkedMatchTimes.Sort((x, y) => DateTime.Compare(y, x));

                        foreach (DateTime matchStart in player.LinkedMatchTimes)
                        {
                            streamerNode.Nodes.Add(matchStart.ToString());

                        }
                       
                        streamerNode.Tag = player;
                        treeView3.Nodes.Add(streamerNode);
                        SortablePlayerList.Add(player);
                    }
                    else
                    {
                        
                    }
                }


                SortablePlayerList.Sort((x, y) => y.LinkedMatchTimes[0].CompareTo(x.LinkedMatchTimes[0]));
                treeView3.Nodes.Clear();

                foreach(DestinyPlayer player in SortablePlayerList)
                {
                    TreeNode streamerNode = new TreeNode(player.MainDisplayName + " | twitch/" + player.TwitchName);
                    foreach (DateTime matchStart in player.LinkedMatchTimes)
                    {
                        streamerNode.Nodes.Add(matchStart.ToString());

                    }
                    streamerNode.Tag = player;
                    treeView3.Nodes.Add(streamerNode);
                }

                //Go back and add in detailed player information for recent games
                foreach (TreeNode matchNode in treeView2.Nodes)
                {
                    foreach (TreeNode childNode in matchNode.Nodes)
                    {
                        bool shouldBreak = false;
                        foreach (DestinyPlayer player in RecentPlayersDetailed)
                        {

                            if (childNode.Text.Contains(player.MainDisplayName))
                            {
                                foreach (DestinyPlayer.BungieAccount bacc in player.LinkedAccounts)
                                {
                                    childNode.Nodes.Add(bacc.DisplayName + " | " + bacc.UserType.ToString());

                                }
                                shouldBreak = true;
                            }
                            if (shouldBreak)
                            {
                                break;
                            }
                        }
                    }
                }

                Task.Run(() => CheckForTwitchVods());

            }
            else
            {
                RecentPlayersDetailed.Add(checkPlayer);
                if (checkPlayer.TwitchLinked)
                {
                    ConfirmedTwitchLinked.Add(checkPlayer);
                }

            }

        }

        //Adds a single carnage report to the middle treeview
        private void AddCarnageReport(ActivityReport PGCR)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                { AddCarnageReport(PGCR); });
                return;
            }

            TreeNode MatchNode = new TreeNode(PGCR.ActivityTypeID + " | " + PGCR.ActivitySpaceID);
            MatchNode.Tag = PGCR;
            MatchNode.Nodes.Add(PGCR.ActivityStart.ToString());
            MatchNode.Nodes.Add("Location ID | " + PGCR.LocationHash);
            MatchNode.Nodes.Add("Activity ID | " + PGCR.ActivityHash);

            foreach(DestinyPlayer matchPlayer in PGCR.ActivityPlayers)
            {
                matchPlayer.LinkedMatches.Add(PGCR);
                MatchNode.Nodes.Add(matchPlayer.MainDisplayName + " | " + matchPlayer.MainType.ToString());
                if(!PlayersToProcess.Contains(matchPlayer.MainDisplayName))
                {
                   
                    if(matchPlayer.MainDisplayName.ToLower().Trim() != PlayerExclusion.ToLower().Trim())
                    {
                        matchPlayer.LinkedMatchTimes.Add(PGCR.ActivityStart);
                       
                        PlayersToProcess.Add(matchPlayer.MainDisplayName);
                        PossibleStreams.Add(matchPlayer);
                        System.Diagnostics.Debug.Print("Player " + matchPlayer.MainDisplayName + " added to queue");

                    }
                }
                else
                {
                    if (matchPlayer.MainDisplayName.ToLower().Trim() != PlayerExclusion.ToLower().Trim())
                    {
                        
                        foreach (DestinyPlayer streamer in PossibleStreams)
                        {
                            if(streamer.MainDisplayName == matchPlayer.MainDisplayName)
                            {
                                System.Diagnostics.Debug.Print("Player " + matchPlayer.MainDisplayName + " was already queued, adding timestamp for other match");
                                streamer.LinkedMatchTimes.Add(PGCR.ActivityStart);
                                streamer.LinkedMatches.Add(PGCR);
                                break;
                            }
                        }
                    }
                }
            }
            treeView2.Nodes.Add(MatchNode);
        }
        
        //Adds a player to the search results 
        private void HandleSeachedAccountProcessed(DestinyPlayer Player)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                { HandleSeachedAccountProcessed(Player); });
                return;
            }

            
            TreeNode PlayerNode = new TreeNode(Player.MainDisplayName);
            foreach(DestinyPlayer.BungieAccount bacc in Player.LinkedAccounts)
            {
                PlayerNode.Nodes.Add(bacc.DisplayName + " | " + bacc.UserType.ToString());
                
            }
            foreach(DestinyPlayer.DestinyCharacter dchar in Player.LinkedCharacters)
            {
                TreeNode charNode = new TreeNode(dchar.CharacterClass.ToString());
                charNode.Tag = dchar;
                charNode.Nodes.Add(dchar.CharacterPower);
                PlayerNode.Nodes.Add(charNode);
            }
            PlayerNode.Tag = Player;
            treeView1.Nodes.Add(PlayerNode);
            UpdateStatusBar(1, 0);
        }

        //Loads the characters for a searched account
        private void HandleSearchedAccountComplete(List<DestinyPlayer> FoundPlayers)
        {
            UpdateStatusBar(0, 0, FoundPlayers.Count, "Loading players");
            foreach(DestinyPlayer player in FoundPlayers)
            {
                Task.Run(() => apiClient.LoadPlayerCharacters(player));
            }    
        }

        //Handles updates to the progress bar
        public void UpdateStatusBar(int addValue = 0, int addMax = 0, int setMax = 0, string StatusMessage = "Working...")
        {
            if(this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                { UpdateStatusBar(addValue, addMax, setMax, StatusMessage); });
                return;
            }

            DisableInput();
            if(PlayerExclusion !=null)
            {
                label1.Text = "Carnage Reports | " + PlayerExclusion;
            }
            label3.Text = LastKnownReport;
            
            label4.Text = StatusMessage;
            if(setMax > 0)
            {
                progressBar1.Maximum = setMax;
            }

            if(progressBar1.Value + addValue <= progressBar1.Maximum)
            {
                progressBar1.Value += addValue;
            }
            else
            {
                progressBar1.Value = progressBar1.Maximum;
            }
            label4.Refresh();
            label3.Refresh();

            if (progressBar1.Value == progressBar1.Maximum)
            {
                label4.Text = "Idle";
                progressBar1.Maximum = 0;
                progressBar1.Value = 0;
                EnableInput();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            treeView2.Nodes.Clear();
            treeView3.Nodes.Clear();
            LastKnownReport = "No players loaded";
            UpdateStatusBar(0, 0, 0, "Searching for player " + textBox1.Text);
            Task.Run(() => apiClient.SearchBungieAccounts(textBox1.Text, DestinyPlayer.BungieAccount.AccountType.Ignore));
            DisableInput();

        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
          

            DestinyPlayer player = (DestinyPlayer)e.Node.Parent.Tag;
            DestinyPlayer.DestinyCharacter playerChar = (DestinyPlayer.DestinyCharacter)e.Node.Tag;
            PlayerExclusion = player.MainDisplayName;

            _lastLoaded = player;
            _lastCharacter = playerChar;


            treeView2.Nodes.Clear();
            PlayersToProcess = new List<string>();
            PossibleStreams = new List<DestinyPlayer>();
            RecentPlayersDetailed = new List<DestinyPlayer>();
            ConfirmedTwitchLinked = new List<DestinyPlayer>();
            treeView3.Nodes.Clear();
            LastKnownReport = "No players loaded";
            UpdateStatusBar(0, 0, 0, "Loading carnage reports");
            PlayerExclusion = _lastLoaded.MainDisplayName.ToLower().Trim();

            Task.Run(() => apiClient.LoadCarnageReports(playerChar,player,(int)numericUpDown1.Value));
            DisableInput();
        }

        private DestinyPlayer _lastLoaded;
        private DestinyPlayer.DestinyCharacter _lastCharacter;

        private void button2_Click(object sender, EventArgs e)
        {
            treeView2.Nodes.Clear();
            PlayersToProcess = new List<string>();
            PossibleStreams = new List<DestinyPlayer>();
            treeView3.Nodes.Clear();
            LastKnownReport = "No players loaded";
            UpdateStatusBar(0, 0, 0, "Loading carnage reports");
            PlayerExclusion = _lastLoaded.MainDisplayName.ToLower().Trim();
            Task.Run(() => apiClient.LoadCarnageReports(_lastCharacter, _lastLoaded, (int)numericUpDown1.Value));
            DisableInput();
        }

        private void treeView3_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Clipboard.SetText(e.Node.Text);
        }
    }
}
