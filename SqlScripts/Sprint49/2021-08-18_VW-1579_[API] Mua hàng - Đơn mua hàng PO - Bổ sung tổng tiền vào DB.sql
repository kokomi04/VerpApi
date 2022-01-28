USE PurchaseOrderDB;

--ALTER TABLE PurchaseOrderDB.dbo.PurchaseOrderDetail
--ADD IntoMoney decimal(18,4);
--ALTER TABLE PurchaseOrderDB.dbo.PurchaseOrderDetail
--ADD IntoAfterTaxMoney decimal(18,4);

UPDATE PurchaseOrderDB.dbo.PurchaseOrderDetail
SET 
	IntoMoney = CAST((PrimaryQuantity * PrimaryUnitPrice) as decimal(18,0)),
	IntoAfterTaxMoney =  CAST((PrimaryQuantity * PrimaryUnitPrice) as decimal(18,0)) + CAST((PrimaryQuantity * PrimaryUnitPrice * CAST(TaxInPercent/100 as decimal(18,2))) as decimal(18,0))
WHERE IsDeleted = 0;
