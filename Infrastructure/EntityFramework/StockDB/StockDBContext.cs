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
        public virtual DbSet<InventoryChange> InventoryChange { get; set; }
        public virtual DbSet<InventoryDetail> InventoryDetail { get; set; }
        public virtual DbSet<InventoryDetailChange> InventoryDetailChange { get; set; }
        public virtual DbSet<InventoryDetailToPackage> InventoryDetailToPackage { get; set; }
        public virtual DbSet<InventoryFile> InventoryFile { get; set; }
        public virtual DbSet<InventoryRequirement> InventoryRequirement { get; set; }
        public virtual DbSet<InventoryRequirementDetail> InventoryRequirementDetail { get; set; }
        public virtual DbSet<InventoryRequirementFile> InventoryRequirementFile { get; set; }
        public virtual DbSet<Location> Location { get; set; }
        public virtual DbSet<Package> Package { get; set; }
        public virtual DbSet<PackageOperation> PackageOperation { get; set; }
        public virtual DbSet<PackageRef> PackageRef { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<ProductAttachment> ProductAttachment { get; set; }
        public virtual DbSet<ProductBom> ProductBom { get; set; }
        public virtual DbSet<ProductCate> ProductCate { get; set; }
        public virtual DbSet<ProductCustomer> ProductCustomer { get; set; }
        public virtual DbSet<ProductExtraInfo> ProductExtraInfo { get; set; }
        public virtual DbSet<ProductMaterial> ProductMaterial { get; set; }
        public virtual DbSet<ProductMaterialsConsumption> ProductMaterialsConsumption { get; set; }
        public virtual DbSet<ProductMaterialsConsumptionGroup> ProductMaterialsConsumptionGroup { get; set; }
        public virtual DbSet<ProductProperty> ProductProperty { get; set; }
        public virtual DbSet<ProductStockInfo> ProductStockInfo { get; set; }
        public virtual DbSet<ProductStockValidation> ProductStockValidation { get; set; }
        public virtual DbSet<ProductType> ProductType { get; set; }
        public virtual DbSet<ProductUnitConversion> ProductUnitConversion { get; set; }
        public virtual DbSet<Property> Property { get; set; }
        public virtual DbSet<RefCustomerBasic> RefCustomerBasic { get; set; }
        public virtual DbSet<RefInputBillBasic> RefInputBillBasic { get; set; }
        public virtual DbSet<Stock> Stock { get; set; }
        public virtual DbSet<StockProduct> StockProduct { get; set; }
        public virtual DbSet<VMappingOusideImportObject> VMappingOusideImportObject { get; set; }

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
                entity.HasIndex(e => new { e.SubsidiaryId, e.InventoryCode })
                    .HasName("IX_Inventory_InventoryCode")
                    .IsUnique()
                    .HasFilter("([IsDeleted]=(0))");

                entity.HasIndex(e => new { e.InventoryCode, e.IsDeleted, e.IsApproved, e.SubsidiaryId })
                    .HasName("IX_Inventory_IsApproved");

                entity.Property(e => e.AccountancyAccountNumber).HasMaxLength(128);

                entity.Property(e => e.BillCode)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.BillForm).HasMaxLength(128);

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

            modelBuilder.Entity<InventoryChange>(entity =>
            {
                entity.HasKey(e => e.InventoryId)
                    .HasName("PK_InventoryTracking");

                entity.Property(e => e.InventoryId).ValueGeneratedNever();
            });

            modelBuilder.Entity<InventoryDetail>(entity =>
            {
                entity.HasIndex(e => new { e.InventoryId, e.PrimaryQuantity, e.IsDeleted, e.SubsidiaryId, e.InventoryRequirementDetailId })
                    .HasName("IX_InventoryDetail_InventoryRequirementDetailId");

                entity.HasIndex(e => new { e.InventoryId, e.ProductId, e.PrimaryQuantity, e.ProductUnitConversionId, e.IsDeleted })
                    .HasName("IDX_InventoryDetail_Product");

                entity.HasIndex(e => new { e.ProductId, e.RefObjectCode, e.OrderCode, e.Pocode, e.ProductionOrderCode, e.InventoryId, e.IsDeleted, e.SubsidiaryId })
                    .HasName("IDX_InventoryDetail_Search");

                entity.Property(e => e.AccountancyAccountNumberDu).HasMaxLength(128);

                entity.Property(e => e.CreatedByUserId).HasDefaultValueSql("((2))");

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.FromPackageId).HasComment("Xuất kho vào kiện nào");

                entity.Property(e => e.InventoryRequirementCode).HasMaxLength(128);

                entity.Property(e => e.OrderCode)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.PackageOptionId).HasDefaultValueSql("((0))");

                entity.Property(e => e.Pocode)
                    .HasColumnName("POCode")
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.PrimaryQuantityRemaning).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductUnitConversionPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ProductUnitConversionQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductUnitConversionQuantityRemaning).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductionOrderCode)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.RefObjectCode)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.RequestPrimaryQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.RequestProductUnitConversionQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ToPackageId).HasComment("Nhập kho vào kiện nào");

                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("((2))");

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
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InventoryDetail_ProductUnitConversion");

                entity.HasOne(d => d.ToPackage)
                    .WithMany(p => p.InventoryDetailToPackage)
                    .HasForeignKey(d => d.ToPackageId)
                    .HasConstraintName("FK_InventoryDetail_ToPackage");
            });

            modelBuilder.Entity<InventoryDetailChange>(entity =>
            {
                entity.HasKey(e => e.InventoryDetailId)
                    .HasName("PK_InventoryDetailQuantityTracking");

                entity.Property(e => e.InventoryDetailId).ValueGeneratedNever();

                entity.Property(e => e.CreatedByUserId).HasDefaultValueSql("((2))");

                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.OldPrimaryQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.OldPuConversionQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("((2))");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<InventoryDetailToPackage>(entity =>
            {
                entity.HasKey(e => new { e.InventoryDetailId, e.ToPackageId });

                entity.Property(e => e.CreatedByUserId).HasDefaultValueSql("((2))");

                entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("((2))");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");

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

            modelBuilder.Entity<InventoryRequirement>(entity =>
            {
                entity.HasIndex(e => new { e.SubsidiaryId, e.InventoryTypeId, e.InventoryRequirementCode })
                    .HasName("IX_InventoryRequirement_InventoryRequirementCode")
                    .IsUnique()
                    .HasFilter("([IsDeleted]=(0))");

                entity.Property(e => e.BillCode)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.BillForm).HasMaxLength(128);

                entity.Property(e => e.BillSerial)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.CensorStatus).HasDefaultValueSql("((1))");

                entity.Property(e => e.Content).HasMaxLength(512);

                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.InventoryRequirementCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ModuleTypeId).HasDefaultValueSql("((2))");

                entity.Property(e => e.Shipper).HasMaxLength(128);

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<InventoryRequirementDetail>(entity =>
            {
                entity.Property(e => e.OrderCode)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.OutsourceStepRequestCode).HasMaxLength(64);

                entity.Property(e => e.Pocode)
                    .HasColumnName("POCode")
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductUnitConversionPrice).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.ProductUnitConversionQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductionOrderCode)
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(18, 4)")
                    .HasDefaultValueSql("((0))");

                entity.HasOne(d => d.AssignStock)
                    .WithMany(p => p.InventoryRequirementDetail)
                    .HasForeignKey(d => d.AssignStockId)
                    .HasConstraintName("FK_InventoryRequirementDetail_Stock");

                entity.HasOne(d => d.InventoryRequirement)
                    .WithMany(p => p.InventoryRequirementDetail)
                    .HasForeignKey(d => d.InventoryRequirementId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InventoryRequirementDetail_InventoryRequirement");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.InventoryRequirementDetail)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InventoryRequirementDetail_Product");

                entity.HasOne(d => d.ProductUnitConversion)
                    .WithMany(p => p.InventoryRequirementDetail)
                    .HasForeignKey(d => d.ProductUnitConversionId)
                    .HasConstraintName("FK_InventoryRequirementDetail_ProductUnitConversion");
            });

            modelBuilder.Entity<InventoryRequirementFile>(entity =>
            {
                entity.HasKey(e => new { e.InventoryRequirementId, e.FileId })
                    .HasName("PK_InventoryFile_copy1");

                entity.HasOne(d => d.InventoryRequirement)
                    .WithMany(p => p.InventoryRequirementFile)
                    .HasForeignKey(d => d.InventoryRequirementId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_InventoryRequirementFile_InventoryRequirement");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.Property(e => e.CreatedByUserId).HasDefaultValueSql("((2))");

                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description)
                    .HasMaxLength(256)
                    .HasDefaultValueSql("(N'')");

                entity.Property(e => e.Name)
                    .HasMaxLength(128)
                    .HasDefaultValueSql("(N'')");

                entity.Property(e => e.Status).HasDefaultValueSql("((0))");

                entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("((2))");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<Package>(entity =>
            {
                entity.HasIndex(e => new { e.SubsidiaryId, e.PackageCode })
                    .HasName("IX_Package_PackageCode")
                    .IsUnique()
                    .HasFilter("([IsDeleted]=(0) AND [PackageCode]<>'' AND [PackageCode] IS NOT NULL)");

                entity.Property(e => e.CreatedByUserId).HasDefaultValueSql("((2))");

                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Date).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.OrderCode).HasMaxLength(64);

                entity.Property(e => e.PackageCode)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.PackageTypeId).HasDefaultValueSql("((1))");

                entity.Property(e => e.Pocode)
                    .HasColumnName("POCode")
                    .HasMaxLength(64)
                    .HasComment("Purchasing Order Code");

                entity.Property(e => e.PrimaryQuantityRemaining).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.PrimaryQuantityWaiting).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductUnitConversionRemaining).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductUnitConversionWaitting).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductionOrderCode).HasMaxLength(64);

                entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("((2))");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.Package)
                    .HasForeignKey(d => d.LocationId)
                    .HasConstraintName("FK_Package_Location");

                entity.HasOne(d => d.ProductUnitConversion)
                    .WithMany(p => p.Package)
                    .HasForeignKey(d => d.ProductUnitConversionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
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

                entity.Property(e => e.CreatedByUserId).HasDefaultValueSql("((2))");

                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PrimaryQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductUnitConversionQuantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("((2))");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");

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

                entity.HasIndex(e => new { e.SubsidiaryId, e.ProductCode })
                    .HasName("IX_Product_ProductCode_Unique")
                    .IsUnique()
                    .HasFilter("([IsDeleted]=(0))");

                entity.Property(e => e.Barcode).HasMaxLength(128);

                entity.Property(e => e.Coefficient)
                    .HasDefaultValueSql("((1))")
                    .HasComment("Cơ số sản phẩm");

                entity.Property(e => e.Color).HasMaxLength(128);

                entity.Property(e => e.EstimatePrice).HasColumnType("decimal(19, 4)");

                entity.Property(e => e.GrossWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Height).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.LoadAbility).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Long).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.Measurement).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.NetWeight).HasColumnType("decimal(18, 4)");

                entity.Property(e => e.PackingMethod).HasMaxLength(255);

                entity.Property(e => e.ProductCode)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasComment("Mã sản phẩm");

                entity.Property(e => e.ProductInternalName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasDefaultValueSql("('')")
                    .HasComment("Tên nội bộ");

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasComment("Tên sản phẩm");

                entity.Property(e => e.ProductNameEng).HasMaxLength(255);

                entity.Property(e => e.ProductStatusId).HasDefaultValueSql("((1))");

                entity.Property(e => e.Quantitative).HasColumnType("decimal(18, 4)");

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

            modelBuilder.Entity<ProductAttachment>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Title).HasMaxLength(1024);

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductAttachment)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ProductBo__Produ__60D24498");
            });

            modelBuilder.Entity<ProductBom>(entity =>
            {
                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description).HasMaxLength(1024);

                entity.Property(e => e.Quantity).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Wastage).HasColumnType("decimal(32, 16)");

                entity.HasOne(d => d.ChildProduct)
                    .WithMany(p => p.ProductBomChildProduct)
                    .HasForeignKey(d => d.ChildProductId)
                    .HasConstraintName("FK_BillOfMaterial_Product_ParentProductId");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductBomProduct)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BillOfMaterial_Product_ProductId");
            });


            modelBuilder.Entity<ProductCate>(entity =>
            {
                entity.Property(e => e.CreatedByUserId).HasDefaultValueSql("((2))");

                entity.Property(e => e.ProductCateName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("((2))");

                entity.HasOne(d => d.ParentProductCate)
                    .WithMany(p => p.InverseParentProductCate)
                    .HasForeignKey(d => d.ParentProductCateId)
                    .HasConstraintName("FK_ProductCate_ProductCate");
            });

            modelBuilder.Entity<ProductCustomer>(entity =>
            {
                entity.Property(e => e.CustomerProductCode).HasMaxLength(128);

                entity.Property(e => e.CustomerProductName).HasMaxLength(128);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductCustomer)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductCustomer_Product");
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
                    .HasConstraintName("FK_ProductExtraInfo_Product");
            });

            modelBuilder.Entity<ProductMaterial>(entity =>
            {
                entity.HasIndex(e => e.RootProductId)
                    .HasName("IDX_RootProductId");

                entity.Property(e => e.PathProductIds).IsRequired();

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductMaterialProduct)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductMaterial_MaterialProduct");

                entity.HasOne(d => d.RootProduct)
                    .WithMany(p => p.ProductMaterialRootProduct)
                    .HasForeignKey(d => d.RootProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductMaterial_RootProduct");
            });

            modelBuilder.Entity<ProductMaterialsConsumption>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.Quantity).HasColumnType("decimal(32, 16)");

                entity.HasOne(d => d.MaterialsConsumption)
                    .WithMany(p => p.ProductMaterialsConsumptionMaterialsConsumption)
                    .HasForeignKey(d => d.MaterialsConsumptionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductMaterialsConsumption_Product1");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductMaterialsConsumptionProduct)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductMaterialsConsumption_Product");

                entity.HasOne(d => d.ProductMaterialsConsumptionGroup)
                    .WithMany(p => p.ProductMaterialsConsumption)
                    .HasForeignKey(d => d.ProductMaterialsConsumptionGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductMaterialsConsumption_ProductMaterialsConsumptionGroup");
            });

            modelBuilder.Entity<ProductMaterialsConsumptionGroup>(entity =>
            {
                entity.Property(e => e.ProductMaterialsConsumptionGroupCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<ProductProperty>(entity =>
            {
                entity.HasIndex(e => e.RootProductId)
                    .HasName("IDX_RootProductId");

                entity.Property(e => e.PathProductIds).IsRequired();

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductPropertyProduct)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductProperty_ProductProperty");

                entity.HasOne(d => d.Property)
                    .WithMany(p => p.ProductProperty)
                    .HasForeignKey(d => d.PropertyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductProperty_Property");

                entity.HasOne(d => d.RootProduct)
                    .WithMany(p => p.ProductPropertyRootProduct)
                    .HasForeignKey(d => d.RootProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductProperty_RootProduct");
            });

            modelBuilder.Entity<ProductStockInfo>(entity =>
            {
                entity.HasKey(e => e.ProductId);

                entity.Property(e => e.ProductId).ValueGeneratedNever();

                entity.Property(e => e.DescriptionToStock).HasMaxLength(512);

                entity.HasOne(d => d.Product)
                    .WithOne(p => p.ProductStockInfo)
                    .HasForeignKey<ProductStockInfo>(d => d.ProductId)
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

                entity.Property(e => e.DecimalPlace).HasDefaultValueSql("((12))");

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
                    .HasConstraintName("FK_ProductUnitConversion_Product");
            });

            modelBuilder.Entity<Property>(entity =>
            {
                entity.Property(e => e.PropertyName).IsRequired();
            });

            modelBuilder.Entity<RefCustomerBasic>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("RefCustomerBasic");

                entity.Property(e => e.CustomerCode).HasMaxLength(128);

                entity.Property(e => e.CustomerId).ValueGeneratedOnAdd();

                entity.Property(e => e.CustomerName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<RefInputBillBasic>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("RefInputBillBasic");

                entity.Property(e => e.InputBillFId).HasColumnName("InputBill_F_Id");

                entity.Property(e => e.SoCt)
                    .HasColumnName("so_ct")
                    .HasMaxLength(512);
            });

            modelBuilder.Entity<Stock>(entity =>
            {
                entity.Property(e => e.CreatedByUserId).HasDefaultValueSql("((2))");

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.Status).HasDefaultValueSql("((0))");

                entity.Property(e => e.StockKeeperName).HasMaxLength(64);

                entity.Property(e => e.StockName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Type).HasDefaultValueSql("((0))");

                entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("((2))");
            });

            modelBuilder.Entity<StockProduct>(entity =>
            {
                entity.HasIndex(e => new { e.StockId, e.ProductId, e.ProductUnitConversionId })
                    .HasName("idx_StockProduct_StockId_ProductId_ProductUnitConversionId")
                    .IsUnique();

                entity.Property(e => e.CreatedByUserId).HasDefaultValueSql("((2))");

                entity.Property(e => e.CreatedDatetimeUtc).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PrimaryQuantityRemaining).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.PrimaryQuantityWaiting).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductUnitConversionRemaining).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.ProductUnitConversionWaitting).HasColumnType("decimal(32, 16)");

                entity.Property(e => e.UpdatedByUserId).HasDefaultValueSql("((2))");

                entity.Property(e => e.UpdatedDatetimeUtc).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<VMappingOusideImportObject>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vMappingOusideImportObject");

                entity.Property(e => e.InputBillFId).HasColumnName("InputBill_F_Id");

                entity.Property(e => e.MappingFunctionKey)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.SourceId)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
