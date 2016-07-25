using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSPE.Models
{
    public class Grade
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Club> Clubs { get; set; }
    }
}