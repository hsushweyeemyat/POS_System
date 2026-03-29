using Microsoft.EntityFrameworkCore;
using POS_System.Data.Entities;

namespace POS_System.Data.Contexts;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblCategory> TblCategories => Set<TblCategory>();

    public virtual DbSet<TblProduct> TblProducts => Set<TblProduct>();

    public virtual DbSet<TblSale> TblSales => Set<TblSale>();

    public virtual DbSet<TblSaleItem> TblSaleItems => Set<TblSaleItem>();

    public virtual DbSet<TblUser> TblUsers => Set<TblUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblCategory>(entity =>
        {
            entity.ToTable("Tbl_Category");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("Is_Active");
            entity.Property(e => e.CreatedBy).HasColumnName("Created_By");
            entity.Property(e => e.CreatedDate)
                .HasColumnName("Created_Date")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasColumnName("Modified_By");
            entity.Property(e => e.ModifiedDate)
                .HasColumnName("Modified_Date")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<TblProduct>(entity =>
        {
            entity.ToTable("Tbl_Products");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.CategoryId).HasColumnName("Category_id");
            entity.Property(e => e.IsActive).HasColumnName("Is_Active");
            entity.Property(e => e.CreatedBy).HasColumnName("Created_By");
            entity.Property(e => e.CreatedDate)
                .HasColumnName("Created_Date")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasColumnName("Modified_By");
            entity.Property(e => e.ModifiedDate)
                .HasColumnName("Modified_Date")
                .HasColumnType("datetime");

            entity.HasOne(e => e.Category)
                .WithMany(e => e.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<TblSale>(entity =>
        {
            entity.ToTable("Tbl_Sales");

            entity.Property(e => e.InvoiceNo)
                .HasColumnName("Invoice_No")
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CashierName)
                .HasColumnName("Cashier_Name")
                .HasMaxLength(200);
            entity.Property(e => e.SaleDate)
                .HasColumnName("Sale_Date")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalAmount).HasColumnName("Total_Amount");
            entity.Property(e => e.UserId).HasColumnName("User_id");
            entity.Property(e => e.CreatedDate)
                .HasColumnName("Created_Date")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasColumnName("Modified_By");
            entity.Property(e => e.ModifiedDate)
                .HasColumnName("Modified_Date")
                .HasColumnType("datetime");

            entity.HasOne(e => e.User)
                .WithMany(e => e.Sales)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<TblSaleItem>(entity =>
        {
            entity.ToTable("Tbl_Sale_Item");

            entity.Property(e => e.SaleId).HasColumnName("Sale_id");
            entity.Property(e => e.ProductId).HasColumnName("Product_id");
            entity.Property(e => e.ProductName)
                .HasColumnName("Product_Name")
                .HasMaxLength(100);

            entity.HasOne(e => e.Product)
                .WithMany(e => e.SaleItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Sale)
                .WithMany(e => e.SaleItems)
                .HasForeignKey(e => e.SaleId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.ToTable("Tbl_Users");

            entity.Property(e => e.FullName)
                .HasColumnName("Full_Name")
                .HasMaxLength(200);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasColumnName("Is_Active");
            entity.Property(e => e.CreatedBy).HasColumnName("Created_By");
            entity.Property(e => e.CreatedDate)
                .HasColumnName("Created_Date")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasColumnName("Modified_By");
            entity.Property(e => e.ModifiedDate)
                .HasColumnName("Modified_Date")
                .HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
