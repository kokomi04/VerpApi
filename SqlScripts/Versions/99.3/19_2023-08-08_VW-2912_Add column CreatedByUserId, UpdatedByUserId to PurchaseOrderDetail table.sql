USE PurchaseOrderDB
GO

Update PurchaseOrderDetail
SET CreatedByUserId = PO.CreatedByUserId,
    UpdatedByUserId = PO.UpdatedByUserId
FROM PurchaseOrderDetail POD
INNER JOIN PurchaseOrder PO ON POD.PurchaseOrderId = PO.PurchaseOrderId