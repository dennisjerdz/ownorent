namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class transactionandpaymentdbchanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Payments", "PlatformTaxBuy", c => c.Single());
            AddColumn("dbo.Payments", "PlatformTaxDailyRent", c => c.Single());
            AddColumn("dbo.Payments", "PlatformTaxRentToOwn", c => c.Single());
            DropColumn("dbo.Transactions", "PlatformTaxBuy");
            DropColumn("dbo.Transactions", "PlatformTaxDailyRent");
            DropColumn("dbo.Transactions", "PlatformTaxRentToOwn");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Transactions", "PlatformTaxRentToOwn", c => c.Single(nullable: false));
            AddColumn("dbo.Transactions", "PlatformTaxDailyRent", c => c.Single(nullable: false));
            AddColumn("dbo.Transactions", "PlatformTaxBuy", c => c.Single(nullable: false));
            DropColumn("dbo.Payments", "PlatformTaxRentToOwn");
            DropColumn("dbo.Payments", "PlatformTaxDailyRent");
            DropColumn("dbo.Payments", "PlatformTaxBuy");
        }
    }
}
