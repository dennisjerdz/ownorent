namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class computenumberofdaysuponaddtocartrentandsavetodb : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Carts", "RentNumberOfDays", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Carts", "RentNumberOfDays");
        }
    }
}
