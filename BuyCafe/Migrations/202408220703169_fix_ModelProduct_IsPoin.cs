namespace BuyCafe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fix_ModelProduct_IsPoin : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "isPoint", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Products", "isPoint");
        }
    }
}
