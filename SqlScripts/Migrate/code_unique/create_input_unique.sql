
ALTER TRIGGER dbo.InputValueRow_AFTER_INSERT_UPDATE
   ON dbo.InputValueRow
   AFTER INSERT,UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE b
		SET b.BillCode = r.so_ct
	FROM dbo.InputBill b
		JOIN Inserted r ON b.F_Id = r.InputBill_F_Id
	WHERE r.IsDeleted = 0
END
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_InputBill_BillCode] ON [dbo].[InputBill]
(
	[SubsidiaryId] ASC,
	[BillCode] ASC
)
WHERE [IsDeleted]=0
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
UPDATE b
SET b.BillCode = r.so_ct
FROM dbo.InputBill b
JOIN (
SELECT v.InputBill_F_Id, v.so_ct, ROW_NUMBER() OVER(PARTITION BY v.InputBill_F_Id ORDER BY v.UpdatedDatetimeUtc) rNumber FROM dbo.InputValueRow v where v.IsDeleted=0
) r ON b.F_Id = r.InputBill_F_Id AND r.rNumber=1


GO

UPDATE b
SET b.BillCode = r.so_ct
FROM dbo.VoucherBill b
JOIN (
SELECT v.VoucherBill_F_Id, v.so_ct, ROW_NUMBER() OVER(PARTITION BY v.VoucherBill_F_Id ORDER BY v.UpdatedDatetimeUtc) rNumber FROM dbo.VoucherValueRow v where v.IsDeleted=0
) r ON b.F_Id = r.VoucherBill_F_Id AND r.rNumber=1

GO
CREATE TRIGGER dbo.VoucherValueRow_AFTER_INSERT_UPDATE
   ON dbo.VoucherValueRow
   AFTER INSERT,UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE b
		SET b.BillCode = r.so_ct
	FROM dbo.VoucherBill b
		JOIN Inserted r ON b.F_Id = r.VoucherBill_F_Id
	WHERE r.IsDeleted = 0
END
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_VoucherBill_BillCode] ON [dbo].[VoucherBill]
(
	[SubsidiaryId] ASC,
	[BillCode] ASC
)
WHERE [IsDeleted]=0
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO