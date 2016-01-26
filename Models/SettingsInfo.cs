using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bitboxx.DNNModules.BBImageStory.Models
{
    public class SettingsInfo
    {
        public int ImageWidth { get; set; }
        public string Partitioning { get; set; }
        public string View { get; set; }
        public string List { get; set; }
        public string[] ListTemplates { get; set; }
        public string[] ViewTemplates { get; set; }

    }
}