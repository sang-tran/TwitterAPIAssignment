using KMSTwitter.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace KMSTwitter.App_Code
{
    public class TwitterAPI
    {
        //Add logger declaration to make logs
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static ConfigOauth config = new ConfigOauth();
        private string oauthToken;
        private string oauthTokenSecret;
        private string oauthConsumerKey;
        private string oauthConsumerSecret;

        private string oauthVersion = "1.0";
        private string oauthSignatureMethod = "HMAC-SHA1";
        private string oauthNonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
        private static TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private string oauthTimeStamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
        private string oauthSignature = string.Empty;


        public TwitterAPI(string oauthToken, 
                            string oauthTokenSecret, 
                            string oauthConsumerKey, 
                            string oauthConsumerSecret,
                            string oauthVersion,
                            string oauthSignatureMethod,
                            string oauthNonce,
                            string oauthTimeStamp,
                            string oauthSignature
                            )
        {
            this.oauthToken = oauthToken;
            this.oauthTokenSecret = oauthConsumerKey;
            this.oauthConsumerKey = oauthConsumerKey;
            this.oauthConsumerSecret = oauthConsumerSecret;
            this.oauthVersion = oauthVersion;
            this.oauthSignatureMethod = oauthSignatureMethod;
            this.oauthNonce = oauthNonce;
            this.oauthTimeStamp = oauthTimeStamp;
            this.oauthSignature = oauthSignature;
        }

        public TwitterAPI()
        {
            oauthToken = config.OauthToken.ToString();
            oauthToken = config.OauthToken.ToString();
            oauthTokenSecret = config.OauthTokenSecret.ToString();
            oauthConsumerKey = config.OauthConsumerKey.ToString();
            oauthConsumerSecret = config.OauthConsumerSecret.ToString();

            oauthVersion = "1.0";
            oauthSignatureMethod = "HMAC-SHA1";
            oauthNonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            oauthTimeStamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
            oauthSignature = string.Empty;
        }



        #region Get list Tweet on home timeline
            /// <summary>
            /// Parst Json become Object
            /// </summary>
            /// <returns>List object Tweet to action PostTweet in HomeController</returns>
            public List<Tweet> GetListTweetObject()
            {
                Logger.Debug("Start GetListTweetObject()");

                string tweetListElement = GetListTweetJson();
                JArray objTweet = JArray.Parse(tweetListElement);
                List<Tweet> tweets = new List<Tweet>();
                foreach (JObject root in objTweet)
                {
                    Tweet tiweetItem = new Tweet();
                    tiweetItem.Id = root.SelectToken("id").ToString();
                    tiweetItem.Name = root.SelectToken("user").SelectToken("name").ToString();
                    tiweetItem.Status = root.SelectToken("text").ToString();
                    tiweetItem.ScreenName = root.SelectToken("user").SelectToken("screen_name").ToString();
                    tiweetItem.CreatedAt = root.SelectToken("user").SelectToken("created_at").ToString();

                    tweets.Add(tiweetItem);
                }

                Logger.Debug("End GetListTweetObject(). Return tweets = "+tweets.ToString());
                return tweets;
            }

            /// <summary>
            /// Get response Json from Twitter API
            /// </summary>
            /// <returns>ResponseData</returns>
            public string GetListTweetJson()
            {
                Logger.Debug("Start GetListTweetJson().");

                string timelineRequestUrl = string.Empty;
                string screenName = string.Empty;
                string baseString = string.Empty;
                CreateGetBaseString(out timelineRequestUrl, out screenName, out baseString);

                var authHeader = CreateGetTweetHeader(baseString);

                // Make the request
                ServicePointManager.Expect100Continue = false;
                var postBody = "screen_name=" + Uri.EscapeDataString(screenName);
                timelineRequestUrl += "?" + postBody;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(timelineRequestUrl);
                request.Headers.Add("Authorization", authHeader);
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";            
                string responseData = string.Empty;
                try
                {
                    WebResponse response = request.GetResponse();
                    responseData = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    Logger.Debug("End GetListTweetJson(). Return responseData.");
                    return responseData;
                }
                catch(WebException e)
                {
                    Logger.Error(e.Message);
                    return e.Message;
                }            
            }

            /// <summary>
            /// Format request data
            /// </summary>            
            private void CreateGetBaseString(out string timelineRequestUrl, out string screenName, out string baseString)
            {                
                timelineRequestUrl = "https://api.twitter.com/1.1/statuses/home_timeline.json";
                screenName = ConfigurationManager.AppSettings["screenName"];

                Logger.Debug("Start CreateGetBaseString(" + timelineRequestUrl.ToString() + "," + screenName.ToString() + ")");

                var baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                                "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&screen_name={6}";

                baseString = string.Format(baseFormat,
                                            oauthConsumerKey,
                                            oauthNonce,
                                            oauthSignatureMethod,
                                            oauthTimeStamp,
                                            oauthToken,
                                            oauthVersion,
                                            Uri.EscapeDataString(screenName)
                                            );
                baseString = string.Concat("GET&", Uri.EscapeDataString(timelineRequestUrl), "&", Uri.EscapeDataString(baseString));

                Logger.Debug("End CreateGetBaseString(). Return baseString = " + baseString.ToString());
            }

            /// <summary>
            /// Generate header request with encrypted oAuth signature which Twitter will use to validate the get request
            /// </summary>
            /// <returns>Header request</returns>
            private string CreateGetTweetHeader(string baseString)
            {
                Logger.Debug("Start CreateGetTweetHeader(" + baseString.ToString() + ")");

                var compositeKey = string.Concat(Uri.EscapeDataString(oauthConsumerSecret), "&", Uri.EscapeDataString(oauthTokenSecret));
                string oauthsignature = string.Empty;

                // Encrypt the data
                using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
                {
                    oauthsignature = Convert.ToBase64String(
                    hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
                }

                // Format Authentication header.
                var headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                                    "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                                    "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                                    "oauth_version=\"{6}\"";
                var authHeader = string.Format(headerFormat,
                                                Uri.EscapeDataString(oauthNonce),
                                                Uri.EscapeDataString(oauthSignatureMethod),
                                                Uri.EscapeDataString(oauthTimeStamp),
                                                Uri.EscapeDataString(oauthConsumerKey),
                                                Uri.EscapeDataString(oauthToken),
                                                Uri.EscapeDataString(oauthsignature),
                                                Uri.EscapeDataString(oauthVersion)
                                                );
                Logger.Debug("End CreateGetTweetHeader(). Return responseData");
                return authHeader;
            }
        #endregion


        #region Post tweet to timeline

            /// <summary>
            /// Post Tweet to Timeline
            /// </summary>
            /// <param name="txtText">Text of textarea in Index View</param>
            /// <returns>Json data response from Twitter API</returns>
            public string PostTweet(string txtText)
            {
                Logger.Debug("Start PostTweet(" + txtText.ToString() + ")");

                string resource_url;
                string status;
                string baseString;
                CreatePostBaseString(txtText, out resource_url, out status, out baseString);

                var authHeader = CreatePostTweetHeader(baseString);

                //Send request
                var postBody = "status=" + Uri.EscapeDataString(status);

                ServicePointManager.Expect100Continue = false;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resource_url);
                request.Headers.Add("Authorization", authHeader);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                using (Stream stream = request.GetRequestStream())
                {
                    byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
                    stream.Write(content, 0, content.Length);
                }            
                try
                {
                    WebResponse response = request.GetResponse();
                    Logger.Debug("End PostTweet(). Return response");
                    return response.ToString();
                }
                catch (WebException e)
                {
                    Logger.Error(e.Message);
                    return e.Message;
                }
            }
            
            /// <summary>
            /// Generate header post with encrypted oAuth signature which Twitter will use to validate the post request
            /// </summary>
            /// <param name="baseString"></param>
            /// <returns>Header post</returns>
            private string CreatePostTweetHeader(string baseString)
            {
                Logger.Debug("Start CreatePostTweetHeader(" + baseString.ToString() + ")");
                var compositeKey = string.Concat(Uri.EscapeDataString(oauthConsumerSecret), "&", Uri.EscapeDataString(oauthTokenSecret));

                // Encrypt the data
                using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
                {
                    oauthSignature = Convert.ToBase64String(
                        hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
                }
                // Format Authentication header.
                var headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                                   "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                                   "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                                   "oauth_version=\"{6}\"";

                var authHeader = string.Format(headerFormat,
                                        Uri.EscapeDataString(oauthNonce),
                                        Uri.EscapeDataString(oauthSignatureMethod),
                                        Uri.EscapeDataString(oauthTimeStamp),
                                        Uri.EscapeDataString(oauthConsumerKey),
                                        Uri.EscapeDataString(oauthToken),
                                        Uri.EscapeDataString(oauthSignature),
                                        Uri.EscapeDataString(oauthVersion)
                                        );
                Logger.Debug("End CreatePostTweetHeader(). Return authHeader.");
                return authHeader;
            }

            /// <summary>
            /// Format post data
            /// </summary>   
            private void CreatePostBaseString(string txtText, out string resource_url, out string status, out string baseString)
            {
                resource_url = "https://api.twitter.com/1.1/statuses/update.json";

                Logger.Debug("Start CreatePostBaseString(" + txtText.ToString() + "," + resource_url.ToString() + ")");

                //Text of textarea in Index View
                status = txtText;
                
                var baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                    "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&status={6}";

                baseString = string.Format(baseFormat,
                                            oauthConsumerKey,
                                            oauthNonce,
                                            oauthSignatureMethod,
                                            oauthTimeStamp,
                                            oauthToken,
                                            oauthVersion,
                                            Uri.EscapeDataString(status)
                                            );

                baseString = string.Concat("POST&", Uri.EscapeDataString(resource_url), "&", Uri.EscapeDataString(baseString));

                Logger.Debug("End CreatePostBaseString(). Return baseString.");
            }
        #endregion
    }
}