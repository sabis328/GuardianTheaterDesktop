using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Collections;


namespace Guardian_Theater_Desktop
{
    public class TheaterClient
    {

        public bool CancelAction { get; set; }
        public string _BungieApiKey { get; set; }
        public string _TwitchApiKey { get; set; }
        public string _TwitchApiSecret { get; set; }

        //Used to prevent multiple overlapping calls
        public bool InProgress { get; set; }

        private string _apiBase = "https://www.bungie.net/Platform/Destiny2/";


        public string PlayerExclusion { get; set; }

        public event EventHandler<ClientEventType> _TheaterClientEvent;
        public enum ClientEventType
        {
            SearchComplete,
            SearchFail,
            CharactersComplete,
            CharactersFail,
            SingleCarnageComplete,
            SingleCarnageFail,
            AllCarnageComplete,
            AllCarnageFail,
            CheckLinkedAccountsComplete,
            CheckLinkedAccountsFail,
            CancelAll
        }
        public string ConvertAccountType(Guardian.BungieAccount.AccountType UserType)
        {
            switch (UserType)
            {
                case Guardian.BungieAccount.AccountType.Xbox:
                    return "1";
                case Guardian.BungieAccount.AccountType.PSN:
                    return "2";
                case Guardian.BungieAccount.AccountType.Steam:
                    return "3";
                case Guardian.BungieAccount.AccountType.BNET:
                    return "254";
                case Guardian.BungieAccount.AccountType.Ignore:
                    return "-1";
                default:
                    return "-1";
            }
        }

        public Guardian.BungieAccount.AccountType AccountTypeFromString(string accountType)
        {
            switch(accountType)
            {
                case "Xbox":
                    return Guardian.BungieAccount.AccountType.Xbox;
                case "PSN":
                    return Guardian.BungieAccount.AccountType.PSN;
                case "Steam":
                    return Guardian.BungieAccount.AccountType.Steam;
                case "BNET":
                    return Guardian.BungieAccount.AccountType.BNET;
                case "Ignore":
                    return Guardian.BungieAccount.AccountType.Ignore;
                default:
                    return Guardian.BungieAccount.AccountType.Ignore;
            }
        }

        public void SearchBungieAccounts(string Username, Guardian.BungieAccount.AccountType UserType)
        {
            while (!CancelAction)
            {
                if (!InProgress)
                {
                    InProgress = true;
                    HttpWebRequest _client;
                    ServicePointManager.DefaultConnectionLimit = 15;
                    try
                    {
                        string accTypeModifier = ConvertAccountType(UserType);

                        _client = (HttpWebRequest)WebRequest.Create(string.Format(_apiBase + "{0}{1}{2}{3}", "SearchDestinyPlayer/", accTypeModifier, "/", Uri.EscapeDataString(Username)));
                        _client.Method = "GET";
                        _client.Headers.Add("X-API-KEY", _BungieApiKey);
                        _client.KeepAlive = false;

                        string responseText;
                        using (HttpWebResponse _response = (HttpWebResponse)_client.GetResponse())
                        {
                            responseText = new StreamReader(_response.GetResponseStream()).ReadToEnd();
                            _response.Close();
                        }
                        List<Guardian> foundPlayers = new List<Guardian>();
                        List<string> compareplayerIDs = new List<string>();
                        List<string> foundPlayerMeta = new List<string>();

                        int start = 0;
                        int end = 0;
                        int tempStart = 0;

                        //System.Diagnostics.Debug.Print(responseText);

                        if (responseText.Contains("\"Response\":[]"))
                        {
                            //No results were found
                            InProgress = false;
                            _TheaterClientEvent?.Invoke(new List<Guardian>(), ClientEventType.SearchFail);
                            break;
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

                            Guardian playerAccount = new Guardian();
                            playerAccount.ProcessPlayerInformation(player);
                            if (!compareplayerIDs.Contains(playerAccount.MainAccountIdentifier))
                            {
                                compareplayerIDs.Add(playerAccount.MainAccountIdentifier);

                                foundPlayers.Add(playerAccount);
                            }
                        }

                        _TheaterClientEvent?.Invoke(foundPlayers, ClientEventType.SearchComplete);
                        InProgress = false;
                        break;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Print("Search for account failed : " + Username + " / " + ex.Message);
                        _TheaterClientEvent?.Invoke(new List<Guardian>(), ClientEventType.SearchFail);
                        InProgress = false;
                        break;
                    }
                }
            }
        }

        public void LoadCharacterEntries(Guardian inputAccount, bool fromAlt = true)
        {
            while (!CancelAction)
            {
                if (!InProgress)
                {
                    HttpWebRequest _client;
                    ServicePointManager.DefaultConnectionLimit = 15;
                    try
                    {
                        string searchType = ConvertAccountType(inputAccount.MainType);


                        _client = (HttpWebRequest)WebRequest.Create(string.Format(_apiBase + "{0}{1}{2}{3}", searchType, "/Profile/", inputAccount.MainAccountIdentifier, "/?components=100"));
                        _client.Method = "GET";
                        _client.Headers.Add("X-API-KEY", _BungieApiKey);
                        _client.KeepAlive = false;

                        System.Diagnostics.Debug.Print("Character request");
                        System.Diagnostics.Debug.Print(_client.RequestUri.ToString());

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
                        inputAccount.CharacterEntries = new List<Guardian.CharacterEntry>();

                        foreach (string charID in charactersCleaned)
                        {
                            _client = (HttpWebRequest)WebRequest.Create(string.Format(_apiBase + "{0}{1}{2}{3}{4}{5}", searchType, "/Profile/", inputAccount.MainAccountIdentifier, "/Character/", charID, "/?components=200"));
                            _client.Method = "GET";
                            _client.Headers.Add("X-API-KEY", _BungieApiKey);

                            System.Diagnostics.Debug.Print("Loading for character : \n" + _client.RequestUri.ToString());
                            using (HttpWebResponse _subresponse = (HttpWebResponse)_client.GetResponse())
                            {
                                charMeta = new StreamReader(_subresponse.GetResponseStream()).ReadToEnd();
                                _subresponse.Close();
                            }
                            Guardian.CharacterEntry PlayerChar = new Guardian.CharacterEntry();
                            PlayerChar.CharacterIdentifier = charID;
                            PlayerChar.parseCharacterMeta(charMeta);

                            inputAccount.CharacterEntries.Add(PlayerChar);

                        }

                        InProgress = false;
                        if (!fromAlt)
                        {
                            LoadLinkedAccounts(inputAccount, true);
                            _TheaterClientEvent?.Invoke(inputAccount, ClientEventType.CharactersComplete);
                           
                            break;
                        }

                        _TheaterClientEvent?.Invoke(inputAccount, ClientEventType.CharactersComplete);
                        break ;

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Print("Error in character loading for : " + inputAccount.MainDisplayName);
                        InProgress = false;
                        _TheaterClientEvent?.Invoke(inputAccount, ClientEventType.CharactersFail);
                        break;
                    }
                }
            }

            if(CancelAction)
            {
                _TheaterClientEvent?.Invoke(this, ClientEventType.CancelAll);
            }
        }

        public void LoadLinkedAccounts(Guardian inputAccount, bool fromPlayerSearch = true, int failCount = 0)
        {
            while (!CancelAction)
            {
                if (!InProgress)
                {
                    InProgress = true;
                    HttpWebRequest _client;
                    ServicePointManager.DefaultConnectionLimit = 15;
                    string responseBody = "notset";
                    string requrl = "";

                    if (failCount == 3)
                    {
                        System.Diagnostics.Debug.Print("Loading linked accounts failed : " + inputAccount.MainDisplayName);
                        _TheaterClientEvent?.Invoke(inputAccount, ClientEventType.CheckLinkedAccountsFail);
                        break;
                    }
                   
                    try
                    {

                        string searchType = ConvertAccountType(inputAccount.MainType);


                        _client = (HttpWebRequest)WebRequest.Create(string.Format(_apiBase + "{0}{1}{2}{3}", searchType, "/Profile/", inputAccount.MainAccountIdentifier, "/LinkedProfiles/?getAllMemberships=true"));
                        requrl = _client.RequestUri.ToString();
                        _client.Method = "GET";
                        _client.Headers.Add("X-API-KEY", _BungieApiKey);
                        _client.KeepAlive = false;

                        System.Diagnostics.Debug.Print("Loading in depth for account : \n" + _client.RequestUri.ToString());


                        using (HttpWebResponse _response = (HttpWebResponse)_client.GetResponse())
                        {
                            responseBody = new StreamReader(_response.GetResponseStream()).ReadToEnd();
                            _response.Close();

                        }


                        System.Diagnostics.Debug.Print(responseBody);
                        inputAccount.LinkedAccounts = new List<Guardian.BungieAccount>();
                        inputAccount.ProcessPlayerInformation(responseBody, fromPlayerSearch);

                        if (!fromPlayerSearch)
                        {
                            foreach (Guardian.BungieAccount bacc in inputAccount.LinkedAccounts)
                            {

                                if (bacc.UserType == Guardian.BungieAccount.AccountType.BNET)
                                {
                                    System.Diagnostics.Debug.Print("Checking " + bacc.DisplayName + " for hard linked twitch");
                                    _client = (HttpWebRequest)WebRequest.Create(string.Format("https://www.bungie.net/en/Profile/254/{0}{1}", bacc.AccountIdentifier, "/", Uri.EscapeDataString(inputAccount.MainDisplayName)));
                                    requrl = _client.RequestUri.ToString();
                                    _client.Method = "GET";
                                    _client.Headers.Add("X-API-KEY", _BungieApiKey);
                                    _client.KeepAlive = false;
                                    using (HttpWebResponse _subResponse = (HttpWebResponse)_client.GetResponse())
                                    {

                                        responseBody = new StreamReader(_subResponse.GetResponseStream()).ReadToEnd();
                                        _subResponse.Close();
                                    }
                                    System.Diagnostics.Debug.Print(_client.RequestUri.ToString());
                                    System.Diagnostics.Debug.Print(responseBody);
                                    
                                    int start = responseBody.IndexOf("profiles-container", 0);
                                    int end = responseBody.IndexOf("reportClanProfileModal", start);
                                    responseBody = responseBody.Substring(start, end - start);
                                    if (responseBody.Contains("href=\"https://twitch.tv/"))
                                    {
                                        inputAccount.HasTwitch = true;
                                        start = responseBody.IndexOf("https://twitch.tv", 0) + 18;
                                        end = responseBody.IndexOf("\"", start);

                                        inputAccount.TwitchName = responseBody.Substring(start, end - start);
                                    }
                                }

                            }

                            _TheaterClientEvent?.Invoke(inputAccount, ClientEventType.CheckLinkedAccountsComplete);
                        }

                        InProgress = false;
                        break;



                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Print("Error for : " + inputAccount.MainDisplayName + "  in linked account loading");
                        _TheaterClientEvent?.Invoke(this, ClientEventType.CheckLinkedAccountsFail);
                        
                    }
                }
            }
            if (CancelAction)
            {
                _TheaterClientEvent?.Invoke(this, ClientEventType.CancelAll);
            }
        }



        //Load list of (x) number match ids
        //Load each match and parse out match data and basic player names
        //Fire event of that match loading so the user has something to look at
        //Load each player into the pool of recent players or add another match tag to the player
        //Load detailed information for each player and resort the players into the matches
        //Sort the recent matches by date and fire event that matches are loaded

        public List<CarnageReport> RecentMatches { get; set; }
        public List<Guardian> RecentPlayers { get; set; }
        private List<string> RecentPlayerCompare { get; set; }
       
        public int ReportsToLoad { get; set; }
        public int ReportsLoaded { get; set; }
        public void LoadCarnageReportList(Guardian.CharacterEntry CharToLoad, Guardian inputAccount, int Count = 1)
        {
            while (!CancelAction)
            {
                RecentMatches = new List<CarnageReport>();
                RecentPlayers = new List<Guardian>();
                RecentPlayerCompare = new List<string>();
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
                    _client.Headers.Add("X-API-KEY", _BungieApiKey);
                    _client.KeepAlive = false;

                    string responseBody;
                    using (HttpWebResponse _response = (HttpWebResponse)_client.GetResponse())
                    {
                        responseBody = new StreamReader(_response.GetResponseStream()).ReadToEnd();
                        _response.Close();
                    }
                    //System.Diagnostics.Debug.Print(responseBody);
                    int start = 0;
                    int end = 0;
                    List<string> Matches = new List<string>();
                    while (start < responseBody.LastIndexOf("instanceId"))
                    {
                        start = responseBody.IndexOf("instanceId", end) + 13;
                        end = responseBody.IndexOf(",", start) - 1;
                        Matches.Add(responseBody.Substring(start, end - start));
                    }

                    //List<CarnageReport> CarnageReports = new List<CarnageReport>();

                    foreach (string matchID in Matches)
                    {
                        Task.Run(() => LoadSingleCarnageReport(matchID, inputAccount));
                    }

                    break;
                }
                catch (Exception ex) { System.Diagnostics.Debug.Print("Failed to load carnage reports : " + ex.Message); _TheaterClientEvent?.Invoke(ReportsLoaded, ClientEventType.AllCarnageFail);break; }

            }
            if(CancelAction)
            {
                _TheaterClientEvent?.Invoke(this, ClientEventType.CancelAll);
            }
        }
        public void LoadSingleCarnageReport(string matchID, Guardian inputAccount)
        {
            while (!CancelAction)
            {
                HttpWebRequest _client;
                ServicePointManager.DefaultConnectionLimit = 15;
                int start = 0;
                int end = 0;
                try
                {

                    CarnageReport PGCR = new CarnageReport();
                    PGCR.ActivityRefID = matchID;

                    _client = (HttpWebRequest)WebRequest.Create("https://www.bungie.net/Platform/Destiny2/Stats/PostGameCarnageReport/" + matchID + "/");
                    _client.Method = "GET";
                    _client.Headers.Add("X-API-KEY", _BungieApiKey);

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
                    PGCR.SetLocation(matchData.Substring(start, end - start));

                    start = matchData.IndexOf("directorActivityHash", end) + 22;
                    end = matchData.IndexOf(",", start);

                    tempact = matchData.Substring(start, end - start);
                    PGCR.SetGameType(matchData.Substring(start, end - start));

                    start = 0;
                    end = 0;

                    List<Guardian> matchPlayers = new List<Guardian>();
                    while (start < matchData.LastIndexOf("displayName"))
                    {
                        start = matchData.IndexOf("membershipType", end);
                        int tempStart = start;
                        end = matchData.IndexOf("displayName", start);
                        start = matchData.IndexOf("}", end);
                        string playerMeta = matchData.Substring(tempStart, start - tempStart);



                        Guardian matchPlayer = new Guardian();
                        matchPlayer.ProcessPlayerInformation(playerMeta);
                        matchPlayers.Add(matchPlayer);


                        int wepstart = matchData.IndexOf("weapons", end);
                        if (wepstart > 0)
                        {
                            int wepend = matchData.IndexOf("destinyUserInfo", wepstart);
                            if (wepend < 0)
                            {
                                wepend = matchData.IndexOf("teams", wepstart);
                            }
                            string wepData = matchData.Substring(wepstart, wepend - wepstart);

                            //System.Diagnostics.Debug.Print("Weapon string found : " + wepData);
                            wepstart = 0;
                            wepend = 0;
                            wepstart = wepData.IndexOf("referenceId", 0);
                            int Tempwepend = wepData.LastIndexOf("uniqueWeaponKillsPrecisionKills");
                            wepend = wepData.IndexOf("displayValue", Tempwepend) + 15;
                            Tempwepend = wepData.IndexOf("\"", wepend);
                            string processedWepData = wepData.Substring(wepstart, Tempwepend - wepstart);

                            matchPlayer.UsedWeapons = ProcessWeaponMeta(processedWepData, PGCR.ActivityRefID);
                        }


                        if (!RecentPlayerCompare.Contains(matchPlayer.MainAccountIdentifier))
                        {
                            if (matchPlayer.MainDisplayName != PlayerExclusion)
                            {
                                RecentPlayerCompare.Add(matchPlayer.MainAccountIdentifier);
                                matchPlayer.LinkedMatches.Add(PGCR);
                                matchPlayer.LinkedMatchTimes.Add(PGCR.ActivityStart);
                                RecentPlayers.Add(matchPlayer);
                            }
                        }
                        else
                        {
                            foreach (Guardian g in RecentPlayers)
                            {
                                if (g.MainAccountIdentifier == matchPlayer.MainAccountIdentifier)
                                {
                                    g.LinkedMatches.Add(PGCR);
                                    g.LinkedMatchTimes.Add(PGCR.ActivityStart);
                                }
                            }
                        }

                    }
                    PGCR.ActivityPlayers = matchPlayers;

                    _TheaterClientEvent?.Invoke(PGCR, ClientEventType.SingleCarnageComplete);

                    ReportsLoaded += 1;

                    RecentMatches.Add(PGCR);
                    System.Diagnostics.Debug.Print("match " + ReportsLoaded + "/" + ReportsToLoad);

                    if (ReportsLoaded == ReportsToLoad)
                    {
                        System.Diagnostics.Debug.Print("All matches loaded");
                        RecentMatches.Sort((x, y) => y.ActivityStart.CompareTo(x.ActivityStart));

                        _TheaterClientEvent?.Invoke(PGCR, ClientEventType.AllCarnageComplete);
                        break;
                    }

                    break;
                }
                catch (Exception ex) { ReportsLoaded += 1; System.Diagnostics.Debug.Print("Failed to load carnage reports : " + ex.Message); _TheaterClientEvent?.Invoke(ReportsLoaded, ClientEventType.SingleCarnageFail); break; }

            }

            if(CancelAction)
            {
                _TheaterClientEvent?.Invoke(this, ClientEventType.CancelAll);
            }
        }


        public List<Guardian.Weapon> ProcessWeaponMeta(string inputData, string LinkedMatchID)
        {
           
            List<Guardian.Weapon> playerWeps = new List<Guardian.Weapon>();
            
            int start = 0;
            int end = 0;

            int lastIndex = inputData.LastIndexOf("referenceId");
            if(lastIndex == 0)
            {
                System.Diagnostics.Debug.Print("only on wep");
                lastIndex = 5;
            }
            if(lastIndex < 0)
            {
                return playerWeps;
            }
            while (start < lastIndex)
            {
                start = inputData.IndexOf("referenceId", end) + 13;
                end = inputData.IndexOf(",", start);

                string wepid = inputData.Substring(start, end - start);

                start = inputData.IndexOf("uniqueWeaponKills", end);
                end = inputData.IndexOf("displayValue", start);
                start = end + 15;
                end = inputData.IndexOf("\"", start);

                string wepkill = inputData.Substring(start, end - start);

                start = inputData.IndexOf("uniqueWeaponKillsPrecisionKills", end);
                end = inputData.IndexOf("displayValue", start);
                start = end + 15;
                end = inputData.IndexOf("\"", start);
                if(end < 0)
                {
                    end = inputData.Length;
                }

                string wepPrec = inputData.Substring(start, end - start);

                Guardian.Weapon wep = new Guardian.Weapon();
                wep.WeaponIdentifier = wepid;
                wep.WeaponPrecisionRatio = wepPrec;
                wep.WeaponKills = wepkill;
                wep.IdentifyWeapon();

                playerWeps.Add(wep);
            }
            
            return playerWeps;
        }


    }

 

    public class Guardian
    {
        public List<Weapon> UsedWeapons { get; set; }
        public List<BungieAccount> LinkedAccounts { get; set; }
        public List<CharacterEntry> CharacterEntries { get; set; }
        public List<DateTime> LinkedMatchTimes { get; set; }
        public List<CarnageReport> LinkedMatches { get; set; }

        public bool HasTwitch { get; set; }

        public bool liveNow { get; set; }
        public string TwitchName { get; set; }
        public string AlternateTwitch { get; set; }

        public bool HasALt { get; set; }
        public string MainDisplayName { get; set; }
        public BungieAccount.AccountType MainType { get; set; }

        public string MainAccountIdentifier { get; set; }

        public void ProcessPlayerInformation(string inputData, bool resetMatches = true)
        {
            LinkedAccounts = new List<BungieAccount>();

            HasTwitch = false;
            liveNow = false;
            if (resetMatches)
            {
                LinkedMatchTimes = new List<DateTime>();
                LinkedMatches = new List<CarnageReport>();
                UsedWeapons = new List<Weapon>();
            }

            int start = 0;
            int end = 0;

            while (start < inputData.LastIndexOf("displayName"))
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

        public class Weapon
        {
            public string WeaponIdentifier { get; set; }
            public string WeaponKills { get; set; }
            public string WeaponPrecisionRatio { get; set; }

            public string LinkedToMatch { get; set; }


            public bool Suspected { get; set; }
            public void IdentifyWeapon()
            {
                Suspected = false;
                switch(WeaponIdentifier)
                {
                    case "4425887":
                        WeaponIdentifier = "The Time-Worn Spire";
                        break;
                    case "19024058":
                        WeaponIdentifier = "Prometheus Lens";
                        break;
                    case "20025671":
                        WeaponIdentifier = "Tango-45 XK5094";
                        break;
                    case "20935540":
                        WeaponIdentifier = "Arctic Haze";
                        break;
                    case "35794111":
                        WeaponIdentifier = "Temptation's Hook";
                        break;
                    case "42351395":
                        WeaponIdentifier = "Subzero Salvo";
                        break;
                    case "47772649":
                        WeaponIdentifier = "THE SWARM";
                        break;
                    case "48361212":
                        WeaponIdentifier = "Controlling Vision";
                        break;
                    case "48361213":
                        WeaponIdentifier = "Eleventh Hour";
                        break;
                    case "48643186":
                        WeaponIdentifier = "Ancient Gospel";
                        break;
                    case "53159280":
                        WeaponIdentifier = "Traveler's Chosen";
                        break;
                    case "53159281":
                        WeaponIdentifier = "Traveler's Chosen (Damaged)";
                        break;
                    case "64236626":
                        WeaponIdentifier = "Good Bone Structure";
                        break;
                    case "65611680":
                        WeaponIdentifier = "The Fool's Remedy";
                        break;
                    case "66875353":
                        WeaponIdentifier = "Avalanche";
                        break;
                    case "79075821":
                        WeaponIdentifier = "Drang (Baroque)";
                        break;
                    case "93253474":
                        WeaponIdentifier = "The Ringing Nail";
                        break;
                    case "94729174":
                        WeaponIdentifier = "Gunnora's Axe";
                        break;
                    case "105567493":
                        WeaponIdentifier = "Hard Truths";
                        break;
                    case "108221785":
                        WeaponIdentifier = "Riiswalker";
                        break;
                    case "136525518":
                        WeaponIdentifier = "Shining Sphere";
                        break;
                    case "137879537":
                        WeaponIdentifier = "Curtain Call";
                        break;
                    case "153979396":
                        WeaponIdentifier = "Luna's Howl";
                        break;
                    case "153979397":
                        WeaponIdentifier = "Better Devils";
                        break;
                    case "153979399":
                        WeaponIdentifier = "Not Forgotten";
                        break;
                    case "159056377":
                        WeaponIdentifier = "Requiem-45";
                        break;
                    case "161537636":
                        WeaponIdentifier = "Machina Dei 4";
                        break;
                    case "161537637":
                        WeaponIdentifier = "Infinite Paths 8";
                        break;
                    case "167561626":
                        WeaponIdentifier = "Bequest";
                        break;
                    case "174192097":
                        WeaponIdentifier = "CALUS Mini-Tool";
                        break;
                    case "174804902":
                        WeaponIdentifier = "The Long Walk";
                        break;
                    case "188882152":
                        WeaponIdentifier = "Last Perdition";
                        break;
                    case "191996029":
                        WeaponIdentifier = "Redrix's Claymore";
                        break;
                    case "195440257":
                        WeaponIdentifier = "Play of the Game";
                        break;
                    case "204878059":
                        WeaponIdentifier = "Malfeasance";
                        break;
                    case "208088207":
                        WeaponIdentifier = "Premonition";
                        break;
                    case "210065223":
                        WeaponIdentifier = "Magnum Shepherd";
                        break;
                    case "211938782":
                        WeaponIdentifier = "Whispering Slab";
                        break;
                    case "217140611":
                        WeaponIdentifier = "The Deicide";
                        break;
                    case "218335759":
                        WeaponIdentifier = "Edge Transit";
                        break;
                    case "227548887":
                        WeaponIdentifier = "Interregnum XVI";
                        break;
                    case "228424224":
                        WeaponIdentifier = "Impromptu-49";
                        break;
                    case "233423981":
                        WeaponIdentifier = "Warden's Law";
                        break;
                    case "253196586":
                        WeaponIdentifier = "Main Ingredient";
                        break;
                    case "254156943":
                        WeaponIdentifier = "Dead Zone Rifle";
                        break;
                    case "273396910":
                        WeaponIdentifier = "Frontier Justice";
                        break;
                    case "276080079":
                        WeaponIdentifier = "Exile's Curse (Adept)";
                        break;
                    case "276918162":
                        WeaponIdentifier = "Hagakure";
                        break;
                    case "287042892":
                        WeaponIdentifier = "Negative Space";
                        break;
                    case "287042893":
                        WeaponIdentifier = "Complex Solution";
                        break;
                    case "287042894":
                        WeaponIdentifier = "Unspoken Promise";
                        break;
                    case "293505772":
                        WeaponIdentifier = "The Vision";
                        break;
                    case "299665907":
                        WeaponIdentifier = "Outlast";
                        break;
                    case "301362380":
                        WeaponIdentifier = "Terran Wind";
                        break;
                    case "301362381":
                        WeaponIdentifier = "Through Fire and Flood";
                        break;
                    case "304659313":
                        WeaponIdentifier = "Ignition Code";
                        break;
                    case "308332265":
                        WeaponIdentifier = "Roar of the Bear";
                        break;
                    case "310708513":
                        WeaponIdentifier = "Survivor's Epitaph";
                        break;
                    case "311852248":
                        WeaponIdentifier = "Bushwhacker";
                        break;
                    case "324382200":
                        WeaponIdentifier = "Breakneck";
                        break;
                    case "325519402":
                        WeaponIdentifier = "Darkest Before";
                        break;
                    case "334171687":
                        WeaponIdentifier = "Waking Vigil";
                        break;
                    case "339163900":
                        WeaponIdentifier = "Nightshade";
                        break;
                    case "339343290":
                        WeaponIdentifier = "Nergal PR4";
                        break;
                    case "339343291":
                        WeaponIdentifier = "Cadenza-43";
                        break;
                    case "346136302":
                        WeaponIdentifier = "Retold Tale";
                        break;
                    case "347366834":
                        WeaponIdentifier = "Ace of Spades";
                        break;
                    case "370712896":
                        WeaponIdentifier = "Salvation's Grip";
                        break;
                    case "372212741":
                        WeaponIdentifier = "Weaver-C";
                        break;
                    case "394810086":
                        WeaponIdentifier = "Dream Breaker";
                        break;
                    case "400096939":
                        WeaponIdentifier = "Outbreak Perfected";
                        break;
                    case "407621213":
                        WeaponIdentifier = "Berenger's Memory";
                        break;
                    case "408440598":
                        WeaponIdentifier = "True Prophecy";
                        break;
                    case "409551876":
                        WeaponIdentifier = "The Keening";
                        break;
                    case "410996590":
                        WeaponIdentifier = "Jack Queen King 3";
                        break;
                    case "417164956":
                        WeaponIdentifier = "Jötunn";
                        break;
                    case "417474224":
                        WeaponIdentifier = "Hoosegow";
                        break;
                    case "417474225":
                        WeaponIdentifier = "Mos Epoch III";
                        break;
                    case "417474226":
                        WeaponIdentifier = "Blue Shift";
                        break;
                    case "421573768":
                        WeaponIdentifier = "The Spiteful Fang";
                        break;
                    case "432476743":
                        WeaponIdentifier = "The Palindrome";
                        break;
                    case "432716552":
                        WeaponIdentifier = "Shining Sphere";
                        break;
                    case "445523360":
                        WeaponIdentifier = "Fatebringer (Timelost)";
                        break;
                    case "460724140":
                        WeaponIdentifier = "The Jade Rabbit";
                        break;
                    case "468276817":
                        WeaponIdentifier = "Nature of the Beast";
                        break;
                    case "471518543":
                        WeaponIdentifier = "Corrective Measure";
                        break;
                    case "472169727":
                        WeaponIdentifier = "Garden Progeny 1";
                        break;
                    case "472847207":
                        WeaponIdentifier = "Guiding Star";
                        break;
                    case "481721032":
                        WeaponIdentifier = "Posterity";
                        break;
                    case "497806049":
                        WeaponIdentifier = "Praedyth's Revenge (Timelost)";
                        break;
                    case "506351160":
                        WeaponIdentifier = "Thermal Erosion";
                        break;
                    case "515224227":
                        WeaponIdentifier = "First In, Last Out";
                        break;
                    case "516243773":
                        WeaponIdentifier = "Beidenhander";
                        break;
                    case "525750263":
                        WeaponIdentifier = "Black Scorpion-4sr";
                        break;
                    case "528834068":
                        WeaponIdentifier = "BrayTech Werewolf";
                        break;
                    case "531591352":
                        WeaponIdentifier = "Forte-15";
                        break;
                    case "531591353":
                        WeaponIdentifier = "Daystar SMG2";
                        break;
                    case "532746994":
                        WeaponIdentifier = "Astral Horizon (Adept)";
                        break;
                    case "534775659":
                        WeaponIdentifier = "PLUG ONE.1 (Adept)";
                        break;
                    case "541053086":
                        WeaponIdentifier = "Telemachus-C";
                        break;
                    case "541188001":
                        WeaponIdentifier = "Farewell";
                        break;
                    case "546372301":
                        WeaponIdentifier = "The Jade Rabbit";
                        break;
                    case "566976652":
                        WeaponIdentifier = "Resonance-42";
                        break;
                    case "566976653":
                        WeaponIdentifier = "Antiope-D";
                        break;
                    case "566976654":
                        WeaponIdentifier = "Phosphorus MG4";
                        break;
                    case "568515759":
                        WeaponIdentifier = "Chattering Bone";
                        break;
                    case "569799273":
                        WeaponIdentifier = "Throne-Cleaver";
                        break;
                    case "569799274":
                        WeaponIdentifier = "Death's Razor";
                        break;
                    case "569799275":
                        WeaponIdentifier = "Goldtusk";
                        break;
                    case "578459533":
                        WeaponIdentifier = "Wendigo GL3";
                        break;
                    case "580961571":
                        WeaponIdentifier = "Loaded Question";
                        break;
                    case "582335600":
                        WeaponIdentifier = "Dire Promise";
                        break;
                    case "594789807":
                        WeaponIdentifier = "Biting Winds";
                        break;
                    case "597679579":
                        WeaponIdentifier = "One Small Step";
                        break;
                    case "599895591":
                        WeaponIdentifier = "Sojourner's Tale";
                        break;
                    case "601592879":
                        WeaponIdentifier = "Age-Old Bond";
                        break;
                    case "603242241":
                        WeaponIdentifier = "Hammerhead";
                        break;
                    case "603721696":
                        WeaponIdentifier = "Cryosthesia 77K";
                        break;
                    case "607191995":
                        WeaponIdentifier = "Hollow Words";
                        break;
                    case "614426548":
                        WeaponIdentifier = "Falling Guillotine";
                        break;
                    case "618554398":
                        WeaponIdentifier = "Proelium FR3";
                        break;
                    case "622058944":
                        WeaponIdentifier = "Jorum's Claw";
                        break;
                    case "625672050":
                        WeaponIdentifier = "Jian 7 Rifle";
                        break;
                    case "631439337":
                        WeaponIdentifier = "Found Verdict (Timelost)";
                        break;
                    case "636912560":
                        WeaponIdentifier = "Dust Rock Blues";
                        break;
                    case "640114618":
                        WeaponIdentifier = "Tigerspite";
                        break;
                    case "653875712":
                        WeaponIdentifier = "Ballyhoo Mk.27";
                        break;
                    case "653875713":
                        WeaponIdentifier = "Lamia HC2";
                        break;
                    case "653875714":
                        WeaponIdentifier = "Azimuth DSu";
                        break;
                    case "653875715":
                        WeaponIdentifier = "Allegro-34";
                        break;
                    case "654370424":
                        WeaponIdentifier = "Nation of Beasts";
                        break;
                    case "654608616":
                        WeaponIdentifier = "Revoker";
                        break;
                    case "679281855":
                        WeaponIdentifier = "Exile's Curse";
                        break;
                    case "681067419":
                        WeaponIdentifier = "Hung Jury SR4 (Adept)";
                        break;
                    case "686951703":
                        WeaponIdentifier = "The Supremacy";
                        break;
                    case "689453941":
                        WeaponIdentifier = "The Frigid Jackal";
                        break;
                    case "690668916":
                        WeaponIdentifier = "Vision of Confluence (Timelost)";
                        break;
                    case "695814388":
                        WeaponIdentifier = "Trust";
                        break;
                    case "701922966":
                        WeaponIdentifier = "Finite Impactor";
                        break;
                    case "705774642":
                        WeaponIdentifier = "Restoration VIII";
                        break;
                    case "711899772":
                        WeaponIdentifier = "Vinegaroon-2si";
                        break;
                    case "711899773":
                        WeaponIdentifier = "Roderic-C";
                        break;
                    case "711899774":
                        WeaponIdentifier = "Requiem SI2";
                        break;
                    case "711899775":
                        WeaponIdentifier = "Dissonance-34";
                        break;
                    case "715338174":
                        WeaponIdentifier = "Just in Case";
                        break;
                    case "717150101":
                        WeaponIdentifier = "BrayTech RWP Mk. II";
                        break;
                    case "720351794":
                        WeaponIdentifier = "No Turning Back";
                        break;
                    case "720351795":
                        WeaponIdentifier = "Arsenic Bite-4b";
                        break;
                    case "731147177":
                        WeaponIdentifier = "Hawthorne's Field-Forged Shotgun";
                        break;
                    case "731147178":
                        WeaponIdentifier = "Good Bone Structure";
                        break;
                    case "731147179":
                        WeaponIdentifier = "Baligant";
                        break;
                    case "736064515":
                        WeaponIdentifier = "Corrective Measure";
                        break;
                    case "736901634":
                        WeaponIdentifier = "Doomsday";
                        break;
                    case "755130877":
                        WeaponIdentifier = "Last Man Standing";
                        break;
                    case "766323545":
                        WeaponIdentifier = "Seventh Seraph VY-7";
                        break;
                    case "772531208":
                        WeaponIdentifier = "Etude-12";
                        break;
                    case "776191470":
                        WeaponIdentifier = "Tommy's Matchbook";
                        break;
                    case "792755504":
                        WeaponIdentifier = "Nightshade";
                        break;
                    case "795875974":
                        WeaponIdentifier = "Corrective Measure (Timelost)";
                        break;
                    case "805677041":
                        WeaponIdentifier = "Buzzard";
                        break;
                    case "806021398":
                        WeaponIdentifier = "Peace by Consensus";
                        break;
                    case "807192446":
                        WeaponIdentifier = "The Day's Fury";
                        break;
                    case "814876684":
                        WeaponIdentifier = "Wish-Ender";
                        break;
                    case "814876685":
                        WeaponIdentifier = "Trinity Ghoul";
                        break;
                    case "816113441":
                        WeaponIdentifier = "Fatebringer";
                        break;
                    case "819358961":
                        WeaponIdentifier = "Spoiler Alert";
                        break;
                    case "819441402":
                        WeaponIdentifier = "Misfit";
                        break;
                    case "821154603":
                        WeaponIdentifier = "Gnawing Hunger";
                        break;
                    case "834081972":
                        WeaponIdentifier = "Service Revolver";
                        break;
                    case "838556752":
                        WeaponIdentifier = "Python";
                        break;
                    case "847329160":
                        WeaponIdentifier = "Edgewise";
                        break;
                    case "847450546":
                        WeaponIdentifier = "IKELOS_SR_v1.0.1";
                        break;
                    case "852228780":
                        WeaponIdentifier = "Uzume RR4 (Adept)";
                        break;
                    case "852551895":
                        WeaponIdentifier = "Occluded Finality";
                        break;
                    case "875848769":
                        WeaponIdentifier = "Borrowed Time";
                        break;
                    case "893527433":
                        WeaponIdentifier = "Far Future";
                        break;
                    case "896923850":
                        WeaponIdentifier = "Acantha-D";
                        break;
                    case "905092001":
                        WeaponIdentifier = "Anniella";
                        break;
                    case "925326392":
                        WeaponIdentifier = "Tango-45";
                        break;
                    case "925326393":
                        WeaponIdentifier = "Manannan SR4";
                        break;
                    case "925326394":
                        WeaponIdentifier = "Black Scorpion-4sr";
                        break;
                    case "930590127":
                        WeaponIdentifier = "The Wizened Rebuke";
                        break;
                    case "946443267":
                        WeaponIdentifier = "Line in the Sand";
                        break;
                    case "958384347":
                        WeaponIdentifier = "Tomorrow's Answer";
                        break;
                    case "962412079":
                        WeaponIdentifier = "Last Perdition";
                        break;
                    case "981718087":
                        WeaponIdentifier = "Deafening Whisper";
                        break;
                    case "982229638":
                        WeaponIdentifier = "Allied Demand";
                        break;
                    case "990416096":
                        WeaponIdentifier = "Silicon Neuroma";
                        break;
                    case "991314988":
                        WeaponIdentifier = "Bad Omens";
                        break;
                    case "1006783454":
                        WeaponIdentifier = "Timelines' Vertex";
                        break;
                    case "1016668089":
                        WeaponIdentifier = "One Small Step";
                        break;
                    case "1018072983":
                        WeaponIdentifier = "It Stared Back";
                        break;
                    case "1018777295":
                        WeaponIdentifier = "Motion to Vacate";
                        break;
                    case "1030895163":
                        WeaponIdentifier = "Glacioclasm";
                        break;
                    case "1046651176":
                        WeaponIdentifier = "Bottom Dollar";
                        break;
                    case "1048266744":
                        WeaponIdentifier = "Better Devils";
                        break;
                    case "1071542914":
                        WeaponIdentifier = "Horror's Least";
                        break;
                    case "1084788061":
                        WeaponIdentifier = "Swift Solstice";
                        break;
                    case "1096206669":
                        WeaponIdentifier = "IKELOS_SG_v1.0.2";
                        break;
                    case "1097616550":
                        WeaponIdentifier = "Extraordinary Rendition";
                        break;
                    case "1099433612":
                        WeaponIdentifier = "The Doubt";
                        break;
                    case "1115104187":
                        WeaponIdentifier = "Sole Survivor";
                        break;
                    case "1119734784":
                        WeaponIdentifier = "Chroma Rush";
                        break;
                    case "1120843238":
                        WeaponIdentifier = "Plemusa-B";
                        break;
                    case "1120843239":
                        WeaponIdentifier = "Stampede Mk.32";
                        break;
                    case "1127995826":
                        WeaponIdentifier = "Bonechiller";
                        break;
                    case "1128225405":
                        WeaponIdentifier = "Midnight Coup";
                        break;
                    case "1137768695":
                        WeaponIdentifier = "Foregone Conclusion";
                        break;
                    case "1159252500":
                        WeaponIdentifier = "Vacuna SR4";
                        break;
                    case "1161276682":
                        WeaponIdentifier = "Redrix's Broadsword";
                        break;
                    case "1162247618":
                        WeaponIdentifier = "Jian 7 Rifle";
                        break;
                    case "1167153950":
                        WeaponIdentifier = "Adhortative";
                        break;
                    case "1170891373":
                        WeaponIdentifier = "Tranquility";
                        break;
                    case "1173780905":
                        WeaponIdentifier = "The Messenger (Adept)";
                        break;
                    case "1177293325":
                        WeaponIdentifier = "Tongeren-LR3";
                        break;
                    case "1177293326":
                        WeaponIdentifier = "Damietta-LR2";
                        break;
                    case "1177293327":
                        WeaponIdentifier = "Aachen-LR2";
                        break;
                    case "1178397318":
                        WeaponIdentifier = "Agrona PR4";
                        break;
                    case "1178397319":
                        WeaponIdentifier = "Battle Scar";
                        break;
                    case "1178886909":
                        WeaponIdentifier = "Thin Line";
                        break;
                    case "1179141605":
                        WeaponIdentifier = "Felwinter's Lie";
                        break;
                    case "1180270692":
                        WeaponIdentifier = "Quickfang";
                        break;
                    case "1180270693":
                        WeaponIdentifier = "Eternity's Edge";
                        break;
                    case "1180270694":
                        WeaponIdentifier = "Crown-Splitter";
                        break;
                    case "1187594590":
                        WeaponIdentifier = "Relentless";
                        break;
                    case "1189790632":
                        WeaponIdentifier = "The Steady Hand";
                        break;
                    case "1195725817":
                        WeaponIdentifier = "Sondok-C";
                        break;
                    case "1195725818":
                        WeaponIdentifier = "Whip Scorpion-3mg";
                        break;
                    case "1195725819":
                        WeaponIdentifier = "Sorrow MG2";
                        break;
                    case "1197486957":
                        WeaponIdentifier = "High Albedo";
                        break;
                    case "1200414607":
                        WeaponIdentifier = "The Showrunner";
                        break;
                    case "1200824700":
                        WeaponIdentifier = "IKELOS_HC_v1.0.2";
                        break;
                    case "1201830623":
                        WeaponIdentifier = "Truth";
                        break;
                    case "1216130969":
                        WeaponIdentifier = "Cold Denial";
                        break;
                    case "1216319404":
                        WeaponIdentifier = "Fatebringer (Timelost)";
                        break;
                    case "1220931350":
                        WeaponIdentifier = "A Fine Memorial";
                        break;
                    case "1230392361":
                        WeaponIdentifier = "Heritage";
                        break;
                    case "1251729046":
                        WeaponIdentifier = "Steelfeather Repeater";
                        break;
                    case "1253087083":
                        WeaponIdentifier = "IKELOS_SR_v1.0.2";
                        break;
                    case "1270948323":
                        WeaponIdentifier = "Nasreddin";
                        break;
                    case "1271343896":
                        WeaponIdentifier = "Widow's Bite";
                        break;
                    case "1277015089":
                        WeaponIdentifier = "Gravity Slingshot";
                        break;
                    case "1280933460":
                        WeaponIdentifier = "Claws of the Wolf";
                        break;
                    case "1281822856":
                        WeaponIdentifier = "Protostar CSu";
                        break;
                    case "1281822857":
                        WeaponIdentifier = "Philippis-B";
                        break;
                    case "1281822858":
                        WeaponIdentifier = "Furina-2mg";
                        break;
                    case "1281822859":
                        WeaponIdentifier = "Harmony-21";
                        break;
                    case "1286686760":
                        WeaponIdentifier = "Gahlran's Right Hand";
                        break;
                    case "1289000550":
                        WeaponIdentifier = "PLUG ONE.1";
                        break;
                    case "1289324202":
                        WeaponIdentifier = "Pyroclastic Flow";
                        break;
                    case "1289997971":
                        WeaponIdentifier = "Breachlight";
                        break;
                    case "1291586825":
                        WeaponIdentifier = "Eystein-D";
                        break;
                    case "1310413524":
                        WeaponIdentifier = "Recital-17";
                        break;
                    case "1310413525":
                        WeaponIdentifier = "Victoire SI2";
                        break;
                    case "1313528549":
                        WeaponIdentifier = "Sola's Scar";
                        break;
                    case "1325579289":
                        WeaponIdentifier = "Retrofuturist";
                        break;
                    case "1327264046":
                        WeaponIdentifier = "Badlander";
                        break;
                    case "1327432221":
                        WeaponIdentifier = "Perseverance";
                        break;
                    case "1331482397":
                        WeaponIdentifier = "MIDA Multi-Tool";
                        break;
                    case "1339362514":
                        WeaponIdentifier = "Stochastic Variable";
                        break;
                    case "1341880512":
                        WeaponIdentifier = "Love and Death";
                        break;
                    case "1342668638":
                        WeaponIdentifier = "Pleiades Corrector";
                        break;
                    case "1345867570":
                        WeaponIdentifier = "Sweet Business";
                        break;
                    case "1345867571":
                        WeaponIdentifier = "Coldheart";
                        break;
                    case "1350102270":
                        WeaponIdentifier = "Niflheim Frost";
                        break;
                    case "1351035691":
                        WeaponIdentifier = "Daedalus Code";
                        break;
                    case "1355744825":
                        WeaponIdentifier = "Vision of Confluence";
                        break;
                    case "1357080535":
                        WeaponIdentifier = "Breath of the Dragon";
                        break;
                    case "1363238943":
                        WeaponIdentifier = "Ruinous Effigy";
                        break;
                    case "1364093401":
                        WeaponIdentifier = "The Last Word";
                        break;
                    case "1366917989":
                        WeaponIdentifier = "Tomorrow's Answer (Adept)";
                        break;
                    case "1369487074":
                        WeaponIdentifier = "Orimund's Anvil";
                        break;
                    case "1386601612":
                        WeaponIdentifier = "Swift Ride XE8375";
                        break;
                    case "1392429335":
                        WeaponIdentifier = "Broadsword Launcher";
                        break;
                    case "1392919471":
                        WeaponIdentifier = "Trustee";
                        break;
                    case "1393021133":
                        WeaponIdentifier = "Equinox Tsu";
                        break;
                    case "1393021134":
                        WeaponIdentifier = "Nox Reve II";
                        break;
                    case "1393021135":
                        WeaponIdentifier = "Nox Calyx II";
                        break;
                    case "1395261499":
                        WeaponIdentifier = "Xenophage";
                        break;
                    case "1402766122":
                        WeaponIdentifier = "Retrofuturist";
                        break;
                    case "1406475890":
                        WeaponIdentifier = "Agamid";
                        break;
                    case "1411084669":
                        WeaponIdentifier = "Zenobia-D";
                        break;
                    case "1443049976":
                        WeaponIdentifier = "Interference VI";
                        break;
                    case "1447973650":
                        WeaponIdentifier = "Rest for the Wicked";
                        break;
                    case "1447973651":
                        WeaponIdentifier = "Future Imperfect";
                        break;
                    case "1449922174":
                        WeaponIdentifier = "Tatara Gaze";
                        break;
                    case "1457394908":
                        WeaponIdentifier = "Fussed Dark Mk.21";
                        break;
                    case "1457394910":
                        WeaponIdentifier = "Botheration Mk.28";
                        break;
                    case "1457394911":
                        WeaponIdentifier = "Badlands Mk.24";
                        break;
                    case "1457979868":
                        WeaponIdentifier = "Duty Bound";
                        break;
                    case "1459443448":
                        WeaponIdentifier = "Escape Velocity";
                        break;
                    case "1481892490":
                        WeaponIdentifier = "The Palindrome (Adept)";
                        break;
                    case "1489452902":
                        WeaponIdentifier = "Courageous Surrender";
                        break;
                    case "1490571337":
                        WeaponIdentifier = "Future Safe 10";
                        break;
                    case "1496419775":
                        WeaponIdentifier = "Bane of Sorrow";
                        break;
                    case "1502662697":
                        WeaponIdentifier = "King Cobra-4fr";
                        break;
                    case "1503609584":
                        WeaponIdentifier = "The Last Breath";
                        break;
                    case "1506719573":
                        WeaponIdentifier = "Cold Front";
                        break;
                    case "1508896098":
                        WeaponIdentifier = "The Wardcliff Coil";
                        break;
                    case "1513927136":
                        WeaponIdentifier = "Swift Ride";
                        break;
                    case "1513927137":
                        WeaponIdentifier = "Disrespectful Stare";
                        break;
                    case "1513927138":
                        WeaponIdentifier = "Lincoln Green";
                        break;
                    case "1513927139":
                        WeaponIdentifier = "Agenda 5";
                        break;
                    case "1513993763":
                        WeaponIdentifier = "Friction Fire";
                        break;
                    case "1518042134":
                        WeaponIdentifier = "Halfdan-D";
                        break;
                    case "1518042135":
                        WeaponIdentifier = "Valakadyn";
                        break;
                    case "1523647826":
                        WeaponIdentifier = "Eternal Slumber";
                        break;
                    case "1529450902":
                        WeaponIdentifier = "Mos Epoch III";
                        break;
                    case "1531295694":
                        WeaponIdentifier = "Adverse Possession IX";
                        break;
                    case "1533499360":
                        WeaponIdentifier = "Athelflad-D";
                        break;
                    case "1533499361":
                        WeaponIdentifier = "Etana SI4";
                        break;
                    case "1533499362":
                        WeaponIdentifier = "Urchin-3si";
                        break;
                    case "1538967359":
                        WeaponIdentifier = "Ticuu's Divination";
                        break;
                    case "1541131350":
                        WeaponIdentifier = "Cerberus+1";
                        break;
                    case "1561006927":
                        WeaponIdentifier = "Seventh Seraph Carbine";
                        break;
                    case "1584643826":
                        WeaponIdentifier = "Hush";
                        break;
                    case "1587439031":
                        WeaponIdentifier = "Three Graves";
                        break;
                    case "1587779165":
                        WeaponIdentifier = "Radiant Stardust";
                        break;
                    case "1594120904":
                        WeaponIdentifier = "No Time to Explain";
                        break;
                    case "1595336070":
                        WeaponIdentifier = "Guseva-C";
                        break;
                    case "1595336071":
                        WeaponIdentifier = "Presto-48";
                        break;
                    case "1600633250":
                        WeaponIdentifier = "21% Delirium";
                        break;
                    case "1619016919":
                        WeaponIdentifier = "Khvostov 7G-02";
                        break;
                    case "1621558458":
                        WeaponIdentifier = "Gridskipper";
                        break;
                    case "1621657423":
                        WeaponIdentifier = "Biting Winds";
                        break;
                    case "1625995655":
                        WeaponIdentifier = "Succession";
                        break;
                    case "1641430382":
                        WeaponIdentifier = "The Guiding Sight";
                        break;
                    case "1642384931":
                        WeaponIdentifier = "Fixed Odds";
                        break;
                    case "1644160541":
                        WeaponIdentifier = "Abide the Return";
                        break;
                    case "1644162710":
                        WeaponIdentifier = "Origin Story";
                        break;
                    case "1644680957":
                        WeaponIdentifier = "Null Composure";
                        break;
                    case "1645386487":
                        WeaponIdentifier = "Tranquility";
                        break;
                    case "1648316470":
                        WeaponIdentifier = "Timecard";
                        break;
                    case "1650442173":
                        WeaponIdentifier = "Loquitor IV";
                        break;
                    case "1650626964":
                        WeaponIdentifier = "Armillary PSu";
                        break;
                    case "1650626965":
                        WeaponIdentifier = "Black Tiger-2sr";
                        break;
                    case "1650626966":
                        WeaponIdentifier = "Trax Lysis II";
                        break;
                    case "1650626967":
                        WeaponIdentifier = "Madrugada SR2";
                        break;
                    case "1664372054":
                        WeaponIdentifier = "Threat Level";
                        break;
                    case "1665952087":
                        WeaponIdentifier = "The Fourth Horseman";
                        break;
                    case "1669771780":
                        WeaponIdentifier = "Encore-25";
                        break;
                    case "1669771781":
                        WeaponIdentifier = "Agrona PR2";
                        break;
                    case "1669771782":
                        WeaponIdentifier = "Psi Cirrus II";
                        break;
                    case "1669771783":
                        WeaponIdentifier = "Bayesian MSu";
                        break;
                    case "1674742470":
                        WeaponIdentifier = "Autumn Wind";
                        break;
                    case "1678957656":
                        WeaponIdentifier = "Bayesian MSu";
                        break;
                    case "1678957657":
                        WeaponIdentifier = "Psi Cirrus II";
                        break;
                    case "1678957658":
                        WeaponIdentifier = "Agrona PR2";
                        break;
                    case "1678957659":
                        WeaponIdentifier = "Encore-25";
                        break;
                    case "1684914716":
                        WeaponIdentifier = "Fate Cries Foul";
                        break;
                    case "1688544136":
                        WeaponIdentifier = "Vision of Confluence (Timelost)";
                        break;
                    case "1690783811":
                        WeaponIdentifier = "The Forward Path";
                        break;
                    case "1697682876":
                        WeaponIdentifier = "Astral Horizon";
                        break;
                    case "1699493316":
                        WeaponIdentifier = "The Last Dance";
                        break;
                    case "1702618248":
                        WeaponIdentifier = "Trust";
                        break;
                    case "1706206669":
                        WeaponIdentifier = "Gallant Charge";
                        break;
                    case "1706536806":
                        WeaponIdentifier = "The Old Fashioned";
                        break;
                    case "1720373217":
                        WeaponIdentifier = "The Permanent Truth";
                        break;
                    case "1723380073":
                        WeaponIdentifier = "Enigma's Draw";
                        break;
                    case "1723472487":
                        WeaponIdentifier = "IKELOS_SMG_v1.0.1";
                        break;
                    case "1738026524":
                        WeaponIdentifier = "Hailing Confusion";
                        break;
                    case "1744115122":
                        WeaponIdentifier = "Legend of Acrius";
                        break;
                    case "1752585070":
                        WeaponIdentifier = "BrayTech Winter Wolf";
                        break;
                    case "1757129747":
                        WeaponIdentifier = "Acantha-D XK8434";
                        break;
                    case "1760543913":
                        WeaponIdentifier = "Legal Action II";
                        break;
                    case "1766088024":
                        WeaponIdentifier = "Thermal Erosion";
                        break;
                    case "1773600468":
                        WeaponIdentifier = "Critical Sass";
                        break;
                    case "1775804198":
                        WeaponIdentifier = "Galliard-42 XN7568";
                        break;
                    case "1786797708":
                        WeaponIdentifier = "Escape Velocity";
                        break;
                    case "1789347249":
                        WeaponIdentifier = "Hazard of the Cast";
                        break;
                    case "1798874854":
                        WeaponIdentifier = "18 Kelvins";
                        break;
                    case "1807343361":
                        WeaponIdentifier = "Hawthorne's Field-Forged Shotgun";
                        break;
                    case "1813667283":
                        WeaponIdentifier = "Dead-Ender";
                        break;
                    case "1821724780":
                        WeaponIdentifier = "Seventh Seraph CQC-12";
                        break;
                    case "1825472717":
                        WeaponIdentifier = "The End";
                        break;
                    case "1833744687":
                        WeaponIdentifier = "Trust";
                        break;
                    case "1835747805":
                        WeaponIdentifier = "Nature of the Beast";
                        break;
                    case "1839565992":
                        WeaponIdentifier = "Ether Doctor";
                        break;
                    case "1842303080":
                        WeaponIdentifier = "Hollow Earth";
                        break;
                    case "1843044398":
                        WeaponIdentifier = "Translation Theory";
                        break;
                    case "1843044399":
                        WeaponIdentifier = "Smuggler's Word";
                        break;
                    case "1852863732":
                        WeaponIdentifier = "Wavesplitter";
                        break;
                    case "1853180924":
                        WeaponIdentifier = "Traveler's Chosen";
                        break;
                    case "1864563948":
                        WeaponIdentifier = "Worldline Zero";
                        break;
                    case "1865351684":
                        WeaponIdentifier = "The Hero's Burden";
                        break;
                    case "1870979911":
                        WeaponIdentifier = "Orewing's Maul";
                        break;
                    case "1873270090":
                        WeaponIdentifier = "Elegy-49";
                        break;
                    case "1877183764":
                        WeaponIdentifier = "Reginar-B";
                        break;
                    case "1877183765":
                        WeaponIdentifier = "Cup-Bearer SA/2";
                        break;
                    case "1879212552":
                        WeaponIdentifier = "A Sudden Death";
                        break;
                    case "1885753220":
                        WeaponIdentifier = "Seven-Six-Five";
                        break;
                    case "1885753222":
                        WeaponIdentifier = "Call to Serve";
                        break;
                    case "1885753223":
                        WeaponIdentifier = "Tone Patrol";
                        break;
                    case "1887808042":
                        WeaponIdentifier = "IKELOS_SG_v1.0.1";
                        break;
                    case "1891561814":
                        WeaponIdentifier = "Whisper of the Worm";
                        break;
                    case "1907698332":
                        WeaponIdentifier = "The Summoner";
                        break;
                    case "1909527966":
                        WeaponIdentifier = "Prosecutor";
                        break;
                    case "1911843788":
                        WeaponIdentifier = "The Rattler";
                        break;
                    case "1911843789":
                        WeaponIdentifier = "Minimum Distance";
                        break;
                    case "1911843790":
                        WeaponIdentifier = "Dead Man Walking";
                        break;
                    case "1911843791":
                        WeaponIdentifier = "Last Hope";
                        break;
                    case "1921159786":
                        WeaponIdentifier = "Hezen Vengeance (Timelost)";
                        break;
                    case "1927800278":
                        WeaponIdentifier = "Eternal Blazon";
                        break;
                    case "1929278169":
                        WeaponIdentifier = "BrayTech Osprey";
                        break;
                    case "1931556011":
                        WeaponIdentifier = "No Feelings";
                        break;
                    case "1937185139":
                        WeaponIdentifier = "High Albedo";
                        break;
                    case "1940885628":
                        WeaponIdentifier = "Archimedes Truth";
                        break;
                    case "1942069133":
                        WeaponIdentifier = "Dark Decider";
                        break;
                    case "1946491241":
                        WeaponIdentifier = "Truthteller";
                        break;
                    case "1952163498":
                        WeaponIdentifier = "Pluperfect";
                        break;
                    case "1960218487":
                        WeaponIdentifier = "Nameless Midnight";
                        break;
                    case "1967303408":
                        WeaponIdentifier = "Archon's Thunder";
                        break;
                    case "1972985595":
                        WeaponIdentifier = "Swarm of the Raven";
                        break;
                    case "1977926913":
                        WeaponIdentifier = "Stubborn Oak";
                        break;
                    case "1982711279":
                        WeaponIdentifier = "Talons of the Eagle";
                        break;
                    case "1983332560":
                        WeaponIdentifier = "Flash and Thunder";
                        break;
                    case "1983332561":
                        WeaponIdentifier = "Orthrus";
                        break;
                    case "1983332562":
                        WeaponIdentifier = "Berenger's Memory";
                        break;
                    case "1987769101":
                        WeaponIdentifier = "Praedyth's Revenge (Timelost)";
                        break;
                    case "1988218406":
                        WeaponIdentifier = "Unification VII";
                        break;
                    case "1995011456":
                        WeaponIdentifier = "Badlands Mk.24";
                        break;
                    case "1995011457":
                        WeaponIdentifier = "Botheration Mk.28";
                        break;
                    case "1995011459":
                        WeaponIdentifier = "Fussed Dark Mk.21";
                        break;
                    case "2009106091":
                        WeaponIdentifier = "The Vow";
                        break;
                    case "2009277538":
                        WeaponIdentifier = "The Last Dance";
                        break;
                    case "2014642399":
                        WeaponIdentifier = "The Forward Path";
                        break;
                    case "2034817450":
                        WeaponIdentifier = "Distant Relation";
                        break;
                    case "2037589099":
                        WeaponIdentifier = "Butler RS/2";
                        break;
                    case "2044500762":
                        WeaponIdentifier = "The Queenbreaker";
                        break;
                    case "2050789284":
                        WeaponIdentifier = "Stars in Shadow";
                        break;
                    case "2056591688":
                        WeaponIdentifier = "Praedyth's Revenge";
                        break;
                    case "2060863616":
                        WeaponIdentifier = "Salvager's Salvo";
                        break;
                    case "2065081837":
                        WeaponIdentifier = "Uzume RR4";
                        break;
                    case "2069224589":
                        WeaponIdentifier = "One Thousand Voices";
                        break;
                    case "2069412054":
                        WeaponIdentifier = "Subzero Salvo";
                        break;
                    case "2071412133":
                        WeaponIdentifier = "A Cold Sweat";
                        break;
                    case "2084611899":
                        WeaponIdentifier = "Last of the Legion";
                        break;
                    case "2084878005":
                        WeaponIdentifier = "Heir Apparent";
                        break;
                    case "2091737595":
                        WeaponIdentifier = "Traveler's Judgment 5";
                        break;
                    case "2094938673":
                        WeaponIdentifier = "Adjudicator";
                        break;
                    case "2105827099":
                        WeaponIdentifier = "Bad Reputation";
                        break;
                    case "2108920981":
                        WeaponIdentifier = "Orewing's Maul";
                        break;
                    case "2112909414":
                        WeaponIdentifier = "Duke Mk. 44";
                        break;
                    case "2112909415":
                        WeaponIdentifier = "Ten Paces";
                        break;
                    case "2121785039":
                        WeaponIdentifier = "Brass Attacks";
                        break;
                    case "2124893005":
                        WeaponIdentifier = "Found Verdict (Timelost)";
                        break;
                    case "2130065553":
                        WeaponIdentifier = "Arbalest";
                        break;
                    case "2138599001":
                        WeaponIdentifier = "Optative";
                        break;
                    case "2145476620":
                        WeaponIdentifier = "Bad News";
                        break;
                    case "2145476622":
                        WeaponIdentifier = "Shattered Peace";
                        break;
                    case "2145476623":
                        WeaponIdentifier = "Annual Skate";
                        break;
                    case "2146650065":
                        WeaponIdentifier = "Prometheus Lens";
                        break;
                    case "2147010335":
                        WeaponIdentifier = "Shadow Price (Adept)";
                        break;
                    case "2149166938":
                        WeaponIdentifier = "Classical-42";
                        break;
                    case "2149166939":
                        WeaponIdentifier = "Countess SA/2";
                        break;
                    case "2154059444":
                        WeaponIdentifier = "The Long Goodbye";
                        break;
                    case "2164448701":
                        WeaponIdentifier = "Apostate";
                        break;
                    case "2168486467":
                        WeaponIdentifier = "Wicked Sister";
                        break;
                    case "2171006181":
                        WeaponIdentifier = "Service Revolver";
                        break;
                    case "2171478765":
                        WeaponIdentifier = "Fatebringer";
                        break;
                    case "2186258845":
                        WeaponIdentifier = "Bellowing Giant";
                        break;
                    case "2199171672":
                        WeaponIdentifier = "Lonesome";
                        break;
                    case "2208405142":
                        WeaponIdentifier = "Telesto";
                        break;
                    case "2209003210":
                        WeaponIdentifier = "Zealot's Reward";
                        break;
                    case "2209451511":
                        WeaponIdentifier = "Pariah";
                        break;
                    case "2213848860":
                        WeaponIdentifier = "Psi Termina II";
                        break;
                    case "2213848861":
                        WeaponIdentifier = "Cadenza-11";
                        break;
                    case "2213848862":
                        WeaponIdentifier = "Standing Tall";
                        break;
                    case "2213848863":
                        WeaponIdentifier = "Psi Ferox II";
                        break;
                    case "2217366863":
                        WeaponIdentifier = "Parcel of Stardust";
                        break;
                    case "2220884262":
                        WeaponIdentifier = "The Steady Hand";
                        break;
                    case "2222560548":
                        WeaponIdentifier = "IKELOS_SMG_v1.0.2";
                        break;
                    case "2232171099":
                        WeaponIdentifier = "Deathbringer";
                        break;
                    case "2248667690":
                        WeaponIdentifier = "Perfect Paradox";
                        break;
                    case "2251716886":
                        WeaponIdentifier = "Qingming Offering";
                        break;
                    case "2257180473":
                        WeaponIdentifier = "Interference VI";
                        break;
                    case "2272470786":
                        WeaponIdentifier = "Stochastic Variable";
                        break;
                    case "2276266837":
                        WeaponIdentifier = "Honor's Edge";
                        break;
                    case "2278995296":
                        WeaponIdentifier = "Does Not Compute";
                        break;
                    case "2286143274":
                        WeaponIdentifier = "The Huckleberry";
                        break;
                    case "2288545809":
                        WeaponIdentifier = "Subzero Salvo";
                        break;
                    case "2290863050":
                        WeaponIdentifier = "Persuader";
                        break;
                    case "2295941920":
                        WeaponIdentifier = "Belfry Bounty";
                        break;
                    case "2295941921":
                        WeaponIdentifier = "Maestro-46";
                        break;
                    case "2313489324":
                        WeaponIdentifier = "Trustee";
                        break;
                    case "2314999489":
                        WeaponIdentifier = "Imperative";
                        break;
                    case "2319004329":
                        WeaponIdentifier = "Trust";
                        break;
                    case "2326716489":
                        WeaponIdentifier = "Gunnora's Axe";
                        break;
                    case "2338088853":
                        WeaponIdentifier = "Calusea Noblesse";
                        break;
                    case "2351180975":
                        WeaponIdentifier = "Igneous Hammer";
                        break;
                    case "2351747816":
                        WeaponIdentifier = "Cuboid ARu";
                        break;
                    case "2351747817":
                        WeaponIdentifier = "Refrain-23";
                        break;
                    case "2351747818":
                        WeaponIdentifier = "Sand Wasp-3au";
                        break;
                    case "2351747819":
                        WeaponIdentifier = "Ros Lysis II";
                        break;
                    case "2357297366":
                        WeaponIdentifier = "Witherhoard";
                        break;
                    case "2359504847":
                        WeaponIdentifier = "Long Shadow";
                        break;
                    case "2362471600":
                        WeaponIdentifier = "Drang";
                        break;
                    case "2362471601":
                        WeaponIdentifier = "Rat King";
                        break;
                    case "2376481550":
                        WeaponIdentifier = "Anarchy";
                        break;
                    case "2386979999":
                        WeaponIdentifier = "The Scholar (Adept)";
                        break;
                    case "2398848320":
                        WeaponIdentifier = "Erentil FR4";
                        break;
                    case "2399110176":
                        WeaponIdentifier = "Eyes of Tomorrow";
                        break;
                    case "2408405461":
                        WeaponIdentifier = "Sacred Provenance";
                        break;
                    case "2414141462":
                        WeaponIdentifier = "The Vision";
                        break;
                    case "2414612776":
                        WeaponIdentifier = "New City";
                        break;
                    case "2414612777":
                        WeaponIdentifier = "Atalanta-D";
                        break;
                    case "2415517654":
                        WeaponIdentifier = "Bastion";
                        break;
                    case "2422664927":
                        WeaponIdentifier = "Atalanta-D XG1992";
                        break;
                    case "2429822976":
                        WeaponIdentifier = "Rose";
                        break;
                    case "2429822977":
                        WeaponIdentifier = "Austringer";
                        break;
                    case "2433826056":
                        WeaponIdentifier = "The Quickstep";
                        break;
                    case "2434225986":
                        WeaponIdentifier = "Shattered Cipher";
                        break;
                    case "2448907086":
                        WeaponIdentifier = "Royal Entry";
                        break;
                    case "2453357042":
                        WeaponIdentifier = "Blast Battue";
                        break;
                    case "2478247171":
                        WeaponIdentifier = "Quitclaim Shotgun III";
                        break;
                    case "2478792241":
                        WeaponIdentifier = "The Scholar";
                        break;
                    case "2481356995":
                        WeaponIdentifier = "Eriana's Vow";
                        break;
                    case "2481881293":
                        WeaponIdentifier = "Cartesian Coordinate";
                        break;
                    case "2482526262":
                        WeaponIdentifier = "Trust";
                        break;
                    case "2492081469":
                        WeaponIdentifier = "The Number";
                        break;
                    case "2496242052":
                        WeaponIdentifier = "Code Duello";
                        break;
                    case "2502422772":
                        WeaponIdentifier = "Cartesian Coordinate";
                        break;
                    case "2502422773":
                        WeaponIdentifier = "Shock and Awe";
                        break;
                    case "2502422774":
                        WeaponIdentifier = "Nox Echo III";
                        break;
                    case "2502422775":
                        WeaponIdentifier = "Tarantula";
                        break;
                    case "2505533224":
                        WeaponIdentifier = "Ghost Primus";
                        break;
                    case "2516360525":
                        WeaponIdentifier = "Purpose";
                        break;
                    case "2517599010":
                        WeaponIdentifier = "Death Adder";
                        break;
                    case "2522817335":
                        WeaponIdentifier = "Witherhoard";
                        break;
                    case "2527666306":
                        WeaponIdentifier = "Igneous Hammer (Adept)";
                        break;
                    case "2535939781":
                        WeaponIdentifier = "The Mornin' Comes";
                        break;
                    case "2544285846":
                        WeaponIdentifier = "Scipio-D";
                        break;
                    case "2545083870":
                        WeaponIdentifier = "Apex Predator";
                        break;
                    case "2553946496":
                        WeaponIdentifier = "Headstrong";
                        break;
                    case "2553946497":
                        WeaponIdentifier = "Helios HC1";
                        break;
                    case "2561659919":
                        WeaponIdentifier = "Antiope-D";
                        break;
                    case "2575506895":
                        WeaponIdentifier = "Kindled Orchid";
                        break;
                    case "2581162758":
                        WeaponIdentifier = "Enigma's Draw";
                        break;
                    case "2582755344":
                        WeaponIdentifier = "Seventh Seraph SAW";
                        break;
                    case "2591586260":
                        WeaponIdentifier = "Allegro-34";
                        break;
                    case "2591586261":
                        WeaponIdentifier = "Azimuth DSu";
                        break;
                    case "2591586262":
                        WeaponIdentifier = "Lamia HC2";
                        break;
                    case "2591586263":
                        WeaponIdentifier = "Ballyhoo Mk.27";
                        break;
                    case "2591746970":
                        WeaponIdentifier = "Leviathan's Breath";
                        break;
                    case "2603483885":
                        WeaponIdentifier = "Cloudstrike";
                        break;
                    case "2605790032":
                        WeaponIdentifier = "Troubadour";
                        break;
                    case "2605790033":
                        WeaponIdentifier = "Luna Nullis II";
                        break;
                    case "2605790034":
                        WeaponIdentifier = "Trondheim-LR2";
                        break;
                    case "2611861926":
                        WeaponIdentifier = "Imminent Storm";
                        break;
                    case "2614062688":
                        WeaponIdentifier = "Eriana's Vow";
                        break;
                    case "2617963291":
                        WeaponIdentifier = "Symmetry";
                        break;
                    case "2621637518":
                        WeaponIdentifier = "Play of the Game";
                        break;
                    case "2625782212":
                        WeaponIdentifier = "Haunted Earth";
                        break;
                    case "2625782213":
                        WeaponIdentifier = "Contingency Plan";
                        break;
                    case "2633186522":
                        WeaponIdentifier = "Shadow Price";
                        break;
                    case "2639743396":
                        WeaponIdentifier = "Duality";
                        break;
                    case "2650944612":
                        WeaponIdentifier = "Trust";
                        break;
                    case "2653316158":
                        WeaponIdentifier = "Pillager";
                        break;
                    case "2660862359":
                        WeaponIdentifier = "Gentleman Vagabond";
                        break;
                    case "2663204025":
                        WeaponIdentifier = "Subjunctive";
                        break;
                    case "2681395357":
                        WeaponIdentifier = "Trackless Waste";
                        break;
                    case "2683682446":
                        WeaponIdentifier = "Traitor's Fate";
                        break;
                    case "2683682447":
                        WeaponIdentifier = "Traitor's Fate";
                        break;
                    case "2693941407":
                        WeaponIdentifier = "Older Sister III";
                        break;
                    case "2694044460":
                        WeaponIdentifier = "Home Again";
                        break;
                    case "2694044461":
                        WeaponIdentifier = "Cydonia-AR1";
                        break;
                    case "2694044462":
                        WeaponIdentifier = "SUROS Throwback";
                        break;
                    case "2694044463":
                        WeaponIdentifier = "Jiangshi AR1";
                        break;
                    case "2694576561":
                        WeaponIdentifier = "Two-Tailed Fox";
                        break;
                    case "2697058914":
                        WeaponIdentifier = "Komodo-4FR";
                        break;
                    case "2700862856":
                        WeaponIdentifier = "Foggy Notion";
                        break;
                    case "2700862858":
                        WeaponIdentifier = "Out of Options";
                        break;
                    case "2700862859":
                        WeaponIdentifier = "Red Mamba";
                        break;
                    case "2703340117":
                        WeaponIdentifier = "Somerled-D";
                        break;
                    case "2703446873":
                        WeaponIdentifier = "Trust";
                        break;
                    case "2707464805":
                        WeaponIdentifier = "Zenith of Your Kind";
                        break;
                    case "2712244741":
                        WeaponIdentifier = "Bygones";
                        break;
                    case "2714022207":
                        WeaponIdentifier = "Corsair's Wrath";
                        break;
                    case "2721249463":
                        WeaponIdentifier = "Tyranny of Heaven";
                        break;
                    case "2723241847":
                        WeaponIdentifier = "Patron of Lost Causes";
                        break;
                    case "2723909519":
                        WeaponIdentifier = "Arc Logic";
                        break;
                    case "2734369894":
                        WeaponIdentifier = "Stay Away";
                        break;
                    case "2738174948":
                        WeaponIdentifier = "Distant Tumulus";
                        break;
                    case "2742490609":
                        WeaponIdentifier = "Death Adder";
                        break;
                    case "2742838700":
                        WeaponIdentifier = "True Prophecy";
                        break;
                    case "2742838701":
                        WeaponIdentifier = "Dire Promise";
                        break;
                    case "2744715540":
                        WeaponIdentifier = "Bug-Out Bag";
                        break;
                    case "2753269585":
                        WeaponIdentifier = "Tempered Dynamo";
                        break;
                    case "2776503072":
                        WeaponIdentifier = "Royal Chase";
                        break;
                    case "2782325300":
                        WeaponIdentifier = "Quickfang";
                        break;
                    case "2782325301":
                        WeaponIdentifier = "Eternity's Edge";
                        break;
                    case "2782325302":
                        WeaponIdentifier = "Crown-Splitter";
                        break;
                    case "2782847179":
                        WeaponIdentifier = "Blasphemer";
                        break;
                    case "2792181427":
                        WeaponIdentifier = "Tiebreaker";
                        break;
                    case "2806900547":
                        WeaponIdentifier = "Hezen Vengeance";
                        break;
                    case "2807687156":
                        WeaponIdentifier = "Distant Tumulus";
                        break;
                    case "2807917121":
                        WeaponIdentifier = "Bonechiller";
                        break;
                    case "2812672356":
                        WeaponIdentifier = "Vertical Orbit QSm";
                        break;
                    case "2812672357":
                        WeaponIdentifier = "Eulogy SI4";
                        break;
                    case "2816212794":
                        WeaponIdentifier = "Bad Juju";
                        break;
                    case "2817798849":
                        WeaponIdentifier = "Hoosegow XE5837";
                        break;
                    case "2817949113":
                        WeaponIdentifier = "The Defiant";
                        break;
                    case "2824241403":
                        WeaponIdentifier = "Bad News XF4354";
                        break;
                    case "2825865804":
                        WeaponIdentifier = "Coriolis Force";
                        break;
                    case "2842493170":
                        WeaponIdentifier = "Sonata-48";
                        break;
                    case "2842493171":
                        WeaponIdentifier = "Trax Dynia";
                        break;
                    case "2850415209":
                        WeaponIdentifier = "Judgment";
                        break;
                    case "2856683562":
                        WeaponIdentifier = "SUROS Regime";
                        break;
                    case "2857348871":
                        WeaponIdentifier = "Honor's Edge";
                        break;
                    case "2860172148":
                        WeaponIdentifier = "Refrain-23";
                        break;
                    case "2860172149":
                        WeaponIdentifier = "Cuboid ARu";
                        break;
                    case "2860172150":
                        WeaponIdentifier = "Ros Lysis II";
                        break;
                    case "2860172151":
                        WeaponIdentifier = "Sand Wasp-3au";
                        break;
                    case "2868432352":
                        WeaponIdentifier = "Commemoration";
                        break;
                    case "2870169846":
                        WeaponIdentifier = "Hailing Confusion";
                        break;
                    case "2891672170":
                        WeaponIdentifier = "Xenoclast IV";
                        break;
                    case "2891976012":
                        WeaponIdentifier = "Future Imperfect";
                        break;
                    case "2891976013":
                        WeaponIdentifier = "Rest for the Wicked";
                        break;
                    case "2896466320":
                        WeaponIdentifier = "The Jade Rabbit";
                        break;
                    case "2903592984":
                        WeaponIdentifier = "Lionheart";
                        break;
                    case "2903592986":
                        WeaponIdentifier = "Rebuke AX-GL";
                        break;
                    case "2903592987":
                        WeaponIdentifier = "Yellowjacket-3au";
                        break;
                    case "2907129556":
                        WeaponIdentifier = "Sturm";
                        break;
                    case "2907129557":
                        WeaponIdentifier = "Sunshot";
                        break;
                    case "2909905776":
                        WeaponIdentifier = "The Hero's Burden";
                        break;
                    case "2919334548":
                        WeaponIdentifier = "Imperial Decree";
                        break;
                    case "2928437919":
                        WeaponIdentifier = "Basilisk";
                        break;
                    case "2931957300":
                        WeaponIdentifier = "Dream Breaker";
                        break;
                    case "2933160903":
                        WeaponIdentifier = "Hailing Confusion";
                        break;
                    case "2936850733":
                        WeaponIdentifier = "Harsh Language";
                        break;
                    case "2936850734":
                        WeaponIdentifier = "Gareth-C";
                        break;
                    case "2936850735":
                        WeaponIdentifier = "Penumbra GSm";
                        break;
                    case "2957367743":
                        WeaponIdentifier = "Toil and Trouble";
                        break;
                    case "2957542878":
                        WeaponIdentifier = "Living Memory";
                        break;
                    case "2961807684":
                        WeaponIdentifier = "The Wizened Rebuke";
                        break;
                    case "2978016230":
                        WeaponIdentifier = "The Jade Rabbit";
                        break;
                    case "2990047042":
                        WeaponIdentifier = "Succession";
                        break;
                    case "3005104939":
                        WeaponIdentifier = "Frostmire's Hex";
                        break;
                    case "3005879472":
                        WeaponIdentifier = "Conjecture TSc";
                        break;
                    case "3005879473":
                        WeaponIdentifier = "Crooked Fang-4fr";
                        break;
                    case "3027844940":
                        WeaponIdentifier = "Proelium FR3";
                        break;
                    case "3027844941":
                        WeaponIdentifier = "Erentil FR4";
                        break;
                    case "3036117446":
                        WeaponIdentifier = "Hezen Vengeance (Timelost)";
                        break;
                    case "3037520408":
                        WeaponIdentifier = "Seventh Seraph Officer Revolver";
                        break;
                    case "3040742682":
                        WeaponIdentifier = "Nameless Midnight";
                        break;
                    case "3055192515":
                        WeaponIdentifier = "Timelines' Vertex";
                        break;
                    case "3067821200":
                        WeaponIdentifier = "Heretic";
                        break;
                    case "3075224551":
                        WeaponIdentifier = "Threaded Needle";
                        break;
                    case "3089417788":
                        WeaponIdentifier = "MIDA Mini-Tool";
                        break;
                    case "3089417789":
                        WeaponIdentifier = "Riskrunner";
                        break;
                    case "3100452337":
                        WeaponIdentifier = "Dreaded Venture";
                        break;
                    case "3110698812":
                        WeaponIdentifier = "Tarrabah";
                        break;
                    case "3116356268":
                        WeaponIdentifier = "Spare Rations";
                        break;
                    case "3117873459":
                        WeaponIdentifier = "Exitus Mk.I";
                        break;
                    case "3141979346":
                        WeaponIdentifier = "D.A.R.C.I.";
                        break;
                    case "3141979347":
                        WeaponIdentifier = "Borealis";
                        break;
                    case "3143732432":
                        WeaponIdentifier = "False Promises";
                        break;
                    case "3164743584":
                        WeaponIdentifier = "Eye of Sol";
                        break;
                    case "3165547384":
                        WeaponIdentifier = "Memory Interdict";
                        break;
                    case "3169616514":
                        WeaponIdentifier = "Bite of the Fox";
                        break;
                    case "3185293912":
                        WeaponIdentifier = "Mos Ultima II";
                        break;
                    case "3185293913":
                        WeaponIdentifier = "One Earth";
                        break;
                    case "3185293914":
                        WeaponIdentifier = "Minuet-12";
                        break;
                    case "3185293915":
                        WeaponIdentifier = "Picayune Mk. 33";
                        break;
                    case "3186018373":
                        WeaponIdentifier = "Vision of Confluence";
                        break;
                    case "3188460622":
                        WeaponIdentifier = "Null Calamity 9";
                        break;
                    case "3190698551":
                        WeaponIdentifier = "Wishbringer";
                        break;
                    case "3197270240":
                        WeaponIdentifier = "Found Verdict";
                        break;
                    case "3199662972":
                        WeaponIdentifier = "Eystein-D";
                        break;
                    case "3211806999":
                        WeaponIdentifier = "Izanagi's Burden";
                        break;
                    case "3214023350":
                        WeaponIdentifier = "Arctic Haze";
                        break;
                    case "3216383791":
                        WeaponIdentifier = "Home for the Lost";
                        break;
                    case "3222518097":
                        WeaponIdentifier = "Anonymous Autumn";
                        break;
                    case "3229272315":
                        WeaponIdentifier = "The Jade Rabbit";
                        break;
                    case "3233390913":
                        WeaponIdentifier = "Infinite Paths 8";
                        break;
                    case "3239754990":
                        WeaponIdentifier = "Maxim XI";
                        break;
                    case "3242052935":
                        WeaponIdentifier = "Tommy's Matchbook";
                        break;
                    case "3242168339":
                        WeaponIdentifier = "Vouchsafe";
                        break;
                    case "3246523828":
                        WeaponIdentifier = "Hadrian-A";
                        break;
                    case "3246523829":
                        WeaponIdentifier = "Resilient People";
                        break;
                    case "3246523831":
                        WeaponIdentifier = "Para Torus I";
                        break;
                    case "3252697558":
                        WeaponIdentifier = "Truthteller";
                        break;
                    case "3258665412":
                        WeaponIdentifier = "Trinary System";
                        break;
                    case "3260753130":
                        WeaponIdentifier = "Ticuu's Divination";
                        break;
                    case "3272713429":
                        WeaponIdentifier = "Eye of Foresight";
                        break;
                    case "3273345532":
                        WeaponIdentifier = "Night Terror";
                        break;
                    case "3281285075":
                        WeaponIdentifier = "Posterity";
                        break;
                    case "3285365666":
                        WeaponIdentifier = "Jack Queen King 3";
                        break;
                    case "3285365667":
                        WeaponIdentifier = "West of Sunfall 7";
                        break;
                    case "3287201781":
                        WeaponIdentifier = "Arc Logic";
                        break;
                    case "3297863558":
                        WeaponIdentifier = "Twilight Oath";
                        break;
                    case "3312073052":
                        WeaponIdentifier = "A Single Clap";
                        break;
                    case "3312073053":
                        WeaponIdentifier = "Shepherd's Watch";
                        break;
                    case "3312073054":
                        WeaponIdentifier = "Show of Force";
                        break;
                    case "3312073055":
                        WeaponIdentifier = "Widow's Bite";
                        break;
                    case "3325463374":
                        WeaponIdentifier = "Thunderlord";
                        break;
                    case "3325744914":
                        WeaponIdentifier = "Inaugural Address";
                        break;
                    case "3325778512":
                        WeaponIdentifier = "A Fine Memorial";
                        break;
                    case "3329842376":
                        WeaponIdentifier = "Memory Interdict";
                        break;
                    case "3334276332":
                        WeaponIdentifier = "Vestian Dynasty";
                        break;
                    case "3334276333":
                        WeaponIdentifier = "Death by Scorn";
                        break;
                    case "3335343363":
                        WeaponIdentifier = "Tarantula";
                        break;
                    case "3336215727":
                        WeaponIdentifier = "Martyr's Make";
                        break;
                    case "3354242550":
                        WeaponIdentifier = "The Recluse";
                        break;
                    case "3356526253":
                        WeaponIdentifier = "Wishbringer";
                        break;
                    case "3361694400":
                        WeaponIdentifier = "Trax Arda II";
                        break;
                    case "3361694401":
                        WeaponIdentifier = "Fare-Thee-Well";
                        break;
                    case "3361694402":
                        WeaponIdentifier = "Sea Scorpion-1sr";
                        break;
                    case "3361694403":
                        WeaponIdentifier = "Inverness-SR2";
                        break;
                    case "3366545721":
                        WeaponIdentifier = "Bequest";
                        break;
                    case "3369545945":
                        WeaponIdentifier = "Temporal Clause";
                        break;
                    case "3376406418":
                        WeaponIdentifier = "Right Side of Wrong";
                        break;
                    case "3380742308":
                        WeaponIdentifier = "Alone as a god";
                        break;
                    case "3383958216":
                        WeaponIdentifier = "Harmony-21";
                        break;
                    case "3383958217":
                        WeaponIdentifier = "Furina-2mg";
                        break;
                    case "3383958218":
                        WeaponIdentifier = "Philippis-B";
                        break;
                    case "3383958219":
                        WeaponIdentifier = "Protostar CSu";
                        break;
                    case "3385326721":
                        WeaponIdentifier = "Reckless Oracle";
                        break;
                    case "3393130645":
                        WeaponIdentifier = "Positive Outlook";
                        break;
                    case "3393519051":
                        WeaponIdentifier = "Perfect Paradox";
                        break;
                    case "3410721600":
                        WeaponIdentifier = "Subtle Calamity";
                        break;
                    case "3413074534":
                        WeaponIdentifier = "Polaris Lance";
                        break;
                    case "3413860062":
                        WeaponIdentifier = "The Chaperone";
                        break;
                    case "3413860063":
                        WeaponIdentifier = "Lord of Wolves";
                        break;
                    case "3419149443":
                        WeaponIdentifier = "Crooked Fang-4fr";
                        break;
                    case "3424403076":
                        WeaponIdentifier = "The Fool's Remedy";
                        break;
                    case "3425561386":
                        WeaponIdentifier = "Motion to Compel";
                        break;
                    case "3434507093":
                        WeaponIdentifier = "Occluded Finality";
                        break;
                    case "3434629515":
                        WeaponIdentifier = "Metronome-52";
                        break;
                    case "3434944005":
                        WeaponIdentifier = "Point of the Stag";
                        break;
                    case "3435238842":
                        WeaponIdentifier = "Song of Justice VI";
                        break;
                    case "3435238843":
                        WeaponIdentifier = "Good Counsel IX";
                        break;
                    case "3437746471":
                        WeaponIdentifier = "Crimson";
                        break;
                    case "3441197112":
                        WeaponIdentifier = "Nox Cordis II";
                        break;
                    case "3441197113":
                        WeaponIdentifier = "Nox Lumen II";
                        break;
                    case "3441197115":
                        WeaponIdentifier = "Parsec TSu";
                        break;
                    case "3445437901":
                        WeaponIdentifier = "Main Ingredient";
                        break;
                    case "3454326177":
                        WeaponIdentifier = "Omniscient Eye";
                        break;
                    case "3460122497":
                        WeaponIdentifier = "Imperial Needle";
                        break;
                    case "3460576091":
                        WeaponIdentifier = "Duality";
                        break;
                    case "3461377698":
                        WeaponIdentifier = "Baligant XU7743";
                        break;
                    case "3473290087":
                        WeaponIdentifier = "Frozen Orbit";
                        break;
                    case "3487253372":
                        WeaponIdentifier = "The Lament";
                        break;
                    case "3493948734":
                        WeaponIdentifier = "Stampede Mk.32";
                        break;
                    case "3493948735":
                        WeaponIdentifier = "Plemusa-B";
                        break;
                    case "3501969491":
                        WeaponIdentifier = "The Cut and Run";
                        break;
                    case "3504336176":
                        WeaponIdentifier = "Night Watch";
                        break;
                    case "3512014804":
                        WeaponIdentifier = "Lumina";
                        break;
                    case "3512349612":
                        WeaponIdentifier = "Coriolis Force";
                        break;
                    case "3514096004":
                        WeaponIdentifier = "Eternal Blazon";
                        break;
                    case "3514144928":
                        WeaponIdentifier = "The Summoner (Adept)";
                        break;
                    case "3524313097":
                        WeaponIdentifier = "Eriana's Vow";
                        break;
                    case "3529780349":
                        WeaponIdentifier = "The Marine";
                        break;
                    case "3535742959":
                        WeaponIdentifier = "Randy's Throwing Knife";
                        break;
                    case "3549153978":
                        WeaponIdentifier = "Fighting Lion";
                        break;
                    case "3549153979":
                        WeaponIdentifier = "The Prospector";
                        break;
                    case "3550697748":
                        WeaponIdentifier = "Thistle and Yew";
                        break;
                    case "3551104348":
                        WeaponIdentifier = "Double-Edged Answer";
                        break;
                    case "3556971406":
                        WeaponIdentifier = "Requiem-43";
                        break;
                    case "3556999246":
                        WeaponIdentifier = "Pleiades Corrector";
                        break;
                    case "3565520715":
                        WeaponIdentifier = "Crowd Pleaser";
                        break;
                    case "3569802112":
                        WeaponIdentifier = "The Old Fashioned";
                        break;
                    case "3569842567":
                        WeaponIdentifier = "Lost and Found";
                        break;
                    case "3580904580":
                        WeaponIdentifier = "Legend of Acrius";
                        break;
                    case "3580904581":
                        WeaponIdentifier = "Tractor Cannon";
                        break;
                    case "3582424018":
                        WeaponIdentifier = "Deadpan Delivery";
                        break;
                    case "3588934839":
                        WeaponIdentifier = "Le Monarque";
                        break;
                    case "3593598010":
                        WeaponIdentifier = "The Time-Worn Spire";
                        break;
                    case "3616586446":
                        WeaponIdentifier = "First In, Last Out";
                        break;
                    case "3622137132":
                        WeaponIdentifier = "Last Hope";
                        break;
                    case "3627718344":
                        WeaponIdentifier = "Triumph DX-PR";
                        break;
                    case "3627718345":
                        WeaponIdentifier = "Nanty Narker";
                        break;
                    case "3628991658":
                        WeaponIdentifier = "Graviton Lance";
                        break;
                    case "3628991659":
                        WeaponIdentifier = "Vigilance Wing";
                        break;
                    case "3629968765":
                        WeaponIdentifier = "Negative Space";
                        break;
                    case "3637570176":
                        WeaponIdentifier = "Eye of Sol (Adept)";
                        break;
                    case "3649055823":
                        WeaponIdentifier = "Crimil's Dagger";
                        break;
                    case "3651075426":
                        WeaponIdentifier = "Holless-IV";
                        break;
                    case "3653573172":
                        WeaponIdentifier = "Praedyth's Revenge";
                        break;
                    case "3654674561":
                        WeaponIdentifier = "Dead Man's Tale";
                        break;
                    case "3658188704":
                        WeaponIdentifier = "The Messenger";
                        break;
                    case "3662200188":
                        WeaponIdentifier = "Nox Lumen II";
                        break;
                    case "3662200189":
                        WeaponIdentifier = "Nox Cordis II";
                        break;
                    case "3662200190":
                        WeaponIdentifier = "Parsec TSu";
                        break;
                    case "3666954561":
                        WeaponIdentifier = "Copperhead-4sn";
                        break;
                    case "3666954562":
                        WeaponIdentifier = "Veleda-D";
                        break;
                    case "3666954563":
                        WeaponIdentifier = "Elegy-49";
                        break;
                    case "3669616453":
                        WeaponIdentifier = "Hoosegow";
                        break;
                    case "3682803680":
                        WeaponIdentifier = "Shayura's Wrath";
                        break;
                    case "3690523502":
                        WeaponIdentifier = "Love and Death";
                        break;
                    case "3691881271":
                        WeaponIdentifier = "Sins of the Past";
                        break;
                    case "3704653637":
                        WeaponIdentifier = "Stryker's Sure-Hand";
                        break;
                    case "3717177717":
                        WeaponIdentifier = "Multimach CCX";
                        break;
                    case "3740842661":
                        WeaponIdentifier = "Sleepless";
                        break;
                    case "3745974521":
                        WeaponIdentifier = "The Militia's Birthright";
                        break;
                    case "3745990145":
                        WeaponIdentifier = "Long Shadow";
                        break;
                    case "3748713778":
                        WeaponIdentifier = "Pentatonic-48";
                        break;
                    case "3748713779":
                        WeaponIdentifier = "Morrigan-D";
                        break;
                    case "3751622019":
                        WeaponIdentifier = "Dead Man Walking XX7463";
                        break;
                    case "3762467076":
                        WeaponIdentifier = "Uriel's Gift";
                        break;
                    case "3762467077":
                        WeaponIdentifier = "Solemn Hymn";
                        break;
                    case "3762467078":
                        WeaponIdentifier = "Scathelocke";
                        break;
                    case "3762467079":
                        WeaponIdentifier = "Valakadyn";
                        break;
                    case "3766045777":
                        WeaponIdentifier = "Black Talon";
                        break;
                    case "3768901236":
                        WeaponIdentifier = "Found Verdict";
                        break;
                    case "3776129137":
                        WeaponIdentifier = "Zenobia-D";
                        break;
                    case "3778520449":
                        WeaponIdentifier = "Jiangshi AR4";
                        break;
                    case "3778520450":
                        WeaponIdentifier = "Halfdan-D";
                        break;
                    case "3778520451":
                        WeaponIdentifier = "Galliard-42";
                        break;
                    case "3792720684":
                        WeaponIdentifier = "Spiderbite-1si";
                        break;
                    case "3796510434":
                        WeaponIdentifier = "Corrective Measure (Timelost)";
                        break;
                    case "3799980700":
                        WeaponIdentifier = "Transfiguration";
                        break;
                    case "3809805228":
                        WeaponIdentifier = "Dissonance-34";
                        break;
                    case "3809805229":
                        WeaponIdentifier = "Requiem SI2";
                        break;
                    case "3809805230":
                        WeaponIdentifier = "Roderic-C";
                        break;
                    case "3809805231":
                        WeaponIdentifier = "Vinegaroon-2si";
                        break;
                    case "3813153080":
                        WeaponIdentifier = "Finite Impactor";
                        break;
                    case "3824106094":
                        WeaponIdentifier = "Devil's Ruin";
                        break;
                    case "3826803617":
                        WeaponIdentifier = "The Dream";
                        break;
                    case "3829285960":
                        WeaponIdentifier = "Horror Story";
                        break;
                    case "3836861464":
                        WeaponIdentifier = "THE SWARM (Adept)";
                        break;
                    case "3843477312":
                        WeaponIdentifier = "Blast Furnace";
                        break;
                    case "3844694310":
                        WeaponIdentifier = "The Jade Rabbit";
                        break;
                    case "3847137620":
                        WeaponIdentifier = "Sola's Scar (Adept)";
                        break;
                    case "3850168899":
                        WeaponIdentifier = "Martyr's Retribution";
                        break;
                    case "3854037061":
                        WeaponIdentifier = "A Swift Verdict";
                        break;
                    case "3854359821":
                        WeaponIdentifier = "The Number";
                        break;
                    case "3856705927":
                        WeaponIdentifier = "Hawkmoon";
                        break;
                    case "3856940575":
                        WeaponIdentifier = "Trust";
                        break;
                    case "3860697508":
                        WeaponIdentifier = "Minuet-42";
                        break;
                    case "3860697509":
                        WeaponIdentifier = "Pribina-D";
                        break;
                    case "3860697510":
                        WeaponIdentifier = "Imset HC4";
                        break;
                    case "3861448240":
                        WeaponIdentifier = "Emperor's Courtesy";
                        break;
                    case "3863882743":
                        WeaponIdentifier = "Uriel's Gift";
                        break;
                    case "3866356643":
                        WeaponIdentifier = "IKELOS_HC_v1.0.1";
                        break;
                    case "3870811754":
                        WeaponIdentifier = "Night Terror";
                        break;
                    case "3886263130":
                        WeaponIdentifier = "I Am Alive";
                        break;
                    case "3889907763":
                        WeaponIdentifier = "Royal Dispensation II";
                        break;
                    case "3890960908":
                        WeaponIdentifier = "The Guiding Sight";
                        break;
                    case "3899270607":
                        WeaponIdentifier = "The Colony";
                        break;
                    case "3906357376":
                        WeaponIdentifier = "Madrugada SR2";
                        break;
                    case "3906357377":
                        WeaponIdentifier = "Trax Lysis II";
                        break;
                    case "3906357378":
                        WeaponIdentifier = "Black Tiger-2sr";
                        break;
                    case "3906357379":
                        WeaponIdentifier = "Armillary PSu";
                        break;
                    case "3906942101":
                        WeaponIdentifier = "Conspirator";
                        break;
                    case "3907337522":
                        WeaponIdentifier = "Oxygen SR3";
                        break;
                    case "3909683950":
                        WeaponIdentifier = "Man o' War";
                        break;
                    case "3920811074":
                        WeaponIdentifier = "Medley-45";
                        break;
                    case "3924212056":
                        WeaponIdentifier = "Loud Lullaby";
                        break;
                    case "3929685100":
                        WeaponIdentifier = "The Deicide";
                        break;
                    case "3937866388":
                        WeaponIdentifier = "Seventh Seraph SI-2";
                        break;
                    case "3950088638":
                        WeaponIdentifier = "Motion to Suppress";
                        break;
                    case "3954531357":
                        WeaponIdentifier = "Mob Justice";
                        break;
                    case "3957603605":
                        WeaponIdentifier = "Wrong Side of Right";
                        break;
                    case "3967155859":
                        WeaponIdentifier = "The Last Dance";
                        break;
                    case "3973202132":
                        WeaponIdentifier = "Thorn";
                        break;
                    case "3991544422":
                        WeaponIdentifier = "Sol Pariah 6";
                        break;
                    case "3991544423":
                        WeaponIdentifier = "The Conqueror 2";
                        break;
                    case "3993415705":
                        WeaponIdentifier = "The Mountaintop";
                        break;
                    case "4008494330":
                        WeaponIdentifier = "Every Waking Moment";
                        break;
                    case "4014434381":
                        WeaponIdentifier = "Kibou AR3";
                        break;
                    case "4017959782":
                        WeaponIdentifier = "Symmetry";
                        break;
                    case "4020742303":
                        WeaponIdentifier = "Prophet of Doom";
                        break;
                    case "4023807721":
                        WeaponIdentifier = "Shayura's Wrath (Adept)";
                        break;
                    case "4024037919":
                        WeaponIdentifier = "Origin Story";
                        break;
                    case "4036115577":
                        WeaponIdentifier = "Sleeper Simulant";
                        break;
                    case "4037745684":
                        WeaponIdentifier = "Bonechiller";
                        break;
                    case "4041111172":
                        WeaponIdentifier = "The Button";
                        break;
                    case "4050645223":
                        WeaponIdentifier = "Hezen Vengeance";
                        break;
                    case "4068264807":
                        WeaponIdentifier = "Monte Carlo";
                        break;
                    case "4077196130":
                        WeaponIdentifier = "Trust";
                        break;
                    case "4083045006":
                        WeaponIdentifier = "Persuader";
                        break;
                    case "4094657108":
                        WeaponIdentifier = "Techeun Force";
                        break;
                    case "4095462486":
                        WeaponIdentifier = "Pit Launcher";
                        break;
                    case "4095896073":
                        WeaponIdentifier = "Accrued Redemption";
                        break;
                    case "4103414242":
                        WeaponIdentifier = "Divinity";
                        break;
                    case "4105447486":
                        WeaponIdentifier = "Nox Veneris II";
                        break;
                    case "4105447487":
                        WeaponIdentifier = "Elatha FR4";
                        break;
                    case "4106983932":
                        WeaponIdentifier = "Elatha FR4";
                        break;
                    case "4117693024":
                        WeaponIdentifier = "Mindbender's Ambition";
                        break;
                    case "4123554961":
                        WeaponIdentifier = "Premonition";
                        break;
                    case "4124357815":
                        WeaponIdentifier = "The Epicurean";
                        break;
                    case "4124984448":
                        WeaponIdentifier = "Hard Light";
                        break;
                    case "4138174248":
                        WeaponIdentifier = "Go Figure";
                        break;
                    case "4138415948":
                        WeaponIdentifier = "Hand in Hand";
                        break;
                    case "4138415949":
                        WeaponIdentifier = "Ded Acumen II";
                        break;
                    case "4138415950":
                        WeaponIdentifier = "Ded Nemoris II";
                        break;
                    case "4145119417":
                        WeaponIdentifier = "Heart of Time";
                        break;
                    case "4146702548":
                        WeaponIdentifier = "Outrageous Fortune";
                        break;
                    case "4148143418":
                        WeaponIdentifier = "Show of Force XF4865";
                        break;
                    case "4149758318":
                        WeaponIdentifier = "Traveler's Judgment 5";
                        break;
                    case "4156253727":
                        WeaponIdentifier = "The Third Axiom";
                        break;
                    case "4157959956":
                        WeaponIdentifier = "Tongeren-LR3";
                        break;
                    case "4157959958":
                        WeaponIdentifier = "Aachen-LR2";
                        break;
                    case "4157959959":
                        WeaponIdentifier = "Damietta-LR2";
                        break;
                    case "4166221755":
                        WeaponIdentifier = "Trophy Hunter";
                        break;
                    case "4169533630":
                        WeaponIdentifier = "Loud Lullaby";
                        break;
                    case "4174481098":
                        WeaponIdentifier = "Steel Sybil Z-14";
                        break;
                    case "4184808992":
                        WeaponIdentifier = "Adored";
                        break;
                    case "4190156464":
                        WeaponIdentifier = "Merciless";
                        break;
                    case "4190932264":
                        WeaponIdentifier = "Beloved";
                        break;
                    case "4193877020":
                        WeaponIdentifier = "Does Not Compute";
                        break;
                    case "4203034886":
                        WeaponIdentifier = "Zephyr";
                        break;
                    case "4211534763":
                        WeaponIdentifier = "Pribina-D";
                        break;
                    case "4213221671":
                        WeaponIdentifier = "Sunrise GL4";
                        break;
                    case "4221925398":
                        WeaponIdentifier = "Cup-Bearer SA/2";
                        break;
                    case "4221925399":
                        WeaponIdentifier = "Reginar-B";
                        break;
                    case "4227181568":
                        WeaponIdentifier = "Exit Strategy";
                        break;
                    case "4230965989":
                        WeaponIdentifier = "Commemoration";
                        break;
                    case "4230993599":
                        WeaponIdentifier = "Steel Sybil Z-14";
                        break;
                    case "4238497225":
                        WeaponIdentifier = "D.F.A.";
                        break;
                    case "4248569242":
                        WeaponIdentifier = "Heritage";
                        break;
                    case "4255268456":
                        WeaponIdentifier = "Skyburner's Oath";
                        break;
                    case "4255586669":
                        WeaponIdentifier = "Empty Vessel";
                        break;
                    case "4265183314":
                        WeaponIdentifier = "Multimach CCX";
                        break;
                    case "4272442416":
                        WeaponIdentifier = "The Domino";
                        break;
                    case "4277547616":
                        WeaponIdentifier = "Every Waking Moment";
                        break;
                    case "4281371574":
                        WeaponIdentifier = "Hung Jury SR4";
                        break;
                    case "4288031461":
                        WeaponIdentifier = "The Emperor's Envy";
                        break;
                    case "4289226715":
                        WeaponIdentifier = "Vex Mythoclast";
                        break;
                    case "4292849692":
                        WeaponIdentifier = "Crimil's Dagger";
                        break;

                    default:
                        WeaponIdentifier = "Unknown Weapon";
                        break;
                }

                int kills = Convert.ToInt32(WeaponKills);
                int ratio = Convert.ToInt32(WeaponPrecisionRatio.Replace("%", ""));

                if(kills > 7 && ratio > 90)
                {
                    Suspected = true;
                }

                System.Diagnostics.Debug.Print(WeaponIdentifier);
                System.Diagnostics.Debug.Print(WeaponKills);
                System.Diagnostics.Debug.Print(WeaponPrecisionRatio);

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
        public class CharacterEntry
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






    public class CarnageReport
    {
        public string ActivityRefID { get; set; }
        public string ActivitySpaceID { get; set; }
        public string ActivityTypeID { get; set; }
        public void SetGameType(string hash)
        {
            ActivityHash = hash;
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
                    ActivityTypeID = "Patrol";
                    break;
                case "2936791966":
                    ActivityTypeID = "Lost Sector";
                    break;
                case "1928961926":
                    ActivityTypeID = "Patrol";
                    break;
                case "1717505396":
                    ActivityTypeID = "Control";
                    break;
                case "2865450620":
                    ActivityTypeID = "Survival";
                    break;
                case "588019350":
                    ActivityTypeID = "Trials";
                    break;
                case "25688104":
                    ActivityTypeID = "Override";
                    break;
                case "2865532048":
                    ActivityTypeID = "Override";
                    break;
                case "1683791010":
                    ActivityTypeID = "Iron Banner";
                    break;
                case "3076038389":
                    ActivityTypeID = "Private Match";
                    break;
                case "1813752023":
                    ActivityTypeID = "Patrol";
                    break;
                case "1070981430":
                    ActivityTypeID = "Lost Sector";
                    break;
                case "3029388710":
                    ActivityTypeID = "Nightfall:Legend";
                    break;
                case "2205920677":
                    ActivityTypeID = "Empire Hunt";
                    break;
                case "135431604":
                    ActivityTypeID = "Gambit";
                    break;
                case "1077850348":
                    ActivityTypeID = "Dungeon";
                    break;
                case "743628305":
                    ActivityTypeID = "Vangaurd Strikes";
                    break;
                case "3029388705":
                    ActivityTypeID = "Nightfall:Master";
                    break;
                case "1655431815":
                    ActivityTypeID = "Expunge";
                    break;
                case "2019961998":
                    ActivityTypeID = "Lost Sector";
                    break;
                case "2122313384":
                    ActivityTypeID = "Raid";
                    break;
                case "3933916447":
                    ActivityTypeID = "Override";
                    break;
                case "3029388711":
                    ActivityTypeID = "Nightfall:Hero";
                    break;
                case "2936791996":
                    ActivityTypeID = "Lost Sector";
                    break;
                case "3293630130":
                    ActivityTypeID = "Nightfall:Legend";
                    break;
                case "506197732":
                    ActivityTypeID = "Patrol";
                    break;
                case "2829206727":
                    ActivityTypeID = "Lost Sector";
                    break;
                case "3293630131":
                    ActivityTypeID = "Nightfall:Hero";
                    break;
                case "2936791995":
                    ActivityTypeID = "Lost Sector";
                    break;
                default:
                    ActivityTypeID = "Unkown Game Mode";
                    break;
            }
        }
        public void SetLocation(string hash)
        {

            LocationHash = hash;
            switch(hash)
            {
                case "2936791995":
                    ActivitySpaceID = "Exodus Garden 2A:Master";
                    break;
                case "3293630131":
                    ActivitySpaceID = "Cosmodrome";
                    break;
                case "2829206727":
                    ActivitySpaceID = "K1 Communication:Legend";
                    break;
                case "506197732":
                    ActivitySpaceID = "Moon";
                    break;
                case "3293630130":
                    ActivitySpaceID = "Cosmodrome";
                    break;
                case "2936791996":
                    ActivitySpaceID = "Exodus Garden 2A:Legend";
                    break;
                case "3029388711":
                    ActivitySpaceID = "Nessus";
                    break;
                case "3933916447":
                    ActivitySpaceID = "Tangled Shore";
                    break;
                case "2122313384":
                    ActivitySpaceID = "The Last Wish";
                    break;
                case "2019961998":
                    ActivitySpaceID = "The Empty Tank:Legend";
                    break;
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
                    ActivitySpaceID = "EDZ";
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
                case "2260508373":
                    ActivitySpaceID = "Bannerfall";
                    break;
                case "1326470716":
                    ActivitySpaceID = "Endless Vale";
                    break;
                case "2905719116":
                    ActivitySpaceID = "Fragment";
                    break;
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
                case "960911914":
                    ActivitySpaceID = "Jav 4";
                    break;
                case "2585183686":
                    ActivitySpaceID = "Fortress";
                    break;
                case "1813752023":
                    ActivitySpaceID = "Europa";
                    break;
                case "1070981430":
                    ActivitySpaceID = "Perdition:Legend";
                    break;
                case "2205920677":
                    ActivitySpaceID = "The Dark Priestess:Master";
                    break;
                case "3029388710":
                    ActivitySpaceID = "Nessus";
                    break;
                case "2505336188":
                    ActivitySpaceID = "The Dead Cliffs";
                    break;
                case "1855216675":
                    ActivitySpaceID = "New Arcadia";
                    break;
                case "1575864965":
                    ActivitySpaceID = "Deep Six";
                    break;
                case "1731870079":
                    ActivitySpaceID = "Legions Folly";
                    break;
                case "1077850348":
                    ActivitySpaceID = "Prophecy";
                    break;
                case "3240321863":
                    ActivitySpaceID = "Arms Dealer";
                    break;
                case "1684420962":
                    ActivitySpaceID = "The Disgraced";
                    break;
                case "3643233460":
                    ActivitySpaceID = "The Scarlet Keep";
                    break;
                case "3029388705":
                    ActivitySpaceID = "Nessus";
                        break;
                case "114977383":
                    ActivitySpaceID = "Exodus Blue";
                    break;
                case "1655431815":
                    ActivitySpaceID = "Tangled Shore";
                    break;
                default:
                    ActivitySpaceID = "Unkown Location";
                    break;
            }
        }

        public string LocationHash { get; set; }
        public string ActivityHash { get; set; }
        public DateTime ActivityStart { get; set; }
        public List<Guardian> ActivityPlayers { get; set; }
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
}
