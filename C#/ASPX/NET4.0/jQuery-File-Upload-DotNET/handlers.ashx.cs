using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jQueryFileUploadDotNET
{
    /// <summary>
    /// Summary description for handlers
    /// </summary>
    public class handlers : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}