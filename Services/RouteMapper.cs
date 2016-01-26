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
using DotNetNuke.Web.Api;

/// <summary>
/// The Services namespace.
/// </summary>
namespace Bitboxx.DNNModules.BBImageStory.Services
{
    /// <summary>
    /// Class Routemapper.
    /// </summary>
    public class Routemapper : IServiceRouteMapper
    {
        /// <summary>
        /// Registers the routes.
        /// </summary>
        /// <param name="routeManager">The route manager.</param>
        public void RegisterRoutes(IMapRoute routeManager)
        {
            routeManager.MapHttpRoute("BBImageStory_Module", "default", "{controller}/{action}",
                    new[] { "Bitboxx.DNNModules.BBImageStory.Services" });
        }
    }
}