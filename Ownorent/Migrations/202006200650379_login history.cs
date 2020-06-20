namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class loginhistory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LoginHistories",
                c => new
                    {
                        LoginHistoryId = c.Int(nullable: false, identity: true),
                        Role = c.String(),
                        UserId = c.String(maxLength: 128),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.LoginHistoryId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LoginHistories", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.LoginHistories", new[] { "UserId" });
            DropTable("dbo.LoginHistories");
        }
    }
}
