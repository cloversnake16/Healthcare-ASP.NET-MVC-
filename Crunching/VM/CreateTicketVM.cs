using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Crunching.Models;
using GTPTracker.repos;

namespace GTPTrackers.VM
{
    public class CreateTicketVM
    {
        public Tickets ticket;
       
        public IEnumerable<String> YesNo =
        new List<string>
        {
            "Yes",
            "No"
        };
    }
}