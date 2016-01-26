using System.Collections.Generic;
using System.Web;
using Bitboxx.DNNModules.BBImageStory.Components;
using Bitboxx.DNNModules.BBImageStory.Models;
using DotNetNuke.Collections;
using DotNetNuke.Entities.Modules;
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
    public class SettingsController : DnnApiController
    {
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpGet, HttpPost]
        public HttpResponseMessage LoadSettings()
        {
            try
            {
                string root = PortalSettings.HomeDirectoryMapPath + "..\\..\\DesktopModules\\" + ActiveModule.DesktopModule.FolderName + "\\";

                SettingsInfo settings = new SettingsInfo();
                settings.ImageWidth = Convert.ToInt32(ActiveModule.ModuleSettings.GetValueOrDefault("Width", 600));
                settings.Partitioning = ActiveModule.ModuleSettings.GetValueOrDefault("Partitioning", "1");
                settings.List = ActiveModule.ModuleSettings.GetValueOrDefault("List", "list.html");
                settings.View = ActiveModule.ModuleSettings.GetValueOrDefault("View", "view.html");
                settings.ListTemplates = BusinessController.Instance.GetTemplates("list", root);
                settings.ViewTemplates = BusinessController.Instance.GetTemplates("view", root);
                return Request.CreateResponse(HttpStatusCode.OK, settings);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage SaveSettings(SettingsInfo settings)
        {
            try
            {
                ModuleController.Instance.UpdateModuleSetting(ActiveModule.ModuleID, "Width", settings.ImageWidth.ToString());
                ModuleController.Instance.UpdateModuleSetting(ActiveModule.ModuleID, "Partitioning", settings.Partitioning);
                ModuleController.Instance.UpdateModuleSetting(ActiveModule.ModuleID, "View", settings.View);
                ModuleController.Instance.UpdateModuleSetting(ActiveModule.ModuleID, "List", settings.List);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}