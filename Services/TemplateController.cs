using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Channels;
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
    public class TemplateController : DnnApiController
    {
        [HttpGet]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage LoadTemplate(string type, string name, bool def)
        {
            try
            {
                TemplateInfo template = new TemplateInfo() { Type = type };
                string root = PortalSettings.HomeDirectoryMapPath + "..\\..\\DesktopModules\\" + ActiveModule.DesktopModule.FolderName + "\\";
                string path = "";
                string defaultFile = "";

                switch (type.ToLower())
                {
                    case "view":
                        path = "js\\View\\";
                        template.Mode = "text/html";
                        defaultFile = "View.html.default";
                        template.FileName = name;
                        break;
                    case "list":
                        path = "js\\List\\";
                        defaultFile = "List.html.default";
                        template.Mode = "text/html";
                        template.FileName = name;
                        break;
                    case "css":
                        path = "";
                        defaultFile = "module.css.default";
                        template.Mode = "text/css";
                        template.FileName = "module.css";
                        break;
                }

                string physFile = root + path + (def ? defaultFile : template.FileName);
                if (File.Exists(physFile))
                    template.Content = File.ReadAllText(physFile);

                return Request.CreateResponse(HttpStatusCode.OK, template);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage GetTemplates(string type)
        {
            try
            {
                string root = PortalSettings.HomeDirectoryMapPath + "..\\..\\DesktopModules\\" + ActiveModule.DesktopModule.FolderName + "\\";
                string[] files = BusinessController.Instance.GetTemplates(type, root);
                return Request.CreateResponse(HttpStatusCode.OK, files);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage SaveTemplate(TemplateInfo template)
        {
            try
            {
                string root = PortalSettings.HomeDirectoryMapPath + "..\\..\\DesktopModules\\" + ActiveModule.DesktopModule.FolderName + "\\";
                string path = "";
                switch (template.Type.ToLower())
                {
                    case "view":
                        path = root + "js\\View\\";
                        break;
                    case "list":
                        path = root + "js\\List\\";
                        break;
                    case "css":
                        path = root;
                        break;
                }

                string physFile = path + template.FileName;
                File.WriteAllText(physFile, template.Content);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}