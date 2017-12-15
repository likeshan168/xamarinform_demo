using Realms;

namespace AllocationApp.Models
{
    public class User : RealmObject
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
