namespace BuyCafe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Updata_Model_Order_Columns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "Note", c => c.String());
            AddColumn("dbo.Orders", "Invoice", c => c.Int());
            AddColumn("dbo.Orders", "InvoiceNumber", c => c.String());
            AddColumn("dbo.Orders", "invoiceCarrier", c => c.String());
            AddColumn("dbo.Orders", "TypeAndNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "TypeAndNumber");
            DropColumn("dbo.Orders", "invoiceCarrier");
            DropColumn("dbo.Orders", "InvoiceNumber");
            DropColumn("dbo.Orders", "Invoice");
            DropColumn("dbo.Orders", "Note");
        }
    }
}
