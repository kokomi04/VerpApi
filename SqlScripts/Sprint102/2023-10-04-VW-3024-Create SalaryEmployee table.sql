	
	
	USE OrganizationDB
	GO

	DECLARE @SubId INT
	DECLARE @SubsidiayCode NVARCHAR(128)
	DECLARE @FieldName NVARCHAR(128)
	DECLARE @DataTypeId INT
	
	DECLARE salary_field_cursor CURSOR READ_ONLY FORWARD_ONLY LOCAL FOR 
    SELECT f.SubsidiaryId, s.SubsidiaryCode, f.SalaryFieldName, f.DataTypeId
    FROM OrganizationDB.dbo.SalaryField f
		JOIN OrganizationDB.dbo.Subsidiary s ON f.SubsidiaryId = s.SubsidiaryId
	WHERE f.IsDeleted = 0 AND f.IsDisplayRefData = 0
  
    OPEN salary_field_cursor  
    FETCH NEXT FROM salary_field_cursor INTO @SubId, @SubsidiayCode, @FieldName, @DataTypeId
  
    IF @@FETCH_STATUS <> 0   
        PRINT '         <<None>>'       
  
    WHILE @@FETCH_STATUS = 0  
    BEGIN  
  
		DECLARE @OldName NVARCHAR(128) = NULL
		DECLARE @TableName NVARCHAR(128) = CONCAT('_SalaryEmployee','_',@SubsidiayCode);

        SELECT @OldName = COLUMN_NAME  FROM [INFORMATION_SCHEMA].[COLUMNS] WHERE TABLE_NAME = @TableName AND COLUMN_NAME = @FieldName

		EXEC dbo.asp_SalaryEmployeeTable_UpdateField
				@SubId = @SubId,	
				@OldFieldName = @OldName,
				@NewFieldName = @FieldName,
				@DataTypeId = @DataTypeId

        FETCH NEXT FROM salary_field_cursor INTO @SubId, @SubsidiayCode, @FieldName, @DataTypeId
        END  
  
    CLOSE salary_field_cursor  
    DEALLOCATE salary_field_cursor  