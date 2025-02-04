namespace BuyCafe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fix_Product_Models : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Products", "SortValue", c => c.Int());
            AlterColumn("dbo.ProductCategories", "SortValue", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ProductCategories", "SortValue", c => c.Int(nullable: false));
            AlterColumn("dbo.Products", "SortValue", c => c.Int(nullable: false));
        }
    }
}
