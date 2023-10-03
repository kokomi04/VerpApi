USE ManufacturingDB
GO
UPDATE dbo.ProductionOrder SET IsFinished = 1 WHERE ProductionOrderStatus IN(400, 350)--Completed, Finished