using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Crunching.Models;
using GTPTracker.repos;
using GTPTracker.VM;
using GTPTracker.Functions;
using GTPTracker.Helpers;
using System.IO;

namespace GTPTracker.Controllers
{
    [TermsFilterAttributes]
    public class TicketsController : Controller
    {
        repoTickets repTickets = new repoTickets();
        repoProducts repProducts = new repoProducts();
        repoCustomers repCustomers = new repoCustomers();
        repoUsers repUsers = new repoUsers();
        repoTicketsFiles repTicketFiles = new repoTicketsFiles();
        repoItems repItems = new repoItems();

        EmailManager emailMgr = new EmailManager();
        TasksManager tasksMgr = new TasksManager();

        // requested on 4/5/16 to add 3 persons whenever a new ticket is created
        private void addSpecialUsersToTicket(Tickets ticket)
        {
            Users person = new Users();
            person.email = "tobias.hesse@gtp-schaefer.de";
            person.firstName = "Tobias Hesse";
            emailMgr.emailNewTicketForSpecialGTPPeople(ticket, person);
            person.firstName = "Nick Richardson" ;
            person.email = "nicholas.richardson@gtp-schaefer.de";
            emailMgr.emailNewTicketForSpecialGTPPeople(ticket, person);
            person.firstName = "Sebastian Bähren";
            person.email = "Sebastian.baehren@gtp-schaefer.de";
            emailMgr.emailNewTicketForSpecialGTPPeople(ticket, person);     
        }

        [SessionExpireFilter]
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        [SessionExpireFilter]
        public ActionResult Reopen(int id)
        {
            Tickets item = repTickets.get(id);
            item.idStatus = 2;
            repTickets.Edit(item);
            return RedirectToAction("Details", "Tickets", new { id = id });
        }

        public ActionResult Details(int id, int? tab = 1, int? goToAnchor = 0)
        {
            ViewBag.tab = tab;
            ViewBag.goToAnchor = goToAnchor;
            Tickets ticket = repTickets.get(id);
            int idTenant = (int)HttpContext.Session["IDTENANT"];
            // security check. 
            if (!User.IsInRole("GTP") && ticket.isInternal == true) return RedirectToAction("Index", "Items");

           /* var c1 = ControllerContext.HttpContext.Request.Cookies["GTPTRACKER"];
            if (c1 == null) return RedirectToAction("signIn", "Account");
            int idTenant = Convert.ToInt32(c1.Values["IDTENANT"]);
            int idUser = Convert.ToInt32(c1.Values["IDUSER"]);
            Session["IDTENANT"] = idTenant;
            Session["IDUSER"] = idUser;
            Session["TENANT"] = c1.Values["TENANT"];
            Users currentUser = repUsers.Get(idUser);
            Session["USERNAME"] = currentUser.firstName + " " + currentUser.lastName;*/
            TicketVM vm = new TicketVM(id, idTenant);
            return View(vm);
        }

        [SessionExpireFilter]
        public ActionResult PartialView(int id, int? tab = 1, int? goToAnchor = 0)
        {
            ViewBag.tab = tab;
            ViewBag.goToAnchor = goToAnchor;
            Tickets ticket = repTickets.get(id);
            int idTenant = (int)HttpContext.Session["IDTENANT"];
            /*var c1 = ControllerContext.HttpContext.Request.Cookies["GTPTRACKER"]; ESTO NO SE PARA QUE VALE EXACTAMENTE
            if (c1 == null) return RedirectToAction("LogOn", "Account");
            int idTenant = Convert.ToInt32(c1.Values["IDTENANT"]);
            int idUser = Convert.ToInt32(c1.Values["IDUSER"]);
            Session["IDTENANT"] = idTenant;
            Session["IDUSER"] = idUser;
            Session["TENANT"] = c1.Values["TENANT"];
            Users currentUser = repUsers.Get(idUser);
            Session["USERNAME"] = currentUser.firstName + " " + currentUser.lastName;*/
            TicketVM vm = new TicketVM(id, idTenant);
            return View(vm);
        }

        [SessionExpireFilter]
        public ActionResult Create()
        {
            Tickets vm = new Tickets();
            int idTenant = (int)Session["IDTENANT"];
            ViewBag.lProducts = repProducts.getList(idTenant);
            ViewBag.lCustomers = repCustomers.getList();
            return View(vm);
        }

        [SessionExpireFilter]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(Tickets item, bool errorAlreadyShowedUp, bool recurringProblem, int idMeasures, String others)
        {
            try
            {
                item.errorAlreadyShownUp = errorAlreadyShowedUp;
                item.recurringProblem = recurringProblem;
                item.cDate = DateTime.Today;
                item.idStatus = 1;
                item.idTenant = (int)Session["IDTENANT"];
                item.idCreator = (int)Session["IDUSER"];
                item.idMeasuresTaken = idMeasures;
                item.others = others;
                repTickets.Create(item);
                IEnumerable<Users> lGTPPeople = repUsers.getGTPPeopleForThisTicket(item.id); //.getGTPPeople(); // add GTP people to the ticket
                foreach (var person in lGTPPeople)
                {
                    repTickets.AddUserToTicket(person.email, item.id);
                    emailMgr.emailNewTicket(item, person);
                }
                addSpecialUsersToTicket(item);
                tasksMgr.createDefaultTask(item, (int)Session["IDUSER"]); // create the default task 
                // return RedirectToAction("AddUserToTicketWeb", new { idTicket = item.id });
                return RedirectToAction("Details", new { id = item.id });
            }
            catch
            {
                return View();
            }
        }
        /************************ DIFFERENT TYPES OF ITEMS ********************************************/

        [SessionExpireFilter]
        public ActionResult CreateReclamation()
        {
            Tickets vm = new Tickets();
            int idTenant = (int)Session["IDTENANT"];
            ViewBag.lProducts = repProducts.getList(idTenant);
            ViewBag.lCustomers = repCustomers.getList();
            return View(vm);
        }

        [SessionExpireFilter]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateReclamation(Tickets item, int? idMeasures, IEnumerable<HttpPostedFileBase> files)
        {
            try
            {
                item.cDate = DateTime.Today;
                item.idStatus = 1;
                item.idTenant = (int)Session["IDTENANT"];
                item.idCreator = (int)Session["IDUSER"];
                item.idType = 2; // reclamation
                using (SecurityMgr SecurityManager = new SecurityMgr())
                {
                    item.idResponsible = SecurityManager.getMyKAM((int)Session["IDUSER"]);
                    item.title = Helpers.formatter.getTicketTypeString(item.idType) + item.title;
                    item.idMeasuresTaken = idMeasures;
                    repTickets.Create(item);

                    if (files != null)
                        foreach (var file in files)
                            addFile(item.id, file);

                    if (!User.IsInRole("GTP"))
                    { // add user to watch list
                        Users creator = repUsers.Get((int)Session["IDUSER"]);
                        repTickets.AddUserToTicket(creator.email, item.id);
                    }

                    IEnumerable<Users> lGTPPeople = repUsers.getGTPPeopleForThisTicket(item.id); //.getGTPPeople(); // add GTP people to the ticket
                    foreach (var person in lGTPPeople)
                    {
                        repTickets.AddUserToTicket(person.email, item.id);
                        emailMgr.emailNewTicket(item, person);
                    }
                    addSpecialUsersToTicket(item);
                    // tasksMgr.AutoTaskReclamation(repTickets.getv(item.id), (int)Session["IDUSER"]);

                    return RedirectToAction("Details", new { id = item.id });
                }
            }
            catch
            {
                return View();
            }
        }

        [SessionExpireFilter]
        public ActionResult CreateTechnicalQuestion()
        {
            Tickets vm = new Tickets();
            int idTenant = (int)Session["IDTENANT"];
            ViewBag.lProducts = repProducts.getList(idTenant);
            ViewBag.lCustomers = repCustomers.getList();
            return View(vm);
        }

        [SessionExpireFilter]
        [HttpPost]
        public ActionResult CreateTechnicalQuestion(Tickets item, bool? errorAlreadyShowedUp, bool? recurringProblem, int? idMeasures, String others, IEnumerable<HttpPostedFileBase> files)
        {
            try
            {
                item.errorAlreadyShownUp = errorAlreadyShowedUp;
                item.recurringProblem = false;//recurringProblem;
                item.cDate = DateTime.Today;
                item.idStatus = 1;
                item.idTenant = (int)Session["IDTENANT"];
                item.idCreator = (int)Session["IDUSER"];
                item.idMeasuresTaken = idMeasures;
                item.others = others;
                item.idType = 1;
                using (SecurityMgr SecurityManager = new SecurityMgr())
                {
                    item.idResponsible = SecurityManager.getMyKAM((int)Session["IDUSER"]);
                    item.title = Helpers.formatter.getTicketTypeString(item.idType) + item.title;
                    repTickets.Create(item);

                    if (files != null)
                        foreach (var file in files)
                            addFile(item.id, file);

                    if (!User.IsInRole("GTP"))
                    { // add user to watch list
                        Users creator = repUsers.Get((int)Session["IDUSER"]);
                        repTickets.AddUserToTicket(creator.email, item.id);
                    }
                    IEnumerable<Users> lGTPPeople = repUsers.getGTPPeopleForThisTicket(item.id); //.getGTPPeople(); // add GTP people to the ticket
                    foreach (var person in lGTPPeople)
                    {
                        repTickets.AddUserToTicket(person.email, item.id);
                        emailMgr.emailNewTicket(item, person);
                    }
                    addSpecialUsersToTicket(item);
                    //  tasksMgr.AutoTaskTechnical(repTickets.getv(item.id), (int)Session["IDUSER"]);

                    return RedirectToAction("Details", new { id = item.id });
                }
            }
            catch 
            {
                return View();
            }
        }


        [SessionExpireFilter]
        public ActionResult CreateBesuchsbericht()
        {
            Tickets vm = new Tickets();
            int idTenant = (int)Session["IDTENANT"];
            ViewBag.lProducts = repProducts.getList(idTenant);
          //  ViewBag.lCustomers = repCustomers.getList();
            ViewBag.lCustomers = new MultiSelectList(repCustomers.getList(), "id", "name");
            return View(vm);
        }

        [SessionExpireFilter]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateBesuchsbericht(Tickets item, IEnumerable<HttpPostedFileBase> files, int[] customersID)
        {
            try
            {
                item.cDate = DateTime.Today;
                item.idStatus = 1;
                item.idTenant = (int)Session["IDTENANT"];
                item.idCreator = (int)Session["IDUSER"];
                item.idType = 4; // besuchsbericht
                using (SecurityMgr SecurityManager = new SecurityMgr())
                {
                    item.idResponsible = SecurityManager.getMyKAM((int)Session["IDUSER"]);
                    item.title = Helpers.formatter.getTicketTypeString(item.idType) + item.title;
                    repTickets.Create(item);

                    if (files != null)
                        foreach (var file in files)
                            addFile(item.id, file);

                    if (!User.IsInRole("GTP"))
                    { // add user to watch list
                        Users creator = repUsers.Get((int)Session["IDUSER"]);
                        repTickets.AddUserToTicket(creator.email, item.id);
                    }
                    IEnumerable<Users> lGTPPeople = repUsers.getGTPPeopleForThisTicket(item.id); //.getGTPPeople(); // add GTP people to the ticket
                    foreach (var person in lGTPPeople)
                    {
                        repTickets.AddUserToTicket(person.email, item.id);
                        emailMgr.emailNewTicket(item, person);
                    }
                    IEnumerable<Users> lUsers = repUsers.getList().Where(p => customersID.Contains(p.idTenant));
                    if (lUsers != null && customersID != null)
                        foreach (var user in lUsers)
                        {
                            repTickets.AddUserToTicket(user.email, item.id);
                            emailMgr.emailNewTicket(item, user);
                        }
                    addSpecialUsersToTicket(item);
                    
                    //tasksMgr.AutoTaskVist(repTickets.getv(item.id), (int)Session["IDUSER"]);

                    return RedirectToAction("Details", new { id = item.id });
                }
            }
            catch
            {
                return View();
            }
        }

        [SessionExpireFilter]
        public ActionResult CreateProject()
        {
            Tickets vm = new Tickets();
            int idTenant = (int)Session["IDTENANT"];
            ViewBag.lCustomers = new MultiSelectList(repCustomers.getList(), "id", "name");
            return View(vm);
        }

        [SessionExpireFilter]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateProject(Tickets item, IEnumerable<HttpPostedFileBase> files)
        {
            try
            {
                item.cDate = DateTime.Today;
                item.idStatus = 1;
                item.idTenant = (int)Session["IDTENANT"];
                item.idCreator = (int)Session["IDUSER"];
                item.idType = 5; // Project
                using (SecurityMgr SecurityManager = new SecurityMgr())
                {
                    item.idResponsible = SecurityManager.getMyKAM((int)Session["IDUSER"]);
                    item.title = Helpers.formatter.getTicketTypeString(item.idType) + item.title;
                    repTickets.Create(item);

                    if (files != null)
                        foreach (var file in files)
                            addFile(item.id, file);

                    if (!User.IsInRole("GTP"))
                    { // add user to watch list
                        Users creator = repUsers.Get((int)Session["IDUSER"]);
                        repTickets.AddUserToTicket(creator.email, item.id);
                    }
                    IEnumerable<Users> lGTPPeople = repUsers.getGTPPeopleForThisTicket(item.id); //.getGTPPeople(); // add GTP people to the ticket
                    foreach (var person in lGTPPeople)
                    {
                        repTickets.AddUserToTicket(person.email, item.id);
                        emailMgr.emailNewTicket(item, person);
                    }
                    addSpecialUsersToTicket(item);
                    //tasksMgr.AutoTaskProject(repTickets.getv(item.id), (int)Session["IDUSER"]);

                    return RedirectToAction("Details", new { id = item.id });
                }
            }
            catch
            {
                return View();
            }
        }

        [SessionExpireFilter]
        public ActionResult CreateSample(int id)
        {
            Tickets vm = new Tickets();
            vm.idProduct = id;
            int idTenant = (int)Session["IDTENANT"];
            return View(vm);
        }

        [SessionExpireFilter]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateSample(Tickets item, bool? springLoadedDrawing, bool? fixedPinDrawing, bool? riserDrawing, bool? technicalDrawing, IEnumerable<HttpPostedFileBase> files)
        {
            try
            {
                item.cDate = DateTime.Today;
                item.idStatus = 1;
                item.idTenant = (int)Session["IDTENANT"];
                item.idCreator = (int)Session["IDUSER"];
                item.idType = 3; // Sample
                using (repoProducts repProduct = new repoProducts())
                {
                    Products product = repProduct.Get((int)item.idProduct);
                    item.title = "Sample request for " + product.type;
                    using (SecurityMgr SecurityManager = new SecurityMgr())
                    {
                        item.idResponsible = SecurityManager.getMyKAM((int)Session["IDUSER"]);
                        repTickets.Create(item);

                        if (files != null)
                            foreach (var file in files)
                                addFile(item.id, file);

                        if (!User.IsInRole("GTP"))
                        { // add user to watch list
                            Users creator = repUsers.Get((int)Session["IDUSER"]);
                            repTickets.AddUserToTicket(creator.email, item.id);
                        }
                        IEnumerable<Users> lGTPPeople = repUsers.getGTPPeopleForThisTicket(item.id); //.getGTPPeople(); // add GTP people to the ticket
                        foreach (var person in lGTPPeople)
                        {
                            repTickets.AddUserToTicket(person.email, item.id);
                            emailMgr.emailNewTicket(item, person);
                        }
                        addSpecialUsersToTicket(item);
                        //tasksMgr.AutoTaskSamples(repTickets.getv(item.id), (int)Session["IDUSER"]);
                        return RedirectToAction("Details", new { id = item.id });
                    }
                }
            }
            catch
            {
                return View();
            }
        }

        /*******************************************************************/
        [SessionExpireFilter]
        public ActionResult Edit(int id)
        {
            Tickets item = repTickets.get(id);
            return View(item);
        }

        [SessionExpireFilter]
        [HttpPost]
        [Authorize(Roles = "GTP")]
        public ActionResult Edit(Tickets item)
        {
            try
            {
                Tickets ticket = repTickets.get(item.id); // we only edit four fields, so the rest will be nulled. So we get the rest from db
                ticket.reason = item.reason;
                ticket.longTimeAction = item.longTimeAction;
                ticket.shortTimeAction = item.shortTimeAction;
                repTickets.Edit(ticket);
                return RedirectToAction("Details", "Tickets", new { id = item.id, tab = 4 });
            }
            catch
            {
                return View();
            }
        }

        [SessionExpireFilter]
        public ActionResult Close(int id)
        {
            Tickets ticket = repTickets.get(id);
            if (ticket != null)
            {
                ticket.idStatus = 3; // cerrado
                ticket.idUserWhoClose = (int)Session["IDUSER"]; // who close the ticket?
                repTickets.Edit(ticket);
                IEnumerable<vTicketDistributionList> lUsers = repTickets.getDistributionList(ticket.id);
                foreach (var person in lUsers)
                {
                    // if (person.idTenant != 1){ // remove gtpusers
                    Users user = repUsers.Get(person.idUser);
                    emailMgr.emailResolution(ticket, user);
                    //}
                }
            }
            if (HttpContext.User.IsInRole("GTP"))
                return RedirectToAction("Edit", "Tickets", new { id = id }); // GTP users when closing have to tell the reason
            else return RedirectToAction("Details", "Tickets", new { id = id });
        }

        public ActionResult CloseAndReturnToStream(int id)
        {
            Tickets ticket = repTickets.get(id);
            if (ticket != null)
            {
                // only the asignee can close the ticket
                if (ticket.idResponsible == (int)Session["IDUSER"])
                {
                    ticket.idStatus = 3; // cerrado
                    ticket.idUserWhoClose = (int)Session["IDUSER"]; // who close the ticket?
                    repTickets.Edit(ticket);
                    IEnumerable<vTicketDistributionList> lUsers = repTickets.getDistributionList(ticket.id);
                    foreach (var person in lUsers)
                    {
                        // if (person.idTenant != 1){ // remove gtpusers
                        Users user = repUsers.Get(person.idUser);
                        emailMgr.emailResolution(ticket, user);
                        //}
                    }
                }
            }
            return RedirectToAction("Index", "Items");
        }

        [SessionExpireFilter]
        public ActionResult Delete(int id)
        {
            Tickets item = repTickets.get(id);
            return View(item);
        }

        [SessionExpireFilter]
        [HttpPost]
        [Authorize(Roles = "GTP")]
        public ActionResult Delete(Tickets item)
        {
            try
            {
                repTickets.Delete(item);
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                return View();
            }
        }

        [SessionExpireFilter]
        public ActionResult Assign(int id)
        {
            int idTenant = (int)Session["IDTENANT"];
            TicketVM vm = new TicketVM(id, idTenant);
            ViewBag.lUsers = repUsers.getGTPPeopleForThisTicket(id);
            ViewBag.id = id;
            return View(vm);
        }

        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Assign(int idResponsible, int id)
        {
            try
            {
                Tickets ticket = repTickets.get(id); // we only edit four fields, so the rest will be nulled. So we get the rest from db
                ticket.idResponsible = idResponsible;
                repTickets.Edit(ticket);
                return RedirectToAction("Details", "Tickets", new { id = id, tab = 4 });
            }
            catch
            {
                return View();
            }
        }

        [SessionExpireFilter]
    //    [HttpPost]  // it will be called from the ticket details 28/04/2016
        public ActionResult Archive(int id)
        {
            Tickets ticket = repTickets.get(id);
            if (ticket != null)
            {
                ticket.archived = true;
                repTickets.Edit(ticket);
                repItems.ArchiveItemForTicket(ticket.id);
            }
            return RedirectToAction("Index", "Items");
        }

        [SessionExpireFilter]
        [HttpPost]
        public ActionResult Unarchive(int id)
        {
            Tickets ticket = repTickets.get(id);
            if (ticket != null)
            {
                ticket.archived = false;
                repTickets.Edit(ticket);
                repItems.UnarchiveItemForTicket(ticket.id);
            }
            return RedirectToAction("Index", "Items");
        }

        /******************************************************/
        [SessionExpireFilter]
        public ActionResult requestSample(int idProduct, int? serie, int? origen)
        {
            ViewBag.idProduct = idProduct;
            ViewBag.serie = serie;
            ViewBag.origen = origen;
            return View();
        }

        [SessionExpireFilter]
        [HttpPost]
        public ActionResult requestSample(int idProduct, int? serie, int? origen, string highlights, string description)
        {
            Tickets item = new Tickets(); // create ticket
            item.cDate = DateTime.Today;
            item.idStatus = 1;
            item.idTenant = (int)Session["IDTENANT"];
            item.idCreator = (int)Session["IDUSER"];
            item.idProduct = idProduct;
            item.description = description;
            item.requestSampleHighlights = highlights;
            item.idType = 3; // a sample is a type 3 reason
            using (repoProducts repProduct = new repoProducts())
            {
                Products product = repProduct.Get(idProduct);
                item.title = "Sample request for " + product.type;
                repTickets.Create(item);
                IEnumerable<Users> lGTPPeople = repUsers.getGTPPeople(); // add GTP people to the ticket
                foreach (var person in lGTPPeople)
                {
                    repTickets.AddUserToTicket(person.email, item.id);
                    emailMgr.emailNewTicket(item, person);
                }
                if (origen == null) return RedirectToAction("getListBySerie", "Products", new { id = (int)serie });
                else return RedirectToAction("Details", "Products", new { id = idProduct, origen = (int)origen });
            }
        }

        // From the ticket details page ******************************************

        [SessionExpireFilter]
        public ActionResult AddUserToTicket(int idTicket) // call to the page
        {
            addUserToTicketVM vm = new addUserToTicketVM(idTicket, (int)Session["IDTENANT"]);
            return View(vm);
        }

        [SessionExpireFilter] 
        public ActionResult addThisUserToTicket(int idUser, int idTicket)
        {
            repTickets.AddUserToTicket(idUser, idTicket);
            Users user = repUsers.Get(idUser);
            Tickets ticket = repTickets.get(idTicket); // whenever an user is added to a ticket, an email is sent
            emailMgr.emailNewTicket(ticket, user);
            return RedirectToAction("Details", new { id = idTicket, tab = 3 });
        }

        [SessionExpireFilter]
        public ActionResult addCustomerToTicket(int idCustomer, int idTicket)
        {
            IEnumerable<Users> lUsers = repUsers.getListByTenant(idCustomer);
            foreach (var user in lUsers)
            {
                repTickets.AddUserToTicket(user.id, idTicket);
                Tickets ticket = repTickets.get(idTicket); // whenever an user is added to a ticket, an email is sent
                emailMgr.emailNewTicket(ticket, user);
            }
            return RedirectToAction("Details", new { id = idTicket, tab = 3 });
        }

        [SessionExpireFilter]
        public ActionResult addCustomerOrUserToTicket(string assignee , int idTicket)
        {

            char[] delimiterChars = {','};

            string[] words = assignee.Split(delimiterChars);
            if (words.Count() == 1) // selected a customer
            {
                int idCustomer = Int32.Parse(words[0]);
                return RedirectToAction("addCustomerToTicket", new { idCustomer = idCustomer, idTicket = idTicket });
            }
            else // selected an user
            {
                int idUser = Int32.Parse(words[1]);
                return RedirectToAction("addThisUserToTicket", new { idUser = idUser, idTicket = idTicket });
            }
        }

        [SessionExpireFilter]
        public ActionResult removeUserFromTicket(int idUser, int idTicket)
        {
            repTickets.RemoveUserFromTicket(idUser, idTicket);
            return RedirectToAction("Details", new { id = idTicket, tab = 3 });
        }

        [SessionExpireFilter]
        public ActionResult removeUserFromWatchList(string email, int idTicket)
        {
            repTickets.RemoveUserFromTicket(email, idTicket);
            return RedirectToAction("Details", new { id = idTicket, tab = 3 });
        }

        [SessionExpireFilter]
        public ActionResult addGTPToTicket(int idTicket)
        {
            Tickets ticket = repTickets.get(idTicket);
            if (ticket != null)
            {   // add each GTP user to the distribution list
                IEnumerable<Users> lGTPPeople = repUsers.getGTPPeople();
                foreach (var item in lGTPPeople)
                {
                    repTickets.AddUserToTicket(item.email, ticket.id);
                    emailMgr.emailNewTicket(ticket, item);
                }
            }
            return RedirectToAction("Details", new { id = idTicket, tab = 3 });
        }

        //****** From the create ticket page
        [SessionExpireFilter]
        public ActionResult AddUserToTicketWeb(int idTicket) // call to the page
        {
            addUserToTicketVM vm = new addUserToTicketVM(idTicket, (int)Session["IDTENANT"]);
            return View(vm);
        }

        [SessionExpireFilter]
        public ActionResult addThisUserToTicketWeb(int idUser, int idTicket)
        {
            repTickets.AddUserToTicket(idUser, idTicket);
            Users user = repUsers.Get(idUser);
            Tickets ticket = repTickets.get(idTicket);
            emailMgr.emailNewTicket(ticket, user);
            return RedirectToAction("AddUserToTicketWeb", new { idTicket = idTicket });
        }

        [SessionExpireFilter]
        public ActionResult removeUserFromTicketWeb(int idUser, int idTicket)
        {
            repTickets.RemoveUserFromTicket(idUser, idTicket);
            return RedirectToAction("AddUserToTicketWeb", new { id = idTicket });
        }

        [SessionExpireFilter]
        public ActionResult addGTPToTicketWeb(int idTicket)
        {
            Tickets ticket = repTickets.get(idTicket);
            if (ticket != null)
            {   // add each GTP user to the distribution list
                IEnumerable<Users> lGTPPeople = repUsers.getGTPPeople();
                foreach (var item in lGTPPeople)
                {
                    repTickets.AddUserToTicket(item.email, ticket.id);
                    emailMgr.emailNewTicket(ticket, item);
                }
            }
            return RedirectToAction("AddUserToTicketWeb", new { idTicket = idTicket });
        }

        [HttpPost]
        public ActionResult CheckBoxDeactivate(int[] ids, int idTicket)
        {
            if (ids != null)
            {
                foreach (var idUser in ids)
                {
                    repTickets.AddUserToTicket(idUser, idTicket);
                    Users user = repUsers.Get(idUser);
                    Tickets ticket = repTickets.get(idTicket);
                    emailMgr.emailNewTicket(ticket, user);
                }
            }
            return RedirectToAction("Index");
        }

        /*************************************************** API ******************************************/

        public JsonResult addUserToTicketJSON(int idUser, int idTicket)
        {
            if (repTickets.AddUserToTicket(idUser, idTicket) == 1) return Json(true, JsonRequestBehavior.AllowGet);
            else return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RemoveUserFromTicket(int idUser, int idTicket)
        {
            if (repTickets.RemoveUserFromTicket(idUser, idTicket) == 1) return Json(true, JsonRequestBehavior.AllowGet);
            else return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AssignTicket(int idResponsible, int id)
        {
            try
            {
                Tickets ticket = repTickets.get(id); // we only edit four fields, so the rest will be nulled. So we get the rest from db
                ticket.idResponsible = idResponsible;
                repTickets.Edit(ticket);
                //return Json(true, JsonRequestBehavior.AllowGet);
                return RedirectToAction("Details", "Tickets", new { id = id });
            }
            catch
            {
                //return Json(false, JsonRequestBehavior.AllowGet);
                return RedirectToAction("Details", "Tickets", new { id = id });
            }
        }

        public JsonResult DetailsJSON(int id)
        {
            Tickets ticket = repTickets.get(id);
            TicketVM vm = new TicketVM(id, "1");
            return Json(vm, JsonRequestBehavior.AllowGet);
        }


        /*************************************************************************/

        private int addFile(int idTicket, HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    if (file.ContentLength > 20000000)
                    {
                        return 0;
                    }
                    var fileName = "File_" + Convert.ToString(idTicket) + "__" + DateTime.UtcNow.Ticks.ToString() + "-" + System.IO.Path.GetFileName(file.FileName);
                    var path = System.IO.Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    file.SaveAs(path);

              /*      TicketsFiles tFile = new TicketsFiles();
                    tFile.fileName = file.FileName;
                    tFile.idTicket = idTicket;
                    tFile.URI = fileName;
                    repTicketFiles.Create(tFile);*/

                    Photos photo = new Photos(); // save on photos, as always are going to be png, jpg or gif
                    photo.cDate = DateTime.Now;
                    photo.idCreator = 1001;
                    photo.idTicket = idTicket;
                    photo.name = fileName;
                    repoPhotos repPhotos = new repoPhotos();
                    repPhotos.Create(photo);

                }
                return 1;
            }
            catch 
            {
                return 0;
            }
        }
    }
}
