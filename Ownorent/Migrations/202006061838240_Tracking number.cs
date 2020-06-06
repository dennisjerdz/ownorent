namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Trackingnumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductTemplates", "TrackingNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductTemplates", "TrackingNumber");
        }
    }
}
