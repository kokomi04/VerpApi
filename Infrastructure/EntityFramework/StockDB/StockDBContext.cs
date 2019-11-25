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
        public virtual DbSet<Inventory> Inventory { get; set; }
        public virtual DbSet<InventoryDetail> InventoryDetail { get; set; }
        public virtual DbSet<InventoryFile> InventoryFile { get; set; }
        public virtual DbSet<Location> Location { get; set; }
        public virtual DbSet<Package> Package { get; set; }
        public virtual DbSet<PackageRef> PackageRef { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<ProductCate> ProductCate { get; set; }
        public virtual DbSet<ProductExtraInfo> ProductExtraInfo { get; set; }
        public virtual DbSet<ProductStockInfo> ProductStockInfo { get; set; }
        public virtual DbSet<ProductStockValidation> ProductStockValidation { get; set; }
        public virtual DbSet<ProductType> ProductType { get; set; }
        public virtual DbSet<ProductUnitConversion> ProductUnitConversion { get; set; }
        public virtual DbSet<Stock> Stock { get; set; }
        public virtual DbSet<StockProduct> StockProduct { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=103.21.149.106;Database=StockDB;User ID=VErpAdmin;Password=VerpDev123$#1;MultipleActiveResultSets=true");
            }
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
                entity.Property(e => e.LargeThumb).HasMaxLength(1024);
                entity.Property(e => e.SmallThumb).HasMaxLength(1024);
            });
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.Property(e => e.Content).HasMaxLength(512);
                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.Department).HasMaxLength(128);
                entity.Property(e => e.InventoryCode)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.Property(e => e.Shipper).HasMaxLength(128);
                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");
                entity.HasOne(d => d.Stock)
                    .WithMany(p => p.Inventory)
                    .HasForeignKey(d => d.StockId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Inventory_Stock");
            });
            modelBuilder.Entity<InventoryDetail>(entity =>
            {
                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.RefObjectCode).HasMaxLength(128);
                entity.Property(e => e.SecondaryQuantity).HasColumnType("decimal(18, 4)");
                entity.HasOne(d => d.FromPackage)
                    .WithMany(p => p.InventoryDetailFromPackage)
                    .HasForeignKey(d => d.FromPackageId)
                    .HasConstraintName("FK_InventoryDetail_FromPackage");
                entity.HasOne(d => d.Inventory)
                    .WithMany(p => p.InventoryDetail)
                    .HasForeignKey(d => d.InventoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InventoryDetail_Inventory");
                entity.HasOne(d => d.Product)
                    .WithMany(p => p.InventoryDetail)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InventoryDetail_Product");
                entity.HasOne(d => d.ToPackage)
                    .WithMany(p => p.InventoryDetailToPackage)
                    .HasForeignKey(d => d.ToPackageId)
                    .HasConstraintName("FK_InventoryDetail_ToPackage");
            });
            modelBuilder.Entity<InventoryFile>(entity =>
            {
                entity.HasKey(e => new { e.InventoryId, e.FileId });
            });
            modelBuilder.Entity<Location>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.Description)
                    .HasMaxLength(256)
                    .HasDefaultValueSql("(N'')");
                entity.Property(e => e.Name)
                    .HasMaxLength(128)
                    .HasDefaultValueSql("(N'')");
                entity.Property(e => e.Status).HasDefaultValueSql("((0))");
                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");
            });
            modelBuilder.Entity<Package>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.Date).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.PackageCode)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasDefaultValueSql("('')");
                entity.Property(e => e.PackageType).HasDefaultValueSql("((1))");
                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.PrimaryQuantityRemaining).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.PrimaryQuantityWaiting).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.SecondaryQuantity).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.SecondaryQuantityRemaining).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.SecondaryQuantityWaitting).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");
                entity.HasOne(d => d.Location)
                    .WithMany(p => p.Package)
                    .HasForeignKey(d => d.LocationId)
                    .HasConstraintName("FK_Package_Location");
                entity.HasOne(d => d.Stock)
                    .WithMany(p => p.Package)
                    .HasForeignKey(d => d.StockId)
                    .HasConstraintName("FK_Package_Stock");
            });
            modelBuilder.Entity<PackageRef>(entity =>
            {
                entity.HasKey(e => new { e.PackageId, e.RefPackageId });
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
                entity.Property(e => e.Description).HasMaxLength(512);
                entity.Property(e => e.Status).HasDefaultValueSql("((0))");
                entity.Property(e => e.StockKeeperName).HasMaxLength(64);
                entity.Property(e => e.StockName)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.Property(e => e.Type).HasDefaultValueSql("((0))");
            });
            modelBuilder.Entity<StockProduct>(entity =>
            {
                entity.HasKey(e => new { e.StockId, e.ProductId, e.SecondaryUnitId });
                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.PrimaryQuantityRemaining).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.PrimaryQuantityWaiting).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.SecondaryQuantity).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.SecondaryQuantityRemaining).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.SecondaryQuantityWaitting).HasColumnType("decimal(18, 4)");
                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");
                entity.HasOne(d => d.Product)
                    .WithMany(p => p.StockProduct)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StockProduct_Product");
                entity.HasOne(d => d.Stock)
                    .WithMany(p => p.StockProduct)
                    .HasForeignKey(d => d.StockId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_StockProduct_Stock");
            });
        }
    }
}
