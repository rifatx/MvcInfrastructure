using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcService.Data
{
    public class TestData : BaseEntity
    {
        public int id { get; set; }
        public string name { get; set; }

        public override string ToString()
        {
            return id.ToString() + " - " + name + ", " + base.Session.Id;
        }
    }
}