
--SELECT 

--pk.PrimaryQuantityRemaining,

--pk.ProductUnitConversionRemaining, 
--pk.PrimaryQuantityRemaining*c.FactorExpression AS ProductUnitConversionRemaining1,

--pk.PrimaryQuantityWaiting,
--pk.ProductUnitConversionWaitting, 
--pk.PrimaryQuantityWaiting*c.FactorExpression AS ProductUnitConversionWaitting1
UPDATE pk
SET
	pk.ProductUnitConversionRemaining = pk.PrimaryQuantityRemaining*c.FactorExpression,
	pk.ProductUnitConversionWaitting = pk.PrimaryQuantityWaiting*c.FactorExpression

FROM dbo.Package pk
JOIN dbo.ProductUnitConversion as c on pk.ProductUnitConversionId = c.ProductUnitConversionId