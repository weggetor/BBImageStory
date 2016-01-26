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

using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Resources;
using System.Web;
using Bitboxx.DNNModules.BBImageStory.Components;
using Bitboxx.DNNModules.BBImageStory.Models;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Api;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;

namespace Bitboxx.DNNModules.BBImageStory.Services
{
    [SupportedModules("BBImageStory_Module")]
    public class StoryController : DnnApiController
    {
        /// <summary>
        /// API that creates a new item in the item list
        /// </summary>
        /// <returns></returns>
        [HttpPost] 
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage NewStory(StoryInfo story)
        {
            try
            {
                int storyId = DbController.Instance.InsertStory(story, System.Threading.Thread.CurrentThread.CurrentCulture.Name, ActiveModule.ModuleID, ActiveModule.TabID);
                return Request.CreateResponse(HttpStatusCode.OK, storyId);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        /// <summary>
        /// API that deletes an item from the item list
        /// </summary>
        /// <returns></returns>
        [HttpPost] 
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage DeleteStory(StoryInfo delItem)
        {
            try
            {
                string imgType = "STORY";
                string imageDir = "Images/BBImageStory/" + delItem.StoryId.ToString();
                IFolderInfo folder = FolderManager.Instance.GetFolder(ActiveModule.PortalID, imageDir);
                if (folder != null)
                {
                    IEnumerable<IFileInfo> files = FolderManager.Instance.GetFiles(folder);
                    FileManager.Instance.DeleteFiles(files);
                    FolderManager.Instance.DeleteFolder(folder);
                }
                DbController.Instance.DeleteStory(delItem.StoryId);
                DbController.Instance.DeleteForeignImages(delItem.StoryId,imgType);
                return Request.CreateResponse(HttpStatusCode.OK, true.ToString());
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, ex);
            }
        }

        /// <summary>
        /// API that creates a new item in the item list
        /// </summary>
        /// <returns></returns>
        [HttpPost] 
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage UpdateStory(StoryInfo story)
        {
            try
            {
                story.LastModifiedByUserID = UserInfo.UserID;
                story.LastModifiedOnDate = DateTime.Now;
                DbController.Instance.UpdateStory(story, System.Threading.Thread.CurrentThread.CurrentCulture.Name, ActiveModule.TabID);
                return Request.CreateResponse(HttpStatusCode.OK, true.ToString());
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        /// <summary>
        /// API that returns a story list
        /// </summary>
        /// <returns></returns>
        [HttpPost, HttpGet]
        [ValidateAntiForgeryToken]
        [ActionName("list")]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage GetStories(int moId, int poId, bool ignoreTime = true )
        {
            string imgType = "STORY";
            try
            {
                var stories = DbController.Instance.GetStories(moId, poId, imgType, System.Threading.Thread.CurrentThread.CurrentCulture.Name, ignoreTime);
                return Request.CreateResponse(HttpStatusCode.OK, stories);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        /// <summary>
        /// API that returns a single story 
        /// </summary>
        /// <returns></returns>
        [HttpPost, HttpGet]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage GetStory(int stId, int moId, int poId, bool ignoreTime = true)
        {
            string imgType = "STORY";
            try
            {
                var story = DbController.Instance.GetStory(stId, imgType, System.Threading.Thread.CurrentThread.CurrentCulture.Name, ignoreTime);
                if (story == null)
                    story = DbController.Instance.GetLastStory(moId, poId, imgType, System.Threading.Thread.CurrentThread.CurrentCulture.Name, ignoreTime);
                return Request.CreateResponse(HttpStatusCode.OK, story);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}