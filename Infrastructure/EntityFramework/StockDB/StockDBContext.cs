using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
namespace VErp.Infrastructure.EF.StockDB
{
    public partial class StockDBContext : DbContext
    {
        public StockDBContext()
        {
        }
        public StockDBContext(DbContextOptions<StockDBContext> options)
            : base(options)
        {
        }
        public virtual DbSet<File> File { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<ProductCate> ProductCate { get; set; }
        public virtual DbSet<ProductExtraInfo> ProductExtraInfo { get; set; }
        public virtual DbSet<ProductStockInfo> ProductStockInfo { get; set; }
        public virtual DbSet<ProductStockValidation> ProductStockValidation { get; set; }
        public virtual DbSet<ProductType> ProductType { get; set; }
        public virtual DbSet<ProductUnitConversion> ProductUnitConversion { get; set; }
        public virtual DbSet<Stock> Stock { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
        protected void OnModelCreated(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");
            modelBuilder.Entity<File>(entity =>
            {
                entity.Property(e => e.ContentType).HasMaxLength(128);
                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .IsUnicode(false);
                entity.Property(e => e.FilePath)
                    .IsRequired()
                    .HasMaxLength(1024);
            });
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Barcode).HasMaxLength(128);
                entity.Property(e => e.EstimatePrice).HasColumnType("decimal(19, 4)");
                entity.Property(e => e.ProductCode)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.HasOne(d => d.ProductCate)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.ProductCateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Product_ProductCate");
                entity.HasOne(d => d.ProductType)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.ProductTypeId)
                    .HasConstraintName("FK_Product_ProductType");
            });
            modelBuilder.Entity<ProductCate>(entity =>
            {
                entity.Property(e => e.ProductCateName)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.HasOne(d => d.ParentProductCate)
                    .WithMany(p => p.InverseParentProductCate)
                    .HasForeignKey(d => d.ParentProductCateId)
                    .HasConstraintName("FK_ProductCate_ProductCate");
            });
            modelBuilder.Entity<ProductExtraInfo>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductId).ValueGeneratedNever();
                entity.Property(e => e.Description).HasMaxLength(512);
                entity.Property(e => e.Specification).HasMaxLength(512);
                entity.HasOne(d => d.Product)
                    .WithOne(p => p.ProductExtraInfo)
                    .HasForeignKey<ProductExtraInfo>(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductExtraInfo_Product");
            });
            modelBuilder.Entity<ProductStockInfo>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductId).ValueGeneratedNever();
                entity.Property(e => e.DescriptionToStock).HasMaxLength(512);
                entity.HasOne(d => d.Product)
                    .WithOne(p => p.ProductStockInfo)
                    .HasForeignKey<ProductStockInfo>(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductStockInfo_Product");
            });
            modelBuilder.Entity<ProductStockValidation>(entity =>
            {
                entity.HasKey(e => new { e.ProductId, e.StockId });
                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductStockValidation)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductStockValidation_Product");
            });
            modelBuilder.Entity<ProductType>(entity =>
            {
                entity.Property(e => e.IdentityCode)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.Property(e => e.ProductTypeName)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.HasOne(d => d.ParentProductType)
                    .WithMany(p => p.InverseParentProductType)
                    .HasForeignKey(d => d.ParentProductTypeId)
                    .HasConstraintName("FK_ProductType_ProductType");
            });
            modelBuilder.Entity<ProductUnitConversion>(entity =>
            {
                entity.Property(e => e.ConversionDescription).HasMaxLength(512);
                entity.Property(e => e.FactorExpression)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.Property(e => e.ProductUnitConversionName)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductUnitConversion)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductUnitConversion_Product");
            });
            modelBuilder.Entity<Stock>(entity =>
            {
                entity.Property(e => e.StockName)
                    .IsRequired()
                    .HasMaxLength(128);
            });
        }
    }
}
