using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Events.Models.Validation
{
    public class RequiredArray : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var list = (IList)value;
            return value == null || list.Cast<object>().All(o => o != null && o.ToString() != "");
        }
    }
}