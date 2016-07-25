using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSPE.Models
{
    public class ClubRegistration
    {
        public int Id { get; set; }
        public string ParentName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string ChildName { get; set; }
        public string Grade { get; set; }
        public string Teacher { get; set; }

        public Club Club { get; set; }
        public int ClubId { get; set; }

        public ChildDisposition ChildDisposition { get; set; }
        public int ChildDispositionId { get; set; }
    }
}