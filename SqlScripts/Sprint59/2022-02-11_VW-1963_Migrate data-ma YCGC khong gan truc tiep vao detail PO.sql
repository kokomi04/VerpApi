use PurchaseOrderDB;

Declare @PurchaseOrderDetailId bigint, @PrimaryQuantity decimal(32,12),@OutsourceRequestId bigint, @OrderCode nvarchar(256),
@ProductId int, @ProductionOrderCode nvarchar(256), @ProductionStepLinkDataId bigint

Declare @Query nvarchar(1024)

Declare cursorPO  CURSOR FOR
select pod.PurchaseOrderDetailId, pod.PrimaryQuantity, pod.OutsourceRequestId, pod.OrderCode, pod.ProductionOrderCode, pod.ProductId, pod.ProductionStepLinkDataId
from PurchaseOrder po
left join PurchaseOrderDetail pod on po.PurchaseOrderId = pod.PurchaseOrderId and pod.IsDeleted = 0
where po.PurchaseOrderType in (1,2) and pod.OutsourceRequestId is not null and po.IsDeleted = 0

OPEN cursorPO
FETCH NEXT FROM cursorPO INTO @PurchaseOrderDetailId , @PrimaryQuantity, @OutsourceRequestId, @OrderCode, @ProductionOrderCode,
@ProductId, @ProductionStepLinkDataId
WHILE @@FETCH_STATUS = 0
BEGIN
    INSERT INTO dbo.PurchaseOrderOutsourceMapping(PurchaseOrderDetailId, OutsourcePartRequestId, ProductId, Quantity, ProductionOrderCode, OrderCode, CreatedDatetimeUtc, CreatedByUserId, IsDeleted,UpdatedDatetimeUtc,UpdatedByUserId, ProductionStepLinkDataId) VALUES(@PurchaseOrderDetailId, @OutsourceRequestId,@ProductId,@PrimaryQuantity, ISNULL(@ProductionOrderCode,''), @OrderCode,GETDATE(),2,0,GETDATE(),2,@ProductionStepLinkDataId);

    FETCH NEXT FROM cursorPO INTO @PurchaseOrderDetailId , @PrimaryQuantity, @OutsourceRequestId, @OrderCode, @ProductionOrderCode,
	@ProductId,@ProductionStepLinkDataId
END
CLOSE cursorPO
DEALLOCATE cursorPO
