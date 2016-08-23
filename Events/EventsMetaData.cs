

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Events
{
    [MetadataType(typeof(EventMeta))]
    public partial class Event { }
    [MetadataType(typeof(EventSequenceMeta))]
    public partial class EventSequence { }
    [MetadataType(typeof(SponsorMeta))]
    public partial class Sponsor { }
    [MetadataType(typeof(SponsorMeta))]
    public partial class SeqSponsor { }

    public partial class EventMeta
    {
        [Display(Name = "Cím")]
        public string Name { get; set; }
        [Display(Name = "Leírás")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [Display(Name = "Időpont")]
        public System.DateTime Time { get; set; }
        [Display(Name = "Helyszín")]
        public string Place { get; set; }
        [Display(Name = "Eseménysorozat")]
        public Nullable<int> EventSequenceId { get; set; }
        [Display(Name = "Férőhelyek száma")]
        public Nullable<short> AttendeeLimit { get; set; }
        [Display(Name = "Időtartam")]
        public short TimeLength { get; set; }
        [Display(Name = "Dokumentumok")]
        public string Documents { get; set; }

        [Display(Name = "Előadók")]
        public virtual ICollection<Lecturer> Lecturers { get; set; }
        [Display(Name = "Eseménysorozat")]
        public virtual EventSequence EventSequence { get; set; }
        [Display(Name = "Résztvevők")]
        public virtual ICollection<Attendee> Attendees { get; set; }
    }

    public class EventSequenceMeta
    {
        [Display(Name = "Cím")]
        public string Name { get; set; }
        [Display(Name = "Leírás")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [Display(Name = "Eleje dátum")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }
        [Display(Name = "Vége dátum")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EndDate { get; set; }
        [Display(Name = "Időpontok")]
        public string WeekTimes { get; set; }
        [Display(Name = "Események")]
        public virtual ICollection<Event> Events { get; set; }
    }

    public class SponsorMeta
    {
        [Required(ErrorMessage = "A szponzor nevének megadása kötelező.")]
        [Display(Name = "Név")]
        public string Name { get; set; }
        [Required(ErrorMessage = "A szponzor url címének megadása kötelező.")]
        [Display(Name = "Url")]
        public string Url { get; set; }
        public string Icon { get; set; }
    }
}