namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class byteorsbytefixes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Carts",
                c => new
                    {
                        CartId = c.Int(nullable: false, identity: true),
                        ProductTemplateId = c.Int(nullable: false),
                        UserId = c.String(maxLength: 128),
                        Quantity = c.Int(nullable: false),
                        CartType = c.Byte(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.CartId)
                .ForeignKey("dbo.ProductTemplates", t => t.ProductTemplateId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.ProductTemplateId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Carts", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Carts", "ProductTemplateId", "dbo.ProductTemplates");
            DropIndex("dbo.Carts", new[] { "UserId" });
            DropIndex("dbo.Carts", new[] { "ProductTemplateId" });
            DropTable("dbo.Carts");
        }
    }
}
