using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FSPE.Models;
using FSPE.ViewModels;

namespace FSPE.Controllers
{
    public class ClubController : Controller
    {
        private ApplicationDbContext _context;

        public ClubController()
        {
            _context = new ApplicationDbContext();
        }

        // GET: Club
        public ActionResult Index()
        {
            var clubs = _context.Clubs.ToList();


            return View(clubs);
        }

        public ActionResult NewRegistration(int id)
        {
            var club = _context.Clubs.FirstOrDefault(c => c.Id == id);

            if(club == null)
                return HttpNotFound();

            var registration = new ClubRegistration
            {
                ClubId = club.Id,
                Club = club
            };

            var viewModel = new RegisterClubViewModel
            {
                ClubRegistration = registration,
                ChildDispositions = _context.ChildDispositions.ToList()
            };
            
            return View(viewModel);
        }
    }
}