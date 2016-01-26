using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace Bitboxx.DNNModules.BBImageStory.Models
{
    [TableName("BBImageStory_Image")]
    [PrimaryKey("ImageId")]
    [Cacheable("BBImageStory_Image", CacheItemPriority.Normal, 20)]
    public class ImageInfo     
    {
        public int ImageId { get; set; }
        public int? FileId { get; set; }
        public int? TextPosition { get; set; }
    }
}