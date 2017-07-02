using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GTPTracker.repos;
using Crunching.Models;
using System.IO;
using GTPTracker.Functions;

namespace GTPTrackers.Controllers
{
    public class APIController : Controller
    {
        repoProducts repProducts = new repoProducts();
        repoCustomers repCustomers = new repoCustomers();
        repoUsers repUsers = new repoUsers();
        repoTickets repTicket = new repoTickets();
        repoPhotos repPhotos = new repoPhotos();

        EmailManager emailMgr = new EmailManager();
        TasksManager tasksMgr = new TasksManager();

        public JsonResult getProducts(string linkID)
        {
            int idTenant = repCustomers.getIdTenantByLinkID(linkID);
            IEnumerable<Products> lProducts = repProducts.getList(idTenant);
            return Json(lProducts, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getPeople(string linkID)
        {
            int idTenant = repCustomers.getIdTenantByLinkID(linkID);
            IEnumerable<Users> lista = repUsers.getListByTenant(idTenant);
            return Json(lista, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult UploadImage(HttpPostedFileBase file, string idTicket, string linkID)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                  //  if (repPhotos.alreadyExists(file.FileName) == true) return Json(new HttpStatusCodeResult(401), JsonRequestBehavior.AllowGet);
                    var fileName = "Photo_" + Convert.ToString(idTicket) + "__" + DateTime.UtcNow.Ticks.ToString() + "-" + System.IO.Path.GetFileName(file.FileName);
                    var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    file.SaveAs(path);

                    int idTenant = repCustomers.getIdTenantByLinkID(linkID);
                    if (idTenant != 0)
                    {
                        int ParsedIdTicket = 0;
                        if (Int32.TryParse(idTicket, out ParsedIdTicket))
                        {
                            Photos photo = new Photos();
                            photo.cDate = DateTime.Now;
                            photo.idCreator = 1001;
                            photo.idTicket = ParsedIdTicket;
                            photo.name = fileName;
                            int idPhoto = repPhotos.Create(photo);
                            //return Json(photo.name);
                            return Json(new HttpStatusCodeResult(200), JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                return Json(new HttpStatusCodeResult(404), JsonRequestBehavior.AllowGet);
            }
            catch { return Json(new HttpStatusCodeResult(404), JsonRequestBehavior.AllowGet); }
        }
        
/*
        [HttpPost]
        public JsonResult UploadImage(HttpPostedFileBase file, string idTicket, string linkID)
        {
            if (file.ContentLength > 0)
            {
                
                var fileName = "Photo_" + Convert.ToString(idTicket) + "__" + DateTime.UtcNow.Ticks.ToString() + "-" + System.IO.Path.GetFileName(file.FileName);
                var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                file.SaveAs(path);

                int idTenant = repCustomers.getIdTenantByLinkID(linkID);
                if (idTenant != 0)
                {
                    int ParsedIdTicket = 0;
                    if (Int32.TryParse(idTicket, out ParsedIdTicket))
                    {
                        Photos photo = new Photos();
                        photo.cDate = DateTime.Now;
                        photo.idCreator = 1001;
                        photo.idTicket = ParsedIdTicket;
                        photo.name = fileName;
                        int idPhoto = repPhotos.Create(photo);
                        return Json(photo.name);
                    }
                }
            }
            return Json(0);
        }*/

        public JsonResult CreateTicket(string product, string description, string pallet, string casting, string technical, string recurringProblem, string emailResponsible, string linkID, string idMeasuresTaken, string others)
        {
            try
            {
                int idTenant = repCustomers.getIdTenantByLinkID(linkID);
                if (idTenant == 0) return Json(new HttpStatusCodeResult(404), JsonRequestBehavior.AllowGet);
                Products p = repProducts.GetByType(product);
                Tickets ticket = new Tickets();
                ticket.description = description;
                ticket.casting = casting;
                ticket.pallet = pallet;
                ticket.cDate = DateTime.Now;
                if (recurringProblem == "1") ticket.recurringProblem = true;
                else ticket.recurringProblem = false;
                if (technical != null)
                {
                    if (technical.Contains("echni")) ticket.idType = 1;
                    else ticket.idType = 2;
                }
                else ticket.idType = 3;
                if (p != null) ticket.idProduct = p.id;
                ticket.idTenant = idTenant;
                ticket.idStatus = 1;
                ticket.others = others;
                try { ticket.idMeasuresTaken = Convert.ToInt32(idMeasuresTaken); }
                catch { return Json(new HttpStatusCodeResult(404), JsonRequestBehavior.AllowGet); }

                using (SecurityMgr SecurityManager = new SecurityMgr())
                {
                    int? idKAM = SecurityManager.getMyKAMByLINKID(linkID);
                    ticket.idResponsible = (idKAM == null) ? 0 : (int)idKAM;
                    if (ticket.idType == 3)
                    {
                        using (repoProducts repProduct = new repoProducts())
                        {
                            Products productElement = repProduct.Get((int)ticket.idProduct);
                            ticket.title = "Sample request for " + productElement.type;
                        }
                    }
                    ticket.title = GTPTracker.Helpers.formatter.getTicketTypeString(ticket.idType) + ticket.title;

                    int idTicket = repTicket.Create(ticket);

                    //tasksMgr.createDefaultTask(ticket, 0); // create the default task 

                    if (ticket.idType == 3) tasksMgr.AutoTaskSamples(repTicket.getv(ticket.id), (int)ticket.idResponsible);
                    if (ticket.idType == 2) tasksMgr.AutoTaskReclamation(repTicket.getv(ticket.id), (int)ticket.idResponsible);
                    if (ticket.idType == 1) tasksMgr.AutoTaskTechnical(repTicket.getv(ticket.id), (int)ticket.idResponsible);

                    return Json(new { StatusCode = "200", idTicket = idTicket }, JsonRequestBehavior.AllowGet);
                }
            }
            catch { return Json(new HttpStatusCodeResult(404), JsonRequestBehavior.AllowGet); }
        }
/*
 public JsonResult CreateTicket(string product, string description, string pallet, string casting, string technical, string recurringProblem, string emailResponsible, string linkID, string idMeasuresTaken, string others)
        {
            int idTenant = repCustomers.getIdTenantByLinkID(linkID);
            if (idTenant == 0) return Json(0);
            Products p = repProducts.GetByType(product);
            Tickets ticket = new Tickets();
            ticket.description = description;
            ticket.casting = casting;
            ticket.pallet = pallet;
            ticket.cDate = DateTime.Now;
            if (recurringProblem == "1") ticket.recurringProblem = true;
            else ticket.recurringProblem = false;
            if (technical != null)
            {
                if (technical.Contains("echni")) ticket.idType = 1;
                else ticket.idType = 2;
            }
            else ticket.idType = 3;
            if (p != null) ticket.idProduct = p.id;
            ticket.idTenant = idTenant;
            ticket.idStatus = 1;
            ticket.others = others;
            try { ticket.idMeasuresTaken = Convert.ToInt32(idMeasuresTaken); }
            catch {  } 
            int idTicket = repTicket.Create(ticket);
            tasksMgr.createDefaultTask(ticket, 0); // create the default task 
            return Json(idTicket, JsonRequestBehavior.AllowGet); 
        }*/

        public JsonResult AddUserToTicket(string idTicket, string email, string linkID)
        {
            try
            {
                int idTenant = repCustomers.getIdTenantByLinkID(linkID);
                if (idTenant != 0)
                {
                    int ParsedIdTicket = 0;
                    if (Int32.TryParse(idTicket, out ParsedIdTicket))
                    {
                        Tickets ticket = repTicket.get(ParsedIdTicket);
                        if (ticket != null)
                            if (email == "GTP")
                            {
                                // add each GTP user to the distribution list
                                IEnumerable<Users> lGTPPeople = repUsers.getGTPPeopleForThisTicket(ParsedIdTicket); //.getGTPPeople();
                                foreach (var item in lGTPPeople)
                                {
                                    repTicket.AddUserToTicket(item.email, ticket.id);
                                    emailMgr.emailNewTicket(ticket, item);
                                }
                            }
                            else
                            {
                                repTicket.AddUserToTicket(email, ticket.id);
                                Users user = repUsers.getUserByEmail(email);
                                emailMgr.emailNewTicket(ticket, user);
                            }
                    }
                }
                return Json(new HttpStatusCodeResult(200), JsonRequestBehavior.AllowGet);
            } catch { return Json(new HttpStatusCodeResult(404), JsonRequestBehavior.AllowGet);}
        }


  /*      public void AddUserToTicket(string idTicket, string email, string linkID)
        {
            int idTenant = repCustomers.getIdTenantByLinkID(linkID);
            if (idTenant != 0)
            {
                int ParsedIdTicket = 0;
                if (Int32.TryParse(idTicket, out ParsedIdTicket))
                {
                    Tickets ticket = repTicket.get(ParsedIdTicket);
                    if (ticket != null)
                        if (email == "GTP")
                        {
                            // add each GTP user to the distribution list
                            IEnumerable<Users> lGTPPeople = repUsers.getGTPPeople();
                            foreach (var item in lGTPPeople)
                            {
                                repTicket.AddUserToTicket(item.email, ticket.id);
                                emailMgr.emailNewTicket(ticket, item);
                            }
                        }
                        else
                        {
                            repTicket.AddUserToTicket(email, ticket.id);
                            Users user = repUsers.getUserByEmail(email);
                            emailMgr.emailNewTicket(ticket, user);
                        }
                }
            }
        }*/

        public int RegisterMobileApp(String linkID)
        {
            return repCustomers.getIdTenantByLinkID(linkID);
        }

        public JsonResult amIAlive(string controllerName, string username, string action)
        {
            using (repoCustomers repCustomers = new repoCustomers())
            {
                try
                {
                    Logs log = new Logs();
                    log.action = action;
                    log.controller = controllerName;
                    log.createdAt = DateTime.UtcNow;
                    log.userName = username;
                    using (repoLogs repLogs = new repoLogs())
                    {
                        repLogs.Create(log);
                    }
                    return Json(new HttpStatusCodeResult(200), JsonRequestBehavior.AllowGet);
                }
                catch { return Json(new HttpStatusCodeResult(500), JsonRequestBehavior.AllowGet); }
            }
        }

        public JsonResult sessionRefreshingEnd(string controllerName, string username, string action)
        {
            using (repoCustomers repCustomers = new repoCustomers())
            {
                try
                {
                    Logs log = new Logs();
                    log.action = action;
                    log.controller = controllerName;
                    log.createdAt = DateTime.UtcNow;
                    log.userName = username;
                    using (repoLogs repLogs = new repoLogs())
                    {
                        repLogs.Create(log);
                    }
                    return Json(new HttpStatusCodeResult(200), JsonRequestBehavior.AllowGet);                    
                }
                catch { return Json(new HttpStatusCodeResult(500), JsonRequestBehavior.AllowGet); }
            }
        }
    }
}
