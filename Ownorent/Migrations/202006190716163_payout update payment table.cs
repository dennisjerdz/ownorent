namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class payoutupdatepaymenttable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Payments", "Bank", c => c.String());
            AddColumn("dbo.Payments", "AccountNumber", c => c.String());
            AddColumn("dbo.Payments", "ConfirmationNumber", c => c.String());
            AddColumn("dbo.Payments", "PlatformTax", c => c.Single());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Payments", "PlatformTax");
            DropColumn("dbo.Payments", "ConfirmationNumber");
            DropColumn("dbo.Payments", "AccountNumber");
            DropColumn("dbo.Payments", "Bank");
        }
    }
}
