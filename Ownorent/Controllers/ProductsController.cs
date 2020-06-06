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

namespace Ownorent.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Products
        public ActionResult Index()
        {
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

        // GET: Products/Details/5
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

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", productTemplate.CategoryId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", productTemplate.UserId);
            ViewBag.WarehouseId = new SelectList(db.Warehouses, "WarehouseId", "WarehouseName", productTemplate.WarehouseId);
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
