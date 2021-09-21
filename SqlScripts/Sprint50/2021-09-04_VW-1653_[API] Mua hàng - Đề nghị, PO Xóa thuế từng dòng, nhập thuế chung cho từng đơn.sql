Use PurchaseOrderDB;

--alter table PurchaseOrder
--add TaxInPercent decimal(18,4);

UPDATE po
SET po.TaxInPercent = d.TaxInPercent
FROM PurchaseOrder po
Left join (
	Select pod.PurchaseOrderId, MAX(ISNULL(pod.TaxInPercent, 0)) TaxInPercent 
	From PurchaseOrderDetail pod 
	where pod.IsDeleted = 0 group by pod.PurchaseOrderId
) d on po.PurchaseOrderId = d.PurchaseOrderId
WHERE po.IsDeleted = 0;


UPDATE po
SET po.TaxInPercent = d.TaxInPercent
FROM PurchasingSuggest po
Left join (
	Select pod.PurchasingSuggestId, MAX(ISNULL(pod.TaxInPercent, 0)) TaxInPercent 
	From PurchasingSuggestDetail pod 
	where pod.IsDeleted = 0 group by pod.PurchasingSuggestId
) d on po.PurchasingSuggestId = d.PurchasingSuggestId
WHERE po.IsDeleted = 0;