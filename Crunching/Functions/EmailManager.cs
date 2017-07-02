using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using GTPTracker.repos;
using Crunching.Models;

namespace GTPTracker.Functions
{
    public class EmailManager : IDisposable
    {
        repoTickets repTickets = new repoTickets();
        repoCustomers repCustomers = new repoCustomers();
        repoUsers repUsers = new repoUsers();
        string baseURL = "http://www.gtp-toolbox.com/";

        public void SendEmail(string subject, string body, string EmailDestinatario)
        {
            MailMessage myMessage = new MailMessage();
            myMessage.From = new MailAddress("toolbox@gtp-schaefer.de", "GTP ToolBox"); // por defecto se pone mi direccion
            myMessage.To.Add(EmailDestinatario);
            myMessage.Subject = subject;
            myMessage.IsBodyHtml = true;
            myMessage.Body = body;

            using (SmtpClient mySmtpClient = new SmtpClient())
            {
                mySmtpClient.ServicePoint.MaxIdleTime = 1;
                try
                {
                    mySmtpClient.Send(myMessage);
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); myMessage.Dispose(); }
                myMessage.Dispose();
            }
        }

        public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void emailResetPassword(Users person, string login, string password)
        {
            string subject = "Password reset GTP ToolBox";
            string body = "Dear "+ person.treatment+ " " + person.lastName + ",<br/><br/>";
            body = body + "your password has been reseted: :<br/></br>";
            body = body + "Login = " + login + "<br/>";
            body = body + "Password = " + password + "<br/>";
            body = body + "Please change your password with your next login: " + baseURL + " <br/></br><br/>";
            body = body + "<br/></br>Best,<br/>GTP-Team";
            enviar(subject, body, person.email);
        }

        public void emailDailyActivationReport(int cont, string customerEmail)
        {
            if (cont > 0) // 
            {
                string subject = "Daily Digest: Unassigned Users @GTP ToolBox";
                string body = "Hi, <br/><br/>";
                body = body + "We have " + cont.ToString() + " (new) users for the GTP ToolBox, which await to get a business role assigned. <br/><br/>";
                body = body + "Please login to admin and assign a business role:<br /><br />";
                body = body + baseURL + "Users/usersManager " + "<br/>";
                body = body + "<br/>Best,<br/><br/>GTP Admin";
                body = body + "<br/><br/>P.S.: Some information on assigning business roles can be found in our WIKI https://goo.gl/U4jZba";
                enviar(subject, body, customerEmail);
            }
        }

   /*     public void emailNewUser(Users person, string login, string password)
        {
            string subject = "Welcome to GTP ToolBox";
            string body = "Dear " + person.name + ",<br/><br/>";
            body = body + "An user for GTP ToolBox web application has been created. Please find your credentials below:<br/></br>";
            body = body + "Login = " + login + "<br/>";
            body = body + "Password = " + password + "<br/>";
            body = body + "Please change your password with your first login: " + baseURL + " <br/></br><br/>";
            body = body + "<br/></br>Best,<br/>GTP-Team";
            enviar(subject, body, person.email);
        }*/

        public void emailResolution(Tickets ticket, Users person)
        {
            Customers customer = repCustomers.Get(ticket.idTenant);
            string reason = "";
            if (ticket.idType == 1) reason = "Technical question";
            if (ticket.idType == 2) reason = "Claim";
            if (ticket.idType == 3) reason = "Request sample";
            string subject = "Ticket closed: " + reason + ", ID " + ticket.id + " (" + customer.name + ")";
            string body = "Dear " + person.treatment+ " " + person.lastName + ",<br/><br/>";
            body = body + "Your ticket " + ticket.id + " (" + reason + ", " + customer.name + ") has been resolved. <br/><br/>";
            body = body + "For more details go to "+ baseURL + "Tickets/Details/" + ticket.id + "#Answer<br/></br>";
            body = body + "<br/></br>Best,<br/>Your GTP-Team";
            enviar(subject, body, person.email);
        }

        public void emailNewTicket(Tickets ticket, Users person)
        {
            Customers customer = repCustomers.Get(ticket.idTenant);

            string reason = "";
            if (ticket.idType == 1) reason = "Technical question";
            if (ticket.idType == 2) reason = "Claim";
            if (ticket.idType == 3) reason = "Request sample";
            string subject = "New ticket: " + reason + ", ID " + ticket.id + " (" + customer.name + ")";
            string body = "Dear " + person.treatment + " " + person.lastName + ",<br/><br/>";
            body = body + "A new ticket has been created. <br/><br/>";
            body = body + customer.name + "<br/>";
            body = body + ticket.reason + "<br/>";
            body = body + ticket.description + "<br/>";
            body = body + baseURL + "Tickets/Details/" + ticket.id + "<br/></br>";
            body = body + "<br/></br>Best,<br/>GTP-Team";
            enviar(subject, body, person.email);
        }

        public void emailNewTicketForSpecialGTPPeople(Tickets ticket, Users person) // send email for these 3 persons and disable the others.
        {
            if (ticket.idTenant == 0) ticket.idTenant = 1;
            Customers customer = repCustomers.Get(ticket.idTenant);

            string reason = "";
            if (ticket.idType == 1) reason = "Technical question";
            if (ticket.idType == 2) reason = "Claim";
            if (ticket.idType == 3) reason = "Request sample";
            string subject = "New ticket: " + reason + ", ID " + ticket.id + " (" + customer.name + ")";
            string body = "Dear " + person.treatment +" " + person.lastName + ",<br/><br/>";
            body = body + "A new ticket has been created. <br/><br/>";
            body = body + customer.name + "<br/>";
            body = body + ticket.reason + "<br/>";
            body = body + ticket.description + "<br/>";
            body = body + baseURL + "Tickets/Details/" + ticket.id + "<br/></br>";
            body = body + "<br/></br>Best,<br/>GTP-Team";
            enviar(subject, body, person.email);
        }

        public void emailNewTask(SubTasks task)
        {
            Tickets ticket = repTickets.get(task.idTicket);
            Customers customer = repCustomers.Get(ticket.idTenant);
            Users creator = repUsers.Get(task.idCreator);
            Users assignee = repUsers.Get(task.idUser);
            String creatorName = "";

            if (creator == null) creatorName = "GTP ToolBox";
            else creatorName = creator.firstName + " " + creator.lastName;
            string reason = "";
            if (ticket.idType == 1) reason = "Technical question";
            if (ticket.idType == 2) reason = "Claim";
            if (ticket.idType == 3) reason = "Request sample";
            string subject = "New task: Ticket ID " + ticket.id + ", " + customer.name + ", assigned by " + creatorName;
            string body = "Dear " +assignee.treatment+ " " +assignee.lastName + ",<br/><br/>";
            body = body + "A new task has been assigned to you by " + creatorName + "<br/><br>";
            body = body + customer.name + "<br/>";
            body = body + reason + "<br/>";
            body = body + task.description + "<br/>";
            body = body + baseURL + "SubTasks/Index " + "<br/></br></br>";
            body = body + "<br/></br>Best,<br/>GTP-Team";
            enviar(subject, body, assignee.email);
        }

        public void emailMyOpenTasks(IEnumerable<vSubTasks> lSubtaks, string customerEmail)
        {
            if (lSubtaks.Count() > 0) // send email only if non completed tasks available
            {
                string subject = "List of your active tasks";
                string body = "Dear " + customerEmail + ",<br/><br/>";
                body = body + "These are the open tasks for you <br/><br>";
                foreach (var item in lSubtaks)
                {
                    body = body + item.description + "<br/></br>";
                }
                body = body + baseURL + "SubTasks/Index " + "<br/></br></br>";
                body = body + "<br/></br>Best,<br/>GTP-Team";
                enviar(subject, body, customerEmail);
            }
        }

        public void emailTaskDeadline(vSubTasks task, string customerEmail, string customerName)
        {
            string subject = "Task reaches its deadline";
            string body = "Dear " + customerEmail + ",<br/><br/>";
            body = body + "This task reaches it deadline<br/><br>";
            body = body + task.description + "<br/></br>";
            body = body + baseURL + "SubTasks/Index " + "<br/></br></br>";
            body = body + "<br/></br>Best,<br/>GTP-Team";
            enviar(subject, body, customerEmail);
        }

        public void emailActiveTaskByCustomer(IEnumerable<vSubTasks> lSubtaks, string customerEmail, string customerName)
        {
            if (lSubtaks.Count() > 0) // send email only if non completed tasks available
            {
                string subject = "List of active task of your company";
                string body = "Dear " + customerEmail + ",<br/><br/>";
                body = body + "These are the open tasks of your company "+customerName+"<br/><br>";
                foreach (var item in lSubtaks)
                {
                    if (item.deadline != null)
                        if (((DateTime) item.deadline).Year >= DateTime.Today.AddDays(-7).Year && ((DateTime) item.deadline).Month >= DateTime.Today.AddDays(-7).Month && ((DateTime) item.deadline).Day == DateTime.Today.AddDays(-7).Day)
                            body = body +  item.description + " Beware! Close to the deadline.<br/></br>";
                        else body = body +  item.description + "<br/></br>";
                    else body = body + item.description + "<br/></br>"; // no deadline, so standard task
                }
                body = body + baseURL + "SubTasks/Index " + "<br/></br></br>";
                body = body + "<br/></br>Best,<br/>GTP-Team";
                enviar(subject, body, customerEmail);
            }
        }

        public void sendActivationLinkForgotPassword(Users person, string token)
        {
            string subject = "You password request at GTP ToolBox";
            string body = "Hi<br/><br/>";
            body = body + "Please follow the link to reset the password for your GTP ToolBox account:<br/><br>";
            body = body + baseURL + "users/setPassword?token=" + token + "<br/><br/>";
            body = body + "If you don’t have requested a new password you can ignore this e-mail.<br/></br/>";
            body = body + "If you have questions, please contact GTP or simply reply to this e-mail.<br/>";
            body = body + "<br/>Best,<br/>Your GTP-Team";
            enviar(subject, body, person.email);
        }

        public void sendActivationLinkNewUser(Users person, string token)
        {
            string subject = "Welcome @ GTP ToolBox";
            string body = "Dear "+ person.treatment + " " + person.lastName + ",<br/><br/>";
            body = body + "Welcome to GTP Toolbox. Please confirm your account:<br/></br>";
            body = body + baseURL + "users/setPassword?token=" + token + "<br/><br/><br/>";
            body = body + "If you have questions, please contact GTP or simply reply to this e-mail. <br/></br><br/>";
            body = body + "<br/></br>Best,<br/>Your GTP-Team";
            enviar(subject, body, person.email);
        }

        public void sendAssignCustomerBusinessRole(Users person, string company)
        {
            string subject = "Your account @ GTP ToolBox";
            string body = "Dear " + person.treatment + " " + person.lastName + ",<br/><br/>";
            body = body + "Congratulations, we have upgraded your account.<br/><br/>";
            body = body + "Your are now an official user of " +company+".<br/><br/>";
            body = body + "In “my products” you find all your GTP products. Check it out:<br/><br/>";
            body = body + baseURL + "<br/><br/><br/>";
            body = body + "If you have questions, please contact GTP or simply reply to this e-mail. <br/><br/><br/>";
            body = body + "<br/><br/>Best,<br/>Your GTP-Team";
            enviar(subject, body, person.email);
        }

        public void enviar(string subject, string body, string emailAddress)
        {
           if (IsValidEmail(emailAddress)) SendEmail(subject, body, emailAddress);
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
                repCustomers.Dispose();
                repTickets.Dispose();
                repUsers.Dispose();
            }
            // free native resources if there are any.
        }
    }
}