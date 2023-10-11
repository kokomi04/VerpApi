USE PurchaseOrderDB
GO
UPDATE pod
SET pod.RefPurchasingSuggestId = sd.PurchasingSuggestId
 FROM dbo.PurchaseOrderDetail pod
JOIN dbo.PurchasingSuggestDetail sd ON pod.PurchasingSuggestDetailId = sd.PurchasingSuggestDetailId

UPDATE sd
SET sd.RefPurchasingRequestId = rd.PurchasingRequestId
 FROM dbo.PurchasingSuggestDetail sd
JOIN dbo.PurchasingRequestDetail rd ON sd.PurchasingRequestDetailId = rd.PurchasingRequestDetailId