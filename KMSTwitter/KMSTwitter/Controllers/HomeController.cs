using KMSTwitter.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using KMSTwitter.App_Code;

namespace KMSTwitter.Controllers
{
    public class HomeController : Controller
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static TwitterAPI twitterAPI = new TwitterAPI();

        public ViewResult Index()
        {
            return View();
        }

        public ActionResult PostTweet(string text)
        {
            string com = Request["txtTweet"].ToString();
            if (string.IsNullOrWhiteSpace(com))
            {
                ViewBag.error = "Tweet is required";
                return RedirectToAction("Error", "Home");
            }
            else
            {
                twitterAPI.PostTweet(com);
            }
            return RedirectToAction("Index", "Home");
        }
        public ActionResult Refresh()
        {
            return RedirectToAction("Index", "Home");
        }

        public PartialViewResult Timeline()
        {
            List<Tweet> list = twitterAPI.GetListTweetObject();
            if (list == null)
            {
                Logger.Debug("GetTweet() return null");
                ViewBag.error("Error get list tweet");
                return PartialView();

            }
            else
            {
                return PartialView(list);
            }
        }
        [HttpGet]
        public ViewResult Error()
        {
            //ViewBag.error = "Tweet is required";
            return View();
        }
        [HttpPost]
        public ActionResult Error(string error)
        {
            return RedirectToAction("Index", "Home");
        }
    } 
}