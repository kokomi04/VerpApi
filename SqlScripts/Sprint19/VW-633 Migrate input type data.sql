DECLARE @Columns nvarchar(max) = ''
DECLARE @ColumnValues nvarchar(max) = ''


DECLARE @InputFieldId int
DECLARE @FieldName nvarchar(128)
DECLARE @Title nvarchar(128)
DECLARE @Placeholder nvarchar(128)
DECLARE @SortOrder int
DECLARE @DataTypeId int
DECLARE @FormTypeId int
DECLARE @DataSize int
DECLARE @RefTableName nvarchar(128)
DECLARE @RefFieldName nvarchar(128)
DECLARE @RefFieldTitle nvarchar(512)

 DECLARE field_cursor CURSOR FOR   
    SELECT 
		f.InputFieldId,
		f.FieldName, 
		f.Title,
		f.Placeholder,
		f.SortOrder,
		f.DataTypeId, 
		f.FormTypeId,
		f.DataSize, 		
		'_' + c.CategoryCode RefTableName, 
		cf.CategoryFieldName RefFieldName, 
		ct.CategoryFieldName RefFieldTitle
	FROM AccountingDB.dbo.InputField f
		LEFT JOIN AccountingDB.dbo.CategoryField cf ON f.ReferenceCategoryFieldId = cf.CategoryFieldId
		LEFT JOIN AccountingDB.dbo.CategoryField ct ON f.ReferenceCategoryTitleFieldId = ct.CategoryFieldId
		LEFT JOIN AccountingDB.dbo.Category c ON cf.CategoryId = c.CategoryId
	WHERE f.IsDeleted = 0
  
    OPEN field_cursor  
    FETCH NEXT FROM field_cursor INTO @InputFieldId,@FieldName,@Title,@Placeholder, @SortOrder,@DataTypeId,@FormTypeId, @DataSize, @RefTableName, @RefFieldName, @RefFieldTitle
  
    IF @@FETCH_STATUS <> 0   
        PRINT '         <<None>>'    
    WHILE @@FETCH_STATUS = 0  
    BEGIN  

		
		SET @Columns = @Columns + ',' + @FieldName

		IF @DataTypeId = 3--date
		BEGIN
			PRINT @FieldName
			SET @ColumnValues = @ColumnValues + ',  DATEADD(s, CONVERT(INT, ' + @FieldName + '), ''1970-01-01'')'
		END
		ELSE
		BEGIN
			SET @ColumnValues = @ColumnValues + ',' + @FieldName
		END

        FETCH NEXT FROM field_cursor INTO @InputFieldId,@FieldName,@Title,@Placeholder, @SortOrder,@DataTypeId,@FormTypeId, @DataSize, @RefTableName, @RefFieldName, @RefFieldTitle
        END  
  
    CLOSE field_cursor  
    DEALLOCATE field_cursor  

BEGIN TRANSACTION 

Set Identity_Insert InputBill OFF
Set Identity_Insert InputBill ON

DELETE FROM InputBill
INSERT INTO dbo.InputBill
(
    F_Id, -- column value is auto-generated
    InputTypeId,
    CreatedByUserId,
    CreatedDatetimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc,
    LatestBillVersion
)
SELECT 
	InputValueBillId,
	InputTypeId,
	2,
	GETUTCDATE(),
	2,
	GETUTCDATE(),
	0,
	NULL,
	1
FROM AccountingDB.dbo.InputValueBill

Set Identity_Insert InputBill OFF;

DECLARE @Sql  nvarchar(max) = '
INSERT INTO dbo.InputValueRow
(
    --F_Id - column value is auto-generated
    InputTypeId,
    InputBill_F_Id,
    BillVersion,
    CreatedByUserId,
    CreatedDatetimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc
 ' + @Columns + ')
  
 SELECT 
	InputTypeId,
	InputValueBillId,
	1,
	2,
	GETUTCDATE(),
	2,
	GETUTCDATE(),
	0,
	NULL 
 '+ @ColumnValues+ '
 FROM AccountingDB.dbo._vInputBillData'

 PRINT @Sql
 EXEC (@Sql)

COMMIT TRANSACTION