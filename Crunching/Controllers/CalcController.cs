using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GTPTracker.Helpers;

namespace GTPTrackers.Controllers
{
    [TermsFilterAttributes]
    public class CalcController : Controller
    {
        //
        // GET: /Calc/

        public ActionResult F48()
        {
            return View();
        }

        public ActionResult Index()
        {
            ViewBag.ShowAnonUserPanel = !(Session["IDUSER"] != null && System.Web.HttpContext.Current.User.Identity.IsAuthenticated);
            return View();
        }

        [HttpPost]
        public ActionResult F48(string r, string h)
        {
            float val_r=0;
            float val_h = 0;
            try
            {
                val_r = float.Parse(r);
                val_h = float.Parse(h);
                float result = (val_r * val_h) / (2 * (val_r + val_h));
                ViewBag.h = h;
                ViewBag.r = r;
                ViewBag.result = result;
            }
            catch 
            {
                ViewBag.result = "Parameters are not float or div by zero";
            }
            return View();
        }

        public ActionResult F51()
        {
            return View();
        }

        [HttpPost]
        public ActionResult F51(string a, string h)
        {
            float val_a = 0;
            float val_h = 0;
            try
            {
                val_a = float.Parse(a);
                val_h = float.Parse(h);
                float result = (val_a * val_h) / (2 * (val_a + val_h));
                ViewBag.a = a;
                ViewBag.h = h;
                ViewBag.result = result;
            }
            catch 
            {
                ViewBag.result = "Parameters are not float or div by zero";
            }
            return View();
        }
        
        public JsonResult truncatedCone(string d)
        {
            float val_d = 0;
            float result = 0;
            try
            {
                val_d = float.Parse(d);  
                result = ((2 * val_d) / 5);
            }
            catch 
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult pyramideStump(string d)
        {
            float val_d = 0;
            float result = 0;
            try
            {
                val_d = float.Parse(d);
                result = ((2 * val_d) / 5);
            }
            catch 
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }
      
        public JsonResult squareBar(string a)
        {
            float val_a = 0;
            float result;
            try
            {
                val_a = float.Parse(a);
                result = (val_a / 4);
            }
            catch 
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult rectangularBar(string a, string b)
        {
            float val_a = 0;
            float val_b = 0;
            float result;
            try
            {
                val_a = float.Parse(a);
                val_b = float.Parse(b);
                result = (val_a * val_b) / (2 * (val_a + val_b));
            }
            catch 
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult cube(string a)
        {
            float val_a = 0;
            float result;
            try
            {
                val_a = float.Parse(a);
                result = (val_a / 6);
            }
            catch 
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult tube(string d)
        {
            float val_d = 0;
            float result;
            try
            {
                val_d = float.Parse(d);
                result = (val_d / 4);
            }
            catch 
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult circleSection(string a, string d)
        {
            float val_d = 0;
            float val_a = 0;
            float result;
            try
            {
                val_d = float.Parse(d);
                val_a = float.Parse(a);
                if (val_a >= val_d * 5)
                    result = (val_d / 2);
                else result = 0;
            }
            catch 
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult enclosedCylinder(string a)
        {
            float val_a = 0;
            float result;
            try
            {
                val_a = float.Parse(a);
                result = (val_a / 6);
            }
            catch 
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult sphere(string a)
        {
            float val_a = 0;
            float result;
            try
            {
                val_a = float.Parse(a);
                result = (val_a / 6);
            }
            catch 
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ovalProfile(string a, string b)
        {
            float val_a = 0;
            float val_b = 0;
            float result;
            try
            {
                val_a = float.Parse(a);
                val_b = float.Parse(b);
                result = (val_a * val_b) / (2 * (val_a + val_b));
            }
            catch 
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult barWithNonCoolingSuperficies(string a, string b, string c)
        {
            float val_a = 0;
            float val_b = 0;
            float val_c = 0;
            float result;
            try
            {
                val_a = float.Parse(a);
                val_b = float.Parse(b);
                val_c = float.Parse(c);
                result = (val_a * val_b) / ((2 * (val_a + val_b))-val_c);
            }
            catch 
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult cylinder(string r, string h)
        {
            float val_r = 0;
            float val_h = 0;
            float result;
            try
            {
                val_r = float.Parse(r);
                val_h = float.Parse(h);
                result = (val_r * val_h) / (2 * (val_r + val_h));
            }
            catch 
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult hollowCylinder(string a, string b)
        {
            float val_a = 0;
            float val_b = 0;
            float result;
            try
            {
                val_a = float.Parse(a);
                val_b = float.Parse(b);
                result = (val_a * val_b) / (2 * (val_a + val_b));
            }
            catch 
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult cylinderWithOneNonCoolingSuperficies(string a, string b, string c, string d, string e)
        {
            float val_a = 0;
            float val_b = 0;
            float val_c = 0;
            float val_d = 0;
            float val_e = 0;
            float result;
            try
            {
                val_a = float.Parse(a);
                val_b = float.Parse(b);
                val_c = float.Parse(c);
                val_d = float.Parse(d);
                val_e = float.Parse(e);
                result = (val_a * val_b * val_c) / ((2 * ((val_a * val_b) + (val_b * val_c) + (val_a * val_c))) - (val_d * val_e));
            }
            catch 
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult cylinderWithSeveralNonCoolingSuperficies(string a, string b, string c, string d1, string e1, string d2, string e2)
        {
            float val_a = 0;
            float val_b = 0;
            float val_c = 0;
            float val_d1 = 0;
            float val_e1 = 0;
            float val_d2 = 0;
            float val_e2 = 0;
            float result;
            try
            {
                val_a = float.Parse(a);
                val_b = float.Parse(b);
                val_c = float.Parse(c);
                val_d1 = float.Parse(d1);
                val_e1 = float.Parse(e1);
                val_d2 = float.Parse(d2);
                val_e2 = float.Parse(e2);
                result = (val_a * val_b * val_c) / ((2 * ((val_a * val_b) + (val_b * val_c) + (val_a * val_c))) - ((val_d1 * val_e1) +(val_d2 * val_e2)) );
            }
            catch 
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult cylinderWithFourNonCoolingSuperficies(string a, string b, string c, string d, string e)
        {
            float val_a = 0;
            float val_b = 0;
            float val_c = 0;
            float val_d = 0;
            float val_e = 0;
            float result;
            try
            {
                val_a = float.Parse(a);
                val_b = float.Parse(b);
                val_c = float.Parse(c);
                val_d = float.Parse(d);
                val_e = float.Parse(e);
                result = (val_a * val_b * val_c) / ((2 * ((val_a * val_b) + (val_b * val_c) + (val_a * val_c))) - ((val_d * val_e)*4));
            }
            catch
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult barWithTwoNonCoolingSuperficies(string a, string b, string c1, string c2)
        {
            float val_a = 0;
            float val_b = 0;
            float val_c1 = 0;
            float val_c2 = 0;
            float result;
            try
            {
                val_a = float.Parse(a);
                val_b = float.Parse(b);
                val_c1 = float.Parse(c1);
                val_c2 = float.Parse(c2);
                result = (val_a * val_b) / ((2 * (val_a + val_b) - val_c1 - val_c2));
            }
            catch 
            {
                result = 0;
            }
            return Json(result.ToString("0.00"), JsonRequestBehavior.AllowGet);
        }

    }
}
