UPDATE id
	SET id.ProductUnitConversionQuantity = id.PrimaryQuantity*c.FactorExpression 
FROM
dbo.InventoryDetail AS id
JOIN dbo.ProductUnitConversion AS c ON c.ProductUnitConversionId = id.ProductUnitConversionId
WHERE id.IsDeleted = 0