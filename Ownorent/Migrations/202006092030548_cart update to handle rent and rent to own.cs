namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cartupdatetohandlerentandrenttoown : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Carts", "RentDateStart", c => c.DateTime());
            AddColumn("dbo.Carts", "RentDateEnd", c => c.DateTime());
            AddColumn("dbo.Carts", "RentToOwnPaymentTermId", c => c.Int());
            CreateIndex("dbo.Carts", "RentToOwnPaymentTermId");
            AddForeignKey("dbo.Carts", "RentToOwnPaymentTermId", "dbo.RentToOwnPaymentTerms", "RentToOwnPaymentTermId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Carts", "RentToOwnPaymentTermId", "dbo.RentToOwnPaymentTerms");
            DropIndex("dbo.Carts", new[] { "RentToOwnPaymentTermId" });
            DropColumn("dbo.Carts", "RentToOwnPaymentTermId");
            DropColumn("dbo.Carts", "RentDateEnd");
            DropColumn("dbo.Carts", "RentDateStart");
        }
    }
}
