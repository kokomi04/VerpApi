USE [StockDB]
GO

/****** Object:  StoredProcedure [dbo].[asp_Product_CheckUsed]    Script Date: 6/21/2022 10:06:22 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP PROCEDURE IF EXISTS [dbo].[asp_Product_CheckUsed_ByList]
GO

DROP TYPE IF EXISTS [dbo].[ListId]
GO

CREATE TYPE [dbo].[ListId] AS TABLE(
    [Item] INT
)
GO

CREATE PROCEDURE [dbo].[asp_Product_CheckUsed_ByList]
	@ProductId [ListId] READONLY,
	@IsUsed BIT OUTPUT
AS
BEGIN
	SET @IsUsed = 0
	--StockDB
	IF EXISTS(SELECT 0 FROM StockDB.dbo.InventoryDetail d WHERE d.ProductId in (Select [Item] from @ProductId) AND d.IsDeleted=0)
	BEGIN
		SET @IsUsed = 1
		RETURN 0;
	END

	IF EXISTS(SELECT 0 FROM StockDB.dbo.InventoryRequirementDetail d WHERE d.ProductId in (Select [Item] from @ProductId) AND d.IsDeleted=0)
	BEGIN
		SET @IsUsed = 1
		RETURN 0;
	END

	IF EXISTS(SELECT 0 FROM StockDB.dbo.StockProduct d WHERE d.ProductId in (Select [Item] from @ProductId) AND d.IsDeleted=0)
	BEGIN
		SET @IsUsed = 1
		RETURN 0;
	END

	IF EXISTS(SELECT 0 FROM StockDB.dbo.ProductBom d WHERE d.ChildProductId in (Select [Item] from @ProductId) AND d.IsDeleted=0)
	BEGIN
		SET @IsUsed = 1
		RETURN 0;
	END

	IF EXISTS(SELECT 0 FROM StockDB.dbo.ProductMaterialsConsumption d WHERE d.MaterialsConsumptionId in (Select [Item] from @ProductId) AND d.IsDeleted=0)
	BEGIN
		SET @IsUsed = 1
		RETURN 0;
	END
	

	--PurchaseOrderDB

	IF EXISTS(SELECT 0 FROM PurchaseOrderDB.dbo.PurchasingRequestDetail d WHERE d.ProductId in (Select [Item] from @ProductId) AND d.IsDeleted=0)
	BEGIN
		SET @IsUsed = 1
		RETURN 0;
	END

	IF EXISTS(SELECT 0 FROM PurchaseOrderDB.dbo.PurchasingSuggestDetail d WHERE d.ProductId in (Select [Item] from @ProductId) AND d.IsDeleted=0)
	BEGIN
		SET @IsUsed = 1
		RETURN 0;
	END

	IF EXISTS(SELECT 0 FROM PurchaseOrderDB.dbo.PurchaseOrderDetail d WHERE d.ProductId in (Select [Item] from @ProductId) AND d.IsDeleted=0)
	BEGIN
		SET @IsUsed = 1
		RETURN 0;
	END

	IF EXISTS(SELECT 0 FROM PurchaseOrderDB.dbo.PoProviderPricingDetail d WHERE d.ProductId in (Select [Item] from @ProductId) AND d.IsDeleted=0)
	BEGIN
		SET @IsUsed = 1
		RETURN 0;
	END

	IF EXISTS(SELECT 0 FROM PurchaseOrderDB.dbo.ProviderProductInfo d WHERE d.ProductId in (Select [Item] from @ProductId))
	BEGIN
		SET @IsUsed = 1
		RETURN 0;
	END

	--Voucher
	
	IF EXISTS(SELECT 0 FROM PurchaseOrderDB.dbo.VoucherValueRow d WHERE d.vthhtp in (Select [Item] from @ProductId) AND d.IsDeleted=0)
	BEGIN
		SET @IsUsed = 1
		RETURN 0;
	END

	--Accountancy
	IF EXISTS(SELECT 0 FROM AccountancyDB.dbo.InputValueRow d WHERE d.vthhtp in (Select [Item] from @ProductId) AND d.IsDeleted=0)
	BEGIN
		SET @IsUsed = 1
		RETURN 0;
	END

	--Maufacturing
	IF EXISTS(SELECT 0 FROM ManufacturingDB.dbo.ProductionOrderDetail d WHERE d.ProductId in (Select [Item] from @ProductId) AND d.IsDeleted=0)
	BEGIN
		SET @IsUsed = 1
		RETURN 0;
	END
	
END
GO