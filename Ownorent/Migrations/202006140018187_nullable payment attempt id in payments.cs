namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nullablepaymentattemptidinpayments : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Payments", "TransactionGroupPaymentAttemptId", "dbo.TransactionGroupPaymentAttempts");
            DropIndex("dbo.Payments", new[] { "TransactionGroupPaymentAttemptId" });
            AlterColumn("dbo.Payments", "TransactionGroupPaymentAttemptId", c => c.Int());
            CreateIndex("dbo.Payments", "TransactionGroupPaymentAttemptId");
            AddForeignKey("dbo.Payments", "TransactionGroupPaymentAttemptId", "dbo.TransactionGroupPaymentAttempts", "TransactionGroupPaymentAttemptId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Payments", "TransactionGroupPaymentAttemptId", "dbo.TransactionGroupPaymentAttempts");
            DropIndex("dbo.Payments", new[] { "TransactionGroupPaymentAttemptId" });
            AlterColumn("dbo.Payments", "TransactionGroupPaymentAttemptId", c => c.Int(nullable: false));
            CreateIndex("dbo.Payments", "TransactionGroupPaymentAttemptId");
            AddForeignKey("dbo.Payments", "TransactionGroupPaymentAttemptId", "dbo.TransactionGroupPaymentAttempts", "TransactionGroupPaymentAttemptId", cascadeDelete: true);
        }
    }
}
