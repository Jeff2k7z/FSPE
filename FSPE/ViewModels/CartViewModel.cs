using FSPE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSPE.ViewModels
{
    public class CartViewModel
    {
        public ClubRegistration registration { get; set; }
        public Club club { get; set; }
        public ChildDisposition childdisposition { get; set; }
    }
}