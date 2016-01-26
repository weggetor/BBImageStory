using System.Collections;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Web;
using System.Web.UI;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using Bitboxx.DNNModules.BBImageStory.Components;
using Bitboxx.DNNModules.BBImageStory.Models;
using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Search;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.Modules;
using DotNetNuke.UI.Modules.Html5;
using DotNetNuke.UI.Utilities;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Globals = DotNetNuke.Common.Globals;

namespace Bitboxx.DNNModules.BBImageStory
{

    /// ----------------------------------------------------------------------------- 
    /// <summary> 
    /// The Controller class for BBImageStory 
    /// </summary> 
    /// <remarks> 
    /// </remarks> 
    /// <history> 
    /// </history> 
    /// ----------------------------------------------------------------------------- 

    [DNNtc.BusinessControllerClass()]
    public class BusinessController : ModuleSearchBase, ICustomTokenProvider, IPortable
    {
        private static BusinessController _instance;

        public static BusinessController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BusinessController();
                }
                return _instance;
            }
        }

        #region "Public Methods"

        public string[] GetTemplates(string type, string root)
        {
            List<string> files = new List<string>();
            string fileMask = "", path = "";
            switch (type.ToLower())
            {
                case "view":
                    path = root + "js\\View";
                    fileMask = "*.html";
                    break;
                case "list":
                    path = root + "js\\List";
                    fileMask = "*.html";
                    break;
                case "css":
                    path = root;
                    fileMask = "module.css";
                    break;
            }
            string[] dirFiles = Directory.GetFiles(path, fileMask);
            foreach (string dirFile in dirFiles)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(dirFile);
                files.Add(fi.Name.ToLower());
            }
            return files.ToArray();
        }

        #endregion

        #region Private methods

        private string GetImageData(ImageLocInfo image)
        {
            string file = PortalSettings.Current.HomeDirectoryMapPath + image.Folder + image.FileName;

            byte[] imageData = null;
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(file);
            long imageFileLength = fileInfo.Length;
            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            imageData = br.ReadBytes((int) imageFileLength);
            return Convert.ToBase64String(imageData);
        }

        private MemoryStream HexStringToStream(string base64String)
        {
            byte[] imageData = Convert.FromBase64String(base64String);
            return new MemoryStream(imageData);
        }

        #endregion

        public IDictionary<string, IPropertyAccess> GetTokens(Page page, ModuleInstanceContext moduleContext)
        {
            var tokens = new Dictionary<string, IPropertyAccess>();
            tokens["moduleproperties"] = new ModulePropertiesPropertyAccess(moduleContext);
            return tokens;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ExportModule implements the IPortable ExportModule Interface
        /// </summary>
        /// <param name="moduleId">The Id of the module to be exported</param>
        /// -----------------------------------------------------------------------------
        public string ExportModule(int moduleId)
        {
            string strXML = "";
            int partModuleId = -1, partPortalId = -1;


            ModuleInfo module = ModuleController.Instance.GetModule(moduleId, PortalSettings.Current.ActiveTab.TabID, true);
            switch ((string) module.ModuleSettings["Partitioning"])
            {
                case "1":
                    partModuleId = moduleId;
                    break;
                case "2":
                    partPortalId = PortalSettings.Current.PortalId;
                    break;

            }

            List<StoryInfo> stories = DbController.Instance.GetStories(partModuleId, partPortalId).ToList();
            if (stories.Any())
            {
                strXML += "<BBImageStories>";

                string templateRoot = PortalSettings.Current.HomeDirectoryMapPath + "..\\..\\DesktopModules\\" + module.DesktopModule.FolderName + "\\";
                string listTemplateFile = templateRoot + "js\\List\\" + (string)module.ModuleSettings["List"];
                string viewTemplateFile = templateRoot + "js\\View\\" + (string)module.ModuleSettings["View"];

                if (File.Exists(listTemplateFile))
                {
                    strXML += "<ListTemplate>" +
                              "<Name>" + XmlUtils.XMLEncode((string) module.ModuleSettings["List"]) + "</Name>" +
                              "<Content>" + XmlUtils.XMLEncode(File.ReadAllText(listTemplateFile)) + "</Content>" +
                              "</ListTemplate>";
                }

                if (File.Exists(viewTemplateFile))
                {
                    strXML += "<ViewTemplate>" +
                              "<Name>" + XmlUtils.XMLEncode((string)module.ModuleSettings["View"]) + "</Name>" +
                              "<Content>" + XmlUtils.XMLEncode(File.ReadAllText(viewTemplateFile)) + "</Content>" +
                              "</ViewTemplate>";
                }

                strXML += "<ImageWidth>" + XmlUtils.XMLEncode((string)module.ModuleSettings["Width"]) + "</ImageWidth>";
                // strXML += "<Partitioning>" + XmlUtils.XMLEncode((string)module.ModuleSettings["Partitioning"]) + "</Partitioning>";

                foreach (StoryInfo story in stories)
                {
                    strXML += "<Story>";
                    strXML += (story.PortalId == null) ? "<PortalId>null</PortalId>" : "<PortalId>" + XmlUtils.XMLEncode(story.PortalId.ToString()) + "</PortalId>";
                    strXML += (story.ModuleId == null) ? "<ModuleId>null</ModuleId>" : "<ModuleId>" + XmlUtils.XMLEncode(story.ModuleId.ToString()) + "</ModuleId>";
                    strXML += (story.StartDate == null) ? "<StartDate>null</StartDate>" : "<StartDate>" + ((DateTime) story.StartDate).ToString("d", CultureInfo.InvariantCulture) + "</StartDate>";
                    strXML += (story.EndDate == null) ? "<EndDate>null</EndDate>" : "<EndDate>" + ((DateTime) story.EndDate).ToString("d", CultureInfo.InvariantCulture) + "</EndDate>";
                    strXML += "<StoryLangs>";
                    List<StoryLangInfo> storyLangs = DbController.Instance.GetStoryLangs(story.StoryId).ToList();
                    foreach (StoryLangInfo storyLang in storyLangs)
                    {
                        strXML += "<StoryLang>";
                        strXML += "<Language>" + storyLang.Language + "</Language>";
                        strXML += "<Title>" + XmlUtils.XMLEncode(storyLang.Title) + "</Title>";
                        strXML += "<Story>" + XmlUtils.XMLEncode(storyLang.Story) + "</Story>";
                        strXML += "</StoryLang>";
                    }
                    strXML += "</StoryLangs>";
                    strXML += "<Images>";
                    List<ImageLocInfo> images = DbController.Instance.GetImagesByForeign(story.StoryId, "STORY").ToList();
                    foreach (ImageLocInfo image in images)
                    {
                        strXML += "<Image>";
                        strXML += "<ViewOrder>" + image.ViewOrder.ToString() + "</ViewOrder>";
                        strXML += "<FileName>" + image.FileName + "</FileName>";
                        strXML += "<ImageData>" + GetImageData(image) + "</ImageData>";

                        List<ImageLangInfo> imageLangs = DbController.Instance.GetImageLangs(image.ImageId).ToList();
                        strXML += "<ImageLangs>";
                        foreach (ImageLangInfo imageLang in imageLangs)
                        {
                            strXML += "<ImageLang>";
                            strXML += "<Language>" + imageLang.Language + "</Language>";
                            strXML += "<ShortDescription>" + XmlUtils.XMLEncode(imageLang.ShortDescription) + "</ShortDescription>";
                            strXML += "<LongDescription>" + XmlUtils.XMLEncode(imageLang.LongDescription) + "</LongDescription>";
                            strXML += "</ImageLang>";
                        }
                        strXML += "</ImageLangs>";
                        strXML += "</Image>";
                    }
                    strXML += "</Images>";
                    strXML += "</Story>";
                }
                strXML += "</BBImageStories>";
            }

            return strXML;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ImportModule implements the IPortable ImportModule Interface
        /// </summary>
        /// <param name="ModuleID">The Id of the module to be imported</param>
        /// <param name="Content">The content to be imported</param>
        /// <param name="Version">The version of the module to be imported</param>
        /// <param name="UserId">The Id of the user performing the import</param>
        /// -----------------------------------------------------------------------------
        public void ImportModule(int ModuleID, string Content, string Version, int UserID)
        {
            int? partModuleId = null, partPortalId = null;
            
            ModuleInfo module = ModuleController.Instance.GetModule(ModuleID, PortalSettings.Current.ActiveTab.TabID, true);
            switch ((string)module.ModuleSettings["Partitioning"])
            {
                case "1":
                    partModuleId = ModuleID;
                    break;
                case "2":
                    partPortalId = PortalSettings.Current.PortalId;
                    break;

            }
            
            XmlNode xmlroot = Globals.GetContent(Content, "BBImageStories");

            // Save Templates
            string templateRoot = PortalSettings.Current.HomeDirectoryMapPath + "..\\..\\DesktopModules\\" + module.DesktopModule.FolderName + "\\";

            XmlNode xmlListTemplate = xmlroot.SelectSingleNode("ListTemplate");
            string contentListTemplate = xmlListTemplate.SelectSingleNode("Content").InnerText;
            string contentListName = xmlListTemplate.SelectSingleNode("Name").InnerText;
            string listTemplateFile = templateRoot + "js\\List\\" + contentListName;
            File.WriteAllText(listTemplateFile, contentListTemplate);
            ModuleController.Instance.UpdateModuleSetting(ModuleID, "List", contentListName.ToLower());

            XmlNode xmlViewTemplate = xmlroot.SelectSingleNode("ViewTemplate");
            string contentViewTemplate = xmlViewTemplate.SelectSingleNode("Content").InnerText;
            string contentViewName = xmlViewTemplate.SelectSingleNode("Name").InnerText;
            string viewTemplateFile = templateRoot + "js\\View\\" + contentViewName;
            File.WriteAllText(viewTemplateFile, contentViewTemplate);
            ModuleController.Instance.UpdateModuleSetting(ModuleID, "View", contentViewName.ToLower());

            XmlNode xmlImageWidth = xmlroot.SelectSingleNode("ImageWidth");
            if (xmlImageWidth != null)
                ModuleController.Instance.UpdateModuleSetting(ModuleID, "Width", xmlImageWidth.InnerText);

            //XmlNode xmlPartitioning = xmlroot.SelectSingleNode("Partitioning");
            //if (xmlPartitioning != null)
            //    ModuleController.Instance.UpdateModuleSetting(ModuleID, "Partitioning", xmlPartitioning.InnerText);

            foreach (XmlNode xmlStory in xmlroot.SelectNodes("Story"))
            {
                // Insert Story
                string strModuleId = xmlStory.SelectSingleNode("ModuleId").InnerText;
                string strPortalId = xmlStory.SelectSingleNode("PortalId").InnerText;
                string strStartDate = xmlStory.SelectSingleNode("StartDate").InnerText;
                string strEndDate = xmlStory.SelectSingleNode("EndDate").InnerText;
                StoryInfo story = new StoryInfo()
                                  {
                                      ModuleId = partModuleId,
                                      PortalId = partPortalId,
                                      StartDate = (strStartDate != "null") ? (DateTime?) Convert.ToDateTime(strStartDate, CultureInfo.InvariantCulture) : null,
                                      EndDate = (strEndDate != "null") ? (DateTime?) Convert.ToDateTime(strEndDate, CultureInfo.InvariantCulture) : null,
                                      LastPublishedDate = null,
                                      CreatedByUserID = UserID,
                                      CreatedOnDate = DateTime.Now,
                                      LastModifiedByUserID = UserID,
                                      LastModifiedOnDate = DateTime.Now
                                  };
                int storyId = DbController.Instance.InsertStory(story, ModuleID, PortalSettings.Current.ActiveTab.TabID);

                // Insert Story Language Info
                XmlNode xmlStoryLangs = xmlStory.SelectSingleNode("StoryLangs");
                foreach (XmlNode xmlStoryLang in xmlStoryLangs.SelectNodes("StoryLang"))
                {
                    string strLanguage = xmlStoryLang.SelectSingleNode("Language").InnerText;
                    string strTitle = xmlStoryLang.SelectSingleNode("Title").InnerText;
                    string strStory = xmlStoryLang.SelectSingleNode("Story").InnerText;
                    StoryLangInfo storyLang = new StoryLangInfo()
                                              {
                                                  StoryId = storyId,
                                                  Language = strLanguage,
                                                  Title = strTitle,
                                                  Story = strStory
                                              };
                    DbController.Instance.InsertStoryLang(storyLang, PortalSettings.Current.ActiveTab.TabID);
                }

                // Insert Images
                string imageDir = "Images/BBImageStory/" + storyId.ToString();

                IFolderInfo folder;
                if (!FolderManager.Instance.FolderExists(PortalSettings.Current.PortalId, imageDir))
                    folder = FolderManager.Instance.AddFolder(PortalSettings.Current.PortalId, imageDir);
                else
                    folder = FolderManager.Instance.GetFolder(PortalSettings.Current.PortalId, imageDir);

                XmlNode xmlImages = xmlStory.SelectSingleNode("Images");
                foreach (XmlNode xmlImage in xmlImages.SelectNodes("Image"))
                {
                    string strViewOrder = xmlImage.SelectSingleNode("ViewOrder").InnerText;
                    string strImageData = xmlImage.SelectSingleNode("ImageData").InnerText;
                    string strFileName = xmlImage.SelectSingleNode("FileName").InnerText;
                    MemoryStream imageStream = HexStringToStream(strImageData);
                    if (!FileManager.Instance.FileExists(folder, strFileName))
                    {
                        // Add File to directory
                        IFileInfo ifile = FileManager.Instance.AddFile(folder, strFileName, imageStream, true);

                        // Insert Image reference into Image Table
                        ImageInfo image = new ImageInfo() {FileId = ifile.FileId};
                        int imageId = DbController.Instance.InsertImage(image);

                        // Combine image and story in crosstable
                        ForeignImageInfo foreignImage = new ForeignImageInfo() {ForeignId = storyId, ForeignToken = "STORY", ImageId = imageId, ViewOrder = Convert.ToInt32(strViewOrder)};
                        DbController.Instance.InsertForeignImage(foreignImage);

                        XmlNode xmlImageLangs = xmlImage.SelectSingleNode("ImageLangs");
                        foreach (XmlNode xmlImageLang in xmlImageLangs.SelectNodes("ImageLang"))
                        {
                            string strLanguage = xmlImageLang.SelectSingleNode("Language").InnerText;
                            string strShortDescription = xmlImageLang.SelectSingleNode("ShortDescription").InnerText;
                            string strLongDescription = xmlImageLang.SelectSingleNode("LongDescription").InnerText;
                            ImageLangInfo imageLang = new ImageLangInfo()
                                                      {
                                                          ImageId = imageId,
                                                          Language = strLanguage,
                                                          ShortDescription = strShortDescription,
                                                          LongDescription = strLongDescription
                                                      };
                            DbController.Instance.InsertImageLang(imageLang);

                        }
                    }
                }
            }
        }



        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo modInfo, DateTime beginDate)
        {
            string partition = (string)modInfo.ModuleSettings["Partitioning"];

            int partModuleId = -1;
            int partPortalId = -1;

            switch (partition)
            {
                case "1":
                    partModuleId = modInfo.ModuleID;
                    break;
                case "2":
                    partPortalId = modInfo.PortalID;
                    break;
                case "3":
                    partPortalId = modInfo.PortalID;
                    partModuleId = modInfo.ModuleID;
                    break;
            }

            var searchDocuments = new List<SearchDocument>();

            LocaleController lc = new LocaleController();
            Dictionary<string, Locale> loc = lc.GetLocales(modInfo.PortalID);
            foreach (KeyValuePair<string, Locale> item in loc)
            {
                string cultureCode = item.Value.Culture.Name;
                List<StoryInfo> stories = DbController.Instance.GetStories(partModuleId, partPortalId, "STORY", cultureCode, false).ToList();
                

                foreach (StoryInfo story in stories)
                {
                    DateTime lastmodified = ((DateTime)story.LastModifiedOnDate).ToUniversalTime();

                    if (lastmodified > beginDate.ToUniversalTime() && lastmodified < DateTime.UtcNow)
                    {
                        var strContent = HtmlUtils.Clean(story.Story, false);

                        // Get the description string
                        var description = strContent.Length <= 500 ? strContent : HtmlUtils.Shorten(strContent, 500, "...");

                        var searchDoc = new SearchDocument
                        {
                            UniqueKey = story.StoryId.ToString(),
                            PortalId = modInfo.PortalID,
                            ModuleId = modInfo.ModuleID,
                            ModuleDefId = modInfo.ModuleDefID,
                            Title = story.Title,
                            Description = description,
                            Body = strContent,
                            ModifiedTimeUtc = lastmodified,
                            AuthorUserId = (story.CreatedByUserID ?? -1),
                            CultureCode = cultureCode,
                            IsActive = (story.StartDate == null || story.StartDate < DateTime.Now) && (story.EndDate == null || story.EndDate + new TimeSpan(1, 0, 0, 0) >= DateTime.Now),
                            SearchTypeId = 1,
                            QueryString = "#view/"+story.StoryId.ToString()
                        };

                        if (modInfo.Terms != null && modInfo.Terms.Count > 0)
                        {
                            searchDoc.Tags = CollectHierarchicalTags(modInfo.Terms);
                        }

                        searchDocuments.Add(searchDoc);
                    }
                }

            }
            return searchDocuments;
        }

        private static List<string> CollectHierarchicalTags(List<Term> terms)
        {
            Func<List<Term>, List<string>, List<string>> collectTagsFunc = null;
            collectTagsFunc = (ts, tags) =>
            {
                if (ts != null && ts.Count > 0)
                {
                    foreach (var t in ts)
                    {
                        tags.Add(t.Name);
                        tags.AddRange(collectTagsFunc(t.ChildTerms, new List<string>()));
                    }
                }
                return tags;
            };

            return collectTagsFunc(terms, new List<string>());
        }


    }

    class ModulePropertiesPropertyAccess : IPropertyAccess
    {
        private ModuleInstanceContext _moduleContext ;
        public ModulePropertiesPropertyAccess(ModuleInstanceContext moduleContext)
        {
            _moduleContext = moduleContext;
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            string retVal = "";
            switch (propertyName.ToLower())
            {
                case "all":
                    int moduleId = _moduleContext.ModuleId;
                    int portalId = _moduleContext.PortalId;
                    int tabId = _moduleContext.TabId;
                    ModuleInfo module = new ModuleController().GetModule(moduleId, tabId);

                    dynamic properties = new ExpandoObject();
                    System.IO.FileInfo fi = new System.IO.FileInfo(HttpContext.Current.Server.MapPath("~/" + _moduleContext.Configuration.ModuleControl.ControlSrc.Replace(".html", "") + ".resx"));
                    string physResourceFile = fi.DirectoryName + "/App_LocalResources/" + fi.Name;
                    string relResourceFile = "/DesktopModules/" + module.DesktopModule.FolderName + "/App_LocalResources/" + fi.Name;
                    if (File.Exists(physResourceFile))
                    {
                        using (var rsxr = new ResXResourceReader(physResourceFile))
                        {
                            var res = rsxr.OfType<DictionaryEntry>()
                                .ToDictionary(
                                    entry => entry.Key.ToString().Replace(".", "_"),
                                    entry => Localization.GetString(entry.Key.ToString(), relResourceFile));

                            properties.Resources = res;
                        }
                    }
                    else
                    {
                        properties.Resources = physResourceFile + " not found";
                    }
                    properties.Settings = _moduleContext.Settings;
                    properties.Editable = _moduleContext.EditMode && _moduleContext.IsEditable;
                    properties.Admin = accessingUser.IsInRole(PortalSettings.Current.AdministratorRoleName);
                    properties.ModuleId = moduleId;
                    properties.PortalId = portalId;
                    properties.UserId = accessingUser.UserID;
                    properties.HomeDirectory = PortalSettings.Current.HomeDirectory.Substring(1);
                    properties.RawUrl = HttpContext.Current.Request.RawUrl;

                    List<string> languages = new List<string>();
                    LocaleController lc = new LocaleController();
                    Dictionary<string, Locale> loc = lc.GetLocales(_moduleContext.PortalId);
                    foreach (KeyValuePair<string, Locale> item in loc)
                    {
                        string cultureCode = item.Value.Culture.Name;
                        languages.Add(cultureCode);
                    }
                    properties.Languages = languages;
                    properties.CurrentLanguage = System.Threading.Thread.CurrentThread.CurrentCulture.Name;

                    retVal = JsonConvert.SerializeObject(properties);
                    break;
                case "view":
                    retVal = (string)_moduleContext.Settings["View"];
                    if (String.IsNullOrEmpty(retVal))
                        retVal = "View.html";
                    break;
                case "list":
                    retVal = (string)_moduleContext.Settings["List"];
                    if (String.IsNullOrEmpty(retVal))
                        retVal = "List.html";
                    break;
            }
            return retVal;

        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }
    }
}