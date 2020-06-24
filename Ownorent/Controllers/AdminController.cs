using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Ownorent.Models;
using Microsoft.AspNet.Identity;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;

namespace Ownorent.Controllers
{
    [Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public async Task<ActionResult> Index()
        {
            DateTime today = DateTime.UtcNow.AddHours(8).AddDays(1).Date;
            DateTime daysAgo = DateTime.UtcNow.AddHours(8).AddDays(-29).Date;

            AdminDashboardModel dashboard = new AdminDashboardModel();

            #region logins by role for the past 5 days
            var loginHistories =
                await db.LoginHistories
                .Where(h => h.DateCreated <= today && h.DateCreated >= daysAgo).ToListAsync();

            var loginHistoriesGroupedByDate =
                loginHistories
                .GroupBy(h => h.DateCreated.Date);

            dashboard.LoginsByRolePast5Days =
                (from h in loginHistories
                group h by h.DateCreated.Date into groupedByDate
                from groupedByDateAndRole in
                    (from l in groupedByDate
                     group l by l.Role)
                group groupedByDateAndRole by groupedByDate.Key)
                .OrderByDescending(t=>t.Key).ToList();
            #endregion

            #region categoryWithMostTransactions
            dashboard.CategoriesWithMostTransactions = await db.Transactions
                .Where(t => t.TransactionStatus != TransactionStatusConstant.PENDING)
                .Select(t => new AdminDashboardModel.TransactionCategoryModel { Transaction = t, Category = t.Product.ProductTemplate.Category })
                .GroupBy(t => t.Category)
                .OrderByDescending(p => p.Count())
                .Take(5)
                .ToListAsync();
            #endregion

            #region categoryWithMostItems
            dashboard.CategoriesWithMostItems = await db.Categories
                .Select(c => new AdminDashboardModel.CategoryCountModel { Category = c, Count = c.ProductTemplates.Count })
                .ToListAsync();
            #endregion

            #region productWithMostItemsSold
            dashboard.ProductsWithMostItemsSold = await db.Transactions
                .Where(t => t.TransactionStatus != TransactionStatusConstant.PENDING)
                .Select(t=> new AdminDashboardModel.TransactionPTemplateModel { Transaction = t, ProductTemplate = t.Product.ProductTemplate })
                .GroupBy(t=>t.ProductTemplate)
                .OrderByDescending(p=>p.Count())
                .Take(5)
                .ToListAsync();
            #endregion

            #region productWithMostIncome
            dashboard.ProductsWithMostIncome = await db.Transactions
                .Include(t=>t.Payments)
                .Where(t => t.TransactionStatus != TransactionStatusConstant.PENDING)
                .Select(t => new AdminDashboardModel.TransactionPTemplateModel { Transaction = t, ProductTemplate = t.Product.ProductTemplate })
                .GroupBy(t => t.ProductTemplate)
                .Select(t => new AdminDashboardModel.ProductTemplateIncomeModel {
                    ProductTemplate = t,
                    Income = t.Sum(s => s.Transaction.Payments
                        .Where(p=>p.TransactionGroupPaymentAttempt.Status == TransactionGroupPaymentStatusConstant.SUCCESS)
                        .Sum(p=>p.Amount)) })
                .OrderByDescending(t => t.Income)
                .Take(5)
                .ToListAsync();
            #endregion

            #region dailyTransactions
            dashboard.TransactionsPast30Days = new List<AdminDashboardModel.TransactionsDatetimeCountModel>();
            for (var dt = daysAgo; dt <= today; dt = dt.AddDays(1))
            {
                dashboard.TransactionsPast30Days.Add(new AdminDashboardModel.TransactionsDatetimeCountModel { Date = dt, Count = 0 });
            }
            
            var transactionsPast30DaysDB = db.Transactions
                .Where(t => 
                    t.TransactionStatus != TransactionStatusConstant.PENDING && 
                    (t.DateCreated <= today && t.DateCreated >= daysAgo)
                ).ToList()
                .GroupBy(t=>t.DateCreated.Date)
                .ToList();

            foreach (var item in transactionsPast30DaysDB)
            {
                if(dashboard.TransactionsPast30Days.FirstOrDefault(x=>x.Date == item.Key) != null)
                {
                    dashboard.TransactionsPast30Days.FirstOrDefault(x => x.Date == item.Key).Count = item.Count();
                }
            }
            #endregion

            return View(dashboard);
        }

        public ActionResult ApproveCancellationRequest(int id)
        {
            string userId = User.Identity.GetUserId();

            var tr = db.Transactions.FirstOrDefault(p => p.TransactionId == id);

            if (tr != null)
            {
                tr.TransactionStatus = TransactionStatusConstant.CANCELLED;
                db.SaveChanges();
                TempData["Message"] = "<strong>Cancellation approved successfully.</strong> Customer will be notified. Please contact the customer to arrange shipment back to the warehouse and refunds if any.";

                try
                {
                    string msg = $"Your cancellation request for Transaction; OWNO-OR-{tr.TransactionId} has been approved. Please contact us to arrange shipment back to the warehouse and refunds if any. An Admin will also contact you for this matter.";
                    OwnorentHelper.SendEmail(tr.TransactionGroup.User.Email, tr.TransactionGroup.User.FirstName, msg);
                }
                catch (Exception e)
                {

                }
                
                return RedirectToAction("ManageOrder", new { id = tr.TransactionGroupId });
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Cancellation failed.</strong> Transaction ID does not exist in the DB.";
                return RedirectToAction("Orders");
            }
        }

        public ActionResult RejectCancellationRequest(int id)
        {
            string userId = User.Identity.GetUserId();

            var tr = db.Transactions.FirstOrDefault(p => p.TransactionId == id);

            if (tr != null)
            {
                tr.TransactionStatus = TransactionStatusConstant.PARTIALLY_PAID;
                db.SaveChanges();
                TempData["Message"] = "<strong>Cancellation rejected successfully.</strong> Customer will be notified. Please contact the customer to explain further.";

                try
                {
                    string msg = $"Your cancellation request for Transaction; OWNO-OR-{tr.TransactionId} has been rejected. Please contact us for clarifications.";
                    OwnorentHelper.SendEmail(tr.TransactionGroup.User.Email, tr.TransactionGroup.User.FirstName, msg);
                }
                catch (Exception e)
                {

                }

                return RedirectToAction("ManageOrder", new { id = tr.TransactionGroupId });
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Reject cancellation failed.</strong> Transaction ID does not exist in the DB.";
                return RedirectToAction("Orders");
            }
        }

        public ActionResult ConfirmCancelledReturned(int id)
        {
            string userId = User.Identity.GetUserId();

            var tr = db.Transactions.FirstOrDefault(p => p.TransactionId == id);

            if (tr != null)
            {
                tr.TransactionStatus = TransactionStatusConstant.CANCELLED_RETURNED;
                tr.Product.ProductStatus = ProductStatusConstant.AVAILABLE;
                db.SaveChanges();
                TempData["Message"] = "<strong>Product return confirmed successfully.</strong> Customer will be notified. Please proceed with issuing refunds if there are any.";

                try
                {
                    string msg = $"The product associated with Transaction; OWNO-OR-{tr.TransactionId} has been returned successfully. An admin will process refunds if there are any. Please expect it in 2 business days.";
                    OwnorentHelper.SendEmail(tr.TransactionGroup.User.Email, tr.TransactionGroup.User.FirstName, msg);
                }
                catch (Exception e)
                {

                }

                return RedirectToAction("ManageOrder", new { id = tr.TransactionGroupId });
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Product return confirmation failed.</strong> Transaction ID does not exist in the DB.";
                return RedirectToAction("Orders");
            }
        }

        public async Task<ActionResult> Payouts()
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }
           
            var payments = await db.Payments.Where(p =>
                    p.Transaction.TransactionStatus != TransactionStatusConstant.PENDING &&
                    p.TransactionGroupPaymentAttempt.Status == TransactionGroupPaymentStatusConstant.SUCCESS).ToListAsync();

            var platformTaxCashout = db.Settings.FirstOrDefault(s => s.Code == "PLATFORM_TAX_CASHOUT");
            ViewBag.platformTaxCashout = (platformTaxCashout != null) ? platformTaxCashout.Value : "1";

            return View(payments);
        }

        public ActionResult MarkPayoutAsSent(int id)
        {
            string userId = User.Identity.GetUserId();

            var payment = db.Payments.FirstOrDefault(p => p.PaymentId == id);

            if (payment != null)
            {
                payment.SellerPaymentStatus = SellerPayoutStatusConstant.SENT;
                db.SaveChanges();

                try
                {
                    string msg = $"Your payout request for Transaction; OWNO-OR-{payment.TransactionId} has been sent with Confirmation Number; {payment.ConfirmationNumber}. NOTE: Payouts sent might reflect on the next banking day. Please contact us if there would be any issues.";
                    OwnorentHelper.SendEmail(payment.Transaction.Product.ProductTemplate.User.Email, payment.Transaction.Product.ProductTemplate.User.FirstName, msg);
                }
                catch (Exception e)
                {

                }

                TempData["Message"] = "<strong>Payout marked as SENT.</strong> Seller will be notified.";
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Mark Payout as SENT failed.</strong> Payment ID does not exist in the DB.";
            }

            return RedirectToAction("Payouts");
        }

        public ActionResult ApprovePayout(int id)
        {
            string userId = User.Identity.GetUserId();

            var payment = db.Payments.FirstOrDefault(p => p.PaymentId == id);

            if (payment != null)
            {
                payment.SellerPaymentStatus = SellerPayoutStatusConstant.APPROVED;
                db.SaveChanges();

                try
                {
                    string msg = $"Your payout request for Transaction; OWNO-OR-{payment.TransactionId} has been approved. Your payout will now be processed. Another email will be sent when the payout has been sent.";
                    OwnorentHelper.SendEmail(payment.Transaction.Product.ProductTemplate.User.Email, payment.Transaction.Product.ProductTemplate.User.FirstName, msg);
                }
                catch (Exception e)
                {

                }

                TempData["Message"] = "<strong>Payout approved successfully.</strong> Seller will be notified.";
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Approve Payout failed.</strong> Payment ID does not exist in the DB.";
            }

            return RedirectToAction("Payouts");
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
                return RedirectToAction("Payouts");
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
                editPayment.ConfirmationNumber = payment.ConfirmationNumber;
                db.SaveChanges();
                TempData["Message"] = "<strong>Payment request edited successfully.</strong>.";
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Request payout failed.</strong> Payment ID does not exist in the DB.";
            }

            return RedirectToAction("Payouts");
        }

        public ActionResult ProductsRequestedPullOut()
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            ViewBag.LastUpdate = db.Settings.FirstOrDefault(s => s.Code == "UPDATE_PRICE_LAST_RAN").Value;

            return View("Products", db.ProductTemplates.Include(t=>t.Products).Where(t=>t.Products.Any(p=>p.ProductStatus == ProductStatusConstant.REQUESTED_REMOVAL)).ToList());
        }

        public ActionResult ManageStock(int id)
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            var products = db.Products.Where(p => p.ProductTemplateId == id).ToList();

            return View(products);
        }

        public ActionResult EditProductStock(int id)
        {
            Product editProduct = db.Products.FirstOrDefault(p => p.ProductId == id);

            if (editProduct != null)
            {
                return View(editProduct);
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Product access failed.</strong> Product ID does not exist in the DB.";
                return RedirectToAction("Products");
            }
        }

        public ActionResult ApprovePullOut(int id)
        {
            Product product = db.Products.FirstOrDefault(p => p.ProductId == id);

            if (product != null)
            {
                TempData["Message"] = "<strong>Product status updated successfully.</strong> PULL OUT has been approved. User will be notified for instructions.";
                product.ProductStatus = ProductStatusConstant.REMOVED;
                db.SaveChanges();

                try
                {
                    string msg = $"Your pull out request for product; {product.ProductName} has been approved. Please contact us for additional shipping/pickup instructions.";
                    OwnorentHelper.SendEmail(product.ProductTemplate.User.Email, product.ProductTemplate.User.FirstName, msg);
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Product update failed.</strong> Product ID does not exist in the DB.";
            }

            return RedirectToAction("ManageStock", new { id = product.ProductTemplateId });
        }

        [HttpPost]
        public ActionResult EditProductStock(Product product)
        {
            Product editProduct = db.Products.FirstOrDefault(p => p.ProductId == product.ProductId);

            if (editProduct != null)
            {
                TempData["Message"] = "<strong>Product updated successfully.</strong>";
                editProduct.ProductSerialNumber = product.ProductSerialNumber;
                db.SaveChanges();
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Product update failed.</strong> Product ID does not exist in the DB.";
            }

            return RedirectToAction("ManageStock", new { id = editProduct.ProductTemplateId });
        }

        public ActionResult UpdatePrices()
        {
            List<string> salvageProperties = new List<string>() { "COMPUTE_SALVAGE_VALUE_PERCENTAGE", "COMPUTE_SALVAGE_RENT_VALUE_PERCENTAGE", "COMPUTE_DAILY_RENT_PERCENTAGE" };
            var salvageSettings = db.Settings.Where(s => salvageProperties.Contains(s.Code)).ToList();
            var salvageValueSetting = salvageSettings.FirstOrDefault(s => s.Code == "COMPUTE_SALVAGE_VALUE_PERCENTAGE");
            var salvageRentValueSetting = salvageSettings.FirstOrDefault(s => s.Code == "COMPUTE_SALVAGE_RENT_VALUE_PERCENTAGE");
            var dailyRentPercentageSetting = salvageSettings.FirstOrDefault(s => s.Code == "COMPUTE_DAILY_RENT_PERCENTAGE");
            
            var products = db.ProductTemplates.Where(p => p.ProductTemplateStatus != ProductTemplateStatusConstant.REMOVED).ToList();

            float salvageValuePercent = float.Parse(salvageValueSetting.Value) / 100;
            float salvageRentValuePercent = float.Parse(salvageRentValueSetting.Value) / 100;
            float dailyRentPercentage = float.Parse(dailyRentPercentageSetting.Value) / 100;

            float salvageValue;
            float salvageRentValue;
            float annualDepreciation;
            float computedPrice;
            float computedDailyRentPrice;

            DateTime datePurchased;
            DateTime now = DateTime.UtcNow.AddHours(8);

            foreach (var p in products)
            {
                datePurchased = p.DatePurchased;

                var totalDays = Math.Round((now - datePurchased).TotalDays, 2);
                float totalYears = (float) totalDays / 365;

                salvageValue = (float)Math.Round((p.Price * salvageValuePercent), 2);
                salvageRentValue = (float)Math.Round((p.Price * salvageRentValuePercent), 2);

                annualDepreciation = (float)((p.Price - salvageValue) / p.Category.UsefulLifeSpan);
                computedPrice = (float) Math.Round(p.Price - (annualDepreciation * totalYears), 2);
                computedDailyRentPrice = (float) Math.Round((computedPrice * dailyRentPercentage), 2);

                p.ComputedPrice = (computedPrice < salvageValue) ? salvageValue : computedPrice;
                p.ComputedDailyRentPrice = (computedDailyRentPrice < salvageRentValue) ? salvageRentValue : computedDailyRentPrice;
                p.DateLastModified = now;
            }

            db.Settings.FirstOrDefault(s => s.Code == "UPDATE_PRICE_LAST_RAN").Value = now.ToString("MM-dd-yyyy hh:mmtt");

            db.SaveChanges();
            TempData["Message"] = "<strong>Update Price ran successfully.</strong> Computed and Computed Daily Rent price have been updated.";
            return RedirectToAction("Products");
        }

        public ActionResult Categories()
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            return View(db.Categories.ToList());
        }

        public ActionResult AddCategory()
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            return View();
        }

        [HttpPost]
        public ActionResult AddCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(new Category() { CategoryName = category.CategoryName, UsefulLifeSpan = category.UsefulLifeSpan });
                db.SaveChanges();
                TempData["Message"] = "<strong>Category has been added successfully.</strong>";
                return RedirectToAction("Categories");
            }
            else
            {
                return View(category);
            }
        }

        public ActionResult EditCategory(int id)
        {
            Category category = db.Categories.FirstOrDefault(c => c.CategoryId == id);

            if (category != null)
            {
                return View(category);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        public ActionResult EditCategory(Category category)
        {
            var editCategory = db.Categories.FirstOrDefault(c => c.CategoryId == category.CategoryId);

            if (category != null)
            {
                editCategory.CategoryName = category.CategoryName;
                editCategory.UsefulLifeSpan = category.UsefulLifeSpan;
                editCategory.DateLastModified = DateTime.UtcNow.AddHours(8);
                db.SaveChanges();

                TempData["Message"] = "<strong>Category updated successfully.</strong> New useful life will be used on recompute.";
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Category update failed.</strong> Category ID not found.";
            }

            return RedirectToAction("Categories");
        }

        public ActionResult DeleteCategory(int id)
        {
            Category category = db.Categories.FirstOrDefault(c => c.CategoryId == id);

            if (category != null)
            {
                if (category.ProductTemplates.Count > 0)
                {
                    TempData["Error"] = "1";
                    TempData["Message"] = "<strong>Category deletion failed.</strong> There are Products using this Category.";
                    return RedirectToAction("Categories");
                }
                else
                {
                    db.Categories.Remove(category);
                    db.SaveChanges();
                    TempData["Message"] = "<strong>Category deleted successfully.</strong>";
                    return RedirectToAction("Categories");
                }
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Category deletion failed.</strong> Category ID not found.";
                return RedirectToAction("Categories");
            }
        }

        public async Task<ActionResult> Orders()
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            var orders = db.TransactionGroups
                .Include(o => o.Transactions)
                .Where(o => !o.Transactions.Any(t => t.TransactionStatus == TransactionStatusConstant.PENDING));

            return View(await orders.ToListAsync());
        }

        public async Task<ActionResult> ManageOrder(int id)
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }
            
            var order = db.TransactionGroups.Include(t => t.Transactions).FirstOrDefault(t => t.TransactionGroupId == id);

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

        public async Task<ActionResult> EditOrders(List<TransactionEditModel> transactionItems)
        {
            int? transactionGroupId = null;
            TransactionGroup order = null;

            int packageCount = 0;
            int pickUpCount = 0;
            int transitCount = 0;
            int deliveredCount = 0;
            int failedCount = 0;

            foreach (var item in transactionItems)
            {
                var transaction = db.Transactions.FirstOrDefault(t => t.TransactionId == item.TransactionId);

                if(order == null)
                {
                    order = transaction.TransactionGroup;
                    transactionGroupId = transaction.TransactionGroupId;
                }

                if (transaction != null)
                {
                    transaction.ShippingStatus = item.ShippingStatus;

                    switch (item.ShippingStatus)
                    {
                        case ShippingStatusConstant.PACKAGED:
                            packageCount++;
                            break;
                        case ShippingStatusConstant.READY_FOR_PICK_UP:
                            pickUpCount++;
                            break;
                        case ShippingStatusConstant.IN_TRANSIT:
                            transitCount++;
                            break;
                        case ShippingStatusConstant.DELIVERED:
                            deliveredCount++;
                            break;
                        case ShippingStatusConstant.FAILED_DELIVERY:
                            failedCount++;
                            break;
                    }
                }
                else
                {
                    TempData["Error"] = "1";
                    TempData["Message"] = "<strong>Transaction update failed.</strong> A transaction item does not exist in the database.";
                    return RedirectToAction("Orders");
                }
            }

            await db.SaveChangesAsync();

            #region shipping status breakdown
            StringBuilder msg = new StringBuilder();
            msg.Append("Your order with ID: OWNO-OR-" + transactionGroupId + " has been updated.");
            if (packageCount>0)
            {
                msg.Append("Packaged " + packageCount+". ");
            }
            if (pickUpCount > 0)
            {
                msg.Append("READY FOR PICK UP " + pickUpCount + ". ");
            }
            if (transitCount > 0)
            {
                msg.Append("IN TRANSIT " + transitCount + ". ");
            }
            if (deliveredCount > 0)
            {
                msg.Append("DELIVERED " + deliveredCount + ". ");
            }
            if (failedCount > 0)
            {
                msg.Append("FAILED DELIVERY " + failedCount + ". ");
            }
            #endregion

            #region Send SMS to Customer
            try
            {
                Task.Run(() =>
                {
                    new SMSController().SendSMS(order.UserId, msg.ToString());
                });
            }
            catch (Exception e)
            {
                Trace.TraceInformation("SMS Send Failed for OWNO-OR-" + order.TransactionGroupId + ": " + e.Message);
            }
            #endregion

            TempData["Message"] = "<strong>Transaction updated successfully.</strong> The customer will be notified.";
            return RedirectToAction("ManageOrder", new { id = transactionGroupId });
        }

        public async Task<ActionResult> Products()
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            var productTemplates = db.ProductTemplates
                .Include(p => p.Category).Include(p=>p.Products);

            ViewBag.LastUpdate = db.Settings.FirstOrDefault(s => s.Code == "UPDATE_PRICE_LAST_RAN").Value;

            return View(await productTemplates.ToListAsync());
        }

        public ActionResult EditProduct(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductTemplate productTemplate = db.ProductTemplates.Find(id);
            if (productTemplate == null)
            {
                return HttpNotFound();
            }

            List<string> salvageProperties = new List<string>() { "COMPUTE_SALVAGE_VALUE_PERCENTAGE", "COMPUTE_SALVAGE_RENT_VALUE_PERCENTAGE", "COMPUTE_DAILY_RENT_PERCENTAGE" };
            var salvageSettings = db.Settings.Where(s => salvageProperties.Contains(s.Code)).ToList();
            var salvageValueSetting = salvageSettings.FirstOrDefault(s => s.Code == "COMPUTE_SALVAGE_VALUE_PERCENTAGE");
            var salvageRentValueSetting = salvageSettings.FirstOrDefault(s => s.Code == "COMPUTE_SALVAGE_RENT_VALUE_PERCENTAGE");
            var dailyRentPercentageSetting = salvageSettings.FirstOrDefault(s => s.Code == "COMPUTE_DAILY_RENT_PERCENTAGE");
            ViewBag.salvageValue = (salvageValueSetting != null) ? salvageValueSetting.Value : "30";
            ViewBag.salvageRentValue = (salvageRentValueSetting != null) ? salvageRentValueSetting.Value : "0.09";
            ViewBag.dailyRentPercentage = (dailyRentPercentageSetting != null) ? dailyRentPercentageSetting.Value : "0.2";

            ViewBag.WarehouseId = new SelectList(db.Warehouses, "WarehouseId", "WarehouseName");
            ViewBag.CategoriesList = db.Categories.ToList();

            return View(productTemplate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct([Bind(Include = "InvoiceDescription,ProductTemplateId,UserId,DateCreated,ProductTemplateStatus,TrackingNumber,ProductName,ProductDescription,ProductPriceToUse,Quantity,Price,DailyRentPrice,ComputedPrice,ComputedDailyRentPrice,AdminDefinedPrice,AdminDefinedDailyRentPrice,ShippingFee,ShippingFeeProvincial,DatePurchased,CategoryId,WarehouseId")] ProductTemplate productTemplate)
        {
            var actualTemplate = db.ProductTemplates.FirstOrDefault(p => p.ProductTemplateId == productTemplate.ProductTemplateId);

            actualTemplate.ProductName = productTemplate.ProductName;
            actualTemplate.ProductDescription = productTemplate.ProductDescription;
            actualTemplate.Price = productTemplate.Price;
            actualTemplate.DailyRentPrice = productTemplate.DailyRentPrice;
            actualTemplate.ComputedPrice = productTemplate.ComputedPrice;
            actualTemplate.ComputedDailyRentPrice = productTemplate.ComputedDailyRentPrice;
            actualTemplate.CategoryId = productTemplate.CategoryId;
            actualTemplate.TrackingNumber = productTemplate.TrackingNumber;
            actualTemplate.WarehouseId = productTemplate.WarehouseId;
            actualTemplate.Quantity = productTemplate.Quantity;
            actualTemplate.DatePurchased = productTemplate.DatePurchased;
            actualTemplate.LastModifiedBy = User.Identity.Name;
            actualTemplate.DateLastModified = DateTime.UtcNow.AddHours(8);
            actualTemplate.InvoiceDescription = productTemplate.InvoiceDescription;

            actualTemplate.ShippingFee = productTemplate.ShippingFee;
            actualTemplate.ShippingFeeProvincial = productTemplate.ShippingFeeProvincial;
            actualTemplate.ProductPriceToUse = productTemplate.ProductPriceToUse;

            if (ModelState.IsValid)
            {
                db.Entry(actualTemplate).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Products");
            }

            List<string> salvageProperties = new List<string>() { "COMPUTE_SALVAGE_VALUE_PERCENTAGE", "COMPUTE_SALVAGE_RENT_VALUE_PERCENTAGE", "COMPUTE_DAILY_RENT_PERCENTAGE" };
            var salvageSettings = db.Settings.Where(s => salvageProperties.Contains(s.Code)).ToList();
            var salvageValueSetting = salvageSettings.FirstOrDefault(s => s.Code == "COMPUTE_SALVAGE_VALUE_PERCENTAGE");
            var salvageRentValueSetting = salvageSettings.FirstOrDefault(s => s.Code == "COMPUTE_SALVAGE_RENT_VALUE_PERCENTAGE");
            var dailyRentPercentageSetting = salvageSettings.FirstOrDefault(s => s.Code == "COMPUTE_DAILY_RENT_PERCENTAGE");
            ViewBag.salvageValue = (salvageValueSetting != null) ? salvageValueSetting.Value : "30";
            ViewBag.salvageRentValue = (salvageRentValueSetting != null) ? salvageRentValueSetting.Value : "0.09";
            ViewBag.dailyRentPercentage = (dailyRentPercentageSetting != null) ? dailyRentPercentageSetting.Value : "0.2";

            ViewBag.WarehouseId = new SelectList(db.Warehouses, "WarehouseId", "WarehouseName");
            ViewBag.CategoriesList = db.Categories.ToList();

            return View(productTemplate);
        }

        public ActionResult Images(int id)
        {
            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"];
            }
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }

            var productTemplate = db.ProductTemplates.Include(p => p.Attachment).FirstOrDefault(p => p.ProductTemplateId == id);

            if (productTemplate == null)
            {
                return HttpNotFound();
            }

            return View(productTemplate);
        }

        [HttpPost]
        public ActionResult Images(string name, int productId, List<HttpPostedFileBase> files)
        {
            if (ModelState.IsValid)
            {
                if (files.Count > 10)
                {
                    TempData["Error"] = "1";
                    TempData["Message"] = "<strong>Failed to save files</strong>, the number of files should not exceed 10.";
                    return RedirectToAction("Images", new { id = productId });
                }

                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        if (file.ContentLength > 5000000)
                        {
                            TempData["Error"] = "1";
                            TempData["Message"] = "<strong>Failed to save files</strong>, one file is more than 5MB.";
                            return RedirectToAction("Images", new { id = productId });
                        }
                    }
                }

                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        if (file.ContentLength <= 5000000)
                        {
                            // extract only the filename
                            string fileName = Path.GetFileName(file.FileName);
                            string fileExt = Path.GetExtension(file.FileName);
                            string newName = OwnorentHelper.RandomString(24) + fileExt;

                            string targetPath = Server.MapPath("~/Content/Uploads/Images/");

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
                                return RedirectToAction("Images", new { id = productId });
                            }

                            db.ProductTemplateAttachments.Add(new ProductTemplateAttachment()
                            {
                                Location = newName,
                                Name = name,
                                ProductTemplateId = productId
                            });
                        }
                    }
                }
            }

            db.SaveChanges();
            return RedirectToAction("Images", new { id = productId });
        }

        public ActionResult DeleteImage(int id, int productId)
        {
            var image = db.ProductTemplateAttachments.FirstOrDefault(p => p.ProductTemplateAttachmentId == id);

            if (image != null)
            {
                var toDelete = Server.MapPath("~/Content/Uploads/Images/" + image.Location);

                if (System.IO.File.Exists(toDelete))
                {
                    System.IO.File.Delete(toDelete);
                }

                TempData["Message"] = "<strong>Image deletion successful.</strong>.";
                db.ProductTemplateAttachments.Remove(image);
                db.SaveChanges();
                return RedirectToAction("Images", new { id = productId });
            }

            TempData["Error"] = "1";
            TempData["Message"] = "<strong>Image deletion failed.</strong> Image does not exist in the database.";
            return RedirectToAction("Images", new { id = productId });
        }

        public ActionResult Approve(int id)
        {
            var productTemplate = db.ProductTemplates.Include(p => p.Attachment).Include(p=>p.Products).FirstOrDefault(p => p.ProductTemplateId == id);

            if (productTemplate == null)
            {
                return HttpNotFound();
            }

            // send email job
            SendApproveEmailJob(productTemplate.ProductName, productTemplate.User.Email, productTemplate.User.FirstName);

            productTemplate.ProductTemplateStatus = ProductTemplateStatusConstant.APPROVED;

            if (productTemplate.Quantity > 0)
            {
                if (productTemplate.Products.Count > 0)
                {
                    // this product has been approved before
                }
                else
                {
                    for (int count = 0; count < productTemplate.Quantity; count++)
                    {
                        db.Products.Add(new Product
                        {
                            ProductName = productTemplate.ProductName,
                            ProductDescription = productTemplate.ProductDescription,
                            ProductStatus = ProductStatusConstant.AVAILABLE,
                            ProductTemplateId = productTemplate.ProductTemplateId
                        });
                    }
                }
            }

            db.SaveChanges();

            TempData["Message"] = "<strong>Product status updated successfully.</strong> User has been notified.";
            return RedirectToAction("Products");
        }

        public void SendApproveEmailJob(string productName, string email, string firstName)
        {
            Task.Run(() =>
            {
                string body = "Congratulations! Your product, " + productName + " , has been approved and will now be publicly listed. An email will be sent whenever a customer orders for your product.";

                MailAddress fromAddress = new MailAddress("ownorent@gmail.com", "Ownorent Registration Service");
                MailAddress toAddress = new MailAddress(email, firstName);
                string fromPassword = "ownorent$123456";
                string subject = "Ownorent Product Application - " + DateTime.UtcNow.AddHours(8).ToString("MM-dd-yy");

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
                message.CC.Add(new MailAddress(email));

                smtp.Send(message);
            });
        }

        public ActionResult RevertToReview(int id)
        {
            var productTemplate = db.ProductTemplates.Include(p => p.Attachment).FirstOrDefault(p => p.ProductTemplateId == id);

            if (productTemplate == null)
            {
                return HttpNotFound();
            }

            #region Email
            string body = "Your product, " + productTemplate.ProductName + " has been reverted back to Pending Review. An Admin will contact you shall there be any clarifications. Another email will be sent once an Admin has finished reviewing your product.";

            MailAddress fromAddress = new MailAddress("ownorent@gmail.com", "Ownorent Registration Service");
            MailAddress toAddress = new MailAddress(productTemplate.User.Email, productTemplate.User.FirstName);
            string fromPassword = "ownorent$123456";
            string subject = "Ownorent Product Application - " + DateTime.UtcNow.AddHours(8).ToString("MM-dd-yy");

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
            message.CC.Add(new MailAddress(productTemplate.User.Email));
            #endregion

            try
            {
                smtp.Send(message);
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }

            productTemplate.ProductTemplateStatus = ProductTemplateStatusConstant.PENDING_REVIEW;
            db.SaveChanges();

            TempData["Message"] = "<strong>Product status updated successfully.</strong> User has been notified.";
            return RedirectToAction("Products");
        }

        public ActionResult ConfirmArrival(int id)
        {
            var productTemplate = db.ProductTemplates.Include(p => p.Attachment).FirstOrDefault(p => p.ProductTemplateId == id);

            if (productTemplate == null)
            {
                return HttpNotFound();
            }

            #region Email
            string body = "We have confirmed the arrival of your product, "+productTemplate.ProductName+" , in our "+productTemplate.Warehouse.WarehouseName+" Warehouse. Another email will be sent once an Admin has finished reviewing your product.";

            MailAddress fromAddress = new MailAddress("ownorent@gmail.com", "Ownorent Registration Service");
            MailAddress toAddress = new MailAddress(productTemplate.User.Email, productTemplate.User.FirstName);
            string fromPassword = "ownorent$123456";
            string subject = "Ownorent Product Application - " + DateTime.UtcNow.AddHours(8).ToString("MM-dd-yy");

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
            message.CC.Add(new MailAddress(productTemplate.User.Email));
            #endregion

            try
            {
                smtp.Send(message);
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }

            productTemplate.ProductTemplateStatus = ProductTemplateStatusConstant.PENDING_REVIEW;
            db.SaveChanges();

            TempData["Message"] = "<strong>Product status updated successfully.</strong> User has been notified.";
            return RedirectToAction("Products");
        }

        public ActionResult RejectRevise(int id)
        {
            var productTemplate = db.ProductTemplates.Include(p => p.Attachment).FirstOrDefault(p => p.ProductTemplateId == id);

            if (productTemplate == null)
            {
                return HttpNotFound();
            }

            #region Email
            string body = "Your product, " + productTemplate.ProductName + ", needs updating. Please provide more information/pictures for your product";

            MailAddress fromAddress = new MailAddress("ownorent@gmail.com", "Ownorent Registration Service");
            MailAddress toAddress = new MailAddress(productTemplate.User.Email, productTemplate.User.FirstName);
            string fromPassword = "ownorent$123456";
            string subject = "Ownorent Product Application - " + DateTime.UtcNow.AddHours(8).ToString("MM-dd-yy");

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
            message.CC.Add(new MailAddress(productTemplate.User.Email));
            #endregion

            try
            {
                smtp.Send(message);
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }

            productTemplate.ProductTemplateStatus = ProductTemplateStatusConstant.REJECTED_REVISE;
            db.SaveChanges();

            TempData["Message"] = "<strong>Product status updated successfully.</strong> User has been notified.";
            return RedirectToAction("Products");
        }

        public ActionResult RejectNotAllowed(int id)
        {
            var productTemplate = db.ProductTemplates.Include(p => p.Attachment).FirstOrDefault(p => p.ProductTemplateId == id);

            if (productTemplate == null)
            {
                return HttpNotFound();
            }

            #region Email
            string body = "Your product, " + productTemplate.ProductName + " , has been rejected. Please visit this <a href='#'>page</a> for the list of prohibited items. Please contact us to arrange shipping out of the warehouse.";

            MailAddress fromAddress = new MailAddress("ownorent@gmail.com", "Ownorent Registration Service");
            MailAddress toAddress = new MailAddress(productTemplate.User.Email, productTemplate.User.FirstName);
            string fromPassword = "ownorent$123456";
            string subject = "Ownorent Product Application - " + DateTime.UtcNow.AddHours(8).ToString("MM-dd-yy");

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
            message.CC.Add(new MailAddress(productTemplate.User.Email));
            #endregion

            try
            {
                smtp.Send(message);
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }

            productTemplate.ProductTemplateStatus = ProductTemplateStatusConstant.REJECTED_NOT_ALLOWED;
            db.SaveChanges();

            TempData["Message"] = "<strong>Product status updated successfully.</strong> User has been notified.";
            return RedirectToAction("Products");
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductTemplate productTemplate = db.ProductTemplates.Find(id);
            if (productTemplate == null)
            {
                return HttpNotFound();
            }
            return View(productTemplate);
        }

        // GET: Admin/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductTemplate productTemplate = db.ProductTemplates.Find(id);
            if (productTemplate == null)
            {
                return HttpNotFound();
            }
            return View(productTemplate);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductTemplate productTemplate = db.ProductTemplates.Find(id);
            db.ProductTemplates.Remove(productTemplate);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
