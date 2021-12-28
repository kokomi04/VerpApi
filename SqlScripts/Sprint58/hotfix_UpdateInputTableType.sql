ALTER PROCEDURE [AccountancyDB].[dbo].[asp_UpdateInputTableType]
AS
BEGIN
	
	IF (EXISTS (SELECT * FROM sys.table_types WHERE name = 'InputTableType'))
		DROP TYPE InputTableType
	
	DECLARE @sql nvarchar(max) = ''
	
	DECLARE @ViewOnlyFormTypeId INT = 6

	DECLARE @FieldName nvarchar(128)
	DECLARE @SqlType nvarchar(10);
	DECLARE @DataSize INT;
	DECLARE @DecimalPrecision INT;
	DECLARE @DecimalPlace INT;
	
	DECLARE fieldCursor CURSOR FOR   
    SELECT f.FieldName FROM InputField f
		WHERE f.FormTypeId <> @ViewOnlyFormTypeId AND f.IsDeleted = 0
  
    OPEN fieldCursor  
			FETCH NEXT FROM fieldCursor INTO @FieldName
  
    IF @@FETCH_STATUS <> 0   
        PRINT '         <<None>>'       
  
    WHILE @@FETCH_STATUS = 0  
    BEGIN  
			
			SELECT @SqlType = DATA_TYPE, @DataSize = CHARACTER_MAXIMUM_LENGTH, @DecimalPrecision = NUMERIC_PRECISION, @DecimalPlace = NUMERIC_SCALE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'InputValueRow' AND COLUMN_NAME = @FieldName
			SET @sql += '
			' + @FieldName + ' ' + @SqlType
			
			IF (@SqlType = 'decimal')
				SET @sql += CONCAT('(',@DecimalPrecision, ',', @DecimalPlace ,'),') 
			ELSE
				IF @DataSize IS NOT NULL
					SET @sql += CONCAT('(',@DataSize ,'),') 
				ELSE
					SET @sql +=',' 
			FETCH NEXT FROM fieldCursor INTO @FieldName
			END  
			
			
  
    CLOSE fieldCursor  
    DEALLOCATE fieldCursor
		
		IF LEN(@sql)>0		
			SET @sql = SUBSTRING(@sql,0,LEN(@sql))	
		
		SET @sql = 'CREATE TYPE InputTableType AS TABLE
								(' + @sql + ')'
								
 		EXEC (@sql)
END;


EXEC [AccountancyDB].dbo.asp_UpdateInputTableType;