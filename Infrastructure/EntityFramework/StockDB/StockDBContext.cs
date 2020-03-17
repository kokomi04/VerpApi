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
        public virtual DbSet<InventoryDetailToPackage> InventoryDetailToPackage { get; set; }
        public virtual DbSet<InventoryFile> InventoryFile { get; set; }
        public virtual DbSet<Location> Location { get; set; }
        public virtual DbSet<Package> Package { get; set; }
        public virtual DbSet<PackageOperation> PackageOperation { get; set; }
        public virtual DbSet<PackageRef> PackageRef { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<ProductBom> ProductBom { get; set; }
        public virtual DbSet<ProductCate> ProductCate { get; set; }
        public virtual DbSet<ProductExtraInfo> ProductExtraInfo { get; set; }
        public virtual DbSet<ProductStockInfo> ProductStockInfo { get; set; }
        public virtual DbSet<ProductStockValidation> ProductStockValidation { get; set; }
        public virtual DbSet<ProductType> ProductType { get; set; }
        public virtual DbSet<ProductUnitConversion> ProductUnitConversion { get; set; }
        public virtual DbSet<Stock> Stock { get; set; }
        public virtual DbSet<StockProduct> StockProduct { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                entity.Property(e => e.BillCode)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.BillSerial)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.Content).HasMaxLength(512);

                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Department).HasMaxLength(128);

                entity.Property(e => e.InventoryCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Shipper).HasMaxLength(128);

                entity.Property(e => e.TotalMoney).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Stock)
                    .WithMany(p => p.Inventory)
                    .HasForeignKey(d => d.StockId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Inventory_Stock");
            });

            modelBuilder.Entity<InventoryDetail>(entity =>
            {
                entity.Property(e => e.FromPackageId).HasComment("Xuất kho vào kiện nào");

                entity.Property(e => e.OrderCode)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.PackageOptionId).HasDefaultValueSql("((0))");

                entity.Property(e => e.Pocode)
                    .HasColumnName("POCode")
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductUnitConversionQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductionOrderCode)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.RefObjectCode)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.ToPackageId).HasComment("Nhập kho vào kiện nào");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 4)");

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

                entity.HasOne(d => d.ProductUnitConversion)
                    .WithMany(p => p.InventoryDetail)
                    .HasForeignKey(d => d.ProductUnitConversionId)
                    .HasConstraintName("FK_InventoryDetail_ProductUnitConversion");

                entity.HasOne(d => d.ToPackage)
                    .WithMany(p => p.InventoryDetailToPackage)
                    .HasForeignKey(d => d.ToPackageId)
                    .HasConstraintName("FK_InventoryDetail_ToPackage");
            });

            modelBuilder.Entity<InventoryDetailToPackage>(entity =>
            {
                entity.HasKey(e => new { e.InventoryDetailId, e.ToPackageId });

                entity.HasOne(d => d.InventoryDetail)
                    .WithMany(p => p.InventoryDetailToPackage)
                    .HasForeignKey(d => d.InventoryDetailId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InventoryDetailToPackage_InventoryDetail");

                entity.HasOne(d => d.ToPackage)
                    .WithMany(p => p.InventoryDetailToPackageNavigation)
                    .HasForeignKey(d => d.ToPackageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InventoryDetailToPackage_Package");
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

                entity.Property(e => e.PackageTypeId).HasDefaultValueSql("((1))");

                entity.Property(e => e.PrimaryQuantityRemaining).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.PrimaryQuantityWaiting).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductUnitConversionRemaining).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductUnitConversionWaitting).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.Package)
                    .HasForeignKey(d => d.LocationId)
                    .HasConstraintName("FK_Package_Location");

                entity.HasOne(d => d.ProductUnitConversion)
                    .WithMany(p => p.Package)
                    .HasForeignKey(d => d.ProductUnitConversionId)
                    .HasConstraintName("FK_Package_ProductUnitConversion");

                entity.HasOne(d => d.Stock)
                    .WithMany(p => p.Package)
                    .HasForeignKey(d => d.StockId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Package_Stock");
            });

            modelBuilder.Entity<PackageRef>(entity =>
            {
                entity.HasKey(e => new { e.PackageId, e.RefPackageId });

                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductUnitConversionQuantity).HasColumnType("decimal(32, 16)");

                entity.HasOne(d => d.Package)
                    .WithMany(p => p.PackageRefPackage)
                    .HasForeignKey(d => d.PackageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PackageRef_Package");

                entity.HasOne(d => d.ProductUnitConversion)
                    .WithMany(p => p.PackageRef)
                    .HasForeignKey(d => d.ProductUnitConversionId)
                    .HasConstraintName("FK_PackageRef_ProductUnitConversion");

                entity.HasOne(d => d.RefPackage)
                    .WithMany(p => p.PackageRefRefPackage)
                    .HasForeignKey(d => d.RefPackageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PackageRef_PackageFrom");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(e => e.ProductCode)
                    .HasName("idx_Product_ProductCode");

                entity.Property(e => e.Barcode).HasMaxLength(128);

                entity.Property(e => e.EstimatePrice).HasColumnType("decimal(19, 4)");

                entity.Property(e => e.Height).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Long).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ProductCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Width).HasColumnType("decimal(18, 4)");

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

            modelBuilder.Entity<ProductBom>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(1024);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Wastage).HasColumnType("decimal(18, 4)");

                entity.HasOne(d => d.ParentProduct)
                    .WithMany(p => p.ProductBomParentProduct)
                    .HasForeignKey(d => d.ParentProductId)
                    .HasConstraintName("FK_BillOfMaterial_Product_ParentProductId");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductBomProduct)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillOfMaterial_Product_ProductId");
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

                entity.Property(e => e.IsFreeStyle).HasDefaultValueSql("((0))");

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
                entity.HasIndex(e => new { e.StockId, e.ProductId, e.ProductUnitConversionId })
                    .HasName("idx_StockProduct_StockId_ProductId_ProductUnitConversionId")
                    .IsUnique();

                entity.Property(e => e.PrimaryQuantityRemaining).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.PrimaryQuantityWaiting).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductUnitConversionRemaining).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductUnitConversionWaitting).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
