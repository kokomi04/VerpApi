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
	CONVERT(NVARCHAR(128),c.ProductionOrderMaterialsConsumptionId),
	CONVERT(NVARCHAR(128),c.ParentId)
FROM dbo.ProductionOrderMaterialsConsumption c
WHERE NOT EXISTS(
	SELECT 0 
	FROM dbo.ProductionOrderMaterials m 
	WHERE c.ProductionOrderId = m.ProductionOrderId 
	AND c.ProductMaterialsConsumptionGroupId = m.ProductMaterialsConsumptionGroupId
);

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
	SET IdClient = CONVERT(NVARCHAR(128), ProductionOrderMaterialsId),
		ParentIdClient = CONVERT(NVARCHAR(128), ParentId)
WHERE IdClient IS NULL;

UPDATE m
	SET m.ParentId = p.ProductionOrderMaterialsId
FROM dbo.ProductionOrderMaterials m
OUTER APPLY(
	SELECT TOP(1) p.ProductionOrderMaterialsId FROM dbo.ProductionOrderMaterials p WHERE m.ParentIdClient = m.IdClient
) p
WHERE m.ParentId IS NULL;
	
SET IDENTITY_INSERT dbo.ProductionOrderMaterials OFF;

SET IDENTITY_INSERT dbo.ProductionOrderMaterialSet ON;

DECLARE @Max_ProductionOrderMaterialSetId BIGINT;
SELECT @Max_ProductionOrderMaterialSetId = MAX(ProductionOrderMaterialSetId) FROM dbo.ProductionOrderMaterialSet;
SET @Max_ProductionOrderMaterialSetId = ISNULL(@Max_ProductionOrderMaterialSetId,0);

DECLARE @Set TABLE (
	[ProductionOrderMaterialSetId] BIGINT NOT NULL,
	[Title] NVARCHAR(128) NULL,
	[ProductionOrderId] BIGINT NOT NULL,
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
	ROW_NUMBER() OVER(ORDER BY ProductionOrderId, ProductMaterialsConsumptionGroupId) + @Max_ProductionOrderMaterialSetId,
	NULL,
	ProductionOrderId, 
	ProductMaterialsConsumptionGroupId,
	0,
	MIN(CreatedByUserId),
	MIN(UpdatedByUserId),
	MIN(CreatedDatetimeUtc),
	MIN(UpdatedDatetimeUtc),
	0,
	NULL
FROM dbo.ProductionOrderMaterials
WHERE IsDeleted = 0 AND ProductionOrderMaterialSetId IS NULL
GROUP BY ProductionOrderId, ProductMaterialsConsumptionGroupId;

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

UPDATE s
	SET s.ProductionOrderMaterialSetId = s.ProductionOrderMaterialSetId
FROM @Set s
	JOIN dbo.ProductionOrderMaterials m ON s.ProductionOrderId = m.ProductionOrderId 
		AND s.ProductMaterialsConsumptionGroupId = m.ProductMaterialsConsumptionGroupId
WHERE m.ProductionOrderMaterialSetId IS NULL;
SET IDENTITY_INSERT dbo.ProductionOrderMaterialSet OFF;
COMMIT TRANSACTION;
GO
