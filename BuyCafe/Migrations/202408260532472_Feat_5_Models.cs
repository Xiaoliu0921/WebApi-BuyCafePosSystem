namespace BuyCafe.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Feat_5_Models : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Employees",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Identity = c.Int(nullable: false),
                        Account = c.String(nullable: false, maxLength: 100),
                        Password = c.String(nullable: false, maxLength: 100),
                        Salt = c.String(nullable: false, maxLength: 100),
                        Name = c.String(maxLength: 50),
                        Phone = c.String(maxLength: 50),
                        Email = c.String(maxLength: 50),
                        Birthday = c.DateTime(),
                        CreateDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Account, unique: true);
            
            CreateTable(
                "dbo.Members",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Phone = c.String(maxLength: 50),
                        Point = c.Int(nullable: false),
                        Gender = c.Int(),
                        Name = c.String(maxLength: 100),
                        Birthday = c.DateTime(),
                        CreateDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Phone, unique: true);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerPhone = c.String(maxLength: 50),
                        OrderStatus = c.Int(nullable: false),
                        Type = c.Int(),
                        Guid = c.String(nullable: false, maxLength: 100),
                        Table = c.Int(),
                        TakeTime = c.DateTime(),
                        TotalAmount = c.Int(),
                        CreateDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OrderItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Customization = c.String(),
                        Quantity = c.Int(nullable: false),
                        Price = c.Int(nullable: false),
                        Point = c.Int(),
                        CreateDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Orders", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);
            
            CreateTable(
                "dbo.Owners",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Account = c.String(nullable: false, maxLength: 100),
                        Password = c.String(nullable: false, maxLength: 100),
                        Salt = c.String(nullable: false, maxLength: 100),
                        Name = c.String(maxLength: 50),
                        Phone = c.String(maxLength: 50),
                        Email = c.String(maxLength: 50),
                        Birthday = c.DateTime(),
                        CreateDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Account, unique: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderItems", "OrderId", "dbo.Orders");
            DropIndex("dbo.Owners", new[] { "Account" });
            DropIndex("dbo.OrderItems", new[] { "OrderId" });
            DropIndex("dbo.Members", new[] { "Phone" });
            DropIndex("dbo.Employees", new[] { "Account" });
            DropTable("dbo.Owners");
            DropTable("dbo.OrderItems");
            DropTable("dbo.Orders");
            DropTable("dbo.Members");
            DropTable("dbo.Employees");
        }
    }
}
