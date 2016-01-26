using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace Bitboxx.DNNModules.BBImageStory.Models
{
    [TableName("BBImageStory_ForeignImage")]
    [Cacheable("BBImageStory_ForeignImage", CacheItemPriority.Normal, 20)]
    public class ForeignImageInfo     
    {
        public int? ImageId { get; set; }
        public int ForeignId { get; set; }
        public string ForeignToken { get; set; }
        public int ViewOrder { get; set; }

    }
}