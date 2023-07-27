USE ManufacturingDB
GO
INSERT INTO dbo.ProductionHandover
(
    ProductionHandoverReceiptId,
    SubsidiaryId,
    HandoverDatetime,
    ProductionOrderId,
    ProductionStepLinkDataId,
    ObjectId,
    ObjectTypeId,
    FromDepartmentId,
    FromProductionStepId,
    ToDepartmentId,
    ToProductionStepId,
    HandoverQuantity,
    Note,
    Status,
    InventoryRequirementDetailId,
    InventoryDetailId,
    InventoryProductId,
    IsAuto,
    AcceptByUserId,
    CreatedByUserId,
    CreatedDatetimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc,
    RowIndex,
    InventoryId,
    InventoryCode,
    InventoryQuantity
)
SELECT
   NULL,          -- ProductionHandoverReceiptId - bigint
    a.SubsidiaryId,             -- SubsidiaryId - int
    NULL,          -- HandoverDatetime - datetime2(7)
    a.ProductionOrderId,             -- ProductionOrderId - bigint
    NULL,          -- ProductionStepLinkDataId - bigint
    ISNULL(a.SourceProductId,0),             -- ObjectId - bigint
    1,             -- ObjectTypeId - int-- 1: Product, 2: Semi
    CASE WHEN iv.InventoryTypeId = 1 THEN a.DepartmentId ELSE 0 END,             -- FromDepartmentId - int
    CASE WHEN iv.InventoryTypeId = 1 THEN a.ProductionStepId ELSE 0 END,          -- FromProductionStepId - bigint
    CASE WHEN iv.InventoryTypeId = 2 THEN a.DepartmentId ELSE 0 END,             -- ToDepartmentId - int
    CASE WHEN iv.InventoryTypeId = 2 THEN a.ProductionStepId ELSE 0 END,          -- ToProductionStepId - bigint
    ISNULL(a.SourceQuantity,0),          -- HandoverQuantity - decimal(32, 12)
    'migrate',          -- Note - nvarchar(max)
    1,             -- Status - int
    rd.InventoryRequirementDetailId,          -- InventoryRequirementDetailId - bigint
    d.InventoryDetailId,          -- InventoryDetailId - bigint
    d.ProductId,          -- InventoryProductId - int
    0,       -- IsAuto - bit
    NULL,          -- AcceptByUserId - int
    0,             -- CreatedByUserId - int
    GETUTCDATE(), -- CreatedDatetimeUtc - datetime2(7)
    0,             -- UpdatedByUserId - int
   GETUTCDATE(), -- UpdatedDatetimeUtc - datetime2(7)
    0,          -- IsDeleted - bit
    NULL,          -- DeletedDatetimeUtc - datetime2(7)
    1,       -- RowIndex - int
    iv.InventoryId,          -- InventoryId - bigint
    iv.InventoryCode,          -- InventoryCode - nvarchar(128)
    a.AllocationQuantity           -- InventoryQuantity - decimal(32, 12)
    
FROM dbo.MaterialAllocation a
JOIN [StockDB].dbo.InventoryDetail d ON a.InventoryDetailId = d.InventoryDetailId
JOIN [StockDB].dbo.Inventory iv ON d.InventoryId = iv.InventoryId
LEFT JOIN StockDB.dbo.InventoryRequirementDetail rd ON d.InventoryRequirementDetailId = rd.InventoryRequirementDetailId
LEFT JOIN StockDB.dbo.InventoryRequirement r ON rd.InventoryRequirementId = r.InventoryRequirementId


INSERT INTO dbo.ProductionOrderInventoryConflict
(
    ProductionOrderId,
    InventoryDetailId,
    ProductId,
    InventoryTypeId,
    InventoryId,
    InventoryDate,
    InventoryCode,
    InventoryQuantity,
    InventoryRequirementDetailId,
    InventoryRequirementId,
    RequireQuantity,
    InventoryRequirementCode,
    Content,
    HandoverInventoryQuantitySum,
    ConflictAllowcationStatusId
)

SELECT
	DISTINCT
	a.ProductionOrderId,
	d.InventoryDetailId,
	d.ProductId,
	iv.InventoryTypeId,
	iv.InventoryId,
	iv.Date,
	iv.InventoryCode,
	d.PrimaryQuantity,
	rd.InventoryRequirementDetailId,
	rd.InventoryRequirementId,
	rd.PrimaryQuantity,
	r.InventoryRequirementCode,
	d.Description,
	(SELECT SUM(h.InventoryQuantity) FROM dbo.ProductionHandover h WHERE d.InventoryDetailId = h.InventoryDetailId AND h.IsDeleted = 0),
	0
FROM dbo.MaterialAllocation a
JOIN [StockDB].dbo.InventoryDetail d ON a.InventoryDetailId = d.InventoryDetailId
JOIN [StockDB].dbo.Inventory iv ON d.InventoryId = iv.InventoryId
LEFT JOIN StockDB.dbo.InventoryRequirementDetail rd ON d.InventoryRequirementDetailId = rd.InventoryRequirementDetailId
LEFT JOIN StockDB.dbo.InventoryRequirement r ON rd.InventoryRequirementId = r.InventoryRequirementId

UPDATE dbo.ProductionOrderInventoryConflict SET ConflictAllowcationStatusId = 
CASE WHEN InventoryQuantity<= HandoverInventoryQuantitySum THEN 2
	WHEN HandoverInventoryQuantitySum>0 THEN 1
	ELSE 0
END
