namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class renameentity : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Payments", "TransactionGroupPaymentId", "dbo.TransactionGroupPayments");
            DropForeignKey("dbo.TransactionGroupPayments", "TransactionGroupId", "dbo.TransactionGroups");
            DropIndex("dbo.Payments", new[] { "TransactionGroupPaymentId" });
            DropIndex("dbo.TransactionGroupPayments", new[] { "TransactionGroupId" });
            CreateTable(
                "dbo.TransactionGroupPaymentAttempts",
                c => new
                    {
                        TransactionGroupPaymentAttemptId = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        Status = c.Byte(nullable: false),
                        TransactionGroupId = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.TransactionGroupPaymentAttemptId)
                .ForeignKey("dbo.TransactionGroups", t => t.TransactionGroupId, cascadeDelete: true)
                .Index(t => t.TransactionGroupId);
            
            AddColumn("dbo.Payments", "TransactionGroupPaymentAttemptId", c => c.Int(nullable: false));
            CreateIndex("dbo.Payments", "TransactionGroupPaymentAttemptId");
            AddForeignKey("dbo.Payments", "TransactionGroupPaymentAttemptId", "dbo.TransactionGroupPaymentAttempts", "TransactionGroupPaymentAttemptId", cascadeDelete: false);
            DropColumn("dbo.Payments", "TransactionGroupPaymentId");
            DropTable("dbo.TransactionGroupPayments");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TransactionGroupPayments",
                c => new
                    {
                        TransactionGroupPaymentId = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        Status = c.Byte(nullable: false),
                        TransactionGroupId = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.TransactionGroupPaymentId);
            
            AddColumn("dbo.Payments", "TransactionGroupPaymentId", c => c.Int(nullable: false));
            DropForeignKey("dbo.TransactionGroupPaymentAttempts", "TransactionGroupId", "dbo.TransactionGroups");
            DropForeignKey("dbo.Payments", "TransactionGroupPaymentAttemptId", "dbo.TransactionGroupPaymentAttempts");
            DropIndex("dbo.TransactionGroupPaymentAttempts", new[] { "TransactionGroupId" });
            DropIndex("dbo.Payments", new[] { "TransactionGroupPaymentAttemptId" });
            DropColumn("dbo.Payments", "TransactionGroupPaymentAttemptId");
            DropTable("dbo.TransactionGroupPaymentAttempts");
            CreateIndex("dbo.TransactionGroupPayments", "TransactionGroupId");
            CreateIndex("dbo.Payments", "TransactionGroupPaymentId");
            AddForeignKey("dbo.TransactionGroupPayments", "TransactionGroupId", "dbo.TransactionGroups", "TransactionGroupId", cascadeDelete: true);
            AddForeignKey("dbo.Payments", "TransactionGroupPaymentId", "dbo.TransactionGroupPayments", "TransactionGroupPaymentId", cascadeDelete: true);
        }
    }
}
