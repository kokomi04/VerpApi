USE PurchaseOrderDB
GO

Alter table PurchaseOrderDetail
Add CreatedByUserId int not null DEFAULT 0, 
UpdatedByUserId int not null DEFAULT 0

GO

Update PurchaseOrderDetail
SET CreatedByUserId = PO.CreatedByUserId,
    UpdatedByUserId = PO.UpdatedByUserId
FROM PurchaseOrderDetail POD
INNER JOIN PurchaseOrder PO ON POD.PurchaseOrderId = PO.PurchaseOrderId