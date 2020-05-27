DECLARE @tblProductUnitConversionQuantity TABLE(
	ProductUnitConversionId int,
	Quantity decimal(32,16)
)

BEGIN TRANSACTION

TRUNCATE TABLE dbo.InventoryChange
TRUNCATE TABLE dbo.InventoryDetailChange

DECLARE @LastStockId INT = 0
DECLARE @LastProductId INT = 0
DECLARE @LastPrimaryQuantity DECIMAL(32,16)=0
DECLARE @LastProductUnitConversionQuantity DECIMAL(32,16)=0

DECLARE @InventoryTypeId INT
DECLARE @InventoryDetailId BIGINT
DECLARE @InventoryId BIGINT
DECLARE @Date DATETIME2
DECLARE @StockId INT
DECLARE @ProductId INT
DECLARE @ProductUnitConversionId INT
DECLARE @ProductUnitConversionQuantity INT
DECLARE @PrimaryQuantity DECIMAL(32,16)


DECLARE cursor_product CURSOR LOCAL READ_ONLY FORWARD_ONLY
FOR SELECT 
		iv.InventoryTypeId,
		d.InventoryDetailId,
		iv.InventoryId,
		iv.[Date],
        iv.StockId,
		d.ProductId,
		d.ProductUnitConversionId,
		d.ProductUnitConversionQuantity,
		d.PrimaryQuantity         
    FROM dbo.Inventory iv 
		JOIN dbo.InventoryDetail d ON d.InventoryId = iv.InventoryId
	WHERE iv.IsApproved = 1 
		AND iv.IsDeleted = 0 
		AND d.IsDeleted = 0
	ORDER BY iv.StockId, 
			d.ProductId, 
			iv.[Date], 
			iv.InventoryTypeId, 
			iv.InventoryId, 
			d.InventoryDetailId

OPEN cursor_product;

FETCH NEXT FROM cursor_product INTO 
	@InventoryTypeId,
	@InventoryDetailId,
	@InventoryId,
	@Date,
    @StockId, 
    @ProductId,
	@ProductUnitConversionId,
	@ProductUnitConversionQuantity,
	@PrimaryQuantity;

WHILE @@FETCH_STATUS = 0
    BEGIN
        
		IF @StockId <> @LastStockId OR @ProductId <> @LastProductId
		BEGIN			
		    SET @LastPrimaryQuantity = 0
			DELETE FROM @tblProductUnitConversionQuantity
		END

		SELECT @LastProductUnitConversionQuantity = Quantity FROM @tblProductUnitConversionQuantity WHERE ProductUnitConversionId = @ProductUnitConversionId

		IF @LastProductUnitConversionQuantity IS NULL
		BEGIN
			INSERT INTO @tblProductUnitConversionQuantity
			(
			    ProductUnitConversionId,
			    Quantity
			)
			VALUES
			(
			    @ProductUnitConversionId, -- ProductUnitConversionId - int
			    @ProductUnitConversionQuantity -- Quantity - decimal
			)
			SET @LastProductUnitConversionQuantity = 0
		END

		SET @LastStockId = @StockId
		SET @LastProductId = @ProductId

		IF @InventoryTypeId = 1
		BEGIN
		    SET @LastPrimaryQuantity = @LastPrimaryQuantity + @PrimaryQuantity
			SET @LastProductUnitConversionQuantity = @LastProductUnitConversionQuantity + @ProductUnitConversionQuantity
		END
		ELSE
		BEGIN
		    SET @LastPrimaryQuantity = @LastPrimaryQuantity - @PrimaryQuantity
			SET @LastProductUnitConversionQuantity = @LastProductUnitConversionQuantity - @ProductUnitConversionQuantity
		END
		
		UPDATE TOP(1) dbo.InventoryDetail SET PrimaryQuantityRemaning = @LastPrimaryQuantity, ProductUnitConversionQuantityRemaning = @LastProductUnitConversionQuantity  WHERE InventoryDetailId = @InventoryDetailId

		IF NOT EXISTS (SELECT 0 FROM dbo.InventoryChange WHERE InventoryId = @InventoryId)
		BEGIN
		    INSERT dbo.InventoryChange
		    (
		        InventoryId,
		        OldDate,
		        IsSync,
		        LastSyncTime
		    )
		    VALUES
		    (   @InventoryId,             -- InventoryId - bigint
		        @Date, -- OldDate - datetime2(7)
		        1,          -- IsSync - bit
		        GETUTCDATE()  -- LastSyncTime - datetime2(7)
		        )
		END

		INSERT INTO dbo.InventoryDetailChange
		(
		    InventoryDetailId,
		    InventoryId,
		    StockId,
		    OldPrimaryQuantity,
		    IsDeleted,
		    ProductId
		)
		VALUES
		(   @InventoryDetailId,    -- InventoryDetailId - bigint
		    @InventoryId,    -- InventoryId - bigint
		    @StockId,    -- StockId - int
		    @PrimaryQuantity, -- OldPrimaryQuantity - decimal(32, 16)
		    0, -- IsDeleted - bit
		    @ProductId     -- ProductId - int
		)


        FETCH NEXT FROM cursor_product INTO
			@InventoryTypeId,
			@InventoryDetailId,
			@InventoryId,
			@Date,
			@StockId, 
			@ProductId,
			@ProductUnitConversionId,
			@ProductUnitConversionQuantity,
			@PrimaryQuantity;
    END;

CLOSE cursor_product;

DEALLOCATE cursor_product;

COMMIT