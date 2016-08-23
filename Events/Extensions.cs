using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Events
{
    public static class Extensions
    {
        public static IEnumerable<T> FindAll<T>(this DbSet<T> db, IEnumerable keys) where T : class
        {
            foreach (var key in keys)
                yield return db.Find(key);
        }
    }
}