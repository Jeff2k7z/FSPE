using Braintree;
using FSPE.Models;
using FSPE.ViewModels;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            var order = (from cr in clubRegistrations
                         join c in _context.Clubs on cr.ClubId equals c.Id
                         select new
                         {
                             c.Name,
                             c.Days,
                             cr.ChildName,
                             c.Price
                         });
            
            var total = order.Sum(s => s.Price);

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

                var bodyText = "";
                try
                {
                    var parentName = clubRegistrations.First().ParentName;
                    var parentEmail = clubRegistrations.First().EmailAddress;
                    foreach (var line in order)
                    {
                        bodyText += "<tr class=\"item\"><td>";
                        bodyText += line.Name + " - " + line.Days + "</td><td style=\"vertical-align: top;text-align: right;padding: 5px 5px 20px;\">";
                        bodyText += "$" + line.Price + "</td></tr>";
                    }
                    SendOrderConfirmationEmail(bodyText, total,parentEmail, parentName, transaction.Id);
                }
                catch { }

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

        public ActionResult SendMail()
        {
            var bodyText = "<tr class=\"item\"><td>Running - Monday</td><td style=\"vertical-align: top;text-align: right;padding: 5px 5px 20px;\">$5.00</td></tr>";
            var total = 5.00d;
          
            var result = SendOrderConfirmationEmail(bodyText, total, "jeff2k7z@gmail.com", "Jeff Bolton", "8675309");
            var status = result.Result;
            result.Wait();

            TempData["header"] = "Sweet Success!";
            TempData["icon"] = "success";
            TempData["message"] = status;



            return View("~/Views/Checkout/Show.cshtml");
        }

        private async Task<string> SendOrderConfirmationEmail(string bodyText, double total, string emailAddress, string name, string invoiceNumber)
        {
            var apiKey = "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";
            dynamic sg = new SendGrid.SendGridAPIClient(apiKey, "https://api.sendgrid.com");

            Email from = new Email("darlene.m.strickland@gmail.com");
            String subject = "Your Receipt from FOSPE";
            Email to = new Email(emailAddress);
            Content content = new Content("text/html", bodyText);
            Mail mail = new Mail(from, subject, to, content);
            //Email email = new Email("jeff2k7z@gmail.com");
            //mail.Personalization[0].AddBcc(email);

            mail.TemplateId = "22f3fd07-7217-457f-99b3-b97f8df2854e";
            //mail.TemplateId = "4b6509b8-c28c-4736-b071-4958d1f775cc";
            mail.Personalization[0].AddSubstitution("-total-", total.ToString("$####.00"));
            mail.Personalization[0].AddSubstitution("-recipient-", emailAddress);
            mail.Personalization[0].AddSubstitution("-name-", name);
            mail.Personalization[0].AddSubstitution("-createddate-", DateTime.Now.AddHours(-4).ToString("MM/dd/yyyy"));
            mail.Personalization[0].AddSubstitution("-invoicenumber-", invoiceNumber);

            dynamic response = await sg.client.mail.send.post(requestBody: mail.Get());
            Console.WriteLine(response.StatusCode);
            Console.WriteLine(response.Body.ReadAsStringAsync().Result);
            Console.WriteLine(response.Headers.ToString());

            return response.StatusCode.ToString();

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
                //TempData["message"] = "Your test transaction has a status of " + transaction.Status + ". See the Braintree API response and try again.";
                TempData["message"] = "Please try again.";
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