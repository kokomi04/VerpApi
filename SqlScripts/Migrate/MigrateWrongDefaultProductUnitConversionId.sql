SET NOCOUNT ON;  
  
DECLARE @StockId int
DECLARE @InventoryDetailId bigint
DECLARE @ProductId int
DECLARE @ProductUnitConversionId int
DECLARE @PrimaryQuantity decimal
DECLARE @ProductUnitConversionQuantity decimal


  
DECLARE inventoryDetail_cursor CURSOR LOCAL FOR  
 
SELECT 
	i.StockId, 
	id.InventoryDetailId, 
	id.ProductId, 
	id.ProductUnitConversionId, 
	id.PrimaryQuantity, 
	id.ProductUnitConversionQuantity 
FROM dbo.InventoryDetail id
	JOIN dbo.Inventory i ON id.InventoryId = i.InventoryId
	JOIN dbo.ProductUnitConversion puc ON id.ProductUnitConversionId = puc.ProductUnitConversionId
	LEFT JOIN dbo.Product p ON puc.ProductId = p.ProductId AND id.ProductId = p.ProductId
WHERE p.ProductId IS NULL AND puc.IsDefault=1 AND i.IsApproved=1
  
OPEN inventoryDetail_cursor  
  
FETCH NEXT FROM inventoryDetail_cursor   
INTO @StockId, 
	@InventoryDetailId, 
	@ProductId, 
	@ProductUnitConversionId, 
	@PrimaryQuantity, 
	@ProductUnitConversionQuantity
  
WHILE @@FETCH_STATUS = 0  
BEGIN  

	DECLARE @DefaultUnitConversionId int = 0;	
	DECLARE @DefaultToPackgeId bigint = 0;
	DECLARE @DefaultStockProductId bigint = 0;

	SELECT @DefaultUnitConversionId = puc.ProductUnitConversionId 
		FROM dbo.ProductUnitConversion puc 
		WHERE puc.ProductId = @ProductId AND puc.IsDefault = 1

	SELECT @DefaultToPackgeId = p.PackageId 
		FROM dbo.Package p 
		WHERE p.StockId = @StockId AND p.ProductId = @ProductId AND p.ProductUnitConversionId = @DefaultUnitConversionId

	SELECT @DefaultStockProductId = sp.StockProductId 
		FROM dbo.StockProduct sp 
		WHERE sp.StockId = @StockId AND sp.ProductId = @ProductId AND sp.ProductUnitConversionId = @DefaultUnitConversionId

	IF ISNULL(@DefaultToPackgeId,0) = 0
	BEGIN
		INSERT INTO dbo.Package
		(
		    --PackageId - column value is auto-generated
		    PackageTypeId,
		    PackageCode,
		    LocationId,
		    StockId,
		    ProductId,
		    ProductUnitConversionId,
		    PrimaryQuantityWaiting,
		    PrimaryQuantityRemaining,
		    ProductUnitConversionWaitting,
		    ProductUnitConversionRemaining,
		    [Date],
		    ExpiryTime,
		    CreatedDatetimeUtc,
		    UpdatedDatetimeUtc,
		    IsDeleted
		)
		VALUES
		(
		    -- PackageId - bigint
		    1, -- PackageTypeId - int
		    N'', -- PackageCode - nvarchar
		    NULL, -- LocationId - int
		    @StockId, -- StockId - int
		    @ProductId, -- ProductId - int
		    @DefaultUnitConversionId, -- ProductUnitConversionId - int
		    0, -- PrimaryQuantityWaiting - decimal
		    0, -- PrimaryQuantityRemaining - decimal
		    0, -- ProductUnitConversionWaitting - decimal
		    0, -- ProductUnitConversionRemaining - decimal
		    NULL, -- Date - datetime2
		    NULL, -- ExpiryTime - datetime2
		    GETDATE(), -- CreatedDatetimeUtc - datetime2
		    GETDATE(), -- UpdatedDatetimeUtc - datetime2
		    0 -- IsDeleted - bit
		)
		SET @DefaultToPackgeId = Scope_identity()
	END

	IF ISNULL(@DefaultStockProductId,0) = 0
	BEGIN
		INSERT INTO dbo.StockProduct
		(
		    StockId,
		    ProductId,
		    ProductUnitConversionId,
		    PrimaryQuantityWaiting,
		    PrimaryQuantityRemaining,
		    ProductUnitConversionWaitting,
		    ProductUnitConversionRemaining,
		    UpdatedDatetimeUtc
		    --StockProductId - column value is auto-generated
		)
		VALUES
		(
		    @StockId, -- StockId - int
		    @ProductId, -- ProductId - int
		    @DefaultUnitConversionId, -- ProductUnitConversionId - int
		    0, -- PrimaryQuantityWaiting - decimal
		    0, -- PrimaryQuantityRemaining - decimal
		    0, -- ProductUnitConversionWaitting - decimal
		    0, -- ProductUnitConversionRemaining - decimal
		    GETDATE() -- UpdatedDatetimeUtc - datetime2
		    -- StockProductId - int
		)
		SET @DefaultStockProductId = Scope_identity()
	END

	DECLARE @Old_UnitConversionId				int = @ProductUnitConversionId;	
	DECLARE @Old_PrimaryQuantityWaiting			decimal = 0;
	DECLARE @Old_PrimaryQuantityRemaining		decimal = 0;
	DECLARE @Old_ProductUnitConversionWaitting	decimal = 0;
	DECLARE @Old_ProductUnitConversionRemaining decimal = 0;
	
	SELECT  @Old_ToPackgeId						= p.PackageId,
			@Old_PrimaryQuantityWaiting			= PrimaryQuantityWaiting,
			@Old_PrimaryQuantityRemaining		= PrimaryQuantityRemaining,
			@Old_ProductUnitConversionWaitting	= ProductUnitConversionWaitting,
			@Old_ProductUnitConversionRemaining	= ProductUnitConversionRemaining	
		FROM dbo.Package p 
		WHERE p.StockId = @StockId AND p.ProductId = @ProductId AND p.ProductUnitConversionId = @Old_UnitConversionId AND p.IsDeleted = 0
	
	SELECT @Old_StockProductId = sp.StockProductId 
		FROM dbo.StockProduct sp 
		WHERE sp.StockId = @StockId AND sp.ProductId = @ProductId AND sp.ProductUnitConversionId = @Old_UnitConversionId

	UPDATE dbo.Package
		SET PrimaryQuantityWaiting			= PrimaryQuantityWaiting			+ @Old_PrimaryQuantityWaiting, -- decimal
			PrimaryQuantityRemaining		= PrimaryQuantityRemaining			+ @Old_PrimaryQuantityRemaining, -- decimal
			ProductUnitConversionWaitting	= ProductUnitConversionWaitting		+ @Old_ProductUnitConversionWaitting, -- decimal
			ProductUnitConversionRemaining	= ProductUnitConversionRemaining	+ @Old_ProductUnitConversionRemaining-- decimal	  
		WHERE PackageId=@DefaultToPackgeId
	
	UPDATE dbo.StockProduct
		SET PrimaryQuantityWaiting			= PrimaryQuantityWaiting			+ @Old_PrimaryQuantityWaiting, -- decimal
			PrimaryQuantityRemaining		= PrimaryQuantityRemaining			+ @Old_PrimaryQuantityRemaining, -- decimal
			ProductUnitConversionWaitting	= ProductUnitConversionWaitting		+ @Old_ProductUnitConversionWaitting, -- decimal
			ProductUnitConversionRemaining	= ProductUnitConversionRemaining	+ @Old_ProductUnitConversionRemaining-- decimal	  
		WHERE StockProductId = @DefaultStockProductId

	UPDATE top(1) InventoryDetail SET ProductUnitConversionId = @DefaultUnitConversionId WHERE InventoryDetailId = @InventoryDetailId
	
	UPDATE TOP(1) dbo.InventoryDetailToPackage
		SET
			ToPackageId = @DefaultToPackgeId
		WHERE InventoryDetailId = @InventoryDetailId

	UPDATE TOP(1) dbo.Package
		SET IsDeleted = 1
		WHERE PackageId = @Old_ToPackgeId

	DELETE TOP(1) dbo.StockProduct WHERE StockProductId = @Old_StockProductId
	
    FETCH NEXT FROM inventoryDetail_cursor   
    INTO INTO @StockId, 
			@InventoryDetailId, 
			@ProductId, 
			@ProductUnitConversionId, 
			@PrimaryQuantity, 
			@ProductUnitConversionQuantity
END   
CLOSE inventoryDetail_cursor;  
DEALLOCATE inventoryDetail_cursor;  