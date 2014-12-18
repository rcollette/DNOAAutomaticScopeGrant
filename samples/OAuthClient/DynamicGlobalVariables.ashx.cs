using System.Web;

namespace CMS.Web
{
    /// <summary>
    /// Summary description for DynamicGlobalVariables
    /// </summary>
    public class DynamicGlobalVariables : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var response = context.Response;
            response.ContentType = "application/javascript";
            response.Write("applicationRoot='" + VirtualPathUtility.ToAbsolute("~/") + "';");
            response.Cache.VaryByParams.IgnoreParams = true;
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}