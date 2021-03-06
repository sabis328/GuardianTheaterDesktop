using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Guardian_Theater_Desktop
{
    public class Twitch_Client
    {
        #region Twitch Validation
        public string Twitch_ClientID { get; set; }
        public string Twitch_Client_Secret { get; set; }
        public string Twitch_Client_Token { get; set; }

        public List<string> PossibleCreators { get; set; }
        public enum Twitch_Validation_Status
        {
            Success,
            Failed,
            Error
        }

        public Twitch_Validation_Status Is_Validated { get; private set; }

        private string twitch_validation_base = "https://id.twitch.tv/oauth2/token?client_id=";
        private string twitch_validation_mid = "&client_secret=";
        private string twitch_validation_end = "&grant_type=client_credentials";
        //h7bled1w6wracl3bytlhqwra3d7pr8
        public event EventHandler<Twitch_Validation_Status> Twitch_Validation_Event;

        public void Validate_Twitch_Client()
        {

            //Twitch_ClientID = "abvhdv9zyqhefmnbjz3fljxx3hpc7u";
            //Twitch_Client_Secret = "h7bled1w6wracl3bytlhqwra3d7pr8";
            string twitch_val_link = twitch_validation_base + Twitch_ClientID + twitch_validation_mid + Twitch_Client_Secret + twitch_validation_end;

            string ValidationBody = "";
            try
            {
                HttpWebRequest query = (HttpWebRequest)WebRequest.Create(twitch_val_link);
                query.Method = "POST";
                query.KeepAlive = true;

                HttpWebResponse res = (HttpWebResponse)query.GetResponse();

                ValidationBody = new StreamReader(res.GetResponseStream()).ReadToEnd();
                System.Diagnostics.Debug.WriteLine(ValidationBody);

                if (ValidationBody.Contains("access_token"))
                {
                    int start = ValidationBody.IndexOf("access_token") + 15;
                    int end = ValidationBody.IndexOf("\"", start);

                    Twitch_Client_Token = ValidationBody.Substring(start, end - start);

                    Is_Validated = Twitch_Validation_Status.Success;
                    Twitch_Validation_Event?.Invoke(this, Is_Validated);
                }
                else
                {
                    System.Diagnostics.Debug.Print("Failed Validation");
                    Is_Validated = Twitch_Validation_Status.Failed; Twitch_Validation_Event?.Invoke(this, Is_Validated);
                }
            }
            catch
            {
                System.Diagnostics.Debug.Print("Validation Errored out");
                Is_Validated = Twitch_Validation_Status.Error;
                Twitch_Validation_Event?.Invoke(this, Is_Validated);
            }
        }
        #endregion



        #region Twitch Channel Search

        public List<TwitchCreator> Found_Channels { get; set; }
        public enum Channel_Search_Result
        {
            Found_Channel,
            No_Matches,
            All_Parsed,
            Error
        }

        public Channel_Search_Result Search_Channel_Result { get; private set; }

        public event EventHandler<Channel_Search_Result> Search_Channel_Complete;

        public void Twitch_Find_Channels(string searchFor, bool matchName)
        {
            System.Diagnostics.Debug.Print("Searching for : " + searchFor);
            Found_Channels = new List<TwitchCreator>();
            List<string> channelJSON = new List<string>();

            string requestBody;
            string queryBase = "https://api.twitch.tv/helix/search/channels?query=";
            try
            {
                HttpWebRequest query = (HttpWebRequest)WebRequest.Create(queryBase + searchFor);
                query.Method = "GET";
                System.Diagnostics.Debug.Print(query.RequestUri.ToString());
                query.Headers.Add("Authorization", string.Format("Bearer {0}", Twitch_Client_Token));
                query.Headers.Add("Client-Id", string.Format("{0}", Twitch_ClientID));

                HttpWebResponse res = (HttpWebResponse)query.GetResponse();
                requestBody = new StreamReader(res.GetResponseStream()).ReadToEnd();


                if (requestBody.Contains("broadcaster_language"))
                {
                    int start = 0;
                    int end = 0;

                    while (start < requestBody.LastIndexOf("broadcaster_language"))
                    {
                        start = requestBody.IndexOf("broadcaster_language", end);

                        end = requestBody.IndexOf("started_at", start);
                        int endS = end + 13;
                        end = requestBody.IndexOf("\"", endS);

                        channelJSON.Add(requestBody.Substring(start, end - start));
                    }

                    foreach (string chan in channelJSON)
                    {
                        TwitchCreator foundChan = new TwitchCreator();
                        foundChan.Parse_Channel_Data(chan);

                        if (foundChan.is_Parsed == TwitchCreator.Channel_result.Parsed)
                        {

                            if (matchName)
                            {
                                if (foundChan.Username.ToLower().Trim() == searchFor.ToLower().Trim())
                                {
                                    Found_Channels.Add(foundChan);
                                    break;
                                }
                            }
                            else
                            { System.Diagnostics.Debug.Print("Dont need to match"); Found_Channels.Add(foundChan); Search_Channel_Result = Channel_Search_Result.Found_Channel; }

                        }
                    }

                    Search_Channel_Result = Channel_Search_Result.All_Parsed;
                    Search_Channel_Complete?.Invoke(this, Search_Channel_Result);
                }
                else
                {
                    //System.Diagnostics.Debug.Print("Failed Validation");
                    Search_Channel_Result = Channel_Search_Result.Error;
                    Search_Channel_Complete?.Invoke(this, Search_Channel_Result);
                }
            }
            catch
            {
                Search_Channel_Result = Channel_Search_Result.Error;
                Search_Channel_Complete?.Invoke(this, Search_Channel_Result);
            }
        }

        #endregion

        #region Load videos for channel


        public event EventHandler<TwitchCreator> Videos_Loaded;
        public event EventHandler<TwitchCreator> Videos_Failed_Load;
        public TwitchCreator Load_Channel_Videos(TwitchCreator channel)
        {
            string requestBase = "https://api.twitch.tv/helix/videos?user_id=";
            string requestBody;

            List<string> VideoJSONArray = new List<string>();
            try
            {
                System.Diagnostics.Debug.Print("Loading videos for " + channel.Username);
                HttpWebRequest query = (HttpWebRequest)WebRequest.Create(requestBase + channel.User_ID_Code);
                query.Method = "GET";

                query.Headers.Add("Authorization", string.Format("Bearer {0}", Twitch_Client_Token));
                query.Headers.Add("Client-Id", string.Format("{0}", Twitch_ClientID));


                HttpWebResponse res = (HttpWebResponse)query.GetResponse();
                requestBody = new StreamReader(res.GetResponseStream()).ReadToEnd();

                if (requestBody.Contains("data"))
                {
                    int startRead = 0;
                    int endRead = 0;

                    while (startRead < requestBody.LastIndexOf("\"id\":\""))
                    {
                        startRead = requestBody.IndexOf("\"id\":\"", endRead);
                        endRead = requestBody.IndexOf("duration", startRead);
                        endRead = requestBody.IndexOf("}", endRead);

                        string videoSPLITJSON = requestBody.Substring(startRead, endRead - startRead);
                        //System.Diagnostics.Debug.Print(videoSPLITJSON);

                        VideoJSONArray.Add(videoSPLITJSON);
                    }

                    List<TwitchVideo> VideosToAdd = Parse_Video_Json(VideoJSONArray);

                    if (VideosToAdd.Count > 0)
                    {
                        channel.Channel_Saved_Videos = VideosToAdd;

                        Videos_Loaded?.Invoke(this, channel);
                        return channel;
                    }
                    else
                    {
                        Videos_Failed_Load?.Invoke(this, channel);
                        System.Diagnostics.Debug.Print("No videos found");
                        return channel;
                    }

                }
                else
                {
                    Videos_Failed_Load?.Invoke(this, channel);
                    System.Diagnostics.Debug.Print("Video Requst failed");
                    return channel;
                }
            }
            catch
            {
                Videos_Failed_Load?.Invoke(this, channel);
                System.Diagnostics.Debug.Print("I fucked something up in the load video section");
                return channel;
            }
        }


        private List<TwitchVideo> Parse_Video_Json(List<string> videoInfoJSON)
        {

            if (videoInfoJSON.Count > 0)
            {
                List<TwitchVideo> videoinfoArray = new List<TwitchVideo>();


                foreach (string videoJSON in videoInfoJSON)
                {

                    TwitchVideo video = new TwitchVideo();

                    try
                    {
                        int start = 0;
                        int end = 0;

                        start = videoJSON.IndexOf("id", start) + 5;
                        end = videoJSON.IndexOf(",") - 1;

                        video.videoID = videoJSON.Substring(start, end - start);
                        // System.Diagnostics.Debug.Print("id found : " + video.videoID);


                        start = videoJSON.IndexOf("title", start) + 8;

                        // System.Diagnostics.Debug.Print("title start : " + start);
                        end = videoJSON.IndexOf(",\"description", start) - 1;

                        // System.Diagnostics.Debug.Print("title end : " + end);

                        video.videoTitle = videoJSON.Substring(start, end - start);



                        start = videoJSON.IndexOf("created_at", start) + 13;
                        end = videoJSON.IndexOf("published_at", start) - 3;

                        string timetostring = videoJSON.Substring(start, end - start);
                        //2021-01-24T02:47:33Z
                        video.videoCreated = DateTime.ParseExact(timetostring, "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
                        video.videoPublished = video.videoCreated;

                        System.Diagnostics.Debug.Print("title found : " + video.videoTitle + "   at   " + video.videoCreated.ToString());

                        start = videoJSON.IndexOf("url", start) + 6;
                        end = videoJSON.IndexOf("thumbnail_url", start) - 3;

                        video.videoLink = videoJSON.Substring(start, end - start);


                        start = end + 19;
                        end = videoJSON.IndexOf("viewable", start) - 3;


                        video.videoThumbnial = videoJSON.Substring(start, end - start);

                        start = videoJSON.IndexOf("view_count", start) + 12;
                        end = videoJSON.IndexOf("language", start) - 2;

                        video.videoViews = videoJSON.Substring(start, end - start);

                        start = videoJSON.IndexOf("duration", start) + 11;
                        end = videoJSON.IndexOf("\",", start);

                        timetostring = videoJSON.Substring(start, end - start);


                        var m = Regex.Match(timetostring, @"^((?<hours>\d+)h)?((?<minutes>\d+)m)?((?<seconds>\d+)s)?$", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.RightToLeft);

                        int hs = m.Groups["hours"].Success ? int.Parse(m.Groups["hours"].Value) : 0;
                        int ms = m.Groups["minutes"].Success ? int.Parse(m.Groups["minutes"].Value) : 0;
                        int ss = m.Groups["seconds"].Success ? int.Parse(m.Groups["seconds"].Value) : 0;

                        video.videoDuration = TimeSpan.FromSeconds(hs * 60 * 60 + ms * 60 + ss);
                        videoinfoArray.Add(video);

                    }
                    catch
                    {

                    }


                }
                return videoinfoArray;
            }
            else
            {
                return new List<TwitchVideo>();
            }

        }
        #endregion
    }

    public class TwitchCreator
    {
        public enum Channel_result
        {
            Parsed,
            Failed
        }
        public Channel_result is_Parsed { get; private set; }
        public string Username { get; set; }
        public string User_ID_Code { get; set; }
        public bool Live_Now { get; set; }
        public string Thumbnail_Link { get; set; }
        public string Stream_Title { get; set; }
        public DateTime Stream_Start { get; set; }
        public List<TwitchVideo> Channel_Saved_Videos { get; set; }

        public DateTime Check_Against { get; set; }
        public void Parse_Channel_Data(string ChannelJSON)
        {
            try
            {
                System.Diagnostics.Debug.Print("Reading channel JSON : " + ChannelJSON);
                int start = 0;
                int end = 0;

                start = ChannelJSON.IndexOf("display_name") + 15;
                end = ChannelJSON.IndexOf("game_id") - 3;

                Username = ChannelJSON.Substring(start, end - start);

                start = ChannelJSON.IndexOf("id", end + 13) + 5;
                end = ChannelJSON.IndexOf("is_live") - 3;

                User_ID_Code = ChannelJSON.Substring(start, end - start);

                start = ChannelJSON.IndexOf("is_live", start) + 9;
                end = ChannelJSON.IndexOf(",", start);

                if (ChannelJSON.Substring(start, end - start) == "true")
                {
                    Live_Now = true;
                }
                else
                {
                    Live_Now = false;
                }

                start = ChannelJSON.IndexOf("thumbnail_url", end) + 16;
                end = ChannelJSON.IndexOf("\",\"title", start);

                Thumbnail_Link = ChannelJSON.Substring(start, end - start);

                start = ChannelJSON.IndexOf("title", end) + 8;
                end = ChannelJSON.IndexOf("started_at", start) - 3;

                Stream_Title = ChannelJSON.Substring(start, end - start);

                if (Live_Now)
                {
                    start = ChannelJSON.IndexOf("started_at") + 13;
                    end = ChannelJSON.Length - 1;
                    string tempHold = ChannelJSON.Substring(start, end - start);

                    Stream_Start = Convert.ToDateTime(tempHold);

                    //System.Diagnostics.Debug.Print(ChannelJSON.ToString());

                }
                else
                {

                }

                is_Parsed = Channel_result.Parsed;
            }
            catch
            {
                is_Parsed = Channel_result.Failed;
            }
        }
    }

    public class TwitchVideo
    {
        public string videoID { get; set; }
        public string videoTitle { get; set; }
        public string videoDescription { get; set; }
        public DateTime videoCreated { get; set; }
        public DateTime videoPublished { get; set; }
        public string videoLink { get; set; }
        public string videoThumbnial { get; set; }
        public string videoViews { get; set; }
        public TimeSpan videoDuration { get; set; }
    }
}
