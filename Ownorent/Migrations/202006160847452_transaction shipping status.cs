namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class transactionshippingstatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Transactions", "ShippingStatus", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Transactions", "ShippingStatus");
        }
    }
}
