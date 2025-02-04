namespace BuyCafe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Fix_Order_TransactionId : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Orders", "TransactionId", c => c.Long());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Orders", "TransactionId", c => c.Int());
        }
    }
}
