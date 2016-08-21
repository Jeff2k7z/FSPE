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
            var locks = _context.RegistrationLocks
                        .Where(rl => rl.LockExpiration > DateTime.Now)
                        .GroupBy(rl => rl.ClubID)
                        .Select(n => new { ClubId = n.Key, LockCount = n.Count() });

            var registrations = _context.ClubRegistrations
                    .Where(cr => cr.IsPaid == true)
                    .GroupBy(cr => cr.ClubId)
                    .Select(n => new { ClubId = n.Key, RegistrationCount = n.Count() });

            var clubs = (from c in _context.Clubs
                         from regLock in locks.Where(rl => rl.ClubId == c.Id)
                                                .DefaultIfEmpty()
                         from reg in registrations.Where(r => r.ClubId == c.Id)
                                                .DefaultIfEmpty()
                         select
                         new ClubSignupViewModel
                         {
                             Id = c.Id,
                             Name = c.Name,
                             Description = c.Description,
                             Days = c.Days,
                             Grades = c.Grades,
                             Time = c.Time,
                             Price = c.Price,
                             Capacity = c.Capacity,
                             Remaining = c.Capacity - (regLock.LockCount == null ? 0 : regLock.LockCount) - (reg.RegistrationCount == null ? 0 : reg.RegistrationCount)
                         });


            return View(clubs);
        }



        public ActionResult NewRegistration(string clubs)
        {
            // Make sure they selected at least one club
            if (string.IsNullOrWhiteSpace(clubs))
            {
                return RedirectToAction("Index", "Club");
            }


            var clubIndexes = clubs.Split(',');
            if (!AreClubsAvailable(clubIndexes.Select(c => int.Parse(c)).ToList()))
            {
                TempData["Flash"] = "One or more of your club selections are not available.  Please choose again.";
                return RedirectToAction("Index", "Club");
            }

            // Read or create registration key
            string registrationKey = "";
            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("FOSPERegistrationKey"))
            {
                registrationKey = this.ControllerContext.HttpContext.Request.Cookies["FOSPERegistrationKey"].Value;
            } else {
                HttpCookie cookie = new HttpCookie("FOSPERegistrationKey");
                registrationKey = Guid.NewGuid().ToString();
                cookie.Value = registrationKey;
                this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
            }


            // Create Registration Locks
            for (var i = 0; i < clubIndexes.Length; i++)
            {
                var reglock = new RegistrationLock
                {
                    ClubID = int.Parse(clubIndexes[i]),
                    LockExpiration = DateTime.Now.AddMinutes(15),
                    LockKey = registrationKey
                };

                _context.RegistrationLocks.Add(reglock);
            }

            _context.SaveChanges();

            var registration = (ClubRegistration)Session["ChildInfo"];
            if (registration == null)
            {
                registration = new ClubRegistration();
            }


            var viewModel = new RegisterClubViewModel
            {
                ClubRegistration = registration,
                ChildDispositions = _context.ChildDispositions.ToList(),
                Clubs = clubs 
            };
            
            return View(viewModel);
        }

        public ActionResult Save(RegisterClubViewModel model)
        {
            // Read or create registration key
            string registrationKey = "";
            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("FOSPERegistrationKey"))
            {
                registrationKey = this.ControllerContext.HttpContext.Request.Cookies["FOSPERegistrationKey"].Value;
            }
            else
            {
                HttpCookie cookie = new HttpCookie("FOSPERegistrationKey");
                registrationKey = Guid.NewGuid().ToString();
                cookie.Value = registrationKey;
                this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
            }


            // Create Registrations
            var clubIndexes = model.Clubs.Split(',');
            for (var i = 0; i < clubIndexes.Length; i++)
            {
                var reg = new ClubRegistration
                {
                    ClubId = int.Parse(clubIndexes[i]),
                    RegistrationKey = registrationKey,
                    EmailAddress = model.ClubRegistration.EmailAddress,
                    ChildName = model.ClubRegistration.ChildName,
                    Grade = model.ClubRegistration.Grade,
                    ParentName = model.ClubRegistration.ParentName,
                    PhoneNumber = model.ClubRegistration.PhoneNumber,
                    RegistrationDate = DateTime.Now,
                    Teacher = model.ClubRegistration.Teacher,
                    IsPaid = false,
                    ChildDispositionId = model.ClubRegistration.ChildDispositionId
                    
                    
                };

                _context.ClubRegistrations.Add(reg);
            }

            _context.SaveChanges();

            model.ClubRegistration.ChildName = "";
            model.ClubRegistration.Grade = "";
            model.ClubRegistration.Teacher = "";
            Session["ChildInfo"] = model.ClubRegistration;

            return RedirectToAction("Cart", "Checkout");
        }

        public ActionResult Checkout()
        {

            return View();
        }

        private bool AreClubsAvailable(List<int> clubs)
        {
            var locks = _context.RegistrationLocks
                        .Where(rl => rl.LockExpiration > DateTime.Now)
                        .GroupBy(rl => rl.ClubID)
                        .Select(n => new { ClubId = n.Key, LockCount = n.Count() });

            var registrations = _context.ClubRegistrations
                    .Where(cr => cr.IsPaid == true)
                    .GroupBy(cr => cr.ClubId)
                    .Select(n => new { ClubId = n.Key, RegistrationCount = n.Count() });

            var availableClubs = (from c in _context.Clubs
                         from regLock in locks.Where(rl => rl.ClubId == c.Id)
                                                .DefaultIfEmpty()
                         from reg in registrations.Where(r => r.ClubId == c.Id)
                                                .DefaultIfEmpty()
                         select
                         new ClubSignupViewModel
                         {
                             Id = c.Id,
                             Name = c.Name,
                             Description = c.Description,
                             Days = c.Days,
                             Grades = c.Grades,
                             Time = c.Time,
                             Price = c.Price,
                             Capacity = c.Capacity,
                             Remaining = c.Capacity - (regLock.LockCount == null ? 0 : regLock.LockCount) - (reg.RegistrationCount == null ? 0 : reg.RegistrationCount)
                         })
                         .Where(c => c.Remaining>0);

            foreach(var clubid in clubs)
            {
                if (availableClubs.Where(c => c.Id == clubid).Count() == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}