using Events.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Events.Models
{
    public class EventSequenceModel : ISponsorsEntity
    {
        public int Id { get; set; }

        [Display(Name = "Cím")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Leírás")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        [Required]
        public string Description { get; set; }

        [Display(Name = "Eleje dátum")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Required]
        public Nullable<System.DateTime> StartDate { get; set; }

        [Display(Name = "Vége dátum")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Required]
        public Nullable<System.DateTime> EndDate { get; set; }

        [Display(Name = "Szponzorok")]
        public SeqSponsor[] Sponsors { get; set; }

        [SponsorIconsRequired(ErrorMessage = "A szponzoroknak kötelező képet megadni.")]
        public HttpPostedFileBase[] SponsorIcons { get; set; }

        public EventSequenceModel() { }
        public EventSequenceModel(EventSequence entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            Description = entity.Description;
            StartDate = entity.StartDate;
            EndDate = entity.EndDate;
            Sponsors = entity.Sponsors.ToArray();
        }

        public void SetEntity(EventSequence entity)
        {
            entity.Id = Id;
            entity.Name = Name;
            entity.Description = Description;
            entity.StartDate = StartDate;
            entity.EndDate = EndDate;
        }

        IList<ISponsor> ISponsorsEntity.Sponsors
        {
            get { return this.Sponsors.Cast<ISponsor>().ToList(); }
        }
    }
}