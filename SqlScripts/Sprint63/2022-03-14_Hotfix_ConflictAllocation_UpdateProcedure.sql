ALTER PROCEDURE [StockDB].[dbo].[asp_ProductionHandover_GetInventoryRequirementByProductionOrder]
  @ProductionOrderCode AS nvarchar(128) ,
  @SubId AS int 
AS
BEGIN
	DECLARE @WAITING int = 1;
	DECLARE @ACCEPTED int = 2;
	
	WITH inventories AS (
		SELECT 
			id.InventoryDetailId,
			id.InventoryRequirementDetailId,
			i.InventoryCode,
			i.InventoryId,
			i.InventoryTypeId,
			id.ProductId,
			i.DepartmentId,
			i.StockId,
			i.CreatedByUserId,
			i.CreatedDatetimeUtc,
			ISNULL(id.PrimaryQuantity,0) PrimaryQuantity
		FROM StockDB.dbo.InventoryDetail id
		INNER JOIN StockDB.dbo.Inventory i ON id.InventoryId = i.InventoryId AND id.IsDeleted = 0 AND i.IsDeleted = 0 AND i.SubsidiaryId = @SubId AND id.SubsidiaryId = @SubId
		WHERE id.ProductionOrderCode = @ProductionOrderCode AND i.IsApproved = 1
	),
	requirements AS (
		SELECT 
			rd.InventoryRequirementDetailId,
			rd.ProductId,
			r.CreatedByUserId,
			r.CreatedDatetimeUtc,
			rd.PrimaryQuantity RequirementQuantity,
			r.InventoryTypeId,
			rd.AssignStockId,
			rd.DepartmentId,
			r.Content,
			r.CensorStatus,
			rd.OutsourceStepRequestId,
			r.InventoryRequirementCode,
			r.InventoryRequirementId,
			rd.ProductionStepId
		FROM StockDB.dbo.InventoryRequirementDetail rd
		INNER JOIN StockDB.dbo.InventoryRequirement r ON rd.InventoryRequirementId = r.InventoryRequirementId AND rd.IsDeleted = 0 AND r.IsDeleted = 0 
		WHERE rd.ProductionOrderCode = @ProductionOrderCode AND r.SubsidiaryId = @SubId
	)
	SELECT 
		ISNULL(inv.ProductId, req.ProductId) ProductId,
		ISNULL(req.CreatedByUserId, inv.CreatedByUserId) CreatedByUserId,
		ISNULL(req.CreatedDatetimeUtc, inv.CreatedDatetimeUtc) CreatedDatetimeUtc,
		ISNULL(req.RequirementQuantity, inv.PrimaryQuantity) RequirementQuantity,
		ISNULL(inv.PrimaryQuantity, 0) ActualQuantity,
		ISNULL(inv.InventoryTypeId, req.InventoryTypeId) InventoryTypeId,
		ISNULL(inv.StockId, req.AssignStockId) AssignStockId,
		ISNULL(inv.DepartmentId, req.DepartmentId) DepartmentId,
		req.Content,
		CASE 
			WHEN req.CensorStatus = @ACCEPTED OR req.CensorStatus IS NULL
			THEN CASE 
				WHEN inv.PrimaryQuantity IS NOT NULL 
				THEN @ACCEPTED
				ELSE @WAITING
				END
			ELSE req.CensorStatus 
			END Status,
			inv.InventoryCode InventoryCode,
			inv.InventoryId,
			inv.InventoryDetailId,
			req.OutsourceStepRequestId,
			req.InventoryRequirementCode,
			req.InventoryRequirementDetailId,
			req.InventoryRequirementId,
			s.StockName,
			req.ProductionStepId
	FROM inventories inv
	FULL OUTER JOIN requirements req ON inv.InventoryRequirementDetailId = req.InventoryRequirementDetailId AND inv.ProductId = req.ProductId AND inv.InventoryTypeId = req.InventoryTypeId
	LEFT JOIN StockDB.dbo.Stock s ON ISNULL(inv.StockId, req.AssignStockId) = s.StockId;
END