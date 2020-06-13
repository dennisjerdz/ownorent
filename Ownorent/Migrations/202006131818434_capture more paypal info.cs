namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class capturemorepaypalinfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TransactionGroupPaymentAttempts", "PayerId", c => c.String());
            AddColumn("dbo.TransactionGroupPaymentAttempts", "DatePaid", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TransactionGroupPaymentAttempts", "DatePaid");
            DropColumn("dbo.TransactionGroupPaymentAttempts", "PayerId");
        }
    }
}
