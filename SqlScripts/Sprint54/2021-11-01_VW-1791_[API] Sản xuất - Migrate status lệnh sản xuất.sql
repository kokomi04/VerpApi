

WITH inv AS (
	SELECT 
		id.ProductionOrderCode, i.SubsidiaryId
	FROM StockDB.dbo.InventoryDetail id
	INNER JOIN StockDB.dbo.Inventory i ON id.InventoryId = i.InventoryId AND id.SubsidiaryId = i.SubsidiaryId
	WHERE i.IsDeleted = 0 AND i.IsApproved = 1 AND id.ProductionOrderCode IS NOT NULL AND LEN(id.ProductionOrderCode) > 0 AND i.InventoryTypeId = 2
	GROUP BY id.ProductionOrderCode, i.SubsidiaryId
)
UPDATE  po
SET po.ProductionOrderStatus = 2
FROM ManufacturingDB.dbo.ProductionOrder po
INNER JOIN inv inv ON po.ProductionOrderCode = inv.ProductionOrderCode AND po.IsDeleted = 0 AND po.SubsidiaryId = inv.SubsidiaryId AND po.ProductionOrderStatus = 1;



WITH inv AS (
	SELECT 
		id.ProductionOrderCode,
		i.SubsidiaryId,
		id.ProductId,
		SUM(id.PrimaryQuantity) Quantity
	FROM StockDB.dbo.InventoryDetail id
	INNER JOIN StockDB.dbo.Inventory i ON id.InventoryId = i.InventoryId AND id.SubsidiaryId = i.SubsidiaryId
	WHERE i.IsDeleted = 0 AND i.IsApproved = 1 AND id.ProductionOrderCode IS NOT NULL AND LEN(id.ProductionOrderCode) > 0 AND i.InventoryTypeId = 1
	GROUP BY id.ProductionOrderCode, id.ProductId, i.SubsidiaryId
),
data AS (
	SELECT 
		po.ProductionOrderId,
		po.ProductionOrderStatus,
		pod.ProductId,
		(pod.Quantity + pod.ReserveQuantity) ProductionQuantity,
		inv.Quantity
	FROM ManufacturingDB.dbo.ProductionOrder po
	INNER JOIN ManufacturingDB.dbo.ProductionOrderDetail pod ON po.ProductionOrderId = pod.ProductionOrderId
	INNER JOIN inv inv ON po.ProductionOrderCode = inv.ProductionOrderCode AND po.IsDeleted = 0 AND po.SubsidiaryId = inv.SubsidiaryId AND pod.ProductId = inv.ProductId
	WHERE po.ProductionOrderStatus = 2
),
groupData AS (
	SELECT
		d.ProductionOrderId,
		MAX(d.ProductionQuantity - d.Quantity) RemainQuantity
	FROM data d
	GROUP BY d.ProductionOrderId
)
UPDATE  po
SET po.ProductionOrderStatus = 3
FROM ManufacturingDB.dbo.ProductionOrder po
INNER JOIN groupData g ON po.ProductionOrderId = g.ProductionOrderId AND g.RemainQuantity <= 0;




