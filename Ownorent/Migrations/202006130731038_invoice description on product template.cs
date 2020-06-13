namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class invoicedescriptiononproducttemplate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductTemplates", "InvoiceDescription", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductTemplates", "InvoiceDescription");
        }
    }
}
