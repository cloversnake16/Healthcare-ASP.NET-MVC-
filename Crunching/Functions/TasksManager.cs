using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GTPTracker.repos;
using Crunching.Models;

namespace GTPTracker.Functions
{
    public class TasksManager : IDisposable
    {
        repoSubtasks repTasks = new repoSubtasks();
        repoUsers repUsers = new repoUsers();
        repoTickets repTickets = new repoTickets();

        EmailManager emailMgr = new EmailManager();

        public void recalculateProgress(int idTicket)
        {
            IEnumerable<vSubTasks> lTasks = repTasks.getList(idTicket);
            int completedCount = lTasks.Where(p => p.solved == true).Count();
            int total = lTasks.Count();
            float progress = 100 * ((float)completedCount / (float)total);
            Tickets ticket = repTickets.get(idTicket);
            ticket.progress = progress;
            repTickets.Edit(ticket);
        }

        public void CreateDefaultTaskForKAMInSample(vTickets ticket, int idUser)
        {
            SubTasks task = new SubTasks();

            task.description = "New sample request";
            task.idUser = (int) ticket.idKeyAccountManager;
            task.idCreator = idUser;
            task.idTicket = ticket.id;
            task.cDate = DateTime.Today;
            task.solved = false;
            task.internalTask = false;
            repTasks.Create(task);
            emailMgr.emailNewTask(task);
        }

        public void AutoTaskSamples(vTickets ticket, int idUser)
        {
            AutoTask(ticket, idUser, "Fill out GTP internal document", true, "sample_request_GTP_form.docx");
            AutoTask(ticket, idUser, "Submit delivery date to costumer", false);
            AutoTask(ticket, idUser, "Submit status of delivery", false);
            AutoTask(ticket, idUser, "Remind costumer of test", true);
            AutoTask(ticket, idUser, "Ask costumer for feedback", false);
            AutoTask(ticket, idUser, "Result of sample request", true);
        }

        public void AutoTaskReclamation(vTickets ticket, int idUser)
        {
            AutoTask(ticket, idUser, "Fill out GTP internal document", true, "Customer_Claim_Form.docx");
            AutoTask(ticket, idUser, "Give costumer feedback", false);
            AutoTask(ticket, idUser, "Solve problem", true);
        }

        public void AutoTaskTechnical(vTickets ticket, int idUser)
        {
            AutoTask(ticket, idUser, "Give costumer feedback", false);
        }

        public void AutoTaskVist(vTickets ticket, int idUser)
        {
            AutoTask(ticket, idUser, "Create tasks out of the visit report", true);
            AutoTask(ticket, idUser, "Assign tasks to costumer if necessary", true);
        }

        public void AutoTaskProject(vTickets ticket, int idUser)
        {
            AutoTask(ticket, idUser, "Create goals", false);
        }

        private void AutoTask(vTickets ticket, int idUser, string description, bool internalTask, string filename=null)
        {
            SubTasks task = new SubTasks();

            task.description = ticket.title + " - " + description;
            task.idUser = (int)ticket.idKeyAccountManager;
            task.idCreator = idUser;
            task.idTicket = ticket.id;
            task.cDate = DateTime.Today;
            task.solved = false;
            task.internalTask = internalTask;
            task.deadline = DateTime.Today.AddDays(21);
            repTasks.Create(task);
            emailMgr.emailNewTask(task);
            if (filename != null)
            {
                TasksFiles tFile = new TasksFiles();
                tFile.name = filename;
                tFile.idTask = task.id;
                tFile.idCreator = 1001;
                tFile.cDate = DateTime.UtcNow;
                using (repoTaskFiles repFiles = new repoTaskFiles())
                    repFiles.Create(tFile);
            }
        }

        private void addSampleFileToTask(int idTask)
        { }

        private void addClaimFileToTask(int idTaks)
        { }

        public void CreateDefaultTaskForCustomerInSample(vTickets ticket, int idUser)
        {
            SubTasks task = new SubTasks();

            task.description = "Sample request. Please test and provide feedback";
            task.idUser = idUser;
            task.idCreator = idUser;
            task.idTicket = ticket.id;
            task.cDate = DateTime.Today;
            task.solved = false;
            task.internalTask = false;
            repTasks.Create(task);
            emailMgr.emailNewTask(task);
        }

        public void createDefaultTask(Tickets ticket, int idUser)
        {
            SubTasks task = new SubTasks();
            Users recipient = null;

            if (ticket.idType == 1) // technical
            {
                task.description = "Technische Kundenfrage: Bitte Kunde wieder glücklich machen.:)";
                recipient = repUsers.getUserByEmail("Joerg.Schaefer@gtp-schaefer.de");
            }
            if (ticket.idType == 2) // Claim
            {
                task.description = "Kundenreklamation: Bitte Kunde wieder glücklich machen.:)";
                recipient = repUsers.getUserByEmail("Alexander.siller@gtp-schaefer.de");
            }
            if (ticket.idType == 3) // Sample Request
            {
                task.description = "Bitte Probenbegleitzettel ausfüllen.";
                recipient = repUsers.getUserByEmail("Sebastian.baehren@gtp-schaefer.de");
            }

            if (recipient != null)
            {
                task.idUser = recipient.id;
                task.idCreator = idUser;
                task.idTicket = ticket.id;
                task.cDate = DateTime.Today;
                task.solved = false;
                task.internalTask = true;
                repTasks.Create(task);
                emailMgr.emailNewTask(task);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                repTasks.Dispose();
                repTickets.Dispose();
                repUsers.Dispose();
                emailMgr.Dispose();               
            }
            // free native resources if there are any.
        }
    }
}