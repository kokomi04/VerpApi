USE ManufacturingDB
GO
UPDATE dbo.ProductionOrder
	SET ProductionOrderStatus = 
	CASE ProductionOrderStatus 
		WHEN 1 THEN 100--Waiting
		WHEN 2 THEN 210--ProcessingLessStarted
		WHEN 3 THEN 350--Finished
		WHEN 4 THEN 300--OverDeadline
	ELSE 9 END