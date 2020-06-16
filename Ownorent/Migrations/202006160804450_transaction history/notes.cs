namespace Ownorent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class transactionhistorynotes : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TransactionNotes",
                c => new
                    {
                        TransactionNoteId = c.Int(nullable: false, identity: true),
                        Note = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        TransactionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TransactionNoteId)
                .ForeignKey("dbo.Transactions", t => t.TransactionId, cascadeDelete: true)
                .Index(t => t.TransactionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TransactionNotes", "TransactionId", "dbo.Transactions");
            DropIndex("dbo.TransactionNotes", new[] { "TransactionId" });
            DropTable("dbo.TransactionNotes");
        }
    }
}
