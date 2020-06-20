using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Ownorent.Models;
using System.Net.Mail;
using System.Collections.Generic;
using System.IO;
using System.Data.Entity;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;

namespace Ownorent.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        private ApplicationDbContext db = new ApplicationDbContext();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        
        public async Task<ActionResult> Wallet()
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            string userId = User.Identity.GetUserId();

            var payments = await db.Payments.Where(p => 
                    p.Transaction.Product.ProductTemplate.UserId == userId && 
                    p.Transaction.TransactionStatus != TransactionStatusConstant.PENDING &&
                    p.TransactionGroupPaymentAttempt.Status == TransactionGroupPaymentStatusConstant.SUCCESS).ToListAsync();

            var platformTaxCashout = db.Settings.FirstOrDefault(s => s.Code == "PLATFORM_TAX_CASHOUT");
            ViewBag.platformTaxCashout = (platformTaxCashout != null) ? platformTaxCashout.Value : "1";

            return View(payments);
        }

        public ActionResult EditPayout(int id)
        {
            string userId = User.Identity.GetUserId();

            var payment = db.Payments.FirstOrDefault(p => p.PaymentId == id);

            if (payment != null)
            {
                return View(payment);
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Request payout failed.</strong> Payment ID does not exist in the DB.";
                return RedirectToAction("Wallet");
            }
        }

        [HttpPost]
        public ActionResult EditPayout(Payment payment)
        {
            string userId = User.Identity.GetUserId();
            int paymentId = payment.PaymentId;

            var editPayment = db.Payments.FirstOrDefault(p => p.PaymentId == paymentId);

            if (payment != null)
            {
                editPayment.Bank = payment.Bank;
                editPayment.AccountNumber = payment.AccountNumber;
                db.SaveChanges();
                TempData["Message"] = "<strong>Payment request edited successfully.</strong>.";
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Request payout failed.</strong> Payment ID does not exist in the DB.";
            }

            return RedirectToAction("Wallet");
        }

        public ActionResult RequestPayout(int id)
        {
            string userId = User.Identity.GetUserId();

            var payment = db.Payments.FirstOrDefault(p => p.PaymentId == id);

            if (payment != null)
            {
                payment.SellerPaymentStatus = SellerPayoutStatusConstant.REQUESTED;
                db.SaveChanges();
                TempData["Message"] = "<strong>Payout requested successfully.</strong> Please wait for an admin to review.";
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Request payout failed.</strong> Payment ID does not exist in the DB.";
            }

            return RedirectToAction("Wallet");
        }

        public ActionResult CancelRequestPayout(int id)
        {
            string userId = User.Identity.GetUserId();

            var payment = db.Payments.FirstOrDefault(p => p.PaymentId == id);

            if (payment != null)
            {
                payment.SellerPaymentStatus = SellerPayoutStatusConstant.PENDING;
                db.SaveChanges();
                TempData["Message"] = "<strong>Payout request cancelled.</strong>.";
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Request payout cancellation failed.</strong> Payment ID does not exist in the DB.";
            }

            return RedirectToAction("Wallet");
        }

        public ActionResult Orders()
        {
            ViewBag.Error = TempData["Error"];
            ViewBag.Message = TempData["Message"];

            string userId = User.Identity.GetUserId();
            var orders = db.TransactionGroups
                .Include(o=>o.Transactions)
                .Where(o => o.UserId == userId && !o.Transactions.Any(t=>t.TransactionStatus == TransactionStatusConstant.PENDING));

            return View(orders.ToList());
        }

        public async Task<ActionResult> ViewOrder(int id)
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            string userId = User.Identity.GetUserId();
            var order = db.TransactionGroups.Include(t => t.Transactions).FirstOrDefault(t => t.TransactionGroupId == id && t.UserId == userId);

            if (order != null)
            {
                foreach (var transaction in order.Transactions)
                {
                    transaction.Payments = await db.Payments.Where(p => p.TransactionId == transaction.TransactionId).ToListAsync();
                }

                return View(order);
            }
            else
            {
                return HttpNotFound();
            }
        }

        public async Task<ActionResult> Pay(List<PaymentPayModel> payments)
        {
            List<int> paymentIds = new List<int>();
            payments.Where(p=>p.Include == true).ToList().ForEach(p => paymentIds.Add(p.PaymentId));

            var paymentsToSend = db.Payments.Where(p => paymentIds.Contains(p.PaymentId)).ToList();

            if (paymentsToSend != null)
            {
                int orderId = paymentsToSend.FirstOrDefault().Transaction.TransactionGroupId;

                TransactionGroupPaymentAttempt paymentAttempt = new TransactionGroupPaymentAttempt() {
                    TransactionGroupId = orderId,
                    Status = TransactionGroupPaymentStatusConstant.PENDING
                };
                db.TransactionGroupPaymentAttempts.Add(paymentAttempt);
                await db.SaveChangesAsync();

                PaypalCreateOrderModel newOrder = new PaypalCreateOrderModel();
                newOrder.application_context.return_url = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("PaymentReceive", "Account");
                PaypalCreateOrderModel.PaypalPurchaseUnitModel mainPurchaseUnit = newOrder.purchase_units.FirstOrDefault();

                float paymentTotal = 0f;

                #region pending payments loop
                foreach (var payment in paymentsToSend)
                {
                    payment.TransactionGroupPaymentAttemptId = paymentAttempt.TransactionGroupPaymentAttemptId;

                    float paymentAmount = (float)Math.Round(payment.Amount, 2);
                    paymentTotal += paymentAmount;

                    mainPurchaseUnit.items.Add(new PaypalCreateOrderModel.PaypalPurchaseUnitModel.PaypalItemModel()
                    {
                        name = payment.Transaction.Product.ProductName,
                        quantity = "1",
                        sku = "OWNO00-" + payment.Transaction.Product.ProductTemplateId,
                        description = payment.Description,
                        unit_amount = new PaypalCreateOrderModel.PaypalPurchaseUnitModel.PaypalUnitAmountModel
                        {
                            currency_code = "PHP",
                            value = paymentAmount.ToString()
                        }
                    });
                }
                #endregion

                float platformTaxOrder;
                Setting platformTaxOrderSetting = db.Settings.FirstOrDefault(s => s.Code == "PLATFORM_TAX_ORDER");
                platformTaxOrder = (platformTaxOrderSetting != null) ? (float.Parse(platformTaxOrderSetting.Value) / 100) : (2 / 100);
                
                mainPurchaseUnit.amount.value = paymentTotal.ToString();
                mainPurchaseUnit.amount.breakdown.item_total.value = paymentTotal.ToString();
                paymentAttempt.TotalAmount = paymentTotal;
                paymentAttempt.PlatformTaxOrder = platformTaxOrder;
                paymentAttempt.AmountForSystem = platformTaxOrder * paymentAttempt.TotalAmount;
                paymentAttempt.AmountForSeller = paymentAttempt.TotalAmount - (platformTaxOrder * paymentAttempt.TotalAmount);

                #region checkout http request
                PaypalCheckoutResultModel checkoutResult;
                using (HttpClient client = new HttpClient())
                {
                    if (ServicePointManager.SecurityProtocol != SecurityProtocolType.Tls12)
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    }

                    try
                    {
                        var uri = new Uri("https://api.sandbox.paypal.com/v2/checkout/orders");
                        client.DefaultRequestHeaders.Authorization = OwnorentHelper.BasicAuthenticationHeader;
                        var content = new StringContent(JsonConvert.SerializeObject(newOrder), Encoding.UTF8, "application/json");
                        var result = await client.PostAsync(uri, content);
                        string resultContent = await result.Content.ReadAsStringAsync();
                        checkoutResult = JsonConvert.DeserializeObject<PaypalCheckoutResultModel>(resultContent);
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = "1";
                        TempData["Message"] = "<strong>Sorry, payment failed. Please send a screenshot of the error to an admin.</strong> " + ex.Message;
                        return RedirectToAction("ViewOrder", new { id = orderId });
                    }
                }
                #endregion

                if (checkoutResult.status == "CREATED")
                {
                    // assign Transaction ID/ Reference ID from paypal to DB
                    paymentAttempt.Code = checkoutResult.id;
                    await db.SaveChangesAsync();

                    var checkoutLinkObject = checkoutResult.links.FirstOrDefault(l => l.rel == "approve");

                    if (checkoutLinkObject != null)
                    {
                        return Redirect(checkoutLinkObject.href);
                    }
                    else
                    {
                        TempData["Error"] = "1";
                        TempData["Message"] = "<strong>Sorry, payment failed. Please send a screenshot of the error to an admin.</strong> Paypal link result doesn't contain APPROVE url.";
                        return RedirectToAction("ViewOrder", new { id = orderId });
                    }
                }
                else
                {
                    TempData["Error"] = "1";
                    TempData["Message"] = "<strong>Sorry, payment failed. Please send a screenshot of the error to an admin.</strong> Paypal checkout request result is not CREATED.";
                    return RedirectToAction("ViewOrder", new { id = orderId });
                }
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Payment attempt failed.</strong> No payments selected.";
                return RedirectToAction("Orders");
            }
        }

        [AllowAnonymous]
        public ActionResult PaymentReceive(string token, string PayerID)
        {
            if (token != null)
            {
                TransactionGroupPaymentAttempt paymentAttempt = db.TransactionGroupPaymentAttempts.FirstOrDefault(o => o.Code == token);

                if (paymentAttempt != null)
                {
                    #region paypal payment checkout success
                    paymentAttempt.Status = TransactionGroupPaymentStatusConstant.SUCCESS;
                    paymentAttempt.DatePaid = DateTime.UtcNow.AddHours(8);
                    paymentAttempt.PayerId = PayerID;

                    var transactions = db.Transactions.Include(t => t.Payments).Where(t => t.TransactionGroupId == paymentAttempt.TransactionGroupId).ToList();

                    // save now, payment check later
                    db.SaveChanges();

                    // change product availability
                    foreach (var transaction in transactions)
                    {
                        if (transaction.Payments.Any(p => p.TransactionGroupPaymentAttemptId == null || p.TransactionGroupPaymentAttempt.Status != TransactionGroupPaymentStatusConstant.SUCCESS))
                        {
                            // if payment attempt is not done for the transaction
                            // or the payments are pending
                            transaction.TransactionStatus = TransactionStatusConstant.PARTIALLY_PAID;
                        }
                        else
                        {
                            // if the payment attempt is paid
                            transaction.TransactionStatus = TransactionStatusConstant.PAID;
                        }
                    }

                    db.SaveChanges();

                    #region Send SMS to Customer
                    try
                    {
                        string userId = paymentAttempt.TransactionGroup.UserId;
                        Task.Run(() =>
                        {
                            new SMSController().SendSMS(userId, "We have received your payment for Transaction: OWNO-OR-" + paymentAttempt.TransactionGroupId + ". Please see the orders page to view details.");
                        });
                    }
                    catch (Exception e)
                    {
                        Trace.TraceInformation("SMS Send Failed for OWNO-OR-" + paymentAttempt.TransactionGroupId + ": " + e.Message);
                    }
                    #endregion

                    TempData["Message"] = "<strong>Thank you! We have received your payment.</strong> The transaction has been updated.";
                    return RedirectToAction("ViewOrder", new { id = paymentAttempt.TransactionGroupId });
                    #endregion
                }
                else
                {
                    return HttpNotFound();
                }
            }
            else
            {
                return HttpNotFound();
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Accounts()
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            var users = db.Users.ToList();
            return View(users);
        }

        public ActionResult ViewRequirements(string email)
        {
            var user = db.Users.Include(u=>u.Attachments).FirstOrDefault(u => u.Email == email);
            ViewBag.Email = user.Email;
            return View(user.Attachments.ToList());
        }

        public ActionResult ActivateAccount(string email)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.AccountStatus = AccountStatusConstant.APPROVED;
            }
            db.SaveChanges();

            string body = "Your account has been activated. Please login <a href='http://ownorent.azurewebsites.net/Account/Login/'>here.</a>";

            MailAddress fromAddress = new MailAddress("ownorent@gmail.com", "Ownorent Registration Service");
            MailAddress toAddress = new MailAddress(user.Email, user.FirstName);
            string fromPassword = "ownorent$123456";
            string subject = "Ownorent Account Activation - " + DateTime.UtcNow.AddHours(8).ToString("MM-dd-yy");

            SmtpClient smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(fromAddress.Address, fromPassword)
            };

            MailMessage message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };
            message.CC.Add(new MailAddress(user.Email));

            try
            {
                smtp.Send(message);
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }

            TempData["Message"] = "<strong>Account activation successful.</strong> "+user.Email+" has been notified.";
            return RedirectToAction("Accounts");
        }

        public ActionResult DisableAccount(string email)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.AccountStatus = AccountStatusConstant.DISABLED;
            }
            db.SaveChanges();
            TempData["Message"] = "<strong>Account modification (Disabled) successful.</strong> " + user.Email + " has been <strong>Disabled</strong>.";
            return RedirectToAction("Accounts");
        }

        public ActionResult RejectReviseAccount(string email)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.AccountStatus = AccountStatusConstant.REJECTED_RESUBMIT_REQUIREMENTS;
            }
            db.SaveChanges();

            string body = "Your account has been rejected. An admin will contact you for additional requirements. Please see your request by logging in <a href='http://ownorent.azurewebsites.net/Account/Login/'>here.</a>";

            MailAddress fromAddress = new MailAddress("ownorent@gmail.com", "Ownorent Registration Service");
            MailAddress toAddress = new MailAddress(user.Email, user.FirstName);
            string fromPassword = "ownorent$123456";
            string subject = "Ownorent Requirements Revision - " + DateTime.UtcNow.AddHours(8).ToString("MM-dd-yy");

            SmtpClient smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(fromAddress.Address, fromPassword)
            };

            MailMessage message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };
            message.CC.Add(new MailAddress(user.Email));

            try
            {
                smtp.Send(message);
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }

            TempData["Message"] = "<strong>Account modification (Rejected - Resubmit Requirements) successful.</strong> " + user.Email + " has been notified.";
            return RedirectToAction("Accounts");
        }

        public ActionResult RejectAccount(string email)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.AccountStatus = AccountStatusConstant.REJECTED;
            }
            db.SaveChanges();

            string body = "Your account registration request has been rejected.</a>";

            MailAddress fromAddress = new MailAddress("ownorent@gmail.com", "Ownorent Registration Service");
            MailAddress toAddress = new MailAddress(user.Email, user.FirstName);
            string fromPassword = "ownorent$123456";
            string subject = "Ownorent Account Rejected - " + DateTime.UtcNow.AddHours(8).ToString("MM-dd-yy");

            SmtpClient smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(fromAddress.Address, fromPassword)
            };

            MailMessage message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };
            message.CC.Add(new MailAddress(user.Email));

            try
            {
                smtp.Send(message);
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }

            TempData["Message"] = "<strong>Account modification (Rejected) successful.</strong> " + user.Email + " has been notified.";
            return RedirectToAction("Accounts");
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            return View();
        }
        
        public ActionResult Requirements()
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            string username = User.Identity.Name;
            var user = db.Users.Include(u => u.Attachments).FirstOrDefault(u => u.Email == username);
            var files = user.Attachments.ToList();

            if (user.AccountStatus == AccountStatusConstant.PENDING_REQUIREMENTS || user.AccountStatus == AccountStatusConstant.PENDING_REVIEW)
            {
                ViewBag.VerificationStatus = "Pending Verification";
            }else if (user.AccountStatus == AccountStatusConstant.REJECTED_RESUBMIT_REQUIREMENTS)
            {
                ViewBag.VerificationStatus = "Please resubmit requirements based on email sent to you";
            }else if (user.AccountStatus == AccountStatusConstant.REJECTED_NOT_ACCEPTING_NEW || user.AccountStatus == AccountStatusConstant.REJECTED)
            {
                ViewBag.VerificationStatus = "Rejected";
            }else if (user.AccountStatus == AccountStatusConstant.APPROVED)
            {
                ViewBag.VerificationStatus = "Verified";
            }

            return View(files);
        }

        [HttpPost]
        public ActionResult Requirements(List<HttpPostedFileBase> files)
        {
            string username = User.Identity.Name;

            if (ModelState.IsValid)
            {
                if (files.Count > 10)
                {
                    TempData["Error"] = "1";
                    TempData["Message"] = "<strong>Failed to save files</strong>, the number of files should not exceed 10.";
                    return RedirectToAction("Requirements");
                }

                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        if (file.ContentLength > 5000000)
                        {
                            TempData["Error"] = "1";
                            TempData["Message"] = "<strong>Failed to save files</strong>, one file is more than 5MB.";
                            return RedirectToAction("Requirements");
                        }
                    }
                }

                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        if(file.ContentLength <= 5000000)
                        {
                            // extract only the filename
                            string fileName = Path.GetFileName(file.FileName);
                            string fileExt = Path.GetExtension(file.FileName);
                            string newName = OwnorentHelper.RandomString(24)+fileExt;

                            string targetPath = Server.MapPath("~/Content/Uploads/Requirements/");

                            if (!System.IO.Directory.Exists(targetPath))
                            {
                                System.IO.Directory.CreateDirectory(targetPath);
                            }

                            string path = Path.Combine(targetPath, newName);

                            try
                            {
                                file.SaveAs(path);
                            }
                            catch (Exception ex)
                            {
                                TempData["Error"] = "1";
                                TempData["Message"] = "<strong>Failed to save files</strong>, error: " + ex.Message;
                                return RedirectToAction("Requirements");
                            }

                            db.UserAttachments.Add(new UserAttachment()
                            {
                                Location = newName,
                                UserId = User.Identity.GetUserId()
                            });
                        }
                    }
                }
            }

            db.SaveChanges();
            return RedirectToAction("Requirements");
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    var user = db.Users.FirstOrDefault(u => u.UserName == model.Email);

                    if (user!=null)
                    {
                        DateTime today = DateTime.UtcNow.AddHours(8);
                        string role = "N/A";

                        switch (user.AccountType)
                        {
                            case AccountTypeConstant.SELLER:
                                role = "SELLER";
                                break;
                            case AccountTypeConstant.CUSTOMER:
                                role = "CUSTOMER";
                                break;
                            case AccountTypeConstant.ADMIN:
                                role = "ADMIN";
                                break;
                        }

                        var loginList = db.LoginHistories.Where(h => h.UserId == user.Id).ToList();

                        if (!loginList.Any(h=>h.DateCreated.Date == today.Date))
                        {
                            db.LoginHistories.Add(new LoginHistory
                            {
                                UserId = user.Id,
                                Role = role,
                                DateCreated = today
                            });
                            db.SaveChanges();
                        }

                        if (user.AccountType == AccountTypeConstant.SELLER)
                        {
                            if (user.AccountStatus == AccountStatusConstant.PENDING_REQUIREMENTS || user.AccountStatus == AccountStatusConstant.PENDING_REVIEW)
                            {
                                return RedirectToAction("Requirements");
                            }
                        }

                        if (user.AccountStatus == AccountStatusConstant.DISABLED || user.EmailConfirmed == false)
                        {
                            ViewBag.Error = "1";
                            ViewBag.Message = "The account you're trying to access either not Verified or has been disabled by the Admin.";
                            return View(model);
                        }
                    }
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        [Authorize(Roles="Admin")]
        public ActionResult AddAccount()
        {
            RegisterViewModel rvm = new RegisterViewModel();
            rvm.Country = "Philippines";

            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            return View(rvm);
        }

        [Authorize(Roles="Admin")]
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAccount(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    MobileNumber = model.MobileNumber,
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    AccountType = model.AccountType,
                    AccountStatus = AccountStatusConstant.APPROVED
                };

                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    db.Addresses.Add(new Address
                    {
                        Line1 = model.Line1,
                        Line2 = model.Line2,
                        Line3 = model.Line3,
                        City = model.City,
                        Zip = model.Zip,
                        Country = model.Country,
                        UserId = user.Id,
                        IsDefault = true
                    });

                    await db.SaveChangesAsync();

                    TempData["Message"] = "<strong>Account creation successful.</strong> Account has also been activated.";
                    return RedirectToAction("Accounts", "Account");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            RegisterViewModel rvm = new RegisterViewModel();
            rvm.Country = "Philippines";

            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            return View(rvm);
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser {
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    MobileNumber = model.MobileNumber,
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = false,
                    AccountType = model.AccountType,
                    AccountStatus = AccountStatusConstant.DISABLED
                };

                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);

                    string emailCode = Guid.NewGuid().ToString();
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { code = emailCode }, protocol: Request.Url.Scheme);
                    string body = "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>";

                    MailAddress fromAddress = new MailAddress("ownorent@gmail.com", "Ownorent Registration Service");
                    MailAddress toAddress = new MailAddress(model.Email, model.FirstName);
                    string fromPassword = "ownorent$123456";
                    string subject = "Ownorent Account Confirmation - " + DateTime.UtcNow.AddHours(8).ToString("MM-dd-yy");

                    SmtpClient smtp = new SmtpClient()
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new System.Net.NetworkCredential(fromAddress.Address, fromPassword)
                    };

                    MailMessage message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body
                    };
                    message.CC.Add(new MailAddress(model.Email));

                    try
                    {
                        smtp.Send(message);
                    }
                    catch (Exception e) {
                        return Content(e.Message);
                    }

                    var findUser = db.Users.FirstOrDefault(u => u.Email == model.Email);
                    findUser.ConfirmationCode = emailCode;

                    db.Addresses.Add(new Address
                    {
                        Line1 = model.Line1,
                        Line2 = model.Line2,
                        Line3 = model.Line3,
                        City = model.City,
                        Zip = model.Zip,
                        Country = model.Country,
                        UserId = findUser.Id,
                        IsDefault = true
                    });

                    await db.SaveChangesAsync();
                    
                    TempData["Message"] = "<strong>Registration Success!</strong> To proceed, please click the confirmation link sent to <strong>"+model.Email+"</strong>.";
                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string code)
        {
            if (code == null)
            {
                return View("Error");
            }

            var user = db.Users.FirstOrDefault(u => u.ConfirmationCode == code);

            if (user == null)
            {
                return Content("Error: Invalid code.");
            }else
            {
                user.EmailConfirmed = true;
                user.AccountStatus = (user.AccountType == AccountTypeConstant.CUSTOMER) ? AccountStatusConstant.APPROVED : AccountStatusConstant.PENDING_REQUIREMENTS;
            }

            await db.SaveChangesAsync();

            if (user.AccountType == AccountTypeConstant.CUSTOMER)
            {
                UserManager.AddToRole(user.Id, "Customer");
            }
            else if (user.AccountType == AccountTypeConstant.SELLER)
            {
                UserManager.AddToRole(user.Id, "Seller");
            }

            TempData["Message"] = (user.AccountType == AccountTypeConstant.CUSTOMER)
                ? "<strong>Verification successful!</strong> Please login."
                : "<strong>Verification successful!</strong> Please login to proceed with Requirements submission.";

            return RedirectToAction("Login","Account");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}