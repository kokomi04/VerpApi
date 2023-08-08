USE PurchaseOrderDB
GO

CREATE TABLE PurchaseOrderOrderMapping (
    PurchaseOrderOrderMappingId BIGINT IDENTITY (1, 1) PRIMARY KEY,
    PurchaseOrderDetailId BIGINT REFERENCES PurchaseOrderDetail (PurchaseOrderDetailId),
    OrderCode NVARCHAR(128) NOT NULL,
    PrimaryQuantity DECIMAL(32, 12),
    PuQuantity DECIMAL(32, 12),
    Note NVARCHAR(256) NULL,
    CreatedByUserId INT NOT NULL,
    CreatedDatetimeUtc DATETIME2(7) NOT NULL,
    UpdatedByUserId INT NOT NULL,
    UpdatedDatetimeUtc DATETIME2(7) NOT NULL,
    IsDeleted BIT NOT NULL,
    DeletedDatetimeUtc DATETIME2(7) NULL
)

GO

INSERT INTO dbo.PurchaseOrderOrderMapping(
    PurchaseOrderDetailId,
    OrderCode ,
    PrimaryQuantity ,
    PuQuantity ,
    CreatedByUserId ,
    CreatedDatetimeUtc ,
    UpdatedByUserId ,
    UpdatedDatetimeUtc ,
    IsDeleted ,
    DeletedDatetimeUtc)
SELECT 
	v.PurchaseOrderDetailId,
    v.OrderCode ,
    v.PrimaryQuantity ,
    v.ProductUnitConversionQuantity,
    v.CreatedByUserId ,
    v.CreatedDatetimeUtc ,
    v.UpdatedByUserId ,
    v.UpdatedDatetimeUtc ,
    v.IsDeleted ,
    v.DeletedDatetimeUtc
FROM  dbo.PurchaseOrderDetail as v 
WHERE v.IsDeleted = 0 AND v.OrderCode IS NOT NULL