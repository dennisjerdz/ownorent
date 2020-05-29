namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Addresses",
                c => new
                    {
                        AddressId = c.Int(nullable: false, identity: true),
                        AddressType = c.Byte(nullable: false),
                        Line1 = c.String(),
                        Line2 = c.String(),
                        Line3 = c.String(),
                        City = c.String(),
                        Zip = c.String(),
                        Country = c.String(),
                        IsDefault = c.Boolean(nullable: false),
                        LastModifiedBy = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        DateLastModified = c.DateTime(nullable: false),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.AddressId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(nullable: false),
                        MiddleName = c.String(),
                        LastName = c.String(),
                        MobileNumber = c.String(),
                        MobileNumberCode = c.String(),
                        AccountType = c.Int(nullable: false),
                        AccountStatus = c.Int(nullable: false),
                        ProfilePictureLocation = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.UserAttachments",
                c => new
                    {
                        UserAttachmentId = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        Location = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.UserAttachmentId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ProductTemplates",
                c => new
                    {
                        ProductTemplateId = c.Int(nullable: false, identity: true),
                        ProductName = c.String(),
                        ProductDescription = c.String(),
                        ProductTemplateStatus = c.Byte(nullable: false),
                        ProductPriceToUse = c.Byte(nullable: false),
                        Quantity = c.Int(nullable: false),
                        Price = c.Single(nullable: false),
                        DailyRentPrice = c.Single(),
                        ComputedPrice = c.Single(),
                        ComputedDailyRentPrice = c.Single(),
                        AdminDefinedPrice = c.Single(),
                        AdminDefinedDailyRentPrice = c.Single(),
                        ShippingFee = c.Single(nullable: false),
                        ShippingFeeProvincial = c.Single(nullable: false),
                        DatePurchased = c.DateTime(nullable: false),
                        LastModifiedBy = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        DateLastModified = c.DateTime(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        WarehouseId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ProductTemplateId)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.Warehouses", t => t.WarehouseId, cascadeDelete: true)
                .Index(t => t.CategoryId)
                .Index(t => t.WarehouseId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.ProductTemplateAttachments",
                c => new
                    {
                        ProductTemplateAttachmentId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Location = c.String(),
                        IsThumbnail = c.Boolean(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        ProductTemplateId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductTemplateAttachmentId)
                .ForeignKey("dbo.ProductTemplates", t => t.ProductTemplateId, cascadeDelete: true)
                .Index(t => t.ProductTemplateId);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryId = c.Int(nullable: false, identity: true),
                        CategoryName = c.String(nullable: false),
                        UsefulLifeSpan = c.Single(nullable: false),
                        LastModifiedBy = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        DateLastModified = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.CategoryId);
            
            CreateTable(
                "dbo.ProductTemplateNotes",
                c => new
                    {
                        ProductTemplateNoteId = c.Int(nullable: false, identity: true),
                        NoteBody = c.String(),
                        NoteType = c.Byte(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        ProductTemplateId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductTemplateNoteId)
                .ForeignKey("dbo.ProductTemplates", t => t.ProductTemplateId, cascadeDelete: true)
                .Index(t => t.ProductTemplateId);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        ProductId = c.Int(nullable: false, identity: true),
                        ProductName = c.String(),
                        ProductDescription = c.String(),
                        ProductSerialNumber = c.String(),
                        ProductStatus = c.Byte(nullable: false),
                        CustomPrice = c.Single(),
                        CustomDailyRentPrice = c.Single(),
                        LastModifiedBy = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        DateLastModified = c.DateTime(nullable: false),
                        ProductTemplateId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductId)
                .ForeignKey("dbo.ProductTemplates", t => t.ProductTemplateId, cascadeDelete: true)
                .Index(t => t.ProductTemplateId);
            
            CreateTable(
                "dbo.ProductAttachments",
                c => new
                    {
                        ProductAttachmentId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Location = c.String(),
                        IsThumbnail = c.Boolean(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        ProductId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductAttachmentId)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.ProductNotes",
                c => new
                    {
                        ProductNoteId = c.Int(nullable: false, identity: true),
                        NoteBody = c.String(),
                        NoteType = c.Byte(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        ProductId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductNoteId)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Warehouses",
                c => new
                    {
                        WarehouseId = c.Int(nullable: false, identity: true),
                        WarehouseName = c.String(),
                        Location = c.String(),
                        IsDefault = c.Boolean(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.WarehouseId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Transactions",
                c => new
                    {
                        TransactionId = c.Int(nullable: false, identity: true),
                        PaypalTransactionId = c.String(),
                        TransactionDescription = c.String(),
                        TransactionType = c.Byte(nullable: false),
                        TransactionStatus = c.Byte(nullable: false),
                        AddressId = c.Int(nullable: false),
                        AddressType = c.Byte(nullable: false),
                        Line1 = c.String(),
                        Line2 = c.String(),
                        Line3 = c.String(),
                        City = c.String(),
                        Zip = c.String(),
                        Country = c.String(),
                        ShippingType = c.Byte(),
                        ShippingFee = c.Single(),
                        RentStartDate = c.DateTime(),
                        RentEndDate = c.DateTime(),
                        RentToOwnPaymentTermId = c.Int(nullable: false),
                        ProductPrice = c.Single(),
                        ProductDailyRentPrice = c.Single(),
                        RentToOwnInterestRate = c.Single(),
                        PlatformTaxBuy = c.Single(nullable: false),
                        PlatformTaxDailyRent = c.Single(nullable: false),
                        PlatformTaxRentToOwn = c.Single(nullable: false),
                        ProductId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        LastModifiedBy = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        DateLastModified = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.TransactionId)
                .ForeignKey("dbo.Addresses", t => t.AddressId, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .ForeignKey("dbo.RentToOwnPaymentTerms", t => t.RentToOwnPaymentTermId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.AddressId)
                .Index(t => t.RentToOwnPaymentTermId)
                .Index(t => t.ProductId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        PaymentId = c.Int(nullable: false, identity: true),
                        Amount = c.Single(nullable: false),
                        PaypalTransactionId = c.String(),
                        LastModifiedBy = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        DateLastModified = c.DateTime(nullable: false),
                        DateDue = c.DateTime(),
                        TransactionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PaymentId)
                .ForeignKey("dbo.Transactions", t => t.TransactionId, cascadeDelete: true)
                .Index(t => t.TransactionId);
            
            CreateTable(
                "dbo.RentToOwnPaymentTerms",
                c => new
                    {
                        RentToOwnPaymentTermId = c.Int(nullable: false, identity: true),
                        Months = c.Int(nullable: false),
                        InterestRate = c.Single(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.RentToOwnPaymentTermId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.Settings",
                c => new
                    {
                        SettingId = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        Value = c.String(),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.SettingId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Transactions", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Transactions", "RentToOwnPaymentTermId", "dbo.RentToOwnPaymentTerms");
            DropForeignKey("dbo.Transactions", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Payments", "TransactionId", "dbo.Transactions");
            DropForeignKey("dbo.Transactions", "AddressId", "dbo.Addresses");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ProductTemplates", "WarehouseId", "dbo.Warehouses");
            DropForeignKey("dbo.ProductTemplates", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Products", "ProductTemplateId", "dbo.ProductTemplates");
            DropForeignKey("dbo.ProductNotes", "ProductId", "dbo.Products");
            DropForeignKey("dbo.ProductAttachments", "ProductId", "dbo.Products");
            DropForeignKey("dbo.ProductTemplateNotes", "ProductTemplateId", "dbo.ProductTemplates");
            DropForeignKey("dbo.ProductTemplates", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.ProductTemplateAttachments", "ProductTemplateId", "dbo.ProductTemplates");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserAttachments", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Addresses", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Payments", new[] { "TransactionId" });
            DropIndex("dbo.Transactions", new[] { "UserId" });
            DropIndex("dbo.Transactions", new[] { "ProductId" });
            DropIndex("dbo.Transactions", new[] { "RentToOwnPaymentTermId" });
            DropIndex("dbo.Transactions", new[] { "AddressId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.ProductNotes", new[] { "ProductId" });
            DropIndex("dbo.ProductAttachments", new[] { "ProductId" });
            DropIndex("dbo.Products", new[] { "ProductTemplateId" });
            DropIndex("dbo.ProductTemplateNotes", new[] { "ProductTemplateId" });
            DropIndex("dbo.ProductTemplateAttachments", new[] { "ProductTemplateId" });
            DropIndex("dbo.ProductTemplates", new[] { "UserId" });
            DropIndex("dbo.ProductTemplates", new[] { "WarehouseId" });
            DropIndex("dbo.ProductTemplates", new[] { "CategoryId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.UserAttachments", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Addresses", new[] { "UserId" });
            DropTable("dbo.Settings");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.RentToOwnPaymentTerms");
            DropTable("dbo.Payments");
            DropTable("dbo.Transactions");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.Warehouses");
            DropTable("dbo.ProductNotes");
            DropTable("dbo.ProductAttachments");
            DropTable("dbo.Products");
            DropTable("dbo.ProductTemplateNotes");
            DropTable("dbo.Categories");
            DropTable("dbo.ProductTemplateAttachments");
            DropTable("dbo.ProductTemplates");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.UserAttachments");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Addresses");
        }
    }
}
