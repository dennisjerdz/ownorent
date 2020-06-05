namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Producttemplatelabels : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ProductTemplates", "ProductName", c => c.String(nullable: false));
            AlterColumn("dbo.ProductTemplates", "ProductDescription", c => c.String(nullable: false));
            AlterColumn("dbo.Products", "ProductName", c => c.String(nullable: false));
            AlterColumn("dbo.Products", "ProductDescription", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Products", "ProductDescription", c => c.String());
            AlterColumn("dbo.Products", "ProductName", c => c.String());
            AlterColumn("dbo.ProductTemplates", "ProductDescription", c => c.String());
            AlterColumn("dbo.ProductTemplates", "ProductName", c => c.String());
        }
    }
}
