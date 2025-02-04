namespace BuyCafe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "TransactionId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "TransactionId");
        }
    }
}
