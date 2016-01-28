using DotNetNuke.Entities.Modules;

namespace Bitboxx.DNNModules.BBImageStory
{
    [DNNtc.ModuleDependencies(DNNtc.ModuleDependency.CoreVersion, "08.00.00")]
    [DNNtc.PackageProperties("BBImageStory_Module", 1, "BBImageStory Module", "DNN Module for creating image stories, step-by-step guides or image galleries with subtexts", "BBAngular.png", "Torsten Weggen", "bitboxx solutions", "http://www.bitboxx.net", "info@bitboxx.net", true)]
    [DNNtc.ModuleProperties("BBImageStory_Module", "BBImageStory Module", 0)]
    [DNNtc.ModuleControlProperties("", "BBImageStory", DNNtc.ControlType.View, "", false, false)]
    public partial class ViewDummy : PortalModuleBase
    {
 
    }
}