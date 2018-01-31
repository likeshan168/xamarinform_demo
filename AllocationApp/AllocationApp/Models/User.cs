using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Realms;

namespace AllocationApp.Models
{
    public class User : RealmObject
    {
        public string UserName { get; set; }
        public String Password { get; set; }
    }
}
