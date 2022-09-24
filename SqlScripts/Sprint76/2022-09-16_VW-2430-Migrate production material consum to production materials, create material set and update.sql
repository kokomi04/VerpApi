USE [ManufacturingDB]
GO


BEGIN TRANSACTION

SET IDENTITY_INSERT dbo.ProductionOrderMaterials ON;

DECLARE @Max_ProductionOrderMaterialsId BIGINT;
SELECT @Max_ProductionOrderMaterialsId = MAX(ProductionOrderMaterialsId) FROM dbo.ProductionOrderMaterials;
SET @Max_ProductionOrderMaterialsId = ISNULL(@Max_ProductionOrderMaterialsId,0);


DECLARE @ProductionOrderMaterials TABLE (
	[ProductionOrderMaterialsId] [BIGINT] NOT NULL,
	[ProductionOrderId] [BIGINT] NOT NULL,
	[ProductionStepLinkDataId] [BIGINT] NULL,
	[ProductId] [BIGINT] NOT NULL,
	[ConversionRate] [DECIMAL](32, 12) NOT NULL,
	[Quantity] [DECIMAL](32, 12) NOT NULL,
	[UnitId] [INT] NOT NULL,
	[StepId] [INT] NULL,
	[DepartmentId] [INT] NULL,
	[InventoryRequirementStatusId] [INT] NOT NULL,
	[ParentId] [BIGINT] NULL,
	[IsReplacement] [BIT] NOT NULL,
	[CreatedDatetimeUtc] [DATETIME2](7) NOT NULL,
	[CreatedByUserId] [INT] NOT NULL,
	[IsDeleted] [BIT] NOT NULL,
	[UpdatedDatetimeUtc] [DATETIME2](7) NOT NULL,
	[UpdatedByUserId] [INT] NOT NULL,
	[DeletedDatetimeUtc] [DATETIME2](7) NULL,
	[ProductionOrderMaterialSetId] [BIGINT] NULL,
	[ProductMaterialsConsumptionGroupId] [INT] NULL,
	[IdClient] [NVARCHAR](128) NULL,
	[ParentIdClient] [NVARCHAR](128) NULL
)

INSERT INTO @ProductionOrderMaterials
(
	ProductionOrderMaterialsId,
    ProductionOrderId,
    ProductionStepLinkDataId,
    ProductId,
    ConversionRate,
    Quantity,
    UnitId,
    StepId,
    DepartmentId,
    InventoryRequirementStatusId,
    ParentId,
    IsReplacement,
    CreatedDatetimeUtc,
    CreatedByUserId,
    IsDeleted,
    UpdatedDatetimeUtc,
    UpdatedByUserId,
    DeletedDatetimeUtc,
    ProductionOrderMaterialSetId,
    ProductMaterialsConsumptionGroupId,
	IdClient,
	ParentIdClient
)
SELECT
	ROW_NUMBER() OVER(ORDER BY c.ProductionOrderMaterialsConsumptionId) + @Max_ProductionOrderMaterialsId,
	ProductionOrderId,
    NULL,
    ProductId,
    ConversionRate,
    Quantity,
    UnitId,
    NULL,
    DepartmentId,
    InventoryRequirementStatusId,
    NULL,
    IsReplacement,
    CreatedDatetimeUtc,
    CreatedByUserId,
    IsDeleted,
    UpdatedDatetimeUtc,
    UpdatedByUserId,
    DeletedDatetimeUtc,
    NULL,
    ProductMaterialsConsumptionGroupId,	
	CONCAT('G-',c.ProductionOrderMaterialsConsumptionId),
	CASE WHEN c.ParentId IS NOT NULL THEN CONCAT('G-',c.ParentId)	ELSE NULL END
FROM dbo.ProductionOrderMaterialsConsumption c
WHERE NOT EXISTS(
	SELECT 0 
	FROM dbo.ProductionOrderMaterials m 
	WHERE c.ProductionOrderId = m.ProductionOrderId 
	AND c.ProductMaterialsConsumptionGroupId = m.ProductMaterialsConsumptionGroupId
);

UPDATE dbo.ProductionOrderMaterials SET ProductMaterialsConsumptionGroupId = 0 WHERE ProductMaterialsConsumptionGroupId IS NULL;

INSERT INTO dbo.ProductionOrderMaterials
(
	ProductionOrderMaterialsId,
    ProductionOrderId,
    ProductionStepLinkDataId,
    ProductId,
    ConversionRate,
    Quantity,
    UnitId,
    StepId,
    DepartmentId,
    InventoryRequirementStatusId,
    ParentId,
    IsReplacement,
    CreatedDatetimeUtc,
    CreatedByUserId,
    IsDeleted,
    UpdatedDatetimeUtc,
    UpdatedByUserId,
    DeletedDatetimeUtc,
    ProductionOrderMaterialSetId,
    ProductMaterialsConsumptionGroupId,
	IdClient,
	ParentIdClient
)
SELECT
	ProductionOrderMaterialsId,
    ProductionOrderId,
    ProductionStepLinkDataId,
    ProductId,
    ConversionRate,
    Quantity,
    UnitId,
    StepId,
    DepartmentId,
    InventoryRequirementStatusId,
    ParentId,
    IsReplacement,
    CreatedDatetimeUtc,
    CreatedByUserId,
    IsDeleted,
    UpdatedDatetimeUtc,
    UpdatedByUserId,
    DeletedDatetimeUtc,
    ProductionOrderMaterialSetId,
    ProductMaterialsConsumptionGroupId,
	IdClient,
	ParentIdClient
FROM @ProductionOrderMaterials;

UPDATE dbo.ProductionOrderMaterials
	SET IdClient = CONCAT('M-',ProductionOrderMaterialsId),
		ParentIdClient =  CASE WHEN ParentId IS NOT NULL THEN CONCAT('M-',ParentId)	ELSE NULL END
WHERE IdClient IS NULL;

UPDATE m
	SET m.ParentId = p.ProductionOrderMaterialsId
FROM dbo.ProductionOrderMaterials m
OUTER APPLY(
	SELECT TOP(1) p.ProductionOrderMaterialsId FROM dbo.ProductionOrderMaterials p WHERE m.ParentIdClient = p.IdClient
) p
WHERE m.ParentId IS NULL AND m.IsReplacement = 1 AND m.IsDeleted = 0;
print 'Done UPDATE ParentId'
SET IDENTITY_INSERT dbo.ProductionOrderMaterials OFF;

SET IDENTITY_INSERT dbo.ProductionOrderMaterialSet ON;

DECLARE @Max_ProductionOrderMaterialSetId BIGINT;
SELECT @Max_ProductionOrderMaterialSetId = MAX(ProductionOrderMaterialSetId) FROM dbo.ProductionOrderMaterialSet;
SET @Max_ProductionOrderMaterialSetId = ISNULL(@Max_ProductionOrderMaterialSetId,0);

DECLARE @Set TABLE (
	[ProductionOrderMaterialSetId] BIGINT NOT NULL,
	[Title] NVARCHAR(128) NULL,
	[ProductionOrderId] BIGINT NOT NULL,
	[ProductionOrderCode] NVARCHAR(128) NULL,
	ProductMaterialsConsumptionGroupId INT NULL,
	[IsMultipleConsumptionGroupId] BIT NOT NULL,
	[CreatedByUserId] INT NOT NULL,
	[UpdatedByUserId] INT NOT NULL,
	[CreatedDatetimeUtc] DATETIME2(7) NOT NULL,
	[UpdatedDatetimeUtc] DATETIME2(7) NOT NULL,
	[IsDeleted] BIT NOT NULL,
	[DeletedDatetimeUtc] DATETIME2(7) NULL
)
INSERT INTO @Set
(
	ProductionOrderMaterialSetId,
    Title,
    ProductionOrderId,
	ProductionOrderCode,
    ProductMaterialsConsumptionGroupId,
    IsMultipleConsumptionGroupId,
    CreatedByUserId,
    UpdatedByUserId,
    CreatedDatetimeUtc,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc
)
SELECT
	ROW_NUMBER() OVER(ORDER BY m.ProductionOrderId, m.ProductMaterialsConsumptionGroupId) + @Max_ProductionOrderMaterialSetId,
	NULL,
	m.ProductionOrderId, 
	p.ProductionOrderCode,
	m.ProductMaterialsConsumptionGroupId,
	0,
	MIN(m.CreatedByUserId),
	MIN(m.UpdatedByUserId),
	MIN(m.CreatedDatetimeUtc),
	MIN(m.UpdatedDatetimeUtc),
	0,
	NULL
FROM dbo.ProductionOrderMaterials m
	JOIN dbo.ProductionOrder p ON m.ProductionOrderId = p.ProductionOrderId
WHERE m.IsDeleted = 0 AND m.ProductionOrderMaterialSetId IS NULL
GROUP BY m.ProductionOrderId, p.ProductionOrderCode, m.ProductMaterialsConsumptionGroupId;

print 'Done INSERT INTO @Set'


IF EXISTS(SELECT 0 FROM @Set)
BEGIN
	SELECT @Max_ProductionOrderMaterialSetId = MAX(ProductionOrderMaterialSetId) FROM @Set;
	SET @Max_ProductionOrderMaterialSetId = ISNULL(@Max_ProductionOrderMaterialSetId,0);
END

INSERT INTO @Set
(
    ProductionOrderMaterialSetId,
    Title,
    ProductionOrderId,
    ProductionOrderCode,
    ProductMaterialsConsumptionGroupId,
    IsMultipleConsumptionGroupId,
    CreatedByUserId,
    UpdatedByUserId,
    CreatedDatetimeUtc,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc
)
SELECT
	ROW_NUMBER() OVER(ORDER BY v.ProductionOrderId, v.ProductMaterialsConsumptionGroupId) + @Max_ProductionOrderMaterialSetId,
	NULL,
	v.ProductionOrderId,
	v.ProductionOrderCode,
	v.ProductMaterialsConsumptionGroupId,
	0,
	1,
	1,
	GETUTCDATE(),
	GETUTCDATE(),
	0,
	NULL
FROM
(
	SELECT
		DISTINCT
		o.ProductionOrderId,
		o.ProductionOrderCode,
		c.ProductMaterialsConsumptionGroupId
	FROM ManufacturingDB.dbo.ProductionOrder o
	JOIN ManufacturingDB.dbo.ProductionOrderDetail d ON d.ProductionOrderId = o.ProductionOrderId
	JOIN StockDB.dbo.ProductMaterialsConsumption c ON c.ProductId = d.ProductId
	WHERE d.IsDeleted = 0 AND o.IsDeleted = 0 AND c.IsDeleted = 0

	UNION ALL

	SELECT
		DISTINCT
		o.ProductionOrderId,
		o.ProductionOrderCode,
		0 ProductMaterialsConsumptionGroupId
	FROM ManufacturingDB.dbo.ProductionOrder o
	WHERE o.IsDeleted = 0
) v
WHERE NOT EXISTS(
	SELECT 0 
	FROM dbo.ProductionOrderMaterials m 
	WHERE v.ProductionOrderId = m.ProductionOrderId 
	AND v.ProductMaterialsConsumptionGroupId = m.ProductMaterialsConsumptionGroupId
	AND m.IsDeleted = 0
)
AND NOT EXISTS(
	SELECT 0 
	FROM @Set m 
	WHERE v.ProductionOrderId = m.ProductionOrderId 
	AND v.ProductMaterialsConsumptionGroupId = m.ProductMaterialsConsumptionGroupId
)
;

INSERT INTO dbo.ProductionOrderMaterialSet
(
	ProductionOrderMaterialSetId,
    Title,
    ProductionOrderId,
    IsMultipleConsumptionGroupId,
    CreatedByUserId,
    UpdatedByUserId,
    CreatedDatetimeUtc,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc
)
SELECT
	ProductionOrderMaterialSetId,
    Title,
    ProductionOrderId,
    IsMultipleConsumptionGroupId,
    CreatedByUserId,
    UpdatedByUserId,
    CreatedDatetimeUtc,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc
FROM @Set;
print 'Done INSERT INTO table Set'

UPDATE m
	SET m.ProductionOrderMaterialSetId = s.ProductionOrderMaterialSetId
FROM @Set s
	JOIN dbo.ProductionOrderMaterials m ON s.ProductionOrderId = m.ProductionOrderId 
		AND s.ProductMaterialsConsumptionGroupId = m.ProductMaterialsConsumptionGroupId
WHERE m.ProductionOrderMaterialSetId IS NULL;
print 'Done UPDATE @Set'


INSERT INTO dbo.ProductionOrderMaterialSetConsumptionGroup
(
    ProductionOrderMaterialSetId,
    ProductMaterialsConsumptionGroupId
)
SELECT DISTINCT ProductionOrderMaterialSetId, ProductMaterialsConsumptionGroupId FROM @Set


UPDATE m
	SET m.ProductionOrderMaterialSetId = s.ProductionOrderMaterialSetId
FROM @Set s
	JOIN PurchaseOrderDB.dbo.PurchasingRequest m ON s.ProductionOrderId = m.ProductionOrderId 
		AND s.ProductMaterialsConsumptionGroupId = m.ProductMaterialsConsumptionGroupId
WHERE m.ProductionOrderMaterialSetId IS NULL AND m.PurchasingRequestTypeId=3;--ProductionOrderMaterialCalc=3
print 'Done UPDATE PurchasingRequest'

UPDATE m
	SET m.ProductionOrderMaterialSetId = s.ProductionOrderMaterialSetId
FROM @Set s
	JOIN(
		SELECT 
			r.InventoryRequirementId,
			r.ProductMaterialsConsumptionGroupId,
			d.ProductionOrderCode
		FROM StockDB.dbo.InventoryRequirement r
		JOIN StockDB.dbo.InventoryRequirementDetail d ON r.InventoryRequirementId = d.InventoryRequirementId
		WHERE r.IsDeleted = 0 AND d.IsDeleted = 0
		GROUP BY r.InventoryRequirementId,
			r.ProductMaterialsConsumptionGroupId,
			d.ProductionOrderCode
	) r ON s.ProductionOrderCode = r.ProductionOrderCode AND s.ProductMaterialsConsumptionGroupId = r.ProductMaterialsConsumptionGroupId
	JOIN  StockDB.dbo.InventoryRequirement m ON r.InventoryRequirementId = m.InventoryRequirementId 
		
WHERE m.ProductionOrderMaterialSetId IS NULL AND m.InventoryRequirementTypeId=1;--Complete=1

print 'Done UPDATE Inv requirement'

SET IDENTITY_INSERT dbo.ProductionOrderMaterialSet OFF;
COMMIT TRANSACTION;
GO
