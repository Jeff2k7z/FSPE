using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Web;

namespace FSPE.Models
{
    public class Club
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Days { get; set; }
        public string Time { get; set; }
        public string Grades { get; set; }
        public double Price { get; set; }
    }
}