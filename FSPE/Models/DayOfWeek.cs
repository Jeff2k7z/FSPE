using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FSPE.Models
{
    [Table("DaysOfWeek")]
    public class DayOfWeek
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Club> Clubs { get; set; }
    }
}