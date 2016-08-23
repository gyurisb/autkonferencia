using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Events.Models.Validation
{
    public class PhoneNumber : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            string tel = (string)value;
            if (tel == null) return true;

            return IsBetween(tel.Count(ch => Char.IsDigit(ch)), 8, 15) && Regex.IsMatch(tel, @"^[+0][0-9\s-/]*$");
        }

        private bool IsBetween(int val, int start, int end)
        {
            return start <= val && val <= end;
        }
    }
}