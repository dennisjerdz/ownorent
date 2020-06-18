using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ownorent.Models
{
    // Straight Line Depreciation
    // https://bench.co/blog/accounting/straight-line-depreciation/

    public class Warehouse {
        public Warehouse() {
            DateCreated = DateTime.UtcNow.AddHours(8);
        }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string Location { get; set; }
        public bool IsDefault { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class UserAttachment {
        public UserAttachment() {
            DateCreated = DateTime.UtcNow.AddHours(8);
        }

        public int UserAttachmentId { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public string Location { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class Address {
        public Address() {
            DateCreated = DateTime.UtcNow.AddHours(8);
            DateLastModified = DateTime.UtcNow.AddHours(8);
        }

        public int AddressId { get; set; }
        public byte AddressType { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public bool IsDefault { get; set; }

        public string LastModifiedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateLastModified { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }

    public class Category {
        public Category() {
            DateCreated = DateTime.UtcNow.AddHours(8);
            DateLastModified = DateTime.UtcNow.AddHours(8);
        }

        public int CategoryId { get; set; }

        [Required]
        public string CategoryName { get; set; }
        public float UsefulLifeSpan { get; set; }

        public string LastModifiedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateLastModified { get; set; }

        public virtual List<ProductTemplate> ProductTemplates { get; set; }
    }

    public class ProductTemplate {

        /* Product Approval Process:
         *  User Creates a product
         *  Product upon creation will be "Pending Warehouse Arrival"
         *  User ships product to the warehouse and fill out Tracking Number
         *  Once product arrives at warehouse, Admin will change status to Pending Review and an Email will be sent to the user
         *  Once the product has been reviewed and approved
         *  Product will now be listed
        */

        public ProductTemplate() {
            DateCreated = DateTime.UtcNow.AddHours(8);
            DateLastModified = DateTime.UtcNow.AddHours(8);
        }

        public int ProductTemplateId { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string ProductName { get; set; }
        [Required]
        [Display(Name = "Description")]
        [AllowHtml]
        public string ProductDescription { get; set; }
        [Display(Name = "Invoice Description")]
        public string InvoiceDescription { get; set; }

        [Display(Name = "Status")]
        public byte ProductTemplateStatus { get; set; }
        public byte ProductPriceToUse { get; set; }

        [Display(Name = "Tracking Number")]
        public string TrackingNumber { get; set; }

        public int Quantity { get; set; }
        public float Price { get; set; }
        public float? DailyRentPrice { get; set; }
        public float? ComputedPrice { get; set; } // use setting percentage, based on category, straight line depreciation
        public float? ComputedDailyRentPrice { get; set; } // use setting percentage, 0.2% daily. Sample: 20k*0.2%=40php, 40*30days=1200, 20k/12months=1666.67
        public float? AdminDefinedPrice { get; set; }
        public float? AdminDefinedDailyRentPrice { get; set; }

        public float ShippingFee { get; set; }
        public float ShippingFeeProvincial { get; set; }

        [Required]
        [Display(Name = "Date Purchased")]
        public DateTime DatePurchased { get; set; }

        [Display(Name = "Last Modified By")]
        public string LastModifiedBy { get; set; }
        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }
        [Display(Name = "Last Modified Date")]
        public DateTime DateLastModified { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public int WarehouseId { get; set; }
        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        // don't lazy load, need to be fast in search results
        public List<ProductTemplateAttachment> Attachment { get; set; }
        public List<Product> Products { get; set; }
        public List<ProductTemplateNote> Notes { get; set; }
    }

    public class ProductTemplateNote
    {
        public ProductTemplateNote()
        {
            DateCreated = DateTime.UtcNow.AddHours(8);
        }

        public int ProductTemplateNoteId { get; set; }
        public string NoteBody { get; set; }
        public byte NoteType { get; set; }
        public DateTime DateCreated { get; set; }

        public int ProductTemplateId { get; set; }
        [ForeignKey("ProductTemplateId")]
        public ProductTemplate ProductTemplate { get; set; }
    }

    public class ProductTemplateAttachment
    {
        public ProductTemplateAttachment()
        {
            DateCreated = DateTime.UtcNow.AddHours(8);
        }

        public int ProductTemplateAttachmentId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public bool IsThumbnail { get; set; }
        public DateTime DateCreated { get; set; }

        public int ProductTemplateId { get; set; }
        [ForeignKey("ProductTemplateId")]
        public ProductTemplate ProductTemplate { get; set; }
    }

    public class Cart
    {
        public Cart()
        {
            DateCreated = DateTime.UtcNow.AddHours(8);
        }

        public int CartId { get; set; }

        public int ProductTemplateId { get; set; }
        [ForeignKey("ProductTemplateId")]
        public ProductTemplate Product { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public int Quantity { get; set; }
        public byte CartType { get; set; }

        public DateTime? RentDateStart { get; set; }
        public DateTime? RentDateEnd { get; set; }
        public int? RentNumberOfDays { get; set; }

        public int? RentToOwnPaymentTermId { get; set; }
        [ForeignKey("RentToOwnPaymentTermId")]
        public RentToOwnPaymentTerm PaymentTerm { get; set; }

        public DateTime DateCreated { get; set; }
    }

    public class Product {
        public Product()
        {
            DateCreated = DateTime.UtcNow.AddHours(8);
            DateLastModified = DateTime.UtcNow.AddHours(8);
        }

        public int ProductId { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string ProductName { get; set; }
        [Required]
        [Display(Name = "Description")]
        public string ProductDescription { get; set; }
        [Display(Name = "Serial Number")]
        public string ProductSerialNumber { get; set; }

        [Display(Name = "Status")]
        public byte ProductStatus { get; set; }

        public float? CustomPrice { get; set; }
        public float? CustomDailyRentPrice { get; set; }

        [Display(Name = "Last Modified By")]
        public string LastModifiedBy { get; set; }
        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }
        [Display(Name = "Last Modified Date")]
        public DateTime DateLastModified { get; set; }

        public int ProductTemplateId { get; set; }
        [ForeignKey("ProductTemplateId")]
        public virtual ProductTemplate ProductTemplate { get; set; }
        public List<ProductAttachment> Attachments { get; set; }
        public List<ProductNote> Notes { get; set; }
    }

    public class ProductAttachment
    {
        public ProductAttachment()
        {
            DateCreated = DateTime.UtcNow.AddHours(8);
        }

        public int ProductAttachmentId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public bool IsThumbnail { get; set; }
        public DateTime DateCreated { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }

    public class ProductNote
    {
        public ProductNote()
        {
            DateCreated = DateTime.UtcNow.AddHours(8);
        }

        public int ProductNoteId { get; set; }
        public string NoteBody { get; set; }
        public byte NoteType { get; set; }
        public DateTime DateCreated { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }

    public class TransactionGroup
    {
        public TransactionGroup()
        {
            DateCreated = DateTime.UtcNow.AddHours(8);
        }

        [Key]
        public int TransactionGroupId { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        
        public List<Transaction> Transactions { get; set; }
        public List<TransactionGroupPaymentAttempt> TransactionGroupPaymentAttempts { get; set; }

        public DateTime DateCreated { get; set; }
    }

    public class Transaction {
        public Transaction()
        {
            DateCreated = DateTime.UtcNow.AddHours(8);
            DateLastModified = DateTime.UtcNow.AddHours(8);
        }

        [Key]
        public int TransactionId { get; set; }
        public string TransactionDescription { get; set; } // put details of payment terms here

        public byte TransactionType { get; set; }
        public byte TransactionStatus { get; set; }
        public int ShippingStatus { get; set; }

        public int AddressId { get; set; }
        [ForeignKey("AddressId")]
        public virtual Address Address { get; set; }

        public byte? AddressType { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }

        public DateTime? RentStartDate { get; set; }
        public DateTime? RentEndDate { get; set; }
        public int? RentNumberOfDays { get; set; }

        public int? RentToOwnPaymentTermId { get; set; }
        [ForeignKey("RentToOwnPaymentTermId")]
        public virtual RentToOwnPaymentTerm RentToOwnPaymentTerm { get; set; }

        public float? ProductPrice { get; set; }
        public float? ProductDailyRentPrice { get; set; }
        public float? RentToOwnInterestRate { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
        
        public int TransactionGroupId { get; set; }
        [ForeignKey("TransactionGroupId")]
        public virtual TransactionGroup TransactionGroup { get; set; }

        public virtual List<TransactionNote> TransactionNotes { get; set; }

        public List<Payment> Payments { get; set; }

        public string LastModifiedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateLastModified { get; set; }
    }

    public class TransactionNote
    {
        public TransactionNote()
        {
            DateCreated = DateTime.UtcNow.AddHours(8);
        }

        public int TransactionNoteId { get; set; }
        public string Note { get; set; }
        public DateTime DateCreated { get; set; }

        public int TransactionId { get; set; }
        [ForeignKey("TransactionId")]
        public Transaction Transaction { get; set; }
    }

    public class Payment {
        public Payment()
        {
            DateCreated = DateTime.UtcNow.AddHours(8);
            DateLastModified = DateTime.UtcNow.AddHours(8);
        }

        [Key]
        public int PaymentId { get; set; }
        public string Description { get; set; }
        public float Amount { get; set; }
        public byte ShippingType { get; set; }
        public float ShippingFee { get; set; }

        public string PaypalTransactionId { get; set; }

        public string LastModifiedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateLastModified { get; set; }

        public DateTime? DateDue { get; set; }
        public int SellerPaymentStatus { get; set; }

        public int TransactionId { get; set; }
        [ForeignKey("TransactionId")]
        public virtual Transaction Transaction { get; set; }
        
        public int? TransactionGroupPaymentAttemptId { get; set; }
        [ForeignKey("TransactionGroupPaymentAttemptId")]
        public virtual TransactionGroupPaymentAttempt TransactionGroupPaymentAttempt { get; set; }
    }

    public class TransactionGroupPaymentAttempt 
    {
        public TransactionGroupPaymentAttempt()
        {
            this.DateCreated = DateTime.UtcNow.AddHours(8);
        }

        // this will handle installment payments/ multiple payments under one transaction
        [Key]
        public int TransactionGroupPaymentAttemptId { get; set; }
        public string Code { get; set; }
        public string PayerId { get; set; }
        public byte Status { get; set; }

        public float TotalAmount { get; set; }
        public float PlatformTaxOrder { get; set; } // in percent
        public float AmountForSystem { get; set; }
        public float AmountForSeller { get; set; }
        public DateTime? DatePaid { get; set; }

        public int TransactionGroupId { get; set; }
        [ForeignKey("TransactionGroupId")]
        public virtual TransactionGroup TransactionGroup { get; set; }

        public virtual List<Payment> Payments { get; set; }

        public DateTime DateCreated { get; set; }
    }

    public class RentToOwnPaymentTerm {
        public RentToOwnPaymentTerm() {
            DateCreated = DateTime.UtcNow.AddHours(8);
        }
        public int RentToOwnPaymentTermId { get; set; }
        public int Months { get; set; }
        public float InterestRate { get; set; } // copy payment terms/interest rate of BDO
        public DateTime DateCreated { get; set; }
    }

    public class Setting {
        public Setting() {
            DateCreated = DateTime.UtcNow.AddHours(8);
        }

        // populate settings for Warehouse, RentToOwnPaymentTerm, Categories, DailyRentComputationPercentage, PlatformTaxBuy, PlatformTaxDailyRent, PlatformTaxRentToOwn, PlatformTaxCashout
        public int SettingId { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public DateTime DateCreated { get; set; }
    }

    /*View Models*/

    public class CartValidateModel{
        public int ProductTemplateId { get; set; }
        public string ProductName { get; set; }
        public int QuantityNeeded { get; set; }

        private int _QuantityAvailable;

        public int QuantityAvailable {
            get
            {
                return this._QuantityAvailable;
            }
            set
            {
                this._QuantityAvailable = value;
                if (this.QuantityNeeded > this._QuantityAvailable)
                {
                    this.Error = true;
                    this.Message = 
                        "Your total requested quantity for "+this.ProductName+" is "+ this.QuantityNeeded+" but the quantity available is "+ this._QuantityAvailable + ".";
                }
                else
                {
                    this.Error = false;
                    this.Message = "";
                }
            }
        }
        public bool Error { get; set; }
        public string Message { get; set; }
    }

    public class PaypalAccessTokenModel
    {
        public string scope { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string app_id { get; set; }
        public string expires_in { get; set; }
        public string nonce { get; set; }
    }

    public class PaypalCreateOrderModel
    {
        public PaypalCreateOrderModel()
        {
            this.intent = "CAPTURE";
            this.application_context = new PaypalApplicationContextModel() {
                brand_name = "Ownorent PH",
                landing_page = "BILLING",
                user_action = "PAY_NOW",
                return_url = ""
            };
            this.purchase_units = new List<PaypalPurchaseUnitModel>()
            {
                new PaypalPurchaseUnitModel()
                {
                    amount = new PaypalPurchaseUnitModel.PaypalAmountModel()
                    {
                        currency_code = "PHP",
                        value = "",
                        breakdown = new PaypalPurchaseUnitModel.PaypalAmountModel.PaypalBreakdownModel()
                        {
                            item_total = new PaypalPurchaseUnitModel.PaypalUnitAmountModel()
                            {
                                currency_code = "PHP",
                                value = ""
                            }
                        }
                    },
                    items = new List<PaypalPurchaseUnitModel.PaypalItemModel>()
                }
            };
        }

        public string intent { get; set; } // CAPTURE
        public PaypalApplicationContextModel application_context { get; set; }
	    public List<PaypalPurchaseUnitModel> purchase_units { get; set; }

        public class PaypalApplicationContextModel
        {
            public string brand_name { get; set; } // Ownorent PH
            public string landing_page { get; set; } // BILLING
            public string user_action { get; set; } // PAY_NOW
            public string return_url { get; set; } // http://localhost:54620/products/receive
        }
        
        public class PaypalPurchaseUnitModel
        {
            public PaypalAmountModel amount { get; set; }
            public List<PaypalItemModel> items { get; set; }

            public class PaypalAmountModel
            {
                public string currency_code { get; set; } // PHP
                public string value { get; set; } // total of order
                public PaypalBreakdownModel breakdown { get; set; }

                public class PaypalBreakdownModel
                {
                    public PaypalUnitAmountModel item_total { get; set; }
                }
            }

            public class PaypalItemModel
            {
                public string name { get; set; }
                public string quantity { get; set; }
                public string sku { get; set; }
                public string description { get; set; }
                public PaypalUnitAmountModel unit_amount { get; set; }
            }

            public class PaypalUnitAmountModel
            {
                public string currency_code { get; set; } // PHP
                public string value { get; set; } // total of order
            }
        }
    }

    public class PaypalCheckoutResultModel
    {
        public string id { get; set; } // Transaction ID
        public List<PaypalLinksModel> links { get; set; }
        public string status { get; set; }

        public class PaypalLinksModel
        {
            public string href { get; set; }
            public string rel { get; set; }
            public string method { get; set; }
        }
    }

    public class PaymentPayModel
    {
        public int PaymentId { get; set; }
        public bool Include { get; set; }
    }

    public class TransactionEditModel
    {
        public int TransactionId { get; set; }
        public int ShippingStatus { get; set; }
    }

    public class ProductViewInfoModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string SellerEmail { get; set; }
        public string SellerFirstName { get; set; }
        public List<ProductTemplateAttachment> Attachments { get; set; }
    }
}