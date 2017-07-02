using Crunching.Models;
using System.Collections.Generic;
using System.Linq;
using GTPTracker.Functions;
using System;

namespace GTPTracker.repos
{
    public class repoSettings : IDisposable
    {
        public void UpdateSetting(string key, string value)
        {
            using (var db = new GTPTrackerEntities())
            {
                Settings row = db.Settings.FirstOrDefault(p => p.Key.EndsWith(key));

                if (row != null)
                {
                    row.LastUpdate = System.DateTime.Now;
                    row.Value = value;
                    Edit(row);
                }
                else
                {
                    Create(new Settings
                    {
                        Key = key,
                        Value = value,
                        LastUpdate = System.DateTime.Now
                    });
                }
            }
        }

        public bool alreadyExists(string settingName)
        {
            using (var db = new GTPTrackerEntities())
            {
                Settings row = db.Settings.FirstOrDefault(p => p.Key.EndsWith(settingName));
                return row != null;
            }
        }

        public Settings get(int id)
        {
            using (var db = new GTPTrackerEntities())
            {
                return db.Settings.FirstOrDefault(p => p.Id == id);
            }
        }

        public string Get(string setting)
        {
            using (var db = new GTPTrackerEntities())
            {
                var row = db.Settings.FirstOrDefault(p => p.Key == setting);
                if (row != null)
                    return row.Value;
                else return null;
            }
        }

        public int Create(Settings item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Settings.AddObject(item);
                db.SaveChanges();
            }
            return item.Id;
        }

        public void Edit(Settings item)
        {
            using (var db = new GTPTrackerEntities())
            {
                db.Settings.FirstOrDefault(p => p.Id == item.Id);
                db.Settings.ApplyCurrentValues(item);
                db.SaveChanges();
            }
        }

        public void Delete(Settings item)
        {
            using (var db = new GTPTrackerEntities())
            {
                Settings des = db.Settings.FirstOrDefault(e => e.Id == item.Id);
                if (des != null)
                {
                    db.Settings.DeleteObject(des);
                    db.SaveChanges();
                }
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
                // free managed resources
            }
            // free native resources if there are any.
        }
    }
}
