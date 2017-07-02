using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GTPTracker.Helpers
{
    public static class VRFormatter
    {
        public static string progress(int? tasks, int? tasksDone)
        {
            if (tasks == null || tasksDone == null) return "";
            if (tasks == 0) return "0%";
            float value = ((float)tasksDone / (float)tasks) * 100;
            return (int) value + "%;";
        }

        public static string getDate(DateTime? fecha)
        {
            if (fecha == null) return "";
            else return ((DateTime) fecha).ToShortDateString();
        }

        public static string getPadlock(bool isInternal)
        {
            if (isInternal) return "<i class=\"fa fa-lock\"></i>";
            else return "";
        }

        public static string getThumbnail(string source)
        {
            if (source.EndsWith(".png") || source.EndsWith(".jpg") || source.EndsWith(".JPG") || source.EndsWith(".PNG"))
                return "<img src=\"/Content/VisitReportsFiles/"+source+"\" width=\"256\" height=\"256\" />";
            else return "<img src=\"/assets/images/holder-1.png\" alt=\"holder\" />";
        }

        public static string useLightBox(string source)
        {
            if (source.EndsWith(".png") || source.EndsWith(".jpg") || source.EndsWith(".JPG") || source.EndsWith(".PNG"))
                return "data-toggle=\"lightbox\"";
            else return "";
        }
    }
}