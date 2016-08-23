using Events.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Events.Models
{
    public class AttendEventModel
    {
        [Display(Name = "Név")]
        public string Name { get; set; }

        [Display(Name = "E-mail")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Cég")]
        public string Company { get; set; }

        [Display(Name = "Beosztás")]
        public string Post { get; set; }

        [Display(Name = "Tel.")]
        [PhoneNumber(ErrorMessage = "A telefonszám nem érvényes.")]
        public string Phone { get; set; }

        [Display(Name = "Csatolmány")]
        [DataType(DataType.Upload)]
        public HttpPostedFileBase Attachment { get; set; }
        public string AttachmentHolder { get; set; }

        [Display(Name = "Megjegyzés")]
        [DataType(DataType.MultilineText)]
        public string Comment { get; set; }


        public bool[] Required { get; set; }
        public bool[] Visible { get; set; }
        public Event Event { get; set; }

        public AttendEventModel() 
        {
            Required = new bool[7];
            Visible = new bool[7];
        }
        public AttendEventModel(Event evt)
        {
            setRequirements(evt);
            Event = evt;
        }
        public Attendee CreateAttendee()
        {
            return new Attendee
            {
                Name = Name,
                Company = Company,
                Email = Email,
                OtherFields = new string[] { Post, Phone, AttachmentHolder, Comment }
            };
        }

        public bool IsValid(Event evt)
        {
            setRequirements(evt);
            string[] fieldNames = new string[] { "Name", "Email", "Company", "Post", "Phone", "Attachment", "Comment" };
            object[] fields = new object[] { Name, Email, Company, Post, Phone, Or(Attachment, AttachmentHolder), Comment };

            Errors = new Dictionary<string, string>();
            for (int i = 0; i < fields.Length; i++)
                if (Required[i] && fields[i] == null)
                    Errors[fieldNames[i]] = "A mező megadása kötelező!";

            return Errors.Count == 0;
        }

        public Dictionary<string, string> Errors { get; private set; }
         
        private void setRequirements(Event evt)
        {
            var req = evt.AttendRequirements.Split(',');
            Required = req.Select(r => r == "2").ToArray();
            Visible = req.Select(r => r == "2" || r == "1").ToArray();
        }

        private object Or(params object[] objects)
        {
            return objects.FirstOrDefault(o => o != null);
        }
    }
}