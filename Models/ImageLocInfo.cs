using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bitboxx.DNNModules.BBImageStory.Models
{
    public class ImageLocInfo
    {
        public int ImageId { get; set; }
        public int ForeignId { get; set; }
        public int TextPosition { get; set; }
        public string Folder { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
        public int Size { get; set; }
        public int ViewOrder { get; set; }
    }
}