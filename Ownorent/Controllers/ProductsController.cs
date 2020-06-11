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
using System.Threading.Tasks;
using System.IO;
using System.Web.Helpers;

namespace Ownorent.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public async Task<ActionResult> p(string search, int? page, int? category)
        {
            IQueryable<ProductTemplate> products;
                        
            if(search != null) {
                products = db.ProductTemplates
                .Include(p => p.Category).Include(p => p.Attachment).Include(p=>p.Products)
                .Where(p => p.ProductTemplateStatus == ProductTemplateStatusConstant.APPROVED
                    && (p.ProductName.Contains(search) || p.ProductDescription.Contains(search)));
            } else {
                products = db.ProductTemplates
                .Include(p => p.Category).Include(p => p.Attachment).Include(p => p.Products)
                .Where(p => p.ProductTemplateStatus == ProductTemplateStatusConstant.APPROVED);
            }

            string userId = User.Identity.GetUserId();
            ViewBag.CategoriesList = await db.Categories.ToListAsync();
            ViewBag.Cart = await db.Carts.CountAsync(c => c.UserId == userId);
            

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                ViewBag.UserStatus = user.AccountStatus;
            }
            else
            {
                ViewBag.UserStatus = -1;
            }

            if (category != null) {
                return View(await products.Where(p => p.CategoryId == category).ToListAsync());
            } else {
                return View(await products.ToListAsync());
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddToCart(int id, byte type)
        {
            var product = db.ProductTemplates.FirstOrDefault(p => p.ProductTemplateId == id);

            if (product != null) {

                string userId = User.Identity.GetUserId();
                var cartItemExist = db.Carts.FirstOrDefault(c=>c.ProductTemplateId == id && c.UserId == userId && c.CartType == type);

                if (cartItemExist != null) {
                    return Json(new { status = 0, message = "<strong>Failed to add Product.</strong> Product already exists in your cart." }, JsonRequestBehavior.AllowGet);
                } else {

                    switch (type)
                    {
                        case CartTypeConstant.BUY:
                            db.Carts.Add(new Cart()
                            {
                                CartType = type,
                                ProductTemplateId = id,
                                UserId = userId,
                                Quantity = 1
                            });
                            break;
                        case CartTypeConstant.RENT:
                            db.Carts.Add(new Cart()
                            {
                                CartType = type,
                                ProductTemplateId = id,
                                UserId = userId,
                                Quantity = 1,
                                RentDateStart = DateTime.UtcNow.AddHours(8).AddDays(2),
                                RentDateEnd = DateTime.UtcNow.AddHours(8).AddDays(2),
                                RentNumberOfDays = 1
                            });
                            break;
                        case CartTypeConstant.RENT_TO_OWN:
                            RentToOwnPaymentTerm defaultPaymentTerm = db.RentToOwnPaymentTerms.FirstOrDefault(r => r.Months == 3);
                            if (defaultPaymentTerm == null)
                            {
                                return Json(new { status = 0, message = "<strong>Failed to add Product.</strong> Default rent to own payment term cannot be found." }, JsonRequestBehavior.AllowGet);
                            }

                            db.Carts.Add(new Cart()
                            {
                                CartType = type,
                                ProductTemplateId = id,
                                UserId = userId,
                                Quantity = 1,
                                RentToOwnPaymentTermId = defaultPaymentTerm.RentToOwnPaymentTermId
                            });
                            break;
                    }
                    
                    try
                    {
                        db.SaveChanges();
                        return Json(new { status = 1, message = "<strong>Success!</strong> Product has been added to cart." }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {
                        return Json(new { status = 0, message = $"<strong>Failed to add Product.</strong> {e.Message}" }, JsonRequestBehavior.AllowGet);
                    }
                    
                }

            } else {
                return Json(new { status = 0, message = "<strong>Failed to add Product.</strong> Product does not exist." }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> Cart()
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
            var cartItems = await db.Carts.Include(c=>c.Product).Include(c=>c.PaymentTerm).Where(c => c.UserId == userId).ToListAsync();

            List<CartValidateModel> cartValidateItems = new List<CartValidateModel>();

            var grouped = cartItems.GroupBy(c => c.ProductTemplateId);
            foreach(var item in grouped)
            {
                int available = db.Products.Count(p => p.ProductTemplateId == item.Key && p.ProductStatus == ProductStatusConstant.AVAILABLE);
                var getName = item.FirstOrDefault();
                string productName = "";

                if (getName != null)
                {
                    productName = getName.Product.ProductName;
                }

                cartValidateItems.Add(new CartValidateModel
                {
                    ProductTemplateId = item.Key,
                    ProductName = productName,
                    QuantityNeeded = item.Sum(i => i.Quantity),
                    QuantityAvailable = available
                });
            }

            ViewBag.cartValidationItems = cartValidateItems;
            ViewBag.rentToOwnPaymentTerms = db.RentToOwnPaymentTerms.ToList();

            return View(cartItems);
        }

        [HttpPost]
        public async Task<ActionResult> Cart(List<Cart> cartItems)
        {
            if (cartItems == null)
            {
                return RedirectToAction("Cart");
            }

            string userId = User.Identity.GetUserId();

            foreach (var item in cartItems)
            {
                var cartItem = db.Carts.Include(c => c.Product).Include(c => c.PaymentTerm).FirstOrDefault(c => c.CartId == item.CartId && c.UserId == userId);

                if (cartItem != null)
                {
                    if (item.Quantity == 0)
                    {
                        db.Carts.Remove(cartItem);
                    }
                    else
                    {
                        cartItem.Quantity = item.Quantity;

                        switch (cartItem.CartType)
                        {
                            case CartTypeConstant.BUY:
                                break;
                            case CartTypeConstant.RENT:
                                cartItem.RentDateStart = item.RentDateStart;
                                cartItem.RentDateEnd = item.RentDateEnd;
                                cartItem.RentNumberOfDays = ((item.RentDateEnd - item.RentDateStart).Value.Days)+1;
                                break;
                            case CartTypeConstant.RENT_TO_OWN:
                                cartItem.RentToOwnPaymentTermId = item.RentToOwnPaymentTermId;
                                break;
                        }
                    }
                }
            }

            await db.SaveChangesAsync();

            TempData["Message"] = "<strong>Cart updated successfully.</strong> Please review changes.";

            return RedirectToAction("Cart");
        }

        public ActionResult RemoveFromCart(int id)
        {
            string userId = User.Identity.GetUserId();
            var cartItem = db.Carts.FirstOrDefault(c => c.CartId == id && c.UserId == userId);

            if (cartItem != null)
            {
                db.Carts.Remove(cartItem);
                db.SaveChanges();
                TempData["Message"] = "<strong>Product removed successfully.</strong>";
            }
            else
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Failed to remove product from cart.</strong> The Product requested to be removed does not exist in your cart.";
            }

            return RedirectToAction("Cart");
        }

        public async Task<ActionResult> Checkout()
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

            if (userId == null)
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Checkout failed.</strong> Please login.";
                return RedirectToAction("Cart");
            }

            var cartItems = await db.Carts.Include(c => c.Product).Include(c => c.PaymentTerm).Where(c => c.UserId == userId).ToListAsync();

            #region validate cart items
            if (cartItems.Count == 0)
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Checkout failed.</strong> No items in cart.";
                return RedirectToAction("Cart");
            }

            List<CartValidateModel> cartValidateItems = new List<CartValidateModel>();

            var grouped = cartItems.GroupBy(c => c.ProductTemplateId);
            foreach (var item in grouped)
            {
                int available = db.Products.Count(p => p.ProductTemplateId == item.Key && p.ProductStatus == ProductStatusConstant.AVAILABLE);
                var getName = item.FirstOrDefault();
                string productName = "";

                if (getName != null)
                {
                    productName = getName.Product.ProductName;
                }

                cartValidateItems.Add(new CartValidateModel
                {
                    ProductTemplateId = item.Key,
                    ProductName = productName,
                    QuantityNeeded = item.Sum(i => i.Quantity),
                    QuantityAvailable = available
                });
            }

            if (cartValidateItems.Any(c=>c.Error))
            {
                ViewBag.cartValidationItems = cartValidateItems;
                ViewBag.rentToOwnPaymentTerms = db.RentToOwnPaymentTerms.ToList();
                return View("Cart",cartItems);
            }
            #endregion

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            #region validate user
            if (user.Addresses.Count == 0)
            {
                TempData["Error"] = "1";
                TempData["Message"] = "<strong>Checkout failed.</strong> No User address found.";
                return RedirectToAction("Cart");
            }
            #endregion

            TransactionGroup tg = new TransactionGroup() {
                UserId = userId
            };

            db.TransactionGroups.Add(tg);

            Address addressToUse = user.Addresses.FirstOrDefault(a => a.IsDefault);

            foreach (var item in cartItems)
            {
                Product availableProduct = db.Products.FirstOrDefault(p => p.ProductTemplateId == item.ProductTemplateId
                    && p.ProductStatus == ProductStatusConstant.AVAILABLE);

                #region validate product availability
                if (availableProduct == null)
                {
                    TempData["Error"] = "1";
                    TempData["Message"] = "<strong>Checkout failed.</strong> No stock available for "+item.Product.ProductName+".";
                    return RedirectToAction("Cart");
                }
                #endregion

                Transaction transactionPerProduct = new Transaction() {
                    TransactionGroupId = tg.TransactionGroupId,
                    AddressId = addressToUse.AddressId,
                    City = addressToUse.City,
                    Country = addressToUse.Country,
                    Line1 = addressToUse.Line1,
                    Line2 = addressToUse.Line2,
                    Line3 = addressToUse.Line3,
                    Zip = addressToUse.Zip,
                    ProductId = availableProduct.ProductId,
                    TransactionStatus = TransactionStatusConstant.PENDING
                };

                #region determine price
                if (item.Product.ProductPriceToUse == ProductPriceToUseConstant.SELLER_DEFINED_PRICE)
                {
                    transactionPerProduct.ProductPrice = item.Product.Price;
                    transactionPerProduct.ProductDailyRentPrice = (item.Product.DailyRentPrice != null) ? item.Product.DailyRentPrice : item.Product.ComputedDailyRentPrice;

                    if (item.Product.ComputedDailyRentPrice == null)
                    {
                        transactionPerProduct.ProductDailyRentPrice = 0;
                    }
                }
                else
                {
                    transactionPerProduct.ProductPrice = (item.Product.ComputedPrice != null) ? item.Product.ComputedPrice.Value : item.Product.Price;
                    transactionPerProduct.ProductDailyRentPrice = (item.Product.ComputedDailyRentPrice != null) ? item.Product.ComputedPrice.Value : item.Product.DailyRentPrice;

                    if (item.Product.DailyRentPrice == null)
                    {
                        transactionPerProduct.ProductDailyRentPrice = 0;
                    }
                }
                #endregion
            }

            return View("Cart", cartItems);
        }

        public ActionResult Index()
        {
            ViewBag.Error = TempData["Error"];
            ViewBag.Message = TempData["Message"];

            string userId = User.Identity.GetUserId();
            var productTemplates = db.ProductTemplates
                .Include(p => p.Category)
                .Where(p => p.UserId == userId);
                // .GroupBy(p=>p.Category.CategoryName);

            return View(productTemplates.ToList());
        }

        public async Task<ActionResult> Index2()
        {
            string userId = User.Identity.GetUserId();
            var productTemplates = await db.ProductTemplates
                .Include(p => p.Category)
                .Where(p => p.UserId == userId).ToListAsync();
                //.GroupBy(p => p.Category.CategoryName).ToListAsync();

            return View(productTemplates);
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

        public ActionResult Create()
        {
            string userId = User.Identity.GetUserId();
            if (db.Users.FirstOrDefault(u=>u.Id == userId).AccountStatus != AccountStatusConstant.APPROVED)
            {
                TempData["Error"] = "1";
                TempData["Message"] = "Your account is not authorized to add products yet. Please go through review process.";
                return RedirectToAction("Requirements", "Account", new { });
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductTemplateId,ProductName,TrackingNumber,ProductDescription,ProductTemplateStatus,Quantity,Price,DailyRentPrice,ComputedPrice,ComputedDailyRentPrice,DatePurchased,CategoryId,WarehouseId")] ProductTemplate productTemplate)
        {
            productTemplate.UserId = User.Identity.GetUserId();
            productTemplate.DateCreated = DateTime.UtcNow.AddHours(8);
            productTemplate.DateLastModified = DateTime.UtcNow.AddHours(8);
            productTemplate.ProductTemplateStatus = ProductTemplateStatusConstant.PENDING_WAREHOUSE_ARRIVAL;

            if (ModelState.IsValid)
            {
                db.ProductTemplates.Add(productTemplate);
                db.SaveChanges();
                TempData["Message"] = "<strong>Product added successfully.</strong> To expedite the approval process, upload images of the receipt/proof of purchase and the actual product.";
                return RedirectToAction("Images", new { id = productTemplate.ProductTemplateId });
            }

            ViewBag.WarehouseId = new SelectList(db.Warehouses, "WarehouseId", "WarehouseName");
            ViewBag.CategoriesList = db.Categories.ToList();
            return View(productTemplate);
        }

        public ActionResult Cancel(int id)
        {
            var productTemplate = db.ProductTemplates.Include(p => p.Attachment).FirstOrDefault(p => p.ProductTemplateId == id);

            if (productTemplate == null)
            {
                return HttpNotFound();
            }

            if (productTemplate.ProductTemplateStatus == ProductTemplateStatusConstant.PENDING_WAREHOUSE_ARRIVAL)
            {
                productTemplate.ProductTemplateStatus = ProductTemplateStatusConstant.REMOVED;
                TempData["Message"] = "<strong>Product Application cancelled successfully.</strong> If your product is in-freight during cancellation, please contact us.";
            }
            else if(productTemplate.ProductTemplateStatus == ProductTemplateStatusConstant.PENDING_REVIEW)
            {
                productTemplate.ProductTemplateStatus = ProductTemplateStatusConstant.REMOVED;
                TempData["Message"] = "<strong>Product Application cancelled successfully.</strong> Please contact us to arrange shipping out of the warehouse.";
            }
            else
            {

            }

            db.SaveChanges();
            return RedirectToAction("Index");
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

            var productTemplate = db.ProductTemplates.Include(p=>p.Attachment).FirstOrDefault(p => p.ProductTemplateId == id);

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

            if (image !=null)
            {
                var toDelete = Server.MapPath("~/Content/Uploads/Images/"+image.Location);

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

        public ActionResult Edit(int? id)
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
        public ActionResult Edit([Bind(Include = "ProductTemplateId,UserId,DateCreated,ProductTemplateStatus,TrackingNumber,ProductName,ProductDescription,ProductPriceToUse,Quantity,Price,DailyRentPrice,ComputedPrice,ComputedDailyRentPrice,AdminDefinedPrice,AdminDefinedDailyRentPrice,ShippingFee,ShippingFeeProvincial,DatePurchased,CategoryId,WarehouseId")] ProductTemplate productTemplate)
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

            if (ModelState.IsValid)
            {
                db.Entry(actualTemplate).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
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

        // GET: Products/Delete/5
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

        // POST: Products/Delete/5
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
