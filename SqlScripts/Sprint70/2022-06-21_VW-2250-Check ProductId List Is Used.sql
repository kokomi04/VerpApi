USE [StockDB]
GO
/****** Object:  StoredProcedure [dbo].[asp_Product_CheckUsed_ByList]    Script Date: 6/21/2022 12:48:19 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP PROCEDURE IF EXISTS [dbo].[asp_Product_CheckUsed_ByList]
GO

CREATE PROCEDURE [dbo].[asp_Product_CheckUsed_ByList]
	@ProductIds [_INTVALUES] READONLY,
	@OutProductId int OUTPUT
AS
BEGIN
	SET @OutProductId = null;
	--StockDB
	SELECT TOP 1 @OutProductId = d.ProductId FROM StockDB.dbo.InventoryDetail d WHERE d.ProductId in (Select [Value] from @ProductIds) AND d.IsDeleted=0
	IF @OutProductId IS NOT NULL RETURN 0;

	SELECT TOP 1 @OutProductId = d.ProductId FROM StockDB.dbo.InventoryRequirementDetail d WHERE d.ProductId in (Select [Value] from @ProductIds) AND d.IsDeleted=0
	IF @OutProductId IS NOT NULL RETURN 0;

	SELECT TOP 1 @OutProductId = d.ProductId FROM StockDB.dbo.StockProduct d WHERE d.ProductId in (Select [Value] from @ProductIds) AND d.IsDeleted=0
	IF @OutProductId IS NOT NULL RETURN 0;

	SELECT TOP 1 @OutProductId = d.ChildProductId FROM StockDB.dbo.ProductBom d WHERE d.ChildProductId in (Select [Value] from @ProductIds) AND d.IsDeleted=0
	IF @OutProductId IS NOT NULL RETURN 0;

	SELECT TOP 1 @OutProductId = d.ProductId FROM StockDB.dbo.ProductMaterialsConsumption d WHERE d.MaterialsConsumptionId in (Select [Value] from @ProductIds) AND d.IsDeleted=0
	IF @OutProductId IS NOT NULL RETURN 0;
	

	--PurchaseOrderDB

	SELECT TOP 1 @OutProductId = d.ProductId FROM PurchaseOrderDB.dbo.PurchasingRequestDetail d WHERE d.ProductId in (Select [Value] from @ProductIds) AND d.IsDeleted=0
	IF @OutProductId IS NOT NULL RETURN 0;

	SELECT TOP 1 @OutProductId = d.ProductId FROM PurchaseOrderDB.dbo.PurchasingSuggestDetail d WHERE d.ProductId in (Select [Value] from @ProductIds) AND d.IsDeleted=0
	IF @OutProductId IS NOT NULL RETURN 0;

	SELECT TOP 1 @OutProductId = d.ProductId FROM PurchaseOrderDB.dbo.PurchaseOrderDetail d WHERE d.ProductId in (Select [Value] from @ProductIds) AND d.IsDeleted=0
	IF @OutProductId IS NOT NULL RETURN 0;

	SELECT TOP 1 @OutProductId = d.ProductId FROM PurchaseOrderDB.dbo.PoProviderPricingDetail d WHERE d.ProductId in (Select [Value] from @ProductIds) AND d.IsDeleted=0
	IF @OutProductId IS NOT NULL RETURN 0;

	SELECT TOP 1 @OutProductId = d.ProductId FROM PurchaseOrderDB.dbo.ProviderProductInfo d WHERE d.ProductId in (Select [Value] from @ProductIds)
	IF @OutProductId IS NOT NULL RETURN 0;

	--Voucher
	
	SELECT TOP 1 @OutProductId = d.vthhtp FROM PurchaseOrderDB.dbo.VoucherValueRow d WHERE d.vthhtp in (Select [Value] from @ProductIds) AND d.IsDeleted=0
	IF @OutProductId IS NOT NULL RETURN 0;

	--Accountancy
	SELECT TOP 1 @OutProductId = d.vthhtp FROM AccountancyDB.dbo.InputValueRow d WHERE d.vthhtp in (Select [Value] from @ProductIds) AND d.IsDeleted=0
	IF @OutProductId IS NOT NULL RETURN 0;

	--Maufacturing
	SELECT TOP 1 @OutProductId = d.ProductId FROM ManufacturingDB.dbo.ProductionOrderDetail d WHERE d.ProductId in (Select [Value] from @ProductIds) AND d.IsDeleted=0
	IF @OutProductId IS NOT NULL RETURN 0;
	
	RETURN 0;
END
