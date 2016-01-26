using System;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace Bitboxx.DNNModules.BBImageStory.Models
{
    [Serializable]
    [TableName("BBImageStory_StoryLang")]
    [Cacheable("BBImageStory_StoryLang", CacheItemPriority.Normal, 20)]
    public class StoryLangInfo
    {
        public int? StoryId { get; set; }
        public string Language { get; set; }
        public string Title { get; set; }
        public string Story { get; set; }

    }

}