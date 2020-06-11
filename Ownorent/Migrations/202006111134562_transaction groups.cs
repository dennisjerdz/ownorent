namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class transactiongroups : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Transactions", name: "UserId", newName: "ApplicationUser_Id");
            RenameIndex(table: "dbo.Transactions", name: "IX_UserId", newName: "IX_ApplicationUser_Id");
            CreateTable(
                "dbo.TransactionGroups",
                c => new
                    {
                        TransactionGroupId = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.TransactionGroupId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            AddColumn("dbo.Transactions", "TransactionGroupId", c => c.Int(nullable: false));
            CreateIndex("dbo.Transactions", "TransactionGroupId");
            AddForeignKey("dbo.Transactions", "TransactionGroupId", "dbo.TransactionGroups", "TransactionGroupId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TransactionGroups", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Transactions", "TransactionGroupId", "dbo.TransactionGroups");
            DropIndex("dbo.TransactionGroups", new[] { "UserId" });
            DropIndex("dbo.Transactions", new[] { "TransactionGroupId" });
            DropColumn("dbo.Transactions", "TransactionGroupId");
            DropTable("dbo.TransactionGroups");
            RenameIndex(table: "dbo.Transactions", name: "IX_ApplicationUser_Id", newName: "IX_UserId");
            RenameColumn(table: "dbo.Transactions", name: "ApplicationUser_Id", newName: "UserId");
        }
    }
}
