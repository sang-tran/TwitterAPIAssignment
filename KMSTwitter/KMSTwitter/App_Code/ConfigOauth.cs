using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace KMSTwitter.App_Code
{
    // Class ConfigOauth define some field to Auth
    public class ConfigOauth
    {
        public string OauthToken { get; set; }
        public string OauthTokenSecret { get; set; }
        public string OauthConsumerKey { get; set; }
        public string OauthConsumerSecret { get; set; }

        // Get value from WebConfig
        public ConfigOauth()
        {
            OauthToken = ConfigurationManager.AppSettings["oauthToken"];
            OauthTokenSecret = ConfigurationManager.AppSettings["oauthTokenSecret"];
            OauthConsumerKey = ConfigurationManager.AppSettings["oauthConsumerKey"];
            OauthConsumerSecret = ConfigurationManager.AppSettings["oauthConsumerSecret"];
        }
    }
}