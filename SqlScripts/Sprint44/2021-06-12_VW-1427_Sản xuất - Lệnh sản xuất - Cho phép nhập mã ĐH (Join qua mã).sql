UPDATE od
SET od.OrderCode = r.so_ct,
 od.PartnerId = r.kh0
FROM ManufacturingDB.dbo.ProductionOrderDetail od
LEFT JOIN [PurchaseOrderDB].[dbo].vVoucherValueRow r ON od.OrderDetailId = r.F_Id
INNER JOIN PurchaseOrderDB.dbo.VoucherType t ON r.VoucherTypeId = t.VoucherTypeId
WHERE (t.VoucherTypeCode = 'CTBH_DON_HANG_XK'
       OR t.VoucherTypeCode = 'CTBH_DON_HANG')
  AND r.IsBillEntry = 0
  AND od.IsDeleted = 0
  AND od.OrderDetailId IS NOT NULL;