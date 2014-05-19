using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MvcService.Data
{
    public class User : BaseEntity
    {
        public int id { get; set; }
        public string name { get; set; }

        public override string ToString()
        {
            //return id.ToString() + " - " + name;
            return JsonConvert.SerializeObject(this);
        }
    }
}