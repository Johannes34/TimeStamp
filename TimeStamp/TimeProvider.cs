using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeStamp
{
    public class TimeProvider
    {
        public virtual DateTime Today => DateTime.Today;

        public virtual DateTime Now => DateTime.Now;

    }
}
