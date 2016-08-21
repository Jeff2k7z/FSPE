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
                PaymentMethodNonce = nonce,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
                
            };

            Result<Transaction> result = gateway.Transaction.Sale(request);
            if (result.IsSuccess())
            {
                Transaction transaction = result.Target;
                return RedirectToAction("Show", new { id = transaction.Id });
            }
            else if (result.Transaction != null)
            {
                // This happens for AVS and other "Gateway" rejections
                TempData["Flash"] = "Error";
                return RedirectToAction("New");
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
                var cookie = this.ControllerContext.HttpContext.Request.Cookies["FOSPERegistrationKey"];

                if (cookie != null) {
                    var registrationKey = cookie.Value;

                    // Update the club registrations to show paid
                    _context.ClubRegistrations
                        .Where(cr => cr.RegistrationKey == registrationKey)
                        .ToList()
                        .ForEach(cr => cr.IsPaid = true);
                    

                    // Remove the locks associated with these registrations
                    _context.RegistrationLocks.RemoveRange(_context.RegistrationLocks.Where(x => x.LockKey == registrationKey));

                    _context.SaveChanges();
                
                    cookie.Expires = DateTime.Now.AddDays(-1);
                    this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                }


                TempData["header"] = "Sweet Success!";
                TempData["icon"] = "success";
                TempData["message"] = "Your payment has been successfully processed. Thanks for signing up for our clubs!";
            }
            else
            {
                TempData["header"] = "There was a problem processing your payment.";
                TempData["icon"] = "fail";
                TempData["message"] = "Your test transaction has a status of " + transaction.Status + ". See the Braintree API response and try again.";
                //TempData["message"] = "Please try again.";
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

        public RedirectToRouteResult ClearCart()
        {
            // Read or create registration key
            string registrationKey = this.ControllerContext.HttpContext.Request.Cookies["FOSPERegistrationKey"].Value;

            _context.ClubRegistrations.RemoveRange(_context.ClubRegistrations.Where(x => x.RegistrationKey == registrationKey));
            _context.RegistrationLocks.RemoveRange(_context.RegistrationLocks.Where(x => x.LockKey == registrationKey));
            _context.SaveChanges();

            return RedirectToAction("Cart", "Checkout");
        } 

        public ViewResult Coupon(string couponCode)
        {
            if (couponCode == "SPEFAC")
            {
                var cookie = this.ControllerContext.HttpContext.Request.Cookies["FOSPERegistrationKey"];

                if (cookie != null)
                {
                    var registrationKey = cookie.Value;

                    // Update the club registrations to show paid
                    _context.ClubRegistrations
                        .Where(cr => cr.RegistrationKey == registrationKey)
                        .ToList()
                        .ForEach(cr => cr.IsPaid = true);

                    // Update the coupon code
                    if (!string.IsNullOrWhiteSpace(couponCode))
                    {
                        _context.ClubRegistrations
                            .Where(cr => cr.RegistrationKey == registrationKey)
                            .ToList()
                            .ForEach(cr => cr.CouponCode = couponCode);
                    }

                    // Remove the locks associated with these registrations
                    _context.RegistrationLocks.RemoveRange(_context.RegistrationLocks.Where(x => x.LockKey == registrationKey));

                    _context.SaveChanges();

                    cookie.Expires = DateTime.Now.AddDays(-1);
                    this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
                }


                TempData["header"] = "Sweet Success!";
                TempData["icon"] = "success";
                TempData["message"] = "Your payment has been successfully processed. Thanks for signing up for our clubs!";
            }

            
            return View("~/Views/Checkout/Show.cshtml");
        }
    }
}