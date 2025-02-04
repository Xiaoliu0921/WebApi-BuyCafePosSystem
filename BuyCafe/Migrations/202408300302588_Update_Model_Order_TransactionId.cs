namespace BuyCafe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_Model_Order_TransactionId : DbMigration
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
