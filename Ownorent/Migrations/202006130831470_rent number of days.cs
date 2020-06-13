namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rentnumberofdays : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Transactions", "RentNumberOfDays", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Transactions", "RentNumberOfDays");
        }
    }
}
