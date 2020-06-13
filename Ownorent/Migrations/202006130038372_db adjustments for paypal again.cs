namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dbadjustmentsforpaypalagain : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionGroupPaymentAttempts", "TotalAmount", c => c.Single(nullable: false));
            AddColumn("dbo.TransactionGroupPaymentAttempts", "PlatformTaxOrder", c => c.Single(nullable: false));
            AddColumn("dbo.TransactionGroupPaymentAttempts", "AmountForSystem", c => c.Single(nullable: false));
            AddColumn("dbo.TransactionGroupPaymentAttempts", "AmountForSeller", c => c.Single(nullable: false));
            DropColumn("dbo.Payments", "PlatformTaxBuy");
            DropColumn("dbo.Payments", "PlatformTaxDailyRent");
            DropColumn("dbo.Payments", "PlatformTaxRentToOwn");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Payments", "PlatformTaxRentToOwn", c => c.Single());
            AddColumn("dbo.Payments", "PlatformTaxDailyRent", c => c.Single());
            AddColumn("dbo.Payments", "PlatformTaxBuy", c => c.Single());
            DropColumn("dbo.TransactionGroupPaymentAttempts", "AmountForSeller");
            DropColumn("dbo.TransactionGroupPaymentAttempts", "AmountForSystem");
            DropColumn("dbo.TransactionGroupPaymentAttempts", "PlatformTaxOrder");
            DropColumn("dbo.TransactionGroupPaymentAttempts", "TotalAmount");
        }
    }
}
