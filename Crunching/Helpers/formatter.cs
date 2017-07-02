using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace GTPTracker.Helpers
{
    public static class formatter
    {
        public static string getOuter(string outer1, string outer2)
        {
            if (outer1 == null && outer2 == null) return null;
            if (outer1 != null && outer2 == null) return outer1;
            if (outer1 != null && outer2 == "0") return outer1;
            if (outer1 != null && outer2 != null) return outer1 + " x " + outer2;
            return null;
        }

        public static string getContact(string contact1, string contact2)
        {
            if (contact1 == null && contact2 == null) return null;
            if (contact1 != null && contact2 == null) return contact1;
            if (contact1 != null && contact2 == "0" ) return contact1;
            if (contact1 != null && contact2 != null) return contact1 + " x " + contact2;
            return null;
        }

        public static string progress(double? value)
        {
            if (value == null) return "";
            else return value + "%";
        }

        public static string isChecked(bool? solved)
        {
            if (solved == null) return "";
            if (solved == false) return "";
            else return "checked";
        }

        public static string getTicketTypeString(int? idType)
        {
            if (idType == 1) return "Technical question - ";
            if (idType == 2) return "Reclamation - ";
            if (idType == 4) return "Visit report - ";
            if (idType == 5) return "Project – ";
            return "";
        }

        // check email is correct using that regular expression : http://stackoverflow.com/questions/1365407/c-sharp-code-to-validate-email-address
        static Regex ValidEmailRegex = CreateValidEmailRegex();
        private static Regex CreateValidEmailRegex()
        {
            string validEmailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";
            return new Regex(validEmailPattern, RegexOptions.IgnoreCase);
        }
        public static bool isValidEmail(string email)
        {
            return ValidEmailRegex.IsMatch(email);
        }
    }
}