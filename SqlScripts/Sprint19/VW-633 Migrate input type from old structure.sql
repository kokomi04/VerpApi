BEGIN TRANSACTION 

DELETE FROM InputTypeViewField
DELETE FROM InputTypeView

DELETE FROM InputValueRow
DELETE FROM InputBill

DELETE FROM InputAreaField
DELETE FROM InputArea
DELETE FROM InputType

DELETE FROM InputTypeGroup

DELETE FROM InputField

Set Identity_Insert InputTypeViewField OFF
Set Identity_Insert InputTypeView OFF
Set Identity_Insert InputValueRow OFF
Set Identity_Insert InputBill OFF
Set Identity_Insert InputAreaField OFF
Set Identity_Insert InputArea OFF
Set Identity_Insert InputType OFF
Set Identity_Insert InputTypeGroup OFF
Set Identity_Insert InputField OFF


Set Identity_Insert InputTypeGroup On
INSERT INTO dbo.InputTypeGroup
(
    InputTypeGroupId, -- column value is auto-generated
    InputTypeGroupName,
    SortOrder,
    CreatedByUserId,
    CreatedDatetimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc
)
SELECT 
	InputTypeGroupId, -- column value is auto-generated
    InputTypeGroupName,
    SortOrder,
    CreatedByUserId,
    CreatedDatetimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc
FROM AccountingDB.dbo.InputTypeGroup WHERE IsDeleted = 0
Set Identity_Insert InputTypeGroup Off


Set Identity_Insert InputType On
INSERT INTO dbo.InputType
(
    InputTypeId, -- column value is auto-generated
    InputTypeGroupId,
    Title,
    InputTypeCode,
    SortOrder,
    CreatedByUserId,
    CreatedDatetimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc,
    PreLoadAction,
    PostLoadAction
)
SELECT
	InputTypeId, -- column value is auto-generated
    InputTypeGroupId,
    Title,
    InputTypeCode,
    SortOrder,
    CreatedByUserId,
    CreatedDatetimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc,
    PreLoadAction,
    PostLoadAction
FROM AccountingDB.dbo.InputType WHERE IsDeleted = 0
Set Identity_Insert InputType OFF

Set Identity_Insert InputArea ON
INSERT INTO dbo.InputArea
(
    InputAreaId, -- column value is auto-generated
    InputTypeId,
    InputAreaCode,
    Title,
    IsMultiRow,
    [Columns],
    SortOrder,
    CreatedByUserId,
    CreatedDatetimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc
)
SELECT 

	InputAreaId, -- column value is auto-generated
    InputTypeId,
    InputAreaCode,
    Title,
    IsMultiRow,
    [Columns],
    SortOrder,
    CreatedByUserId,
    CreatedDatetimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc
FROM AccountingDB.dbo.InputArea WHERE IsDeleted=0
Set Identity_Insert InputArea OFF


Set Identity_Insert InputField ON

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

			DECLARE @SqlDataType nvarchar(128) = ''
			DECLARE @ResStatus int = 0;
			DECLARE @IsAddNew bit = 1;

			IF EXISTS (SELECT 0
						FROM [AccountancyDB].[INFORMATION_SCHEMA].[COLUMNS]
						WHERE [TABLE_NAME] = 'InputValueRow' AND [COLUMN_NAME] = @FieldName)
			BEGIN
				SET @IsAddNew = 0
			END

			
			INSERT INTO dbo.InputField
			(
			    InputFieldId, -- column value is auto-generated
			    FieldName,
			    Title,
			    Placeholder,
			    SortOrder,
			    DataTypeId,
			    DataSize,
			    DecimalPlace,
			    FormTypeId,
			    DefaultValue,
			    RefTableCode,
			    RefTableField,
			    RefTableTitle,
			    CreatedByUserId,
			    CreatedDatetimeUtc,
			    UpdatedByUserId,
			    UpdatedDatetimeUtc,
			    IsDeleted,
			    DeletedDatetimeUtc
			)
			VALUES
			(
			    @InputFieldId, -- int
			    @FieldName, -- FieldName - nvarchar
			    @Title, -- Title - nvarchar
			    @Placeholder, -- Placeholder - nvarchar
			    @SortOrder, -- SortOrder - int
			    @DataTypeId, -- DataTypeId - int
			    @DataSize, -- DataSize - int
			    5, -- DecimalPlace - int
			    @FormTypeId, -- FormTypeId - int
			    N'', -- DefaultValue - nvarchar
			    @RefTableName, -- RefTableCode - nvarchar
			    @RefFieldName, -- RefTableField - nvarchar
			    @RefFieldTitle, -- RefTableTitle - nvarchar
			    2, -- CreatedByUserId - int
			    GETUTCDATE(), -- CreatedDatetimeUtc - datetime2
			    2, -- UpdatedByUserId - int
			    GETUTCDATE(), -- UpdatedDatetimeUtc - datetime2
			    0, -- IsDeleted - bit
			    NULL -- DeletedDatetimeUtc - datetime2
			)
			

			--EnumDataType.Text => SqlDbType.NVarChar,
			IF @DataTypeId = 1 SET @SqlDataType = 'NVarChar'
           -- EnumDataType.Int => SqlDbType.Int,
			IF @DataTypeId = 2 
			BEGIN 
				SET @SqlDataType = 'Int'; 
				SET @DataSize=0 

				IF @FormTypeId NOT IN(2,4)--select, SearchTable
				BEGIN
					SET @DataTypeId = 9
					SET @SqlDataType = 'Decimal'
					SET @DataSize = 18					
				END
				
			END

            --EnumDataType.Date => SqlDbType.DateTime2,
			IF @DataTypeId = 3 SET @SqlDataType = 'DateTime2'
            --EnumDataType.PhoneNumber => SqlDbType.NVarChar,
			IF @DataTypeId = 4 SET @SqlDataType = 'NVarChar'
            --EnumDataType.Email => SqlDbType.NVarChar,
			IF @DataTypeId = 5 SET @SqlDataType = 'NVarChar'
            --EnumDataType.Boolean => SqlDbType.Bit,
			IF @DataTypeId = 6 BEGIN SET @SqlDataType = 'Bit'; SET @DataSize=0 END
            --EnumDataType.Percentage => SqlDbType.TinyInt,
			IF @DataTypeId = 7 BEGIN SET @SqlDataType = 'TinyInt'; SET @DataSize=0 END
            --EnumDataType.BigInt => SqlDbType.BigInt,
			IF @DataTypeId = 8 BEGIN SET @SqlDataType = 'BigInt'; SET @DataSize=0 END
            --EnumDataType.Decimal => SqlDbType.Decimal,
			IF @DataTypeId = 9 SET @SqlDataType = 'Decimal'
            --_ => SqlDbType.NVarChar
			IF LEN(@SqlDataType)=0 SET @SqlDataType = 'NVarChar'

		IF @SqlDataType = 'NVarChar' AND @DataSize<=1
		BEGIN
			SET @DataSize = 512
		END

		EXEC asp_Table_UpdateField
			@IsAddNew = @IsAddNew,
			@TableName = 'InputValueRow',
			@FieldName = @FieldName,
			@DataType = @SqlDataType,
			@DataSize = @DataSize,
			@DecimalPlace = 5,
			@DefaultValue = '',
			@IsNullable = 1,
			@ResStatus = @ResStatus OUTPUT
			

        FETCH NEXT FROM field_cursor INTO @InputFieldId,@FieldName,@Title,@Placeholder, @SortOrder,@DataTypeId,@FormTypeId, @DataSize, @RefTableName, @RefFieldName, @RefFieldTitle
        END  
  
    CLOSE field_cursor  
    DEALLOCATE field_cursor  
Set Identity_Insert InputField OFF


Set Identity_Insert InputAreaField ON

INSERT INTO dbo.InputAreaField
(
    InputAreaFieldId, -- column value is auto-generated
    InputFieldId,
    InputTypeId,
    InputAreaId,
    Title,
    Placeholder,
    SortOrder,
    IsAutoIncrement,
    IsRequire,
    IsUnique,
    IsHidden,
    IsCalcSum,
    RegularExpression,
    DefaultValue,
    Filters,
    Width,
    Height,
    TitleStyleJson,
    InputStyleJson,
    OnFocus,
    OnKeydown,
    OnKeypress,
    OnBlur,
    OnChange,
    AutoFocus,
    [Column],
    CreatedByUserId,
    CreatedDatetimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc
)
SELECT
	InputAreaFieldId, -- column value is auto-generated
    InputFieldId,
    InputTypeId,
    InputAreaId,
    Title,
    Placeholder,
    SortOrder,
    IsAutoIncrement,
    IsRequire,
    IsUnique,
    IsHidden,
    IsCalcSum,
    RegularExpression,
    DefaultValue,
    Filters,
    Width,
    Height,
    TitleStyleJson,
    InputStyleJson,
    OnFocus,
    OnKeydown,
    OnKeypress,
    OnBlur,
    OnChange,
    AutoFocus,
    [Column],
    CreatedByUserId,
    CreatedDatetimeUtc,
    UpdatedByUserId,
    UpdatedDatetimeUtc,
    IsDeleted,
    DeletedDatetimeUtc
FROM AccountingDB.dbo.InputAreaField WHERE IsDeleted=0
Set Identity_Insert InputAreaField OFF

COMMIT TRANSACTION

EXEC asp_InputValueRow_UpdateView