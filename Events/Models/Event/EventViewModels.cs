using Events.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Events.Models
{
    public class EventViewModel
    {
        public Event Event;
        public IEnumerable<Lecturer> Lecturers;

        public EventViewModel(Event e)
        {
            Event = e;
            Event.EventSequence = e.EventSequence;
            Lecturers = e.Lecturers.ToList();
        }

        public static IEnumerable<EventViewModel> ExecQuery(IQueryable<Event> events)
        {
            var ret = events.ToList().Select(e => new EventViewModel(e)).ToList();
            return ret;
        }
    }

    public class EventShowModel : EventViewModel
    {
        public bool EventIsFull;
        public bool Started;
        public FileTypesModel Files;
        public ISponsor[] Sponsors;
        public bool AttendEnabled;
        public EventShowModel(Event e)
            : base(e)
        {
            EventIsFull = e.IsFull;
            Started = e.Time < DateTime.Now;
            Files = new FileTypesModel(e);
            Sponsors = e.AllSponsors.ToArray();
            AttendEnabled = e.AttendEnabled;
        }
    }
    public class FileTypesModel
    {
        public string[] Images = new string[0];
        public string[] Documents = new string[0];

        public string EmbedHtml;
        public bool EmbedVideo;

        public FileTypesModel() { }
        public FileTypesModel(Event e)
        {
            if (e.Documents != null && e.Documents != "")
            {
                string[] entries = e.GetDocumentEntries().ToArray();
                string[] documents = entries
                    .Where(d => !d.Contains('='))
                    .Select(l => createLink(l, e.Id))
                    .ToArray();

                Images = documents.Where(x => x.EndsWith(".jpg") || x.EndsWith(".png") || x.EndsWith(".gif")).ToArray();
                Documents = documents.Except(Images).ToArray();

                if (EmbedVideo = entries.Contains("embedVideo=true"))
                {
                    EmbedHtml = entries.Single(entry => entry.StartsWith("embedHtml=")).Substring("embedHtml=".Length);
                }
            }
        }
        private static string createLink(string link, int id)
        {
            if (link.StartsWith("http://") || link.StartsWith("https://")) return link;
            return Base.Url + "Uploads/Events/" + id + "/" + link;
        }
    }
}