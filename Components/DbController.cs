/*
' Copyright (c) 2015  bitboxx solutions
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using DotNetNuke.Data;
using Bitboxx.DNNModules.BBImageStory.Models;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Log.EventLog;

namespace Bitboxx.DNNModules.BBImageStory.Components
{
    /// <summary>
    /// Class DbController.
    /// </summary>
    public class DbController
    {
        /// <summary>
        /// The _instance
        /// </summary>
        private static DbController _instance;
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static DbController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DbController();
                }
                return _instance;
            }
        }

        private static int _contentTypeId = -1;
        public static int ContentTypeId
        {
            get
            {
                if (_contentTypeId < 0)
                {
                    var typeController = new ContentTypeController();
                    var contentType = (from t in typeController.GetContentTypes() where t.ContentType == "BBImageStory" select t).FirstOrDefault();
                    if (contentType == null)
                    {
                        var newContenType = new ContentType("BBImageStory");
                        _contentTypeId = typeController.AddContentType(newContenType);
                    }
                    else
                    {
                        _contentTypeId = contentType.ContentTypeId;
                    }
                }
                return _contentTypeId;
            }
        }

        private const string Prefix = "BBImageStory_";

        /// <summary>
        /// Clears the cache.
        /// </summary>

        public void ClearCache(int moduleId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                try
                {
                    // Setup fictitious item to delete (just to clear the scope cache)
                    DeleteStory(-1);
                }
                catch { } // ignore
            }
        }
        

        #region Story 

        public IEnumerable<StoryInfo> GetStories(int moduleId, int portalId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                string sqlCmd = "SELECT * FROM {databaseOwner}[{objectQualifier}" + Prefix + "Story] story";
                string where = "";

                if (moduleId > -1 && portalId > -1)
                {
                    sqlCmd += " WHERE portalId = @0 AND moduleId = @1";
                    return ctx.ExecuteQuery<StoryInfo>(CommandType.Text, sqlCmd, portalId, moduleId);
                }

                if (moduleId > -1 && portalId == -1)
                {
                    sqlCmd += " WHERE moduleId = @0";
                    return ctx.ExecuteQuery<StoryInfo>(CommandType.Text, sqlCmd, moduleId);
                }

                if (moduleId == -1 && portalId > -1)
                {
                    sqlCmd += " WHERE portalId = @2";
                    return ctx.ExecuteQuery<StoryInfo>(CommandType.Text, sqlCmd, portalId);
                }

                return ctx.ExecuteQuery<StoryInfo>(CommandType.Text, sqlCmd);
            }
        }
        
        public IEnumerable<StoryInfo> GetStories(int moduleId, int portalId, string token, string language, bool ignoreTime)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                string sqlCmd = ";WITH cte as (SELECT StoryId, Title, Story FROM {databaseOwner}[{objectQualifier}" + Prefix + "StoryLang] WHERE Language = @1)" +
                                " SELECT story.*,cte.Title, cte.Story, " +
                                " (SELECT Username FROM {databaseOwner}[{objectQualifier}Users] users WHERE story.CreatedByUserID = users.UserID) AS CreatedByUserName," +
                                " (SELECT Username FROM {databaseOwner}[{objectQualifier}Users] users WHERE story.LastModifiedByUserID = users.UserID) AS LastModifiedByUserName," +
                                " (SELECT TOP 1 (dnnFiles.Folder + dnnFiles.FileName) " +
                                "   FROM {databaseOwner}[{objectQualifier}" + Prefix + "ForeignImage] ForeignImage" +
                                "   INNER JOIN {databaseOwner}[{objectQualifier}" + Prefix + "Image] [Image] ON ForeignImage.ImageId = Image.ImageId" +
                                "   INNER JOIN {databaseOwner}[{objectQualifier}Files] dnnFiles ON Image.FileId = dnnFiles.FileId" +
                                "       WHERE ForeignImage.ForeignId = story.StoryId AND ForeignImage.ForeignToken = @0 " +
                                "       ORDER BY ForeignImage.ViewOrder) AS StoryImage" +
                                " FROM {databaseOwner}[{objectQualifier}" + Prefix + "Story] story" +
                                " LEFT JOIN cte ON story.storyId = cte.StoryId";

                string where = ignoreTime ? " WHERE 1=1" : " WHERE (StartDate IS NULL OR StartDate < GetDate()) AND (EndDate IS NULL OR DATEADD(day,1,EndDate) >= GetDate())";
                string order = " ORDER BY story.CreatedOnDate DESC";

                if (moduleId > -1 && portalId > -1)
                {
                    //if (ignoreTime)
                    //    where = " AND (portalId = @2 OR portalId IS NULL) AND (moduleId = @3 OR moduleId IS NULL)";
                    //else
                        where += " AND portalId = @2 AND moduleId = @3";
                    return ctx.ExecuteQuery<StoryInfo>(CommandType.Text, sqlCmd + where + order, token ,language, portalId, moduleId);
                }
                
                if (moduleId > -1 && portalId == -1)
                {
                    //if (ignoreTime)
                    //    where = " AND (moduleId = @2 OR moduleId IS NULL)";
                    //else
                        where += " AND moduleId = @2";
                    return ctx.ExecuteQuery<StoryInfo>(CommandType.Text, sqlCmd + where + order, token, language, moduleId);
                }
                
                if (moduleId == -1 && portalId > -1)
                {
                    //if (ignoreTime)
                    //    where = " AND (portalId = @2 OR portalId IS NULL)";
                    //else
                        where += " AND portalId = @2";
                    return ctx.ExecuteQuery<StoryInfo>(CommandType.Text, sqlCmd + where + order, token, language, portalId);
                }

                return ctx.ExecuteQuery<StoryInfo>(CommandType.Text, sqlCmd + where + order, token, language);
            }
        }

        public StoryInfo GetLastStory(int moduleId, int portalId, string token, string language, bool ignoreTime)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                string sqlCmd = ";WITH cte as (SELECT StoryId, Title, Story FROM {databaseOwner}[{objectQualifier}" + Prefix + "StoryLang] WHERE Language = @1)" +
                                " SELECT TOP 1 story.*,cte.Title, cte.Story, " +
                                " (SELECT Username FROM {databaseOwner}[{objectQualifier}Users] users WHERE story.CreatedByUserID = users.UserID) AS CreatedByUserName," +
                                " (SELECT Username FROM {databaseOwner}[{objectQualifier}Users] users WHERE story.LastModifiedByUserID = users.UserID) AS LastModifiedByUserName," +
                                " (SELECT TOP 1 (dnnFiles.Folder + dnnFiles.FileName) " +
                                "   FROM {databaseOwner}[{objectQualifier}" + Prefix + "ForeignImage] ForeignImage" +
                                "   INNER JOIN {databaseOwner}[{objectQualifier}" + Prefix + "Image] [Image] ON ForeignImage.ImageId = Image.ImageId" +
                                "   INNER JOIN {databaseOwner}[{objectQualifier}Files] dnnFiles ON Image.FileId = dnnFiles.FileId" +
                                "       WHERE ForeignImage.ForeignId = story.StoryId AND ForeignImage.ForeignToken = @0 " +
                                "       ORDER BY ForeignImage.ViewOrder) AS StoryImage" +
                                " FROM {databaseOwner}[{objectQualifier}" + Prefix + "Story] story" +
                                " LEFT JOIN cte ON story.storyId = cte.StoryId";

                string where = ignoreTime ? " WHERE 1=1" : " WHERE (StartDate IS NULL OR StartDate < GetDate()) AND (EndDate IS NULL OR DATEADD(day,1,EndDate) >= GetDate())";
                string order = " ORDER BY story.CreatedOnDate DESC";

                if (moduleId > -1 && portalId > -1)
                {
                    where += " AND portalId = @2 AND moduleId = @3";
                    return ctx.ExecuteQuery<StoryInfo>(CommandType.Text, sqlCmd + where + order, token, language, portalId, moduleId).FirstOrDefault();
                }

                if (moduleId > -1 && portalId == -1)
                {
                    where += " AND moduleId = @2";
                    return ctx.ExecuteQuery<StoryInfo>(CommandType.Text, sqlCmd + where + order, token, language, moduleId).FirstOrDefault();
                }

                if (moduleId == -1 && portalId > -1)
                {
                    where += " AND portalId = @2";
                    return ctx.ExecuteQuery<StoryInfo>(CommandType.Text, sqlCmd + where + order, token, language, portalId).FirstOrDefault();
                }

                return ctx.ExecuteQuery<StoryInfo>(CommandType.Text, sqlCmd + where + order, token, language).FirstOrDefault();
            }
        }

        public StoryInfo GetStory(int storyId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                string sqlCmd = "SELECT * FROM {databaseOwner}[{objectQualifier}" + Prefix + "Story] story WHERE storyId = @0";
                return ctx.ExecuteQuery<StoryInfo>(CommandType.Text, sqlCmd, storyId).FirstOrDefault();
            }
        }
        
        public StoryInfo GetStory(int storyId, string token, string language, bool ignoreTime)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                string sqlCmd = ";WITH cte as (SELECT StoryId, Title, Story FROM {databaseOwner}[{objectQualifier}" + Prefix + "StoryLang] WHERE StoryId = @1 AND Language = @2)" +
                                " SELECT story.*,cte.Title, cte.Story, " +
                                "    (SELECT Username FROM {databaseOwner}[{objectQualifier}Users] users WHERE story.CreatedByUserID = users.UserID) AS CreatedByUserName, " +
                                "    (SELECT Username FROM {databaseOwner}[{objectQualifier}Users] users WHERE story.LastModifiedByUserID = users.UserID) AS LastModifiedByUserName, " +
                                "    (SELECT TOP 1 (dnnFiles.Folder + dnnFiles.FileName)" +
                                "       FROM {databaseOwner}[{objectQualifier}" + Prefix + "ForeignImage] ForeignImage" +
                                "       INNER JOIN {databaseOwner}[{objectQualifier}" + Prefix + "Image] [Image] ON ForeignImage.ImageId = Image.ImageId" +
                                "       INNER JOIN {databaseOwner}[{objectQualifier}Files] dnnFiles ON Image.FileId = dnnFiles.FileId" +
                                "       WHERE ForeignImage.ForeignId = story.StoryId" +
                                "       AND ForeignImage.ForeignToken = @0" +
                                "       ORDER BY ForeignImage.ViewOrder) AS StoryImage" +
                                " FROM {databaseOwner}[{objectQualifier}" + Prefix + "Story] story" +
                                " LEFT JOIN cte ON story.storyId = cte.StoryId" +
                                (ignoreTime ? " WHERE 1=1" : " WHERE (StartDate IS NULL OR StartDate < GetDate()) AND (EndDate IS NULL OR DATEADD(day,1,EndDate) >= GetDate())") + 
                                " AND story.StoryId = @1 ";
                
 
                return ctx.ExecuteQuery<StoryInfo>(CommandType.Text, sqlCmd , token, storyId, language).FirstOrDefault();
            }
        }

        public int InsertStory(StoryInfo story, int moduleId, int tabId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<StoryInfo>();
                rep.Insert(story);
                var contentItem = new ContentItem()
                                  {
                                      Content = GetStoryContent(story.StoryId),
                                      ContentTypeId = ContentTypeId,
                                      Indexed = false,
                                      ContentKey = "#view/" + story.StoryId.ToString(),
                                      ModuleID = moduleId,
                                      TabID = tabId
                                  };
                story.ContentItemID = Util.GetContentController().AddContentItem(contentItem);
                rep.Update(story);
                return story.StoryId;
            }
        }

        public int InsertStory(StoryInfo story, string language, int moduleId, int tabId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<StoryInfo>();
                rep.Insert(story);
                StoryLangInfo lang = new StoryLangInfo
                                     {
                                         StoryId = story.StoryId,
                                         Language = language,
                                         Story = story.Story,
                                         Title = story.Title
                                     };
                InsertStoryLang(lang, tabId);
                var contentItem = new ContentItem()
                {
                    Content = GetStoryContent(story.StoryId),
                    ContentTypeId = ContentTypeId,
                    Indexed = false,
                    ContentKey = "#view/" + story.StoryId.ToString(),
                    ModuleID = moduleId,
                    TabID = tabId
                };
                story.ContentItemID = Util.GetContentController().AddContentItem(contentItem);
                rep.Update(story);
                return story.StoryId;
            }
        }

        public void UpdateStory(StoryInfo story, string language, int tabId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                
                var rep = ctx.GetRepository<StoryInfo>();
                rep.Update(story);
                DeleteStoryLang(story.StoryId, language);
                StoryLangInfo lang = new StoryLangInfo
                {
                    StoryId = story.StoryId,
                    Language = language,
                    Story = story.Story,
                    Title = story.Title
                };
                InsertStoryLang(lang, tabId);
            }
        }

        public void DeleteStory(int storyId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<StoryInfo>();
                StoryInfo story = GetStory(storyId);
                if (story != null)
                {
                    try
                    {
                        if (story.ContentItemID != null)
                            Util.GetContentController().DeleteContentItem((int)story.ContentItemID);
                    }
                    catch (Exception ex)
                    {
                        EventLogController evCtrl = new EventLogController();
                        evCtrl.AddLog("BBImagestory","Error deleting story (contentItem): " + ex.ToString(), PortalSettings.Current,-1,EventLogController.EventLogType.ADMIN_ALERT );
                    }
                    try
                    {
                        rep.Delete(story);
                    }
                    catch (Exception ex)
                    {
                        EventLogController evCtrl = new EventLogController();
                        evCtrl.AddLog("BBImagestory", "Error deleting story: " + ex.ToString(), PortalSettings.Current, -1, EventLogController.EventLogType.ADMIN_ALERT);
                        throw;
                    }
                }
            }
        }

        public IEnumerable<StoryLangInfo> GetStoryLangs(int storyId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                string sqlCmd = "SELECT * FROM {databaseOwner}[{objectQualifier}" + Prefix + "StoryLang]" +
                                " WHERE StoryId = @0";
                return ctx.ExecuteQuery<StoryLangInfo>(CommandType.Text, sqlCmd, storyId);
            }
        }
        
        public void InsertStoryLang(StoryLangInfo storyLang, int tabId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                string sqlCmd = "INSERT INTO {databaseOwner}[{objectQualifier}" + Prefix + "StoryLang]" +
                                " (StoryId,Language,Title,Story) VALUES (@0,@1,@2,@3)";
                ctx.Execute(CommandType.Text, sqlCmd,storyLang.StoryId, storyLang.Language, storyLang.Title, storyLang.Story);
            }
            StoryInfo story = GetStory((int) storyLang.StoryId);
            if (story.ContentItemID != null)
            {
                ContentItem contentItem = Util.GetContentController().GetContentItem((int)story.ContentItemID);
                contentItem.TabID = tabId;
                contentItem.Content = GetStoryContent(story.StoryId);
                Util.GetContentController().UpdateContentItem(contentItem);
            }
        }
        
        
        public void DeleteStoryLang(int storyId, string language)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                string sqlCmd = "DELETE FROM {databaseOwner}[{objectQualifier}" + Prefix + "StoryLang] WHERE StoryId = @0 AND Language = @1";
                ctx.Execute(CommandType.Text, sqlCmd,storyId, language);
            }
        }

        public void SetStoryOrder(int itemId, int sort)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                string sql = "UPDATE {databaseOwner}[{objectQualifier}" + Prefix + "Story] SET Sort = @1 WHERE ItemId = @0";
                ctx.Execute(CommandType.Text, sql, itemId,sort);
            }
        }

        #endregion

        #region Images
        public ImageInfo GetImage(int imageId)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repository = context.GetRepository<ImageInfo>();
                return repository.GetById(imageId);
            }
        }

        public IEnumerable<ImageLocInfo> GetImagesByForeign(int foreignId, string token, string language)
        {
            using (IDataContext context = DataContext.Instance())
            {
                string sqlCmd = "; WITH cte as (SELECT ImageLang.ShortDescription, ImageLang.LongDescription, Image.ImageId " +
                                "   FROM {databaseOwner}[{objectQualifier}" + Prefix + "ForeignImage] ForeignImage" +
                                "   LEFT JOIN {databaseOwner}[{objectQualifier}" + Prefix + "Image] Image ON ForeignImage.ImageId = Image.ImageId " +
                                "   LEFT JOIN {databaseOwner}[{objectQualifier}" + Prefix + "ImageLang] ImageLang ON Image.ImageId = ImageLang.ImageId " +
                                "   WHERE ForeignImage.ForeignId = @0 " +
                                "   AND ForeignImage.ForeignToken = @1 " +
                                "   AND ImageLang.Language = @2)" +
                                " SELECT Image.ImageId, Image.TextPosition, cte.ShortDescription, cte.LongDescription,iFiles.FileName, " +
                                " iFiles.Extension , iFiles.Folder , iFiles.Height , iFiles.Width , iFiles.Size," +
                                " ForeignImage.ViewOrder, @0 AS ForeignId" +
                                " FROM {databaseOwner}[{objectQualifier}" + Prefix + "ForeignImage] ForeignImage" +
                                " LEFT JOIN cte ON ForeignImage.ImageId = cte.ImageId" +
                                " LEFT JOIN {databaseOwner}[{objectQualifier}" + Prefix + "Image] Image ON ForeignImage.ImageId = Image.ImageId " +
                                " LEFT JOIN {databaseOwner}[{objectQualifier}Files] iFiles ON Image.FileId = iFiles.FileId " +
                                " WHERE ForeignImage.ForeignId = @0 " +
                                " AND ForeignImage.ForeignToken = @1 " +
                                " ORDER BY ForeignImage.ViewOrder";
                
                return context.ExecuteQuery<ImageLocInfo>(CommandType.Text, sqlCmd, foreignId, token.ToUpper(), language);
            }
        }

        public IEnumerable<ImageLocInfo> GetImagesByForeign(int foreignId, string token)
        {
            using (IDataContext context = DataContext.Instance())
            {
                string sqlCmd = " SELECT Image.ImageId, Image.TextPosition, '' AS ShortDescription, '' AS LongDescription,iFiles.FileName, " +
                                " iFiles.Extension , iFiles.Folder , iFiles.Height , iFiles.Width , iFiles.Size," +
                                " ForeignImage.ViewOrder, @0 AS ForeignId" +
                                " FROM {databaseOwner}[{objectQualifier}" + Prefix + "ForeignImage] ForeignImage" +
                                " LEFT JOIN {databaseOwner}[{objectQualifier}" + Prefix + "Image] Image ON ForeignImage.ImageId = Image.ImageId " +
                                " LEFT JOIN {databaseOwner}[{objectQualifier}Files] iFiles ON Image.FileId = iFiles.FileId " +
                                " WHERE ForeignImage.ForeignId = @0 " +
                                " AND ForeignImage.ForeignToken = @1 " +
                                " ORDER BY ForeignImage.ViewOrder";

                return context.ExecuteQuery<ImageLocInfo>(CommandType.Text, sqlCmd, foreignId, token.ToUpper());
            }
        }

        public ImageLocInfo GetImageByForeign(int imageId, int foreignId, string token, string language)
        {
            using (IDataContext context = DataContext.Instance())
            {
                string sqlCmd = "; WITH cte as (SELECT ImageLang.ShortDescription, ImageLang.LongDescription, Image.ImageId " +
                                "   FROM {databaseOwner}[{objectQualifier}" + Prefix + "ForeignImage] ForeignImage" +
                                "   LEFT JOIN {databaseOwner}[{objectQualifier}" + Prefix + "Image] Image ON ForeignImage.ImageId = Image.ImageId " +
                                "   LEFT JOIN {databaseOwner}[{objectQualifier}" + Prefix + "ImageLang] ImageLang ON Image.ImageId = ImageLang.ImageId " +
                                "   WHERE ForeignImage.ForeignId = @1 " +
                                "   AND ForeignImage.ForeignToken = @2 " +
                                "   AND ImageLang.Language = @3)" +
                                " SELECT Image.ImageId, Image.TextPosition, cte.ShortDescription, cte.LongDescription,iFiles.FileName, " +
                                " iFiles.Extension , iFiles.Folder , iFiles.Height , iFiles.Width , iFiles.Size," +
                                " ForeignImage.ViewOrder, @0 AS ForeignId" +
                                " FROM {databaseOwner}[{objectQualifier}" + Prefix + "ForeignImage] ForeignImage" +
                                " LEFT JOIN cte ON ForeignImage.ImageId = cte.ImageId" +
                                " LEFT JOIN {databaseOwner}[{objectQualifier}" + Prefix + "Image] Image ON ForeignImage.ImageId = Image.ImageId" +
                                " LEFT JOIN {databaseOwner}[{objectQualifier}Files] iFiles ON Image.FileId = iFiles.FileId" +
                                " WHERE ForeignImage.ImageId = @0 AND ForeignImage.ForeignId = @1 AND ForeignImage.ForeignToken = @2" +
                                " ORDER BY ForeignImage.ViewOrder";
                return context.ExecuteQuery<ImageLocInfo>(CommandType.Text, sqlCmd, imageId, foreignId, token.ToUpper(), language).FirstOrDefault();
            }
        }

        public int InsertImage(ImageInfo image)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repository = context.GetRepository<ImageInfo>();
                repository.Insert(image);
                return image.ImageId;
            }
        }

        public void UpdateImage(ImageInfo image)
        {
            using (IDataContext context = DataContext.Instance())
            {
                var repository = context.GetRepository<ImageInfo>();
                repository.Update(image);
            }
        }

        public IEnumerable<ImageLangInfo> GetImageLangs(int imageId)
        {
            using (IDataContext context = DataContext.Instance())
            {
                string sqlCmd = "SELECT * FROM {databaseOwner}[{objectQualifier}" + Prefix + "ImageLang]"+
                    " WHERE ImageId = @0";
                return context.ExecuteQuery<ImageLangInfo>(CommandType.Text, sqlCmd, imageId);
            }
        }

        public void InsertImageLang(ImageLangInfo imageLang)
        {
            using (IDataContext context = DataContext.Instance())
            {
                string sqlCmd = "INSERT INTO {databaseOwner}[{objectQualifier}" + Prefix + "ImageLang] (ImageId,Language,ShortDescription,LongDescription) VALUES (@0,@1,@2,@3)";
                context.Execute(CommandType.Text, sqlCmd, imageLang.ImageId, imageLang.Language, imageLang.ShortDescription, imageLang.LongDescription);
            }
        }

        public void DeleteImageLang(ImageLangInfo imageLang)
        {
            using (IDataContext context = DataContext.Instance())
            {
                string sqlCmd = "DELETE FROM {databaseOwner}[{objectQualifier}" + Prefix + "ImageLang] WHERE ImageId = @0 AND Language = @1";
                context.Execute(CommandType.Text, sqlCmd, imageLang.ImageId, imageLang.Language);
            }
        }
        
        public void UpdateImageLang(ImageLangInfo imageLang)
        {
            using (IDataContext context = DataContext.Instance())
            {
                string sqlCmd = "UPDATE {databaseOwner}[{objectQualifier}" + Prefix + "ImageLang] SET ShortDescription = @2,LongDescription = @3 WHERE ImageId = @0 AND Language = @1";
                context.Execute(CommandType.Text, sqlCmd, imageLang.ImageId, imageLang.Language, imageLang.ShortDescription, imageLang.LongDescription);
            }
        }

        public void InsertForeignImage(ForeignImageInfo foreignImage)
        {
            using (IDataContext context = DataContext.Instance())
            {
                string sqlCmd = "INSERT INTO {databaseOwner}[{objectQualifier}" + Prefix + "ForeignImage] (ImageId,ForeignId,ForeignToken,ViewOrder) VALUES (@0,@1,@2,@3)";
                context.Execute(CommandType.Text, sqlCmd, foreignImage.ImageId, foreignImage.ForeignId, foreignImage.ForeignToken, foreignImage.ViewOrder);
            }
        }

        public void UpdateForeignImage(ForeignImageInfo foreignImage)
        {
            using (IDataContext context = DataContext.Instance())
            {
                string sqlCmd = "UPDATE {databaseOwner}[{objectQualifier}" + Prefix + "ForeignImage] SET ViewOrder = @3 WHERE ImageId = @0 AND ForeignId = @1 AND ForeignToken = @2";
                context.Execute(CommandType.Text, sqlCmd, foreignImage.ImageId, foreignImage.ForeignId, foreignImage.ForeignToken, foreignImage.ViewOrder);
            }
        }

        public void DeleteForeignImage(ForeignImageInfo foreignImage)
        {
            using (IDataContext context = DataContext.Instance())
            {
                string sqlCmd = "DELETE FROM {databaseOwner}[{objectQualifier}" + Prefix + "ForeignImage] WHERE ImageId = @0 AND ForeignId = @1 AND ForeignToken = @2 ";
                context.Execute(CommandType.Text, sqlCmd, foreignImage.ImageId, foreignImage.ForeignId, foreignImage.ForeignToken);
            }
        }

        public void DeleteForeignImages(int storyId, string token)
        {
            using (IDataContext context = DataContext.Instance())
            {
                string sqlCmd = "DELETE FROM {databaseOwner}[{objectQualifier}" + Prefix + "ForeignImage] WHERE ForeignId = @0 AND ForeignToken = @1 ";
                context.Execute(CommandType.Text, sqlCmd, storyId, token);
            }
        }

        #endregion

        #region Helper methods

        private string GetStoryContent(int storyId)
        {
            StoryInfo story = GetStory(storyId);
            story.Langs = GetStoryLangs(storyId).ToList();
            return new JavaScriptSerializer().Serialize(story);
        }

        #endregion
    }
}