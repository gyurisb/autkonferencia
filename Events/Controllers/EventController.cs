using Events.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Events.Helpers;
using System.Threading;


namespace Events.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        [HttpGet]
        public ActionResult Create()
        {
            populateDropdownLists();
            return View(new CreateEventModel(true));
        }

        [HttpPost]
        public ActionResult Create(CreateEventModel eventModel)
        {
            if (ModelState.IsValid)
            {
                using (var ctx = new EventsEntities())
                {
                    var evt = eventModel.CreateEntity();
                    var lecturers = eventModel.CreateLecturers(evt).ToList();

                    ctx.Events.Add(evt);
                    ctx.Lecturers.AddRange(lecturers);
                    ctx.Sponsors.AddRange(eventModel.Sponsors);

                    ctx.SaveChanges();

                    setSponsorFile(eventModel);
                    setLecturerFiles(lecturers, eventModel);
                    ctx.SaveChanges();

                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                foreach (ModelState state in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in state.Errors)
                    {
                        Console.WriteLine(error);
                    }
                }
                populateDropdownLists();
                return View(eventModel);
            }
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            using (var ctx = new EventsEntities())
            {
                populateDropdownLists();
                var evt = ctx.Events.Find(id);
                return View(new CreateEventModel(evt));
            }
        }
        [HttpPost]
        public ActionResult Edit(int id, CreateEventModel eventModel)
        {
            if (ModelState.IsValid)
            {
                using (var ctx = new EventsEntities())
                {
                    var evt = ctx.Events.Find(id);
                    if (evt == null)
                        return HttpNotFound();
                    //régi szponzorok/előadók ID-jeinek gyűjtése
                    var oldSponsorIds = ctx.Sponsors.AsNoTracking().Where(s => s.EventId == id).Select(s => s.Id).ToList();
                    var oldLecturerIds = ctx.Lecturers.AsNoTracking().Where(l => l.EventId == id).Select(l => l.Id).ToList();

                    //beállítom az új model tulajdonságaira az eseményt
                    eventModel.SetEntity(evt);

                    //hozzáadom a módosított szponzorokat
                    ctx.InsertRange(eventModel.Sponsors, s => s.Id ?? 0);
                    //törlöm a kikerülő szponzorokat
                    var emptySponsors = ctx.Sponsors.FindAll(oldSponsorIds.Except(eventModel.Sponsors.Select(s => s.Id))).ToList();
                    ctx.Sponsors.RemoveRange(emptySponsors);
                    deleteSponsorFiles(emptySponsors);

                    //felülírom az új előadókkal
                    var lecturers = eventModel.CreateLecturers(evt, ctx).ToList();
                    ctx.InsertRange(lecturers, l => l.Id);
                    //törlöm a kikerülő előadókat
                    var emptyLecturers = ctx.Lecturers.FindAll(oldLecturerIds.Except(lecturers.Select(s => s.Id))).ToList();
                    ctx.Lecturers.RemoveRange(emptyLecturers);
                    deleteLecturerFiles(emptyLecturers);

                    ctx.SaveChanges();

                    //beállítom a feltöltött fájlokra az URL attribútumokat
                    setSponsorFile(eventModel);
                    setLecturerFiles(lecturers, eventModel);

                    ctx.SaveChanges();

                    return RedirectToAction("Show", "Event", new { Id = id });
                }
            }
            populateDropdownLists();
            return View(eventModel);
        }

        private void populateDropdownLists()
        {
            using (var ctx = new EventsEntities())
            {
                var seqList = ctx.EventSequences.Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() }).ToList();
                seqList.Insert(0, new SelectListItem { Text = "", Value = null });
                ViewBag.EventSequenceIds = seqList;
            }
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            using (var ctx = new EventsEntities())
            {
                var evt = ctx.Events.Find(id);
                if (evt == null)
                    return HttpNotFound();

                ViewBag.EventName = evt.Name;
                ViewBag.DocumentCount = evt.DocumentsCount();
                ViewBag.AttendeeCount = evt.Attendees.Count;

                return View();
            }
        }

        [HttpPost]
        public ActionResult Delete(int id, object model)
        {
            using (var ctx = new EventsEntities())
            {
                var evt = ctx.Events.Find(id);
                if (evt == null)
                    return HttpNotFound();

                //törlöm a kikerülő előadókat
                var emptyLecturers = evt.Lecturers.ToList();
                ctx.Lecturers.RemoveRange(emptyLecturers);
                deleteLecturerFiles(emptyLecturers);
                //törlöm a szponzorokat
                var emptySponsors = evt.Sponsors.ToList();
                ctx.Sponsors.RemoveRange(emptySponsors);
                deleteSponsorFiles(emptySponsors);
                //törlöm a résztvevőket
                ctx.Attendees.RemoveRange(evt.Attendees);
                //törlöm a feltöltött fájlokat + jelentkezők feltöltött fájljait
                TryDeleteDirectory(getPublicEventFolder(id));
                TryDeleteDirectory(getPrivateEventFolder(id));
                //törlöm magát az eseményt
                ctx.Events.Remove(evt);

                ctx.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Show(int id, string source, string attended)
        {
            using (var ctx = new EventsEntities())
            {
                var evnt = ctx.Events.Find(id);
                if (evnt == null) return HttpNotFound();
                ViewBag.Source = source;
                ViewBag.Attended = attended;
                return View(new EventShowModel(evnt));
            }
        }

        [HttpGet]
        public ActionResult Attends(int id)
        {
            using (var ctx = new EventsEntities())
            {
                var evnt = ctx.Events.Single(e => e.Id == id);
                var attends = ctx.Attendees.Where(a => a.EventId == id).ToList();

                ViewBag.EventId = evnt.Id;
                ViewBag.EventTitle = evnt.Name;
                ViewBag.EventLimit = evnt.AttendeeLimit != null ? evnt.AttendeeLimit.ToString() : "korlátlan";
                ViewBag.Columns = evnt.VisibleFields;
                ViewBag.FieldIndexes = evnt.VisibleFieldIndexes;
                return View(attends);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Attend(int id)
        {
            using (var ctx = new EventsEntities())
            {
                var evt = ctx.Events.Find(id);
                if (evt == null || !evt.CanAttend) return HttpNotFound();
                return View(new AttendEventModel(evt));
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Attend(int id, AttendEventModel model)
        {
            using (var ctx = new EventsEntities())
            {
                var evt = ctx.Events.Find(id);
                if (evt == null) return HttpNotFound();
                model.Event = evt;

                if (!evt.CanAttend)
                {
                    ModelState.AddModelError("", "Az eseményre sajnos már nincs szabad hely, nem lehet jelentkezni.");
                }
                else if (!model.IsValid(evt) || !ModelState.IsValid)
                {
                    foreach (var error in model.Errors)
                        ModelState.AddModelError(error.Key, error.Value);
                }
                else
                {
                    var attendee = model.CreateAttendee();
                    attendee.Event = evt;
                    ctx.Attendees.Add(attendee);
                    ctx.SaveChanges();

                    #region move_uploaded_big_file
                    if (model.AttachmentHolder != null)
                    {
                        var tempPath = getBigFilePath(id, model.AttachmentHolder);
                        if (System.IO.File.Exists(tempPath))
                        {
                            string newFile = getAttendeeFilePath(attendee.Id, id, model.AttachmentHolder);
                            Directory.CreateDirectory(Path.GetDirectoryName(newFile));
                            System.IO.File.Move(tempPath, newFile);
                            attendee[Attendee.AttachmentField] = Path.GetFileName(newFile);
                            ctx.SaveChanges();
                        }
                        else
                        {
                            ctx.Attendees.Remove(attendee);
                            ctx.SaveChanges();
                            return new HttpStatusCodeResult(500, "Hiba, a feltöltött fájl nem található.");
                        }
                    }
                    #endregion
                    #region store_uploaded_file
                    if (model.Attachment != null)
                    {
                        string fileName = getAttendeeFilePath(attendee.Id, id, model.Attachment.FileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                        using (var file = new FileStream(fileName, FileMode.CreateNew))
                        {
                            model.Attachment.InputStream.CopyTo(file);
                        }
                        attendee[Attendee.AttachmentField] = Path.GetFileName(fileName);
                        ctx.SaveChanges();
                    }
                    #endregion
                    return RedirectToAction("Show", "Event", new { Id = id, attended = attendee.Name });
                }
            }
            //return Json(ModelStateErrors());
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AttendBigFile(int id)
        {
            var file = RequestFile;
            using (var ctx = new EventsEntities())
            {
                var evt = ctx.Events.Find(id);
                if (evt == null) return HttpNotFound();

                string fileName = generateBigFilePath(id, file.FileName);
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                using (FileStream newFile = new FileStream(fileName, FileMode.CreateNew))
                {
                    file.InputStream.CopyTo(newFile);
                }

                return Json(Path.GetFileName(fileName));
            }
        }

        [HttpGet]
        public ActionResult Upload(int id)
        {
            using (var ctx = new EventsEntities())
            {
                var evnt = ctx.Events.SingleOrDefault(e => e.Id == id);
                if (evnt == null) return HttpNotFound();
                return View(new FileTypesModel(evnt));
            }
        }

        [HttpPost]
        public ActionResult UploadFile(int id)
        {
            using (var ctx = new EventsEntities())
            {
                var evnt = ctx.Events.SingleOrDefault(e => e.Id == id);
                if (evnt == null) return HttpNotFound();

                List<string> names = new List<string>();
                foreach (var file in RequestFiles)
                {
                    names.Add(file.FileName);
                    string filePath = getUploadFilePath(id, file.FileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    using (var newfile = new FileStream(filePath, FileMode.Create))
                        file.InputStream.CopyTo(newfile);
                }

                evnt.AddDocumentEntries(names);
                ctx.SaveChanges();
                return PartialView(names);
            }
        }

        [HttpPost]
        public ActionResult EmbedVideo(int id)
        {
            using (var ctx = new EventsEntities())
            {
                var evt = ctx.Events.Find(id);
                if (evt == null) return HttpNotFound();

                string data = new StreamReader(Request.InputStream).ReadToEnd();
                evt.RemoveDocumentEntry("embedVideo");
                evt.RemoveDocumentEntry("embedHtml");
                if (data != null && data != "")
                {
                    evt.AddDocumentEntry("embedVideo=true");
                    evt.AddDocumentEntry("embedHtml=" + data);
                }

                ctx.SaveChanges();
                return PartialView(data);
            }
        }

        [HttpPost]
        public ActionResult DeleteFile(int id, string name)
        {
            using (var ctx = new EventsEntities())
            {
                var evnt = ctx.Events.SingleOrDefault(e => e.Id == id);
                if (evnt == null) return HttpNotFound();

                evnt.RemoveDocument(name);
                ctx.SaveChanges();

                string file = getUploadFilePath(id, name);
                if (System.IO.File.Exists(file))
                    System.IO.File.Delete(file);
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpGet]
        public ActionResult ToggleLock(int id)
        {
            using (var ctx = new EventsEntities())
            {
                var evt = ctx.Events.Find(id);
                if (evt == null)
                    return HttpNotFound();
                evt.IsLocked = !evt.IsLocked;
                ctx.SaveChanges();
            }
            return Redirect(Request.UrlReferrer.LocalPath);
        }

        [HttpGet]
        public ActionResult Export(int id)
        {
            // Create a spreadsheet document by supplying the filepath.
            // By default, AutoSave = true, Editable = true, and Type = xlsx.
            using (var document = XlsxHelper.CreateDocument())
            using (var ctx = new EventsEntities())
            {
                var evt = ctx.Events.Find(id);
                if (evt == null) return HttpNotFound();
                string serverName = Request.Url.GetLeftPart(UriPartial.Authority);

                int headCol = 0;
                foreach (var columnHeader in evt.VisibleFields)
                {
                    XlsxHelper.SetCellText(document, XlsxHelper.ToCol(headCol), 1, columnHeader, true);
                    headCol++;
                }
                uint row = 2;
                var fieldIndexes = evt.VisibleFieldIndexes;
                foreach (var attendee in evt.Attendees)
                {
                    int col = 0;
                    foreach (int index in fieldIndexes)
                    {
                        string value = attendee[index];
                        if (index == Attendee.AttachmentField && attendee.AttachmentUrl != null)
                            value = serverName + attendee.AttachmentUrl;
                        XlsxHelper.SetCellText(document, XlsxHelper.ToCol(col), row, value);
                        col++;
                    }
                    row++;
                }

                return File(document.ToByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
        }

        #region helpers
        
        private IEnumerable<HttpPostedFileBase> RequestFiles
        {
            get
            {
                foreach (string fileId in Request.Files)
                    yield return Request.Files[fileId];
            }
        }
        private HttpPostedFileBase RequestFile { get { return RequestFiles.Single(); } }

        private void setLecturerFiles(IEnumerable<Lecturer> lecturers, CreateEventModel eventModel)
        {
            int n = 0;
            foreach (var lecturer in lecturers)
            {
                if (eventModel.LecturerAvatars[n] != null)
                {
                    if (lecturer.Avatar != null)
                    {
                        System.IO.File.Delete(getLecturerAvatarFilePath(lecturer.Id, lecturer.Avatar));
                    }
                    var newFile = getLecturerAvatarFilePath(lecturer.Id, eventModel.LecturerAvatars[n].FileName);
                    using (var fs = new FileStream(newFile, FileMode.Create))
                    {
                        eventModel.LecturerAvatars[n].InputStream.CopyTo(fs);
                    }
                    lecturer.Avatar = Path.GetFileName(newFile);
                }
                n++;
            }
        }
        private void deleteLecturerFiles(IEnumerable<Lecturer> lecturers)
        {
            foreach (var lecturer in lecturers)
                if (lecturer.Avatar != null)
                    System.IO.File.Delete(getLecturerAvatarFilePath(lecturer.Id, lecturer.Avatar));
        }
        private void setSponsorFile(CreateEventModel eventModel)
        {
            int n = 0;
            foreach (var sponsor in eventModel.Sponsors ?? new Sponsor[0])
            {
                if (eventModel.SponsorIcons[n] != null)
                {
                    if (sponsor.Icon != null)
                    {
                        System.IO.File.Delete(getSponsorIconPath(sponsor.Id.Value, sponsor.Icon));
                    }
                    var newFile = getSponsorIconPath(sponsor.Id.Value, eventModel.SponsorIcons[n].FileName);
                    using (var fs = new FileStream(newFile, FileMode.Create))
                    {
                        eventModel.SponsorIcons[n].InputStream.CopyTo(fs);
                    }
                    sponsor.Icon = Path.GetFileName(newFile);
                }
                n++;
            }
        }
        private void deleteSponsorFiles(IEnumerable<Sponsor> sponsors)
        {
            foreach (var sponsor in sponsors)
                if (sponsor.Icon != null)
                    System.IO.File.Delete(getSponsorIconPath(sponsor.Id.Value, sponsor.Icon));
        }
        private string getSponsorIconPath(int sponsorId, string name)
        {
            return Server.MapPath("~/Uploads/Sponsors") + "\\" + sponsorId + Path.GetExtension(name);
        }
        private string getLecturerAvatarFilePath(int lecturerId, string name)
        {
            return Server.MapPath("~/Uploads/Lecturers") + "\\" + lecturerId + Path.GetExtension(name);
        }
        private string getPublicEventFolder(int eventId)
        {
            return Server.MapPath("~/Uploads/Events") + "\\" + eventId + "\\";
        }
        private string getPrivateEventFolder(int eventId)
        {
            return Server.MapPath("~/App_Data/Private") + "\\" + eventId + "\\";
        }
        private string getUploadFilePath(int eventId, string name)
        {
            return getPublicEventFolder(eventId) + name;
        }
        private string getAttendeeFilePath(int attendeeId, int eventId, string fileName)
        {
            return getPrivateEventFolder(eventId) + "Attendees\\" + attendeeId + Path.GetExtension(fileName);
        }
        private string getAttendeeFilePath(Attendee attendee)
        {
            return getAttendeeFilePath(attendee.Id, attendee.Event.Id, attendee[Attendee.AttachmentField]);
        }
        private string getBigFilePath(int eventId, string fileName)
        {
            return getPrivateEventFolder(eventId) + "Temp\\" + fileName;
        }
        private string generateBigFilePath(int eventId, string fileName)
        {
            string name = new Random().Next(Int32.MaxValue).ToString() + DateTime.Now.ToString(".yy.MM.dd.HH.mm.ss");
            return getPrivateEventFolder(eventId) + "Temp\\" + name + Path.GetExtension(fileName);
        }
        private void TryDeleteDirectory(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
        #endregion
    }
}