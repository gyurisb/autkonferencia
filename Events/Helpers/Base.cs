using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Events.Helpers
{
    public class Base
    {
        public static string Url
        {
            get
            {
                return HttpRuntime.AppDomainAppVirtualPath;
            }
        }
        public static string UploadsUrl
        {
            get
            {
                return Url + "Uploads/";
            }
        }
    }
}