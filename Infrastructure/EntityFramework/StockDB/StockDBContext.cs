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
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<ProductCate> ProductCate { get; set; }
        public virtual DbSet<ProductExtraInfo> ProductExtraInfo { get; set; }
        public virtual DbSet<ProductIdentityCode> ProductIdentityCode { get; set; }
        public virtual DbSet<ProductStockInfo> ProductStockInfo { get; set; }
        public virtual DbSet<ProductStockValidation> ProductStockValidation { get; set; }
        public virtual DbSet<ProductUnitConversion> ProductUnitConversion { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
        }
        protected void OnModelCreated(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");
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
                entity.HasOne(d => d.ProductIdentityCode)
                    .WithMany(p => p.Product)
                    .HasForeignKey(d => d.ProductIdentityCodeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Product_ProductIdentityCode");
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
            modelBuilder.Entity<ProductIdentityCode>(entity =>
            {
                entity.Property(e => e.IdentityCode)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.Property(e => e.ProductIdentityCodeName)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.HasOne(d => d.ParentProductIdentityCode)
                    .WithMany(p => p.InverseParentProductIdentityCode)
                    .HasForeignKey(d => d.ParentProductIdentityCodeId)
                    .HasConstraintName("FK_ProductIdentityCode_ProductIdentityCode");
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
            modelBuilder.Entity<ProductUnitConversion>(entity =>
            {
                entity.HasKey(e => new { e.ProductId, e.SecondaryUnitId });
                entity.Property(e => e.ConversionDescription).HasMaxLength(512);
                entity.Property(e => e.FactorExpression)
                    .IsRequired()
                    .HasMaxLength(256);
                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductUnitConversion)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductUnitConversion_Product");
            });
        }
    }
}
