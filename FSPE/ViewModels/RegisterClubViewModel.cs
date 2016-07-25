using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FSPE.Models;

namespace FSPE.ViewModels
{
    public class RegisterClubViewModel
    {
        public ClubRegistration ClubRegistration { get; set; }
        public IEnumerable<ChildDisposition> ChildDispositions { get; set; }

    }
}