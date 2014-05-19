using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcService.Data
{
    public class BaseEntity
    {
        private Session _session = null;

        public Session Session
        {
            get { return _session; }
            set { _session = value; }
        }
    }
}
