using Gestion_de_inventario_PI.Models;
using System.Data.Entity;

public partial class Model1 : DbContext
{
    public Model1()
        : base("name=model_db")
    {
    }

    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<movement> movements { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<Sale> Sales { get; set; }
    public virtual DbSet<sales_details> sales_details { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .HasMany(e => e.Products)
            .WithOptional(e => e.Category)
            .HasForeignKey(e => e.categoryId);

        modelBuilder.Entity<Product>()
            .HasMany(e => e.movements)
            .WithOptional(e => e.Product)
            .HasForeignKey(e => e.ProductId);

        modelBuilder.Entity<Product>()
            .HasMany(e => e.sales_details)
            .WithOptional(e => e.Product)
            .HasForeignKey(e => e.productid);

        modelBuilder.Entity<Sale>()
            .HasMany(e => e.sales_details)
            .WithOptional(e => e.Sale)
            .HasForeignKey(e => e.saleid);
    }
}
