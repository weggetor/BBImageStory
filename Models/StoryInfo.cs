using System;
using System.Collections.Generic;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace Bitboxx.DNNModules.BBImageStory.Models
{
    [TableName("BBImageStory_Story")]
    [PrimaryKey("StoryId")]
    [Cacheable("BBImageStory_Story", CacheItemPriority.Normal, 20)]
    public class StoryInfo     
    {
        public StoryInfo()
        {
            Langs = new List<StoryLangInfo>();
        }
        public int StoryId { get; set; }
        public int? ModuleId { get; set; }
        public int? PortalId { get; set; }
        public int? ContentItemID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? LastPublishedDate { get; set; }
        public int? CreatedByUserID { get; set; }
        public DateTime? CreatedOnDate { get; set; }
        public int? LastModifiedByUserID { get; set; }
        public DateTime? LastModifiedOnDate { get; set; }
        [ReadOnlyColumn]
        public string CreatedByUserName { get; set; }
        [ReadOnlyColumn]
        public string LastModifiedByUserName { get; set; }
        [ReadOnlyColumn]
        public string StoryImage { get; set; }
        [ReadOnlyColumn]
        public string Title { get; set; }
        [ReadOnlyColumn]
        public string Story { get; set; }
        [ReadOnlyColumn]
        public List<StoryLangInfo> Langs { get; set; }
    }
}