USE ManufacturingDB
GO
SET IDENTITY_INSERT dbo.ProductionHandoverReceipt ON;
GO
BEGIN TRANSACTION
DECLARE @tbl TABLE(
	[ProductionHandoverReceiptId] [BIGINT] IDENTITY(1,1) NOT NULL,
	[ProductionHandoverReceiptCode] [NVARCHAR](128) NULL,
	[ProductionOrderId] [BIGINT] NOT NULL,
	[HandoverDatetime] [DATETIME2](7) NULL,
	[HandoverStatusId] [INT] NOT NULL,
	[AcceptByUserId] [INT] NULL,
	[SubsidiaryId] [INT] NOT NULL,
	[CreatedByUserId] [INT] NOT NULL,
	[CreatedDatetimeUtc] [DATETIME2](7) NOT NULL,
	[UpdatedByUserId] [INT] NOT NULL,
	[UpdatedDatetimeUtc] [DATETIME2](7) NOT NULL,
	[IsDeleted] [BIT] NOT NULL,
	[DeletedDatetimeUtc] [DATETIME2](7) NULL,
	ProductionHandoverId BIGINT NULL
)

INSERT INTO @tbl
(
    ProductionHandoverReceiptCode,
    ProductionOrderId,
    HandoverDatetime,
    HandoverStatusId,
    AcceptByUserId,
    SubsidiaryId,
    CreatedByUserId,
    CreatedDatetimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc,
	ProductionHandoverId
)

SELECT	
	'',
    ProductionOrderId,
    HandoverDatetime,
    [Status],
    AcceptByUserId,
    SubsidiaryId,
    CreatedByUserId,
    CreatedDatetimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc,
	ProductionHandoverId
FROM dbo.ProductionHandover
ORDER BY CreatedDatetimeUtc;

UPDATE @tbl SET ProductionHandoverReceiptCode = CONCAT('BG', ProductionHandoverReceiptId);


INSERT INTO dbo.ProductionHandoverReceipt
(
	ProductionHandoverReceiptId,
    ProductionHandoverReceiptCode,
    ProductionOrderId,
    HandoverDatetime,
    HandoverStatusId,
    AcceptByUserId,
    SubsidiaryId,
    CreatedByUserId,
    CreatedDatetimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc
)
SELECT
	ProductionHandoverReceiptId,
    ProductionHandoverReceiptCode,
    ProductionOrderId,
    HandoverDatetime,
    HandoverStatusId,
    AcceptByUserId,
    SubsidiaryId,
    CreatedByUserId,
    CreatedDatetimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc
FROM @tbl

UPDATE h 
SET h.ProductionHandoverReceiptId = t.ProductionHandoverReceiptId
FROM dbo.ProductionHandover h
JOIN @tbl t ON t.ProductionHandoverId = h.ProductionHandoverId

COMMIT TRANSACTION;
GO

SET IDENTITY_INSERT dbo.ProductionHandoverReceipt OFF;