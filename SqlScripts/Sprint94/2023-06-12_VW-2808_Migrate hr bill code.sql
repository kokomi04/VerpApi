USE OrganizationDB;
GO

DECLARE @Sql NVARCHAR(MAX);

DECLARE @FieldName NVARCHAR(128);
DECLARE @HrAreaCode NVARCHAR(128);
DECLARE @HrTypeId INT;
DECLARE @Title NVARCHAR(128);
DECLARE @HrTypeCode NVARCHAR(128);

DECLARE fieldCursor CURSOR LOCAL FORWARD_ONLY READ_ONLY FOR
SELECT f.FieldName,
       a.HrAreaCode,
       t.HrTypeId,
       af.Title,
       t.HrTypeCode
FROM dbo.HrField f
    JOIN dbo.HrAreaField af
        ON af.HrFieldId = f.HrFieldId
    JOIN dbo.HrArea a
        ON a.HrAreaId = af.HrAreaId
    JOIN dbo.HrType t
        ON t.HrTypeId = a.HrTypeId
WHERE f.FieldName = 'so_ct'
      AND f.IsDeleted = 0
      AND af.IsDeleted = 0
      AND a.IsDeleted = 0
      AND t.IsDeleted = 0;

OPEN fieldCursor;
FETCH NEXT FROM fieldCursor
INTO @FieldName,
     @HrAreaCode,
     @HrTypeId,
     @Title,
     @HrTypeCode;

IF @@FETCH_STATUS <> 0
    PRINT '         <<None>>';

WHILE @@FETCH_STATUS = 0
BEGIN


    SET @Sql
        = CONCAT(
                    '
			UPDATE b
			SET b.BillCode = a.so_ct
			FROM dbo.HrBill b
				JOIN dbo._HR_',
                    @HrTypeCode,
                    '_',
                    @HrAreaCode,
                    ' a ON b.F_Id = a.HrBill_F_Id
			WHERE b.IsDeleted = 0 AND a.IsDeleted = 0

		'
                );

    EXEC sys.sp_executesql @Sql;

    FETCH NEXT FROM fieldCursor
    INTO @FieldName,
         @HrAreaCode,
         @HrTypeId,
         @Title,
         @HrTypeCode;
END;

CLOSE fieldCursor;
DEALLOCATE fieldCursor;