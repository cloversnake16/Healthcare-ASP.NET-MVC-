using System;
using Crunching.Models;

namespace GTPTracker.repos
{
    public abstract class repoBase : IDisposable
    {
        protected GTPTrackerEntities db = new GTPTrackerEntities();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (db != null)
                db.Dispose();
        }

        ~repoBase()
        {
            Dispose(false);
        }
    }
}