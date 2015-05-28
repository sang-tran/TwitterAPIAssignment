using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KMSTwitter;
using KMSTwitter.App_Code;
using KMSTwitter.Models;
using System.Text;

namespace KMSTwitterTest
{
    [TestClass]
    public class TestPostTweet
    {
        private string oauthToken = "3220987741-Frh9ojnTJM8TRd3iGX8yQEJHHbfi4QDphA53Bc1";
        private string oauthTokenSecret = "KbZu89ruSAknBlaiyeC2oahPLJF7eAGs19WEaoVZmrg0C";
        private string oauthConsumerKey = "eMbFbshAw8EzIWcjmtsYEt3M1";
        private string oauthConsumerSecret = "2lfHaufd837nS9NICm1R33cMDuypUPm6sn0tf6yjfly3PxtoTU";

        private string oauthVersion = "1.0";
        private string oauthSignatureMethod = "HMAC-SHA1";
        private string oauthNonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
        private static TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private string oauthTimeStamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
        private string oauthSignature = string.Empty;
        private Random random = new Random();
        
        [TestMethod]
        public void PostTweet()
        {
            TwitterAPI tweet = new TwitterAPI(oauthToken, 
                oauthTokenSecret,
                oauthConsumerKey, 
                oauthConsumerSecret,
                oauthVersion,
                oauthSignatureMethod,
                oauthNonce,
                oauthTimeStamp,
                oauthSignature
                );
            string res = tweet.PostTweet("Auto post No: " + random.Next(1000000));

        }
    }
}
