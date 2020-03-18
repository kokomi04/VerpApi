UPDATE pk
SET pk.ProductUnitConversionRemaining = ivin.ProductUnitConversionQuantity - ISNULL(ivout.ProductUnitConversionQuantity,0),
	pk.ProductUnitConversionWaitting = ISNULL(outn.ProductUnitConversionQuantity,0),
	pk.PrimaryQuantityRemaining = ivin.PrimaryQuantity - ISNULL(ivout.PrimaryQuantity,0),
	pk.PrimaryQuantityWaiting = ISNULL(outn.PrimaryQuantity,0)

FROM
(
	SELECT 
	iv.StockId, id.ProductId, id.ProductUnitConversionId, SUM(id.ProductUnitConversionQuantity) ProductUnitConversionQuantity, SUM(id.PrimaryQuantity) PrimaryQuantity
	FROM dbo.InventoryDetail AS id
	JOIN dbo.Inventory AS iv ON iv.InventoryId = id.InventoryId	
	WHERE iv.IsApproved=1 AND iv.IsDeleted=0 AND id.IsDeleted=0 AND iv.InventoryTypeId=1
	GROUP BY iv.StockId, id.ProductId, id.ProductUnitConversionId
) AS ivin
LEFT JOIN dbo.Package AS pk ON ivin.StockId = pk.StockId AND ivin.ProductId = pk.ProductId AND ivin.ProductUnitConversionId = pk.ProductUnitConversionId
LEFT JOIN

(
SELECT 
iv.StockId, id.ProductId, id.ProductUnitConversionId, SUM(id.ProductUnitConversionQuantity) ProductUnitConversionQuantity, SUM(id.PrimaryQuantity) PrimaryQuantity
FROM dbo.InventoryDetail AS id
JOIN dbo.Inventory AS iv ON iv.InventoryId = id.InventoryId
WHERE iv.IsApproved=1 AND iv.IsDeleted=0 AND id.IsDeleted=0 AND iv.InventoryTypeId=2
GROUP BY iv.StockId, id.ProductId, id.ProductUnitConversionId

) AS ivout ON ivin.StockId = ivout.StockId AND ivin.ProductId = ivout.ProductId AND ivin.ProductUnitConversionId = ivout.ProductUnitConversionId

LEFT JOIN

(
SELECT 
iv.StockId, id.ProductId, id.ProductUnitConversionId, SUM(id.ProductUnitConversionQuantity) ProductUnitConversionQuantity, SUM(id.PrimaryQuantity) PrimaryQuantity
FROM dbo.InventoryDetail AS id
JOIN dbo.Inventory AS iv ON iv.InventoryId = id.InventoryId
WHERE iv.IsApproved=0 AND iv.IsDeleted=0 AND id.IsDeleted=0 AND iv.InventoryTypeId=2
GROUP BY iv.StockId, id.ProductId, id.ProductUnitConversionId

) AS outn ON ivin.StockId = outn.StockId AND ivin.ProductId = outn.ProductId AND ivin.ProductUnitConversionId = outn.ProductUnitConversionId



UPDATE sp
SET sp.PrimaryQuantityWaiting = pk.PrimaryQuantityWaiting,
	sp.PrimaryQuantityRemaining = pk.PrimaryQuantityRemaining,
	sp.ProductUnitConversionWaitting = pk.ProductUnitConversionWaitting,
	sp.ProductUnitConversionRemaining = pk.ProductUnitConversionRemaining
FROM dbo.StockProduct sp 
JOIN dbo.Package AS pk ON sp.StockId = pk.StockId AND sp.ProductId = pk.ProductId AND sp.ProductUnitConversionId = pk.ProductUnitConversionId