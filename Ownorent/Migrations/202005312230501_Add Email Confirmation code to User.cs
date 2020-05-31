namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEmailConfirmationcodetoUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "ConfirmationCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "ConfirmationCode");
        }
    }
}
