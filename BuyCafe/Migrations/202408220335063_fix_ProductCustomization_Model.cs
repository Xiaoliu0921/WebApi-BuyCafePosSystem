namespace BuyCafe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fix_ProductCustomization_Model : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductCustomizations", "Title", c => c.String(maxLength: 50));
            AddColumn("dbo.ProductCustomizations", "CustomizationEnum", c => c.Int(nullable: false));
            AddColumn("dbo.ProductCustomizations", "CreateDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductCustomizations", "CreateDate");
            DropColumn("dbo.ProductCustomizations", "CustomizationEnum");
            DropColumn("dbo.ProductCustomizations", "Title");
        }
    }
}
