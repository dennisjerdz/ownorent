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

namespace Ownorent.Controllers
{
    [Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Products()
        {
            ViewBag.Error = TempData["Error"];
            ViewBag.Message = TempData["Message"];

            var productTemplates = db.ProductTemplates
                .Include(p => p.Category);

            return View(productTemplates.ToList());
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
        public ActionResult EditProduct([Bind(Include = "ProductTemplateId,UserId,DateCreated,ProductTemplateStatus,TrackingNumber,ProductName,ProductDescription,ProductPriceToUse,Quantity,Price,DailyRentPrice,ComputedPrice,ComputedDailyRentPrice,AdminDefinedPrice,AdminDefinedDailyRentPrice,ShippingFee,ShippingFeeProvincial,DatePurchased,CategoryId,WarehouseId")] ProductTemplate productTemplate)
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
