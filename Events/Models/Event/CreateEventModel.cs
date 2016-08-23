using Events.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Events.Models
{
    public class CreateEventModel : ISponsorsEntity
    {
        public CreateEventModel() { }
        public CreateEventModel(bool initialize) 
        {
            Visibles = new bool[Attendee.Fields.Length];
            Requireds = new bool[Attendee.Fields.Length];
            Visibles[Attendee.NameField] = Visibles[Attendee.EmailField] = true;
            Requireds[Attendee.NameField] = Requireds[Attendee.EmailField] = true;
            IncludeSponsors = true;
        }
        public CreateEventModel(Event evt)
        {
            Name = evt.Name;
            Description = evt.Description;
            Place = evt.Place;
            EventSequenceId = evt.EventSequenceId;
            AttendeeLimit = evt.AttendeeLimit;
            IncludeSponsors = evt.IncludeSponsors;
            AttendMessage = evt.AttendMessage;

            Date = evt.Time.Date;
            HourFrom = evt.Time.Hour;
            MinuteFrom = evt.Time.Minute;
            HourTo = evt.EndTime.Hour;
            MinuteTo = evt.EndTime.Minute;

            var lecturers = evt.Lecturers.ToList();
            LecturerNames = lecturers.Select(l => l.Name).ToArray();
            LecturerJobs = lecturers.Select(l => l.CompanyRank).ToArray();
            LecturerUrls = lecturers.Select(l => l.Url).ToArray();
            LecturerDescriptions = lecturers.Select(l => l.Introduction).ToArray();
            LecturerIds = lecturers.Select(l => (int?)l.Id).ToArray();
            LecturerAvatars = null; //nem tudjuk átadni a böngészőnek
            avatarUrls = evt.Lecturers.Select(l => l.Avatar).ToArray();

            Sponsors = evt.Sponsors.OrderBy(s => s.Id).ToArray();

            var req = evt.AttendRequirements.Split(',');
            Requireds = req.Select(r => r == "2").ToArray();
            Visibles = req.Select(r => r == "2" || r == "1").ToArray();
        }
        
        [Required]
        [Display(Name = "Cím")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Leírás")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Időpont")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime? Date { get; set; }
        [Display(Name = "Kezdés órája")]     public int HourFrom { get; set; }
        [Display(Name = "Kezdés perce")]     public int MinuteFrom { get; set; }
        [Display(Name = "Befejezés órája")]  public int HourTo { get; set; }
        [Display(Name = "Befejezés perce")]  public int MinuteTo { get; set; }

        [Required]
        [Display(Name = "Helyszín")]
        public string Place { get; set; }

        [Display(Name = "Sorozat")]
        public Nullable<int> EventSequenceId { get; set; }

        [Display(Name = "Férőhelyek száma")]
        public Nullable<short> AttendeeLimit { get; set; }

        [Display(Name = "Előadók")]
        [RequiredArray(ErrorMessage = "Hiányzik az egyik előadó neve.")]
        public string[] LecturerNames { get; set; }
        public string[] LecturerJobs { get; set; }
        public string[] LecturerUrls { get; set; }
        public string[] LecturerDescriptions { get; set; }
        public int?[] LecturerIds { get; set; }
        public HttpPostedFileBase[] LecturerAvatars { get; set; }

        [Display(Name = "Előadássorozat szponzorainak megjelenítése")]
        public bool IncludeSponsors { get; set; }

        [Display(Name = "Szponzorok")]
        public Sponsor[] Sponsors { get; set; }
        [SponsorIconsRequired(ErrorMessage = "A szponzoroknak kötelező képet megadni.")]
        public HttpPostedFileBase[] SponsorIcons { get; set; }

        [Display(Name = "Jelentkező oldali üzenet")]
        [DataType(DataType.MultilineText)]
        public string AttendMessage { get; set; }

        public bool[] Requireds { get; set; }
        public bool[] Visibles { get; set; }

        private string CreateRequirements()
        {
            return string.Join(",", Requireds.Zip(Visibles, (r, v) => r ? '2' : (v ? '1' : '0')).ToArray());
        }

        private  string[] avatarUrls;
        public string GetAvatarUrl(int id)
        {
            if (avatarUrls == null) return null;
            return Lecturer.GetAvatarSource() + avatarUrls[id];
        }

        public Event CreateEntity()
        {
            Sponsors = Sponsors ?? new Sponsor[0];
            var evt = new Event
            {
                Name = Name,
                Description = Description,
                Time = Date.Value + new TimeSpan(HourFrom, MinuteFrom, 0),
                TimeLength = (short)(new TimeSpan(HourTo, MinuteTo, 0) - new TimeSpan(HourFrom, MinuteFrom, 0)).TotalMinutes,
                Place = Place,
                EventSequenceId = EventSequenceId,
                AttendeeLimit = AttendeeLimit,
                AttendRequirements = CreateRequirements(),
                IncludeSponsors = IncludeSponsors,
                AttendMessage = AttendMessage
            };
            foreach (var s in Sponsors)
                s.Event = evt;
            return evt;
        }

        public void SetEntity(Event e)
        {
            Sponsors = Sponsors ?? new Sponsor[0];

            e.Name = Name;
            e.Description = Description;
            e.Time = Date.Value + new TimeSpan(HourFrom, MinuteFrom, 0);
            e.TimeLength = (short)(new TimeSpan(HourTo, MinuteTo, 0) - new TimeSpan(HourFrom, MinuteFrom, 0)).TotalMinutes;
            e.Place = Place;
            e.EventSequenceId = EventSequenceId;
            e.AttendeeLimit = AttendeeLimit;
            e.AttendRequirements = CreateRequirements();
            e.IncludeSponsors = IncludeSponsors;
            e.AttendMessage = AttendMessage;

            foreach (var sponsor in Sponsors)
                sponsor.EventId = e.Id;
        }

        public IEnumerable<Lecturer> CreateLecturers(Event evt, EventsEntities ctx = null)
        {

            for (int i = 0; i < LecturerNames.Length; i++)
            {
                var lect = new Lecturer
                {
                    Name = LecturerNames[i],
                    Url = LecturerUrls[i],
                    CompanyRank = LecturerJobs[i],
                    Introduction = LecturerDescriptions[i],
                    EventId = evt.Id
                };
                if (LecturerIds[i] != null)
                {
                    int id = LecturerIds[i].Value;
                    var oldLect = ctx.Lecturers.AsNoTracking().Single(l => l.Id == id);
                    lect.Id = id;
                    lect.Avatar = oldLect.Avatar;
                }
                yield return lect;
            }
        }

        IList<ISponsor> ISponsorsEntity.Sponsors
        {
            get { return this.Sponsors.Cast<ISponsor>().ToList(); }
        }
    }
}