using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GTPTracker.repos;
using Crunching.Models;
using GTPTracker.Helpers;
using GTPTracker.Functions;

namespace GTPTracker.Controllers
{
    public class ItemsController : Controller
    {
        repoItems repItems = new repoItems();

        [SessionExpireFilter]
        [TermsFilterAttributes]
        public ActionResult Index(int? type, int page=0, bool archived = false)
        {
            ItemsManager itemsMgr = new ItemsManager();
            int idUser = (int) Session["IDUSER"];
            IEnumerable<vWatchListItems> lItems;
            if (type!= null) lItems = itemsMgr.getMyItems(idUser).Where(p=>p.idType == (int) type );
            else lItems = itemsMgr.getMyItems(idUser);

            if (archived == false) lItems = lItems.Where(p => p.archived != true);
            else lItems = lItems.Where(p => p.archived == true);

            const int pageSize = 10;
            lItems = lItems.Skip(page * pageSize).Take(pageSize).ToList();
            lItems.OrderByDescending(p => p.cDate);
            ViewBag.page = page + 1;
            ViewBag.type = type;
            ViewBag.archived = archived;
            return View(lItems);
        }

        public JsonResult IndexJSON(int? type, int page = 0, bool archived = false)
        {
            ItemsManager itemsMgr = new ItemsManager();
            int idUser = (int)Session["IDUSER"];
            IEnumerable<vWatchListItems> lItems;
            if (type != null) lItems = itemsMgr.getMyItems(idUser).Where(p => p.idType == (int)type);
            else lItems = itemsMgr.getMyItems(idUser);

            if (archived == false) lItems = lItems.Where(p => p.archived != true);
            else lItems = lItems.Where(p => p.archived == true);

            const int pageSize = 10;
            lItems = lItems.Skip(page * pageSize).Take(pageSize).ToList();
            lItems.OrderByDescending(p => p.cDate);
            ViewBag.page = page + 1;
            ViewBag.type = type;
            ViewBag.archived = archived;
            return Json(lItems, JsonRequestBehavior.AllowGet);
        }


        [SessionExpireFilter]
        public ActionResult Create()
        {
            return View();
        }
    }
}
