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

            var gateway = config.GetGateway();
            var clientToken = gateway.ClientToken.generate();
            ViewBag.ClientToken = clientToken;

            var viewModel = new PaymentViewModel
            {
                Total = 29.95f
            };

            return View(viewModel);
        }

        public ActionResult Create()
        {
            var gateway = config.GetGateway();
            Decimal amount;

            try
            {
                amount = 29.95M;// Convert.ToDecimal(Request["amount"]);
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
    }
}