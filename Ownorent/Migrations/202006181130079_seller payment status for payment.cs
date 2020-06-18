namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sellerpaymentstatusforpayment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Payments", "SellerPaymentStatus", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Payments", "SellerPaymentStatus");
        }
    }
}
