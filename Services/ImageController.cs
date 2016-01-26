using System.Collections.Generic;
using System.Web;
using Bitboxx.DNNModules.BBImageStory.Components;
using Bitboxx.DNNModules.BBImageStory.Models;
using DotNetNuke.Security;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Api;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace Bitboxx.DNNModules.BBImageStory.Services
{
    [SupportedModules("BBImageStory_Module")]
    public class ImageController : DnnApiController
    {
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpGet, HttpPost]
        public HttpResponseMessage GetImagesByStory(int stId)
        {
            string imgType = "STORY";
            try
            {
                IEnumerable<ImageLocInfo> images = DbController.Instance.GetImagesByForeign(stId, imgType.ToUpper(), System.Threading.Thread.CurrentThread.CurrentCulture.Name);
                return Request.CreateResponse(HttpStatusCode.OK, images);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet,HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage DeleteImage(int stId, int imId)
        {
            string imgType = "STORY";
            string imageDir = "Images/BBImageStory/" + stId.ToString();
            try
            {
                IFolderInfo folder = FolderManager.Instance.GetFolder(ActiveModule.PortalID, imageDir);
                if (folder != null)
                {
                    ImageInfo img = DbController.Instance.GetImage(imId);
                    if (img != null)
                    {
                        IFileInfo file = FileManager.Instance.GetFile((int)img.FileId);
                        if (file != null)
                            FileManager.Instance.DeleteFile(file);
                    }
                }
                ForeignImageInfo fi = new ForeignImageInfo() { ForeignId = stId, ForeignToken = imgType.ToUpper(), ImageId = imId };
                DbController.Instance.DeleteForeignImage(fi);
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage UpdateImages(List<ImageLocInfo> images)
        {
            string imgType = "STORY";
            try
            {
                foreach (ImageLocInfo image in images)
                {
                    ImageInfo upImage = DbController.Instance.GetImage(image.ImageId);
                    upImage.TextPosition = image.TextPosition;
                    DbController.Instance.UpdateImage(upImage);

                    ImageLangInfo imageLang = new ImageLangInfo();
                    imageLang.ImageId = image.ImageId;
                    imageLang.Language = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
                    imageLang.LongDescription = image.LongDescription;
                    imageLang.ShortDescription = image.ShortDescription;
                    DbController.Instance.DeleteImageLang(imageLang);
                    DbController.Instance.InsertImageLang(imageLang);

                    ForeignImageInfo foreignImage = new ForeignImageInfo() { ForeignId = image.ForeignId, ForeignToken = imgType, ImageId = image.ImageId, ViewOrder = image.ViewOrder};
                    DbController.Instance.UpdateForeignImage(foreignImage);
                }
                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage UploadImage()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                int storyId = Convert.ToInt32(httpRequest["storyId"]);
                if (httpRequest.Files.Count > 0)
                {
                    string imageDir = "Images/BBImageStory/" + storyId.ToString();

                    IFolderInfo folder;
                    if (!FolderManager.Instance.FolderExists(ActiveModule.PortalID, imageDir))
                        folder = FolderManager.Instance.AddFolder(ActiveModule.PortalID, imageDir);
                    else
                        folder = FolderManager.Instance.GetFolder(ActiveModule.PortalID, imageDir);


                    // Save uploaded files to upload directory
                    for (int i = 0; i < httpRequest.Files.Count; i++)
                    {
                        var file = httpRequest.Files[i];

                        if (!FileManager.Instance.FileExists(folder, file.FileName))
                        {
                            // Add File to directory
                            IFileInfo ifile = FileManager.Instance.AddFile(folder, file.FileName, file.InputStream, true);

                            // Insert Image reference into Image Table
                            ImageInfo image = new ImageInfo() { FileId = ifile.FileId };
                            int imageId = DbController.Instance.InsertImage(image);

                            // Create empty language info for image
                            ImageLangInfo imageLang = new ImageLangInfo() { ImageId = imageId, Language = System.Threading.Thread.CurrentThread.CurrentCulture.Name, LongDescription = "", ShortDescription = "" };
                            DbController.Instance.InsertImageLang(imageLang);

                            // Combine image and story in crosstable
                            ForeignImageInfo foreignImage = new ForeignImageInfo() { ForeignId = storyId, ForeignToken = "STORY", ImageId = imageId, ViewOrder = 0 };
                            DbController.Instance.InsertForeignImage(foreignImage);
                        }
                    }

                    var retval = new HttpResponseMessage(HttpStatusCode.OK);
                    retval.Content = new StringContent("File transfer completed", System.Text.Encoding.UTF8, "text/plain");
                    return retval;
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}