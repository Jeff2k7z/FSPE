using FSPE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSPE.ViewModels
{
    public class CartViewModel
    {
        public ClubRegistration ClubRegistration { get; set; }
        public IEnumerable<ChildDisposition> ChildDispositions { get; set; }
    }
}