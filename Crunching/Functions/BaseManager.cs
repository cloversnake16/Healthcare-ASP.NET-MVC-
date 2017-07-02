using System;

namespace GTPTracker.Functions
{
    public class BaseManager : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        ~BaseManager()
        {
            Dispose(false);
        }
    }
}