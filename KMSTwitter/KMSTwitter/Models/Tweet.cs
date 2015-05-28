using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KMSTwitter.Models
{
    public class Tweet
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public string ScreenName { get; set; }
        public string CreatedAt { get; set; }
    }
}