USE AccountancyDB
GO

UPDATE r
SET sourceBillCodes = NULL
FROM dbo.InputValueRow r
WHERE r.IsDeleted = 0 AND sourceBillCodes = r.so_ct

GO

;WITH _type AS(
SELECT 
DISTINCT
a.InputTypeId
FROM dbo.InputField f 
JOIN dbo.InputAreaField af ON af.InputFieldId = f.InputFieldId
JOIN dbo.InputArea a ON a.InputAreaId = af.InputAreaId
WHERE f.FieldName = 'sourceBillCodes'
)
UPDATE r
SET sourceBillCodes = r.so_ct
FROM dbo.InputValueRow r
JOIN _type t ON t.InputTypeId = r.InputTypeId
WHERE r.IsDeleted = 0
AND EXISTS (SELECT 0 FROM [StockDB].dbo.Inventory inv WHERE inv.IsDeleted = 0 AND inv.InventoryCode = r.so_ct)

GO
TRUNCATE TABLE dbo.InputValueRowSourceBillCode
GO
INSERT INTO dbo.InputValueRowSourceBillCode
(
    InputValueRow_F_Id,
    SourceBillCode
)
SELECT
	r.F_Id,
	c.[value]
FROM dbo.InputValueRow r
	OUTER APPLY(
		SELECT LTRIM(RTRIM([value])) [value] FROM dbo.ufn_Split(r.sourceBillCodes,',')
	) c
WHERE-- r.so_ct = @so_ct AND 
r.IsDeleted = 0 AND LEN([c].[value])>0
