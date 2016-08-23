using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Events.Models.Validation
{
    public interface ISponsorsEntity
    {
        IList<ISponsor> Sponsors { get; }
    }

    public class SponsorIconsRequired : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var parent = (ISponsorsEntity)validationContext.ObjectInstance;

            var list = (IList)value;
            if (list != null)
                for (int i = 0; i < parent.Sponsors.Count; i++)
                    if (parent.Sponsors[i].Id == 0 && list[i] == null)
                        return new ValidationResult("A szponzoroknak kötelező képet megadni.");

            return ValidationResult.Success;
        }
    }
}