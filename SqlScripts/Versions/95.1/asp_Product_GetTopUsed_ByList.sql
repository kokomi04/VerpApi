USE [StockDB]
GO
/****** Object:  StoredProcedure [dbo].[asp_Product_GetTopUsed_ByList]    Script Date: 7/21/2023 5:25:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[asp_Product_GetTopUsed_ByList]
	@SubId INT,
	@ProductIds [_INTVALUES] READONLY,
	@IsCheckExistOnly BIT = 0
AS
BEGIN
	

	DECLARE @result TABLE(
		ProductId INT NULL,
		Id BIGINT NULL,
		ObjectTypeId INT NULL,
		BillTypeId INT NULL,
		BillId BIGINT NULL,
		BillCode NVARCHAR(128) NULL,
		[Description] NVARCHAR(512) NULL
	)

	--StockDB
	DECLARE @InventoryType_Input INT = 1
	DECLARE @InventoryType_Output INT = 2
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		c.[Value],
		c.[Value],
		30,--InventoryInput,
		iv.InventoryTypeId,
		d.InventoryId,
		iv.InventoryCode,
		CONCAT(N'Nhập kho. Mã: ', iv.InventoryCode)		
	FROM StockDB.dbo.InventoryDetail d 
		JOIN StockDB.dbo.Inventory iv ON iv.InventoryId = d.InventoryId
		JOIN @ProductIds c ON d.ProductId = c.[Value]
	WHERE iv.IsDeleted = 0 AND d.IsDeleted=0 AND iv.InventoryTypeId = @InventoryType_Input
	ORDER BY d.InventoryId DESC

	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END

	--output
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		c.[Value],
		c.[Value],
		31,--InventoryOutput,
		iv.InventoryTypeId,
		d.InventoryId,
		iv.InventoryCode,
		CONCAT(N'Xuất kho. Mã: ', iv.InventoryCode)		
	FROM StockDB.dbo.InventoryDetail d 
		JOIN StockDB.dbo.Inventory iv ON iv.InventoryId = d.InventoryId
		JOIN @ProductIds c ON d.ProductId = c.[Value]
	WHERE iv.IsDeleted = 0 AND d.IsDeleted=0 AND iv.InventoryTypeId = @InventoryType_Output
	ORDER BY d.InventoryId DESC

	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END


	--require input
	
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		c.[Value],
		c.[Value],
		300,--RequestInventoryInput,
		iv.InventoryTypeId,
		d.InventoryRequirementId,
		iv.InventoryRequirementCode,
		CONCAT(N'Phiếu yêu cầu nhập kho. Mã: ', iv.InventoryRequirementCode)		
	FROM StockDB.dbo.InventoryRequirementDetail d 
		JOIN StockDB.dbo.InventoryRequirement iv ON iv.InventoryRequirementId = d.InventoryRequirementId
		JOIN @ProductIds c ON d.ProductId = c.[Value]
	WHERE iv.IsDeleted = 0 AND d.IsDeleted=0 AND iv.InventoryTypeId = @InventoryType_Input
	ORDER BY d.InventoryRequirementId DESC;

	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END

	--require out
	
	INSERT INTO @result
	(
	   ProductId,
	   Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		p.[Value],
		p.[Value],
		301,--RequestInventoryOutput,
		iv.InventoryTypeId,
		d.InventoryRequirementId,
		iv.InventoryRequirementCode,
		CONCAT(N'Phiếu yêu cầu xuất kho. Mã: ', iv.InventoryRequirementCode)		
	FROM StockDB.dbo.InventoryRequirementDetail d 
		JOIN StockDB.dbo.InventoryRequirement iv ON iv.InventoryRequirementId = d.InventoryRequirementId
		JOIN @ProductIds p ON d.ProductId = p.[Value]
	WHERE iv.IsDeleted = 0 AND d.IsDeleted=0 AND iv.InventoryTypeId = @InventoryType_Input
	ORDER BY d.InventoryRequirementId DESC;

	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END

	--Stock take
	
	INSERT INTO @result
	(
	   ProductId,
	   Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		p.[Value],
		p.[Value],
		553,--StockTakePeriod,
		NULL,
		po.StockTakePeriodId,
		po.StockTakePeriodCode,
		CONCAT(N'Kỳ kiểm kê kho. Mã: ', po.StockTakePeriodCode)		
	FROM StockDB.dbo.StockTakeDetail d 
		JOIN StockDB.dbo.StockTake iv ON iv.StockTakeId = d.StockTakeId
		JOIN @ProductIds p ON d.ProductId = p.[Value]
		JOIN StockDB.dbo.StockTakePeriod po ON po.StockTakePeriodId = iv.StockTakePeriodId 
	WHERE iv.IsDeleted = 0 AND d.IsDeleted=0 AND po.IsDeleted = 0
	ORDER BY po.StockTakePeriodId DESC;

	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END

	--SELECT TOP 1 @OutProductId = d.ProductId,
	--	@OutMessage = N'Mặt hàng đã từng nhập kho'
	--FROM StockDB.dbo.StockProduct d WHERE d.ProductId in (Select [Value] from @ProductIds) AND d.IsDeleted=0
	--IF @OutProductId IS NOT NULL RETURN 0;


	--BOM
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		c.[Value],
		c.[Value],
		6,--Product,
		NULL,
		p.ProductId,
		p.ProductCode,
		CONCAT(N'Thành phần cấu thành (BOM) mặt hàng. Mã: ', p.ProductCode)		
	FROM StockDB.dbo.Product p 
		 JOIN StockDB.dbo.ProductBom d ON p.ProductId = d.ProductId
		 JOIN @ProductIds c ON d.ChildProductId = c.[Value]
	WHERE p.IsDeleted = 0 AND d.IsDeleted=0
	ORDER BY p.ProductId DESC;

	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END

	--Consum
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		c.[Value],
		c.[Value],
		6,--Product,
		NULL,
		p.ProductId,
		p.ProductCode,
		CONCAT(N'Là vật tư tiêu hao của mặt hàng. Mã: ', p.ProductCode)		
	FROM StockDB.dbo.Product p 
		JOIN StockDB.dbo.ProductMaterialsConsumption d ON p.ProductId = d.ProductId
		JOIN @ProductIds c ON d.MaterialsConsumptionId = c.[Value]
	WHERE p.IsDeleted = 0 AND d.IsDeleted=0
	ORDER BY p.ProductId DESC;

	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END

	--PurchaseOrderDB

	--request
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		c.[Value],
		c.[Value],
		19,--PurchasingRequest,
		NULL,
		r.PurchasingRequestId,
		r.PurchasingRequestCode,
		CONCAT(N'Yêu cầu mua hàng. Mã: ', r.PurchasingRequestCode)		
	FROM PurchaseOrderDB.dbo.PurchasingRequestDetail d 
		 JOIN PurchaseOrderDB.dbo.PurchasingRequest r ON r.PurchasingRequestId = d.PurchasingRequestId
		 JOIN @ProductIds c ON d.ProductId = c.[Value]
	WHERE r.IsDeleted = 0 AND d.IsDeleted=0
	ORDER BY r.PurchasingRequestId DESC;

	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END

	--suggest
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		c.[Value],
		c.[Value],
		21,--PurchasingSuggest,
		NULL,
		s.PurchasingSuggestId,
		s.PurchasingSuggestCode,
		CONCAT(N'Đề nghị mua hàng. Mã: ', s.PurchasingSuggestCode)		
	FROM PurchaseOrderDB.dbo.PurchasingSuggestDetail d 
		 JOIN PurchaseOrderDB.dbo.PurchasingSuggest s ON s.PurchasingSuggestId = d.PurchasingSuggestId
		 JOIN @ProductIds c ON d.ProductId = c.[Value]
	WHERE s.IsDeleted = 0 AND d.IsDeleted=0
	ORDER BY s.PurchasingSuggestId DESC;

	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END

	--po
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		c.[Value],
		c.[Value],
		25,--PurchaseOrder,
		po.PurchaseOrderType,
		po.PurchaseOrderId,
		po.PurchaseOrderCode,
		CONCAT(N'Đơn đặt mua/gia công. Mã: ', po.PurchaseOrderCode)		
	FROM PurchaseOrderDB.dbo.PurchaseOrderDetail d 
		 JOIN PurchaseOrderDB.dbo.PurchaseOrder po ON po.PurchaseOrderId = d.PurchaseOrderId
		 JOIN @ProductIds c ON d.ProductId = c.[Value]
	WHERE po.IsDeleted = 0 AND d.IsDeleted=0
	ORDER BY po.PurchaseOrderId DESC;

	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END

	--PurchaseOrderExcess
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		c.[Value],
		c.[Value],
		25,--PurchaseOrder,
		po.PurchaseOrderType,
		po.PurchaseOrderId,
		po.PurchaseOrderCode,
		CONCAT(N'Vật tư dư thừa Đơn đặt mua/gia công. Mã: ', po.PurchaseOrderCode)		
	FROM PurchaseOrderDB.dbo.PurchaseOrderExcess d
		 JOIN PurchaseOrderDB.dbo.PurchaseOrder po ON po.PurchaseOrderId = d.PurchaseOrderId
		 JOIN @ProductIds c ON d.ProductId = c.[Value]
	WHERE po.IsDeleted = 0 AND d.IsDeleted=0
	ORDER BY po.PurchaseOrderId DESC;

	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END


	--PoProviderPricing
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		c.[Value],
		c.[Value],
		25001,--PoProviderPricing,
		NULL,
		r.PoProviderPricingId,
		r.PoProviderPricingCode,
		CONCAT(N'Báo giá nhà cung cấp. Mã: ', r.PoProviderPricingCode)		
	FROM PurchaseOrderDB.dbo.PoProviderPricingDetail d
		 JOIN PurchaseOrderDB.dbo.PoProviderPricing r ON d.PoProviderPricingId = r.PoProviderPricingId
		 JOIN @ProductIds c ON d.ProductId = c.[Value]
	WHERE r.IsDeleted = 0 AND d.IsDeleted=0
	ORDER BY r.PoProviderPricingId DESC;	

	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END


	--MaterialCalc
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		r.[Value],
		r.[Value],
		85,--MaterialCalc,
		NULL,
		r.MaterialCalcId,
		r.MaterialCalcCode,
		CONCAT(N'Tính nhu cầu vật tư. Mã: ', r.MaterialCalcCode)		
	FROM (
		SELECT c.[Value],
			   r.MaterialCalcId,
			   r.MaterialCalcCode
		FROM PurchaseOrderDB.dbo.MaterialCalcProduct d
		 JOIN PurchaseOrderDB.dbo.MaterialCalc r ON r.MaterialCalcId = d.MaterialCalcId
		 JOIN @ProductIds c ON d.ProductId = c.[Value]
		 WHERE r.IsDeleted = 0

		 UNION ALL

		 SELECT c.[Value],
			   r.MaterialCalcId,
			   r.MaterialCalcCode
		FROM PurchaseOrderDB.dbo.MaterialCalcProductDetail pd
		 JOIN PurchaseOrderDB.dbo.MaterialCalcProduct d ON pd.MaterialCalcProductId = d.MaterialCalcProductId
		 JOIN PurchaseOrderDB.dbo.MaterialCalc r ON r.MaterialCalcId = d.MaterialCalcId
		 JOIN @ProductIds c ON pd.MaterialProductId = c.[Value]
		 WHERE r.IsDeleted = 0

		  UNION ALL

		 SELECT c.[Value],
			   r.MaterialCalcId,
			   r.MaterialCalcCode
		  FROM PurchaseOrderDB.dbo.MaterialCalcSummary d		 
				 JOIN PurchaseOrderDB.dbo.MaterialCalc r ON r.MaterialCalcId = d.MaterialCalcId
				 JOIN @ProductIds c ON d.MaterialProductId = c.[Value]
		  WHERE r.IsDeleted = 0

	) r	
	ORDER BY r.MaterialCalcId DESC;	

	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END


	--PropertyCalc
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		r.[Value],
		r.[Value],
		402,--PropertyCalc,
		NULL,
		r.PropertyCalcId,
		r.PropertyCalcCode,
		CONCAT(N'Tính nhu cầu vật tư theo thuộc tính. Mã: ', r.PropertyCalcCode)		
	FROM (
		SELECT c.[Value],
			   r.PropertyCalcId,
			   r.PropertyCalcCode
		FROM PurchaseOrderDB.dbo.PropertyCalcProduct d
		 JOIN PurchaseOrderDB.dbo.PropertyCalc r ON r.PropertyCalcId = d.PropertyCalcId
		 JOIN @ProductIds c ON d.ProductId = c.[Value]
		 WHERE r.IsDeleted = 0

		 UNION ALL

		 SELECT c.[Value],
			   r.PropertyCalcId,
			   r.PropertyCalcCode
		FROM PurchaseOrderDB.dbo.PropertyCalcProductDetail pd
		 JOIN PurchaseOrderDB.dbo.PropertyCalcProduct d ON pd.PropertyCalcProductId = d.PropertyCalcProductId
		 JOIN PurchaseOrderDB.dbo.PropertyCalc r ON r.PropertyCalcId = d.PropertyCalcId
		 JOIN @ProductIds c ON pd.MaterialProductId = c.[Value]
		 WHERE r.IsDeleted = 0

		 UNION ALL

		 SELECT c.[Value],
			   r.PropertyCalcId,
			   r.PropertyCalcCode
		  FROM PurchaseOrderDB.dbo.PropertyCalcSummary d		 
				 JOIN PurchaseOrderDB.dbo.PropertyCalc r ON r.PropertyCalcId = d.PropertyCalcId
				 JOIN @ProductIds c ON d.MaterialProductId = c.[Value]
		  WHERE r.IsDeleted = 0

		  UNION ALL

		 SELECT c.[Value],
			   r.PropertyCalcId,
			   r.PropertyCalcCode
		  FROM PurchaseOrderDB.dbo.CuttingWorkSheet d		 
				 JOIN PurchaseOrderDB.dbo.PropertyCalc r ON r.PropertyCalcId = d.PropertyCalcId
				 JOIN @ProductIds c ON d.InputProductId = c.[Value]
		  WHERE r.IsDeleted = 0

		 UNION ALL

		 SELECT c.[Value],
			   r.PropertyCalcId,
			   r.PropertyCalcCode
		  FROM PurchaseOrderDB.dbo.CuttingWorkSheet d
				JOIN PurchaseOrderDB.dbo.CuttingWorkSheetDest de ON de.CuttingWorkSheetId = d.CuttingWorkSheetId
				 JOIN PurchaseOrderDB.dbo.PropertyCalc r ON r.PropertyCalcId = d.PropertyCalcId
				 JOIN @ProductIds c ON de.ProductId = c.[Value]
		  WHERE r.IsDeleted = 0


		  UNION ALL

		 SELECT c.[Value],
			   r.PropertyCalcId,
			   r.PropertyCalcCode
		  FROM PurchaseOrderDB.dbo.CuttingWorkSheet d
				JOIN PurchaseOrderDB.dbo.CuttingExcessMaterial ep ON ep.CuttingWorkSheetId = d.CuttingWorkSheetId
				 JOIN PurchaseOrderDB.dbo.PropertyCalc r ON r.PropertyCalcId = d.PropertyCalcId
				 JOIN @ProductIds c ON ep.ProductId = c.[Value]
		  WHERE r.IsDeleted = 0

	) r	
	ORDER BY r.PropertyCalcId DESC
	

	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END
	

	--SELECT TOP 1 @OutProductId = d.ProductId,
	--	@OutMessage = N'Tên gọi khác phía nhà cung cấp ' + d.ProviderProductName
	--FROM PurchaseOrderDB.dbo.ProviderProductInfo d
	--WHERE d.ProductId in (Select [Value] from @ProductIds)
	--IF @OutProductId IS NOT NULL RETURN 0;

	--Voucher
	
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		c.[Value],
		c.[Value],
		49,--VoucherBill,
		d.VoucherTypeId,
		d.VoucherBill_F_Id,
		d.so_ct,
		CONCAT(N'Chứng từ ', t.Title ,'. Mã: ', d.so_ct)		
	FROM PurchaseOrderDB.dbo.VoucherValueRow d 
		JOIN PurchaseOrderDB.dbo.VoucherType t ON t.VoucherTypeId = d.VoucherTypeId
		JOIN @ProductIds c ON d.vthhtp = c.[Value]
	WHERE d.IsDeleted=0
	ORDER BY d.VoucherBill_F_Id DESC;	
	
	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END


	--Accountancy
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		c.[Value],
		c.[Value],
		39,--InputBill,
		d.InputTypeId,
		d.InputBill_F_Id,
		d.so_ct,
		CONCAT(N'Chứng từ ', t.Title ,'. Mã: ', d.so_ct)		
	FROM AccountancyDB.dbo.InputValueRow d 
		JOIN AccountancyDB.dbo.InputType t ON t.InputTypeId = d.InputTypeId
		JOIN @ProductIds c ON d.vthhtp = c.[Value]
	WHERE d.IsDeleted=0
	ORDER BY d.InputBill_F_Id DESC;	
	
	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END

	--Accountancy public
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		c.[Value],
		c.[Value],
		39001,--InputBillPublic,
		d.InputTypeId,
		d.InputBill_F_Id,
		d.so_ct,
		CONCAT(N'Chứng từ kế toán thuế ', t.Title ,'. Mã: ', d.so_ct)		
	FROM AccountancyPublicDB.dbo.InputValueRow d 
		JOIN AccountancyDB.dbo.InputType t ON t.InputTypeId = d.InputTypeId
		JOIN @ProductIds c ON d.vthhtp = c.[Value]
	WHERE d.IsDeleted=0
	ORDER BY d.InputBill_F_Id DESC;	
	
	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END


	--Maufacturing
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		c.[Value],
		c.[Value],
		70,--ProductionOrder,
		NULL,
		t.ProductionOrderId,
		t.ProductionOrderCode,
		CONCAT(N'Lệnh sản xuất. Mã: ', t.ProductionOrderCode)		
	FROM ManufacturingDB.dbo.ProductionOrderDetail d 
		JOIN ManufacturingDB.dbo.ProductionOrder t ON t.ProductionOrderId = d.ProductionOrderId
		JOIN @ProductIds c ON d.ProductId = c.[Value]
	WHERE d.IsDeleted=0
	ORDER BY t.ProductionOrderId DESC;	
	
	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END



	DECLARE @LINK_DATA_OBJECT_TYPE_PRODUCT INT = 1;
	DECLARE @CONTAINER_TYPE_PRODUCTION_ORDER INT = 2;
	DECLARE @CONTAINER_TYPE_PRODUCT INT = 1;

	--ProductionOrderProcess - production order
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		c.[Value],
		c.[Value],
		70,--ProductionOrder,
		NULL,
		o.ProductionOrderId,
		o.ProductionOrderCode,
		CONCAT(N'Quy trình sản xuất của lệnh. Mã: ', o.ProductionOrderCode)		
	FROM ManufacturingDB.dbo.ProductionStepLinkData d 
		JOIN ManufacturingDB.dbo.ProductionStepLinkDataRole r ON r.ProductionStepLinkDataId = d.ProductionStepLinkDataId
		JOIN ManufacturingDB.dbo.ProductionStep s ON s.ProductionStepId = r.ProductionStepId
		JOIN ManufacturingDB.dbo.ProductionOrder o ON o.ProductionOrderId = s.ContainerId AND s.ContainerTypeId = @CONTAINER_TYPE_PRODUCTION_ORDER 
		JOIN @ProductIds c ON d.LinkDataObjectId = c.[Value] AND d.LinkDataObjectTypeId = @LINK_DATA_OBJECT_TYPE_PRODUCT 
	WHERE d.IsDeleted=0
	ORDER BY o.ProductionOrderId DESC;	
	
	IF @IsCheckExistOnly = 1 AND EXISTS(SELECT TOP(1) 0 FROM @result)
	BEGIN
	    SELECT * FROM @result
		RETURN 0;
	END
	


	--ProductionOrderProcess - product
	INSERT INTO @result
	(
	    ProductId,
		Id,
	    ObjectTypeId,
		BillTypeId,
	    BillId,
	    BillCode,
	    Description
	)
	SELECT DISTINCT TOP (10 )
		c.[Value],
		c.[Value],
		6,--Product,
		NULL,
		p.ProductId,
		p.ProductCode,
		CONCAT(N'Quy trình sản xuất của mặt hàng. Mã: ', p.ProductCode)		
	FROM ManufacturingDB.dbo.ProductionStepLinkData d 
		JOIN ManufacturingDB.dbo.ProductionStepLinkDataRole r ON r.ProductionStepLinkDataId = d.ProductionStepLinkDataId
		JOIN ManufacturingDB.dbo.ProductionStep s ON s.ProductionStepId = r.ProductionStepId
		JOIN ManufacturingDB.dbo.RefProduct p ON p.ProductId = s.ContainerId AND s.ContainerTypeId = @CONTAINER_TYPE_PRODUCT 
		JOIN @ProductIds c ON d.LinkDataObjectId = c.[Value] AND d.LinkDataObjectTypeId = @LINK_DATA_OBJECT_TYPE_PRODUCT 
	WHERE d.IsDeleted=0 AND p.IsDeleted = 0
	ORDER BY p.ProductId DESC;	

	SELECT * FROM @result;
END


