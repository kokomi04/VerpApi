USE ManufacturingDB
GO
UPDATE dbo.ProductionOrderDetail SET Quantity = 0 WHERE Quantity IS NULL
UPDATE dbo.ProductionOrderDetail SET ReserveQuantity = 0 WHERE ReserveQuantity IS NULL
GO
ALTER TABLE dbo.ProductionOrderDetail ALTER COLUMN Quantity decimal(32, 12) NOT NULL
ALTER TABLE dbo.ProductionOrderDetail ALTER COLUMN ReserveQuantity decimal(32, 12) NOT NULL