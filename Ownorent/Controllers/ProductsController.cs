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

        // GET: Products/Create
        public ActionResult Create()
        {
            List<string> salvageProperties = new List<string>() { "COMPUTE_SALVAGE_VALUE_PERCENTAGE", "COMPUTE_SALVAGE_RENT_VALUE_PERCENTAGE" };
            var salvageSettings = db.Settings.Where(s => salvageProperties.Contains(s.Code)).ToList();
            var salvageRentValueSetting = salvageSettings.FirstOrDefault(s => s.Code == "COMPUTE_SALVAGE_VALUE_PERCENTAGE");
            var salvageValueSetting = salvageSettings.FirstOrDefault(s => s.Code == "COMPUTE_SALVAGE_RENT_VALUE_PERCENTAGE");
            ViewBag.salvageValue = (salvageRentValueSetting != null) ? salvageRentValueSetting.Value : "30";
            ViewBag.salvageRentValue = (salvageRentValueSetting != null) ? salvageRentValueSetting.Value : "0.09";

            ViewBag.WarehouseId = new SelectList(db.Warehouses, "WarehouseId", "WarehouseName");
            ViewBag.CategoriesList = db.Categories.ToList();
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductTemplateId,ProductName,ProductDescription,ProductTemplateStatus,Quantity,Price,DailyRentPrice,ComputedPrice,ComputedDailyRentPrice,DatePurchased,CategoryId,WarehouseId")] ProductTemplate productTemplate)
        {
            productTemplate.DateCreated = DateTime.UtcNow.AddHours(8);
            productTemplate.DateLastModified = DateTime.UtcNow.AddHours(8);
            productTemplate.ProductTemplateStatus = ProductTemplateStatusConstant.PENDING_REVIEW;

            if (ModelState.IsValid)
            {
                db.ProductTemplates.Add(productTemplate);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.WarehouseId = new SelectList(db.Warehouses, "WarehouseId", "WarehouseName");
            ViewBag.CategoriesList = db.Categories.ToList();
            return View(productTemplate);
        }

        public ActionResult ProductPhotos(int id)
        {
            var productTemplate = db.ProductTemplates.Include(p=>p.Attachment).FirstOrDefault(p => p.ProductTemplateId == id);

            if (productTemplate == null)
            {
                return HttpNotFound();
            }

            return View(productTemplate);
        }

        // GET: Products/Edit/5
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
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", productTemplate.CategoryId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", productTemplate.UserId);
            ViewBag.WarehouseId = new SelectList(db.Warehouses, "WarehouseId", "WarehouseName", productTemplate.WarehouseId);
            return View(productTemplate);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductTemplateId,ProductName,ProductDescription,ProductTemplateStatus,ProductPriceToUse,Quantity,Price,DailyRentPrice,ComputedPrice,ComputedDailyRentPrice,AdminDefinedPrice,AdminDefinedDailyRentPrice,ShippingFee,ShippingFeeProvincial,DatePurchased,LastModifiedBy,DateCreated,DateLastModified,CategoryId,WarehouseId,UserId")] ProductTemplate productTemplate)
        {
            if (ModelState.IsValid)
            {
                db.Entry(productTemplate).State = EntityState.Modified;
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
