USE AccountancyDB
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
		SELECT LTRIM(RTRIM([value])) [value] FROM dbo.ufn_Split(CASE WHEN LEN(r.sourceBillCodes)>0 THEN r.sourceBillCodes ELSE r.so_ct END,',')
	) c
WHERE-- r.so_ct = @so_ct AND 
r.IsDeleted = 0 AND LEN([c].[value])>0