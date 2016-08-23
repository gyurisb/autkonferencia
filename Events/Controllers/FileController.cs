using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Events.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        [HttpGet]
        public ActionResult EventAttachment(int eventId, string fileName)
        {
            string path = Server.MapPath("~/App_Data/Private") + "\\" + eventId + "\\Attendees\\" + fileName;
            if (!System.IO.File.Exists(path))
                return HttpNotFound();

            return File(path, MimeMapping.GetMimeMapping(path));
        }
    }
}