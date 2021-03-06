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
    
    public partial class EventSequence
    {
        public EventSequence()
        {
            this.Events = new HashSet<Event>();
            this.Sponsors = new HashSet<SeqSponsor>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string WeekTimes { get; set; }
    
        public virtual ICollection<Event> Events { get; set; }
        public virtual ICollection<SeqSponsor> Sponsors { get; set; }
    }
}
