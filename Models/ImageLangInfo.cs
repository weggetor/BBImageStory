using System;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace Bitboxx.DNNModules.BBImageStory.Models
{
    [Serializable]
    [TableName("BBImageStory_ImageLang")]
    [Cacheable("BBImageStory_ImageLang", CacheItemPriority.Normal, 20)]
    public class ImageLangInfo     
    {
        public int? ImageId { get; set; }
        public string Language { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }

    }
}