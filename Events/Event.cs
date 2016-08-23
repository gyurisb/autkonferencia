//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Events
{
    using System;
    using System.Collections.Generic;
    
    public partial class Event
    {
        public Event()
        {
            this.Attendees = new HashSet<Attendee>();
            this.Sponsors = new HashSet<Sponsor>();
            this.Lecturers = new HashSet<Lecturer>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public System.DateTime Time { get; set; }
        public string Place { get; set; }
        public Nullable<int> EventSequenceId { get; set; }
        public Nullable<short> AttendeeLimit { get; set; }
        public short TimeLength { get; set; }
        public string Documents { get; set; }
        public string AttendRequirements { get; set; }
        public bool IsLocked { get; set; }
        public bool IncludeSponsors { get; set; }
        public string AttendMessage { get; set; }
    
        public virtual EventSequence EventSequence { get; set; }
        public virtual ICollection<Attendee> Attendees { get; set; }
        public virtual ICollection<Sponsor> Sponsors { get; set; }
        public virtual ICollection<Lecturer> Lecturers { get; set; }
    }
}