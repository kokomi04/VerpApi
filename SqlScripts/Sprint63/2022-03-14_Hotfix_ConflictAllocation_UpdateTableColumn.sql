SELECT * FROM [dbo].[MaterialAllocation]

ALTER TABLE [ManufacturingDB].[dbo].[MaterialAllocation]
ADD InventoryDetailId bigint;

UPDATE ma
SET ma.InventoryDetailId = id.InventoryDetailId
FROM [ManufacturingDB].[dbo].[MaterialAllocation] ma 
INNER JOIN [StockDB].[dbo].[Inventory] i ON ma.InventoryCode = i.InventoryCode AND i.IsDeleted = 0
INNER JOIN [StockDB].[dbo].[InventoryDetail] id ON ma.ProductId = id.ProductId AND i.InventoryId = id.InventoryId AND id.IsDeleted = 0


ALTER TABLE [ManufacturingDB].[dbo].[IgnoreAllocation]
ADD InventoryDetailId bigint;

UPDATE ia
SET ia.InventoryDetailId = id.InventoryDetailId
FROM [ManufacturingDB].[dbo].[IgnoreAllocation] ia 
INNER JOIN [StockDB].[dbo].[Inventory] i ON ia.InventoryCode = i.InventoryCode AND i.IsDeleted = 0
INNER JOIN [StockDB].[dbo].[InventoryDetail] id ON ia.ProductId = id.ProductId AND i.InventoryId = id.InventoryId AND id.IsDeleted = 0