using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Events;
using Events.Models;
using System.IO;

namespace Events.Controllers
{
    [Authorize]
    public class EventSequencesController : Controller
    {
        private EventsEntities db = new EventsEntities();

        // GET: EventSequences
        public ActionResult Index()
        {
            return View(db.EventSequences.ToList());
        }

        [AllowAnonymous]
        // GET: EventSequences/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            EventSequence eventSequence = db.EventSequences.SingleOrDefault(e => e.Id == id);

            if (eventSequence == null)
                return HttpNotFound();

            ViewBag.UpcomingEvents = db.FindUpcomingEvents(eventSequence).ToList();
            ViewBag.FinishedEvents = db.FindFinishedEvents(eventSequence).ToList();
            return View(eventSequence);
        }

        // GET: EventSequences/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EventSequences/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EventSequenceModel model)
        {
            if (ModelState.IsValid)
            {
                EventSequence eventSequence = new EventSequence();
                model.SetEntity(eventSequence);
                db.EventSequences.Add(eventSequence);
                foreach (var sponsor in model.Sponsors)
                {
                    sponsor.EventSequence = eventSequence;
                    db.SeqSponsors.Add(sponsor);
                }
                db.SaveChanges();
                setSponsorFiles(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: EventSequences/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EventSequence eventSequence = db.EventSequences.Find(id);
            if (eventSequence == null)
            {
                return HttpNotFound();
            }
            return View(new EventSequenceModel(eventSequence));
        }

        // POST: EventSequences/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EventSequenceModel model)
        {
            if (ModelState.IsValid)
            {
                EventSequence eventSequence = db.EventSequences.Find(model.Id);
                if (eventSequence == null)
                    return HttpNotFound();
                model.SetEntity(eventSequence);
                db.Entry(eventSequence).State = System.Data.Entity.EntityState.Modified;
                foreach (var sponsor in model.Sponsors)
                {
                    sponsor.EventSequenceId = eventSequence.Id;
                    db.Insert(sponsor, s => s.Id ?? 0);
                }
                removeSponsors(eventSequence.Sponsors.Except(model.Sponsors, new IDComparer()));
                db.SaveChanges();
                setSponsorFiles(model);
                db.SaveChanges();
                return RedirectToAction("Details", new { Id = eventSequence.Id });
            }
            return View(model);
        }

        // GET: EventSequences/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EventSequence eventSequence = db.EventSequences.Find(id);
            if (eventSequence == null)
            {
                return HttpNotFound();
            }
            return View(eventSequence);
        }

        // POST: EventSequences/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EventSequence eventSequence = db.EventSequences.Find(id);
            removeSponsors(eventSequence.Sponsors);
            db.EventSequences.Remove(eventSequence);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #region helpers
        private void removeSponsors(IEnumerable<SeqSponsor> sponsors_)
        {
            var sponsors = sponsors_.ToList();
            db.SeqSponsors.RemoveRange(sponsors);
            deleteSponsorFiles(sponsors);
        }
        private void setSponsorFiles(EventSequenceModel model)
        {
            int n = 0;
            foreach (var sponsor in model.Sponsors ?? new SeqSponsor[0])
            {
                if (model.SponsorIcons[n] != null)
                {
                    if (sponsor.Icon != null)
                    {
                        //var fileName = sponsor.Icon.Substring(sponsor.Icon.LastIndexOf('/'));
                        System.IO.File.Delete(getSponsorIconPath(sponsor.Id.Value, sponsor.Icon));
                    }
                    var newFile = getSponsorIconPath(sponsor.Id.Value, model.SponsorIcons[n].FileName);
                    using (var fs = new FileStream(newFile, FileMode.Create))
                    {
                        model.SponsorIcons[n].InputStream.CopyTo(fs);
                    }
                    sponsor.Icon = Path.GetFileName(newFile);
                }
                n++;
            }
        }
        private void deleteSponsorFiles(IEnumerable<SeqSponsor> sponsors)
        {
            foreach (var sponsor in sponsors)
                if (sponsor.Icon != null)
                    System.IO.File.Delete(getSponsorIconPath(sponsor.Id.Value, sponsor.Icon));
        }
        private string getSponsorIconPath(int sponsorId, string name)
        {
            return Server.MapPath("~/Uploads/SeqSponsors") + "\\" + sponsorId + Path.GetExtension(name);
        }
        private class IDComparer : IEqualityComparer<SeqSponsor>
        {
            public bool Equals(SeqSponsor x, SeqSponsor y)
            {
                return x.Id == y.Id;
            }
            public int GetHashCode(SeqSponsor obj)
            {
                return obj.Id ?? 0;
            }
        }
        #endregion
    }
}
