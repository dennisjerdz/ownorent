namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dbadjustmentsforpaypal : DbMigration
    {
        public override void Up()
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
                .PrimaryKey(t => t.TransactionGroupPaymentId)
                .ForeignKey("dbo.TransactionGroups", t => t.TransactionGroupId, cascadeDelete: true)
                .Index(t => t.TransactionGroupId);
            
            AddColumn("dbo.Payments", "TransactionGroupPaymentId", c => c.Int(nullable: false));
            CreateIndex("dbo.Payments", "TransactionGroupPaymentId");
            AddForeignKey("dbo.Payments", "TransactionGroupPaymentId", "dbo.TransactionGroupPayments", "TransactionGroupPaymentId", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TransactionGroupPayments", "TransactionGroupId", "dbo.TransactionGroups");
            DropForeignKey("dbo.Payments", "TransactionGroupPaymentId", "dbo.TransactionGroupPayments");
            DropIndex("dbo.TransactionGroupPayments", new[] { "TransactionGroupId" });
            DropIndex("dbo.Payments", new[] { "TransactionGroupPaymentId" });
            DropColumn("dbo.Payments", "TransactionGroupPaymentId");
            DropTable("dbo.TransactionGroupPayments");
        }
    }
}
