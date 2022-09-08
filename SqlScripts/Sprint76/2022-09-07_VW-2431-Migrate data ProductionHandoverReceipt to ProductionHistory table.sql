USE ManufacturingDB
GO
SET IDENTITY_INSERT dbo.ProductionHandoverReceipt ON;
GO
BEGIN TRANSACTION

DECLARE @EnumHandoverStatus_Accepted INT = 1;

DECLARE @MaxId BIGINT = 0;
SELECT @MaxId = MAX(r.ProductionHandoverReceiptId) FROM dbo.ProductionHandoverReceipt r;
SET @MaxId = ISNULL(@MaxId,1);

DECLARE @tbl TABLE(
	[ProductionHandoverReceiptId] [BIGINT] NOT NULL,
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
	ProductionHistoryId BIGINT NULL
)

INSERT INTO @tbl
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
    DeletedDatetimeUtc,
	ProductionHistoryId
)

SELECT
	@MaxId + ROW_NUMBER() OVER(ORDER BY ProductionHistoryId),
	'',
    ProductionOrderId,
    [Date],
    @EnumHandoverStatus_Accepted,
    CreatedByUserId,
    SubsidiaryId,
    CreatedByUserId,
    CreatedDatetimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc,
	ProductionHistoryId
FROM dbo.ProductionHistory
WHERE ProductionHandoverReceiptId IS NULL
ORDER BY CreatedDatetimeUtc;

UPDATE @tbl SET ProductionHandoverReceiptCode = CONCAT('PHIS', ProductionHandoverReceiptId);


INSERT INTO dbo.ProductionHandoverReceipt
(
	ProductionHandoverReceiptId,
    ProductionHandoverReceiptCode,   
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
FROM dbo.ProductionHistory h
JOIN @tbl t ON t.ProductionHistoryId = h.ProductionHistoryId

COMMIT TRANSACTION;
GO

SET IDENTITY_INSERT dbo.ProductionHandoverReceipt OFF;