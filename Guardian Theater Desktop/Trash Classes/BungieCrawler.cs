using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Collections;

namespace Guardian_Theater_Desktop
{
    public class BungieCrawler
    {
        public string _APIKEY { get; set; }
        private string _apiBase = "https://www.bungie.net/Platform/Destiny2/";
        public enum CrawlerEventType
        {
            SearchForAccount_Complete,
            SearchForAccount_Failed,
            AccountCharactersLoaded_Complete,
            AccountCharactersLoaded_Failed,
            SingleCarnageLoaded,
            LoadedCarnageReports_Complete,
            LoadedCarnageReports_Failed,
            CheckLinkedAccounts_Complete,
            CheckLinkedAccounts_Failed
        }

        public event EventHandler<CrawlerEventType> CrawlerEvent;
        public string ConvertAccountType(DestinyPlayer.BungieAccount.AccountType UserType)
        {
            switch (UserType)
            {
                case DestinyPlayer.BungieAccount.AccountType.Xbox:
                    return "1";
                case DestinyPlayer.BungieAccount.AccountType.PSN:
                    return "2";
                case DestinyPlayer.BungieAccount.AccountType.Steam:
                    return "3";
                case DestinyPlayer.BungieAccount.AccountType.BNET:
                    return "254";
                case DestinyPlayer.BungieAccount.AccountType.Ignore:
                    return "-1";
                default:
                    return "-1";
            }
        }
        public void SearchBungieAccounts(string Username, DestinyPlayer.BungieAccount.AccountType UserType)
        {
            HttpWebRequest _client;
            ServicePointManager.DefaultConnectionLimit = 15;
            try
            {
                string accTypeModifier = ConvertAccountType(UserType);


                _client = (HttpWebRequest)WebRequest.Create(string.Format(_apiBase + "{0}{1}{2}{3}", "SearchDestinyPlayer/", accTypeModifier, "/", Username));
                _client.Method = "GET";
                _client.Headers.Add("X-API-KEY", _APIKEY);
                _client.KeepAlive = false;

                System.Diagnostics.Debug.Print("Searching for account : \n" + _client.RequestUri.ToString());
                //_response = (HttpWebResponse)_client.GetResponse();
                string responseText;
                using (HttpWebResponse _response = (HttpWebResponse)_client.GetResponse())
                {
                    responseText = new StreamReader(_response.GetResponseStream()).ReadToEnd();
                    _response.Close();
                }
                List<DestinyPlayer> foundPlayers = new List<DestinyPlayer>();

                List<string> foundPlayerMeta = new List<string>();

                int start = 0;
                int end = 0;
                int tempStart = 0;

                System.Diagnostics.Debug.Print(responseText);

                if (responseText.Contains("\"Response\":[]"))
                {
                    System.Diagnostics.Debug.Print("Search for account failed : " + Username + " / no results found");
                    CrawlerEvent?.Invoke(new List<DestinyPlayer>(), CrawlerEventType.SearchForAccount_Failed);
                    return;
                }
                while (start < responseText.LastIndexOf("displayName"))
                {
                    start = responseText.IndexOf("membershipType", end);
                    tempStart = start;
                    end = responseText.IndexOf("displayName", start);
                    start = responseText.IndexOf("}", end);
                    string playerMeta = responseText.Substring(tempStart, start - tempStart);

                    foundPlayerMeta.Add(playerMeta);
                }

                foreach (string player in foundPlayerMeta)
                {

                    DestinyPlayer playerAccount = new DestinyPlayer();
                    playerAccount.ProcessPlayerInformation(player);
                    foundPlayers.Add(playerAccount);

                    System.Diagnostics.Debug.Print("Located account : " + playerAccount.MainDisplayName);
                }

                CrawlerEvent?.Invoke(foundPlayers, CrawlerEventType.SearchForAccount_Complete);
            
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print("Search for account failed : " + Username + " / " + ex.Message);
                CrawlerEvent?.Invoke(new List<DestinyPlayer>(), CrawlerEventType.SearchForAccount_Failed);
            }
        }
        public void LoadPlayerCharacters(DestinyPlayer inputAccount)
        {
            HttpWebRequest _client;
            ServicePointManager.DefaultConnectionLimit = 15;
            try
            {
                string searchType = ConvertAccountType(inputAccount.MainType);


                _client = (HttpWebRequest)WebRequest.Create(string.Format(_apiBase + "{0}{1}{2}{3}", searchType, "/Profile/", inputAccount.MainAccountIdentifier, "/?components=100"));
                _client.Method = "GET";
                _client.Headers.Add("X-API-KEY", _APIKEY);
                _client.KeepAlive = false;

                System.Diagnostics.Debug.Print("Loading characters for account : \n" + _client.RequestUri.ToString());
                string responseBody;
                using (HttpWebResponse _response = (HttpWebResponse)_client.GetResponse())
                {
                 responseBody = new StreamReader(_response.GetResponseStream()).ReadToEnd();
                    _response.Close();
                }

                int start = 0;
                int end = 0;

                start = responseBody.IndexOf("characterIds", end) + 16;
                end = responseBody.IndexOf("]", start);

                string charMeta = responseBody.Substring(start, end - start);
                string[] idHolder = charMeta.Split(',');
                List<string> charactersCleaned = new List<string>();
                foreach (string id in idHolder)
                {
                    string parsedID = id.Replace("\"", "");
                    charactersCleaned.Add(parsedID);
                }
                inputAccount.LinkedCharacters = new List<DestinyPlayer.DestinyCharacter>();

                foreach (string charID in charactersCleaned)
                {
                    System.Diagnostics.Debug.Print(inputAccount.MainDisplayName + " charID | " + charID + " located");
                    _client = (HttpWebRequest)WebRequest.Create(string.Format(_apiBase + "{0}{1}{2}{3}{4}{5}", searchType, "/Profile/", inputAccount.MainAccountIdentifier, "/Character/", charID, "/?components=200"));
                    _client.Method = "GET";
                    _client.Headers.Add("X-API-KEY", _APIKEY);

                    System.Diagnostics.Debug.Print("Loading for character : \n" + _client.RequestUri.ToString());
                    using (HttpWebResponse _subresponse = (HttpWebResponse)_client.GetResponse())
                    //_response = (HttpWebResponse)_client.GetResponse();
                    {
                        charMeta = new StreamReader(_subresponse.GetResponseStream()).ReadToEnd();
                        _subresponse.Close();
                    }
                            DestinyPlayer.DestinyCharacter PlayerChar = new DestinyPlayer.DestinyCharacter();
                            PlayerChar.CharacterIdentifier = charID;
                            PlayerChar.parseCharacterMeta(charMeta);

                            inputAccount.LinkedCharacters.Add(PlayerChar);
                        
                }

                LoadPlayerLinkedAccounts(inputAccount, true);

                CrawlerEvent?.Invoke(inputAccount, CrawlerEventType.AccountCharactersLoaded_Complete);
            
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.Print("Error : " + ex.Message + " for char load : " + inputAccount.MainDisplayName);

                CrawlerEvent?.Invoke(inputAccount, CrawlerEventType.AccountCharactersLoaded_Failed);
            }
        }

        public void LoadPlayerLinkedAccounts(DestinyPlayer inputAccount, bool fromPlayerSearch = true, int failCount = 0)
        {
           
            HttpWebRequest _client;
            ServicePointManager.DefaultConnectionLimit = 15;
            string responseBody = "notset";
            string requrl = "";

            if (failCount ==3)
            {
                System.Diagnostics.Debug.Print("Loading linked accounts failed : " + inputAccount.MainDisplayName);
                CrawlerEvent?.Invoke(inputAccount, CrawlerEventType.CheckLinkedAccounts_Failed);
                return;
            }
            if(failCount > 0)
            {
                System.Threading.Thread.Sleep(1000);
            }
            try
            {

                string searchType = ConvertAccountType(inputAccount.MainType);


                _client = (HttpWebRequest)WebRequest.Create(string.Format(_apiBase + "{0}{1}{2}{3}", searchType, "/Profile/", inputAccount.MainAccountIdentifier, "/LinkedProfiles/?getAllMemberships=true"));
                requrl = _client.RequestUri.ToString();
                _client.Method = "GET";
                _client.Headers.Add("X-API-KEY", _APIKEY);
                _client.KeepAlive = false;

                System.Diagnostics.Debug.Print("Loading in depth for account : \n" + _client.RequestUri.ToString());

               
                using (HttpWebResponse _response = (HttpWebResponse)_client.GetResponse())
                {
                 responseBody = new StreamReader(_response.GetResponseStream()).ReadToEnd();
                    _response.Close();

                }

                inputAccount.LinkedAccounts = new List<DestinyPlayer.BungieAccount>();
                inputAccount.ProcessPlayerInformation(responseBody, fromPlayerSearch);

                if (!fromPlayerSearch)
                {
                    foreach (DestinyPlayer.BungieAccount bacc in inputAccount.LinkedAccounts)
                    {

                        if (bacc.UserType == DestinyPlayer.BungieAccount.AccountType.BNET)
                        {
                            System.Diagnostics.Debug.Print("Checking " + bacc.DisplayName + " for hard linked twitch");
                            _client = (HttpWebRequest)WebRequest.Create(string.Format("https://www.bungie.net/en/Profile/254/{0}{1}", bacc.AccountIdentifier, "/", Uri.EscapeDataString(bacc.DisplayName)));
                            requrl = _client.RequestUri.ToString();
                            _client.Method = "GET";
                            _client.Headers.Add("X-API-KEY", _APIKEY);
                            _client.KeepAlive = false;
                                using (HttpWebResponse _subResponse = (HttpWebResponse)_client.GetResponse())
                                {
                                   
                                    responseBody = new StreamReader(_subResponse.GetResponseStream()).ReadToEnd();
                                    _subResponse.Close();
                                }
                            int start = responseBody.IndexOf("profiles-container", 0);
                            int end = responseBody.IndexOf("reportClanProfileModal", start);
                            responseBody = responseBody.Substring(start, end - start);
                            if (responseBody.Contains("href=\"https://twitch.tv/"))
                            {
                                inputAccount.TwitchLinked = true;
                                start = responseBody.IndexOf("https://twitch.tv", 0) + 18;
                                end = responseBody.IndexOf("\"", start);

                                inputAccount.TwitchName = responseBody.Substring(start, end - start);
                            }
                        }

                    }
                    
                    CrawlerEvent?.Invoke(inputAccount, CrawlerEventType.CheckLinkedAccounts_Complete);
                }


            
                

            }
            catch (Exception ex)
            {

                failCount += 1;
                LoadPlayerLinkedAccounts(inputAccount, false, failCount);
                
            }
        }

        public int ReportsToLoad { get; set; }

        private int ReportsLoaded { get; set; }
        public void LoadSingleCarnageReport(string matchID,DestinyPlayer inputAccount)
        {

            HttpWebRequest _client;
            ServicePointManager.DefaultConnectionLimit = 15;
            int start = 0;
            int end = 0;
            try
            {
          
                    ActivityReport PGCR = new ActivityReport();
                    PGCR.ActivityRefID = matchID;

                    _client = (HttpWebRequest)WebRequest.Create("https://www.bungie.net/Platform/Destiny2/Stats/PostGameCarnageReport/" + matchID + "/");
                    _client.Method = "GET";
                    _client.Headers.Add("X-API-KEY", _APIKEY);

                    string matchData;
                    using (HttpWebResponse _subresponse = (HttpWebResponse)_client.GetResponse())
                    {
                        matchData = new StreamReader(_subresponse.GetResponseStream()).ReadToEnd();
                        _subresponse.Close();
                    }
                    start = 0;
                    end = 0;
                   
                    start = matchData.IndexOf("period", end) + 9;
                    end = matchData.IndexOf(",", start) - 1;
                    string timetoParse = matchData.Substring(start, end - start);

                string temploc;
                string tempact;
                    DateTime startAT = DateTime.Parse(timetoParse).ToUniversalTime();
                    PGCR.ActivityStart = startAT;

                    start = matchData.IndexOf("referenceId", end) + 13;
                    end = matchData.IndexOf(",", start);

                temploc = matchData.Substring(start, end - start);
                    PGCR.CompareLocation(matchData.Substring(start, end - start));

                    start = matchData.IndexOf("directorActivityHash", end) + 22;
                    end = matchData.IndexOf(",", start);

                tempact = matchData.Substring(start, end - start);
                    PGCR.CompareGameType(matchData.Substring(start, end - start));

                    start = 0;
                    end = 0;
                System.Diagnostics.Debug.Print(matchData);
                System.Diagnostics.Debug.Print("Carnage Report Loaded : \n" + "Loc hash : " + temploc + "\nAct Hash : " + tempact);

                    List<DestinyPlayer> matchPlayers = new List<DestinyPlayer>();
                    while (start < matchData.LastIndexOf("displayName"))
                    {
                        start = matchData.IndexOf("membershipType", end);
                        int tempStart = start;
                        end = matchData.IndexOf("displayName", start);
                        start = matchData.IndexOf("}", end);
                        string playerMeta = matchData.Substring(tempStart, start - tempStart);

                    

                        DestinyPlayer matchPlayer = new DestinyPlayer();
                        matchPlayer.ProcessPlayerInformation(playerMeta);
                        matchPlayers.Add(matchPlayer);
                        
                    }
                    PGCR.ActivityPlayers = matchPlayers;


                    //CarnageReports.Add(PGCR);
                    CrawlerEvent?.Invoke(PGCR, CrawlerEventType.SingleCarnageLoaded);

                ReportsLoaded += 1;

                System.Diagnostics.Debug.Print("match " + ReportsLoaded + "/" + ReportsToLoad);
                if (ReportsLoaded == ReportsToLoad)
                {
                    CrawlerEvent?.Invoke(PGCR, CrawlerEventType.LoadedCarnageReports_Complete);
                }
            }
            catch (Exception ex) { ReportsLoaded += 1; System.Diagnostics.Debug.Print("Failed to load carnage reports : " + ex.Message); CrawlerEvent?.Invoke(ReportsLoaded, CrawlerEventType.LoadedCarnageReports_Failed); }
        }
    
        public void LoadCarnageReports(DestinyPlayer.DestinyCharacter CharToLoad, DestinyPlayer inputAccount, int Count = 1)
        {
            HttpWebRequest _client;
            ServicePointManager.DefaultConnectionLimit = 15;
            ReportsToLoad = Count;
            ReportsLoaded = 0;
            try
            {
                string searchType = ConvertAccountType(inputAccount.MainType);


                _client = (HttpWebRequest)WebRequest.Create(string.Format(_apiBase + "{0}{1}{2}{3}{4}{5}", searchType, "/Account/", inputAccount.MainAccountIdentifier
                    , "/Character/", CharToLoad.CharacterIdentifier, "/Stats/Activities/?count=" + Count.ToString()));

                _client.Method = "GET";
                _client.Headers.Add("X-API-KEY", _APIKEY);
                _client.KeepAlive = false;
                System.Diagnostics.Debug.Print("Loading matches : \n" + _client.RequestUri);
                string responseBody;
                using (HttpWebResponse _response = (HttpWebResponse)_client.GetResponse())
                {
                    responseBody = new StreamReader(_response.GetResponseStream()).ReadToEnd();
                    _response.Close();
                }
                System.Diagnostics.Debug.Print(responseBody);
                int start = 0;
                int end = 0;
                List<string> Matches = new List<string>();
                while (start < responseBody.LastIndexOf("instanceId"))
                {
                    start = responseBody.IndexOf("instanceId", end) + 13;
                    end = responseBody.IndexOf(",", start) - 1;
                    Matches.Add(responseBody.Substring(start, end - start));
                }

                List<ActivityReport> CarnageReports = new List<ActivityReport>();

                System.Diagnostics.Debug.Print("Attempting to load carnage reports for : " + inputAccount.MainDisplayName + " | " + CharToLoad.CharacterClass.ToString());
                foreach (string matchID in Matches)
                {
                    Task.Run(() => LoadSingleCarnageReport(matchID, inputAccount));
                }

                System.Diagnostics.Debug.Print("ALl carnage reports queued for load");
            }
            catch (Exception ex) { System.Diagnostics.Debug.Print("Failed to load carnage reports : " + ex.Message); CrawlerEvent?.Invoke(ReportsLoaded, CrawlerEventType.LoadedCarnageReports_Failed); }
        }
      }   
}


    public class DestinyPlayer
    {
        public List<BungieAccount> LinkedAccounts { get; set; }
        public List<DestinyCharacter> LinkedCharacters { get; set; }

        public List<DateTime> LinkedMatchTimes { get; set; }
        public bool TwitchLinked { get; set; }
        public string TwitchName { get; set; }

        public bool PGCRTied { get; set; }
        public bool AccountCrawled { get; set; }

        public string MainDisplayName { get; set; }
        public BungieAccount.AccountType MainType { get; set; }
        public string MainAccountIdentifier { get; set; }
        
        public List<ActivityReport> LinkedMatches { get; set; }
        public void ProcessPlayerInformation(string inputData, bool resetMatches = true)
        {
            LinkedAccounts = new List<BungieAccount>();
            
            TwitchLinked = false;
             if (resetMatches)
             {
                LinkedMatchTimes = new List<DateTime>();
                LinkedMatches = new List<ActivityReport>();
             }

            int start = 0;
            int end = 0;

            while(start< inputData.LastIndexOf("displayName"))
            {
                BungieAccount userAcc = new BungieAccount();

                start = inputData.IndexOf("membershipType", end) + 16;
                end = inputData.IndexOf(",", start);

                string tempType = inputData.Substring(start, end - start);

                switch (tempType)
                {
                    case "1":
                        userAcc.UserType = BungieAccount.AccountType.Xbox;

                        break;
                    case "2":
                        userAcc.UserType = BungieAccount.AccountType.PSN;

                        break;
                    case "3":
                        userAcc.UserType = BungieAccount.AccountType.Steam;
                       
                        break;
                    case "254":
                        userAcc.UserType = BungieAccount.AccountType.BNET;
                        break;
                }
                start = inputData.IndexOf("membershipId", end) + 15;
                end = inputData.IndexOf(",", start) - 1;

                userAcc.AccountIdentifier = inputData.Substring(start, end - start);

                start = inputData.IndexOf("displayName", end) + 14;
                end = inputData.IndexOf("\"", start);

                userAcc.DisplayName = inputData.Substring(start, end - start);

                LinkedAccounts.Add(userAcc);
            }

            if (LinkedAccounts.Count > 0)
            {
                MainDisplayName = LinkedAccounts[0].DisplayName;
                MainType = LinkedAccounts[0].UserType;
                MainAccountIdentifier = LinkedAccounts[0].AccountIdentifier;
            }
        }

        public class BungieAccount
        {
            public enum AccountType
            {
                Xbox,
                PSN,
                Steam,
                BNET,
                Ignore
            }

            
            public AccountType UserType { get; set; }
            public string DisplayName { get; set; }
            public string AccountIdentifier { get; set; }


        }
        public class DestinyCharacter
        {
            public enum CharacterType
            {
                Hunter,
                Warlock,
                Titan
            }

            public string LinkedIdentifier { get; set; }
            public CharacterType CharacterClass { get; set; }

            public string CharacterIdentifier { get; set; }
            public string CharacterPower { get; set; }
            public string CharcterEmeblemLink { get; set; }

            public void parseCharacterMeta(string inputData)
            {
                int start = 0;
                int end = 0;

                start = inputData.IndexOf("light", end) + 7;
                end = inputData.IndexOf(",", start);

                CharacterPower = inputData.Substring(start, end - start);

                string classHash;

                start = inputData.IndexOf("classHash", end) + 11;
                end = inputData.IndexOf(",", start);

                classHash = inputData.Substring(start, end - start);
                switch (classHash)
                {
                    case "671679327":
                        CharacterClass = CharacterType.Hunter;
                        break;
                    case "2271682572":
                        CharacterClass = CharacterType.Warlock;
                        break;
                    case "3655393761":
                        CharacterClass = CharacterType.Titan;
                        break;
                }
            }

        }
    }

    public class ActivityReport
    {

        public void CompareGameType(string hash)
        {

            ActivityHash = hash;
            //System.Diagnostics.Debug.Print("Gametype to find : " + hash);
            switch (hash)
            {
            
            case "1738383283":
                ActivityTypeID = "Harbinger";
                break;
            case "5517242":
                ActivityTypeID = "Empire Hunt";
                break;
            case "3881495763":
                ActivityTypeID = "Raid";
                break;
            case "70223475":
                ActivityTypeID = "Tangled Shore";
                break;
            case "2936791966":
                    ActivitySpaceID = "Lost Sector";
                    break;
                case "1928961926":
                    ActivityTypeID = "Patrol";
                    break;
                case "2865450620":
                    ActivityTypeID = "Survival";
                    break;
                case "1717505396":
                    ActivityTypeID = "Control";
                    break;
                case "588019350":
                    ActivityTypeID = "Trials";
                    break;
                case "25688104":
                    ActivityTypeID = "Override";
                    break;
                case "1683791010":
                    ActivityTypeID = "Iron Banner";
                    break;
            case "2865532048":
                ActivityTypeID = "Override";
                break;
            default:
                    ActivityTypeID = "Uncategorized Game mode";
                    break;
            }
        }

        public void CompareLocation(string hash)
        {
            LocationHash = hash;
            //System.Diagnostics.Debug.Print("Location to find : " + hash);
            switch (hash)
            {
            case "2585182686":
                ActivitySpaceID = "Fortess";
                    break;
            case "236451195":
                ActivitySpaceID = "Distant shore";
                break;
            case "3445454561":
                ActivitySpaceID = "The Anomaly";
                break;
            case "1738383283":
                ActivitySpaceID = "Harbinger";
                break;
            case "5517242":
                ActivitySpaceID = "Empire Hunt";
                break;
            case "3881495763":
                ActivitySpaceID = "Vault of Glass";
                break;
            case "70223475":
                ActivitySpaceID = "Tangled Shore";
                break;
            case "2903548701":
                ActivitySpaceID = "Radiant Cliffs";
                break;
            case "4242091248":
                ActivitySpaceID = "Twilight Gap";
                break;
            case "1486201898":
                ActivitySpaceID = "Convergence";
                break;
            case "3284567441":
                ActivitySpaceID = "Exodus Blue";
                break;
            case "1448047125":
                ActivitySpaceID = "Widows Court";
                break;
            ///////////
            case "2260508373":
                ActivitySpaceID = "Bannerfall";
                break;
            case "1326470716":
                ActivitySpaceID = "Endless Vale";
                break;
            case "2905719116":
                ActivitySpaceID = "Fragment";
                break;
            //Normal Locations
            case "2865532048":
                ActivitySpaceID = "Override:Moon";
                break;
            case "1429033007":
                    ActivitySpaceID = "Bannerfall";
                    break;
                case "25688104":
                    ActivitySpaceID = "Override:Europa";
                    break;
                case "2936791966":
                    ActivitySpaceID = "Exodus Garden 2A:Legend";
                    break;
                case "1928961926":
                    ActivitySpaceID = "Cosmodrome";
                    break;    
                case "279800038":
                    ActivitySpaceID = "Midtown";
                    break;
                case "2081877020":
                    ActivitySpaceID = "Fortress";
                    break;
                case "1375350354":
                    ActivitySpaceID = "Twilight Gap";
                    break;
                case "780203647":
                    ActivitySpaceID = "Radiant Cliffs";
                    break;
                case "2133997042":
                    ActivitySpaceID = "Rusted Lands";
                    break;
                case "2361232192":
                    ActivitySpaceID = "Cauldron";
                    break;
                case "528628085":
                    ActivitySpaceID = "Distant Shore";
                    break;
                case "1009746517":
                    ActivitySpaceID = "Wormhaven";
                    break;
                case "2296512046":
                    ActivitySpaceID = "Endless Vale";
                    break;
                case "1960008274":
                    ActivitySpaceID = "Endless Vale";
                    break;
                case "2851089328":
                    ActivitySpaceID = "Pacifica";
                    break;
                case "1141558876":
                    ActivitySpaceID = "Altar of Flame";
                    break;
                case "2238426332":
                    ActivitySpaceID = "Burnout";
                    break;
                case "2755115715":
                    ActivitySpaceID = "Exodus Blue";
                    break;
                case "503710612":
                    ActivitySpaceID = "Convergence";
                    break;
                case "427041827":
                    ActivitySpaceID = "Widows Court";
                    break;
                case "1008195646":
                    ActivitySpaceID = "Fragment";
                    break;
                case "788769683":
                    ActivitySpaceID = "Jav 4";
                    break;
                default:
                    ActivitySpaceID = "Unkown Location";
                    break;
            }
        }
        public List<DestinyPlayer> ActivityPlayers { get; set; }

        public string ActivityRefID { get; set; }
        public string ActivitySpaceID { get; set; }
        public string ActivityTypeID { get; set; }
        
        public string LocationHash { get; set; }
        public string ActivityHash { get; set; }
        public DateTime ActivityStart { get; set; }
        public enum ActivtyType
        {
            Strike,
            Control,
            Clash,
            Survival,
            Solo_Survival,
            Trials
        }

        public ActivtyType GameType { get; set; }
    }


