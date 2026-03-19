namespace Gestion_de_inventario_PI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UnificarIdentity : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id_category = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.Id_category);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        id_product = c.Int(nullable: false, identity: true),
                        code = c.String(maxLength: 50),
                        name = c.String(nullable: false, maxLength: 150),
                        buyingprice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        saleprice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        stock = c.Int(nullable: false),
                        image = c.String(maxLength: 500),
                        status = c.Boolean(nullable: false),
                        categoryId = c.Int(),
                        Descrption = c.String(),
                    })
                .PrimaryKey(t => t.id_product)
                .ForeignKey("dbo.Categories", t => t.categoryId)
                .Index(t => t.categoryId);
            
            CreateTable(
                "dbo.movements",
                c => new
                    {
                        id_movement = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(),
                        datemovement = c.DateTime(nullable: false),
                        type_movement = c.String(nullable: false, maxLength: 50),
                        amount = c.Int(nullable: false),
                        motive = c.String(maxLength: 255),
                        userid = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.id_movement)
                .ForeignKey("dbo.Products", t => t.ProductId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.sales_details",
                c => new
                    {
                        id_detail = c.Int(nullable: false, identity: true),
                        saleid = c.Int(),
                        productid = c.Int(),
                        amount = c.Int(nullable: false),
                        unit_price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        subtotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.id_detail)
                .ForeignKey("dbo.Sales", t => t.saleid)
                .ForeignKey("dbo.Products", t => t.productid)
                .Index(t => t.saleid)
                .Index(t => t.productid);
            
            CreateTable(
                "dbo.Sales",
                c => new
                    {
                        id_sale = c.Int(nullable: false, identity: true),
                        date = c.DateTime(nullable: false),
                        Total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        payment = c.String(nullable: false, maxLength: 50),
                        pay = c.Decimal(nullable: false, precision: 18, scale: 2),
                        change = c.Decimal(nullable: false, precision: 18, scale: 2),
                        status = c.Boolean(nullable: false),
                        userId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.id_sale);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Products", "categoryId", "dbo.Categories");
            DropForeignKey("dbo.sales_details", "productid", "dbo.Products");
            DropForeignKey("dbo.sales_details", "saleid", "dbo.Sales");
            DropForeignKey("dbo.movements", "ProductId", "dbo.Products");
            DropIndex("dbo.sales_details", new[] { "productid" });
            DropIndex("dbo.sales_details", new[] { "saleid" });
            DropIndex("dbo.movements", new[] { "ProductId" });
            DropIndex("dbo.Products", new[] { "categoryId" });
            DropTable("dbo.Sales");
            DropTable("dbo.sales_details");
            DropTable("dbo.movements");
            DropTable("dbo.Products");
            DropTable("dbo.Categories");
        }
    }
}
