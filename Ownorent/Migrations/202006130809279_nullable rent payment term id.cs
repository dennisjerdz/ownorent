namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nullablerentpaymenttermid : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Transactions", "RentToOwnPaymentTermId", "dbo.RentToOwnPaymentTerms");
            DropIndex("dbo.Transactions", new[] { "RentToOwnPaymentTermId" });
            AlterColumn("dbo.Transactions", "RentToOwnPaymentTermId", c => c.Int());
            CreateIndex("dbo.Transactions", "RentToOwnPaymentTermId");
            AddForeignKey("dbo.Transactions", "RentToOwnPaymentTermId", "dbo.RentToOwnPaymentTerms", "RentToOwnPaymentTermId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Transactions", "RentToOwnPaymentTermId", "dbo.RentToOwnPaymentTerms");
            DropIndex("dbo.Transactions", new[] { "RentToOwnPaymentTermId" });
            AlterColumn("dbo.Transactions", "RentToOwnPaymentTermId", c => c.Int(nullable: false));
            CreateIndex("dbo.Transactions", "RentToOwnPaymentTermId");
            AddForeignKey("dbo.Transactions", "RentToOwnPaymentTermId", "dbo.RentToOwnPaymentTerms", "RentToOwnPaymentTermId", cascadeDelete: true);
        }
    }
}
