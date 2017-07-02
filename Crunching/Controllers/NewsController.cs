using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Crunching.Models;
using GTPTracker.repos;
using GTPTracker.Helpers;

namespace GTPTracker.Controllers
{
    public class NewsController : Controller
    {
        repoNews repNews = new repoNews();
        repoProducts repProducts = new repoProducts();
        repoCustomers repCustomers = new repoCustomers();
        repoUsers repUsers = new repoUsers();

        [SessionExpireFilter]
        public ActionResult Index()
        {
            IEnumerable<News> lNews = repNews.getList();
           return View(lNews);
        }

        [SessionExpireFilter]
        public ActionResult Create()
        {
            ViewBag.lProducts = new MultiSelectList(repProducts.getAll(), "id", "type");
            ViewBag.lCustomers = new MultiSelectList(repCustomers.getList(), "id", "name");
            News noticia = new News();
            return View(noticia);
        }

        public JsonResult DetailsJSON(int id)
        {
            News element = repNews.get(id);
            return Json(element, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(int id)
        {
            News element = repNews.get(id);
            return View(element);
        }

        public ActionResult PartialView(int id)
        {
            News element = repNews.get(id);
            return View(element);
        }

        [SessionExpireFilter]
        [HttpPost]
        [Authorize(Roles = "GTP")]
        public ActionResult Create(News news, int[] productsID, int[] customersID)
        {
            try
            {
                news.idCreator = (int) Session["IDUSER"];
                repNews.Create(news);
                if (news.everyone == true)
                {
                    IEnumerable<Users> lUsers = repUsers.getList();
                    foreach (var user in lUsers)
                        repNews.addUser(news.id, user.id);
                }
                else
                {
                    repNews.addUser(news.id, (int)Session["IDUSER"]); // add creator to the watchlist
                    // users from selected customers
                    IEnumerable<Users> lUsers = repUsers.getList().Where(p => customersID.Contains(p.idTenant));
                    if (lUsers!= null && customersID!=null)
                        foreach (var user in lUsers)
                            repNews.addUser(news.id, user.id);
                    IEnumerable<int> lCustomers = repProducts.getCustomersByProductsList(productsID);
                    if (lCustomers != null && productsID!=null)
                    {
                        IEnumerable<Users> lAllUsers = repUsers.getList();
                        IEnumerable<Users> lUsersFromProducts = lAllUsers.Where(p => lCustomers.Contains(p.idTenant));
                        foreach (var user in lUsersFromProducts)
                            repNews.addUser(news.id, user.id);
                    }
                }
               // return RedirectToAction("Index", "News");
                return RedirectToAction("Index", "Items");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(news);
            }
        }

     
    }
}
