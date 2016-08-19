using Braintree;
using FSPE.Models;
using FSPE.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FSPE.Controllers
{
    public class CheckoutController : Controller
    {
        public IBraintreeConfiguration config = new BraintreeConfiguration();

        public static readonly TransactionStatus[] transactionSuccessStatuses = {
                                                                                    TransactionStatus.AUTHORIZED,
                                                                                    TransactionStatus.AUTHORIZING,
                                                                                    TransactionStatus.SETTLED,
                                                                                    TransactionStatus.SETTLING,
                                                                                    TransactionStatus.SETTLEMENT_CONFIRMED,
                                                                                    TransactionStatus.SETTLEMENT_PENDING,
                                                                                    TransactionStatus.SUBMITTED_FOR_SETTLEMENT
                                                                                };

        private ApplicationDbContext _context;

        public CheckoutController()
        {
            _context = new ApplicationDbContext();
        }

        public ActionResult New(RegisterClubViewModel model)
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

            var clubRegistrations = _context.ClubRegistrations.Where(cr => cr.RegistrationKey == registrationKey);
            var clubs = _context.Clubs.ToList();

            var total = (from cr in clubRegistrations
                         join c in _context.Clubs on cr.ClubId equals c.Id
                         select new
                         {
                             c.Price
                         }).Sum(s => s.Price);

            var gateway = config.GetGateway();
            var clientToken = gateway.ClientToken.generate();
            ViewBag.ClientToken = clientToken;

            var viewModel = new PaymentViewModel
            {
                Total = (decimal)total
            };

            return View(viewModel);
        }

        public ActionResult Create(string couponCode)
        {
            var gateway = config.GetGateway();
            Decimal amount;

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

            var clubRegistrations = _context.ClubRegistrations.Where(cr => cr.RegistrationKey == registrationKey);
            var clubs = _context.Clubs.ToList();

            var total = (from cr in clubRegistrations
                         join c in _context.Clubs on cr.ClubId equals c.Id
                         select new
                         {
                             c.Price
                         }).Sum(s => s.Price);

            try
            {
                amount = (decimal)total;// Convert.ToDecimal(Request["amount"]);
            }
            catch (FormatException e)
            {
                TempData["Flash"] = "Error: 81503: Amount is an invalid format.";
                return RedirectToAction("New");
            }

            var nonce = Request["payment_method_nonce"];
            var request = new TransactionRequest
            {
                Amount = amount,
                PaymentMethodNonce = nonce
            };

            Result<Transaction> result = gateway.Transaction.Sale(request);
            if (result.IsSuccess())
            {
                Transaction transaction = result.Target;
                return RedirectToAction("Show", new { id = transaction.Id });
            }
            else if (result.Transaction != null)
            {
                return RedirectToAction("Show", new { id = result.Transaction.Id });
            }
            else
            {
                string errorMessages = "";
                foreach (ValidationError error in result.Errors.DeepAll())
                {
                    errorMessages += "Error: " + (int)error.Code + " - " + error.Message + "\n";
                }
                TempData["Flash"] = errorMessages;
                return RedirectToAction("New");
            }

            

        }

        public ActionResult Show(String id)
        {
            var gateway = config.GetGateway();
            Transaction transaction = gateway.Transaction.Find(id);

            if (transactionSuccessStatuses.Contains(transaction.Status))
            {
                string registrationKey = this.ControllerContext.HttpContext.Request.Cookies["FOSPERegistrationKey"].Value;

                // Update the club registrations to show paid
                _context.ClubRegistrations
                    .Where(cr => cr.RegistrationKey == registrationKey)
                    .ToList()
                    .ForEach(cr => cr.IsPaid = true);

                // Remove the locks associated with these registrations
                _context.RegistrationLocks.RemoveRange(_context.RegistrationLocks.Where(x => x.LockKey == registrationKey));

                _context.SaveChanges();

                // Remove the registration cookies
                if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("FOSPERegistrationKey"))
                {
                    HttpCookie cookie = this.ControllerContext.HttpContext.Request.Cookies["FOSPERegistrationKey"];
                    cookie.Expires = DateTime.Now.AddDays(-1);
                    this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                }


                TempData["header"] = "Sweet Success!";
                TempData["icon"] = "success";
                TempData["message"] = "Your test transaction has been successfully processed. See the Braintree API response and try again.";
            }
            else
            {
                TempData["header"] = "Transaction Failed";
                TempData["icon"] = "fail";
                TempData["message"] = "Your test transaction has a status of " + transaction.Status + ". See the Braintree API response and try again.";
            };

            ViewBag.Transaction = transaction;
            return View();
        }

        public ViewResult Cart()
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

            var clubRegistrations = _context.ClubRegistrations.Where(cr => cr.RegistrationKey == registrationKey);
            var clubs = _context.Clubs.ToList();

            var viewModel = from cr in clubRegistrations
                            join c in _context.Clubs on cr.ClubId equals c.Id
                            select new CartViewModel
                            {
                                registration = cr,
                                club = c
                            };

            return View(viewModel);
        }
    }
}