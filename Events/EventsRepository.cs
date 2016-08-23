using Events.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Events
{
    partial class Event
    {
        public string IconSrc { get { return this.Lecturers.First().AvatarSrc; } }
        public DateTime EndTime { get { return Time + TimeSpan.FromMinutes(TimeLength); } }
        public IEnumerable<ISponsor> AllSponsors { get { return this.Sponsors.Cast<ISponsor>().Concat(eventSequenceSponsors()); } }
        private IEnumerable<ISponsor> eventSequenceSponsors()
        {
            if (this.IncludeSponsors && this.EventSequence != null)
                return this.EventSequence.Sponsors.Cast<ISponsor>();
            return new ISponsor[0];
        }

        public bool CanAttend { get { return  !IsLocked && AttendEnabled && Time > DateTime.Now && !IsFull; } }
        public bool AttendEnabled { get { return !AttendRequirements.All(ch => ch == '0' || ch == ','); } }
        public bool IsFull { get { return this.IsLocked || this.Attendees.Count == this.AttendeeLimit; } }

        public string[] VisibleFields
        {
            get
            {
                return Attendee.Fields.Zip(this.AttendRequirements.Split(',').Select(ch => ch != "0"), (x, y) => new { Visible = y, Field = x })
                    .Where(x => x.Visible)
                    .Select(x => x.Field)
                    .ToArray();
            }
        }
        public int[] VisibleFieldIndexes
        {
            get
            {
                return this.AttendRequirements.Split(',')
                    .Select((str, i) => new { Visible = (str != "0"), Index = i })
                    .Where(x => x.Visible)
                    .Select(x => x.Index)
                    .ToArray();
            }
        }

        #region uploaded document handler functions
        public static readonly char Delimiter = (char)0x01; //control character for delimiter

        public IEnumerable<string> GetDocumentEntries() { return (this.Documents ?? "").Split(Delimiter).Except(new string[] { "" }); }
        public void AddDocumentEntry(string entry)
        {
            AddDocumentEntries(new string[] { entry });
        }
        public void AddDocumentEntries(IEnumerable<string> entries)
        {
            var oldEntries = this.GetDocumentEntries().ToArray();
            if (oldEntries.Intersect(entries).Count() != 0)
                throw new InvalidOperationException("A file with the same name is already uploaded.");
            Documents = String.Join("" + Delimiter, oldEntries.Concat(entries));
        }
        public void RemoveDocumentEntry(string entryKey)
        {
            var entries = this.GetDocumentEntries().ToList();
            var document = entries.SingleOrDefault(e => e.StartsWith(entryKey + "="));
            entries.Remove(document);
            Documents = String.Join("" + Delimiter, entries);
        }
        public void RemoveDocument(string document)
        {
            var entries = this.GetDocumentEntries().ToList();
            entries.Remove(document);
            Documents = String.Join("" + Delimiter, entries);
        }
        public int DocumentsCount()
        {
            var docs = GetDocumentEntries().ToList();
            int sum = docs.Count();
            if (docs.Contains("embedVideo=true"))
                sum -= 2;
            return sum;
        }
        #endregion
    }

    partial class EventsEntities
    {
        public void Insert<T>(T e, Func<T, int> primaryKeySelector) where T : class
        {
            this.Entry(e).State = primaryKeySelector(e) == 0 ? EntityState.Added : EntityState.Modified;
        }

        public void InsertRange<T>(IEnumerable<T> elements, Func<T, int> primaryKeySelector) where T : class
        {
            foreach (var e in elements)
            {
                this.Entry(e).State = primaryKeySelector(e) == 0 ? EntityState.Added : EntityState.Modified;
            }
        }
    }

    partial class EventSequence
    {
        public class WeekEntry
        {
            public int Day;
            public int StartHour, StartMinute;
            public int EndHour, EndMinute;
            public override string ToString()
            {
                int currentDay = (int)DateTime.Today.DayOfWeek;
                var date = DateTime.Today + TimeSpan.FromDays(Day - currentDay);
                var startTime = date + TimeSpan.FromHours(StartHour) + TimeSpan.FromMinutes(StartMinute);
                var endTime = date + TimeSpan.FromHours(EndHour) + TimeSpan.FromMinutes(EndMinute);
                return startTime.ToString("ddd HH:mm") + " - " + endTime.ToString("HH:mm");
            }
        }

        public IEnumerable<WeekEntry> WeekTimeEntries
        {
            get
            {
                return WeekTimes.Split(';').Select(e =>
                {
                    var parts = e.Split('-');
                    return new WeekEntry
                    {
                        Day = int.Parse(parts[0]),
                        StartHour = int.Parse(parts[1]),
                        StartMinute = int.Parse(parts[2]),
                        EndHour = int.Parse(parts[3]),
                        EndMinute = int.Parse(parts[4]),
                    };
                });
            }
        }
    }

    partial class Attendee
    {
        public static string[] Fields = new string[] { "Név", "E-mail", "Cég", "Beosztás", "Tel", "Csatolmány", "Megjegyzés" };
        public static readonly int NameField = 0,
                                EmailField = 1,
                                CompanyField = 2,
                                PostField = 3,
                                PhoneField = 4,
                                AttachmentField = 5,
                                CommentField = 6;

        public string[] OtherFields
        {
            get { return Other != null ? Other.Split(',') : Enumerable.Repeat((string)null, Fields.Length - 3).ToArray(); }
            set
            {
                if (value.Any(str => str != null))
                    Other = String.Join(",", value.Select(x => x ?? ""));
            }
        }

        public string this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return this.Name;
                    case 1: return this.Email;
                    case 2: return this.Company;
                    default: return OtherFields[i - 3];
                }
            }
            set
            {
                switch (i)
                {
                    case 0: this.Name = value; break;
                    case 1: this.Email = value; break;
                    case 2: this.Company = value; break;
                    default: setField(i - 3, value); break;
                }
            }
        }
        private void setField(int i, string value)
        {
            var other = OtherFields;
            other[i] = value;
            OtherFields = other;
        }

        public string AttachmentUrl { get { return getAttachmentUrl(); } }
        private string getAttachmentUrl()
        {
            if (this[AttachmentField] != null && this[AttachmentField] != "")
                return Base.Url + "Files/" + Event.Id + "/" + this[AttachmentField];
            return null;
        }
    }

    partial class EventsEntities
    {
        public IQueryable<Event> FindUpcomingEvents()
        {
            return (from e in this.Events
                   //where DateTime.Now <= e.Time && e.Time < DateTime.Now + TimeSpan.FromDays(7)
                   where DateTime.Now <= e.Time
                   orderby e.Time
                   select e).Take(10);
        }
        public IQueryable<Event> FindFinishedEvents()
        {
            return (from e in this.Events
                   //where DateTime.Now - TimeSpan.FromDays(7) <= e.Time && e.Time < DateTime.Now
                   where e.Time < DateTime.Now
                   orderby e.Time descending
                   select e).Take(10);
        }
        public IQueryable<Event> FindUpcomingEvents(EventSequence eventSequence)
        {
            return FindUpcomingEvents().Where(e => e.EventSequenceId == eventSequence.Id);
        }
        public IQueryable<Event> FindFinishedEvents(EventSequence eventSequence)
        {
            return FindFinishedEvents().Where(e => e.EventSequenceId == eventSequence.Id);
        }
    }

    partial class Lecturer
    {
        public string AvatarSrc { get { return GetAvatarSource() + Avatar; } }
        public static string GetAvatarSource()
        {
            return Base.UploadsUrl + "Lecturers/";
        }
        public string UrlNull { get { return Url == "" ? null : Url; } }
    }

    public interface ISponsor
    {
        int? Id { get; }
        string Icon { get; }
        string Name { get; }
        string Url { get; }
        string IconSrc { get; }
    }
    partial class Sponsor : ISponsor
    {
        public string IconSrc { get { return Icon == "" || Icon == null ? null : Base.UploadsUrl + "Sponsors/" + Icon; } }
    }
    partial class SeqSponsor : ISponsor
    {
        public string IconSrc { get { return Icon == "" || Icon == null ? null : Base.UploadsUrl + "SeqSponsors/" + Icon; } }
    }

    public class EventsRepository
    {

    }
}