using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GTPTracker.repos;
using Crunching.Models;
using GTPTracker.VM;
using GTPTracker.Functions;

namespace GTPTracker.BL
{
    public class VisitReportsManager
    {
        public IEnumerable<vVisitReports> Get(int idUser)
        {
            using (VRSecurityMgr vrSecurityMgr = new VRSecurityMgr())
                if (vrSecurityMgr.canI("View", "Visit Report", idUser))
                {
                    using (repoVisitReports repVisitReports = new repoVisitReports())
                    using (repoUsers repUsers = new repoUsers())
                    using (repoCustomers repCustomers = new repoCustomers())
                    {
                        IEnumerable<vVisitReports> lVisitReports = repVisitReports.getAll();
                        Users user = repUsers.Get(idUser);
                        if (user != null)
                        {
                            if (user.businessRole == "GTP General Manager")
                                return repVisitReports.getAll();
                            if (user.businessRole == "GTP KAM")
                            {
                                IEnumerable<int> lCustomersOfThisKAM = repCustomers.getvByKAM(idUser).Select(p => p.id);
                                return repVisitReports.getAll().Where(p => lCustomersOfThisKAM.Contains(p.idTenant));
                            }
                            if (user.businessRole == "Customer")
                                return repVisitReports.getAll().Where(p=>p.idTenant == user.idTenant);
                            return Enumerable.Empty<vVisitReports>();
                        }
                        else return Enumerable.Empty<vVisitReports>();
                    }
                }
                else return Enumerable.Empty<vVisitReports>();
        }

        // TODO check if used
        private int addFile(int idTicket, HttpPostedFileBase file, string directory)
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
                    var path = System.IO.Path.Combine(directory, fileName);
                    file.SaveAs(path);
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


        public string Create(VisitReportsVM vm, int idUser, string directory, IEnumerable<HttpPostedFileBase> files)
        {
            using (VRSecurityMgr vrSecurityMgr = new VRSecurityMgr())
            {
                if (vrSecurityMgr.canI("Create", "Visit Report", idUser, vm.ticket.idTenant))
                {
                    using (repoVisitReports repVisitReports = new repoVisitReports())
                    using (repoTickets repTickets = new repoTickets())
                    {
                        try
                        {
                            vm.ticket.cDate = DateTime.UtcNow;
                            vm.ticket.idCreator = idUser;
                            vm.ticket.idType = 4; // visit report type
                            int idTicket = repTickets.Create(vm.ticket);
                            vm.visitReport.idTicket = idTicket;
                            repVisitReports.Create(vm.visitReport);
                            if (files != null)
                                foreach (var file in files)
                                    addFile(idTicket, file, directory);
                            return "";
                        }
                        catch (Exception ex)
                        {
                            return ex.Message;
                        }
                    }
                }
                else return "Not enough permissions to create a visit report";
            }
        }

        public string Edit(VisitReportsVM vm, int idUser)
        {
            using (VRSecurityMgr vrSecurityMgr = new VRSecurityMgr())
            {
                if (vrSecurityMgr.canI("Edit", "Visit Report", idUser, vm.visitReport.id))
                {
                    using (repoVisitReports repVisitReports = new repoVisitReports())
                    using (repoTickets repTickets = new repoTickets())
                    {
                        try
                        {
                            repTickets.Edit(vm.ticket);
                            repVisitReports.Edit(vm.visitReport);

                            return "";
                        }
                        catch (Exception ex)
                        {
                            return ex.Message;
                        }
                    }
                }
                else return "Not enough permissions to edit a visit report";
            }
        }

        public string changeStatus(int idVisitReport, int newStatus, int idUser)
        {
            using (VRSecurityMgr vrSecurityMgr = new VRSecurityMgr())
            {
                if (vrSecurityMgr.canI("Edit", "Visit Report", idUser, idVisitReport))
                {
                    using (repoVisitReports repVisitReports = new repoVisitReports())
                    using (repoTickets repTickets = new repoTickets())
                    {
                        try
                        {
                            VisitReports visitReport = repVisitReports.get(idVisitReport);
                            if (visitReport != null)
                            {
                                Tickets ticket = repTickets.get(visitReport.idTicket);
                                if (ticket != null)
                                {
                                    ticket.idStatus = newStatus;
                                    repTickets.Edit(ticket);
                                    return "";
                                }
                                else return "Incorrect idTicket";
                            }
                            else return "Incorrect idVisitReport";
                        }
                        catch (Exception ex)
                        {
                            return ex.Message;
                        }
                    }
                }
                else return "Not enough permissions to edit a visit report";
            }
        }
    }
}