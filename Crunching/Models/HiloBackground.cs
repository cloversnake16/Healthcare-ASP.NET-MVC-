using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using GTPTracker.repos;
using GTPTracker.Functions;
using GTPTracker.BL;
using GTPTracker.Helpers;

namespace Crunching.Models
{
    public class HiloBackground
    { /// Valores que indican a qué hora del día de mañana se ejecutará el servicio
        private int horaMañana, minutosMañana, segundosMañana;
        /// Delegado que indica la acción que ha de ejecutarse cuando se despierte el hilo
        private Action accionAEjecutar;
        repoSubtasks repTasks = new repoSubtasks();
        repoCustomers repCustomers = new repoCustomers();
        repoUsers repUsers = new repoUsers();
        repoMilestones repMilestones = new repoMilestones();
        
        GTPTracker.Functions.EmailManager emailMgr = new GTPTracker.Functions.EmailManager();
        ItemsManager itemMgr = new ItemsManager();

        /// Calcula los milisegundos que faltan hasta una hora del próximo día
        /// <param name="hora">Hora del próximo día</param>
        /// <param name="minutos">Minutos del próximo día</param>
        /// <param name="segundos">Segundos del próximo día</param>
        private long CalcularMillisHastaHoraMañana(int hora, int minutos, int segundos)
        {
            /*   DateTime mañana = DateTime.UtcNow.AddDays(1).Date;
               DateTime momentoExacto = mañana.AddHours(hora).AddMinutes(minutos).AddSeconds(segundos);
               return (momentoExacto - DateTime.UtcNow).Ticks / TimeSpan.TicksPerMillisecond;*/
            // return 3600000; // cada hora
           // return 3600000;// *24;
            return 600000; // cada 10 minutos
        }
        /// Método que posee el control del hilo.
        /// Es infinito porque se trata de un hilo
        public void LoopInfinito()
        {
            while (true)
            {
               using (UsersManager um = new UsersManager())
                {
                    um.getNotAssignedUsers();
                    GTPTracker.Helpers.LogMgr logMgr = new LogMgr();
                    logMgr.BackgroundLog("1-dev emailDailyActivationReport");
                    Console.WriteLine("1 emailDaily " + DateTime.UtcNow);
                }
                foreach (var mil in repMilestones.getList())
                {    // update the milestones
                    if (mil.activationDate.Year == DateTime.Today.Year && mil.activationDate.Month == DateTime.Today.Month && mil.activationDate.Day == DateTime.Today.Day)
                        itemMgr.updateMilestoneItem(mil.id);
                }
                foreach (var task in repTasks.getAll().Where(p => p.solved == false))
                {// if the deadline of some task is reached update the items and send an email
                    if (task.deadline != null)
                    {
                        if (((DateTime)task.deadline).Year == DateTime.Today.Year && ((DateTime)task.deadline).Month == DateTime.Today.Month && ((DateTime)task.deadline).Day == DateTime.Today.Day)
                        {
                            itemMgr.updateMilestoneItem(task.id);
                           // emailMgr.emailTaskDeadline(task, task.email, task.name);
                        }
                    }
                }
              /*  foreach (vCustomers customer in repCustomers.getList())
                {
                    string kamEmail = "razonasistemas@gmail.com";
                    if (customer.idKeyAccountManager != null)
                    {
                        Models.Users kam = repUsers.Get((int)customer.idKeyAccountManager);
                        kamEmail = kam.email;
                    }
                    IEnumerable<vSubTasks> lSubTasks = repTasks.getAllTasksFromCustomer(customer.id).Where(p => p.solved == false);
                   // emailMgr.emailActiveTaskByCustomer(lSubTasks, kamEmail, customer.name);
                }*/
                foreach (var user in repUsers.getList())
                {
                    IEnumerable<vSubTasks> lSubTasks = repTasks.getMyActiveList(user.id);
                    //emailMgr.emailMyOpenTasks(lSubTasks, user.email);
                }              
                // Nos dormimos...
                Thread.Sleep((int)
                    this.CalcularMillisHastaHoraMañana(this.horaMañana, this.minutosMañana, this.segundosMañana)
                    );
                // ... y ejecutamos la acción
                if (this.accionAEjecutar != null)
                    this.accionAEjecutar();
                else
                {
                    // send every day at 1 am
                    if (true)//DateTime.UtcNow.Hour == 19 )
                    {
                        using (UsersManager um = new UsersManager())
                        {
                            um.getNotAssignedUsers();
                            GTPTracker.Helpers.LogMgr logMgr = new LogMgr();
                            logMgr.BackgroundLog("2dev emailDailyActivationReport");
                            Console.WriteLine("2 emailDaily " + DateTime.UtcNow);
                        }
                        
                        foreach (var mil in repMilestones.getList())
                        {// update the milestones
                            if (mil.activationDate.Year == DateTime.Today.Year && mil.activationDate.Month == DateTime.Today.Month && mil.activationDate.Day == DateTime.Today.Day)
                                itemMgr.updateMilestoneItem(mil.id);
                        }
                        foreach (var task in repTasks.getAll().Where(p => p.solved == false))
                        { // if the deadline of some task is reached update the items and send an email
                            if (task.deadline != null)
                            {
                                if (((DateTime)task.deadline).Year == DateTime.Today.Year && ((DateTime)task.deadline).Month == DateTime.Today.Month && ((DateTime)task.deadline).Day == DateTime.Today.Day)
                                {
                                    itemMgr.updateMilestoneItem(task.id);
                                    //emailMgr.emailTaskDeadline(task, task.email, task.name);
                                }
                            }
                       /*     foreach (vCustomers customer in repCustomers.getList())
                            {
                                string kamEmail = "razonasistemas@gmail.com";
                                if (customer.idKeyAccountManager != null)
                                {
                                    Models.Users kam = repUsers.Get((int)customer.idKeyAccountManager);
                                    kamEmail = kam.email;
                                }
                                IEnumerable<vSubTasks> lSubTasks = repTasks.getAllTasksFromCustomer(customer.id).Where(p => p.solved == false);
                              //  emailMgr.emailActiveTaskByCustomer(lSubTasks, kamEmail, customer.name);
                            }*/
                            foreach (var user in repUsers.getList())
                            {
                                IEnumerable<vSubTasks> lSubTasks = repTasks.getMyActiveList(user.id);
                              //  emailMgr.emailMyOpenTasks(lSubTasks, user.email);
                            }
                        }
                    }
                }
            }
        }

        /// El hilo lo añadimos al objeto para que no lo libere el recolector
        private Thread hilo;

        public HiloBackground(int horaMañana, int minutosMañana, int segundosMañana, Action accionAEjecutar)
        {
            // Guardamos los valores
            this.horaMañana = horaMañana;
            this.minutosMañana = minutosMañana;
            this.segundosMañana = segundosMañana;
            this.accionAEjecutar = accionAEjecutar;
            // Creamos el hilo y lo ejecutamos
            this.hilo = new Thread(new ThreadStart(this.LoopInfinito));
            this.hilo.Name = "Calculo Productividad y alertas";
            this.hilo.IsBackground = true;
            this.hilo.Start();
        }

    }

}
