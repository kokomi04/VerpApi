
--SELECT 

--sp.PrimaryQuantityRemaining,

--sp.ProductUnitConversionRemaining, 
--sp.PrimaryQuantityRemaining*c.FactorExpression AS ProductUnitConversionRemaining1,

--sp.PrimaryQuantityWaiting,
--sp.ProductUnitConversionWaitting, 
--sp.PrimaryQuantityWaiting*c.FactorExpression AS ProductUnitConversionWaitting1
UPDATE sp
SET
	sp.ProductUnitConversionRemaining = sp.PrimaryQuantityRemaining*c.FactorExpression,
	sp.ProductUnitConversionWaitting = sp.PrimaryQuantityWaiting*c.FactorExpression

FROM dbo.StockProduct sp
JOIN dbo.ProductUnitConversion as c on sp.ProductUnitConversionId = c.ProductUnitConversionId