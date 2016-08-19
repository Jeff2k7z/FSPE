using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSPE.Models
{
    public class RegistrationLock
    {
        public int Id { get; set; }
        public int ClubID { get; set; }
        public DateTime LockExpiration { get; set; }
        public string LockKey { get; set; }
    }
}