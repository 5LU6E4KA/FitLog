using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLog.DatabaseContext
{
    static class DbContext
    {
        private static readonly Entities.FitLogEntities _context = new Entities.FitLogEntities();

        public static Entities.FitLogEntities Context => _context;
    }
}
