	
	USE OrganizationDB
	GO

	DECLARE @SubId INT
	DECLARE @SubSidiayCode NVARCHAR(128)	
	
	DECLARE subsidiary_cursor CURSOR READ_ONLY FORWARD_ONLY LOCAL FOR 
    SELECT s.SubsidiaryId, s.SubsidiaryCode
    FROM OrganizationDB.dbo.Subsidiary s
	WHERE s.IsDeleted = 0
  
    OPEN subsidiary_cursor  
    FETCH NEXT FROM subsidiary_cursor INTO @SubId, @SubSidiayCode
  
    IF @@FETCH_STATUS <> 0   
        PRINT '         <<None>>'       
  
    WHILE @@FETCH_STATUS = 0  
    BEGIN  
  
		DECLARE @TableName NVARCHAR(128) = CONCAT('_SalaryEmployee','_',@SubsidiayCode);


		DECLARE @SelectFields NVARCHAR(MAX) = '
		SubsidiaryId,
		EmployeeId,
		SalaryPeriodId,
		SalaryGroupId,
		CreatedByUserId,
		CreatedDatetimeUtc,
		UpdatedByUserId,
		UpdatedDatetimeUtc,
		IsDeleted,
		DeletedDatetimeUtc
		';

		DECLARE @Fields NVARCHAR(MAX) = NULL;	
		DECLARE @CastIsEditedFields NVARCHAR(MAX) = NULL;
		DECLARE @ConvertColumns NVARCHAR(MAX) = NULL;

		SELECT @SelectFields = CONCAT(@SelectFields, ',[', f.SalaryFieldName+']') + CONCAT(',[',f.SalaryFieldName,'_IsEdited]'),
			   @Fields = COALESCE(@Fields + ', ', '') + QUOTENAME(f.SalaryFieldName),
			   @CastIsEditedFields = COALESCE(@CastIsEditedFields + ', ', '') + QUOTENAME(f.SalaryFieldName) + ' AS [' +  CONCAT(f.SalaryFieldName,'_IsEdited]'),
			   @ConvertColumns = COALESCE(@ConvertColumns + ', ', '')  + CONCAT(
					CASE DataTypeId 
						WHEN 2 THEN 'ISNULL(CONVERT(INT, '  + QUOTENAME(f.SalaryFieldName) + '),0)'
						WHEN 8 THEN 'ISNULL(CONVERT(BIGINT,' + QUOTENAME(f.SalaryFieldName) + '),0)'
						WHEN 9 THEN 'ISNULL(CONVERT(DECIMAL(32,12),'+ QUOTENAME(f.SalaryFieldName) + '),0)'
						ELSE 'ISNULL(CONVERT(NVARCHAR(1024),'+ QUOTENAME(f.SalaryFieldName) + '),'''')'
					END,
					' ',
					QUOTENAME(SalaryFieldName)
					)
		FROM SalaryField f 
		WHERE f.IsDeleted = 0 AND f.IsDisplayRefData = 0 AND f.SubsidiaryId = @SubId
		ORDER BY f.SalaryFieldId
		
		
		DECLARE @Sql NVARCHAR(MAX) = '
			TRUNCATE TABLE [' + @TableName+']
			;WITH v AS (
				SELECT
						SalaryEmployeeId,
						SubsidiaryId,						
						EmployeeId,
						SalaryPeriodId,
						SalaryGroupId,
						CreatedByUserId,
						CreatedDatetimeUtc,
						UpdatedByUserId,
						UpdatedDatetimeUtc,
						IsDeleted,
						DeletedDatetimeUtc
				' + COALESCE(',' + @ConvertColumns, '') + '
				FROM
				(
					SELECT
						se.SalaryEmployeeId,
						sf.SubsidiaryId,						
						se.EmployeeId,
						se.SalaryPeriodId,
						se.SalaryGroupId,
						se.CreatedByUserId,
						se.CreatedDatetimeUtc,
						se.UpdatedByUserId,
						se.UpdatedDatetimeUtc,
						se.IsDeleted,
						se.DeletedDatetimeUtc,
						sf.SalaryFieldName,
						sev.Value	
					FROM  SalaryEmployee se						
						JOIN SalaryEmployeeValue sev ON se.SalaryEmployeeId = sev.SalaryEmployeeId
						JOIN SalaryField sf ON sev.SalaryFieldId = sf.SalaryFieldId
					WHERE se.IsDeleted = 0 AND sf.IsDeleted = 0 AND sf.SubsidiaryId = ' + CONVERT(NVARCHAR(MAX),@SubId)  +'
				) as sfd
				PIVOT
				(
					MAX(sfd.Value)
					FOR sfd.SalaryFieldName IN (' + @Fields + ')
				) As SalaryFlatData
			), e AS (
				SELECT						
				SalaryEmployeeId, ' + @CastIsEditedFields + '
				FROM
				(
					SELECT
						se.SalaryEmployeeId,
						sf.SubsidiaryId,						
						se.EmployeeId,
						se.SalaryPeriodId,
						se.SalaryGroupId,
						se.CreatedByUserId,
						se.CreatedDatetimeUtc,
						se.UpdatedByUserId,
						se.UpdatedDatetimeUtc,
						se.IsDeleted,
						se.DeletedDatetimeUtc,
						sf.SalaryFieldName,
						--sev.IsEdited,
						CASE WHEN sev.IsEdited = 1 THEN 1 ELSE 0 END IsEdited
					FROM  SalaryEmployee se CROSS JOIN SalaryField sf
						--JOIN SalaryField sf ON sev.SalaryFieldId = sf.SalaryFieldId
						LEFT JOIN SalaryEmployeeValue sev ON se.SalaryEmployeeId = sev.SalaryEmployeeId AND sf.SalaryFieldId = sev.SalaryFieldId
						
					WHERE se.IsDeleted = 0 AND sf.IsDeleted = 0 AND sf.SubsidiaryId = ' + CONVERT(NVARCHAR(MAX),@SubId)  +'
				) as sfd
				PIVOT
				(
					MAX(sfd.IsEdited)
					FOR sfd.SalaryFieldName IN (' + @Fields + ')
				) As SalaryFlatData
			)'
			+ CONCAT('INSERT INTO ', @TableName,'(',@SelectFields,')')
			+ 'SELECT ' + @SelectFields + ' FROM v JOIN e ON v.SalaryEmployeeId = e.SalaryEmployeeId'

			SELECT CAST(@Sql AS XML)
			PRINT(@Sql)
			EXEC (@Sql)
        FETCH NEXT FROM subsidiary_cursor INTO @SubId, @SubSidiayCode
        END  
  
    CLOSE subsidiary_cursor  
    DEALLOCATE subsidiary_cursor  